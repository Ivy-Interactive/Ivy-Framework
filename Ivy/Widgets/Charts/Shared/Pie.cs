using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

public record Pie
{
    public Pie(string dataKey, string nameKey)
    {
        DataKey = dataKey;
        NameKey = nameKey;
    }

    public string DataKey { get; }
    public string NameKey { get; set; }
    public LegendTypes LegendType { get; set; } = LegendTypes.Line;
    public Colors? Stroke { get; set; } = null;
    public int StrokeWidth { get; set; } = 1;
    public Colors? Fill { get; set; } = null;
    public double? FillOpacity { get; set; } = null;
    public string? StrokeDashArray { get; set; }
    public bool Animated { get; set; } = false;
    public object? Cx { get; set; }
    public object? Cy { get; set; }
    public object? InnerRadius { get; set; }
    public object? OuterRadius { get; set; }
    public int StartAngle { get; set; } = 0;
    public int EndAngle { get; set; } = 360;
    public LabelList[] LabelLists { get; set; } = [];
}

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

