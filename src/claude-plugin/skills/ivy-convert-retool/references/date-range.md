# Date Range

An input field to select or enter a date range. Provides a calendar interface for selecting start and end dates, commonly used for filtering data by date ranges or scheduling events.

## Retool

```toolscript
// Read the current date range value
dateRange1.value; // e.g. ["2024-01-01", "2024-01-31"]

// Set a date range programmatically
dateRange1.setValue(["2024-01-01", "2024-01-31"]);

// Set start and end individually
dateRange1.setStartRange("2024-01-01");
dateRange1.setEndRange("2024-01-31");

// Clear the selection
dateRange1.clearValue();

// Validate the input
dateRange1.validate();
```

## Ivy

```csharp
// Bind to state
var dateRange = new State<(DateOnly, DateOnly)>((new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31)));
dateRange.ToDateRangeInput().Placeholder("Select date range");

// With change handler
var dateRangeWithHandler = UseState((new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31)));
dateRangeWithHandler.ToDateRangeInput();

// Nullable date range
var nullableDateRange = UseState<(DateOnly?, DateOnly?)>((null, null));
nullableDateRange.ToDateRangeInput().Placeholder("Select date range");
```

## Parameters

| Parameter        | Retool                                      | Ivy                        |
|------------------|---------------------------------------------|----------------------------|
| Value            | `value` - array of strings (read-only)      | `Value` - `TDateRange`     |
| Date Format      | `dateFormat` - controls display format       | `Format` - `string`        |
| Placeholder      | `startPlaceholder` / `endPlaceholder`        | `Placeholder` - `string`   |
| Text Between     | `textBetween` - text between start/end dates | Not supported              |
| Min Date         | `minDate` - earliest selectable date         | Not supported              |
| Max Date         | `maxDate` - latest selectable date           | Not supported              |
| Disabled         | `disabled` - prevents interaction            | `Disabled` - `bool`        |
| Hidden/Visible   | `hidden` - controls visibility               | `Visible` - `bool`         |
| Validation       | `validate()` method + `clearValidation()`    | `Invalid` - `string`       |
| Nullable         | N/A (inherently nullable)                    | `Nullable` - `bool`        |
| Width            | N/A (layout-based)                           | `Width` - `Size`           |
| Height           | N/A (layout-based)                           | `Height` - `Size`          |
| Scale            | N/A                                          | `Scale` - `Scale?`         |
| Margin           | `margin` - external spacing                  | Not supported              |
| ID               | `id` - unique identifier                     | Not supported              |
| Events: Change   | `Change` event                               | `OnChange` event           |
| Events: Focus    | `Focus` event                                | Not supported              |
| Events: Blur     | `Blur` event                                 | `OnBlur` event             |
| Events: Submit   | `Submit` event                               | Not supported              |
| clearValue()     | Removes current selection                    | Not supported              |
| setValue()       | Sets date range programmatically             | Via state binding          |
| setRange()       | Sets start and end dates                     | Not supported              |
| setStartRange()  | Sets only start date                         | Not supported              |
| setEndRange()    | Sets only end date                           | Not supported              |
| resetValue()     | Restores default value                       | Not supported              |
| focus()          | Activates the component                      | Not supported              |
| scrollIntoView() | Brings component into viewport               | Not supported              |
