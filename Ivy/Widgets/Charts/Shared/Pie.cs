using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

/// <summary>
/// Represents a pie chart configuration.
/// </summary>
public record Pie
{
    /// <summary>
    /// Initializes a new instance of the Pie class with the specified data and name keys.
    /// </summary>
    /// <param name="dataKey">The key that identifies the data property containing the numerical values for pie slices.</param>
    /// <param name="nameKey">The key that identifies the property containing the names/labels for pie slices.</param>
    public Pie(string dataKey, string nameKey)
    {
        DataKey = dataKey;
        NameKey = nameKey;
    }

    /// <summary>
    /// Gets the key that identifies which data property contains the numerical values for pie slices.
    /// </summary>
    public string DataKey { get; }

    /// <summary>
    /// Gets or sets the key that identifies which property contains the names/labels for pie slices.
    /// </summary>
    public string NameKey { get; set; }

    /// <summary>
    /// Gets or sets the visual representation type for this pie in the legend.
    /// </summary>
    public LegendTypes LegendType { get; set; } = LegendTypes.Line;

    /// <summary>
    /// Gets or sets the color of the pie slice borders/strokes.
    /// </summary>
    public Colors? Stroke { get; set; } = null;

    /// <summary>
    /// Gets or sets the width of the pie slice borders/strokes in pixels.
    /// </summary>
    public int StrokeWidth { get; set; } = 1;

    /// <summary>
    /// Gets or sets the fill color for pie slices.
    /// </summary>
    public Colors? Fill { get; set; } = null;

    /// <summary>
    /// Gets or sets the opacity of the pie slice fill colors.
    /// </summary>
    public double? FillOpacity { get; set; } = null;

    /// <summary>
    /// Gets or sets the dash pattern for the pie slice borders/strokes.
    /// </summary>
    public string? StrokeDashArray { get; set; }

    /// <summary>
    /// Gets or sets whether this pie chart should be animated when first rendered or updated.
    /// </summary>
    public bool Animated { get; set; } = false;

    /// <summary>
    /// Gets or sets the horizontal center position of the pie chart.
    /// </summary>
    public object? Cx { get; set; }

    /// <summary>
    /// Gets or sets the vertical center position of the pie chart.
    /// </summary>
    public object? Cy { get; set; }

    /// <summary>
    /// Gets or sets the inner radius of the pie chart, creating a donut chart effect.
    /// </summary>
    public object? InnerRadius { get; set; }

    /// <summary>
    /// Gets or sets the outer radius of the pie chart, controlling the overall size.
    /// </summary>
    public object? OuterRadius { get; set; }

    /// <summary>
    /// Gets or sets the starting angle for the pie chart in degrees. This controls where the first slice begins.
    /// </summary>
    public int StartAngle { get; set; } = 0;

    /// <summary>
    /// Gets or sets the ending angle for the pie chart in degrees. This controls where the last slice ends.
    /// </summary>
    public int EndAngle { get; set; } = 360;

    /// <summary>
    /// Gets or sets an array of label configurations for displaying values on or near pie slices.
    /// </summary>
    public LabelList[] LabelLists { get; set; } = [];
}

/// <summary>
/// Extension methods for the Pie class.
/// </summary>
public static class PieExtensions
{
    /// <summary>
    /// Sets the visual representation type for this pie in the legend.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="legendType">The legend type to use for representing this pie.</param>
    /// <returns>A new Pie instance with the updated legend type.</returns>
    public static Pie LegendType(this Pie pie, LegendTypes legendType)
    {
        return pie with { LegendType = legendType };
    }

    /// <summary>
    /// Sets the color of the pie slice borders/strokes.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="stroke">The color to use for the pie slice borders.</param>
    /// <returns>A new Pie instance with the updated stroke color.</returns>
    public static Pie Stroke(this Pie pie, Colors stroke)
    {
        return pie with { Stroke = stroke };
    }

    /// <summary>
    /// Sets the width of the pie slice borders/strokes in pixels.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="strokeWidth">The width of the pie slice borders in pixels.</param>
    /// <returns>A new Pie instance with the updated stroke width.</returns>
    public static Pie StrokeWidth(this Pie pie, int strokeWidth)
    {
        return pie with { StrokeWidth = strokeWidth };
    }

    /// <summary>
    /// Sets the dash pattern for the pie slice borders/strokes.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="strokeDashArray">The dash pattern string (e.g., "5,5" for dashed, "2,2" for dotted).</param>
    /// <returns>A new Pie instance with the updated dash pattern.</returns>
    public static Pie StrokeDashArray(this Pie pie, string strokeDashArray)
    {
        return pie with { StrokeDashArray = strokeDashArray };
    }

    /// <summary>
    /// Sets the fill color for pie slices.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="fill">The color to use for filling pie slices.</param>
    /// <returns>A new Pie instance with the updated fill color.</returns>
    public static Pie Fill(this Pie pie, Colors fill)
    {
        return pie with { Fill = fill };
    }

