# Ivy Framework Weekly Notes - Week of 2025-11-07

## Bug Fixes & Improvements

### Fixed Reconnection Redirects with Parent Sessions

Fixed an issue where the framework would incorrectly redirect to `chrome=false` when reconnecting to an app that has a parent session. The redirect logic now properly checks for the presence of a parent session instead of trying to find a Chrome app, which could create false negatives if the parent had disconnected.

This fix ensures smoother reconnection handling in multi-window and embedded app scenarios where child apps maintain references to parent sessions.

### Improved Blade Widget Button Styling

The Blade widget's refresh and close buttons now use a more subtle "ghost" variant instead of the "outline" variant. This provides a cleaner, less visually cluttered appearance in the blade header while maintaining full functionality. The ghost variant removes the button border, making the UI feel lighter and more modern, especially when working with multiple blades.

### Fixed Tab Close Button in Dropdown Menus

The tabs layout dropdown menu now correctly respects the closeable setting. Previously, when you had many tabs and they overflowed into a dropdown menu (the "..." menu), the close button (X icon) would always appear on each tab item in the dropdown, even when tabs were configured to not be closeable using `.Closeable(false)`.

The close button now only appears in the dropdown menu when tabs are actually closeable, maintaining consistency with the tab bar behavior. This prevents user confusion and accidental tab closures in layouts where tabs should be permanent.

### Enhanced Job Scheduler Error Handling

The JobScheduler now has improved error handling that prevents unobserved task exceptions from being raised when jobs fail. Previously, if a job threw an exception, it could result in unobserved task exceptions that would crash the application when the task was garbage collected.

The scheduler also now shows error messages more intelligently in the UI. When a parent job has child jobs and one of the children fails, the error is only displayed once at the child level instead of being duplicated in both the parent and child job displays. This prevents cluttered error displays in complex job hierarchies.

### Sticky Category Headers in Sidebar Navigation

Sidebar category headers now remain visible at the top of the sidebar as you scroll through long menu lists. This makes it easier to maintain context about which section of the navigation you're currently viewing, especially in applications with many menu items organized into multiple categories.

The improvement automatically applies to both root-level and nested sidebar categories, helping users stay oriented as they navigate through complex menu structures. Category headers will "stick" to the top of the sidebar viewport and stay visible until the next category comes into view.

### Fixed Sidebar Selection Highlighting During Search

Fixed an issue where clicking on sidebar items while the search was active would not properly update the selection highlighting. The sidebar now correctly maintains visual feedback when you click on search results, ensuring that the selected item is always clearly indicated. This makes navigation more intuitive when using the sidebar search feature to quickly find and access menu items.

### Fixed Duplicate Browser History Entries When Switching Tabs

Fixed an issue where switching between tabs in the Chrome interface would create duplicate entries in the browser's history stack. Previously, clicking on a tab that was already selected would trigger an unnecessary URL redirect, adding a redundant history entry even though the user remained on the same tab.

The framework now checks if the selected tab index has actually changed before updating the browser URL. This prevents duplicate history entries and provides a cleaner navigation experience when clicking through tabs. Users can now use browser back/forward buttons more predictably without encountering unnecessary duplicate tab states in their history.

### Simplified App URL Scheme

App URLs in the framework now use a cleaner format without the `-app` suffix. This makes navigation URIs more concise and easier to read. The change applies to all app navigation scenarios, including programmatic navigation and documentation links.

**Before:**

```csharp
navigator.Navigate("app://concepts/links-app");
navigator.Navigate("app://hidden/hidden-args-app", args);
```

**After:**

```csharp
navigator.Navigate("app://concepts/links");
navigator.Navigate("app://hidden/hidden-args", args);
```

The framework automatically handles the suffix removal when generating app IDs from class names. For example, `HiddenArgsApp` now generates the app ID `hidden/hidden-args` instead of `hidden/hidden-args-app`. This applies to both manual navigation calls and automatically generated URLs in documentation and sample code.

### Better Loading State in Filtered Lists

The FilteredListView now displays loading states with muted text styling instead of plain text, providing better visual consistency with the rest of the framework's UI patterns. The loading message now appears in a subtle gray color that's appropriate for secondary information.

This improvement applies automatically to all uses of FilteredListView, including the app repository sidebar and any custom filtered lists in your applications.

### Improved Download Button Behavior

Buttons with download URLs now behave more intuitively. When a button's URL points to a download endpoint (URLs starting with `/download/`), clicking the button now downloads the file directly in the current tab instead of opening it in a new window or tab.

This improvement applies automatically to any button that uses a download URL:

```csharp
// Button with download URL - downloads in current tab
return new Button("Download Report")
    .Url("/download/report-123.pdf");

// Button with regular URL - still opens in new tab
return new Button("View Documentation")
    .Url("https://docs.example.com");
```

This provides a better user experience for file downloads while maintaining the expected behavior for external links and navigation URLs. Users no longer need to deal with unnecessary new tabs when downloading files from your application.

### Improved Copy Button Visibility in Code Widgets

The copy button in CodeInput widgets is now more visible across different UI contexts, especially when used inside Card components. Previously, the button could blend into the background when placed in certain elements, making it difficult for users to discover the copy-to-clipboard functionality. The button now uses a transparent background that adapts better to its container while maintaining good hover states.

This improvement is particularly noticeable when using CodeInput widgets within Cards or other nested components:

```csharp
// Copy button is now clearly visible in all contexts
var code = UseState("Console.WriteLine(\"Hello World\");");

return new Card(
    code.ToCodeInput()
        .Language(Languages.Csharp)
        .ShowCopyButton()
)
.Title("Code Example");
```

### Improved DataTable Child Column Spacing

Child columns in hierarchical DataTables now have more compact horizontal padding, reducing visual clutter and improving the presentation of nested data. The horizontal padding has been reduced from 16px to 8px, making child rows more visually distinct from parent rows while maximizing available space for content.

This improvement is automatically applied to all DataTables that use child columns or grouped data, providing better visual hierarchy and more efficient use of screen space.

### Cleaner DataTable Cell Selection

DataTable cell selection now uses a more subtle visual style. When you select a cell, instead of showing a prominent green border, the cell now displays only a light background highlight. This provides clear visual feedback about which cell is selected while reducing visual noise and creating a cleaner, more professional appearance.

The filter dropdown in DataTables has also been refined with smaller, more consistent typography. The dropdown font size has been reduced from 14px to 12px to match the placeholder text, and completion details are now shown in 11px font. The dropdown items also use improved flexbox layout for better alignment of filter options and their descriptions.

### Row Hover Effect Enabled by Default

DataTables now enable row hover highlighting by default, making it easier to track which row you're viewing when scanning through data. When you move your mouse over a row, it highlights with a subtle background color to help maintain visual context. This behavior was previously disabled by default but is now automatically enabled, providing better usability out of the box.

### Enhanced Expandable Widget Interaction Area

The Expandable widget now has a much more intuitive interaction model—clicking anywhere on the header area toggles the expansion, not just the chevron icon. Previously, users had to click precisely on the small chevron button to expand or collapse the content, which could be frustrating especially on touch devices.

The entire header area is now clickable with subtle hover feedback (a light accent background), making it much easier to expand and collapse sections. The chevron icon remains visible as a visual indicator but no longer needs to be the precise click target. This brings the Expandable widget in line with modern UX patterns where the entire header acts as an interactive surface.

When the Expandable is disabled, the entire header area correctly shows a "not-allowed" cursor and doesn't respond to clicks or show hover effects.

### Kanban Board Design Improvements

The Kanban widget has received several visual and UX improvements for a cleaner, more modern appearance:

**Visual Enhancements:**

- **Cleaner layout**: Removed vertical borders from scroll bars and column containers, reducing visual clutter
- **Better spacing**: Reduced gaps between cards and columns for a more compact, organized appearance
- **Card title interaction**: Only card titles are now clickable and show a hover underline effect, making it clearer where to click to open cards
- **Description formatting**: Card descriptions now preserve line breaks, making multi-line text more readable

