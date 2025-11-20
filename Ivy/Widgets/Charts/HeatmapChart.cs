using Ivy.Charts;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record HeatmapChart : WidgetBase<HeatmapChart>
{
    public HeatmapChart(object data)
    {
        Data = data;
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public object Data { get; init; }
    [Prop] public string? Title { get; init; }
    [Prop] public string MetricType { get; init; } = "number";
    [Prop] public bool ShowTotal { get; init; } = false;
    [Prop] public Toolbox? Toolbox { get; init; }
}

public static class HeatmapChartExtensions
{
    public static HeatmapChart Title(this HeatmapChart chart, string title)
    {
        return chart with { Title = title };
    }

    public static HeatmapChart MetricType(this HeatmapChart chart, string metricType)
    {
        return chart with { MetricType = metricType };
    }

    public static HeatmapChart ShowTotal(this HeatmapChart chart, bool showTotal = true)
    {
        return chart with { ShowTotal = showTotal };
    }

    public static HeatmapChart Toolbox(this HeatmapChart chart, Toolbox toolbox)
    {
        return chart with { Toolbox = toolbox };
    }

    public static HeatmapChart Toolbox(this HeatmapChart chart)
    {
        return chart with { Toolbox = new Toolbox() };
    }
}
