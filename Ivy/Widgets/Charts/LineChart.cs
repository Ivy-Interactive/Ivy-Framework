using Ivy.Charts;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record LineChart : WidgetBase<LineChart>
{
    public LineChart(object data, params Line[] lines)
    {
        Data = data;
        Lines = lines;
        Width = Size.Full();
        Height = Size.Full();
    }

    public LineChart(object data, string dataKey, string nameKey)
    {
        Data = data;
        Lines = [new Line(dataKey, Utils.SplitPascalCase(dataKey))];
        XAxis = [new XAxis(nameKey)];
        YAxis = [new YAxis(dataKey)];
        Tooltip = new();
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public object Data { get; init; }
    [Prop] public Layouts Layout { get; init; } = Layouts.Vertical; //todo: not implemented on the frontend
    [Prop] public ColorScheme ColorScheme { get; init; } = ColorScheme.Default;
    [Prop] public Line[] Lines { get; init; }
    [Prop] public CartesianGrid? CartesianGrid { get; init; }
    [Prop] public Ivy.Charts.Tooltip? Tooltip { get; init; }
    [Prop] public Legend? Legend { get; init; } = null;
    [Prop] public XAxis[] XAxis { get; init; } = [];
    [Prop] public YAxis[] YAxis { get; init; } = [];
    [Prop] public ReferenceArea[] ReferenceAreas { get; init; } = [];
    [Prop] public ReferenceDot[] ReferenceDots { get; init; } = [];
    [Prop] public ReferenceLine[] ReferenceLines { get; init; } = [];

    public static LineChart operator |(LineChart widget, object child)
    {
        throw new NotSupportedException("LineChart does not support children.");
    }
}

public static class LineChartExtensions
{
    public static LineChart Layout(this LineChart chart, Layouts layout)
    {
        return chart with { Layout = layout };
    }

    public static LineChart Horizontal(this LineChart chart)
    {
        return chart with { Layout = Layouts.Horizontal };
    }

    public static LineChart Vertical(this LineChart chart)
    {
        return chart with { Layout = Layouts.Vertical };
    }

    public static LineChart Line(this LineChart chart, params Line[] lines)
    {
        return chart with { Lines = [.. chart.Lines, .. lines] };
    }

    public static LineChart Line(this LineChart chart, string dataKey, string? name = null)
    {
        return chart with { Lines = [.. chart.Lines, new Line(dataKey, name ?? Utils.SplitPascalCase(dataKey))] };
    }

    public static LineChart CartesianGrid(this LineChart chart, CartesianGrid cartesianGrid)
    {
        return chart with { CartesianGrid = cartesianGrid };
    }

    public static LineChart CartesianGrid(this LineChart chart)
    {
        return chart with { CartesianGrid = new CartesianGrid() };
    }

    public static LineChart XAxis(this LineChart chart, XAxis xAxis)
    {
        return chart with { XAxis = [.. chart.XAxis, xAxis] };
    }

    public static LineChart XAxis(this LineChart chart, string dataKey)
    {
        return chart with { XAxis = [.. chart.XAxis, new XAxis(dataKey)] };
    }

    public static LineChart YAxis(this LineChart chart, YAxis yAxis)
    {
        return chart with { YAxis = [.. chart.YAxis, yAxis] };
    }

    public static LineChart YAxis(this LineChart chart, string dataKey)
    {
        return chart with { YAxis = [.. chart.YAxis, new YAxis(dataKey)] };
    }

    public static LineChart YAxis(this LineChart chart)
    {
        return chart with { YAxis = [.. chart.YAxis, new YAxis()] };
    }

    public static LineChart Tooltip(this LineChart chart, Ivy.Charts.Tooltip? tooltip)
    {
        return chart with { Tooltip = tooltip };
    }

    public static LineChart Tooltip(this LineChart chart)
    {
        return chart with { Tooltip = new Ivy.Charts.Tooltip() };
    }

    public static LineChart Legend(this LineChart chart, Legend legend)
    {
        return chart with { Legend = legend };
    }

    public static LineChart Legend(this LineChart chart)
    {
        return chart with { Legend = new Legend() };
    }

    public static LineChart ReferenceArea(this LineChart chart, ReferenceArea referenceArea)
    {
        return chart with { ReferenceAreas = [.. chart.ReferenceAreas, referenceArea] };
    }

    public static LineChart ReferenceArea(this LineChart chart, double x1, double y1, double x2, double y2, string? label = null)
    {
        return chart with { ReferenceAreas = [.. chart.ReferenceAreas, new ReferenceArea(x1, y1, x2, y2, label)] };
    }

    public static LineChart ReferenceDot(this LineChart chart, ReferenceDot referenceDot)
    {
        return chart with { ReferenceDots = [.. chart.ReferenceDots, referenceDot] };
    }

    public static LineChart ReferenceDot(this LineChart chart, double x, double y, string? label = null)
    {
        return chart with { ReferenceDots = [.. chart.ReferenceDots, new ReferenceDot(x, y, label)] };
    }

    public static LineChart ReferenceLine(this LineChart chart, ReferenceLine referenceLine)
    {
        return chart with { ReferenceLines = [.. chart.ReferenceLines, referenceLine] };
    }

    public static LineChart ReferenceLine(this LineChart chart, double? x, double? y, string? label = null)
    {
        return chart with { ReferenceLines = [.. chart.ReferenceLines, new ReferenceLine(x, y, label)] };
    }

    public static LineChart ColorScheme(this LineChart chart, ColorScheme colorScheme)
    {
        return chart with { ColorScheme = colorScheme };
    }
}

