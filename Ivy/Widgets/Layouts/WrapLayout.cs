using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary> Represents a wrap layout widget that arranges child elements in rows that automatically wrap to the next line when they reach the end of the available horizontal space. </summary>
public record WrapLayout : WidgetBase<WrapLayout>
{
    /// <summary> Initializes a new instance of the WrapLayout class with the specified children and configuration options. </summary>
    /// <param name="children">Array of child elements to arrange in the wrap layout.</param>
    /// <param name="gap">The space between child elements in pixels. This creates uniform spacing between all children, both horizontally and vertically, providing visual separation and breathing room.</param>
    /// <param name="padding">Optional padding around the entire wrap layout container. When null, no padding is applied. When specified using a <see cref="Thickness"/> object, creates space between the layout content and its outer boundaries.</param>
    /// <param name="margin">Optional margin around the entire wrap layout container. When null, no margin is applied. When specified using a <see cref="Thickness"/> object, creates space between the layout and surrounding elements.</param>
    /// <param name="background">Optional background color for the wrap layout container. When null, no background color is applied. When specified, fills the entire layout area with the selected color for visual distinction.</param>
    /// <param name="alignment">Optional alignment for child elements within the wrap layout. When null, uses default alignment behavior. When specified, controls how children are positioned relative to the available space in each row.</param>
    /// <param name="removeParentPadding">Whether to remove any padding inherited from parent containers. When true, the wrap layout will ignore parent padding and extend to the full available space, allowing it to break out of parent container boundaries.</param>
    public WrapLayout(object[] children, int gap = 4, Thickness? padding = null, Thickness? margin = null,
        Colors? background = null, Align? alignment = null, bool removeParentPadding = false) : base(children)
    {
        Gap = gap;
        Padding = padding;
        Margin = margin;
        Background = background;
        Alignment = alignment;
        RemoveParentPadding = removeParentPadding;
    }

    /// <summary> Gets or sets the space between child elements in pixels. </summary>
    [Prop] public int Gap { get; set; }

    /// <summary> Gets or sets the padding around the entire wrap layout container. </summary>
    [Prop] public Thickness? Padding { get; set; }

    /// <summary> Gets or sets the margin around the entire wrap layout container. </summary>
    [Prop] public Thickness? Margin { get; set; }

    /// <summary> Gets or sets the background color for the entire wrap layout container. </summary>
    [Prop] public Colors? Background { get; set; }

    /// <summary> Gets or sets the alignment for child elements within the wrap layout. </summary>
    [Prop] public Align? Alignment { get; set; }

    /// <summary> Gets or sets whether to remove any padding inherited from parent containers. </summary>
    [Prop] public bool RemoveParentPadding { get; set; }
}