using System.Text;
using Isopoh.Cryptography.Argon2;
using Ivy.Tendril.Auth;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test.Auth;

public class TendrilAuthProviderTests
{
    private static AuthConfig CreateAuthConfig(string password = "test-password", LoginRateLimitConfig? rateLimit = null)
    {
        var secret = GenerateSecret();
        var secretBytes = Convert.FromBase64String(secret);
        var salt = new byte[16];
        System.Security.Cryptography.RandomNumberGenerator.Fill(salt);
        var hash = Argon2.Hash(new Argon2Config
        {
            Type = Argon2Type.DataIndependentAddressing,
            Version = Argon2Version.Nineteen,
            TimeCost = 3,
            MemoryCost = 65536,
            Lanes = 1,
            Threads = 1,
            Password = Encoding.UTF8.GetBytes(password),
            Salt = salt,
            Secret = secretBytes,
            HashLength = 32
        });

        return new AuthConfig { Password = hash, HashSecret = secret, RateLimit = rateLimit };
    }

    private static string GenerateSecret()
    {
        var bytes = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static TendrilAuthProvider CreateProvider(AuthConfig auth)
    {
        var settings = new TendrilSettings { Auth = auth };
        var configService = new ConfigService(settings);
        return new TendrilAuthProvider(configService);
    }

    [Fact]
    public void PasswordMatches_CorrectPassword_ReturnsTrue()
    {
        var auth = CreateAuthConfig("my-secret-password");
        var provider = CreateProvider(auth);

        Assert.True(provider.PasswordMatches("my-secret-password"));
    }

    [Fact]
    public void PasswordMatches_IncorrectPassword_ReturnsFalse()
    {
        var auth = CreateAuthConfig("my-secret-password");
        var provider = CreateProvider(auth);

        Assert.False(provider.PasswordMatches("wrong-password"));
    }

    [Fact]
    public async Task LoginAsync_CorrectPassword_ReturnsSuccess()
    {
        var auth = CreateAuthConfig("login-test");
        var provider = CreateProvider(auth);

        var result = await provider.LoginAsync(null!, "anyone", "login-test", CancellationToken.None);

        Assert.Equal(LoginStatus.Success, result.Status);
        Assert.NotNull(result.Token);
        Assert.False(string.IsNullOrEmpty(result.Token.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.Token.RefreshToken));
    }

    [Fact]
    public async Task LoginAsync_IncorrectPassword_ReturnsInvalidCredentials()
    {
        var auth = CreateAuthConfig("login-test");
        var provider = CreateProvider(auth);

        var result = await provider.LoginAsync(null!, "anyone", "bad-password", CancellationToken.None);

        Assert.Equal(LoginStatus.InvalidCredentials, result.Status);
        Assert.Null(result.Token);
    }

    [Fact]
    public void Constructor_MissingAuthConfig_Throws()
    {
        var settings = new TendrilSettings { Auth = null };
        var configService = new ConfigService(settings);

        Assert.Throws<InvalidOperationException>(() => new TendrilAuthProvider(configService));
    }

    [Fact]
    public void Constructor_InvalidHashSecret_Throws()
    {
        var auth = new AuthConfig { Password = "hash", HashSecret = "not-valid-base64!!!" };
        var settings = new TendrilSettings { Auth = auth };
        var configService = new ConfigService(settings);

        Assert.ThrowsAny<Exception>(() => new TendrilAuthProvider(configService));
    }

    [Fact]
    public void GetAuthOptions_ReturnsEmailPassword()
    {
        var auth = CreateAuthConfig();
        var provider = CreateProvider(auth);

        var options = provider.GetAuthOptions();

        Assert.Single(options);
        Assert.Equal(Ivy.AuthFlow.EmailPassword, options[0].Flow);
    }

    [Fact]
    public async Task LoginAsync_RateLimited_ReturnsRateLimitedWithRetryAfter()
    {
        var rateLimit = new LoginRateLimitConfig { Threshold = 1, BaseDelaySeconds = 100.0 };
        var auth = CreateAuthConfig("test-pass", rateLimit);
        var provider = CreateProvider(auth);

        // Two failed attempts to exceed threshold
        await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);
        await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);

        // Even correct password should be blocked
        var result = await provider.LoginAsync(null!, "anyone", "test-pass", CancellationToken.None);
        Assert.Equal(LoginStatus.RateLimited, result.Status);
        Assert.Null(result.Token);
        Assert.NotNull(result.RetryAfter);
        Assert.True(result.RetryAfter.Value.TotalSeconds > 0);
    }

    [Fact]
    public async Task LoginAsync_FailedAttempts_TriggersRateLimit()
    {
        var rateLimit = new LoginRateLimitConfig { Threshold = 2, BaseDelaySeconds = 100.0 };
        var auth = CreateAuthConfig("test-pass", rateLimit);
        var provider = CreateProvider(auth);

        // First 2 attempts are under threshold
        var t1 = await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);
        Assert.Equal(LoginStatus.InvalidCredentials, t1.Status);
        var t2 = await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);
        Assert.Equal(LoginStatus.InvalidCredentials, t2.Status);

        // 3rd attempt exceeds threshold — rate limited
        var t3 = await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);
        Assert.Equal(LoginStatus.RateLimited, t3.Status);

        // Correct password is also blocked
        var t4 = await provider.LoginAsync(null!, "anyone", "test-pass", CancellationToken.None);
        Assert.Equal(LoginStatus.RateLimited, t4.Status);
    }

    [Fact]
    public async Task LoginAsync_SuccessfulLogin_ClearsRateLimit()
    {
        var rateLimit = new LoginRateLimitConfig { Threshold = 2, BaseDelaySeconds = 0.0 };
        var auth = CreateAuthConfig("test-pass", rateLimit);
        var provider = CreateProvider(auth);

        // Fail a few times
        await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);
        await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);

        // Successful login resets counter
        var result = await provider.LoginAsync(null!, "anyone", "test-pass", CancellationToken.None);
        Assert.Equal(LoginStatus.Success, result.Status);
        Assert.NotNull(result.Token);

        // Subsequent login should work (counter was reset)
        var result2 = await provider.LoginAsync(null!, "anyone", "test-pass", CancellationToken.None);
        Assert.Equal(LoginStatus.Success, result2.Status);
        Assert.NotNull(result2.Token);
    }

    [Fact]
    public async Task LoginAsync_CustomRateLimitConfig_Respected()
    {
        var rateLimit = new LoginRateLimitConfig { Threshold = 1, BaseDelaySeconds = 100.0, MaxDelaySeconds = 200.0 };
        var auth = CreateAuthConfig("test-pass", rateLimit);
        var provider = CreateProvider(auth);

        // 1 failure is under threshold
        await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);
        var t1 = await provider.LoginAsync(null!, "anyone", "test-pass", CancellationToken.None);
        Assert.Equal(LoginStatus.Success, t1.Status);
        Assert.NotNull(t1.Token);

        // After success, counter is reset. Fail again to trigger rate limit.
        await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);
        await provider.LoginAsync(null!, "anyone", "wrong", CancellationToken.None);

        // Now blocked
        var t2 = await provider.LoginAsync(null!, "anyone", "test-pass", CancellationToken.None);
        Assert.Equal(LoginStatus.RateLimited, t2.Status);
        Assert.Null(t2.Token);
    }
}
