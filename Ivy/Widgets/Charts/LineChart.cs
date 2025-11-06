using Ivy.Charts;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents a line chart widget.
/// </summary>
public record LineChart : WidgetBase<LineChart>
{
    /// <summary>
    /// Initializes a new instance of the LineChart class.
    /// </summary>
    /// <param name="data">The data source.</param>
    /// <param name="lines">Variable number of Line configurations.</param>
    public LineChart(object data, params Line[] lines)
    {
        Data = data;
        Lines = lines;
        Width = Size.Full();
        Height = Size.Full();
    }

    /// <summary>
    /// Initializes a new instance of the LineChart class with the specified data, data key, and name key.
    /// This constructor automatically creates a basic line configuration with default axes and tooltip.
    /// </summary>
    /// <param name="data">The data source containing the values to be displayed in the line chart.</param>
    /// <param name="dataKey">The key that identifies the data property containing the numerical values.</param>
    /// <param name="nameKey">The key that identifies the property containing the names/labels for data points.</param>
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

    /// <summary>
    /// Gets or sets the data source.
    /// </summary>
    [Prop] public object Data { get; init; }

    /// <summary>
    /// Gets or sets the layout orientation.
    /// </summary>
    [Prop] public Layouts Layout { get; init; } = Layouts.Vertical; //todo: not implemented on the frontend

    /// <summary>
    /// Gets or sets the color scheme.
    /// </summary>
    [Prop] public ColorScheme ColorScheme { get; init; } = ColorScheme.Default;

    /// <summary>
    /// Gets or sets the array of Line configurations.
    /// </summary>
    [Prop] public Line[] Lines { get; init; }

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
    [Prop] public Toolbox? Toolbox { get; init; } = new Toolbox();

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
    /// Operator overload that prevents LineChart from accepting child widgets.
    /// </summary>
    /// <param name="widget">The LineChart widget.</param>
    /// <param name="child">The child widget (not supported).</param>
    /// <returns>Throws NotSupportedException.</returns>
    /// <exception cref="NotSupportedException">LineChart does not support children.</exception>
    public static LineChart operator |(LineChart widget, object child)
    {
        throw new NotSupportedException("LineChart does not support children.");
    }
}

/// <summary>
/// Extension methods for the LineChart class.
/// </summary>
public static class LineChartExtensions
{
    public static LineChart Layout(this LineChart chart, Layouts layout)
    {
        return chart with { Layout = layout };
    }

    //todo: not implemented on the frontend
    public static LineChart Horizontal(this LineChart chart)
    {
        return chart with { Layout = Layouts.Horizontal };
    }

    //todo: not implemented on the frontend
    public static LineChart Vertical(this LineChart chart)
    {
        return chart with
        {
            Layout = Layouts.Vertical,
            Toolbox = new Toolbox()
        };
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

    public static LineChart Toolbox(this LineChart chart, Toolbox? toolbox)
    {
        return chart with { Toolbox = toolbox };
    }

    public static LineChart Toolbox(this LineChart chart)
    {
        return chart with { Toolbox = new Toolbox() };
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

