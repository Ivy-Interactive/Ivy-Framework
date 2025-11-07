// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

/// <summary>
/// Represents a toolbox configuration for charts.
/// </summary>
public record Toolbox
{
    /// <summary>
    /// Defines the orientation for the toolbox.
    /// </summary>
    public enum Orientations
    {
        /// <summary>Toolbox is oriented horizontally.</summary>
        Horizontal,
        /// <summary>Toolbox is oriented vertically.</summary>
        Vertical
    }

    /// <summary>
    /// Defines the horizontal alignment of the toolbox within its container.
    /// </summary>
    public enum Alignments
    {
        /// <summary>Toolbox is aligned to the left side of its container.</summary>
        Left,
        /// <summary>Toolbox is centered horizontally within its container.</summary>
        Center,
        /// <summary>Toolbox is aligned to the right side of its container.</summary>
        Right
    }

    /// <summary>
    /// Defines the vertical alignment of the toolbox within its container.
    /// </summary>
    public enum VerticalAlignments
    {
        /// <summary>Toolbox is aligned to the top of its container.</summary>
        Top,
        /// <summary>Toolbox is centered vertically within its container.</summary>
        Middle,
        /// <summary>Toolbox is aligned to the bottom of its container.</summary>
        Bottom
    }

    /// <summary>
    /// Initializes a new instance of the Toolbox class with default values.
    /// </summary>
    public Toolbox()
    {

    }

    /// <summary>
    /// Gets or sets the orientation of the toolbox.
    /// </summary>
    public Orientations Orientation { get; set; } = Orientations.Horizontal;

    /// <summary>
    /// Gets or sets the horizontal alignment of the toolbox within its container.
    /// </summary>
    public Alignments Align { get; set; } = Alignments.Right;

    /// <summary>
    /// Gets or sets the vertical alignment of the toolbox within its container.
    /// </summary>
    public VerticalAlignments VerticalAlign { get; set; } = VerticalAlignments.Top;

