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
    private List<(string user, string password)> _users = new();
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;

    /// <summary>
    /// Initializes a new instance of the BasicAuthProvider with configuration from environment variables and user secrets.
    /// </summary>
    public BasicAuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

        _secret = configuration["BasicAuth:JwtSecret"] ?? throw new Exception("BasicAuth:JwtSecret is required");
        _issuer = configuration["BasicAuth:JwtIssuer"] ?? "ivy";
        _audience = configuration["BasicAuth:JwtAudience"] ?? "ivy-app";

        var users = configuration.GetSection("BasicAuth:Users").Value ?? throw new Exception("BasicAuth:Users is required");
        foreach (var user in users.Split(';'))
        {
            var parts = user.Split(':');
            _users.Add((parts[0], parts[1]));
        }
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <param name="password">The user's password</param>
    /// <returns>An authentication token if successful, null otherwise</returns>
    public Task<AuthToken?> LoginAsync(string email, string password)
    {
        var found = _users.Any(u => u.user == email && u.password == password);
        if (!found) return Task.FromResult<AuthToken?>(null);

        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, email) };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        var authToken = jwt != null
            ? new AuthToken(jwt)
            : null;
        return Task.FromResult(authToken);
    }

    /// <summary>
    /// Logs out a user by invalidating their JWT token.
    /// </summary>
    /// <param name="jwt">The JWT token to invalidate</param>
    public Task LogoutAsync(string jwt)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Refreshes an expired or expiring JWT token.
    /// </summary>
    /// <param name="jwt">The current authentication token</param>
    /// <returns>A new authentication token if successful, null otherwise</returns>
    public Task<AuthToken?> RefreshJwtAsync(AuthToken jwt) => Task.FromResult<AuthToken?>(jwt);

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
    {
        var handler = new JwtSecurityTokenHandler();
        try
        {
            handler.ValidateToken(jwt, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Retrieves user information from a valid JWT token.
    /// </summary>
    /// <param name="jwt">The JWT token</param>
    /// <returns>User information if successful, null otherwise</returns>
    public Task<UserInfo?> GetUserInfoAsync(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var principal = handler.ValidateToken(jwt, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);
            var email = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Task.FromResult<UserInfo?>(new UserInfo(email!, email!, null, null));
        }
        catch
        {
            return Task.FromResult<UserInfo?>(null);
        }
    }

    /// <summary>
    /// Gets the available authentication options for this provider.
    /// </summary>
    /// <returns>Array of supported authentication options</returns>
    public AuthOption[] GetAuthOptions()
    {
        return [new AuthOption(AuthFlow.EmailPassword)];
    }
}