**Column Width Configuration:**

You can now set custom widths for Kanban columns using the new `.Width()` methods on the builder:

```csharp
// Set individual column widths using group key selector
return tasks.ToKanban(
    groupSelector: e => e.Status,
    titleSelector: e => e.Title,
    descriptionSelector: e => e.Description,
    orderSelector: e => e.Priority)
    .Width(Size.Full())  // Make kanban take full width
    .Width(e => e.Status, Size.Fraction(0.33f))  // Each column takes 1/3 width
    .ColumnTitle(status => status switch
    {
        "Todo" => "To Do",
        "InProgress" => "In Progress",
        "Done" => "Completed",
        _ => status
    })
    .Build();
```

You can also set widths for specific column values:

```csharp
// Set width for a specific column by group key value
return tasks.ToKanban(e => e.Status, e => e.Title, e => e.Description, e => e.Priority)
    .Width("Todo", Size.Px(300))        // Todo column: 300px
    .Width("InProgress", Size.Px(500))  // In Progress: 500px
    .Width("Done", Size.Px(400))        // Done: 400px
    .Build();
```

These changes make Kanban boards more visually polished and provide better control over layout, especially useful when working with different amounts of content in each column.

## New Features

### TextInput Prefix and Suffix Support

TextInput widgets now support prefix and suffix decorations, allowing you to add helpful context directly inside the input field. You can use either text or icons as prefixes/suffixes, making it easy to create professional-looking inputs for URLs, currencies, email addresses, and more.

**Basic Text Prefix and Suffix:**

```csharp
var domain = UseState("example");

return Layout.Vertical()
    | domain.ToTextInput().Prefix("https://")
    | domain.ToTextInput().Suffix(".com")
    | domain.ToTextInput().Prefix("https://").Suffix(".com");
```

**Icon Prefix and Suffix:**

```csharp
var email = UseState("");

return Layout.Vertical()
    | email.ToTextInput().Prefix(Icons.Mail)
    | email.ToTextInput().Suffix(Icons.Search)
    | email.ToTextInput().Prefix(Icons.Mail).Suffix(Icons.Mail);
```

**Practical Examples:**

```csharp
// Currency input
var amount = UseState<string?>(null);
return amount.ToTextInput().Prefix("$").Placeholder("Amount");

// Percentage input
var rate = UseState<string?>(null);
return rate.ToTextInput().Suffix("%").Placeholder("Percentage");

// URL input
var website = UseState("mysite");
return website.ToTextInput().Prefix("https://").Suffix(".com").Placeholder("domain");

// Email input with icon
var email = UseState("");
return email.ToTextInput().Prefix(Icons.Mail).Placeholder("Enter email");

// Search input with icon
var search = UseState("");
return search.ToTextInput().Suffix(Icons.Search).Placeholder("Search...");
```

**Styling:**

Prefixes and suffixes are displayed in a muted background color with proper borders, creating a clear visual distinction from the input area. They integrate seamlessly with all TextInput features including validation, disabled states, and shortcut keys:

```csharp
// Works with validation and shortcuts
var username = UseState<string?>(null);
return username.ToTextInput()
    .Prefix("@")
    .Invalid("Username is required")
    .ShortcutKey("Ctrl+U");
```

The prefix/suffix feature uses a discriminated union type internally, ensuring that each affix can be either text or an icon, but never both simultaneously. This provides type safety and prevents configuration errors.

### DataTable Row Action Buttons

DataTables now support row action buttons that appear on the right side of rows when you hover over them. This feature provides a clean way to add contextual actions (like edit, delete, view, or custom operations) to each row without cluttering your table with extra columns.

**Basic Usage:**

```csharp
// Configure row actions with icons
return employees.ToDataTable()
    .Builder<Employee>()
    .Header(e => e.Name, "Name")
    .Header(e => e.Email, "Email")
    .RowActions(
        new RowAction { Id = "edit", Icon = "Pencil", EventName = "OnEdit" },
        new RowAction { Id = "delete", Icon = "Trash", EventName = "OnDelete" },
        new RowAction { Id = "view", Icon = "Eye", EventName = "OnView" }
    )
    .OnRowAction(e =>
    {
        var args = e.Value;
        Console.WriteLine($"Action: {args.EventName}, Row: {args.RowIndex}");

        // Access row data
        var rowData = args.RowData;
        var name = rowData["Name"];
        var email = rowData["Email"];

        return ValueTask.CompletedTask;
    })
    .Build();
```

**Advanced Example with Sheet Integration:**

```csharp
public override object? Build()
{
    var employees = GetEmployees().AsQueryable();
    var selectedRow = UseState<int?>(null);
    var sheetOpen = UseState(false);

    var dataTable = employees.ToDataTable()
        .Builder<Employee>()
        .RowActions(
            new RowAction { Id = "menu", Icon = "EllipsisVertical", EventName = "OnRowMenu" }
        )
        .OnRowAction(e =>
        {
            selectedRow.Set(e.Value.RowIndex);
            sheetOpen.Set(true);
            return ValueTask.CompletedTask;
        })
        .Build();

    return new Fragment(
        dataTable,
        sheetOpen.Value
            ? new Sheet(_ => { sheetOpen.Set(false); return ValueTask.CompletedTask; },
                GetEmployeeDetails(selectedRow.Value),
                "Employee Details",
                "View and manage employee information")
            : null
    );
}
```

**Row Action Properties:**

Each `RowAction` has the following properties:

- **Id**: Unique identifier for the action
- **Icon**: Lucide icon name (e.g., "Pencil", "Trash", "Eye", "EllipsisVertical")
- **EventName**: The event name passed to your handler (useful for routing different actions)
- **Tooltip**: Optional tooltip text (currently not displayed but reserved for future use)

**RowActionClickEventArgs Properties:**

When a row action is clicked, the event handler receives these details:

- **ActionId**: The ID of the clicked action
- **EventName**: The event name you configured
- **RowIndex**: The zero-based index of the row
- **RowData**: A dictionary containing all column values for that row, keyed by column name

The action buttons automatically position themselves at the vertical center of each row and smoothly fade in/out as you hover. This provides a clean, unobtrusive way to access row-level actions without permanently dedicating table columns to action buttons.

### DataTable Cell Event Handlers

DataTables now support cell-level event handlers, allowing you to respond to user interactions with individual cells. This enables scenarios like opening detail views, triggering workflows, or navigating to related data when users click or double-click on cells.

**OnCellClick (Single-Click):**

```csharp
return employees.ToDataTable()
    .Builder<Employee>()
    .OnCellClick(e =>
    {
        var args = e.Value;
        Console.WriteLine($"Clicked: Row {args.RowIndex}, Column {args.ColumnName}");
        Console.WriteLine($"Value: {args.CellValue}");

        return ValueTask.CompletedTask;
    })
    .Build();
```

**OnCellActivated (Double-Click):**

```csharp
public override object? Build()
{
    var employees = GetEmployees().AsQueryable();
    var selectedCell = UseState<CellClickEventArgs?>(null);
    var sheetOpen = UseState(false);

    var dataTable = employees.ToDataTable()
        .Builder<Employee>()
        .Header(e => e.Name, "Name")
        .Header(e => e.Email, "Email")
        .OnCellActivated(e =>
        {
            selectedCell.Set(e.Value);
            sheetOpen.Set(true);
            return ValueTask.CompletedTask;
        })
        .Build();

    return new Fragment(
        dataTable,
        sheetOpen.Value
            ? new Sheet(_ => { sheetOpen.Set(false); return ValueTask.CompletedTask; },
                new Card(
                    new StackLayout([
                        $"Row: {selectedCell.Value?.RowIndex}",
                        $"Column: {selectedCell.Value?.ColumnName}",
                        $"Value: {selectedCell.Value?.CellValue}"
                    ], gap: 8)
                ).Title("Cell Details"),
                "Cell Information",
                "Details about the activated cell")
            : null
    );
}
```

**CellClickEventArgs Properties:**

Both event handlers receive these properties:

