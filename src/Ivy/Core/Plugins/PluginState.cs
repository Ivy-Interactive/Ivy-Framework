using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Core.Plugins;

internal class PluginState
{
    public string PluginId { get; }
    public string Directory { get; }
    public ServiceCollection PluginServices { get; } = new();
    public string? EndpointSlug { get; set; }

    public List<Func<AppDescriptor[]>> AppFactories { get; } = [];

    public PluginState(string pluginId, string directory)
    {
        PluginId = pluginId;
        Directory = directory;
    }
}
