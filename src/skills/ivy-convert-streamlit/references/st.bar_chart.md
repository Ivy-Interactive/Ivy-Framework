# st.bar_chart

Display a bar chart. Syntax sugar around `st.altair_chart` that uses the data's columns and indices to automatically determine the chart specification.

## Streamlit

```python
import pandas as pd
import streamlit as st
from numpy.random import default_rng as rng

df = pd.DataFrame(rng(0).standard_normal((20, 3)), columns=["a", "b", "c"])
st.bar_chart(df)
```

## Ivy

```csharp
public class BarChartBasic : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Desktop = 186, Mobile = 100 },
            new { Month = "Feb", Desktop = 305, Mobile = 200 },
            new { Month = "Mar", Desktop = 237, Mobile = 300 },
        };

        return data.ToBarChart()
            .Dimension("Month", e => e.Month)
            .Measure("Desktop", e => e.Sum(f => f.Desktop))
            .Measure("Mobile", e => e.Sum(f => f.Mobile))
            .Tooltip()
            .Legend()
            .Toolbox();
    }
}
```

## Parameters

| Parameter | Streamlit | Ivy |
|-----------|-----------|-----|
| data | `data` - Anything supported by `st.dataframe` (DataFrame, dict, array, etc.) | Constructor parameter or `.ToBarChart()` extension method on a collection |
| x | `x` (str) - Column name for the x-axis; uses index if None | `.Dimension("name", e => e.Property)` - defines the x-axis grouping |
| y | `y` (str or list of str) - Column name(s) for y-axis; all remaining columns if None | `.Measure("name", e => e.Sum(f => f.Property))` - one call per series |
| x_label | `x_label` (str) - Label for the x-axis | `.XAxis()` configuration |
| y_label | `y_label` (str) - Label for the y-axis | `.YAxis()` configuration |
| color | `color` (str, tuple, or sequence) - Color for the series; supports hex, RGB/RGBA, or color names | `.Fill(Colors.Red)` on individual `Bar` objects, or `.ColorScheme(ColorScheme.Rainbow)` on the chart |
| horizontal | `horizontal` (bool, default False) - Whether to make the bars horizontal | `.Layout(Layouts.Horizontal)` |
| sort | `sort` (bool or str, default True) - Sorting method for bars | `.SortBy(SortOrder.Ascending)` or `.SortBy(e => e.Key, SortOrder.Descending)` |
| stack | `stack` (bool, "normalize", "center", "layered", or None) - Stacking behavior | `.StackOffset(StackOffsetTypes.*)` and `.ReverseStackOrder(bool)` |
| use_container_width | `use_container_width` (bool) - Deprecated; use `width="stretch"` | Not supported |
