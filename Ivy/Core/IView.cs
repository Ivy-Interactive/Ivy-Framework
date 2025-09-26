using Ivy.Core.Hooks;

namespace Ivy.Core;

/// <summary>
/// Base interface for all views in the Ivy framework, providing the foundation for component-based UI development.
/// Views are C# classes that implement reactive, server-side rendering with React-like patterns and lifecycle management.
/// </summary>
public interface IView : IDisposable
{
    /// <summary>Gets the view context that provides access to hooks like UseState, UseEffect, and services.</summary>
    public IViewContext Context { get; }

    /// <summary>Gets or sets the unique identifier for this view instance within the widget tree.</summary>
    public string? Id { get; set; }

    /// <summary>Gets or sets the optional key for this view, used for efficient re-rendering and component identification.</summary>
    public string? Key { get; set; }

    /// <summary>
    /// Builds and returns the visual content for this view as widgets, other views, or data objects.
    /// This method is called during each render cycle and should return the current UI representation.
    /// </summary>
    /// <returns>The visual content as widgets, views, or other displayable objects.</returns>
    public object? Build();

    /// <summary>
    /// Called before the Build method to initialize the view context and prepare for rendering.
    /// </summary>
    /// <param name="context">The view context providing access to hooks and services.</param>
    void BeforeBuild(IViewContext context);

    /// <summary>
    /// Called after the Build method to clean up the view context and finalize the render cycle.
    /// </summary>
    void AfterBuild();

    /// <summary>Gets whether this view is stateless and should not have access to hooks or context.</summary>
    public bool IsStateless { get; }

    /// <summary>
    /// Tracks disposable resources to be automatically cleaned up when the view is disposed.
    /// </summary>
    /// <param name="disposable">The disposable resources to track for cleanup.</param>
    public void TrackDisposable(params IDisposable[] disposable);
}