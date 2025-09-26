using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A widget for rendering scalable vector graphics (SVG) content.
/// </summary>
/// <remarks>
/// Renders SVG markup directly in the application. Perfect for icons, illustrations, charts, 
/// and other graphics that need to scale without losing quality. Automatically sizes to content.
/// </remarks>
public record Svg : WidgetBase<Svg>
{
    /// <summary>
    /// Initializes a new SVG widget with the specified SVG markup content.
    /// </summary>
    /// <param name="content">The SVG markup content to render.</param>
    public Svg(string content)
    {
        Content = content;
        Width = Size.Auto();
        Height = Size.Auto();
    }

    /// <summary>Gets or sets the SVG markup content to render.</summary>
    /// <value>The SVG markup string that will be rendered as scalable vector graphics.</value>
    [Prop] public string Content { get; set; }
}