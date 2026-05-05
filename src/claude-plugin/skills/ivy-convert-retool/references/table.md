# Table

A table to display data that can be sorted, filtered, paginated, and edited. Retool's Table component renders an array of data as a tabulated list with support for inline editing, row selection, search, grouping, and export.

## Retool

```toolscript
// Bind data
table1.data = {{ getUsers.data }}

// Filtering
table1.setFilterStack({
  filters: [
    { columnId: "status", operator: "=", value: "active" }
  ]
});

// Sorting
table1.setSort({ columnId: "name", sortDirection: "ascending" });

// Row selection
table1.selectRow("index", "display", 0);

// Pagination
table1.setPage(2);

// Export
table1.exportData({ fileName: "users", fileType: "csv" });

// Access selected data
const row = table1.selectedRow;
const rows = table1.selectedRows;

// Changeset tracking (inline editing)
const edits = table1.changesetArray;
table1.clearChangeset();
```

## Ivy

Ivy has two table widgets: **Table** (simple, read-only display with totals and pivot support) and **DataTable** (advanced, high-performance with sorting, filtering, selection, and row actions).

### Table (simple)

```csharp
products.ToTable()
    .Width(Size.Full())
    .Header(p => p.Price, "Unit Price")
    .Align(p => p.Price, Align.Right)
    .ColumnWidth(p => p.Name, Size.Fraction(0.3f))
    .Order(p => p.Name, p => p.Price, p => p.Sku)
    .Remove(p => p.Url)
    .Totals(p => p.Price)
    .Totals(p => p.Stock, items => items.Sum(p => p.Stock))
    .Builder(p => p.Url, f => f.Link())
    .Multiline(p => p.Description)
    .RemoveEmptyColumns()
    .Empty(new Card("No products found").Width(Size.Full()));
```

### DataTable (advanced — equivalent to Retool Table)

```csharp
sampleUsers.ToDataTable(u => u.Id)
    .Header(u => u.Name, "Full Name")
    .Header(u => u.Email, "Email Address")
    .Header(u => u.Salary, "Annual Salary")
    .Width(u => u.Name, Size.Units(50))
    .Align(u => u.Salary, Align.Right)
    .Icon(u => u.Name, Icons.User.ToString())
    .SortDirection(u => u.Salary, SortDirection.Descending)
    .Sortable(u => u.Email, false)
    .Group(u => u.Name, "Personal")
    .Group(u => u.Salary, "Employment")
    .Config(config =>
    {
        config.ShowGroups = true;
        config.ShowIndexColumn = true;
        config.FreezeColumns = 1;
        config.SelectionMode = SelectionModes.Rows;
        config.AllowCopySelection = true;
        config.AllowColumnReordering = true;
        config.AllowColumnResizing = true;
        config.AllowLlmFiltering = true;
        config.AllowSorting = true;
        config.AllowFiltering = true;
        config.ShowSearch = true;
        config.EnableCellClickEvents = true;
        config.ShowVerticalBorders = false;
        config.BatchSize = 50;
    })
    .OnRowAction((e) => { /* handle row action */ })
    .OnCellClick((e) => { /* handle cell click */ })
    .OnCellActivated((e) => { /* handle double-click */ })
    .Height(Size.Units(100));
```

## Parameters

### Data & Content

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| Data source | `data` — any array/object | `IEnumerable<T>.ToTable()` or `IQueryable<T>.ToDataTable()` |
| Primary key | Configured in inspector | `ToDataTable(u => u.Id)` — lambda selector |
| Column ordering | `columnOrdering` — array of column IDs | `.Order(p => p.Col1, p => p.Col2)` |
| Column headers | Automatic from data keys | `.Header(p => p.Col, "Label")` |
| Column width | `autoColumnWidth` — boolean | `.ColumnWidth()` (Table) / `.Width()` (DataTable) |
| Column alignment | Per-column in inspector | `.Align(p => p.Col, Align.Right)` |
| Column icons | Not built-in | `.Icon(p => p.Col, Icons.User.ToString())` |
| Column groups | `groupByColumns` — array of column IDs | `.Group(p => p.Col, "GroupName")` with `config.ShowGroups` |
| Column help text | Not built-in | `.Help(p => p.Col, "tooltip text")` |
| Hide columns | Per-column visibility toggle | `.Remove(p => p.Col)` (Table) / `.Hidden()` (DataTable) |
| Empty state | `emptyMessage` — string | `.Empty(widget)` — accepts any widget |

