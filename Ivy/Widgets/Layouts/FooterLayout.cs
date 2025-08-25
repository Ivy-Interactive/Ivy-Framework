using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents a footer layout widget that creates a layout with a fixed footer at the bottom
/// and scrollable content above it. The FooterLayout widget uses a two-slot system: a "Footer" slot that remains fixed at the bottom
/// of the view, and a "Content" slot that can scroll freely above it.
/// </summary>
public record FooterLayout : WidgetBase<FooterLayout>
{
    /// <summary>
    /// Initializes a new instance of the FooterLayout class with the specified footer and content.
    /// </summary>
    /// <param name="footer">The content to display in the fixed footer area at the bottom of the view.</param>
    /// <param name="content">The main content to display above the footer. This content can be any length and will scroll independently while the footer remains fixed at the bottom.</param>
    public FooterLayout(object footer, object content) : base([new Slot("Footer", footer), new Slot("Content", content)])
    {
    }

    /// <summary>
    /// Operator overload that prevents adding children to the FooterLayout using the pipe operator. 
    /// FooterLayout uses a predefined two-slot system (Footer and Content) and does not support additional children beyond the initial footer and content parameters. 
    /// </summary>
    /// <param name="widget">The FooterLayout widget.</param>
    /// <param name="child">The child content to add (not supported).</param>
    /// <returns>This method always throws a NotSupportedException.</returns>
    /// <exception cref="NotSupportedException">Always thrown, as FooterLayout does not support additional children.</exception>
    public static FooterLayout operator |(FooterLayout widget, object child)
    {
        throw new NotSupportedException("FooterLayout does not support children.");
    }
}