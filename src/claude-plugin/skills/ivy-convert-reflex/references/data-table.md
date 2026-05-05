# Data Table

Displays static data in a tabular format with built-in search, sorting, pagination, and column resizing. Ideal for read-only datasets like CSVs or dataframes.

## Reflex

```python
import pandas as pd

nba_data = pd.read_csv("data/nba.csv")

rx.data_table(
    data=nba_data[["Name", "Height", "Age"]],
    pagination=True,
    search=True,
    sort=True,
)
```

## Ivy

```csharp
sampleUsers.ToDataTable()
    .Header(u => u.Name, "Full Name")
    .Header(u => u.Email, "Email Address")
    .Header(u => u.Salary, "Salary")
    .Header(u => u.Status, "Status")
    .Width(u => u.Name, Size.Units(50))
    .Align(u => u.Salary, Align.Right)
    .Config(config =>
    {
        config.ShowSearch = true;
        config.AllowSorting = true;
        config.AllowFiltering = true;
        config.AllowColumnResizing = true;
        config.BatchSize = 50;
    })
    .Height(Size.Units(100))
```

## Parameters

| Parameter    | Documentation                                                                 | Ivy                                                                                          |
|--------------|-------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------|
| `data`       | `Any` - Pandas dataframe or list of lists as the data source                  | `.ToDataTable()` extension method on `IQueryable`/`IEnumerable`                              |
| `columns`    | `Sequence` - Column definitions for the table                                 | `.Header(expr, text)` fluent API per column                                                  |
| `search`     | `bool` - Enables search functionality                                         | `config.ShowSearch = true`                                                                   |
| `sort`       | `bool` - Enables column sorting                                               | `config.AllowSorting = true`                                                                 |
| `resizable`  | `bool` - Allows column resizing                                               | `config.AllowColumnResizing = true`                                                          |
| `pagination` | `Union[bool, dict]` - Enables pagination with optional configuration          | `config.BatchSize = N` for incremental loading; `.LoadAllRows(true)` to disable               |
| —            | Not supported                                                                 | `config.AllowFiltering = true` - Column-level filtering                                       |
| —            | Not supported                                                                 | `config.AllowLlmFiltering = true` - AI-powered natural language filtering                     |
| —            | Not supported                                                                 | `config.FreezeColumns = N` - Freeze columns during horizontal scroll                          |
| —            | Not supported                                                                 | `config.SelectionMode` - Cell, row, or column selection (`None`, `Cells`, `Rows`, `Columns`)  |
| —            | Not supported                                                                 | `.Group(expr, groupName)` - Organize columns into logical groups                              |
| —            | Not supported                                                                 | `.Icon(expr, iconString)` - Add icons to column headers                                       |
| —            | Not supported                                                                 | `.Help(expr, text)` - Tooltip help text per column                                            |
| —            | Not supported                                                                 | `.RowActions()` - Context menu actions per row                                                |
| —            | Not supported                                                                 | `.OnCellClick` / `.OnCellActivated` - Cell click and double-click event handlers              |
