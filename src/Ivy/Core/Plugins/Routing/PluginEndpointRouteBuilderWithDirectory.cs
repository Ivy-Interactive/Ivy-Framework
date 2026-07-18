using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Ivy.Core.Plugins.Routing;

internal sealed class PluginEndpointRouteBuilderWithDirectory : IEndpointRouteBuilder
{
    private readonly IEndpointRouteBuilder _inner;

    public string PluginDirectory { get; }

    public PluginEndpointRouteBuilderWithDirectory(IEndpointRouteBuilder inner, string pluginDirectory)
    {
        _inner = inner;
        PluginDirectory = pluginDirectory;
    }

    public IServiceProvider ServiceProvider => _inner.ServiceProvider;
    public ICollection<EndpointDataSource> DataSources => _inner.DataSources;
    public IApplicationBuilder CreateApplicationBuilder() => _inner.CreateApplicationBuilder();
}
