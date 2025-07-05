using Ivy.Core;
using Ivy.Shared;


// ReSharper disable once CheckNamespace
namespace Ivy;

public abstract record WidgetBase<T> : AbstractWidget where T : WidgetBase<T>
{
    protected WidgetBase(params object[] children) : base(children)
    {
    }

    [Prop] public Size? Width { get; set; }
    [Prop] public Size? Height { get; set; }
    [Prop] public bool Visible { get; set; } = true;
    [Prop] public string? TestId { get; set; }
}

public static class WidgetBaseExtensions
{
    public static T Width<T>(this T widget, Size? width) where T : WidgetBase<T>
    {
        return widget with { Width = width };
    }

    public static T Width<T>(this T widget, int units) where T : WidgetBase<T>
    {
        return widget with { Width = Shared.Size.Units(units) };
    }

    public static T Width<T>(this T widget, float units) where T : WidgetBase<T>
    {
        return widget with { Width = Shared.Size.Fraction(units) };
    }

    public static T Width<T>(this T widget, double units) where T : WidgetBase<T>
    {
        return widget with { Width = Shared.Size.Fraction(Convert.ToSingle(units)) };
    }

    public static T Width<T>(this T widget, string percent) where T : WidgetBase<T>
    {
        if (percent.EndsWith("%"))
        {
            if (float.TryParse(percent.Substring(0, percent.Length - 1), out var value))
                return widget with { Width = Shared.Size.Fraction(value / 100) };
        }
        throw new ArgumentException("Invalid percentage value.");
    }

    public static T Height<T>(this T widget, Size? height) where T : WidgetBase<T>
    {
        widget.Height = height;
        return widget;
    }

    public static T Height<T>(this T widget, int units) where T : WidgetBase<T>
    {
        return widget with { Height = Shared.Size.Units(units) };
    }

    public static T Height<T>(this T widget, float units) where T : WidgetBase<T>
    {
        return widget with { Height = Shared.Size.Fraction(units) };
    }

    public static T Height<T>(this T widget, double units) where T : WidgetBase<T>
    {
        return widget with { Height = Shared.Size.Fraction(Convert.ToSingle(units)) };
    }

    public static T Height<T>(this T widget, string percent) where T : WidgetBase<T>
    {
        if (percent.EndsWith("%"))
        {
            if (float.TryParse(percent.Substring(0, percent.Length - 1), out var value))
                return widget with { Height = Shared.Size.Fraction(value / 100) };
        }
        throw new ArgumentException("Invalid percentage value.");
    }

    public static T Size<T>(this T widget, Size? size) where T : WidgetBase<T>
    {
        return widget.Width(size).Height(size);
    }

    public static T Size<T>(this T widget, int units) where T : WidgetBase<T>
    {
        return widget.Width(units).Height(units);
    }

    public static T Size<T>(this T widget, float units) where T : WidgetBase<T>
    {
        return widget.Width(units).Height(units);
    }

    public static T Size<T>(this T widget, double units) where T : WidgetBase<T>
    {
        return widget.Width(units).Height(units);
    }

    public static T Size<T>(this T widget, string percent) where T : WidgetBase<T>
    {
        if (percent.EndsWith("%"))
        {
            if (float.TryParse(percent.Substring(0, percent.Length - 1), out var value))
            {
                var val = Shared.Size.Fraction(value / 100);
                return widget with { Width = val, Height = val };
            }
        }
        throw new ArgumentException("Invalid percentage value.");
    }

    public static T Visible<T>(this T widget, bool visible = true) where T : WidgetBase<T>
    {
        return widget with { Visible = visible };
    }

    public static T Show<T>(this T widget) where T : WidgetBase<T>
    {
        return widget with { Visible = true };
    }

    public static T Hide<T>(this T widget) where T : WidgetBase<T>
    {
        return widget with { Visible = false };
    }

    public static T TestId<T>(this T widget, string testId) where T : WidgetBase<T>
    {
        return widget with { TestId = testId };
    }
}