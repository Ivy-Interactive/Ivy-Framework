# st.segmented_control

A segmented control widget that displays a linear set of segments where each option functions like a toggle button. Supports single and multi-selection modes.

## Streamlit

```python
import streamlit as st

options = ["North", "East", "South", "West"]
selection = st.segmented_control(
    "Directions", options, selection_mode="multi"
)
st.markdown(f"Your selected options: {selection}.")
```

## Ivy

In Ivy, the `SelectInput` widget with `Variant(SelectInputs.Toggle)` provides equivalent segmented control functionality.

```csharp
var directions = UseState<string[]>([]);
var options = new[] { "North", "East", "South", "West" }.ToOptions();

return directions.ToSelectInput(options)
    .Variant(SelectInputs.Toggle)
    .WithField()
    .Label("Directions");
```

## Parameters

| Parameter        | Documentation                                                                                          | Ivy                                                                |
|------------------|--------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------|
| label            | A short label explaining to the user what this widget is for. Supports Markdown.                       | `.WithField().Label()`                                             |
| options          | Labels for the select options. Can be a list, set, or dataframe column.                                | `IEnumerable<IAnyOption>` via `.ToOptions()`                       |
| selection_mode   | `"single"` or `"multi"`. Controls whether one or multiple options can be selected.                     | `selectMany` parameter (`bool`)                                    |
| default          | The initial widget value. A list for multi-mode, a single value for single-mode, or `None`.            | Initial value of `UseState`                                        |
| format_func      | Function to modify the display of options. Receives the raw option and outputs a display label.         | Custom `Option` objects with separate value and label               |
| help             | Optional tooltip displayed next to the label.                                                          | Not supported                                                      |
| on_change        | Optional callback invoked when the widget value changes.                                               | `onChange` parameter or state binding                               |
| disabled         | Disables the widget when `True`.                                                                       | `Disabled` property / `disabled` parameter                         |
| label_visibility | Controls label display: `"visible"`, `"hidden"`, or `"collapsed"`.                                     | Not supported                                                      |
