using System.Collections.Concurrent;
using Ivy.Plugins;
using Microsoft.Extensions.Logging;

namespace Ivy.Core.Plugins;

internal class PluginWatcher : IDisposable
{
    private readonly string _pluginsDirectory;
    private readonly IPluginManager _pluginManager;
    private readonly ILogger _logger;
    private readonly FileSystemWatcher _watcher;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _pendingReloads = new();
    private readonly ConcurrentDictionary<string, DateTime> _reloadCooldowns = new();
    private readonly TimeSpan _debounceDelay = TimeSpan.FromMilliseconds(500);
    private readonly TimeSpan _cooldownPeriod = TimeSpan.FromSeconds(2);
    private bool _disposed;

    public PluginWatcher(string pluginsDirectory, IPluginManager pluginManager, ILogger logger)
    {
        _pluginsDirectory = pluginsDirectory;
        _pluginManager = pluginManager;
        _logger = logger;

        _watcher = new FileSystemWatcher(pluginsDirectory)
        {
            NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite,
            IncludeSubdirectories = true,
            EnableRaisingEvents = false
        };

        _watcher.Created += OnCreated;
        _watcher.Deleted += OnDeleted;
        _watcher.Changed += OnChanged;
    }

    public void Start()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PluginWatcher));

        _logger.LogInformation("Starting plugin hot-reload watcher for: {Directory}", _pluginsDirectory);
        _watcher.EnableRaisingEvents = true;
    }

    public void Stop()
    {
        if (_disposed)
            return;

        _logger.LogInformation("Stopping plugin hot-reload watcher");
        _watcher.EnableRaisingEvents = false;

        // Cancel all pending reloads
        foreach (var cts in _pendingReloads.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _pendingReloads.Clear();
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        // Handle new DLL files (dotnet build creates rather than overwrites DLLs)
        if (e.FullPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            OnDllChanged(e.FullPath);
            return;
        }

        // Handle directory creation (new plugin added)
        if (!Directory.Exists(e.FullPath))
            return;

        // Check if this is a top-level plugin directory (direct child of plugins directory)
        var parent = Path.GetDirectoryName(e.FullPath);
        if (parent == null)
            return;

        var normalizedParent = Path.GetFullPath(parent);
        var normalizedPluginsDir = Path.GetFullPath(_pluginsDirectory);
        if (!string.Equals(normalizedParent, normalizedPluginsDir, StringComparison.OrdinalIgnoreCase))
            return;

        _logger.LogInformation("New plugin directory detected: {Path}", e.FullPath);
        ScheduleLoad(e.FullPath);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        // Check if a top-level plugin directory was deleted
        var parent = Path.GetDirectoryName(e.FullPath);
        if (parent == null)
            return;

        var normalizedParent = Path.GetFullPath(parent);
        var normalizedPluginsDir = Path.GetFullPath(_pluginsDirectory);
        if (!string.Equals(normalizedParent, normalizedPluginsDir, StringComparison.OrdinalIgnoreCase))
            return;

        _logger.LogInformation("Plugin directory deleted: {Path}", e.FullPath);

        // Cancel any pending reload for this directory
        if (_pendingReloads.TryRemove(e.FullPath, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }

        // Find and unload the plugin immediately (no debouncing for deletions)
        if (_pluginManager is PluginLoader loader)
        {
            var pluginId = loader.GetPluginIdByDirectory(e.FullPath);
            if (pluginId != null)
            {
                _logger.LogInformation("Unloading plugin: {PluginId}", pluginId);
                try
                {
                    _pluginManager.UnloadPlugin(pluginId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to unload plugin {PluginId}", pluginId);
                }
            }
        }
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (!e.FullPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            return;

        OnDllChanged(e.FullPath);
    }

    private void OnDllChanged(string fullPath)
    {
        // Ignore intermediate build artifacts (obj/ directory) — consistent with
        // LoadPluginFromDirectory which also filters these out when loading.
        if (fullPath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            return;

        // Walk up from the changed file to find which top-level plugin directory it's under
        var normalizedPluginsDir = Path.GetFullPath(_pluginsDirectory);
        var current = Path.GetDirectoryName(fullPath);
        while (current != null)
        {
            var parent = Path.GetDirectoryName(current);
            if (parent != null && string.Equals(Path.GetFullPath(parent), normalizedPluginsDir, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("DLL changed in plugin: {Path}", fullPath);
                ScheduleReload(current);
                return;
            }
            current = parent;
        }
    }

    private void ScheduleLoad(string pluginDirectory)
    {
        // Cancel any existing pending load for this directory
        if (_pendingReloads.TryRemove(pluginDirectory, out var existingCts))
            existingCts.Cancel();

        var cts = new CancellationTokenSource();
        _pendingReloads[pluginDirectory] = cts;
        var token = cts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(_debounceDelay, token);

                _logger.LogInformation("Loading plugin from: {Directory}", pluginDirectory);
                try
                {
                    _pluginManager.LoadPlugin(pluginDirectory);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load plugin from {Directory}", pluginDirectory);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when a new change occurs during the debounce period
            }
            finally
            {
                _pendingReloads.TryRemove(pluginDirectory, out _);
                cts.Dispose();
            }
        }, token);
    }

    private void ScheduleReload(string pluginDirectory)
    {
        // Skip if this directory was recently loaded/reloaded (builds produce multiple
        // waves of file events that can span longer than the debounce window)
        if (_reloadCooldowns.TryGetValue(pluginDirectory, out var cooldownUntil) && DateTime.UtcNow < cooldownUntil)
            return;

        // Cancel any existing pending reload for this directory
        if (_pendingReloads.TryRemove(pluginDirectory, out var existingCts))
            existingCts.Cancel();

        var cts = new CancellationTokenSource();
        _pendingReloads[pluginDirectory] = cts;
        var token = cts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(_debounceDelay, token);

                // Re-check cooldown after debounce (another operation may have completed during the wait)
                if (_reloadCooldowns.TryGetValue(pluginDirectory, out var cd) && DateTime.UtcNow < cd)
                    return;

                bool success = false;

                if (_pluginManager is PluginLoader loader)
                {
                    var pluginId = loader.GetPluginIdByDirectory(pluginDirectory);
                    if (pluginId != null)
                    {
                        _logger.LogInformation("Reloading plugin: {PluginId}", pluginId);
                        try
                        {
                            success = _pluginManager.ReloadPlugin(pluginId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to reload plugin {PluginId}", pluginId);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Plugin not yet loaded, attempting to load from: {Directory}", pluginDirectory);
                        try
                        {
                            success = _pluginManager.LoadPlugin(pluginDirectory);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to load plugin from {Directory}", pluginDirectory);
                        }
                    }
                }

                // Set cooldown to suppress duplicate reloads from subsequent file events
                // (applies on both success and failure — a failed load shouldn't retry
                // until the DLLs actually change again from a new build)
                _reloadCooldowns[pluginDirectory] = DateTime.UtcNow + _cooldownPeriod;
            }
            catch (OperationCanceledException)
            {
                // Expected when a new change occurs during the debounce period
            }
            finally
            {
                _pendingReloads.TryRemove(pluginDirectory, out _);
                cts.Dispose();
            }
        }, token);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        Stop();

        _watcher.Created -= OnCreated;
        _watcher.Deleted -= OnDeleted;
        _watcher.Changed -= OnChanged;
        _watcher.Dispose();
    }
}
