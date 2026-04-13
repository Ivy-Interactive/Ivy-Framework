using System.Collections.Concurrent;

namespace Ivy.Tendril.Auth;

public class LoginRateLimiter
{
    private readonly ConcurrentDictionary<string, LoginAttemptRecord> _attempts = new();
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
    private readonly LoginRateLimitConfig _config;
    private DateTime _lastCleanup = DateTime.UtcNow;

    public LoginRateLimiter(LoginRateLimitConfig config)
    {
        _config = config;
    }

    public bool IsLoginAllowed(string ipAddress)
    {
        CleanupOldEntries();

        if (!_attempts.TryGetValue(ipAddress, out var record))
            return true;

        var timeSinceLastAttempt = DateTime.UtcNow - record.LastAttempt;
        var requiredDelay = CalculateRequiredDelay(record.FailedAttempts);

        return timeSinceLastAttempt >= requiredDelay;
    }

    public void RecordFailedAttempt(string ipAddress)
    {
        _attempts.AddOrUpdate(
            ipAddress,
            _ => new LoginAttemptRecord(1, DateTime.UtcNow),
            (_, existing) => new LoginAttemptRecord(existing.FailedAttempts + 1, DateTime.UtcNow));
    }

    public void RecordSuccessfulLogin(string ipAddress)
    {
        _attempts.TryRemove(ipAddress, out _);
    }

    public TimeSpan GetRequiredDelay(string ipAddress)
    {
        if (!_attempts.TryGetValue(ipAddress, out var record))
            return TimeSpan.Zero;

        var timeSinceLastAttempt = DateTime.UtcNow - record.LastAttempt;
        var requiredDelay = CalculateRequiredDelay(record.FailedAttempts);
        var remaining = requiredDelay - timeSinceLastAttempt;

        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    // min(baseDelay * 2^(attempts - threshold), maxDelay)
    private TimeSpan CalculateRequiredDelay(int failedAttempts)
    {
        if (failedAttempts <= _config.Threshold)
            return TimeSpan.Zero;

        var exponent = failedAttempts - _config.Threshold;
        var delaySeconds = _config.BaseDelaySeconds * Math.Pow(2, exponent - 1);
        var cappedDelay = Math.Min(delaySeconds, _config.MaxDelaySeconds);

        return TimeSpan.FromSeconds(cappedDelay);
    }

    private void CleanupOldEntries()
    {
        if (DateTime.UtcNow - _lastCleanup < _cleanupInterval)
            return;

        _lastCleanup = DateTime.UtcNow;
        var cutoff = DateTime.UtcNow - TimeSpan.FromSeconds(_config.MaxDelaySeconds) - _cleanupInterval;

        var keysToRemove = _attempts
            .Where(kvp => kvp.Value.LastAttempt < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _attempts.TryRemove(key, out _);
        }
    }

    private sealed record LoginAttemptRecord(int FailedAttempts, DateTime LastAttempt);
}

public record LoginRateLimitConfig
{
    public int Threshold { get; init; } = 3;
    public double BaseDelaySeconds { get; init; } = 1.0;
    public double MaxDelaySeconds { get; init; } = 60.0;
}
