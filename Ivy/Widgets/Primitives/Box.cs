using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A versatile container widget that provides comprehensive styling and layout control for content presentation.
/// Offers extensive customization options including borders, padding, margins, colors, and content alignment,
/// making it ideal for creating visually distinct sections, cards, panels, and decorative containers.
/// </summary>
public record Box : WidgetBase<Box>
{
    /// <summary>
    /// Initializes a new box container with the specified content elements.
    /// Creates a styled container that can hold various types of content while providing
    /// comprehensive visual customization through borders, spacing, and alignment properties.
    /// </summary>
    /// <param name="content">The content elements to include in the box. Can be text, widgets, or any renderable objects.</param>
    /// <remarks>
    /// The Box widget is designed for visual content organization and styling:
    /// <list type="bullet">
    /// <item><description><strong>Visual containers:</strong> Create distinct sections with borders and backgrounds</description></item>
    /// <item><description><strong>Content cards:</strong> Display information in styled, bordered containers</description></item>
    /// <item><description><strong>Layout panels:</strong> Organize content with consistent spacing and alignment</description></item>
    /// <item><description><strong>Decorative elements:</strong> Add visual emphasis and structure to interfaces</description></item>
    /// </list>
    /// <para>Default styling provides a primary-colored border with rounded corners and centered content alignment.</para>
    /// </remarks>
    public Box(params IEnumerable<object> content) : base(content.ToArray())
    {
    }

    /// <summary>Gets or sets the color scheme for the box border and styling.</summary>
    /// <value>The color from the Colors enumeration, or null for default styling. Default is Colors.Primary.</value>
    /// <remarks>
    /// The color affects the border color and may influence background and text colors depending on the theme.
    /// Using theme colors ensures consistency with the application's visual design system.
    /// </remarks>
    [Prop] public Colors? Color { get; set; } = Colors.Primary;

    /// <summary>Gets or sets the thickness of the box border on all sides.</summary>
    /// <value>A Thickness object specifying border width. Default is 2 units on all sides.</value>
    /// <remarks>
    /// Border thickness can be uniform (same on all sides) or varied (different for top, right, bottom, left).
    /// Set to zero thickness to create a borderless box while maintaining other styling properties.
    /// </remarks>
    [Prop] public Thickness BorderThickness { get; set; } = new(2);

    /// <summary>Gets or sets the border radius for rounded corners.</summary>
    /// <value>A BorderRadius value specifying corner rounding. Default is BorderRadius.Rounded.</value>
    /// <remarks>
    /// Border radius controls the curvature of box corners, from sharp rectangular corners to fully rounded.
    /// Consistent border radius usage across an application creates visual harmony and modern appearance.
    /// </remarks>
    [Prop] public BorderRadius BorderRadius { get; set; } = BorderRadius.Rounded;

    /// <summary>Gets or sets the style of the box border.</summary>
    /// <value>A BorderStyle value specifying the border appearance. Default is BorderStyle.Solid.</value>
    /// <remarks>
    /// Border style determines the visual appearance of the border line, such as solid, dashed, or dotted.
    /// Different border styles can convey different meanings or visual emphasis in the interface.
    /// </remarks>
    [Prop] public BorderStyle BorderStyle { get; set; } = BorderStyle.Solid;

    /// <summary>Gets or sets the internal spacing between the border and the content.</summary>
    /// <value>A Thickness object specifying internal padding. Default is 2 units on all sides.</value>
    /// <remarks>
    /// Padding creates space between the box border and its content, improving readability and visual appeal.
    /// Adequate padding prevents content from appearing cramped against the container edges.
    /// </remarks>
    [Prop] public Thickness Padding { get; set; } = new(2);

    /// <summary>Gets or sets the external spacing around the box.</summary>
    /// <value>A Thickness object specifying external margin. Default is 0 (no margin).</value>
    /// <remarks>
    /// Margin creates space between the box and surrounding elements, controlling layout spacing.
    /// Margins are useful for creating consistent spacing in layouts and preventing visual crowding.
    /// </remarks>
    [Prop] public Thickness Margin { get; set; } = new(0);

    /// <summary>Gets or sets the alignment of content within the box.</summary>
    /// <value>An Align value specifying content positioning, or null for default alignment. Default is Align.Center.</value>
    /// <remarks>
    /// Content alignment controls how child elements are positioned within the box container.
    /// Center alignment is often used for emphasis, while other alignments serve specific layout needs.
    /// </remarks>
    [Prop] public Align? ContentAlign { get; set; } = Align.Center;
}

