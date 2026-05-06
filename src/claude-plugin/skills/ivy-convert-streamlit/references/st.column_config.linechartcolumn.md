# st.column_config.LineChartColumn

Configure a line chart column in `st.dataframe` or `st.data_editor`. Renders a sparkline (inline mini line chart) inside each table cell. Each cell must contain a list of numbers.

Ivy does not have a direct equivalent for embedding charts inside table cells. The closest approach is using a standalone `LineChart` widget alongside a `DataTable`, or using the `Table` widget's `Builder()` method for custom cell rendering.

## Streamlit

```python
import pandas as pd
import streamlit as st

data_df = pd.DataFrame({
    "sales": [
        [0, 4, 26, 80, 100, 40],
        [80, 20, 80, 35, 40, 100],
        [10, 20, 80, 80, 70, 0],
        [10, 100, 20, 100, 30, 100],
    ],
})

st.data_editor(
    data_df,
    column_config={
        "sales": st.column_config.LineChartColumn(
            "Sales (last 6 months)",
            help="The sales volume in the last 6 months",
            y_min=0,
            y_max=100,
        ),
    },
    hide_index=True,
)
```

## Ivy

```csharp
// Ivy does not support inline chart columns in tables.
// The closest alternative is a standalone LineChart:

record SalesData(string Month, double Sales);

var data = new[] {
    new SalesData("Jan", 0),
    new SalesData("Feb", 4),
    new SalesData("Mar", 26),
    new SalesData("Apr", 80),
    new SalesData("May", 100),
    new SalesData("Jun", 40),
};

data.ToLineChart()
    .Dimension(d => d.Month)
    .Measure(d => d.Sales);
```

## Parameters

| Parameter | Documentation                                                                                                          | Ivy           |
|-----------|------------------------------------------------------------------------------------------------------------------------|---------------|
| label     | The label shown at the top of the column. If None, the column name is used.                                            | `Header()`    |
| help      | A tooltip displayed when hovering over the column label. Supports GitHub-flavored Markdown.                             | `Help()`      |
| pinned    | Whether the column is pinned. Pinned columns stay visible when scrolling horizontally.                                 | `FreezeColumns` in `Config()` |
| y_min     | The minimum value on the y-axis for all cells in the column. If None, every cell uses its own minimum.                 | Not supported |
| y_max     | The maximum value on the y-axis for all cells in the column. If None, every cell uses its own maximum.                 | Not supported |
| color     | The color of the line chart. Supports hex codes like `"#483d8b"`, `"auto"` (green/red based on trend), and `"auto-inverse"`. | Not supported |
