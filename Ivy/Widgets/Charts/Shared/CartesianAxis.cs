// ReSharper disable once CheckNamespace

using Ivy.Shared;

namespace Ivy.Charts;

/// <summary>
/// Represents a Cartesian axis configuration for charts, providing control over the positioning, sizing, and styling of chart axes.
/// This class allows you to customize the appearance and behavior of both horizontal and vertical axes in your charts, including their position, dimensions, and visual styling.
/// </summary>
public record CartesianAxis
{
    /// <summary>
    /// Gets or sets the X-coordinate position of the axis in pixels.
    /// </summary>
    public double? X { get; set; } = null;

    /// <summary>
    /// Gets or sets the Y-coordinate position of the axis in pixels.
    /// </summary>
    public double? Y { get; set; } = null;

    /// <summary>
    /// Gets or sets the width of the axis in pixels.
    /// </summary>
    public double? Width { get; set; } = null;

    /// <summary>
    /// Gets or sets the height of the axis in pixels.
    /// </summary>
    public double? Height { get; set; } = null;

    /// <summary>
    /// Gets or sets whether horizontal grid lines are displayed.
    /// </summary>
    public bool Horizontal { get; set; } = true;

    /// <summary>
    /// Gets or sets whether vertical grid lines are displayed.
    /// </summary>
    public bool Vertical { get; set; } = true;

    /// <summary>
    /// Gets or sets the fill color for the axis grid lines.
    /// <see cref="Colors"/> is an enum that contains all the available colors for the chart.
    /// </summary>
    public Colors? Fill { get; set; } = null;

    /// <summary>
    /// Gets or sets the opacity of the fill color for the axis grid lines.
    /// </summary>
    public double? FillOpacity { get; set; } = null;

    /// <summary>
    /// Gets or sets the dash pattern for the axis grid lines, creating dashed or dotted patterns.
    /// </summary>
    public string? StrokeDashArray { get; set; }
}

/// <summary>
/// Extension methods for the CartesianAxis class that provide a fluent API for easy configuration.
/// These methods allow you to chain multiple configuration calls for better readability and maintainability.
/// Each method returns a new CartesianAxis instance with the updated configuration, following the immutable pattern.
/// </summary>
public static class CartesianAxisExtensions
{
    /// <summary>
    /// Sets the X-coordinate position of the axis in pixels.
    /// </summary>
    /// <param name="cartesianGrid">The CartesianAxis to configure.</param>
    /// <param name="x">The X-coordinate position in pixels.</param>
    /// <returns>A new CartesianAxis instance with the updated X position.</returns>
    public static CartesianAxis X(this CartesianAxis cartesianGrid, double x)
    {
        return cartesianGrid with { X = x };
    }

    /// <summary>
    /// Sets the Y-coordinate position of the axis in pixels.
    /// </summary>
    /// <param name="cartesianGrid">The CartesianAxis to configure.</param>
    /// <param name="y">The Y-coordinate position in pixels.</param>
    /// <returns>A new CartesianAxis instance with the updated Y position.</returns>
    public static CartesianAxis Y(this CartesianAxis cartesianGrid, double y)
    {
        return cartesianGrid with { Y = y };
    }

    /// <summary>
    /// Sets the width of the axis in pixels.
    /// </summary>
    /// <param name="cartesianGrid">The CartesianAxis to configure.</param>
    /// <param name="width">The width of the axis in pixels.</param>
    /// <returns>A new CartesianAxis instance with the updated width.</returns>
    public static CartesianAxis Width(this CartesianAxis cartesianGrid, double width)
    {
        return cartesianGrid with { Width = width };
    }

    /// <summary>
    /// Sets the height of the axis in pixels.
    /// </summary>
    /// <param name="cartesianGrid">The CartesianAxis to configure.</param>
    /// <param name="height">The height of the axis in pixels.</param>
    /// <returns>A new CartesianAxis instance with the updated height.</returns>
    public static CartesianAxis Height(this CartesianAxis cartesianGrid, double height)
    {
        return cartesianGrid with { Height = height };
    }

    /// <summary>
    /// Sets whether horizontal grid lines are displayed.
    /// </summary>
    /// <param name="cartesianGrid">The CartesianAxis to configure.</param>
    /// <param name="horizontal">True to show horizontal grid lines, false to hide them.</param>
    /// <returns>A new CartesianAxis instance with the updated horizontal grid line setting.</returns>
    public static CartesianAxis Horizontal(this CartesianAxis cartesianGrid, bool horizontal = true)
    {
        return cartesianGrid with { Horizontal = horizontal };
    }

    /// <summary>
    /// Sets whether vertical grid lines are displayed.
    /// </summary>
    /// <param name="cartesianGrid">The CartesianAxis to configure.</param>
    /// <param name="vertical">True to show vertical grid lines, false to hide them.</param>
    /// <returns>A new CartesianAxis instance with the updated vertical grid line setting.</returns>
    public static CartesianAxis Vertical(this CartesianAxis cartesianGrid, bool vertical = true)
    {
        return cartesianGrid with { Vertical = vertical };
    }

    /// <summary>
    /// Sets the fill color for the axis grid lines.
    /// </summary>
    /// <param name="cartesianGrid">The CartesianAxis to configure.</param>
    /// <param name="fill">The color to use for the grid lines.</param>
    /// <returns>A new CartesianAxis instance with the updated fill color.</returns>
    public static CartesianAxis Fill(this CartesianAxis cartesianGrid, Colors fill)
    {
        return cartesianGrid with { Fill = fill };
    }

    /// <summary>
    /// Sets the opacity of the fill color for the axis grid lines.
    /// </summary>
    /// <param name="cartesianGrid">The CartesianAxis to configure.</param>
    /// <param name="fillOpacity">The opacity value ranging from 0.0 (transparent) to 1.0 (opaque).</param>
    /// <returns>A new CartesianAxis instance with the updated fill opacity.</returns>
    public static CartesianAxis FillOpacity(this CartesianAxis cartesianGrid, double fillOpacity)
    {
        return cartesianGrid with { FillOpacity = fillOpacity };
    }

    /// <summary>
    /// Sets the dash pattern for the axis grid lines, creating dashed or dotted patterns.
    /// </summary>
    /// <param name="cartesianGrid">The CartesianAxis to configure.</param>
    /// <param name="strokeDashArray">The dash pattern (e.g., "5,5" for dashed lines, "10,5,5,5" for dash-dot pattern).</param>
    /// <returns>A new CartesianAxis instance with the updated stroke dash array.</returns>
    public static CartesianAxis StrokeDashArray(this CartesianAxis cartesianGrid, string strokeDashArray)
    {
        return cartesianGrid with { StrokeDashArray = strokeDashArray };
    }
}