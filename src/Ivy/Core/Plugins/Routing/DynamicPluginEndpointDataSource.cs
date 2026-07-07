using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace Ivy.Core.Plugins.Routing;

internal sealed class DynamicPluginEndpointDataSource : EndpointDataSource
{
    private readonly object _lock = new();
    private readonly Dictionary<string, IReadOnlyList<Endpoint>> _pluginEndpoints = new();
    private CancellationTokenSource _cts = new();
    private IReadOnlyList<Endpoint>? _cachedEndpoints;

    public override IReadOnlyList<Endpoint> Endpoints
    {
        get
        {
            lock (_lock)
            {
                _cachedEndpoints ??= _pluginEndpoints.Values.SelectMany(e => e).ToList();
                return _cachedEndpoints;
            }
        }
    }

    public override IChangeToken GetChangeToken()
    {
        return new CancellationChangeToken(_cts.Token);
    }

    public void AddEndpoints(string slug, IReadOnlyList<Endpoint> endpoints)
    {
        lock (_lock)
        {
            _pluginEndpoints[slug] = endpoints;
            InvalidateCache();
        }
    }

    public void RemoveEndpoints(string slug)
    {
        lock (_lock)
        {
            if (_pluginEndpoints.Remove(slug))
                InvalidateCache();
        }
    }

    public bool HasSlug(string slug)
    {
        lock (_lock)
        {
            return _pluginEndpoints.ContainsKey(slug);
        }
    }

    private void InvalidateCache()
    {
        _cachedEndpoints = null;
        var oldCts = _cts;
        _cts = new CancellationTokenSource();
        oldCts.Cancel();
        oldCts.Dispose();
    }
}
