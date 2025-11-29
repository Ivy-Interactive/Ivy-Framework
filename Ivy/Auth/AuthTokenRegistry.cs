using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Ivy.Auth;

public readonly struct AuthTokenId
{
    private readonly string _value;

    internal AuthTokenId(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    internal string Value => _value;

    public override string ToString() => _value;
}

public interface IAuthTokenRegistry
{
    AuthTokenId Register(AuthToken? token);
}

public class AuthTokenRegistry : IAuthTokenRegistry, IDisposable
{
    private static readonly ConcurrentDictionary<string, TokenEntry> GlobalTokens = new();
    private static readonly TimeSpan TokenExpiration = TimeSpan.FromMinutes(1);
    private static readonly Timer CleanupTimer;
    private readonly ConcurrentBag<string> _sessionTokenIds = new();

    static AuthTokenRegistry()
    {
        // Run cleanup every minute
        CleanupTimer = new Timer(
            _ => CleanupExpiredTokens(),
            null,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(1));
    }

    public AuthTokenId Register(AuthToken? token)
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var id = Convert.ToBase64String(bytes)
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');
        var entry = new TokenEntry { Token = token, LastAccessed = DateTime.UtcNow };

        if (!GlobalTokens.TryAdd(id, entry))
            throw new InvalidOperationException($"Token already registered for id '{id}'");

        _sessionTokenIds.Add(id);
        return new AuthTokenId(id);
    }

    public static bool TryRemove(AuthTokenId tokenId, out AuthToken? token)
    {
        if (GlobalTokens.TryRemove(tokenId.Value, out var entry))
        {
            token = entry.Token;
            return true;
        }

        token = null;
        return false;
    }

    private static void CleanupExpiredTokens()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = GlobalTokens
            .Where(kvp => now - kvp.Value.LastAccessed > TokenExpiration)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            GlobalTokens.TryRemove(key, out _);
        }
    }

    public void Dispose()
    {
        foreach (var tokenId in _sessionTokenIds)
        {
            GlobalTokens.TryRemove(tokenId, out _);
        }
    }
}

internal class TokenEntry
{
    public required AuthToken? Token { get; set; }
    public DateTime LastAccessed { get; set; }
}
