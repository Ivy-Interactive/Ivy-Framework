# Plotly

Plotly is a graphing library wrapper (`rx.plotly`) that renders interactive Plotly figures directly in the UI. It supports any Plotly figure type (line, bar, 3D surface, etc.) via the `data` prop, with built-in responsive resizing and rich event handling (click, hover, select, relayout, etc.). Figures can be static or driven by state for dynamic updates at runtime.

Ivy does not wrap Plotly. Instead it provides built-in chart builder widgets (`LineChart`, `BarChart`, `AreaChart`, `PieChart`) powered by a fluent `.Dimension()` / `.Measure()` API on data sources. The comparison below maps Reflex's Plotly line-chart example to Ivy's `LineChart`.

## Reflex

```python
import reflex as rx
import plotly.express as px

df = px.data.gapminder().query("country=='Canada'")
fig = px.line(df, x="year", y="lifeExp", title="Life expectancy in Canada")

def line_chart():
    return rx.center(
        rx.plotly(data=fig),
    )
```

## Ivy

```csharp
var data = new[]
{
    new { Year = 1960, LifeExp = 71.3 },
    new { Year = 1970, LifeExp = 72.4 },
    new { Year = 1980, LifeExp = 74.2 },
    new { Year = 1990, LifeExp = 77.0 },
    new { Year = 2000, LifeExp = 79.8 },
};

return data.ToLineChart()
    .Dimension("Year", e => e.Year.ToString())
    .Measure("Life Expectancy", e => e.Sum(f => f.LifeExp))
    .XAxis()
    .YAxis()
    .Tooltip()
    .Toolbox();
```

## Parameters

| Parameter              | Documentation                                                                 | Ivy                                                                                     |
|------------------------|-------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------|
| `data`                 | The Plotly `Figure` object to display                                         | Data is passed via `ToLineChart()` / `ToBarChart()` etc. on a collection                |
| `layout`               | Dict to configure chart layout (axes, margins, background, title, etc.)       | Individual methods: `.XAxis()`, `.YAxis()`, `.Legend()`, `.CartesianGrid()`, `.Height()` |
| `template`             | Plotly visual styling template                                                | `.ColorScheme()` (e.g. `ColorScheme.Default`, `ColorScheme.Rainbow`)                    |
| `config`               | Additional Plotly configuration options                                       | Not supported                                                                           |
| `use_resize_handler`   | Bool (default `True`) - enables responsive sizing to container                | Not supported (charts fill their container by default)                                   |
| `on_click`             | Fired when the plot is clicked                                                | Not supported                                                                           |
| `on_hover`             | Fired when a plot element is hovered over                                     | Not supported                                                                           |
| `on_selected`          | Fired after selecting plot elements                                           | Not supported                                                                           |
| `on_relayout`          | Fired after zoom, pan, or layout change                                       | Not supported                                                                           |
| `on_restyle`           | Fired after the plot style is changed                                         | Not supported                                                                           |
| `on_after_plot`        | Fired after the plot is redrawn                                               | Not supported                                                                           |
| `on_double_click`      | Fired when the plot is double clicked                                         | Not supported                                                                           |
| `on_unhover`           | Fired when a hovered element is no longer hovered                             | Not supported                                                                           |
| 3D charts / surfaces   | Supported via any Plotly figure type (`go.Surface`, scatter3d, etc.)          | Not supported                                                                           |
| State-driven updates   | Figure can be a state var, re-rendered on change                              | Data source can be updated; chart rebuilds via Ivy state management                     |
| `.Dimension()`         | N/A - axes configured inside the Plotly figure                                | Defines the X-axis categorical grouping                                                 |
| `.Measure()`           | N/A - data series configured inside the Plotly figure                         | Adds a Y-axis data series with aggregation                                              |
| `.Toolbox()`           | N/A - configured via Plotly `config` dict                                     | Enables built-in interactive exploration tools (zoom, save, etc.)                       |
| `.SortBy()`            | N/A - sorting handled in the dataframe before passing to Plotly               | Sorts dimension axis (ascending, descending, or custom)                                 |