/// <summary>
/// Provides extension methods for configuring box widgets with fluent syntax.
/// Enables convenient configuration of box styling properties including borders, spacing, colors, and content
/// through method chaining for improved readability and ease of use in box customization.
/// </summary>
public static class BoxExtensions
{
    /// <summary>
    /// Sets the color scheme for the box border and styling.
    /// </summary>
    /// <param name="box">The box to configure.</param>
    /// <param name="color">The color from the Colors enumeration to apply to the box.</param>
    /// <returns>The box with the specified color scheme.</returns>
    public static Box Color(this Box box, Colors color) => box with { Color = color };

    /// <summary>
    /// Sets a uniform border thickness for all sides of the box.
    /// </summary>
    /// <param name="box">The box to configure.</param>
    /// <param name="thickness">The uniform thickness value to apply to all border sides.</param>
    /// <returns>The box with the specified uniform border thickness.</returns>
    public static Box BorderThickness(this Box box, int thickness) => box with { BorderThickness = new(thickness) };

    /// <summary>
    /// Sets the border thickness using a Thickness object for individual side control.
    /// </summary>
    /// <param name="box">The box to configure.</param>
    /// <param name="thickness">The Thickness object specifying individual border widths for each side.</param>
    /// <returns>The box with the specified border thickness configuration.</returns>
    public static Box BorderThickness(this Box box, Thickness thickness) => box with { BorderThickness = thickness };

    /// <summary>
    /// Sets the border radius for rounded corners.
    /// </summary>
    /// <param name="box">The box to configure.</param>
    /// <param name="radius">The BorderRadius value specifying the corner rounding style.</param>
    /// <returns>The box with the specified border radius.</returns>
    public static Box BorderRadius(this Box box, BorderRadius radius) => box with { BorderRadius = radius };

    /// <summary>
    /// Sets the border style for the box.
    /// </summary>
    /// <param name="box">The box to configure.</param>
    /// <param name="style">The BorderStyle value specifying the border appearance (solid, dashed, etc.).</param>
    /// <returns>The box with the specified border style.</returns>
    public static Box BorderStyle(this Box box, BorderStyle style) => box with { BorderStyle = style };

    /// <summary>
    /// Sets uniform padding for all sides of the box content area.
    /// </summary>
    /// <param name="box">The box to configure.</param>
    /// <param name="padding">The uniform padding value to apply to all sides.</param>
    /// <returns>The box with the specified uniform padding.</returns>
    public static Box Padding(this Box box, int padding) => box with { Padding = new(padding) };

    /// <summary>
    /// Sets the padding using a Thickness object for individual side control.
    /// </summary>
    /// <param name="box">The box to configure.</param>
    /// <param name="padding">The Thickness object specifying individual padding values for each side.</param>
    /// <returns>The box with the specified padding configuration.</returns>
    public static Box Padding(this Box box, Thickness padding) => box with { Padding = padding };

    /// <summary>
    /// Sets uniform margin for all sides of the box.
    /// </summary>
    /// <param name="box">The box to configure.</param>
    /// <param name="margin">The uniform margin value to apply to all sides.</param>
    /// <returns>The box with the specified uniform margin.</returns>
    public static Box Margin(this Box box, int margin) => box with { Margin = new(margin) };

    /// <summary>
    /// Sets the margin using a Thickness object for individual side control.
    /// </summary>
    /// <param name="box">The box to configure.</param>
    /// <param name="margin">The Thickness object specifying individual margin values for each side.</param>
    /// <returns>The box with the specified margin configuration.</returns>
    public static Box Margin(this Box box, Thickness margin) => box with { Margin = margin };

    /// <summary>
    /// Sets the content elements for the box, replacing any existing content.
    /// </summary>
    /// <param name="box">The box to configure.</param>
    /// <param name="content">The content elements to place within the box.</param>
    /// <returns>The box with the specified content elements.</returns>
    public static Box Content(this Box box, params object[] content) => box with { Children = content };

    /// <summary>
    /// Sets the alignment of content within the box.
    /// </summary>
    /// <param name="box">The box to configure.</param>
    /// <param name="align">The Align value specifying how content should be positioned within the box, or null for default alignment.</param>
    /// <returns>The box with the specified content alignment.</returns>
    public static Box ContentAlign(this Box box, Align? align) => box with { ContentAlign = align };
}