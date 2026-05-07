using System.Reflection;
using Ivy.Core.Apps;
using Ivy.Plugins;
using Ivy.Plugins.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Core.Plugins;

public abstract class PluginContextBase : IIvyPluginContext, IPluginServiceProvider
{
    private readonly List<Func<IEnumerable<MenuItem>, IEnumerable<MenuItem>>> _menuTransformers = [];
    private readonly List<Func<IEnumerable<MenuItem>, INavigator, IEnumerable<MenuItem>>> _footerMenuTransformers = [];
    private readonly List<(string Tag, Func<IServiceProvider, int> CountProvider)> _badgeProviders = [];
    private readonly List<Action<WebApplication>> _appActions = [];
    private readonly AggregatePluginServiceProvider _aggregateProvider = new();
    private readonly Dictionary<string, PluginState> _pluginStates = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private string? _currentPluginId;

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

    public abstract IConfiguration Configuration { get; }

    protected abstract AppRepository AppRepository { get; }
    protected abstract IReadOnlySet<string> ReservedPaths { get; }
    protected abstract WebApplicationBuilder Builder { get; }

    public IReadOnlyList<Func<IEnumerable<MenuItem>, IEnumerable<MenuItem>>> MenuTransformers => _menuTransformers;
    public IReadOnlyList<Func<IEnumerable<MenuItem>, INavigator, IEnumerable<MenuItem>>> FooterMenuTransformers => _footerMenuTransformers;
    public IReadOnlyList<(string Tag, Func<IServiceProvider, int> CountProvider)> BadgeProviders => _badgeProviders;

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

    public void AddMenuItems(Func<IEnumerable<MenuItem>, IEnumerable<MenuItem>> transformer)
    {
        _menuTransformers.Add(transformer);

        if (_currentPluginId is not null && _pluginStates.TryGetValue(_currentPluginId, out var state))
            state.MenuTransformers.Add(transformer);
    }

    public void AddFooterMenuItems(Func<IEnumerable<MenuItem>, INavigator, IEnumerable<MenuItem>> transformer)
    {
        _footerMenuTransformers.Add(transformer);

        if (_currentPluginId is not null && _pluginStates.TryGetValue(_currentPluginId, out var state))
            state.FooterMenuTransformers.Add(transformer);
    }

    public void AddBadgeProvider(string menuTag, Func<IServiceProvider, int> countProvider)
    {
        _badgeProviders.Add((menuTag, countProvider));

        if (_currentPluginId is not null && _pluginStates.TryGetValue(_currentPluginId, out var state))
            state.BadgeProviders.Add((menuTag, countProvider));
    }

    public void RegisterMessagingChannel(IMessagingChannel channel)
    {
        Services.AddSingleton<IMessagingChannel>(channel);
    }

    public void UseWebApplication(Action<WebApplication> configure)
    {
        _appActions.Add(configure);

        if (_currentPluginId is not null && _pluginStates.TryGetValue(_currentPluginId, out var state))
            state.AppActions.Add(configure);
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

    internal IServiceProvider? GetPluginServiceProvider(string pluginId)
    {
        // The aggregate provider manages individual providers, just return it
        return null; // individual providers are managed by the aggregate
    }

    public T? GetService<T>() where T : class
    {
        return _aggregateProvider.GetService<T>();
    }

    public IEnumerable<T> GetServices<T>() where T : class
    {
        return _aggregateProvider.GetServices<T>();
    }

    internal void RemovePluginContributions(string pluginId)
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

            foreach (var t in state.FooterMenuTransformers)
                _footerMenuTransformers.Remove(t);

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
        foreach (var action in _appActions)
            action(app);
    }
}

internal class PluginContext(Ivy.Server server, WebApplicationBuilder builder) : PluginContextBase
{
    public override IConfiguration Configuration => server.Configuration;
    protected override AppRepository AppRepository => server.AppRepository;
    protected override IReadOnlySet<string> ReservedPaths => server.ReservedPaths;
    protected override WebApplicationBuilder Builder => builder;
}
