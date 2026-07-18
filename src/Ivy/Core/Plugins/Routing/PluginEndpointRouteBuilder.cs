using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Ivy.Core.Plugins.Routing;

/// <summary>
/// Collects endpoint data sources registered by a plugin.
/// Prefixing is handled by calling MapGroup on this builder before
/// passing the group to the plugin's configure callback.
/// </summary>
internal sealed class PluginEndpointRouteBuilder : IEndpointRouteBuilder
{
    private readonly IApplicationBuilder _appBuilder;

    public PluginEndpointRouteBuilder(IServiceProvider serviceProvider, IApplicationBuilder appBuilder)
    {
        ServiceProvider = serviceProvider;
        _appBuilder = appBuilder;
    }

    public IServiceProvider ServiceProvider { get; }
    public ICollection<EndpointDataSource> DataSources { get; } = new List<EndpointDataSource>();

    public IApplicationBuilder CreateApplicationBuilder() => _appBuilder.New();

    /// <summary>
    /// Collects all endpoints from registered data sources.
    /// Prefixing is already applied by the RouteGroupBuilder returned from MapGroup.
    /// </summary>
    internal IReadOnlyList<Endpoint> CollectEndpoints()
    {
        return DataSources.SelectMany(ds => ds.Endpoints).ToList();
    }
}
