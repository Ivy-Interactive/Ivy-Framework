using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Ivy.Core.ExternalWidgets;
using Ivy.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ivy.Core.Plugins;


public class PluginLoader : IPluginManager
{
    private readonly string _pluginsDirectory;
    private readonly ILogger<PluginLoader> _logger;
    private readonly IReadOnlySet<string> _sharedAssemblyNames;
    private readonly List<LoadedPlugin> _plugins = [];
    private readonly Dictionary<string, string> _knownPlugins = new(); // id -> directory
    private readonly Dictionary<string, (string Reason, DateTime FailedAt)> _failedPlugins = new(); // directory -> failure info
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly ConcurrentDictionary<string, object> _reloadLocks = new();
    private PluginContextBase? _pluginContext;
    private IIvyPluginConfigFactory? _configFactory;
    private Func<IServiceProvider>? _serviceProviderFactory;
    private Version? _hostVersion;

    public event Action<string>? PluginLoaded;
    public event Action<string>? PluginLoadFailed;
    public event Action<string>? PluginUnloaded;
    public event Action<string>? PluginRemoved;
    public event Action<string>? PluginReloaded;
    public event Action<string>? PluginActivated;
    public event Action<string>? PluginDeactivated;

    internal bool BuildSourcePlugins { get; }
    internal bool DeferPluginLoads { get; }

    internal PluginLoader(string pluginsDirectory, ILogger<PluginLoader> logger, IEnumerable<string>? sharedAssemblyNames = null, bool buildSourcePlugins = false, bool deferPluginLoads = false)
    {
        _pluginsDirectory = pluginsDirectory;
        _logger = logger;
        BuildSourcePlugins = buildSourcePlugins;
        DeferPluginLoads = deferPluginLoads;
        _sharedAssemblyNames = new HashSet<string>(sharedAssemblyNames ?? [])
        {
            "Ivy.Plugin.Abstractions",
            "Ivy",
            // Microsoft.Extensions types appear in the plugin contract surface
            // (IServiceCollection, ILogger, etc.) and must share type identity with the host.
            "Microsoft.Extensions.DependencyInjection",
            "Microsoft.Extensions.DependencyInjection.Abstractions",
            "Microsoft.Extensions.Logging.Abstractions",
        };

        AppDomain.CurrentDomain.ProcessExit += (_, _) => CleanupAllShadowDirectories();
    }

    public IReadOnlyList<LoadedPlugin> Plugins
    {
        get
        {
            _lock.EnterReadLock();
            try { return _plugins.ToList(); }
            finally { _lock.ExitReadLock(); }
        }
    }

