using Ivy.Plugins;
using static Ivy.Layout;
using static Ivy.Text;

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

    public PluginConfigurationSchema? ConfigurationSchema { get; } = new()
    {
        Fields =
        [
            new() { Key = "AppTitle", Type = ConfigFieldType.String, IsRequired = true, DefaultValue = "Example App", Description = "Title shown in the sidebar" },
            new() { Key = "AppIcon", Type = ConfigFieldType.String, IsRequired = false, DefaultValue = "Star", Description = "Icon name for the app" },
            new() { Key = "ApiKey", Type = ConfigFieldType.Secret, IsRequired = true, DefaultValue = "sk-example-key", Description = "API key for the backend service" }
        ]
    };

    public object? BuildConfigurationView(IIvyPluginConfig config) =>
        new ExampleAppProviderConfigView(config);

    public void Configure(IIvyPluginContext context)
    {
        var appTitle = context.Config.GetValue("AppTitle") ?? "Example App";

        // Use the AsExtendedContext() extension method to access Ivy-specific features
        var ivyContext = context.AsExtendedContext();

        // Add an app to the host application
        ivyContext.AddApp(new AppDescriptor
        {
            Id = "example-app",
            Title = appTitle,
            Icon = Icons.Star,
            Description = "Example app added by plugin",
            Type = typeof(ExampleApp),
            Group = ["Examples"],
            IsVisible = true
        });
    }
}

/// <summary>
/// Custom configuration view that demonstrates how plugins can provide their own UI
/// instead of relying on the default schema-driven form.
/// </summary>
public class ExampleAppProviderConfigView(IIvyPluginConfig config) : Ivy.ViewBase
{
    public override object? Build()
    {
        var appTitle = UseState(config.GetValue("AppTitle") ?? "");
        var appIcon = UseState(config.GetValue("AppIcon") ?? "Star");
        var apiKey = UseState(config.GetValue("ApiKey") ?? "");
        var status = UseState<string?>(null);

        return Vertical().Gap(4)
            | H3("App Provider Settings")
            | Muted("Configure how the example app appears in your application.")
            | new Ivy.Field(appTitle.ToTextInput(placeholder: "My Custom App"), label: "App Title", required: true)
            | new Ivy.Field(appIcon.ToTextInput(placeholder: "Star, Home, Settings..."), label: "Icon Name", description: "Use any icon name from the Icons enum")
            | new Ivy.Field(apiKey.ToTextInput(variant: Ivy.TextInputVariant.Password, placeholder: "sk-..."), label: "API Key", required: true)
            | new Ivy.Button("Save Configuration", onClick: _ =>
            {
                if (!string.IsNullOrEmpty(appTitle.Value))
                    config.SetValue("AppTitle", appTitle.Value);
                if (!string.IsNullOrEmpty(appIcon.Value))
                    config.SetValue("AppIcon", appIcon.Value);
                if (!string.IsNullOrEmpty(apiKey.Value))
                    config.SetValue("ApiKey", apiKey.Value);
                config.Save();
                status.Set("Configuration saved successfully!");
                return ValueTask.CompletedTask;
            }, icon: Ivy.Icons.Save)
            | (status.Value is not null
                ? new Ivy.Badge(status.Value, Ivy.BadgeVariant.Success)
                : null);
    }
}
