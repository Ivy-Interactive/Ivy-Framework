---
prepare: |
    var sampleUsers = new[] {
        new { Name = "John Smith", Email = "john@example.com", Age = 28, IsActive = true, Status = Icons.Rocket, Salary = 45000, JoinDate = DateTime.Now.AddDays(-200) },
        new { Name = "Sarah Johnson", Email = "sarah@example.com", Age = 34, IsActive = true, Status = Icons.Star, Salary = 55000, JoinDate = DateTime.Now.AddDays(-350) },
        new { Name = "Mike Brown", Email = "mike@example.com", Age = 42, IsActive = false, Status = Icons.ThumbsUp, Salary = 65000, JoinDate = DateTime.Now.AddDays(-500) },
        new { Name = "Emily Davis", Email = "emily@example.com", Age = 26, IsActive = true, Status = Icons.Rocket, Salary = 48000, JoinDate = DateTime.Now.AddDays(-150) },
        new { Name = "Alex Wilson", Email = "alex@example.com", Age = 31, IsActive = true, Status = Icons.Star, Salary = 52000, JoinDate = DateTime.Now.AddDays(-400) },
        new { Name = "Lisa Chen", Email = "lisa@example.com", Age = 29, IsActive = true, Status = Icons.Heart, Salary = 47000, JoinDate = DateTime.Now.AddDays(-180) },
        new { Name = "David Miller", Email = "david@example.com", Age = 35, IsActive = false, Status = Icons.X, Salary = 58000, JoinDate = DateTime.Now.AddDays(-320) },
        new { Name = "Jessica Taylor", Email = "jessica@example.com", Age = 27, IsActive = true, Status = Icons.Check, Salary = 46000, JoinDate = DateTime.Now.AddDays(-120) },
        new { Name = "Robert Garcia", Email = "robert@example.com", Age = 39, IsActive = true, Status = Icons.Star, Salary = 62000, JoinDate = DateTime.Now.AddDays(-450) },
        new { Name = "Amanda White", Email = "amanda@example.com", Age = 33, IsActive = false, Status = Icons.Clock, Salary = 54000, JoinDate = DateTime.Now.AddDays(-280) },
        new { Name = "Kevin Lee", Email = "kevin@example.com", Age = 30, IsActive = true, Status = Icons.Rocket, Salary = 50000, JoinDate = DateTime.Now.AddDays(-220) },
        new { Name = "Michelle Rodriguez", Email = "michelle@example.com", Age = 36, IsActive = true, Status = Icons.ThumbsUp, Salary = 59000, JoinDate = DateTime.Now.AddDays(-380) },
        new { Name = "Christopher Martinez", Email = "chris@example.com", Age = 41, IsActive = false, Status = Icons.X, Salary = 64000, JoinDate = DateTime.Now.AddDays(-520) },
        new { Name = "Jennifer Lopez", Email = "jennifer@example.com", Age = 32, IsActive = true, Status = Icons.Heart, Salary = 53000, JoinDate = DateTime.Now.AddDays(-240) },
        new { Name = "Daniel Anderson", Email = "daniel@example.com", Age = 25, IsActive = true, Status = Icons.Star, Salary = 43000, JoinDate = DateTime.Now.AddDays(-90) },
        new { Name = "Nicole Thompson", Email = "nicole@example.com", Age = 38, IsActive = true, Status = Icons.Check, Salary = 61000, JoinDate = DateTime.Now.AddDays(-420) },
        new { Name = "Matthew Jackson", Email = "matthew@example.com", Age = 37, IsActive = false, Status = Icons.Circle, Salary = 60000, JoinDate = DateTime.Now.AddDays(-360) },
        new { Name = "Stephanie Harris", Email = "stephanie@example.com", Age = 24, IsActive = true, Status = Icons.Rocket, Salary = 42000, JoinDate = DateTime.Now.AddDays(-60) },
        new { Name = "Andrew Clark", Email = "andrew@example.com", Age = 40, IsActive = true, Status = Icons.Star, Salary = 63000, JoinDate = DateTime.Now.AddDays(-480) },
        new { Name = "Rachel Lewis", Email = "rachel@example.com", Age = 29, IsActive = false, Status = Icons.Clock, Salary = 49000, JoinDate = DateTime.Now.AddDays(-200) }
    }.AsQueryable();
