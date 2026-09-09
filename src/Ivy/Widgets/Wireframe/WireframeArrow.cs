// ReSharper disable once CheckNamespace
namespace Ivy;

public enum ArrowHeads
{
    None,
    Start,
    End,
    Both
}

public enum ArrowBend
{
    None,
    Left,
    Right
}

public enum ArrowDirection
{
    Right,
    Left,
    Up,
    Down,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight
}

public record WireframeArrow : WidgetBase<WireframeArrow>
{
    public WireframeArrow(ArrowDirection direction = ArrowDirection.Right, Colors color = Colors.Black)
    {
        Direction = direction;
        Color = color;
    }

    [Prop] public Colors Color { get; set; } = Colors.Black;

    [Prop] public ArrowDirection Direction { get; set; } = ArrowDirection.Right;

    [Prop] public ArrowHeads Heads { get; set; } = ArrowHeads.End;

    [Prop] public ArrowBend Bend { get; set; } = ArrowBend.None;

    [Prop] public bool Dashed { get; set; }

    public static WireframeArrow operator |(WireframeArrow widget, object child)
    {
        throw new NotSupportedException("WireframeArrow does not support children.");
    }
}

public static class WireframeArrowExtensions
{
    public static WireframeArrow Color(this WireframeArrow arrow, Colors color)
        => arrow with { Color = color };

    public static WireframeArrow Direction(this WireframeArrow arrow, ArrowDirection direction)
        => arrow with { Direction = direction };

    public static WireframeArrow Heads(this WireframeArrow arrow, ArrowHeads heads)
        => arrow with { Heads = heads };

    public static WireframeArrow Bend(this WireframeArrow arrow, ArrowBend bend)
        => arrow with { Bend = bend };

    public static WireframeArrow Dashed(this WireframeArrow arrow, bool dashed = true)
        => arrow with { Dashed = dashed };
}
