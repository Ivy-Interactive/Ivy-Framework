// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A multi-dimensional chart displaying data across multiple axes radiating from a center point, ideal for comparing profiles and skill assessments.
/// </summary>
public record RadarChart : WidgetBase<RadarChart>
{
    public RadarChart(object data) : this()
    {
        Data = data;
    }

    internal RadarChart()
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public object? Data { get; init; }
    [Prop] public ColorScheme ColorScheme { get; init; } = ColorScheme.Default;
    [Prop] public Radar[] Radars { get; init; } = [];
    [Prop] public RadarIndicator[] Indicators { get; init; } = [];
    [Prop] public ChartTooltip? Tooltip { get; init; }
    [Prop] public Legend? Legend { get; init; } = null;
    [Prop] public Toolbox? Toolbox { get; init; } = null;

    // Radar-specific configuration
    [Prop] public RadarShape Shape { get; init; } = RadarShape.Polygon;
    [Prop] public object? Cx { get; init; } = "50%";
    [Prop] public object? Cy { get; init; } = "50%";
    [Prop] public object? Radius { get; init; } = "75%";
    [Prop] public int StartAngle { get; init; } = 90;
    [Prop] public bool SplitLine { get; init; } = true;
    [Prop] public bool SplitArea { get; init; } = false;
    [Prop] public bool AxisLine { get; init; } = true;

    public static RadarChart operator |(RadarChart widget, object child)
    {
        throw new NotSupportedException("RadarChart does not support children.");
    }
}

public enum RadarShape
{
    Polygon,
    Circle
}

public static partial class RadarChartExtensions
{
    public static RadarChart Radar(this RadarChart chart, Radar radar)
        => chart with { Radars = [.. chart.Radars, radar] };

    public static RadarChart Radar(this RadarChart chart, string dataKey)
        => chart with { Radars = [.. chart.Radars, new Radar(dataKey)] };

    public static RadarChart Indicator(this RadarChart chart, RadarIndicator indicator)
        => chart with { Indicators = [.. chart.Indicators, indicator] };

    public static RadarChart Indicator(this RadarChart chart, string name, double? max = null)
        => chart with { Indicators = [.. chart.Indicators, new RadarIndicator(name) { Max = max }] };

    public static RadarChart ColorScheme(this RadarChart chart, ColorScheme colorScheme)
        => chart with { ColorScheme = colorScheme };

    public static RadarChart Legend(this RadarChart chart, Legend? legend)
        => chart with { Legend = legend };

    public static RadarChart Legend(this RadarChart chart)
        => chart with { Legend = new Legend() };

    public static RadarChart Toolbox(this RadarChart chart, Toolbox toolbox)
        => chart with { Toolbox = toolbox };

    public static RadarChart Toolbox(this RadarChart chart)
        => chart with { Toolbox = new Toolbox() };

    public static RadarChart Tooltip(this RadarChart chart, ChartTooltip tooltip)
        => chart with { Tooltip = tooltip };

    public static RadarChart Tooltip(this RadarChart chart)
        => chart with { Tooltip = new ChartTooltip() };

    public static RadarChart Shape(this RadarChart chart, RadarShape shape)
        => chart with { Shape = shape };

    public static RadarChart Radius(this RadarChart chart, string radius)
        => chart with { Radius = radius };

    public static RadarChart Radius(this RadarChart chart, int radius)
        => chart with { Radius = radius };

    public static RadarChart StartAngle(this RadarChart chart, int startAngle)
        => chart with { StartAngle = startAngle };

    public static RadarChart SplitLine(this RadarChart chart, bool splitLine = true)
        => chart with { SplitLine = splitLine };

    public static RadarChart SplitArea(this RadarChart chart, bool splitArea = true)
        => chart with { SplitArea = splitArea };

    public static RadarChart AxisLine(this RadarChart chart, bool axisLine = true)
        => chart with { AxisLine = axisLine };
}
