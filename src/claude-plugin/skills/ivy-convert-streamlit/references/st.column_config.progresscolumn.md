# st.column_config.ProgressColumn

Configures a progress column in `st.dataframe` or `st.data_editor`. It renders numeric values as visual progress bars within table cells. Progress columns are not editable.

## Streamlit

```python
import pandas as pd
import streamlit as st

data_df = pd.DataFrame({
    "sales": [200, 550, 1000, 80],
})

st.data_editor(
    data_df,
    column_config={
        "sales": st.column_config.ProgressColumn(
            "Sales volume",
            help="The sales volume in USD",
            format="$%f",
            min_value=0,
            max_value=1000,
        ),
    },
    hide_index=True,
)
```

## Ivy

Ivy does not have a built-in progress column type for tables. The standalone `Progress` widget can be used to display progress bars, and the `Table` widget's `Builder()` method allows custom cell rendering, but there is no direct equivalent of a progress column.

```csharp
// Standalone Progress widget
new Progress(75).Goal("75% Complete")

// Table with basic column configuration (no built-in progress column)
new Table<Sale>(sales)
    .Header(p => p.Volume, "Sales volume")
    .Align(p => p.Volume, Align.Right)
```

## Parameters

| Parameter   | Documentation                                                                                              | Ivy                                                         |
|-------------|------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------|
| label       | Column header text. If None, uses the column name.                                                         | `Header(p => p.Column, "label")`                            |
| help        | Tooltip on column header hover. Supports GitHub-flavored Markdown.                                         | `Help(p => p.Column, "tooltip")` (DataTable only)           |
| format      | Number format string: "plain", "localized", "percent", "dollar", "euro", "yen", "accounting", "bytes", "compact", "scientific", "engineering", or printf-style. | Not supported                                               |
| pinned      | Whether column stays visible when scrolling horizontally. Index columns are pinned by default.              | Not supported                                               |
| min_value   | Minimum value of the progress bar. Defaults to 0.                                                          | Progress widget is fixed 0–100                              |
| max_value   | Maximum value of the progress bar. Defaults to 100 for integers, 1.0 for floats.                           | Progress widget is fixed 0–100                              |
| step        | Number precision. Defaults to 1 for integers, 0.01 for floats.                                             | Not supported                                               |
| color       | Bar color. "auto" uses green for >50% and red for <50%. Accepts any CSS color string.                      | `ColorVariant` on standalone Progress widget                |
