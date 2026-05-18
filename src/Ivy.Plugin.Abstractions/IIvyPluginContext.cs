using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Plugins;

public interface IIvyPluginContext
{
    IServiceCollection Services { get; }
    IConfiguration Configuration { get; }
}
