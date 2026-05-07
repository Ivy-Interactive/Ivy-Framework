# Reference (ReferenceLine, ReferenceDot, ReferenceArea)

Visual aids and annotations for charts that highlight specific data points, ranges, or thresholds. Includes ReferenceLine (horizontal/vertical lines at specified positions), ReferenceDot (markers at specific coordinates), and ReferenceArea (highlighted rectangular regions).

## Reflex

```python
def reference_line():
    return rx.recharts.area_chart(
        rx.recharts.area(data_key="pv", stroke="#8884d8", fill="#8884d8"),
        rx.recharts.reference_line(x="Page C", stroke="red", label="Max PV PAGE"),
        rx.recharts.reference_line(y=9800, stroke="green", label="Max"),
        rx.recharts.reference_line(y=4343, stroke="green", label="Average"),
        rx.recharts.x_axis(data_key="name"),
        rx.recharts.y_axis(),
        data=data,
        height=300,
        width="100%",
    )

def reference_dot():
    return rx.recharts.scatter_chart(
        rx.recharts.scatter(data=data, fill="#8884d8", name="A"),
        rx.recharts.x_axis(data_key="x", name="x", type_="number"),
        rx.recharts.y_axis(data_key="y", name="y", type_="number"),
        rx.recharts.reference_dot(x=160, y=350, r=15, fill="blue", stroke="black"),
        rx.recharts.reference_dot(x=170, y=300, r=20, fill="green"),
        height=200,
        width="100%",
    )

def reference_area():
    return rx.recharts.scatter_chart(
        rx.recharts.scatter(data=data, fill="#8884d8", name="A"),
        rx.recharts.reference_area(
            x1=150, x2=180, y1=150, y2=300, fill="#8884d8", fill_opacity=0.3
        ),
        rx.recharts.x_axis(data_key="x", name="x", type_="number"),
        rx.recharts.y_axis(data_key="y", name="y", type_="number"),
        height=300,
        width="100%",
    )
```

## Ivy

```csharp
// ReferenceLine - available on LineChart, BarChart, and AreaChart
new LineChart(data, new Line("pv"))
    .ReferenceLine(x: null, y: 9800, label: "Max")
    .ReferenceLine(x: null, y: 4343, label: "Average")
    .XAxis()
    .YAxis()

// ReferenceDot - mark specific data points
new LineChart(data, new Line("pv"))
    .ReferenceDot(160, 350, label: "Outlier")
    .ReferenceDot(170, 300)

// ReferenceArea - highlight a rectangular region
new LineChart(data, new Line("pv"))
    .ReferenceArea(150, 150, 180, 300, label: "Target Range")
```

## Parameters

### ReferenceLine

| Parameter      | Reflex                                   | Ivy             |
|----------------|------------------------------------------|-----------------|
| x              | `Union[str, int]` - vertical line at x   | `double?` - vertical line at x |
| y              | `Union[str, int]` - horizontal line at y | `double?` - horizontal line at y |
| stroke         | `Union[str, Color]` - line color         | Not supported   |
| stroke_width   | `Union[str, int, float]` - line width (default: 1) | `int` StrokeWidth (default: 1) |
| label          | `Union[str, int]` - text label           | `string?` - text label |
| segment        | `Sequence` - line segment coordinates    | Not supported   |
| x_axis_id      | `Union[str, int]` - linked x-axis (default: 0) | Not supported |
| y_axis_id      | `Union[str, int]` - linked y-axis (default: 0) | Not supported |
| if_overflow    | `"discard" \| "hidden"` - overflow behavior (default: "discard") | Not supported |

### ReferenceDot

| Parameter      | Reflex                                   | Ivy             |
|----------------|------------------------------------------|-----------------|
| x              | `Union[str, int]` - x coordinate        | `double` - x coordinate |
| y              | `Union[str, int]` - y coordinate        | `double` - y coordinate |
| r              | `int` - dot radius                       | Not supported   |
| fill           | `Union[str, Color]` - fill color         | Not supported   |
| stroke         | `Union[str, Color]` - stroke color       | Not supported   |
| label          | `Union[str, int]` - text label           | `string?` - text label |
| x_axis_id      | `Union[str, int]` - linked x-axis (default: 0) | Not supported |
| y_axis_id      | `Union[str, int]` - linked y-axis (default: 0) | Not supported |
| if_overflow    | `"discard" \| "hidden"` - overflow behavior (default: "discard") | Not supported |
| on_click       | Event handler for click                  | Not supported   |

### ReferenceArea

| Parameter      | Reflex                                   | Ivy             |
|----------------|------------------------------------------|-----------------|
| x1             | `Union[str, int]` - start x coordinate  | `double` - start x coordinate |
| x2             | `Union[str, int]` - end x coordinate    | `double` - end x coordinate |
| y1             | `Union[str, int]` - start y coordinate  | `double` - start y coordinate |
| y2             | `Union[str, int]` - end y coordinate    | `double` - end y coordinate |
| stroke         | `Union[str, Color]` - border color       | Not supported   |
| fill           | `Union[str, Color]` - fill color         | Not supported   |
| fill_opacity   | `float` - fill transparency              | Not supported   |
| label          | Not a direct prop (use Label child)      | `string?` - text label |
| x_axis_id      | `Union[str, int]` - linked x-axis       | Not supported   |
| y_axis_id      | `Union[str, int]` - linked y-axis       | Not supported   |
| if_overflow    | `"discard" \| "hidden"` - overflow behavior (default: "discard") | Not supported |
