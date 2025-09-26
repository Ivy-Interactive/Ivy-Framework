using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A widget that hosts and embeds another Ivy application within the current application context.
/// Provides application composition capabilities by allowing one application to render and interact
/// with another application, enabling modular application architectures and micro-frontend patterns.
/// </summary>
public record AppHost : WidgetBase<AppHost>
{
    /// <summary>
    /// Initializes a new application host widget that will embed the specified application.
    /// Creates a hosting environment for running another Ivy application within the current context,
    /// with optional arguments and parent relationship tracking.
    /// </summary>
    /// <param name="appId">The unique identifier of the application to host and render.</param>
    /// <param name="appArgs">Optional arguments to pass to the hosted application for initialization or configuration.</param>
    /// <param name="parentId">Optional identifier of the parent application or context for relationship tracking.</param>
    /// <remarks>
    /// The AppHost enables several architectural patterns:
    /// <list type="bullet">
    /// <item><description><strong>Micro-frontends:</strong> Compose multiple independent applications into a single interface</description></item>
    /// <item><description><strong>Plugin systems:</strong> Dynamically load and render plugin applications</description></item>
    /// <item><description><strong>Modular architecture:</strong> Break large applications into smaller, manageable components</description></item>
    /// <item><description><strong>Application embedding:</strong> Embed specialized applications within broader contexts</description></item>
    /// </list>
    /// </remarks>
    public AppHost(string appId, string? appArgs, string? parentId)
    {
        ParentId = parentId ?? string.Empty;
        AppId = appId;
        AppArgs = appArgs;
    }

    /// <summary>Gets or sets the unique identifier of the application to host and render.</summary>
    /// <value>The application identifier used to locate and instantiate the hosted application.</value>
    /// <remarks>
    /// The AppId must correspond to a registered application in the Ivy application registry.
    /// This identifier is used by the framework to resolve and instantiate the appropriate application class.
    /// </remarks>
    [Prop] public string AppId { get; set; }

    /// <summary>Gets or sets the arguments passed to the hosted application for initialization or configuration.</summary>
    /// <value>The argument string passed to the hosted application, or null if no arguments are provided.</value>
    /// <remarks>
    /// Arguments are typically used to configure the hosted application's initial state, behavior, or data context.
    /// The format and interpretation of arguments depend on the specific hosted application's requirements.
    /// Common uses include passing IDs, configuration parameters, or serialized state information.
    /// </remarks>
    [Prop] public string? AppArgs { get; set; }

    /// <summary>Gets or sets the identifier of the parent application or context for relationship tracking.</summary>
    /// <value>The parent identifier for establishing application hierarchy, or empty string if no parent is specified.</value>
    /// <remarks>
    /// The ParentId enables tracking of application relationships and hierarchies, which can be useful for:
    /// <list type="bullet">
    /// <item><description>Navigation and breadcrumb generation</description></item>
    /// <item><description>Context sharing between parent and child applications</description></item>
    /// <item><description>Security and permission inheritance</description></item>
    /// <item><description>Lifecycle management and cleanup</description></item>
    /// </list>
    /// </remarks>
    [Prop] public string? ParentId { get; set; }
}