using System.Collections.Concurrent;
using Ivy.Plugins;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ivy.Core.Plugins;

internal class PluginReferencesWatcher : IDisposable
{
    private readonly string _pluginsDirectory;
    private readonly string _referencesFilePath;
    private readonly IPluginManager _pluginManager;
    private readonly ILogger _logger;
    private readonly bool _buildSourcePlugins;
    private readonly SourcePluginBuilder? _sourceBuilder;
    private FileSystemWatcher? _fileWatcher;
    private readonly ConcurrentDictionary<string, FileSystemWatcher> _directoryWatchers = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _pendingReloads = new();
    private readonly ConcurrentDictionary<string, DateTime> _reloadCooldowns = new();
    private readonly TimeSpan _debounceDelay = TimeSpan.FromMilliseconds(500);
    private readonly TimeSpan _cooldownPeriod = TimeSpan.FromSeconds(2);
    private HashSet<string> _currentReferences = new();
    private CancellationTokenSource? _pendingFileReload;
    private bool _disposed;

    public const string FileName = "plugin-references.yaml";

    public PluginReferencesWatcher(
        string pluginsDirectory,
        IPluginManager pluginManager,
        ILogger logger,
        bool buildSourcePlugins = false)
    {
        _pluginsDirectory = pluginsDirectory;
        _referencesFilePath = Path.Combine(pluginsDirectory, FileName);
        _pluginManager = pluginManager;
        _logger = logger;
        _buildSourcePlugins = buildSourcePlugins;
        _sourceBuilder = buildSourcePlugins ? new SourcePluginBuilder(logger) : null;
    }

