using System.Text.Json.Nodes;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Json : WidgetBase<Json>
{
    public Json(JsonNode json) : this(json.ToString())
    {
    }

    public Json(string content)
    {
        Content = content;
    }

    [Prop] public string Content { get; set; }
}