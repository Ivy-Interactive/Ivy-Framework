using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A chart that displays data points on a two-dimensional coordinate system, optionally with size and color encoding.
/// </summary>
public record ScatterChart : WidgetBase<ScatterChart>
{
    public ScatterChart(object data, params Scatter[] scatters) : this()
    {
        Data = data;
        Scatters = scatters;
    }

    internal ScatterChart()
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public object? Data { get; init; }

    [Prop] public ColorScheme ColorScheme { get; init; } = ColorScheme.Default;

    [Prop] public Scatter[] Scatters { get; init; } = [];

    [Prop] public CartesianGrid? CartesianGrid { get; init; }

    [Prop] public ChartTooltip? Tooltip { get; init; }

    [Prop] public Legend? Legend { get; init; } = null;

    [Prop] public Toolbox? Toolbox { get; init; } = null;

    [Prop] public XAxis[] XAxis { get; init; } = [];

    [Prop] public YAxis[] YAxis { get; init; } = [];

    [Prop] public ZAxis? ZAxis { get; init; } = null;

    [Prop] public ReferenceArea[] ReferenceAreas { get; init; } = [];

    [Prop] public ReferenceDot[] ReferenceDots { get; init; } = [];

    [Prop] public ReferenceLine[] ReferenceLines { get; init; } = [];

    public static ScatterChart operator |(ScatterChart widget, object child)
    {
        throw new NotSupportedException("ScatterChart does not support children.");
    }
}

public static partial class ScatterChartExtensions
{
    public static ScatterChart Scatter(this ScatterChart chart, Scatter scatter)
    {
        return chart with { Scatters = [.. chart.Scatters, scatter] };
    }

    public static ScatterChart Scatter(this ScatterChart chart, params Scatter[] scatters)
    {
        return chart with { Scatters = [.. chart.Scatters, .. scatters] };
    }

    public static ScatterChart Scatter(this ScatterChart chart, string dataKey, string? name = null)
    {
        return chart with { Scatters = [.. chart.Scatters, new Scatter(dataKey, name ?? StringHelper.SplitPascalCase(dataKey))] };
    }

    public static ScatterChart CartesianGrid(this ScatterChart chart, CartesianGrid cartesianGrid)
    {
        return chart with { CartesianGrid = cartesianGrid };
    }

    public static ScatterChart CartesianGrid(this ScatterChart chart)
    {
        return chart with { CartesianGrid = new CartesianGrid() };
    }

    public static ScatterChart XAxis(this ScatterChart chart, XAxis xAxis)
    {
        return chart with { XAxis = [.. chart.XAxis, xAxis] };
    }

    public static ScatterChart XAxis(this ScatterChart chart, string dataKey)
    {
        return chart with { XAxis = [.. chart.XAxis, new XAxis(dataKey)] };
    }

    public static ScatterChart YAxis(this ScatterChart chart, YAxis yAxis)
    {
        return chart with { YAxis = [.. chart.YAxis, yAxis] };
    }

    public static ScatterChart YAxis(this ScatterChart chart, string dataKey)
    {
        return chart with { YAxis = [.. chart.YAxis, new YAxis(dataKey)] };
    }

    public static ScatterChart YAxis(this ScatterChart chart)
    {
        return chart with { YAxis = [.. chart.YAxis, new YAxis()] };
    }

    public static ScatterChart ZAxis(this ScatterChart chart, ZAxis zAxis)
    {
        return chart with { ZAxis = zAxis };
    }

    public static ScatterChart ZAxis(this ScatterChart chart, string dataKey)
    {
        return chart with { ZAxis = new ZAxis(dataKey) };
    }

    public static ScatterChart Tooltip(this ScatterChart chart, ChartTooltip? tooltip)
    {
        return chart with { Tooltip = tooltip };
    }

    public static ScatterChart Tooltip(this ScatterChart chart)
    {
        return chart with { Tooltip = new ChartTooltip() };
    }

    public static ScatterChart Legend(this ScatterChart chart, Legend legend)
    {
        return chart with { Legend = legend };
    }

    public static ScatterChart Legend(this ScatterChart chart)
    {
        return chart with { Legend = new Legend() };
    }

    public static ScatterChart Toolbox(this ScatterChart chart, Toolbox? toolbox)
    {
        return chart with { Toolbox = toolbox };
    }

    public static ScatterChart Toolbox(this ScatterChart chart)
    {
        return chart with { Toolbox = new Toolbox() };
    }

    public static ScatterChart ReferenceArea(this ScatterChart chart, ReferenceArea referenceArea)
    {
        return chart with { ReferenceAreas = [.. chart.ReferenceAreas, referenceArea] };
    }

    public static ScatterChart ReferenceArea(this ScatterChart chart, double x1, double y1, double x2, double y2, string? label = null)
    {
        return chart with { ReferenceAreas = [.. chart.ReferenceAreas, new ReferenceArea(x1, y1, x2, y2, label)] };
    }

    public static ScatterChart ReferenceDot(this ScatterChart chart, ReferenceDot referenceDot)
    {
        return chart with { ReferenceDots = [.. chart.ReferenceDots, referenceDot] };
    }

    public static ScatterChart ReferenceDot(this ScatterChart chart, double x, double y, string? label = null)
    {
        return chart with { ReferenceDots = [.. chart.ReferenceDots, new ReferenceDot(x, y, label)] };
    }

    public static ScatterChart ReferenceLine(this ScatterChart chart, ReferenceLine referenceLine)
    {
        return chart with { ReferenceLines = [.. chart.ReferenceLines, referenceLine] };
    }

    public static ScatterChart ReferenceLine(this ScatterChart chart, double? x, double? y, string? label = null)
    {
        return chart with { ReferenceLines = [.. chart.ReferenceLines, new ReferenceLine(x, y, label)] };
    }

    public static ScatterChart ColorScheme(this ScatterChart chart, ColorScheme colorScheme)
    {
        return chart with { ColorScheme = colorScheme };
    }
}
