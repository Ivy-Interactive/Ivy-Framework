using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A circular bar chart displaying categorical data as concentric arcs radiating from a center point.
/// </summary>
public record RadialBarChart : WidgetBase<RadialBarChart>
{
    public RadialBarChart(object data) : this()
    {
        Data = data;
    }

    internal RadialBarChart()
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public object? Data { get; init; }
    [Prop] public ColorScheme ColorScheme { get; init; } = ColorScheme.Default;
    [Prop] public RadialBar[] RadialBars { get; init; } = [];
    [Prop] public ChartTooltip? Tooltip { get; init; }
    [Prop] public Legend? Legend { get; init; } = null;
    [Prop] public Toolbox? Toolbox { get; init; } = null;

    // Polar sub-components
    [Prop] public PolarAngleAxis? PolarAngleAxis { get; init; }
    [Prop] public PolarRadiusAxis? PolarRadiusAxis { get; init; }
    [Prop] public PolarGrid? PolarGrid { get; init; }

    // Center position
    [Prop] public object? Cx { get; init; } = "50%";
    [Prop] public object? Cy { get; init; } = "50%";

    // Radius
    [Prop] public object? InnerRadius { get; init; } = "30%";
    [Prop] public object? OuterRadius { get; init; } = "80%";

    // Angular range
    [Prop] public int StartAngle { get; init; } = 0;
    [Prop] public int EndAngle { get; init; } = 360;

    // Bar layout
    [Prop] public int BarGap { get; init; } = 4;
    [Prop] public object? BarCategoryGap { get; init; } = "10%";
    [Prop] public int? BarSize { get; init; } = null;

    public static RadialBarChart operator |(RadialBarChart widget, object child)
    {
        throw new NotSupportedException("RadialBarChart does not support children.");
    }
}

public static partial class RadialBarChartExtensions
{
    public static RadialBarChart RadialBar(this RadialBarChart chart, RadialBar bar)
        => chart with { RadialBars = [.. chart.RadialBars, bar] };

    public static RadialBarChart RadialBar(this RadialBarChart chart, string dataKey)
        => chart with { RadialBars = [.. chart.RadialBars, new RadialBar(dataKey)] };

    public static RadialBarChart ColorScheme(this RadialBarChart chart, ColorScheme colorScheme)
        => chart with { ColorScheme = colorScheme };

    public static RadialBarChart Legend(this RadialBarChart chart, Legend? legend)
        => chart with { Legend = legend };

    public static RadialBarChart Legend(this RadialBarChart chart)
        => chart with { Legend = new Legend() };

    public static RadialBarChart Toolbox(this RadialBarChart chart, Toolbox toolbox)
        => chart with { Toolbox = toolbox };

    public static RadialBarChart Toolbox(this RadialBarChart chart)
        => chart with { Toolbox = new Toolbox() };

    public static RadialBarChart Tooltip(this RadialBarChart chart, ChartTooltip tooltip)
        => chart with { Tooltip = tooltip };

    public static RadialBarChart Tooltip(this RadialBarChart chart)
        => chart with { Tooltip = new ChartTooltip() };

    public static RadialBarChart PolarAngleAxis(this RadialBarChart chart, PolarAngleAxis axis)
        => chart with { PolarAngleAxis = axis };

    public static RadialBarChart PolarRadiusAxis(this RadialBarChart chart, PolarRadiusAxis axis)
        => chart with { PolarRadiusAxis = axis };

    public static RadialBarChart PolarGrid(this RadialBarChart chart, PolarGrid grid)
        => chart with { PolarGrid = grid };

    public static RadialBarChart PolarGrid(this RadialBarChart chart)
        => chart with { PolarGrid = new PolarGrid() };

    public static RadialBarChart InnerRadius(this RadialBarChart chart, string innerRadius)
        => chart with { InnerRadius = innerRadius };

    public static RadialBarChart InnerRadius(this RadialBarChart chart, int innerRadius)
        => chart with { InnerRadius = innerRadius };

    public static RadialBarChart OuterRadius(this RadialBarChart chart, string outerRadius)
        => chart with { OuterRadius = outerRadius };

    public static RadialBarChart OuterRadius(this RadialBarChart chart, int outerRadius)
        => chart with { OuterRadius = outerRadius };

    public static RadialBarChart StartAngle(this RadialBarChart chart, int startAngle)
        => chart with { StartAngle = startAngle };

    public static RadialBarChart EndAngle(this RadialBarChart chart, int endAngle)
        => chart with { EndAngle = endAngle };

    public static RadialBarChart BarGap(this RadialBarChart chart, int barGap)
        => chart with { BarGap = barGap };
}
