using System.Reflection;
using System.Runtime.Loader;
using Ivy.Plugins;
using Ivy.Plugins.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ivy.Core.Plugins;

internal class PluginContextWithConfiguration(PluginContextBase inner, IConfiguration configuration) : IIvyPluginContext
{
    public IServiceCollection Services => inner.Services;
    public IConfiguration Configuration => configuration;

    public void AddApp(AppDescriptor descriptor) => inner.AddApp(descriptor);
    public void AddAppsFromAssembly(Assembly assembly) => inner.AddAppsFromAssembly(assembly);
    public void AddMenuItems(Func<IEnumerable<MenuItem>, IEnumerable<MenuItem>> transformer) => inner.AddMenuItems(transformer);
    public void AddFooterMenuItems(Func<IEnumerable<MenuItem>, INavigator, IEnumerable<MenuItem>> transformer) => inner.AddFooterMenuItems(transformer);
    public void AddBadgeProvider(string menuTag, Func<IServiceProvider, int> countProvider) => inner.AddBadgeProvider(menuTag, countProvider);
    public void RegisterMessagingChannel(IMessagingChannel channel) => inner.RegisterMessagingChannel(channel);
    public void UseWebApplication(Action<WebApplication> configure) => inner.UseWebApplication(configure);
    public void UseWebApplicationBuilder(Action<WebApplicationBuilder> configure) => inner.UseWebApplicationBuilder(configure);
}

public class PluginLoader : IPluginManager
{
    private readonly string _pluginsDirectory;
    private readonly ILogger<PluginLoader> _logger;
    private readonly IReadOnlySet<string> _sharedAssemblyNames;
    private readonly List<LoadedPlugin> _plugins = [];
    private readonly Dictionary<string, string> _knownPlugins = new(); // id -> directory
    private readonly Dictionary<string, (string Reason, DateTime FailedAt)> _failedPlugins = new(); // directory -> failure info
    private readonly ReaderWriterLockSlim _lock = new();
    private PluginContextBase? _pluginContext;
    private IConfiguration? _configuration;
    private Func<IServiceProvider>? _serviceProviderFactory;
    private Version? _hostVersion;

    public event Action<string>? PluginLoaded;
    public event Action<string>? PluginUnloaded;
    public event Action<string>? PluginReloaded;

