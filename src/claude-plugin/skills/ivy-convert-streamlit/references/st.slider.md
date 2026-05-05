# st.slider

Display a slider widget for selecting numeric values, dates, times, or ranges. Supports int, float, date, time, and datetime types. Passing a two-element tuple as the value creates a range slider.

## Streamlit

```python
import streamlit as st
from datetime import time

age = st.slider("How old are you?", 0, 130, 25)
st.write("I'm ", age, "years old")

values = st.slider("Select a range of values", 0.0, 100.0, (25.0, 75.0))
st.write("Values:", values)

appointment = st.slider("Schedule your appointment:",
                        value=(time(11, 30), time(12, 45)))
st.write("You're scheduled for:", appointment)
```

## Ivy

```csharp
var age = UseState(25);
age.ToNumberInput()
    .Min(0)
    .Max(130)
    .Variant(NumberInputs.Slider);

var tapes = UseState(50.0);
tapes.ToNumberInput()
    .Min(0.0)
    .Max(100.0)
    .Step(0.5)
    .Precision(2)
    .Variant(NumberInputs.Slider);
```

## Parameters

| Parameter        | Documentation                                                                                          | Ivy                                                              |
|------------------|--------------------------------------------------------------------------------------------------------|------------------------------------------------------------------|
| label            | A short label explaining to the user what this slider is for. Supports Markdown.                       | Wrap with `new Field(input, label: "...")` or `.WithField()`     |
| min_value        | Minimum permitted value. Defaults to 0 for int, 0.0 for float.                                        | `.Min(value)`                                                    |
| max_value        | Maximum permitted value. Defaults to 100 for int, 1.0 for float.                                      | `.Max(value)`                                                    |
| value            | The initial value of the slider. A tuple/list creates a range slider.                                  | First constructor parameter. Range slider not supported.         |
| step             | The stepping interval. Defaults to 1 for int, 0.01 for float, timedelta for date/time types.          | `.Step(value)`                                                   |
| format           | Printf-style format string (e.g. `"%d"`, `"%.2f"`) or predefined (`"percent"`, `"dollar"`).           | `.FormatStyle(Decimal/Currency/Percent)` and `.Precision(n)`     |
| help             | Tooltip displayed next to the slider label.                                                            | Wrap with `new Field(input, help: "...")`                        |
| on_change        | Optional callback invoked when the slider value changes.                                               | `state.ToNumberInput()` with `.OnChange(e => ...)` method        |
| disabled         | If `True`, the slider is greyed out and cannot be interacted with.                                     | `.Disabled(true)`                                                |
| label_visibility | Controls label display: `"visible"`, `"hidden"`, or `"collapsed"`.                                    | Field wrapper with `.Visible(bool)`                              |
