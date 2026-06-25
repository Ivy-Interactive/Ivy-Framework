using System.Collections.Concurrent;
using Ivy.Plugins;
using Microsoft.Extensions.Logging;

namespace Ivy.Core.Plugins;

internal class PluginReloadScheduler : IDisposable
{
    private readonly IPluginManager _pluginManager;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _pendingReloads = new();
    private readonly ConcurrentDictionary<string, DateTime> _reloadCooldowns = new();
    private readonly ConcurrentDictionary<string, int> _consecutiveFailures = new();
    private readonly TimeSpan _debounceDelay = TimeSpan.FromMilliseconds(500);
    private readonly TimeSpan _cooldownPeriod = TimeSpan.FromSeconds(2);
    private const int MaxConsecutiveFailures = 5;
    private bool _disposed;

    public PluginReloadScheduler(IPluginManager pluginManager, ILogger logger)
    {
        _pluginManager = pluginManager;
        _logger = logger;
    }

    public void ScheduleLoad(string pluginDirectory)
    {
        if (_reloadCooldowns.TryGetValue(pluginDirectory, out var cooldownUntil) && DateTime.UtcNow < cooldownUntil)
            return;

        if (_consecutiveFailures.TryGetValue(pluginDirectory, out var failures) && failures >= MaxConsecutiveFailures)
            return;

        if (_pendingReloads.TryRemove(pluginDirectory, out var existingCts))
        {
            existingCts.Cancel();
            existingCts.Dispose();
        }

        _ = Task.Run(async () =>
        {
            using var cts = new CancellationTokenSource();
            _pendingReloads[pluginDirectory] = cts;
            var token = cts.Token;

            try
            {
                await Task.Delay(_debounceDelay, token);

                _logger.LogInformation("Loading plugin from: {Directory}", pluginDirectory);
                try
                {
                    _pluginManager.LoadPlugin(pluginDirectory);
                    _reloadCooldowns[pluginDirectory] = DateTime.UtcNow + _cooldownPeriod;
                    _consecutiveFailures.TryRemove(pluginDirectory, out _);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    RecordFailure(pluginDirectory, ex);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Plugin load cancelled for {Directory}", pluginDirectory);
            }
            finally
            {
                _pendingReloads.TryRemove(pluginDirectory, out _);
            }
        });
    }

    public void ScheduleReload(string pluginDirectory)
    {
        if (_reloadCooldowns.TryGetValue(pluginDirectory, out var cooldownUntil) && DateTime.UtcNow < cooldownUntil)
            return;

        if (_consecutiveFailures.TryGetValue(pluginDirectory, out var failures) && failures >= MaxConsecutiveFailures)
            return;

        if (_pendingReloads.ContainsKey(pluginDirectory))
            return;

        _ = Task.Run(async () =>
        {
            using var cts = new CancellationTokenSource();
            _pendingReloads[pluginDirectory] = cts;
            var token = cts.Token;

            try
            {
                await Task.Delay(_debounceDelay, token);

                if (_reloadCooldowns.TryGetValue(pluginDirectory, out var cd) && DateTime.UtcNow < cd)
                    return;

                if (_pluginManager is PluginLoader loader)
                {
                    var pluginId = loader.GetPluginIdByDirectory(pluginDirectory);
                    if (pluginId != null)
                    {
                        _logger.LogInformation("Reloading plugin: {PluginId}", pluginId);
                        _pluginManager.ReloadPlugin(pluginId);
                    }
                    else
                    {
                        _logger.LogInformation("Plugin not yet loaded, loading from: {Directory}", pluginDirectory);
                        _pluginManager.LoadPlugin(pluginDirectory);
                    }
                }

                _reloadCooldowns[pluginDirectory] = DateTime.UtcNow + _cooldownPeriod;
                _consecutiveFailures.TryRemove(pluginDirectory, out _);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Plugin reload cancelled for {Directory}", pluginDirectory);
            }
            catch (Exception ex)
            {
                RecordFailure(pluginDirectory, ex);
            }
            finally
            {
                _pendingReloads.TryRemove(pluginDirectory, out _);
            }
        });
    }

    public void ResetFailures(string pluginDirectory)
    {
        _consecutiveFailures.TryRemove(pluginDirectory, out _);
    }

    private void RecordFailure(string pluginDirectory, Exception ex)
    {
        var count = _consecutiveFailures.AddOrUpdate(pluginDirectory, 1, (_, c) => c + 1);
        _reloadCooldowns[pluginDirectory] = DateTime.UtcNow + GetBackoffCooldown(count);
        _logger.LogError(ex, "Failed to load/reload plugin from {Directory} (attempt {Count}/{Max})",
            pluginDirectory, count, MaxConsecutiveFailures);
        if (count >= MaxConsecutiveFailures)
            _logger.LogWarning("Plugin {Directory} has failed {Max} consecutive times, suspending reload attempts until source changes",
                pluginDirectory, MaxConsecutiveFailures);
    }

    private TimeSpan GetBackoffCooldown(int failureCount)
    {
        var seconds = Math.Min(2 * Math.Pow(2, failureCount - 1), 60);
        return TimeSpan.FromSeconds(seconds);
    }

    public void Cancel(string pluginDirectory)
    {
        if (_pendingReloads.TryRemove(pluginDirectory, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    public void CancelAll()
    {
        foreach (var cts in _pendingReloads.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _pendingReloads.Clear();
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        CancelAll();
    }
}
