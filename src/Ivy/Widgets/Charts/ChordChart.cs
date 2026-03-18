using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record ChordData(ChordNode[] Nodes, ChordLink[] Links);

public record ChordNode(string Name);

public record ChordLink(int Source, int Target, double Value);

/// <summary>
/// A circular visualization showing inter-relationships and flows between entities.
/// </summary>
public record ChordChart : WidgetBase<ChordChart>
{
    public ChordChart(ChordData data) : this()
    {
        Data = data;
    }

    internal ChordChart()
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public ChordData? Data { get; init; }
    [Prop] public ColorScheme ColorScheme { get; init; } = ColorScheme.Default;
    [Prop] public bool Sort { get; init; } = false;
    [Prop] public bool SortSubGroups { get; init; } = false;
    [Prop] public int PadAngle { get; init; } = 2;
    [Prop] public ChartTooltip? Tooltip { get; init; }
    [Prop] public Legend? Legend { get; init; } = null;
    [Prop] public Toolbox? Toolbox { get; init; } = null;

    public static ChordChart operator |(ChordChart widget, object child)
    {
        throw new NotSupportedException("ChordChart does not support children.");
    }
}

public static partial class ChordChartExtensions
{
    public static ChordChart ColorScheme(this ChordChart chart, ColorScheme colorScheme)
        => chart with { ColorScheme = colorScheme };

    public static ChordChart Sort(this ChordChart chart, bool sort = true)
        => chart with { Sort = sort };

    public static ChordChart SortSubGroups(this ChordChart chart, bool sortSubGroups = true)
        => chart with { SortSubGroups = sortSubGroups };

    public static ChordChart PadAngle(this ChordChart chart, int padAngle)
        => chart with { PadAngle = padAngle };

    public static ChordChart Legend(this ChordChart chart, Legend? legend)
        => chart with { Legend = legend };

    public static ChordChart Legend(this ChordChart chart)
        => chart with { Legend = new Legend() };

    public static ChordChart Toolbox(this ChordChart chart, Toolbox toolbox)
        => chart with { Toolbox = toolbox };

    public static ChordChart Toolbox(this ChordChart chart)
        => chart with { Toolbox = new Toolbox() };

    public static ChordChart Tooltip(this ChordChart chart, ChartTooltip tooltip)
        => chart with { Tooltip = tooltip };

    public static ChordChart Tooltip(this ChordChart chart)
        => chart with { Tooltip = new ChartTooltip() };
}
