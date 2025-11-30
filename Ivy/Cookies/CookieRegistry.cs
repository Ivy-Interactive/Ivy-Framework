using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Ivy.Cookies;

public static class CookieJarIntents
{
    public const string SetAuthCookies = "set-auth-cookies";
}

class CookieJarEntry
{
    public required CookieJar CookieJar { get; set; }
    public required string Intent { get; set; }
    public required DateTime LastAccessed { get; set; }
}

public class GlobalCookieRegistry : IGlobalCookieRegistry, IDisposable
{
    private readonly ConcurrentDictionary<string, CookieJarEntry> _entries = new();
    private readonly TimeSpan _cookieJarLifetime = TimeSpan.FromMinutes(1);
    private readonly Timer _cleanupTimer;

    public GlobalCookieRegistry()
    {
        // Run cleanup every minute
        _cleanupTimer = new Timer(
            _ => CleanupExpiredCookieJars(),
            null,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(1));
    }

    public CookieJarId Register(CookieJar cookieJar, string intent)
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var id = Convert.ToBase64String(bytes)
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');
        var entry = new CookieJarEntry { CookieJar = cookieJar, Intent = intent, LastAccessed = DateTime.UtcNow };

        if (!_entries.TryAdd(id, entry))
            throw new InvalidOperationException($"Cookie jar already registered for id '{id}'");

        return new CookieJarId(id);
    }

    public bool TryRemove(CookieJarId cookieJarId, string intent, out CookieJar cookieJar)
    {
        if (_entries.TryRemove(cookieJarId.Value, out var entry) && entry.Intent == intent)
        {
            cookieJar = entry.CookieJar;
            return true;
        }

        cookieJar = null!;
        return false;
    }

    public bool TryRemove(CookieJarId cookieJarId)
        => _entries.TryRemove(cookieJarId.Value, out _);

    private void CleanupExpiredCookieJars()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _entries
            .Where(kvp => now - kvp.Value.LastAccessed > _cookieJarLifetime)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _entries.TryRemove(key, out _);
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}

public class CookieRegistry(IGlobalCookieRegistry global) : ICookieRegistry, IDisposable
{
    private readonly IGlobalCookieRegistry _global = global;
    private readonly ConcurrentBag<string> _sessionCookieJarIds = [];

    public CookieJarId Register(CookieJar cookieJar, string intent)
    {
        var cookieJarId = _global.Register(cookieJar, intent);
        _sessionCookieJarIds.Add(cookieJarId.Value);
        return cookieJarId;
    }

    public void Dispose()
    {
        foreach (var cookieJarId in _sessionCookieJarIds)
        {
            _global.TryRemove(new CookieJarId(cookieJarId));
        }
    }
}
