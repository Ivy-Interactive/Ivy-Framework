using Ivy.Helpers;
using Ivy.Shared;

namespace Ivy.Views;

/// <summary>
/// Provides static factory methods for creating common layout views including
/// horizontal, vertical, center, wrap, grid, and tab layouts.
/// </summary>
public static class Layout
{
    /// <summary>
    /// Creates a horizontal layout view that arranges elements side by side
    /// in a row.
    /// </summary>
    /// <param name="elements">Variable number of element collections to arrange
    /// horizontally. Null elements are automatically filtered out.</param>
    /// <returns>A LayoutView configured for horizontal arrangement with a full height.</returns>
    public static LayoutView Horizontal(params IEnumerable<object?> elements)
    {
        return (new LayoutView()).Horizontal(elements.Where(e => e != null).Cast<object>().ToArray())
            .Height(Size.Full());
    }

    /// <summary>
    /// Creates a vertical layout view that arranges elements in a column from top to bottom.
    /// </summary>
    /// <param name="elements">Variable number of element collections to arrange
    /// vertically. Null elements are automatically filtered out.</param>
    /// <returns>A LayoutView configured for vertical arrangement with full width.</returns>
    public static LayoutView Vertical(params IEnumerable<object?> elements)
    {
        return (new LayoutView()).Vertical(elements.Where(e => e != null).Cast<object>().ToArray())
            .Width(Size.Full());
    }

    /// <summary>
    /// Creates a centered horizontal layout view that arranges elements
    /// side by side and centers them within their container.
    /// </summary>
    /// <param name="elements">Variable number of element collections to arrange
    /// horizontally and center. Null elements are automatically filtered out.</param>
    /// <returns>A LayoutView configured for centered horizontal arrangement.</returns>
    public static LayoutView Center(params IEnumerable<object?> elements)
    {
        return Horizontal(elements.Where(e => e != null).Cast<object>().ToArray())
            .RemoveParentPadding().Align(Align.Center);
    }

    /// <summary>
    /// Creates a wrap layout view that arranges elements in rows that
    /// automatically wrap to new lines when they reach the container's
    /// edge.
    /// </summary>
    /// <param name="elements">Variable number of element collections to arrange
    /// in a wrapping layout. Null elements are automatically filtered out.</param>
    /// <returns>A LayoutView configured for wrapping arrangement.</returns>
    public static LayoutView Wrap(params IEnumerable<object?> elements)
    {
        return (new LayoutView()).Wrap(elements.Where(e => e != null).Cast<object>().ToArray());
    }

    /// <summary>
    /// Creates a grid layout view that arranges elements in a structured
    /// grid format with rows and columns.
    /// </summary>
    /// <param name="elements">Variable number of element collections to arrange
    /// in a grid layout. Null elements are automatically filtered out.</param>
    /// <returns>A GridView configured for grid arrangement.</returns>
    public static GridView Grid(params IEnumerable<object?> elements)
    {
        return new GridView(elements.Where(e => e != null).Cast<object>().ToArray());
    }

    /// <summary>
    /// Creates a tab view that displays multiple tabs with their associated
    /// content.
    /// </summary>
    /// <param name="tabs">Array of Tab objects that define the tab structure,
    /// content, and behavior for the tabbed interface.</param>
    /// <returns>A TabView configured with the specified tabs.</returns>
    public static TabView Tabs(params Tab[] tabs)
    {
        return new TabView(tabs.ToArray());
    }
}

/// <summary>
/// Provides extension methods for adding margin to any object, converting
/// it into a layout view with specified margin spacing.
/// </summary>
public static class LayoutExtensions
{
    /// <summary>
    /// Adds uniform margin around an object, converting it to a layout view
    /// with the specified margin on all sides.
    /// </summary>
    /// <param name="anything">The object to add margin around.</param>
    /// <param name="margin">The uniform margin size in pixels to apply to all sides.</param>
    /// <returns>A LayoutView containing the original object with uniform margin.</returns>
    public static LayoutView WithMargin(this object anything, int margin)
    {
        return Layout.Horizontal(anything).Margin(margin);
    }

    /// <summary>
    /// Adds different horizontal and vertical margins around an object,
    /// converting it to a layout view with the specified margin spacing.
    /// </summary>
    /// <param name="anything">The object to add margin around.</param>
    /// <param name="marginX">The horizontal margin size in pixels (left and right).</param>
    /// <param name="marginY">The vertical margin size in pixels (top and bottom).</param>
    /// <returns>A LayoutView containing the original object with specified horizontal and vertical margins.</returns>
    public static LayoutView WithMargin(this object anything, int marginX, int marginY)
    {
        return Layout.Horizontal(anything).Margin(marginX, marginY);
    }

    /// <summary>
    /// Adds specific margins around an object on each side, converting it
    /// to a layout view with precise margin control for all four sides.
    /// </summary>
    /// <param name="anything">The object to add margin around.</param>
    /// <param name="left">The left margin size in pixels.</param>
    /// <param name="top">The top margin size in pixels.</param>
    /// <param name="right">The right margin size in pixels.</param>
    /// <param name="bottom">The bottom margin size in pixels.</param>
    /// <returns>A LayoutView containing the original object with specified margins on all sides.</returns>
    public static LayoutView WithMargin(this object anything, int left, int top, int right, int bottom)
    {
        return Layout.Horizontal(anything).Margin(left, top, right, bottom);
    }
}