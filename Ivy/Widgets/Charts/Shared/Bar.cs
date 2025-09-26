using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

/// <summary>
/// Represents a bar chart configuration with customizable styling, positioning, and behavior options.
/// </summary>
public record Bar
{
    /// <summary>
    /// Initializes a new instance of the Bar class with the specified data key and optional configuration.
    /// </summary>
    /// <param name="dataKey">The key that identifies the data series for this bar. This should match a property name in your data objects.</param>
    /// <param name="stackId">Optional identifier for stacking multiple bars together. Bars with the same stack ID will be grouped.</param>
    /// <param name="name">Optional display name for the bar. If not provided, will use the dataKey value.</param>
    public Bar(string dataKey, object? stackId = null, string? name = null)
    {
        DataKey = dataKey;
        Name = name ?? dataKey;
        StackId = stackId?.ToString();
    }

    /// <summary>
    /// Gets the key that identifies the data series for this bar. This key should match a property name in your data objects.
    /// This key is used to bind the bar to specific data values in your chart configuration.
    /// </summary>
    public string DataKey { get; }

    /// <summary>
    /// Gets or sets the type of legend representation for this bar. Determines how the bar appears in the chart legend.
    /// Common options include:
    /// - <see cref="LegendTypes.Line"/>: Line legend (default)
    /// - <see cref="LegendTypes.Square"/>: Square legend
    /// - <see cref="LegendTypes.Circle"/>: Circle legend
    /// - <see cref="LegendTypes.Triangle"/>: Triangle legend
    /// </summary>
    public LegendTypes LegendType { get; set; } = LegendTypes.Line;

    /// <summary>
    /// Gets or sets the stroke color for the bar border. If null, no stroke is applied.
    /// <see cref="Colors"/> is an enum that contains all the available colors for the chart.
    /// </summary>
    public Colors? Stroke { get; set; } = null;

    /// <summary>
    /// Gets or sets the width of the stroke line in pixels. The stroke creates the border around the bar.
    /// Default is 1.
    /// </summary>
    public int StrokeWidth { get; set; } = 1;

    /// <summary>
    /// Gets or sets the fill color for the bar. If null, a default color from the chart's color scheme will be used.
    /// <see cref="Colors"/> is an enum that contains all the available colors for the chart.
    /// Default is null.
    /// </summary>
    public Colors? Fill { get; set; } = null;

    /// <summary>
    /// Gets or sets the opacity of the fill color. Value ranges from 0.0 (transparent) to 1.0 (opaque).
    /// Default is null.
    /// </summary>
    public double? FillOpacity { get; set; } = null;

    /// <summary>
    /// Gets or sets the dash pattern for the stroke line (e.g., "5,5" for dashed borders).
    /// </summary>
    public string? StrokeDashArray { get; set; }

    /// <summary>
    /// Gets or sets the display name for this bar in legends and tooltips.
    /// Default is null.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the unit of measurement for the data values (e.g., "px", "kg", "%", "$").
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Gets or sets whether the bar should animate when data changes or when the chart is first rendered.
    /// Default is false.
    /// </summary>
    public bool Animated { get; set; } = false;

    /// <summary>
    /// Gets or sets the identifier for stacking multiple bars together. Bars with the same stack ID will be grouped vertically.
    /// Default is null.
    /// </summary>
    public string? StackId { get; set; }

    /// <summary>
    /// Gets or sets the corner radius for the bar, creating rounded corners. The array contains [topLeft, topRight, bottomRight, bottomLeft] values.
    /// Default is [0, 0, 0, 0].
    /// </summary>
    public int[] Radius { get; set; } = [0, 0, 0, 0];

    /// <summary>
    /// Gets or sets the label configurations for displaying values on or near the bar.
    /// Default is [].
    /// </summary>
    public LabelList[] LabelLists { get; set; } = [];
}

