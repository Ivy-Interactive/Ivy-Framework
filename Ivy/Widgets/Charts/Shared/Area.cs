using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

/// <summary>
/// Represents an area chart configuration with customizable styling and behavior options.
/// </summary>
public record Area
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Area"/> class.
    /// </summary>
    /// <param name="dataKey">The key that identifies the data series for this area.</param>
    /// <param name="stackId">Optional identifier for stacking multiple areas together.</param>
    /// <param name="name">Optional display name for the area. If not provided, will be auto-generated from dataKey.</param>
    public Area(string dataKey, object? stackId = null, string? name = null)
    {
        DataKey = dataKey;
        Name = name ?? Utils.SplitPascalCase(dataKey);
        StackId = stackId?.ToString();
    }

    /// <summary>
    /// Gets the key that identifies the data series for this area.
    /// </summary>
    public string DataKey { get; }

    /// <summary>
    /// Gets or sets the type of curve used to render the area boundary.
    /// Common options include:
    /// - <see cref="CurveTypes.Natural"/>: Smooth natural curves (default)
    /// - <see cref="CurveTypes.Linear"/>: Straight line connections
    /// - <see cref="CurveTypes.Monotone"/>: Monotonic curves for time series
    /// - <see cref="CurveTypes.Step"/>: Step-like connections
    /// </summary>
    public CurveTypes CurveType { get; set; } = CurveTypes.Natural;

    /// <summary>
    /// Gets or sets the type of legend representation for this area.
    /// Common options include:
    /// - <see cref="LegendTypes.Line"/>: Line legend (default)
    /// - <see cref="LegendTypes.Square"/>: Square legend
    /// - <see cref="LegendTypes.Circle"/>: Circle legend
    /// - <see cref="LegendTypes.Triangle"/>: Triangle legend
    /// </summary>
    public LegendTypes LegendType { get; set; } = LegendTypes.Line;

    /// <summary>
    /// Gets or sets the stroke color for the area boundary. If null, no stroke is applied.
    /// Common options include:
    /// Gets or sets the stroke color for the area boundary. If null, no stroke is applied.
    /// Available colors include: <see cref="Colors.Red"/>, <see cref="Colors.Blue"/>, 
    /// <see cref="Colors.Green"/>, <see cref="Colors.Primary"/>, etc.
    /// </summary>
    public Colors? Stroke { get; set; } = null;

    /// <summary>
    /// Gets or sets the width of the stroke line in pixels.
    /// Default is 1.
    /// </summary>
    public int StrokeWidth { get; set; } = 1;

    /// <summary>
    /// Gets or sets the fill color for the area. If null, no fill is applied.
    /// </summary>
    public Colors? Fill { get; set; } = null;

    /// <summary>
    /// Gets or sets the opacity of the fill color. Value ranges from 0.0 (transparent) to 1.0 (opaque).
    /// </summary>
    public double? FillOpacity { get; set; } = null;

    /// <summary>
    /// Gets or sets the dash pattern for the stroke line (e.g., "5,5" for dashed lines).
    /// Default is null.
    /// </summary>
    public string? StrokeDashArray { get; set; }

    /// <summary>
    /// Gets or sets whether to connect data points when null values are encountered.
    /// </summary>
    public bool ConnectNulls { get; set; } = false;

    /// <summary>
    /// Gets or sets the display name for this area in legends and tooltips.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the unit of measurement for the data values (e.g., "px", "kg", "%").
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Gets or sets whether the area should animate when data changes.
    /// </summary>
    public bool Animated { get; set; } = false;

    /// <summary>
    /// Gets or sets the label configuration for displaying values on the area.
    /// </summary>
    public Label? Label { get; set; } = null;

    /// <summary>
    /// Gets or sets the identifier for stacking multiple areas together.
    /// </summary>
    public string? StackId { get; set; }
}

/// <summary>
/// Extension methods for the <see cref="Area"/> class to provide a fluent API for configuration.
/// </summary>
public static class AreaExtensions
{
    /// <summary>
    /// Sets the curve type for the area boundary.
    /// </summary>
    /// <param name="area">The area to configure.</param>
    /// <param name="curveType">The type of curve to use for rendering the area boundary.</param>
    /// <returns>A new <see cref="Area"/> instance with the updated curve type.</returns>
    public static Area CurveType(this Area area, CurveTypes curveType)
    {
        return area with { CurveType = curveType };
    }

    /// <summary>
    /// Sets the legend type for the area.
    /// </summary>
    /// <param name="area">The area to configure.</param>
    /// <param name="legendType">The type of legend representation to use.</param>
    /// <returns>A new <see cref="Area"/> instance with the updated legend type.</returns>
    public static Area LegendType(this Area area, LegendTypes legendType)
    {
        return area with { LegendType = legendType };
    }

