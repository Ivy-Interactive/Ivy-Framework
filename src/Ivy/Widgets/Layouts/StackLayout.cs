// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Arranges items sequentially in a horizontal or vertical stack.
/// </summary>
internal record StackLayout : WidgetBase<StackLayout>
{
    public StackLayout(object[] children, Orientation orientation = Orientation.Vertical, int gap = 4, Thickness? padding = null, Thickness? margin = null, Colors? background = null, Align? alignContent = null, bool removeParentPadding = false, bool wrap = false) : base(children)
    {
        Orientation = orientation;
        RowGap = gap;
        ColumnGap = gap;
        Padding = padding;
        Margin = margin;
        Background = background;
        AlignContent = alignContent;
        RemoveParentPadding = removeParentPadding;
        Wrap = wrap;
    }

    internal StackLayout() { }

    [Prop] public Orientation Orientation { get; set; } = Orientation.Vertical;

    [Prop] public int RowGap { get; set; } = 4;

    [Prop] public int ColumnGap { get; set; } = 4;

    [Prop] public Thickness? Padding { get; set; }

    [Prop] public Thickness? Margin { get; set; }

    [Prop] public Colors? Background { get; set; }

    [Prop] public Align? AlignContent { get; set; }

    [Prop] public bool RemoveParentPadding { get; set; }

    [Prop] public bool Wrap { get; set; }

    [Prop] public Colors? BorderColor { get; set; } = null;
    [Prop] public BorderRadius BorderRadius { get; set; } = BorderRadius.None;
    [Prop] public BorderStyle BorderStyle { get; set; } = BorderStyle.None;
    [Prop] public Thickness BorderThickness { get; set; } = new(0);

    [Prop] public Scroll Scroll { get; set; } = Scroll.None;

    [Prop(attached: nameof(StackLayoutExtensions.AlignSelf))] public Align?[] ChildAlignSelf { get; set; } = null!;
}

public static class StackLayoutExtensions
{
    public static WidgetBase<T> AlignSelf<T>(this WidgetBase<T> child, Align align) where T : WidgetBase<T>
    {
        child.SetAttachedValue(typeof(StackLayout), nameof(AlignSelf), align);
        child.SetAttachedValue(typeof(GridLayout), nameof(AlignSelf), align);
        return child;
    }
}