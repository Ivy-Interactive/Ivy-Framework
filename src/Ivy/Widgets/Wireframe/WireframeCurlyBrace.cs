// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Which way the brace's centre nub points. This follows the wireframing convention the
/// widget is modelled on: the name is the direction of the nub, not of the span. A
/// Horizontal brace is the familiar "{" -- it spans downwards with the nub pointing left.
/// </summary>
public enum CurlyBraceVariant
{
    Horizontal,
    Vertical
}

public record WireframeCurlyBrace : WidgetBase<WireframeCurlyBrace>
{
    public WireframeCurlyBrace(CurlyBraceVariant variant = CurlyBraceVariant.Horizontal, Colors color = Colors.Black)
    {
        Variant = variant;
        Color = color;
    }

    [Prop] public CurlyBraceVariant Variant { get; set; } = CurlyBraceVariant.Horizontal;

    [Prop] public Colors Color { get; set; } = Colors.Black;

    public static WireframeCurlyBrace operator |(WireframeCurlyBrace widget, object child)
    {
        throw new NotSupportedException("WireframeCurlyBrace does not support children.");
    }
}

public static class WireframeCurlyBraceExtensions
{
    public static WireframeCurlyBrace Variant(this WireframeCurlyBrace brace, CurlyBraceVariant variant)
        => brace with { Variant = variant };

    public static WireframeCurlyBrace Color(this WireframeCurlyBrace brace, Colors color)
        => brace with { Color = color };
}
