

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

/// <summary>
/// Represents a reference line configuration.
/// </summary>
public record ReferenceLine
{
    /// <summary>
    /// Initializes a new instance of the ReferenceLine class.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="label">The text label.</param>
    public ReferenceLine(double? x, double? y, string? label = null)
    {
        X = x;
        Y = y;
        Label = label;
    }

    /// <summary>
    /// Gets or sets the X coordinate.
    /// </summary>
    public double? X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    public double? Y { get; set; }

    /// <summary>
    /// Gets or sets the optional text label.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the width of the reference line stroke in pixels.
    /// </summary>
    public int StrokeWidth { get; set; } = 1;
}

/// <summary>
/// Extension methods for the ReferenceLine class.
/// </summary>
public static class ReferenceLineExtensions
{
    /// <summary>
    /// Sets the text label.
    /// </summary>
    /// <param name="referenceLine">The ReferenceLine to configure.</param>
    /// <param name="label">The text label to display.</param>
    /// <returns>A new ReferenceLine instance with the updated label.</returns>
    public static ReferenceLine Label(this ReferenceLine referenceLine, string label)
    {
        return referenceLine with { Label = label };
    }

    /// <summary>
    /// Sets the width of the reference line stroke in pixels.
    /// </summary>
    /// <param name="referenceLine">The ReferenceLine to configure.</param>
    /// <param name="strokeWidth">The width of the reference line stroke in pixels.</param>
    /// <returns>A new ReferenceLine instance with the updated stroke width.</returns>
    public static ReferenceLine StrokeWidth(this ReferenceLine referenceLine, int strokeWidth)
    {
        return referenceLine with { StrokeWidth = strokeWidth };
    }
}