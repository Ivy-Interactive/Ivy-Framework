using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Ivy.Hooks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Ivy.Auth;

/// <summary>
/// Basic authentication provider that uses JWT tokens and in-memory user storage.
/// </summary>
public class BasicAuthProvider : IAuthProvider
{
    private readonly List<(string user, string password)> _users = [];
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SymmetricSecurityKey _signingKey;

    /// <summary>
    /// Initializes a new instance of the BasicAuthProvider with configuration from environment variables and user secrets.
    /// </summary>
    public BasicAuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

        var secret = configuration["BasicAuth:JwtSecret"] ?? throw new Exception("BasicAuth:JwtSecret is required");
        _issuer = configuration["BasicAuth:JwtIssuer"] ?? "ivy";
        _audience = configuration["BasicAuth:JwtAudience"] ?? "ivy-app";

        var users = configuration.GetSection("BasicAuth:Users").Value ?? throw new Exception("BasicAuth:Users is required");
        foreach (var user in users.Split(';'))
        {
            var parts = user.Split(':');
            _users.Add((parts[0], parts[1]));
        }

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }

    /// <summary>
    /// Authenticates a user with username and password.
    /// </summary>
    /// <param name="user">The user's username</param>
    /// <param name="password">The user's password</param>
    /// <returns>An authentication token if successful, null otherwise</returns>
    public Task<AuthToken?> LoginAsync(string user, string password)
    {
        var found = _users.Any(u => u.user == user && u.password == password);
        if (!found) return Task.FromResult<AuthToken?>(null);

        var now = DateTimeOffset.UtcNow;
        var authToken = CreateToken(user, now, now.ToUnixTimeSeconds());
        return Task.FromResult<AuthToken?>(authToken);
    }

    private AuthToken CreateToken(string user, DateTimeOffset now, long authTime)
    {
        var expiresAt = now.AddMinutes(15);
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user) };
        var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: creds);
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        var maxAgeSeconds = (long)TimeSpan.FromDays(365).TotalSeconds;
        var rtExpiresAt = now.AddHours(24);

        var rtClaims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user),
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

        return new AuthToken(jwt, refreshToken, expiresAt);
    }

    /// <summary>
    /// Logs out a user by invalidating their JWT token.
    /// </summary>
    /// <param name="jwt">The JWT token to invalidate</param>
    public Task LogoutAsync(string jwt)
    {
        // No server-side state to invalidate
        return Task.CompletedTask;
    }

    /// <summary>
    /// Refreshes an expired or expiring JWT token.
    /// </summary>
    /// <param name="jwt">The current authentication token</param>
    /// <returns>A new authentication token if successful, null otherwise</returns>
    public Task<AuthToken?> RefreshJwtAsync(AuthToken jwt)
    {
        // Check that refresh token is provided
        if (string.IsNullOrEmpty(jwt.RefreshToken))
        {
            return Task.FromResult<AuthToken?>(null);
        }

        if (ValidateJwt(jwt.Jwt))
        {
            // No need to refresh if current JWT is still valid
            return Task.FromResult<AuthToken?>(jwt);
        }

        // Validate refresh token
        if (ValidateToken(jwt.RefreshToken, "oauth2/token") is not { } principal)
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

        var token = CreateToken(user, now, authTime);
        return Task.FromResult<AuthToken?>(token);
    }

    /// <summary>
    /// Generates an OAuth authorization URI for the specified option.
    /// </summary>
    /// <param name="option">The OAuth authentication option</param>
    /// <param name="callback">The webhook endpoint for handling the OAuth callback</param>
    /// <returns>The OAuth authorization URI</returns>
    public Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handles the OAuth callback request and extracts the authentication token.
    /// </summary>
    /// <param name="request">The HTTP request containing OAuth callback data</param>
    /// <returns>An authentication token if successful, null otherwise</returns>
    public Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Validates whether a JWT token is still valid.
    /// </summary>
    /// <param name="jwt">The JWT token to validate</param>
    /// <returns>True if the token is valid, false otherwise</returns>
    public Task<bool> ValidateJwtAsync(string jwt)
        => Task.FromResult(ValidateJwt(jwt));

    /// <summary>
    /// Validates whether a JWT token is still valid.
    /// </summary>
    /// <param name="jwt">The JWT token to validate</param>
    /// <returns>True if the token is valid, false otherwise</returns>
    private bool ValidateJwt(string jwt)
    {
        return ValidateToken(jwt, _audience) != null;
    }

    /// <summary>
    /// Retrieves user information from a valid JWT token.
    /// </summary>
    /// <param name="jwt">The JWT token</param>
    /// <returns>User information if successful, null otherwise</returns>
    public Task<UserInfo?> GetUserInfoAsync(string jwt)
    {
        if (ValidateToken(jwt, _audience) is not { } principal ||
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value is not { } user)
        {
            return Task.FromResult<UserInfo?>(null);
        }

        return Task.FromResult<UserInfo?>(new UserInfo(user, user, null, null));
    }

    /// <summary>
    /// Gets the available authentication options for this provider.
    /// </summary>
    /// <returns>Array of supported authentication options</returns>
    public AuthOption[] GetAuthOptions()
    {
        return [new AuthOption(AuthFlow.EmailPassword)];
    }

    private ClaimsPrincipal? ValidateToken(string jwt, string audience)
    {
        var handler = new JwtSecurityTokenHandler();
        try
        {
            return handler.ValidateToken(jwt, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
            }, out _);
        }
        catch
        {
            return null;
        }
    }
}
