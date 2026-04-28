# Calendar

A content area to display calendar events. Retool's Calendar uses the [FullCalendar](https://fullcalendar.io/docs/event-object) library and supports day, week, month, year, and list views. Events can be created, edited, clicked, and removed interactively.

Ivy does not have a full event calendar widget. The closest equivalents are `DateTimeInput` (a date/time picker with calendar UI) and `DateRangeInput` (a date range picker). These cover date selection but not event display, drag-and-drop scheduling, or multi-view calendar layouts.

## Retool

```toolscript
// Calendar configured with mapped event data
calendar1.data = {{ query1.data }}
calendar1.viewType = "month"
calendar1.displayWeekends = true
calendar1.displayEventTime = true
calendar1.firstDayOfWeek = 0 // Sunday

// Access selected event
calendar1.selectedEvent
calendar1.selectedInterval

// Event handlers: Change Event, Click Event, Create Event, Remove Event
```

## Ivy

```csharp
// DateTimeInput — calendar picker for single date selection
var dateState = UseState(DateTime.Today);
dateState.ToDateInput()
    .Format("dd/MM/yyyy")
    .WithField()
    .Label("Select Date");

// DateRangeInput — calendar picker for date range selection
var rangeState = UseState((DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddDays(7))));
rangeState.ToDateRangeInput()
    .Format("yyyy-MM-dd")
    .WithField()
    .Label("Date Range");
```

## Parameters

| Parameter | Retool Documentation | Ivy |
|---|---|---|
| `data` | The event data source (array of event objects) | Not supported |
| `viewType` | The view type: `day`, `week`, `month` | Not supported |
| `view` | Read-only current view: `day`, `week`, `month`, `year`, `list` | Not supported |
| `listType` | Date range for list view: `day`, `week`, `month`, `year` | Not supported |
| `initialDate` | The initial date to display | `Value` — initial date via state |
| `firstDayOfWeek` | First day of the week (0=Sunday through 6=Saturday) | Not supported |
| `displayWeekends` | Whether to display weekends | Not supported |
| `displayEventTime` | Whether to display event times | Not supported |
| `displayAllDaySlot` | Whether to display the all-day event slot | Not supported |
| `displayTimeZone` | Time zone for display values | Not supported |
| `timeFormat` | The format of the time | `Format` — .NET date/time format string |
| `dayMaxEvents` | Max events per day in month view | Not supported |
| `eventMaxStack` | Max events in a stack | Not supported |
| `selectedEvent` | The currently selected event (read-only) | Not supported |
| `selectedInterval` | The selected time interval (read-only) | `Value` — selected date/range |
| `hoveredEvent` | The event under the cursor (read-only) | Not supported |
| `changeSet` | Edited values for each row (read-only) | Not supported |
| `titleByIndex` | List of event titles by index (read-only) | Not supported |
| `startByIndex` | List of start times by index (read-only) | Not supported |
| `endByIndex` | List of end times by index (read-only) | Not supported |
| `colorByIndex` | List of colors per event by index (read-only) | Not supported |
| `eventIdByIndex` | List of event IDs by index (read-only) | Not supported |
| `groupIdByIndex` | List of group IDs by index (read-only) | Not supported |
| `hidden` | Whether the component is hidden | `Visible` — controls visibility |
| `isHiddenOnDesktop` | Hide on desktop layout | Not supported |
| `isHiddenOnMobile` | Hide on mobile layout | Not supported |
| `maintainSpaceWhenHidden` | Keep layout space when hidden | Not supported |
| `showInEditor` | Show in editor even when hidden | Not supported |
| `margin` | Margin spacing (`4px 8px` or `0`) | Not supported (layout handled differently) |
| `style` | Custom style options | Not supported |
| `id` | Unique component identifier | Not supported (state-based identity) |
| `itemMode` | Options config mode: `dynamic` (mapped) or `static` (manual) | Not supported |
| `events` | Event handler configuration (Change, Click, Create, Remove) | `OnChange`, `OnBlur` — limited to value change and blur |
| `Disabled` | N/A (no direct equivalent) | `Disabled` — disables the input |
| `Placeholder` | N/A | `Placeholder` — hint text |
| `Invalid` | N/A | `Invalid` — validation error message |
| `Nullable` | N/A | `Nullable` — allows null values |
| `Scale` | N/A | `Scale` — sizing scale |
| `Width` / `Height` | Controlled via canvas layout | `Width`, `Height` — explicit sizing |

## Methods

| Method | Retool Documentation | Ivy |
|---|---|---|
| `scrollIntoView(options)` | Scrolls to bring the calendar into the visible area | Not supported |
| `setHidden(hidden)` | Toggles component visibility | `.Visible(bool)` — equivalent |

## Events

| Event | Retool Documentation | Ivy |
|---|---|---|
| Change Event | An event is changed (drag/resize) | `OnChange` — fires when selected date value changes |
| Click Event | An event is clicked | Not supported |
| Create Event | An event is created | Not supported |
| Remove Event | An event is removed | Not supported |
| N/A | N/A | `OnBlur` — fires when focus leaves the input |
