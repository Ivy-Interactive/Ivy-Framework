// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>The anchor a transform pivots and scales around.</summary>
public enum TransformOrigin
{
    Center,
    TopLeft,
    Top,
    TopRight,
    Left,
    Right,
    BottomLeft,
    Bottom,
    BottomRight
}

/// <summary>
/// Renders its children under a transform: rotated, scaled, skewed, flipped or nudged.
/// The transform is visual only -- children keep the layout box they started with, so a
/// rotated child does not push its neighbours around. Place one on a CanvasLayout when
/// the transformed bounds matter.
/// </summary>
public record WireframeTransform : WidgetBase<WireframeTransform>
{
    public WireframeTransform(params IEnumerable<object> content) : base(content.ToArray())
    {
    }

    internal WireframeTransform()
    {
    }

    /// <summary>Clockwise rotation in degrees.</summary>
    [Prop] public double Rotate { get; set; }

    /// <summary>Uniform scale factor. 1 is unchanged.</summary>
    [Prop] public double Scale { get; set; } = 1;

    /// <summary>Horizontal scale, overriding <see cref="Scale"/> on that axis.</summary>
    [Prop] public double? ScaleX { get; set; }

    /// <summary>Vertical scale, overriding <see cref="Scale"/> on that axis.</summary>
    [Prop] public double? ScaleY { get; set; }

    /// <summary>Horizontal skew in degrees.</summary>
    [Prop] public double SkewX { get; set; }

    /// <summary>Vertical skew in degrees.</summary>
    [Prop] public double SkewY { get; set; }

    /// <summary>Horizontal nudge in pixels.</summary>
    [Prop] public double OffsetX { get; set; }

    /// <summary>Vertical nudge in pixels.</summary>
    [Prop] public double OffsetY { get; set; }

    [Prop] public bool FlipHorizontal { get; set; }

    [Prop] public bool FlipVertical { get; set; }

    [Prop] public TransformOrigin Origin { get; set; } = TransformOrigin.Center;

    /// <summary>
    /// Shrinks the widget's own layout box to the transformed bounds, so a scaled or
    /// rotated child takes up the room it visually occupies instead of the room it
    /// started with. Off by default, which is the plain CSS behaviour. When on,
    /// <see cref="Origin"/> no longer affects the result -- the bounds are recentred
    /// either way.
    /// </summary>
    [Prop] public bool Fit { get; set; }

    /// <summary>Fades the children. 1 is opaque.</summary>
    [Prop] public double Opacity { get; set; } = 1;
}

public static class WireframeTransformExtensions
{
    public static WireframeTransform Rotate(this WireframeTransform transform, double degrees)
        => transform with { Rotate = degrees };

    public static WireframeTransform Scale(this WireframeTransform transform, double scale)
        => transform with { Scale = scale };

    public static WireframeTransform Scale(this WireframeTransform transform, double scaleX, double scaleY)
        => transform with { ScaleX = scaleX, ScaleY = scaleY };

    public static WireframeTransform Skew(this WireframeTransform transform, double skewX, double skewY = 0)
        => transform with { SkewX = skewX, SkewY = skewY };

    public static WireframeTransform Offset(this WireframeTransform transform, double offsetX, double offsetY)
        => transform with { OffsetX = offsetX, OffsetY = offsetY };

    public static WireframeTransform FlipHorizontal(this WireframeTransform transform, bool flip = true)
        => transform with { FlipHorizontal = flip };

    public static WireframeTransform FlipVertical(this WireframeTransform transform, bool flip = true)
        => transform with { FlipVertical = flip };

    public static WireframeTransform Origin(this WireframeTransform transform, TransformOrigin origin)
        => transform with { Origin = origin };

    public static WireframeTransform Opacity(this WireframeTransform transform, double opacity)
        => transform with { Opacity = opacity };

    public static WireframeTransform Fit(this WireframeTransform transform, bool fit = true)
        => transform with { Fit = fit };
}
