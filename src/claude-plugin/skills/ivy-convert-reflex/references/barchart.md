# BarChart

A bar chart presents categorical data with rectangular bars whose heights or lengths are proportional to the values they represent. Supports multiple bars, stacked bars, ranged data, vertical/horizontal layout, and stateful interactivity.

## Reflex

```python
def bar_simple():
    return rx.recharts.bar_chart(
        rx.recharts.bar(
            data_key="uv",
            stroke=rx.color("accent", 9),
            fill=rx.color("accent", 8),
        ),
        rx.recharts.bar(
            data_key="pv",
            stroke=rx.color("green", 9),
            fill=rx.color("green", 8),
        ),
        rx.recharts.x_axis(data_key="name"),
        rx.recharts.y_axis(),
        rx.recharts.legend(),
        data=data,
        width="100%",
        height=250,
    )
```

## Ivy

```csharp
public class BarChartBasic : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Name = "Page A", Uv = 4000, Pv = 2400 },
            new { Name = "Page B", Uv = 3000, Pv = 1398 },
            new { Name = "Page C", Uv = 2000, Pv = 9800 },
        };

        return Layout.Vertical()
            | data.ToBarChart()
                    .Dimension("Name", e => e.Name)
                    .Measure("Uv", e => e.Sum(f => f.Uv))
                    .Measure("Pv", e => e.Sum(f => f.Pv))
                    .Legend()
                    .Toolbox();
    }
}
```

## Parameters

### BarChart

| Parameter | Reflex | Ivy |
|-----------|--------|-----|
| `bar_category_gap` | `Union[str, int]`, default `"10%"` — spacing between bar categories | `BarCategoryGap` — controls spacing between bar categories |
| `bar_gap` | `Union[str, int]`, default `4` — gap between individual bars | `BarGap` — sets gap between individual bars |
| `bar_size` | `int` — fixed size of bars | Not supported |
| `max_bar_size` | `int` — maximum bar width | `MaxBarSize` — limits maximum bar width |
| `stack_offset` | `"expand" \| "none"`, default `"none"` — stacking behavior | `StackOffset` (`StackOffsetTypes`) — defines stacking behavior |
| `reverse_stack_order` | `bool`, default `False` — reverses stack ordering | `ReverseStackOrder` — reverses stacked bar ordering |
| `data` | `Sequence` — the dataset to render | Constructor parameter: `new BarChart(data, bars)` |
| `margin` | `Dict[str, Any]` — chart margins | Not supported |
| `sync_id` | `str` — syncs multiple charts together | Not supported |
| `sync_method` | `"index" \| "value"`, default `"index"` — sync method | Not supported |
| `layout` | `"vertical" \| "horizontal"`, default `"horizontal"` — chart orientation | `Layout` (`Layouts`) — determines bar orientation |
| `width` | `Union[str, int]`, default `"100%"` — chart width | `Width` (`Size`) — sets chart width |
| `height` | `Union[str, int]`, default `"100%"` — chart height | `Height` (`Size`) — sets chart height |
| `on_click` | Event trigger for click on chart | Not supported |

### Bar

| Parameter | Reflex | Ivy |
|-----------|--------|-----|
| `data_key` | `Union[str, int]` — which data field to plot | `.Measure("field", aggregation)` via fluent API or `new Bar("field")` |
| `fill` | `Union[str, Color]`, default `Color("accent", 9)` — bar fill color | `.Fill(Colors.X)` — assigns bar color |
| `stroke` | `Union[str, Color]` — bar border color | Not supported |
| `stroke_width` | `Union[str, int, float]` — border width | Not supported |
| `background` | `bool`, default `False` — show background shape | Not supported |
| `stack_id` | `str` — groups bars into stacks | Not supported (stacking via constructor) |
| `name` | `Union[str, int]` — series display name | `.Name("label")` — renames bar series label |
| `radius` | `Union[int, Sequence]`, default `0` — bar corner radius | Not supported |
| `legend_type` | `"circle" \| "cross" \| ...` — legend icon shape | `.LegendType(LegendTypes.Square)` — specifies legend symbol shape |
| `label` | `Union[dict, bool]`, default `False` — bar labels | Not supported |
| `is_animation_active` | `bool`, default `True` — enable animation | Not supported |
| `animation_begin` | `int`, default `0` — animation delay (ms) | Not supported |
| `animation_duration` | `int`, default `1500` — animation length (ms) | Not supported |
| `animation_easing` | `"ease" \| "ease-in" \| ...`, default `"ease"` | Not supported |
| `x_axis_id` | `Union[str, int]`, default `0` — binds bar to an x-axis | `XAxis` — configures horizontal axis |
| `y_axis_id` | `Union[str, int]`, default `0` — binds bar to a y-axis | `YAxis` — configures vertical axis |
| `unit` | `Union[str, int]` — unit suffix for values | Not supported |
| `min_point_size` | `int` — minimum bar height for small values | Not supported |
| `on_click` | Event trigger for click on bar | Not supported |
| `on_animation_start` | Event trigger when animation starts | Not supported |
| `on_animation_end` | Event trigger when animation ends | Not supported |

### Chart Sub-Components

| Component | Reflex | Ivy |
|-----------|--------|-----|
| X Axis | `rx.recharts.x_axis()` child component | `.Dimension()` fluent method or `XAxis` property |
| Y Axis | `rx.recharts.y_axis()` child component | `YAxis` property |
| Legend | `rx.recharts.legend()` child component | `.Legend()` fluent method |
| Tooltip | `rx.recharts.graphing_tooltip()` child component | `.Tooltip()` fluent method |
| Cartesian Grid | `rx.recharts.cartesian_grid()` child component | `CartesianGrid` property |
| Reference Area | `rx.recharts.reference_area()` child component | `ReferenceAreas` property |
| Reference Line | `rx.recharts.reference_line()` child component | `ReferenceLines` property |
| Reference Dot | `rx.recharts.reference_dot()` child component | `ReferenceDots` property |
| Brush | `rx.recharts.brush()` child component | Not supported |
| Toolbox | Not supported | `.Toolbox()` — adds interactive chart tools |
| Color Scheme | Not supported | `ColorScheme` — applies preset color schemes |
