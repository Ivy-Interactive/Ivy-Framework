using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Expandable : WidgetBase<Expandable>
{
    /// <param name="header">Content to display in header section, remains visible at all times.</param>
    /// <param name="content">Content that can be expanded or collapsed, initially hidden.</param>
    public Expandable(object header, object content) : base([new Slot("Header", header), new Slot("Content", content)])
    {

    }

    [Prop] public bool Disabled { get; set; } = false;

    [Prop] public bool Open { get; set; } = false;
}

public static class ExpandableExtensions
{
    public static Expandable Disabled(this Expandable widget, bool disabled)
    {
        widget.Disabled = disabled;
        return widget;
    }

    /// <param name="widget">Expandable widget to configure.</param>
    /// <param name="open">True to open by default; false to close by default.</param>
    /// <returns>Configured expandable widget for method chaining.</returns>
    public static Expandable Open(this Expandable widget, bool open = true)
    {
        widget.Open = open;
        return widget;
    }
}


