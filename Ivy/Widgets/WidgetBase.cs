using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Base class for all widgets.</summary>
public abstract record WidgetBase<T> : AbstractWidget where T : WidgetBase<T>
{
    /// <summary>Constructor for the widget.</summary>
    protected WidgetBase(params object[] children) : base(children)
    {
    }

    /// <summary>The width of the widget.</summary>
    [Prop] public Size? Width { get; set; }

    /// <summary>The height of the widget.</summary>
    [Prop] public Size? Height { get; set; }

    /// <summary>Whether the widget is visible.</summary>
    [Prop] public bool Visible { get; set; } = true;

    /// <summary>The test ID of the widget. Used for finding the widget in the DOM for testing.</summary>
    [Prop] public string? TestId { get; set; }
}

public static class WidgetBaseExtensions
{
    /// <summary>Set the width of the widget.</summary>
    /// <param name="widget">The widget to set the width of.</param>
    /// <param name="width">The width of the widget.</param>
    public static T Width<T>(this T widget, Size? width) where T : WidgetBase<T>
    {
        return widget with { Width = width };
    }

    /// <summary>Set the width of the widget.</summary>
    /// <param name="widget">The widget to set the width of.</param>
    /// <param name="units">The width of the widget in units.</param>
    public static T Width<T>(this T widget, int units) where T : WidgetBase<T>
    {
        return widget with { Width = Shared.Size.Units(units) };
    }

    /// <summary>Set the width of the widget.</summary>
    /// <param name="widget">The widget to set the width of.</param>
    /// <param name="units">The width of the widget in units.</param>
    public static T Width<T>(this T widget, float units) where T : WidgetBase<T>
    {
        return widget with { Width = Shared.Size.Fraction(units) };
    }

    /// <summary>Set the width of the widget.</summary>
    /// <param name="widget">The widget to set the width of.</param>
    /// <param name="units">The width of the widget in units.</param>
    public static T Width<T>(this T widget, double units) where T : WidgetBase<T>
    {
        return widget with { Width = Shared.Size.Fraction(Convert.ToSingle(units)) };
    }

    /// <summary>Set the width of the widget.</summary>
    /// <param name="widget">The widget to set the width of.</param>
    /// <param name="percent">The width of the widget in percent.</param>
    public static T Width<T>(this T widget, string percent) where T : WidgetBase<T>
    {
        if (percent.EndsWith("%"))
        {
            if (float.TryParse(percent.Substring(0, percent.Length - 1), out var value))
                return widget with { Width = Shared.Size.Fraction(value / 100) };
        }
        throw new ArgumentException("Invalid percentage value.");
    }

    /// <summary>Set the height of the widget.</summary>
    /// <param name="widget">The widget to set the height of.</param>
    /// <param name="height">The height of the widget.</param>
    public static T Height<T>(this T widget, Size? height) where T : WidgetBase<T>
    {
        return widget with { Height = height };
    }

    /// <summary>Set the height of the widget.</summary>
    /// <param name="widget">The widget to set the height of.</param>
    /// <param name="units">The height of the widget in units.</param>
    public static T Height<T>(this T widget, int units) where T : WidgetBase<T>
    {
        return widget with { Height = Shared.Size.Units(units) };
    }

    /// <summary>Set the height of the widget.</summary>
    /// <param name="widget">The widget to set the height of.</param>
    /// <param name="units">The height of the widget in units.</param>
    public static T Height<T>(this T widget, float units) where T : WidgetBase<T>
    {
        return widget with { Height = Shared.Size.Fraction(units) };
    }

    /// <summary>Set the height of the widget.</summary>
    /// <param name="widget">The widget to set the height of.</param>
    /// <param name="units">The height of the widget in units.</param>
    public static T Height<T>(this T widget, double units) where T : WidgetBase<T>
    {
        return widget with { Height = Shared.Size.Fraction(Convert.ToSingle(units)) };
    }

    /// <summary>Set the height of the widget.</summary>
    /// <param name="widget">The widget to set the height of.</param>
    /// <param name="percent">The height of the widget in percent.</param>
    public static T Height<T>(this T widget, string percent) where T : WidgetBase<T>
    {
        if (percent.EndsWith("%"))
        {
            if (float.TryParse(percent.Substring(0, percent.Length - 1), out var value))
                return widget with { Height = Shared.Size.Fraction(value / 100) };
        }
        throw new ArgumentException("Invalid percentage value.");
    }

    /// <summary>Set the size of the widget.</summary>
    /// <param name="widget">The widget to set the size of.</param>
    /// <param name="size">The size of the widget.</param>
    public static T Size<T>(this T widget, Size? size) where T : WidgetBase<T>
    {
        return widget.Width(size).Height(size);
    }

    /// <summary>Set the size of the widget.</summary>
    /// <param name="widget">The widget to set the size of.</param>
    /// <param name="units">The size of the widget in units.</param>
    public static T Size<T>(this T widget, int units) where T : WidgetBase<T>
    {
        return widget.Width(units).Height(units);
    }

    /// <summary>Set the size of the widget.</summary>
    /// <param name="widget">The widget to set the size of.</param>
    /// <param name="units">The size of the widget in units.</param>
    public static T Size<T>(this T widget, float units) where T : WidgetBase<T>
    {
        return widget.Width(units).Height(units);
    }

    /// <summary>Set the size of the widget.</summary>
    /// <param name="widget">The widget to set the size of.</param>
    /// <param name="units">The size of the widget in units.</param>
    public static T Size<T>(this T widget, double units) where T : WidgetBase<T>
    {
        return widget.Width(units).Height(units);
    }

    /// <summary>Set the size of the widget.</summary>
    /// <param name="widget">The widget to set the size of.</param>
    /// <param name="percent">The size of the widget in percent.</param>
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

    /// <summary>Set the visibility of the widget.</summary>
    /// <param name="widget">The widget to set the visibility of.</param>
    /// <param name="visible">Whether the widget is visible.</param>
    public static T Visible<T>(this T widget, bool visible = true) where T : WidgetBase<T>
    {
        return widget with { Visible = visible };
    }

    /// <summary>Show the widget.</summary>
    /// <param name="widget">The widget to show.</param>
    public static T Show<T>(this T widget) where T : WidgetBase<T>
    {
        return widget with { Visible = true };
    }

    /// <summary>Hide the widget.</summary>
    /// <param name="widget">The widget to hide.</param>
    public static T Hide<T>(this T widget) where T : WidgetBase<T>
    {
        return widget with { Visible = false };
    }

    /// <summary>Set the test ID of the widget.</summary>
    /// <param name="widget">The widget to set the test ID of.</param>
    /// <param name="testId">The test ID of the widget.</param>
    public static T TestId<T>(this T widget, string testId) where T : WidgetBase<T>
    {
        return widget with { TestId = testId };
    }
}