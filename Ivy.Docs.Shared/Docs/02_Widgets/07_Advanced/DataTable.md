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

- **LoadAllRows(true)** - Load all rows at once for maximum performance with very large datasets
- **BatchSize(n)** - Load data in batches of n rows for incremental loading

</Body>
</Details>

<WidgetDocs Type="Ivy.DataTable" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/DataTables/DataTable.cs"/>
