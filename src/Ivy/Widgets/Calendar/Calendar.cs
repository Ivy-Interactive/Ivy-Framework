using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Calendar : WidgetBase<Calendar>
{
    public Calendar(params CalendarEvent[] events) : base([.. events])
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    internal Calendar()
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public CalendarDisplayMode DefaultView { get; set; } = CalendarDisplayMode.Month;
    [Prop] public DateTime? Date { get; set; }
    [Prop] public DateTime? MinDate { get; set; }
    [Prop] public DateTime? MaxDate { get; set; }
    [Prop] public bool ShowToolbar { get; set; } = true;
    [Prop] public bool EnableDragDrop { get; set; } = false;

    [Event] public EventHandler<Event<Calendar, (object? EventId, DateTime Start, DateTime End)>>? OnEventMove { get; set; }
    [Event] public EventHandler<Event<Calendar, (object? EventId, DateTime Start, DateTime End)>>? OnEventResize { get; set; }
    [Event] public EventHandler<Event<Calendar, object?>>? OnEventClick { get; set; }
    [Event] public EventHandler<Event<Calendar, (DateTime Start, DateTime End)>>? OnSelectSlot { get; set; }

    public static Calendar operator |(Calendar calendar, CalendarEvent child)
    {
        return calendar with { Children = [.. calendar.Children, child] };
    }
}

public enum CalendarDisplayMode
{
    Month,
    Week,
    Day,
    Agenda
}

public static class CalendarExtensions
{
    public static Calendar DefaultView(this Calendar calendar, CalendarDisplayMode view)
        => calendar with { DefaultView = view };

    public static Calendar Date(this Calendar calendar, DateTime date)
        => calendar with { Date = date };

    public static Calendar MinDate(this Calendar calendar, DateTime minDate)
        => calendar with { MinDate = minDate };

    public static Calendar MaxDate(this Calendar calendar, DateTime maxDate)
        => calendar with { MaxDate = maxDate };

    public static Calendar ShowToolbar(this Calendar calendar, bool show = true)
        => calendar with { ShowToolbar = show };

    public static Calendar EnableDragDrop(this Calendar calendar, bool enable = true)
        => calendar with { EnableDragDrop = enable };

    public static Calendar OnEventMove(this Calendar calendar, Action<(object? EventId, DateTime Start, DateTime End)> handler)
        => calendar with { OnEventMove = new EventHandler<Event<Calendar, (object? EventId, DateTime Start, DateTime End)>>(e => { handler(e.Value); return ValueTask.CompletedTask; }) };

    public static Calendar OnEventResize(this Calendar calendar, Action<(object? EventId, DateTime Start, DateTime End)> handler)
        => calendar with { OnEventResize = new EventHandler<Event<Calendar, (object? EventId, DateTime Start, DateTime End)>>(e => { handler(e.Value); return ValueTask.CompletedTask; }) };

    public static Calendar OnEventClick(this Calendar calendar, Action<object?> handler)
        => calendar with { OnEventClick = new EventHandler<Event<Calendar, object?>>(e => { handler(e.Value); return ValueTask.CompletedTask; }) };

    public static Calendar OnSelectSlot(this Calendar calendar, Action<(DateTime Start, DateTime End)> handler)
        => calendar with { OnSelectSlot = new EventHandler<Event<Calendar, (DateTime Start, DateTime End)>>(e => { handler(e.Value); return ValueTask.CompletedTask; }) };
}
