using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum FunnelSort
{
    Descending,
    Ascending,
    None
}

public enum FunnelOrientation
{
    Vertical,
    Horizontal
}

/// <summary>
/// A funnel chart that visualizes data as progressively narrowing stages, useful for conversion rates and process flows.
/// </summary>
public record FunnelChart : WidgetBase<FunnelChart>
{
    public FunnelChart(object data) : this()
    {
        Data = data;
    }

    internal FunnelChart()
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public object? Data { get; init; }
    [Prop] public ColorScheme ColorScheme { get; init; } = ColorScheme.Default;
    [Prop] public Legend? Legend { get; init; } = null;
    [Prop] public Toolbox? Toolbox { get; init; } = null;
    [Prop] public Funnel[] Funnels { get; init; } = [];
    [Prop] public ChartTooltip? Tooltip { get; init; }
    [Prop] public FunnelSort Sort { get; init; } = FunnelSort.Descending;
    [Prop] public FunnelOrientation Orientation { get; init; } = FunnelOrientation.Vertical;
    [Prop] public int Gap { get; init; } = 0;

    public static FunnelChart operator |(FunnelChart widget, object child)
    {
        throw new NotSupportedException("FunnelChart does not support children.");
    }
}

public static partial class FunnelChartExtensions
{
    public static FunnelChart Funnel(this FunnelChart chart, Funnel funnel)
        => chart with { Funnels = [.. chart.Funnels, funnel] };

    public static FunnelChart Funnel(this FunnelChart chart, string dataKey, string nameKey)
        => chart with { Funnels = [.. chart.Funnels, new Funnel(dataKey, nameKey)] };

    public static FunnelChart ColorScheme(this FunnelChart chart, ColorScheme colorScheme)
        => chart with { ColorScheme = colorScheme };

    public static FunnelChart Legend(this FunnelChart chart, Legend? legend)
        => chart with { Legend = legend };

    public static FunnelChart Legend(this FunnelChart chart)
        => chart with { Legend = new Legend() };

    public static FunnelChart Toolbox(this FunnelChart chart, Toolbox toolbox)
        => chart with { Toolbox = toolbox };

    public static FunnelChart Toolbox(this FunnelChart chart)
        => chart with { Toolbox = new Toolbox() };

    public static FunnelChart Tooltip(this FunnelChart chart, ChartTooltip tooltip)
        => chart with { Tooltip = tooltip };

    public static FunnelChart Tooltip(this FunnelChart chart)
        => chart with { Tooltip = new ChartTooltip() };

    public static FunnelChart Sort(this FunnelChart chart, FunnelSort sort)
        => chart with { Sort = sort };

    public static FunnelChart Orientation(this FunnelChart chart, FunnelOrientation orientation)
        => chart with { Orientation = orientation };

    public static FunnelChart Gap(this FunnelChart chart, int gap)
        => chart with { Gap = gap };
}
