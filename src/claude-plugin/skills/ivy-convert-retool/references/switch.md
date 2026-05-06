# Switch

An input field to toggle a boolean value. Allows users to switch between true and false states.

## Retool

```toolscript
switch.setValue(true);
switch.toggle();
switch.setDisabled(false);
```

## Ivy

```csharp
var state = UseState(false);
return state.ToBoolInput()
    .Label("Accept Terms")
    .Variant(BoolInputs.Switch);

// Or using the extension method:
return state.ToSwitchInput().Label("Accept Terms");
```

## Parameters

| Parameter                | Documentation                                           | Ivy                                                              |
|--------------------------|---------------------------------------------------------|------------------------------------------------------------------|
| `value`                  | Current toggle state (boolean)                          | `Value` property via bound `IAnyState`                           |
| `disabled`               | Disables user interaction                               | `.Disabled(bool)`                                                |
| `id`                     | Unique identifier for the component                     | Not supported (auto-generated)                                   |
| `label`                  | Text label for the switch                               | `.Label(string)`                                                 |
| `labelPosition`          | Position of label: `top` or `left`                      | Not supported                                                    |
| `margin`                 | External spacing around the component                   | Not supported (handled by layout)                                |
| `required`               | Whether a value must be selected                        | Not supported directly; use validation                           |
| `tooltipText`            | Helper text displayed on hover                          | `.Description(string)`                                           |
| `isHiddenOnDesktop`      | Hide component in desktop layout                        | Not supported                                                    |
| `isHiddenOnMobile`       | Hide component in mobile layout                         | Not supported                                                    |
| `maintainSpaceWhenHidden`| Reserve space when component is hidden                  | Not supported                                                    |
| **Methods**              |                                                         |                                                                  |
| `setValue(value)`        | Set the toggle value                                    | Set via bound state: `state.Set(true)`                           |
| `toggle()`              | Switch between true/false                               | `state.Set(!state.Value)`                                        |
| `clearValue()`          | Clear the current value                                 | `state.Set(default)` or use nullable `BoolInput<bool?>`          |
| `resetValue()`          | Restore default value                                   | Not supported                                                    |
| `setDisabled(disabled)` | Toggle disabled state                                   | `.Disabled(bool)` with bound state                               |
| `setHidden(hidden)`     | Toggle visibility                                       | Not supported                                                    |
| `focus()`               | Set component focus                                     | Not supported                                                    |
| `scrollIntoView()`      | Scroll component into view                              | Not supported                                                    |
| `clearValidation()`     | Remove validation messages                              | `.Invalid(string)` set to `null`                                 |
| **Events**               |                                                         |                                                                  |
| Change                   | Triggered when value changes                            | `.OnChange(Action)`                                              |
| True                     | Triggered when value becomes true                       | Not supported (use `OnChange` with condition)                    |
| False                    | Triggered when value becomes false                      | Not supported (use `OnChange` with condition)                    |