    public void DiscoverAndLoad(Version hostVersion, IServiceProvider serviceProvider)
    {
        _hostVersion = hostVersion;

        if (!Directory.Exists(_pluginsDirectory))
        {
            _logger.LogDebug("Plugins directory not found: {Directory}", _pluginsDirectory);
            return;
        }

        var candidates = new List<(IIvyPlugin Instance, Assembly Assembly, AssemblyLoadContext Context, string Directory, string ShadowDirectory)>();

        // Load plugins from the references file first (explicit references take priority)
        var referencesFilePath = Path.Combine(_pluginsDirectory, PluginReferencesWatcher.FileName);
        var referencedPaths = PluginReferencesWatcher.ParseReferencesFile(referencesFilePath, _pluginsDirectory, _logger);
        foreach (var directory in referencedPaths)
        {
            if (!Directory.Exists(directory))
            {
                RecordFailure(directory, "Directory does not exist");
                continue;
            }

            TryLoadCandidate(directory, serviceProvider, hostVersion, candidates);
        }

        // Load plugins from subdirectories of the plugins directory
        foreach (var directory in Directory.GetDirectories(_pluginsDirectory))
            TryLoadCandidate(directory, serviceProvider, hostVersion, candidates);

        _lock.EnterWriteLock();
        try
        {
            _plugins.AddRange(candidates.Select(c => new LoadedPlugin(c.Instance, c.Assembly, c.Context, c.Directory, c.ShadowDirectory)));
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        foreach (var plugin in _plugins)
        {
            _logger.LogInformation("Loaded plugin: {Id} v{Version}", plugin.Instance.Manifest.Id, plugin.Instance.Manifest.Version);
            ExternalWidgetRegistry.Instance.RegisterAssembly(plugin.Assembly);
        }
    }

    private (IIvyPlugin Instance, Assembly Assembly, AssemblyLoadContext Context, string Directory, string ShadowDirectory)? LoadPluginFromDirectory(
        string directory, IServiceProvider serviceProvider, out string? failureReason)
    {
        failureReason = null;
        var sep = Path.DirectorySeparatorChar;
        var dllFiles = Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{sep}obj{sep}"))
            .GroupBy(Path.GetFileName)
            .Select(g => g.Count() == 1 ? g.First()
                : g.FirstOrDefault(f => f.Contains($"{sep}Debug{sep}")) ?? g.First())
            .ToArray();
        if (dllFiles.Length == 0)
        {
            _logger.LogWarning("No DLL files found in plugin directory: {Directory}", directory);
            failureReason = "No DLL files found";
            return null;
        }

        // Shadow-copy all DLLs and deps.json so the runtime loads fresh bytes on reload
        var shadowDir = Path.Combine(Path.GetTempPath(), "ivy-plugins", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(shadowDir);
        var shadowFiles = new List<string>();
        foreach (var src in dllFiles)
        {
            var dest = Path.Combine(shadowDir, Path.GetFileName(src));
            File.Copy(src, dest, overwrite: true);
            shadowFiles.Add(dest);

            // Also copy the .deps.json if one exists next to this DLL
            var depsFile = Path.ChangeExtension(src, ".deps.json");
            if (File.Exists(depsFile))
            {
                File.Copy(depsFile, Path.Combine(shadowDir, Path.GetFileName(depsFile)), overwrite: true);
            }
        }

        // Determine the main plugin DLL (matches directory name) for the AssemblyDependencyResolver
        var dirName = Path.GetFileName(directory);
        var mainDllPath = shadowFiles.FirstOrDefault(f =>
            Path.GetFileNameWithoutExtension(f).Equals(dirName, StringComparison.OrdinalIgnoreCase));

        // Fall back to first DLL that has a matching .deps.json in the shadow directory
        mainDllPath ??= shadowFiles.FirstOrDefault(f =>
            File.Exists(Path.ChangeExtension(f, ".deps.json")));

        // Final fallback: first DLL
        mainDllPath ??= shadowFiles[0];

        // Pre-flight check: verify the plugin's shared assembly dependencies are satisfied by the host
        var compatError = CheckSharedAssemblyCompatibility(shadowDir, directory);
        if (compatError is not null)
        {
            _logger.LogError("Plugin in '{Directory}' is incompatible: {Error}", directory, compatError);
            failureReason = compatError;
            DeleteShadowDirectory(shadowDir);
            return null;
        }

        // Create a single load context for the entire plugin
        var loadContext = new PluginAssemblyLoadContext(mainDllPath, _sharedAssemblyNames);

        // Load non-shared DLLs into the context and look for the [IvyPlugin] attribute
        var found = new List<(Type PluginType, Assembly Assembly)>();

        foreach (var dllPath in shadowFiles)
        {
            // Skip DLLs that match shared assemblies — they come from the host
            var assemblyName = Path.GetFileNameWithoutExtension(dllPath);
            if (_sharedAssemblyNames.Contains(assemblyName))
                continue;

            Assembly assembly;
            try
            {
                assembly = loadContext.LoadFromAssemblyPath(dllPath);
            }
            catch (BadImageFormatException ex)
            {
                _logger.LogDebug(ex, "Could not load {Dll}, skipping.", dllPath);
                continue;
            }
            catch (IOException ex)
            {
                _logger.LogDebug(ex, "Could not load {Dll}, skipping.", dllPath);
                continue;
            }

            var attr = assembly.GetCustomAttribute<IvyPluginAttribute>();
            if (attr is null) continue;

            if (!typeof(IIvyPlugin).IsAssignableFrom(attr.PluginType))
            {
                _logger.LogError(
                    "Assembly {Dll} has [IvyPlugin({Type})] but that type does not implement IIvyPlugin. Skipping directory.",
                    dllPath, attr.PluginType.FullName);
                failureReason = $"[IvyPlugin({attr.PluginType.FullName})] does not implement IIvyPlugin";
                DeleteShadowDirectory(shadowDir);
                return null;
            }

            found.Add((attr.PluginType, assembly));
        }

        if (found.Count == 0)
        {
            _logger.LogDebug("No [assembly: IvyPlugin] attribute found in {Directory}. Skipping.", directory);
            failureReason = "No [IvyPlugin] attribute found";
            DeleteShadowDirectory(shadowDir);
            return null;
        }

        if (found.Count > 1)
        {
            _logger.LogError(
                "Multiple [assembly: IvyPlugin] attributes found in {Directory} ({Assemblies}). Skipping.",
                directory, string.Join(", ", found.Select(f => f.Assembly.GetName().Name)));
            failureReason = "Multiple [IvyPlugin] attributes found";
            DeleteShadowDirectory(shadowDir);
            return null;
        }

        var (pluginType, pluginAssembly) = found[0];
        var instance = (IIvyPlugin)ActivatorUtilities.CreateInstance(serviceProvider, pluginType);
        return (instance, pluginAssembly, loadContext, directory, shadowDir);
    }

    internal void SetHostVersion(Version version)
    {
        _hostVersion = version;
    }

    internal void SetPluginConfigFactory(IIvyPluginConfigFactory factory)
    {
        _configFactory = factory;
    }

    internal void SetServiceProviderFactory(Func<IServiceProvider> factory)
    {
        _serviceProviderFactory = factory;
        if (DeferPluginLoads)
            LoadDeferredPluginsAsync();
    }

    private void LoadDeferredPluginsAsync()
    {
        _ = Task.Run(() =>
        {
            try
            {
                _logger.LogInformation("Loading plugins in background...");
                var directories = DiscoverPluginDirectories();
                foreach (var directory in directories)
                {
                    try
                    {
                        LoadPlugin(directory);
                    }
                    catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
                    {
                        _logger.LogError(ex, "Error loading deferred plugin from {Directory}", directory);
                    }
                }
                _logger.LogInformation("Background plugin loading complete. {Count} plugin(s) loaded.", Plugins.Count);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
            {
                _logger.LogError(ex, "Error during background plugin loading");
            }
        });
    }

    private List<string> DiscoverPluginDirectories()
    {
        var directories = new List<string>();
        if (!Directory.Exists(_pluginsDirectory)) return directories;

        // Referenced plugins first (explicit references take priority)
        var referencesFilePath = Path.Combine(_pluginsDirectory, PluginReferencesWatcher.FileName);
        var referencedPaths = PluginReferencesWatcher.ParseReferencesFile(referencesFilePath, _pluginsDirectory, _logger);
        foreach (var directory in referencedPaths.Where(Directory.Exists))
        {
            directories.Add(directory);
        }

        // Then subdirectories of the plugins directory
        foreach (var directory in Directory.GetDirectories(_pluginsDirectory))
            directories.Add(directory);

        return directories;
    }

    public void Configure(PluginContextBase context)
    {
        _pluginContext = context;

        _lock.EnterReadLock();
        try
        {
            foreach (var plugin in _plugins)
            {
                if (plugin.Instance.ConfigurationSchema is { } schema)
                {
                    var errors = ValidatePluginConfiguration(
                        schema,
                        CreatePluginConfig(plugin.Instance));

                    if (errors.Count > 0)
                    {
                        _logger.LogWarning(
                            "Plugin '{Id}' configuration is incomplete: {Errors}. Plugin loaded as unconfigured.",
                            plugin.Instance.Manifest.Id,
                            string.Join(", ", errors));
                        plugin.Status = PluginStatus.Unconfigured;
                        continue;
                    }
                }

                context.SetCurrentPlugin(plugin.Instance.Manifest.Id, plugin.Directory);
                context.SetPluginConfig(CreatePluginConfig(plugin.Instance));
                plugin.Instance.Configure(context);
                context.ClearCurrentPlugin();
                context.ClearPluginConfig();
                plugin.Status = PluginStatus.Active;
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool UnloadPlugin(string pluginId)
    {
        // Get the plugin instance under read lock for shutdown hook
        IIvyPlugin? pluginInstance;
        _lock.EnterReadLock();
        try
        {
            var plugin = _plugins.FirstOrDefault(p => p.Instance.Manifest.Id == pluginId);
            if (plugin is null)
            {
                _logger.LogWarning("Plugin '{Id}' not found for unload.", pluginId);
                return false;
            }
            pluginInstance = plugin.Status == PluginStatus.Active ? plugin.Instance : null;
        }
        finally
        {
            _lock.ExitReadLock();
        }

        // Call shutdown hook outside the lock (blocks up to 5s)
        if (pluginInstance is not null)
            InvokeShutdownHook(pluginInstance, pluginId, PluginShutdownReason.Unload);

        _lock.EnterWriteLock();
        try
        {
            var plugin = _plugins.FirstOrDefault(p => p.Instance.Manifest.Id == pluginId);
            if (plugin is null)
                return false;

            // Remove contributions from context (only if it was active)
            if (plugin.Status == PluginStatus.Active)
                _pluginContext?.RemovePluginContributions(pluginId);

            // Unregister external widgets before unloading the assembly
            ExternalWidgetRegistry.Instance.UnregisterAssembly(plugin.Assembly);

            // Unload the assembly context
            plugin.LoadContext.Unload();
            DeleteShadowDirectory(plugin.ShadowDirectory);

            _plugins.Remove(plugin);
            _logger.LogInformation("Unloaded plugin: {Id}", pluginId);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        // Fire event outside the lock to avoid deadlocks — subscribers may call
        // GetActivePluginIds() which needs a read lock.
        PluginUnloaded?.Invoke(pluginId);

        return true;
    }

    public bool LoadPlugin(string pluginPath)
    {
        if (_serviceProviderFactory is null || _hostVersion is null)
        {
            _logger.LogError("Cannot load plugin: PluginLoader has not been initialized.");
            return false;
        }

        if (BuildSourcePlugins && SourcePluginBuilder.IsSourcePlugin(pluginPath))
        {
            if (!SourcePluginBuilder.BuildSync(pluginPath, _logger))
            {
                RecordFailure(pluginPath, "Build failed");
                return false;
            }
        }

        string? loadedPluginId = null;
        string? loadFailureReason = null;
        PluginStatus loadedStatus = PluginStatus.Active;
        _lock.EnterWriteLock();
        try
        {
            var loaded = LoadPluginFromDirectory(pluginPath, _serviceProviderFactory(), out var loadFailReason);
            if (loaded is null)
            {
                RecordFailure(pluginPath, loadFailReason ?? "unknown", fireEvent: false);
                loadFailureReason = loadFailReason;
                goto done;
            }

            var manifest = loaded.Value.Instance.Manifest;

            if (_plugins.Any(p => p.Instance.Manifest.Id == manifest.Id))
            {
                _logger.LogError("Plugin '{Id}' is already loaded.", manifest.Id);
                goto done;
            }

            if (manifest.MinimumHostVersion is { } minVersion && _hostVersion < minVersion)
            {
                _logger.LogError(
                    "Plugin '{Id}' requires host version {Required} but current is {Current}.",
                    manifest.Id, minVersion, _hostVersion);
                loadFailureReason = $"Requires host version {minVersion} but current is {_hostVersion}";
                RecordFailure(pluginPath, loadFailureReason, fireEvent: false);
                goto done;
            }

            var plugin = new LoadedPlugin(loaded.Value.Instance, loaded.Value.Assembly, loaded.Value.Context, loaded.Value.Directory, loaded.Value.ShadowDirectory);

            // Validate configuration
            if (plugin.Instance.ConfigurationSchema is { } schema)
            {
                var errors = ValidatePluginConfiguration(
                    schema,
                    CreatePluginConfig(plugin.Instance));

                if (errors.Count > 0)
                {
                    _logger.LogWarning(
                        "Plugin '{Id}' configuration is incomplete: {Errors}. Plugin loaded as unconfigured.",
                        manifest.Id,
                        string.Join(", ", errors));
                    plugin.Status = PluginStatus.Unconfigured;
                    loadedStatus = PluginStatus.Unconfigured;

                    _knownPlugins[manifest.Id] = pluginPath;
                    _plugins.Add(plugin);
                    _failedPlugins.Remove(pluginPath);

                    _logger.LogInformation("Loaded plugin (unconfigured): {Id} v{Version}", manifest.Id, manifest.Version);
                    loadedPluginId = manifest.Id;

                    return true;
                }
            }

            // Configure context — plugin has valid config
            if (_pluginContext is not null)
            {
                try
                {
                    _pluginContext.SetCurrentPlugin(manifest.Id, pluginPath);
                    _pluginContext.SetPluginConfig(CreatePluginConfig(plugin.Instance));
                    plugin.Instance.Configure(_pluginContext);
                    _pluginContext.ClearCurrentPlugin();
                    _pluginContext.ClearPluginConfig();
                    _pluginContext.BuildPluginServiceProvider(manifest.Id, plugin.Services);
                }
                catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
                {
                    _pluginContext.ClearCurrentPlugin();
                    _pluginContext.ClearPluginConfig();
                    plugin.LoadContext.Unload();
                    DeleteShadowDirectory(plugin.ShadowDirectory);
                    RecordFailure(pluginPath, ex, fireEvent: false);
                    loadFailureReason = $"Exception during load: {ex.Message}";
                    goto done;
                }
            }

            plugin.Status = PluginStatus.Active;
            _knownPlugins[manifest.Id] = pluginPath;
            _plugins.Add(plugin);

            // Clear from failed plugins if it was there
            _failedPlugins.Remove(pluginPath);

            ExternalWidgetRegistry.Instance.RegisterAssembly(plugin.Assembly);
            _logger.LogInformation("Loaded plugin: {Id} v{Version}", manifest.Id, manifest.Version);

            loadedPluginId = manifest.Id;
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        // Reload app repository so newly added apps appear in the UI
        if (loadedPluginId is not null && loadedStatus == PluginStatus.Active)
        {
            _pluginContext?.ReloadApps();
            // Refresh any open tabs showing apps from this plugin (e.g. after reload)
            var appIds = _pluginContext?.GetPluginAppIds(loadedPluginId!);
            if (appIds is { Count: > 0 })
                _pluginContext!.RefreshApps(appIds);
        }

    done:
        // Fire event outside the lock to avoid deadlocks — subscribers may call
        // GetActivePluginIds() which needs a read lock.
        if (loadedPluginId is not null)
            PluginLoaded?.Invoke(loadedPluginId);
        else if (loadFailureReason is not null)
            PluginLoadFailed?.Invoke(Path.GetFileName(pluginPath));

        return loadedPluginId is not null;
    }

    public bool ReloadPlugin(string pluginId)
    {
        if (_serviceProviderFactory is null || _hostVersion is null)
        {
            _logger.LogError("Cannot reload plugin: PluginLoader has not been initialized.");
            return false;
        }

        var reloadLock = _reloadLocks.GetOrAdd(pluginId, _ => new object());
        lock (reloadLock)
        {
            return ReloadPluginCore(pluginId);
        }
    }

    private bool ReloadPluginCore(string pluginId)
    {
        string? directory;
        PluginStatus oldStatus;
        _lock.EnterReadLock();
        try
        {
            var plugin = _plugins.FirstOrDefault(p => p.Instance.Manifest.Id == pluginId);
            if (plugin is null)
            {
                // Plugin was discovered but failed to load previously — try a fresh load
                if (_knownPlugins.TryGetValue(pluginId, out var knownDir))
                {
                    _logger.LogInformation("Plugin '{Id}' previously failed to load, attempting fresh load.", pluginId);
                    directory = knownDir;
                }
                else
                {
                    _logger.LogWarning("Plugin '{Id}' not found for reload.", pluginId);
                    return false;
                }

                // Release lock before attempting load
                _lock.ExitReadLock();
                return LoadPlugin(directory);
            }
            directory = plugin.Directory;
            oldStatus = plugin.Status;
        }
        finally
        {
            if (_lock.IsReadLockHeld)
                _lock.ExitReadLock();
        }

        // Build source plugins before attempting reload
        if (BuildSourcePlugins && SourcePluginBuilder.IsSourcePlugin(directory))
        {
            if (!SourcePluginBuilder.BuildSync(directory, _logger))
            {
                _logger.LogError("Failed to reload plugin '{Id}': source build failed", pluginId);
                return false;
            }
        }

        // Load the new plugin assembly first, before unloading the old one
        var loaded = LoadPluginFromDirectory(directory, _serviceProviderFactory!(), out var reloadFailReason);
        if (loaded is null)
        {
            _logger.LogError("Failed to reload plugin '{Id}': {Reason}", pluginId, reloadFailReason ?? "new version could not be loaded");
            return false;
        }

        var manifest = loaded.Value.Instance.Manifest;
        if (manifest.MinimumHostVersion is { } minVersion && _hostVersion < minVersion)
        {
            _logger.LogError(
                "Plugin '{Id}' requires host version {Required} but current is {Current}.",
                manifest.Id, minVersion, _hostVersion);
            return false;
        }

        PluginStatus newStatus;

        // Call shutdown hook on old plugin before swapping (blocks up to 5s)
        _lock.EnterReadLock();
        try
        {
            var oldPlugin = _plugins.FirstOrDefault(p => p.Instance.Manifest.Id == pluginId);
            if (oldPlugin is not null && oldPlugin.Status == PluginStatus.Active)
                InvokeShutdownHook(oldPlugin.Instance, pluginId, PluginShutdownReason.Reload);
        }
        finally
        {
            _lock.ExitReadLock();
        }

        // Now atomically swap: unload old, insert new
        _lock.EnterWriteLock();
        try
        {
            var oldPlugin = _plugins.FirstOrDefault(p => p.Instance.Manifest.Id == pluginId);
            if (oldPlugin is not null)
            {
                if (oldPlugin.Status == PluginStatus.Active)
                    _pluginContext?.RemovePluginContributions(pluginId);
                ExternalWidgetRegistry.Instance.UnregisterAssembly(oldPlugin.Assembly);
                oldPlugin.LoadContext.Unload();
                DeleteShadowDirectory(oldPlugin.ShadowDirectory);
                _plugins.Remove(oldPlugin);
            }

            var newPlugin = new LoadedPlugin(loaded.Value.Instance, loaded.Value.Assembly, loaded.Value.Context, loaded.Value.Directory, loaded.Value.ShadowDirectory);

            // Validate configuration
            if (newPlugin.Instance.ConfigurationSchema is { } schema)
            {
                var errors = ValidatePluginConfiguration(
                    schema, CreatePluginConfig(newPlugin.Instance));

                if (errors.Count > 0)
                {
                    _logger.LogWarning(
                        "Plugin '{Id}' configuration is incomplete after reload: {Errors}. Plugin loaded as unconfigured.",
                        manifest.Id, string.Join(", ", errors));
                    newPlugin.Status = PluginStatus.Unconfigured;
                    newStatus = PluginStatus.Unconfigured;

                    _knownPlugins[manifest.Id] = directory;
                    _plugins.Add(newPlugin);
                    _failedPlugins.Remove(directory);

                    _logger.LogInformation("Reloaded plugin (unconfigured): {Id} v{Version}", manifest.Id, manifest.Version);

                    return true;
                }
            }

            if (_pluginContext is not null)
            {
                _pluginContext.SetCurrentPlugin(manifest.Id, directory);
                _pluginContext.SetPluginConfig(CreatePluginConfig(newPlugin.Instance));
                newPlugin.Instance.Configure(_pluginContext);
                _pluginContext.ClearCurrentPlugin();
                _pluginContext.ClearPluginConfig();
                _pluginContext.BuildPluginServiceProvider(manifest.Id, newPlugin.Services);
            }

            newPlugin.Status = PluginStatus.Active;
            newStatus = PluginStatus.Active;
            _knownPlugins[manifest.Id] = directory;
            _plugins.Add(newPlugin);
            _failedPlugins.Remove(directory);

            ExternalWidgetRegistry.Instance.RegisterAssembly(newPlugin.Assembly);
            _logger.LogInformation("Reloaded plugin: {Id} v{Version}", manifest.Id, manifest.Version);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        // Refresh UI after the swap is complete
        if (newStatus == PluginStatus.Active)
        {
            _pluginContext?.ReloadApps();
            var appIds = _pluginContext?.GetPluginAppIds(pluginId);
            if (appIds is { Count: > 0 })
                _pluginContext!.RefreshApps(appIds);
        }

        PluginReloaded?.Invoke(pluginId);

        // Fire activation/deactivation events based on state transitions
        if (oldStatus == PluginStatus.Unconfigured && newStatus == PluginStatus.Active)
            PluginActivated?.Invoke(pluginId);
        else if (oldStatus == PluginStatus.Active && newStatus == PluginStatus.Unconfigured)
            PluginDeactivated?.Invoke(pluginId);

        return true;
    }

    public bool ReconfigurePlugin(string pluginId)
    {
        if (_configFactory is null || _pluginContext is null)
        {
            _logger.LogError("Cannot reconfigure plugin: PluginLoader has not been initialized.");
            return false;
        }

        LoadedPlugin? plugin;
        _lock.EnterReadLock();
        try
        {
            plugin = _plugins.FirstOrDefault(p => p.Instance.Manifest.Id == pluginId);
            if (plugin is null)
            {
                _logger.LogWarning("Plugin '{Id}' not found for reconfiguration.", pluginId);
                return false;
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }

        var manifest = plugin.Instance.Manifest;
        var oldStatus = plugin.Status;

        // Validate the current configuration
        List<string> errors = [];
        if (plugin.Instance.ConfigurationSchema is { } schema)
        {
            errors = ValidatePluginConfiguration(
                schema,
                CreatePluginConfig(plugin.Instance));
        }

        if (errors.Count > 0)
        {
            // Configuration is invalid
            if (oldStatus == PluginStatus.Active)
            {
                // Call shutdown hook before deactivating
                InvokeShutdownHook(plugin.Instance, pluginId, PluginShutdownReason.Reconfigure);

                // Deactivate: remove contributions
                _lock.EnterWriteLock();
                try
                {
                    _pluginContext.RemovePluginContributions(pluginId);
                    plugin.Status = PluginStatus.Unconfigured;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }

                _logger.LogWarning(
                    "Plugin '{Id}' deactivated due to invalid configuration: {Errors}",
                    pluginId, string.Join(", ", errors));

                _pluginContext.ReloadApps();
                PluginDeactivated?.Invoke(pluginId);
            }
            return false;
        }

        // Call shutdown hook before reconfiguring (if already active)
        if (oldStatus == PluginStatus.Active)
            InvokeShutdownHook(plugin.Instance, pluginId, PluginShutdownReason.Reconfigure);

        // Configuration is valid — activate or reconfigure
        _lock.EnterWriteLock();
        try
        {
            // If already active, remove old contributions first
            if (oldStatus == PluginStatus.Active)
            {
                _pluginContext.RemovePluginContributions(pluginId);
            }

            _pluginContext.SetCurrentPlugin(manifest.Id, plugin.Directory);
            _pluginContext.SetPluginConfig(CreatePluginConfig(plugin.Instance));
            plugin.Instance.Configure(_pluginContext);
            _pluginContext.ClearCurrentPlugin();
            _pluginContext.ClearPluginConfig();
            _pluginContext.BuildPluginServiceProvider(manifest.Id, plugin.Services);
            plugin.Status = PluginStatus.Active;
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        _logger.LogInformation("Reconfigured plugin: {Id}", pluginId);

        _pluginContext.ReloadApps();
        var appIds = _pluginContext.GetPluginAppIds(pluginId);
        if (appIds is { Count: > 0 })
            _pluginContext.RefreshApps(appIds);

        if (oldStatus == PluginStatus.Unconfigured)
            PluginActivated?.Invoke(pluginId);

        return true;
    }

    public IReadOnlyList<string> GetActivePluginIds()
    {
        _lock.EnterReadLock();
        try
        {
            return _plugins
                .Where(p => p.Status == PluginStatus.Active)
                .Select(p => p.Instance.Manifest.Id)
                .ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public PluginManifest? GetPluginManifest(string pluginId)
    {
        _lock.EnterReadLock();
        try
        {
            return _plugins
                .FirstOrDefault(p => p.Instance.Manifest.Id == pluginId)
                ?.Instance.Manifest;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public PluginConfigurationSchema? GetPluginSchema(string pluginId)
    {
        _lock.EnterReadLock();
        try
        {
            return _plugins
                .FirstOrDefault(p => p.Instance.Manifest.Id == pluginId)
                ?.Instance.ConfigurationSchema;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public object? BuildPluginConfigurationView(string pluginId, IIvyPluginConfig config)
    {
        _lock.EnterReadLock();
        try
        {
            var plugin = _plugins.FirstOrDefault(p => p.Instance.Manifest.Id == pluginId);
            return plugin?.Instance.BuildConfigurationView(config);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IReadOnlyList<UnconfiguredPlugin> GetUnconfiguredPlugins()
    {
        _lock.EnterReadLock();
        try
        {
            var result = new List<UnconfiguredPlugin>();
            foreach (var plugin in _plugins.Where(p => p.Status == PluginStatus.Unconfigured))
            {
                var manifest = plugin.Instance.Manifest;
                var schema = plugin.Instance.ConfigurationSchema;
                if (schema is null) continue;

                var errors = _configFactory is not null
                    ? ValidatePluginConfiguration(schema, CreatePluginConfig(plugin.Instance))
                    : ["No configuration available"];

                result.Add(new UnconfiguredPlugin(
                    manifest.Id,
                    manifest.Title,
                    plugin.Directory,
                    schema,
                    errors));
            }
            return result;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IReadOnlyList<PluginCandidate> GetUnloadedPlugins()
    {
        _lock.EnterReadLock();
        try
        {
            var loadedIds = _plugins.Select(p => p.Instance.Manifest.Id).ToHashSet();
            var result = new List<PluginCandidate>();

            // Add known plugins that aren't loaded
            foreach (var (id, directory) in _knownPlugins)
            {
                if (!loadedIds.Contains(id))
                {
                    result.Add(new PluginCandidate(id, directory));
                }
            }

            // Add failed plugins
            foreach (var (directory, (reason, failedAt)) in _failedPlugins)
            {
                // Extract ID from directory name or use directory name as fallback
                var id = Path.GetFileName(directory);
                result.Add(new PluginCandidate(id, directory, reason, failedAt));
            }

            return result;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    internal string? GetPluginIdByDirectory(string directory)
    {
        _lock.EnterReadLock();
        try
        {
            return _knownPlugins.FirstOrDefault(kv => kv.Value == directory).Key;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    internal void ForgetPlugin(string pluginId)
    {
        _lock.EnterWriteLock();
        try
        {
            _knownPlugins.Remove(pluginId);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
        PluginRemoved?.Invoke(pluginId);
    }

    internal void RemoveFailedPlugin(string directory)
    {
        _lock.EnterWriteLock();
        try
        {
            _failedPlugins.Remove(directory);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
        PluginRemoved?.Invoke(Path.GetFileName(directory));
    }

    private static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(5);

    private void InvokeShutdownHook(IIvyPlugin pluginInstance, string pluginId, PluginShutdownReason reason)
    {
        using var cts = new CancellationTokenSource(ShutdownTimeout);
        var shutdownContext = new PluginShutdownContext
        {
            CancellationToken = cts.Token,
            Reason = reason,
            Logger = _logger
        };

        try
        {
            Task.Run(() => pluginInstance.ShutdownAsync(shutdownContext)).Wait(cts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Plugin '{Id}' shutdown timed out after {Timeout}s", pluginId, ShutdownTimeout.TotalSeconds);
        }
        catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
        {
            _logger.LogWarning("Plugin '{Id}' shutdown timed out after {Timeout}s", pluginId, ShutdownTimeout.TotalSeconds);
        }
        catch (AggregateException ex)
        {
            _logger.LogError(ex.InnerException ?? ex, "Plugin '{Id}' shutdown failed", pluginId);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex, "Plugin '{Id}' shutdown failed", pluginId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Plugin '{Id}' shutdown failed", pluginId);
        }
    }

    public async Task ShutdownAllAsync()
    {
        List<(IIvyPlugin Instance, string Id)> activePlugins;
        _lock.EnterReadLock();
        try
        {
            activePlugins = _plugins
                .Where(p => p.Status == PluginStatus.Active)
                .Select(p => (p.Instance, p.Instance.Manifest.Id))
                .ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }

        if (activePlugins.Count == 0)
            return;

        using var cts = new CancellationTokenSource(ShutdownTimeout);
        var tasks = activePlugins.Select(async p =>
        {
            var shutdownContext = new PluginShutdownContext
            {
                CancellationToken = cts.Token,
                Reason = PluginShutdownReason.HostExit,
                Logger = _logger
            };

            try
            {
                await p.Instance.ShutdownAsync(shutdownContext).WaitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Plugin '{Id}' shutdown timed out during host exit", p.Id);
            }
            catch (AggregateException ex)
            {
                _logger.LogError(ex.InnerException ?? ex, "Plugin '{Id}' shutdown failed during host exit", p.Id);
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "Plugin '{Id}' shutdown failed during host exit", p.Id);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Plugin '{Id}' shutdown failed during host exit", p.Id);
            }
        });

        await Task.WhenAll(tasks);
    }

    private void RecordFailure(string directory, Exception ex, bool fireEvent = true)
        => RecordFailure(directory, $"Exception during load: {ex.Message}", fireEvent);

    private void RecordFailure(string directory, string reason, bool fireEvent = true)
    {
        _logger.LogError("Failed to load plugin from {Directory}: {Reason}", directory, reason);

        var writeLockHeld = _lock.IsWriteLockHeld;
        if (writeLockHeld)
        {
            _failedPlugins[directory] = (reason, DateTime.UtcNow);
        }
        else
        {
            _lock.EnterWriteLock();
            try
            {
                _failedPlugins[directory] = (reason, DateTime.UtcNow);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        if (fireEvent)
        {
            if (writeLockHeld)
            {
                throw new InvalidOperationException("Cannot fire PluginLoadFailed event while holding write lock");
            }

            PluginLoadFailed?.Invoke(Path.GetFileName(directory));
        }
    }

    private void TryLoadCandidate(
        string directory,
        IServiceProvider serviceProvider,
        Version hostVersion,
        List<(IIvyPlugin Instance, Assembly Assembly, AssemblyLoadContext Context, string Directory, string ShadowDirectory)> candidates)
    {
        try
        {
            if (BuildSourcePlugins && SourcePluginBuilder.IsSourcePlugin(directory) && !SourcePluginBuilder.BuildSync(directory, _logger))
            {
                RecordFailure(directory, "Source plugin build failed");
                return;
            }

            var loaded = LoadPluginFromDirectory(directory, serviceProvider, out var reason);
            if (loaded is null)
            {
                RecordFailure(directory, reason ?? "No valid plugin found");
                return;
            }

            var manifest = loaded.Value.Instance.Manifest;

            if (manifest.MinimumHostVersion is { } minVersion && hostVersion < minVersion)
            {
                _logger.LogError(
                    "Plugin '{Id}' requires host version {Required} but current is {Current}. Skipping.",
                    manifest.Id, minVersion, hostVersion);
                return;
            }

            ValidatePluginIcon(manifest, directory);

            // Reject duplicates — if a plugin with this ID was already discovered, skip it.
            var existingIndex = candidates.FindIndex(c => c.Instance.Manifest.Id == manifest.Id);
            if (existingIndex >= 0)
            {
                _logger.LogWarning(
                    "Plugin '{Id}' from '{Directory}' skipped: already loaded from '{ExistingDirectory}'.",
                    manifest.Id, directory, candidates[existingIndex].Directory);
                loaded.Value.Context.Unload();
                DeleteShadowDirectory(loaded.Value.ShadowDirectory);
                return;
            }

            _knownPlugins[manifest.Id] = directory;
            candidates.Add(loaded.Value);
        }
        catch (IOException ex)
        {
            RecordFailure(directory, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            RecordFailure(directory, ex);
        }
        catch (BadImageFormatException ex)
        {
            RecordFailure(directory, ex);
        }
        catch (ReflectionTypeLoadException ex)
        {
            RecordFailure(directory, ex);
        }
        catch (InvalidOperationException ex)
        {
            RecordFailure(directory, ex);
        }
    }

    internal void AddTestPlugin(IIvyPlugin instance, string directory)
    {
        _lock.EnterWriteLock();
        try
        {
            _plugins.Add(new LoadedPlugin(instance, typeof(PluginLoader).Assembly, AssemblyLoadContext.Default, directory));
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    private void ValidatePluginIcon(PluginManifest manifest, string directory)
    {
        if (manifest.Icon is { Kind: PluginIconKind.File } icon)
        {
            if (string.IsNullOrWhiteSpace(icon.Value) || Path.IsPathRooted(icon.Value))
            {
                _logger.LogWarning(
                    "Plugin '{Id}' has an invalid icon path '{Path}' (must be relative).",
                    manifest.Id, icon.Value);
                return;
            }

            var fullPath = Path.GetFullPath(Path.Join(directory, icon.Value));
            if (!File.Exists(fullPath))
            {
                _logger.LogWarning(
                    "Plugin '{Id}' references icon file '{Path}' which does not exist in '{Directory}'.",
                    manifest.Id, icon.Value, directory);
            }
        }
    }

    internal List<string> ValidatePluginConfiguration(
        PluginConfigurationSchema schema,
        IIvyPluginConfig config)
    {
        var errors = new List<string>();

        foreach (var field in schema.Fields)
        {
            var value = config.GetValue(field.Key);

            if (field.IsRequired && string.IsNullOrEmpty(value))
            {
                errors.Add($"Required field '{field.Key}' is missing");
                continue;
            }

            if (!string.IsNullOrEmpty(value) && !ValidateFieldType(value, field.Type))
            {
                errors.Add($"Field '{field.Key}' has invalid type (expected {field.Type})");
            }
        }

        return errors;
    }

    internal static bool ValidateFieldType(string value, ConfigFieldType type)
    {
        return type switch
        {
            ConfigFieldType.Integer => int.TryParse(value, out _),
            ConfigFieldType.Boolean => bool.TryParse(value, out _),
            _ => true // String and Secret are always valid if non-empty
        };
    }

    private IIvyPluginConfig CreatePluginConfig(IIvyPlugin plugin)
    {
        if (_configFactory is null)
            throw new InvalidOperationException(
                $"No IIvyPluginConfigFactory has been set. Cannot create config for plugin '{plugin.Manifest.Id}'.");

        var config = _configFactory.Create(plugin.Manifest.Id);

        if (plugin.ConfigurationSchema is { } schema)
        {
            var defaults = schema.Fields
                .Where(f => f.DefaultValue is not null)
                .ToDictionary(f => f.Key, f => f.DefaultValue!);

            if (defaults.Count > 0)
                return new PluginConfigWithDefaults(config, defaults);
        }

        return config;
    }

    private sealed class PluginConfigWithDefaults(IIvyPluginConfig inner, IReadOnlyDictionary<string, string> defaults) : IIvyPluginConfig
    {
        public string? GetValue(string key)
        {
            var value = inner.GetValue(key);
            if (string.IsNullOrEmpty(value) && defaults.TryGetValue(key, out var defaultValue))
                return defaultValue;
            return value;
        }

        public void SetValue(string key, string value) => inner.SetValue(key, value);
        public void RemoveValue(string key) => inner.RemoveValue(key);
        public void Save() => inner.Save();
    }

    private void DeleteShadowDirectory(string? shadowDirectory)
    {
        if (shadowDirectory is null) return;
        try
        {
            if (Directory.Exists(shadowDirectory))
                Directory.Delete(shadowDirectory, recursive: true);
        }
        catch (IOException ex)
        {
            _logger.LogDebug(ex, "Failed to delete shadow directory: {Directory}", shadowDirectory);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogDebug(ex, "Failed to delete shadow directory: {Directory}", shadowDirectory);
        }
    }

    private void CleanupAllShadowDirectories()
    {
        _lock.EnterReadLock();
        try
        {
            foreach (var plugin in _plugins)
                DeleteShadowDirectory(plugin.ShadowDirectory);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Checks that the plugin's deps.json does not require newer versions of shared assemblies
    /// than what the host currently provides. Returns an error message if incompatible, null if OK.
    /// </summary>
    private string? CheckSharedAssemblyCompatibility(string shadowDir, string originalDirectory)
    {
        // Find all .deps.json files in the shadow directory
        var depsFiles = Directory.GetFiles(shadowDir, "*.deps.json");
        if (depsFiles.Length == 0)
            return null; // No deps.json — can't verify, allow loading (will fail naturally if incompatible)

        foreach (var depsFile in depsFiles)
        {
            try
            {
                using var stream = File.OpenRead(depsFile);
                using var doc = JsonDocument.Parse(stream);

                if (!doc.RootElement.TryGetProperty("libraries", out var libraries))
                    continue;

                foreach (var library in libraries.EnumerateObject())
                {
                    // Library keys are in the format "PackageName/Version"
                    var slashIndex = library.Name.LastIndexOf('/');
                    if (slashIndex < 0) continue;

                    var packageName = library.Name[..slashIndex];

                    // Only check assemblies that are in our shared set
                    if (!_sharedAssemblyNames.Contains(packageName))
                        continue;

                    var versionStr = library.Name[(slashIndex + 1)..];
                    if (!Version.TryParse(versionStr, out var requiredVersion))
                        continue;

                    // Get the host's version of this assembly
                    var hostAssembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name == packageName);

                    if (hostAssembly is null)
                    {
                        // Try loading it to get its version
                        try
                        {
                            hostAssembly = Assembly.Load(new AssemblyName(packageName));
                        }
                        catch (FileNotFoundException)
                        {
                            // Assembly not available in host — plugin may still work if it's optional
                            continue;
                        }
                        catch (FileLoadException)
                        {
                            continue;
                        }
                        catch (BadImageFormatException)
                        {
                            continue;
                        }
                    }

                    var hostVersion = hostAssembly.GetName().Version;
                    if (hostVersion is null)
                        continue;

                    // Version 0.0.0.0 means the host was built from source without assembly
                    // versioning (e.g. local dev with GenerateAssemblyInfo=false) — skip check.
                    if (hostVersion == new Version(0, 0, 0, 0))
                        continue;

                    // Compare major.minor (ignore patch/revision for flexibility)
                    // A plugin requiring 2.0.0 should not load on a host with 1.x
                    if (requiredVersion.Major > hostVersion.Major ||
                        (requiredVersion.Major == hostVersion.Major && requiredVersion.Minor > hostVersion.Minor))
                    {
                        return $"Requires {packageName} >= {requiredVersion} but host provides {hostVersion}";
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogDebug(ex, "Could not parse {DepsFile}, skipping compatibility check.", depsFile);
            }
            catch (IOException ex)
            {
                _logger.LogDebug(ex, "Could not read {DepsFile}, skipping compatibility check.", depsFile);
            }
        }

        return null;
    }
}

public record LoadedPlugin(
    IIvyPlugin Instance,
    Assembly Assembly,
    AssemblyLoadContext LoadContext,
    string Directory,
    string? ShadowDirectory = null)
{
    public PluginStatus Status { get; internal set; } = PluginStatus.Active;
    public ServiceCollection Services { get; } = new();
}
