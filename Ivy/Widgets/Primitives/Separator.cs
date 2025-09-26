using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A visual separator line that divides content sections, with optional text label.
/// </summary>
/// <remarks>
/// Creates horizontal or vertical separator lines to visually divide content areas.
/// Can include optional text that appears centered on the separator line.
/// </remarks>
public record Separator : WidgetBase<Separator>
{
    /// <summary>
    /// Initializes a new separator with optional text and orientation.
    /// </summary>
    /// <param name="text">Optional text to display on the separator line.</param>
    /// <param name="orientation">The orientation of the separator line (horizontal or vertical).</param>
    public Separator(string? text = null, Orientation orientation = Orientation.Horizontal)
    {
        Text = text;
        Orientation = orientation;
    }

    /// <summary>Gets or sets the orientation of the separator line.</summary>
    /// <value>The orientation (horizontal or vertical) of the separator.</value>
    [Prop] public Orientation Orientation { get; set; }

    /// <summary>Gets or sets the optional text to display on the separator line.</summary>
    /// <value>The text to display centered on the separator, or null for a plain line.</value>
    [Prop] public string? Text { get; set; }
}

/// <summary>
/// Extension methods for configuring Separator widgets.
/// </summary>
public static class SeparatorExtensions
{
    /// <summary>
    /// Sets the orientation of the separator.
    /// </summary>
    /// <param name="separator">The separator to configure.</param>
    /// <param name="orientation">The orientation (horizontal or vertical).</param>
    /// <returns>The separator with the specified orientation.</returns>
    public static Separator Orientation(this Separator separator, Orientation orientation) => separator with { Orientation = orientation };

    /// <summary>
    /// Sets the text to display on the separator line.
    /// </summary>
    /// <param name="separator">The separator to configure.</param>
    /// <param name="text">The text to display, or null for a plain line.</param>
    /// <returns>The separator with the specified text.</returns>
    public static Separator Text(this Separator separator, string? text) => separator with { Text = text };
}