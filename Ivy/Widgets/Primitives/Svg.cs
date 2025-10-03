using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>SVG widget for scalable vector graphics. Default size: auto.</summary>
public record Svg : WidgetBase<Svg>
{
    /// <summary>Initializes SVG widget.</summary>
    /// <param name="content">SVG markup to render.</param>
    public Svg(string content)
    {
        Content = content;
        Width = Size.Auto();
        Height = Size.Auto();
    }

    /// <summary>SVG markup content.</summary>
    [Prop] public string Content { get; set; }
}