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
    /// <summary>
    /// Sets the layout orientation.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <param name="layout">The layout orientation to use.</param>
    /// <returns>A new AreaChart instance with the updated layout.</returns>
    public static AreaChart Layout(this AreaChart chart, Layouts layout)
    {
        return chart with { Layout = layout };
    }

    /// <summary>
    /// Sets the layout orientation to horizontal.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <returns>A new AreaChart instance with horizontal layout.</returns>
    public static AreaChart Horizontal(this AreaChart chart)
    {
        return chart with { Layout = Layouts.Horizontal };
    }

    /// <summary>
    /// Sets the layout orientation to vertical.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <returns>A new AreaChart instance with vertical layout.</returns>
    public static AreaChart Vertical(this AreaChart chart)
    {
        return chart with { Layout = Layouts.Vertical };
    }

    /// <summary>
    /// Adds one or more Area configurations to the existing area list.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <param name="area">Variable number of Area configurations to add.</param>
    /// <returns>A new AreaChart instance with the additional area configurations.</returns>
    public static AreaChart Area(this AreaChart chart, params Area[] area)
    {
        return chart with { Areas = [.. chart.Areas, .. area] };
    }

    /// <summary>
    /// Adds a simple area configuration for the specified data key to the existing area list.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <param name="dataKey">The data property key to create an area for.</param>
    /// <param name="stackId">Optional identifier for stacking multiple areas together.</param>
    /// <param name="name">Optional display name for the area. If not provided, will be auto-generated from dataKey.</param>
    /// <returns>A new AreaChart instance with the additional area configuration.</returns>
    public static AreaChart Area(this AreaChart chart, string dataKey, object? stackId = null, string? name = null)
    {
        return chart with { Areas = [.. chart.Areas, new Area(dataKey, stackId?.ToString(), name ?? Utils.SplitPascalCase(dataKey))] };
    }

    /// <summary>
    /// Sets the Cartesian grid configuration.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <param name="cartesianGrid">The CartesianGrid configuration to use.</param>
    /// <returns>A new AreaChart instance with the updated Cartesian grid configuration.</returns>
    public static AreaChart CartesianGrid(this AreaChart chart, CartesianGrid cartesianGrid)
    {
        return chart with { CartesianGrid = cartesianGrid };
    }

    /// <summary>
    /// Enables the Cartesian grid.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <returns>A new AreaChart instance with default Cartesian grid enabled.</returns>
    public static AreaChart CartesianGrid(this AreaChart chart)
    {
        return chart with { CartesianGrid = new CartesianGrid() };
    }

    /// <summary>
    /// Adds an X-axis configuration to the existing X-axis list.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <param name="xAxis">The XAxis configuration to add.</param>
    /// <returns>A new AreaChart instance with the additional X-axis configuration.</returns>
    public static AreaChart XAxis(this AreaChart chart, XAxis xAxis)
    {
        return chart with { XAxis = [.. chart.XAxis, xAxis] };
    }

    /// <summary>
    /// Adds a simple X-axis configuration for the specified data key to the existing X-axis list.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <param name="dataKey">The data property key to create an X-axis for.</param>
    /// <returns>A new AreaChart instance with the additional X-axis configuration.</returns>
    public static AreaChart XAxis(this AreaChart chart, string dataKey)
    {
        return chart with { XAxis = [.. chart.XAxis, new XAxis(dataKey)] };
    }

    /// <summary>
    /// Adds a Y-axis configuration to the existing Y-axis list.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <param name="yAxis">The YAxis configuration to add.</param>
    /// <returns>A new AreaChart instance with the additional Y-axis configuration.</returns>
    public static AreaChart YAxis(this AreaChart chart, YAxis yAxis)
    {
        return chart with { YAxis = [.. chart.YAxis, yAxis] };
    }

    /// <summary>
    /// Adds a simple Y-axis configuration for the specified data key to the existing Y-axis list.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <param name="dataKey">The data property key to create a Y-axis for.</param>
    /// <returns>A new AreaChart instance with the additional Y-axis configuration.</returns>
    public static AreaChart YAxis(this AreaChart chart, string dataKey)
    {
        return chart with { YAxis = [.. chart.YAxis, new YAxis(dataKey)] };
    }

    /// <summary>
    /// Sets the tooltip configuration.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <param name="tooltip">The Tooltip configuration to use, or null to disable tooltips.</param>
    /// <returns>A new AreaChart instance with the updated tooltip configuration.</returns>
    public static AreaChart Tooltip(this AreaChart chart, Ivy.Charts.Tooltip? tooltip)
    {
        return chart with { Tooltip = tooltip };
    }

    /// <summary>
    /// Enables the tooltip.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <returns>A new AreaChart instance with default tooltip enabled.</returns>
    public static AreaChart Tooltip(this AreaChart chart)
    {
        return chart with { Tooltip = new Ivy.Charts.Tooltip() };
    }

    /// <summary>
    /// Sets the legend configuration.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <param name="legend">The Legend configuration to use.</param>
    /// <returns>A new AreaChart instance with the updated legend configuration.</returns>
    public static AreaChart Legend(this AreaChart chart, Legend legend)
    {
        return chart with { Legend = legend };
    }

    /// <summary>
    /// Enables the legend.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <returns>A new AreaChart instance with default legend enabled.</returns>
    public static AreaChart Legend(this AreaChart chart)
    {
        return chart with { Legend = new Legend() };
    }

    /// <summary>
    /// Adds a reference area configuration to the existing reference area list.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <param name="referenceArea">The ReferenceArea configuration to add.</param>
    /// <returns>A new AreaChart instance with the additional reference area configuration.</returns>
    public static AreaChart ReferenceArea(this AreaChart chart, ReferenceArea referenceArea)
    {
        return chart with { ReferenceAreas = [.. chart.ReferenceAreas, referenceArea] };
    }

    /// <summary>
    /// Adds a simple reference area configuration with the specified coordinates to the existing list.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <param name="x1">The leftmost X coordinate of the reference area.</param>
    /// <param name="y1">The topmost Y coordinate of the reference area.</param>
    /// <param name="x2">The rightmost X coordinate of the reference area.</param>
    /// <param name="y2">The bottommost Y coordinate of the reference area.</param>
    /// <param name="label">Optional text label to display on or near the reference area.</param>
    /// <returns>A new AreaChart instance with the additional reference area configuration.</returns>
    public static AreaChart ReferenceArea(this AreaChart chart, double x1, double y1, double x2, double y2, string? label = null)
    {
        return chart with { ReferenceAreas = [.. chart.ReferenceAreas, new ReferenceArea(x1, y1, x2, y2, label)] };
    }

    /// <summary>
    /// Adds a reference dot configuration to the existing reference dot list.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <param name="referenceDot">The ReferenceDot configuration to add.</param>
    /// <returns>A new AreaChart instance with the additional reference dot configuration.</returns>
    public static AreaChart ReferenceDot(this AreaChart chart, ReferenceDot referenceDot)
    {
        return chart with { ReferenceDots = [.. chart.ReferenceDots, referenceDot] };
    }

    /// <summary>
    /// Adds a simple reference dot configuration with the specified coordinates to the existing list.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <param name="x">The X coordinate of the reference dot.</param>
    /// <param name="y">The Y coordinate of the reference dot.</param>
    /// <param name="label">Optional text label to display on or near the reference dot.</param>
    /// <returns>A new AreaChart instance with the additional reference dot configuration.</returns>
    public static AreaChart ReferenceDot(this AreaChart chart, double x, double y, string? label = null)
    {
        return chart with { ReferenceDots = [.. chart.ReferenceDots, new ReferenceDot(x, y, label)] };
    }

    /// <summary>
    /// Adds a reference line configuration to the existing reference line list.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <param name="referenceLine">The ReferenceLine configuration to add.</param>
    /// <returns>A new AreaChart instance with the additional reference line configuration.</returns>
    public static AreaChart ReferenceLine(this AreaChart chart, ReferenceLine referenceLine)
    {
        return chart with { ReferenceLines = [.. chart.ReferenceLines, referenceLine] };
    }

    /// <summary>
    /// Adds a simple reference line configuration with the specified coordinates to the existing list.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <param name="x">The X coordinate for a vertical reference line, or null for a horizontal line.</param>
    /// <param name="y">The Y coordinate for a horizontal reference line, or null for a vertical line.</param>
    /// <param name="label">Optional text label to display on or near the reference line.</param>
    /// <returns>A new AreaChart instance with the additional reference line configuration.</returns>
    public static AreaChart ReferenceLine(this AreaChart chart, double? x, double? y, string? label = null)
    {
        return chart with { ReferenceLines = [.. chart.ReferenceLines, new ReferenceLine(x, y, label)] };
    }

    /// <summary>
    /// Sets the color scheme.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <param name="colorScheme">The color scheme to use for the chart.</param>
    /// <returns>A new AreaChart instance with the updated color scheme.</returns>
    public static AreaChart ColorScheme(this AreaChart chart, ColorScheme colorScheme)
    {
        return chart with { ColorScheme = colorScheme };
    }

    /// <summary>
    /// Sets the stack offset type.
    /// </summary>
    /// <param name="chart">The AreaChart to configure.</param>
    /// <param name="stackOffset">The stack offset type to use for positioning multiple areas.</param>
    /// <returns>A new AreaChart instance with the updated stack offset configuration.</returns>
    public static AreaChart StackOffset(this AreaChart chart, StackOffsetTypes stackOffset)
    {
        return chart with { StackOffset = stackOffset };
    }
}

