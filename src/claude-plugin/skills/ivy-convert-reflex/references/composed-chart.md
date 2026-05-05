# ComposedChart

A higher-level charting component that layers multiple chart types (Area, Bar, Line) on top of each other within a single coordinated visualization. Children are rendered in the order they are provided, allowing combinations like a bar chart with an overlaid line trend.

## Reflex

```python
def composed():
    return rx.recharts.composed_chart(
        rx.recharts.area(data_key="uv", stroke="#8884d8", fill="#8884d8"),
        rx.recharts.bar(data_key="amt", bar_size=20, fill="#413ea0"),
        rx.recharts.line(data_key="pv", type_="monotone", stroke="#ff7300"),
        rx.recharts.x_axis(data_key="name"),
        rx.recharts.y_axis(),
        rx.recharts.cartesian_grid(stroke_dasharray="3 3"),
        rx.recharts.graphing_tooltip(),
        data=data,
        height=250,
        width="100%",
    )
```

## Ivy

Ivy does not have a dedicated ComposedChart widget. Each chart type (LineChart, BarChart, AreaChart) is a standalone component. There is no built-in way to layer multiple chart types onto a single set of axes.

The closest approximation is placing individual charts in a vertical layout:

```csharp
// No direct equivalent — Ivy charts are standalone per type
var data = new[]
{
    new { Name = "Page A", Uv = 590, Pv = 800, Amt = 1400 },
    new { Name = "Page B", Uv = 868, Pv = 967, Amt = 1506 },
    new { Name = "Page C", Uv = 1397, Pv = 1098, Amt = 989 },
    new { Name = "Page D", Uv = 1480, Pv = 1200, Amt = 1228 },
    new { Name = "Page E", Uv = 1520, Pv = 1108, Amt = 1100 },
    new { Name = "Page F", Uv = 1400, Pv = 680, Amt = 1700 },
};

// Individual charts only — cannot overlay Area + Bar + Line together
new BarChart(data, new Bar("Amt").Fill("#413ea0"))
    .XAxis(new XAxis("Name"))
    .Tooltip()
    .Legend();
```

## Parameters

| Parameter             | Documentation                                                            | Ivy                                              |
|-----------------------|--------------------------------------------------------------------------|--------------------------------------------------|
| `data`                | The source data, an array of objects                                     | Passed as first constructor arg to each chart     |
| `layout`              | `"horizontal"` or `"vertical"` orientation                               | `Layouts.Horizontal` / `Layouts.Vertical` on each chart |
| `width`               | Chart width (string or int), default `"100%"`                            | `.Width()` method on each chart                   |
| `height`              | Chart height (string or int), default `"100%"`                           | `.Height()` method on each chart                  |
| `bar_gap`             | Gap in px between bars in the same category, default `4`                 | `.BarGap()` on BarChart                           |
| `bar_category_gap`    | Gap between bar categories, default `"10%"`                              | `.BarCategoryGap()` on BarChart                   |
| `bar_size`            | Custom width of each bar in px                                           | Not supported                                     |
| `base_value`          | Base value for area charts (`"dataMin"`, `"dataMax"`, `"auto"`)          | Not supported                                     |
| `stack_offset`        | Stacking strategy (`"expand"`, `"none"`, etc.)                           | `.StackOffset()` on AreaChart                     |
| `reverse_stack_order` | Reverses the stacking order of children                                  | Not supported                                     |
| `margin`              | Chart margin as `Dict[str, Any]`                                         | Not supported                                     |
| `sync_id`             | Sync identifier for coordinating multiple charts                         | Not supported                                     |
| `sync_method`         | Sync method: `"index"` or `"value"`                                      | Not supported                                     |
| `on_click`            | Event handler for click on the chart                                     | Not supported                                     |
| Children: `Area`      | Area series child component                                              | Standalone `AreaChart` only                       |
| Children: `Bar`       | Bar series child component                                               | Standalone `BarChart` only                        |
| Children: `Line`      | Line series child component                                              | Standalone `LineChart` only                       |
| Children: `XAxis`     | X-axis configuration                                                     | `.XAxis(new XAxis(...))`                          |
| Children: `YAxis`     | Y-axis configuration                                                     | `.YAxis(new YAxis(...))`                          |
| Children: `CartesianGrid` | Grid lines overlay                                                   | `.CartesianGrid()`                                |
| Children: `Legend`     | Chart legend                                                             | `.Legend()`                                       |
| Children: `GraphingTooltip` | Hover tooltip                                                      | `.Tooltip()`                                      |
| Children: `Brush`     | Range selector for zooming                                               | Not supported                                     |
| Children: `ReferenceArea` | Highlighted reference region                                         | `.ReferenceAreas()` on individual charts          |
| Children: `ReferenceLine` | Reference line overlay                                               | `.ReferenceLines()` on individual charts          |
| Children: `ReferenceDot`  | Reference dot overlay                                                | `.ReferenceDots()` on LineChart                   |
