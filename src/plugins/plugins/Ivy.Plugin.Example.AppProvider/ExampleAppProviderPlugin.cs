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
public class ExampleAppProviderPlugin : IIvyPlugin<IIvyExtendedPluginContext>
{
    public PluginManifest Manifest { get; } = new()
    {
        Id = "Ivy.Plugin.Example.AppProvider",
        Title = "Example App Provider",
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

    public void Configure(IIvyExtendedPluginContext context)
    {
        var appTitle = context.Config.GetValue("AppTitle") ?? "Example App";
        var appIconName = context.Config.GetValue("AppIcon") ?? "Star";
        var appIcon = Enum.TryParse<Icons>(appIconName, ignoreCase: true, out var parsed) ? parsed : Icons.Star;

        context.AddApp(new AppDescriptor
        {
            Id = "example-app",
            Title = appTitle,
            Icon = appIcon,
            Description = "Example app added by plugin",
            Type = typeof(ExampleApp),
            Group = ["Examples"],
            IsVisible = true
        });
    }
}

/// <summary>
/// Custom configuration view demonstrating a multi-step wizard UI.
/// This looks radically different from the default schema-driven form.
/// </summary>
public class ExampleAppProviderConfigView(IIvyPluginConfig config) : Ivy.ViewBase
{
    public override object? Build()
    {
        var step = UseState(0);
        var appTitle = UseState(config.GetValue("AppTitle") ?? "Example App");
        var appIcon = UseState(config.GetValue("AppIcon") ?? "Star");
        var apiKey = UseState(config.GetValue("ApiKey") ?? "");
        var status = UseState<string?>(null);

        var stepper = new Ivy.Stepper(
            onSelect: e => { step.Set(e.Value); return ValueTask.CompletedTask; },
            selectedIndex: step.Value,
            new Ivy.StepperItem("1", Icon: Ivy.Icons.Palette, Label: "Appearance"),
            new Ivy.StepperItem("2", Icon: Ivy.Icons.Key, Label: "Authentication"),
            new Ivy.StepperItem("3", Icon: Ivy.Icons.Check, Label: "Confirm"));

        object stepContent = step.Value switch
        {
            0 => BuildAppearanceStep(appTitle, appIcon, step),
            1 => BuildAuthStep(apiKey, step),
            _ => BuildConfirmStep(appTitle, appIcon, apiKey, config, status, step)
        };

        return Vertical().Gap(5)
            | stepper
            | new Ivy.Card(content: stepContent)
            | (status.Value is not null
                ? Ivy.Callout.Success(status.Value, title: "Done")
                : null);
    }

    private static object BuildAppearanceStep(Ivy.IState<string> appTitle, Ivy.IState<string> appIcon, Ivy.IState<int> step)
    {
        return Vertical().Gap(4)
            | H2("How should your app look?")
            | Muted("Choose a title and icon that will appear in the sidebar navigation.")
            | new Ivy.Field(appTitle.ToTextInput(placeholder: "My Awesome App"), label: "App Title", required: true)
            | new Ivy.Field(appIcon.ToSelectInput(
                new Ivy.Option<string>[] {
                    new("Star", "Star", icon: Ivy.Icons.Star),
                    new("House", "House", icon: Ivy.Icons.House),
                    new("Settings", "Settings", icon: Ivy.Icons.Settings),
                    new("Database", "Database", icon: Ivy.Icons.Database),
                    new("Globe", "Globe", icon: Ivy.Icons.Globe),
                    new("Zap", "Zap", icon: Ivy.Icons.Zap),
                    new("Heart", "Heart", icon: Ivy.Icons.Heart),
                    new("Rocket", "Rocket", icon: Ivy.Icons.Rocket),
                },
                placeholder: "Pick an icon..."), label: "Icon")
            | Horizontal()
                | new Ivy.Button("Next →", onClick: _ => { step.Set(1); return ValueTask.CompletedTask; });
    }

    private static object BuildAuthStep(Ivy.IState<string> apiKey, Ivy.IState<int> step)
    {
        return Vertical().Gap(4)
            | H2("Connect your backend")
            | Ivy.Callout.Info("Your API key is stored securely and never exposed to the browser.", title: "Security")
            | new Ivy.Field(apiKey.ToTextInput(variant: Ivy.TextInputVariant.Password, placeholder: "sk-..."), label: "API Key", required: true)
            | (Horizontal().Gap(2)
                | new Ivy.Button("← Back", onClick: _ => { step.Set(0); return ValueTask.CompletedTask; }, variant: Ivy.ButtonVariant.Outline)
                | new Ivy.Button("Next →", onClick: _ => { step.Set(2); return ValueTask.CompletedTask; }));
    }

    private static object BuildConfirmStep(
        Ivy.IState<string> appTitle, Ivy.IState<string> appIcon, Ivy.IState<string> apiKey,
        IIvyPluginConfig config, Ivy.IState<string?> status, Ivy.IState<int> step)
    {
        return Vertical().Gap(4)
            | H2("Review & Save")
            | Muted("Confirm your settings below.")
            | (Vertical().Gap(2)
                | (Horizontal().Gap(2) | Bold("Title:") | P(appTitle.Value))
                | (Horizontal().Gap(2) | Bold("Icon:") | P(appIcon.Value))
                | (Horizontal().Gap(2) | Bold("API Key:") | P(string.IsNullOrEmpty(apiKey.Value) ? "not set" : "••••••••")))
            | (Horizontal().Gap(2)
                | new Ivy.Button("← Back", onClick: _ => { step.Set(1); return ValueTask.CompletedTask; }, variant: Ivy.ButtonVariant.Outline)
                | new Ivy.Button("Save & Activate", onClick: _ =>
                {
                    config.SetValue("AppTitle", appTitle.Value);
                    config.SetValue("AppIcon", appIcon.Value);
                    if (!string.IsNullOrEmpty(apiKey.Value))
                        config.SetValue("ApiKey", apiKey.Value);
                    config.Save();
                    status.Set("Plugin configured and activated!");
                    return ValueTask.CompletedTask;
                }, icon: Ivy.Icons.Check));
    }
}
