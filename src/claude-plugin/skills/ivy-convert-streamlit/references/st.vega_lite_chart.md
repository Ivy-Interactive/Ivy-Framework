# st.vega_lite_chart

Display a chart using the Vega-Lite grammar. Vega-Lite is a high-level grammar of interactive graphics built on top of Vega. It allows you to define charts using a declarative JSON spec, supporting many chart types (scatter, bar, line, area, pie, etc.) with encoding channels for x, y, color, size, and more. Streamlit wraps this into a single function that accepts a dataframe and a Vega-Lite specification dict.

## Streamlit

```python
import pandas as pd
import streamlit as st
from numpy.random import default_rng as rng

df = pd.DataFrame(rng(0).standard_normal((60, 3)), columns=["a", "b", "c"])

st.vega_lite_chart(
    df,
    {
        "mark": {"type": "circle", "tooltip": True},
        "encoding": {
            "x": {"field": "a", "type": "quantitative"},
            "y": {"field": "b", "type": "quantitative"},
            "size": {"field": "c", "type": "quantitative"},
            "color": {"field": "c", "type": "quantitative"},
        },
    },
)
```

## Ivy

Ivy does not have a single Vega-Lite equivalent. Instead it provides strongly-typed chart widgets (`LineChart`, `BarChart`, `AreaChart`, `PieChart`) with a fluent builder API. The closest general-purpose comparison uses individual chart types with `.Dimension()` and `.Measure()` methods.

```csharp
var data = new[]
{
    new { Month = "Jan", Desktop = 186, Mobile = 100 },
    new { Month = "Feb", Desktop = 305, Mobile = 200 },
    new { Month = "Mar", Desktop = 237, Mobile = 300 },
    new { Month = "Apr", Desktop = 186, Mobile = 100 },
    new { Month = "May", Desktop = 325, Mobile = 200 }
};

return data.ToBarChart()
    .Dimension("Month", e => e.Month)
    .Measure("Desktop", e => e.Sum(f => f.Desktop))
    .Measure("Mobile", e => e.Sum(f => f.Mobile))
    .Legend()
    .Tooltip()
    .Toolbox();
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| data | The dataset to visualize, anything supported by `st.dataframe` | Constructor parameter or extension method receiver (e.g. `data.ToBarChart()`) — accepts anonymous object arrays |
| spec | Vega-Lite specification dict defining mark type, encoding channels, and transforms | Not supported — chart type is chosen via explicit widget class (`LineChart`, `BarChart`, `AreaChart`, `PieChart`) and configured through fluent methods (`.Dimension()`, `.Measure()`) |
| use_container_width | Deprecated. Whether the chart should stretch to fill the container width | `.Width(Size.Stretch)` or `.Width(pixels)` on all chart types |
| theme | `"streamlit"` applies Streamlit's design theme; `None` uses Vega-Lite defaults | `.ColorScheme(ColorScheme.Default)` or `.ColorScheme(ColorScheme.Rainbow)` for color theming; no full theme system |
| on_select | Controls selection event handling: `"ignore"`, `"rerun"`, or a callable callback | Not supported — Ivy charts do not expose selection event callbacks |
| selection_mode | Specifies which Vega-Lite selection parameters trigger reruns | Not supported |
