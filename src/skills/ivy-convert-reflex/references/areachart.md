# AreaChart

Displays quantitative data using filled areas between a line connecting data points and the axis. Supports stacking, multiple axes, tooltips, legends, and animation.

## Reflex

```python
data = [
    {"Month": "Jan", "Desktop": 186, "Mobile": 80},
    {"Month": "Feb", "Desktop": 305, "Mobile": 200},
    {"Month": "Mar", "Desktop": 237, "Mobile": 120},
]

rx.recharts.area_chart(
    rx.recharts.area(data_key="Desktop", stroke="#8884d8", fill="#8884d8", stack_id="1"),
    rx.recharts.area(data_key="Mobile", stroke="#82ca9d", fill="#82ca9d", stack_id="1"),
    rx.recharts.x_axis(data_key="Month"),
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
var data = new[]
{
    new { Month = "Jan", Desktop = 186, Mobile = 80 },
    new { Month = "Feb", Desktop = 305, Mobile = 200 },
    new { Month = "Mar", Desktop = 237, Mobile = 120 },
};

new AreaChart(data)
    .Area(new Area("Desktop", 1).Fill(Colors.Blue))
    .Area(new Area("Mobile", 1).Fill(Colors.Green))
    .XAxis(new XAxis("Month"))
    .YAxis(new YAxis())
    .CartesianGrid(new CartesianGrid())
    .Legend()
    .Tooltip()
    .Toolbox();
```

## Parameters

| Parameter | Reflex | Ivy |
|-----------|--------|-----|
| `data` | `data: Sequence` — chart data passed as prop | `object data` — passed via constructor |
| `width` | `width: Union[str, int]` — default `"100%"` | `Width: Size` — set via constructor |
| `height` | `height: Union[str, int]` — default `"100%"` | `Height: Size` — set via constructor |
| `layout` | `layout: "vertical" \| "horizontal"` — chart orientation | `Layout: Layouts` — via `.Layout()` setter |
| `stack_offset` | `stack_offset: "expand" \| "none" \| "wiggle" \| ...` | `StackOffset: StackOffsetTypes` — via `.StackOffset()` setter |
| `sync_id` | `sync_id: str` — synchronize multiple charts | Not supported |
| `sync_method` | `sync_method: "index" \| "value"` | Not supported |
| `base_value` | `base_value: "dataMin" \| "dataMax" \| "auto"` | Not supported |
| `margin` | `margin: Dict[str, Any]` — chart margins | Not supported |
| `color_scheme` | Not supported | `ColorScheme: ColorScheme` — via `.ColorScheme()` setter |
| `scale` | Not supported | `Scale: Scale?` — axis scale type |
| `toolbox` | Not supported | `Toolbox: Toolbox` — via `.Toolbox()` setter |
| `visible` | Not supported | `Visible: bool` — toggle visibility |
| `cartesian_grid` | Child component `rx.recharts.cartesian_grid()` | `CartesianGrid: CartesianGrid` — via `.CartesianGrid()` setter |
| `legend` | Child component `rx.recharts.legend()` | `Legend: Legend` — via `.Legend()` setter |
| `tooltip` | Child component `rx.recharts.graphing_tooltip()` | `Tooltip: Tooltip` — via `.Tooltip()` setter |
| `x_axis` | Child component `rx.recharts.x_axis()` | `XAxis: XAxis[]` — via `.XAxis()` setter |
| `y_axis` | Child component `rx.recharts.y_axis()` | `YAxis: YAxis[]` — via `.YAxis()` setter |
| `reference_lines` | Child component `rx.recharts.reference_line()` | `ReferenceLines: ReferenceLine[]` |
| `reference_areas` | Child component `rx.recharts.reference_area()` | `ReferenceAreas: ReferenceArea[]` |
| `reference_dots` | Child component `rx.recharts.reference_dot()` | `ReferenceDots: ReferenceDot[]` |
| `on_click` | `on_click` event trigger | Not supported |
| `on_animation_start` | `on_animation_start` event trigger | Not supported |
| `on_animation_end` | `on_animation_end` event trigger | Not supported |

### Area Series Parameters

| Parameter | Reflex | Ivy |
|-----------|--------|-----|
| `data_key` | `data_key: Union[str, int]` — field to plot | First constructor arg (name/key) |
| `fill` | `fill: Union[str, Color]` — area fill color | `.Fill(Colors.X)` — fill color |
| `stroke` | `stroke: Union[str, Color]` — line color | Not documented separately |
| `stroke_width` | `stroke_width: Union[str, int, float]` | Not documented separately |
| `type_` | `type_: "monotone" \| "basis" \| ...` — curve interpolation | Not supported |
| `stack_id` | `stack_id: Union[str, int]` — stacking group | Second constructor arg (stack group int) |
| `dot` | `dot: Union[dict, bool]` — show data points | Not supported |
| `active_dot` | `active_dot: Union[dict, bool]` — hover dot style | Not supported |
| `connect_nulls` | `connect_nulls: bool` — bridge null gaps | Not supported |
| `is_animation_active` | `is_animation_active: bool` — enable animation | Not supported |
| `animation_duration` | `animation_duration: int` — ms, default 1500 | Not supported |
