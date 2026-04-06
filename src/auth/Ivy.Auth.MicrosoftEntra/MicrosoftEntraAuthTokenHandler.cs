using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ivy.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Ivy.Auth.MicrosoftEntra;

public class MicrosoftEntraAuthTokenHandler : IAuthTokenHandler
{
    protected readonly string TenantId;
    protected readonly string ClientId;
    protected readonly string ClientSecret;
    protected readonly string[] Scopes;
    protected readonly ConfigurationManager<OpenIdConnectConfiguration> ConfigurationManager;

    private IConfidentialClientApplication? _app;
    private string? _baseUrl = null;
    private TokenCache? _tokenCache = null;

    record struct TokenCache(Dictionary<string, RefreshToken> RefreshToken);
    record struct RefreshToken([property: JsonPropertyName("secret")] string Secret);

    public MicrosoftEntraAuthTokenHandler(IConfiguration configuration)
    {
        TenantId = configuration.GetValue<string>("MicrosoftEntra:TenantId") ?? throw new Exception("MicrosoftEntra:TenantId is required");
        ClientId = configuration.GetValue<string>("MicrosoftEntra:ClientId") ?? throw new Exception("MicrosoftEntra:ClientId is required");
        ClientSecret = configuration.GetValue<string>("MicrosoftEntra:ClientSecret") ?? throw new Exception("MicrosoftEntra:ClientSecret is required");
        Scopes = ["User.Read", "openid", "profile", "email", "offline_access"];
        ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"https://login.microsoftonline.com/{TenantId}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever()
        );
    }

    public Task InitializeAsync(IAuthTokenHandlerSession authSession, string requestScheme, string requestHost, string? basePath = null, CancellationToken cancellationToken = default)
    {
        var baseUrl = WebhookEndpoint.BuildAuthCallbackBaseUrl(requestScheme, requestHost, basePath);
        SetBaseUrl(baseUrl);
        return Task.CompletedTask;
    }

    public void SetBaseUrl(string baseUrl)
    {
        _baseUrl = baseUrl;
    }

    public void ClearApp()
    {
        _tokenCache = null;
        _app = null;
    }

    protected IConfidentialClientApplication GetApp()
    {
        if (_app != null)
        {
            return _app;
        }

        if (_baseUrl == null)
        {
            throw new InvalidOperationException("SetBaseUrl() must be called before GetApp()");
        }

        // Create a confidential client application for OAuth flow
        _app = ConfidentialClientApplicationBuilder
            .Create(ClientId)
            .WithClientSecret(ClientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{TenantId}"))
            .WithRedirectUri(_baseUrl)
            .Build();

        _app.UserTokenCache.SetAfterAccess(args =>
        {
            var cacheBytes = args.TokenCache.SerializeMsalV3();
            _tokenCache = JsonSerializer.Deserialize<TokenCache>(cacheBytes);
        });

        return _app;
    }

    protected string? GetCurrentRefreshToken(string accountId)
    {
        if (_tokenCache is not { } tokenCache)
        {
            return null;
        }

        if (tokenCache.RefreshToken.TryGetValue($"{accountId}-login.windows.net-refreshtoken-{ClientId}--", out var token))
        {
            return token.Secret;
        }
        else
        {
            return null;
        }
    }

    public async Task<AuthToken?> RefreshAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
    {
        var app = GetApp();

        if (authSession.AuthToken is not { } token)
        {
            return null;
        }

        var accountId = token.Tag;
        if (app is not IByRefreshToken refresher || string.IsNullOrEmpty(accountId))
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

                var result = await GetApp().AcquireTokenSilent(Scopes, account)
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
                var result = await refresher.AcquireTokenByRefreshToken(Scopes, token.RefreshToken)
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

    public async Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
    {
        return await VerifyToken(authSession.AuthToken?.AccessToken, cancellationToken) is not null;
    }

    public Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
    {
        if (authSession.AuthToken?.AccessToken is not { } idToken)
        {
            return Task.FromResult<UserInfo?>(null);
        }

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

    public async Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
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

    protected async Task<(ClaimsPrincipal, DateTimeOffset)?> VerifyToken(string? jwt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(jwt))
        {
            return null;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler
            {
                InboundClaimTypeMap = new Dictionary<string, string>()
            };

            var discoveryDocument = await ConfigurationManager.GetConfigurationAsync(cancellationToken);
            var signingKeys = discoveryDocument.SigningKeys;

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[]
                {
                    $"https://sts.windows.net/{TenantId}/",
                    $"https://login.microsoftonline.com/{TenantId}/v2.0"
                },
                ValidateAudience = true,
                ValidAudience = ClientId,
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
