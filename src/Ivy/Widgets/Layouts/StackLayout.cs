// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Arranges items sequentially in a horizontal or vertical stack.
/// </summary>
internal record StackLayout : WidgetBase<StackLayout>
{
    public StackLayout(object[] children, Responsive<Orientation?>? orientation = null, Responsive<int?>? gap = null, Responsive<Thickness?>? padding = null, Thickness? margin = null, Colors? background = null, Align? alignContent = null, bool removeParentPadding = false, bool wrap = false) : base(children)
    {
        Orientation = orientation ?? (Orientation?)Ivy.Orientation.Vertical;
        RowGap = gap ?? (int?)4;
        ColumnGap = gap ?? (int?)4;
        Padding = padding;
        Margin = margin;
        Background = background;
        AlignContent = alignContent;
        RemoveParentPadding = removeParentPadding;
        Wrap = wrap;
    }

    internal StackLayout() { }

    [Prop] public Responsive<Orientation?>? Orientation { get; internal set; } = (Orientation?)Ivy.Orientation.Vertical;

    [Prop] public Responsive<int?>? RowGap { get; internal set; } = (int?)4;

    [Prop] public Responsive<int?>? ColumnGap { get; internal set; } = (int?)4;

    [Prop] public Responsive<Thickness?>? Padding { get; internal set; }

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