using Ivy.Charts;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record PieChartTotal(string FormattedValue, string Label);

public record PieChart : WidgetBase<PieChart>
{
    public PieChart(object data)
    {
        Width = Size.Full();
        Height = Size.Full();
        Data = data;
    }

    [Prop] public object Data { get; init; }
    [Prop] public ColorScheme ColorScheme { get; init; } = ColorScheme.Default;
    [Prop] public Legend? Legend { get; init; } = null;
    [Prop] public Pie[] Pies { get; init; } = [];
    [Prop] public Ivy.Charts.Tooltip? Tooltip { get; init; }
    [Prop] public PieChartTotal? Total { get; init; }

    public static PieChart operator |(PieChart widget, object child)
    {
        throw new NotSupportedException("PieChart does not support children.");
    }
}

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