using Ivy.Plugins;

namespace Ivy;

/// <summary>
/// Extension methods for IPluginContext to simplify working with Ivy-specific plugin features.
/// </summary>
public static class PluginContextExtensions
{
    /// <summary>
    /// Converts an IPluginContext to IIvyPluginContext, enabling access to Ivy-specific features
    /// such as app registration, menu customization, and badge providers.
    /// </summary>
    /// <param name="context">The plugin context to convert.</param>
    /// <returns>The context as IIvyPluginContext.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the context is not an IIvyPluginContext instance,
    /// indicating the plugin is not running in an Ivy host application.
    /// </exception>
    /// <example>
    /// <code>
    /// public void Configure(IPluginContext context)
    /// {
    ///     var ivyContext = context.AsIvyContext();
    ///     ivyContext.AddApp(new AppDescriptor
    ///     {
    ///         Id = "MyApp",
    ///         Name = "My Application",
    ///         Component = typeof(MyAppComponent)
    ///     });
    /// }
    /// </code>
    /// </example>
    public static IIvyPluginContext AsIvyContext(this IPluginContext context)
    {
        return context as IIvyPluginContext
            ?? throw new InvalidOperationException(
                "This plugin requires Ivy framework features. " +
                "Ensure the plugin is loaded in an Ivy host application.");
    }

    /// <summary>
    /// Attempts to convert an IPluginContext to IIvyPluginContext.
    /// Returns null if the context is not an Ivy context.
    /// </summary>
    /// <param name="context">The plugin context to convert.</param>
    /// <returns>The context as IIvyPluginContext, or null if not an Ivy context.</returns>
    /// <example>
    /// <code>
    /// public void Configure(IPluginContext context)
    /// {
    ///     var ivyContext = context.TryGetIvyContext();
    ///     if (ivyContext != null)
    ///     {
    ///         ivyContext.AddApp(new AppDescriptor { ... });
    ///     }
    ///     else
    ///     {
    ///         // Plugin is running in a non-Ivy host
    ///         // Use only IPluginContext features
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IIvyPluginContext? TryGetIvyContext(this IPluginContext context)
    {
        return context as IIvyPluginContext;
    }
}