    /// <summary>
    /// Sets the stroke color for the area boundary.
    /// </summary>
    /// <param name="area">The area to configure.</param>
    /// <param name="stroke">The color to use for the stroke line.</param>
    /// <returns>A new <see cref="Area"/> instance with the updated stroke color.</returns>
    public static Area Stroke(this Area area, Colors stroke)
    {
        return area with { Stroke = stroke };
    }

    /// <summary>
    /// Sets the stroke width for the area boundary.
    /// </summary>
    /// <param name="area">The area to configure.</param>
    /// <param name="strokeWidth">The width of the stroke line in pixels.</param>
    /// <returns>A new <see cref="Area"/> instance with the updated stroke width.</returns>
    public static Area StrokeWidth(this Area area, int strokeWidth)
    {
        return area with { StrokeWidth = strokeWidth };
    }

    /// <summary>
    /// Sets the stroke dash array for creating dashed or dotted stroke lines.
    /// </summary>
    /// <param name="area">The area to configure.</param>
    /// <param name="strokeDashArray">The dash pattern (e.g., "5,5" for dashed lines).</param>
    /// <returns>A new <see cref="Area"/> instance with the updated stroke dash array.</returns>
    public static Area StrokeDashArray(this Area area, string strokeDashArray)
    {
        return area with { StrokeDashArray = strokeDashArray };
    }

    /// <summary>
    /// Sets the fill color for the area.
    /// </summary>
    /// <param name="area">The area to configure.</param>
    /// <param name="fill">The color to use for filling the area.</param>
    /// <returns>A new <see cref="Area"/> instance with the updated fill color.</returns>
    public static Area Fill(this Area area, Colors fill)
    {
        return area with { Fill = fill };
    }

    /// <summary>
    /// Sets whether to connect data points when null values are encountered.
    /// </summary>
    /// <param name="area">The area to configure.</param>
    /// <param name="connectNulls">True to connect null values, false to leave gaps.</param>
    /// <returns>A new <see cref="Area"/> instance with the updated connect nulls setting.</returns>
    public static Area ConnectNulls(this Area area, bool connectNulls = true)
    {
        return area with { ConnectNulls = connectNulls };
    }

    /// <summary>
    /// Sets the display name for the area in legends and tooltips.
    /// </summary>
    /// <param name="area">The area to configure.</param>
    /// <param name="name">The display name for the area.</param>
    /// <returns>A new <see cref="Area"/> instance with the updated name.</returns>
    public static Area Name(this Area area, string name)
    {
        return area with { Name = name };
    }

    /// <summary>
    /// Sets the unit of measurement for the data values.
    /// </summary>
    /// <param name="area">The area to configure.</param>
    /// <param name="unit">The unit of measurement (e.g., "px", "kg", "%").</param>
    /// <returns>A new <see cref="Area"/> instance with the updated unit.</returns>
    public static Area Unit(this Area area, string unit)
    {
        return area with { Unit = unit };
    }

    /// <summary>
    /// Sets whether the area should animate when data changes.
    /// </summary>
    /// <param name="area">The area to configure.</param>
    /// <param name="animated">True to enable animations, false to disable them.</param>
    /// <returns>A new <see cref="Area"/> instance with the updated animation setting.</returns>
    public static Area Animated(this Area area, bool animated = true)
    {
        return area with { Animated = animated };
    }

    /// <summary>
    /// Sets the label configuration for displaying values on the area.
    /// </summary>
    /// <param name="area">The area to configure.</param>
    /// <param name="label">The label configuration, or null to disable labels.</param>
    /// <returns>A new <see cref="Area"/> instance with the updated label configuration.</returns>
    public static Area Label(this Area area, Label? label)
    {
        return area with { Label = label };
    }

    /// <summary>
    /// Sets the identifier for stacking multiple areas together.
    /// </summary>
    /// <param name="area">The area to configure.</param>
    /// <param name="stackId">The stack identifier for grouping areas.</param>
    /// <returns>A new <see cref="Area"/> instance with the updated stack identifier.</returns>
    public static Area StackId(this Area area, string stackId)
    {
        return area with { StackId = stackId };
    }

    /// <summary>
    /// Sets the opacity of the fill color.
    /// </summary>
    /// <param name="area">The area to configure.</param>
    /// <param name="fillOpacity">The opacity value ranging from 0.0 (transparent) to 1.0 (opaque).</param>
    /// <returns>A new <see cref="Area"/> instance with the updated fill opacity.</returns>
    public static Area FillOpacity(this Area area, double fillOpacity)
    {
        return area with { FillOpacity = fillOpacity };
    }
}

