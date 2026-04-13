using System.Text;
using Ivy.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Isopoh.Cryptography.Argon2;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class BasicAuthProvider : BasicAuthTokenHandler, IAuthProvider
{
    private readonly List<(string user, string hash)> _users = [];
    private readonly byte[] _hashSecret;

    public BasicAuthProvider(IConfiguration configuration)
        : base(configuration)
    {
        var hashSecret = configuration["BasicAuth:HashSecret"] ?? throw new Exception("BasicAuth:HashSecret is required");

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
    }

    public Task<LoginResult> LoginAsync(IAuthSession authSession, string user, string password, CancellationToken cancellationToken)
    {
        var found = _users.Any(u => u.user == user && PasswordMatches(user, password, u.hash));
        if (!found) return Task.FromResult(LoginResult.InvalidCredentials());

        var now = DateTimeOffset.UtcNow;
        var authToken = CreateToken(user, now, now.ToUnixTimeSeconds());
        return Task.FromResult(LoginResult.Success(authToken));
    }

    private bool PasswordMatches(string username, string password, string hash)
    {
        return Argon2.Verify(hash, new Argon2Config
        {
            Password = Encoding.UTF8.GetBytes(password),
            Secret = _hashSecret,
        });
    }

    public Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken)
    {
        // No server-side state to invalidate
        return Task.CompletedTask;
    }

    public Task<Uri> GetOAuthUriAsync(IAuthSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<AuthToken?> HandleOAuthCallbackAsync(IAuthSession authSession, HttpRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public AuthOption[] GetAuthOptions() => [new AuthOption(AuthFlow.EmailPassword)];
}