- **RowIndex**: The zero-based row index of the clicked cell
- **ColumnIndex**: The zero-based column index
- **ColumnName**: The name of the column (as configured in your DataTable)
- **CellValue**: The actual value in the cell (typed as `object?`)

**Performance Note:**

Cell event handlers are only enabled when you configure them using `.OnCellClick()` or `.OnCellActivated()`. The DataTable automatically sets `EnableCellClickEvents = true` in the configuration when these handlers are present, ensuring optimal performance when cell events aren't needed.

These event handlers integrate seamlessly with the rest of the Ivy Framework, including Sheets, navigation, and state management. This makes it easy to build rich, interactive data experiences with minimal code.

### Improved DataTable Connection Persistence

The DataTable's connection management has been significantly improved to prevent unnecessary reconnections and data refetching. Previously, when using DataTables inside components like Sheets or other conditionally rendered containers, the connection would be recreated each time the component re-rendered, causing data to be fetched again unnecessarily.

**The Problem (Before):**

```csharp
// Opening/closing a Sheet would recreate the DataTable connection
var sheetOpen = UseState(false);

return new Fragment(
    sheetOpen.Value
        ? new Sheet(
            _ => { sheetOpen.Set(false); return ValueTask.CompletedTask; },
            employees.ToDataTable().Build(), // Connection recreated each time!
            "Employees"
          )
        : null
);
```

**The Solution (Now):**

The DataTable infrastructure now uses smart connection caching with `buildOnChange: false` and a `hasRun` flag to ensure connections are created only once and persist across re-renders:

```csharp
// Connection persists even when Sheet opens/closes
var sheetOpen = UseState(false);
var dataTable = employees.ToDataTable().Build(); // Created once

return new Fragment(
    dataTable, // Always rendered
    sheetOpen.Value
        ? new Sheet(_ => { sheetOpen.Set(false); return ValueTask.CompletedTask; },
            GetDetails(),
            "Details")
        : null
);
```

**Implementation Details:**

The `UseDataTable` hook now:

1. Creates the connection only once using a `hasRun` flag
2. Disables automatic rebuilding when the connection state changes (`buildOnChange: false`)
3. Properly registers cleanup handlers to dispose connections when the view is unmounted
4. Implements `IMemoized` on `DataTableView` and `DataTableBuilder` to optimize memoization

**Benefits:**

- No unnecessary data refetching when parent components re-render
- Maintains scroll position and selection state across re-renders
- Improved performance for DataTables inside Sheets, dialogs, or conditional layouts
- Single connection lifecycle management

This change is automatic and requires no code changes—your existing DataTables will immediately benefit from improved connection persistence.

### Customizable Chrome Footer Menu Items

Chrome settings now support a footer menu transformer that lets you customize the menu items displayed in the sidebar footer (the area at the bottom of the sidebar with theme controls, logout, and other system actions). This is useful when you want to add custom actions, reorder existing items, or filter out default items to create a tailored experience for your users.

**Basic Usage:**

```csharp
public override ChromeSettings GetChromeSettings()
{
    return new ChromeSettings()
        .UseSidebar()
        .UseFooterMenuItemsTransformer((items, navigator) =>
        {
            // Add custom items to the footer
            var customItems = new[]
            {
                MenuItem.Default("Documentation")
                    .Icon(Icons.Book)
                    .HandleSelect(() => navigator.Navigate("app://docs")),
                MenuItem.Default("Settings")
                    .Icon(Icons.Settings)
                    .HandleSelect(() => navigator.Navigate<SettingsApp>())
            };

            // Return combined menu items
            return items.Concat(customItems);
        });
}
```

**Filtering Default Items:**

You can filter or reorder the default footer items (Theme, GitHub, Logout) using the transformer:

```csharp
.UseFooterMenuItemsTransformer((items, navigator) =>
{
    // Remove GitHub link for internal apps
    return items.Where(item => item.Tag != "$github");
})
```

**Advanced Customization:**

```csharp
.UseFooterMenuItemsTransformer((items, navigator) =>
{
    var customItems = new[]
    {
        // Add a Help submenu
        MenuItem.Default("Help")
            .Icon(Icons.HelpCircle)
            | MenuItem.Default("User Guide")
                .HandleSelect(() => navigator.Navigate<UserGuideApp>())
            | MenuItem.Default("Keyboard Shortcuts")
                .HandleSelect(() => navigator.Navigate<ShortcutsApp>())
            | MenuItem.Default("About")
                .HandleSelect(() => navigator.Navigate<AboutApp>()),

        // Add a status indicator
        MenuItem.Default("System Status")
            .Icon(Icons.Activity)
            .HandleSelect(() => navigator.Navigate<StatusApp>())
    };

    return items.Concat(customItems);
})
```

The transformer receives two parameters:

- `items`: The default footer menu items (Theme, GitHub, Logout, etc.)
- `navigator`: An `INavigator` instance for programmatic navigation

**Default Item Tags:**

The framework assigns tags to default menu items for easy identification:

- `$theme` - Theme selector menu
- `$github` - GitHub repository link
- `$logout` - Logout action

Use these tags to filter, reorder, or conditionally display default items based on your application's requirements.

### DateTime Column Filtering in DataTables

DataTables now fully support filtering on DateTime columns using natural date syntax. Previously, DateTime columns would not work properly with the filter system, but now you can filter dates using ISO format strings with all standard comparison operators.

**Supported Filter Operations:**

```csharp
// Filtering DateTime columns in DataTables
public record Employee(
    int Id,
    string Name,
    DateTime HireDate,
    DateTime? LastReview
);

public override object? Build()
{
    var employees = GetEmployees().AsQueryable();

    return employees.ToDataTable()
        .Builder<Employee>()
        .Header(e => e.HireDate, "Hire Date")
        .Header(e => e.LastReview, "Last Review")
        .Build();
}
```

**Filter Syntax Examples:**

Users can now filter DateTime columns using intuitive expressions:

```
[HireDate] = "2024-05-30"
[OrderDate] >= "2024-01-01" AND [OrderDate] <= "2024-12-31"
[CreatedDate] > "2023-01-01"
[LastModified] is not blank
```

**Supported Operators:**

- Equality: `=`, `!=`
- Comparison: `>`, `<`, `>=`, `<=`
- Null checks: `is blank`, `is not blank`

The filter system automatically recognizes DateTime columns and provides appropriate examples in the filter placeholder text. Date values are handled internally as Date objects from Apache Arrow Timestamp vectors, ensuring proper type safety and performance throughout the data pipeline.

### DataTable Label Columns

DataTables now support a new "Labels" column type for displaying arrays of tags or categories as visual chips. This is perfect for showing multi-valued properties like skills, tags, categories, or any other list-based data in a clean, readable format.

When you use a `string[]` property in your DataTable model, the framework automatically recognizes it as a Labels column and displays each value as a colored chip. Users can filter by individual labels, sort by the first label alphabetically, and the labels automatically wrap to fit the column width.

**Basic Usage:**

```csharp
public record Employee(
    int Id,
    string Name,
    string[] Skills  // Automatically rendered as labels
);

public override object? Build()
{
    var employees = new[]
    {
        new Employee(1, "John Smith", new[] { "C#", "JavaScript", "Leadership" }),
        new Employee(2, "Jane Doe", new[] { "Python", "SQL", "Team Player" }),
        new Employee(3, "Mike Johnson", new[] { "React", "Agile", "Communication" })
    }.AsQueryable();

    return employees.ToDataTable()
        .Builder<Employee>()
        .Header(e => e.Skills, "Skills")
        .Width(e => e.Skills, Size.Px(300))
        .Build();
}
```

**Advanced Configuration:**

```csharp
// Configure labels column with custom settings
return employees.ToDataTable()
    .Builder<Employee>()
    .Header(e => e.Skills, "Technical Skills")
    .Width(e => e.Skills, Size.Px(400))
    .Align(e => e.Skills, Align.Left)
    .Sortable(e => e.Skills, true)  // Sorts by first label alphabetically
    .Group(e => e.Skills, "Profile")
    .Build();
```

