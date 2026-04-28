# st.plotly_chart

Display an interactive Plotly chart. Renders Plotly figures within an application, functioning as a replacement for Plotly's native `py.plot()` or `py.iplot()`. Supports interactive selection events (points, box, lasso) and Plotly configuration options.

Ivy does not have a direct Plotly wrapper. Instead it provides native chart widgets (`LineChart`, `BarChart`, `AreaChart`, `PieChart`) with a fluent C# API.

## Streamlit

```python
import plotly.graph_objects as go
import streamlit as st

fig = go.Figure()
fig.add_trace(go.Scatter(x=[1, 2, 3, 4, 5], y=[1, 3, 2, 5, 4]))

st.plotly_chart(fig, theme="streamlit", on_select="rerun",
                selection_mode=["points", "lasso"],
                config={"scrollZoom": False})
```

## Ivy

```csharp
var data = new[]
{
    new { X = 1, Y = 1 },
    new { X = 2, Y = 3 },
    new { X = 3, Y = 2 },
    new { X = 4, Y = 5 },
    new { X = 5, Y = 4 },
};

data.ToLineChart(style: LineChartStyles.Default)
    .Dimension("X", e => e.X)
    .Measure("Y", e => e.Sum(f => f.Y))
    .Toolbox()
    .Tooltip()
    .Legend();
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| figure_or_data | The Plotly Figure, Data object, or dict/list to render. Charts exceeding 1000 data points use WebGL rendering. | `Data` property / `ToLineChart(data)` — Ivy takes raw data directly instead of a pre-built Plotly figure |
| use_container_width | **Deprecated.** Controls whether the chart stretches to the parent container width. | Not supported (deprecated in Streamlit too) |
| theme | `"streamlit"` applies Streamlit's design defaults; `None` uses Plotly defaults. | `ColorScheme` — apply a preset color palette via `.ColorScheme()` |
| on_select | Selection event response: `"ignore"` (default), `"rerun"`, or a callable callback. | Not supported |
| selection_mode | Selection interaction modes: `"points"`, `"box"`, `"lasso"`, or an iterable of these. All enabled by default. | Not supported |
| config | Dict of Plotly configuration options passed to Plotly's `show()` for customizing chart behavior (e.g. `scrollZoom`, `displayModeBar`). | `Toolbox` — `.Toolbox()` enables a built-in interactive toolbar with zoom, pan, save, and reset controls |
