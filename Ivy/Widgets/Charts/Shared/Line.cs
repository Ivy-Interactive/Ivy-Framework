using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

public record Line
{
    public Line(string dataKey, string? name = null)
    {
        DataKey = dataKey;
        Name = name ?? Utils.SplitPascalCase(dataKey);
    }

    public string DataKey { get; }
    public CurveTypes CurveType { get; set; } = CurveTypes.Natural;
    public LegendTypes LegendType { get; set; } = LegendTypes.Line;
    public Scales Scale { get; set; } = Scales.Linear;
    public Colors? Stroke { get; set; } = null;
    public int StrokeWidth { get; set; } = 1;
    public string? StrokeDashArray { get; set; }
    public bool ConnectNulls { get; set; } = false;
    public string? Name { get; set; }
    public string? Unit { get; set; }
    public bool Animated { get; set; } = false;
    public Label? Label { get; set; } = null;
}

public static class LineExtensions
{
    public static Line CurveType(this Line line, CurveTypes curveType)
    {
        return line with { CurveType = curveType };
    }

    public static Line LegendType(this Line line, LegendTypes legendType)
    {
        return line with { LegendType = legendType };
    }

    public static Line Stroke(this Line line, Colors stroke)
    {
        return line with { Stroke = stroke };
    }

    public static Line StrokeWidth(this Line line, int strokeWidth)
    {
        return line with { StrokeWidth = strokeWidth };
    }

    public static Line StrokeDashArray(this Line line, string strokeDashArray)
    {
        return line with { StrokeDashArray = strokeDashArray };
    }

    public static Line ConnectNulls(this Line line, bool connectNulls = true)
    {
        return line with { ConnectNulls = connectNulls };
    }

    public static Line Name(this Line line, string name)
    {
        return line with { Name = name };
    }

    public static Line Unit(this Line line, string unit)
    {
        return line with { Unit = unit };
    }

    public static Line Animated(this Line line, bool animated = true)
    {
        return line with { Animated = animated };
    }

    public static Line Label(this Line line, Label? label)
    {
        return line with { Label = label };
    }

    public static Line Scale(this Line line, Scales scale)
    {
        return line with { Scale = scale };
    }
}

