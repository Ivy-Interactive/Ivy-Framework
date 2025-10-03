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
        _audience = configuration.GetValue<string>("Auth0:Audience") ?? "";
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

    public async Task<AuthToken?> LoginAsync(string email, string password)
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

        var response = await _authClient.GetTokenAsync(request);
        return new AuthToken(response.AccessToken, response.RefreshToken, DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn));
    }

    public Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback)
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

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request)
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

            var response = await _authClient.GetTokenAsync(request_);

            return new AuthToken(response.AccessToken, response.RefreshToken, DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn));
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to exchange authorization code for tokens: {ex.Message}");
        }
    }

    public async Task LogoutAsync(string jwt)
    {
        try
        {
            // Auth0 logout is typically handled on the client side
            // This method can be extended to revoke tokens if needed
            await Task.CompletedTask;
        }
        catch (Exception)
        {
            // Logout failures are typically not critical
        }
    }

    public async Task<AuthToken?> RefreshJwtAsync(AuthToken jwt)
    {
        if (jwt.ExpiresAt == null || jwt.RefreshToken == null || DateTimeOffset.UtcNow < jwt.ExpiresAt)
        {
            return jwt;
        }

        try
        {
            var request = new RefreshTokenRequest
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                RefreshToken = jwt.RefreshToken
            };

            var response = await _authClient.GetTokenAsync(request);
            return new AuthToken(response.AccessToken, response.RefreshToken ?? jwt.RefreshToken, DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn));
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<ClaimsPrincipal?> VerifyToken(string jwt)
    {
        try
        {
            var discoveryDocument = await _configurationManager.GetConfigurationAsync(CancellationToken.None);
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
            };

            var handler = new JwtSecurityTokenHandler
            {
                InboundClaimTypeMap = new Dictionary<string, string>()
            };
            return handler.ValidateToken(jwt, tokenValidationParameters, out SecurityToken validatedToken);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> ValidateJwtAsync(string jwt)
    {
        var claims = await VerifyToken(jwt);
        return claims is not null;
    }

    public async Task<UserInfo?> GetUserInfoAsync(string jwt)
    {
        var claims = await VerifyToken(jwt);
        if (claims is null)
        {
            return null;
        }
        foreach (var claim in claims.Claims)
        {
            Console.WriteLine(claim.ToString());
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