---

# DataTable

<Ingress>
Display and interact with large datasets using high-performance data tables with sorting, filtering, pagination, and real-time updates powered by Apache Arrow.
</Ingress>

The `DataTable` widget provides a powerful, high-performance solution for displaying tabular data with advanced features like sorting, filtering, column management, and real-time data streaming. Built on Apache Arrow for optimal performance with large datasets.

## Basic Usage

Create a DataTable from any `IQueryable<T>` using the `.ToDataTable()` extension method:

```csharp demo
sampleUsers.ToDataTable()
    .Header(u => u.Name, "Full Name")
    .Header(u => u.Email, "Email Address")
    .Header(u => u.Age, "Age")
```

## Column Configuration

### Headers and Labels

Customize column headers with descriptive labels:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Header(u => u.Name, "Employee Name")
    .Header(u => u.Email, "Contact Email")
    .Header(u => u.Age, "Years Old")
    .Header(u => u.Salary, "Annual Salary")
```

### Column Alignment

Control text alignment based on data type:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Header(u => u.Name, "Name").Align(u => u.Name, Align.Left)
    .Header(u => u.Age, "Age").Align(u => u.Age, Align.Center)
    .Header(u => u.Salary, "Salary").Align(u => u.Salary, Align.Right)
```

### Column Width

Set specific widths for optimal layout:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Header(u => u.Name, "Name").Width(u => u.Name, Size.Px(200))
    .Header(u => u.Email, "Email").Width(u => u.Email, Size.Px(250))
    .Header(u => u.Age, "Age").Width(u => u.Age, Size.Px(80))
```

### Column Ordering

Specify the display order of columns:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Order(u => u.Name, u => u.Age, u => u.Salary, u => u.Email)
    .Header(u => u.Name, "Name")
    .Header(u => u.Email, "Email")
    .Header(u => u.Age, "Age")
    .Header(u => u.Salary, "Salary")
```

## Sorting and Filtering

### Column Sorting

Enable or disable sorting per column:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Header(u => u.Name, "Name").Sortable(u => u.Name, true)
    .Header(u => u.Age, "Age").Sortable(u => u.Age, true)
    .Header(u => u.Email, "Email").Sortable(u => u.Email, false)
```

### Default Sort Direction

Set initial sort order:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Header(u => u.Name, "Name").SortDirection(u => u.Name, SortDirection.Ascending)
    .Header(u => u.Salary, "Salary").SortDirection(u => u.Salary, SortDirection.Descending)
```

### Column Filtering

Control which columns can be filtered:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Header(u => u.Name, "Name").Filterable(u => u.Name, true)
    .Header(u => u.Age, "Age").Filterable(u => u.Age, true)
    .Header(u => u.JoinDate, "Join Date").Filterable(u => u.JoinDate, false)
```

## Data Type Handling

### Automatic Type Detection

DataTable automatically detects column types:

- `string`, `char` → Text
- `int`, `long`, `decimal`, `double` → Number  
- `bool` → Boolean
- `DateTime`, `DateTimeOffset` → DateTime
- `DateOnly` → Date
- `Icons` enum → Icon

### Manual Type Hints

Override automatic detection when needed:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Header(u => u.Age, "Age Category")
    .DataTypeHint(u => u.Age, ColType.Text)
```

## Table Configuration

### Search and Filtering Options

Configure global table behavior:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Config(config => {
        config.AllowSearch = true;
        config.FilterType = FilterTypes.List;
        config.AllowSorting = true;
        config.AllowFiltering = true;
    })
