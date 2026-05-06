# Checkbox

An input field to toggle a boolean value. Retool provides a single Checkbox component, while Ivy offers a `BoolInput` widget with three visual variants: Checkbox, Switch, and Toggle.

## Retool

```js
// Read value
checkbox1.value // true or false

// Set value programmatically
checkbox1.setValue(true);

// Toggle
checkbox1.toggle();

// Disable/enable
checkbox1.setDisabled(true);

// Show/hide
checkbox1.setHidden(false);

// Reset to default
checkbox1.resetValue();

// Event handler: fires on Change, True, False events
```

## Ivy

```csharp
public class CheckboxDemo : ViewBase
{
    public override object? Build()
    {
        var agreed = UseState(false);

        return Layout.Horizontal()
            | agreed.ToBoolInput()
                .Variant(BoolInputs.Checkbox)
                .Label("Agree to terms and conditions")
            | (agreed.Value ? Text.InlineCode("You are all set!") : null);
    }
}
```

## Parameters

| Parameter                | Retool Documentation                                                            | Ivy                                                                             |
|--------------------------|---------------------------------------------------------------------------------|---------------------------------------------------------------------------------|
| `value`                  | The current boolean value (read-only)                                           | `Value` property on state; read via `state.Value`                               |
| `disabled`               | Whether interaction is disabled (boolean, default `false`)                      | `.Disabled(bool)` fluent method                                                 |
| `label`                  | Not a direct property; set via the component inspector label field              | `.Label(string)` fluent method                                                  |
| `tooltipText`            | Tooltip text displayed next to the label on hover (Markdown)                    | `.Description(string)` (partial equivalent)                                     |
| `required`               | Whether a value is required to be selected (boolean)                            | `.Invalid(string)` for validation messaging; no built-in `required` flag        |
| `style`                  | Custom style options (object)                                                   | Not supported (styling via CSS/theme)                                           |
| `margin`                 | Margin around the component (`"4px 8px"` or `"0"`)                              | Not supported                                                                   |
| `id`                     | Unique identifier/name (e.g. `checkbox1`)                                       | Not applicable (C# variable references)                                         |
| `isHiddenOnDesktop`      | Hide on desktop layout (boolean)                                                | `.Visible(bool)` controls visibility (not layout-specific)                      |
| `isHiddenOnMobile`       | Hide on mobile layout (boolean)                                                 | Not supported (no separate mobile layout toggle)                                |
| `maintainSpaceWhenHidden`| Keep space when hidden (boolean)                                                | Not supported                                                                   |
| `showInEditor`           | Show in editor when hidden (boolean)                                            | Not supported                                                                   |
| `variant`                | Single checkbox style only                                                      | `.Variant(BoolInputs)` — `Checkbox`, `Switch`, or `Toggle`                     |
| `icon`                   | Not supported                                                                   | `.Icon(Icons)` sets an icon on the input                                        |
| `nullable`               | Not supported                                                                   | Supports `bool?` and nullable numeric types for tri-state (null/true/false)     |
| `events`                 | `Change`, `True`, `False` events configured via event handlers                  | `.OnChange()` and `.OnBlur()` events                                            |
| `setValue()`             | Set the current value                                                           | `state.Set(value)` on the bound state                                           |
| `toggle()`               | Toggle between true and false                                                   | Not a built-in method; use `state.Set(!state.Value)`                            |
| `clearValue()`           | Clear the current value                                                         | Not supported                                                                   |
| `resetValue()`           | Reset to default value                                                          | Not supported                                                                   |
| `setDisabled()`          | Programmatically disable/enable                                                 | Controlled via reactive state bound to `.Disabled()`                            |
| `setHidden()`            | Programmatically show/hide                                                      | Controlled via reactive state bound to `.Visible()`                             |
| `focus()`                | Set focus on the component                                                      | Not supported                                                                   |
| `validate()`             | Validate the input value                                                        | Not supported (use `.Invalid(string)` for manual validation)                    |
| `clearValidation()`      | Clear the validation message                                                    | Set `.Invalid("")` or remove the invalid binding                                |
