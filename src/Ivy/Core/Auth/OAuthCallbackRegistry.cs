using System.Collections.Concurrent;

namespace Ivy.Core.Auth;

public interface IOAuthCallbackRegistry
{
    string RegisterPending(string connectionId, string optionId, string? provider = null);

    PendingOAuthCallback? GetAndRemove(string state);

    bool HasPendingForConnection(string connectionId);
}

public record PendingOAuthCallback(string ConnectionId, string OptionId, DateTime CreatedAt, string? Provider = null);

public class OAuthCallbackRegistry : IOAuthCallbackRegistry
{
    private readonly ConcurrentDictionary<string, PendingOAuthCallback> _pending = new();
    private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(10);
    private DateTime _lastCleanup = DateTime.UtcNow;
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(1);

    public string RegisterPending(string connectionId, string optionId, string? provider = null)
    {
        CleanupExpiredIfNeeded();

        var state = Guid.NewGuid().ToString();
        _pending[state] = new PendingOAuthCallback(connectionId, optionId, DateTime.UtcNow, provider);
        return state;
    }

    public bool HasPendingForConnection(string connectionId)
        => _pending.Values.Any(p => p.ConnectionId == connectionId);

    public PendingOAuthCallback? GetAndRemove(string state)
    {
        if (string.IsNullOrEmpty(state))
            return null;

        if (_pending.TryRemove(state, out var callback))
        {
            if (DateTime.UtcNow - callback.CreatedAt < Expiration)
                return callback;
        }

        return null;
    }

    private void CleanupExpiredIfNeeded()
    {
        if (DateTime.UtcNow - _lastCleanup < CleanupInterval)
            return;

        _lastCleanup = DateTime.UtcNow;

        var expiredKeys = _pending
            .Where(kvp => DateTime.UtcNow - kvp.Value.CreatedAt > Expiration)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _pending.TryRemove(key, out _);
        }
    }
}
