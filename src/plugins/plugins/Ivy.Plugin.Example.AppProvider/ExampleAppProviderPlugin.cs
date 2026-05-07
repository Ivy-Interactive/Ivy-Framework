using Ivy.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: IvyPlugin(typeof(Ivy.Plugin.Example.AppProvider.ExampleAppProviderPlugin))]

namespace Ivy.Plugin.Example.AppProvider;

/// <summary>
/// Example plugin demonstrating how plugins can add apps to the Ivy host application.
/// This plugin uses the AsIvyContext() extension method to cast IPluginContext to IIvyPluginContext,
/// enabling access to Ivy-specific features like app registration.
/// </summary>
public class ExampleAppProviderPlugin : IIvyPlugin
{
    public PluginManifest Manifest { get; } = new()
    {
        Id = "Ivy.Plugin.Example.AppProvider",
        Name = "Example App Provider",
        ConfigSectionName = "ExampleAppProvider",
        Version = new Version(1, 0, 0),
    };

    public PluginConfigurationSchema? ConfigurationSchema => null;

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    public void Configure(IPluginContext context)
    {
        // Use the AsIvyContext() extension method to access Ivy-specific features
        var ivyContext = context.AsIvyContext();

        // Add an app to the host application
        ivyContext.AddApp(new AppDescriptor
        {
            Id = "example-app",
            Title = "Example App",
            Icon = Icons.Star,
            Description = "Example app added by plugin",
            Type = typeof(ExampleApp),
            Group = ["Examples"],
            IsVisible = true
        });
    }
}
