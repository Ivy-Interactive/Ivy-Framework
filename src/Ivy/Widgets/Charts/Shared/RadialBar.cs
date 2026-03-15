
// ReSharper disable once CheckNamespace
namespace Ivy;

public record RadialBar
{
    public RadialBar(string dataKey, string? name = null)
    {
        DataKey = dataKey;
        Name = name;
    }

    internal RadialBar()
    {
    }

    [Prop] public string? DataKey { get; }
    [Prop] public string? Name { get; set; }
    [Prop] public int MinAngle { get; set; } = 0;
    [Prop] public bool Background { get; set; } = false;
    [Prop] public LegendTypes LegendType { get; set; } = LegendTypes.Line;
    [Prop] public bool Animated { get; set; } = true;
    [Prop] public LabelList[] LabelLists { get; set; } = [];
    [Prop] public Colors? Fill { get; set; } = null;
}

public static class RadialBarExtensions
{
    public static RadialBar MinAngle(this RadialBar bar, int minAngle)
        => bar with { MinAngle = minAngle };

    public static RadialBar Background(this RadialBar bar, bool background = true)
        => bar with { Background = background };

    public static RadialBar LegendType(this RadialBar bar, LegendTypes legendType)
        => bar with { LegendType = legendType };

    public static RadialBar Animated(this RadialBar bar, bool animated = true)
        => bar with { Animated = animated };

    public static RadialBar Fill(this RadialBar bar, Colors fill)
        => bar with { Fill = fill };

    public static RadialBar LabelList(this RadialBar bar, LabelList labelList)
        => bar with { LabelLists = [.. bar.LabelLists, labelList] };
}
