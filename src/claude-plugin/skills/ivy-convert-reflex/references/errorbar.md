# ErrorBar

An error bar is a graphical representation of the uncertainty or variability of a data point in a chart, depicted as a line extending from the data point parallel to one of the axes. In Reflex it is used as a child of scatter charts (via Recharts) to show error ranges on both X and Y axes.

## Reflex

```python
def error():
    return rx.recharts.scatter_chart(
        rx.recharts.scatter(
            rx.recharts.error_bar(
                data_key="errorY", direction="y", width=4, stroke_width=2, stroke="red"
            ),
            rx.recharts.error_bar(
                data_key="errorX", direction="x", width=4, stroke_width=2
            ),
            data=data,
            fill="#8884d8",
            name="A",
        ),
        rx.recharts.x_axis(data_key="x", name="x", type_="number"),
        rx.recharts.y_axis(data_key="y", name="y", type_="number"),
        width="100%",
        height=300,
    )
```

## Ivy

Ivy does not support error bars. Its charting library (LineChart, BarChart, AreaChart, PieChart) does not include a scatter chart or an error bar component. There is no equivalent widget or builder method.

```csharp
// Not supported — Ivy charts are limited to Line, Bar, Area, and Pie.
// There is no scatter chart or error bar component.
```

## Parameters

| Parameter      | Documentation                                                    | Ivy           |
|----------------|------------------------------------------------------------------|---------------|
| `direction`    | `"x" \| "y"` — axis orientation for the error bar               | Not supported |
| `data_key`     | `Union[str, int]` — identifies the data source for error values  | Not supported |
| `width`        | `int` (default `5`) — width of the error bar caps                | Not supported |
| `stroke`       | `Union[str, Color]` (default `rx.color("gray", 8)`) — line color | Not supported |
| `stroke_width` | `Union[str, int, float]` (default `1.5`) — line thickness        | Not supported |
