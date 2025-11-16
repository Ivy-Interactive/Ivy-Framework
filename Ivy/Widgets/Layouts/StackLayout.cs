using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record StackLayout : WidgetBase<StackLayout>
{
    /// <param name="orientation">Vertical creates a top-to-bottom stack, while horizontal creates a left-to-right stack. Default is <see cref="Orientation.Vertical"/>.</param>
    /// <param name="gap">Default is 4 pixels.</param>
    /// <param name="padding">When null, no padding is applied.</param>
    /// <param name="margin">When null, no margin is applied.</param>
    /// <param name="background">When null, no background color is applied.</param>
    /// <param name="align">When specified, controls how children are positioned relative to the stack's cross-axis (perpendicular to the orientation direction).</param>
    /// <param name="removeParentPadding">When true, the stack will ignore parent padding and extend to the full available space. Default is false (parent padding is respected).</param>
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

    [Prop] public Orientation Orientation { get; set; }

    [Prop] public int Gap { get; set; }

    [Prop] public Thickness? Padding { get; set; }

    [Prop] public Thickness? Margin { get; set; }

    [Prop] public Colors? Background { get; set; }

    [Prop] public Align? Align { get; set; }

    [Prop] public bool RemoveParentPadding { get; set; }
}