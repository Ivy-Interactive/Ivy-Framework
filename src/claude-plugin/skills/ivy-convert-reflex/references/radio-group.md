# Radio Group

A set of interactive radio buttons where only one can be selected at a time, enabling users to choose a single option from multiple alternatives.

## Reflex

```python
rx.radio_group(
    items=["Option 1", "Option 2", "Option 3"],
    direction="column",
    value="Option 1",
    size="2",
    variant="classic",
    name="choice_group",
    required=True,
)
```

## Ivy

Ivy does not have a dedicated RadioGroup widget. The closest equivalent is `SelectInput` with the `Toggle` variant, which renders selectable button toggles for single-item selection.

```csharp
public class RadioGroupDemo : ViewBase
{
    public override object? Build()
    {
        var options = new string[] { "Option 1", "Option 2", "Option 3" };
        var selected = UseState("Option 1");
        return selected.ToSelectInput(options.ToOptions())
                       .Variant(SelectInputs.Toggle)
                       .WithField()
                       .Label("Choose an option");
    }
}
```

## Parameters

| Parameter        | Documentation                                       | Ivy                                                                 |
|------------------|-----------------------------------------------------|---------------------------------------------------------------------|
| `items`          | List of options to display                          | `options` parameter (via `.ToOptions()`)                            |
| `direction`      | Layout direction (`"row"` / `"column"`)             | Not supported                                                       |
| `spacing`        | Gap between items (`"0"`–`"9"`)                     | Not supported                                                       |
| `size`           | Button size (`"1"`–`"4"`)                           | `.Scale()` (numeric scaling)                                        |
| `variant`        | Visual style (`"classic"` / `"surface"`)            | `.Variant()` (`Select` / `List` / `Toggle`)                        |
| `color_scheme`   | Color theme                                         | Not supported (controlled via theming)                              |
| `high_contrast`  | Enhanced contrast mode                              | Not supported                                                       |
| `value`          | Current selected value                              | Bound via `UseState` (first arg to `ToSelectInput`)                 |
| `default_value`  | Initial selection                                   | Set as initial value of `UseState`                                  |
| `disabled`       | Disable all options                                 | `.Disabled(bool)`                                                   |
| `name`           | Form submission identifier                          | Handled by Ivy form binding                                         |
| `required`       | Mandate selection before form submission            | Not supported (use validation logic)                                |
| `on_change`      | Event fired when selection changes                  | `.OnChange(Func<Event<IInput<T>, T>, ValueTask>)`                  |
