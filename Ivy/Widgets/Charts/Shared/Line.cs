using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

/// <summary>
/// Represents a line chart configuration.
/// </summary>
public record Line
{
    /// <summary>
    /// Initializes a new instance of the Line class with the specified data key.
    /// </summary>
    /// <param name="dataKey">The key that identifies the data property to plot on this line.</param>
    /// <param name="name">Optional display name for the line.</param>
    public Line(string dataKey, string? name = null)
    {
        DataKey = dataKey;
        Name = name ?? Utils.SplitPascalCase(dataKey);
    }

    /// <summary>
    /// Gets the key that identifies which data property this line represents.
    /// </summary>
    public string DataKey { get; }

    /// <summary>
    /// Gets or sets the type of curve used to connect data points on the line.
    /// </summary>
    public CurveTypes CurveType { get; set; } = CurveTypes.Natural;

    /// <summary>
    /// Gets or sets the visual representation type for this line in the legend.
    /// </summary>
    public LegendTypes LegendType { get; set; } = LegendTypes.Line;

    /// <summary>
    /// Gets or sets the scale type used for this line's data values.
    /// </summary>
    public Scales Scale { get; set; } = Scales.Linear;

    /// <summary>
    /// Gets or sets the color of the line stroke.
    /// </summary>
    public Colors? Stroke { get; set; } = null;

    /// <summary>
    /// Gets or sets the width of the line stroke in pixels.
    /// </summary>
    public int StrokeWidth { get; set; } = 1;

    /// <summary>
    /// Gets or sets the dash pattern for the line stroke.
    /// </summary>
    public string? StrokeDashArray { get; set; }

    /// <summary>
    /// Gets or sets whether null or missing data points should be connected.
    /// </summary>
    public bool ConnectNulls { get; set; } = false;

    /// <summary>
    /// Gets or sets the display name for this line in legends, tooltips, and other UI elements.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the unit of measurement for this line's data values.
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Gets or sets whether this line should be animated when the chart is first rendered or updated.
    /// </summary>
    public bool Animated { get; set; } = false;

    /// <summary>
    /// Gets or sets the label configuration for displaying values on or near this line.
    /// </summary>
    public Label? Label { get; set; } = null;
}

/// <summary>
/// Extension methods for the Line class.
/// </summary>
public static class LineExtensions
{
    /// <summary>
    /// Sets the type of curve used to connect data points on the line.
    /// </summary>
    /// <param name="line">The Line to configure.</param>
    /// <param name="curveType">The curve type to use for connecting data points.</param>
    /// <returns>A new Line instance with the updated curve type.</returns>
    public static Line CurveType(this Line line, CurveTypes curveType)
    {
        return line with { CurveType = curveType };
    }

    /// <summary>
    /// Sets the visual representation type for this line in the legend.
    /// </summary>
    /// <param name="line">The Line to configure.</param>
    /// <param name="legendType">The legend type to use for representing this line.</param>
    /// <returns>A new Line instance with the updated legend type.</returns>
    public static Line LegendType(this Line line, LegendTypes legendType)
    {
        return line with { LegendType = legendType };
    }

    /// <summary>
    /// Sets the color of the line stroke. This overrides the chart's default color scheme.
    /// </summary>
    /// <param name="line">The Line to configure.</param>
    /// <param name="stroke">The color to use for the line stroke.</param>
    /// <returns>A new Line instance with the updated stroke color.</returns>
    public static Line Stroke(this Line line, Colors stroke)
    {
        return line with { Stroke = stroke };
    }

    /// <summary>
    /// Sets the width of the line stroke in pixels. Thicker lines improve visibility and emphasis.
    /// </summary>
    /// <param name="line">The Line to configure.</param>
    /// <param name="strokeWidth">The width of the line stroke in pixels.</param>
    /// <returns>A new Line instance with the updated stroke width.</returns>
    public static Line StrokeWidth(this Line line, int strokeWidth)
    {
        return line with { StrokeWidth = strokeWidth };
    }

    /// <summary>
    /// Sets the dash pattern for the line stroke, creating dashed or dotted line effects.
    /// </summary>
    /// <param name="line">The Line to configure.</param>
    /// <param name="strokeDashArray">The dash pattern string (e.g., "5,5" for dashed, "2,2" for dotted).</param>
    /// <returns>A new Line instance with the updated dash pattern.</returns>
    public static Line StrokeDashArray(this Line line, string strokeDashArray)
    {
        return line with { StrokeDashArray = strokeDashArray };
    }

    /// <summary>
    /// Sets whether null or missing data points should be connected, creating a continuous line.
    /// </summary>
    /// <param name="line">The Line to configure.</param>
    /// <param name="connectNulls">True to connect null points, false to break the line at gaps. Default is true.</param>
    /// <returns>A new Line instance with the updated null connection behavior.</returns>
    public static Line ConnectNulls(this Line line, bool connectNulls = true)
    {
        return line with { ConnectNulls = connectNulls };
    }

    /// <summary>
    /// Sets the display name for this line in legends, tooltips, and other UI elements.
    /// </summary>
    /// <param name="line">The Line to configure.</param>
    /// <param name="name">The user-friendly display name for this line.</param>
    /// <returns>A new Line instance with the updated display name.</returns>
    public static Line Name(this Line line, string name)
    {
        return line with { Name = name };
    }

    /// <summary>
    /// Sets the unit of measurement for this line's data values.
    /// </summary>
    /// <param name="line">The Line to configure.</param>
    /// <param name="unit">The unit string to display with data values (e.g., "USD", "kg", "°C").</param>
    /// <returns>A new Line instance with the updated unit.</returns>
    public static Line Unit(this Line line, string unit)
    {
        return line with { Unit = unit };
    }

    /// <summary>
    /// Sets whether this line should be animated when the chart is rendered or updated.
    /// </summary>
    /// <param name="line">The Line to configure.</param>
    /// <param name="animated">True to enable animation, false to disable. Default is true.</param>
    /// <returns>A new Line instance with the updated animation setting.</returns>
    public static Line Animated(this Line line, bool animated = true)
    {
        return line with { Animated = animated };
    }

    /// <summary>
    /// Sets the label configuration for displaying values on or near this line.
    /// </summary>
    /// <param name="line">The Line to configure.</param>
    /// <param name="label">The label configuration, or null to remove labels.</param>
    /// <returns>A new Line instance with the updated label configuration.</returns>
    public static Line Label(this Line line, Label? label)
    {
        return line with { Label = label };
    }

    /// <summary>
    /// Sets the scale type used for this line's data values.
    /// </summary>
    /// <param name="line">The Line to configure.</param>
    /// <param name="scale">The scale type to use for data visualization.</param>
    /// <returns>A new Line instance with the updated scale type.</returns>
    public static Line Scale(this Line line, Scales scale)
    {
        return line with { Scale = scale };
    }
}

