# Checkbox

A boolean input component that allows users to toggle between checked and unchecked states. Commonly used in forms for accepting terms, enabling options, or selecting multiple items from a list.

## Reflex

```python
rx.checkbox(
    text="Accept Terms",
    default_checked=False,
    on_change=handle_change,
    size="2",
    variant="surface",
    color_scheme="tomato",
)
```

## Ivy

```csharp
public class CheckboxDemo : ViewBase
{
    public override object? Build()
    {
        var state = UseState(false);
        return state.ToBoolInput().Label("Accept Terms");
    }
}
```

## Parameters

| Parameter        | Documentation                                      | Ivy                                                              |
|------------------|-----------------------------------------------------|------------------------------------------------------------------|
| `text`           | Label text displayed with the checkbox              | `.Label("text")`                                                 |
| `size`           | Size of the checkbox (`"1"`, `"2"`, `"3"`)          | Not supported                                                    |
| `variant`        | Visual style (`"classic"`, `"surface"`, `"soft"`)   | `.Variant(BoolInputs.Checkbox \| Switch \| Toggle)`              |
| `color_scheme`   | Color scheme (e.g. `"tomato"`, `"red"`, `"blue"`)   | Not supported                                                    |
| `high_contrast`  | Enable high contrast mode                           | Not supported                                                    |
| `default_checked`| Initial checked state (uncontrolled)                | `UseState(false)` sets initial value                             |
| `checked`        | Controlled checked state                            | Bound via `UseState<bool>`                                       |
| `disabled`       | Disable the checkbox                                | `.Disabled(true)`                                                |
| `required`       | Mark as required in forms                           | `.Invalid("message")` for validation                             |
| `name`           | Form field name for submission                      | Not supported                                                    |
| `value`          | Form field value for submission                     | `.Value` (read-only)                                             |
| `on_change`      | Event fired when checked state changes              | State binding handles reactivity automatically                   |
| `as_child`       | Render as child component                           | Not supported                                                    |
| `spacing`        | Spacing between checkbox and label                  | Not supported                                                    |
| N/A              | N/A                                                 | `.Icon(Icons.X)` - optional icon display                         |
| N/A              | N/A                                                 | `.Description("text")` - helper text below input                 |
| N/A              | N/A                                                 | `Nullable` - supports `bool?` for three-state checkbox           |
| N/A              | N/A                                                 | `.ToSwitchInput()` / `.ToToggleInput()` - alternative variants   |
