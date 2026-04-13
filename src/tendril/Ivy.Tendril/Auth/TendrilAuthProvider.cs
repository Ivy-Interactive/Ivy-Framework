using System.Text;
using Isopoh.Cryptography.Argon2;
using Ivy.Core;
using Ivy.Tendril.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Ivy.Tendril.Auth;

public class TendrilAuthProvider : BasicAuthTokenHandler, IAuthProvider
{
    private readonly string _passwordHash;
    private readonly byte[] _hashSecret;
    private readonly LoginRateLimiter _rateLimiter;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public TendrilAuthProvider(IConfigService configService, IHttpContextAccessor? httpContextAccessor = null)
        : base(BuildConfiguration(configService))
    {
        var auth = configService.Settings.Auth
            ?? throw new InvalidOperationException("Auth configuration is missing");

        _passwordHash = auth.Password;

        try
        {
            _hashSecret = Convert.FromBase64String(auth.HashSecret);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("Auth:HashSecret is not a valid base64 string");
        }

        _httpContextAccessor = httpContextAccessor;
        _rateLimiter = new LoginRateLimiter(auth.RateLimit ?? new LoginRateLimitConfig());
    }

    public Task<LoginResult> LoginAsync(IAuthSession authSession, string user, string password, CancellationToken cancellationToken)
    {
        var ipAddress = _httpContextAccessor?.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "global";

        if (!_rateLimiter.IsLoginAllowed(ipAddress))
        {
            var retryAfter = _rateLimiter.GetRequiredDelay(ipAddress);
            return Task.FromResult(LoginResult.RateLimited(retryAfter));
        }

        if (!PasswordMatches(password))
        {
            _rateLimiter.RecordFailedAttempt(ipAddress);
            return Task.FromResult(LoginResult.InvalidCredentials());
        }

        _rateLimiter.RecordSuccessfulLogin(ipAddress);

        var now = DateTimeOffset.UtcNow;
        var authToken = CreateToken("tendril", now, now.ToUnixTimeSeconds());
        return Task.FromResult(LoginResult.Success(authToken));
    }

    public Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken)
    {
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

    internal bool PasswordMatches(string password)
    {
        return Argon2.Verify(_passwordHash, new Argon2Config
        {
            Password = Encoding.UTF8.GetBytes(password),
            Secret = _hashSecret,
        });
    }

    private static IConfiguration BuildConfiguration(IConfigService configService)
    {
        var auth = configService.Settings.Auth
            ?? throw new InvalidOperationException("Auth configuration is missing");

        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BasicAuth:JwtSecret"] = auth.HashSecret,
                ["BasicAuth:JwtIssuer"] = "tendril",
                ["BasicAuth:JwtAudience"] = "tendril-app",
            })
            .Build();
    }
}
