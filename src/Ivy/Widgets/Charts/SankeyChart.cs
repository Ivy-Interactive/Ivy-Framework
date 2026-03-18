using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record SankeyData(SankeyNode[] Nodes, SankeyLink[] Links);

public record SankeyNode(string Name);

public record SankeyLink(int Source, int Target, double Value);

public enum SankeyAlign
{
    Justify,
    Left
}

/// <summary>
/// A flow visualization where arrow widths are proportional to flow quantities between nodes.
/// </summary>
public record SankeyChart : WidgetBase<SankeyChart>
{
    public SankeyChart(SankeyData data) : this()
    {
        Data = data;
    }

    internal SankeyChart()
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public SankeyData? Data { get; init; }
    [Prop] public ColorScheme ColorScheme { get; init; } = ColorScheme.Default;
    [Prop] public int NodeWidth { get; init; } = 20;
    [Prop] public int NodeGap { get; init; } = 8;
    [Prop] public double Curvature { get; init; } = 0.5;
    [Prop] public int LayoutIterations { get; init; } = 32;
    [Prop] public SankeyAlign NodeAlign { get; init; } = SankeyAlign.Justify;
    [Prop] public ChartTooltip? Tooltip { get; init; }
    [Prop] public Legend? Legend { get; init; } = null;
    [Prop] public Toolbox? Toolbox { get; init; } = null;

    public static SankeyChart operator |(SankeyChart widget, object child)
    {
        throw new NotSupportedException("SankeyChart does not support children.");
    }
}

public static partial class SankeyChartExtensions
{
    public static SankeyChart ColorScheme(this SankeyChart chart, ColorScheme colorScheme)
        => chart with { ColorScheme = colorScheme };

    public static SankeyChart NodeWidth(this SankeyChart chart, int nodeWidth)
        => chart with { NodeWidth = nodeWidth };

    public static SankeyChart NodeGap(this SankeyChart chart, int nodeGap)
        => chart with { NodeGap = nodeGap };

    public static SankeyChart Curvature(this SankeyChart chart, double curvature)
        => chart with { Curvature = curvature };

    public static SankeyChart LayoutIterations(this SankeyChart chart, int iterations)
        => chart with { LayoutIterations = iterations };

    public static SankeyChart NodeAlign(this SankeyChart chart, SankeyAlign align)
        => chart with { NodeAlign = align };

    public static SankeyChart Legend(this SankeyChart chart, Legend? legend)
        => chart with { Legend = legend };

    public static SankeyChart Legend(this SankeyChart chart)
        => chart with { Legend = new Legend() };

    public static SankeyChart Toolbox(this SankeyChart chart, Toolbox toolbox)
        => chart with { Toolbox = toolbox };

    public static SankeyChart Toolbox(this SankeyChart chart)
        => chart with { Toolbox = new Toolbox() };

    public static SankeyChart Tooltip(this SankeyChart chart, ChartTooltip tooltip)
        => chart with { Tooltip = tooltip };

    public static SankeyChart Tooltip(this SankeyChart chart)
        => chart with { Tooltip = new ChartTooltip() };
}
