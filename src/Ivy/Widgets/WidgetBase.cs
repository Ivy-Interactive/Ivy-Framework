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

    [Prop] public Size? Width { get; set; }

    [Prop] public Size? Height { get; set; }

    [Prop] public float? AspectRatio { get; set; }

    [Prop] public Density? Density { get; set; }

    [Prop, ScaffoldColumn(false)] public bool Visible { get; set; } = true;

    [Prop, ScaffoldColumn(false)] public string? TestId { get; set; }
}

public abstract record WidgetBase<T> : WidgetBase where T : WidgetBase<T>
{
    protected WidgetBase(params object[] children) : base(children)
    {
    }
}

public static class WidgetBaseExtensions
{
    public static T Width<T>(this T widget, Size? width) where T : WidgetBase => widget with { Width = width };

    public static T Height<T>(this T widget, Size? height) where T : WidgetBase => widget with { Height = height };

    public static T Size<T>(this T widget, Size? size) where T : WidgetBase => widget.Width(size).Height(size);

    public static T AspectRatio<T>(this T widget, float ratio) where T : WidgetBase => widget with { AspectRatio = ratio };

    public static T Grow<T>(this T widget) where T : WidgetBase => widget.Width(Ivy.Size.Grow());

    public static T Density<T>(this T widget, Density density) where T : WidgetBase => widget with { Density = density };

    public static T Small<T>(this T widget) where T : WidgetBase => widget with { Density = Ivy.Density.Small };

    public static T Medium<T>(this T widget) where T : WidgetBase => widget with { Density = Ivy.Density.Medium };

    public static T Large<T>(this T widget) where T : WidgetBase => widget with { Density = Ivy.Density.Large };

    public static T Visible<T>(this T widget, bool visible = true) where T : WidgetBase => widget with { Visible = visible };

    public static T Show<T>(this T widget) where T : WidgetBase => widget with { Visible = true };

    public static T Hide<T>(this T widget) where T : WidgetBase => widget with { Visible = false };

    public static T TestId<T>(this T widget, string testId) where T : WidgetBase => widget with { TestId = testId };

    internal static void SetDensityViaReflection(object input, Density? density)
    {
        var type = input.GetType();
        var prop = type.GetProperty(
            nameof(Density),
            BindingFlags.Instance | BindingFlags.Public
        );

        if (prop is null) return;
        if (!prop.CanWrite) return;
        if (!prop.PropertyType.IsAssignableFrom(typeof(Density))) return;

        prop.SetValue(input, density);
    }
}