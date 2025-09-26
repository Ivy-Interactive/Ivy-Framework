using System.Text.Json.Nodes;

namespace Ivy.Core;

/// <summary>
/// Base interface for all widgets in the Ivy framework, providing the foundation for declarative UI components.
/// Widgets are serializable, event-capable objects that represent visual elements and can be composed hierarchically.
/// </summary>
public interface IWidget
{
    /// <summary>Gets or sets the unique identifier for this widget instance within the widget tree.</summary>
    public string? Id { get; set; }

    /// <summary>Gets or sets the optional key for this widget, used for efficient re-rendering and widget identification.</summary>
    public string? Key { get; set; }

    /// <summary>Gets or sets the child widgets contained within this widget for hierarchical composition.</summary>
    public object[] Children { get; set; }

    /// <summary>
    /// Serializes this widget and its children to JSON for transmission to the client-side React frontend.
    /// Processes properties marked with PropAttribute and events marked with EventAttribute.
    /// </summary>
    /// <returns>A JSON representation of the widget including its properties, events, and children.</returns>
    public JsonNode Serialize();

    /// <summary>
    /// Invokes an event handler on this widget with the specified arguments from client-side interactions.
    /// </summary>
    /// <param name="eventName">The name of the event to invoke (e.g., "onClick", "onChange").</param>
    /// <param name="args">The arguments passed from the client-side event.</param>
    /// <returns>True if the event was successfully invoked; false if the event handler was not found.</returns>
    public bool InvokeEvent(string eventName, JsonArray args);

    /// <summary>
    /// Gets an attached property value that was set by a parent widget for layout or behavior purposes.
    /// </summary>
    /// <param name="t">The type of the parent widget that defines the attached property.</param>
    /// <param name="name">The name of the attached property to retrieve.</param>
    /// <returns>The value of the attached property, or null if not set.</returns>
    public object? GetAttachedValue(Type t, string name);

    /// <summary>
    /// Sets an attached property value on this widget, typically used by parent widgets to store layout information.
    /// </summary>
    /// <param name="parentType">The type of the parent widget that defines the attached property.</param>
    /// <param name="name">The name of the attached property to set.</param>
    /// <param name="value">The value to set for the attached property.</param>
    void SetAttachedValue(Type parentType, string name, object? value);
}