using Ivy.Charts;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents a bar chart widget.
/// </summary>
public record BarChart : WidgetBase<BarChart>
{
    /// <summary>
    /// Initializes a new instance of the BarChart class.
    /// </summary>
    /// <param name="data">The data source.</param>
    /// <param name="bars">Variable number of Bar configurations.</param>
    public BarChart(object data, params Bar[] bars)
    {
        Data = data;
        Bars = bars;
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
    [Prop] public Layouts Layout { get; init; } = Layouts.Horizontal;

    /// <summary>
    /// Gets or sets the color scheme.
    /// </summary>
    [Prop] public ColorScheme ColorScheme { get; init; } = ColorScheme.Default;

    /// <summary>
    /// Gets or sets the array of Bar configurations.
    /// </summary>
    [Prop] public Bar[] Bars { get; init; }

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
    /// Gets or sets the gap between bars within the same category.
    /// </summary>
    [Prop] public int BarGap { get; init; } = 4;

    /// <summary>
    /// Gets or sets the gap between bar categories.
    /// </summary>
    [Prop] public object? BarCategoryGap { get; init; } = "10%";

    /// <summary>
    /// Gets or sets the maximum size.
    /// </summary>
    [Prop] public int? MaxBarSize { get; init; } = null;

    /// <summary>
    /// Gets or sets whether the stacking order of bars should be reversed.
    /// </summary>
    [Prop] public bool ReverseStackOrder { get; init; } = false;

    /// <summary>
    /// Operator overload that prevents BarChart from accepting child widgets.
    /// </summary>
    /// <param name="widget">The BarChart widget.</param>
    /// <param name="child">The child widget (not supported).</param>
    /// <returns>Throws NotSupportedException.</returns>
    /// <exception cref="NotSupportedException">BarChart does not support children.</exception>
    public static BarChart operator |(BarChart widget, object child)
    {
        throw new NotSupportedException("BarChart does not support children.");
    }
}

/// <summary>
/// Extension methods for the BarChart class.
/// </summary>
public static class BarChartExtensions
{
    /// <summary>
    /// Sets the layout orientation.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="layout">The layout orientation to use.</param>
    /// <returns>A new BarChart instance with the updated layout.</returns>
    public static BarChart Layout(this BarChart chart, Layouts layout)
    {
        return chart with { Layout = layout };
    }

    /// <summary>
    /// Sets the layout to horizontal.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <returns>A new BarChart instance with horizontal layout.</returns>
    public static BarChart Horizontal(this BarChart chart)
    {
        return chart with { Layout = Layouts.Horizontal };
    }

    /// <summary>
    /// Sets the layout to vertical.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <returns>A new BarChart instance with vertical layout.</returns>
    public static BarChart Vertical(this BarChart chart)
    {
        return chart with { Layout = Layouts.Vertical };
    }

    /// <summary>
    /// Adds a single Bar configuration to the existing bar list.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="bar">The Bar configuration to add.</param>
    /// <returns>A new BarChart instance with the additional bar configuration.</returns>
    public static BarChart Bar(this BarChart chart, Bar bar)
    {
        return chart with { Bars = [.. chart.Bars, bar] };
    }

    /// <summary>
    /// Adds multiple Bar configurations from an enumerable collection to the existing bar list.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="bars">The collection of Bar configurations to add.</param>
    /// <returns>A new BarChart instance with the additional bar configurations.</returns>
    public static BarChart Bar(this BarChart chart, params IEnumerable<Bar> bars)
    {
        return chart with { Bars = [.. chart.Bars, .. bars] };
    }

    /// <summary>
    /// Adds a simple bar configuration for the specified data key to the existing bar list.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="dataKey">The data property key to create a bar for.</param>
    /// <param name="stackId">Optional identifier for stacking multiple bars together.</param>
    /// <param name="name">Optional display name for the bar. If not provided, will be auto-generated from dataKey.</param>
    /// <returns>A new BarChart instance with the additional bar configuration.</returns>
    public static BarChart Bar(this BarChart chart, string dataKey, object? stackId = null, string? name = null)
    {
        return chart with { Bars = [.. chart.Bars, new Bar(dataKey, stackId?.ToString(), name ?? Utils.SplitPascalCase(dataKey))] };
    }

    /// <summary>
    /// Sets the Cartesian grid configuration.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="cartesianGrid">The CartesianGrid configuration to use.</param>
    /// <returns>A new BarChart instance with the updated Cartesian grid configuration.</returns>
    public static BarChart CartesianGrid(this BarChart chart, CartesianGrid cartesianGrid)
    {
        return chart with { CartesianGrid = cartesianGrid };
    }

