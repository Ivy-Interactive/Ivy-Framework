using System.Reflection;
using Ivy.Core.Apps;
using Ivy.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Core.Plugins;

/// <summary>
/// Base class for plugin context implementations. NOT intended for use by plugins directly.
/// This is an internal implementation detail of the plugin hosting infrastructure.
/// Plugins should only depend on the <see cref="IIvyPluginContext"/> (or derived) interfaces.
/// </summary>
public abstract class PluginContextBase : IIvyExtendedPluginContext, IPluginServiceProvider, IPluginMenuContributions
{
    protected AppRepository AppRepository { get; }
    protected IReadOnlySet<string> ReservedPaths { get; }
    protected WebApplicationBuilder Builder { get; }

    public PluginContextBase(Ivy.Server server, WebApplicationBuilder builder)
    {
        AppRepository = server.AppRepository;
        ReservedPaths = server.ReservedPaths;
        Builder = builder;
    }

    protected PluginContextBase(AppRepository appRepository, IReadOnlySet<string> reservedPaths, WebApplicationBuilder builder)
    {
        AppRepository = appRepository;
        ReservedPaths = reservedPaths;
        Builder = builder;
    }
    private readonly List<(Func<IEnumerable<MenuItem>, IEnumerable<MenuItem>> Transformer, int Priority)> _menuTransformers = [];
    private readonly List<(string Tag, Func<IServiceProvider, int> CountProvider)> _badgeProviders = [];
    private readonly List<Action<WebApplication>> _appActions = [];
    private readonly AggregatePluginServiceProvider _aggregateProvider = new();
    private readonly Dictionary<string, PluginState> _pluginStates = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private string? _currentPluginId;
    protected string? CurrentPluginId => _currentPluginId;

    public IServiceCollection Services
    {
        get
        {
            if (_currentPluginId is not null && _pluginStates.TryGetValue(_currentPluginId, out var state))
                return state.PluginServices;
            return _fallbackServices;
        }
    }

    // Fallback service collection for non-plugin code
    private readonly ServiceCollection _fallbackServices = new();

    private IIvyPluginConfig? _currentPluginConfig;

    public IIvyPluginConfig Config => _currentPluginConfig ?? throw new InvalidOperationException("No plugin config is currently active.");

    internal void SetPluginConfig(IIvyPluginConfig config) => _currentPluginConfig = config;
    internal void ClearPluginConfig() => _currentPluginConfig = null;

