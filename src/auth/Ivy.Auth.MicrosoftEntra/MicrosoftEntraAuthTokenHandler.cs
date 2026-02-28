using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Ivy.Auth.MicrosoftEntra;

[OAuthTokenHandler(OAuthProvider.Microsoft)]
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

    public MicrosoftEntraAuthTokenHandler(
        string tenantId,
        string clientId,
        string clientSecret,
        string[] scopes,
        ConfigurationManager<OpenIdConnectConfiguration> configurationManager)
    {
        TenantId = tenantId;
        ClientId = clientId;
        ClientSecret = clientSecret;
        Scopes = scopes;
        ConfigurationManager = configurationManager;
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

        foreach (var (key, token) in tokenCache.RefreshToken)
        {
            if (key.StartsWith(accountId))
            {
                return token.Secret;
            }
        }

        return null;
    }

    public async Task<AuthToken?> RefreshAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
    {
        var app = GetApp();

        if (authSession.AuthToken is not { } token)
        {
            return null;
        }

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
