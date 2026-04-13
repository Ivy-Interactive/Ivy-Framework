
namespace Ivy.Samples.Shared.Apps.Widgets;

public record CalendarMeeting
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required DateTime Start { get; set; }
    public required DateTime End { get; set; }
    public required bool IsAllDay { get; set; }
    public required string Category { get; set; }
}

[App(icon: Icons.Calendar, group: ["Widgets"], searchHints: ["calendar", "events", "schedule"])]
public class CalendarApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("Basic Example", new BasicCalendarExample()),
            new Tab("Builder Example", new CalendarBuilderExample()),
            new Tab("Week View", new CalendarWeekExample()),
            new Tab("Agenda View", new CalendarAgendaExample()),
            new Tab("Event Handlers", new CalendarEventHandlerExample()),
            new Tab("Density", new CalendarDensityExample())
        ).Variant(TabsVariant.Content);
    }
}

public class BasicCalendarExample : ViewBase
{
    public override object? Build()
    {
        var today = DateTime.Today;

        return new Calendar(
            new CalendarEvent()
                .EventId("1").Title("Team Standup")
                .Start(today.AddHours(9))
                .End(today.AddHours(9).AddMinutes(30)),
            new CalendarEvent()
                .EventId("2").Title("Sprint Planning")
                .Start(today.AddDays(1).AddHours(10))
                .End(today.AddDays(1).AddHours(12))
                .Color("Blue"),
            new CalendarEvent()
                .EventId("3").Title("Company Retreat")
                .Start(today.AddDays(3))
                .End(today.AddDays(5))
                .AllDay(),
            new CalendarEvent()
                .EventId("4").Title("Code Review")
                .Start(today.AddHours(14))
                .End(today.AddHours(15))
                .Color("Green"),
            new CalendarEvent()
                .EventId("5").Title("Design Review")
                .Start(today.AddDays(2).AddHours(11))
                .End(today.AddDays(2).AddHours(12))
                .Color("Purple")
        );
    }
}

public class CalendarBuilderExample : ViewBase
{
    public override object? Build()
    {
        var meetings = UseState(() => CreateMeetings());

        return meetings.Value
            .ToCalendar(
                startSelector: m => m.Start,
                endSelector: m => m.End,
                eventIdSelector: m => m.Id)
            .Title(m => m.Title)
            .AllDay(m => m.IsAllDay)
            .Color(m => GetCategoryColor(m.Category))
            .EnableDragDrop()
            .OnMove(move =>
            {
                var updated = meetings.Value.Select(m =>
                {
                    if (m.Id == move.EventId?.ToString())
                        return m with { Start = move.Start, End = move.End };
                    return m;
                }).ToArray();
                meetings.Set(updated);
            });
    }

    private static string? GetCategoryColor(string category) => category switch
    {
        "Work" => "Blue",
        "Personal" => "Green",
        "Health" => "Red",
        _ => null
    };

    private static CalendarMeeting[] CreateMeetings()
    {
        var today = DateTime.Today;
        return
        [
            new() { Id = "1", Title = "Team Sync", Start = today.AddHours(9), End = today.AddHours(10), IsAllDay = false, Category = "Work" },
            new() { Id = "2", Title = "Gym", Start = today.AddHours(7), End = today.AddHours(8), IsAllDay = false, Category = "Health" },
            new() { Id = "3", Title = "Dentist", Start = today.AddDays(2).AddHours(14), End = today.AddDays(2).AddHours(15), IsAllDay = false, Category = "Personal" },
            new() { Id = "4", Title = "Sprint Review", Start = today.AddDays(4).AddHours(10), End = today.AddDays(4).AddHours(12), IsAllDay = false, Category = "Work" },
            new() { Id = "5", Title = "Holiday", Start = today.AddDays(7), End = today.AddDays(8), IsAllDay = true, Category = "Personal" },
            new() { Id = "6", Title = "Standup", Start = today.AddDays(1).AddHours(9), End = today.AddDays(1).AddHours(9).AddMinutes(15), IsAllDay = false, Category = "Work" },
        ];
    }
}

public class CalendarWeekExample : ViewBase
{
    public override object? Build()
    {
        var today = DateTime.Today;

        return new Calendar(
            new CalendarEvent()
                .EventId("1").Title("Morning Meeting")
                .Start(today.AddHours(9))
                .End(today.AddHours(10)),
            new CalendarEvent()
                .EventId("2").Title("Lunch Break")
                .Start(today.AddHours(12))
                .End(today.AddHours(13))
                .Color("Orange"),
            new CalendarEvent()
                .EventId("3").Title("Workshop")
                .Start(today.AddDays(1).AddHours(14))
                .End(today.AddDays(1).AddHours(16))
                .Color("Green"),
            new CalendarEvent()
                .EventId("4").Title("All Hands")
                .Start(today.AddDays(2))
                .End(today.AddDays(2).AddDays(1))
                .AllDay()
                .Color("Purple")
        ).DefaultView(CalendarDisplayMode.Week);
    }
}

