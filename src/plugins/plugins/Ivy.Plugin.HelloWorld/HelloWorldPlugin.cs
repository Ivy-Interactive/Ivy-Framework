using Ivy.Plugins;

[assembly: IvyPlugin(typeof(Ivy.Plugin.HelloWorld.HelloWorldPlugin))]

namespace Ivy.Plugin.HelloWorld;

public class HelloWorldPlugin : IIvyPlugin<IIvyPluginContext>
{
    public PluginManifest Manifest { get; } = new()
    {
        Id = "Ivy.Plugin.HelloWorld",
        Title = "Hello World Plugin",
        ConfigSectionName = "HelloWorld",
        Version = new Version(1, 0, 0),
    };

    public PluginConfigurationSchema? ConfigurationSchema { get; } = new()
    {
        Fields =
        [
            new() { Key = "Greeting", Type = ConfigFieldType.String, IsRequired = true, DefaultValue = "Hello", Description = "The greeting prefix" },
            new() { Key = "Enthusiastic", Type = ConfigFieldType.Boolean, IsRequired = false, DefaultValue = "false", Description = "Add exclamation marks" }
        ]
    };

    public void Configure(IIvyPluginContext context)
    {
        var greeting = context.Config.GetValue("Greeting") ?? "Hello";
        var enthusiastic = context.Config.GetValue("Enthusiastic") == "true";
        context.RegisterGreeter(new HelloWorldGreeter(greeting, enthusiastic));
    }
}
