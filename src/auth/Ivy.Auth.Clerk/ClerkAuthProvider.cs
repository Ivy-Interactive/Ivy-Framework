using Ivy.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Ivy.Auth.Clerk.ApiClient;
using Ivy.Auth.Clerk.ApiClient.Responses;

namespace Ivy.Auth.Clerk;

public class ClerkOAuthException(string? error, string? errorDescription)
    : Exception($"Clerk error: '{error}' - {errorDescription}")
{
    public string? Error { get; } = error;
    public string? ErrorDescription { get; } = errorDescription;
}

public class ClerkAuthProvider : ClerkAuthTokenHandler, IAuthProvider
{
    private readonly string _secretKey;
    private readonly List<AuthOption> _authOptions = [];
    private readonly BackendApiClient _backendClient;

    public bool OpenOAuthLoginInNewTab => true;

    public ClerkAuthProvider(IConfiguration configuration)
        : base(configuration)
    {
        _secretKey = configuration.GetValue<string>("Clerk:SecretKey") ?? throw new Exception("Clerk:SecretKey is required");
        _backendClient = new BackendApiClient(_secretKey);
    }

    private async Task<AuthToken?> TryRestoreExistingSessionAsync(IAuthSession authSession, ClerkCredentials credentials, CancellationToken cancellationToken)
    {
        try
        {
            var frontendClient = MakeFrontendApiClient(authSession);
            var activeSession = await GetActiveSession(frontendClient, credentials, cancellationToken);
            if (activeSession == null)
            {
                return null;
            }

            await frontendClient.TouchSessionAsync(activeSession.Id, credentials, cancellationToken);
            var newToken = await frontendClient.CreateSessionTokenAsync(activeSession.Id, credentials, cancellationToken);

            if (await ValidateToken(newToken.Jwt, lenientLifetimeValidation: true, cancellationToken) == null)
            {
                return null;
            }

            return new AuthToken(newToken.Jwt!);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool IsSessionExistsError(ClerkException ex)
        => ex.Errors?.Any(e => e.Code == "session_exists") == true;


    public async Task<LoginResult> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var credentials = await GetClerkCredentialsAsync(authSession, cancellationToken: cancellationToken);
            var frontendClient = MakeFrontendApiClient(authSession);

            ClerkSignInResponse signInResponse;
            try
            {
                signInResponse = await frontendClient.CreatePasswordSignInAsync(credentials, email, password, cancellationToken);
            }
            catch (ClerkException ex) when (IsSessionExistsError(ex))
            {
                var restoredToken = await TryRestoreExistingSessionAsync(authSession, credentials, cancellationToken);
                if (restoredToken != null)
                {
                    return LoginResult.Success(restoredToken);
                }

                await frontendClient.RemoveAllSessionsAsync(credentials, cancellationToken);
                signInResponse = await frontendClient.CreatePasswordSignInAsync(credentials, email, password, cancellationToken);
            }

            if (signInResponse.Response?.CreatedSessionId is not { } sessionId)
            {
                return LoginResult.InvalidCredentials();
            }

            var newToken = await frontendClient.CreateSessionTokenAsync(sessionId, credentials, cancellationToken);

            if (await ValidateToken(newToken.Jwt, lenientLifetimeValidation: false, cancellationToken) == null)
            {
                throw new Exception("New JWT from Clerk is invalid.");
            }

            return LoginResult.Success(new AuthToken(newToken.Jwt!));
        }
        catch (Exception)
        {
            return LoginResult.InvalidCredentials();
        }
    }

