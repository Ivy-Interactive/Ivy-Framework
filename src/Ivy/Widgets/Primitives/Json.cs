using System.Text.Json;
using System.Text.Json.Nodes;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Displays JSON data.
/// </summary>
public record Json : WidgetBase<Json>
{
    public Json(JsonNode json) : this(json.ToString())
    {
    }

    public Json(object obj) : this(JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }))
    {
    }

    public Json(string content)
    {
        Content = content;
    }

    internal Json() { }

    [Prop] public string Content { get; set; } = string.Empty;
    [Prop] public int? Expanded { get; set; }
}