# Brush

A slider overlay for charts that lets users select a visible range of data points, enabling interactive exploration of large datasets. It renders two draggable handles on the chart axis so the viewer can zoom into a specific region.

## Reflex

```python
def brush_simple():
    return rx.recharts.bar_chart(
        rx.recharts.bar(data_key="uv", stroke="#8884d8", fill="#8884d8"),
        rx.recharts.bar(data_key="pv", stroke="#82ca9d", fill="#82ca9d"),
        rx.recharts.brush(data_key="name", height=30, stroke="#8884d8"),
        rx.recharts.x_axis(data_key="name"),
        rx.recharts.y_axis(),
        data=data,
        width="100%",
        height=300,
    )
```

## Ivy

Ivy does not have a dedicated Brush component. The closest interactive chart feature is `.Toolbox()`, which adds an interactive toolbar to charts but does not provide a range-selection slider.

```csharp
// No direct Brush equivalent — Toolbox is the closest interactive feature
public class ChartWithToolbox : ViewBase
{
    public override object? Build()
    {
        var salesData = new[]
        {
            new { Month = "Jan", Desktop = 186, Mobile = 80 },
            new { Month = "Feb", Desktop = 305, Mobile = 200 },
            new { Month = "Mar", Desktop = 237, Mobile = 120 },
            new { Month = "Apr", Desktop = 289, Mobile = 190 },
        };

        return salesData.ToLineChart()
            .Dimension("Month", e => e.Month)
            .Measure("Desktop", e => e.Sum(f => f.Desktop))
            .Measure("Mobile", e => e.Sum(f => f.Mobile))
            .Toolbox();
    }
}
```

## Parameters

| Parameter        | Documentation                                              | Ivy           |
|------------------|------------------------------------------------------------|---------------|
| `data_key`       | Key of data displayed in the brush                         | Not supported |
| `stroke`         | Border/outline color of the brush                          | Not supported |
| `fill`           | Fill color of the brush area                               | Not supported |
| `height`         | Height of the brush component (default: 40)                | Not supported |
| `width`          | Width of the brush component (default: 0)                  | Not supported |
| `x`              | X-axis position of the brush                               | Not supported |
| `y`              | Y-axis position of the brush                               | Not supported |
| `traveller_width`| Width of the draggable handles (default: 5)                | Not supported |
| `gap`            | Spacing between refresh intervals (default: 1)             | Not supported |
| `start_index`    | Default start index of the visible range (default: 0)      | Not supported |
| `end_index`      | Default end index of the visible range                     | Not supported |
| `data`           | Data source for the brush                                  | Not supported |
| `on_change`      | Event triggered when the selected range changes            | Not supported |
