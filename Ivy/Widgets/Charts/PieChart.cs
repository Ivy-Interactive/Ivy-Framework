using Ivy.Charts;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents a total value display for pie charts.
/// </summary>
/// <param name="FormattedValue">The formatted string representation of the total value.</param>
/// <param name="Label">The descriptive label for the total value.</param>
public record PieChartTotal(string FormattedValue, string Label);

/// <summary>
/// Represents a pie chart widget.
/// </summary>
public record PieChart : WidgetBase<PieChart>
{
    /// <summary>
    /// Initializes a new instance of the PieChart class.
    /// </summary>
    /// <param name="data">The data source.</param>
    public PieChart(object data)
    {
        Width = Size.Full();
        Height = Size.Full();
        Data = data;
    }

    /// <summary>
    /// Gets or sets the data source.
    /// </summary>
    [Prop] public object Data { get; init; }

    /// <summary>
    /// Gets or sets the color scheme.
    /// </summary>
    [Prop] public ColorScheme ColorScheme { get; init; } = ColorScheme.Default;

    /// <summary>
    /// Gets or sets the legend configuration.
    /// </summary>
    [Prop] public Legend? Legend { get; init; } = null;

    /// <summary>
    /// Gets or sets the array of Pie configurations.
    /// </summary>
    [Prop] public Pie[] Pies { get; init; } = [];

    /// <summary>
    /// Gets or sets the tooltip configuration.
    /// </summary>
    [Prop] public Ivy.Charts.Tooltip? Tooltip { get; init; }

    /// <summary>
    /// Gets or sets the total value display configuration.
    /// </summary>
    [Prop] public PieChartTotal? Total { get; init; }

    /// <summary>
    /// Operator overload that prevents PieChart from accepting child widgets.
    /// </summary>
    /// <param name="widget">The PieChart widget.</param>
    /// <param name="child">The child widget (not supported).</param>
    /// <returns>Throws NotSupportedException.</returns>
    /// <exception cref="NotSupportedException">PieChart does not support children.</exception>
    public static PieChart operator |(PieChart widget, object child)
    {
        throw new NotSupportedException("PieChart does not support children.");
    }
}

/// <summary>
/// Each method returns a new PieChart instance with the updated configuration, following the immutable pattern.
/// </summary>
public static class PieChartExtensions
{
    /// <summary>
    /// Adds a Pie configuration.
    /// </summary>
    /// <param name="chart">The PieChart to configure.</param>
    /// <param name="pie">The Pie configuration to add.</param>
    /// <returns>A new PieChart instance with the additional pie configuration.</returns>
    public static PieChart Pie(this PieChart chart, Pie pie)
    {
        return chart with { Pies = [.. chart.Pies, pie] };
    }

    /// <summary>
    /// Adds a simple pie configuration.
    /// </summary>
    /// <param name="chart">The PieChart to configure.</param>
    /// <param name="dataKey">The data property key containing the numerical values for pie slices.</param>
    /// <param name="nameKey">The property key containing the names/labels for pie slices.</param>
    /// <returns>A new PieChart instance with the additional pie configuration.</returns>
    public static PieChart Pie(this PieChart chart, string dataKey, string nameKey)
    {
        return chart with { Pies = [.. chart.Pies, new Pie(dataKey, nameKey)] };
    }

    /// <summary>
    /// Sets the color scheme.
    /// </summary>
    /// <param name="chart">The PieChart to configure.</param>
    /// <param name="colorScheme">The color scheme to use for the chart.</param>
    /// <returns>A new PieChart instance with the updated color scheme.</returns>
    public static PieChart ColorScheme(this PieChart chart, ColorScheme colorScheme)
    {
        return chart with { ColorScheme = colorScheme };
    }

    /// <summary>
    /// Sets the legend configuration.
    /// </summary>
    /// <param name="chart">The PieChart to configure.</param>
    /// <param name="legend">The Legend configuration to use, or null to disable the legend.</param>
    /// <returns>A new PieChart instance with the updated legend configuration.</returns>
    public static PieChart Legend(this PieChart chart, Legend? legend)
    {
        return chart with { Legend = legend };
    }

    /// <summary>
    /// Enables the legend.
    /// </summary>
    /// <param name="chart">The PieChart to configure.</param>
    /// <returns>A new PieChart instance with default legend enabled.</returns>
    public static PieChart Legend(this PieChart chart)
    {
        return chart with { Legend = new Legend() };
    }

    /// <summary>
    /// Sets the tooltip configuration.
    /// </summary>
    /// <param name="chart">The PieChart to configure.</param>
    /// <param name="tooltip">The Tooltip configuration to use.</param>
    /// <returns>A new PieChart instance with the updated tooltip configuration.</returns>
    public static PieChart Tooltip(this PieChart chart, Ivy.Charts.Tooltip tooltip)
    {
        return chart with { Tooltip = tooltip };
    }

    /// <summary>
    /// Enables the tooltip.
    /// </summary>
    /// <param name="chart">The PieChart to configure.</param>
    /// <returns>A new PieChart instance with default tooltip enabled.</returns>
    public static PieChart Tooltip(this PieChart chart)
    {
        return chart with { Tooltip = new Ivy.Charts.Tooltip() };
    }

    /// <summary>
    /// Sets the total value display configuration.
    /// </summary>
    /// <param name="chart">The PieChart to configure.</param>
    /// <param name="pieChartTotal">The PieChartTotal configuration to use, or null to disable the total display.</param>
    /// <returns>A new PieChart instance with the updated total configuration.</returns>
    public static PieChart Total(this PieChart chart, PieChartTotal? pieChartTotal)
    {
        return chart with { Total = pieChartTotal };
    }

    /// <summary>
    /// Sets the total value display with a string value and label.
    /// </summary>
    /// <param name="chart">The PieChart to configure.</param>
    /// <param name="value">The string value to display as the total.</param>
    /// <param name="label">The descriptive label for the total value.</param>
    /// <returns>A new PieChart instance with the updated total configuration.</returns>
    public static PieChart Total(this PieChart chart, string value, string label)
    {
        return chart with { Total = new PieChartTotal(value, label) };
    }

    /// <summary>
    /// Sets the total value display with a numeric value and label.
    /// </summary>
    /// <param name="chart">The PieChart to configure.</param>
    /// <param name="value">The numeric value to display as the total.</param>
    /// <param name="label">The descriptive label for the total value.</param>
    /// <returns>A new PieChart instance with the updated total configuration.</returns>
    public static PieChart Total(this PieChart chart, double value, string label)
    {
        return chart with { Total = new PieChartTotal(value.ToString("N0"), label) };
    }
}