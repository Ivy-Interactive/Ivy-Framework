// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Renders SVG content.
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