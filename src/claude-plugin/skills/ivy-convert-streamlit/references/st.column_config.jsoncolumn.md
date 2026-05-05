# st.column_config.JsonColumn

Configure a JSON column in `st.dataframe` or `st.data_editor`. Cells display JSON strings or JSON-compatible objects in a formatted view. These columns are currently read-only.

## Streamlit

```python
import pandas as pd
import streamlit as st

data_df = pd.DataFrame(
    {
        "json": [
            {"foo": "bar", "bar": "baz"},
            {"foo": "baz", "bar": "qux"},
            {"foo": "qux", "bar": "foo"},
            None,
        ],
    }
)

st.dataframe(
    data_df,
    column_config={
        "json": st.column_config.JsonColumn(
            "JSON Data",
            help="JSON strings or objects",
        ),
    },
    hide_index=True,
)
```

## Ivy

Ivy does not have a dedicated JSON column type within its `DataTable` or `Table` widgets. The closest equivalent is the standalone `Json` widget, which renders formatted, syntax-highlighted JSON data.

```csharp
public class JsonExample : ViewBase
{
    public override object? Build()
    {
        var data = new
        {
            foo = "bar",
            bar = "baz"
        };

        return new Json(System.Text.Json.JsonSerializer.Serialize(data));
    }
}
```

## Parameters

| Parameter | Documentation                                                                                                          | Ivy           |
|-----------|------------------------------------------------------------------------------------------------------------------------|---------------|
| label     | The label shown at the top of the column. If None, the column name is used.                                            | `Header()` on DataTable/Table column configuration |
| help      | A tooltip displayed when hovering over the column label. Supports GitHub-flavored Markdown.                             | `Help()` on DataTable column configuration |
| pinned    | Whether the column is pinned. A pinned column stays visible on the left side regardless of horizontal scroll position. | Not supported |
