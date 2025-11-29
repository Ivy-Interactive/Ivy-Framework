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

public interface IGlobalAuthTokenRegistry : IDisposable
{
    AuthTokenId Register(AuthToken? token);
    bool TryRemove(AuthTokenId tokenId, out AuthToken? token);
}

public class GlobalAuthTokenRegistry : IGlobalAuthTokenRegistry, IDisposable
{
    private readonly ConcurrentDictionary<string, TokenEntry> _tokens = new();
    private readonly TimeSpan _tokenExpiration = TimeSpan.FromMinutes(1);
    private readonly Timer _cleanupTimer;

    public GlobalAuthTokenRegistry()
    {
        // Run cleanup every minute
        _cleanupTimer = new Timer(
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

        if (!_tokens.TryAdd(id, entry))
            throw new InvalidOperationException($"Token already registered for id '{id}'");

        return new AuthTokenId(id);
    }

    public bool TryRemove(AuthTokenId tokenId, out AuthToken? token)
    {
        if (_tokens.TryRemove(tokenId.Value, out var entry))
        {
            token = entry.Token;
            return true;
        }

        token = null;
        return false;
    }

    private void CleanupExpiredTokens()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _tokens
            .Where(kvp => now - kvp.Value.LastAccessed > _tokenExpiration)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _tokens.TryRemove(key, out _);
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}

public interface IAuthTokenRegistry
{
    AuthTokenId Register(AuthToken? token);
}

public class AuthTokenRegistry(IGlobalAuthTokenRegistry global) : IAuthTokenRegistry, IDisposable
{
    private readonly IGlobalAuthTokenRegistry _global = global;
    private readonly ConcurrentBag<string> _sessionTokenIds = [];

    public AuthTokenId Register(AuthToken? token)
    {
        var tokenId = _global.Register(token);
        _sessionTokenIds.Add(tokenId.Value);
        return tokenId;
    }

    public void Dispose()
    {
        foreach (var tokenId in _sessionTokenIds)
        {
            _global.TryRemove(new AuthTokenId(tokenId), out _);
        }
    }
}

internal class TokenEntry
{
    public required AuthToken? Token { get; set; }
    public DateTime LastAccessed { get; set; }
}
