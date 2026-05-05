# SelectboxColumn

Configure a selectbox column in `st.dataframe` or `st.data_editor`. This column type renders a dropdown menu for selecting from a predefined list of options, and is the default for Pandas categorical values. When used with `st.data_editor`, it enables inline dropdown editing.

## Streamlit

```python
import pandas as pd
import streamlit as st

data_df = pd.DataFrame({
    "category": [
        "Data Exploration",
        "Data Visualization",
        "LLM",
        "Data Exploration",
    ]
})

st.data_editor(
    data_df,
    column_config={
        "category": st.column_config.SelectboxColumn(
            "App Category",
            help="The category of the app",
            options=[
                "Data Exploration",
                "Data Visualization",
                "LLM",
            ],
            required=True,
        )
    },
    hide_index=True,
)
```

## Ivy

Ivy's `DataTable` does not support inline cell editing or dropdown columns. The closest standalone equivalent is `SelectInput`, which provides a dropdown selection outside of a table context.

```csharp
var category = UseState("Data Exploration");

category.ToSelectInput(
    new[] { "Data Exploration", "Data Visualization", "LLM" }.ToOptions()
)
.WithField()
.Label("App Category")
.Help("The category of the app");
```

## Parameters

| Parameter   | Documentation                                                                 | Ivy                                                           |
|-------------|-------------------------------------------------------------------------------|---------------------------------------------------------------|
| label       | The label shown at the top of the column. Defaults to the column name.        | `.Label()` on a `SelectInput` field, `.Header()` on a column  |
| help        | Tooltip on column header hover. Supports GitHub-flavored Markdown.            | `.Help()` on a `SelectInput` field or DataTable column        |
| disabled    | Whether editing is disabled for the column.                                   | `.Disabled()` on `SelectInput`; no column-level equivalent    |
| required    | Whether edited cells must have a value. Defaults to `False`.                  | Not supported                                                 |
| pinned      | Whether the column is pinned to the left when scrolling.                      | `FreezeColumns` on DataTable (freezes N leftmost columns)     |
| default     | Default value when a new row is added.                                        | Not supported                                                 |
| options     | The list of selectable options during editing.                                | Options collection passed to `SelectInput`                    |
| format_func | Function to modify option display labels. Receives raw value, returns string. | Not supported                                                 |
