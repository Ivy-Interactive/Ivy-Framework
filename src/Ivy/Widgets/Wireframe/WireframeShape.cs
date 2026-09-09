// ReSharper disable once CheckNamespace
namespace Ivy;

public enum WireframeShapeKind
{
    Rectangle,
    Ellipse,
    Triangle,
    Diamond,
    Pentagon,
    Hexagon,
    Octagon,
    Star,
    Cross,
    Parallelogram
}

public record WireframeShape : WidgetBase<WireframeShape>
{
    public WireframeShape(WireframeShapeKind shape = WireframeShapeKind.Rectangle, string? text = null, Colors color = Colors.Black)
    {
        Shape = shape;
        Text = text;
        Color = color;
    }

    [Prop] public WireframeShapeKind Shape { get; set; } = WireframeShapeKind.Rectangle;

    /// <summary>
    /// Draws a regular polygon with this many sides, ignoring <see cref="Shape"/>. Values
    /// below 3 leave <see cref="Shape"/> in charge. This is the escape hatch for the
    /// shapes the enum does not name -- a heptagon is <c>Sides="7"</c>.
    /// </summary>
    [Prop] public int Sides { get; set; }

    [Prop] public string? Text { get; set; }

    [Prop] public Colors Color { get; set; } = Colors.Black;

    /// <summary>Washes the interior with the color. Off by default, so shapes can be laid over content.</summary>
    [Prop] public bool Filled { get; set; }

    public static WireframeShape operator |(WireframeShape widget, object child)
    {
        throw new NotSupportedException("WireframeShape does not support children. Use the Text property.");
    }
}

public static class WireframeShapeExtensions
{
    public static WireframeShape Shape(this WireframeShape shape, WireframeShapeKind kind)
        => shape with { Shape = kind };

    public static WireframeShape Sides(this WireframeShape shape, int sides)
        => shape with { Sides = sides };

    public static WireframeShape Text(this WireframeShape shape, string text)
        => shape with { Text = text };

    public static WireframeShape Color(this WireframeShape shape, Colors color)
        => shape with { Color = color };

    public static WireframeShape Filled(this WireframeShape shape, bool filled = true)
        => shape with { Filled = filled };
}