**Filtering:**

The Labels column type supports intelligent filtering:

- **Equals filter**: Shows rows where the label array contains the specified value
- **Not equals filter**: Shows rows where the label array does not contain the specified value
- **Dropdown values**: When you click the filter dropdown, all unique labels across all rows are listed as individual options, even if they appear in different arrays

**Sorting:**

Sorting a Labels column orders rows alphabetically by the first label in each array. Rows with empty arrays or null values appear last.

The Labels column type provides a much cleaner visual representation than displaying arrays as comma-separated text, and the built-in filtering makes it easy for users to find rows with specific labels. The column automatically handles serialization between the backend and frontend, so you just need to use `string[]` properties in your models.

### DataTable Link Columns

DataTables now support clickable link columns that let users navigate to internal apps or external URLs directly from table cells. Links are activated with Ctrl+Click (or Cmd+Click on Mac), with external links opening in new tabs and internal links navigating within the application.

**Basic Usage:**

```csharp
public record Employee(
    int Id,
    string Name,
    string? WidgetLink,    // Internal app link
    string? ProfileLink    // External link
);

public override object? Build()
{
    var employees = new[]
    {
        new Employee(
            1,
            "John Smith",
            "/widgets/charts/area-chart-app",  // Relative URL for internal navigation
            "https://linkedin.com/in/johnsmith"
        ),
        new Employee(
            2,
            "Jane Doe",
            "/widgets/dataTables/dataTable-app",
            "https://linkedin.com/in/janedoe"
        )
    }.AsQueryable();

    return employees.ToDataTable()
        .Builder<Employee>()
        .Header(e => e.Name, "Name")
        .Header(e => e.WidgetLink, "Widgets")
        .Header(e => e.ProfileLink, "Profile")
        .DataTypeHint(e => e.WidgetLink, ColType.Link)
        .DataTypeHint(e => e.ProfileLink, ColType.Link)
        .Width(e => e.WidgetLink, Size.Px(200))
        .Width(e => e.ProfileLink, Size.Px(250))
        .Build();
}
```

**How It Works:**

- **Ctrl+Click or Cmd+Click**: Users activate links by holding Ctrl (Windows/Linux) or Cmd (Mac) while clicking
- **External URLs**: Links starting with `http://` or `https://` open in new tabs with proper security attributes
- **Internal URLs**: Relative URLs navigate within the application in the same tab
- **Visual Feedback**: Link cells display with hover effects to indicate they're clickable
- **Read-only**: Link cells are automatically read-only to prevent editing conflicts

**Link Cell Properties:**

Link columns automatically:

- Display the full URL as text in the cell
- Show hover effects to indicate interactivity
- Prevent cell selection (which could cause visual artifacts)
- Disable cell editing to avoid conflicts with link activation

This feature makes it easy to connect your DataTable rows to related data, documentation, external profiles, or any other resources your users need to access.

### Custom Display Content in Job Scheduler

Jobs in the JobScheduler can now display custom UI content alongside their status information. The new `.SetDisplay()` method allows you to add any widget or view content to a job, which will be shown in the job's UI representation below the job title and progress bar.

This is particularly useful for showing real-time information about what a job is doing, displaying intermediate results, or providing context-specific UI elements during long-running operations.

**Basic Usage:**

```csharp
public override object? Build()
{
    var scheduler = new JobScheduler(maxParallelJobs: 4);

    var job = new Job("Processing data", async (job, scheduler, progress, token) =>
    {
        // Show status information while the job runs
        job.SetDisplay(new Text("Analyzing files...").FontSize(12));
        await Task.Delay(1000);

        // Update display content as the job progresses
        job.SetDisplay(new Text("Generating report...").FontSize(12));
        await Task.Delay(1000);

        return true;
    });

    scheduler.ScheduleJob(job);

    return scheduler.ToJobSchedulerView();
}
```

**Advanced Usage with Live Updates:**

```csharp
var job = new Job("Batch processing", async (job, scheduler, progress, token) =>
{
    var processedCount = 0;
    var totalItems = 100;

    for (int i = 0; i < totalItems; i++)
    {
        processedCount++;

        // Show live statistics
        job.SetDisplay(
            Layout.Horizontal().Gap(2)
                | new Text($"Processed: {processedCount}/{totalItems}")
                | new Text($"Success rate: {(i + 1) * 100 / totalItems}%")
        );

        await ProcessItemAsync(i);
        progress.Report((double)processedCount / totalItems);
    }

    return true;
});
```

The display content automatically appears in the job's UI between the title/progress bar and any error messages. When using `.ToJobSchedulerView()`, the custom display integrates seamlessly with the built-in job status visualization.

### Field Widget Help Text Support

Field widgets now support inline help text displayed as tooltips, making it easier to provide contextual guidance to users without cluttering your forms. The new `.Help()` method adds an information icon next to the field label that displays your help text in a tooltip when hovered or focused.

**Using Help Text in Forms:**

```csharp
// Add help text to individual fields in a form
public class UserFormExample : ViewBase
{
    public override object? Build()
    {
        var model = UseState(new UserModel());

        return model.ToForm()
            .Label(m => m.Email, "Email Address")
            .Help(m => m.Email, "We'll never share your email with third parties")
            .Label(m => m.Password, "Password")
            .Help(m => m.Password, "Use a mix of letters, numbers, and symbols for better security")
            .Build();
    }
}
```

**Using Help Text with Individual Fields:**

```csharp
// Add help text when using .WithField()
public class InputExample : ViewBase
{
    public override object? Build()
    {
        var username = UseState("");

        return username.ToTextInput()
            .Placeholder("Enter username")
            .WithField()
            .Label("Username")
            .Description("Must be at least 8 characters long")
            .Help("Your username must be unique and contain only letters, numbers, and underscores");
    }
}
```

Help text appears as a small info icon next to the field label and is fully accessible with keyboard navigation. This complements the existing `.Description()` method, which displays text below the input field—use `.Help()` for optional contextual guidance and `.Description()` for essential field requirements.

### Improved Chart Theme Switching

Chart widgets (Area, Bar, Line, and Pie charts) now respond more smoothly to theme changes. When switching between light and dark modes, charts will update their colors and styling automatically without flickering or performance issues.

The improvements include:

- **Better performance**: Charts now use memoization to avoid unnecessary re-renders when theme changes
- **Smoother transitions**: Theme-aware colors use the framework's chromatic color system (emerald, red, teal, purple, orange, etc.) that automatically adapts for light/dark mode
- **Enhanced visual consistency**: Axis lines, split lines, and grid elements now use consistent styling with improved opacity and dashed line styles
- **Rainbow color scheme support**: The Rainbow color scheme now properly reads from CSS variables for better theme integration
- **Streamlined color palette**: The chart color system now uses the same chromatic colors as the rest of the framework, providing better visual consistency across your entire application

The default color scheme uses: emerald, red, teal, purple, orange, green, cyan, pink, amber, and indigo—all of which automatically adapt to your theme's light or dark mode. All chart widgets automatically benefit from these improvements with no code changes required on your part.

### Better Chart Y-Axis Spacing

Charts now display with more space at the top for improved readability. Previously, when charts had large value ranges, the highest data points could appear too close to the top edge of the chart area, making them harder to read and visually cramped.

The Y-axis scaling logic has been refined to provide better automatic spacing when dealing with large value spreads. For charts with significant value ranges, the framework now lets ECharts handle the maximum value calculation automatically instead of forcing a fixed maximum, which results in more balanced and visually pleasing charts with appropriate whitespace at the top.

This improvement applies automatically to all Bar, Line, and Area charts without requiring any code changes.

### Searchable AsyncSelectInput Widget

The AsyncSelectInput widget now includes a built-in search field at the top of the dropdown, making it much easier to find items in large lists. Previously, the widget would only load a filtered set of options based on the query function, but there was no interactive search capability once the list was displayed.

