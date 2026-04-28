# Graphing

Components for creating various types of charts and data visualizations. Reflex wraps the Recharts library (plus Plotly/Matplotlib) to provide area, bar, line, pie, scatter, radar, radial bar, funnel, and composed charts. Ivy provides built-in area, bar, line, and pie charts with a fluent builder API.

## Chart Types

| Chart Type     | Reflex                         | Ivy                |
|----------------|--------------------------------|--------------------|
| Area Chart     | `rx.recharts.area_chart`       | `AreaChart`        |
| Bar Chart      | `rx.recharts.bar_chart`        | `BarChart`         |
| Line Chart     | `rx.recharts.line_chart`       | `LineChart`        |
| Pie Chart      | `rx.recharts.pie_chart`        | `PieChart`         |
| Scatter Chart  | `rx.recharts.scatter_chart`    | Not supported      |
| Radar Chart    | `rx.recharts.radar_chart`      | Not supported      |
| Radial Bar     | `rx.recharts.radial_bar_chart` | Not supported      |
| Funnel Chart   | `rx.recharts.funnel_chart`     | Not supported      |
| Composed Chart | `rx.recharts.composed_chart`   | Not supported      |
| Error Bar      | `rx.recharts.error_bar`        | Not supported      |
| Plotly         | `rx.plotly`                    | Not supported      |
| Matplotlib     | `pyplot` (via reflex-pyplot)   | Not supported      |

---

## Area Chart

Displays quantitative data over time using filled areas that can be stacked.

### Reflex

```python
data = [
    {"name": "Jan", "uv": 4000, "pv": 2400},
    {"name": "Feb", "uv": 3000, "pv": 1398},
    {"name": "Mar", "uv": 2000, "pv": 9800},
]

def area_chart_example():
    return rx.recharts.area_chart(
        rx.recharts.area(data_key="uv", stroke="#8884d8", fill="#8884d8"),
        rx.recharts.area(data_key="pv", stroke="#82ca9d", fill="#82ca9d"),
        rx.recharts.x_axis(data_key="name"),
        rx.recharts.y_axis(),
        rx.recharts.cartesian_grid(stroke_dasharray="3 3"),
        rx.recharts.legend(),
        rx.recharts.graphing_tooltip(),
        data=data,
    )
```

### Ivy

```csharp
var data = new[]
{
    new { Name = "Jan", Uv = 4000, Pv = 2400 },
    new { Name = "Feb", Uv = 3000, Pv = 1398 },
    new { Name = "Mar", Uv = 2000, Pv = 9800 },
};

data.ToAreaChart()
    .Dimension("Name", e => e.Name)
    .Measure("Uv", e => e.Sum(f => f.Uv))
    .Measure("Pv", e => e.Sum(f => f.Pv))
    .XAxis()
    .YAxis()
    .CartesianGrid()
    .Legend()
    .Tooltip();
```

### Parameters

| Parameter        | Reflex                                          | Ivy                                |
|------------------|------------------------------------------------|------------------------------------|
| data             | `data: Sequence`                               | Constructor parameter              |
| layout           | `layout: "vertical" \| "horizontal"`           | Not supported                      |
| stack_offset     | `stack_offset: "expand" \| "none" \| ...`      | `StackOffset(StackOffsetTypes)`    |
| sync_id          | `sync_id: str`                                 | Not supported                      |
| width            | `width: Union[int, str]`                       | `Width(Size)`                      |
| height           | `height: Union[int, str]`                      | `Height(Size)`                     |
| margin           | `margin: Dict[str, Any]`                       | Not supported                      |
| cartesian_grid   | `rx.recharts.cartesian_grid()`                 | `.CartesianGrid()`                 |
| x_axis           | `rx.recharts.x_axis()`                         | `.XAxis()`                         |
| y_axis           | `rx.recharts.y_axis()`                         | `.YAxis()`                         |
| legend           | `rx.recharts.legend()`                         | `.Legend()`                        |
| tooltip          | `rx.recharts.graphing_tooltip()`               | `.Tooltip()`                       |
| toolbox          | Not supported (use Plotly)                     | `.Toolbox()`                       |
| color_scheme     | Manual per-area `fill`/`stroke`                | `ColorScheme(ColorScheme)`         |
| fill_opacity     | Per-area `fill_opacity`                        | `.FillOpacity(double)`             |
| brush            | `rx.recharts.brush()`                          | Not supported                      |
| reference_line   | `rx.recharts.reference_line()`                 | Not supported                      |
| reference_area   | `rx.recharts.reference_area()`                 | Not supported                      |
| animation        | `is_animation_active`, `animation_duration`    | Not supported                      |
| on_click         | `on_click` event trigger                       | Not supported                      |

---

## Bar Chart

Displays comparative values across categories with rectangular bars.

### Reflex

