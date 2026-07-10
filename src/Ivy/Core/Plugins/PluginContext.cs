using System.Reflection;
using System.Text.RegularExpressions;
using Ivy.Core.Apps;
using Ivy.Core.Plugins.Routing;
using Ivy.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Core.Plugins;

/// <summary>
/// Base class for plugin context implementations. NOT intended for use by plugins directly.
/// This is an internal implementation detail of the plugin hosting infrastructure.
/// Plugins should only depend on the <see cref="IIvyPluginContext"/> (or derived) interfaces.
/// </summary>
public abstract class PluginContextBase : IIvyExtendedPluginContext, IPluginServiceProvider
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

    private readonly AggregatePluginServiceProvider _aggregateProvider = new();
    private readonly Dictionary<string, PluginState> _pluginStates = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<string, string> _slugToPluginId = new();
    private DynamicPluginEndpointDataSource? _endpointDataSource;
    private WebApplication? _app;
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

    public void UseEndpoints(string slug, Action<IEndpointRouteBuilder> configure)
    {
        ValidateSlug(slug);

        var pluginId = _currentPluginId
            ?? throw new InvalidOperationException("UseEndpoints can only be called during plugin configuration.");

        _lock.EnterWriteLock();
        try
        {
            if (_slugToPluginId.TryGetValue(slug, out var existingId) && existingId != pluginId)
                throw new InvalidOperationException($"Endpoint slug '{slug}' is already claimed by plugin '{existingId}'.");
            _slugToPluginId[slug] = pluginId;

            if (_pluginStates.TryGetValue(pluginId, out var state))
                state.EndpointSlug = slug;
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        if (_endpointDataSource is null || _app is null)
            throw new InvalidOperationException("UseEndpoints cannot be called before the application is built.");

        var pluginDir = _pluginStates.TryGetValue(pluginId, out var ps) ? ps.Directory : null;
        var builder = new PluginEndpointRouteBuilder(_app.Services, _app);

        // Use ASP.NET's MapGroup for correct prefix handling across all endpoint types
        var group = builder.MapGroup($"/ivy/plugins/{slug}");

        // Wrap the group to provide plugin directory for MapStaticAssets
        IEndpointRouteBuilder target = pluginDir is not null
            ? new PluginEndpointRouteBuilderWithDirectory(group, pluginDir)
            : group;

        configure(target);

        var endpoints = builder.CollectEndpoints();
        _endpointDataSource.AddEndpoints(slug, endpoints);
    }

    private static readonly Regex SlugPattern = new(@"^[a-z0-9]([a-z0-9\-]*[a-z0-9])?$", RegexOptions.Compiled);

    private static void ValidateSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Endpoint slug cannot be empty.", nameof(slug));
        if (!SlugPattern.IsMatch(slug))
            throw new ArgumentException(
                $"Endpoint slug '{slug}' is invalid. Must be lowercase alphanumeric with optional hyphens, not starting or ending with a hyphen.",
                nameof(slug));
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

            // Remove dynamic endpoints
            if (state.EndpointSlug is not null)
            {
                _endpointDataSource?.RemoveEndpoints(state.EndpointSlug);
                _slugToPluginId.Remove(state.EndpointSlug);
            }

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

    internal void SetEndpointDataSource(DynamicPluginEndpointDataSource dataSource)
    {
        _endpointDataSource = dataSource;
    }

    public void Apply(WebApplication app)
    {
        _app = app;

        // Register the dynamic endpoint data source for plugin routes
        _endpointDataSource ??= new DynamicPluginEndpointDataSource();
        ((IEndpointRouteBuilder)app).DataSources.Add(_endpointDataSource);

        // Plugin icon route — serves files referenced by PluginIcon.File()
        app.MapGet("/ivy/plugin-icons/{pluginId}/{**filePath}", (string pluginId, string filePath) =>
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
