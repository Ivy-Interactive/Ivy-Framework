using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Plugins;

public interface IIvyPlugin
{
    PluginManifest Manifest { get; }
    PluginConfigurationSchema? ConfigurationSchema { get; }
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    void Configure(IIvyPluginContext context);
}
