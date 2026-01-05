using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record StackLayout : WidgetBase<StackLayout>
{
    public StackLayout(object[] children, Orientation orientation = Orientation.Vertical, int gap = 4, Thickness? padding = null, Thickness? margin = null, Colors? background = null, Align? align = null, bool removeParentPadding = false) : base(children)
    {
        Orientation = orientation;
        Gap = gap;
        Padding = padding;
        Margin = margin;
        Background = background;
        Align = align;
        RemoveParentPadding = removeParentPadding;
    }

    internal StackLayout() { }

    [Prop] public Orientation Orientation { get; set; } = Orientation.Vertical;

    [Prop] public int Gap { get; set; } = 4;

    [Prop] public Thickness? Padding { get; set; }

    [Prop] public Thickness? Margin { get; set; }

    [Prop] public Colors? Background { get; set; }

    [Prop] public Align? Align { get; set; }

    [Prop] public bool RemoveParentPadding { get; set; }
}