Now when you open an AsyncSelectInput dropdown, you'll see a search box at the top. As you type, the search automatically filters the displayed options with a 250ms throttle to avoid excessive queries. The search also shows a loading indicator while fetching results, providing clear feedback during longer queries.

**Enhanced Sample Implementation:**

The framework samples now demonstrate improved async querying with case-insensitive search, consistent ordering, and proper result deduplication:

```csharp
async Task<Option<Guid?>[]> QueryCategories(string query)
{
    await using var db = factory.CreateDbContext();
    var lowerQuery = query.ToLowerInvariant();

    return [.. (await db.Categories
            .Where(e => EF.Functions.Like(e.Name.ToLower(), $"%{lowerQuery}%"))
            .OrderBy(e => e.Name)
            .ThenBy(e => e.Id)
            .Select(e => new { e.Id, e.Name })
            .Distinct()
            .Take(50)
            .ToArrayAsync())
        .Select(e => new Option<Guid?>(e.Name, e.Id))];
}

// Use in your app
var category = UseState<Guid?>(null);
return category.ToAsyncSelectInput(QueryCategories)
    .Placeholder("Select a category");
```

The search field automatically integrates with your existing query function, so any AsyncSelectInput you've already built will automatically gain this improved search capability. The widget uses reactive throttling to ensure smooth performance even with expensive database queries.

### Chrome Wallpaper Apps

Chrome settings now support configuring a "wallpaper app" that displays when no tabs are open. This is perfect for creating welcome screens, dashboards, or ambient displays that appear when users close all tabs, providing a more polished experience than showing an empty screen.

**Basic Usage:**

```csharp
// Configure a wallpaper app that shows when no tabs are open
public override ChromeSettings GetChromeSettings()
{
    return new ChromeSettings()
        .UseSidebar()
        .WallpaperApp<WelcomeScreen>();
}

// Or use the app ID directly
public override ChromeSettings GetChromeSettings()
{
    return new ChromeSettings()
        .UseSidebar()
        .WallpaperAppId("my-welcome-app");
}
```

When users close all tabs, the wallpaper app automatically displays in the main content area. Once a user opens a new tab, the wallpaper disappears and the normal tab interface returns.

This feature works exclusively with tab-based Chrome navigation. Use it to create:

- Welcome screens with quick actions
- Dashboard views showing key metrics
- Branded landing pages
- Helpful getting-started guides

### Interactive Chart Toolbox

All chart widgets now support an interactive toolbox that provides users with powerful chart interaction capabilities. The toolbox adds a set of icons to your charts allowing users to save charts as images, view raw data, switch between chart types, and restore the chart to its original state.

**Basic Usage:**

```csharp
// Enable toolbox with default settings
var data = new[]
{
    new { Month = "Jan", Sales = 120 },
    new { Month = "Feb", Sales = 200 },
    new { Month = "Mar", Sales = 150 }
};

return new BarChart(data)
    .Bar("Sales")
    .XAxis(new XAxis("Month"))
    .Toolbox(); // Adds interactive toolbox
```

**Customize Toolbox Features:**

```csharp
// Configure which features to enable
return new LineChart(data)
    .Line("Revenue")
    .XAxis(new XAxis("Quarter"))
    .Toolbox(new Toolbox()
        .SaveAsImage(true)   // Allow saving chart as PNG
        .DataView(true)      // Show raw data table
        .MagicType(true)     // Toggle between line/bar chart
        .Restore(true)       // Reset chart to original state
    );
```

**Customize Toolbox Position:**

```csharp
// Position toolbox in different locations
return new AreaChart(data)
    .Area("Users")
    .XAxis(new XAxis("Date"))
    .Toolbox(new Toolbox()
        .Orientation(Toolbox.Orientations.Vertical)
        .Align(Toolbox.Alignments.Left)
        .VerticalAlign(Toolbox.VerticalAlignments.Top)
    );
```

**Available Toolbox Features:**

- **SaveAsImage**: Allows users to download the chart as a PNG image
- **DataView**: Displays the raw chart data in a table format that users can view and edit
- **MagicType**: Enables dynamic switching between compatible chart types (line ↔ bar)
- **Restore**: Resets the chart to its original configuration after user interactions

The toolbox automatically adapts to your application's theme and positions itself intelligently to avoid overlapping with chart elements. By default, the toolbox appears in the top-right corner with all features enabled, but you can customize both the position and which features to show.

**Note:** The MagicType feature (switching between chart types) is not supported for PieChart and AreaChart widgets, as these chart types don't have compatible alternatives. For these charts, the toolbox will automatically disable the MagicType option.

### Simplified PieChart API

The PieChart widget now has a cleaner, more intuitive API. Instead of configuring complex `Pie` objects with explicit measure and dimension names, you can now use the simplified `.Pie()` method that automatically reads from your `PieChartData` records:

**New simplified approach:**

```csharp
// Create data using PieChartData records
var data = new[]
{
    new PieChartData("United States", 333),
    new PieChartData("Sweden", 10),
    new PieChartData("China", 1412)
};

// Use the simplified Pie configuration
return new PieChart(data)
    .Pie("Measure", "Dimension")
    .Tooltip();
```

**Previous approach (still supported):**

```csharp
return new PieChart(data)
    .Pie(new Pie("Population", "Country")
        .InnerRadius(60)
        .OuterRadius(80))
    .Tooltip();
```

The new API reduces boilerplate while maintaining all the customization options you need. When you use `PieChartData` records, the chart automatically knows which property contains the measure (value) and which contains the dimension (label).

### OpenAPI Connection Support

The Ivy CLI now supports adding OpenAPI connections to your projects using the new `ivy connect openapi add` command. This feature makes it easy to integrate REST APIs into your Ivy applications by generating all the necessary scaffolding automatically.

When you add an OpenAPI connection, the CLI generates:

- A Connection class that implements `IConnection` and `IHaveSecrets` for secure credential management
- A client factory for creating API clients with automatic authentication
- Automatic service registration
- Entity discovery from API endpoints

Each connection is organized in its own directory under `Connections/[ConnectionName]/`, keeping your project structure clean and maintainable.

The CLI automatically detects the authentication scheme from your OpenAPI specification and supports both **API Key** and **Bearer Token** authentication:

```bash
# Add an OpenAPI connection to your project
ivy connect openapi add

# Or specify the OpenAPI URL directly
ivy connect openapi add https://api.example.com/openapi.json

# The CLI will automatically detect authentication and prompt you for:
# - Connection name
# - API Key or Bearer Token (based on the spec)
```

The generated connection class automatically:

- Extracts available endpoints from the generated client interface
- Registers the API client in your service collection
- Manages secrets through environment variables
- Provides context about the connection for the framework

Example of the generated connection structure:

```csharp
namespace YourApp.Connections.MyApi;

public class MyApiConnection : IConnection, IHaveSecrets
{
    public string GetName() => nameof(MyApi);

    public ConnectionEntity[] GetEntities()
    {
        // Automatically discovers all API endpoints
        var clientType = typeof(IMyApiClient);
        var methods = clientType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

        return methods
            .Select(m => new ConnectionEntity(m.Name, m.Name))
            .ToArray();
    }

    public void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IMyApiClient>(_ =>
            MyApiClientFactory.CreateClient());
    }

    public Secret[] GetSecrets()
    {
        return
        [
            new Secret("MY_API_ENDPOINT_URL"),
            new Secret("MY_API_KEY") // or MY_API_BEARER_TOKEN for bearer auth
        ];
    }
}
```

**Note:** The environment variable naming has been updated from `*_HOST_URL` to `*_ENDPOINT_URL` for better clarity. If you've previously added OpenAPI connections, you'll need to rename your environment variables accordingly.

This makes it seamless to work with external REST APIs in your Ivy applications while maintaining proper separation of concerns and security best practices.

## File Upload System Overhaul

The file upload system has been completely redesigned to provide better performance, automatic state management, and improved user experience with progress tracking and validation.

### New Upload Architecture

File uploads now use a streamlined three-component architecture:

