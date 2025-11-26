using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Expandable : WidgetBase<Expandable>
{
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

    public static Expandable Open(this Expandable widget, bool open = true)
    {
        widget.Open = open;
        return widget;
    }
}


