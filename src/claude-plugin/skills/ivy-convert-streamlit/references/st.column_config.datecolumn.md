# st.column_config.DateColumn

Configure a date column in `st.dataframe` or `st.data_editor`. This is the default column type for date values. When used with `st.data_editor`, editing is enabled via a date picker widget.

## Streamlit

```python
from datetime import date
import pandas as pd
import streamlit as st

data_df = pd.DataFrame({
    "birthday": [
        date(1980, 1, 1),
        date(1990, 5, 3),
        date(1974, 5, 19),
        date(2001, 8, 17),
    ]
})

st.data_editor(
    data_df,
    column_config={
        "birthday": st.column_config.DateColumn(
            "Birthday",
            min_value=date(1900, 1, 1),
            max_value=date(2005, 1, 1),
            format="DD.MM.YYYY",
            step=1,
        ),
    },
    hide_index=True,
)
```

## Ivy

The Ivy `DataTable` supports `DateTime`, `Date`, and `DateTimeOffset` columns with automatic type detection. Column display is configured via a fluent API. Date filtering uses ISO-8601 strings.

```csharp
record Person(int Id, string Name, DateTime Birthday);

var people = new[]
{
    new Person(1, "Alice",   new DateTime(1980, 1, 1)),
    new Person(2, "Bob",     new DateTime(1990, 5, 3)),
    new Person(3, "Charlie", new DateTime(1974, 5, 19)),
    new Person(4, "Diana",   new DateTime(2001, 8, 17)),
}
.AsQueryable();

return people.ToDataTable()
    .Header(p => p.Birthday, "Birthday")
    .Help(p => p.Birthday, "Date of birth")
    .Config(c => { c.AllowFiltering = true; });
```

## Parameters

| Parameter  | Documentation                                                                                      | Ivy                                                        |
|------------|----------------------------------------------------------------------------------------------------|------------------------------------------------------------|
| label      | Display text shown at the column top. If None, the column name is used.                            | `.Header(x => x.Col, "Label")`                             |
| help       | Tooltip on column header hover. Supports GitHub-flavored Markdown.                                 | `.Help(x => x.Col, "text")`                                |
| disabled   | Whether editing is disabled. Defaults to enabling edits where possible.                            | Not supported (DataTable is not an inline editor)           |
| required   | If True, edited cells must contain a value.                                                        | Not supported                                              |
| pinned     | Pins the column to the left side during horizontal scrolling.                                      | `Config(c => c.FreezeColumns = n)` freezes first n columns |
| default    | Default value for new rows added by the user. Accepts `datetime.date` or None.                     | Not supported                                              |
| format     | Display format string (e.g. `"DD.MM.YYYY"`). Supports `"localized"`, `"distance"`, `"iso8601"`.   | Not supported                                              |
| min_value  | Minimum selectable date. No minimum if None.                                                       | Not supported                                              |
| max_value  | Maximum selectable date. No maximum if None.                                                       | Not supported                                              |
| step       | Stepping interval in days for the date picker. Defaults to 1.                                      | Not supported                                              |