1. **State for Files**: Holds uploaded file(s) data with full metadata
2. **UseUpload Hook**: Creates upload endpoints with validation
3. **Upload Handlers**: Automatically manage file data and progress

**Before (old API):**

```csharp
// Old way - manual handling
var files = UseState<FileInput?>(() => null);
var uploadUrl = this.UseUpload(fileBytes => {
    // Process bytes manually
}, "*/*", "file");

return files.ToFileInput(uploadUrl, "Choose a file");
```

**After (new API):**

```csharp
// New way - automatic state management
var file = UseState<FileUpload<byte[]>?>();
var upload = this.UseUpload(MemoryStreamUploadHandler.Create(file))
    .Accept("*/*")
    .MaxFileSize(10 * 1024 * 1024); // 10 MB

return file.ToFileInput(upload).Placeholder("Choose a file");
```

### FileUpload Record

The new `FileUpload<T>` record replaces the old `FileInput` record, providing richer metadata:

```csharp
public record FileUpload<TContent>
{
    public Guid Id { get; }              // Unique identifier for tracking
    public string FileName { get; }      // Original file name
    public string ContentType { get; }   // MIME type
    public long Length { get; }          // File size in bytes
    public float Progress { get; }       // Upload progress (0.0 to 1.0)
    public TContent Content { get; }     // File content (byte[] or string)
    public FileUploadStatus Status { get; } // Pending, Loading, Finished, Failed, Aborted
}
```

### Built-in Upload Handlers

**MemoryStreamUploadHandler** - For standard uploads:

```csharp
// Binary content (byte[])
var file = UseState<FileUpload<byte[]>?>();
var upload = this.UseUpload(MemoryStreamUploadHandler.Create(file));

// Text content (string)
var textFile = UseState<FileUpload<string>?>();
var upload = this.UseUpload(MemoryStreamUploadHandler.Create(textFile, Encoding.UTF8));

// Multiple files
var files = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
var upload = this.UseUpload(MemoryStreamUploadHandler.Create(files));
```

**ChunkedMemoryStreamUploadHandler** - For streaming uploads (e.g., audio recording):

```csharp
var audioFile = UseState<FileUpload<byte[]>?>();
var upload = this.UseUpload(ChunkedMemoryStreamUploadHandler.Create(audioFile));

return new AudioRecorder(upload, "Record", "Recording...")
    .ChunkInterval(2000); // Upload every 2 seconds
```

### Progress Tracking

File uploads now automatically track and display progress:

```csharp
var files = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
var upload = this.UseUpload(MemoryStreamUploadHandler.Create(files));

return Layout.Vertical()
    | files.ToFileInput(upload)
    | files.Value.ToTable()
        .Builder(e => e.Length, e => e.Func((long x) => Utils.FormatBytes(x)))
        .Builder(e => e.Progress, e => e.Func((float x) => x.ToString("P0")))
        .Remove(e => e.Id);
```

### Upload Validation with User Feedback

Validation now provides immediate user feedback via toast notifications:

```csharp
var files = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
var upload = this.UseUpload(MemoryStreamUploadHandler.Create(files))
    .Accept("image/*")                    // Only images
    .MaxFileSize(5 * 1024 * 1024)        // 5 MB per file
    .MaxFiles(3);                         // Maximum 3 files

return files.ToFileInput(upload).Placeholder("Choose up to 3 images");
```

When users exceed limits or select invalid file types, they receive clear toast notifications explaining the issue.

### Upload Cancellation

Users can now cancel in-progress uploads:

```csharp
var file = UseState<FileUpload<byte[]>?>();
var upload = this.UseUpload(MemoryStreamUploadHandler.Create(file));

return file.ToFileInput(upload)
    .HandleCancel(fileId => {
        // Custom cleanup logic when user cancels
        Console.WriteLine($"Upload {fileId} canceled");
    });
```

### Form Integration with Upload Context

File uploads in forms now use context-aware builders for proper hook access:

```csharp
public record FormModel
{
    [Required]
    public FileUpload<byte[]>? Document { get; set; }
}

public override object? Build()
{
    var model = UseState(new FormModel());

    return model.ToForm()
        .Builder(e => e.Document, (state, view) => {
            var uploadContext = view.UseUpload(MemoryStreamUploadHandler.Create(state))
                .Accept("application/pdf")
                .MaxFileSize(5 * 1024 * 1024);
            return state.ToFileInput(uploadContext);
        })
        .Label(x => x.Document, "Upload PDF Document");
}
```

**Form Upload Protection**: Forms automatically prevent submission while file uploads are in progress, displaying a toast notification if users try to submit prematurely.

### Audio Recording Enhancements

The `AudioRecorder` widget now requires an upload context and supports both streaming and single-upload modes:

```csharp
// Streaming mode - upload chunks while recording
var upload = this.UseUpload(ChunkedMemoryStreamUploadHandler.Create(audioFile));
return new AudioRecorder(upload, "Record", "Recording...")
    .ChunkInterval(1000); // Upload every 1 second

// Single upload mode - upload when stopped
var upload = this.UseUpload(MemoryStreamUploadHandler.Create(audioFile));
return new AudioRecorder(upload, "Record", "Recording...");
// No ChunkInterval = upload when recording stops
```

### Performance Improvements

- **Dedicated Event Queue**: Each session now has a dedicated event dispatch queue to handle UI updates and file uploads without consuming ThreadPool workers
- **Update Coalescing**: UI updates are automatically coalesced with a 16ms delay to reduce unnecessary re-renders during file uploads
- **Async Event Handling**: File upload events are now handled asynchronously to prevent blocking
- **Increased ThreadPool Workers**: Minimum ThreadPool workers automatically scaled based on processor count for better upload concurrency

### Breaking Changes

The file upload API has completely changed. If you're using the old `FileInput` with `UseUpload`, you'll need to migrate to the new API:

1. Replace `FileInput` records with `FileUpload<byte[]>` or `FileUpload<string>`
2. Update `UseUpload` calls to use `MemoryStreamUploadHandler.Create()`
3. Change `ToFileInput(uploadUrl)` to `ToFileInput(uploadContext)`
4. Update `AudioRecorder` to pass upload context as first parameter
5. Use `FileUpload.Length` instead of `FileInput.Size`
6. Use `FileUpload.FileName` instead of `FileInput.Name`
7. Use `FileUpload.ContentType` instead of `FileInput.Type`
8. Remove `FileInput.LastModified` references (no longer tracked)

## New Utilities

### FileSize Helper Class

A new `FileSize` utility class provides convenient methods for converting file sizes from various units to bytes. This is particularly useful when setting file upload size limits or working with file size constraints:

```csharp
// Use FileSize helpers for clearer code
var upload = this.UseUpload(MemoryStreamUploadHandler.Create(file))
    .MaxFileSize(FileSize.FromMegabytes(5));  // 5 MB limit

// Also supports other units
var smallLimit = FileSize.FromKilobytes(500);   // 500 KB
var largeLimit = FileSize.FromGigabytes(2);     // 2 GB
var hugeLimit = FileSize.FromTerabytes(1);      // 1 TB

// All methods return long values in bytes
```

This replaces manual byte calculations (e.g., `5 * 1024 * 1024`) with more readable method calls, reducing errors and improving code clarity.

### FileTypes Constants

A new `FileTypes` utility class provides constants for common MIME types, making file type validation more maintainable:

```csharp
// Use FileTypes constants instead of hardcoded strings
var upload = this.UseUpload(MemoryStreamUploadHandler.Create(file))
    .Accept(FileTypes.Pdf)  // "application/pdf"
    .MaxFileSize(FileSize.FromMegabytes(10));

// Currently available types
FileTypes.Text  // "text/plain"
FileTypes.Pdf   // "application/pdf"
```

This prevents typos in MIME type strings and provides a central location for commonly used file types. More file type constants will be added in future releases based on common usage patterns.

## Performance Improvements

### Better Async Event Handling

