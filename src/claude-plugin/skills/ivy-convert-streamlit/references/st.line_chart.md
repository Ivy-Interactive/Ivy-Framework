# st.line_chart

Displays a line chart. This is syntax-sugar around `st.altair_chart` that uses the data's own columns and indices to figure out the chart's Altair spec. Useful for quick visualizations at the cost of customization.

## Streamlit

```python
import pandas as pd
import streamlit as st
from numpy.random import default_rng as rng

df = pd.DataFrame(
    {
        "col1": list(range(20)) * 3,
        "col2": rng(0).standard_normal(60),
        "col3": ["a"] * 20 + ["b"] * 20 + ["c"] * 20,
    }
)

st.line_chart(df, x="col1", y="col2", color="col3")
```

## Ivy

```csharp
var data = new[]
{
    new { Month = "January", Desktop = 186, Mobile = 100 },
    new { Month = "February", Desktop = 305, Mobile = 200 },
    new { Month = "March", Desktop = 237, Mobile = 150 },
    new { Month = "April", Desktop = 203, Mobile = 180 },
    new { Month = "May", Desktop = 290, Mobile = 220 },
    new { Month = "June", Desktop = 265, Mobile = 250 },
};

return data.ToLineChart(style: LineChartStyles.Default)
    .Dimension("Month", e => e.Month)
    .Measure("Desktop", e => e.Sum(f => f.Desktop))
    .Measure("Mobile", e => e.Sum(f => f.Mobile))
    .Toolbox()
    .Legend();
```

## Parameters

| Parameter | Streamlit | Ivy |
|-----------|-----------|-----|
| data | `data` - Anything supported by `st.dataframe` (DataFrame, dict, array, etc.) | Data passed as first argument to `new LineChart(data, lines)` or via `.ToLineChart()` extension method on any enumerable |
| x | `x` - Column name for x-axis values; defaults to the data index | `.Dimension("name", e => e.Property)` - sets the x-axis field with a name and accessor expression |
| y | `y` - Column name or list of column names for y-axis data series; defaults to all remaining columns | `.Measure("name", e => e.Sum(f => f.Property))` - called once per series, with a name and aggregation expression |
| x_label | `x_label` - Custom label for the x-axis | `.XAxis()` configuration |
| y_label | `y_label` - Custom label for the y-axis | `.YAxis()` configuration |
| color | `color` - Line colors as hex string, RGB/RGBA tuple, color name, or column name for color grouping | `.ColorScheme(ColorScheme.Default)` - applies a color palette; individual line colors via `Line` configuration |
