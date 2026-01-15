using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Renders a raw SVG string or file directly component. Useful for custom graphics or icons not included in the standard set.
/// </summary>
public record Svg : WidgetBase<Svg>
{
    public Svg(string content)
    {
        Content = content;
    }

    internal Svg()
    {
        Width = Size.Auto();
        Height = Size.Auto();
    }

    [Prop] public string Content { get; set; } = string.Empty;
}