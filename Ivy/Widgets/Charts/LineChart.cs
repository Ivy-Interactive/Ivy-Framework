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
    /// <summary>
    /// Sets the layout orientation.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <param name="layout">The layout orientation.</param>
    /// <returns>A new LineChart instance with the updated layout.</returns>
    public static LineChart Layout(this LineChart chart, Layouts layout)
    {
        return chart with { Layout = layout };
    }

    /// <summary>
    /// Sets the layout to horizontal.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <returns>A new LineChart instance with horizontal layout.</returns> //todo: not implemented on the frontend
    public static LineChart Horizontal(this LineChart chart)
    {
        return chart with { Layout = Layouts.Horizontal };
    }

    /// <summary>
    /// Sets the layout to vertical.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <returns>A new LineChart instance with vertical layout.</returns> //todo: not implemented on the frontend
    public static LineChart Vertical(this LineChart chart)
    {
        return chart with { Layout = Layouts.Vertical };
    }

    /// <summary>
    /// Adds one or more Line configurations.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <param name="lines">Variable number of Line configurations.</param>
    /// <returns>A new LineChart instance with the additional line configurations.</returns>
    public static LineChart Line(this LineChart chart, params Line[] lines)
    {
        return chart with { Lines = [.. chart.Lines, .. lines] };
    }

    /// <summary>
    /// Adds a simple line configuration.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <param name="dataKey">The data property key to create a line for.</param>
    /// <param name="name">Optional display name for the line.</param>
    /// <returns>A new LineChart instance with the additional line configuration.</returns>
    public static LineChart Line(this LineChart chart, string dataKey, string? name = null)
    {
        return chart with { Lines = [.. chart.Lines, new Line(dataKey, name ?? Utils.SplitPascalCase(dataKey))] };
    }

    /// <summary>
    /// Sets the Cartesian grid configuration.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <param name="cartesianGrid">The CartesianGrid configuration to use.</param>
    /// <returns>A new LineChart instance with the updated Cartesian grid configuration.</returns>
    public static LineChart CartesianGrid(this LineChart chart, CartesianGrid cartesianGrid)
    {
        return chart with { CartesianGrid = cartesianGrid };
    }

    /// <summary>
    /// Enables the Cartesian grid.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <returns>A new LineChart instance with default Cartesian grid enabled.</returns>
    public static LineChart CartesianGrid(this LineChart chart)
    {
        return chart with { CartesianGrid = new CartesianGrid() };
    }

    /// <summary>
    /// Adds an X-axis configuration.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <param name="xAxis">The XAxis configuration to add.</param>
    /// <returns>A new LineChart instance with the additional X-axis configuration.</returns>
    public static LineChart XAxis(this LineChart chart, XAxis xAxis)
    {
        return chart with { XAxis = [.. chart.XAxis, xAxis] };
    }

    /// <summary>
    /// Adds a simple X-axis configuration.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <param name="dataKey">The data property key to create an X-axis for.</param>
    /// <returns>A new LineChart instance with the additional X-axis configuration.</returns>
    public static LineChart XAxis(this LineChart chart, string dataKey)
    {
        return chart with { XAxis = [.. chart.XAxis, new XAxis(dataKey)] };
    }

    /// <summary>
    /// Adds a Y-axis configuration.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <param name="yAxis">The YAxis configuration to add.</param>
    /// <returns>A new LineChart instance with the additional Y-axis configuration.</returns>
    public static LineChart YAxis(this LineChart chart, YAxis yAxis)
    {
        return chart with { YAxis = [.. chart.YAxis, yAxis] };
    }

    /// <summary>
    /// Adds a simple Y-axis configuration.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <param name="dataKey">The data property key to create a Y-axis for.</param>
    /// <returns>A new LineChart instance with the additional Y-axis configuration.</returns>
    public static LineChart YAxis(this LineChart chart, string dataKey)
    {
        return chart with { YAxis = [.. chart.YAxis, new YAxis(dataKey)] };
    }

    /// <summary>
    /// Adds a default Y-axis configuration.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <returns>A new LineChart instance with the additional default Y-axis configuration.</returns>
    public static LineChart YAxis(this LineChart chart)
    {
        return chart with { YAxis = [.. chart.YAxis, new YAxis()] };
    }

    /// <summary>
    /// Sets the tooltip configuration.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <param name="tooltip">The Tooltip configuration to use, or null to disable tooltips.</param>
    /// <returns>A new LineChart instance with the updated tooltip configuration.</returns>
    public static LineChart Tooltip(this LineChart chart, Ivy.Charts.Tooltip? tooltip)
    {
        return chart with { Tooltip = tooltip };
    }

    /// <summary>
    /// Enables the tooltip.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <returns>A new LineChart instance with default tooltip enabled.</returns>
    public static LineChart Tooltip(this LineChart chart)
    {
        return chart with { Tooltip = new Ivy.Charts.Tooltip() };
    }

    /// <summary>
    /// Sets the legend configuration.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <param name="legend">The Legend configuration to use.</param>
    /// <returns>A new LineChart instance with the updated legend configuration.</returns>
    public static LineChart Legend(this LineChart chart, Legend legend)
    {
        return chart with { Legend = legend };
    }

    /// <summary>
    /// Enables the legend.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <returns>A new LineChart instance with default legend enabled.</returns>
    public static LineChart Legend(this LineChart chart)
    {
        return chart with { Legend = new Legend() };
    }

    /// <summary>
    /// Adds a reference area configuration.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <param name="referenceArea">The ReferenceArea configuration to add.</param>
    /// <returns>A new LineChart instance with the additional reference area configuration.</returns>
    public static LineChart ReferenceArea(this LineChart chart, ReferenceArea referenceArea)
    {
        return chart with { ReferenceAreas = [.. chart.ReferenceAreas, referenceArea] };
    }

    /// <summary>
    /// Adds a simple reference area configuration.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <param name="x1">The leftmost X coordinate of the reference area.</param>
    /// <param name="y1">The topmost Y coordinate of the reference area.</param>
    /// <param name="x2">The rightmost X coordinate of the reference area.</param>
    /// <param name="y2">The bottommost Y coordinate of the reference area.</param>
    /// <param name="label">Optional text label to display on or near the reference area.</param>
    /// <returns>A new LineChart instance with the additional reference area configuration.</returns>
    public static LineChart ReferenceArea(this LineChart chart, double x1, double y1, double x2, double y2, string? label = null)
    {
        return chart with { ReferenceAreas = [.. chart.ReferenceAreas, new ReferenceArea(x1, y1, x2, y2, label)] };
    }

    /// <summary>
    /// Adds a reference dot configuration.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <param name="referenceDot">The ReferenceDot configuration to add.</param>
    /// <returns>A new LineChart instance with the additional reference dot configuration.</returns>
    public static LineChart ReferenceDot(this LineChart chart, ReferenceDot referenceDot)
    {
        return chart with { ReferenceDots = [.. chart.ReferenceDots, referenceDot] };
    }

    /// <summary>
    /// Adds a simple reference dot configuration.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <param name="x">The X coordinate of the reference dot.</param>
    /// <param name="y">The Y coordinate of the reference dot.</param>
    /// <param name="label">Optional text label to display on or near the reference dot.</param>
    /// <returns>A new LineChart instance with the additional reference dot configuration.</returns>
    public static LineChart ReferenceDot(this LineChart chart, double x, double y, string? label = null)
    {
        return chart with { ReferenceDots = [.. chart.ReferenceDots, new ReferenceDot(x, y, label)] };
    }

    /// <summary>
    /// Adds a reference line configuration.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <param name="referenceLine">The ReferenceLine configuration to add.</param>
    /// <returns>A new LineChart instance with the additional reference line configuration.</returns>
    public static LineChart ReferenceLine(this LineChart chart, ReferenceLine referenceLine)
    {
        return chart with { ReferenceLines = [.. chart.ReferenceLines, referenceLine] };
    }

    /// <summary>
    /// Adds a simple reference line configuration.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <param name="x">The X coordinate for a vertical reference line, or null for a horizontal line.</param>
    /// <param name="y">The Y coordinate for a horizontal reference line, or null for a vertical line.</param>
    /// <param name="label">Optional text label to display on or near the reference line.</param>
    /// <returns>A new LineChart instance with the additional reference line configuration.</returns>
    public static LineChart ReferenceLine(this LineChart chart, double? x, double? y, string? label = null)
    {
        return chart with { ReferenceLines = [.. chart.ReferenceLines, new ReferenceLine(x, y, label)] };
    }

    /// <summary>
    /// Sets the color scheme.
    /// </summary>
    /// <param name="chart">The LineChart to configure.</param>
    /// <param name="colorScheme">The color scheme to use for the chart.</param>
    /// <returns>A new LineChart instance with the updated color scheme.</returns>
    public static LineChart ColorScheme(this LineChart chart, ColorScheme colorScheme)
    {
        return chart with { ColorScheme = colorScheme };
    }
}

