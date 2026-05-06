# PieChart

A circular statistical graphic divided into slices to illustrate numerical proportion. Supports pie and donut styles, custom labels, tooltips, legends, animations, and dynamic data binding.

## Reflex

```python
def pie_simple():
    data = [
        {"name": "Group A", "value": 400},
        {"name": "Group B", "value": 300},
        {"name": "Group C", "value": 300},
        {"name": "Group D", "value": 200},
    ]
    return rx.recharts.pie_chart(
        rx.recharts.pie(
            data=data,
            data_key="value",
            name_key="name",
            fill="#8884d8",
            label=True,
        ),
        rx.recharts.graphing_tooltip(),
        width="100%",
        height=300,
    )
```

## Ivy

```csharp
public class PieChartDemo : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "January", Desktop = 186, Mobile = 100 },
            new { Month = "February", Desktop = 305, Mobile = 200 },
            new { Month = "March", Desktop = 237, Mobile = 300 },
        };

        return Layout.Vertical()
            | data.ToPieChart(
                e => e.Month,
                e => e.Sum(f => f.Mobile),
                PieChartStyles.Dashboard
            )
            .Tooltip(new ChartTooltip().Animated(true));
    }
}
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `data` | Data source for the chart slices. Reflex: `Sequence` passed to `rx.recharts.pie(data=...)` | `object data` passed to `new PieChart(data)` or via `.ToPieChart()` extension method |
| `data_key` | The key in each data item that maps to the slice value | `Measure` — second lambda in `.ToPieChart()` or `nameof(PieChartData.Measure)` in `Pie()` |
| `name_key` | The key in each data item that maps to the slice name/label | `Dimension` — first lambda in `.ToPieChart()` or `nameof(PieChartData.Dimension)` in `Pie()` |
| `fill` | Color of the pie slices (`Union[str, Color]`) | `.ColorScheme(ColorScheme.Default)` — colors are set via scheme, not per-slice fill |
| `cx` | X-coordinate of the center (`Union[str, int]`, default `"50%"`) | Not supported |
| `cy` | Y-coordinate of the center (`Union[str, int]`, default `"50%"`) | Not supported |
| `inner_radius` | Inner radius for donut shape (`Union[str, int]`, default `0`) | `.InnerRadius("40%")` on the `Pie` object |
| `outer_radius` | Outer radius of the pie (`Union[str, int]`, default `"80%"`) | `.OuterRadius("90%")` on the `Pie` object |
| `start_angle` | Start angle in degrees (`int`, default `0`) | Not supported |
| `end_angle` | End angle in degrees (`int`, default `360`) | Not supported |
| `min_angle` | Minimum angle for smallest slice (`int`, default `0`) | Not supported |
| `padding_angle` | Spacing angle between slices (`int`, default `0`) | Not supported |
| `label` | Show slice labels (`Union[dict, bool]`, default `False`) | `.LabelList(new LabelList(...).Position(...).Fill(...).FontSize(...))` — more granular per-layer label config |
| `label_line` | Show label connector lines (`Union[dict, bool]`, default `False`) | Controlled via `LabelList` positioning (`Positions.Outside` vs `Positions.Inside`) |
| `stroke` | Stroke color of slice borders (`Union[str, Color]`) | Not supported (controlled by color scheme) |
| `legend_type` | Shape of legend icon (`"circle"`, `"cross"`, `"rect"`, etc.) | `.Legend(new Legend().IconType(Legend.IconTypes.Rect))` |
| `is_animation_active` | Enable/disable animation (`bool`) | `.Animated(true)` on the `Pie` object |
| `animation_begin` | Delay before animation starts in ms (`int`, default `400`) | Not supported |
| `animation_duration` | Animation duration in ms (`int`, default `1500`) | Not supported |
| `animation_easing` | Easing function (`"ease"`, `"ease-in"`, etc.) | Not supported |
| `width` | Chart width (`Union[str, int]`, default `"100%"`) | `.Width(Size)` on `PieChart` |
| `height` | Chart height (`Union[str, int]`, default `"100%"`) | `.Height(Size)` on `PieChart` |
| `margin` | Chart margin (`Dict[str, Any]`) | Not supported (handled via layout) |
| `on_click` | Click event handler on slices | State-based drill-down via `UseState` |
| — | Not supported | `.Toolbox(new Toolbox())` — toolbar with save/reset actions |
| — | Not supported | `.Total(value, "label")` — summary total display in center |
| — | Not supported | `.ColorScheme(ColorScheme.Rainbow)` — named color scheme presets |
| — | Not supported | `PieChartStyles` enum (`Dashboard`, `Donut`) for quick chart style |
| — | Not supported | `.Scale(Scale)` — optional scaling configuration |
