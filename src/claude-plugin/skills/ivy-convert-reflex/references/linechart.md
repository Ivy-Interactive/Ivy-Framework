# Line Chart

A line chart visualization component used to display information that changes over time by plotting a series of data points connected with lines. Supports multiple data series, customizable styling, tooltips, legends, and reference annotations.

## Reflex

```python
import reflex as rx

data = [
    {"month": "January", "desktop": 186, "mobile": 100},
    {"month": "February", "desktop": 305, "mobile": 200},
    {"month": "March", "desktop": 237, "mobile": 300},
    {"month": "April", "desktop": 186, "mobile": 100},
    {"month": "May", "desktop": 325, "mobile": 200},
]

def line_chart_example():
    return rx.recharts.line_chart(
        rx.recharts.line(data_key="desktop", stroke="#8884d8", type_="monotone"),
        rx.recharts.line(data_key="mobile", stroke="#82ca9d", type_="monotone"),
        rx.recharts.x_axis(data_key="month"),
        rx.recharts.y_axis(),
        rx.recharts.cartesian_grid(stroke_dasharray="3 3"),
        rx.recharts.legend(),
        rx.recharts.graphing_tooltip(),
        data=data,
        width="100%",
        height=300,
    )
```

## Ivy

```csharp
public class LineChartDemo : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "January", Desktop = 186, Mobile = 100 },
            new { Month = "February", Desktop = 305, Mobile = 200 },
            new { Month = "March", Desktop = 237, Mobile = 300 },
            new { Month = "April", Desktop = 186, Mobile = 100 },
            new { Month = "May", Desktop = 325, Mobile = 200 },
        };

        return data.ToLineChart(style: LineChartStyles.Default)
            .Dimension("Month", e => e.Month)
            .Measure("Desktop", e => e.Sum(f => f.Desktop))
            .Measure("Mobile", e => e.Sum(f => f.Mobile))
            .CartesianGrid()
            .Legend()
            .Tooltip()
            .Toolbox();
    }
}
```

## Parameters

### Chart Container

| Parameter | Reflex | Ivy |
|-----------|--------|-----|
| Data | `data` (Sequence) on `line_chart` | Constructor param or `.ToLineChart()` builder |
| Width | `width` (str \| int), default `"100%"` | `.Width(Size)` |
| Height | `height` (str \| int), default `"100%"` | `.Height(Size)` |
| Layout | `layout` (`"vertical"` \| `"horizontal"`) | `.Layout(Layouts)` |
| Margin / Spacing | `margin` (Dict) | Not supported |
| Sync ID | `sync_id` (str) | Not supported |
| Sync Method | `sync_method` (`"index"` \| `"value"`) | Not supported |
| Stack Offset | `stack_offset` (`"expand"` \| `"none"` \| ...) | Not supported |
| Color Scheme | Not supported (per-line `stroke`) | `.ColorScheme(ColorScheme)` |
| Scale | Not supported | `.Scale(Scale)` |
| Visibility | Not supported | `.Visible(bool)` |
| Sort Order | Not supported | `.SortBy(SortOrder)` |

### Line Series

| Parameter | Reflex | Ivy |
|-----------|--------|-----|
| Data Key | `data_key` (str \| int) on `line` | Constructor param or `.Measure()` builder |
| Interpolation | `type_` (`"monotone"` \| `"basis"` \| `"linear"` \| ...) | `LineChartStyles` (Default=spline, Custom=straight) |
| Stroke Color | `stroke` (str \| Color) | Controlled via `.ColorScheme()` |
| Stroke Width | `stroke_width` (str \| int \| float), default `1` | `.StrokeWidth(int)` on `Line` |
| Dot / Point | `dot` (dict \| bool) | Not supported |
| Active Dot | `active_dot` (dict \| bool) | Not supported |
| Hide | `hide` (bool) | Not supported |
| Connect Nulls | `connect_nulls` (bool) | Not supported |
| Unit | `unit` (str \| int) | Not supported |
| Dash Pattern | `stroke_dasharray` (str) | Not supported |
| Series Name | `name` (str \| int) | Name via constructor or `.Measure()` label |
| Label | `label` (dict \| bool) | Not supported |
| Animation Active | `is_animation_active` (bool), default `True` | Not supported |
| Animation Begin | `animation_begin` (int), default `0` | Not supported |
| Animation Duration | `animation_duration` (int), default `1500` | Not supported |
| Animation Easing | `animation_easing` (`"ease"` \| `"ease-in"` \| ...) | Not supported |
| Legend Type | `legend_type` (`"circle"` \| `"cross"` \| ...) | Not supported |
| X Axis ID | `x_axis_id` (str \| int) | Not supported (single X axis) |
| Y Axis ID | `y_axis_id` (str \| int) | Not supported (single Y axis) |

### Child Components / Features

| Feature | Reflex | Ivy |
|---------|--------|-----|
| X Axis | `rx.recharts.x_axis` child | `.XAxis()` |
| Y Axis | `rx.recharts.y_axis` child | `.YAxis()` |
| Cartesian Grid | `rx.recharts.cartesian_grid` child | `.CartesianGrid()` |
| Legend | `rx.recharts.legend` child | `.Legend()` |
| Tooltip | `rx.recharts.graphing_tooltip` child | `.Tooltip()` |
| Brush / Toolbox | `rx.recharts.brush` child | `.Toolbox()` |
| Reference Line | `rx.recharts.reference_line` child | `.ReferenceLine()` |
| Reference Area | `rx.recharts.reference_area` child | `.ReferenceArea()` |
| Reference Dot | `rx.recharts.reference_dot` child | `.ReferenceDot()` |
| Error Bar | `rx.recharts.error_bar` (child of Line) | Not supported |
| Label List | `rx.recharts.label_list` (child of Line) | Not supported |

### Events

| Event | Reflex | Ivy |
|-------|--------|-----|
| Click | `on_click` on chart and line | Not documented |
| Animation Start | `on_animation_start` on line | Not supported |
| Animation End | `on_animation_end` on line | Not supported |
