# st.data_editor

Displays an interactive data editor widget that allows users to view and modify tabular data (DataFrames, dicts, lists, etc.) through a spreadsheet-like interface with support for adding/deleting rows, column configuration, and type-aware editing.

## Streamlit

```python
import pandas as pd
import streamlit as st

df = pd.DataFrame([
    {"command": "st.selectbox", "rating": 4, "is_widget": True},
    {"command": "st.balloons", "rating": 5, "is_widget": False},
])

edited_df = st.data_editor(
    df,
    column_config={
        "rating": st.column_config.NumberColumn(
            "Your rating", min_value=1, max_value=5, format="%d ⭐"
        ),
    },
    num_rows="dynamic",
    disabled=["command"],
    hide_index=True,
)
```

## Ivy

The closest equivalent is `DataTable`, a high-performance read-only table powered by Apache Arrow. It supports sorting, filtering, pagination, and cell-click events but does **not** support inline cell editing or row add/delete.

```csharp
sampleUsers.ToDataTable()
    .Header(u => u.Name, "Full Name")
    .Header(u => u.Email, "Email Address")
    .Sortable(u => u.Name, true)
    .Filterable(u => u.Email, true)
    .Config(config =>
    {
        config.ShowIndexColumn = false;
        config.SelectionMode = SelectionMode.Rows;
        config.AllowSorting = true;
        config.AllowFiltering = true;
    })
    .OnCellClick(async args =>
    {
        var value = args.CellValue;
    })
    .RowActions(new MenuItem { Label = "Edit", Tag = "edit" })
    .OnRowAction(async args =>
    {
        var rowId = args.Id;
    })
```

## Parameters

| Parameter       | Documentation                                                                                                      | Ivy                                                                                       |
|-----------------|--------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|
| `data`          | The data to edit. Supports DataFrame, Series, PyArrow Table, NumPy array, list, set, tuple, or dict.              | `IQueryable<T>` passed via `.ToDataTable()` extension method                              |
| `hide_index`    | Whether to hide the index column. `None` auto-determines visibility.                                               | `Config.ShowIndexColumn` (bool)                                                           |
| `column_order`  | Ordered list of column names to display; omitted columns are hidden.                                               | Column order defined by `.Header()` call order, or `.Hidden()` to hide columns            |
| `column_config` | Dict of column configuration objects for customizing display (labels, types, formatting, constraints).             | Fluent methods: `.Header()`, `.Width()`, `.Align()`, `.Icon()`, `.Help()`, `.Group()`     |
| `num_rows`      | Controls row add/delete: `"fixed"`, `"dynamic"`, `"add"`, or `"delete"`.                                          | Not supported                                                                             |
| `disabled`      | Disables editing. `True` disables all, or pass an iterable of column names/indices to disable specific columns.    | Not applicable (DataTable is read-only by design)                                         |
| `on_change`     | Callback invoked when the editor value changes.                                                                    | `OnCellClick` / `OnCellActivated` (click events, not edit events)                         |
| `use_container_width` | **Deprecated.** Overrides width to match parent container.                                                   | `.Width(Size.Full())` or omit for default stretch                                         |
| `row_height`    | Height of each row in pixels.                                                                                      | Not supported                                                                             |
| `placeholder`   | Text displayed for missing/null values. Defaults to `"None"`.                                                      | Not supported                                                                             |
