using System.Collections.Concurrent;
using System.Security.Cryptography;
using Ivy.Core.Apps;
using Ivy.Core.Auth;
using Ivy.Core.Tui;

namespace Ivy.Core.Server;

class CookieJarEntry
{
    public required CookieJar CookieJar { get; set; }
    public required string Intent { get; set; }
    public required DateTime RegisteredAt { get; set; }
    /// <summary>When true, set-auth-cookies propagates auth to other sessions on the same machine (stored server-side).</summary>
    public bool TriggerMachineAuthSync { get; set; }
}

public class AppSessionStore : IDisposable
{
    public readonly ConcurrentDictionary<string, AppSession> Sessions = new();
    private readonly ConcurrentDictionary<string, CookieJarEntry> _cookieJarEntries = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _deferredRemovals = new();
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

    public CookieJarId RegisterCookies(CookieJar cookieJar, string intent, bool triggerMachineAuthSync = false)
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var id = Convert.ToBase64String(bytes)
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');
        var entry = new CookieJarEntry
        {
            CookieJar = cookieJar,
            Intent = intent,
            RegisteredAt = DateTime.UtcNow,
            TriggerMachineAuthSync = triggerMachineAuthSync,
        };

        if (!_cookieJarEntries.TryAdd(id, entry))
            throw new InvalidOperationException($"Cookie jar already registered for id '{id}'");

        return new CookieJarId(id);
    }

    public bool TryRemoveCookies(CookieJarId cookieJarId, string intent, out CookieJar cookieJar, out bool triggerMachineAuthSync)
    {
        if (_cookieJarEntries.TryRemove(cookieJarId.Value, out var entry) &&
            entry.Intent == intent &&
            DateTime.UtcNow - entry.RegisteredAt <= _cookieJarLifetime)
        {
            cookieJar = entry.CookieJar;
            triggerMachineAuthSync = entry.TriggerMachineAuthSync;
            return true;
        }

        cookieJar = null!;
        triggerMachineAuthSync = false;
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

    public void ScheduleDeferredRemoval(string connectionId, TimeSpan delay, Func<string, Task> cleanupAction)
    {
        var cts = new CancellationTokenSource();
        if (!_deferredRemovals.TryAdd(connectionId, cts))
        {
            // Already has a deferred removal scheduled — cancel old one and replace
            if (_deferredRemovals.TryRemove(connectionId, out var oldCts))
            {
                oldCts.Cancel();
                oldCts.Dispose();
            }
            _deferredRemovals[connectionId] = cts;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(delay, cts.Token);
                _deferredRemovals.TryRemove(connectionId, out _);
                await cleanupAction(connectionId);
            }
            catch (OperationCanceledException)
            {
                // Deferred removal was cancelled (session reconnected or cleaned up)
            }
            finally
            {
                cts.Dispose();
            }
        });
    }

    public bool CancelDeferredRemoval(string connectionId)
    {
        if (_deferredRemovals.TryRemove(connectionId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            return true;
        }
        return false;
    }

    public bool HasDeferredRemoval(string connectionId)
        => _deferredRemovals.ContainsKey(connectionId);

    public void Dispose()
    {
        _cookieJarCleanupTimer?.Dispose();
        foreach (var kvp in _deferredRemovals)
        {
            kvp.Value.Cancel();
            kvp.Value.Dispose();
        }
        _deferredRemovals.Clear();
    }

    public AppSession? FindAppShell(AppSession session)
    {
        if (session.ParentId == null)
        {
            return session.AppDescriptor.IsAppShell ? session : null;
        }
        if (!Sessions.TryGetValue(session.ParentId, out var parent))
            return null;
        return FindAppShell(parent);
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