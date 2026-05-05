# st.column_config.BarChartColumn

Configures a bar chart column in `st.dataframe` or `st.data_editor`, rendering inline sparkline-style bar charts from list data within table cells. Cells must contain lists of numbers. The column is read-only.

Ivy does not support embedding charts inside table cells. The closest equivalent is the standalone `BarChart` widget.

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
        "sales": st.column_config.BarChartColumn(
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
var data = new[]
{
    new { Month = "Jan", Sales = 80 },
    new { Month = "Feb", Sales = 20 },
    new { Month = "Mar", Sales = 80 },
    new { Month = "Apr", Sales = 35 },
    new { Month = "May", Sales = 40 },
    new { Month = "Jun", Sales = 100 },
};

return data.ToBarChart()
    .Dimension("Month", e => e.Month)
    .Measure("Sales", e => e.Sum(f => f.Sales))
    .Tooltip()
    .ColorScheme(ColorScheme.Default);
```

## Parameters

| Parameter | Documentation                                                    | Ivy                                      |
|-----------|------------------------------------------------------------------|------------------------------------------|
| label     | Column header display text. Defaults to the column name.         | `.Measure("Label", ...)` or `.XAxis()`   |
| help      | Tooltip shown on hover. Supports GitHub-flavored Markdown.       | `.Tooltip()`                             |
| pinned    | Pins the column so it stays visible when scrolling horizontally. | Not supported                            |
| y_min     | Minimum value on the y-axis across all cells.                    | `.YAxis()` (min not explicitly documented)|
| y_max     | Maximum value on the y-axis across all cells.                    | `.YAxis()` (max not explicitly documented)|
| color     | Bar color as hex string, named color, or "auto"/"auto-inverse".  | `.Fill(Colors.value)` / `.ColorScheme()` |
