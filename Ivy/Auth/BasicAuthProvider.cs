using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Ivy.Hooks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Isopoh.Cryptography.Argon2;

namespace Ivy.Auth;

/// <summary> Basic authentication provider that uses JWT tokens and in-memory user storage. </summary>
public class BasicAuthProvider : IAuthProvider
{
    private readonly List<(string user, string hash)> _users = [];
    private readonly string _issuer;
    private readonly string _audience;
    private readonly byte[] _hashSecret;
    private readonly SymmetricSecurityKey _signingKey;

    private static string TokenUseClaim => "https://ivy.app/claims/token_use";

    public BasicAuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

        var hashSecret = configuration["BasicAuth:HashSecret"] ?? throw new Exception("BasicAuth:HashSecret is required");
        var jwtSecret = configuration["BasicAuth:JwtSecret"] ?? throw new Exception("BasicAuth:JwtSecret is required");
        _issuer = configuration["BasicAuth:JwtIssuer"] ?? "ivy";
        _audience = configuration["BasicAuth:JwtAudience"] ?? "ivy-app";

        var users = configuration.GetSection("BasicAuth:Users").Value ?? throw new Exception("BasicAuth:Users is required");
        foreach (var user in users.Split(';'))
        {
            var parts = user.Split(':');
            _users.Add((parts[0], parts[1]));
        }

        try
        {
            _hashSecret = Convert.FromBase64String(hashSecret);
        }
        catch (FormatException)
        {
            throw new Exception("BasicAuth:HashSecret is not a valid base64 string");
        }

        try
        {
            var jwtSecretBytes = Convert.FromBase64String(jwtSecret);
            _signingKey = new SymmetricSecurityKey(jwtSecretBytes);
        }
        catch (FormatException)
        {
            throw new Exception("BasicAuth:JwtSecret is not a valid base64 string");
        }
    }

    public Task<AuthToken?> LoginAsync(string user, string password, CancellationToken cancellationToken)
    {
        var found = _users.Any(u => u.user == user && PasswordMatches(user, password, u.hash));
        if (!found) return Task.FromResult<AuthToken?>(null);

        var now = DateTimeOffset.UtcNow;
        var authToken = CreateToken(user, now, now.ToUnixTimeSeconds());
        return Task.FromResult<AuthToken?>(authToken);
    }

    private bool PasswordMatches(string username, string password, string hash)
    {
        return Argon2.Verify(hash, new Argon2Config
        {
            Password = Encoding.UTF8.GetBytes(password),
            Secret = _hashSecret,
        });
    }

    private AuthToken CreateToken(string user, DateTimeOffset now, long authTime)
    {
        var expiresAt = now.AddMinutes(15);
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, user),
            new Claim(TokenUseClaim, "access"),
        };
        var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: creds);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var rtExpiresAt = now.AddHours(24);
        var maxAgeSeconds = (long)TimeSpan.FromDays(365).TotalSeconds;

        var rtClaims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user),
            new Claim(TokenUseClaim, "refresh"),
            new Claim("sid", Guid.NewGuid().ToString("n")),
            new Claim("auth_time", authTime.ToString()),
            new Claim("max_age", maxAgeSeconds.ToString())
        };

        var refreshJwt = new JwtSecurityToken(
            issuer: _issuer,
            audience: "oauth2/token",
            claims: rtClaims,
            notBefore: now.UtcDateTime,
            expires: rtExpiresAt.UtcDateTime,
            signingCredentials: creds
        );
        var refreshToken = new JwtSecurityTokenHandler().WriteToken(refreshJwt);

        return new AuthToken(accessToken, refreshToken);
    }

    public Task LogoutAsync(string token, CancellationToken cancellationToken)
    {
        // No server-side state to invalidate
        return Task.CompletedTask;
    }

    /// <returns>A new authentication token if successful, null otherwise</returns>
    public Task<AuthToken?> RefreshAccessTokenAsync(AuthToken token, CancellationToken cancellationToken)
    {
        // Check that refresh token is provided
        if (string.IsNullOrEmpty(token.RefreshToken))
        {
            return Task.FromResult<AuthToken?>(null);
        }

        // Validate refresh token
        if (ValidateToken(token.RefreshToken, "oauth2/token", "refresh") is not var (principal, _))
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

    public Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <returns>An authentication token if successful, null otherwise</returns>
    public Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <returns>True if the token is valid, false otherwise</returns>
    public Task<bool> ValidateAccessTokenAsync(string token, CancellationToken cancellationToken)
        => Task.FromResult(ValidateAccessToken(token));

    private bool ValidateAccessToken(string token)
    {
        return ValidateToken(token, _audience, "access") != null;
    }

    public Task<UserInfo?> GetUserInfoAsync(string token, CancellationToken cancellationToken)
    {
        if (ValidateToken(token, _audience, "access") is not var (principal, _) ||
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value is not { } user)
        {
            return Task.FromResult<UserInfo?>(null);
        }

        return Task.FromResult<UserInfo?>(new UserInfo(user, user, null, null));
    }

    public AuthOption[] GetAuthOptions()
    {
        return [new AuthOption(AuthFlow.EmailPassword)];
    }

    public Task<DateTimeOffset?> GetTokenExpiration(AuthToken token, CancellationToken cancellationToken)
    {
        if (ValidateToken(token.AccessToken, _audience, "access") is var (_, expiration))
        {
            return Task.FromResult<DateTimeOffset?>(expiration);
        }
        else
        {
            return Task.FromResult<DateTimeOffset?>(null);
        }
    }

    private (ClaimsPrincipal, DateTimeOffset)? ValidateToken(string jwt, string audience, string tokenUse)
    {
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var claims = handler.ValidateToken(jwt, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
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
}