    /// <summary>
    /// Sets the opacity of the pie slice fill colors.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="fillOpacity">The opacity value from 0.0 (transparent) to 1.0 (opaque).</param>
    /// <returns>A new Pie instance with the updated fill opacity.</returns>
    public static Pie FillOpacity(this Pie pie, double fillOpacity)
    {
        return pie with { FillOpacity = fillOpacity };
    }

    /// <summary>
    /// Sets whether this pie chart should be animated.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="animated">True to enable animation, false to disable. Default is true.</param>
    /// <returns>A new Pie instance with the updated animation setting.</returns>
    public static Pie Animated(this Pie pie, bool animated = true)
    {
        return pie with { Animated = animated };
    }

    /// <summary>
    /// Sets the horizontal center position of the pie chart.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="cx">The horizontal center position in pixels.</param>
    /// <returns>A new Pie instance with the updated horizontal center position.</returns>
    public static Pie Cx(this Pie pie, int cx)
    {
        return pie with { Cx = cx };
    }

    /// <summary>
    /// Sets the horizontal center position of the pie chart.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="cx">The horizontal center position as a string (e.g., "50%", "200px").</param>
    /// <returns>A new Pie instance with the updated horizontal center position.</returns>
    public static Pie Cx(this Pie pie, string cx)
    {
        return pie with { Cx = cx };
    }

    /// <summary>
    /// Sets the vertical center position of the pie chart.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="cy">The vertical center position in pixels.</param>
    /// <returns>A new Pie instance with the updated vertical center position.</returns>
    public static Pie Cy(this Pie pie, int cy)
    {
        return pie with { Cy = cy };
    }

    /// <summary>
    /// Sets the vertical center position of the pie chart.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="cy">The vertical center position as a string (e.g., "50%", "200px").</param>
    /// <returns>A new Pie instance with the updated vertical center position.</returns>
    public static Pie Cy(this Pie pie, string cy)
    {
        return pie with { Cy = cy };
    }

    /// <summary>
    /// Sets the inner radius of the pie chart.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="innerRadius">The inner radius in pixels.</param>
    /// <returns>A new Pie instance with the updated inner radius.</returns>
    public static Pie InnerRadius(this Pie pie, int innerRadius)
    {
        return pie with { InnerRadius = innerRadius };
    }

    /// <summary>
    /// Sets the inner radius of the pie chart.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="innerRadius">The inner radius as a string (e.g., "30%", "100px").</param>
    /// <returns>A new Pie instance with the updated inner radius.</returns>
    public static Pie InnerRadius(this Pie pie, string innerRadius)
    {
        return pie with { InnerRadius = innerRadius };
    }

    /// <summary>
    /// Sets the outer radius of the pie chart.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="outerRadius">The outer radius in pixels.</param>
    /// <returns>A new Pie instance with the updated outer radius.</returns>
    public static Pie OuterRadius(this Pie pie, int outerRadius)
    {
        return pie with { OuterRadius = outerRadius };
    }

    /// <summary>
    /// Sets the outer radius of the pie chart.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="outerRadius">The outer radius as a string (e.g., "80%", "300px").</param>
    /// <returns>A new Pie instance with the updated outer radius.</returns>
    public static Pie OuterRadius(this Pie pie, string outerRadius)
    {
        return pie with { OuterRadius = outerRadius };
    }

    /// <summary>
    /// Sets the starting angle for the pie chart.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="startAngle">The starting angle in degrees (0-360, where 0 is at the top).</param>
    /// <returns>A new Pie instance with the updated starting angle.</returns>
    public static Pie StartAngle(this Pie pie, int startAngle)
    {
        return pie with { StartAngle = startAngle };
    }

    /// <summary>
    /// Sets the ending angle for the pie chart.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="endAngle">The ending angle in degrees (0-360).</param>
    /// <returns>A new Pie instance with the updated ending angle.</returns>
    public static Pie EndAngle(this Pie pie, int endAngle)
    {
        return pie with { EndAngle = endAngle };
    }

    /// <summary>
    /// Sets the complete array of label configurations.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="labelLists">The array of label configurations to use.</param>
    /// <returns>A new Pie instance with the updated label configurations.</returns>
    public static Pie LabelLists(this Pie pie, LabelList[] labelLists)
    {
        return pie with { LabelLists = labelLists };
    }

    /// <summary>
    /// Adds a single label configuration to the existing label list.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="labelList">The label configuration to add.</param>
    /// <returns>A new Pie instance with the additional label configuration.</returns>
    public static Pie LabelList(this Pie pie, LabelList labelList)
    {
        return pie with { LabelLists = [.. pie.LabelLists, labelList] };
    }

    /// <summary>
    /// Adds a simple label configuration for the specified data key to the existing label list.
    /// </summary>
    /// <param name="pie">The Pie to configure.</param>
    /// <param name="dataKey">The data property key to create a label for.</param>
    /// <returns>A new Pie instance with the additional label configuration.</returns>
    public static Pie LabelList(this Pie pie, string dataKey)
    {
        return pie with { LabelLists = [.. pie.LabelLists, new LabelList(dataKey)] };
    }
}

