# st.column_config.Column

Configure a generic column in `st.dataframe` or `st.data_editor`. The column type is automatically inferred from the data types. This must be used within the `column_config` parameter of `st.dataframe` or `st.data_editor`.

## Streamlit

```python
import pandas as pd
import streamlit as st

data_df = pd.DataFrame({
    "widgets": ["st.selectbox", "st.number_input", "st.text_area", "st.button"],
})

st.data_editor(
    data_df,
    column_config={
        "widgets": st.column_config.Column(
            "Streamlit Widgets",
            help="Streamlit **widget** commands",
            required=True,
        )
    },
    hide_index=True,
    num_rows="dynamic",
)
```

## Ivy

```csharp
DataTable<Widget>(widgets)
    .Header(w => w.Name, "Streamlit Widgets")
    .Help(w => w.Name, "Streamlit **widget** commands")
```

## Parameters

| Parameter  | Documentation                                                                                                                                       | Ivy                  |
|------------|-----------------------------------------------------------------------------------------------------------------------------------------------------|----------------------|
| `label`    | The label shown at the top of the column. If None, the column name is used.                                                                         | `.Header()`          |
| `help`     | A tooltip displayed when hovering over the column label. Supports GitHub-flavored Markdown.                                                         | `.Help()`            |
| `disabled` | Whether editing should be disabled for this column. Defaults to allowing edits where possible.                                                      | Not supported        |
| `required` | Whether edited cells need a value. When True, cells require non-None values before submission.                                                      | Not supported        |
| `pinned`   | Whether the column is pinned. A pinned column stays visible on the left side no matter where the user scrolls. Index columns are pinned by default. | Not supported        |
