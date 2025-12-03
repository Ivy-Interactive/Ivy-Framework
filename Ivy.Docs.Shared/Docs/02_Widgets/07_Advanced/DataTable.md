---
prepare: |
  var firstNames = new[] { "John", "Sarah", "Mike", "Emily", "Alex", "Lisa", "David", "Jessica", "Robert", "Amanda" };
  var lastNames = new[] { "Smith", "Johnson", "Brown", "Davis", "Wilson", "Chen", "Miller", "Taylor", "Garcia", "White" };
  var statusIcons = new[] { Icons.Rocket.ToString(), Icons.Star.ToString(), Icons.ThumbsUp.ToString(), Icons.Heart.ToString(), Icons.Check.ToString(), Icons.Clock.ToString() };
  var sampleUsers = Enumerable.Range(0, 100).Select(id =>
  {
      var random = new Random(id * 17 + 42);
      var firstName = firstNames[random.Next(firstNames.Length)];
      var lastName = lastNames[random.Next(lastNames.Length)];
      var name = $"{firstName} {lastName}";
      var email = $"{firstName.ToLower()}.{lastName.ToLower()}{id}@example.com";
      var salary = random.Next(40000, 150000);
      var status = statusIcons[random.Next(statusIcons.Length)];
      var isActive = random.Next(100) > 25;
      return new { Name = name, Email = email, Salary = salary, Status = status, IsActive = isActive };
  }).AsQueryable();
searchHints:
  - table
  - grid
  - data
  - rows
  - columns
  - sort
  - filter
  - search
  - dataset
---

# DataTable

<Ingress>
Display and interact with large datasets using high-performance data tables with sorting, filtering, pagination, and real-time updates powered by Apache Arrow.
</Ingress>

The `DataTable` widget provides a powerful, high-performance solution for displaying tabular data. Built on Apache Arrow for optimal performance with large datasets, it supports automatic type detection, sorting, filtering, column grouping, and customization.

## Basic Usage

Create a DataTable from any `IQueryable<T>` using the `.ToDataTable()` extension method:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Header(u => u.Name, "Full Name")
    .Header(u => u.Email, "Email Address")
    .Header(u => u.Salary, "Salary")
    .Header(u => u.Status, "Status")
    .Height(Size.Units(100))
```

## Table Sizing

Control the overall dimensions of the DataTable using `Width()` and `Height()` methods:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Header(u => u.Name, "Full Name")
    .Header(u => u.Email, "Email Address")
    .Header(u => u.Salary, "Salary")
    .Width(Size.Px(800))
    .Height(Size.Units(100))
```

**Table sizing methods:**

- **Width** - Set the overall width of the table using `Size.Px()`, `Size.Units()`, `Size.Fraction()`, etc. For column-specific widths, use `Width(expression, size)`.
- **Height** - Set the overall height of the table

## Column Configuration

Customize column appearance and behavior with a fluent API:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Header(u => u.Name, "Full Name")
    .Header(u => u.Email, "Email Address")
    .Header(u => u.Salary, "Annual Salary")
    .Header(u => u.Status, "Status")
    .Width(u => u.Name, Size.Units(50))
    .Width(u => u.Email, Size.Units(60))
    .Width(u => u.Salary, Size.Units(80))
    .Align(u => u.Salary, Align.Right)
    .Icon(u => u.Name, Icons.User.ToString())
    .Icon(u => u.Email, Icons.Mail.ToString())
    .Icon(u => u.Salary, Icons.DollarSign.ToString())
    .Icon(u => u.Status, Icons.Activity.ToString())
    .Sortable(u => u.Email, false)
    .SortDirection(u => u.Salary, SortDirection.Descending)
    .Help(u => u.Name, "Employee full name")
    .Help(u => u.Salary, "Annual salary in USD")
    .Height(Size.Units(100))
