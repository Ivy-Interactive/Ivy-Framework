using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public abstract record WidgetBase<T> : AbstractWidget where T : WidgetBase<T>
{
    protected WidgetBase(params object[] children) : base(children)
    {
    }

    [Prop] public Size? Width { get; set; }

    [Prop] public Size? Height { get; set; }

    [Prop] public Density? Density { get; set; }

    [Prop, ScaffoldColumn(false)] public bool Visible { get; set; } = true;

    [Prop, ScaffoldColumn(false)] public string? TestId { get; set; }
}

public static class WidgetBaseExtensions
{
    public static T Width<T>(this T widget, Size? width) where T : WidgetBase<T> => widget with { Width = width };

    public static T Width<T>(this T widget, int units) where T : WidgetBase<T> => widget with { Width = Ivy.Size.Units(units) };

    public static T Width<T>(this T widget, float units) where T : WidgetBase<T> => widget with { Width = Ivy.Size.Fraction(units) };

    public static T Width<T>(this T widget, double units) where T : WidgetBase<T> => widget with { Width = Ivy.Size.Fraction(Convert.ToSingle(units)) };

    public static T Width<T>(this T widget, string percent) where T : WidgetBase<T>
    {
        if (percent.EndsWith("%"))
        {
            if (float.TryParse(percent[..^1], out var value))
                return widget with { Width = Ivy.Size.Fraction(value / 100) };
        }
        throw new ArgumentException("Invalid percentage value.");
    }

    public static T Height<T>(this T widget, Size? height) where T : WidgetBase<T> => widget with { Height = height };

    public static T Height<T>(this T widget, int units) where T : WidgetBase<T> => widget with { Height = Ivy.Size.Units(units) };

    public static T Height<T>(this T widget, float units) where T : WidgetBase<T> => widget with { Height = Ivy.Size.Fraction(units) };

    public static T Height<T>(this T widget, double units) where T : WidgetBase<T> => widget with { Height = Ivy.Size.Fraction(Convert.ToSingle(units)) };

    public static T Height<T>(this T widget, string percent) where T : WidgetBase<T>
    {
        if (!percent.EndsWith("%")) throw new ArgumentException("Invalid percentage value.");
        if (float.TryParse(percent[..^1], out var value))
            return widget with { Height = Ivy.Size.Fraction(value / 100) };
        throw new ArgumentException("Invalid percentage value.");
    }

    public static T Size<T>(this T widget, Size? size) where T : WidgetBase<T> => widget.Width(size).Height(size);

    public static T Size<T>(this T widget, int units) where T : WidgetBase<T> => widget.Width(units).Height(units);

    public static T Size<T>(this T widget, float units) where T : WidgetBase<T> => widget.Width(units).Height(units);

    public static T Size<T>(this T widget, double units) where T : WidgetBase<T> => widget.Width(units).Height(units);

    public static T Size<T>(this T widget, string percent) where T : WidgetBase<T>
    {
        if (!percent.EndsWith("%")) throw new ArgumentException("Invalid percentage value.");
        if (!float.TryParse(percent[..^1], out var value)) throw new ArgumentException("Invalid percentage value.");
        var val = Ivy.Size.Fraction(value / 100);
        return widget with { Width = val, Height = val };
    }

    public static T Grow<T>(this T widget) where T : WidgetBase<T> => widget.Width(Ivy.Size.Grow());

    public static T Density<T>(this T widget, Density density) where T : WidgetBase<T> => widget with { Density = density };

    public static T Small<T>(this T widget) where T : WidgetBase<T> => widget with { Density = Ivy.Density.Small };

    public static T Medium<T>(this T widget) where T : WidgetBase<T> => widget with { Density = Ivy.Density.Medium };

    public static T Large<T>(this T widget) where T : WidgetBase<T> => widget with { Density = Ivy.Density.Large };

    public static T Visible<T>(this T widget, bool visible = true) where T : WidgetBase<T> => widget with { Visible = visible };

    public static T Show<T>(this T widget) where T : WidgetBase<T> => widget with { Visible = true };

    public static T Hide<T>(this T widget) where T : WidgetBase<T> => widget with { Visible = false };

    public static T TestId<T>(this T widget, string testId) where T : WidgetBase<T> => widget with { TestId = testId };

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