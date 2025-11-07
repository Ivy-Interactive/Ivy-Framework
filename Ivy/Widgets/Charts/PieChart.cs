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
    /// Gets or sets the toolbox configuration.
    /// </summary>
    [Prop] public Toolbox? Toolbox { get; init; } = null;


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
/// Extension methods for the PieChart class.
/// </summary>
public static class PieChartExtensions
{
    public static PieChart Pie(this PieChart chart, Pie pie)
    {
        return chart with { Pies = [.. chart.Pies, pie] };
    }

    public static PieChart Pie(this PieChart chart, string dataKey, string nameKey)
    {
        return chart with { Pies = [.. chart.Pies, new Pie(dataKey, nameKey)] };
    }

    public static PieChart ColorScheme(this PieChart chart, ColorScheme colorScheme)
    {
        return chart with { ColorScheme = colorScheme };
    }

    public static PieChart Legend(this PieChart chart, Legend? legend)
    {
        return chart with { Legend = legend };
    }

    public static PieChart Legend(this PieChart chart)
    {
        return chart with { Legend = new Legend() };
    }

    public static PieChart Toolbox(this PieChart chart, Toolbox toolbox)
    {
        return chart with { Toolbox = toolbox };
    }

    public static PieChart Toolbox(this PieChart chart)
    {
        return chart with { Toolbox = new Toolbox() };
    }

    public static PieChart Tooltip(this PieChart chart, Ivy.Charts.Tooltip tooltip)
    {
        return chart with { Tooltip = tooltip };
    }

    public static PieChart Tooltip(this PieChart chart)
    {
        return chart with { Tooltip = new Ivy.Charts.Tooltip() };
    }

    public static PieChart Total(this PieChart chart, PieChartTotal? pieChartTotal)
    {
        return chart with { Total = pieChartTotal };
    }

    public static PieChart Total(this PieChart chart, string value, string label)
    {
        return chart with { Total = new PieChartTotal(value, label) };
    }

    public static PieChart Total(this PieChart chart, double value, string label)
    {
        return chart with { Total = new PieChartTotal(value.ToString("N0"), label) };
    }
}