# MultiselectColumn

Configures a multiselect column in `st.dataframe` or `st.data_editor`. Users can select multiple options from a dropdown menu, with support for colored labels and freely typed options.

Ivy does not have an equivalent inline multiselect column for tables. The `DataTable` widget is read-only and does not support cell editing. The closest equivalent is the standalone `SelectInput` widget with multi-select enabled via a collection state type.

## Streamlit

```python
import pandas as pd
import streamlit as st

data_df = pd.DataFrame({
    "category": [
        ["exploration", "visualization"],
        ["llm", "visualization"],
        ["exploration"],
    ],
})

st.data_editor(
    data_df,
    column_config={
        "category": st.column_config.MultiselectColumn(
            "App Categories",
            help="The categories of the app",
            options=["exploration", "visualization", "llm"],
            color=["#ffa421", "#803df5", "#00c0f2"],
            format_func=lambda x: x.capitalize(),
        ),
    },
)
```

## Ivy

```csharp
public class MultiSelectDemo : ViewBase
{
    public override object? Build()
    {
        var categories = UseState<string[]>([]);

        var options = new[] { "exploration", "visualization", "llm" }.ToOptions();

        return categories.ToSelectInput(options)
            .Variant(SelectInputs.Select)
            .Placeholder("Choose categories...")
            .WithField()
            .Label("App Categories")
            .Help("The categories of the app");
    }
}
```

## Parameters

| Parameter          | Documentation                                                                 | Ivy                                                         |
|--------------------|-------------------------------------------------------------------------------|-------------------------------------------------------------|
| label              | The label shown at the top of the column. If None, the column name is used.   | `.WithField().Label()`                                      |
| help               | Tooltip text displayed on column label hover. Supports Markdown.              | `.WithField().Help()`                                       |
| disabled           | Whether editing should be disabled for this column. Default: False.           | `.Disabled()`                                               |
| required           | Whether edited cells need to have a value other than None. Default: False.    | `.WithField().Required()`                                   |
| pinned             | Whether the column stays visible on the left during horizontal scrolling.     | Not supported                                               |
| default            | Default value when a user adds a new row. Accepts an iterable of strings.     | Initial state value (e.g. `UseState<string[]>([])`)         |
| options            | Available options for selection during editing. Accepts an iterable of strings. | `new[] { ... }.ToOptions()`                                |
| accept_new_options | Whether the user can add selections not included in options. Default: False.  | Not supported                                               |
| color              | Color for labels. Accepts CSS names, hex codes, RGB/HSL, or "primary".       | Not supported                                               |
| format_func        | Function to modify the display of options. Receives the raw option value.     | Not supported                                               |
