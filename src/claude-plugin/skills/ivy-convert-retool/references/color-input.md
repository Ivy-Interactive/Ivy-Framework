# Color Input

An input field that lets users select a color value through a color picker interface, with support for various display formats.

## Retool

```toolscript
colorInput1.setValue("#ff5733")
colorInput1.value          // "#ff5733"
colorInput1.formattedValue // "rgb(255, 87, 51)" when colorFormat is "rgb"
```

## Ivy

```csharp
var colorState = UseState("#ff5733");
return colorState.ToColorInput(
    placeholder: "Select a color"
);
```

## Parameters

| Parameter          | Documentation                                          | Ivy                                                        |
|--------------------|--------------------------------------------------------|------------------------------------------------------------|
| `value`            | Selected color value                                   | `Value` property / state binding                           |
| `placeholder`      | Default text shown when empty                          | `Placeholder` property                                     |
| `disabled`         | Disables user interaction                              | `Disabled` property                                        |
| `colorFormat`      | Display format (hex, rgb)                              | Not supported (hex string only)                            |
| `allowAlpha`       | Enables opacity/alpha channel                          | Not supported                                              |
| `required`         | Makes selection mandatory                              | Not supported (use `Invalid` for validation)               |
| `readOnly`         | Prevents user edits                                    | Not supported                                              |
| `loading`          | Shows a loading indicator                              | Not supported                                              |
| `showClear`        | Displays a clear button                                | `Nullable` property (allows clearing to null)              |
| `labelPosition`    | Label placement (top/left)                             | Not supported                                              |
| `tooltipText`      | Helper text shown on hover                             | Not supported                                              |
| `iconBefore`       | Prefix icon                                            | Not supported                                              |
| `iconAfter`        | Suffix icon                                            | Not supported                                              |
| `textBefore`       | Prefix text                                            | Not supported                                              |
| `textAfter`        | Suffix text                                            | Not supported                                              |
| `margin`           | External spacing                                       | Not supported (use layout containers)                      |
| `isHiddenOnDesktop`| Controls desktop visibility                            | `Visible` property                                         |
| `isHiddenOnMobile` | Controls mobile visibility                             | `Visible` property                                         |
| `formattedValue`   | Value in the specified color format (read-only)        | Not supported                                              |
| N/A                | N/A                                                    | `Variant` (Text, Picker, TextAndPicker, Swatch)            |
| N/A                | N/A                                                    | `Scale` (size scaling)                                     |
| N/A                | N/A                                                    | `Foreground` (styling option)                              |
| N/A                | N/A                                                    | `Invalid` (validation error message)                       |
| **Events**         |                                                        |                                                            |
| Change             | Value modified                                         | `OnChange` event                                           |
| Focus              | Input gains focus                                      | Not supported                                              |
| Blur               | Input loses focus                                      | `OnBlur` event                                             |
| Submit             | Value submitted                                        | Not supported                                              |
| **Methods**        |                                                        |                                                            |
| `setValue()`       | Sets color value                                       | State binding (`state.Set(value)`)                         |
| `clearValue()`     | Empties the current selection                          | `Nullable` + state binding                                 |
| `resetValue()`     | Restores default value                                 | State binding (reset to initial value)                     |
| `focus()`          | Sets focus on component                                | Not supported                                              |
| `setDisabled()`    | Toggles disabled state                                 | `Disabled` property                                        |
| `setHidden()`      | Toggles visibility                                     | `Visible` property                                         |
| `clearValidation()`| Removes validation messages                            | `Invalid` property (set to null)                           |
| `scrollIntoView()` | Scrolls component into view                            | Not supported                                              |
