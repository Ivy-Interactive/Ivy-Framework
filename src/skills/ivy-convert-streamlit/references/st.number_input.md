# st.number_input

Display a numeric input widget. Allows users to enter and adjust numeric values with optional min/max constraints, step increments, and formatting.

## Streamlit

```python
import streamlit as st

number = st.number_input("Insert a number")
st.write("The current number is ", number)
```

## Ivy

```csharp
var value = UseState(0);
return value.ToNumberInput()
    .Min(-10)
    .Max(10)
    .WithField()
    .Label("Insert a number");
```

## Parameters

| Parameter        | Documentation                                                                                          | Ivy                                                                                      |
|------------------|--------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------|
| label            | A short label displayed above the input. Supports markdown.                                            | `.WithField().Label("...")` via the field wrapper                                        |
| min_value        | Minimum permitted value. Defaults to `None`.                                                           | `.Min(value)`                                                                            |
| max_value        | Maximum permitted value. Defaults to `None`.                                                           | `.Max(value)`                                                                            |
| value            | Initial widget value. Defaults to `"min"`. Set to `None` for an empty input.                           | Constructor parameter `value` or via `UseState`                                          |
| step             | Stepping interval. Defaults to `1` for integers, `0.01` for floats.                                   | `.Step(value)`                                                                           |
| format           | Printf-style format string (e.g. `"%0.1f"`).                                                          | `.Precision(n)` for decimal places, `.FormatStyle(...)` for decimal/currency/percent     |
| help             | Tooltip text shown next to the label. Supports markdown.                                               | Not supported                                                                            |
| on_change        | Callback invoked when the value changes.                                                               | `onChange` constructor parameter: `Func<Event<IInput<TNumber>, TNumber>, ValueTask>`     |
| placeholder      | Text displayed when the input is empty.                                                                | `.Placeholder("...")`                                                                   |
| disabled         | Disables the input when `True`.                                                                        | `.Disabled(true)`                                                                        |
| label_visibility | Controls label visibility: `"visible"`, `"hidden"`, or `"collapsed"`.                                  | Not supported                                                                            |
| icon             | Optional emoji or Material Symbols icon shown in the input.                                            | Not supported                                                                            |
