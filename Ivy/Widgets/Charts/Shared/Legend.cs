// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

/// <summary>
/// Represents a legend configuration for charts.
/// </summary>
public record Legend
{
    /// <summary>
    /// Defines the layout orientation for the legend, determining how legend items are arranged.
    /// </summary>
    public enum Layouts
    {
        /// <summary>Legend items are arranged horizontally, from left to right.</summary>
        Horizontal,
        /// <summary>Legend items are arranged vertically, from top to bottom.</summary>
        Vertical
    }

    /// <summary>
    /// Defines the horizontal alignment of the legend within its container.
    /// </summary>
    public enum Alignments
    {
        /// <summary>Legend is aligned to the left side of its container.</summary>
        Left,
        /// <summary>Legend is centered horizontally within its container.</summary>
        Center,
        /// <summary>Legend is aligned to the right side of its container.</summary>
        Right
    }

    /// <summary>
    /// Defines the vertical alignment of the legend within its container.
    /// </summary>
    public enum VerticalAlignments
    {
        /// <summary>Legend is aligned to the top of its container.</summary>
        Top,
        /// <summary>Legend is centered vertically within its container.</summary>
        Middle,
        /// <summary>Legend is aligned to the bottom of its container.</summary>
        Bottom
    }

    /// <summary>
    /// Defines the visual representation types for legend icons. Icon types determine how data series
    /// are displayed in the legend, affecting both appearance and user understanding.
    /// </summary>
    public enum IconTypes
    {
        /// <summary>Line representation in the legend, typically a short line segment.</summary>
        Line,
        /// <summary>Plain line representation without additional styling.</summary>
        PlainLine,
        /// <summary>Square representation in the legend.</summary>
        Square,
        /// <summary>Rectangle representation in the legend.</summary>
        Rect,
        /// <summary>Circle representation in the legend.</summary>
        Circle,
        /// <summary>Cross (X) representation in the legend.</summary>
        Cross,
        /// <summary>Diamond representation in the legend.</summary>
        Diamond,
        /// <summary>Star representation in the legend.</summary>
        Star,
        /// <summary>Triangle representation in the legend.</summary>
        Triangle,
        /// <summary>Wye (Y) representation in the legend.</summary>
        Wye
    }

    /// <summary>
    /// Initializes a new instance of the Legend class with default values.
    /// </summary>
    public Legend()
    {

    }

    /// <summary>
    /// Gets or sets the layout orientation for the legend, determining how legend items are arranged.
    /// </summary>
    public Layouts Layout { get; set; } = Layouts.Horizontal;

    /// <summary>
    /// Gets or sets the horizontal alignment of the legend within its container.
    /// </summary>
    public Alignments Align { get; set; } = Alignments.Center;

    /// <summary>
    /// Gets or sets the vertical alignment of the legend within its container.
    /// </summary>
    public VerticalAlignments VerticalAlign { get; set; } = VerticalAlignments.Bottom;

    /// <summary>
    /// Gets or sets the size of legend icons in pixels.
    /// </summary>
    public int IconSize { get; set; } = 14;

    /// <summary>
    /// Gets or sets the visual representation type for legend icons.
    /// </summary>
    public IconTypes? IconType { get; set; } = null;
}

/// <summary>
/// Extension methods for the Legend class.
/// </summary>
public static class LegendExtensions
{
    /// <summary>
    /// Sets the layout orientation for the legend, determining how legend items are arranged.
    /// </summary>
    /// <param name="legend">The Legend to configure.</param>
    /// <param name="layout">The layout orientation (Horizontal or Vertical).</param>
    /// <returns>A new Legend instance with the updated layout.</returns>
    public static Legend Layout(this Legend legend, Legend.Layouts layout)
    {
        return legend with { Layout = layout };
    }

    /// <summary>
    /// Sets the legend layout to horizontal, arranging legend items from left to right.
    /// </summary>
    /// <param name="legend">The Legend to configure.</param>
    /// <returns>A new Legend instance with horizontal layout.</returns>
    public static Legend Horizontal(this Legend legend)
    {
        return legend with { Layout = Legend.Layouts.Horizontal };
    }

