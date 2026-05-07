# st.scatter_chart

Displays a scatterplot chart. This is a simplified wrapper around `st.altair_chart` that automatically determines the chart's Altair specification from the data's columns and indices, making it ideal for quick plotting with minimal configuration.

## Streamlit

```python
import pandas as pd
import streamlit as st
from numpy.random import default_rng as rng

df = pd.DataFrame(rng(0).standard_normal((20, 3)), columns=["col1", "col2", "col3"])
df["col4"] = rng(0).choice(["a", "b", "c"], 20)

st.scatter_chart(df, x="col1", y="col2", color="col4", size="col3")
```

## Ivy

Ivy does not have a dedicated scatter chart widget. The available chart types are `LineChart`, `BarChart`, `AreaChart`, and `PieChart`. A `LineChart` is the closest equivalent but does not support hiding the connecting lines to produce a scatter-style visualization.

```csharp
// No direct equivalent — closest approximation using LineChart
salesData
    .ToLineChart()
    .Dimension("Month")
    .Measure("Revenue", Aggregate.Sum)
    .Toolbox()
    .Legend();
```

## Parameters

| Parameter | Streamlit | Ivy |
|-----------|-----------|-----|
| data | Data to be plotted. Supports DataFrames, arrays, and other `st.dataframe`-compatible types. | `Data` property / first argument to chart builder (e.g. `.ToLineChart()`) |
| x | Column name for x-axis values. Uses the data index if `None`. | `.Dimension()` method specifying the category field |
| y | Column name(s) for y-axis values. Plots all remaining numeric columns if `None`. | `.Measure()` method specifying the value field and aggregation |
| x_label | Custom label for the x-axis. Defaults to the column name. | `.XAxis()` configuration |
| y_label | Custom label for the y-axis. Defaults to the column name. | `.YAxis()` configuration |
| color | Circle color(s) as hex string, RGB/RGBA tuple, CSS color name, or a column name for categorical color mapping. | `.ColorScheme()` (preset palettes only, no column-based mapping) |
| size | Circle size as a fixed number or a column name for variable sizing. | Not supported |
| use_container_width | **Deprecated.** Use `width="stretch"` instead. | `.Width()` / `.Height()` on the chart |
