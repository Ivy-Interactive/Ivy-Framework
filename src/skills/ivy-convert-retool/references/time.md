# Time

An input field to select or enter a time value. Supports time zone display and automatic adjustment for time differences.

## Retool

```toolscript
// Basic usage - access the selected time value
time1.value

// Set a time value programmatically
time1.setValue("14:30:00");

// Clear or reset
time1.clearValue();
time1.resetValue();

// Disable the input
time1.setDisabled(true);

// Listen for changes via event handler
// Event: Change → Run Script
console.log(time1.value);
```

## Ivy

```csharp
// Time-only input using TimeOnly state
var time = new State<TimeOnly>(new TimeOnly(14, 30));
time.ToTimeInput();

// With placeholder and disabled
time.ToDateTimeInput()
    .Placeholder("Select time")
    .Variant(DateTimeInputs.Time);

// With change handler
var timeWithHandler = UseState(new TimeOnly(14, 30));
timeWithHandler.ToDateTimeInput();

// Using nullable TimeOnly
var nullableTime = new State<TimeOnly?>(null);
nullableTime.ToTimeInput();

// With custom format
time.ToTimeInput().Format("hh:mm tt");
```

## Parameters

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| value | Current time value | `Value` (read-only property on widget) |
| defaultValue | Initial default value | Initial value passed via `State<T>` constructor |
| disabled | `boolean` - disables input | `Disabled` property (`bool`) |
| placeholder | Placeholder text | `Placeholder` property (`string`) |
| format / formatMode | Display format string | `Format` property (supports .NET format strings e.g. `"hh:mm tt"`) |
| required | Whether input is required | Not supported |
| label | Label text | Not supported |
| labelPosition | Position of the label | Not supported |
| hideLabel | Hide the label | Not supported |
| tooltipText | Helper text on hover (markdown) | Not supported |
| minTime | Minimum selectable time | Not supported |
| maxTime | Maximum selectable time | Not supported |
| minuteStep | Minute increment step | Not supported |
| readOnly | Read-only mode | Not supported |
| loading | Show loading state | Not supported |
| timeZone | Time zone for display | Not supported (use `DateTimeOffset` for offset-aware values) |
| customValidation | Custom validation rule | `Invalid` property (`string` - validation message) |
| formDataKey | Key for form data | Not supported |
| iconBefore / iconAfter | Icons around input | Not supported |
| isHiddenOnDesktop | Hide on desktop layout | `Visible` property (`bool`) |
| isHiddenOnMobile | Hide on mobile layout | Not supported |
| maintainSpaceWhenHidden | Reserve space when hidden | Not supported |
| margin | External spacing | Not supported |
| style | Custom styling object | Not supported |
| id | Unique component identifier | Not applicable (C# variable reference) |
| **Events** | | |
| Change | Value is changed | `OnChange` event (`Event<IInput<TDate>, TDate>`) |
| Blur | Field loses focus | `OnBlur` event (`Event<IAnyInput>`) |
| Focus | Field gains focus | Not supported |
| Submit | Value is submitted | Not supported |
| **Methods** | | |
| setValue() | Set current value | Set via `State<T>.Value` |
| clearValue() | Clear current value | Set state to `default` / `null` (with nullable type) |
| resetValue() | Reset to default | Not supported (manage via state) |
| validate() | Validate input | Not supported (set `Invalid` property manually) |
| clearValidation() | Clear validation message | Set `Invalid` to `null` |
| focus() | Set focus on field | Not supported |
| setDisabled() | Toggle disabled state | Set `Disabled` property |
| setHidden() | Toggle visibility | Set `Visible` property |
| scrollIntoView() | Scroll component into view | Not supported |
