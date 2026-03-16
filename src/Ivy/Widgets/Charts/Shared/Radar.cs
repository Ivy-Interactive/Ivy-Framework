
// ReSharper disable once CheckNamespace
namespace Ivy;

public record Radar
{
    public Radar(string dataKey, string? name = null)
    {
        DataKey = dataKey;
        Name = name;
    }

    internal Radar()
    {
    }

    [Prop] public string? DataKey { get; }
    [Prop] public string? Name { get; set; }
    [Prop] public bool Filled { get; set; } = false;
    [Prop] public Colors? Fill { get; set; } = null;
    [Prop] public Colors? Stroke { get; set; } = null;
    [Prop] public int StrokeWidth { get; set; } = 2;
    [Prop] public string? StrokeDashArray { get; set; }
    [Prop] public bool ShowSymbol { get; set; } = true;
    [Prop] public LegendTypes LegendType { get; set; } = LegendTypes.Line;
    [Prop] public LabelList[] LabelLists { get; set; } = [];
}

public static class RadarExtensions
{
    public static Radar Filled(this Radar radar, bool filled = true)
        => radar with { Filled = filled };

    public static Radar Fill(this Radar radar, Colors fill)
        => radar with { Fill = fill };

    public static Radar Stroke(this Radar radar, Colors stroke)
        => radar with { Stroke = stroke };

    public static Radar StrokeWidth(this Radar radar, int strokeWidth)
        => radar with { StrokeWidth = strokeWidth };

    public static Radar StrokeDashArray(this Radar radar, string strokeDashArray)
        => radar with { StrokeDashArray = strokeDashArray };

    public static Radar ShowSymbol(this Radar radar, bool showSymbol = true)
        => radar with { ShowSymbol = showSymbol };

    public static Radar LegendType(this Radar radar, LegendTypes legendType)
        => radar with { LegendType = legendType };

    public static Radar LabelList(this Radar radar, LabelList labelList)
        => radar with { LabelLists = [.. radar.LabelLists, labelList] };
}
