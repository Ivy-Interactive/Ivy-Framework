# Date Time

An input field to select or enter a date and time. Supports date-only, time-only, and combined date-time selection.

## Retool

```toolscript
dateTime1.setValue("2025-06-15T14:30:00")
dateTime1.clearValue()
dateTime1.setDisabled(true)
dateTime1.validate()
```

## Ivy

```csharp
var date = new State<DateTime>(DateTime.Now);

date.ToDateTimeInput()
    .Label("Select date and time")
    .Format("MM/dd/yyyy HH:mm")
    .Placeholder("Pick a date and time")
    .Nullable(true);

// Date-only variant
date.ToDateInput()
    .Label("Select date");

// Time-only variant
var time = new State<TimeOnly>(TimeOnly.FromDateTime(DateTime.Now));
time.ToTimeInput()
    .Label("Select time");
```

## Parameters

| Parameter                | Retool                                                                 | Ivy                                                                 |
|--------------------------|------------------------------------------------------------------------|---------------------------------------------------------------------|
| `disabled`               | `boolean` - Toggles whether input interaction is allowed               | `bool` - `.Disabled(true)` toggles input availability               |
| `readOnly`               | `boolean` - Prevents user modification of the field                    | Not supported                                                       |
| `required`               | `boolean` - Mandates a value selection before submission               | Partial - `.Invalid("message")` for validation display              |
| `showClear`              | `boolean` - Displays a button to empty the field                       | Not supported                                                       |
| `formattedValue`         | `string` - The selected value in specified format (read-only)          | `.Format("...")` controls display format via .NET format strings     |
| `value`                  | `string` - Current date-time value                                     | `TDate` - Generic typed value (`DateTime`, `DateOnly`, `TimeOnly`, etc.) |
| `placeholder`            | Not documented as property                                             | `string` - `.Placeholder("...")` hint text when empty               |
| `iconBefore`             | `string` - Prefix icon to display                                      | Not supported                                                       |
| `iconAfter`              | `string` - Suffix icon to display                                      | Not supported                                                       |
| `textBefore`             | `string` - Prefix text label                                           | Not supported                                                       |
| `textAfter`              | `string` - Suffix text label                                           | Not supported                                                       |
| `labelPosition`          | `string` - Label placement (`top` or `left`)                           | Not supported                                                       |
| `tooltipText`            | `string` - Helper text displayed on hover                              | Not supported                                                       |
| `label`                  | Built into component layout                                            | `.Label("...")` adds a descriptive label                            |
| `variant`                | Single date-time picker only                                           | `DateTimeInputs` enum - `Date`, `DateTime`, `Time` variants        |
| `nullable`               | Not documented                                                         | `bool` - `.Nullable(true)` allows null selection                    |
| `margin`                 | `string` - External spacing around component (`4px 8px` default)       | Not supported directly                                              |
| `style`                  | `object` - Custom styling options                                      | Not supported directly                                              |
| `visible`                | `isHiddenOnDesktop` / `isHiddenOnMobile` booleans                      | `bool` - `.Visible(true/false)` display toggle                      |
| `maintainSpaceWhenHidden`| `boolean` - Reserves layout space when hidden                          | Not supported                                                       |
| `height`                 | Not documented as property                                             | `Size` - `.Height(...)` vertical dimension                          |
| `width`                  | Not documented as property                                             | `Size` - `.Width(...)` horizontal dimension                         |
| `scale`                  | Not documented                                                         | `Scale?` - `.Scale(...)` sizing scale                               |
| `id`                     | `string` - Unique component identifier (`dateTime1` default)           | Not applicable - referenced via variable binding                    |

### Events

| Event       | Retool                                      | Ivy                                                        |
|-------------|---------------------------------------------|------------------------------------------------------------|
| `onChange`  | Change event fires when value is modified   | `OnChange` handler receives `Event<IInput<TDate>, TDate>`  |
| `onBlur`    | Blur event fires when field loses focus     | `OnBlur` triggered when focus leaves input                  |
| `onFocus`   | Focus event fires when field gains focus    | Not supported                                               |
| `onSubmit`  | Submit event fires when value is submitted  | Not supported                                               |

### Methods

| Method              | Retool                                                              | Ivy                          |
|---------------------|---------------------------------------------------------------------|------------------------------|
| `clearValidation()` | Removes validation error messages                                  | Not supported                |
| `clearValue()`      | Empties the current selection                                      | Set state to `null`          |
| `focus()`           | Sets keyboard focus on the component                               | Not supported                |
| `resetValue()`      | Restores the default value                                         | Reset state to initial value |
| `scrollIntoView()`  | Brings component into viewport with behavior/block options         | Not supported                |
| `setDisabled()`     | Controls disabled state                                            | `.Disabled(bool)`            |
| `setHidden()`       | Controls visibility                                                | `.Visible(bool)`             |
| `setValue()`        | Assigns a new value (string, number, boolean, object, or array)    | Set state value directly     |
| `validate()`        | Triggers validation checks                                         | `.Invalid("message")`        |
