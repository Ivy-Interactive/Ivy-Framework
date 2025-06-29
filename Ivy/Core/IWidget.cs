using System.Text.Json.Nodes;

namespace Ivy.Core;

public interface IWidget
{
    public string? Id { get; set; }

    public string? Key { get; set; }

    public object[] Children { get; set; }

    public JsonNode Serialize();

    public bool InvokeEvent(string eventName, JsonArray args);

    public object? GetAttachedValue(Type t, string name);

    void SetAttachedValue(Type parentType, string name, object? value);
}