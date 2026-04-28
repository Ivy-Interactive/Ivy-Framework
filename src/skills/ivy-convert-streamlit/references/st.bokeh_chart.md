# st.bokeh_chart

Displays an interactive Bokeh chart. Deprecated in Streamlit 1.49.0 and removed in 1.52.0 in favor of the `streamlit-bokeh` custom component.

## Streamlit

```python
import streamlit as st
from bokeh.plotting import figure

x = [1, 2, 3, 4, 5]
y = [6, 7, 2, 4, 5]

p = figure(title="simple line example", x_axis_label="x", y_axis_label="y")
p.line(x, y, legend_label="Trend", line_width=2)

st.bokeh_chart(p, use_container_width=True)
```

## Ivy

Ivy does not support embedding Bokeh figures directly. The closest equivalent is the built-in `LineChart` widget.

```csharp
new LineChart(
    new { data = new[] {
        new { x = 1, y = 6 },
        new { x = 2, y = 7 },
        new { x = 3, y = 2 },
        new { x = 4, y = 4 },
        new { x = 5, y = 5 },
    }},
    new Line[] { new Line("y").StrokeWidth(2) }
)
```

## Parameters

| Parameter           | Documentation                                                                                            | Ivy           |
|---------------------|----------------------------------------------------------------------------------------------------------|---------------|
| figure              | A Bokeh figure object (`bokeh.plotting.figure.Figure`) to render                                         | Not supported |
| use_container_width | When `True`, overrides the figure's native width to fill the parent container width. Default: `True`.    | Not supported |
