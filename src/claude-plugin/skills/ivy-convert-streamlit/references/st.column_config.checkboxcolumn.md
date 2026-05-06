# CheckboxColumn

Configures a checkbox column in a data table. This is the default column type for boolean values. When used with an editable table, it renders a checkbox widget for toggling values.

## Streamlit

```python
import pandas as pd
import streamlit as st

data_df = pd.DataFrame({
    "widgets": ["st.selectbox", "st.number_input", "st.text_area", "st.button"],
    "favorite": [True, False, False, True],
})

st.data_editor(
    data_df,
    column_config={
        "favorite": st.column_config.CheckboxColumn(
            "Your favorite?",
            help="Select your **favorite** widgets",
            default=False,
        )
    },
    disabled=["widgets"],
    hide_index=True,
)
```

## Ivy

Ivy's `DataTable` does not have a dedicated CheckboxColumn type. Boolean columns are rendered based on the underlying data type without a specialized checkbox builder. Column-level configuration is done through fluent API methods on the `DataTable`.

```csharp
record Widget(string Name, bool Favorite);

var data = new[]
{
    new Widget("SelectInput", true),
    new Widget("NumberInput", false),
    new Widget("TextArea", false),
    new Widget("Button", true),
};

data.AsQueryable()
    .ToDataTable(e => e.Name)
    .Header(e => e.Favorite, "Your favorite?")
    .Help(e => e.Favorite, "Select your **favorite** widgets")
    .Hidden(e => e.Name, false);
```

## Parameters

| Parameter  | Documentation                                                              | Ivy                                                         |
|------------|----------------------------------------------------------------------------|-------------------------------------------------------------|
| `label`    | The heading displayed at the top of the column. Defaults to column name.   | `Header(e => e.Prop, "label")`                              |
| `help`     | Tooltip shown on hover over column label. Supports Markdown.               | `Help(e => e.Prop, "text")`                                 |
| `disabled` | Whether editing is disabled for this column.                               | Not supported (DataTable is read-only)                      |
| `required` | Whether cells must have a value when adding new rows.                      | Not supported                                               |
| `pinned`   | Whether the column stays visible during horizontal scrolling.              | `Config(c => c.FreezeColumns = n)` (freezes first n columns)|
| `default`  | Default value when users add new rows.                                     | Not supported (DataTable is read-only)                      |
