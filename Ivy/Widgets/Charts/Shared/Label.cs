using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

/// <summary>
/// Represents a label configuration for chart elements.
/// </summary>
public record Label
{
    /// <summary>
    /// Initializes a new instance of the Label class with default values.
    /// </summary>
    public Label()
    {
    }

    /// <summary>
    /// Gets or sets the offset distance of the label from its associated chart element in pixels.
    /// Default is 5 pixels.
    /// </summary>
    public double Offset { get; set; } = 5;

    /// <summary>
    /// Gets or sets the rotation angle of the label text in degrees. Positive values rotate clockwise.
    /// Default is null (no rotation).
    /// </summary>
    public double? Angle { get; set; } = null;

    /// <summary>
    /// Gets or sets the position of the label relative to its associated chart element.
    /// <see cref="Positions"/> is an enum that contains all the available positioning options.
    /// </summary>
    public Positions? Position { get; set; } = null;

    /// <summary>
    /// Gets or sets the color of the label text.
    /// <see cref="Colors"/> is an enum that contains all the available colors for the chart.
    /// </summary>
    public Colors? Color { get; set; } = null;
}

/// <summary>
/// Extension methods for the Label class that provide a fluent API for easy configuration.
/// </summary>
public static class LabelExtensions
{
    public static Label Offset(this Label label, double offset)
    {
        return label with { Offset = offset };
    }

    public static Label Angle(this Label label, double angle)
    {
        return label with { Angle = angle };
    }

    public static Label Position(this Label label, Positions position)
    {
        return label with { Position = position };
    }

    public static Label Color(this Label label, Colors color)
    {
        return label with { Color = color };
    }
}

