# st.pyplot

Display a matplotlib.pyplot figure. Requires `matplotlib>=3.0.0` to be installed. Ivy does not support matplotlib directly but provides native chart widgets (BarChart, LineChart, AreaChart, PieChart) with a fluent builder API.

## Streamlit

```python
import matplotlib.pyplot as plt
import streamlit as st
from numpy.random import default_rng as rng

arr = rng(0).normal(1, 1, size=100)
fig, ax = plt.subplots()
ax.hist(arr, bins=20)

st.pyplot(fig)
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
            .Toolbox();
    }
}
```

## Parameters

| Parameter     | Documentation                                                                                                         | Ivy                                                                                          |
|---------------|-----------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------|
| fig           | The Matplotlib Figure object to render. When unspecified, uses the global figure (deprecated).                         | Not supported. Data is passed directly to chart constructors (e.g. `data.ToBarChart()`).     |
| clear_figure  | If True, clears the figure after rendering. Defaults to False when `fig` is set, True otherwise.                      | Not supported                                                                                |
