using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

/// <summary>
/// Represents a pie chart configuration.
/// </summary>
public record Pie
{
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
    public static Pie LegendType(this Pie pie, LegendTypes legendType)
    {
        return pie with { LegendType = legendType };
    }

    public static Pie Stroke(this Pie pie, Colors stroke)
    {
        return pie with { Stroke = stroke };
    }

    public static Pie StrokeWidth(this Pie pie, int strokeWidth)
    {
        return pie with { StrokeWidth = strokeWidth };
    }

    public static Pie StrokeDashArray(this Pie pie, string strokeDashArray)
    {
        return pie with { StrokeDashArray = strokeDashArray };
    }

    public static Pie Fill(this Pie pie, Colors fill)
    {
        return pie with { Fill = fill };
    }

    public static Pie FillOpacity(this Pie pie, double fillOpacity)
    {
        return pie with { FillOpacity = fillOpacity };
    }

    public static Pie Animated(this Pie pie, bool animated = true)
    {
        return pie with { Animated = animated };
    }

    public static Pie Cx(this Pie pie, int cx)
    {
        return pie with { Cx = cx };
    }

    public static Pie Cx(this Pie pie, string cx)
    {
        return pie with { Cx = cx };
    }

    public static Pie Cy(this Pie pie, int cy)
    {
        return pie with { Cy = cy };
    }

    public static Pie Cy(this Pie pie, string cy)
    {
        return pie with { Cy = cy };
    }

    public static Pie InnerRadius(this Pie pie, int innerRadius)
    {
        return pie with { InnerRadius = innerRadius };
    }

    public static Pie InnerRadius(this Pie pie, string innerRadius)
    {
        return pie with { InnerRadius = innerRadius };
    }

    public static Pie OuterRadius(this Pie pie, int outerRadius)
    {
        return pie with { OuterRadius = outerRadius };
    }

    public static Pie OuterRadius(this Pie pie, string outerRadius)
    {
        return pie with { OuterRadius = outerRadius };
    }

    public static Pie StartAngle(this Pie pie, int startAngle)
    {
        return pie with { StartAngle = startAngle };
    }

    public static Pie EndAngle(this Pie pie, int endAngle)
    {
        return pie with { EndAngle = endAngle };
    }

    public static Pie LabelLists(this Pie pie, LabelList[] labelLists)
    {
        return pie with { LabelLists = labelLists };
    }

    public static Pie LabelList(this Pie pie, LabelList labelList)
    {
        return pie with { LabelLists = [.. pie.LabelLists, labelList] };
    }

    public static Pie LabelList(this Pie pie, string dataKey)
    {
        return pie with { LabelLists = [.. pie.LabelLists, new LabelList(dataKey)] };
    }
}

