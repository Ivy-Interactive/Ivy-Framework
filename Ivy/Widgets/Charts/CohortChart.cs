using Ivy.Charts;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record CohortChart : WidgetBase<CohortChart>
{
    public CohortChart(object data)
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

public static class CohortChartExtensions
{
    public static CohortChart Title(this CohortChart chart, string title)
    {
        return chart with { Title = title };
    }

    public static CohortChart MetricType(this CohortChart chart, string metricType)
    {
        return chart with { MetricType = metricType };
    }

    public static CohortChart ShowTotal(this CohortChart chart, bool showTotal = true)
    {
        return chart with { ShowTotal = showTotal };
    }

    public static CohortChart Toolbox(this CohortChart chart, Toolbox toolbox)
    {
        return chart with { Toolbox = toolbox };
    }

    public static CohortChart Toolbox(this CohortChart chart)
    {
        return chart with { Toolbox = new Toolbox() };
    }
}
