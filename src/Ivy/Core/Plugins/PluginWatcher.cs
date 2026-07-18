using Ivy.Plugins;
using Microsoft.Extensions.Logging;

namespace Ivy.Core.Plugins;

internal class PluginWatcher : IDisposable
{
    private readonly string _pluginsDirectory;
    private readonly IPluginManager _pluginManager;
    private readonly ILogger _logger;
    private readonly bool _buildSourcePlugins;
    private readonly FileSystemWatcher _watcher;
    private readonly SourcePluginBuilder? _sourceBuilder;
    private readonly PluginReloadScheduler _scheduler;
    private bool _disposed;

    public PluginWatcher(string pluginsDirectory, IPluginManager pluginManager, ILogger logger, bool buildSourcePlugins = false)
    {
        _pluginsDirectory = pluginsDirectory;
        _pluginManager = pluginManager;
        _logger = logger;
        _buildSourcePlugins = buildSourcePlugins;
        _sourceBuilder = buildSourcePlugins ? new SourcePluginBuilder(logger) : null;
        _scheduler = new PluginReloadScheduler(pluginManager, logger);

        _watcher = new FileSystemWatcher(pluginsDirectory)
        {
            NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite,
            IncludeSubdirectories = true,
            EnableRaisingEvents = false
        };

        _watcher.Created += OnCreated;
        _watcher.Deleted += OnDeleted;
        _watcher.Changed += OnChanged;
        _watcher.Renamed += OnRenamed;
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
        _scheduler.CancelAll();
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (e.FullPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            OnDllChanged(e.FullPath);
            return;
        }

        if (_buildSourcePlugins && SourcePluginBuilder.IsSourceFile(e.FullPath))
        {
            OnSourceFileChanged(e.FullPath);
            return;
        }

        if (!Directory.Exists(e.FullPath))
            return;

        var parent = Path.GetDirectoryName(e.FullPath);
        if (parent == null)
            return;

        var normalizedParent = Path.GetFullPath(parent);
        var normalizedPluginsDir = Path.GetFullPath(_pluginsDirectory);
        if (!string.Equals(normalizedParent, normalizedPluginsDir, StringComparison.OrdinalIgnoreCase))
            return;

        _logger.LogInformation("New plugin directory detected: {Path}", e.FullPath);
        _scheduler.ScheduleLoad(e.FullPath);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        var parent = Path.GetDirectoryName(e.FullPath);
        if (parent == null)
            return;

        var normalizedParent = Path.GetFullPath(parent);
        var normalizedPluginsDir = Path.GetFullPath(_pluginsDirectory);
        if (!string.Equals(normalizedParent, normalizedPluginsDir, StringComparison.OrdinalIgnoreCase))
            return;

        _logger.LogInformation("Plugin directory deleted: {Path}", e.FullPath);
        _scheduler.Cancel(e.FullPath);

        if (_pluginManager is PluginLoader loader)
        {
            var pluginId = loader.GetPluginIdByDirectory(e.FullPath);
            if (pluginId != null)
            {
                _logger.LogInformation("Unloading plugin: {PluginId}", pluginId);
                try
                {
                    _pluginManager.UnloadPlugin(pluginId);
                    loader.ForgetPlugin(pluginId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to unload plugin {PluginId}", pluginId);
                }
            }
            else
            {
                loader.RemoveFailedPlugin(e.FullPath);
            }
        }
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.FullPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            OnDllChanged(e.FullPath);
            return;
        }

        if (_buildSourcePlugins && SourcePluginBuilder.IsSourceFile(e.FullPath))
        {
            OnSourceFileChanged(e.FullPath);
        }
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        if (e.FullPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            OnDllChanged(e.FullPath);
            return;
        }

        if (Directory.Exists(e.FullPath))
        {
            var parent = Path.GetDirectoryName(e.FullPath);
            if (parent != null &&
                string.Equals(Path.GetFullPath(parent), Path.GetFullPath(_pluginsDirectory), StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Plugin directory renamed into place: {Path}", e.FullPath);
                _scheduler.ScheduleLoad(e.FullPath);
            }
            return;
        }

        if (_buildSourcePlugins && SourcePluginBuilder.IsSourceFile(e.FullPath))
        {
            OnSourceFileChanged(e.FullPath);
        }
    }

    private void OnDllChanged(string fullPath)
    {
        if (fullPath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            return;

        var normalizedPluginsDir = Path.GetFullPath(_pluginsDirectory);
        var current = Path.GetDirectoryName(fullPath);
        while (current != null)
        {
            var parent = Path.GetDirectoryName(current);
            if (parent != null && string.Equals(Path.GetFullPath(parent), normalizedPluginsDir, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("DLL changed in plugin: {Path}", fullPath);
                _scheduler.ScheduleReload(current);
                return;
            }
            current = parent;
        }
    }

    private void OnSourceFileChanged(string fullPath)
    {
        if (fullPath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") ||
            fullPath.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            return;

        var normalizedPluginsDir = Path.GetFullPath(_pluginsDirectory);
        var current = Path.GetDirectoryName(fullPath);
        while (current != null)
        {
            var parent = Path.GetDirectoryName(current);
            if (parent != null && string.Equals(Path.GetFullPath(parent), normalizedPluginsDir, StringComparison.OrdinalIgnoreCase))
            {
                if (SourcePluginBuilder.IsSourcePlugin(current))
                    _sourceBuilder?.ScheduleBuild(current);
                return;
            }
            current = parent;
        }
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
        _watcher.Renamed -= OnRenamed;
        _watcher.Dispose();
        _sourceBuilder?.Dispose();
        _scheduler.Dispose();
    }
}
