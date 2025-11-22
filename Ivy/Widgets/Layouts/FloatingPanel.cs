using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Floating panel widget positioning content at fixed screen location, ideal for elements remaining accessible while scrolling.</summary>
public record FloatingPanel : WidgetBase<FloatingPanel>
{
    /// <param name="child">Content to display in floating panel. Can be null for empty panel.</param>
    /// <param name="align">Alignment position for floating panel on screen. Default is <see cref="Align.BottomRight"/>.</param>
    public FloatingPanel(object? child = null, Align align = Align.BottomRight) : base(child != null ? [child] : [])
    {
        Align = align;
    }

    [Prop] public Align Align { get; set; }

    [Prop] public Thickness? Offset { get; set; }

    /// <param name="widget">Floating panel to add content to.</param>
    /// <param name="child">Single child content to add to panel.</param>
    /// <exception cref="NotSupportedException">Thrown when attempting to add multiple children using IEnumerable.</exception>
    public static FloatingPanel operator |(FloatingPanel widget, object child)
    {
        if (child is IEnumerable<object> _)
        {
            throw new NotSupportedException("FloatingLayer does not support multiple children.");
        }

        return widget with { Children = [child] };
    }
}

public static class FloatingLayerExtensions
{
    /// <param name="floatingButton">Floating panel to configure.</param>
    /// <param name="align">New alignment position for panel.</param>
    public static FloatingPanel Align(this FloatingPanel floatingButton, Align align) => floatingButton with { Align = align };

    /// <param name="floatingButton">Floating panel to configure.</param>
    /// <param name="offset">Thickness object containing left, top, right, and bottom offset values.</param>
    public static FloatingPanel Offset(this FloatingPanel floatingButton, Thickness? offset) => floatingButton with { Offset = offset };

    /// <param name="floatingButton">Floating panel to configure.</param>
    /// <param name="offset">Number of units to offset panel to right from base position.</param>
    public static FloatingPanel OffsetLeft(this FloatingPanel floatingButton, int offset) => floatingButton with { Offset = new Thickness(offset, 0, 0, 0) };

    /// <param name="floatingButton">Floating panel to configure.</param>
    /// <param name="offset">Number of units to offset panel down from base position.</param>
    public static FloatingPanel OffsetTop(this FloatingPanel floatingButton, int offset) => floatingButton with { Offset = new Thickness(0, offset, 0, 0) };

    /// <param name="floatingButton">Floating panel to configure.</param>
    /// <param name="offset">Number of units to offset panel to left from base position.</param>
    public static FloatingPanel OffsetRight(this FloatingPanel floatingButton, int offset) => floatingButton with { Offset = new Thickness(0, 0, offset, 0) };

    /// <param name="floatingButton">Floating panel to configure.</param>
    /// <param name="offset">Number of units to offset panel up from base position.</param>
    public static FloatingPanel OffsetBottom(this FloatingPanel floatingButton, int offset) => floatingButton with { Offset = new Thickness(0, 0, 0, offset) };
}