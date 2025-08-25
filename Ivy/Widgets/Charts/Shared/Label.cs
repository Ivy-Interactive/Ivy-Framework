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
    /// <summary>
    /// Sets the offset distance of the label from its associated chart element in pixels.
    /// </summary>
    /// <param name="label">The Label to configure.</param>
    /// <param name="offset">The offset distance in pixels.</param>
    /// <returns>A new Label instance with the updated offset.</returns>
    public static Label Offset(this Label label, double offset)
    {
        return label with { Offset = offset };
    }

    /// <summary>
    /// Sets the rotation angle of the label text in degrees. Positive values rotate clockwise.
    /// </summary>
    /// <param name="label">The Label to configure.</param>
    /// <param name="angle">The rotation angle in degrees.</param>
    /// <returns>A new Label instance with the updated rotation angle.</returns>
    public static Label Angle(this Label label, double angle)
    {
        return label with { Angle = angle };
    }

    /// <summary>
    /// Sets the position of the label relative to its associated chart element.
    /// </summary>
    /// <param name="label">The Label to configure.</param>
    /// <param name="position">The position for the label.</param>
    /// <returns>A new Label instance with the updated position.</returns>
    public static Label Position(this Label label, Positions position)
    {
        return label with { Position = position };
    }

    /// <summary>
    /// Sets the color of the label text.
    /// </summary>
    /// <param name="label">The Label to configure.</param>
    /// <param name="color">The color to use for the label text.</param>
    /// <returns>A new Label instance with the updated color.</returns>
    public static Label Color(this Label label, Colors color)
    {
        return label with { Color = color };
    }
}

