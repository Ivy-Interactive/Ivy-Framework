# CartesianGrid

A visual grid overlay for Cartesian charts (line, bar, area, scatter) that renders horizontal and vertical reference lines across the chart area, improving data readability and interpretation.

## Reflex

```python
def cgrid_simple():
    return rx.recharts.line_chart(
        rx.recharts.line(
            data_key="pv",
        ),
        rx.recharts.x_axis(data_key="name"),
        rx.recharts.y_axis(),
        rx.recharts.cartesian_grid(stroke_dasharray="4 4"),
        data=data,
        width="100%",
        height=300,
    )
```

## Ivy

```csharp
var data = new[]
{
    new { Month = "Jan", Desktop = 186 },
    new { Month = "Feb", Desktop = 305 },
    new { Month = "Mar", Desktop = 237 },
};

data.ToLineChart()
    .Dimension("Month", e => e.Month)
    .Measure("Desktop", e => e.Sum(f => f.Desktop))
    .CartesianGrid(new CartesianGrid().Horizontal())
    .Toolbox();
```

## Parameters

| Parameter           | Documentation                                                                 | Ivy           |
|---------------------|-------------------------------------------------------------------------------|---------------|
| `horizontal`        | Enable/disable horizontal grid lines (default: `True`)                        | `.Horizontal()` on `CartesianGrid` object |
| `vertical`          | Enable/disable vertical grid lines (default: `True`)                          | `.Vertical()` on `CartesianGrid` object |
| `stroke_dasharray`  | SVG stroke dash pattern, e.g. `"4 4"` for dashed lines                        | Not supported |
| `horizontal_points` | Custom horizontal grid line positions as pixel offsets from top               | Not supported |
| `vertical_points`   | Custom vertical grid line positions as pixel offsets from left                | Not supported |
| `fill`              | Fill color for grid areas                                                     | Not supported |
| `fill_opacity`      | Opacity of the fill color                                                     | Not supported |
| `stroke`            | Line color (default: `rx.color("gray", 7)`)                                  | Not supported |
| `x`                 | X-coordinate offset of the grid (default: `0`)                                | Not supported |
| `y`                 | Y-coordinate offset of the grid (default: `0`)                                | Not supported |
| `width`             | Width of the grid component (default: `0`)                                    | Not supported |
| `height`            | Height of the grid component (default: `0`)                                   | Not supported |
