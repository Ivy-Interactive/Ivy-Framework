using System.Text.Json.Nodes;

namespace Ivy.Core;

public interface IWidget
{
    public string? Id { get; set; }

    public string? Key { get; set; }

    public object[] Children { get; set; }

    /// <returns>A JSON representation of the widget including its properties, events, and children.</returns>
    public JsonNode Serialize();

    /// <param name="eventName">The name of the event to invoke (e.g., "onClick", "onChange").</param>
    /// <param name="args">The arguments passed from the client-side event.</param>
    /// <returns>True if the event was successfully invoked; false if the event handler was not found.</returns>
    public Task<bool> InvokeEventAsync(string eventName, JsonArray args);

    /// <param name="t">The type of the parent widget that defines the attached property.</param>
    /// <param name="name">The name of the attached property to retrieve.</param>
    /// <returns>The value of the attached property, or null if not set.</returns>
    public object? GetAttachedValue(Type t, string name);

    /// <param name="parentType">The type of the parent widget that defines the attached property.</param>
    /// <param name="name">The name of the attached property to set.</param>
    /// <param name="value">The value to set for the attached property.</param>
    void SetAttachedValue(Type parentType, string name, object? value);
}