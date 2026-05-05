# Axis

Configure and customize chart axes (X, Y, Z) to control appearance, behavior, and formatting of chart visualizations. Axes define data mapping, labels, tick marks, orientation, and scaling for Cartesian chart types.

## Reflex

```python
def axis_simple():
    return rx.recharts.area_chart(
        rx.recharts.area(
            data_key="uv",
            stroke=rx.color("accent", 9),
            fill=rx.color("accent", 8),
        ),
        rx.recharts.x_axis(
            data_key="name",
            label={"value": "Pages", "position": "bottom"},
        ),
        rx.recharts.y_axis(
            data_key="uv",
            label={"value": "Views", "angle": -90, "position": "left"},
        ),
        data=data,
        width="100%",
        height=300,
    )


def multi_axis():
    return rx.recharts.area_chart(
        rx.recharts.area(data_key="uv", stroke="#8884d8", fill="#8884d8", y_axis_id="left"),
        rx.recharts.area(data_key="pv", y_axis_id="right", stroke="#82ca9d", fill="#82ca9d"),
        rx.recharts.x_axis(data_key="name"),
        rx.recharts.y_axis(data_key="uv", y_axis_id="left"),
        rx.recharts.y_axis(data_key="pv", y_axis_id="right", orientation="right"),
        rx.recharts.legend(),
        data=data,
        width="100%",
        height=300,
    )
```

## Ivy

```csharp
new AreaChart(data, areas)
    .XAxis(new XAxis("Month").TickLine(false).AxisLine(false))
    .CartesianGrid(new CartesianGrid().Horizontal())
    .Tooltip()
    .Legend()
    .Toolbox();

// Bar chart example with XAxis configuration
new BarChart(data)
    .Bar(new Bar("Rating", 1).Radius(8).Fill(Colors.Purple))
    .Bar(new Bar("Change", 2).Radius(8).Fill(Colors.Orange))
    .CartesianGrid(new CartesianGrid().Horizontal())
    .XAxis(new XAxis("Language").TickLine(false).AxisLine(false))
    .Legend()
    .Toolbox();
```

## Parameters

### XAxis

| Parameter                  | Reflex                                        | Ivy                          |
|----------------------------|-----------------------------------------------|------------------------------|
| `data_key` / constructor   | `data_key: Union[str, int]`                   | Constructor arg: `"Name"`    |
| `orientation`              | `"top" \| "bottom"` (default `"bottom"`)      | Not documented               |
| `label`                    | `Union[str, int, dict]` with position/offset  | `.Label<XAxis>("text")`      |
| `axis_line`                | `bool` (default `True`)                       | `.AxisLine(bool)`            |
| `tick_line`                | `bool` (default `True`)                       | `.TickLine(bool)`            |
| `tick_count`               | `int` (default `5`)                           | Not documented               |
| `tick_size`                | `int` (default `6`)                           | Not documented               |
| `min_tick_gap`             | `int` (default `5`)                           | Not documented               |
| `ticks`                    | `Sequence`                                    | Not documented               |
| `tick`                     | `Union[bool, dict]`                           | Not documented               |
| `hide`                     | `bool` (default `False`)                      | Not documented               |
| `domain`                   | `Sequence` (default `[0, "auto"]`)            | Not documented               |
| `scale`                    | `"auto" \| "linear" \| "log" \| ...`          | Not documented               |
| `type_`                    | `"number" \| "category"`                      | Not documented               |
| `interval`                 | `"preserveStart" \| "preserveEnd" \| ...`     | Not documented               |
| `angle`                    | `int` (default `0`)                           | Not documented               |
| `x_axis_id`                | `Union[str, int]` (default `0`)               | Not documented               |
| `allow_decimals`           | `bool` (default `True`)                       | Not documented               |
| `allow_data_overflow`      | `bool` (default `False`)                      | Not documented               |
| `allow_duplicated_category`| `bool` (default `True`)                       | Not documented               |
| `reversed`                 | `bool` (default `False`)                      | Not documented               |
| `mirror`                   | `bool` (default `False`)                      | Not documented               |
| `unit`                     | `Union[str, int]`                             | Not documented               |
| `name`                     | `Union[str, int]`                             | Not documented               |
| `stroke`                   | `Union[str, Color]` (default `gray 9`)        | Not documented               |
| `text_anchor`              | `"start" \| "middle" \| "end"`                | Not documented               |
| `padding`                  | `Dict[str, int]` (default `{left:0, right:0}`)| Not documented               |
| `width` / `height`        | `Union[str, int]`                             | Not documented               |
| `include_hidden`           | `bool` (default `False`)                      | Not documented               |
| `on_click`                 | Event handler on tick click                   | Not documented               |

### YAxis

| Parameter                  | Reflex                                        | Ivy                          |
|----------------------------|-----------------------------------------------|------------------------------|
| `data_key` / constructor   | `data_key: Union[str, int]`                   | Via `.Measure()` on chart    |
| `orientation`              | `"left" \| "right"` (default `"left"`)        | Not documented               |
| `label`                    | `Union[str, int, dict]` with position/offset  | `.Label<YAxis>("text")`      |
| `axis_line`                | `bool` (default `True`)                       | Not documented               |
| `tick_line`                | `bool` (default `True`)                       | Not documented               |
| `y_axis_id`                | `Union[str, int]` (default `0`)               | Not documented               |
| All other XAxis params     | Same as XAxis (see above)                     | Not documented               |

### ZAxis

| Parameter                  | Reflex                                        | Ivy            |
|----------------------------|-----------------------------------------------|----------------|
| `data_key`                 | `Union[str, int]`                             | Not supported  |
| `z_axis_id`                | `Union[str, int]` (default `0`)               | Not supported  |
| `range`                    | `Sequence` (default `[60, 400]`)              | Not supported  |
| `unit`                     | `Union[str, int]`                             | Not supported  |
| `name`                     | `Union[str, int]`                             | Not supported  |
| `scale`                    | `"auto" \| "linear" \| ...`                   | Not supported  |
