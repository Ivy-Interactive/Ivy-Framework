using Ivy.Charts;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record AreaChart : WidgetBase<AreaChart>
{
    public AreaChart(object data, params Area[] areas)
    {
        Data = data;
        Areas = areas;
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public object Data { get; init; }
    [Prop] public Layouts Layout { get; init; } = Layouts.Vertical; //todo: not implemented on the frontend
    [Prop] public ColorScheme ColorScheme { get; init; } = ColorScheme.Default;
    [Prop] public Area[] Areas { get; init; }
    [Prop] public CartesianGrid? CartesianGrid { get; init; }
    [Prop] public Ivy.Charts.Tooltip? Tooltip { get; init; }
    [Prop] public Legend? Legend { get; init; } = null;
    [Prop] public XAxis[] XAxis { get; init; } = [];
    [Prop] public YAxis[] YAxis { get; init; } = [];
    [Prop] public ReferenceArea[] ReferenceAreas { get; init; } = [];
    [Prop] public ReferenceDot[] ReferenceDots { get; init; } = [];
    [Prop] public ReferenceLine[] ReferenceLines { get; init; } = [];
    [Prop] public StackOffsetTypes StackOffset { get; init; } = StackOffsetTypes.None;

    public static AreaChart operator |(AreaChart widget, object child)
    {
        throw new NotSupportedException("AreaChart does not support children.");
    }
}

public static class AreaChartExtensions
{
    public static AreaChart Layout(this AreaChart chart, Layouts layout)
    {
        return chart with { Layout = layout };
    }

    public static AreaChart Horizontal(this AreaChart chart)
    {
        return chart with { Layout = Layouts.Horizontal };
    }

    public static AreaChart Vertical(this AreaChart chart)
    {
        return chart with { Layout = Layouts.Vertical };
    }

    public static AreaChart Area(this AreaChart chart, params Area[] area)
    {
        return chart with { Areas = [.. chart.Areas, .. area] };
    }

    public static AreaChart Area(this AreaChart chart, string dataKey, object? stackId = null, string? name = null)
    {
        return chart with { Areas = [.. chart.Areas, new Area(dataKey, stackId?.ToString(), name ?? Utils.SplitPascalCase(dataKey))] };
    }

    public static AreaChart CartesianGrid(this AreaChart chart, CartesianGrid cartesianGrid)
    {
        return chart with { CartesianGrid = cartesianGrid };
    }

    public static AreaChart CartesianGrid(this AreaChart chart)
    {
        return chart with { CartesianGrid = new CartesianGrid() };
    }

    public static AreaChart XAxis(this AreaChart chart, XAxis xAxis)
    {
        return chart with { XAxis = [.. chart.XAxis, xAxis] };
    }

    public static AreaChart XAxis(this AreaChart chart, string dataKey)
    {
        return chart with { XAxis = [.. chart.XAxis, new XAxis(dataKey)] };
    }

    public static AreaChart YAxis(this AreaChart chart, YAxis yAxis)
    {
        return chart with { YAxis = [.. chart.YAxis, yAxis] };
    }

    public static AreaChart YAxis(this AreaChart chart, string dataKey)
    {
        return chart with { YAxis = [.. chart.YAxis, new YAxis(dataKey)] };
    }

    public static AreaChart Tooltip(this AreaChart chart, Ivy.Charts.Tooltip? tooltip)
    {
        return chart with { Tooltip = tooltip };
    }

    public static AreaChart Tooltip(this AreaChart chart)
    {
        return chart with { Tooltip = new Ivy.Charts.Tooltip() };
    }

    public static AreaChart Legend(this AreaChart chart, Legend legend)
    {
        return chart with { Legend = legend };
    }

    public static AreaChart Legend(this AreaChart chart)
    {
        return chart with { Legend = new Legend() };
    }

    public static AreaChart ReferenceArea(this AreaChart chart, ReferenceArea referenceArea)
    {
        return chart with { ReferenceAreas = [.. chart.ReferenceAreas, referenceArea] };
    }

    public static AreaChart ReferenceArea(this AreaChart chart, double x1, double y1, double x2, double y2, string? label = null)
    {
        return chart with { ReferenceAreas = [.. chart.ReferenceAreas, new ReferenceArea(x1, y1, x2, y2, label)] };
    }

    public static AreaChart ReferenceDot(this AreaChart chart, ReferenceDot referenceDot)
    {
        return chart with { ReferenceDots = [.. chart.ReferenceDots, referenceDot] };
    }

    public static AreaChart ReferenceDot(this AreaChart chart, double x, double y, string? label = null)
    {
        return chart with { ReferenceDots = [.. chart.ReferenceDots, new ReferenceDot(x, y, label)] };
    }

    public static AreaChart ReferenceLine(this AreaChart chart, ReferenceLine referenceLine)
    {
        return chart with { ReferenceLines = [.. chart.ReferenceLines, referenceLine] };
    }

    public static AreaChart ReferenceLine(this AreaChart chart, double? x, double? y, string? label = null)
    {
        return chart with { ReferenceLines = [.. chart.ReferenceLines, new ReferenceLine(x, y, label)] };
    }

    public static AreaChart ColorScheme(this AreaChart chart, ColorScheme colorScheme)
    {
        return chart with { ColorScheme = colorScheme };
    }

    public static AreaChart StackOffset(this AreaChart chart, StackOffsetTypes stackOffset)
    {
        return chart with { StackOffset = stackOffset };
    }
}

