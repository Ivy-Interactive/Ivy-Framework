# st.column_config.DatetimeColumn

Configure a datetime column in `st.dataframe` or `st.data_editor`. This is the default column type for datetime values. When used with `st.data_editor`, edited cells display a datetime picker widget.

## Streamlit

```python
from datetime import datetime
import pandas as pd
import streamlit as st

data_df = pd.DataFrame({
    "appointment": [
        datetime(2024, 2, 5, 12, 30),
        datetime(2023, 11, 10, 18, 0),
    ]
})

st.data_editor(
    data_df,
    column_config={
        "appointment": st.column_config.DatetimeColumn(
            "Appointment",
            min_value=datetime(2023, 6, 1),
            max_value=datetime(2025, 1, 1),
            format="D MMM YYYY, h:mm a",
            step=60,
        ),
    },
    hide_index=True,
)
```

## Ivy

Ivy does not have column-level datetime configuration on its `DataTable` widget. DateTime columns are supported natively via ISO-8601 and can be filtered, but display formatting, min/max constraints, and stepping are not available per-column. For standalone datetime editing, the `DateTimeInput` widget is the closest equivalent.

```csharp
// DataTable with a DateTime column (no per-column format/min/max/step)
var table = new DataTable<Appointment>(dataSource)
    .Columns(c => c
        .Add(x => x.Date).Header("Appointment").Filterable().Sortable()
    );

// Standalone DateTimeInput for editing a single datetime value
var dateState = UseState(DateTime.Today);
dateState.ToDateTimeInput()
    .Format("dd/MM/yyyy HH:mm")
    .Disabled(false)
    .WithField()
    .Label("Appointment");
```

## Parameters

| Parameter  | Documentation                                                                                   | Ivy                                                                                      |
|------------|-------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------|
| label      | Column header text shown at the top of the column. Uses the column name if `None`.              | `DataTableColumn.Header()` sets the column header text.                                  |
| help       | Tooltip shown on hover, supports GitHub-flavored Markdown.                                      | `DataTableColumn.Help()` sets tooltip text on the column header.                         |
| disabled   | When `True`, prevents the user from editing cells in this column.                               | Not supported on DataTable columns. `DateTimeInput.Disabled()` exists for standalone use. |
| required   | When `True`, cells cannot be left empty (`None`) when editing.                                  | Not supported                                                                            |
| pinned     | When `True`, the column stays visible during horizontal scrolling.                              | Not supported directly; column ordering/grouping is available.                            |
| default    | Default `datetime` value pre-filled for newly added rows.                                       | Not supported                                                                            |
| format     | Display format string (momentJS syntax) or presets: `"localized"`, `"distance"`, `"iso8601"`.   | Not supported on DataTable columns. `DateTimeInput.Format()` exists for standalone use.   |
| min_value  | Minimum selectable datetime in the picker.                                                      | Not supported                                                                            |
| max_value  | Maximum selectable datetime in the picker.                                                      | Not supported                                                                            |
| step       | Stepping interval for the datetime picker, in seconds (accepts `int`, `float`, or `timedelta`). | Not supported                                                                            |
| timezone   | Timezone identifier (e.g. `"US/Central"`). Inferred from data if `None`.                        | Not supported (Ivy uses `DateTimeOffset` for timezone-aware values).                     |
