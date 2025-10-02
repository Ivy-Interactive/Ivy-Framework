using System.Text.Json.Nodes;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>JSON widget with syntax highlighting, indentation, and expand/collapse functionality.</summary>
public record Json : WidgetBase<Json>
{
    /// <summary>Initializes JSON widget from JsonNode.</summary>
    /// <param name="json">JsonNode to display.</param>
    public Json(JsonNode json) : this(json.ToString())
    {
    }

    /// <summary>Initializes JSON widget from string.</summary>
    /// <param name="content">JSON content string.</param>
    public Json(string content)
    {
        Content = content;
    }

    /// <summary>JSON content to display.</summary>
    [Prop] public string Content { get; set; }
}