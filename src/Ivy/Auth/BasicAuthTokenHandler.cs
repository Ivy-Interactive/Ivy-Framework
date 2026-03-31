using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class BasicAuthTokenHandler : IAuthTokenHandler
{
    private static readonly TimeSpan ClockSkew = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromHours(24);
    private static readonly TimeSpan MaxSessionAge = TimeSpan.FromDays(365);

    protected readonly string Issuer;
    protected readonly string Audience;
    protected readonly SymmetricSecurityKey SigningKey;

    protected static string TokenUseClaim => "https://ivy.app/claims/token_use";

    public BasicAuthTokenHandler(IConfiguration configuration)
    {
        Issuer = configuration["BasicAuth:JwtIssuer"] ?? "ivy";
        Audience = configuration["BasicAuth:JwtAudience"] ?? "ivy-app";

        var jwtSecret = configuration["BasicAuth:JwtSecret"] ?? throw new Exception("BasicAuth:JwtSecret is required");
        try
        {
            var jwtSecretBytes = Convert.FromBase64String(jwtSecret);
            SigningKey = new SymmetricSecurityKey(jwtSecretBytes);
        }
        catch (FormatException)
        {
            throw new Exception("BasicAuth:JwtSecret is not a valid base64 string");
        }
    }

    public Task<AuthToken?> RefreshAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
    {
        // Check that refresh token is provided
        if (string.IsNullOrEmpty(authSession.AuthToken?.RefreshToken))
        {
            return Task.FromResult<AuthToken?>(null);
        }

        // Validate refresh token
        if (ValidateToken(authSession.AuthToken.RefreshToken, "oauth2/token", "refresh") is not var (principal, _))
        {
            return Task.FromResult<AuthToken?>(null);
        }

        if (principal.FindFirst("auth_time")?.Value is not { } authTimeString ||
            principal.FindFirst("max_age")?.Value is not { } maxAgeString ||
            !long.TryParse(authTimeString, out var authTime) ||
            !long.TryParse(maxAgeString, out var maxAge) ||
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value is not { } user)
        {
            // Missing or invalid required claims
            return Task.FromResult<AuthToken?>(null);
        }

        var now = DateTimeOffset.UtcNow;
        if (now.ToUnixTimeSeconds() > authTime + maxAge)
        {
            // Refresh token expired due to max_age
            return Task.FromResult<AuthToken?>(null);
        }

        var newToken = CreateToken(user, now, authTime);
        return Task.FromResult<AuthToken?>(newToken);
    }

    public Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
        => Task.FromResult(ValidateAccessToken(authSession.AuthToken?.AccessToken));

    public Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
    {
        if (ValidateToken(authSession.AuthToken?.AccessToken, Audience, "access") is not var (principal, _) ||
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value is not { } user)
        {
            return Task.FromResult<UserInfo?>(null);
        }

        return Task.FromResult<UserInfo?>(new UserInfo(user, user, null, null));
    }

    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
    {
        if (ValidateToken(authSession.AuthToken?.AccessToken, Audience, "access") is var (_, expiration))
        {
            return Task.FromResult<TokenLifetime?>(new TokenLifetime(expiration));
        }
        else
        {
            return Task.FromResult<TokenLifetime?>(null);
        }
    }

    protected bool ValidateAccessToken(string? token)
    {
        return ValidateToken(token, Audience, "access") != null;
    }

    protected (ClaimsPrincipal, DateTimeOffset)? ValidateToken(string? jwt, string audience, string tokenUse)
    {
        if (string.IsNullOrEmpty(jwt))
        {
            return null;
        }

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var claims = handler.ValidateToken(jwt, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = SigningKey,
                ValidateLifetime = true,
                ClockSkew = ClockSkew,
            }, out var validatedToken);
            if (claims.FindFirst(TokenUseClaim)?.Value != tokenUse)
            {
                return null;
            }
            return (claims, validatedToken.ValidTo);
        }
        catch
        {
            return null;
        }
    }

    protected AuthToken CreateToken(string user, DateTimeOffset now, long authTime)
    {
        var expiresAt = now.Add(AccessTokenLifetime);
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, user),
            new Claim(TokenUseClaim, "access"),
        };
        var creds = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: creds);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var rtExpiresAt = now.Add(RefreshTokenLifetime);
        var maxAgeSeconds = (long)MaxSessionAge.TotalSeconds;

        var rtClaims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user),
            new Claim(TokenUseClaim, "refresh"),
            new Claim("sid", Guid.NewGuid().ToString("n")),
            new Claim("auth_time", authTime.ToString()),
            new Claim("max_age", maxAgeSeconds.ToString())
        };

        var refreshJwt = new JwtSecurityToken(
            issuer: Issuer,
            audience: "oauth2/token",
            claims: rtClaims,
            notBefore: now.UtcDateTime,
            expires: rtExpiresAt.UtcDateTime,
            signingCredentials: creds
        );
        var refreshToken = new JwtSecurityTokenHandler().WriteToken(refreshJwt);

        return new AuthToken(accessToken, refreshToken);
    }
}
