using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

public record Area
{
    public Area(string dataKey, object? stackId = null, string? name = null)
    {
        DataKey = dataKey;
        Name = name ?? Utils.SplitPascalCase(dataKey);
        StackId = stackId?.ToString();
    }

    public string DataKey { get; }
    public CurveTypes CurveType { get; set; } = CurveTypes.Natural;
    public LegendTypes LegendType { get; set; } = LegendTypes.Line;
    public Colors? Stroke { get; set; } = null;
    public int StrokeWidth { get; set; } = 1;
    public Colors? Fill { get; set; } = null;
    public double? FillOpacity { get; set; } = null;
    public string? StrokeDashArray { get; set; }
    public bool ConnectNulls { get; set; } = false;
    public string? Name { get; set; }
    public string? Unit { get; set; }
    public bool Animated { get; set; } = false;
    public Label? Label { get; set; } = null;
    public string? StackId { get; set; }
}

public static class AreaExtensions
{
    public static Area CurveType(this Area area, CurveTypes curveType)
    {
        return area with { CurveType = curveType };
    }

    public static Area LegendType(this Area area, LegendTypes legendType)
    {
        return area with { LegendType = legendType };
    }

    public static Area Stroke(this Area area, Colors stroke)
    {
        return area with { Stroke = stroke };
    }

    public static Area StrokeWidth(this Area area, int strokeWidth)
    {
        return area with { StrokeWidth = strokeWidth };
    }

    public static Area StrokeDashArray(this Area area, string strokeDashArray)
    {
        return area with { StrokeDashArray = strokeDashArray };
    }

    public static Area Fill(this Area area, Colors fill)
    {
        return area with { Fill = fill };
    }

    public static Area ConnectNulls(this Area area, bool connectNulls = true)
    {
        return area with { ConnectNulls = connectNulls };
    }

    public static Area Name(this Area area, string name)
    {
        return area with { Name = name };
    }

    public static Area Unit(this Area area, string unit)
    {
        return area with { Unit = unit };
    }

    public static Area Animated(this Area area, bool animated = true)
    {
        return area with { Animated = animated };
    }

    public static Area Label(this Area area, Label? label)
    {
        return area with { Label = label };
    }

    public static Area StackId(this Area area, string stackId)
    {
        return area with { StackId = stackId };
    }

    public static Area FillOpacity(this Area area, double fillOpacity)
    {
        return area with { FillOpacity = fillOpacity };
    }
}

