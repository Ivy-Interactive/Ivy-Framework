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

public class BasicAuthProvider : IAuthProvider
{
    private List<(string user, string password)> _users = new();
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;

    public BasicAuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

        _secret = configuration["JWT_SECRET"] ?? throw new Exception("JWT_SECRET is required");
        _issuer = configuration["JWT_ISSUER"] ?? "ivy";
        _audience = configuration["JWT_AUDIENCE"] ?? "ivy-app";

        var users = configuration.GetSection("USERS").Value ?? throw new Exception("USERS is required");
        foreach (var user in users.Split(';'))
        {
            var parts = user.Split(':');
            _users.Add((parts[0], parts[1]));
        }
    }

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

    public Task LogoutAsync(string jwt)
    {
        return Task.CompletedTask;
    }

    public Task<AuthToken?> RefreshJwtAsync(AuthToken jwt) => Task.FromResult<AuthToken?>(jwt);

    public Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback)
    {
        throw new NotImplementedException();
    }

    public Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request)
    {
        throw new NotImplementedException();
    }

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

    public AuthOption[] GetAuthOptions()
    {
        return [new AuthOption(AuthFlow.EmailPassword)];
    }
}