using Ivy;
using Ivy.Core;
using Ivy.Core.ExternalWidgets;

namespace Ivy.Widgets.Bryntum;

[ExternalWidget("frontend/dist/Ivy_Widgets_Bryntum.js", ExportName = "Gantt")]
public record Gantt : WidgetBase<Gantt>
{
    public Gantt(string? label = null)
    {
        Label = label;
    }

    internal Gantt()
    {
    }

    [Prop] public string? Label { get; set; }

    [Event] public Func<Event<Gantt>, ValueTask>? OnClick { get; set; }
}

public static class GanttExtensions
{
    public static Gantt Label(this Gantt widget, string label) =>
        widget with { Label = label };

    public static Gantt HandleClick(this Gantt widget, Func<Event<Gantt>, ValueTask> handler) =>
        widget with { OnClick = handler };

    public static Gantt HandleClick(this Gantt widget, Action<Event<Gantt>> handler) =>
        widget with { OnClick = e => { handler(e); return ValueTask.CompletedTask; } };

    public static Gantt HandleClick(this Gantt widget, Action handler) =>
        widget with { OnClick = _ => { handler(); return ValueTask.CompletedTask; } };
}