```python
data = [
    {"name": "Jan", "uv": 4000, "pv": 2400},
    {"name": "Feb", "uv": 3000, "pv": 1398},
    {"name": "Mar", "uv": 2000, "pv": 9800},
]

def bar_chart_example():
    return rx.recharts.bar_chart(
        rx.recharts.bar(data_key="uv", stroke="#8884d8", fill="#8884d8"),
        rx.recharts.bar(data_key="pv", stroke="#82ca9d", fill="#82ca9d"),
        rx.recharts.x_axis(data_key="name"),
        rx.recharts.y_axis(),
        rx.recharts.legend(),
        rx.recharts.graphing_tooltip(),
        data=data,
    )
```

### Ivy

```csharp
var data = new[]
{
    new { Name = "Jan", Uv = 4000, Pv = 2400 },
    new { Name = "Feb", Uv = 3000, Pv = 1398 },
    new { Name = "Mar", Uv = 2000, Pv = 9800 },
};

data.ToBarChart()
    .Dimension("Name", e => e.Name)
    .Measure("Uv", e => e.Sum(f => f.Uv))
    .Measure("Pv", e => e.Sum(f => f.Pv))
    .XAxis()
    .YAxis()
    .Legend()
    .Tooltip();
```

### Parameters

| Parameter         | Reflex                                          | Ivy                                |
|-------------------|------------------------------------------------|------------------------------------|
| data              | `data: Sequence`                               | Constructor parameter              |
| layout            | `layout: "vertical" \| "horizontal"`           | `Layout(Layouts)`                  |
| bar_gap           | `bar_gap: int`                                 | `BarGap(int)`                      |
| bar_category_gap  | `bar_category_gap: Union[int, str]`            | `BarCategoryGap(object)`           |
| bar_size          | `bar_size: int`                                | `MaxBarSize(int?)`                 |
| stack_offset      | `stack_offset: "expand" \| "none" \| ...`      | `StackOffset(StackOffsetTypes)`    |
| width             | `width: Union[int, str]`                       | `Width(Size)`                      |
| height            | `height: Union[int, str]`                      | `Height(Size)`                     |
| color_scheme      | Manual per-bar `fill`                          | `ColorScheme(ColorScheme)`         |
| legend            | `rx.recharts.legend()`                         | `.Legend()`                        |
| tooltip           | `rx.recharts.graphing_tooltip()`               | `.Tooltip()`                       |
| toolbox           | Not supported (use Plotly)                     | `.Toolbox()`                       |
| reference_line    | `rx.recharts.reference_line()`                 | Not supported                      |
| brush             | `rx.recharts.brush()`                          | Not supported                      |
| animation         | `is_animation_active`, `animation_duration`    | Not supported                      |
| on_click          | `on_click` event trigger                       | Not supported                      |

---

## Line Chart

Displays trends and changes over time with lines connecting data points.

### Reflex

```python
data = [
    {"name": "Jan", "uv": 4000, "pv": 2400},
    {"name": "Feb", "uv": 3000, "pv": 1398},
    {"name": "Mar", "uv": 2000, "pv": 9800},
]

def line_chart_example():
    return rx.recharts.line_chart(
        rx.recharts.line(data_key="uv", stroke="#8884d8", type_="monotone"),
        rx.recharts.line(data_key="pv", stroke="#82ca9d", type_="monotone"),
        rx.recharts.x_axis(data_key="name"),
        rx.recharts.y_axis(),
        rx.recharts.cartesian_grid(stroke_dasharray="3 3"),
        rx.recharts.legend(),
        rx.recharts.graphing_tooltip(),
        data=data,
    )
```

### Ivy

```csharp
var data = new[]
{
    new { Name = "Jan", Uv = 4000, Pv = 2400 },
    new { Name = "Feb", Uv = 3000, Pv = 1398 },
    new { Name = "Mar", Uv = 2000, Pv = 9800 },
};

data.ToLineChart()
    .Dimension("Name", e => e.Name)
    .Measure("Uv", e => e.Sum(f => f.Uv))
    .Measure("Pv", e => e.Sum(f => f.Pv))
    .XAxis()
    .YAxis()
    .CartesianGrid()
    .Legend()
    .Tooltip();
```

### Parameters

| Parameter         | Reflex                                          | Ivy                                |
|-------------------|------------------------------------------------|------------------------------------|
| data              | `data: Sequence`                               | Constructor parameter              |
| layout            | `layout: "vertical" \| "horizontal"`           | `Layout(Layouts)`                  |
| line type/curve   | `type_: "basis" \| "monotone" \| ...`          | Style: Default (spline), Custom (linear) |
| stroke_width      | Per-line `stroke_width`                        | `.StrokeWidth(int)`                |
| dot               | Per-line `dot: Union[dict, bool]`              | Not supported                      |
| connect_nulls     | Per-line `connect_nulls: bool`                 | Not supported                      |
| stroke_dasharray  | Per-line `stroke_dasharray: str`               | Not supported                      |
| width             | `width: Union[int, str]`                       | `Width(Size)`                      |
| height            | `height: Union[int, str]`                      | `Height(Size)`                     |
| color_scheme      | Manual per-line `stroke`                       | `ColorScheme(ColorScheme)`         |
| legend            | `rx.recharts.legend()`                         | `.Legend()`                        |
| tooltip           | `rx.recharts.graphing_tooltip()`               | `.Tooltip()`                       |
| toolbox           | Not supported (use Plotly)                     | `.Toolbox()`                       |
| reference_line    | `rx.recharts.reference_line()`                 | Not supported                      |
| brush             | `rx.recharts.brush()`                          | Not supported                      |
| animation         | `is_animation_active`, `animation_duration`    | Not supported                      |
| on_click          | `on_click` event trigger                       | Not supported                      |
| sorting           | Not built-in                                   | `.SortBy(SortOrder)`               |

