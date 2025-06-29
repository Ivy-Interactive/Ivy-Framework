using Ivy.Charts;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record BarChart : WidgetBase<BarChart>
{
    public BarChart(object data, params Bar[] bars)
    {
        Data = data;
        Bars = bars;
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public object Data { get; init; }
    [Prop] public Layouts Layout { get; init; } = Layouts.Horizontal;
    [Prop] public ColorScheme ColorScheme { get; init; } = ColorScheme.Default;
    [Prop] public Bar[] Bars { get; init; }
    [Prop] public CartesianGrid? CartesianGrid { get; init; }
    [Prop] public Ivy.Charts.Tooltip? Tooltip { get; init; }
    [Prop] public Legend? Legend { get; init; } = null;
    [Prop] public XAxis[] XAxis { get; init; } = [];
    [Prop] public YAxis[] YAxis { get; init; } = [];
    [Prop] public ReferenceArea[] ReferenceAreas { get; init; } = [];
    [Prop] public ReferenceDot[] ReferenceDots { get; init; } = [];
    [Prop] public ReferenceLine[] ReferenceLines { get; init; } = [];
    [Prop] public StackOffsetTypes StackOffset { get; init; } = StackOffsetTypes.None;
    [Prop] public int BarGap { get; init; } = 4;
    [Prop] public object? BarCategoryGap { get; init; } = "10%";
    [Prop] public int? MaxBarSize { get; init; } = null;
    [Prop] public bool ReverseStackOrder { get; init; } = false;

    public static BarChart operator |(BarChart widget, object child)
    {
        throw new NotSupportedException("BarChart does not support children.");
    }
}

public static class BarChartExtensions
{
    public static BarChart Layout(this BarChart chart, Layouts layout)
    {
        return chart with { Layout = layout };
    }

    public static BarChart Horizontal(this BarChart chart)
    {
        return chart with { Layout = Layouts.Horizontal };
    }

    public static BarChart Vertical(this BarChart chart)
    {
        return chart with { Layout = Layouts.Vertical };
    }

    public static BarChart Bar(this BarChart chart, Bar bar)
    {
        return chart with { Bars = [.. chart.Bars, bar] };
    }

    public static BarChart Bar(this BarChart chart, params IEnumerable<Bar> bars)
    {
        return chart with { Bars = [.. chart.Bars, .. bars] };
    }

    public static BarChart Bar(this BarChart chart, string dataKey, object? stackId = null, string? name = null)
    {
        return chart with { Bars = [.. chart.Bars, new Bar(dataKey, stackId?.ToString(), name ?? Utils.SplitPascalCase(dataKey))] };
    }

    public static BarChart CartesianGrid(this BarChart chart, CartesianGrid cartesianGrid)
    {
        return chart with { CartesianGrid = cartesianGrid };
    }

    public static BarChart CartesianGrid(this BarChart chart)
    {
        return chart with { CartesianGrid = new CartesianGrid() };
    }

    public static BarChart XAxis(this BarChart chart, XAxis xAxis)
    {
        return chart with { XAxis = [.. chart.XAxis, xAxis] };
    }

    public static BarChart XAxis(this BarChart chart, string dataKey)
    {
        return chart with { XAxis = [.. chart.XAxis, new XAxis(dataKey)] };
    }

    public static BarChart YAxis(this BarChart chart, YAxis yAxis)
    {
        return chart with { YAxis = [.. chart.YAxis, yAxis] };
    }

    public static BarChart YAxis(this BarChart chart, string dataKey)
    {
        return chart with { YAxis = [.. chart.YAxis, new YAxis(dataKey)] };
    }

    public static BarChart YAxis(this BarChart chart)
    {
        return chart with { YAxis = [.. chart.YAxis, new YAxis()] };
    }

    public static BarChart Tooltip(this BarChart chart, Ivy.Charts.Tooltip? tooltip)
    {
        return chart with { Tooltip = tooltip };
    }

    public static BarChart Tooltip(this BarChart chart)
    {
        return chart with { Tooltip = new Ivy.Charts.Tooltip() };
    }

    public static BarChart Legend(this BarChart chart, Legend legend)
    {
        return chart with { Legend = legend };
    }

    public static BarChart Legend(this BarChart chart)
    {
        return chart with { Legend = new Legend() };
    }

    public static BarChart ReferenceArea(this BarChart chart, ReferenceArea referenceArea)
    {
        return chart with { ReferenceAreas = [.. chart.ReferenceAreas, referenceArea] };
    }

    public static BarChart ReferenceArea(this BarChart chart, double x1, double y1, double x2, double y2, string? label = null)
    {
        return chart with { ReferenceAreas = [.. chart.ReferenceAreas, new ReferenceArea(x1, y1, x2, y2, label)] };
    }

    public static BarChart ReferenceDot(this BarChart chart, ReferenceDot referenceDot)
    {
        return chart with { ReferenceDots = [.. chart.ReferenceDots, referenceDot] };
    }

    public static BarChart ReferenceDot(this BarChart chart, double x, double y, string? label = null)
    {
        return chart with { ReferenceDots = [.. chart.ReferenceDots, new ReferenceDot(x, y, label)] };
    }

    public static BarChart ReferenceLine(this BarChart chart, ReferenceLine referenceLine)
    {
        return chart with { ReferenceLines = [.. chart.ReferenceLines, referenceLine] };
    }

    public static BarChart ReferenceLine(this BarChart chart, double? x, double? y, string? label = null)
    {
        return chart with { ReferenceLines = [.. chart.ReferenceLines, new ReferenceLine(x, y, label)] };
    }

    public static BarChart ColorScheme(this BarChart chart, ColorScheme colorScheme)
    {
        return chart with { ColorScheme = colorScheme };
    }

    public static BarChart StackOffset(this BarChart chart, StackOffsetTypes stackOffset)
    {
        return chart with { StackOffset = stackOffset };
    }

    public static BarChart BarGap(this BarChart chart, int barGap)
    {
        return chart with { BarGap = barGap };
    }

    public static BarChart BarCategoryGap(this BarChart chart, int barCategoryGap)
    {
        return chart with { BarCategoryGap = barCategoryGap };
    }

    public static BarChart BarCategoryGap(this BarChart chart, string barCategoryGap)
    {
        return chart with { BarCategoryGap = barCategoryGap };
    }

    public static BarChart MaxBarSize(this BarChart chart, int maxBarSize)
    {
        return chart with { MaxBarSize = maxBarSize };
    }

    public static BarChart ReverseStackOrder(this BarChart chart, bool reverseStackOrder = true)
    {
        return chart with { ReverseStackOrder = reverseStackOrder };
    }
}

