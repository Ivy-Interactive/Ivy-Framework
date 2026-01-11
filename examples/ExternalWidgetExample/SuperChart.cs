using System.Runtime.CompilerServices;
using Ivy;
using Ivy.Core;
using Ivy.Core.ExternalWidgets;

namespace ExternalWidgetExample;

[ExternalWidget("frontend/dist/ExternalWidgets.js", ExportName = "SuperChart")]
public record SuperChart : WidgetBase<SuperChart>
{
    public SuperChart(string title, double[] data)
    {
        Title = title;
        Data = data;
    }

    internal SuperChart()
    {
    }

    [Prop] public string? Title { get; set; }

    [Prop] public double[] Data { get; set; } = [];

    [Prop] public string Color { get; set; } = "#3b82f6";

    [Prop] public bool ShowLabels { get; set; } = true;

    [Event] public Func<Event<SuperChart, int>, ValueTask>? OnPointClick { get; set; }
}

public static class SuperChartExtensions
{
    public static SuperChart Color(this SuperChart chart, string color) =>
        chart with { Color = color };

    public static SuperChart ShowLabels(this SuperChart chart, bool show = true) =>
        chart with { ShowLabels = show };

    public static SuperChart HandlePointClick(this SuperChart chart, Func<Event<SuperChart, int>, ValueTask> handler) =>
        chart with { OnPointClick = handler };

    public static SuperChart HandlePointClick(this SuperChart chart, Action<Event<SuperChart, int>> handler) =>
        chart with { OnPointClick = e => { handler(e); return ValueTask.CompletedTask; } };

    public static SuperChart HandlePointClick(this SuperChart chart, Action<int> handler) =>
        chart with { OnPointClick = e => { handler(e.Value); return ValueTask.CompletedTask; } };
}
