# st.toggle

Displays a toggle widget that allows users to switch between two states (on/off). Returns a `bool` indicating the current state.

## Streamlit

```python
import streamlit as st

on = st.toggle("Activate feature")

if on:
    st.write("Feature activated!")
```

## Ivy

```csharp
var state = UseState(false);
return state.ToSwitchInput()
    .Label("Activate feature");
```

## Parameters

| Parameter          | Documentation                                                                                                  | Ivy                                               |
|--------------------|----------------------------------------------------------------------------------------------------------------|----------------------------------------------------|
| `label`            | A short label explaining to the user what this toggle is for. Supports GitHub-flavored Markdown.               | `.Label("text")`                                   |
| `value`            | Pre-selects the toggle on initial render. Defaults to `False`.                                                 | `UseState(false)` sets the initial value            |
| `help`             | A tooltip displayed next to the widget label.                                                                  | `.Description("text")`                             |
| `on_change`        | Optional callback invoked when the toggle value changes.                                                       | `.OnChange(handler)`                               |
| `disabled`         | Disables the toggle when `True`. Defaults to `False`.                                                          | `.Disabled(true)`                                  |
| `label_visibility` | Controls label display: `"visible"`, `"hidden"` (spacer remains), or `"collapsed"` (no label and no spacer).  | Not supported                                      |
