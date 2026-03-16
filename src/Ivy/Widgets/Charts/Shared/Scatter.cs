
// ReSharper disable once CheckNamespace
namespace Ivy;

public record Scatter
{
    public Scatter(string dataKey, string? name = null)
    {
        DataKey = dataKey;
        Name = name ?? StringHelper.SplitPascalCase(dataKey);
    }

    internal Scatter()
    {
    }

    public string? DataKey { get; }

    public ScatterShape Shape { get; set; } = ScatterShape.Circle;

    public ScatterShape? LegendType { get; set; } = null;

    public Colors? Fill { get; set; } = null;

    public Colors? Stroke { get; set; } = null;

    public int StrokeWidth { get; set; } = 1;

    public string? StrokeDashArray { get; set; }

    public string? Name { get; set; }

    public string? Unit { get; set; }

    public bool Animated { get; set; } = false;

    public bool Line { get; set; } = false;

    public ScatterLineType LineType { get; set; } = ScatterLineType.Joint;

    public double? FillOpacity { get; set; } = null;
}

public static class ScatterExtensions
{
    public static Scatter Shape(this Scatter scatter, ScatterShape shape)
    {
        return scatter with { Shape = shape };
    }

    public static Scatter LegendType(this Scatter scatter, ScatterShape legendType)
    {
        return scatter with { LegendType = legendType };
    }

    public static Scatter Fill(this Scatter scatter, Colors fill)
    {
        return scatter with { Fill = fill };
    }

    public static Scatter Stroke(this Scatter scatter, Colors stroke)
    {
        return scatter with { Stroke = stroke };
    }

    public static Scatter StrokeWidth(this Scatter scatter, int strokeWidth)
    {
        return scatter with { StrokeWidth = strokeWidth };
    }

    public static Scatter StrokeDashArray(this Scatter scatter, string strokeDashArray)
    {
        return scatter with { StrokeDashArray = strokeDashArray };
    }

    public static Scatter Name(this Scatter scatter, string name)
    {
        return scatter with { Name = name };
    }

    public static Scatter Unit(this Scatter scatter, string unit)
    {
        return scatter with { Unit = unit };
    }

    public static Scatter Animated(this Scatter scatter, bool animated = true)
    {
        return scatter with { Animated = animated };
    }

    public static Scatter Line(this Scatter scatter, bool line = true)
    {
        return scatter with { Line = line };
    }

    public static Scatter LineType(this Scatter scatter, ScatterLineType lineType)
    {
        return scatter with { LineType = lineType };
    }

    public static Scatter FillOpacity(this Scatter scatter, double fillOpacity)
    {
        return scatter with { FillOpacity = fillOpacity };
    }
}
