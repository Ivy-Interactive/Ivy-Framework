using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record WrapLayout : WidgetBase<WrapLayout>
{
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