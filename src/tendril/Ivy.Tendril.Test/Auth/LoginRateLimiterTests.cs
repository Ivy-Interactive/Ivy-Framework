using Ivy.Tendril.Auth;

namespace Ivy.Tendril.Test.Auth;

public class LoginRateLimiterTests
{
    private static LoginRateLimiter CreateLimiter(int threshold = 3, double baseDelay = 1.0, double maxDelay = 60.0)
    {
        return new LoginRateLimiter(new LoginRateLimitConfig
        {
            Threshold = threshold,
            BaseDelaySeconds = baseDelay,
            MaxDelaySeconds = maxDelay
        });
    }

    [Fact]
    public void IsLoginAllowed_UnderThreshold_ReturnsTrue()
    {
        var limiter = CreateLimiter(threshold: 3);

        limiter.RecordFailedAttempt("192.168.1.1");
        Assert.True(limiter.IsLoginAllowed("192.168.1.1"));

        limiter.RecordFailedAttempt("192.168.1.1");
        Assert.True(limiter.IsLoginAllowed("192.168.1.1"));

        limiter.RecordFailedAttempt("192.168.1.1");
        Assert.True(limiter.IsLoginAllowed("192.168.1.1"));
    }

    [Fact]
    public void IsLoginAllowed_OverThreshold_RequiresDelay()
    {
        var limiter = CreateLimiter(threshold: 3, baseDelay: 10.0);

        for (var i = 0; i < 4; i++)
            limiter.RecordFailedAttempt("192.168.1.1");

        Assert.False(limiter.IsLoginAllowed("192.168.1.1"));
    }

    [Fact]
    public void RecordFailedAttempt_IncrementsCounter()
    {
        var limiter = CreateLimiter(threshold: 1, baseDelay: 100.0);

        limiter.RecordFailedAttempt("10.0.0.1");
        Assert.True(limiter.IsLoginAllowed("10.0.0.1"));

        limiter.RecordFailedAttempt("10.0.0.1");
        Assert.False(limiter.IsLoginAllowed("10.0.0.1"));
    }

    [Fact]
    public void RecordSuccessfulLogin_ResetsCounter()
    {
        var limiter = CreateLimiter(threshold: 1, baseDelay: 100.0);

        limiter.RecordFailedAttempt("10.0.0.1");
        limiter.RecordFailedAttempt("10.0.0.1");
        Assert.False(limiter.IsLoginAllowed("10.0.0.1"));

        limiter.RecordSuccessfulLogin("10.0.0.1");
        Assert.True(limiter.IsLoginAllowed("10.0.0.1"));
    }

    [Fact]
    public void GetRequiredDelay_ExponentialBackoff()
    {
        var limiter = CreateLimiter(threshold: 3, baseDelay: 1.0, maxDelay: 60.0);

        for (var i = 0; i < 4; i++)
            limiter.RecordFailedAttempt("10.0.0.1");

        var delay4 = limiter.GetRequiredDelay("10.0.0.1");
        Assert.True(delay4.TotalSeconds > 0 && delay4.TotalSeconds <= 1.0,
            $"After 4 failures (1 over threshold), expected ~1s delay, got {delay4.TotalSeconds}s");

        limiter.RecordFailedAttempt("10.0.0.1");
        var delay5 = limiter.GetRequiredDelay("10.0.0.1");
        Assert.True(delay5.TotalSeconds > 1.0 && delay5.TotalSeconds <= 2.0,
            $"After 5 failures (2 over threshold), expected ~2s delay, got {delay5.TotalSeconds}s");

        limiter.RecordFailedAttempt("10.0.0.1");
        var delay6 = limiter.GetRequiredDelay("10.0.0.1");
        Assert.True(delay6.TotalSeconds > 2.0 && delay6.TotalSeconds <= 4.0,
            $"After 6 failures (3 over threshold), expected ~4s delay, got {delay6.TotalSeconds}s");

        limiter.RecordFailedAttempt("10.0.0.1");
        var delay7 = limiter.GetRequiredDelay("10.0.0.1");
        Assert.True(delay7.TotalSeconds > 4.0 && delay7.TotalSeconds <= 8.0,
            $"After 7 failures (4 over threshold), expected ~8s delay, got {delay7.TotalSeconds}s");
    }

    [Fact]
    public void GetRequiredDelay_MaxCapRespected()
    {
        var limiter = CreateLimiter(threshold: 1, baseDelay: 1.0, maxDelay: 5.0);

        for (var i = 0; i < 20; i++)
            limiter.RecordFailedAttempt("10.0.0.1");

        var delay = limiter.GetRequiredDelay("10.0.0.1");
        Assert.True(delay.TotalSeconds <= 5.0,
            $"Expected delay capped at 5s, got {delay.TotalSeconds}s");
    }

    [Fact]
    public void CleanupOldEntries_RemovesStaleRecords()
    {
        var limiter = CreateLimiter(threshold: 1, baseDelay: 1.0, maxDelay: 1.0);

        limiter.RecordFailedAttempt("10.0.0.1");
        limiter.RecordFailedAttempt("10.0.0.1");

        // Immediately after recording, the entry exists
        Assert.False(limiter.IsLoginAllowed("10.0.0.1"));

        // After a successful login, the entry is removed
        limiter.RecordSuccessfulLogin("10.0.0.1");
        Assert.True(limiter.IsLoginAllowed("10.0.0.1"));
    }

    [Fact]
    public void MultipleIPs_IndependentTracking()
    {
        var limiter = CreateLimiter(threshold: 1, baseDelay: 100.0);

        limiter.RecordFailedAttempt("10.0.0.1");
        limiter.RecordFailedAttempt("10.0.0.1");
        limiter.RecordFailedAttempt("10.0.0.2");

        Assert.False(limiter.IsLoginAllowed("10.0.0.1"));
        Assert.True(limiter.IsLoginAllowed("10.0.0.2"));
    }

    [Fact]
    public void GetRequiredDelay_NoAttempts_ReturnsZero()
    {
        var limiter = CreateLimiter();

        Assert.Equal(TimeSpan.Zero, limiter.GetRequiredDelay("unknown-ip"));
    }

    [Fact]
    public void IsLoginAllowed_NewIP_ReturnsTrue()
    {
        var limiter = CreateLimiter();

        Assert.True(limiter.IsLoginAllowed("new-ip"));
    }
}
