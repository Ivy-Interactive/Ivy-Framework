

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

/// <summary>
/// Represents a reference dot configuration.
/// </summary>
public record ReferenceDot
{
    /// <summary>
    /// Initializes a new instance of the ReferenceDot class with the specified coordinates and optional label.
    /// </summary>
    /// <param name="x">The X coordinate of the reference dot.</param>
    /// <param name="y">The Y coordinate of the reference dot.</param>
    /// <param name="label">Optional text label to display on or near the reference dot.</param>
    public ReferenceDot(double x, double y, string? label = null)
    {
        X = x;
        Y = y;
        Label = label;
    }

    /// <summary>
    /// Gets or sets the X coordinate.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Gets or sets the optional text label.
    /// </summary>
    public string? Label { get; set; }
}

/// <summary>
/// Extension methods for the ReferenceDot class.
/// </summary>
public static class ReferenceDotExtensions
{
    /// <summary>
    /// Sets the text label.
    /// </summary>
    /// <param name="referenceDot">The ReferenceDot to configure.</param>
    /// <param name="label">The text label to display.</param>
    /// <returns>A new ReferenceDot instance with the updated label.</returns>
    public static ReferenceDot Label(this ReferenceDot referenceDot, string label)
    {
        return referenceDot with { Label = label };
    }
}