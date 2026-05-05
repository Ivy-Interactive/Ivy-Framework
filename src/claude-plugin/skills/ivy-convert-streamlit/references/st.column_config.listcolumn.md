# st.column_config.ListColumn

Configures a list column in `st.dataframe` or `st.data_editor`. This is the default column type for list-like values. When used with `st.data_editor`, users can add new options and remove existing ones.

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
        "sales": st.column_config.ListColumn(
            "Sales (last 6 months)",
            help="The sales volume in the last 6 months",
        ),
    },
    hide_index=True,
)
```

## Ivy

Ivy's `DataTable` does not have a dedicated list column type. Column configuration is done through a fluent API on the `DataTable` widget, but there is no built-in renderer for displaying list values inside a cell.

```csharp
// No direct equivalent — DataTable supports column config but not list-type columns.
// The closest approach is using a custom Renderer on a DataTable column.

var data = new[]
{
    new { Name = "Product A", Sales = "0, 4, 26, 80, 100, 40" },
    new { Name = "Product B", Sales = "80, 20, 80, 35, 40, 100" },
};

new DataTable(data)
    .Column(nameof(Sales), c => c
        .Header("Sales (last 6 months)")
        .Help("The sales volume in the last 6 months")
    );
```

## Parameters

| Parameter | Documentation                                                                     | Ivy                                          |
|-----------|-----------------------------------------------------------------------------------|----------------------------------------------|
| label     | Display label shown at the top of the column. Uses column name if `None`.         | `.Header("...")`                             |
| help      | Tooltip on hover over the column label. Supports GitHub-flavored Markdown.        | `.Help("...")`                               |
| pinned    | Whether the column stays visible when scrolling horizontally.                     | Not supported                                |
| disabled  | If `True`, prevents the user from editing cells in this column.                   | Not supported (DataTable is read-only)       |
| required  | If `True`, cells must have a non-`None` value before form submission.             | Not supported                                |
| default   | Default value (`Iterable[str]`) used when users add new rows.                     | Not supported                                |
