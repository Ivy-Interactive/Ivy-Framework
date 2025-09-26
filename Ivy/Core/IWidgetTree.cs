using System.Text.Json.Nodes;

namespace Ivy.Core;

/// <summary>
/// Defines the contract for a widget tree that manages the hierarchical structure
/// of widgets and views.
/// </summary>
public interface IWidgetTree : IDisposable
{
    /// <summary>
    /// Gets the root view of the widget tree. The root view serves as the
    /// entry point for the entire widget hierarchy and contains all child
    /// widgets and views.
    /// </summary>
    public IView RootView { get; }

    /// <summary>
    /// Retrieves the collection of widgets from the widget tree.
    /// </summary>
    /// <returns>A collection of widgets managed by the widget tree.</returns>
    public IWidget GetWidgets();

    /// <summary>
    /// Asynchronously builds the widget tree by constructing all widgets
    /// and establishing their hierarchical relationships.
    /// </summary>
    /// <returns>A task that represents the asynchronous build operation.</returns>
    public Task BuildAsync();

    /// <summary>
    /// Refreshes a specific view node in the widget tree by its identifier.
    /// </summary>
    /// <param name="nodeId">The identifier of the view node to refresh.</param>
    public void RefreshView(string nodeId);

    /// <summary>
    /// Triggers an event on a specific widget by its identifier.
    /// </summary>
    /// <param name="widgetId">The identifier of the widget to trigger the event on.</param>
    /// <param name="eventName">The name of the event to trigger.</param>
    /// <param name="args">The arguments to pass with the event as a JSON array.</param>
    /// <returns>True if the event was successfully triggered, false otherwise.</returns>
    public bool TriggerEvent(string widgetId, string eventName, JsonArray args);

    /// <summary>
    /// Performs a hot reload of the widget tree, allowing for dynamic updates
    /// and modifications without requiring a complete rebuild.
    /// </summary>
    void HotReload();
}