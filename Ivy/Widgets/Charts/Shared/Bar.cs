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
    public static Bar LegendType(this Bar area, LegendTypes legendType)
    {
        return area with { LegendType = legendType };
    }

    public static Bar Stroke(this Bar area, Colors stroke)
    {
        return area with { Stroke = stroke };
    }

    public static Bar StrokeWidth(this Bar area, int strokeWidth)
    {
        return area with { StrokeWidth = strokeWidth };
    }

    public static Bar StrokeDashArray(this Bar area, string strokeDashArray)
    {
        return area with { StrokeDashArray = strokeDashArray };
    }

    public static Bar Fill(this Bar area, Colors fill)
    {
        return area with { Fill = fill };
    }

    public static Bar Name(this Bar area, string name)
    {
        return area with { Name = name };
    }

    public static Bar Unit(this Bar area, string unit)
    {
        return area with { Unit = unit };
    }

    public static Bar Animated(this Bar area, bool animated = true)
    {
        return area with { Animated = animated };
    }

    public static Bar LabelList(this Bar bar, params LabelList[] labelList)
    {
        return bar with { LabelLists = [.. bar.LabelLists, .. labelList] };
    }

    public static Bar LabelList(this Bar bar, string dataKey)
    {
        return bar with { LabelLists = [.. bar.LabelLists, new LabelList(dataKey)] };
    }

    public static Bar StackId(this Bar area, string stackId)
    {
        return area with { StackId = stackId };
    }

    public static Bar FillOpacity(this Bar area, double fillOpacity)
    {
        return area with { FillOpacity = fillOpacity };
    }

    public static Bar Radius(this Bar area, int radius)
    {
        return area with { Radius = [radius, radius, radius, radius] };
    }

    public static Bar Radius(this Bar area, int[] radius)
    {
        return area with { Radius = radius };
    }

    public static Bar Radius(this Bar area, int topLeft, int topRight, int bottomRight, int bottomLeft)
    {
        return area with { Radius = [topLeft, topRight, bottomRight, bottomLeft] };
    }

    public static Bar Radius(this Bar area, int top, int bottom)
    {
        return area with { Radius = [top, top, bottom, bottom] };
    }
}

