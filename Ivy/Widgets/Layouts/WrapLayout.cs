using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record WrapLayout : WidgetBase<WrapLayout>
{
    /// <param name="gap">This creates uniform spacing between all children, both horizontally and vertically.</param>
    /// <param name="padding">When null, no padding is applied.</param>
    /// <param name="margin">When null, no margin is applied.</param>
    /// <param name="background">When null, no background color is applied.</param>
    /// <param name="alignment">When specified, controls how children are positioned relative to the available space in each row.</param>
    /// <param name="removeParentPadding">When true, the wrap layout will ignore parent padding and extend to the full available space, allowing it to break out of parent container boundaries.</param>
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

    [Prop] public int Gap { get; set; }

    [Prop] public Thickness? Padding { get; set; }

    [Prop] public Thickness? Margin { get; set; }

    [Prop] public Colors? Background { get; set; }

    [Prop] public Align? Alignment { get; set; }

    [Prop] public bool RemoveParentPadding { get; set; }
}