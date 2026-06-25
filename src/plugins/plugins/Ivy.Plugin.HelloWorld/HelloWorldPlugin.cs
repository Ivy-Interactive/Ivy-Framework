using Ivy.Plugins;

[assembly: IvyPlugin(typeof(Ivy.Plugin.HelloWorld.HelloWorldPlugin))]

namespace Ivy.Plugin.HelloWorld;

public class HelloWorldPlugin : IIvyPlugin<IIvyPluginContext>
{
    public PluginManifest Manifest { get; } = new()
    {
        Id = "Ivy.Plugin.HelloWorld",
        Title = "Hello World Plugin",
        Version = new Version(1, 0, 0),
    };

    public PluginConfigurationSchema? ConfigurationSchema { get; } = new SchemaBuilder()
        .AddString("Greeting", defaultValue: "Hello", description: "The greeting prefix", isRequired: true)
        .AddBoolean("Enthusiastic", defaultValue: false, description: "Add exclamation marks")
        .Build();

    public void Configure(IIvyPluginContext context)
    {
        var greeting = context.Config.GetValue("Greeting") ?? "Hello";
        var enthusiastic = context.Config.GetBool("Enthusiastic") ?? false;
        context.RegisterGreeter(new HelloWorldGreeter(greeting, enthusiastic));
    }
}
