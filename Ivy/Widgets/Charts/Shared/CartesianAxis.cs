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
    public static CartesianAxis X(this CartesianAxis cartesianGrid, double x)
    {
        return cartesianGrid with { X = x };
    }

    public static CartesianAxis Y(this CartesianAxis cartesianGrid, double y)
    {
        return cartesianGrid with { Y = y };
    }

    public static CartesianAxis Width(this CartesianAxis cartesianGrid, double width)
    {
        return cartesianGrid with { Width = width };
    }

    public static CartesianAxis Height(this CartesianAxis cartesianGrid, double height)
    {
        return cartesianGrid with { Height = height };
    }

    public static CartesianAxis Horizontal(this CartesianAxis cartesianGrid, bool horizontal = true)
    {
        return cartesianGrid with { Horizontal = horizontal };
    }

    public static CartesianAxis Vertical(this CartesianAxis cartesianGrid, bool vertical = true)
    {
        return cartesianGrid with { Vertical = vertical };
    }

    public static CartesianAxis Fill(this CartesianAxis cartesianGrid, Colors fill)
    {
        return cartesianGrid with { Fill = fill };
    }

    public static CartesianAxis FillOpacity(this CartesianAxis cartesianGrid, double fillOpacity)
    {
        return cartesianGrid with { FillOpacity = fillOpacity };
    }

    public static CartesianAxis StrokeDashArray(this CartesianAxis cartesianGrid, string strokeDashArray)
    {
        return cartesianGrid with { StrokeDashArray = strokeDashArray };
    }
}