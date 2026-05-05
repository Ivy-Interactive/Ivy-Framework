# st.column_config.TimeColumn

Configures a time column in `st.dataframe` or `st.data_editor`. This is the default column type for time values. When used with `st.data_editor`, it provides a time picker widget for editing cells.

## Streamlit

```python
from datetime import time
import pandas as pd
import streamlit as st

data_df = pd.DataFrame({
    "appointment": [
        time(12, 30),
        time(18, 0),
        time(9, 10),
        time(16, 25),
    ]
})

st.data_editor(
    data_df,
    column_config={
        "appointment": st.column_config.TimeColumn(
            "Appointment",
            min_value=time(8, 0, 0),
            max_value=time(19, 0, 0),
            format="hh:mm a",
            step=60,
        ),
    },
    hide_index=True,
)
```

## Ivy

Ivy's `DataTable` supports `DateTime` and `TimeOnly` columns through automatic type detection. Column-level formatting and time-specific constraints (min/max, step) are not available; time display follows ISO-8601 filtering conventions. For standalone time input, Ivy provides `DateTimeInput` with a `.ToTimeInput()` variant.

```csharp
public record Appointment(int Id, TimeOnly Time, string Description);

public class AppointmentTableDemo : ViewBase
{
    public override object? Build()
    {
        var appointments = new[]
        {
            new Appointment(1, new TimeOnly(12, 30), "Lunch"),
            new Appointment(2, new TimeOnly(18, 0), "Dinner"),
            new Appointment(3, new TimeOnly(9, 10), "Standup"),
            new Appointment(4, new TimeOnly(16, 25), "Review"),
        }.AsQueryable();

        return appointments.ToDataTable(a => a.Id)
            .Header(a => a.Time, "Appointment")
            .Header(a => a.Description, "Description")
            .Help(a => a.Time, "Scheduled appointment time")
            .Sortable(a => a.Time, true)
            .SortDirection(a => a.Time, SortDirection.Ascending)
            .Config(c =>
            {
                c.AllowFiltering = true;
                c.AllowSorting = true;
            })
            .Height(Size.Units(100));
    }
}
```

## Parameters

| Parameter | Documentation                                                                                     | Ivy                                                                        |
|-----------|---------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------|
| label     | The label shown at the top of the column. If None, the column name is used.                       | `.Header(x => x.Prop, "Label")`                                           |
| help      | Tooltip displayed when hovering over the column label. Supports GitHub-flavored Markdown.         | `.Help(x => x.Prop, "Tooltip text")`                                      |
| disabled  | Whether editing is disabled for this column.                                                      | Not supported (no inline editing in DataTable)                             |
| required  | Whether cells must have a value. Defaults to False.                                               | Not supported                                                              |
| pinned    | Whether the column is pinned to the left side when scrolling.                                     | `config.FreezeColumns = N` (freezes first N columns, not per-column)       |
| default   | The default value for new rows added by the user.                                                 | Not supported                                                              |
| format    | Display format string (e.g. `"hh:mm a"`). Supports `"localized"`, `"iso8601"`, or momentJS format. | Not supported (DataTable uses automatic type detection and ISO-8601 display) |
| min_value | The minimum allowable time value.                                                                 | Not supported                                                              |
| max_value | The maximum allowable time value.                                                                 | Not supported                                                              |
| step      | The stepping interval for the time picker in seconds.                                             | Not supported                                                              |
