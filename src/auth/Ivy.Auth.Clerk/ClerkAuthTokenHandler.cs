using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ivy.Auth.Clerk.ApiClient;
using Ivy.Auth.Clerk.ApiClient.Models;
using Ivy.Core;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Ivy.Auth.Clerk;

public class ClerkAuthTokenHandler : IAuthTokenHandler
{
    protected HttpClient HttpClient;
    protected readonly string FrontendApiDomain;
    protected readonly bool IsProduction;
    protected string? _origin = null;
    protected string? _callbackBaseUrl = null;

    private ICollection<SecurityKey>? _signingKeys;
    private DateTime _signingKeysLastFetched = DateTime.MinValue;

    private readonly HttpMessageHandler _defaultHttpMessageHandler = new HttpClientHandler();

    private static (bool IsProduction, string Key) ParseKey(string name, string type, string key)
    {
        var tokens = key.Split('_', 3);
        if (tokens.Length != 3 || tokens[0] != type || (tokens[1] != "test" && tokens[1] != "live"))
        {
            throw new Exception($"{name} is invalid");
        }
        return (tokens[1] == "live", tokens[2]);
    }

    public ClerkAuthTokenHandler(IConfiguration configuration)
    {
        var secretKey = configuration.GetValue<string>("Clerk:SecretKey") ?? throw new Exception("Clerk:SecretKey is required");
        var publishableKey = configuration.GetValue<string>("Clerk:PublishableKey") ?? throw new Exception("Clerk:PublishableKey is required");

        IsProduction = ProcessHelper.IsProduction();

        var (secretIsProduction, _) = ParseKey("Clerk:SecretKey", "sk", secretKey);
        var (publishableIsProduction, _) = ParseKey("Clerk:PublishableKey", "pk", publishableKey);

        if (secretIsProduction != publishableIsProduction)
        {
            throw new Exception("Clerk:SecretKey and Clerk:PublishableKey must both be for the same environment (test or live)");
        }

        if (secretIsProduction != IsProduction)
        {
            throw new Exception($"Clerk:SecretKey and Clerk:PublishableKey environment ({(secretIsProduction ? "live" : "test")}) does not match IVY_ENVIRONMENT ({(IsProduction ? "Production" : "Development")})");
        }

        var (_, publishableKeyValue) = ParseKey("Clerk:PublishableKey", "pk", publishableKey);
        try
        {
            var base64Decoded = WebEncoders.Base64UrlDecode(publishableKeyValue);
            var base64DecodedString = Encoding.UTF8.GetString(base64Decoded);
            FrontendApiDomain = base64DecodedString.Split('$', 2)[0];
        }
        catch (Exception ex)
        {
            throw new Exception("Clerk:PublishableKey contains an invalid base64 string", ex);
        }

        var userAgent = AuthProviderHelpers.GetUserAgent(configuration, "Clerk:UserAgent");
        HttpClient = new HttpClient();
        HttpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
    }

    public async Task InitializeAsync(IAuthTokenHandlerSession authSession, string requestScheme, string requestHost, CancellationToken cancellationToken = default)
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

    public async Task<AuthToken?> RefreshAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = authSession.AuthToken;
            var credentials = await GetClerkCredentialsAsync(authSession, cancellationToken: cancellationToken);

            var (principal, _) = await ValidateToken(token?.AccessToken, lenientLifetimeValidation: true, cancellationToken)
                ?? throw new Exception("Failed to validate access token during token refresh.");

            if (principal.FindFirst("sid")?.Value is not { } sessionId)
            {
                throw new Exception("No session ID found in access token.");
            }

