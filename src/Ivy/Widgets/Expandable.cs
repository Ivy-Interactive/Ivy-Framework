// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A widget that can expand or collapse its content.
/// </summary>
public record Expandable : WidgetBase<Expandable>
{
    public Expandable(object header, object content) : base([new Slot("Header", header), new Slot("Content", content)])
    {

    }

    internal Expandable() { }

    [Prop] public bool Disabled { get; set; } = false;

    [Prop] public bool Open { get; set; } = false;

    [Prop] public Icons? Icon { get; set; }

    [Prop] public bool Ghost { get; set; } = false;
}

public static class ExpandableExtensions
{
    public static Expandable Disabled(this Expandable widget, bool disabled = true)
    {
        widget.Disabled = disabled;
        return widget;
    }

    public static Expandable Open(this Expandable widget, bool open = true)
    {
        widget.Open = open;
        return widget;
    }

    public static Expandable Icon(this Expandable widget, Icons? icon)
    {
        widget.Icon = icon;
        return widget;
    }

    public static Expandable Ghost(this Expandable widget, bool ghost = true)
    {
        widget.Ghost = ghost;
        return widget;
    }
}


