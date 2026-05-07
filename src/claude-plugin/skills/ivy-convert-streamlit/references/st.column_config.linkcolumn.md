# st.column_config.LinkColumn

Configures a link column in `st.dataframe` or `st.data_editor`. Cell values are strings displayed as clickable links. In `st.data_editor`, the column is editable via a text input widget. Supports regex-based display text, validation, and Material icons.

## Streamlit

```python
import pandas as pd
import streamlit as st

data_df = pd.DataFrame({
    "apps": [
        "https://roadmap.streamlit.app",
        "https://extras.streamlit.app",
    ],
    "creator": [
        "https://github.com/streamlit",
        "https://github.com/arnaudmiribel",
    ],
})

st.data_editor(
    data_df,
    column_config={
        "apps": st.column_config.LinkColumn(
            "Trending apps",
            help="The top trending Streamlit apps",
            validate=r"^https://[a-z]+\.streamlit\.app$",
            max_chars=100,
            display_text=r"https://(.*?)\.streamlit\.app"
        ),
        "creator": st.column_config.LinkColumn(
            "App Creator",
            display_text="Open profile"
        ),
    },
    hide_index=True,
)
```

## Ivy

In Ivy, clickable link columns are supported via `Builder(p => p.Column, f => f.Link())` on the Table widget, or via `Renderer(expr, new LinkDisplayRenderer { Type = LinkDisplayType.Url })` on the DataTable widget.

```csharp
public class LinkColumnExample : ViewBase
{
    public override object? Build()
    {
        var data = new[] {
            new { Apps = "https://roadmap.streamlit.app", Creator = "https://github.com/streamlit" },
            new { Apps = "https://extras.streamlit.app", Creator = "https://github.com/arnaudmiribel" },
        };

        return data.ToTable()
            .Header(p => p.Apps, "Trending apps")
            .Header(p => p.Creator, "App Creator")
            .Builder(p => p.Apps, f => f.Link())
            .Builder(p => p.Creator, f => f.Link());
    }
}
```

## Parameters

| Parameter    | Documentation                                                                                       | Ivy                                                        |
|--------------|-----------------------------------------------------------------------------------------------------|------------------------------------------------------------|
| label        | The label shown at the top of the column. Defaults to the column name.                              | `.Header(p => p.Column, "Label")`                          |
| help         | Tooltip displayed when hovering over the column label. Supports GitHub-flavored Markdown.           | `.Help(p => p.Column, "tooltip")` (DataTable only)         |
| disabled     | Whether editing should be disabled for this column.                                                 | Not supported                                              |
| required     | Whether edited cells in the column need to have a value.                                            | Not supported                                              |
| pinned       | Whether the column is pinned on the left during scrolling.                                          | `config.FreezeColumns = N` (DataTable only, by count)      |
| default      | Default value when a user adds a new row.                                                           | Not supported                                              |
| max_chars    | Maximum number of characters that can be entered.                                                   | Not supported                                              |
| validate     | A JS-flavored regular expression that edited values are validated against.                          | Not supported                                              |
| display_text | The display text for link cells. Can be a URL, static string, Material icon, or regex pattern.      | Not supported (always displays the URL)                    |
