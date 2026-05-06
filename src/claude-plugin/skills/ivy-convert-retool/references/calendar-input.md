# Calendar Input

An input field to select a specific date on a calendar. In Retool this is the `CalendarInput` component; in Ivy the closest equivalent is the `DateTimeInput` widget (specifically the `ToDateInput()` variant for date-only selection, or `ToDateTimeInput()` for date+time).

## Retool

```toolscript
// Read the selected date
const selectedDate = calendarInput1.value;

// Set a date programmatically
calendarInput1.setValue("2026-03-15");

// Clear the value
calendarInput1.clearValue();

// Disable the input
calendarInput1.setDisabled(true);

// Listen for changes via event handler
// Event: "Change" -> triggers configured action
```

## Ivy

```csharp
// Date-only picker (closest to Retool CalendarInput)
var date = new State<DateOnly>(DateOnly.FromDateTime(DateTime.Today));
date.ToDateInput()
    .Format("yyyy-MM-dd")
    .Placeholder("Select a date");

// Date+Time picker
var dateTime = new State<DateTime>(DateTime.Now);
dateTime.ToDateTimeInput()
    .Format("dd/MM/yyyy HH:mm")
    .Placeholder("Select date and time");

// With change handler
var dateWithHandler = UseState(DateOnly.FromDateTime(DateTime.Today));
dateWithHandler.ToDateInput();

// Disabled state
date.ToDateInput().Disabled(true);
```

## Parameters

| Parameter        | Retool                                                                 | Ivy                                                                 |
|------------------|------------------------------------------------------------------------|---------------------------------------------------------------------|
| value            | Current selected date string (read-only property)                      | `Value` - typed (`DateTime`, `DateOnly`, `DateTimeOffset`, etc.)    |
| disabled         | `boolean` - disables interaction. Default `false`                      | `Disabled(bool)` - disables interaction                             |
| readOnly         | `boolean` - makes input read-only. Default `false`                     | Not supported                                                       |
| required         | `boolean` - whether a value is required. Default `false`               | Not supported (use external validation)                             |
| minDate          | `string` - earliest date to allow                                      | Not supported                                                       |
| maxDate          | `string` - latest date to allow                                        | Not supported                                                       |
| firstDayOfWeek   | `string` - index 0-6, Sunday=0                                        | Not supported                                                       |
| format           | Not supported (display format not configurable)                        | `Format(string)` - .NET format string e.g. `"dd/MM/yyyy"`          |
| placeholder      | Not supported                                                          | `Placeholder(string)` - hint text shown when empty                  |
| nullable         | Not supported (value is always nullable string)                        | `Nullable(bool)` - allows null values for nullable types            |
| invalid          | Not supported (use validation rules)                                   | `Invalid(string)` - displays error message                          |
| tooltipText      | `string` (Markdown) - tooltip next to label on hover                   | Not supported                                                       |
| labelPosition    | `"top"` or `"left"` - label placement relative to input               | Not supported (use layout primitives)                               |
| visible/hidden   | `setHidden(bool)` - toggle visibility                                  | `Visible(bool)` - toggle visibility                                 |
| style            | `object` - custom CSS-like style options                               | Not supported (use Ivy styling primitives)                          |
| margin           | `"4px 8px"` (normal) or `"0"` (none)                                  | Not supported (use layout primitives)                               |
| width/height     | Controlled via canvas drag sizing                                      | `Width(Size)` / `Height(Size)`                                      |
| events           | `Change` event handler configured in IDE                               | `OnChange` / `OnBlur` event handlers                                |
| variant          | Single variant (calendar date picker)                                  | `ToDateInput()`, `ToDateTimeInput()`, `ToTimeInput()`               |
| setValue()       | `calendarInput.setValue(value)` - set value programmatically           | Set via state: `state.Set(newValue)`                                |
| clearValue()     | `calendarInput.clearValue()` - clear selection                         | Set state to default/null                                           |
| focus()          | `calendarInput.focus()` - focus the component                         | Not supported                                                       |
| scrollIntoView() | `calendarInput.scrollIntoView(options)` - scroll to component          | Not supported                                                       |
| scale            | Not supported                                                          | `Scale(Scale?)` - responsive scaling                                |
