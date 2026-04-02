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
  - footer
  - aggregate
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
    .AlignContent(u => u.Salary, Align.Right)
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
- **Width** - Set column width using [Size](../../04_ApiReference/Ivy/Size.md) (e.g. `Size.Px()`, `Size.Percent()`).
- **AlignContent** - Control text alignment ([Align](../../04_ApiReference/Ivy/Align.md): Left, Right, Center)
- **Icon** - Add an icon to the column header
- **Help** - Add tooltip help text to the column header
- **Sortable** - Enable or disable sorting for specific columns
- **SortDirection** - Set default sort direction (Ascending, Descending, None)
- **Filterable** - Enable or disable filtering for specific columns
- **Hidden** - Hide columns from display
- **Order** - Control the display order of columns
- **Group** - Organize columns into logical groups (requires `ShowGroups` config)

## Footer Aggregates

Display aggregate calculations (sum, average, count, etc.) in column footers. The `.Footer()` method accepts a column selector, a label, and an aggregate function:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Header(u => u.Name, "Full Name")
    .Header(u => u.Salary, "Salary")
        .Footer(u => u.Salary, "Total", values => values.Sum())
        .Footer(u => u.Salary, "Avg", values => (int)values.Average())
    .Height(Size.Units(100))
```

**Multiple aggregates per column** can be added by calling `.Footer()` multiple times on the same column, or by using the tuple overload:

```csharp
data.ToDataTable()
    .Header(x => x.Qty, "Quantity")
        .Footer(x => x.Qty, new[]
        {
            ("Total", values => values.Sum()),
            ("Avg", values => (int)values.Average())
        })
    .Height(Size.Units(80))
