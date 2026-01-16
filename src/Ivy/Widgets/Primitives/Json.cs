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

    public Json(string content)
    {
        Content = content;
    }

    internal Json() { }

    [Prop] public string Content { get; set; } = string.Empty;
}