# st.area_chart

Display an area chart. A simplified charting function that plots quantitative data with filled areas, supporting stacking, color mapping, and multiple data series.

## Streamlit

```python
import pandas as pd
import streamlit as st
from numpy.random import default_rng as rng

df = pd.DataFrame(rng(0).standard_normal((20, 3)), columns=["a", "b", "c"])
st.area_chart(df)
```

## Ivy

```csharp
public class BasicAreaChart : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Desktop = 186, Mobile = 80 },
            new { Month = "Feb", Desktop = 305, Mobile = 200 },
            new { Month = "Mar", Desktop = 237, Mobile = 120 },
        };

        return new AreaChart(data)
                .Area(new Area("Mobile").Fill(Colors.Red))
                .Area(new Area("Desktop").Fill(Colors.Blue))
                .CartesianGrid(new CartesianGrid().Horizontal())
                .Tooltip()
                .XAxis(new XAxis("Month").TickLine(false).AxisLine(false))
                .Legend()
                .Toolbox();
   }
}
```

## Parameters

| Parameter | Streamlit | Ivy |
|-----------|-----------|-----|
| data | `data` - Anything supported by `st.dataframe` (DataFrame, Array, etc.) | `data` - object passed to `AreaChart` constructor |
| x | `x` - Column name for x-axis; defaults to data index | `XAxis("column")` - configured via `.XAxis()` fluent method |
| y | `y` - Column name(s) for y-axis; defaults to all remaining columns | `Area("column")` - each series added via `.Area()` fluent method |
| x_label | `x_label` - Custom label for the x-axis | Not supported |
| y_label | `y_label` - Custom label for the y-axis | Not supported |
| color | `color` - Series colors as hex strings, RGB/RGBA tuples, or CSS color names | `.Fill(Colors.X)` / `.FillOpacity(decimal)` on each `Area` |
| stack | `stack` - Stacking behavior: `True`, `False`, `"normalize"`, or `"center"` | `.StackOffset(StackOffsetTypes)` on the chart |
| use_container_width | `use_container_width` - Deprecated; use `width="stretch"` | Not supported |
