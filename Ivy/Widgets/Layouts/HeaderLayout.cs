using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents a header layout widget that creates a layout with a fixed header section at the top and a scrollable content area below.
/// </summary>
public record HeaderLayout : WidgetBase<HeaderLayout>
{
    /// <summary>
    /// Initializes a new instance of the HeaderLayout class with the specified header and content.
    /// The header will be positioned at the top of the view and remain fixed, while the content will be displayed below it with independent scrolling capabilities.
    /// </summary>
    /// <param name="header">The content to display in the fixed header area at the top of the view. This typically contains navigation elements, toolbars, status indicators, or action buttons that should remain visible regardless of content scroll position.</param>
    /// <param name="content">The main content to display below the header. This content can be any length and will scroll independently while the header remains fixed at the top.</param>
    public HeaderLayout(object header, object content) : base([new Slot("Header", header), new Slot("Content", content)])
    {
    }

    [Prop] public bool ShowHeaderDivider { get; init; } = true;

    /// <summary>
    /// Operator overload that prevents adding children to the HeaderLayout using the pipe operator. 
    /// HeaderLayout uses a predefined two-slot system (Header and Content) and does not support additional children beyond the initial header and content parameters. 
    /// </summary>
    /// <param name="widget">The HeaderLayout widget.</param>
    /// <param name="child">The child content to add (not supported).</param>
    /// <exception cref="NotSupportedException">Always thrown, as HeaderLayout does not support additional children.</exception>
    public static HeaderLayout operator |(HeaderLayout widget, object child)
    {
        throw new NotSupportedException("HeaderLayout does not support children.");
    }
}