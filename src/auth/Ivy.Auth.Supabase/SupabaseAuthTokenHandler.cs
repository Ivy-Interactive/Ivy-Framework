using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Supabase.Gotrue;

namespace Ivy.Auth.Supabase;

public class SupabaseAuthTokenHandler : IAuthTokenHandler
{
    protected HttpClient HttpClient;
    protected readonly string Issuer;
    protected readonly string JwksUrl;
    protected readonly SymmetricSecurityKey? LegacyJwtKey;
    protected readonly global::Supabase.Client Client;

    private JsonWebKeySet? _cachedJwks = null;
    private DateTime _jwksCacheExpiry = DateTime.MinValue;

    public SupabaseAuthTokenHandler(IConfiguration configuration)
    {
        var url = configuration.GetValue<string>("Supabase:Url") ?? throw new Exception("Supabase:Url is required");
        var apiKey = configuration.GetValue<string>("Supabase:ApiKey") ?? throw new Exception("Supabase:ApiKey is required");

        Issuer = new Uri(new Uri(url), "auth/v1").ToString();
        JwksUrl = $"{Issuer}/.well-known/jwks.json";

        var legacyJwtSecret = configuration.GetValue<string?>("Supabase:LegacyJwtSecret");
        if (!string.IsNullOrEmpty(legacyJwtSecret))
        {
            var keyBytes = Encoding.UTF8.GetBytes(legacyJwtSecret);
            LegacyJwtKey = new SymmetricSecurityKey(keyBytes);
        }
        else
        {
            LegacyJwtKey = null;
        }

        var options = new global::Supabase.SupabaseOptions
        {
            AutoRefreshToken = false,
            AutoConnectRealtime = false
        };
        Client = new global::Supabase.Client(url, apiKey, options);

        var userAgent = AuthProviderHelpers.GetUserAgent(configuration, "Supabase:UserAgent");
        HttpClient = new HttpClient();
        HttpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
    }

    public async Task<AuthToken?> RefreshAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
    {
        if (authSession.AuthToken is not { } token || token.RefreshToken == null)
        {
            return null;
        }

        try
        {
            var session = await Client.Auth.SetSession(token.AccessToken, token.RefreshToken, forceAccessTokenRefresh: true)
                .WaitAsync(cancellationToken);
            var authToken = MakeAuthToken(session);
            return authToken;
        }
        catch (Exception)
        {
            return null;
        }
    }

    protected static AuthToken? MakeAuthToken(Session? session) =>
        session?.AccessToken != null
            ? new AuthToken(session.AccessToken, session.RefreshToken)
            : null;

    public async Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
    {
        return await VerifyToken(authSession.AuthToken?.AccessToken, cancellationToken) is not null;
    }

    public async Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
    {
        if (await VerifyToken(authSession.AuthToken?.AccessToken, cancellationToken) is not var (claims, _))
        {
            return null;
        }

        var userId = claims.FindFirst("sub")?.Value;
        var email = claims.FindFirst("email")?.Value;
        string? name = null, avatarUrl = null;

        var metadataJson = claims.FindFirst("user_metadata")?.Value;
        try
        {
            if (!string.IsNullOrEmpty(metadataJson))
            {
                using var doc = JsonDocument.Parse(metadataJson);
                var root = doc.RootElement;

                if (root.TryGetProperty("full_name", out var fullNameProperty))
                {
                    name = fullNameProperty.GetString();
                }
                if (root.TryGetProperty("avatar_url", out var avatarUrlProperty))
                {
                    avatarUrl = avatarUrlProperty.GetString();
                }
            }
        }
        catch (JsonException)
        {
            // Ignore JSON parsing errors
        }

        if (userId == null || email == null)
        {
            return null;
        }

        return new UserInfo(
            userId,
            email,
            name,
            avatarUrl
        );
    }

    public async Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
    {
        if (await VerifyToken(authSession.AuthToken?.AccessToken, cancellationToken) is var (_, expiration))
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

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = "authenticated",
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
            };

            var parsedToken = handler.ReadJwtToken(jwt);
            if (parsedToken.Header.Alg == SecurityAlgorithms.HmacSha256)
            {
                tokenValidationParameters.IssuerSigningKey = LegacyJwtKey;
            }
            else
            {
                // Check cache first
                if (_cachedJwks == null || DateTime.UtcNow >= _jwksCacheExpiry)
                {
                    var jwksJson = await HttpClient.GetStringAsync(JwksUrl, cancellationToken);
                    _cachedJwks = new JsonWebKeySet(jwksJson);
                    _jwksCacheExpiry = DateTime.UtcNow.AddHours(24);
                }

                if (_cachedJwks.Keys.Count == 0)
                {
                    return null;
                }

                tokenValidationParameters.IssuerSigningKeys = _cachedJwks.Keys;
            }

            var claims = handler.ValidateToken(jwt, tokenValidationParameters, out SecurityToken validatedToken);
            return (claims, validatedToken.ValidTo);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