    internal PluginLoader(string pluginsDirectory, ILogger<PluginLoader> logger, IEnumerable<string>? sharedAssemblyNames = null)
    {
        _pluginsDirectory = pluginsDirectory;
        _logger = logger;
        _sharedAssemblyNames = new HashSet<string>(sharedAssemblyNames ?? [])
        {
            "Ivy.Plugin.Abstractions",
            "Ivy"
        };
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

        var candidates = new List<(IIvyPlugin Instance, Assembly Assembly, AssemblyLoadContext Context, string Directory)>();

        foreach (var directory in Directory.GetDirectories(_pluginsDirectory))
        {
            try
            {
                var loaded = LoadPluginFromDirectory(directory, serviceProvider);
                if (loaded is null)
                {
                    // Track that this directory failed to load
                    _lock.EnterWriteLock();
                    try
                    {
                        _failedPlugins[directory] = (
                            "No valid plugin found (no DLLs, no [IvyPlugin] attribute, or multiple attributes)",
                            DateTime.UtcNow);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                    continue;
                }

                var manifest = loaded.Value.Instance.Manifest;

                if (manifest.MinimumHostVersion is { } minVersion && hostVersion < minVersion)
                {
                    _logger.LogError(
                        "Plugin '{Id}' requires host version {Required} but current is {Current}. Skipping.",
                        manifest.Id, minVersion, hostVersion);
                    continue;
                }

                _knownPlugins[manifest.Id] = directory;
                candidates.Add(loaded.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load plugin from {Directory}. Skipping.", directory);

                _lock.EnterWriteLock();
                try
                {
                    _failedPlugins[directory] = (
                        $"Exception during load: {ex.Message}",
                        DateTime.UtcNow);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        var sorted = TopologicalSort(candidates);
        _lock.EnterWriteLock();
        try
        {
            _plugins.AddRange(sorted.Select(c => new LoadedPlugin(c.Instance, c.Assembly, c.Context, c.Directory)));
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        foreach (var plugin in _plugins)
            _logger.LogInformation("Loaded plugin: {Id} v{Version}", plugin.Instance.Manifest.Id, plugin.Instance.Manifest.Version);
    }

    private (IIvyPlugin Instance, Assembly Assembly, AssemblyLoadContext Context, string Directory)? LoadPluginFromDirectory(
        string directory, IServiceProvider serviceProvider)
    {
        var dllFiles = Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .ToArray();
        if (dllFiles.Length == 0)
        {
            _logger.LogWarning("No DLL files found in plugin directory: {Directory}", directory);
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
            catch (Exception ex)
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
                return null;
            }

            found.Add((attr.PluginType, assembly));
        }

        if (found.Count == 0)
        {
            _logger.LogDebug("No [assembly: IvyPlugin] attribute found in {Directory}. Skipping.", directory);
            return null;
        }

        if (found.Count > 1)
        {
            _logger.LogError(
                "Multiple [assembly: IvyPlugin] attributes found in {Directory} ({Assemblies}). Skipping.",
                directory, string.Join(", ", found.Select(f => Path.GetFileName(f.Assembly.Location))));
            return null;
        }

        var (pluginType, pluginAssembly) = found[0];
        var instance = (IIvyPlugin)ActivatorUtilities.CreateInstance(serviceProvider, pluginType);
        return (instance, pluginAssembly, loadContext, directory);
    }

    private List<(IIvyPlugin Instance, Assembly Assembly, AssemblyLoadContext Context, string Directory)> TopologicalSort(
        List<(IIvyPlugin Instance, Assembly Assembly, AssemblyLoadContext Context, string Directory)> candidates)
    {
        var byId = candidates.ToDictionary(c => c.Instance.Manifest.Id);
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>(); // cycle detection
        var sorted = new List<(IIvyPlugin, Assembly, AssemblyLoadContext, string)>();

        foreach (var candidate in candidates)
        {
            Visit(candidate.Instance.Manifest.Id);
        }

        return sorted;

        void Visit(string id)
        {
            if (visited.Contains(id)) return;

            if (!visiting.Add(id))
            {
                _logger.LogError("Circular dependency detected involving plugin '{Id}'. Skipping.", id);
                return;
            }

            if (byId.TryGetValue(id, out var candidate))
            {
                foreach (var dep in candidate.Instance.Manifest.Dependencies)
                {
                    if (!byId.ContainsKey(dep))
                    {
                        _logger.LogWarning("Plugin '{Id}' depends on '{Dep}' which is not loaded.", id, dep);
                        continue;
                    }
                    Visit(dep);
                }

                sorted.Add(candidate);
            }

            visiting.Remove(id);
            visited.Add(id);
        }
    }

    public void ConfigureServices(IServiceCollection hostServices, IConfiguration configuration)
    {
        _configuration = configuration;
        _lock.EnterReadLock();
        try
        {
            foreach (var plugin in _plugins)
                plugin.Instance.ConfigureServices(plugin.Services, configuration);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    internal void SetServiceProviderFactory(Func<IServiceProvider> factory)
    {
        _serviceProviderFactory = factory;
    }

    public void Configure(PluginContextBase context)
    {
        _pluginContext = context;
        var invalidPlugins = new List<LoadedPlugin>();

        _lock.EnterReadLock();
        try
        {
            foreach (var plugin in _plugins)
            {
                IIvyPluginContext contextToUse = context;

                if (plugin.Instance.ConfigurationSchema is { } schema)
                {
                    var errors = ValidatePluginConfiguration(
                        plugin.Instance.Manifest.ConfigSectionName,
                        schema,
                        context.Configuration);

                    if (errors.Count > 0)
                    {
                        _logger.LogError(
                            "Plugin '{Id}' configuration is invalid: {Errors}",
                            plugin.Instance.Manifest.Id,
                            string.Join(", ", errors));
                        invalidPlugins.Add(plugin);
                        continue;
                    }

                    // Apply defaults before calling Configure
                    var configWithDefaults = ApplyConfigurationDefaults(
                        plugin.Instance.Manifest.ConfigSectionName,
                        schema,
                        context.Configuration);

                    // Create context wrapper with defaults applied
                    contextToUse = new PluginContextWithConfiguration(context, configWithDefaults);
                }

                context.SetCurrentPlugin(plugin.Instance.Manifest.Id, plugin.Directory);
                plugin.Instance.Configure(contextToUse);
                context.ClearCurrentPlugin();
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }

        if (invalidPlugins.Count > 0)
        {
            _lock.EnterWriteLock();
            try
            {
                foreach (var plugin in invalidPlugins)
                {
                    _plugins.Remove(plugin);
                    _failedPlugins[plugin.Directory] = (
                        $"Configuration invalid for '{plugin.Instance.Manifest.Id}'",
                        DateTime.UtcNow);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    public bool UnloadPlugin(string pluginId)
    {
        _lock.EnterWriteLock();
        try
        {
            var plugin = _plugins.FirstOrDefault(p => p.Instance.Manifest.Id == pluginId);
            if (plugin is null)
            {
                _logger.LogWarning("Plugin '{Id}' not found for unload.", pluginId);
                return false;
            }

            // Check if other loaded plugins depend on this one
            var dependents = _plugins
                .Where(p => p.Instance.Manifest.Id != pluginId &&
                            p.Instance.Manifest.Dependencies.Contains(pluginId))
                .Select(p => p.Instance.Manifest.Id)
                .ToList();

            if (dependents.Count > 0)
            {
                _logger.LogError(
                    "Cannot unload plugin '{Id}': plugins [{Dependents}] depend on it.",
                    pluginId, string.Join(", ", dependents));
                return false;
            }

            // Remove contributions from context
            _pluginContext?.RemovePluginContributions(pluginId);

            // Dispose the plugin's service provider
            (plugin.ServiceProvider as IDisposable)?.Dispose();

            // Unload the assembly context
            plugin.LoadContext.Unload();

            _plugins.Remove(plugin);
            _logger.LogInformation("Unloaded plugin: {Id}", pluginId);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        // Fire event outside the lock to avoid deadlocks — subscribers may call
        // GetLoadedPluginIds() which needs a read lock.
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

        string? loadedPluginId = null;
        _lock.EnterWriteLock();
        try
        {
            var loaded = LoadPluginFromDirectory(pluginPath, _serviceProviderFactory());
            if (loaded is null)
            {
                _logger.LogError("Failed to load plugin from {Path}.", pluginPath);
                return false;
            }

            var manifest = loaded.Value.Instance.Manifest;

            if (_plugins.Any(p => p.Instance.Manifest.Id == manifest.Id))
            {
                _logger.LogError("Plugin '{Id}' is already loaded.", manifest.Id);
                return false;
            }

            if (manifest.MinimumHostVersion is { } minVersion && _hostVersion < minVersion)
            {
                _logger.LogError(
                    "Plugin '{Id}' requires host version {Required} but current is {Current}.",
                    manifest.Id, minVersion, _hostVersion);
                return false;
            }

            // Check dependencies are loaded
            foreach (var dep in manifest.Dependencies)
            {
                if (_plugins.All(p => p.Instance.Manifest.Id != dep))
                {
                    _logger.LogError("Plugin '{Id}' depends on '{Dep}' which is not loaded.", manifest.Id, dep);
                    return false;
                }
            }

            var plugin = new LoadedPlugin(loaded.Value.Instance, loaded.Value.Assembly, loaded.Value.Context, loaded.Value.Directory);

            // Configure services
            if (_configuration is not null)
                plugin.Instance.ConfigureServices(plugin.Services, _configuration);

            // Validate configuration before Configure
            if (plugin.Instance.ConfigurationSchema is { } schema && _configuration is not null)
            {
                var errors = ValidatePluginConfiguration(
                    manifest.ConfigSectionName,
                    schema,
                    _configuration);

                if (errors.Count > 0)
                {
                    _logger.LogError(
                        "Plugin '{Id}' configuration is invalid: {Errors}. Plugin load failed.",
                        manifest.Id,
                        string.Join(", ", errors));

                    (plugin.ServiceProvider as IDisposable)?.Dispose();
                    plugin.LoadContext.Unload();
                    return false;
                }
            }

            // Configure context
            if (_pluginContext is not null)
            {
                IIvyPluginContext contextToUse = _pluginContext;

                // Apply defaults if schema exists
                if (plugin.Instance.ConfigurationSchema is { } configSchema && _configuration is not null)
                {
                    var configWithDefaults = ApplyConfigurationDefaults(
                        manifest.ConfigSectionName,
                        configSchema,
                        _configuration);

                    contextToUse = new PluginContextWithConfiguration(_pluginContext, configWithDefaults);
                }

                _pluginContext.SetCurrentPlugin(manifest.Id, pluginPath);
                plugin.Instance.Configure(contextToUse);
                _pluginContext.ClearCurrentPlugin();
                _pluginContext.BuildPluginServiceProvider(manifest.Id, plugin.Services);
                plugin.ServiceProvider = _pluginContext.GetPluginServiceProvider(manifest.Id);
            }

            _knownPlugins[manifest.Id] = pluginPath;
            _plugins.Add(plugin);

            // Clear from failed plugins if it was there
            _failedPlugins.Remove(pluginPath);

            _logger.LogInformation("Loaded plugin: {Id} v{Version}", manifest.Id, manifest.Version);

            loadedPluginId = manifest.Id;
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        // Reload app repository so newly added apps appear in the UI
        if (loadedPluginId is not null)
        {
            _pluginContext?.ReloadApps();
            // Refresh any open tabs showing apps from this plugin (e.g. after reload)
            var appIds = _pluginContext?.GetPluginAppIds(loadedPluginId!);
            if (appIds is { Count: > 0 })
                _pluginContext!.RefreshApps(appIds);
        }

        // Fire event outside the lock to avoid deadlocks — subscribers may call
        // GetLoadedPluginIds() which needs a read lock.
        if (loadedPluginId is not null)
            PluginLoaded?.Invoke(loadedPluginId);

        return loadedPluginId is not null;
    }

    public bool ReloadPlugin(string pluginId)
    {
        _lock.EnterReadLock();
        string? directory;
        try
        {
            var plugin = _plugins.FirstOrDefault(p => p.Instance.Manifest.Id == pluginId);
            if (plugin is null)
            {
                _logger.LogWarning("Plugin '{Id}' not found for reload.", pluginId);
                return false;
            }
            directory = plugin.Directory;
        }
        finally
        {
            _lock.ExitReadLock();
        }

        if (!UnloadPlugin(pluginId)) return false;
        var loaded = LoadPlugin(directory);
        if (loaded)
        {
            PluginReloaded?.Invoke(pluginId);
        }
        return loaded;
    }

    public IReadOnlyList<string> GetLoadedPluginIds()
    {
        _lock.EnterReadLock();
        try
        {
            return _plugins.Select(p => p.Instance.Manifest.Id).ToList();
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

    internal List<string> ValidatePluginConfiguration(
        string configSectionName,
        PluginConfigurationSchema schema,
        IConfiguration config)
    {
        var errors = new List<string>();
        var section = config.GetSection($"Plugins:{configSectionName}");

        foreach (var field in schema.Fields)
        {
            var value = section[field.Key];

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

    internal IConfiguration ApplyConfigurationDefaults(
        string configSectionName,
        PluginConfigurationSchema schema,
        IConfiguration config)
    {
        var defaults = new Dictionary<string, string?>();
        var section = config.GetSection($"Plugins:{configSectionName}");

        foreach (var field in schema.Fields.Where(f => f.DefaultValue is not null))
        {
            var value = section[field.Key];

            // Only inject default if field is missing or empty
            if (string.IsNullOrEmpty(value))
            {
                defaults[$"Plugins:{configSectionName}:{field.Key}"] = field.DefaultValue;
            }
        }

        // Create a configuration that layers defaults under the actual config
        // Actual config takes precedence over defaults
        return new ConfigurationBuilder()
            .AddInMemoryCollection(defaults)
            .AddConfiguration(config)
            .Build();
    }
}

public record LoadedPlugin(
    IIvyPlugin Instance,
    Assembly Assembly,
    AssemblyLoadContext LoadContext,
    string Directory)
{
    public ServiceCollection Services { get; } = new();
    public IServiceProvider? ServiceProvider { get; internal set; }
}
