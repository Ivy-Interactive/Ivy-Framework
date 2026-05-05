# st.dataframe

Display data as an interactive table with sorting, filtering, and selection support. Accepts dataframe-like and collection-like objects.

## Streamlit

```python
import pandas as pd
import streamlit as st

df = pd.DataFrame({
    "name": ["Roadmap", "Extras", "Issues"],
    "url": [
        "https://roadmap.streamlit.app",
        "https://extras.streamlit.app",
        "https://issues.streamlit.app",
    ],
    "stars": [723, 291, 118],
})

st.dataframe(
    df,
    column_config={
        "name": "App name",
        "stars": st.column_config.NumberColumn("GitHub Stars", format="%d ⭐"),
        "url": st.column_config.LinkColumn("App URL"),
    },
    hide_index=True,
)
```

## Ivy

```csharp
var apps = new[]
{
    new { Name = "Roadmap", Url = "https://roadmap.streamlit.app", Stars = 723 },
    new { Name = "Extras", Url = "https://extras.streamlit.app", Stars = 291 },
    new { Name = "Issues", Url = "https://issues.streamlit.app", Stars = 118 },
};

apps.ToDataTable()
    .Header(a => a.Name, "App name")
    .Header(a => a.Stars, "GitHub Stars")
    .Header(a => a.Url, "App URL")
    .Config(c => c.ShowIndexColumn(false));
```

## Parameters

| Parameter | Streamlit | Ivy |
|-----------|-----------|-----|
| data | `data` - The dataset to display (DataFrame, dict, iterable, etc.). If unrecognized, attempts `.to_pandas()` or `.to_arrow()` conversion. | `.ToDataTable()` extension method on any `IEnumerable<T>`. Uses Apache Arrow internally. |
| hide_index | `hide_index` (`bool \| None`) - Controls whether the index column is visible. `None` auto-determines based on data. | `.Config(c => c.ShowIndexColumn(bool))` - Show or hide the row index column. |
| column_order | `column_order` (`Iterable[str] \| None`) - Ordered list of column names to display. Unlisted columns are hidden. | `.Header()` call order combined with `.Order()` to control column sequence. Use `.Remove()` to hide columns. |
| column_config | `column_config` (`dict \| None`) - Dict mapping column names to display configuration (label, type, format). Supports `NumberColumn`, `LinkColumn`, etc. | Fluent column methods: `.Header()` for labels, `.Width()` for sizing, `.Align()` for alignment, `.Icon()` for header icons, `.Help()` for tooltips. |
| on_select | `on_select` (`"ignore" \| "rerun" \| callable`) - Behavior when a selection is made: ignore it, rerun the app, or call a function. | `OnCellClick` / `OnCellActivated` event handlers with `CellClickEventArgs` providing row, column, and value info. |
| selection_mode | `selection_mode` (`str \| Iterable`) - Allowed selection types: `"single-row"`, `"multi-row"`, `"single-column"`, `"multi-column"`, `"single-cell"`, `"multi-cell"`, or a combination. | `.Config(c => c.SelectionMode(...))` - Supports `None`, `Cells`, `Rows`, `Columns`. |
| row_height | `row_height` (`int \| None`) - Row height in pixels. `None` uses default single-line height. | Not supported |
| placeholder | `placeholder` (`str \| None`) - Text shown for missing values. Defaults to `"None"`. | Not supported |
| use_container_width | `use_container_width` (`bool \| None`) - Deprecated. Stretches table to container width. | `.Width(Size.Full())` for full-width, or `.Width(Size.Units(n))` for fixed pixel width. |
