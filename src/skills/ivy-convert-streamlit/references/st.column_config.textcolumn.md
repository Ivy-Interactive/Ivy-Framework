# st.column_config.TextColumn

Configures a text column in `st.dataframe` or `st.data_editor`. This is the default column type for string values. When used with `st.data_editor`, editing is done via a text input widget.

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
        "widgets": st.column_config.TextColumn(
            "Widgets",
            help="Streamlit **widget** commands",
            default="st.",
            max_chars=50,
            validate=r"^st\.[a-z_]+$",
        )
    },
    hide_index=True,
)
```

## Ivy

```csharp
var products = new List<Product>
{
    new("st.selectbox"),
    new("st.number_input"),
    new("st.text_area"),
    new("st.button"),
};

products.ToDataTable()
    .Header(p => p.Widget, "Widgets")
    .Help(p => p.Widget, "Streamlit **widget** commands")
    .Width(Size.Full());
```

## Parameters

| Parameter  | Documentation                                                                 | Ivy                                                              |
|------------|-------------------------------------------------------------------------------|------------------------------------------------------------------|
| label      | Header text displayed above the column. Uses column name if `None`.           | `Header(p => p.Column, "text")`                                  |
| help       | Tooltip shown on hover over the column header. Supports GitHub-flavored Markdown. | `Help(p => p.Column, "text")`                                    |
| disabled   | When `True`, disables editing for the column.                                 | Not supported (DataTable is read-only)                           |
| required   | When `True`, cells must contain non-None values before submission.            | Not supported                                                    |
| pinned     | When `True`, column stays visible on the left when scrolling horizontally.    | `Config(c => { c.FreezeColumns = N; })` (table-level, not per-column) |
| default    | Default value for new rows added by users.                                    | Not supported                                                    |
| max_chars  | Maximum character limit for text entries.                                     | Not supported                                                    |
| validate   | JavaScript-flavored regex pattern (e.g., `"^[a-z]+$"`) for input validation. | Not supported                                                    |
