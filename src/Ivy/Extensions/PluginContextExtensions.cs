using Ivy.Plugins;

namespace Ivy;

/// <summary>
/// Extension methods for IIvyPluginContext to simplify working with Ivy-specific plugin features.
/// </summary>
public static class PluginContextExtensions
{
    /// <summary>
    /// Converts an IIvyPluginContext to IIvyExtendedPluginContext, enabling access to extended features
    /// such as app registration, menu customization, and badge providers.
    /// </summary>
    /// <param name="context">The plugin context to convert.</param>
    /// <returns>The context as IIvyExtendedPluginContext.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the context is not an IIvyExtendedPluginContext instance,
    /// indicating the plugin is not running in an Ivy host application.
    /// </exception>
    /// <example>
    /// <code>
    /// public void Configure(IIvyPluginContext context)
    /// {
    ///     var ivyContext = context.AsExtendedContext();
    ///     ivyContext.AddApp(new AppDescriptor
    ///     {
    ///         Id = "MyApp",
    ///         Name = "My Application",
    ///         Component = typeof(MyAppComponent)
    ///     });
    /// }
    /// </code>
    /// </example>
    public static IIvyExtendedPluginContext AsExtendedContext(this IIvyPluginContext context)
    {
        return context as IIvyExtendedPluginContext
            ?? throw new InvalidOperationException(
                "This plugin requires Ivy framework features. " +
                "Ensure the plugin is loaded in an Ivy host application.");
    }

    /// <summary>
    /// Attempts to convert an IIvyPluginContext to IIvyExtendedPluginContext.
    /// Returns null if the context is not an extended Ivy context.
    /// </summary>
    /// <param name="context">The plugin context to convert.</param>
    /// <returns>The context as IIvyExtendedPluginContext, or null if not an extended context.</returns>
    /// <example>
    /// <code>
    /// public void Configure(IIvyPluginContext context)
    /// {
    ///     var ivyContext = context.TryGetExtendedContext();
    ///     if (ivyContext != null)
    ///     {
    ///         ivyContext.AddApp(new AppDescriptor { ... });
    ///     }
    ///     else
    ///     {
    ///         // Plugin is running in a non-Ivy host
    ///         // Use only IIvyPluginContext features
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IIvyExtendedPluginContext? TryGetExtendedContext(this IIvyPluginContext context)
    {
        return context as IIvyExtendedPluginContext;
    }
}