    /// <summary>
    /// Enables the Cartesian grid.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <returns>A new BarChart instance with default Cartesian grid enabled.</returns>
    public static BarChart CartesianGrid(this BarChart chart)
    {
        return chart with { CartesianGrid = new CartesianGrid() };
    }

    /// <summary>
    /// Adds an X-axis configuration to the existing X-axis list.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="xAxis">The XAxis configuration to add.</param>
    /// <returns>A new BarChart instance with the additional X-axis configuration.</returns>
    public static BarChart XAxis(this BarChart chart, XAxis xAxis)
    {
        return chart with { XAxis = [.. chart.XAxis, xAxis] };
    }

    /// <summary>
    /// Adds a simple X-axis configuration for the specified data key to the existing X-axis list.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="dataKey">The data property key to create an X-axis for.</param>
    /// <returns>A new BarChart instance with the additional X-axis configuration.</returns>
    public static BarChart XAxis(this BarChart chart, string dataKey)
    {
        return chart with { XAxis = [.. chart.XAxis, new XAxis(dataKey)] };
    }

    /// <summary>
    /// Adds a Y-axis configuration to the existing Y-axis list.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="yAxis">The YAxis configuration to add.</param>
    /// <returns>A new BarChart instance with the additional Y-axis configuration.</returns>
    public static BarChart YAxis(this BarChart chart, YAxis yAxis)
    {
        return chart with { YAxis = [.. chart.YAxis, yAxis] };
    }

    /// <summary>
    /// Adds a simple Y-axis configuration for the specified data key to the existing Y-axis list.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="dataKey">The data property key to create a Y-axis for.</param>
    /// <returns>A new BarChart instance with the additional Y-axis configuration.</returns>
    public static BarChart YAxis(this BarChart chart, string dataKey)
    {
        return chart with { YAxis = [.. chart.YAxis, new YAxis(dataKey)] };
    }

    /// <summary>
    /// Adds a default Y-axis configuration to the existing Y-axis list.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <returns>A new BarChart instance with the additional default Y-axis configuration.</returns>
    public static BarChart YAxis(this BarChart chart)
    {
        return chart with { YAxis = [.. chart.YAxis, new YAxis()] };
    }

    /// <summary>
    /// Sets the tooltip configuration.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="tooltip">The Tooltip configuration to use, or null to disable tooltips.</param>
    /// <returns>A new BarChart instance with the updated tooltip configuration.</returns>
    public static BarChart Tooltip(this BarChart chart, Ivy.Charts.Tooltip? tooltip)
    {
        return chart with { Tooltip = tooltip };
    }

    /// <summary>
    /// Enables the tooltip.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <returns>A new BarChart instance with default tooltip enabled.</returns>
    public static BarChart Tooltip(this BarChart chart)
    {
        return chart with { Tooltip = new Ivy.Charts.Tooltip() };
    }

    /// <summary>
    /// Sets the legend configuration.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="legend">The Legend configuration to use.</param>
    /// <returns>A new BarChart instance with the updated legend configuration.</returns>
    public static BarChart Legend(this BarChart chart, Legend legend)
    {
        return chart with { Legend = legend };
    }

    /// <summary>
    /// Enables the legend.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <returns>A new BarChart instance with default legend enabled.</returns>
    public static BarChart Legend(this BarChart chart)
    {
        return chart with { Legend = new Legend() };
    }

    /// <summary>
    /// Adds a reference area configuration to the existing reference area list.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="referenceArea">The ReferenceArea configuration to add.</param>
    /// <returns>A new BarChart instance with the additional reference area configuration.</returns>
    public static BarChart ReferenceArea(this BarChart chart, ReferenceArea referenceArea)
    {
        return chart with { ReferenceAreas = [.. chart.ReferenceAreas, referenceArea] };
    }

    /// <summary>
    /// Adds a simple reference area configuration with the specified coordinates to the existing list.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="x1">The leftmost X coordinate of the reference area.</param>
    /// <param name="y1">The topmost Y coordinate of the reference area.</param>
    /// <param name="x2">The rightmost X coordinate of the reference area.</param>
    /// <param name="y2">The bottommost Y coordinate of the reference area.</param>
    /// <param name="label">Optional text label to display on or near the reference area.</param>
    /// <returns>A new BarChart instance with the additional reference area configuration.</returns>
    public static BarChart ReferenceArea(this BarChart chart, double x1, double y1, double x2, double y2, string? label = null)
    {
        return chart with { ReferenceAreas = [.. chart.ReferenceAreas, new ReferenceArea(x1, y1, x2, y2, label)] };
    }

