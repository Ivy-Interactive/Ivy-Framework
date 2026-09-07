using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Ivy.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Ivy.Auth.Auth0;

public class Auth0OAuthException(string? error, string? errorDescription)
    : Exception($"Auth0 error: '{error}' - {errorDescription}")
{
    public string? Error { get; } = error;
    public string? ErrorDescription { get; } = errorDescription;
}

public class Auth0AuthProvider : Auth0AuthTokenHandler, IAuthProvider
{
    private readonly List<AuthOption> _authOptions = new();
    private ManagementApiClient? _managementClient;
    private DateTime _managementTokenExpiry = DateTime.MinValue;

    public Auth0AuthProvider(IConfiguration configuration)
        : base(configuration)
    {
    }

    public async Task<LoginResult> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken)
    {
        var request = new ResourceOwnerTokenRequest
        {
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            Username = email,
            Password = password,
            Scope = "openid profile email",
            Audience = Audience,
            Realm = "Username-Password-Authentication",
        };

        var response = await AuthClient.GetTokenAsync(request, cancellationToken);
        return LoginResult.Success(new AuthToken(response.AccessToken, response.RefreshToken));
    }

    public Task<Uri> GetOAuthUriAsync(IAuthSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
    {
        var connection = option.Id switch
        {
            "google-oauth2" => "google-oauth2",
            "github" => "github",
            "twitter" => "twitter",
            "microsoft" => "windowslive",
            "apple" => "apple",
            _ => throw new ArgumentException($"Unknown OAuth provider: {option.Id}"),
        };

        var callbackUri = callback.GetUri(includeIdInPath: false);
        var authorizationUrl = AuthClient.BuildAuthorizationUrl()
            .WithResponseType(AuthorizationResponseType.Code)
            .WithClient(ClientId)
            .WithConnection(connection)
            .WithRedirectUrl(callbackUri.ToString())
            .WithScope("openid profile email")
            .WithState(callback.Id);

        if (!string.IsNullOrEmpty(Audience))
        {
            authorizationUrl = authorizationUrl.WithAudience(Audience);
        }

        return Task.FromResult(authorizationUrl.Build());
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(IAuthSession authSession, HttpRequest request, CancellationToken cancellationToken)
    {
        var code = request.Query["code"].ToString();
        var error = request.Query["error"].ToString();
        var errorDescription = request.Query["error_description"].ToString();

        if (error.Length > 0 || errorDescription.Length > 0)
        {
            throw new Auth0OAuthException(error, errorDescription);
        }

        if (code.Length == 0)
        {
            throw new Exception("Received no authorization code from Auth0.");
        }

        try
        {
            var request_ = new AuthorizationCodeTokenRequest
            {
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                Code = code,
                RedirectUri = $"{request.Scheme}://{request.Host}{request.Path}"
            };

            var response = await AuthClient.GetTokenAsync(request_, cancellationToken);

            return new AuthToken(response.AccessToken, response.RefreshToken);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to exchange authorization code for tokens: {ex.Message}");
        }
    }

    public Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public AuthOption[] GetAuthOptions()
    {
        return _authOptions.ToArray();
    }

    public Auth0AuthProvider UseEmailPassword()
    {
        _authOptions.Add(new AuthOption(AuthFlow.EmailPassword));
        return this;
    }

    public Auth0AuthProvider UseGoogle()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Google", "google-oauth2", Icons.Google));
        return this;
    }

    public Auth0AuthProvider UseGithub()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "GitHub", "github", Icons.Github));
        return this;
    }

    public Auth0AuthProvider UseTwitter()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Twitter", "twitter", Icons.Twitter));
        return this;
    }

    public Auth0AuthProvider UseApple()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Apple", "apple", Icons.Apple));
        return this;
    }

    public Auth0AuthProvider UseMicrosoft()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Microsoft", "microsoft", Icons.Microsoft));
        return this;
    }

    private async Task<ManagementApiClient> GetManagementClientAsync(CancellationToken cancellationToken)
    {
        // Check if we have a valid management client
        if (_managementClient != null && DateTime.UtcNow < _managementTokenExpiry)
        {
            return _managementClient;
        }

        // Request a new management API token
        var tokenRequest = new ClientCredentialsTokenRequest
        {
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            Audience = $"https://{Domain}/api/v2/"
        };

        var tokenResponse = await AuthClient.GetTokenAsync(tokenRequest, cancellationToken);

        if (string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            throw new Exception("Failed to obtain Auth0 Management API token");
        }

        _managementClient = new ManagementApiClient(tokenResponse.AccessToken, Domain);
        // Tokens typically last 24 hours, refresh after 23 hours to be safe
        _managementTokenExpiry = DateTime.UtcNow.AddHours(23);

        return _managementClient;
    }

    public async Task<BrokeredSessionsResult> GetBrokeredSessionsAsync(IAuthSession authSession, bool skipCache = false, CancellationToken cancellationToken = default)
    {
        // Return stored sessions if available and not skipping cache
        if (!skipCache && authSession.BrokeredSessions.Count > 0)
        {
            return BrokeredSessionsResult.Success(
                new Dictionary<string, IAuthTokenHandlerSession>(authSession.BrokeredSessions));
        }

        // Get user ID from the current access token
        if (await VerifyToken(authSession.AuthToken?.AccessToken, cancellationToken) is not var (claims, _))
        {
            return BrokeredSessionsResult.Failure();
        }

        var userId = claims.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BrokeredSessionsResult.Failure();
        }

        // Get management API client
        var managementClient = await GetManagementClientAsync(cancellationToken);

        // Get user with identities
        var user = await managementClient.Users.GetAsync(userId);

        if (user.Identities == null || !user.Identities.Any())
        {
            return BrokeredSessionsResult.Success(new Dictionary<string, IAuthTokenHandlerSession>());
        }

        var sessions = new Dictionary<string, IAuthTokenHandlerSession>();

        foreach (var identity in user.Identities)
        {
            // Skip non-OAuth identities (like auth0 username/password)
            if (identity.Connection == "Username-Password-Authentication" ||
                string.IsNullOrEmpty(identity.AccessToken))
            {
                continue;
            }

            string? provider = identity.Provider.ToLowerInvariant() switch
            {
                "google-oauth2" => OAuthProviders.Google,
                "github" => OAuthProviders.GitHub,
                "windowslive" => OAuthProviders.Microsoft,
                "apple" => OAuthProviders.Apple,
                "twitter" => OAuthProviders.Twitter,
                _ => null
            };

            if (provider == null)
            {
                continue; // Skip unsupported providers
            }

            // Create the session
            var session = new AuthTokenHandlerSession(authToken: new AuthToken(identity.AccessToken));
            sessions[provider] = session;
        }

        return BrokeredSessionsResult.Success(sessions);
    }
}