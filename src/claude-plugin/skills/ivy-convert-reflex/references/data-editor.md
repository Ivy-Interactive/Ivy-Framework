# Data Editor

A datagrid editor component for displaying and editing tabular data. In Reflex it is based on [Glide Data Grid](https://grid.glideapps.com/); in Ivy the equivalent is the **DataTable** widget built on Apache Arrow.

## Reflex

```python
columns: list[dict[str, str]] = [
    {"title": "Code", "type": "str"},
    {"title": "Value", "type": "int"},
    {"title": "Activated", "type": "bool"},
]
data: list[list[Any]] = [
    ["A", 1, True],
    ["B", 2, False],
    ["C", 3, False],
    ["D", 4, True],
]

rx.data_editor(
    columns=columns,
    data=data,
    row_height=50,
    smooth_scroll_x=True,
    smooth_scroll_y=True,
    column_select="single",
    freeze_columns=1,
    on_cell_clicked=State.click_cell,
    on_cell_edited=State.edit_cell,
)
```

## Ivy

```csharp
sampleData.ToDataTable(idSelector: e => e.Id)
    .Header(e => e.Code, "Code")
    .Header(e => e.Value, "Value")
    .Header(e => e.Activated, "Activated")
    .Width(e => e.Code, Size.Px(100))
    .Sortable(e => e.Value, true)
    .Filterable(e => e.Code, true)
    .Height(Size.Units(100))
    .Config(config =>
    {
        config.FreezeColumns = 1;
        config.SelectionMode = SelectionMode.Cells;
        config.ShowVerticalBorders = true;
        config.AllowColumnResizing = true;
        config.AllowSorting = true;
        config.AllowFiltering = true;
        config.ShowSearch = true;
        config.EnableCellClickEvents = true;
    })
    .HandleOnCellClick(args =>
    {
        // args.RowIndex, args.ColumnIndex, args.CellValue
    });
```

## Parameters

| Parameter | Reflex | Ivy |
|-----------|--------|-----|
| `rows` | `int` — Number of rows | Not needed (inferred from data source) |
| `columns` | `Sequence` — Column definitions as list of dicts | Defined via `.Header()`, `.Width()`, `.Align()` etc. fluent methods |
| `data` | `Sequence` — List of lists for row data | `IQueryable<T>` passed to `.ToDataTable()` |
| `freeze_columns` | `int` — Number of frozen columns | `config.FreezeColumns` (`int`) |
| `row_height` | `int` — Height of each row in px | Not directly supported (auto-sized) |
| `header_height` | `int` — Height of column headers in px | Not supported |
| `group_header_height` | `int` — Height of group headers in px | Not supported (groups configured via `.Group()`) |
| `smooth_scroll_x` | `bool` — Smooth horizontal scrolling | Not supported |
| `smooth_scroll_y` | `bool` — Smooth vertical scrolling | Not supported |
| `vertical_border` | `bool` — Show vertical column borders | `config.ShowVerticalBorders` (`bool`) |
| `column_select` | `"none" \| "single" \| "multiple"` | `config.SelectionMode` (`None / Cells / Rows / Columns`) |
| `row_select` | `"none" \| "single" \| "multiple"` | `config.SelectionMode` (`Rows`) |
| `range_select` | `"none" \| "cell" \| "rect" \| "multi-rect"` | `config.SelectionMode` (`Cells`) |
| `row_markers` | `"none" \| "number" \| "checkbox" \| ...` | `config.ShowIndexColumn` (`bool`) |
| `draw_focus_ring` | `bool` — Show focus ring on selected cell | Not supported |
| `fill_handle` | `bool` — Show fill/drag handle | Not supported |
| `fixed_shadow_x` | `bool` — Shadow on frozen columns | Not supported |
| `fixed_shadow_y` | `bool` — Shadow on frozen rows | Not supported |
| `max_column_width` | `int` — Max column width in px | Not supported (use `.Width()` per column) |
| `min_column_width` | `int` — Min column width in px | Not supported |
| `max_column_auto_width` | `int` — Max auto-sized column width | Not supported |
| `prevent_diagonal_scrolling` | `bool` — Prevent diagonal scroll | Not supported |
| `overscroll_x` / `overscroll_y` | `int` — Extra scroll area in px | Not supported |
| `theme` | `DataEditorTheme \| dict` — Full theme customization | Not supported (uses global app theme) |
| `on_cell_clicked` | Fired when a cell is clicked | `OnCellClick` (`CellClickEventArgs`) |
| `on_cell_activated` | Fired when a cell is activated | `OnCellActivated` (`CellClickEventArgs`) |
| `on_cell_edited` | Fired when a cell is edited | Not supported (DataTable is read-only) |
| `on_cell_context_menu` | Fired on right-click | Not supported |
| `on_header_clicked` | Fired when a header is clicked | Not supported (sorting handled automatically) |
| `on_column_resize` | Fired when a column is resized | Not supported (`AllowColumnResizing` is config-only) |
| `on_row_appended` | Fired when a row is appended | Not supported |
| `on_delete` | Fired when a selection is deleted | Not supported |
| `on_grid_selection_change` | Fired when selection changes | Not supported |
| `on_finished_editing` | Fired when editing finishes | Not supported |
| N/A | — | `config.AllowSorting` — Built-in column sorting |
| N/A | — | `config.AllowFiltering` — Built-in column filtering |
| N/A | — | `config.ShowSearch` — Search bar (Ctrl+F) |
| N/A | — | `config.AllowLlmFiltering` — AI-powered natural language filtering |
| N/A | — | `config.AllowCopySelection` — Copy cells to clipboard |
| N/A | — | `config.AllowColumnReordering` — Drag to reorder columns |
| N/A | — | `config.BatchSize` / `config.LoadAllRows` — Incremental data loading |
| N/A | — | `.RowActions()` — Contextual row action menus |
| N/A | — | `.Renderer()` — Custom cell rendering |
