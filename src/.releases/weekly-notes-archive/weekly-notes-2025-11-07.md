# Ivy Framework Weekly Notes - Week of 2025-11-07

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## Overview

This release introduces a complete file upload system overhaul with automatic state management, progress tracking, and validation, along with breaking API changes. Major enhancements include extensive DataTable improvements (row actions, cell handlers, label/link columns, DateTime filtering), TextInput prefix/suffix support, customizable Chrome footer menus and wallpaper apps and interactive chart toolboxes.

## Bug Fixes

- **Reconnection Redirects**: Fixed incorrect redirect to `chrome=false` when reconnecting to apps with parent sessions
- **Blade Buttons**: Changed refresh/close buttons to ghost variant for cleaner appearance
- **Tab Close Button**: Dropdown menu now respects `.Closeable(false)` setting
- **Job Scheduler**: Improved error handling prevents unobserved task exceptions; errors shown once in child jobs
- **Sidebar Selection**: Fixed selection highlighting when clicking items during search
- **Browser History**: Fixed duplicate entries when switching tabs
- **App URLs**: Removed `-app` suffix from navigation URLs (e.g., `app://concepts/links` instead of `app://concepts/links-app`)
- **Filtered Lists**: Loading states now use muted text styling
- **Download Buttons**: Buttons with `/download/` URLs download in current tab instead of opening new tab
- **Code Widgets**: Copy button visibility improved in Card components
- **DataTable**: Child column padding reduced (16px → 8px), cell selection uses subtle highlight, row hover enabled by default
- **Expandable Widget**: Entire header area is now clickable (not just chevron)
- **Kanban**: Visual improvements (removed borders, better spacing, clickable titles only, line break preservation)

## Improvements

### Kanban Column Widths

Set custom widths for Kanban columns:

```csharp
return tasks.ToKanban(e => e.Status, e => e.Title, e => e.Description, e => e.Priority)
    .Width(Size.Full())
    .Width(e => e.Status, Size.Fraction(0.33f))  // Each column 1/3 width
    .Width("Todo", Size.Px(300))                 // Or specific widths
    .Build();
```

### Chart Theme Switching

Charts now respond smoothly to theme changes with memoization, improved color system, and better visual consistency.

### Chart Y-Axis Spacing

Improved automatic spacing for large value ranges.

### AsyncSelectInput Search

Built-in search field added to dropdown with 250ms throttling:

```csharp
var category = UseState<Guid?>(null);
return category.ToAsyncSelectInput(QueryCategories)
    .Placeholder("Select a category");
```

## New Features

### TextInput Prefix and Suffix Support

Add text or icons as prefixes/suffixes:

```csharp
var domain = UseState("example");
return domain.ToTextInput().Prefix("https://").Suffix(".com");

var email = UseState("");
return email.ToTextInput().Prefix(Icons.Mail).Placeholder("Enter email");
```

### DataTable Row Action Buttons

Action buttons appear on row hover:

```csharp
return employees.ToDataTable()
    .Builder<Employee>()
    .RowActions(
        new RowAction { Id = "edit", Icon = "Pencil", EventName = "OnEdit" },
        new RowAction { Id = "delete", Icon = "Trash", EventName = "OnDelete" }
    )
    .OnRowAction(e => {
        var args = e.Value;
        Console.WriteLine($"Action: {args.EventName}, Row: {args.RowIndex}");
        return ValueTask.CompletedTask;
    })
    .Build();
```

### DataTable Cell Event Handlers

Handle cell clicks and double-clicks:

```csharp
return employees.ToDataTable()
    .Builder<Employee>()
    .OnCellClick(e => {
        Console.WriteLine($"Clicked: {e.Value.ColumnName}");
        return ValueTask.CompletedTask;
    })
    .OnCellActivated(e => {
        // Double-click handler
        return ValueTask.CompletedTask;
    })
    .Build();
```

### DataTable Connection Persistence

Connections now persist across re-renders, preventing unnecessary data refetching when used in Sheets or conditional layouts.

### Customizable Chrome Footer Menu Items

Transform footer menu items:

```csharp
public override ChromeSettings GetChromeSettings()
{
    return new ChromeSettings()
        .UseSidebar()
        .UseFooterMenuItemsTransformer((items, navigator) =>
        {
            var customItems = new[]
            {
                MenuItem.Default("Documentation")
                    .Icon(Icons.Book)
                    .HandleSelect(() => navigator.Navigate("app://docs"))
            };
            return items.Concat(customItems);
        });
}
```

Default item tags: `$theme`, `$github`, `$logout`.

### DateTime Column Filtering in DataTables

Full DateTime filtering support with ISO format:

```
[HireDate] = "2024-05-30"
[OrderDate] >= "2024-01-01" AND [OrderDate] <= "2024-12-31"
```

### DataTable Label Columns

Display `string[]` properties as visual chips:

```csharp
public record Employee(int Id, string Name, string[] Skills);

return employees.ToDataTable()
    .Builder<Employee>()
    .Header(e => e.Skills, "Skills")
    .Build();
```

Supports filtering by individual labels and sorting by first label.

### DataTable Link Columns

Clickable links in cells (Ctrl/Cmd+Click):