    public IReadOnlyList<Func<IEnumerable<MenuItem>, IEnumerable<MenuItem>>> MenuTransformers
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _menuTransformers
                    .OrderBy(t => t.Priority)
                    .Select(t => t.Transformer)
                    .ToList();
            }
            finally { _lock.ExitReadLock(); }
        }
    }

    public IReadOnlyList<(string Tag, Func<IServiceProvider, int> CountProvider)> BadgeProviders
    {
        get
        {
            _lock.EnterReadLock();
            try { return _badgeProviders.ToList(); }
            finally { _lock.ExitReadLock(); }
        }
    }

    internal void SetCurrentPlugin(string pluginId, string directory)
    {
        _currentPluginId = pluginId;
        _lock.EnterWriteLock();
        try
        {
            if (!_pluginStates.ContainsKey(pluginId))
                _pluginStates[pluginId] = new PluginState(pluginId, directory);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    internal void ClearCurrentPlugin() => _currentPluginId = null;

    public void AddApp(AppDescriptor descriptor)
    {
        Func<AppDescriptor[]> factory = () => [descriptor];
        AppRepository.AddFactory(factory);

        if (_currentPluginId is not null && _pluginStates.TryGetValue(_currentPluginId, out var state))
            state.AppFactories.Add(factory);
    }

    public void AddAppsFromAssembly(Assembly assembly)
    {
        Func<AppDescriptor[]> factory = () => AppHelpers.GetApps(assembly);
        AppRepository.AddFactory(factory);

        if (_currentPluginId is not null && _pluginStates.TryGetValue(_currentPluginId, out var state))
            state.AppFactories.Add(factory);
    }

    public void AddMenuItems(Func<IEnumerable<MenuItem>, IEnumerable<MenuItem>> transformer, int priority = 0)
    {
        _lock.EnterWriteLock();
        try
        {
            _menuTransformers.Add((transformer, priority));
            if (_currentPluginId is not null && _pluginStates.TryGetValue(_currentPluginId, out var state))
                state.MenuTransformers.Add((transformer, priority));
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void AddBadgeProvider(string menuTag, Func<IServiceProvider, int> countProvider)
    {
        _lock.EnterWriteLock();
        try
        {
            _badgeProviders.Add((menuTag, countProvider));
            if (_currentPluginId is not null && _pluginStates.TryGetValue(_currentPluginId, out var state))
                state.BadgeProviders.Add((menuTag, countProvider));
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void UseWebApplication(Action<WebApplication> configure)
    {
        _lock.EnterWriteLock();
        try
        {
            _appActions.Add(configure);
            if (_currentPluginId is not null && _pluginStates.TryGetValue(_currentPluginId, out var state))
                state.AppActions.Add(configure);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void UseWebApplicationBuilder(Action<WebApplicationBuilder> configure)
    {
        configure(Builder);
    }

    public void BuildServiceProvider()
    {
        _lock.EnterReadLock();
        try
        {
            foreach (var (pluginId, state) in _pluginStates)
            {
                var provider = state.PluginServices.BuildServiceProvider();
                _aggregateProvider.AddProvider(pluginId, provider);
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }

        // Also build fallback services if any were registered outside plugin context
        if (_fallbackServices.Count > 0)
        {
            var fallbackProvider = _fallbackServices.BuildServiceProvider();
            _aggregateProvider.AddProvider("__fallback__", fallbackProvider);
        }
    }

    internal void BuildPluginServiceProvider(string pluginId, IServiceCollection services)
    {
        var provider = services.BuildServiceProvider();
        _aggregateProvider.AddProvider(pluginId, provider);

        if (_pluginStates.TryGetValue(pluginId, out var state))
        {
            // Also build the plugin's context-level services
            var contextProvider = state.PluginServices.BuildServiceProvider();
            _aggregateProvider.AddProvider($"{pluginId}__context", contextProvider);
        }
    }

    public T? GetService<T>() where T : class
    {
        return _aggregateProvider.GetService<T>();
    }

    public IEnumerable<T> GetServices<T>() where T : class
    {
        return _aggregateProvider.GetServices<T>();
    }

    internal virtual void RemovePluginContributions(string pluginId)
    {
        HashSet<string> affectedAppIds;

        _lock.EnterWriteLock();
        try
        {
            if (!_pluginStates.TryGetValue(pluginId, out var state)) return;

            // Collect app IDs before removing factories so we can reload affected sessions
            affectedAppIds = GetAppIdsFromFactories(state.AppFactories);

            foreach (var t in state.MenuTransformers)
                _menuTransformers.Remove(t);

            foreach (var b in state.BadgeProviders)
                _badgeProviders.Remove(b);

            foreach (var a in state.AppActions)
                _appActions.Remove(a);

            foreach (var f in state.AppFactories)
                AppRepository.RemoveFactory(f);

            _aggregateProvider.RemoveProvider(pluginId);
            _aggregateProvider.RemoveProvider($"{pluginId}__context");
            _pluginStates.Remove(pluginId);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        // Reload the app repository so removed apps are reflected in the UI
        ReloadApps();

        // Request refresh for any open tabs showing apps from this plugin
        RefreshApps(affectedAppIds);
    }

    internal void ReloadApps()
    {
        AppRepository.Reload(ReservedPaths);
    }

    internal void RefreshApps(IReadOnlySet<string> appIds)
    {
        if (appIds.Count == 0) return;
        AppRepository.RequestAppRefresh(appIds);
    }

    internal HashSet<string> GetPluginAppIds(string pluginId)
    {
        _lock.EnterReadLock();
        try
        {
            if (!_pluginStates.TryGetValue(pluginId, out var state))
                return [];
            return GetAppIdsFromFactories(state.AppFactories);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    private static HashSet<string> GetAppIdsFromFactories(List<Func<AppDescriptor[]>> factories)
    {
        var ids = new HashSet<string>();
        foreach (var factory in factories)
        {
            try
            {
                foreach (var app in factory())
                    ids.Add(app.Id);
            }
            catch
            {
                // Factory may fail if plugin is partially unloaded; ignore
            }
        }
        return ids;
    }

    internal IReadOnlyDictionary<string, PluginState> PluginStates => _pluginStates;

    public void Apply(WebApplication app)
    {
        List<Action<WebApplication>> actions;
        _lock.EnterReadLock();
        try { actions = _appActions.ToList(); }
        finally { _lock.ExitReadLock(); }

        foreach (var action in actions)
            action(app);

        app.MapGet("/ivy/plugins/{pluginId}/assets/{**filePath}", (string pluginId, string filePath) =>
        {
            _lock.EnterReadLock();
            try
            {
                if (!_pluginStates.TryGetValue(pluginId, out var state))
                    return Results.NotFound();

                if (string.IsNullOrEmpty(filePath) || Path.IsPathRooted(filePath))
                    return Results.NotFound();

                var pluginDir = Path.GetFullPath(state.Directory);
                var fullPath = Path.GetFullPath(Path.Join(pluginDir, filePath));

                if (!fullPath.StartsWith(pluginDir + Path.DirectorySeparatorChar))
                    return Results.NotFound();

                if (!File.Exists(fullPath))
                    return Results.NotFound();

                var contentType = Path.GetExtension(fullPath).ToLowerInvariant() switch
                {
                    ".svg" => "image/svg+xml",
                    ".png" => "image/png",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    ".ico" => "image/x-icon",
                    _ => "application/octet-stream"
                };
                return Results.File(fullPath, contentType);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        });
    }
}

internal class PluginContext(Ivy.Server server, WebApplicationBuilder builder) : PluginContextBase(server, builder);