/// <summary>
/// Extension methods for the Bar class that provide a fluent API for easy configuration.
/// These methods allow you to chain multiple configuration calls for better readability and maintainability.
/// </summary>
public static class BarExtensions
{
    /// <summary>
    /// Sets the type of legend representation for the bar, determining how it appears in the chart legend.
    /// </summary>
    /// <param name="area">The bar to configure.</param>
    /// <param name="legendType">The type of legend representation to use.</param>
    /// <see cref="LegendTypes"/> is an enum that contains all the available legend types for the chart.
    /// <see cref="Colors"/> is an enum that contains all the available colors for the chart.
    /// <returns>A new Bar instance with the updated legend type.</returns>
    public static Bar LegendType(this Bar area, LegendTypes legendType)
    {
        return area with { LegendType = legendType };
    }

    /// <summary>
    /// Sets the stroke color for the bar border. The stroke creates the visual outline around the bar.
    /// </summary>
    /// <param name="area">The bar to configure.</param>
    /// <param name="stroke">The color to use for the stroke line.</param>
    /// <returns>A new Bar instance with the updated stroke color.</returns>
    public static Bar Stroke(this Bar area, Colors stroke)
    {
        return area with { Stroke = stroke };
    }

    /// <summary>
    /// Sets the width of the stroke line in pixels. The stroke creates the border around the bar.
    /// </summary>
    /// <param name="area">The bar to configure.</param>
    /// <param name="strokeWidth">The width of the stroke line in pixels.</param>
    /// <returns>A new Bar instance with the updated stroke width.</returns>
    public static Bar StrokeWidth(this Bar area, int strokeWidth)
    {
        return area with { StrokeWidth = strokeWidth };
    }

    /// <summary>
    /// Sets the dash pattern for the stroke line, creating dashed or dotted borders.
    /// </summary>
    /// <param name="area">The bar to configure.</param>
    /// <param name="strokeDashArray">The dash pattern (e.g., "5,5" for dashed lines, "10,5,5,5" for dash-dot pattern).</param>
    /// <returns>A new Bar instance with the updated stroke dash array.</returns>
    public static Bar StrokeDashArray(this Bar area, string strokeDashArray)
    {
        return area with { StrokeDashArray = strokeDashArray };
    }

    /// <summary>
    /// Sets the fill color for the bar. This is the main color that fills the interior of the bar.
    /// </summary>
    /// <param name="area">The bar to configure.</param>
    /// <param name="fill">The color to use for filling the bar.</param>
    /// <returns>A new Bar instance with the updated fill color.</returns>
    public static Bar Fill(this Bar area, Colors fill)
    {
        return area with { Fill = fill };
    }

    /// <summary>
    /// Sets the display name for the bar in legends and tooltips.
    /// </summary>
    /// <param name="area">The bar to configure.</param>
    /// <param name="name">The display name for the bar.</param>
    /// <returns>A new Bar instance with the updated name.</returns>
    public static Bar Name(this Bar area, string name)
    {
        return area with { Name = name };
    }

    /// <summary>
    /// Sets the unit of measurement for the data values.
    /// </summary>
    /// <param name="area">The bar to configure.</param>
    /// <param name="unit">The unit of measurement (e.g., "px", "kg", "%", "$").</param>
    /// <returns>A new Bar instance with the updated unit.</returns>
    public static Bar Unit(this Bar area, string unit)
    {
        return area with { Unit = unit };
    }

    /// <summary>
    /// Sets whether the bar should animate when data changes or when the chart is first rendered.
    /// </summary>
    /// <param name="area">The bar to configure.</param>
    /// <param name="animated">True to enable animations, false to disable them.</param>
    /// <returns>A new Bar instance with the updated animation setting.</returns>
    public static Bar Animated(this Bar area, bool animated = true)
    {
        return area with { Animated = animated };
    }

