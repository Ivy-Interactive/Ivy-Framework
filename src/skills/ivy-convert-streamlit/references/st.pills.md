# st.pills

Displays selection options as pill-shaped buttons instead of a dropdown list. Supports single and multi-select modes. The Ivy equivalent is `SelectInput` with the `SelectInputs.Toggle` variant, which renders options as toggle buttons.

## Streamlit

```python
import streamlit as st

options = ["North", "East", "South", "West"]
selection = st.pills("Directions", options, selection_mode="multi")
st.markdown(f"Your selected options: {selection}.")
```

## Ivy

```csharp
public class PillsDemo : ViewBase
{
    public override object? Build()
    {
        var options = new[] { "North", "East", "South", "West" };
        var selection = UseState<string[]>([]);

        return selection.ToSelectInput(options.ToOptions())
            .Variant(SelectInputs.Toggle)
            .WithField()
            .Label("Directions");
    }
}
```

## Parameters

| Parameter        | Documentation                                                                                          | Ivy                                                                                      |
|------------------|--------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------|
| label            | Short label explaining the widget to the user. Supports Markdown.                                      | `.WithField().Label("...")`                                                              |
| options          | Iterable of selection option labels. Accepts lists, sets, or dataframe-like objects.                   | `IEnumerable<IAnyOption>` passed via constructor or `.ToOptions()`                        |
| selection_mode   | `"single"` (default) or `"multi"` — controls whether one or multiple options can be selected.          | Single: `UseState<string>("...")`, Multi: `UseState<string[]>([])` — inferred from state type |
| default          | Initial widget value. Single value or list depending on mode.                                          | Initial value passed to `UseState`                                                       |
| format_func      | Function to transform option display text without affecting the return value.                          | Custom `IAnyOption` with separate label/value                                            |
| help             | Tooltip shown next to the label.                                                                       | `.WithField().Help("...")`                                                               |
| on_change        | Callback triggered when the selection changes.                                                         | `onChange` parameter in constructor                                                      |
| disabled         | Disables the widget when `True`.                                                                       | `.Disabled(true)`                                                                        |
| label_visibility | Controls label display: `"visible"`, `"hidden"`, or `"collapsed"`.                                    | Not supported                                                                            |