The framework now handles long-running async operations more efficiently, preventing UI updates from being blocked during slow event handlers. Previously, when a button click or other event took a long time to complete, the entire UI could become unresponsive because event handling was queued on the same thread as UI updates.

The AppHub now uses `Task.Run` to process UI update events, ensuring that slow operations in your event handlers don't block the rendering pipeline. This means your app stays responsive even when performing time-consuming operations like:

```csharp
public override object? Build()
{
    var loading = UseState(false);

    async ValueTask OnLongOperation()
    {
        loading.Set(true);
        // This slow operation won't block UI updates anymore
        await Task.Delay(10000);
        await ProcessLargeDataset();
        loading.Set(false);
    }

    return new Button("Start Process")
        .HandleClick(OnLongOperation)
        .Loading(loading);
}
```

The improvement is automatic and requires no code changes—your existing apps will immediately benefit from better responsiveness during async operations.

## API Changes

### Button Loading State with IState

The Button widget's `.Loading()` method now accepts an `IState<bool>` parameter directly, eliminating the need to manually extract the `.Value` property:

**New simplified approach:**

```csharp
var loading = UseState(false);
return new Button("Submit")
    .HandleClick(OnSubmit)
    .Loading(loading);  // Pass IState<bool> directly
```

**Previous approach (still works):**

```csharp
var loading = UseState(false);
return new Button("Submit")
    .HandleClick(OnSubmit)
    .Loading(loading.Value);  // Extract value manually
```

This small improvement makes button loading states more concise and consistent with other state-aware widgets in the framework.

### MenuItem Composition Operator

MenuItem now supports a convenient pipe operator (`|`) for composing parent-child menu structures. This makes it much more readable to build nested menus with multiple children:

**New approach with pipe operator:**

```csharp
var menu = MenuItem.Default("File")
    .Icon(Icons.Folder)
    | MenuItem.Default("New").HandleSelect(CreateNew)
    | MenuItem.Default("Open").HandleSelect(Open)
    | MenuItem.Default("Save").HandleSelect(Save);
```

**Previous approach:**

```csharp
var menu = MenuItem.Default("File")
    .Icon(Icons.Folder)
    .Children(
        MenuItem.Default("New").HandleSelect(CreateNew),
        MenuItem.Default("Open").HandleSelect(Open),
        MenuItem.Default("Save").HandleSelect(Save)
    );
```

The pipe operator adds each menu item as a child to the parent, making it easier to build complex menu hierarchies incrementally. This is particularly useful when building menus conditionally or when composing menu items from different sources.

### Generic Navigation Extensions

The `INavigator` interface now includes a generic `Navigate<T>()` method for type-safe navigation without needing to pass `typeof(T)`:

```csharp
// New generic method
navigator.Navigate<SettingsApp>();
navigator.Navigate<DashboardApp>(new DashboardArgs { Tab = "overview" });

// Previous approach (still works)
navigator.Navigate(typeof(SettingsApp));
navigator.Navigate(typeof(DashboardApp), new DashboardArgs { Tab = "overview" });
```

This provides better compile-time type safety and cleaner syntax when navigating to app types programmatically.

### Server Registration Extensions

The `Server` class now includes a generic `AddApp<T>()` method for registering apps without using `typeof`:

```csharp
// New generic method
server.AddApp<MyApp>(isDefault: true);
server.AddApp<SettingsApp>();

// Previous approach (still works)
server.AddApp(typeof(MyApp), isDefault: true);
server.AddApp(typeof(SettingsApp));
```

This provides a more concise and type-safe way to register applications with the server.

### Configuration Access in Server

The `Server` class now registers the `IConfiguration` instance as a singleton in the service collection, making it accessible throughout your application via dependency injection:

```csharp
public class MyApp : AppBase
{
    public override object? Build()
    {
        var config = UseService<IConfiguration>();
        var apiKey = config["ApiKey"];

        // Use configuration values in your app
        return new Text($"API Endpoint: {config["ApiEndpoint"]}");
    }
}
```

This eliminates the need to manually register `IConfiguration` in your service setup code.

### Smarter Column Removal in Tables and DataTables

The automatic column removal logic for fields starting with underscore has been refined. Previously, any field starting with `_` would be hidden. Now, the framework only hides fields that start with `_` followed by a letter (e.g., `_hidden`, `_internal`), but not fields like `_1` or `_$special`:

```csharp
public record Employee(
    int Id,
    string Name,
    string _internal,  // Hidden - starts with _ followed by letter
    int _1,            // Visible - starts with _ followed by digit
    string _$metadata  // Visible - starts with _ followed by special char
);
```

This provides more flexibility when using underscore prefixes for different purposes while maintaining the convention of hiding internal fields.

### DropDownMenu Items Type Flexibility

The `DropDownMenu.Items()` method now accepts `IEnumerable<MenuItem>` instead of requiring a `MenuItem[]` array. This makes it easier to work with LINQ queries and other enumerable sources:

```csharp
// Now works with any IEnumerable
var menu = new DropDownMenu()
    .Items(menuItems.Where(x => x.IsVisible))
    .Items(menuItems.OrderBy(x => x.Label))
    .Items(menuItems.Concat(additionalItems));

// Previously required .ToArray()
var menu = new DropDownMenu()
    .Items(menuItems.Where(x => x.IsVisible).ToArray());
```

The method automatically converts the enumerable to an array internally, eliminating unnecessary `.ToArray()` calls in your code.

### Clearer DataTable Sizing API

The DataTable API has been clarified to make it easier to understand how to set table dimensions versus column widths. The `Width()` and `Height()` methods now have improved XML documentation that explicitly states:

- **Width(Size)** - Sets the overall width of the table
- **Width(Expression, Size)** - Sets the width of a specific column
- **Height(Size)** - Sets the overall height of the table

**Example:**

```csharp
// Set table dimensions and column widths
return employees.ToDataTable()
    .Builder<Employee>()
    .Width(Size.Units(120))              // Table width: 120 units (30rem)
    .Height(Size.Units(100))             // Table height: 100 units (25rem)
    .Width(e => e.Name, Size.Px(300))    // Name column width: 300px
    .Width(e => e.Email, Size.Px(400))   // Email column width: 400px
    .Build();
```

Previously, the API documentation didn't clearly distinguish between these two uses of `Width()`, which could cause confusion when trying to size the table versus sizing individual columns. The improved documentation and updated samples now make this distinction clear.

### DataTable Configuration API Simplified

The DataTable configuration API has been simplified with more concise naming. The `DataTableConfiguration` class has been renamed to `DataTableConfig`, and the `Configuration` property on the DataTable widget has been renamed to `Config`.

**What changed:**

```csharp
// Before
var table = new DataTable(connection, width, height, columns, configuration);
var config = table.Configuration;

// After
var table = new DataTable(connection, width, height, columns, config);
var config = table.Config;
```

**Using the builder API:**

```csharp
// Before
return table.Builder<User>()
    .Config(config => {
        config.AllowSorting = true;
        config.ShowIndexColumn = true;
    })
    .Build();

// After - same method name, just updated parameter type
return table.Builder<User>()
    .Config(config => {
        config.AllowSorting = true;
        config.ShowIndexColumn = true;
    })
    .Build();
```

The builder method `.Config()` still works the same way—the change is purely internal with the type rename from `DataTableConfiguration` to `DataTableConfig`. This makes the API more concise while maintaining all functionality.

On the frontend, the TypeScript types have been updated accordingly:

- `DataTableConfiguration` interface is now `DataTableConfig`
- The `configuration` prop is now `config`

This is a breaking change if you're directly referencing the `Configuration` property or the `DataTableConfiguration` class in your code. Update your references to use `Config` and `DataTableConfig` respectively.

## Security & Dependencies

### Authentication Protection for Upload, Download, and DataTable Services

The framework now automatically protects file uploads, downloads, and DataTable operations with authentication when you've configured an `IAuthProvider`. Previously, these services could be accessed without authentication even when your app required login, potentially exposing sensitive data or functionality.

**What's Protected:**

