# Calendar View

Odoo's calendar view for displaying records as events on a daily, weekly, monthly, or yearly calendar. Supports drag-and-drop rescheduling, quick event creation, and color coding by field values.

## Odoo

```python
class CalendarEvent(models.Model):
    _name = 'calendar.event'

    name = fields.Char(string='Meeting Subject', required=True)
    start = fields.Datetime(string='Start Date', required=True)
    stop = fields.Datetime(string='End Date', required=True)
    allday = fields.Boolean(string='All Day')
    duration = fields.Float(string='Duration')
    user_id = fields.Many2one('res.users', string='Responsible', default=lambda self: self.env.user)
    partner_ids = fields.Many2many('res.partner', string='Attendees')
    description = fields.Text(string='Description')
    location = fields.Char(string='Location')
    categ_ids = fields.Many2many('calendar.event.type', string='Tags')
```

```xml
<record id="view_calendar_event_calendar" model="ir.ui.view">
    <field name="name">calendar.event.calendar</field>
    <field name="model">calendar.event</field>
    <field name="arch" type="xml">
        <calendar string="Meetings" date_start="start" date_stop="stop"
                  color="user_id" mode="week" all_day="allday"
                  event_open_popup="true" quick_create="true"
                  form_view_id="%(calendar.view_calendar_event_form)d">
            <field name="name"/>
            <field name="partner_ids" avatar_field="avatar_128"/>
            <field name="location"/>
            <field name="categ_ids" filters="1" color="color"/>
        </calendar>
    </field>
</record>

<!-- Calendar with duration instead of stop date -->
<calendar string="Tasks" date_start="date_start" date_delay="planned_hours"
          color="project_id" mode="month" quick_create="false">
    <field name="name"/>
    <field name="user_ids" widget="many2many_avatar_user"/>
    <field name="project_id" filters="1"/>
</calendar>
```

## Ivy

```csharp
// Calendar view → Calendar widget with CalendarEvent items
var events = UseQuery(() => db.CalendarEvents
    .Select(e => new CalendarEvent
    {
        Id = e.Id.ToString(),
        Title = e.Name,
        Start = e.Start,
        End = e.Stop,
        AllDay = e.AllDay,
        Color = GetColorForUser(e.UserId),
    })
    .ToList());

new Calendar(events.Value)
    .OnEventClick(async ev => Navigate($"/event/{ev.Id}"))
    .OnDateSelect(async range => {
        // Quick create - open dialog for new event
        var newEvent = await ShowDialog<EventFormState>("New Event");
        if (newEvent != null)
            await api.CreateEvent(newEvent with { Start = range.Start, End = range.End });
    });

// With event details shown on cards
var calendarEvents = meetings.Value.Select(m => new CalendarEvent
{
    Id = m.Id.ToString(),
    Title = m.Name,
    Start = m.Start,
    End = m.Stop,
    AllDay = m.AllDay,
    Description = m.Location,
}).ToList();

new Calendar(calendarEvents);

// Filter by category/user with sidebar controls
var selectedUsers = UseState<int[]>([]);
var filteredEvents = events.Value
    .Where(e => selectedUsers.Value.Length == 0 || selectedUsers.Value.Contains(e.UserId))
    .ToList();

selectedUsers.ToSelectInput(users.Value.ToOptions(u => u.Id, u => u.Name))
    .WithField().Label("Filter by User");
new Calendar(filteredEvents);
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `<calendar>` | Root calendar view | `new Calendar(events)` |
| `date_start` | Event start date field | `CalendarEvent.Start` property |
| `date_stop` | Event end date field | `CalendarEvent.End` property |
| `date_delay` | Duration field (days) | Calculate `End` from `Start + Duration` |
| `all_day` | All-day event flag | `CalendarEvent.AllDay` property |
| `color` | Field for color segmentation | `CalendarEvent.Color` property with color mapping |
| `mode="day/week/month/year"` | Default display scale | Calendar default view configuration |
| `scales` | Available scale options | Calendar available views |
| `event_open_popup` | Open event in popup | `.OnEventClick()` handler opening Dialog |
| `quick_create` | Enable quick event creation | `.OnDateSelect()` handler |
| `quick_create_view_id` | Form for quick create | Dialog with custom form state |
| `form_view_id` | Form view for editing | Navigate to event detail page |
| `create="false"` | Disable event creation | Omit `.OnDateSelect()` handler |
| `<field filters="1">` | Add sidebar filter | `SelectInput` with collection state for filtering |
| `<field avatar_field="...">` | Show avatar instead of name | `Avatar` widget in event template |
| `string="..."` | View title | Page heading |
