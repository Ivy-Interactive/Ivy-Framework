using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Ivy.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Ivy.Auth.Auth0;

public class Auth0OAuthException(string? error, string? errorDescription)
    : Exception($"Auth0 error: '{error}' - {errorDescription}")
{
    public string? Error { get; } = error;
    public string? ErrorDescription { get; } = errorDescription;
}

public class Auth0AuthProvider : IAuthProvider
{
    private readonly AuthenticationApiClient _authClient;
    private readonly string _domain;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _audience;
    private readonly string _namespace;
    private readonly List<AuthOption> _authOptions = new();

    private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
    private ManagementApiClient? _managementClient;
    private DateTime _managementTokenExpiry = DateTime.MinValue;

    public Auth0AuthProvider(IConfiguration configuration)
    {
        _domain = configuration.GetValue<string>("Auth0:Domain") ?? throw new Exception("Auth0:Domain is required");
        _clientId = configuration.GetValue<string>("Auth0:ClientId") ?? throw new Exception("Auth0:ClientId is required");
        _clientSecret = configuration.GetValue<string>("Auth0:ClientSecret") ?? throw new Exception("Auth0:ClientSecret is required");
        _audience = configuration.GetValue<string>("Auth0:Audience") ?? throw new Exception("Auth0:Audience is required");
        _namespace = configuration.GetValue<string>("Auth0:Namespace") ?? "https://ivy.app/";

        _authClient = new AuthenticationApiClient(_domain);

        // Setup OpenID configuration manager for JWKS
        var authority = $"https://{_domain}/";
        var documentRetriever = new HttpDocumentRetriever { RequireHttps = true };
        _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"{authority}.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever(),
            documentRetriever
        );
    }

    public async Task<AuthToken?> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken)
    {
        var request = new ResourceOwnerTokenRequest
        {
            ClientId = _clientId,
            ClientSecret = _clientSecret,
            Username = email,
            Password = password,
            Scope = "openid profile email",
            Audience = _audience,
            Realm = "Username-Password-Authentication",
        };

        var response = await _authClient.GetTokenAsync(request, cancellationToken);
        return new AuthToken(response.AccessToken, response.RefreshToken);
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
        var authorizationUrl = _authClient.BuildAuthorizationUrl()
            .WithResponseType(AuthorizationResponseType.Code)
            .WithClient(_clientId)
            .WithConnection(connection)
            .WithRedirectUrl(callbackUri.ToString())
            .WithScope("openid profile email")
            .WithState(callback.Id);

        if (!string.IsNullOrEmpty(_audience))
        {
            authorizationUrl = authorizationUrl.WithAudience(_audience);
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
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                Code = code,
                RedirectUri = $"{request.Scheme}://{request.Host}{request.Path}"
            };

            var response = await _authClient.GetTokenAsync(request_, cancellationToken);

            return new AuthToken(response.AccessToken, response.RefreshToken);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to exchange authorization code for tokens: {ex.Message}");
        }
    }

    public Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public async Task<AuthToken?> RefreshAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken)
    {
        if (authSession.AuthToken is not { } token || token.RefreshToken == null)
        {
            return null;
        }

        try
        {
            var request = new RefreshTokenRequest
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                RefreshToken = token.RefreshToken
            };

            var response = await _authClient.GetTokenAsync(request, cancellationToken);
            return new AuthToken(response.AccessToken, response.RefreshToken ?? token.RefreshToken);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<(ClaimsPrincipal, DateTimeOffset)?> VerifyToken(string? jwt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(jwt))
        {
            return null;
        }

        try
        {
            var discoveryDocument = await _configurationManager.GetConfigurationAsync(cancellationToken);
            var signingKeys = discoveryDocument.SigningKeys;

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = $"https://{_domain}/",
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
            };

            var handler = new JwtSecurityTokenHandler
            {
                InboundClaimTypeMap = new Dictionary<string, string>()
            };
            var claims = handler.ValidateToken(jwt, tokenValidationParameters, out SecurityToken validatedToken);
            return (claims, validatedToken.ValidTo);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> ValidateAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken)
    {
        return (await VerifyToken(authSession.AuthToken?.AccessToken, cancellationToken)) is not null;
    }

    public async Task<UserInfo?> GetUserInfoAsync(IAuthSession authSession, CancellationToken cancellationToken)
    {
        if (await VerifyToken(authSession.AuthToken?.AccessToken, cancellationToken) is not var (claims, _))
        {
            return null;
        }
        return new UserInfo(
            claims.FindFirst("sub")?.Value ?? "",
            claims.FindFirst(_namespace + "email")?.Value ?? "",
            claims.FindFirst(_namespace + "name")?.Value ?? "",
            claims.FindFirst(_namespace + "avatar")?.Value ?? ""
        );
    }

    public AuthOption[] GetAuthOptions()
    {
        return _authOptions.ToArray();
    }

    public async Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthSession authSession, CancellationToken cancellationToken)
    {
        if (authSession.AuthToken?.AccessToken is not { } accessToken)
        {
            return null;
        }

        if (await VerifyToken(accessToken, cancellationToken) is var (_, expiration))
        {
            return new TokenLifetime(expiration);
        }
        else
        {
            return null;
        }
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
            ClientId = _clientId,
            ClientSecret = _clientSecret,
            Audience = $"https://{_domain}/api/v2/"
        };

        var tokenResponse = await _authClient.GetTokenAsync(tokenRequest, cancellationToken);

        if (string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            throw new Exception("Failed to obtain Auth0 Management API token");
        }

        _managementClient = new ManagementApiClient(tokenResponse.AccessToken, _domain);
        // Tokens typically last 24 hours, refresh after 23 hours to be safe
        _managementTokenExpiry = DateTime.UtcNow.AddHours(23);

        return _managementClient;
    }

    public async Task<Dictionary<OAuthProvider, OAuthProviderToken>?> GetOAuthProviderTokensAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        // Get user ID from the current access token
        if (await VerifyToken(authSession.AuthToken?.AccessToken, cancellationToken) is not var (claims, _))
        {
            return null;
        }

        var userId = claims.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        // Get management API client
        var managementClient = await GetManagementClientAsync(cancellationToken);

        // Get user with identities
        var user = await managementClient.Users.GetAsync(userId);

        if (user.Identities == null || !user.Identities.Any())
        {
            return [];
        }

        var tokens = new Dictionary<OAuthProvider, OAuthProviderToken>();

        foreach (var identity in user.Identities)
        {
            // Skip non-OAuth identities (like auth0 username/password)
            if (identity.Connection == "Username-Password-Authentication" ||
                string.IsNullOrEmpty(identity.AccessToken))
            {
                continue;
            }

            OAuthProvider? provider = identity.Provider.ToLowerInvariant() switch
            {
                "google-oauth2" => OAuthProvider.Google,
                "github" => OAuthProvider.GitHub,
                "windowslive" => OAuthProvider.Microsoft,
                "apple" => OAuthProvider.Apple,
                "twitter" => OAuthProvider.Twitter,
                _ => null
            };

            if (provider == null)
            {
                continue; // Skip unsupported providers
            }

            tokens[provider.Value] = new OAuthProviderToken(
                Provider: provider.Value,
                AccessToken: identity.AccessToken);
        }

        return tokens;
    }
}