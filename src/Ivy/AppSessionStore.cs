using System.Collections.Concurrent;
using Ivy.Apps;
using Ivy.Helpers.Tui;
using System.Security.Cryptography;
using Ivy.Auth;

namespace Ivy;

class CookieJarEntry
{
    public required CookieJar CookieJar { get; set; }
    public required string Intent { get; set; }
    public required DateTime RegisteredAt { get; set; }
}

public class AppSessionStore : IDisposable
{
    public readonly ConcurrentDictionary<string, AppSession> Sessions = new();
    private readonly ConcurrentDictionary<string, CookieJarEntry> _cookieJarEntries = new();
    private readonly TimeSpan _cookieJarLifetime = TimeSpan.FromMinutes(1);
    private readonly Timer _cookieJarCleanupTimer;

    public AppSessionStore()
    {
        // Run cleanup every minute
        _cookieJarCleanupTimer = new Timer(
            _ => CleanupExpiredCookieJars(),
            null,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(1));
    }

    public CookieJarId RegisterCookies(CookieJar cookieJar, string intent)
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var id = Convert.ToBase64String(bytes)
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');
        var entry = new CookieJarEntry { CookieJar = cookieJar, Intent = intent, RegisteredAt = DateTime.UtcNow };

        if (!_cookieJarEntries.TryAdd(id, entry))
            throw new InvalidOperationException($"Cookie jar already registered for id '{id}'");

        return new CookieJarId(id);
    }

    public bool TryRemoveCookies(CookieJarId cookieJarId, string intent, out CookieJar cookieJar)
    {
        if (_cookieJarEntries.TryRemove(cookieJarId.Value, out var entry) &&
            entry.Intent == intent &&
            DateTime.UtcNow - entry.RegisteredAt <= _cookieJarLifetime)
        {
            cookieJar = entry.CookieJar;
            return true;
        }

        cookieJar = null!;
        return false;
    }

    private void CleanupExpiredCookieJars()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _cookieJarEntries
            .Where(kvp => now - kvp.Value.RegisteredAt > _cookieJarLifetime)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _cookieJarEntries.TryRemove(key, out _);
        }
    }

    public void Dispose()
    {
        _cookieJarCleanupTimer?.Dispose();
    }

    public AppSession? FindChrome(AppSession session)
    {
        if (session.ParentId == null)
        {
            return session.AppDescriptor.IsChrome ? session : null;
        }
        var parent = Sessions.Values.FirstOrDefault(s => s.ConnectionId == session.ParentId);
        return parent == null ? null : FindChrome(parent!);
    }

    public void Dump()
    {
        var rows = Sessions.Values.Select(e => new
        {
            e.MachineId,
            e.AppId,
            e.ConnectionId,
            e.ParentId,
            e.LastInteraction
        });

        var table = new AnsiTable();
        table.AddColumn("MachineId");
        table.AddColumn("AppId");
        table.AddColumn("ConnectionId");
        table.AddColumn("ParentId");
        table.AddColumn("LastInteraction");

        foreach (var row in rows)
        {
            table.AddRow(row.MachineId, row.AppId, row.ConnectionId, row.ParentId ?? "", row.LastInteraction.ToString("HH:mm:ss"));
        }

        AnsiConsole.Write(table);
    }
}