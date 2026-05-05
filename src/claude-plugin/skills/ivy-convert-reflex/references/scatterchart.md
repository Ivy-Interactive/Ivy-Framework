# Scatter Chart

A scatter chart displays data points at the intersection of x and y numerical values, combining them into single data points. It always has two value axes — one horizontal and one vertical — making it ideal for visualizing correlations, clusters, and distributions across two (or three, via dot size) dimensions.

## Reflex

```python
def scatter_simple():
    return rx.recharts.scatter_chart(
        rx.recharts.scatter(
            data=data01,
            fill="#8884d8",
        ),
        rx.recharts.x_axis(data_key="x", type_="number"),
        rx.recharts.y_axis(data_key="y"),
        width="100%",
        height=300,
    )
```

Multiple series with a Z-axis (dot size) and legend:

```python
def scatter_double():
    return rx.recharts.scatter_chart(
        rx.recharts.scatter(data=data01, fill="#8884d8", name="A"),
        rx.recharts.scatter(data=data02, fill="#82ca9d", name="B"),
        rx.recharts.cartesian_grid(stroke_dasharray="3 3"),
        rx.recharts.x_axis(data_key="x", type_="number"),
        rx.recharts.y_axis(data_key="y"),
        rx.recharts.z_axis(data_key="z", range=[60, 400], name="score"),
        rx.recharts.legend(),
        rx.recharts.graphing_tooltip(),
        width="100%",
        height=300,
    )
```

## Ivy

Ivy does not have a dedicated Scatter Chart widget. The supported chart types are:

| Chart Type | Builder Method   |
|------------|------------------|
| LineChart  | `.ToLineChart()` |
| BarChart   | `.ToBarChart()`  |
| AreaChart  | `.ToAreaChart()`  |
| PieChart   | `.ToPieChart()`  |

None of these chart types expose scatter/point-only rendering. There is no equivalent to plotting raw (x, y) coordinate pairs as individual dots.

```csharp
// Closest alternative: LineChart (but connects points with lines, not scatter dots)
new LineChart(data, new[]
{
    new Line("Series A").StrokeWidth(1)
})
.XAxis(new XAxis("x"))
.YAxis(new YAxis("y"))
.Tooltip(new Tooltip())
.Legend(new Legend())
.Height(300);
```

## Parameters

### ScatterChart (container)

| Parameter | Reflex                                                    | Ivy           |
|-----------|-----------------------------------------------------------|---------------|
| margin    | `Dict[str, Any]` — default `{"top":5,"right":5,"bottom":5,"left":5}` | Not supported |
| width     | `Union[str, int]` — default `"100%"`                     | `.Width()`    |
| height    | `Union[str, int]` — default `"100%"`                     | `.Height()`   |

### Scatter (data series)

| Parameter          | Reflex                                                         | Ivy                     |
|--------------------|----------------------------------------------------------------|-------------------------|
| data               | `Sequence` — the data source for points                       | Constructor parameter   |
| name               | `str` — series name shown in legend                           | `Line("name")`          |
| fill               | `Union[str, Color]` — dot color, default accent               | `.ColorScheme()`        |
| shape              | `"square" \| "circle" \| "cross" \| "diamond" \| "star" \| "triangle" \| "wye"` | Not supported           |
| legend_type        | `"circle" \| "cross" \| "diamond" \| "star" \| "triangle" \| "wye" \| "square"` | Not supported           |
| line               | `bool` — connect dots with a line, default `False`            | Always on (LineChart)   |
| line_type          | `"joint" \| "fitting"` — line interpolation style             | `LineChartStyles.*`     |
| x_axis_id          | `Union[str, int]` — bind to a specific X axis                 | Not supported           |
| y_axis_id          | `Union[str, int]` — bind to a specific Y axis                 | Not supported           |
| z_axis_id          | `Union[str, int]` — third dimension via dot size              | Not supported           |
| is_animation_active| `bool` — enable entry animation                               | Not supported           |
| animation_begin    | `int` — delay in ms before animation starts                   | Not supported           |
| animation_duration | `int` — animation duration in ms, default `1500`              | Not supported           |
| animation_easing   | `"ease" \| "ease-in" \| "ease-out" \| "ease-in-out" \| "linear"` | Not supported           |

### Chart sub-components

| Component        | Reflex                        | Ivy              |
|------------------|-------------------------------|------------------|
| CartesianGrid    | `rx.recharts.cartesian_grid()`| `.CartesianGrid()` |
| XAxis            | `rx.recharts.x_axis()`       | `.XAxis()`       |
| YAxis            | `rx.recharts.y_axis()`       | `.YAxis()`       |
| ZAxis            | `rx.recharts.z_axis()`       | Not supported    |
| Legend           | `rx.recharts.legend()`       | `.Legend()`       |
| Tooltip          | `rx.recharts.graphing_tooltip()` | `.Tooltip()`  |
| Toolbox          | Not available                 | `.Toolbox()`     |
| Brush            | `rx.recharts.brush()`        | Not supported    |
| ReferenceArea    | `rx.recharts.reference_area()`| Not supported   |
| ReferenceDot     | `rx.recharts.reference_dot()` | Not supported   |
| ReferenceLine    | `rx.recharts.reference_line()`| Not supported   |
| LabelList        | Child of Scatter              | Not supported    |
| ErrorBar         | Child of Scatter              | Not supported    |