    public async Task<Uri> GetOAuthUriAsync(IAuthSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_origin) || string.IsNullOrEmpty(_callbackBaseUrl))
        {
            throw new Exception("ClerkAuthProvider is not initialized. Call InitializeAsync before using.");
        }

        var credentials = await GetClerkCredentialsAsync(authSession, cancellationToken: cancellationToken);

        var strategy = option.Id switch
        {
            "google" => "oauth_google",
            "github" => "oauth_github",
            "twitter" => "oauth_twitter",
            "apple" => "oauth_apple",
            "microsoft" => "oauth_microsoft",
            _ => throw new Exception($"Unsupported OAuth strategy: {option.Id}"),
        };

        var redirectUrl = callback.GetUri(includeIdInPath: true).ToString();
        var frontendClient = MakeFrontendApiClient(authSession);

        ClerkSignInResponse signInResponse;
        try
        {
            signInResponse = await frontendClient.CreateSignInAsync(credentials, _origin, strategy, redirectUrl, null, cancellationToken);
        }
        catch (ClerkException ex) when (IsSessionExistsError(ex))
        {
            await frontendClient.RemoveAllSessionsAsync(credentials, cancellationToken);
            signInResponse = await frontendClient.CreateSignInAsync(credentials, _origin, strategy, redirectUrl, null, cancellationToken);
        }

        // Store the sign-in ID so we can retrieve status in the callback
        var sessionData = authSession.GetAuthSessionData<ClerkAuthSessionData>() ?? new();
        sessionData.PendingSignInId = signInResponse.Response!.Id;
        authSession.SetAuthSessionData(sessionData);

        var firstFactorVerificationResponse = await frontendClient.PrepareFirstFactorVerificationAsync(credentials, _origin, signInResponse.Response!.Id, strategy, redirectUrl, null, cancellationToken);

        if (firstFactorVerificationResponse.Response?.FirstFactorVerification?.ExternalVerificationRedirectUrl is not { } oauthUri)
        {
            throw new Exception("Failed to get OAuth redirect URL from Clerk.");
        }
        return new Uri(oauthUri);
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(IAuthSession authSession, HttpRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_origin) || string.IsNullOrEmpty(_callbackBaseUrl))
        {
            throw new Exception("ClerkAuthProvider is not initialized. Call InitializeAsync before using.");
        }

        var credentials = await GetClerkCredentialsAsync(authSession, cancellationToken: cancellationToken);
        var frontendClient = MakeFrontendApiClient(authSession);
        var sessionId = request.Query["created_session_id"].ToString();
        var sessionData = authSession.GetAuthSessionData<ClerkAuthSessionData>() ?? new();
        if (sessionData?.PendingSignInId is not { } pendingSignInId)
        {
            throw new Exception("No pending sign-in found in OAuth callback.");
        }

        var signIn = await frontendClient.RetrieveSignInAsync(pendingSignInId, credentials, cancellationToken);
        if (signIn.Response?.Status == "complete" && signIn.Response.CreatedSessionId is { } createdSessionId && createdSessionId != sessionId)
        {
            throw new Exception($"Session ID from query does not match session ID from sign-in status.");
        }

        try
        {

            if (signIn.Response?.Status == "needs_identifier" && signIn.Response.FirstFactorVerification?.Status == "transferable")
            {
                var redirectUrl = $"{_callbackBaseUrl}/{Guid.NewGuid()}";
                var signUpResponse = await frontendClient.CreateSignUpAsync(credentials, _origin, signIn.Response.FirstFactorVerification.Strategy, redirectUrl, redirectUrl, transfer: true, cancellationToken);

                if (signUpResponse.Response?.CreatedSessionId is { } newSessionId)
                {
                    sessionId = newSessionId;
                }

                return null;
            }

            if (!string.IsNullOrEmpty(sessionId))
            {
                await frontendClient.TouchSessionAsync(sessionId, credentials, cancellationToken);
                var newToken = await frontendClient.CreateSessionTokenAsync(sessionId, credentials, cancellationToken);

                if (await ValidateToken(newToken.Jwt, lenientLifetimeValidation: false, cancellationToken) == null)
                {
                    throw new Exception("Failed to get new JWT from Clerk.");
                }
                else
                {
                    return new AuthToken(newToken.Jwt!);
                }
            }

            throw new Exception("No session ID found.");
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            // Clear the pending sign-in ID after handling the callback
            sessionData.PendingSignInId = null;
            authSession.SetAuthSessionData(sessionData);
        }
    }

    public async Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        var credentials = await GetClerkCredentialsAsync(authSession, cancellationToken: cancellationToken);
        var jwt = authSession.AuthToken?.AccessToken;

        var sessionData = authSession.GetAuthSessionData<ClerkAuthSessionData>() ?? new();
        sessionData.PendingSignInId = null;
        authSession.SetAuthSessionData(sessionData);

        try
        {
            var (principal, _) = await ValidateToken(jwt, lenientLifetimeValidation: true, cancellationToken)
                ?? throw new Exception("Failed to validate access token.");

            if (principal.FindFirst("sid")?.Value is not { } sessionId)
            {
                throw new Exception("No session ID found in access token.");
            }

            var frontendClient = MakeFrontendApiClient(authSession);
            await frontendClient.EndSessionAsync(sessionId, credentials, cancellationToken);
        }
        catch (Exception)
        {
        }
    }

    public AuthOption[] GetAuthOptions()
    {
        return _authOptions.ToArray();
    }

    public ClerkAuthProvider UseEmailPassword()
    {
        _authOptions.Add(new AuthOption(AuthFlow.EmailPassword));
        return this;
    }

    public ClerkAuthProvider UseGoogle()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Google", "google", Icons.Google));
        return this;
    }

    public ClerkAuthProvider UseGithub()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "GitHub", "github", Icons.Github));
        return this;
    }

    public ClerkAuthProvider UseTwitter()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Twitter", "twitter", Icons.Twitter));
        return this;
    }

    public ClerkAuthProvider UseApple()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Apple", "apple", Icons.Apple));
        return this;
    }

    public ClerkAuthProvider UseMicrosoft()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Microsoft", "microsoft", Icons.Microsoft));
        return this;
    }


    public async Task<BrokeredSessionsResult> GetBrokeredSessionsAsync(IAuthSession authSession, bool skipCache = false, CancellationToken cancellationToken = default)
    {
        // Return stored sessions if available and not skipping cache
        if (!skipCache && authSession.BrokeredSessions.Count > 0)
        {
            return BrokeredSessionsResult.Success(
                new Dictionary<string, IAuthTokenHandlerSession>(authSession.BrokeredSessions));
        }

        try
        {
            // Get user ID from the current session token
            if (await ValidateToken(authSession.AuthToken?.AccessToken, lenientLifetimeValidation: false, cancellationToken) is not var (claims, _))
            {
                return BrokeredSessionsResult.Failure();
            }

            var userId = claims.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BrokeredSessionsResult.Failure();
            }

            // Get user details to find their external accounts
            var user = await _backendClient.GetUserAsync(userId, cancellationToken);

            if (user?.ExternalAccounts == null || user.ExternalAccounts.Count == 0)
            {
                return BrokeredSessionsResult.Success([]);
            }

            var sessions = new Dictionary<string, IAuthTokenHandlerSession>();

            // Fetch brokered OAuth tokens for each external account
            foreach (var externalAccount in user.ExternalAccounts)
            {
                try
                {
                    // Clerk's Backend API uses "oauth_" prefix for OAuth providers
                    var providerForApi = externalAccount.Provider.StartsWith("oauth_")
                        ? externalAccount.Provider
                        : $"oauth_{externalAccount.Provider}";

                    // Clerk uses format like "oauth_google", "oauth_github", etc.
                    var provider = providerForApi.Replace("oauth_", "").ToLowerInvariant() switch
                    {
                        "google" => OAuthProviders.Google,
                        "github" => OAuthProviders.GitHub,
                        "microsoft" => OAuthProviders.Microsoft,
                        "apple" => OAuthProviders.Apple,
                        "twitter" => OAuthProviders.Twitter,
                        _ => (string?)null
                    };

                    if (provider == null)
                    {
                        continue; // Skip unsupported providers
                    }

                    var tokenResponse = await _backendClient.GetOAuthAccessTokenAsync(
                        userId,
                        providerForApi,
                        cancellationToken);

                    if (tokenResponse != null)
                    {
                        // Create the session
                        var session = new AuthTokenHandlerSession(authToken: new AuthToken(tokenResponse.Token));
                        sessions[provider] = session;
                    }
                }
                catch (Exception)
                {
                    // Skip this provider if we can't get the token
                    continue;
                }
            }

            return BrokeredSessionsResult.Success(sessions);
        }
        catch (Exception)
        {
            return BrokeredSessionsResult.Failure();
        }
    }
}