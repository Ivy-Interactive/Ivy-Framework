using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Tooltip widget providing contextual information when hovering or focusing on trigger element.</summary>
public record Tooltip : WidgetBase<Tooltip>
{
    /// <summary>Initializes Tooltip with specified trigger element and tooltip content.</summary>
    /// <param name="trigger">Element that triggers tooltip display when hovered or focused.</param>
    /// <param name="content">Content to display in tooltip, typically explanatory text.</param>
    public Tooltip(object trigger, object content) : base([new Slot("Trigger", trigger), new Slot("Content", content)])
    {
    }

    /// <summary>Prevents adding children to Tooltip using pipe operator.</summary>
    /// <param name="widget">Tooltip widget.</param>
    /// <param name="child">Child content to add (not supported).</param>
    /// <returns>Always throws NotSupportedException.</returns>
    /// <exception cref="NotSupportedException">Tooltip does not support children.</exception>
    public static Tooltip operator |(Tooltip widget, object child)
    {
        throw new NotSupportedException("Tooltip does not support children.");
    }
}

/// <summary>Extension methods for adding tooltips to widgets and views enabling fluent API for tooltip integration.</summary>
public static class TooltipExtensions
{
    /// <summary>Adds tooltip to widget displaying specified text when hovered or focused.</summary>
    /// <param name="widget">Widget to add tooltip to.</param>
    /// <param name="toolTip">Tooltip text to display.</param>
    /// <returns>New Tooltip instance with widget as trigger and text as content.</returns>
    public static IWidget WithTooltip(this IWidget widget, string toolTip)
    {
        return new Tooltip(widget, toolTip);
    }

    /// <summary>Adds tooltip to view displaying specified text when hovered or focused.</summary>
    /// <param name="view">View to add tooltip to.</param>
    /// <param name="toolTip">Tooltip text to display.</param>
    /// <returns>New Tooltip instance with view as trigger and text as content.</returns>
    public static IWidget WithTooltip(this IView view, string toolTip)
    {
        return new Tooltip(view, toolTip);
    }
}