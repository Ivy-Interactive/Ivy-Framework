# Legend

A legend displays labels for each data series in a chart, helping readers understand what each line, bar, or area represents. In Reflex it is a standalone `rx.recharts.legend()` component placed inside a chart. In Ivy it is a configuration method `.Legend()` chained onto a chart builder.

## Reflex

```python
import reflex as rx

def legend_props():
    return rx.recharts.composed_chart(
        rx.recharts.line(
            data_key="pv",
            type_="monotone",
            stroke=rx.color("accent", 7),
        ),
        rx.recharts.line(
            data_key="amt",
            type_="monotone",
            stroke=rx.color("green", 7),
        ),
        rx.recharts.line(
            data_key="uv",
            type_="monotone",
            stroke=rx.color("red", 7),
        ),
        rx.recharts.x_axis(data_key="name"),
        rx.recharts.y_axis(),
        rx.recharts.legend(
            width=60,
            height=100,
            layout="vertical",
            align="right",
            vertical_align="top",
            icon_size=15,
            icon_type="square",
        ),
        data=data,
        width="100%",
        height=300,
    )
```

## Ivy

```csharp
public class LegendExample : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Apples = 100, Oranges = 40, Blueberry = 35 },
            new { Month = "Feb", Apples = 150, Oranges = 60, Blueberry = 55 },
            new { Month = "Mar", Apples = 170, Oranges = 70, Blueberry = 65 },
        };

        return Layout.Vertical()
            | new BarChart(data,
                    new Bar("Apples")
                        .Fill(Colors.Red)
                        .LegendType(LegendTypes.Square))
                .Bar(new Bar("Oranges")
                        .Fill(Colors.Orange)
                        .LegendType(LegendTypes.Square))
                .Bar(new Bar("Blueberry")
                        .Fill(Colors.Blue)
                        .Name("Blueberries")
                        .LegendType(LegendTypes.Square))
                .Tooltip()
                .Legend();
    }
}
```

## Parameters

| Parameter        | Documentation                                                                 | Ivy                                                                 |
|------------------|-------------------------------------------------------------------------------|---------------------------------------------------------------------|
| `width`          | Sets the width of the legend container                                        | Not supported (legend sizes automatically)                          |
| `height`         | Sets the height of the legend container                                       | Not supported (legend sizes automatically)                          |
| `layout`         | Orientation: `"vertical"` or `"horizontal"`                                   | `.Legend().Layout()` with `Layouts` enum                             |
| `align`          | Horizontal position: `"left"`, `"center"`, `"right"`                          | Not supported                                                       |
| `vertical_align` | Vertical position: `"top"`, `"middle"`, `"bottom"`                            | `.Legend().VerticalAlign()`                                          |
| `icon_size`      | Size of the legend icon markers (default `14`)                                | Not supported                                                       |
| `icon_type`      | Shape of the icon: `"circle"`, `"cross"`, `"square"`, etc.                    | `.LegendType(LegendTypes.Square)` on individual series (Bar/Line/Area) |
| `payload`        | Custom legend data items                                                      | Not supported (derived from series automatically)                   |
| `chart_width`    | Width of the associated chart                                                 | `.Width()` on the chart itself                                      |
| `chart_height`   | Height of the associated chart                                                | `.Height()` on the chart itself                                     |
| `margin`         | Margin around the legend                                                      | Not supported                                                       |
| `on_click`       | Event trigger when a legend item is clicked                                   | Not supported                                                       |
| N/A              | N/A                                                                           | `.Name()` on a series to override its legend label                  |
| N/A              | N/A                                                                           | `.ColorScheme()` on the chart to control legend colors              |