    /// <summary>
    /// Adds one or more label configurations to the bar for displaying values on or near the bar.
    /// </summary>
    /// <param name="bar">The bar to configure.</param>
    /// <param name="labelList">The label configurations to add to the bar.</param>
    /// <returns>A new Bar instance with the additional label configurations.</returns>
    public static Bar LabelList(this Bar bar, params LabelList[] labelList)
    {
        return bar with { LabelLists = [.. bar.LabelLists, .. labelList] };
    }

    /// <summary>
    /// Adds a label configuration to the bar using a data key. This creates a simple label that displays the value from the specified data property.
    /// </summary>
    /// <param name="bar">The bar to configure.</param>
    /// <param name="dataKey">The data key to use for the label value.</param>
    /// <returns>A new Bar instance with the additional label configuration.</returns>
    public static Bar LabelList(this Bar bar, string dataKey)
    {
        return bar with { LabelLists = [.. bar.LabelLists, new LabelList(dataKey)] };
    }

    /// <summary>
    /// Sets the identifier for stacking multiple bars together. Bars with the same stack ID will be grouped vertically.
    /// </summary>
    /// <param name="area">The bar to configure.</param>
    /// <param name="stackId">The stack identifier for grouping bars.</param>
    /// <returns>A new Bar instance with the updated stack identifier.</returns>
    public static Bar StackId(this Bar area, string stackId)
    {
        return area with { StackId = stackId };
    }

    /// <summary>
    /// Sets the opacity of the fill color. This allows you to create semi-transparent bars for better visual layering.
    /// </summary>
    /// <param name="area">The bar to configure.</param>
    /// <param name="fillOpacity">The opacity value ranging from 0.0 (transparent) to 1.0 (opaque).</param>
    /// <returns>A new Bar instance with the updated fill opacity.</returns>
    public static Bar FillOpacity(this Bar area, double fillOpacity)
    {
        return area with { FillOpacity = fillOpacity };
    }

    /// <summary>
    /// Sets all corner radii to the same value, creating uniformly rounded corners on the bar.
    /// </summary>
    /// <param name="area">The bar to configure.</param>
    /// <param name="radius">The radius value to apply to all four corners in pixels.</param>
    /// <returns>A new Bar instance with the updated corner radius.</returns>
    public static Bar Radius(this Bar area, int radius)
    {
        return area with { Radius = [radius, radius, radius, radius] };
    }

    /// <summary>
    /// Sets the corner radii using an array of four values for [topLeft, topRight, bottomRight, bottomLeft].
    /// </summary>
    /// <param name="area">The bar to configure.</param>
    /// <param name="radius">An array of four radius values in pixels.</param>
    /// <returns>A new Bar instance with the updated corner radii.</returns>
    public static Bar Radius(this Bar area, int[] radius)
    {
        return area with { Radius = radius };
    }

    /// <summary>
    /// Sets the corner radii using four individual values for precise control over each corner.
    /// </summary>
    /// <param name="area">The bar to configure.</param>
    /// <param name="topLeft">The radius for the top-left corner in pixels.</param>
    /// <param name="topRight">The radius for the top-right corner in pixels.</param>
    /// <param name="bottomRight">The radius for the bottom-right corner in pixels.</param>
    /// <param name="bottomLeft">The radius for the bottom-left corner in pixels.</param>
    /// <returns>A new Bar instance with the updated corner radii.</returns>
    public static Bar Radius(this Bar area, int topLeft, int topRight, int bottomRight, int bottomLeft)
    {
        return area with { Radius = [topLeft, topRight, bottomRight, bottomLeft] };
    }

    /// <summary>
    /// Sets the corner radii using two values for horizontal and vertical symmetry.
    /// </summary>
    /// <param name="area">The bar to configure.</param>
    /// <param name="top">The radius for the top corners (topLeft and topRight) in pixels.</param>
    /// <param name="bottom">The radius for the bottom corners (bottomLeft and bottomRight) in pixels.</param>
    /// <returns>A new Bar instance with the updated corner radii.</returns>
    public static Bar Radius(this Bar area, int top, int bottom)
    {
        return area with { Radius = [top, top, bottom, bottom] };
    }
}

