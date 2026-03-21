using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Ivy.Auth.Auth0;

public class Auth0AuthTokenHandler : IAuthTokenHandler
{
    protected readonly AuthenticationApiClient AuthClient;
    protected readonly string Domain;
    protected readonly string ClientId;
    protected readonly string ClientSecret;
    protected readonly string Audience;
    protected readonly string Namespace;
    protected readonly ConfigurationManager<OpenIdConnectConfiguration> ConfigurationManager;

    public Auth0AuthTokenHandler(IConfiguration configuration)
    {
        Domain = configuration.GetValue<string>("Auth0:Domain") ?? throw new Exception("Auth0:Domain is required");
        ClientId = configuration.GetValue<string>("Auth0:ClientId") ?? throw new Exception("Auth0:ClientId is required");
        ClientSecret = configuration.GetValue<string>("Auth0:ClientSecret") ?? throw new Exception("Auth0:ClientSecret is required");
        Audience = configuration.GetValue<string>("Auth0:Audience") ?? throw new Exception("Auth0:Audience is required");
        Namespace = configuration.GetValue<string>("Auth0:Namespace") ?? "https://ivy.app/";

        AuthClient = new AuthenticationApiClient(Domain);

        var authority = $"https://{Domain}/";
        var documentRetriever = new HttpDocumentRetriever { RequireHttps = true };
        ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"{authority}.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever(),
            documentRetriever
        );
    }

    public async Task<AuthToken?> RefreshAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
    {
        if (authSession.AuthToken is not { } token || token.RefreshToken == null)
        {
            return null;
        }

        try
        {
            var request = new RefreshTokenRequest
            {
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                RefreshToken = token.RefreshToken
            };

            var response = await AuthClient.GetTokenAsync(request, cancellationToken);
            return new AuthToken(response.AccessToken, response.RefreshToken ?? token.RefreshToken);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
    {
        return (await VerifyToken(authSession.AuthToken?.AccessToken, cancellationToken)) is not null;
    }

    public async Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
    {
        if (await VerifyToken(authSession.AuthToken?.AccessToken, cancellationToken) is not var (claims, _))
        {
            return null;
        }
        return new UserInfo(
            claims.FindFirst("sub")?.Value ?? "",
            claims.FindFirst(Namespace + "email")?.Value ?? "",
            claims.FindFirst(Namespace + "name")?.Value ?? "",
            claims.FindFirst(Namespace + "avatar")?.Value ?? ""
        );
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
            var discoveryDocument = await ConfigurationManager.GetConfigurationAsync(cancellationToken);
            var signingKeys = discoveryDocument.SigningKeys;

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = $"https://{Domain}/",
                ValidateAudience = true,
                ValidAudience = Audience,
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
}