```

**Column customization methods:**

- **Header** - Set custom column header text
- **Width** - Set column width using `Size.Px()`, `Size.Percent()`, etc.
- **Align** - Control text alignment (Left, Right, Center)
- **Icon** - Add an icon to the column header
- **Help** - Add tooltip help text to the column header
- **Sortable** - Enable or disable sorting for specific columns
- **SortDirection** - Set default sort direction (Ascending, Descending, None)
- **Filterable** - Enable or disable filtering for specific columns
- **Hidden** - Hide columns from display
- **Order** - Control the display order of columns
- **Group** - Organize columns into logical groups (requires `ShowGroups` config)

## Advanced Configuration

Use the `.Config()` method to control table behavior and user interactions:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Header(u => u.Name, "Name")
    .Header(u => u.Email, "Email")
    .Header(u => u.Salary, "Salary")
    .Header(u => u.Status, "Status")
    .Group(u => u.Name, "Personal")
    .Group(u => u.Email, "Personal")
    .Group(u => u.Salary, "Employment")
    .Group(u => u.Status, "Employment")
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
    })
    .Height(Size.Units(100))
```

**Configuration options:**

- **ShowGroups** - Display column group headers
- **ShowIndexColumn** - Show row index numbers in the first column
- **FreezeColumns** - Number of columns to freeze (remain visible when scrolling horizontally)
- **SelectionMode** - How users can select data (None, Cells, Rows, Columns)
- **AllowCopySelection** - Enable copying selected cells to clipboard
- **AllowColumnReordering** - Allow users to drag and reorder columns
- **AllowColumnResizing** - Allow users to resize column widths
- **AllowLlmFiltering** - Enable AI-powered natural language filtering
- **AllowSorting** - Enable/disable sorting globally
- **AllowFiltering** - Enable/disable filtering globally
- **ShowSearch** - Enable search functionality (accessible via Ctrl/Cmd + F keyboard shortcut)
- **EnableCellClickEvents** - Enable cell click and activation events. When enabled, you can handle `OnCellClick` (single-click) and `OnCellActivated` (double-click) events on the DataTable widget. Events provide `CellClickEventArgs` with `RowIndex`, `ColumnIndex`, `ColumnName`, and `CellValue`.
- **ShowVerticalBorders** - Show vertical borders between columns. Set to `false` to hide column borders for a cleaner appearance

## Row Actions

Add contextual actions to each row using `RowActions()` and handle them via `OnRowAction()`. Actions are rendered as icons or buttons within a dedicated column. Row actions support nested menus.

```csharp demo-tabs
public class RowActionsDemo : ViewBase
{
    public record Employee(int Id, string Name, string Title, string ProfileLink);

    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var employees = new[]
        {
            new Employee(1, "Alice", "Designer", "https://github.com/Ivy-Interactive/Ivy-Framework"),
            new Employee(2, "Bob", "Developer", "https://github.com/Ivy-Interactive"),
            new Employee(3, "Charlie", "Project Manager", "https://github.com/Ivy-Interactive/Ivy-Examples")
        }.AsQueryable();

        return employees
            .ToDataTable()
            .Header(e => e.ProfileLink, "Profile")
            .Renderer(e => e.ProfileLink, new LinkDisplayRenderer { Type = LinkDisplayType.Url })
            .RowActions(
                MenuItem.Default(Icons.Pencil, "edit").Tooltip("Edit employee"),
                MenuItem.Default(Icons.EllipsisVertical, "menu")
                    .Children([
                        MenuItem.Default(Icons.Archive, "archive").Label("Archive"),
                        MenuItem.Default(Icons.Download, "export").Label("Export")
                    ])
            )
            .HandleRowAction(async e =>
            {
                var args = e.Value;
                var actionId = args.ActionId;
                var rowIndex = args.RowIndex;
                var rowData = args.RowData;

                // Access row data by column name
                var employeeName = rowData.TryGetValue("Name", out var name) ? name?.ToString() : "Unknown";
                client.Toast($"Action: {actionId} on {employeeName} (row {rowIndex})");
            });
    }
}
```

<Callout Type="tip">
Use <code>Renderer(expr, new LinkDisplayRenderer { Type = LinkDisplayType.Url })</code> to mark a URL string column as a clickable hyperlink. Users can open the link with <kbd>Ctrl</kbd>/<kbd>Cmd</kbd> + click or via the context menu. External links (http/https) open in a new focused tab, while relative URLs navigate in the same tab.
</Callout>

Use `HandleRowAction` to respond to row action menu selections. The handler receives an `Event<DataTable, RowActionClickEventArgs>` containing:

- **ActionId** - The identifier of the action that was clicked (from the MenuItem tag or label)
- **RowIndex** - The zero-based index of the row where the action was triggered
- **RowData** - A dictionary containing the row data, keyed by column name

