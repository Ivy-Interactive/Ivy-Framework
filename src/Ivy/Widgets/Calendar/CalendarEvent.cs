// ReSharper disable once CheckNamespace
namespace Ivy;

public record CalendarEvent : WidgetBase<CalendarEvent>
{
    public CalendarEvent(object? content = null) : base(content != null ? [content] : [])
    {
    }

    internal CalendarEvent() { }

    [Prop] public object? EventId { get; set; }
    [Prop] public string? Title { get; set; }
    [Prop] public DateTime Start { get; set; }
    [Prop] public DateTime End { get; set; }
    [Prop] public bool AllDay { get; set; } = false;
    [Prop] public string? Color { get; set; }
}

public static class CalendarEventExtensions
{
    public static CalendarEvent EventId(this CalendarEvent evt, object id)
        => evt with { EventId = id };

    public static CalendarEvent Title(this CalendarEvent evt, string title)
        => evt with { Title = title };

    public static CalendarEvent Start(this CalendarEvent evt, DateTime start)
        => evt with { Start = start };

    public static CalendarEvent End(this CalendarEvent evt, DateTime end)
        => evt with { End = end };

    public static CalendarEvent AllDay(this CalendarEvent evt, bool allDay = true)
        => evt with { AllDay = allDay };

    public static CalendarEvent Color(this CalendarEvent evt, string color)
        => evt with { Color = color };
}
