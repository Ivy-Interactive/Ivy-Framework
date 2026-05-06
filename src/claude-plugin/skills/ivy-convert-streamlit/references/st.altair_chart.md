# st.altair_chart

Display a chart using the Vega-Altair library, a declarative statistical visualization library for Python based on Vega and Vega-Lite. Supports interactive selection events and theming.

## Streamlit

```python
import altair as alt
import pandas as pd
import streamlit as st
from numpy.random import default_rng as rng

df = pd.DataFrame(rng(0).standard_normal((60, 3)), columns=["a", "b", "c"])

chart = (
    alt.Chart(df)
    .mark_circle()
    .encode(x="a", y="b", size="c", color="c", tooltip=["a", "b", "c"])
)

st.altair_chart(chart, theme="streamlit", on_select="rerun")
```

## Ivy

Ivy does not support Vega-Altair charts directly. Instead it provides built-in chart types (LineChart, BarChart, AreaChart, PieChart) with a fluent API.

```csharp
var data = new[]
{
    new { A = 0.5, B = 1.2, C = 0.3 },
    new { A = -0.7, B = 0.8, C = -0.5 },
    new { A = 1.1, B = -0.4, C = 0.9 },
};

data.ToLineChart()
    .Dimension(x => x.A)
    .Measure(x => x.B)
    .Measure(x => x.C)
    .Tooltip()
    .Legend()
    .CartesianGrid();
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| altair_chart | The Altair chart object to display | Not supported — use built-in chart types (`ToLineChart()`, `ToBarChart()`, `ToAreaChart()`, `ToPieChart()`) |
| use_container_width | Deprecated. Overrides the chart width to match the parent container width | Not supported |
| theme | `"streamlit"` or `None`. Applies a Streamlit design theme to the chart | `ColorScheme` (e.g. `ColorScheme.Default`, `ColorScheme.Rainbow`) |
| on_select | `"ignore"`, `"rerun"`, or callable. Controls response to user selection events on the chart | Not supported |
| selection_mode | `str` or `Iterable[str]`. Specifies which Altair selection parameters to monitor | Not supported |
