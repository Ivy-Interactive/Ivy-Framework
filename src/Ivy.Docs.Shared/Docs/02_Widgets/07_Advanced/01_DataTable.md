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
Display and interact with large datasets using high-performance data tables with sorting, filtering, [pagination](../03_Common/09_Pagination.md), and real-time updates powered by Apache Arrow.
</Ingress>

The `DataTable` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) provides a powerful, high-performance solution for displaying tabular data. Use it inside [views](../../01_Onboarding/02_Concepts/02_Views.md) and [layouts](../../01_Onboarding/02_Concepts/04_Layout.md), with [state](../../03_Hooks/02_Core/03_UseState.md) for dynamic data or row actions. Built on Apache Arrow for optimal performance with large datasets, it supports automatic type detection, sorting, filtering, column grouping, and customization.

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
- **Width** - Set column width using [Size](../../04_ApiReference/IvyShared/Size.md) (e.g. `Size.Px()`, `Size.Percent()`).
- **Align** - Control text alignment ([Align](../../04_ApiReference/IvyShared/Align.md): Left, Right, Center)
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

Add contextual actions to each row using `RowActions()` and handle them via `HandleRowAction()`. Actions are rendered as icons or [buttons](../03_Common/01_Button.md) that appear when hovering over a row. Row actions support both simple menu items and nested [dropdown menus](../03_Common/11_DropDownMenu.md).

Use `RowActions()` to define one or more `MenuItem` objects. Each menu item can have an icon, label, tooltip, and tag. For nested menus, use `.Children()` to create a dropdown menu with sub-items. For example, you can create individual action buttons like edit, delete, or view, as well as a menu button with a dropdown containing additional actions like archive, export, or share.

When creating the DataTable, specify an ID selector using `.ToDataTable(idSelector: e => e.Id)` where `Id` is the property that uniquely identifies each row. This allows the row action handler to identify which row was clicked.

Use `HandleRowAction()` to respond to row action menu selections. The handler receives an `Event<DataTable, RowActionClickEventArgs>` containing:

- **Id** - The ID of the row (extracted using the `idSelector` parameter passed to `ToDataTable()`)
- **Tag** - The tag of the menu item that was clicked (useful for identifying which action was selected, especially with nested menus)

The handler can access both properties: `args.Id` to identify the row and `args.Tag` to determine which action was selected. This is particularly useful when handling nested menu items, as each child menu item can have its own tag.

```csharp demo-tabs
public class RowActionsDemo : ViewBase
{
    public record Employee(int Id, string Name, string Email, int Salary);

    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var employees = Enumerable.Range(1, 50)
            .Select(i => new Employee(i, $"Employee {i}", $"emp{i}@company.com", 40000 + i * 1000))
            .AsQueryable();

        return employees.ToDataTable(idSelector: e => e.Id)
            .Header(e => e.Name, "Name")
            .Header(e => e.Email, "Email")
            .Header(e => e.Salary, "Salary")
            .RowActions(
                MenuItem.Default(Icons.Pencil, "edit"),
                MenuItem.Default(Icons.Trash2, "delete"),
                MenuItem.Default(Icons.EllipsisVertical, "more")
                    .Children([
                        MenuItem.Default(Icons.Archive, "archive").Label("Archive"),
                        MenuItem.Default(Icons.Download, "export").Label("Export"),
                        MenuItem.Default(Icons.Share2, "share").Label("Share")
                    ])
            )
            .OnRowAction(async e =>
            {
                var args = e.Value;
                client.Toast($"Action: {args.Tag} on row ID: {args.Id}");
                await ValueTask.CompletedTask;
            })
            .Height(Size.Units(100));
    }
}
```

<Callout Type="tip">
Use <code>Renderer(expr, new LinkDisplayRenderer { Type = LinkDisplayType.Url })</code> to mark a URL string column as a clickable hyperlink. Click on a link to open it. External links (http/https) open in a new focused tab, while relative URLs navigate in the same tab.
</Callout>

## Cell Click Events

