using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Expandable widget creating collapsible content sections with slot-based architecture for header and content areas.</summary>
public record Expandable : WidgetBase<Expandable>
{
    /// <summary>Initializes Expandable with specified header and content.</summary>
    /// <param name="header">Content to display in header section, remains visible at all times.</param>
    /// <param name="content">Content that can be expanded or collapsed, initially hidden.</param>
    public Expandable(object header, object content) : base([new Slot("Header", header), new Slot("Content", content)])
    {

    }

    /// <summary>Whether expandable widget is disabled and cannot be interacted with. Default is false.</summary>
    [Prop] public bool Disabled { get; set; } = false;

    /// <summary>Whether expandable widget is open by default. Default is false.</summary>
    [Prop] public bool Open { get; set; } = false;
}

/// <summary>Extension methods for Expandable widget providing fluent API for configuring behavior and appearance.</summary>
public static class ExpandableExtensions
{
    public static Expandable Disabled(this Expandable widget, bool disabled)
    {
        widget.Disabled = disabled;
        return widget;
    }

    /// <summary>Sets whether expandable widget should be open by default.</summary>
    /// <param name="widget">Expandable widget to configure.</param>
    /// <param name="open">True to open by default; false to close by default.</param>
    /// <returns>Configured expandable widget for method chaining.</returns>
    public static Expandable Open(this Expandable widget, bool open = true)
    {
        widget.Open = open;
        return widget;
    }
}


