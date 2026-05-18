using Ivy.Plugins;

[assembly: IvyPlugin(typeof(Ivy.Plugin.Example.AppProvider.ExampleAppProviderPlugin))]

namespace Ivy.Plugin.Example.AppProvider;

/// <summary>
/// Example plugin demonstrating how plugins can add apps to the Ivy host application.
/// This plugin uses the AsExtendedContext() extension method to cast IIvyPluginContext to IIvyExtendedPluginContext,
/// enabling access to extended features like app registration.
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

    public void Configure(IIvyPluginContext context)
    {
        // Use the AsExtendedContext() extension method to access Ivy-specific features
        var ivyContext = context.AsExtendedContext();

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
