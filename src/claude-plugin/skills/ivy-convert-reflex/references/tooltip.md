# Graphing Tooltip

A tooltip component for charts that displays contextual popup information when hovering over data points, bars, lines, or other chart elements. In Reflex this is `rx.recharts.graphing_tooltip()`, added as a child of a chart component. In Ivy this is the `.Tooltip()` builder method chained onto a chart.

## Reflex

```python
def tooltip_example():
    return rx.recharts.bar_chart(
        rx.recharts.bar(data_key="sales", fill="#8884d8"),
        rx.recharts.x_axis(data_key="month"),
        rx.recharts.y_axis(),
        rx.recharts.graphing_tooltip(
            separator=": ",
            offset=10,
            cursor={"strokeWidth": 1, "fill": rx.color("gray", 3)},
            is_animation_active=True,
            animation_duration=1500,
            animation_easing="ease",
        ),
        data=data,
    )
```

## Ivy

```csharp
return data.ToBarChart()
    .Dimension("Month", e => e.Month)
    .Measure("Sales", e => e.Sum(f => f.Sales))
    .Tooltip()
    .Legend();
```

## Parameters

| Parameter            | Documentation                                                             | Ivy           |
|----------------------|---------------------------------------------------------------------------|---------------|
| `separator`          | Separator between name and value (default `":"`)                          | Not supported |
| `offset`             | Distance in px from the trigger point (default `10`)                      | Not supported |
| `filter_null`        | Whether to hide items with null values (default `True`)                   | Not supported |
| `cursor`             | Style of the cursor line/area shown on hover                              | Not supported |
| `view_box`           | The chart viewbox dimensions `{x, y, width, height}`                      | Not supported |
| `allow_escape_view_box` | Whether tooltip can render outside the chart area `{x: bool, y: bool}` | Not supported |
| `wrapper_style`      | CSS style for the outer tooltip wrapper                                   | Not supported |
| `content_style`      | CSS style for the tooltip content area                                    | Not supported |
| `label_style`        | CSS style for the tooltip label text                                      | Not supported |
| `is_animation_active`| Enable/disable tooltip show/hide animation (default `True`)               | Not supported |
| `animation_duration` | Animation duration in ms (default `1500`)                                 | Not supported |
| `animation_easing`   | Easing function for the animation (default `"ease"`)                      | Not supported |
| `active`             | Whether tooltip is visible initially (default `False`)                    | Not supported |
