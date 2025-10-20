using System.Reflection;
using System.Text.Json;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Ivy.Hooks;
using System.Text.Json.Serialization;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Ivy.Auth.MicrosoftEntra;

public class MicrosoftEntraOAuthException(string? error, string? errorCode, string? errorDescription)
    : Exception($"Microsoft Entra error: '{error}', code '{errorCode}' - {errorDescription}")
{
    public string? Error { get; } = error;
    public string? ErrorCode { get; } = errorCode;
    public string? ErrorDescription { get; } = errorDescription;
}

public class MicrosoftEntraAuthProvider : IAuthProvider
{
    private IConfidentialClientApplication? _app;
    private HttpContext? _httpContext = null;
    private readonly string[] _scopes = ["User.Read", "openid", "profile", "email", "offline_access"];

    private readonly List<AuthOption> _authOptions = [];
    TokenCache? _tokenCache = null;

    private string? _codeVerifier = null;

    private readonly string _tenantId;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

    record struct TokenCache(Dictionary<string, RefreshToken> RefreshToken);

    record struct RefreshToken([property: JsonPropertyName("secret")] string Secret);

    public MicrosoftEntraAuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

        _tenantId = configuration.GetValue<string>("MicrosoftEntra:TenantId") ?? throw new Exception("MicrosoftEntra:TenantId is required");
        _clientId = configuration.GetValue<string>("MicrosoftEntra:ClientId") ?? throw new Exception("MicrosoftEntra:ClientId is required");
        _clientSecret = configuration.GetValue<string>("MicrosoftEntra:ClientSecret") ?? throw new Exception("MicrosoftEntra:ClientSecret is required");

