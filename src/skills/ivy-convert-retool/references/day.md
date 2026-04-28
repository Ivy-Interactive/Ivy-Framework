# Day

A dropdown menu and input field to select or enter a day. In Retool this is a preset version of the Select component preconfigured with day values (Monday–Sunday).

## Retool

```toolscript
// Set a day value
day.setValue("Monday");

// Clear the selection
day.clearValue();

// Toggle disabled state
day.setDisabled(false);
```

## Ivy

Ivy has no dedicated Day widget. Use `SelectInput` with day options:

```csharp
var selectedDay = UseState("Monday");

var dayOptions = new[] {
    "Monday", "Tuesday", "Wednesday", "Thursday",
    "Friday", "Saturday", "Sunday"
}.ToOptions();

return selectedDay.ToSelectInput(dayOptions)
    .Placeholder("Enter a value")
    .WithField()
    .Label("Day");
```

## Parameters

| Parameter             | Documentation                                      | Ivy                                                        |
|-----------------------|----------------------------------------------------|------------------------------------------------------------|
| `value`               | Current selected value (read-only)                 | `Value` property / bound state                             |
| `disabled`            | Prevents interaction and selection                 | `Disabled` property                                        |
| `placeholder`         | Input field hint text                              | `Placeholder` property                                     |
| `loading`             | Displays a loading indicator                       | Not supported                                              |
| `required`            | Mandates value selection                           | Not supported (use validation via `Invalid`)               |
| `readOnly`            | Prevents user modification                         | Not supported (use `Disabled`)                             |
| `allowCustomValue`    | Permits values outside the standard list           | Not supported                                              |
| `allowDeselect`       | Enables clearing selected items                    | `Nullable` property                                        |
| `showClear`           | Displays a clear button                            | `Nullable` property                                        |
| `labelPosition`       | Label placement ("top" or "left")                  | Handled by `WithField().Label()` / layout                  |
| `emptyMessage`        | Text displayed when no value exists                | Not supported                                              |
| `inputValue`          | Current entered text value (read-only)             | Not supported                                              |
| `selectedIndex`       | Index of selected value (read-only)                | Not supported                                              |
| `selectedItem`        | Selected item data (read-only)                     | Not supported                                              |
| `selectedLabel`       | Label text of selection (read-only)                | Not supported                                              |
| `isHiddenOnDesktop`   | Controls desktop visibility                        | `Visible` property (no desktop/mobile distinction)         |
| `isHiddenOnMobile`    | Controls mobile visibility                         | `Visible` property (no desktop/mobile distinction)         |
| `clearInputValueOnChange` | Clears input when selection updates            | Not supported                                              |
| **Events**            |                                                    |                                                            |
| `Change`              | Triggered on value modification                    | `OnChange` event                                           |
| `Focus`               | Triggered when input gains focus                   | Not supported                                              |
| `Blur`                | Triggered when input loses focus                   | `OnBlur` event                                             |
| `Input Value Change`  | Triggered when typed input changes                 | Not supported                                              |
| **Methods**           |                                                    |                                                            |
| `setValue()`          | Sets the component's value                         | `state.Set(value)`                                         |
| `clearValue()`        | Empties current selection                          | `state.Set(null)` with `Nullable`                          |
| `resetValue()`        | Restores default value                             | Not supported                                              |
| `setDisabled()`       | Toggles disabled state                             | Set `Disabled` property                                    |
| `setHidden()`         | Toggles visibility                                 | Set `Visible` property                                     |
| `focus()`             | Sets input focus                                   | Not supported                                              |
| `blur()`              | Removes input focus                                | Not supported                                              |
| `clearValidation()`   | Removes validation messages                        | Set `Invalid` to `null`                                    |
| `scrollIntoView()`    | Brings component into viewport                     | Not supported                                              |