    /// <summary>
    /// Adds a reference dot configuration to the existing reference dot list.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="referenceDot">The ReferenceDot configuration to add.</param>
    /// <returns>A new BarChart instance with the additional reference dot configuration.</returns>
    public static BarChart ReferenceDot(this BarChart chart, ReferenceDot referenceDot)
    {
        return chart with { ReferenceDots = [.. chart.ReferenceDots, referenceDot] };
    }

    /// <summary>
    /// Adds a simple reference dot configuration with the specified coordinates to the existing list.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="x">The X coordinate of the reference dot.</param>
    /// <param name="y">The Y coordinate of the reference dot.</param>
    /// <param name="label">Optional text label to display on or near the reference dot.</param>
    /// <returns>A new BarChart instance with the additional reference dot configuration.</returns>
    public static BarChart ReferenceDot(this BarChart chart, double x, double y, string? label = null)
    {
        return chart with { ReferenceDots = [.. chart.ReferenceDots, new ReferenceDot(x, y, label)] };
    }

    /// <summary>
    /// Adds a reference line configuration to the existing reference line list.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="referenceLine">The ReferenceLine configuration to add.</param>
    /// <returns>A new BarChart instance with the additional reference line configuration.</returns>
    public static BarChart ReferenceLine(this BarChart chart, ReferenceLine referenceLine)
    {
        return chart with { ReferenceLines = [.. chart.ReferenceLines, referenceLine] };
    }

    /// <summary>
    /// Adds a simple reference line configuration with the specified coordinates to the existing list.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="x">The X coordinate for a vertical reference line, or null for a horizontal line.</param>
    /// <param name="y">The Y coordinate for a horizontal reference line, or null for a vertical line.</param>
    /// <param name="label">Optional text label to display on or near the reference line.</param>
    /// <returns>A new BarChart instance with the additional reference line configuration.</returns>
    public static BarChart ReferenceLine(this BarChart chart, double? x, double? y, string? label = null)
    {
        return chart with { ReferenceLines = [.. chart.ReferenceLines, new ReferenceLine(x, y, label)] };
    }

    /// <summary>
    /// Sets the color scheme.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="colorScheme">The color scheme to use for the chart.</param>
    /// <returns>A new BarChart instance with the updated color scheme.</returns>
    public static BarChart ColorScheme(this BarChart chart, ColorScheme colorScheme)
    {
        return chart with { ColorScheme = colorScheme };
    }

    /// <summary>
    /// Sets the stack offset type.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="stackOffset">The stack offset type to use for positioning multiple bars.</param>
    /// <returns>A new BarChart instance with the updated stack offset configuration.</returns>
    public static BarChart StackOffset(this BarChart chart, StackOffsetTypes stackOffset)
    {
        return chart with { StackOffset = stackOffset };
    }

    /// <summary>
    /// Sets the gap between bars within the same category.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="barGap">The gap between bars in pixels.</param>
    /// <returns>A new BarChart instance with the updated bar gap configuration.</returns>
    public static BarChart BarGap(this BarChart chart, int barGap)
    {
        return chart with { BarGap = barGap };
    }

    /// <summary>
    /// Sets the gap between bar categories.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="barCategoryGap">The gap between categories in pixels.</param>
    /// <returns>A new BarChart instance with the updated category gap configuration.</returns>
    public static BarChart BarCategoryGap(this BarChart chart, int barCategoryGap)
    {
        return chart with { BarCategoryGap = barCategoryGap };
    }

    /// <summary>
    /// Sets the gap between bar categories.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="barCategoryGap">The gap between categories as a string (e.g., "10%", "20px").</param>
    /// <returns>A new BarChart instance with the updated category gap configuration.</returns>
    public static BarChart BarCategoryGap(this BarChart chart, string barCategoryGap)
    {
        return chart with { BarCategoryGap = barCategoryGap };
    }

    /// <summary>
    /// Sets the maximum size.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="maxBarSize">The maximum bar size in pixels.</param>
    /// <returns>A new BarChart instance with the updated maximum bar size configuration.</returns>
    public static BarChart MaxBarSize(this BarChart chart, int maxBarSize)
    {
        return chart with { MaxBarSize = maxBarSize };
    }

    /// <summary>
    /// Sets whether the stacking order of bars should be reversed.
    /// </summary>
    /// <param name="chart">The BarChart to configure.</param>
    /// <param name="reverseStackOrder">True to reverse the stacking order.</param>
    /// <returns>A new BarChart instance with the updated reverse stack order configuration.</returns>
    public static BarChart ReverseStackOrder(this BarChart chart, bool reverseStackOrder = true)
    {
        return chart with { ReverseStackOrder = reverseStackOrder };
    }
}