Access row values from the `RowData` dictionary using column names as keys.

## Cell Click Events

Enable single- and double-click handlers for any cell by turning on `EnableCellClickEvents` in the table configuration and wiring up `.OnCellClick()` / `.OnCellActivated()` delegates.

```csharp demo-tabs
public class CellClickDemo : ViewBase
{
    public record Employee(int Id, string Name, string Department);

    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var employees = new[]
        {
            new Employee(1, "Alice", "Design"),
            new Employee(2, "Bob", "Development"),
            new Employee(3, "Charlie", "QA")
        }.AsQueryable();

        return employees
            .ToDataTable()
            .Config(c => c.EnableCellClickEvents = true)
            .OnCellClick(e =>
            {
                var args = e.Value;
                client.Toast($"Clicked: {args.ColumnName} (row {args.RowIndex})");
                return ValueTask.CompletedTask;
            })
            .OnCellActivated(e =>
            {
                var args = e.Value;
                client.Toast($"Double-clicked: {args.ColumnName} (row {args.RowIndex})");
                return ValueTask.CompletedTask;
            });
    }
}
```

`CellClickEventArgs` exposes `RowIndex`, `ColumnIndex`, `ColumnName`, and `CellValue`, allowing you to perform any context-specific action.

## DateTime Filtering

`DataTable` fully supports filtering `DateTime`, `Date`, and `DateTimeOffset` columns using ISO-8601 date strings. Users can type expressions such as

```text
[HireDate] = "2024-05-30"
[OrderDate] >= "2025-11-01" AND [OrderDate] <= "2025-11-31"
```

into the filter box (Ctrl/Cmd&nbsp;+&nbsp;F) or the column filter UI, and the underlying dataset will be queried server-side. The parsing engine recognises dates without needing quotes if there are no spaces, but quoting is recommended.

```csharp demo-tabs
public class DateFilterDemo : ViewBase
{
    public record Order(int Id, DateTime OrderDate, DateTime? ShippedDate, int Total);

    public override object? Build()
    {
        var orders = Enumerable.Range(1, 50)
            .Select(i => new Order(
                i,
                OrderDate: DateTime.Today.AddDays(-i),
                ShippedDate: i % 3 == 0 ? DateTime.Today.AddDays(-i + 2) : null,
                Total: 55 + i * 5))
            .AsQueryable();

        return orders
            .ToDataTable()
            .Header(o => o.OrderDate, "Order Date")
            .Header(o => o.ShippedDate, "Shipped")
            .Header(o => o.Total, "Total ($)")
            .Config(c =>
            {
                c.AllowFiltering = true;     // enable filter row + Ctrl/Cmd+F search
                c.ShowSearch = true;
            })
            .Height(Size.Units(100));
    }
}
```

Try filtering the _Order Date_ column with a range such as [OrderDate] >= "2025-11-01" AND [OrderDate] <= "2025-11-31" to see the results update in real time.

## Performance with Large Datasets

DataTable is optimized for handling extremely large datasets efficiently. For optimal performance with large datasets (100,000+ rows), configure how data is loaded:

```csharp demo-tabs
Enumerable.Range(1, 500)
    .Select(i => new { Id = i, Value = $"Row {i}" })
    .AsQueryable()
    .ToDataTable()
    .Header(x => x.Id, "ID")
    .Header(x => x.Value, "Value")
    .LoadAllRows(true)  // Load all rows at once
    .Height(Size.Units(100))
```

**Performance options:**

- **LoadAllRows(true)** - Load all rows at once for maximum performance with very large datasets. Set to `false` to enable incremental loading with batching.
- **BatchSize(n)** - Load data in batches of n rows for incremental loading. Use this when `LoadAllRows` is `false` to control how many rows are loaded per batch. Default is typically 50 rows per batch.

**Example with performance configuration:**

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Header(u => u.Name, "Name")
    .Header(u => u.Email, "Email")
    .Header(u => u.Salary, "Salary")
    .Config(config =>
    {
        config.BatchSize = 50;        // Load 50 rows per batch
        config.LoadAllRows = false;   // Enable incremental loading
    })
    .Height(Size.Units(100))
```

</Body>
</Details>

<WidgetDocs Type="Ivy.DataTable" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/DataTables/DataTable.cs"/>
