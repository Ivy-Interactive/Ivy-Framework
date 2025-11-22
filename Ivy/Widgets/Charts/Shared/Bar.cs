using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

public record Bar
{
    /// <param name="stackId">Bars with the same stack ID will be grouped vertically.</param>
    /// <param name="name">If not provided, will use the dataKey value.</param>
    public Bar(string dataKey, object? stackId = null, string? name = null)
    {
        DataKey = dataKey;
        Name = name ?? dataKey;
        StackId = stackId?.ToString();
    }

    public string DataKey { get; }

    public LegendTypes LegendType { get; set; } = LegendTypes.Line;

    /// <summary>If null, no stroke is applied.</summary>
    public Colors? Stroke { get; set; } = null;

    public int StrokeWidth { get; set; } = 1;

    /// <summary>If null, a default color from the chart's color scheme will be used.</summary>
    public Colors? Fill { get; set; } = null;

    /// <summary>Value ranges from 0.0 (transparent) to 1.0 (opaque).</summary>
    public double? FillOpacity { get; set; } = null;

    public string? StrokeDashArray { get; set; }

    public string? Name { get; set; }

    public string? Unit { get; set; }

    public bool Animated { get; set; } = false;

    public string? StackId { get; set; }

    /// <summary>The array contains [topLeft, topRight, bottomRight, bottomLeft] values.</summary>
    public int[] Radius { get; set; } = [0, 0, 0, 0];

    public LabelList[] LabelLists { get; set; } = [];
}

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

