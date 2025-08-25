using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents a floating panel widget that positions its content at a fixed location on the screen,
/// making it ideal for elements that should remain accessible while users scroll through content.
/// </summary>
public record FloatingPanel : WidgetBase<FloatingPanel>
{
    /// <summary>
    /// Initializes a new instance of the FloatingPanel class with the specified child content and alignment.
    /// </summary>
    /// <param name="child">The content to display in the floating panel. Can be null for an empty panel.</param>
    /// <param name="align">The alignment position for the floating panel on the screen. Default is <see cref="Align.BottomRight"/>.</param>
    public FloatingPanel(object? child = null, Align align = Align.BottomRight) : base(child != null ? [child] : [])
    {
        Align = align;
    }

    /// <summary>
    /// Gets or sets the alignment position of the floating panel on the screen.
    /// </summary>
    [Prop] public Align Align { get; set; }

    /// <summary>
    /// Gets or sets the offset adjustment for the floating panel's position.
    /// </summary>
    [Prop] public Thickness? Offset { get; set; }

    /// <summary>
    /// Operator overload that allows adding a single child to the floating panel using the pipe operator.
    /// </summary>
    /// <param name="widget">The floating panel to add content to.</param>
    /// <param name="child">The single child content to add to the panel.</param>
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

/// <summary>
/// Provides extension methods for the FloatingPanel widget that enable a fluent API for configuring panel positioning and alignment.
/// </summary>
public static class FloatingLayerExtensions
{
    /// <summary>
    /// Sets the alignment position of the floating panel on the screen.
    /// </summary>
    /// <param name="floatingButton">The floating panel to configure.</param>
    /// <param name="align">The new alignment position for the panel.</param>
    public static FloatingPanel Align(this FloatingPanel floatingButton, Align align) => floatingButton with { Align = align };

    /// <summary>
    /// Sets the offset adjustment for the floating panel's position using a Thickness object.
    /// </summary>
    /// <param name="floatingButton">The floating panel to configure.</param>
    /// <param name="offset">The Thickness object containing left, top, right, and bottom offset values.</param>
    public static FloatingPanel Offset(this FloatingPanel floatingButton, Thickness? offset) => floatingButton with { Offset = offset };

    /// <summary>
    /// Applies a left offset to the floating panel's position.
    /// </summary>
    /// <param name="floatingButton">The floating panel to configure.</param>
    /// <param name="offset">The number of units to offset the panel to the right from its base position.</param>
    public static FloatingPanel OffsetLeft(this FloatingPanel floatingButton, int offset) => floatingButton with { Offset = new Thickness(offset, 0, 0, 0) };

    /// <summary>
    /// Applies a top offset to the floating panel's position.
    /// </summary>
    /// <param name="floatingButton">The floating panel to configure.</param>
    /// <param name="offset">The number of units to offset the panel down from its base position.</param>
    public static FloatingPanel OffsetTop(this FloatingPanel floatingButton, int offset) => floatingButton with { Offset = new Thickness(0, offset, 0, 0) };

    /// <summary>
    /// Applies a right offset to the floating panel's position.
    /// </summary>
    /// <param name="floatingButton">The floating panel to configure.</param>
    /// <param name="offset">The number of units to offset the panel to the left from its base position.</param>
    public static FloatingPanel OffsetRight(this FloatingPanel floatingButton, int offset) => floatingButton with { Offset = new Thickness(0, 0, offset, 0) };

    /// <summary>
    /// Applies a bottom offset to the floating panel's position.
    /// </summary>
    /// <param name="floatingButton">The floating panel to configure.</param>
    /// <param name="offset">The number of units to offset the panel up from its base position.</param>
    public static FloatingPanel OffsetBottom(this FloatingPanel floatingButton, int offset) => floatingButton with { Offset = new Thickness(0, 0, 0, offset) };
}