When you configure authentication in your server, the following services now automatically require valid auth tokens:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        var server = new Server()
            .UseAuth<MyAuthProvider>()  // Enable authentication
            .AddApp<MyApp>();

        server.Start(args);
    }
}
```

With authentication enabled, these operations are now protected:

- **File uploads** via `UploadService` - Users must be authenticated to upload files
- **File downloads** via `DownloadService` - Users must be authenticated to download files
- **DataTable queries** via `DataTableService` - Users must be authenticated to query remote DataTables

**User Feedback:**

When authentication fails, users now receive clear feedback:

- **Toast notifications** appear explaining the authentication failure
- **Error messages** provide context about missing or invalid tokens
- **HTTP 401 responses** are returned for unauthorized requests

**Auth Token Handling:**

The framework includes improved auth token management:

- **Smart cookie handling**: Auth tokens that exceed browser cookie size limits (4KB) are automatically split across multiple cookies
- **URL-encoded length calculation**: Token size validation now correctly accounts for URL encoding overhead
- **Token extraction**: The new `AuthHelper` class provides utilities for extracting tokens from HTTP and gRPC contexts

**For Developers:**

If you're building custom services that need authentication, you can use the new `AuthHelper` utilities:

```csharp
// In a custom controller
public class MyController : Controller
{
    public async Task<IActionResult> ProtectedAction([FromServices] Server server, [FromServices] IServiceProvider services)
    {
        // Validate auth if required
        if (await this.ValidateAuthIfRequired(server, services) is { } errorResult)
        {
            return errorResult;
        }

        // Your protected logic here
        return Ok("Success");
    }
}
```

**New Exception Types:**

The framework now provides specific exception types for authentication errors:

- `MissingAuthTokenException` - No token provided
- `InvalidAuthTokenException` - Token is invalid or expired
- `AuthProviderNotConfiguredException` - Auth provider not set up correctly
- `AuthValidationException` - Error during token validation

These changes ensure that your authenticated apps properly protect all data access points, not just your app views. If your app doesn't use authentication (no `IAuthProvider` configured), these services continue to work without any authentication checks.

### Frontend Security Update

The frontend has received a comprehensive security update with all npm packages updated to their latest versions. This update improves the overall security posture of Ivy applications by addressing known vulnerabilities and ensuring compatibility with the latest browser security standards.

Key dependency updates include:

- Updated all Radix UI components to latest versions for improved accessibility and performance
- Updated React syntax highlighter for better code display
- Updated Mermaid to version 11.12.1 for improved diagram rendering
- Updated Lucide React icons to latest version
- Removed several obsolete packages that are no longer needed

This is a maintenance update that doesn't require any code changes on your part—your existing Ivy applications will automatically benefit from these security improvements.

## Ivy CLI Enhancements

### Advanced Model Configuration

The `ivy fix` command now supports an advanced `--model-id` option for power users who want to specify a particular AI model for debugging sessions. This hidden option provides more control over the AI model used during the fix process.

```bash
# Advanced usage - specify a custom model
ivy fix --model-id your-model-id
```

This option is primarily intended for advanced scenarios and testing different model configurations.

### Improved Login Experience

The `ivy login` command now displays your subscription details immediately after successful authentication, showing your account email, plan type, credits balance, and renewal/expiration dates. Previously, this information was only available through the `ivy version` command.

### Better Error Messages for `ivy fix`

The `ivy fix` command now validates that you're running it in a valid .NET project directory. If the directory doesn't contain a `.csproj` or `.sln` file, you'll get a clear error message instead of the command failing later in the process:

```bash
# Running ivy fix in a non-project directory
ivy fix
# Error: The current working directory does not contain a project or solution file.
```

This helps catch configuration issues early and provides clearer guidance when the command is run in the wrong location.

### Verbose Mode for `ivy fix`

The `ivy fix` command now supports a `--verbose` flag that provides more detailed output during the debugging process. This is particularly useful when troubleshooting issues or understanding what the AI agent is doing behind the scenes.

```bash
# Run ivy fix with verbose output
ivy fix --verbose
```

When verbose mode is enabled, the command disables interactive capabilities to ensure all output is captured and displayed in the console.

### Fixed Exit Codes for CI/CD Integration

The `ivy fix` command now correctly returns exit codes based on the build status after applying fixes. When using Claude Code for automated fixes, the command will:

- Return exit code `0` if the build succeeds after fixes are applied
- Return exit code `1` if the build still fails after attempted fixes

This makes `ivy fix` more reliable for CI/CD pipelines and automated workflows where exit codes are used to determine success or failure:

```bash
# Example: Using ivy fix in a CI pipeline
ivy fix --use-claude-code
if [ $? -eq 0 ]; then
  echo "Build fixed successfully"
else
  echo "Build still has errors"
  exit 1
fi
```

Previously, the command always returned exit code 1 even when fixes were successfully applied, which could cause false failures in automated scripts.

### Database Generation Automation Support

The `ivy db generate` command now supports full automation through command-line arguments and STDIN input, making it perfect for CI/CD pipelines and scripting workflows. Previously, the database generator required interactive input at each step; now you can provide everything upfront.

**New Command-Line Options:**

```bash
# Generate a database with all options specified
ivy db generate \
  --prompt "A blog database with posts, authors, and comments" \
  --provider Postgres \
  --connection-string "Host=localhost;Database=mydb" \
  --yes-to-all

# Or provide DBML directly instead of a prompt
ivy db generate \
  --dbml "Table posts { id int [pk] }" \
  --provider SqlServer \
  --yes-to-all

# Read prompt from STDIN for scripting
echo "A task management system" | ivy db generate --yes-to-all
```

**Available Options:**

- `--prompt` - Database schema description in plain English
- `--dbml` - DBML schema content (alternative to prompt)
- `--provider` - Database provider: `Sqlite`, `Postgres`, `MySql`, or `SqlServer`
- `--connection-string` - Custom database connection string
- `--yes-to-all` - Skip all interactive prompts and use defaults

**Automated Workflow:**

The command automatically advances through the generation steps when automation flags are provided:

1. If `--prompt` is provided, it auto-generates DBML from your description
2. If `--dbml` is provided, it parses the schema directly
3. With `--yes-to-all`, it skips the settings confirmation and proceeds directly to code generation

**Example CI/CD Integration:**

```bash
# In your CI pipeline
ivy db generate \
  --prompt "$(cat schema-description.txt)" \
  --provider Postgres \
  --connection-string "$DB_CONNECTION_STRING" \
  --yes-to-all
```

This makes it easy to regenerate your database layer as part of automated deployments or when schema requirements change, without manual intervention.

**Generated Database Recreation Script:**

When you generate a database using `ivy db generate`, the CLI now creates a PowerShell script named `RecreateDatabase.ps1` (previously `Run.ps1`) in your generator directory. This script is automatically configured to recreate and seed your database without any interactive prompts:

```powershell
# The generated script includes --yes-to-all for full automation
dotnet run -- --data-provider Postgres --connection-string "..." --seed-database --yes-to-all
```

You can run this script anytime to drop, recreate, and reseed your database. The improved naming makes it clearer what the script does, and the automatic `--yes-to-all` flag ensures it runs smoothly in automated scenarios without requiring manual confirmation.

### Project Initialization Automation

The `ivy init` command now supports a `--yes-to-all` flag to skip all interactive prompts and use default values, making it perfect for automated project setup in CI/CD pipelines and scripting scenarios.

```bash
# Create a new Ivy project with default settings
ivy init --yes-to-all

# Combine with other options for full automation
ivy init --yes-to-all --namespace MyApp --prerelease
```

**What happens with `--yes-to-all`:**

- **Namespace:** Automatically uses the current folder name as the namespace (or "IvyApplication" if the folder name is invalid/reserved)
- **Template:** Skips template selection and creates a basic project without applying any template

This is especially useful for creating new projects in automated environments or when you want to quickly scaffold a project with default settings:

```bash
# Example: Automated project setup in CI
mkdir my-ivy-app
cd my-ivy-app
ivy init --yes-to-all --namespace MyApp
```
