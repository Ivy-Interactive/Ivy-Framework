using System.Collections;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class Layout
{
    private static object[] Flatten(IEnumerable<object?> elements)
    {
        return elements
            .SelectMany(e => e is string or not IEnumerable ? [e] : ((IEnumerable)e).Cast<object?>())
            .Where(e => e != null)
            .Cast<object>()
            .ToArray();
    }

    public static LayoutView Horizontal(params IEnumerable<object?> elements)
    {
        return (new LayoutView()).Horizontal(Flatten(elements))
            .Height(Size.Full());
    }

    public static LayoutView Vertical(params IEnumerable<object?> elements)
    {
        return (new LayoutView()).Vertical(Flatten(elements))
            .Width(Size.Full());
    }

    public static LayoutView Center(params IEnumerable<object?> elements)
    {
        return Horizontal(Flatten(elements))
            .Height(Size.Screen())
            .RemoveParentPadding().AlignContent(Align.Center);
    }

    public static LayoutView TopCenter(params IEnumerable<object?> elements)
    {
        return Horizontal(Flatten(elements))
            .RemoveParentPadding().AlignContent(Align.TopCenter);
    }

    public static LayoutView Wrap(params IEnumerable<object?> elements)
    {
        return (new LayoutView()).Wrap(Flatten(elements));
    }

    public static GridView Grid(params IEnumerable<object?> elements)
    {
        return new GridView(Flatten(elements));
    }

    public static Spacer Spacer()
    {
        return new Spacer();
    }

    public static TabView Tabs(params Tab[] tabs)
    {
        return new TabView(tabs.ToArray());
    }
}

public static class LayoutExtensions
{
    public static LayoutView WithMargin(this object anything, int margin)
    {
        return Layout.Horizontal(anything).Margin(margin);
    }

    public static LayoutView WithMargin(this object anything, int marginX, int marginY)
    {
        return Layout.Horizontal(anything).Margin(marginX, marginY);
    }

    public static LayoutView WithMargin(this object anything, int left, int top, int right, int bottom)
    {
        return Layout.Horizontal(anything).Margin(left, top, right, bottom);
    }

    public static LayoutView WithLayout(this object anything)
    {
        return Layout.Vertical(anything);
    }

    public static LayoutView Width(this ViewBase view, Size width)
    {
        return view.WithLayout().Width(width);
    }

    public static LayoutView Height(this ViewBase view, Size height)
    {
        return view.WithLayout().Height(height);
    }
}