    public void Start()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PluginReferencesWatcher));

        // Watch the references file for changes
        _fileWatcher = new FileSystemWatcher(_pluginsDirectory, FileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName,
            EnableRaisingEvents = true
        };
        _fileWatcher.Changed += OnReferencesFileChanged;
        _fileWatcher.Created += OnReferencesFileChanged;
        _fileWatcher.Deleted += OnReferencesFileChanged;

        _logger.LogInformation("Watching plugin references file: {Path}", _referencesFilePath);

        // Start watching all currently referenced directories
        foreach (var dir in _currentReferences)
            StartWatchingDirectory(dir);
    }

    public void SetInitialReferences(IEnumerable<string> resolvedPaths)
    {
        _currentReferences = new HashSet<string>(resolvedPaths, StringComparer.OrdinalIgnoreCase);
    }

    private void OnReferencesFileChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce file changes
        _pendingFileReload?.Cancel();
        var cts = new CancellationTokenSource();
        _pendingFileReload = cts;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(_debounceDelay, cts.Token);
                ProcessReferencesFileChange();
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("References file change debounce was cancelled");
            }
        }, cts.Token);
    }

    private void ProcessReferencesFileChange()
    {
        var newReferences = ParseReferencesFile(_referencesFilePath, _pluginsDirectory, _logger);
        var newSet = new HashSet<string>(newReferences, StringComparer.OrdinalIgnoreCase);

        var added = newSet.Except(_currentReferences, StringComparer.OrdinalIgnoreCase).ToList();
        var removed = _currentReferences.Except(newSet, StringComparer.OrdinalIgnoreCase).ToList();

        // Handle removals
        foreach (var dir in removed)
        {
            _logger.LogInformation("Plugin reference removed: {Directory}", dir);
            StopWatchingDirectory(dir);

            if (_pluginManager is PluginLoader loader)
            {
                var pluginId = loader.GetPluginIdByDirectory(dir);
                if (pluginId != null)
                {
                    _logger.LogInformation("Unloading plugin from removed reference: {PluginId}", pluginId);
                    try
                    {
                        _pluginManager.UnloadPlugin(pluginId);
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger.LogError(ex, "Failed to unload plugin {PluginId}", pluginId);
                    }
                }
            }
        }

        // Handle additions
        foreach (var dir in added)
        {
            _logger.LogInformation("Plugin reference added: {Directory}", dir);
            StartWatchingDirectory(dir);
            ScheduleLoad(dir);
        }

        _currentReferences = newSet;
    }

    private void StartWatchingDirectory(string directory)
    {
        if (!Directory.Exists(directory))
            return;

        if (_directoryWatchers.ContainsKey(directory))
            return;

        var watcher = new FileSystemWatcher(directory)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        watcher.Changed += (_, e) => OnFileChanged(e.FullPath, directory);
        watcher.Created += (_, e) => OnFileChanged(e.FullPath, directory);

        _directoryWatchers[directory] = watcher;
        _logger.LogDebug("Watching referenced plugin directory: {Directory}", directory);
    }

    private void StopWatchingDirectory(string directory)
    {
        if (_directoryWatchers.TryRemove(directory, out var watcher))
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        if (_pendingReloads.TryRemove(directory, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    private void OnFileChanged(string fullPath, string pluginDirectory)
    {
        if (fullPath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            return;

        if (fullPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            if (fullPath.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            {
                _logger.LogInformation("DLL changed in referenced plugin: {Path}", fullPath);
                ScheduleReload(pluginDirectory);
            }
            return;
        }

        if (_buildSourcePlugins && SourcePluginBuilder.IsSourceFile(fullPath) &&
            !fullPath.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
            SourcePluginBuilder.IsSourcePlugin(pluginDirectory))
        {
            _sourceBuilder?.ScheduleBuild(pluginDirectory);
        }
    }

    private void ScheduleLoad(string pluginDirectory)
    {
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

                _logger.LogInformation("Loading referenced plugin from: {Directory}", pluginDirectory);
                try
                {
                    _pluginManager.LoadPlugin(pluginDirectory);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, "Failed to load referenced plugin from {Directory}", pluginDirectory);
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "Failed to load referenced plugin from {Directory}", pluginDirectory);
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

    private void ScheduleReload(string pluginDirectory)
    {
        if (_reloadCooldowns.TryGetValue(pluginDirectory, out var cooldownUntil) && DateTime.UtcNow < cooldownUntil)
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

                if (_reloadCooldowns.TryGetValue(pluginDirectory, out var cd) && DateTime.UtcNow < cd)
                    return;

                if (_pluginManager is PluginLoader loader)
                {
                    var pluginId = loader.GetPluginIdByDirectory(pluginDirectory);
                    if (pluginId != null)
                    {
                        _logger.LogInformation("Reloading referenced plugin: {PluginId}", pluginId);
                        try
                        {
                            _pluginManager.ReloadPlugin(pluginId);
                        }
                        catch (InvalidOperationException ex)
                        {
                            _logger.LogError(ex, "Failed to reload referenced plugin {PluginId}", pluginId);
                        }
                        catch (IOException ex)
                        {
                            _logger.LogError(ex, "Failed to reload referenced plugin {PluginId}", pluginId);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Referenced plugin not yet loaded, loading from: {Directory}", pluginDirectory);
                        try
                        {
                            _pluginManager.LoadPlugin(pluginDirectory);
                        }
                        catch (InvalidOperationException ex)
                        {
                            _logger.LogError(ex, "Failed to load referenced plugin from {Directory}", pluginDirectory);
                        }
                        catch (IOException ex)
                        {
                            _logger.LogError(ex, "Failed to load referenced plugin from {Directory}", pluginDirectory);
                        }
                    }
                }

                _reloadCooldowns[pluginDirectory] = DateTime.UtcNow + _cooldownPeriod;
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Plugin reload cancelled for {Directory}", pluginDirectory);
            }
            finally
            {
                _pendingReloads.TryRemove(pluginDirectory, out _);
            }
        });
    }

    internal static List<string> ParseReferencesFile(string filePath, string pluginsDirectory, ILogger? logger = null)
    {
        if (!File.Exists(filePath))
            return [];

        try
        {
            var content = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(content))
                return [];

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var paths = deserializer.Deserialize<List<string>>(content);
            if (paths is null)
                return [];

            return paths
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => Path.IsPathRooted(p) ? Path.GetFullPath(p) : Path.GetFullPath(Path.Combine(pluginsDirectory, p)))
                .ToList();
        }
        catch (IOException ex)
        {
            logger?.LogError(ex, "Failed to parse plugin references file: {Path}", filePath);
            return [];
        }
        catch (UnauthorizedAccessException ex)
        {
            logger?.LogError(ex, "Failed to parse plugin references file: {Path}", filePath);
            return [];
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            logger?.LogError(ex, "Failed to parse plugin references file: {Path}", filePath);
            return [];
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _pendingFileReload?.Cancel();
        _pendingFileReload?.Dispose();

        if (_fileWatcher != null)
        {
            _fileWatcher.EnableRaisingEvents = false;
            _fileWatcher.Dispose();
        }

        foreach (var (dir, _) in _directoryWatchers)
            StopWatchingDirectory(dir);

        foreach (var cts in _pendingReloads.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _pendingReloads.Clear();
        _sourceBuilder?.Dispose();
    }
}
