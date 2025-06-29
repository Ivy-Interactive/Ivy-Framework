using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Tooltip : WidgetBase<Tooltip>
{
    public Tooltip(object trigger, object content) : base([new Slot("Trigger", trigger), new Slot("Content", content)])
    {
    }

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
}