            var frontendClient = MakeFrontendApiClient(authSession);
            var newToken = await frontendClient.CreateSessionTokenAsync(sessionId, credentials, cancellationToken);
            if (await ValidateToken(newToken.Jwt, lenientLifetimeValidation: false, cancellationToken) == null)
            {
                throw new Exception("New JWT from Clerk is invalid.");
            }
            else
            {
                return new AuthToken(newToken.Jwt!);
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    protected FrontendApiClient MakeFrontendApiClient(IAuthTokenHandlerSession authSession)
        => new(FrontendApiDomain, authSession.TunneledHttpMessageHandler ?? _defaultHttpMessageHandler);

    protected static ClerkSession? GetActiveSession(ClerkClient client)
    {
        var activeSessions = client.Sessions.Where(session => session.Status == "active");

        // Prefer the last active session, but don't force it
        return activeSessions.FirstOrDefault(session => session.Id == client.LastActiveSessionId)
            ?? activeSessions.FirstOrDefault();
    }

    protected Task<ClerkCredentials> GetClerkCredentialsAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
        => GetClerkCredentialsAsync(authSession, includeSessionToken: false, cancellationToken);

    protected async Task<ClerkCredentials> GetClerkCredentialsAsync(IAuthTokenHandlerSession authSession, bool includeSessionToken, CancellationToken cancellationToken)
    {
        var credentials = new ClerkCredentials();

        var frontendClient = MakeFrontendApiClient(authSession);

        if (IsProduction)
        {
            if (!includeSessionToken || await ValidateToken(authSession.AuthToken?.AccessToken, lenientLifetimeValidation: false, cancellationToken) == null)
            {
                if (await GetActiveSession(frontendClient, credentials, cancellationToken) is { } session)
                {
                    credentials.Session = session;
                    if (includeSessionToken)
                    {
                        authSession.AuthToken = new AuthToken(session.LastActiveToken.Jwt);
                    }
                }
            }
        }
        else
        {
            var sessionData = authSession.GetAuthSessionData<ClerkAuthSessionData>() ?? new();

            if (sessionData.DevBrowserToken is { } devBrowserToken)
            {
                credentials.DevBrowserToken = devBrowserToken;
            }
            else
            {
                var devBrowserTokenResponse = await frontendClient.CreateDevBrowserTokenAsync(cancellationToken);
                sessionData.DevBrowserToken = devBrowserTokenResponse.Id;
                authSession.SetAuthSessionData(sessionData);
                credentials.DevBrowserToken = devBrowserTokenResponse.Id;
            }
        }

        if (includeSessionToken && credentials.SessionToken == null)
        {
            credentials.SessionToken = authSession.AuthToken?.AccessToken;
        }

        return credentials;
    }

    protected async Task<ClerkSession?> GetActiveSession(FrontendApiClient frontendClient, ClerkCredentials credentials, CancellationToken cancellationToken)
    {
        var clientResponse = await frontendClient.GetCurrentClientAsync(credentials, cancellationToken);
        if (clientResponse.Response is { } client &&
            GetActiveSession(client) is { } session &&
            session?.LastActiveToken.Jwt is { } sessionToken &&
            await ValidateToken(sessionToken, lenientLifetimeValidation: false, cancellationToken) != null)
        {
            return session;
        }
        else
        {
            return null;
        }
    }

    public async Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        return (await ValidateToken(authSession.AuthToken?.AccessToken, lenientLifetimeValidation: false, cancellationToken)) is not null;
    }

    public async Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        if (await ValidateToken(authSession.AuthToken?.AccessToken, lenientLifetimeValidation: false, cancellationToken) is not var (claims, _))
        {
            return null;
        }

        return new UserInfo(
            claims.FindFirst("sub")?.Value.NullIfEmpty() ?? "",
            claims.FindFirst("email")?.Value.NullIfEmpty() ?? claims.FindFirst("username")?.Value.NullIfEmpty() ?? "",
            claims.FindFirst("full_name")?.Value.NullIfEmpty(),
            claims.FindFirst("has_image")?.Value != "false"
                ? claims.FindFirst("image_url")?.Value.NullIfEmpty()
                : null
        );
    }

    public async Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        if (await ValidateToken(authSession.AuthToken?.AccessToken, lenientLifetimeValidation: true, cancellationToken) is var (_, lifetime))
        {
            return lifetime;
        }
        else
        {
            return null;
        }
    }

    protected async Task<ICollection<SecurityKey>> GetSigningKeysAsync(CancellationToken cancellationToken)
    {
        // Cache keys for 1 hour
        if (_signingKeys != null && DateTime.UtcNow - _signingKeysLastFetched < TimeSpan.FromHours(1))
        {
            return _signingKeys;
        }

        var jwksUrl = $"https://{FrontendApiDomain}/.well-known/jwks.json";
        var jwksJson = await HttpClient.GetStringAsync(jwksUrl, cancellationToken);
        var jwks = new JsonWebKeySet(jwksJson);

        _signingKeys = jwks.GetSigningKeys();
        _signingKeysLastFetched = DateTime.UtcNow;

        return _signingKeys;
    }

    protected async Task<(ClaimsPrincipal, TokenLifetime)?> ValidateToken(string? jwt, bool lenientLifetimeValidation, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(jwt))
        {
            return null;
        }

        var signingKeys = await GetSigningKeysAsync(cancellationToken);

        var handler = new JwtSecurityTokenHandler
        {
            MapInboundClaims = false
        };
        try
        {
            var principal = handler.ValidateToken(jwt, new TokenValidationParameters
            {
                TryAllIssuerSigningKeys = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                ValidateIssuer = true,
                ValidIssuer = $"https://{FrontendApiDomain}",
                ValidateAudience = false,
                ClockSkew = lenientLifetimeValidation
                    ? TimeSpan.FromDays(1)
                    : TimeSpan.Zero,
            }, out SecurityToken validatedToken);

            return (principal, new TokenLifetime(validatedToken.ValidTo, validatedToken.ValidFrom));
        }
        catch (Exception)
        {
            return null;
        }
    }
}
