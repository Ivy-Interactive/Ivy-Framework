# st.pydeck_chart

Draws a chart using the PyDeck library, supporting 3D maps, point clouds, and advanced geospatial visualizations. It wraps [pydeck](https://deckgl.readthedocs.io/) and renders interactive WebGL-based map layers with built-in selection support.

## Streamlit

```python
import pandas as pd
import pydeck as pdk
import streamlit as st
from numpy.random import default_rng as rng

df = pd.DataFrame(
    rng(0).standard_normal((1000, 2)) / [50, 50] + [37.76, -122.4],
    columns=["lat", "lon"],
)

st.pydeck_chart(
    pdk.Deck(
        map_style=None,
        initial_view_state=pdk.ViewState(
            latitude=37.76, longitude=-122.4, zoom=11, pitch=50
        ),
        layers=[
            pdk.Layer(
                "HexagonLayer", data=df, get_position="[lon, lat]",
                radius=200, elevation_scale=4
            ),
            pdk.Layer(
                "ScatterplotLayer", data=df, get_position="[lon, lat]",
                get_color="[200, 30, 0, 160]", get_radius=200
            ),
        ],
    )
)
```

## Ivy

Ivy has no built-in map or PyDeck widget. The closest option is embedding a map via `Iframe` or building a custom React map component with `ExternalWidget`.

```csharp
// Option A: Embed a map service via Iframe
new Iframe("https://maps.example.com/embed?lat=37.76&lon=-122.4&zoom=11");

// Option B: Build a custom deck.gl React component via ExternalWidget
[ExternalWidget(
    "frontend/dist/DeckGlMap.js",
    StylePath = "frontend/dist/style.css",
    ExportName = "DeckGlMap",
    GlobalName = "MyProject_Widgets_DeckGlMap")]
public record DeckGlMap : WidgetBase<DeckGlMap>
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public int Zoom { get; init; }
}
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| pydeck_obj | `pydeck.Deck` or `None` - the PyDeck chart specification to render | Not supported (no native map widget) |
| use_container_width | `bool` or `None` - deprecated, use `width="stretch"` instead | Not supported |
| selection_mode | `"single-object"` or `"multi-object"` - restricts selection to one or multiple map objects | Not supported |
| on_select | `"ignore"`, `"rerun"`, or `callable` - behaviour when a user selects objects on the map | Not supported |
