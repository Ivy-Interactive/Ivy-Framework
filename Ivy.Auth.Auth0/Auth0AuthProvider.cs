using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Ivy.Hooks;
using Ivy.Shared;
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

    public Auth0AuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

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

    public async Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken)
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

    public Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
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

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken)
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

    public Task LogoutAsync(string token, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public async Task<AuthToken?> RefreshAccessTokenAsync(AuthToken token, CancellationToken cancellationToken)
    {
        if (token.RefreshToken == null)
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

    private async Task<(ClaimsPrincipal, DateTimeOffset)?> VerifyToken(string jwt, CancellationToken cancellationToken)
    {
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

    public async Task<bool> ValidateAccessTokenAsync(string token, CancellationToken cancellationToken)
    {
        return (await VerifyToken(token, cancellationToken)) is not null;
    }

    public async Task<UserInfo?> GetUserInfoAsync(string token, CancellationToken cancellationToken)
    {
        if (await VerifyToken(token, cancellationToken) is not var (claims, _))
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

    public async Task<DateTimeOffset?> GetTokenExpiration(AuthToken token, CancellationToken cancellationToken)
    {
        if (await VerifyToken(token.AccessToken, cancellationToken) is var (_, expiration))
        {
            return expiration;
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
}