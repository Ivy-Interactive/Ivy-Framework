namespace Ivy.Plugins;

public interface IIvyPlugin
{
    PluginManifest Manifest { get; }
    PluginConfigurationSchema? ConfigurationSchema { get; }
    void Configure(IIvyPluginContext context);
    object? BuildConfigurationView(IIvyPluginConfig configWriter) => null;
    Task ShutdownAsync(PluginShutdownContext context) => Task.CompletedTask;
}
