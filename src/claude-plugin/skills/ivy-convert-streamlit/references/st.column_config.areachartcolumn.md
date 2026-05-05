# st.column_config.AreaChartColumn

Configures an area chart column in `st.dataframe` or `st.data_editor`. Each cell renders an inline area chart from a list of numbers, useful for showing trends like sparklines directly inside a table.

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
        "sales": st.column_config.AreaChartColumn(
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

Ivy does not support inline chart columns within its DataTable. The closest equivalent is the standalone `AreaChart` widget, which displays a full area chart from a data source.

```csharp
salesData.ToAreaChart()
    .Dimension("Month", e => e.Month)
    .Measure("Sales", e => e.Sum(f => f.Sales))
    .Tooltip()
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| label | The label shown at the top of the column. If None, the column name is used. | Not supported (standalone chart has no column label; use `.Measure("name", ...)` for series naming) |
| help | Tooltip displayed when hovering over the column header. Supports GitHub-flavored Markdown. | Not supported as inline column; DataTable columns support `.Help()` for tooltips |
| pinned | Whether the column stays visible when scrolling horizontally. | Not supported as inline column; DataTable supports `.Config(c => c.FreezeColumns(n))` |
| y_min | Minimum value on the y-axis. If None, uses the minimum value in the data. | `.YAxis(y => y.Domain(min, max))` on standalone AreaChart |
| y_max | Maximum value on the y-axis. If None, uses the maximum value in the data. | `.YAxis(y => y.Domain(min, max))` on standalone AreaChart |
| color | The chart color. Can be a named color, hex string, or "auto"/"auto-inverse" for theme-aware coloring. | `.ColorScheme(ColorScheme.Default)` or `area.Fill(Colors.Blue)` on standalone AreaChart |
