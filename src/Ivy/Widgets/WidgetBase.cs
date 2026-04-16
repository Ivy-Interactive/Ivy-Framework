using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public abstract record WidgetBase : AbstractWidget
{
    protected WidgetBase(params object[] children) : base(children)
    {
    }

    [Prop] public Responsive<Size>? Width { get; internal set; }

    [Prop] public Responsive<Size>? Height { get; internal set; }

    [Prop] public float? AspectRatio { get; set; }

    [Prop] public Responsive<Density?>? Density { get; internal set; }

    [Prop, ScaffoldColumn(false)] public string? TestId { get; set; }

    [Prop] public Responsive<bool?>? ResponsiveVisible { get; set; }
}

public abstract record WidgetBase<T> : WidgetBase where T : WidgetBase<T>
{
    protected WidgetBase(params object[] children) : base(children)
    {
    }
}

public static class WidgetBaseExtensions
{
    public static T Width<T>(this T widget, Size? width) where T : WidgetBase
        => widget with { Width = width is not null ? (Responsive<Size>)width : null };

    public static T Width<T>(this T widget, Responsive<Size> width) where T : WidgetBase
        => widget with { Width = width };

    public static T Height<T>(this T widget, Size? height) where T : WidgetBase
        => widget with { Height = height is not null ? (Responsive<Size>)height : null };

    public static T Height<T>(this T widget, Responsive<Size> height) where T : WidgetBase
        => widget with { Height = height };

    public static T Size<T>(this T widget, Size? size) where T : WidgetBase => widget.Width(size).Height(size);

    public static T AspectRatio<T>(this T widget, float ratio) where T : WidgetBase => widget with { AspectRatio = ratio };

    public static T Grow<T>(this T widget) where T : WidgetBase => widget.Width(Ivy.Size.Grow());

    public static T Density<T>(this T widget, Density density) where T : WidgetBase
        => widget with { Density = (Responsive<Density?>)(Density?)density };

    public static T Small<T>(this T widget) where T : WidgetBase => widget.Density(Ivy.Density.Small);

    public static T Medium<T>(this T widget) where T : WidgetBase => widget.Density(Ivy.Density.Medium);

    public static T Large<T>(this T widget) where T : WidgetBase => widget.Density(Ivy.Density.Large);

    public static T TestId<T>(this T widget, string testId) where T : WidgetBase => widget with { TestId = testId };

    public static T HideOn<T>(this T widget, params Breakpoint[] breakpoints) where T : WidgetBase
    {
        var visible = new Responsive<bool?> { Default = true };
        foreach (var bp in breakpoints)
            visible = visible.And(bp, false);
        return widget with { ResponsiveVisible = visible };
    }

    public static T ShowOn<T>(this T widget, params Breakpoint[] breakpoints) where T : WidgetBase
    {
        var visible = new Responsive<bool?> { Default = false };
        foreach (var bp in breakpoints)
            visible = visible.And(bp, true);
        return widget with { ResponsiveVisible = visible };
    }

    public static T Density<T>(this T widget, Responsive<Density?> density) where T : WidgetBase
        => widget with { Density = density };

    internal static object[] WithSlot<T>(this T widget, string slotName, object? value) where T : WidgetBase
    {
        var others = widget.Children.Where(c => c is not Slot s || s.Name != slotName);
        var result = value != null ? others.Append(new Slot(slotName, value)) : others;
        return result.ToArray();
    }

    internal static void SetDensityViaReflection(object input, Density? density)
    {
        var type = input.GetType();
        var prop = type.GetProperty(
            nameof(Density),
            BindingFlags.Instance | BindingFlags.Public
        );

        if (prop is null) return;
        if (!prop.CanWrite) return;

        if (prop.PropertyType == typeof(Responsive<Density?>))
        {
            Responsive<Density?>? value = density.HasValue ? (Responsive<Density?>)(Density?)density.Value : null;
            prop.SetValue(input, value);
        }
        else if (prop.PropertyType.IsAssignableFrom(typeof(Density)))
        {
            prop.SetValue(input, density);
        }
    }
}
