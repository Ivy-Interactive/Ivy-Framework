# Date and Time Inputs

A family of input components for selecting or entering dates and times. Retool provides eight specialized sub-components (Date, DateTime, Date Range, Time, Calendar Input, Day, Month, Year), while Ivy covers this with two widgets: `DateTimeInput` (with Date, DateTime, and Time variants) and `DateRangeInput`.

## Retool

```toolscript
// Date input
date1.setValue("2024-03-15")
date1.setDisabled(false)

// DateTime input
dateTime1.setValue("2024-03-15T14:30:00")

// Date Range input
dateRange1.setValue(["2024-03-01", "2024-03-31"])
dateRange1.setRange({ start: "2024-03-01", end: "2024-03-31" })

// Time input
time1.setValue("14:30:00")

// Calendar Input
calendarInput1.setValue("2024-03-15")

// Day / Month / Year (preset Select dropdowns)
day1.setValue(15)
month1.setValue(3)
year1.setValue(2024)
```

## Ivy

```csharp
// Date input (DateOnly)
var dateState = new State<DateOnly>(DateOnly.FromDateTime(DateTime.Today));
dateState.ToDateInput(placeholder: "Select a date");

// DateTime input
var dateTimeState = new State<DateTime>(DateTime.Now);
dateTimeState.ToDateTimeInput(placeholder: "Select date and time");

// Time input (TimeOnly)
var timeState = new State<TimeOnly>(TimeOnly.FromDateTime(DateTime.Now));
timeState.ToTimeInput(placeholder: "Select a time");

// Date Range input
var rangeState = new State<(DateOnly, DateOnly)>(
    (DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddDays(30)))
);
rangeState.ToDateRangeInput(placeholder: "Select date range");

// With custom format and validation
dateState.ToDateInput()
    .Format("MM/dd/yyyy")
    .Disabled(false)
    .Invalid("Date is required");
```

## Components

| Retool Component | Description                                          | Ivy Equivalent                                        |
|------------------|------------------------------------------------------|-------------------------------------------------------|
| Date             | Input field to select or enter a date                | `DateTimeInput` with `ToDateInput()` variant          |
| DateTime         | Input field to select or enter a date and time       | `DateTimeInput` with `ToDateTimeInput()` variant      |
| Time             | Input field to select or enter a time                | `DateTimeInput` with `ToTimeInput()` variant          |
| Date Range       | Input field to select or enter a date range          | `DateRangeInput` / `ToDateRangeInput()`               |
| Calendar Input   | Calendar-style date picker                           | Not supported (use `ToDateInput()`)                   |
| Day              | Dropdown to select a day (1-31)                      | Not supported                                         |
| Month            | Dropdown to select a month                           | Not supported                                         |
| Year             | Dropdown to select a year                            | Not supported                                         |

## Parameters

### Date / DateTime / Time