```csharp
return employees.ToDataTable()
    .Builder<Employee>()
    .Header(e => e.ProfileLink, "Profile")
    .DataTypeHint(e => e.ProfileLink, ColType.Link)
    .Build();
```

External URLs open in new tabs; internal URLs navigate in same tab.

### Custom Display Content in Job Scheduler

Display custom UI content in jobs:

```csharp
var job = new Job("Processing data", async (job, scheduler, progress, token) =>
{
    job.SetDisplay(new Text("Analyzing files...").FontSize(12));
    await Task.Delay(1000);
    job.SetDisplay(new Text("Generating report...").FontSize(12));
    return true;
});
```

### Field Widget Help Text Support

Add help tooltips to fields:

```csharp
return model.ToForm()
    .Label(m => m.Email, "Email Address")
    .Help(m => m.Email, "We'll never share your email with third parties")
    .Build();
```

### Chrome Wallpaper Apps

Display an app when no tabs are open:

```csharp
public override ChromeSettings GetChromeSettings()
{
    return new ChromeSettings()
        .UseSidebar()
        .WallpaperApp<WelcomeScreen>();
}
```

### Interactive Chart Toolbox

Enable interactive toolbox on charts:

```csharp
return new BarChart(data)
    .Bar("Sales")
    .XAxis(new XAxis("Month"))
    .Toolbox(); // Save as image, view data, switch chart types, restore

// Customize features
return new LineChart(data)
    .Toolbox(new Toolbox()
        .SaveAsImage(true)
        .DataView(true)
        .MagicType(true)  // Toggle line/bar
        .Restore(true)
    );
```

### OpenAPI Connection Support

Add OpenAPI connections via CLI:

```bash
ivy connect openapi add https://api.example.com/openapi.json
```

Automatically generates connection class, client factory, and service registration. Supports API Key and Bearer Token authentication.

## File Upload System Overhaul

Complete redesign with automatic state management, progress tracking, and validation.

**New API:**

```csharp
var file = UseState<FileUpload<byte[]>?>();
var upload = this.UseUpload(MemoryStreamUploadHandler.Create(file))
    .Accept("image/*")
    .MaxFileSize(FileSize.FromMegabytes(5))
    .MaxFiles(3);

return file.ToFileInput(upload).Placeholder("Choose up to 3 images");
```

**Features:**

- Automatic progress tracking
- Toast notifications for validation errors
- Upload cancellation
- Form integration with upload protection
- Streaming support via `ChunkedMemoryStreamUploadHandler`

**Breaking Changes:**

- `FileInput` → `FileUpload<T>`
- `UseUpload` now requires handler: `MemoryStreamUploadHandler.Create(state)`
- `ToFileInput(uploadUrl)` → `ToFileInput(uploadContext)`
- `AudioRecorder` now requires upload context as first parameter

## New Utilities

### FileSize Helper

```csharp
.MaxFileSize(FileSize.FromMegabytes(5))
.MaxFileSize(FileSize.FromKilobytes(500))
.MaxFileSize(FileSize.FromGigabytes(2))
```

### FileTypes Constants

```csharp
.Accept(FileTypes.Pdf)  // "application/pdf"
.Accept(FileTypes.Text) // "text/plain"
```

## Performance Improvements

- **Async Event Handling**: Long-running operations no longer block UI updates
- **File Upload Performance**: Dedicated event queue, update coalescing, increased ThreadPool workers

## API Changes

- **Button Loading**: `.Loading()` now accepts `IState<bool>` directly
- **MenuItem Composition**: Pipe operator `|` for building menus: `MenuItem.Default("File") | MenuItem.Default("New")`
- **Generic Navigation**: `navigator.Navigate<SettingsApp>()` (no `typeof` needed)
- **Server Registration**: `server.AddApp<MyApp>()` (no `typeof` needed)
- **Configuration Access**: `IConfiguration` automatically registered in service collection
- **Column Removal**: Only hides fields starting with `_` + letter (e.g., `_hidden`), not `_1` or `_$special`
- **DropDownMenu**: `.Items()` now accepts `IEnumerable<MenuItem>`
- **DataTable Config**: `DataTableConfiguration` → `DataTableConfig`, `Configuration` property → `Config`

## Security & Dependencies

### Authentication Protection

Upload, download, and DataTable services now automatically require authentication when `IAuthProvider` is configured. Includes improved token handling with smart cookie splitting for large tokens.

### Frontend Security Update

All npm packages updated to latest versions (Radix UI, React syntax highlighter, Mermaid 11.12.1, Lucide React icons).

## CLI & Tooling

### Database Generator Automation

```bash
ivy db generate \
  --prompt "A blog database with posts, authors, and comments" \
  --provider Postgres \
  --connection-string "Host=localhost;Database=mydb" \
  --yes-to-all
```

Supports `--prompt`, `--dbml`, `--provider`, `--connection-string`, and `--yes-to-all` flags. Generates `RecreateDatabase.ps1` script.

### Project Initialization Automation

```bash
ivy init --yes-to-all --namespace MyApp
```

### ivy fix Improvements

- `--verbose` flag for detailed output
- `--model-id` option for custom AI models
- Better error messages for invalid directories
- Fixed exit codes for CI/CD integration

### ivy login

Now displays subscription details immediately after authentication.