        _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"https://login.microsoftonline.com/{_tenantId}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever()
        );
    }

    public void SetHttpContext(HttpContext context) => _httpContext = context;

    private IConfidentialClientApplication GetApp()
    {
        if (_app != null)
        {
            return _app;
        }

        if (_httpContext == null)
        {
            throw new InvalidOperationException("SetHttpContext() must be called before GetApp()");
        }

        // Create a confidential client application for OAuth flow
        var baseUrl = WebhookEndpoint.BuildBaseUrl(_httpContext.Request.Scheme, _httpContext.Request.Host.Value!);

        _app = ConfidentialClientApplicationBuilder
            .Create(_clientId)
            .WithClientSecret(_clientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{_tenantId}"))
            .WithRedirectUri(baseUrl)
            .Build();

        _app.UserTokenCache.SetAfterAccess(args =>
        {
            var cacheBytes = args.TokenCache.SerializeMsalV3();
            _tokenCache = JsonSerializer.Deserialize<TokenCache>(cacheBytes);
        });

        return _app;
    }

    public Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken)
        => throw new InvalidOperationException("Microsoft Entra login with email/password is not supported");

    public async Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
    {
        _codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(_codeVerifier);

        var authUrl = await GetApp()
            .GetAuthorizationRequestUrl(_scopes)
            .WithRedirectUri(callback.GetUri(includeIdInPath: false).ToString())
            .WithExtraQueryParameters(new Dictionary<string, string>
            {
                ["code_challenge"] = codeChallenge,
                ["code_challenge_method"] = "S256",
                ["state"] = callback.Id,
            })
            .ExecuteAsync(cancellationToken);

        return authUrl;
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var code = request.Query["code"].ToString();
        var error = request.Query["error"].ToString();
        var errorDescription = request.Query["error_description"].ToString();

        if (error.Length > 0 || errorDescription.Length > 0)
        {
            throw new MicrosoftEntraOAuthException(error, null, errorDescription);
        }
        else if (code.Length == 0)
        {
            throw new Exception("Received no authorization code from Microsoft Entra.");
        }

        if (_codeVerifier == null)
        {
            throw new Exception("Code verifier not found. OAuth flow was not properly initiated.");
        }

        var result = await GetApp().AcquireTokenByAuthorizationCode(_scopes, code)
            .WithPkceCodeVerifier(_codeVerifier)
            .ExecuteAsync(cancellationToken);

        var accountId = result.Account.HomeAccountId!.Identifier;
        return new AuthToken(
            result.IdToken,
            GetCurrentRefreshToken(accountId),
            accountId
        );
    }

    public Task LogoutAsync(string token, CancellationToken cancellationToken)
    {
        _tokenCache = null;
        _app = null;

        return Task.CompletedTask;
    }

    public async Task<AuthToken?> RefreshAccessTokenAsync(AuthToken token, CancellationToken cancellationToken)
    {
        var app = GetApp();

        if (app is not IByRefreshToken refresher
            || token.Tag is not JsonElement tag
            || tag.GetString() is not string accountId
            || accountId.Length <= 0)
        {
            return null;
        }

        if (token.RefreshToken == null)
        {
            return null;
        }

        try
        {
            var account = await app.GetAccountAsync(accountId)
                .WaitAsync(cancellationToken);

            if (account != null)
            {
                if (account.HomeAccountId?.Identifier != accountId)
                {
                    throw new Exception("account ID does not match");
                }

                var result = await GetApp().AcquireTokenSilent(_scopes, account)
                    .ExecuteAsync(cancellationToken);

                if (result == null)
                {
                    return null;
                }

                return new AuthToken(
                    result.IdToken,
                    GetCurrentRefreshToken(accountId),
                    accountId
                );
            }
            else
            {
                var result = await refresher.AcquireTokenByRefreshToken(_scopes, token.RefreshToken)
                    .ExecuteAsync(cancellationToken);

                if (result == null)
                {
                    return null;
                }

                if (result.Account.HomeAccountId.Identifier != accountId)
                {
                    throw new Exception("account ID does not match");
                }

                return new AuthToken(
                    result.IdToken,
                    GetCurrentRefreshToken(accountId),
                    accountId
                );
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> ValidateAccessTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await VerifyToken(token, cancellationToken) is not null;
    }

    public Task<UserInfo?> GetUserInfoAsync(string idToken, CancellationToken cancellationToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(idToken);

            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "oid")?.Value
                ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value
                ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
            var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

            if (userId == null || email == null)
            {
                return Task.FromResult<UserInfo?>(null);
            }

            return Task.FromResult<UserInfo?>(new UserInfo(
                userId,
                email,
                name,
                null
            ));
        }
        catch (Exception)
        {
            return Task.FromResult<UserInfo?>(null);
        }
    }

    public AuthOption[] GetAuthOptions()
    {
        return [.. _authOptions];
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

    public MicrosoftEntraAuthProvider UseMicrosoftEntra()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Microsoft", "microsoft", Icons.Microsoft));
        return this;
    }

    private static string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(codeVerifier);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToBase64String(hash)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private string? GetCurrentRefreshToken(string accountId)
    {
        if (_tokenCache is not { } tokenCache)
        {
            return null;
        }

        foreach (var (key, token) in tokenCache.RefreshToken)
        {
            if (key.StartsWith(accountId))
            {
                return token.Secret;
            }
        }

        return null;
    }

    private async Task<(ClaimsPrincipal, DateTimeOffset)?> VerifyToken(string jwt, CancellationToken cancellationToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler
            {
                InboundClaimTypeMap = new Dictionary<string, string>()
            };

            var discoveryDocument = await _configurationManager.GetConfigurationAsync(cancellationToken);
            var signingKeys = discoveryDocument.SigningKeys;

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[]
                {
                    $"https://sts.windows.net/{_tenantId}/",
                    $"https://login.microsoftonline.com/{_tenantId}/v2.0"
                },
                ValidateAudience = true,
                ValidAudience = _clientId,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
            };

            var claims = handler.ValidateToken(jwt, tokenValidationParameters, out SecurityToken validatedToken);
            return (claims, validatedToken.ValidTo);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
