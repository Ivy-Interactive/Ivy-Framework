using Ivy.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Ivy.Auth.Clerk.ApiClient;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Ivy.Auth.Clerk.ApiClient.Models;
using Ivy.Auth.Clerk.ApiClient.Responses;
using System.Text.Json;

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
    private string? _origin = null;
    private string? _callbackBaseUrl = null;

    public bool OpenOAuthLoginInNewTab => true;

    private static (bool IsProduction, string Key) ParseKey(string name, string type, string key)
    {
        var tokens = key.Split('_', 3);
        if (tokens.Length != 3 || tokens[0] != type || (tokens[1] != "test" && tokens[1] != "live"))
        {
            throw new Exception($"{name} is invalid");
        }
        return (tokens[1] == "live", tokens[2]);
    }

    public ClerkAuthProvider(IConfiguration configuration)
        : base(GetFrontendApiDomain(configuration), GetIsProduction(configuration))
    {
        HttpClient = CreateHttpClient(configuration);
        _secretKey = configuration.GetValue<string>("Clerk:SecretKey") ?? throw new Exception("Clerk:SecretKey is required");

        _backendClient = new BackendApiClient(_secretKey);
    }

    private static bool GetIsProduction(IConfiguration configuration)
    {
        var secretKey = configuration.GetValue<string>("Clerk:SecretKey") ?? throw new Exception("Clerk:SecretKey is required");
        var publishableKey = configuration.GetValue<string>("Clerk:PublishableKey") ?? throw new Exception("Clerk:PublishableKey is required");

        var (secretIsProduction, _) = ParseKey("Clerk:SecretKey", "sk", secretKey);
        var (publishableIsProduction, _) = ParseKey("Clerk:PublishableKey", "pk", publishableKey);

        if (secretIsProduction != publishableIsProduction)
        {
            throw new Exception("Clerk:SecretKey and Clerk:PublishableKey must both be for the same environment (test or live)");
        }

        return secretIsProduction;
    }

    private static HttpClient CreateHttpClient(IConfiguration configuration)
    {
        var userAgent = AuthProviderHelpers.GetUserAgent(configuration, "Clerk:UserAgent");
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
        return httpClient;
    }

    private static string GetFrontendApiDomain(IConfiguration configuration)
    {
        var publishableKey = configuration.GetValue<string>("Clerk:PublishableKey") ?? throw new Exception("Clerk:PublishableKey is required");
        var (_, publishableKeyValue) = ParseKey("Clerk:PublishableKey", "pk", publishableKey);

        try
        {
            var base64Decoded = WebEncoders.Base64UrlDecode(publishableKeyValue);
            var base64DecodedString = Encoding.UTF8.GetString(base64Decoded);
            return base64DecodedString.Split('$', 2)[0];
        }
        catch (Exception ex)
        {
            throw new Exception("Clerk:PublishableKey contains an invalid base64 string", ex);
        }
    }

    private async Task<AuthToken?> TryRestoreExistingSessionAsync(IAuthProviderSession authSession, ClerkCredentials credentials, CancellationToken cancellationToken)
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

    public async Task InitializeAsync(IAuthProviderSession authSession, string requestScheme, string requestHost, CancellationToken cancellationToken = default)
    {
        _origin = $"{requestScheme}://{requestHost}";
        _callbackBaseUrl = WebhookEndpoint.BuildAuthCallbackBaseUrl(requestScheme, requestHost);

        var frontendClient = MakeFrontendApiClient(authSession);
        if (IsProduction)
        {
            await frontendClient.GetEnvironmentAsync(cancellationToken: cancellationToken);
            await GetClerkCredentialsAsync(authSession, includeSessionToken: true, cancellationToken: cancellationToken);
        }
        else
        {
            var credentials = await GetClerkCredentialsAsync(authSession, includeSessionToken: false, cancellationToken: cancellationToken);
            await frontendClient.UpdateEnvironmentAsync(credentials, _origin, cancellationToken);
        }
    }


    public async Task<AuthToken?> LoginAsync(IAuthProviderSession authSession, string email, string password, CancellationToken cancellationToken = default)
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
                    return restoredToken;
                }

                await frontendClient.RemoveAllSessionsAsync(credentials, cancellationToken);
                signInResponse = await frontendClient.CreatePasswordSignInAsync(credentials, email, password, cancellationToken);
            }

            if (signInResponse.Response?.CreatedSessionId is not { } sessionId)
            {
                return null;
            }

            var newToken = await frontendClient.CreateSessionTokenAsync(sessionId, credentials, cancellationToken);

            if (await ValidateToken(newToken.Jwt, lenientLifetimeValidation: false, cancellationToken) == null)
            {
                throw new Exception("New JWT from Clerk is invalid.");
            }

            return new AuthToken(newToken.Jwt!);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<Uri> GetOAuthUriAsync(IAuthProviderSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default)
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

    public async Task<AuthToken?> HandleOAuthCallbackAsync(IAuthProviderSession authSession, HttpRequest request, CancellationToken cancellationToken = default)
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

        if (signIn.Response?.Status == "needs_identifier" && signIn.Response.FirstFactorVerification?.Status == "transferable")
        {
            try
            {
                var redirectUrl = $"{_callbackBaseUrl}/{Guid.NewGuid()}";
                var signUpResponse = await frontendClient.CreateSignUpAsync(credentials, _origin, signIn.Response.FirstFactorVerification.Strategy, redirectUrl, redirectUrl, transfer: true, cancellationToken);

                if (signUpResponse.Response?.CreatedSessionId is { } newSessionId)
                {
                    await frontendClient.TouchSessionAsync(newSessionId, credentials, cancellationToken);
                    var newToken = await frontendClient.CreateSessionTokenAsync(newSessionId, credentials, cancellationToken);

                    if (await ValidateToken(newToken.Jwt, lenientLifetimeValidation: false, cancellationToken) == null)
                    {
                        throw new Exception("New JWT from Clerk is invalid.");
                    }

                    return new AuthToken(newToken.Jwt!);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Clerk OAuth Callback] Failed to transfer sign-in: {ex.Message}");
            }

            Console.WriteLine($"[Clerk OAuth Callback] Transferable sign-in could not be completed. User needs to sign up first.");
            return null;
        }

        try
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                Console.WriteLine($"[Clerk OAuth Callback] Sign-in ID: {pendingSignInId}");
                Console.WriteLine($"[Clerk OAuth Callback] Status: {signIn.Response?.Status}");
                Console.WriteLine($"[Clerk OAuth Callback] Created Session ID: {signIn.Response?.CreatedSessionId}");
                Console.WriteLine($"[Clerk OAuth Callback] First Factor Status: {signIn.Response?.FirstFactorVerification?.Status}");

                if (signIn.Response?.FirstFactorVerification?.Error is { } error)
                {
                    Console.WriteLine($"[Clerk OAuth Callback] Error Code: {error.Code}");
                    Console.WriteLine($"[Clerk OAuth Callback] Error Message: {error.Message}");
                    Console.WriteLine($"[Clerk OAuth Callback] Error Long Message: {error.LongMessage}");
                }
                return null;
            }
            else
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

    public async Task LogoutAsync(IAuthProviderSession authSession, CancellationToken cancellationToken = default)
    {
        var credentials = await GetClerkCredentialsAsync(authSession, cancellationToken: cancellationToken);
        var jwt = authSession.AuthToken?.AccessToken;

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


    public async Task<OAuthProviderSessionsResult> GetOAuthProviderSessionsAsync(IAuthProviderSession authSession, bool skipCache = false, CancellationToken cancellationToken = default)
    {
        // Return stored sessions if available and not skipping cache
        if (!skipCache && authSession.OAuthProviderSessions.Count > 0)
        {
            return OAuthProviderSessionsResult.Success(
                new Dictionary<string, IAuthTokenHandlerSession>(authSession.OAuthProviderSessions));
        }

        try
        {
            // Get user ID from the current session token
            if (await ValidateToken(authSession.AuthToken?.AccessToken, lenientLifetimeValidation: false, cancellationToken) is not var (claims, _))
            {
                return OAuthProviderSessionsResult.Failure();
            }

            var userId = claims.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return OAuthProviderSessionsResult.Failure();
            }

            // Get user details to find their external accounts
            var user = await _backendClient.GetUserAsync(userId, cancellationToken);

            if (user?.ExternalAccounts == null || user.ExternalAccounts.Count == 0)
            {
                return OAuthProviderSessionsResult.Success(new Dictionary<string, IAuthTokenHandlerSession>());
            }

            var sessions = new Dictionary<string, IAuthTokenHandlerSession>();

            // Fetch OAuth tokens for each external account
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
                        var session = new AuthTokenHandlerSession(new AuthToken(tokenResponse.Token), null);
                        sessions[provider] = session;
                    }
                }
                catch (Exception)
                {
                    // Skip this provider if we can't get the token
                    continue;
                }
            }

            return OAuthProviderSessionsResult.Success(sessions);
        }
        catch (Exception)
        {
            return OAuthProviderSessionsResult.Failure();
        }
    }
}