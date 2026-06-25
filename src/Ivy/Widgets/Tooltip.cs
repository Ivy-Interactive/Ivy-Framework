using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A brief informational message that appears when hovering over an element,
/// or that can be opened programmatically in response to an event (for example,
/// to surface an error next to the widget that produced it).
/// </summary>
[Slot("Trigger")]
[Slot("Content")]
public record Tooltip : WidgetBase<Tooltip>
{
    public Tooltip(object trigger, object content) : base([new Slot("Trigger", trigger), new Slot("Content", content)])
    {
    }

    internal Tooltip() { }

    /// <summary>
    /// When set, controls whether the tooltip is open instead of relying on hover/focus.
    /// Use together with <see cref="OnOpenChange"/> (or a persistent tooltip) to drive the
    /// tooltip from application events such as validation or error events.
    /// </summary>
    [Prop] public bool? Open { get; set; }

    /// <summary>
    /// Renders a small arrow ("bubble") on the tooltip pointing at the widget it is attached to.
    /// Set via <c>.Bubble()</c>.
    /// </summary>
    [Prop] public bool ShowArrow { get; set; }

    /// <summary>
    /// Keeps the tooltip open until it is explicitly dismissed and renders a close (X) button.
    /// Set via <c>.Persist()</c>.
    /// </summary>
    [Prop] public bool Persistent { get; set; }

    /// <summary>
    /// Fired when the tooltip opens.
    /// </summary>
    [Event] public EventHandler<Event<Tooltip>>? OnOpen { get; set; }

    /// <summary>
    /// Fired when the tooltip closes (including when the close button of a persistent tooltip is clicked).
    /// </summary>
    [Event] public EventHandler<Event<Tooltip>>? OnClose { get; set; }

    /// <summary>
    /// Fired whenever the open state changes, carrying the new open value.
    /// </summary>
    [Event] public EventHandler<Event<Tooltip, bool>>? OnOpenChange { get; set; }

    public static Tooltip operator |(Tooltip widget, object child)
    {
        throw new NotSupportedException("Tooltip does not support children.");
    }
}

public static class TooltipExtensions
{
    public static IWidget WithTooltip(this IWidget widget, string toolTip)
    {
        return new Tooltip(widget, toolTip);
    }

    public static IWidget WithTooltip(this IView view, string toolTip)
    {
        return new Tooltip(view, toolTip);
    }

    public static IWidget WithTooltip(this IWidget widget, IWidget content)
    {
        return new Tooltip(widget, content);
    }

    public static IWidget WithTooltip(this IView view, IWidget content)
    {
        return new Tooltip(view, content);
    }

    /// <summary>
    /// Wraps the widget with a tooltip whose content is built from a view.
    /// </summary>
    public static IWidget WithTooltip(this IWidget widget, IView content)
    {
        return new Tooltip(widget, content);
    }

    /// <summary>
    /// Wraps the view with a tooltip whose content is built from a view.
    /// </summary>
    public static IWidget WithTooltip(this IView view, IView content)
    {
        return new Tooltip(view, content);
    }

    /// <summary>
    /// Controls whether the tooltip is open. This lets a tooltip appear in response to an
    /// application event (for example an error) rather than only on hover/focus.
    /// </summary>
    public static Tooltip Open(this Tooltip tooltip, bool open = true)
    {
        return tooltip with { Open = open };
    }

    /// <summary>
    /// Renders a small arrow ("bubble") on the tooltip pointing at the widget it is attached to.
    /// </summary>
    public static Tooltip Bubble(this Tooltip tooltip, bool showArrow = true)
    {
        return tooltip with { ShowArrow = showArrow };
    }

    /// <summary>
    /// Keeps the tooltip open until it is explicitly dismissed and renders a close (X) button.
    /// </summary>
    public static Tooltip Persist(this Tooltip tooltip, bool persistent = true)
    {
        return tooltip with { Persistent = persistent };
    }

    /// <summary>
    /// Registers a handler invoked when the tooltip opens.
    /// </summary>
    public static Tooltip HandleOpen(this Tooltip tooltip, Func<Event<Tooltip>, ValueTask> onOpen)
    {
        return tooltip with { OnOpen = onOpen };
    }

    /// <summary>
    /// Registers a handler invoked when the tooltip opens.
    /// </summary>
    public static Tooltip HandleOpen(this Tooltip tooltip, Action onOpen)
    {
        return tooltip with { OnOpen = new EventHandler<Event<Tooltip>>(_ => { onOpen(); return ValueTask.CompletedTask; }) };
    }

    /// <summary>
    /// Registers a handler invoked when the tooltip closes.
    /// </summary>
    public static Tooltip HandleClose(this Tooltip tooltip, Func<Event<Tooltip>, ValueTask> onClose)
    {
        return tooltip with { OnClose = onClose };
    }

    /// <summary>
    /// Registers a handler invoked when the tooltip closes.
    /// </summary>
    public static Tooltip HandleClose(this Tooltip tooltip, Action onClose)
    {
        return tooltip with { OnClose = new EventHandler<Event<Tooltip>>(_ => { onClose(); return ValueTask.CompletedTask; }) };
    }

    /// <summary>
    /// Registers a handler invoked whenever the open state changes, carrying the new open value.
    /// </summary>
    public static Tooltip HandleOpenChange(this Tooltip tooltip, Func<Event<Tooltip, bool>, ValueTask> onOpenChange)
    {
        return tooltip with { OnOpenChange = onOpenChange };
    }

    /// <summary>
    /// Registers a handler invoked whenever the open state changes, carrying the new open value.
    /// </summary>
    public static Tooltip HandleOpenChange(this Tooltip tooltip, Action<bool> onOpenChange)
    {
        return tooltip with { OnOpenChange = new EventHandler<Event<Tooltip, bool>>(e => { onOpenChange(e.Value); return ValueTask.CompletedTask; }) };
    }
}