public class CalendarAgendaExample : ViewBase
{
    public override object? Build()
    {
        var today = DateTime.Today;

        return new Calendar(
            new CalendarEvent()
                .EventId("1").Title("Team Standup")
                .Start(today.AddHours(9))
                .End(today.AddHours(9).AddMinutes(30)),
            new CalendarEvent()
                .EventId("2").Title("Sprint Planning")
                .Start(today.AddDays(1).AddHours(10))
                .End(today.AddDays(1).AddHours(12))
                .Color("Blue"),
            new CalendarEvent()
                .EventId("3").Title("Design Review")
                .Start(today.AddDays(2).AddHours(14))
                .End(today.AddDays(2).AddHours(15))
                .Color("Green"),
            new CalendarEvent()
                .EventId("4").Title("Team Offsite")
                .Start(today.AddDays(5))
                .End(today.AddDays(7))
                .AllDay()
                .Color("Purple"),
            new CalendarEvent()
                .EventId("5").Title("Release Review")
                .Start(today.AddDays(10).AddHours(15))
                .End(today.AddDays(10).AddHours(16))
                .Color("Red")
        ).DefaultView(CalendarDisplayMode.Agenda);
    }
}

public class CalendarEventHandlerExample : ViewBase
{
    public override object? Build()
    {
        var eventLog = UseState<string[]>(() => []);
        var events = UseState(() => CreateEvents());

        void Log(string message)
        {
            eventLog.Set([message, .. eventLog.Value.Take(9)]);
        }

        var calendar = new Calendar(events.Value)
            .EnableDragDrop()
            .DefaultView(CalendarDisplayMode.Week)
            .OnEventClick(eventId =>
            {
                Log($"Clicked: {eventId}");
            })
            .OnEventMove(move =>
            {
                Log($"Moved: {move.EventId} to {move.Start:HH:mm}");
                var updated = events.Value.Select(e =>
                {
                    if (e.EventId?.ToString() == move.EventId?.ToString())
                        return e.Start(move.Start).End(move.End);
                    return e;
                }).ToArray();
                events.Set(updated);
            })
            .OnSelectSlot(slot =>
            {
                Log($"Slot: {slot.Start:MMM dd HH:mm}");
            });

        return Layout.Horizontal()
            | (Layout.Vertical().Width(Size.Full())
                | calendar)
            | (Layout.Vertical().Width(Size.Rem(18))
                | Text.H3("Event Log")
                | new Card(
                    Layout.Vertical()
                        | eventLog.Value.Select(msg => (object)Text.Block(msg)).ToArray()
                ));
    }

    private static CalendarEvent[] CreateEvents()
    {
        var today = DateTime.Today;
        return
        [
            new CalendarEvent()
                .EventId("1").Title("Drag Me")
                .Start(today.AddHours(9)).End(today.AddHours(10))
                .Color("Blue"),
            new CalendarEvent()
                .EventId("2").Title("Click Me")
                .Start(today.AddHours(14)).End(today.AddHours(15))
                .Color("Green"),
            new CalendarEvent()
                .EventId("3").Title("All Day")
                .Start(today).End(today.AddDays(1))
                .AllDay(),
        ];
    }
}

public class CalendarDensityExample : ViewBase
{
    public override object? Build()
    {
        var today = DateTime.Today;

        CalendarEvent[] MakeEvents() =>
        [
            new CalendarEvent()
                .EventId("1").Title("Team Standup")
                .Start(today.AddHours(9))
                .End(today.AddHours(9).AddMinutes(30)),
            new CalendarEvent()
                .EventId("2").Title("Sprint Planning")
                .Start(today.AddDays(1).AddHours(10))
                .End(today.AddDays(1).AddHours(12))
                .Color("Blue"),
            new CalendarEvent()
                .EventId("3").Title("Company Retreat")
                .Start(today.AddDays(3))
                .End(today.AddDays(5))
                .AllDay()
                .Color("Purple"),
        ];

        return Layout.Horizontal().Gap(4)
            | (Layout.Vertical().Width(Size.Full())
                | Text.H4("Small")
                | new Calendar(MakeEvents()).Small())
            | (Layout.Vertical().Width(Size.Full())
                | Text.H4("Medium")
                | new Calendar(MakeEvents()).Medium())
            | (Layout.Vertical().Width(Size.Full())
                | Text.H4("Large")
                | new Calendar(MakeEvents()).Large());
    }
}
