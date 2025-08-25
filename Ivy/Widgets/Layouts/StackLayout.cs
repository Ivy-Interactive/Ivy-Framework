using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents a stack layout widget that arranges child elements in either a vertical or horizontal stack with configurable spacing, alignment, and styling options.
/// This widget provides the foundation for creating linear layouts where elements are arranged sequentially in a single direction.
/// The StackLayout widget is the core building block for most layout compositions, offering flexible configuration for orientation, gaps between elements, padding, margins, background colors, and content alignment.
/// It can be used to create simple stacks or as the foundation for more complex layout systems.
/// </summary>
public record StackLayout : WidgetBase<StackLayout>
{
    /// <summary>
    /// Initializes a new instance of the StackLayout class with the specified children and configuration options.
    /// The layout will arrange the children according to the orientation setting, with configurable spacing, alignment, and styling properties.
    /// </summary>
    /// <param name="children">Array of child elements to arrange in the stack layout. These elements will be positioned sequentially according to the orientation and alignment settings.</param>
    /// <param name="orientation">The direction in which child elements are arranged. Vertical creates a top-to-bottom stack, while horizontal creates a left-to-right stack. Default is <see cref="Orientation.Vertical"/>.</param>
    /// <param name="gap">The space between child elements in pixels. This creates uniform spacing between all children in the stack. Default is 4 pixels.</param>
    /// <param name="padding">Optional padding around the entire stack container. When null, no padding is applied. When specified, creates space between the stack content and its outer boundaries.</param>
    /// <param name="margin">Optional margin around the entire stack container. When null, no margin is applied. When specified, creates space between the stack and surrounding elements.</param>
    /// <param name="background">Optional background color for the stack container. When null, no background color is applied. When specified, fills the entire stack area with the selected color.</param>
    /// <param name="align">Optional alignment for child elements within the stack. When null, uses default alignment behavior. When specified, controls how children are positioned relative to the stack's cross-axis (perpendicular to the orientation direction).</param>
    /// <param name="removeParentPadding">Whether to remove any padding inherited from parent containers. When true, the stack will ignore parent padding and extend to the full available space. Default is false (parent padding is respected).</param>
    public StackLayout(object[] children, Orientation orientation = Orientation.Vertical, int gap = 4, Thickness? padding = null, Thickness? margin = null, Colors? background = null, Align? align = null,
        bool removeParentPadding = false) : base(children)
    {
        Orientation = orientation;
        Gap = gap;
        Padding = padding;
        Margin = margin;
        Background = background;
        Align = align;
        RemoveParentPadding = removeParentPadding;
    }

    /// <summary> Gets or sets the direction in which child elements are arranged within the stack. </summary>
    [Prop] public Orientation Orientation { get; set; }

    /// <summary> Gets or sets the space between child elements in pixels. </summary>
    [Prop] public int Gap { get; set; }

    /// <summary> Gets or sets the padding around the entire stack container. </summary>
    [Prop] public Thickness? Padding { get; set; }

    /// <summary> Gets or sets the margin around the entire stack container. </summary>
    [Prop] public Thickness? Margin { get; set; }

    /// <summary> Gets or sets the background color for the entire stack container. </summary>
    [Prop] public Colors? Background { get; set; }

    /// <summary> Gets or sets the alignment for child elements within the stack. </summary>
    [Prop] public Align? Align { get; set; }

    /// <summary> Gets or sets whether to remove any padding inherited from parent containers. </summary>
    [Prop] public bool RemoveParentPadding { get; set; }
}