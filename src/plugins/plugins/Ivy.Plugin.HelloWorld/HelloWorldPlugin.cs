using Ivy.Plugins;

[assembly: IvyPlugin(typeof(Ivy.Plugin.HelloWorld.HelloWorldPlugin))]

namespace Ivy.Plugin.HelloWorld;

public class HelloWorldPlugin : IIvyPlugin
{
    public PluginManifest Manifest { get; } = new()
    {
        Id = "Ivy.Plugin.HelloWorld",
        Name = "Hello World Plugin",
        ConfigSectionName = "HelloWorld",
        Version = new Version(1, 0, 0),
    };

    public PluginConfigurationSchema? ConfigurationSchema => null;

    public void Configure(IIvyPluginContext context)
    {
        context.RegisterGreeter(new HelloWorldGreeter());
    }
}
