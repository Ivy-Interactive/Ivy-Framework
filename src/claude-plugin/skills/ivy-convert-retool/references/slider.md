# Slider

A slider to select a numeric value within a defined range.

## Retool

```js
// The Slider component provides a draggable control
// for selecting a numeric value.
// Configure min, max, step, and read the value:
slider1.value           // current numeric value
slider1.setValue(50)    // set value programmatically
slider1.resetValue()    // reset to default
```

## Ivy

```csharp
var volume = UseState(50.0);

return volume.ToNumberInput()
    .Min(0)
    .Max(100)
    .Step(1)
    .Variant(NumberInputs.Slider)
    .WithField()
    .Label("Volume");
```

## Parameters

| Parameter              | Retool                                                                 | Ivy                                                        |
|------------------------|------------------------------------------------------------------------|------------------------------------------------------------|
| `value`                | Current numeric value (read-only)                                      | Bound via `IAnyState` or value + `onChange`                 |
| `min`                  | Minimum allowed value                                                  | `.Min(double)`                                             |
| `max`                  | Maximum allowed value                                                  | `.Max(double)`                                             |
| `step`                 | Increment/decrement interval                                           | `.Step(double)`                                            |
| `disabled`             | Disables interaction (`true`/`false`)                                  | `disabled` constructor parameter or `.Disabled(bool)`      |
| `customValidation`     | Custom validation rule using `{{ }}` expressions                       | `.Invalid(string)` sets a validation message               |
| `hideValidationMessage`| Hides validation error messages                                        | Not supported                                              |
| `hideOutput`           | Hides the displayed value caption                                      | Not supported                                              |
| `formDataKey`          | Key for Form component data binding                                    | Not supported (Ivy uses direct state binding)              |
| `labelPosition`        | Label placement (`top` or `left`)                                      | Configured via `.WithField().Label()` and layout           |
| `tooltipText`          | Helper text displayed on hover                                         | Not supported                                              |
| `iconBefore`           | Prefix icon                                                            | Not supported                                              |
| `iconAfter`            | Suffix icon                                                            | Not supported                                              |
| `margin`               | External spacing (`4px 8px` or `0`)                                    | Not supported (handled by layout)                          |
| `style`                | Custom styling options                                                 | Not supported (handled by theming)                         |
| `isHiddenOnDesktop`    | Desktop layout visibility toggle                                       | Not supported                                              |
| `isHiddenOnMobile`     | Mobile layout visibility toggle                                        | Not supported                                              |
| `maintainSpaceWhenHidden` | Reserves space when component is hidden                             | Not supported                                              |
| `showInEditor`         | Visibility in editor when hidden                                       | Not supported                                              |
| `id`                   | Unique identifier / name                                               | Not applicable (C# variable binding)                       |
| `invalid`              | Whether validation has failed (read-only)                              | `.Invalid(string)` — non-null means invalid                |
| `validationMessage`    | Validation error message (read-only)                                   | `.Invalid(string)` serves as both flag and message         |
| `events.Change`        | Triggered when the slider value changes                                | `OnChange` callback                                        |
| N/A                    | N/A                                                                    | `.Precision(int)` — decimal places to display              |
| N/A                    | N/A                                                                    | `.FormatStyle(NumberFormatStyle)` — Decimal/Currency/Percent|
| N/A                    | N/A                                                                    | `.Currency(string)` — currency code (e.g. "USD")           |
| N/A                    | N/A                                                                    | `OnBlur` — triggered on focus loss                         |
| N/A                    | N/A                                                                    | `.Width(Size)` / `.Height(Size)`                           |

## Methods

| Method                          | Retool                                           | Ivy                                        |
|---------------------------------|--------------------------------------------------|--------------------------------------------|
| Set value                       | `slider.setValue(value)`                         | `state.Set(value)` via bound state         |
| Clear value                     | `slider.clearValue()`                            | `state.Set(default)`                       |
| Reset to default                | `slider.resetValue()`                            | Re-initialize state to default value       |
| Clear validation                | `slider.clearValidation()`                       | `.Invalid(null)`                           |
| Focus                           | `slider.focus()`                                 | Not supported                              |
| Set disabled                    | `slider.setDisabled(bool)`                       | Re-render with `disabled: true`            |
| Set hidden                      | `slider.setHidden(bool)`                         | Conditional rendering                      |
| Scroll into view                | `slider.scrollIntoView(options)`                 | Not supported                              |