### Sorting

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| Allow sorting (global) | Enabled by default | `config.AllowSorting = true` |
| Sort configuration | `sortArray` — `[{columnId, sortDirection}]` | `.SortDirection(p => p.Col, SortDirection.Descending)` |
| Disable sorting per column | Per-column setting in inspector | `.Sortable(p => p.Col, false)` |
| Set sort programmatically | `table.setSort()` | Not supported |
| Clear sort | `table.clearSort()` | Not supported |

### Filtering

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| Allow filtering | Built-in toolbar | `config.AllowFiltering = true` |
| Filter stack | `filterStack` — `{filters, operator}` | User-driven UI only |
| Set filter programmatically | `table.setFilter()` / `table.setFilterStack()` | Not supported (use LINQ on data source) |
| Clear filters | `table.clearFilter()` / `table.clearFilterStack()` | Not supported |
| Reset to default filters | `table.resetFilterStack()` | Not supported |
| Case-sensitive filtering | `caseSensitiveFiltering` — boolean | Not supported |
| Search | `searchTerm` / `searchMode` (fuzzy, caseInsensitive, caseSensitive) | `config.ShowSearch = true` |
| AI-powered filtering | Not supported | `config.AllowLlmFiltering = true` |

### Pagination & Performance

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| Overflow behavior | `overflowType` — `scroll` or `pagination` | Virtual scrolling with `config.BatchSize` |
| Server-side pagination | `serverPaginated` / `serverPaginationType` (limitOffset, cursor) | Not supported (client-side via `IQueryable`) |
| Page size | `pagination.pageSize` | `config.BatchSize = 50` |
| Current page | `pagination.currentPage` / `table.setPage()` | Not supported |
| Cursor pagination | `pagination.afterCursor` / `pagination.beforeCursor` | Not supported |
| Load all rows | N/A (all data sent to client) | `.LoadAllRows(true)` |

### Selection

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| Selected row | `selectedRow` — object | Not directly exposed (use events) |
| Selected rows (multi) | `selectedRows` — array of objects | `config.SelectionMode = SelectionModes.Rows` |
| Selected row key | `selectedRowKey` / `selectedRowKeys` | Not supported |
| Selected data index | `selectedDataIndex` / `selectedDataIndexes` | Not supported |
| Selected source row | `selectedSourceRow` / `selectedSourceRows` | Not supported |
| Selected cell | `selectedCell` | `OnCellClick` / `OnCellActivated` events |
| Clear selection | `table.clearSelection()` | Not supported |
| Select row programmatically | `table.selectRow(mode, indexType, index, key)` | Not supported |
| Select next/previous row | `table.selectNextRow()` / `table.selectPreviousRow()` | Not supported |
| Copy selection | Not built-in | `config.AllowCopySelection = true` |

### Inline Editing

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| Inline editing | Built-in with changeset tracking | Not supported (read-only) |
| Changeset (array) | `changesetArray` — edited rows by index | Not supported |
| Changeset (object) | `changesetObject` — edited rows by key | Not supported |
| New rows | `newRows` — added rows | Not supported |
| Disable edits | `disableEdits` — boolean | N/A |
| Disable save | `disableSave` — boolean | N/A |
| Clear changeset | `table.clearChangeset()` | N/A |