```

### Column Management

Control column interaction features:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Config(config => {
        config.AllowColumnReordering = true;
        config.AllowColumnResizing = true;
        config.FreezeColumns = 2;
    })
```

### Selection Modes

Configure how users can select data:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Config(config => {
        config.SelectionMode = SelectionModes.MultipleRows;
        config.AllowCopySelection = true;
    })
```

### Visual Options

Customize table appearance:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Config(config => {
        config.ShowIndexColumn = true;
        config.ShowGroups = false;
    })
```

## Size and Layout

### Table Dimensions

Control the overall table size:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Width(Size.Px(300))
    .Height(Size.Px(400))
```

### Hidden Columns

Hide specific columns while keeping them in the data:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Hidden([u => u.Email])
    .Header(u => u.Name, "Name")
    .Header(u => u.Age, "Age")
```

## Help and Documentation

Add contextual help to columns:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Header(u => u.Salary, "Salary")
    .Help(u => u.Salary, "Annual salary in USD before taxes")
    .Header(u => u.IsActive, "Status")
    .Help(u => u.IsActive, "Whether the employee is currently active")
```

## Column Grouping

Organize related columns under group headers:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Header(u => u.Name, "Name").Group(u => u.Name, "Personal")
    .Header(u => u.Age, "Age").Group(u => u.Age, "Personal")
    .Header(u => u.Salary, "Salary").Group(u => u.Salary, "Employment")
    .Header(u => u.JoinDate, "Join Date").Group(u => u.JoinDate, "Employment")
    .Config(config => config.ShowGroups = true)
```

## Advanced Features

### Custom Renderers

Use specialized renderers for different data types:

```csharp
// Number formatting
.Renderer(u => u.Salary, new NumberDisplayRenderer { Format = "C2" })

// Date formatting  
.Renderer(u => u.JoinDate, new DateTimeDisplayRenderer { Format = "MMM dd, yyyy" })

// Link rendering
.Renderer(u => u.Email, new LinkDisplayRenderer { Type = LinkDisplayType.Email })

// Progress bars
.Renderer(u => u.Progress, new ProgressDisplayRenderer())
```

### Real-time Data Updates

DataTable automatically reflects changes to the underlying `IQueryable`:

```csharp
var users = GetUsersFromDatabase().AsQueryable();
return users.ToDataTable()
    .Header(u => u.Name, "Name")
    .Header(u => u.LastUpdated, "Last Seen");
```

### Performance with Large Datasets

DataTable is optimized for large datasets using Apache Arrow:

```csharp
var bigDataset = GetMillionsOfRecords().AsQueryable();
return bigDataset.ToDataTable()
    .Config(config => {
        config.AllowSearch = true;
        config.FilterType = FilterTypes.Tree;
    });
```

## Configuration Reference

### FilterTypes

- `None` - No filtering
- `List` - Simple list-based filters  
- `Tree` - Hierarchical tree filters

### SelectionModes

- `None` - No selection
- `SingleRow` - Single row selection
- `SingleColumn` - Single column selection  
- `MultipleRows` - Multiple row selection
- `MultipleColumns` - Multiple column selection
- `Cells` - Individual cell selection

### ColType

- `Text` - String data
- `Number` - Numeric data
- `Boolean` - True/false values
- `Date` - Date only
- `DateTime` - Date and time
- `Icon` - Icon enum values

<Callout Type="tip">
DataTable provides automatic scaffolding - it will detect your model properties and create appropriate columns automatically. You only need to customize the columns you want to change.
</Callout>

<Callout Type="info">
For optimal performance with very large datasets (1M+ rows), consider using server-side filtering and pagination by implementing custom `IQueryable` providers.
</Callout>

<WidgetDocs Type="Ivy.DataTable" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/DataTables/DataTable.cs"/>
