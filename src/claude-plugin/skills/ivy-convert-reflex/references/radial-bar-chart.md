# RadialBarChart

A circular visualization where data categories are represented by bars extending outward from a central point, with the length of each bar proportional to its value. Part of the Recharts integration in Reflex.

## Reflex

```python
def radial_bar_simple():
    return rx.recharts.radial_bar_chart(
        rx.recharts.radial_bar(
            data_key="x",
            min_angle=15,
        ),
        data=data,
        width="100%",
        height=500,
    )


def radial_bar_advanced():
    return rx.recharts.radial_bar_chart(
        rx.recharts.radial_bar(
            data_key="uv",
            min_angle=90,
            background=True,
            label={"fill": "#666", "position": "insideStart"},
        ),
        data=data_radial_bar,
        inner_radius="10%",
        outer_radius="80%",
        start_angle=180,
        end_angle=0,
        width="100%",
        height=300,
    )
```

## Ivy

Ivy does not have a dedicated RadialBarChart widget. The closest equivalent is a `PieChart` configured as a donut chart using `InnerRadius` / `OuterRadius`, which can represent parts-of-a-whole data in a circular layout but does not support radial bars extending from a center point.

```csharp
// Closest approximation: PieChart with donut configuration
var data = new[]
{
    new PieChartData("Category A", 80),
    new PieChartData("Category B", 60),
    new PieChartData("Category C", 40),
};

return new PieChart(data)
    .Pie(new Pie("Measure", "Dimension")
        .InnerRadius("30%")
        .OuterRadius("90%")
        .LabelList(new LabelList("Measure")
            .Position(Positions.Outside)))
    .Legend(new Legend())
    .Tooltip(new ChartTooltip().Animated(true));
```

## Parameters

| Parameter          | Documentation                                                                 | Ivy                                                    |
|--------------------|-------------------------------------------------------------------------------|--------------------------------------------------------|
| `data`             | `Sequence` — the data source for the chart                                    | `Data` (object) on `PieChart`                          |
| `margin`           | `Dict[str, Any]` — chart margin, default `{"top":5,"right":5,"left":5,"bottom":5}` | Not supported                                          |
| `cx`               | `Union[str, int]` — x-coordinate of the center, default `"50%"`              | Not supported                                          |
| `cy`               | `Union[str, int]` — y-coordinate of the center, default `"50%"`              | Not supported                                          |
| `start_angle`      | `int` — start angle of the radial bars, default `0`                           | Not supported                                          |
| `end_angle`        | `int` — end angle of the radial bars, default `360`                           | Not supported                                          |
| `inner_radius`     | `Union[str, int]` — inner radius of the chart, default `"30%"`               | `InnerRadius` on `Pie` (e.g. `"40%"`)                 |
| `outer_radius`     | `Union[str, int]` — outer radius of the chart, default `"100%"`              | `OuterRadius` on `Pie` (e.g. `"90%"`)                 |
| `bar_category_gap` | `Union[str, int]` — gap between bar categories, default `"10%"`              | Not supported                                          |
| `bar_gap`          | `str` — gap between bars, default `4`                                         | Not supported                                          |
| `bar_size`         | `int` — size of each bar                                                      | Not supported                                          |
| `width`            | `Union[str, int]` — chart width, default `"100%"`                             | `Width` (Size) on `PieChart`                           |
| `height`           | `Union[str, int]` — chart height, default `"100%"`                            | `Height` (Size) on `PieChart`                          |
| `on_click`         | Event trigger — click handler for chart interactions                          | Not documented for `PieChart`                          |
| Children: `RadialBar` | Defines the data key and bar appearance                                    | `Pie` object via `.Pie()` builder                      |
| Children: `Legend` | Displays a chart legend                                                       | `Legend` via `.Legend()`                                |
| Children: `GraphingTooltip` | Hover tooltip for data points                                        | `Tooltip` via `.Tooltip()`                             |
| Children: `PolarAngleAxis` | Configures the angle axis                                              | Not supported                                          |
| Children: `PolarRadiusAxis` | Configures the radius axis                                            | Not supported                                          |
| Children: `PolarGrid` | Displays a polar coordinate grid                                           | Not supported                                          |
