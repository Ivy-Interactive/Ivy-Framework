using Ivy.Shared;

namespace Ivy.Helpers;

public static class Layout
{
    public static LayoutView Horizontal(params IEnumerable<object?> elements)
    {
        return (new LayoutView()).Horizontal(elements.Where(e => e != null).Cast<object>().ToArray())
            .Height(Size.Full());
    }

    public static LayoutView Vertical(params IEnumerable<object?> elements)
    {
        return (new LayoutView()).Vertical(elements.Where(e => e != null).Cast<object>().ToArray())
            .Width(Size.Full());
    }

    public static LayoutView Center(params IEnumerable<object?> elements)
    {
        return Horizontal(elements.Where(e => e != null).Cast<object>().ToArray())
            .Height(Size.Screen()).RemoveParentPadding().Align(Align.Center);
    }

    public static LayoutView Wrap(params IEnumerable<object?> elements)
    {
        return (new LayoutView()).Wrap(elements.Where(e => e != null).Cast<object>().ToArray());
    }

    public static GridView Grid(params IEnumerable<object?> elements)
    {
        return new GridView(elements.Where(e => e != null).Cast<object>().ToArray());
    }

    public static TabView Tabs(params IEnumerable<Tab> tabs)
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
}