| Parameter          | Retool Documentation                                                         | Ivy                                                     |
|--------------------|------------------------------------------------------------------------------|----------------------------------------------------------|
| `value`            | Current selected value (read-only)                                           | `Value` (read-only on widget, via `State<T>`)            |
| `disabled`         | Prevents user interaction (`bool`, default `false`)                          | `Disabled` (`bool`)                                      |
| `placeholder`      | Text shown when field is empty (default `"Enter a value"`)                   | `Placeholder` (`string`)                                 |
| `required`         | Mandates value selection (`bool`, default `false`)                           | Not supported (use form-level validation)                |
| `readOnly`         | Prevents editing while visible (`bool`, default `false`)                     | Not supported                                            |
| `dateFormat`       | Specifies display format (string)                                            | `Format` (any .NET format string)                        |
| `minDate`          | Earliest selectable date                                                     | Not supported                                            |
| `maxDate`          | Latest selectable date                                                       | Not supported                                            |
| `firstDayOfWeek`   | Sets week start day (0=Sunday through 6=Saturday)                            | Not supported                                            |
| `showClear`        | Displays a button to clear the input (`bool`, default `false`)               | Not supported                                            |
| `labelPosition`    | Label placement: `top` or `left`                                             | `.Label()` / `.WithField()` (form integration)           |
| `tooltipText`      | Helper text on hover (supports Markdown)                                     | Not supported                                            |
| `iconBefore`       | Prefix icon                                                                  | Not supported                                            |
| `iconAfter`        | Suffix icon                                                                  | Not supported                                            |
| `textBefore`       | Prefix text label                                                            | Not supported                                            |
| `textAfter`        | Suffix text label                                                            | Not supported                                            |
| `formattedValue`   | Selected value in specified format (read-only)                               | Not supported (format via `Format` property)             |
| `invalid`          | N/A                                                                          | `Invalid` (`string`) - sets validation error message     |
| `nullable`         | N/A                                                                          | `Nullable` (`bool`) - enables null values                |
| `variant`          | Separate components per variant                                              | `Variant` (`DateTimeInputs` enum: Date, DateTime, Time)  |
| `margin`           | External spacing (default `"4px 8px"`)                                       | Not supported (use layout system)                        |
| `style`            | Custom styling options                                                       | Not supported                                            |
| `isHiddenOnDesktop`| Desktop visibility toggle (`bool`, default `false`)                          | `Visible` (`bool`)                                       |
| `isHiddenOnMobile` | Mobile visibility toggle (`bool`, default `true`)                            | `Visible` (`bool`)                                       |

### Date Range

| Parameter          | Retool Documentation                                                         | Ivy                                                     |
|--------------------|------------------------------------------------------------------------------|----------------------------------------------------------|
| `value`            | Selected date range as array (read-only)                                     | `Value` (`(DateOnly, DateOnly)` tuple)                   |
| `disabled`         | Prevents user interaction (`bool`, default `false`)                          | `Disabled` (`bool`)                                      |
| `placeholder`      | N/A (uses `startPlaceholder` / `endPlaceholder`)                             | `Placeholder` (`string`)                                 |
| `startPlaceholder` | Placeholder for start date field                                             | Not supported (single placeholder)                       |
| `endPlaceholder`   | Placeholder for end date field                                               | Not supported (single placeholder)                       |
| `textBetween`      | Text displayed between the two dates                                         | Not supported                                            |
| `dateFormat`       | Specifies how dates display                                                  | `Format` (any .NET format string)                        |
| `minDate`          | Earliest selectable date                                                     | Not supported                                            |
| `maxDate`          | Latest selectable date                                                       | Not supported                                            |
| `invalid`          | N/A                                                                          | `Invalid` (`string`) - sets validation error message     |

### Events

| Event              | Retool Documentation                                                         | Ivy                                                     |
|--------------------|------------------------------------------------------------------------------|----------------------------------------------------------|
| `Change`           | Triggered on value modification                                              | `OnChange` event handler                                 |
| `Blur`             | Triggered when field loses focus                                             | `OnBlur` event handler                                   |
| `Focus`            | Triggered when field receives focus                                          | Not supported                                            |
| `Submit`           | Triggered on value submission                                                | Not supported                                            |

### Methods

| Method               | Retool Documentation                                                       | Ivy                                                     |
|----------------------|----------------------------------------------------------------------------|----------------------------------------------------------|
| `setValue()`         | Programmatically sets the value                                            | Set via `State<T>.Value`                                 |
| `clearValue()`       | Empties current selection                                                  | Set state to default/null                                |
| `resetValue()`       | Restores default value                                                     | Not supported                                            |
| `focus()`            | Sets keyboard focus                                                        | Not supported                                            |
| `setDisabled()`      | Toggles disabled state                                                     | `.Disabled(bool)`                                        |
| `setHidden()`        | Toggles visibility                                                         | Not supported                                            |
| `validate()`         | Checks input validity                                                      | `.Invalid(string)`                                       |
| `clearValidation()`  | Removes validation messages                                                | `.Invalid(null)`                                         |
| `scrollIntoView()`   | Scrolls component into viewport                                            | Not supported                                            |
| `setRange()`         | Sets start and end dates (Date Range only)                                 | Set via `State<(DateOnly, DateOnly)>.Value`              |