Enable single- and double-click handlers for any cell by setting `EnableCellClickEvents = true` in the table configuration and wiring up `.OnCellClick()` and `.OnCellActivated()` delegates. See [event handling](../../01_Onboarding/02_Concepts/05_EventHandlers.md) for patterns.

The `OnCellClick()` handler is triggered on a single click, while `OnCellActivated()` is triggered on a double-click. Both handlers receive an `Event<DataTable, CellClickEventArgs>` containing:

- **RowIndex** - The zero-based index of the row that was clicked
- **ColumnIndex** - The zero-based index of the column that was clicked
- **ColumnName** - The name of the column that was clicked
- **CellValue** - The value of the cell that was clicked

These properties allow you to perform context-specific actions based on which cell was interacted with. For example, you can display a toast notification with the column name and row index, or navigate to a detail view based on the cell's value.

## Triggering Refreshes

You can programmatically force a DataTable to refresh its data by utilizing the `UseRefreshToken` hook and the `.RefreshToken()` fluent API. This is especially useful for reloading the table after users perform actions like creating, updating, or deleting records in a separate dialog or blade.

First, obtain a token using `UseRefreshToken()`, pass it to the `DataTableBuilder`, and then call `refreshToken.Refresh()` whenever you need the table to reload:

```csharp
public class RefreshTokenDemo : ViewBase
{
    public override object? Build()
    {
        var refreshToken = UseRefreshToken();
        var employees = GetEmployees().AsQueryable();

        // Pass the token to the builder
        var table = employees
            .ToDataTable(e => e.Name)
            .RefreshToken(refreshToken)
            .Header(e => e.Name, "Name")
            .Width(e => e.Name, Size.Units(50))
            .Height(Size.Units(100));

        var refreshButton = new Button("Reload Table").OnClick(e =>
        {
            // Trigger a refresh of the DataTable
            refreshToken.Refresh();
        });

        return new Fragment(refreshButton, table);
    }
}
```

## AI-Powered Filtering

DataTable supports natural language filtering powered by a Large Language Model (LLM). Instead of writing formal filter expressions, users can type conversational queries and the AI will convert them to the appropriate filter syntax.

### Enabling AI Filtering

Enable AI-powered filtering with a single configuration option:

```csharp
public record Employee(int Id, string Name, decimal Salary, bool IsActive);

[App]
public class EmployeeTableApp : ViewBase
{
    public override object? Build()
    {
        var employees = GetEmployees().AsQueryable();
        return employees.ToDataTable(e => e.Id)
            .Header(e => e.Name, "Employee Name")
            .Header(e => e.Salary, "Salary")
            .Header(e => e.IsActive, "Active")
            .Width(e => e.Salary, Size.Px(120))
            .Config(config =>
            {
                config.AllowSorting = true;
                config.AllowFiltering = true;
                config.AllowLlmFiltering = true;
                config.BatchSize = 50;
            });
    }
}
```

### Natural Language Queries

Once enabled with `.Config(config => config.AllowLlmFiltering = true)`, users can filter using conversational phrases:

- "employees older than 30"
- "salary above 100000"
- "active managers"
- "hired in 2023"

### Smart Interpretation

The AI agent provides intelligent query handling:

- **Typo tolerance** - Automatically corrects misspellings
- **Concept mapping** - Converts phrases like "retirement age" into structured conditions (e.g., `[Age] >= 65`)
- **Type mismatch resolution** - If a field type doesn't match user intent, the agent identifies appropriate alternative fields

### Supported Filter Grammar

The AI filter agent converts natural language queries to structured filter expressions like `[Age] > 30` or `[Salary] > 100000 AND [IsManager] = true`.

The AI generates filters using these operations:

- **Comparisons:** `=`, `!=`, `>`, `>=`, `<`, `<=`
- **Text operations:** `contains`, `starts with`, `ends with`
- **Existence checks:** `IS BLANK`, `IS NOT BLANK`
- **Logical operators:** `AND`, `OR`, `NOT`
- **Parenthetical grouping** for complex expressions

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

<WidgetDocs Type="Ivy.DataTable" ExtensionTypes="Ivy.DataTableWidgetExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/DataTables/DataTable.cs"/>
