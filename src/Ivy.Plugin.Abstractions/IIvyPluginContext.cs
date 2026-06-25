using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Plugins;

public interface IIvyPluginContext
{
    IServiceCollection Services { get; }
    IIvyPluginConfig Config { get; }
}
