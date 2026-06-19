using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Core.Plugins;

internal class PluginState
{
    public string PluginId { get; }
    public string Directory { get; }
    public ServiceCollection PluginServices { get; } = new();

    public List<(Func<IEnumerable<MenuItem>, IEnumerable<MenuItem>> Transformer, int Priority)> MenuTransformers { get; } = [];
    public List<Action<WebApplication>> AppActions { get; } = [];
    public List<Func<AppDescriptor[]>> AppFactories { get; } = [];

    public PluginState(string pluginId, string directory)
    {
        PluginId = pluginId;
        Directory = directory;
    }
}
