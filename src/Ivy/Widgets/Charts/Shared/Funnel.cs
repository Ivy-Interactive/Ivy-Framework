
// ReSharper disable once CheckNamespace
namespace Ivy;

public record Funnel
{
    public Funnel(string dataKey, string nameKey)
    {
        DataKey = dataKey;
        NameKey = nameKey;
    }

    internal Funnel()
    {
    }

    public string? DataKey { get; }

    public string? NameKey { get; set; }

    public LegendTypes LegendType { get; set; } = LegendTypes.Line;

    public Colors? Fill { get; set; } = null;

    public double? FillOpacity { get; set; } = null;

    public Colors? Stroke { get; set; } = null;

    public int StrokeWidth { get; set; } = 1;

    public bool Animated { get; set; } = false;

    public string? MinSize { get; set; } = "0%";

    public string? MaxSize { get; set; } = "100%";

    public LabelList[] LabelLists { get; set; } = [];
}

public static class FunnelExtensions
{
    public static Funnel LegendType(this Funnel funnel, LegendTypes legendType)
    {
        return funnel with { LegendType = legendType };
    }

    public static Funnel Fill(this Funnel funnel, Colors fill)
    {
        return funnel with { Fill = fill };
    }

    public static Funnel FillOpacity(this Funnel funnel, double fillOpacity)
    {
        return funnel with { FillOpacity = fillOpacity };
    }

    public static Funnel Stroke(this Funnel funnel, Colors stroke)
    {
        return funnel with { Stroke = stroke };
    }

    public static Funnel StrokeWidth(this Funnel funnel, int strokeWidth)
    {
        return funnel with { StrokeWidth = strokeWidth };
    }

    public static Funnel Animated(this Funnel funnel, bool animated = true)
    {
        return funnel with { Animated = animated };
    }

    public static Funnel MinSize(this Funnel funnel, string minSize)
    {
        return funnel with { MinSize = minSize };
    }

    public static Funnel MaxSize(this Funnel funnel, string maxSize)
    {
        return funnel with { MaxSize = maxSize };
    }

    public static Funnel LabelLists(this Funnel funnel, LabelList[] labelLists)
    {
        return funnel with { LabelLists = labelLists };
    }

    public static Funnel LabelList(this Funnel funnel, LabelList labelList)
    {
        return funnel with { LabelLists = [.. funnel.LabelLists, labelList] };
    }

    public static Funnel LabelList(this Funnel funnel, string dataKey)
    {
        return funnel with { LabelLists = [.. funnel.LabelLists, new LabelList(dataKey)] };
    }
}
