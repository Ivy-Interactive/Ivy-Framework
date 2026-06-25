namespace Ivy.Plugins;

public interface IIvyPlugin
{
    PluginManifest Manifest { get; }
    PluginConfigurationSchema? ConfigurationSchema { get; }
    void Configure(IIvyPluginContext context);
    object? BuildConfigurationView(IIvyPluginConfig configWriter) => null;
    Task ShutdownAsync(PluginShutdownContext context) => Task.CompletedTask;
}

/// <summary>
/// Generic plugin interface that provides compile-time type safety for the context.
/// Plugin authors implement this instead of the non-generic IIvyPlugin to receive
/// the correct context type directly in their Configure method.
/// </summary>
/// <typeparam name="TContext">
/// The context type this plugin requires (e.g. IIvyPluginContext, IIvyExtendedPluginContext,
/// ITendrilPluginContext, ITendrilExtendedPluginContext).
/// </typeparam>
public interface IIvyPlugin<TContext> : IIvyPlugin
    where TContext : IIvyPluginContext
{
    void Configure(TContext context);

    void IIvyPlugin.Configure(IIvyPluginContext context)
    {
        if (context is TContext typed)
            Configure(typed);
        else
            throw new InvalidOperationException(
                $"Plugin '{Manifest.Id}' requires context type '{typeof(TContext).Name}' " +
                $"but the host provided '{context.GetType().Name}'. " +
                $"Ensure this plugin is loaded in a compatible host.");
    }
}
