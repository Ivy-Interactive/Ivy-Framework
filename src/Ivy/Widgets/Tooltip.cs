using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum TooltipVariant
{
    Default,
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// A brief informational message that appears when hovering over an element.
/// </summary>
[Slot("Trigger")]
[Slot("Content")]
public record Tooltip : WidgetBase<Tooltip>
{
    public Tooltip(object trigger, object content) : base([new Slot("Trigger", trigger), new Slot("Content", content)])
    {
    }

    internal Tooltip() { }

    [Prop] public bool? Open { get; set; }

    [Prop] public bool ShowArrow { get; set; }

    [Prop] public bool Persistent { get; set; }

    [Prop] public TooltipVariant Variant { get; set; } = TooltipVariant.Default;

    [Event] public EventHandler<Event<Tooltip>>? OnOpen { get; set; }

    [Event] public EventHandler<Event<Tooltip>>? OnClose { get; set; }

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

    public static Tooltip Open(this Tooltip tooltip, bool open = true)
    {
        return tooltip with { Open = open };
    }

    public static Tooltip Bubble(this Tooltip tooltip, bool showArrow = true)
    {
        return tooltip with { ShowArrow = showArrow };
    }

    public static Tooltip Persist(this Tooltip tooltip, bool persistent = true)
    {
        return tooltip with { Persistent = persistent };
    }

    public static Tooltip Variant(this Tooltip tooltip, TooltipVariant variant)
    {
        return tooltip with { Variant = variant };
    }

    public static Tooltip Error(this Tooltip tooltip)
    {
        return tooltip with { Variant = TooltipVariant.Error };
    }

    public static Tooltip Info(this Tooltip tooltip)
    {
        return tooltip with { Variant = TooltipVariant.Info };
    }

    public static Tooltip Success(this Tooltip tooltip)
    {
        return tooltip with { Variant = TooltipVariant.Success };
    }

    public static Tooltip Warning(this Tooltip tooltip)
    {
        return tooltip with { Variant = TooltipVariant.Warning };
    }

    public static Tooltip HandleOpen(this Tooltip tooltip, Func<Event<Tooltip>, ValueTask> onOpen)
    {
        return tooltip with { OnOpen = onOpen };
    }

    public static Tooltip HandleOpen(this Tooltip tooltip, Action onOpen)
    {
        return tooltip with { OnOpen = new EventHandler<Event<Tooltip>>(_ => { onOpen(); return ValueTask.CompletedTask; }) };
    }

    public static Tooltip HandleClose(this Tooltip tooltip, Func<Event<Tooltip>, ValueTask> onClose)
    {
        return tooltip with { OnClose = onClose };
    }

    public static Tooltip HandleClose(this Tooltip tooltip, Action onClose)
    {
        return tooltip with { OnClose = new EventHandler<Event<Tooltip>>(_ => { onClose(); return ValueTask.CompletedTask; }) };
    }

    public static Tooltip HandleOpenChange(this Tooltip tooltip, Func<Event<Tooltip, bool>, ValueTask> onOpenChange)
    {
        return tooltip with { OnOpenChange = onOpenChange };
    }

    public static Tooltip HandleOpenChange(this Tooltip tooltip, Action<bool> onOpenChange)
    {
        return tooltip with { OnOpenChange = new EventHandler<Event<Tooltip, bool>>(e => { onOpenChange(e.Value); return ValueTask.CompletedTask; }) };
    }
}
