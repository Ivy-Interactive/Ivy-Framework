using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Tooltip widget providing contextual information when hovering or focusing on trigger element.</summary>
public record Tooltip : WidgetBase<Tooltip>
{
    public Tooltip(object trigger, object content) : base([new Slot("Trigger", trigger), new Slot("Content", content)])
    {
    }

    /// <exception cref="NotSupportedException">Tooltip does not support children.</exception>
    public static Tooltip operator |(Tooltip widget, object child)
    {
        throw new NotSupportedException("Tooltip does not support children.");
    }
}

/// <summary>Extension methods for adding tooltips to widgets and views enabling fluent API for tooltip integration.</summary>
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
}