    /// <summary>
    /// Sets the legend layout to vertical, arranging legend items from top to bottom.
    /// </summary>
    /// <param name="legend">The Legend to configure.</param>
    /// <returns>A new Legend instance with vertical layout.</returns>
    public static Legend Vertical(this Legend legend)
    {
        return legend with { Layout = Legend.Layouts.Vertical };
    }

    /// <summary>
    /// Sets the horizontal alignment of the legend within its container.
    /// </summary>
    /// <param name="legend">The Legend to configure.</param>
    /// <param name="align">The horizontal alignment (Left, Center, or Right).</param>
    /// <returns>A new Legend instance with the updated horizontal alignment.</returns>
    public static Legend Align(this Legend legend, Legend.Alignments align)
    {
        return legend with { Align = align };
    }

    /// <summary>
    /// Sets the legend to align to the left side of its container.
    /// </summary>
    /// <param name="legend">The Legend to configure.</param>
    /// <returns>A new Legend instance aligned to the left.</returns>
    public static Legend Left(this Legend legend)
    {
        return legend with { Align = Legend.Alignments.Left };
    }

    /// <summary>
    /// Sets the legend to align to the center of its container.
    /// </summary>
    /// <param name="legend">The Legend to configure.</param>
    /// <returns>A new Legend instance aligned to the center.</returns>
    public static Legend Center(this Legend legend)
    {
        return legend with { Align = Legend.Alignments.Center };
    }

    /// <summary>
    /// Sets the legend to align to the right side of its container.
    /// </summary>
    /// <param name="legend">The Legend to configure.</param>
    /// <returns>A new Legend instance aligned to the right.</returns>
    public static Legend Right(this Legend legend)
    {
        return legend with { Align = Legend.Alignments.Right };
    }

    /// <summary>
    /// Sets the vertical alignment of the legend within its container.
    /// </summary>
    /// <param name="legend">The Legend to configure.</param>
    /// <param name="verticalAlign">The vertical alignment (Top, Middle, or Bottom).</param>
    /// <returns>A new Legend instance with the updated vertical alignment.</returns>
    public static Legend VerticalAlign(this Legend legend, Legend.VerticalAlignments verticalAlign)
    {
        return legend with { VerticalAlign = verticalAlign };
    }

    /// <summary>
    /// Sets the legend to align to the top of its container.
    /// </summary>
    /// <param name="legend">The Legend to configure.</param>
    /// <returns>A new Legend instance aligned to the top.</returns>
    public static Legend Top(this Legend legend)
    {
        return legend with { VerticalAlign = Legend.VerticalAlignments.Top };
    }

    /// <summary>
    /// Sets the legend to align to the middle of its container.
    /// </summary>
    /// <param name="legend">The Legend to configure.</param>
    /// <returns>A new Legend instance aligned to the middle.</returns>
    public static Legend Middle(this Legend legend)
    {
        return legend with { VerticalAlign = Legend.VerticalAlignments.Middle };
    }

    /// <summary>
    /// Sets the legend to align to the bottom of its container.
    /// </summary>
    /// <param name="legend">The Legend to configure.</param>
    /// <returns>A new Legend instance aligned to the bottom.</returns>
    public static Legend Bottom(this Legend legend)
    {
        return legend with { VerticalAlign = Legend.VerticalAlignments.Bottom };
    }

    /// <summary>
    /// Sets the size of legend icons in pixels.
    /// </summary>
    /// <param name="legend">The Legend to configure.</param>
    /// <param name="iconSize">The size of legend icons in pixels.</param>
    /// <returns>A new Legend instance with the updated icon size.</returns>
    public static Legend IconSize(this Legend legend, int iconSize)
    {
        return legend with { IconSize = iconSize };
    }

    /// <summary>
    /// Sets the visual representation type for legend icons.
    /// </summary>
    /// <param name="legend">The Legend to configure.</param>
    /// <param name="iconType">The icon type to use for legend representations.</param>
    /// <returns>A new Legend instance with the updated icon type.</returns>
    public static Legend IconType(this Legend legend, Legend.IconTypes iconType)
    {
        return legend with { IconType = iconType };
    }
}