```

**Footer features:**
- Calculate aggregates across the full dataset (not just visible rows)
- Common patterns: `.Sum()`, `.Average()`, `.Count()`, `.Min()`, `.Max()`
- Supports single or multiple aggregates per column

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

Add contextual actions to each row using `RowActions()` and handle them via `OnRowAction()`. Actions are rendered as icons or [buttons](../03_Common/01_Button.md) that appear when hovering over a row. Row actions support both simple menu items and nested [dropdown menus](../03_Common/11_DropDownMenu.md).

1. **Define actions with a tag** – Each `MenuItem` must have a **tag** (set via `.Tag(value)`) so the handler can tell which action was clicked. Use the fluent API: `MenuItem.Default(Icons.Pencil).Tag(YourEnum.Edit)` or `.Tag("edit")` for strings. Without a tag, the handler cannot distinguish actions.

2. **Single handler** – There is one `OnRowAction()` handler for all row actions. Inside it you must **identify the action from the tag** and branch (e.g. edit vs delete). If you don't branch on the tag, every click runs the same logic and edit/delete will not behave differently.

3. **Tag is `object?`** – The handler receives `RowActionClickEventArgs` with `args.Tag` of type `object?`. You must **convert it to a string** before comparing: use **`args.Tag?.ToString()`**. Comparing `args.Tag` directly to a string (e.g. `args.Tag == "edit"`) can fail and the action may do nothing. Prefer an enum and **`Enum.TryParse<YourEnum>(args.Tag?.ToString(), ignoreCase: true, out var action)`** for compile-time safety.

4. **Row ID** – Pass **`idSelector`** to `ToDataTable()` (e.g. `.ToDataTable(idSelector: e => e.Id)`) so `args.Id` identifies the row. Parse or cast `args.Id` to your entity ID type in the handler.

```csharp demo-tabs
public class RowActionsDemo : ViewBase
{
    private enum RowAction { Edit, Delete, More, Archive, Export, Share }

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
                MenuItem.Default(Icons.Pencil).Tag(RowAction.Edit),
                MenuItem.Default(Icons.Trash2).Tag(RowAction.Delete),
                MenuItem.Default(Icons.EllipsisVertical).Tag(RowAction.More)
                    .Children([
                        MenuItem.Default(Icons.Archive).Tag(RowAction.Archive).Label("Archive"),
                        MenuItem.Default(Icons.Download).Tag(RowAction.Export).Label("Export"),
                        MenuItem.Default(Icons.Share2).Tag(RowAction.Share).Label("Share")
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

## Empty States

Configure what users see when the table has no data. By default, DataTable shows a generic "No items found" message, but you can customize it to match your application's needs:

```csharp demo-tabs
public class EmptyDataTableDemo : ViewBase
{
    public record Person(int Id, string Name, string Email);

    public override object? Build()
    {
        // Empty list to demonstrate empty state
        var emptyList = new List<Person>().AsQueryable();

        return emptyList.ToDataTable(e => e.Id)
            .Header(e => e.Name, "Name")
            .Header(e => e.Email, "Email")
            .Empty((context) => Layout.Vertical()
                .Padding(16)
                .Gap(8)
                .AlignContent(Align.Center)
                | Text.Block("No people found")
                    .Large()
                    .Bold()
                | Text.Block("Add people to get started")
                    .Color(Colors.Muted)
                | new Button("Add Person", variant: ButtonVariant.Primary)
            )
            .Height(Size.Units(100));
    }
}
```

The `Empty` method accepts a `FuncViewBuilder` (which is `Func<IViewContext, object?>`) — you can use the context parameter to access hooks like UseState or UseQuery if needed, or simply ignore it and return a widget tree. Use Layout.Vertical(), Card, or any other layout to design your empty state exactly how you want it.

<Callout Type="tip">
Empty states are checked server-side by executing a Count() query. The empty state view is only rendered if the count is zero, preventing unnecessary "Loading..." states when data doesn't exist.
</Callout>

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

## Faq

<Details>
<Summary>
How do I handle row actions on a DataTable?
</Summary>
<Body>

**Requirements:** (1) Give each action a tag via `.Tag(...)`. (2) Use a single `.OnRowAction()` and **branch on the tag** so edit/delete run different logic. (3) **Convert the tag in the handler:** `args.Tag` is `object?` — use **`args.Tag?.ToString()`** (or `Enum.TryParse&lt;T&gt;(args.Tag?.ToString(), ...)`) before comparing. Without this, clicks can appear to do nothing.

Use the **fluent API** with **enum tags**: `MenuItem.Default(Icons.X).Tag(YourEnum.Value)` and parse in the handler with <c>Enum.TryParse&lt;YourEnum&gt;(args.Tag?.ToString(), ignoreCase: true, out var action)</c>:

```csharp
private enum RowAction { Edit, Delete }

items.ToDataTable(idSelector: e => e.Id)
    .RowActions(
        MenuItem.Default(Icons.Pencil).Tag(RowAction.Edit),
        MenuItem.Default(Icons.Trash2).Tag(RowAction.Delete))
    .OnRowAction(e =>
    {
        var args = e.Value;
        if (!Enum.TryParse<RowAction>(args.Tag?.ToString(), ignoreCase: true, out var action)) return ValueTask.CompletedTask;
        var rowId = ResolveId(args.Id);
        if (rowId < 0) return ValueTask.CompletedTask;
        if (action == RowAction.Edit) EditItem(rowId);
        else if (action == RowAction.Delete) DeleteItem(rowId);
        return ValueTask.CompletedTask;
    })
```

</Body>
</Details>

<Details>
<Summary>
How do I show navigation properties in a DataTable?
</Summary>
<Body>

`DataTableBuilder` only supports top-level properties of the model type. Nested property access like `p.Author.Username` will throw a `KeyNotFoundException` at runtime because only direct properties are scaffolded as columns.

**Solution:** Project your query into a flat DTO with all needed fields as direct properties:

```csharp
// BAD - nested property access will fail at runtime
var posts = db.Posts.Include(p => p.Author).AsQueryable();
posts.ToDataTable()
    .Header(p => p.Author.Username, "Author"); // KeyNotFoundException!

// GOOD - project into a flat DTO
record PostListItem(int Id, string Title, string AuthorName, string Status);

var posts = db.Posts
    .Include(p => p.Author)
    .Select(p => new PostListItem(p.Id, p.Title, p.Author.Username, p.Status.ToString()))
    .AsQueryable();

posts.ToDataTable()
    .Header(p => p.AuthorName, "Author"); // Works!
```

This also simplifies the DataTable configuration since you don't need to `.Hidden()` navigation properties or other fields you don't want displayed.

</Body>
</Details>

<Details>
<Summary>
How do I display dictionary or dynamic data in a DataTable?
</Summary>
<Body>

`ToDataTable()` uses reflection to discover columns from the model type's top-level properties. It does not expand `Dictionary<TKey, TValue>` properties into separate columns.

To display dynamic data, project it into a flat record first:

```csharp
// Instead of this:
record DataRow(int Id, Dictionary<string, string> Values);
rows.AsQueryable().ToDataTable(); // Shows "Id" and "Values" columns

// Do this — project into a flat anonymous type or record:
record FlatRow(string Name, int Age, string City);
var flat = rows.Select(r => new FlatRow(r.Values["Name"], int.Parse(r.Values["Age"]), r.Values["City"]));
flat.AsQueryable().ToDataTable(); // Shows Name, Age, City columns
```

If columns are truly dynamic (unknown at compile time), consider building a `List<Dictionary<string, object>>` and using `.ToTable()` with explicit column definitions instead.

</Body>
</Details>

<Details>
<Summary>
How do I handle row clicks in DataTable?
</Summary>
<Body>

There is no `OnRowClick` method. Use `OnCellClick` to handle individual cell clicks:

```csharp
items.ToDataTable()
    .OnCellClick(async e =>
    {
        var rowIndex = e.Value.RowIndex;
        var item = items[rowIndex];
        selectedItem.Set(item);
    })
```

For row-level action buttons (edit, delete), use the **fluent API** with **enum tags** and <c>Enum.TryParse</c>:

```csharp
private enum RowAction { Edit, Delete }

items.ToDataTable(idSelector: e => e.Id)
    .RowActions(
        MenuItem.Default(Icons.Pencil).Tag(RowAction.Edit),
        MenuItem.Default(Icons.Trash2).Tag(RowAction.Delete))
    .OnRowAction(e =>
    {
        var args = e.Value;
        if (!Enum.TryParse<RowAction>(args.Tag?.ToString(), ignoreCase: true, out var action)) return ValueTask.CompletedTask;
        var rowId = ResolveId(args.Id);
        if (rowId < 0) return ValueTask.CompletedTask;
        if (action == RowAction.Edit) { /* open edit dialog */ }
        else if (action == RowAction.Delete) { /* remove row */ }
        return ValueTask.CompletedTask;
    })
```

</Body>
</Details>

<Details>
<Summary>
When should I use DataTable instead of Table?
</Summary>
<Body>

**Use DataTable when:**

- ✅ You have a data collection to display (IQueryable, IEnumerable)
- ✅ You need sorting, filtering, or search functionality
- ✅ You're working with large datasets (100+ rows)
- ✅ You want pagination or incremental loading
- ✅ You need row actions (edit, delete buttons per row)
- ✅ You're converting from data grid libraries (AG Grid, React Table, Handsontable)

**Use Table when:**

- ✅ You need a simple layout table without interactivity
- ✅ You're displaying a small amount of static data (<20 rows)
- ✅ You need manual control over individual cells (TableCell, TableRow)
- ✅ You're rendering a report or summary (not raw data browsing)

**Example scenarios:**

```csharp
// DataTable — interactive stock data grid
stocks.AsQueryable().ToDataTable()
    .Header(s => s.Symbol, "Ticker")
    .Header(s => s.Price, "Price")
    .Config(c => c.AllowSorting = true)
    .Height(Size.Units(100))

// Table — static summary report
new[] {
    new { Metric = "Total Sales", Value = "$1.2M" },
    new { Metric = "Orders", Value = "342" }
}.ToTable().Width(Size.Full())
```

See [Table](../../03_Common/08_Table.md) documentation for simple table layouts.

</Body>
</Details>

<Details>
<Summary>
What properties are available on CellClickEventArgs?
</Summary>
<Body>

The `CellClickEventArgs` type provides these properties:

- `RowIndex` — zero-based index of the clicked row
- `ColumnIndex` — zero-based index of the clicked column
- `ColumnName` — name of the clicked column
- `CellValue` — value of the clicked cell

**Note**: There is NO `Row` or `Rows` property directly on the event args. To access the clicked row data, retrieve it from your original data source using `RowIndex`:

```csharp
var table = db.Tenants.ToList();
table.AsQueryable().ToDataTable((builder) => builder
    .OnCellClick((e) => {
        var tenant = table[e.Value.RowIndex]; // Access via index
        blades.Open(new TenantDetailsBlade(tenant.Id));
    })
);
```

</Body>
</Details>

<Details>
<Summary>
How do I define columns in a DataTable?
</Summary>
<Body>

Use the `Header()` method to define columns:

```csharp
.ToDataTable((table) => table
    .Header(t => t.Name, "Name")
    .Header(t => t.Email, "Email")
)
```

**Note**: There is NO `Table.Column` or `DataTableBuilder.AddColumn` method. Use `Header()` to define column headers and specify which properties to display.

</Body>
</Details>

<Details>
<Summary>
What's the difference between generic and non-generic UseMutation?
</Summary>
<Body>

Use the **non-generic** `UseMutation`:

```csharp
var mutation = this.UseMutation(async () => {
    await db.SaveChangesAsync();
});
```

**Note**: There is NO generic `UseMutation<TInput, TOutput>` or `MutationOptions<,>`. The method signature is:

```csharp
UseMutation(Func<Task> mutation, QueryOptions? options = null)
```

</Body>
</Details>

<Details>
<Summary>
How does UseRefreshToken work with DataTable?
</Summary>
<Body>

To programmatically refresh a `DataTable`, use the `UseRefreshToken` hook to create a token and pass it to the `DataTable` using the `.RefreshToken()` method. When you call `token.Refresh()`, the table will reload its data from the source.

```csharp
public override object? Build()
{
    var refreshToken = UseRefreshToken();
    var data = GetData().AsQueryable();

    return Layout.Vertical()
        | new Button("Refresh Data", _ => refreshToken.Refresh())
        | data.ToDataTable()
            .RefreshToken(refreshToken)
            .Height(Size.Units(100));
}
```

This is particularly useful after performing CRUD operations in dialogs or blades to ensure the table shows the most recent data.

</Body>
</Details>

<Details>
<Summary>
What options are available for QueryOptions?
</Summary>
<Body>

The `QueryOptions` type has these properties:

- `RefreshInterval` — polling frequency
- `LifetimeType` — controls when query is disposed
- `Tags` — for invalidation

**Note**: There is NO `OnSuccess` callback property on `QueryOptions`. Handle success in your mutation function directly.

</Body>
</Details>

<WidgetDocs Type="Ivy.DataTable" ExtensionTypes="Ivy.DataTableWidgetExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/DataTables/DataTable.cs"/>
