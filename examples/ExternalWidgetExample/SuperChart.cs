using System.Runtime.CompilerServices;
using Ivy;
using Ivy.Core;
using Ivy.Core.ExternalWidgets;

namespace ExternalWidgetExample;

/// <summary>
/// Example external widget that renders a custom chart.
/// The React component is bundled in frontend/dist/SuperChartWidget.js
/// </summary>
[ExternalWidget("frontend/dist/SuperChartWidget.js", ExportName = "SuperChartWidget")]
public record SuperChart : WidgetBase<SuperChart>
{
    public SuperChart(string title, double[] data)
    {
        Title = title;
        Data = data;
    }

    internal SuperChart() { }

    /// <summary>
    /// The chart title.
    /// </summary>
    [Prop]
    public string? Title { get; set; }

    /// <summary>
    /// The data points to display.
    /// </summary>
    [Prop]
    public double[] Data { get; set; } = [];

    /// <summary>
    /// The chart color (CSS color value).
    /// </summary>
    [Prop]
    public string Color { get; set; } = "#3b82f6";

    /// <summary>
    /// Whether to show data labels.
    /// </summary>
    [Prop]
    public bool ShowLabels { get; set; } = true;

    /// <summary>
    /// Event triggered when a data point is clicked.
    /// </summary>
    [Event]
    public Func<Event<SuperChart, int>, ValueTask>? OnPointClick { get; set; }
}

/// <summary>
/// Extension methods for SuperChart widget.
/// </summary>
public static class SuperChartExtensions
{
    public static SuperChart Color(this SuperChart chart, string color) =>
        chart with { Color = color };

    public static SuperChart ShowLabels(this SuperChart chart, bool show = true) =>
        chart with { ShowLabels = show };

    public static SuperChart OnPointClick(this SuperChart chart, Func<Event<SuperChart, int>, ValueTask> handler) =>
        chart with { OnPointClick = handler };

    public static SuperChart OnPointClick(this SuperChart chart, Action<Event<SuperChart, int>> handler) =>
        chart with { OnPointClick = e => { handler(e); return ValueTask.CompletedTask; } };

    public static SuperChart HandlePointClick(this SuperChart chart, Action<int> handler) =>
        chart with { OnPointClick = e => { handler(e.Value); return ValueTask.CompletedTask; } };
}