### Appearance & Layout

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| Height | `heightType` — `fixed` or `auto` | `.Height(Size.Units(100))` |
| Width | Set via layout | `.Width(Size.Full())` |
| Hidden | `hidden` — boolean | `Visible` property |
| Auto column width | `autoColumnWidth` — boolean | Not supported (manual via `.Width()`) |
| Show on desktop | `isHiddenOnDesktop` | Not supported |
| Show on mobile | `isHiddenOnMobile` | Not supported |
| Maintain space when hidden | `maintainSpaceWhenHidden` | Not supported |
| Margin | `margin` — CSS value | Not supported (use layout) |
| Custom styles | `style` — object | Not supported |
| Show index column | Not built-in | `config.ShowIndexColumn = true` |
| Freeze columns | Not built-in (column pinning) | `config.FreezeColumns = n` |
| Vertical borders | Enabled by default | `config.ShowVerticalBorders = false` |
| Column reordering | Built-in drag | `config.AllowColumnReordering = true` |
| Column resizing | Built-in drag | `config.AllowColumnResizing = true` |

### Events

| Event | Retool | Ivy |
|-------|--------|-----|
| Row click / select | `Click Row` / `Select Row` / `Change Row Selection` | Not supported on DataTable |
| Cell click | `Click Cell` / `Select Cell` | `OnCellClick` (single) / `OnCellActivated` (double) |
| Row action | `Click Action` / `Click Toolbar` | `OnRowAction` with `MenuItem` definitions |
| Sort change | `Change Sort` | Not supported |
| Filter change | `Change Filter` | Not supported |
| Page change | `Change Page` | Not supported |
| Row expand | `Expand Row` | Not supported |
| Row deselect | `Deselect Row` | Not supported |
| Double click row | `Double Click Row` | Not supported |
| Focus / Blur | `Focus` / `Blur` | Not supported |
| Cell value change | `Change Cell` | Not supported (read-only) |
| Save / Cancel | `Save` / `Cancel` | Not supported |

### Methods

| Method | Retool | Ivy |
|--------|--------|-----|
| Refresh data | `table.refresh()` | Not supported (reactive via state/IQueryable) |
| Export data | `table.exportData({ fileName, fileType })` | Not supported |
| Get displayed data | `await table.getDisplayedData()` | Not supported |
| Scroll into view | `table.scrollIntoView({ behavior, block })` | Not supported |
| Scroll to row | `table.scrollToRow(row)` | Not supported |
| Collapse/expand rows | `table.collapseRows()` / `table.expandRows()` | Not supported |
| Set hidden | `table.setHidden(bool)` | `Visible` property |
| Set grouping | `table.setGrouping(options)` | `.Group()` fluent API (declarative) |
| Update linked filter | `table.updateLinkedFilter(filterStack)` | Not supported |

### Ivy-Only Features

| Feature | Description |
|---------|-------------|
| Totals / Aggregations | `.Totals(p => p.Price)` with custom LINQ aggregation functions |
| Pivot Tables | `.ToPivotTable()` with `.Dimension()` and `.Measure()` for data reshaping |
| Cell builders | `.Builder(p => p.Url, f => f.Link())` — Link, Text, CopyToClipboard, Default |
| Multi-line cells | `.Multiline(p => p.Description)` |
| Remove empty columns | `.RemoveEmptyColumns()` auto-hides columns with no data |
| Manual table construction | `new Table(new TableRow(new TableCell(...)))` for fully custom layouts |
| AI filtering (LLM) | `config.AllowLlmFiltering = true` — natural language filter queries |
| Column icons | `.Icon(p => p.Col, Icons.User.ToString())` per column |
| Column help tooltips | `.Help(p => p.Col, "description")` per column |
| Strongly-typed data | Generic `IEnumerable<T>` / `IQueryable<T>` with compile-time safety |
| Clear/Add columns | `.Clear()` then `.Add(p => p.Col)` for selective column display |
| Reset configuration | `.Reset()` to restore defaults then reapply |
