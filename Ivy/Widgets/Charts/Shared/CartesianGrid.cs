// ReSharper disable once CheckNamespace

using Ivy.Shared;

namespace Ivy.Charts;

/// <summary>
/// Represents a Cartesian grid configuration for charts, providing control over the positioning, sizing, and styling
/// of chart grid lines.
/// </summary>
public record CartesianGrid
{
    /// <summary>
    /// Gets or sets the X-coordinate position of the grid in pixels.
    /// </summary>
    public double? X { get; set; } = null;

    /// <summary>
    /// Gets or sets the Y-coordinate position of the grid in pixels.
    /// </summary>
    public double? Y { get; set; } = null;

    /// <summary>
    /// Gets or sets the width of the grid in pixels.
    /// </summary>
    public double? Width { get; set; } = null;

    /// <summary>
    /// Gets or sets the height of the grid in pixels.
    /// </summary>
    public double? Height { get; set; } = null;

    /// <summary>
    /// Gets or sets whether horizontal grid lines are displayed.
    /// </summary>
    public bool Horizontal { get; set; } = true;

    /// <summary>
    /// Gets or sets whether vertical grid lines are displayed.
    /// Default is true.    
    /// </summary>
    public bool Vertical { get; set; } = true;

    /// <summary>
    /// Gets or sets the fill color for the grid lines.
    /// <see cref="Colors"/> is an enum that contains all the available colors for the chart.
    /// </summary>
    public Colors? Fill { get; set; } = null;

    /// <summary>
    /// Gets or sets the opacity of the fill color for the grid lines. Value ranges from 0.0 to 1.0.
    /// </summary>
    public double? FillOpacity { get; set; } = null;

    /// <summary>
    /// Gets or sets the dash pattern for the grid lines, creating dashed or dotted patterns.
    /// </summary>
    public string? StrokeDashArray { get; set; }
}

/// <summary>
/// Extension methods for the CartesianGrid class that provide a fluent API for easy configuration.
/// </summary>
public static class CartesianGridExtensions
{
    /// <summary>
    /// Sets the X-coordinate position of the grid in pixels.
    /// </summary>
    /// <param name="cartesianGrid">The CartesianGrid to configure.</param>
    /// <param name="x">The X-coordinate position in pixels.</param>
    /// <returns>A new CartesianGrid instance with the updated X position.</returns>
    public static CartesianGrid X(this CartesianGrid cartesianGrid, double x)
    {
        return cartesianGrid with { X = x };
    }

    /// <summary>
    /// Sets the Y-coordinate position of the grid in pixels.
    /// </summary>
    /// <param name="cartesianGrid">The CartesianGrid to configure.</param>
    /// <param name="y">The Y-coordinate position in pixels.</param>
    /// <returns>A new CartesianGrid instance with the updated Y position.</returns>
    public static CartesianGrid Y(this CartesianGrid cartesianGrid, double y)
    {
        return cartesianGrid with { Y = y };
    }

    /// <summary>
    /// Sets the width of the grid in pixels.
    /// </summary>
    /// <param name="cartesianGrid">The CartesianGrid to configure.</param>
    /// <param name="width">The width of the grid in pixels.</param>
    /// <returns>A new CartesianGrid instance with the updated width.</returns>
    public static CartesianGrid Width(this CartesianGrid cartesianGrid, double width)
    {
        return cartesianGrid with { Width = width };
    }

    /// <summary>
    /// Sets the height of the grid in pixels.
    /// </summary>
    /// <param name="cartesianGrid">The CartesianGrid to configure.</param>
    /// <param name="height">The height of the grid in pixels.</param>
    /// <returns>A new CartesianGrid instance with the updated height.</returns>
    public static CartesianGrid Height(this CartesianGrid cartesianGrid, double height)
    {
        return cartesianGrid with { Height = height };
    }

    /// <summary>
    /// Enables only horizontal grid lines and disables vertical grid lines.
    /// </summary>
    /// <param name="cartesianGrid">The CartesianGrid to configure.</param>
    /// <returns>A new CartesianGrid instance with only horizontal grid lines enabled.</returns>
    public static CartesianGrid Horizontal(this CartesianGrid cartesianGrid)
    {
        return cartesianGrid with { Horizontal = true, Vertical = false };
    }

    /// <summary>
    /// Enables only vertical grid lines and disables horizontal grid lines.
    /// </summary>
    /// <param name="cartesianGrid">The CartesianGrid to configure.</param>
    /// <returns>A new CartesianGrid instance with only vertical grid lines enabled.</returns>
    public static CartesianGrid Vertical(this CartesianGrid cartesianGrid)
    {
        return cartesianGrid with { Vertical = true, Horizontal = false };
    }

    /// <summary>
    /// Sets the fill color for the grid lines.
    /// </summary>
    /// <param name="cartesianGrid">The CartesianGrid to configure.</param>
    /// <param name="fill">The color to use for the grid lines.</param>
    /// <returns>A new CartesianGrid instance with the updated fill color.</returns>
    public static CartesianGrid Fill(this CartesianGrid cartesianGrid, Colors fill)
    {
        return cartesianGrid with { Fill = fill };
    }

    /// <summary>
    /// Sets the opacity of the fill color for the grid lines.
    /// </summary>
    /// <param name="cartesianGrid">The CartesianGrid to configure.</param>
    /// <param name="fillOpacity">The opacity value ranging from 0.0 (transparent) to 1.0 (opaque).</param>
    /// <returns>A new CartesianGrid instance with the updated fill opacity.</returns>
    public static CartesianGrid FillOpacity(this CartesianGrid cartesianGrid, double fillOpacity)
    {
        return cartesianGrid with { FillOpacity = fillOpacity };
    }

    /// <summary>
    /// Sets the dash pattern for the grid lines, creating dashed or dotted patterns.
    /// </summary>
    /// <param name="cartesianGrid">The CartesianGrid to configure.</param>
    /// <param name="strokeDashArray">The dash pattern (e.g., "5,5" for dashed lines, "10,5,5,5" for dash-dot pattern).</param>
    /// <returns>A new CartesianGrid instance with the updated stroke dash array.</returns>
    public static CartesianGrid StrokeDashArray(this CartesianGrid cartesianGrid, string strokeDashArray)
    {
        return cartesianGrid with { StrokeDashArray = strokeDashArray };
    }
}