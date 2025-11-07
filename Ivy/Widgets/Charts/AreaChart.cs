using Ivy.Charts;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents an area chart widget.
/// </summary>
public record AreaChart : WidgetBase<AreaChart>
{
    /// <summary>
    /// Initializes a new instance of the AreaChart class.
    /// </summary>
    /// <param name="data">The data source.</param>
    /// <param name="areas">Variable number of Area configurations.</param>
    public AreaChart(object data, params Area[] areas)
    {
        Data = data;
        Areas = areas;
        Width = Size.Full();
        Height = Size.Full();
    }

    /// <summary>
    /// Gets or sets the data source.
    /// </summary>
    [Prop] public object Data { get; init; }

    /// <summary>
    /// Gets or sets the layout orientation.
    /// Note: This property is not currently implemented on the frontend.
    /// </summary>
    [Prop] public Layouts Layout { get; init; } = Layouts.Vertical; //todo: not implemented on the frontend

    /// <summary>
    /// Gets or sets the color scheme.
    /// </summary>
    [Prop] public ColorScheme ColorScheme { get; init; } = ColorScheme.Default;

    /// <summary>
    /// Gets or sets the array of Area configurations.
    /// </summary>
    [Prop] public Area[] Areas { get; init; }

    /// <summary>
    /// Gets or sets the Cartesian grid configuration.
    /// </summary>
    [Prop] public CartesianGrid? CartesianGrid { get; init; }

    /// <summary>
    /// Gets or sets the tooltip configuration.
    /// </summary>
    [Prop] public Ivy.Charts.Tooltip? Tooltip { get; init; }

    /// <summary>
    /// Gets or sets the legend configuration.
    /// </summary>
    [Prop] public Legend? Legend { get; init; } = null;

    /// <summary>
    /// Gets or sets the toolbox configuration.
    /// </summary>
    [Prop] public Toolbox? Toolbox { get; init; } = null;
    /// <summary>
    /// Gets or sets the array of X-axis configurations.
    /// </summary>
    [Prop] public XAxis[] XAxis { get; init; } = [];

    /// <summary>
    /// Gets or sets the array of Y-axis configurations.
    /// </summary>
    [Prop] public YAxis[] YAxis { get; init; } = [];

    /// <summary>
    /// Gets or sets the array of reference area configurations.
    /// </summary>
    [Prop] public ReferenceArea[] ReferenceAreas { get; init; } = [];

    /// <summary>
    /// Gets or sets the array of reference dot configurations.
    /// </summary>
    [Prop] public ReferenceDot[] ReferenceDots { get; init; } = [];

    /// <summary>
    /// Gets or sets the array of reference line configurations.
    /// </summary>
    [Prop] public ReferenceLine[] ReferenceLines { get; init; } = [];

    /// <summary>
    /// Gets or sets the stack offset type.
    /// </summary>
    [Prop] public StackOffsetTypes StackOffset { get; init; } = StackOffsetTypes.None;

    /// <summary>
    /// Operator overload that prevents AreaChart from accepting child widgets.
    /// </summary>
    /// <param name="widget">The AreaChart widget.</param>
    /// <param name="child">The child widget (not supported).</param>
    /// <returns>Throws NotSupportedException.</returns>
    /// <exception cref="NotSupportedException">AreaChart does not support children.</exception>
    public static AreaChart operator |(AreaChart widget, object child)
    {
        throw new NotSupportedException("AreaChart does not support children.");
    }
}

/// <summary>
/// Extension methods for the AreaChart class.
/// </summary>
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

    public static AreaChart Toolbox(this AreaChart chart, Toolbox? toolbox)
    {
        return chart with { Toolbox = toolbox };
    }

    public static AreaChart Toolbox(this AreaChart chart)
    {
        return chart with { Toolbox = new Toolbox() };
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

