# Label

Label is a component used to display a single label at a specific position within a chart or axis. LabelList automatically renders a list of labels for each data point in a chart series, providing a convenient way to display multiple labels without manually positioning each one.

## Reflex

```python
def label_simple():
    return rx.recharts.bar_chart(
        rx.recharts.cartesian_grid(stroke_dasharray="3 3"),
        rx.recharts.bar(
            rx.recharts.label_list(data_key="uv", position="top"),
            data_key="uv",
            fill=rx.color("accent", 8),
        ),
        rx.recharts.x_axis(
            rx.recharts.label(
                value="center",
                position="center",
                offset=30,
            ),
            rx.recharts.label(
                value="inside left",
                position="insideLeft",
                offset=10,
            ),
            height=50,
        ),
        data=data,
        width="100%",
        height=250,
    )
```

## Ivy

Ivy does not have a standalone `Label` component for charts. Axis labels are set implicitly via `XAxis("Label")`. `LabelList` is supported on `PieChart` for displaying data point labels with positioning and styling, but is not documented for cartesian charts (Bar, Line, Area).

```csharp
new PieChart()
    .Pie(new Pie(data, nameof(PieChartData.Measure))
        .LabelList(new LabelList(nameof(PieChartData.Measure))
            .Position(Positions.Outside)
            .Fill(Colors.Blue)
            .FontSize(11)
            .NumberFormat("$0,0"))
        .LabelList(new LabelList(nameof(PieChartData.Dimension))
            .Position(Positions.Inside)
            .Fill(Colors.White)
            .FontSize(9)
            .FontFamily("Arial")))
```

## Parameters

### rx.recharts.Label

| Parameter  | Documentation                                                        | Ivy                                                        |
|------------|----------------------------------------------------------------------|------------------------------------------------------------|
| value      | `str` - The text content of the label                                | Not supported (no standalone Label component)              |
| position   | `"top" \| "left" \| "right" \| "bottom" \| "center" \| ...` - Where the label is placed within the axis | Not supported                          |
| offset     | `int` - Fine-tunes the label's position                              | Not supported                                              |
| view_box   | `Dict[str, Any]` - The view box for the label                       | Not supported                                              |

### rx.recharts.LabelList

| Parameter | Documentation                                                          | Ivy                                                        |
|-----------|------------------------------------------------------------------------|------------------------------------------------------------|
| data_key  | `Union[str, int]` - The data column to use for label values            | Constructor arg: `new LabelList(fieldName)`                |
| position  | `"top" \| "left" \| "right" \| "bottom" \| "center" \| ...` - Label placement | `.Position(Positions.Outside \| Positions.Inside)` (PieChart only) |
| offset    | `int` (default `5`) - Adjusts label position                          | Not supported                                              |
| fill      | `Union[str, Color]` (default `rx.color("gray", 10)`) - Label color    | `.Fill(Colors.Blue \| Colors.White \| ...)`                |
| stroke    | `Union[str, Color]` (default `"none"`) - Label stroke color           | Not supported                                              |
| —         | —                                                                      | `.FontSize(int)` - Ivy-specific                            |
| —         | —                                                                      | `.FontFamily(string)` - Ivy-specific                       |
| —         | —                                                                      | `.NumberFormat(string)` - Ivy-specific (e.g. `"$0,0"`)     |