---

## Pie Chart

Circular chart divided into slices to illustrate numerical proportion.

### Reflex

```python
data = [
    {"name": "Group A", "value": 400},
    {"name": "Group B", "value": 300},
    {"name": "Group C", "value": 300},
    {"name": "Group D", "value": 200},
]

def pie_chart_example():
    return rx.recharts.pie_chart(
        rx.recharts.pie(
            data=data,
            data_key="value",
            name_key="name",
            fill="#8884d8",
            label=True,
        ),
        rx.recharts.graphing_tooltip(),
    )
```

### Ivy

```csharp
var data = new[]
{
    new { Name = "Group A", Value = 400 },
    new { Name = "Group B", Value = 300 },
    new { Name = "Group C", Value = 300 },
    new { Name = "Group D", Value = 200 },
};

data.ToPieChart(e => e.Name, e => e.Value, PieChartStyles.Dashboard)
    .Tooltip()
    .Legend();
```

### Parameters

| Parameter         | Reflex                                          | Ivy                                |
|-------------------|------------------------------------------------|------------------------------------|
| data              | Per-pie `data: Sequence`                       | Constructor parameter              |
| data_key          | `data_key: Union[int, str]`                    | Lambda in `ToPieChart`             |
| name_key          | `name_key: str`                                | Lambda in `ToPieChart`             |
| inner_radius      | `inner_radius: Union[int, str]`                | Supported (donut style)            |
| outer_radius      | `outer_radius: Union[int, str]`                | Supported                          |
| start_angle       | `start_angle: int`                             | Not supported                      |
| end_angle         | `end_angle: int`                               | Not supported                      |
| padding_angle     | `padding_angle: int`                           | Not supported                      |
| cx / cy           | `cx`, `cy` center position                     | Not supported                      |
| label             | `label: Union[dict, bool]`                     | Custom label layers                |
| label_line        | `label_line: Union[dict, bool]`                | Not supported                      |
| fill              | `fill: Union[str, Color]`                      | `ColorScheme(ColorScheme)`         |
| legend            | `rx.recharts.legend()`                         | `.Legend()`                        |
| tooltip           | `rx.recharts.graphing_tooltip()`               | `.Tooltip()`                       |
| toolbox           | Not supported                                  | `.Toolbox()`                       |
| style             | Manual styling                                 | `PieChartStyles` enum              |
| total             | Not built-in                                   | `.Total()` display                 |
| animation         | `is_animation_active`, `animation_duration`    | Not supported                      |
| on_click          | `on_click` event trigger                       | Not supported                      |
| drill-down        | Not built-in                                   | Supported                          |

---

## General Charting Components (Reflex)

Reflex provides shared sub-components used across chart types:

| Component                       | Description                                      | Ivy Equivalent       |
|---------------------------------|--------------------------------------------------|----------------------|
| `rx.recharts.x_axis`           | Configure X axis                                 | `.XAxis()`           |
| `rx.recharts.y_axis`           | Configure Y axis                                 | `.YAxis()`           |
| `rx.recharts.cartesian_grid`   | Grid lines behind chart                          | `.CartesianGrid()`   |
| `rx.recharts.legend`           | Chart legend                                     | `.Legend()`          |
| `rx.recharts.graphing_tooltip` | Hover tooltips                                   | `.Tooltip()`         |
| `rx.recharts.brush`            | Range selector for zooming                       | Not supported        |
| `rx.recharts.reference_line`   | Horizontal/vertical reference lines              | Not supported        |
| `rx.recharts.reference_area`   | Shaded reference region                          | Not supported        |
| `rx.recharts.reference_dot`    | Reference point marker                           | Not supported        |
| `rx.recharts.label`            | Label on axis or element                         | Not supported        |
| `rx.recharts.label_list`       | Auto-render labels for each data point           | Not supported        |
| N/A                             | Interactive toolbox (zoom, save, reset)          | `.Toolbox()`         |

---

## Key Differences

- **API Style**: Reflex uses a compositional component tree (children of chart component). Ivy uses a fluent builder pattern with method chaining.
- **Chart Library**: Reflex wraps Recharts (React). Ivy has built-in chart rendering.
- **Extensibility**: Reflex also supports Plotly and Matplotlib for advanced/scientific charts. Ivy focuses on four core chart types.
- **Interactivity**: Reflex provides event triggers (`on_click`, `on_mouse_enter`, etc.) and dynamic state updates. Ivy provides a Toolbox with zoom/save/reset.
- **Sorting**: Ivy has built-in `SortBy` for chart data ordering. Reflex relies on pre-sorting the data.
- **Styling**: Ivy provides `ColorScheme` and `PieChartStyles` enums for quick theming. Reflex requires manual color assignment per series.
