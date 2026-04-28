# st.select_slider

Displays a slider widget that allows users to select a value or range from an ordered list of options. Unlike `st.slider`, it accepts any datatype — not just numbers or dates. Passing a two-element tuple/list as the value creates a range slider.

Ivy has no slider-based selection widget. The closest equivalent is `SelectInput`, which provides dropdown, list, or toggle-based selection from a set of options.

## Streamlit

```python
import streamlit as st

color = st.select_slider(
    "Select a color of the rainbow",
    options=["red", "orange", "yellow", "green", "blue", "indigo", "violet"],
)
st.write("My favorite color is", color)

# Range slider
start_color, end_color = st.select_slider(
    "Select a range of color wavelength",
    options=["red", "orange", "yellow", "green", "blue", "indigo", "violet"],
    value=("red", "blue"),
)
st.write("You selected wavelengths between", start_color, "and", end_color)
```

## Ivy

```csharp
var color = UseState("red");

color.ToSelectInput(
    new[] { "red", "orange", "yellow", "green", "blue", "indigo", "violet" }
        .ToOptions()
).Variant(SelectInputs.Toggle)
 .Placeholder("Select a color of the rainbow");
```

## Parameters

| Parameter        | Documentation                                                                                       | Ivy                                                      |
|------------------|-----------------------------------------------------------------------------------------------------|----------------------------------------------------------|
| label            | Short label explaining the slider's purpose. Supports GitHub-flavored Markdown.                     | `Placeholder` (or wrap in a `Field` for a label)         |
| options          | Iterable of labels for the slider options (list, set, or dataframe column).                         | `Options` via `.ToOptions()`                             |
| value            | Initial value. A two-element tuple/list creates a range slider. Defaults to first option.           | `Value` (range selection not supported)                  |
| format_func      | Function to modify the display label of each option. Output is cast to string.                      | Custom `IAnyOption` rendering                            |
| help             | Tooltip displayed next to the label when `label_visibility="visible"`.                              | Not supported                                            |
| on_change        | Optional callback invoked when the slider value changes.                                            | `OnChange`                                               |
| disabled         | If `True`, the slider is greyed out and not interactive. Defaults to `False`.                        | `Disabled`                                               |
| label_visibility | Controls label display: `"visible"` (default), `"hidden"`, or `"collapsed"`.                        | `Visible` (widget-level only, no label-specific control) |