    /// <summary>
    /// Gets or sets whether the save as image feature is enabled.
    /// </summary>
    public bool SaveAsImage { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the restore feature is enabled.
    /// </summary>
    public bool Restore { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the data view feature is enabled.
    /// </summary>
    public bool DataView { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the magic type feature is enabled.
    /// </summary>
    public bool MagicType { get; set; } = true;
}

/// <summary>
/// Extension methods for the Toolbox class.
/// </summary>
public static class ToolboxExtensions
{

    /// <summary>
    /// Sets the orientation of the toolbox.
    /// </summary>
    /// <param name="toolbox">The Toolbox to configure.</param>
    /// <param name="orientation">The orientation (Horizontal or Vertical).</param>
    /// <returns>A new Toolbox instance with the updated orientation.</returns>
    public static Toolbox Orientation(this Toolbox toolbox, Toolbox.Orientations orientation)
    {
        return toolbox with { Orientation = orientation };
    }

    /// <summary>
    /// Sets the toolbox orientation to horizontal.
    /// </summary>
    /// <param name="toolbox">The Toolbox to configure.</param>
    /// <returns>A new Toolbox instance with horizontal orientation.</returns>
    public static Toolbox Horizontal(this Toolbox toolbox)
    {
        return toolbox with { Orientation = Toolbox.Orientations.Horizontal };
    }

    /// <summary>
    /// Sets the toolbox orientation to vertical.
    /// </summary>
    /// <param name="toolbox">The Toolbox to configure.</param>
    /// <returns>A new Toolbox instance with vertical orientation.</returns>
    public static Toolbox Vertical(this Toolbox toolbox)
    {
        return toolbox with { Orientation = Toolbox.Orientations.Vertical };
    }

    /// <summary>
    /// Sets the horizontal alignment of the toolbox within its container.
    /// </summary>
    /// <param name="toolbox">The Toolbox to configure.</param>
    /// <param name="align">The horizontal alignment (Left, Center, or Right).</param>
    /// <returns>A new Toolbox instance with the updated horizontal alignment.</returns>
    public static Toolbox Align(this Toolbox toolbox, Toolbox.Alignments align)
    {
        return toolbox with { Align = align };
    }

    /// <summary>
    /// Sets the toolbox to align to the left side of its container.
    /// </summary>
    /// <param name="toolbox">The Toolbox to configure.</param>
    /// <returns>A new Toolbox instance aligned to the left.</returns>
    public static Toolbox Left(this Toolbox toolbox)
    {
        return toolbox with { Align = Toolbox.Alignments.Left };
    }

    /// <summary>
    /// Sets the toolbox to align to the center of its container.
    /// </summary>
    /// <param name="toolbox">The Toolbox to configure.</param>
    /// <returns>A new Toolbox instance aligned to the center.</returns>
    public static Toolbox Center(this Toolbox toolbox)
    {
        return toolbox with { Align = Toolbox.Alignments.Center };
    }

    /// <summary>
    /// Sets the toolbox to align to the right side of its container.
    /// </summary>
    /// <param name="toolbox">The Toolbox to configure.</param>
    /// <returns>A new Toolbox instance aligned to the right.</returns>
    public static Toolbox Right(this Toolbox toolbox)
    {
        return toolbox with { Align = Toolbox.Alignments.Right };
    }

    /// <summary>
    /// Sets the vertical alignment of the toolbox within its container.
    /// </summary>
    /// <param name="toolbox">The Toolbox to configure.</param>
    /// <param name="verticalAlign">The vertical alignment (Top, Middle, or Bottom).</param>
    /// <returns>A new Toolbox instance with the updated vertical alignment.</returns>
    public static Toolbox VerticalAlign(this Toolbox toolbox, Toolbox.VerticalAlignments verticalAlign)
    {
        return toolbox with { VerticalAlign = verticalAlign };
    }

    /// <summary>
    /// Sets the toolbox to align to the top of its container.
    /// </summary>
    /// <param name="toolbox">The Toolbox to configure.</param>
    /// <returns>A new Toolbox instance aligned to the top.</returns>
    public static Toolbox Top(this Toolbox toolbox)
    {
        return toolbox with { VerticalAlign = Toolbox.VerticalAlignments.Top };
    }

    /// <summary>
    /// Sets the toolbox to align to the middle of its container.
    /// </summary>
    /// <param name="toolbox">The Toolbox to configure.</param>
    /// <returns>A new Toolbox instance aligned to the middle.</returns>
    public static Toolbox Middle(this Toolbox toolbox)
    {
        return toolbox with { VerticalAlign = Toolbox.VerticalAlignments.Middle };
    }

    /// <summary>
    /// Sets the toolbox to align to the bottom of its container.
    /// </summary>
    /// <param name="toolbox">The Toolbox to configure.</param>
    /// <returns>A new Toolbox instance aligned to the bottom.</returns>
    public static Toolbox Bottom(this Toolbox toolbox)
    {
        return toolbox with { VerticalAlign = Toolbox.VerticalAlignments.Bottom };
    }

    /// <summary>
    /// Sets whether the save as image feature is enabled.
    /// </summary>
    /// <param name="toolbox">The Toolbox to configure.</param>
    /// <param name="enabled">True to enable save as image, false to disable.</param>
    /// <returns>A new Toolbox instance with the updated save as image setting.</returns>
    public static Toolbox SaveAsImage(this Toolbox toolbox, bool enabled = true)
    {
        return toolbox with { SaveAsImage = enabled };
    }

    /// <summary>
    /// Sets whether the restore feature is enabled.
    /// </summary>
    /// <param name="toolbox">The Toolbox to configure.</param>
    /// <param name="enabled">True to enable restore, false to disable.</param>
    /// <returns>A new Toolbox instance with the updated restore setting.</returns>
    public static Toolbox Restore(this Toolbox toolbox, bool enabled = true)
    {
        return toolbox with { Restore = enabled };
    }

    /// <summary>
    /// Sets whether the data view feature is enabled.
    /// </summary>
    /// <param name="toolbox">The Toolbox to configure.</param>
    /// <param name="enabled">True to enable data view, false to disable.</param>
    /// <returns>A new Toolbox instance with the updated data view setting.</returns>
    public static Toolbox DataView(this Toolbox toolbox, bool enabled = true)
    {
        return toolbox with { DataView = enabled };
    }

    /// <summary>
    /// Sets whether the magic type feature is enabled.
    /// </summary>
    /// <param name="toolbox">The Toolbox to configure.</param>
    /// <param name="enabled">True to enable magic type, false to disable.</param>
    /// <returns>A new Toolbox instance with the updated magic type setting.</returns>
    public static Toolbox MagicType(this Toolbox toolbox, bool enabled = true)
    {
        return toolbox with { MagicType = enabled };
    }
}
