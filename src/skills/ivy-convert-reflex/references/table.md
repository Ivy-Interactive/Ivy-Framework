# Table

A semantic table component for presenting structured tabular data, supporting headers, rows, cells, sorting, filtering, and various layout options.

## Reflex

```python
import reflex as rx

def table_example():
    return rx.table.root(
        rx.table.header(
            rx.table.row(
                rx.table.column_header_cell("Name"),
                rx.table.column_header_cell("Age"),
                rx.table.column_header_cell("Email"),
            ),
        ),
        rx.table.body(
            rx.table.row(
                rx.table.cell("Alice"),
                rx.table.cell("30"),
                rx.table.cell("alice@example.com"),
            ),
            rx.table.row(
                rx.table.cell("Bob"),
                rx.table.cell("25"),
                rx.table.cell("bob@example.com"),
            ),
        ),
        size="2",
        variant="surface",
    )
```

## Ivy

Ivy has two table widgets: `Table` (basic) and `DataTable` (advanced with sorting, filtering, pagination).

### Basic Table (manual construction)

```csharp
public class TableExample : ViewBase
{
    public override object? Build()
    {
        return new Table(
            new TableRow(
                new TableCell("Name").IsHeader().AlignContent(Align.Left),
                new TableCell("Age").IsHeader().AlignContent(Align.Center),
                new TableCell("Email").IsHeader().AlignContent(Align.Left)
            ),
            new TableRow(
                new TableCell("Alice"),
                new TableCell("30").AlignContent(Align.Center),
                new TableCell("alice@example.com")
            ),
            new TableRow(
                new TableCell("Bob"),
                new TableCell("25").AlignContent(Align.Center),
                new TableCell("bob@example.com")
            )
        ).Width(Size.Full());
    }
}
```

### Collection-based Table (recommended)

```csharp
public class CollectionTableExample : ViewBase
{
    public override object? Build()
    {
        var people = new[]
        {
            new { Name = "Alice", Age = 30, Email = "alice@example.com" },
            new { Name = "Bob", Age = 25, Email = "bob@example.com" },
        };

        return people.ToTable()
            .Width(Size.Full())
            .Header(p => p.Age, "Age")
            .Align(p => p.Age, Align.Center);
    }
}
```

### DataTable (advanced)

```csharp
public class DataTableExample : ViewBase
{
    public override object? Build()
    {
        var people = new[]
        {
            new { Name = "Alice", Age = 30, Email = "alice@example.com" },
            new { Name = "Bob", Age = 25, Email = "bob@example.com" },
        };

        return people.ToDataTable()
            .Header(p => p.Name, "Name")
            .Header(p => p.Email, "Email Address")
            .Align(p => p.Age, Align.Center)
            .Config(config =>
            {
                config.AllowSorting = true;
                config.AllowFiltering = true;
                config.ShowSearch = true;
            })
            .Height(Size.Units(100));
    }
}
```

## Parameters

### Table Root / Container

| Parameter         | Reflex                                           | Ivy                                                        |
|-------------------|--------------------------------------------------|------------------------------------------------------------|
| Size              | `size="1"\|"2"\|"3"`                             | Not supported (use `Width()` / `Height()`)                 |
| Variant           | `variant="surface"\|"ghost"`                     | Not supported                                              |
| Width             | CSS via `width` style prop                       | `.Width(Size.Full())` / `.Width(Size.Units(n))`            |
| Height            | CSS via `height` style prop                      | `.Height(Size.Units(n))`                                   |
| Align             | `align="left"\|"center"\|"right"`                | `.Align(col, Align.Left\|Center\|Right)`                   |
| Summary           | `summary="..."` (accessibility)                  | Not supported                                              |
| Column widths     | CSS on individual cells                          | `.ColumnWidth(col, Size.Units(n)\|Size.Fraction(f))`       |
| Column headers    | `rx.table.column_header_cell("Name")`            | `.Header(col, "Name")`                                     |
| Column order      | Defined by source order in markup                | `.Order(col1, col2, ...)`                                  |
| Column removal    | Omit from markup                                 | `.Remove(col)` or `.Clear()` + `.Add(col)`                 |
| Column span       | `col_span=n` on cell                             | Not supported                                              |
| Row span          | `row_span=n` on cell                             | Not supported                                              |
| Empty state       | Conditional rendering in Python                  | `.Empty(new Card("No data"))`                              |
| Totals/Aggregation| Manual computation in Python                     | `.Totals(col)` / `.Totals(col, items => items.Sum(...))`   |
| Pivot tables      | Not supported                                    | `.ToPivotTable().Dimension(...).Measure(...).ExecuteAsync()`|
| Multi-line cells  | CSS / manual formatting                          | `.Multiline(col)`                                          |
| Cell builders     | Custom `rx.component` in cell                    | `.Builder(col, f => f.Link()\|f.Text()\|f.CopyToClipboard())` |
| Auto-hide empty   | Not supported                                    | `.RemoveEmptyColumns()`                                    |
| Auto-conversion   | Not supported                                    | Any `IEnumerable` returned from a view becomes a table     |

### DataTable-specific (Ivy advanced widget)

| Parameter              | Reflex                                       | Ivy DataTable                                         |
|------------------------|----------------------------------------------|-------------------------------------------------------|
| Sorting                | Manual implementation via state              | `.Sortable(col, true)` / `config.AllowSorting = true` |
| Filtering              | Manual via `.where()` / list comprehensions  | `config.AllowFiltering = true`                        |
| AI/LLM filtering       | Not supported                                | `config.AllowLlmFiltering = true`                     |
| Search                 | Manual via state + input binding             | `config.ShowSearch = true` (Ctrl/Cmd+F)               |
| Pagination             | Manual via `.offset()` / `.limit()`          | Built-in via `config.BatchSize = n`                   |
| Freeze columns         | Not supported                                | `config.FreezeColumns = n`                            |
| Row index column       | Not supported                                | `config.ShowIndexColumn = true`                       |
| Selection mode         | Not supported                                | `config.SelectionMode = Rows\|Cells\|Columns`        |
| Copy selection         | Not supported                                | `config.AllowCopySelection = true`                    |
| Column reordering      | Not supported                                | `config.AllowColumnReordering = true`                 |
| Column resizing        | Not supported                                | `config.AllowColumnResizing = true`                   |
| Row actions            | Custom buttons in cells                      | `.RowActions(new MenuItem { Label, Tag, Icon })`      |
| Cell click events      | `on_click` on individual cells               | `.OnCellClick(handler)` / `.OnCellActivated(handler)` |
| Vertical borders       | CSS styling                                  | `config.ShowVerticalBorders = true`                   |
| Column grouping        | Not supported                                | `.Group(col, "Group Name")`                           |
| Column icon            | Custom component in header                   | `.Icon(col, Icons.User)`                              |
| Column help tooltip    | Not supported                                | `.Help(col, "tooltip text")`                          |
| Data export (CSV/JSON) | `rx.download` with manual serialization      | Not supported                                         |
