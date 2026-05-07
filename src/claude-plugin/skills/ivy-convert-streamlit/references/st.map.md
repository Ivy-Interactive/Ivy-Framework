# st.map

Displays an interactive scatterplot map using OpenStreetMap tiles. Wraps `st.pydeck_chart` to quickly plot geographic coordinate data with automatic centering and zooming.

Ivy has no native map widget. The closest alternatives are embedding a third-party map via `Iframe`, or building a custom `ExternalWidget` with a React mapping library.

## Streamlit

```python
import pandas as pd
import streamlit as st
from numpy.random import default_rng as rng

df = pd.DataFrame(
    rng(0).standard_normal((1000, 2)) / [50, 50] + [37.76, -122.4],
    columns=["lat", "lon"],
)

st.map(df, size=20, color="#0044ff")
```

## Ivy

```csharp
// No native map widget exists in Ivy.
// Workaround: embed a third-party map via Iframe.
new Iframe("https://www.openstreetmap.org/export/embed.html?bbox=-122.5,37.7,-122.3,37.8&layer=mapnik")
    .Width(Size.Stretch)
    .Height(500);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| data | Data to plot. Accepts anything supported by `st.dataframe` (DataFrame, ndarray, Iterable, dict, etc.). | Not supported |
| latitude | Column name for latitude values. Auto-detects `lat`, `latitude`, `LAT`, `LATITUDE` when `None`. | Not supported |
| longitude | Column name for longitude values. Auto-detects `lon`, `longitude`, `LON`, `LONGITUDE` when `None`. | Not supported |
| color | Circle color. Accepts `None` (default), hex string (e.g. `"#ffaa00"`), RGB/RGBA tuple, or a column name for per-point colors. | Not supported |
| size | Circle size in meters. Accepts `None` (default), a number, or a column name for variable sizes. | Not supported |
| zoom | Zoom level per the OpenStreetMap spec. Auto-calculated when `None`. | Not supported |
