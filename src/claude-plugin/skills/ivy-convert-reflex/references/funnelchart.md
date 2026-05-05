# FunnelChart

A funnel chart is a graphical representation used to visualize how data moves through a process. The dependent variable's value diminishes in subsequent stages, making it useful to demonstrate the flow of users through a business or sales process.

## Reflex

```python
data = [
    {"value": 100, "name": "Visits", "fill": "#8884d8"},
    {"value": 80, "name": "Cart", "fill": "#83a6ed"},
    {"value": 50, "name": "Checkout", "fill": "#8dd1e1"},
    {"value": 40, "name": "Payment", "fill": "#82ca9d"},
    {"value": 26, "name": "Purchase", "fill": "#a4de6c"},
]

def funnel_simple():
    return rx.recharts.funnel_chart(
        rx.recharts.funnel(
            rx.recharts.label_list(
                position="right",
                data_key="name",
                fill="#000",
                stroke="none",
            ),
            data_key="value",
            data=data,
        ),
        rx.recharts.graphing_tooltip(),
        width="100%",
        height=250,
    )
```

## Ivy

Ivy does not have a dedicated FunnelChart widget. The closest alternative is a horizontal BarChart, which can visualize decreasing values across stages but lacks the tapered funnel shape.

```csharp
salesFunnel.ToBarChart()
    .Dimension("Stage", e => e.Stage)
    .Measure("Count", e => e.Sum(f => f.Count))
    .Tooltip()
    .Legend()
    .Toolbox();
```

## Parameters

### FunnelChart (container)

| Parameter   | Documentation                                                      | Ivy                                            |
|-------------|--------------------------------------------------------------------|-------------------------------------------------|
| `layout`    | Chart layout orientation. Default `"centric"`.                     | `Layout` property on BarChart (`Layouts` enum)  |
| `margin`    | Chart margins `{top, right, bottom, left}`. Default all `5`.      | Not supported                                   |
| `stroke`    | Stroke color for the chart outline.                                | Not supported                                   |
| `width`     | Width of the chart. Default `"100%"`.                              | `.Width()` method                               |
| `height`    | Height of the chart. Default `"100%"`.                             | `.Height()` method                              |
| `on_click`  | Event handler for click on the chart.                              | Not supported                                   |

### Funnel (data series)

| Parameter              | Documentation                                                         | Ivy                                          |
|------------------------|-----------------------------------------------------------------------|----------------------------------------------|
| `data`                 | Sequence of data items to render.                                     | Data source passed to `.ToBarChart()`        |
| `data_key`             | Key in data for the funnel values.                                    | `.Measure()` lambda                          |
| `name_key`             | Key in data for segment names. Default `"name"`.                      | `.Dimension()` lambda                        |
| `legend_type`          | Type of legend icon (`"line"`, `"circle"`, `"cross"`, etc.).          | `.LegendType()` on Bar                       |
| `is_animation_active`  | Whether animation is enabled. Default `True`.                         | Not supported                                |
| `animation_begin`      | Delay before animation starts (ms). Default `0`.                      | Not supported                                |
| `animation_duration`   | Duration of animation (ms). Default `1500`.                           | Not supported                                |
| `animation_easing`     | Easing function (`"ease"`, `"ease-in"`, `"ease-in-out"`, etc.).       | Not supported                                |
| `stroke`               | Stroke color for funnel segments. Default `rx.color("gray", 3)`.      | `.Fill()` on Bar for segment color           |
| `trapezoids`           | Custom trapezoid shapes for the funnel.                               | Not supported (no funnel shape)              |
