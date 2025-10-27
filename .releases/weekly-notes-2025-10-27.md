# Ivy Framework Weekly Notes - Week of 2025-10-27

## Bug Fixes

### DataTable Theme Support

Datatables now properly work with custom themes, both in dark and light modes.

### DataTable CORS setup

Datatables CORS setup has been improved, leading to impenetrable system stability

### Database Code Generator

Fixed an issue in the database connection code generator where ambiguous type references could cause compilation errors in generated code. All `System.*` types are now fully qualified in the generated connection files.

### Cross-Platform MCP Installation

Fixed an issue where the `ivy mcp install` command would fail on Mac and Linux systems.

## Improvements

### Form, Card and Table Size Variants

Forms, Cards and Table now support having their `Size` set which mostly effect visual gaps and font sizes for corresponding components.

### DataTable Column Header Icons

DataTables now support visual icons in column headers using the `.Icon()` method

### DataTable Performance Configuration for Large Datasets

DataTables now support configurable data loading strategies to optimize performance for datasets of any size, including millions of rows. You can control how data is fetched and rendered using two new methods `.LoadAllRows()` and `BatchSize()`:

```csharp
// Load all rows at once for maximum performance with very large datasets
var largeDataset = Enumerable.Range(1, 1_000_000)
    .Select(i => new { Id = i, Value = $"Row {i}" })
    .AsQueryable()
    .ToDataTable()
    .Header(x => x.Id, "ID")
    .Header(x => x.Value, "Value")
    .LoadAllRows();  // Fetch all 1 million rows in one request

// Or customize batch size for incremental loading
var incrementalDataset = products.ToDataTable()
    .Header(p => p.Name, "Product")
    .Header(p => p.Price, "Price")
    .BatchSize(50);  // Load 50 rows at a time as user scrolls
```

### Performance and Stability Improvements

The framework's frontend has received significant performance optimizations, particularly around state management and rendering. These improvements reduce unnecessary re-renders and fix React state update violations, resulting in a smoother and more responsive user interface across all widgets, especially in forms, data tables, and interactive components like the sidebar navigation.

### Chart Widgets Migrated to Apache ECharts

The chart widgets (`BarChart`, `LineChart`, `AreaChart`, and `PieChart`) have been upgraded to use Apache ECharts instead of the previous charting library. This migration brings several improvements:

- **Better Animation Performance**: Charts now animate smoothly without flickering, especially in dark mode
- **Smoother Rendering**: Charts have been optimized to eliminate visual stuttering and jank during rendering and resizing
- **Enhanced Dark Mode**: Charts properly render in dark mode with correct colors and gradients
- **Real-Time Theme Switching**: Charts now dynamically respond to theme changes in your application, automatically updating all text colors, borders, tooltips, and backgrounds when you switch between light and dark modes
- **Improved Layout and Spacing**: Charts now use flexbox layouts that adapt better to different container sizes and properly allocate space for legends and labels. Pie charts have been refined with better vertical centering when displaying totals or legends, and the center value display has been removed for a cleaner appearance

### Supabase Legacy JWT Secret Support

The Supabase authentication provider now supports optional legacy JWT secrets. When configuring Supabase authentication with `ivy auth add`, you'll be prompted to provide a legacy JWT secret in addition to your URL and API key.

The legacy JWT secret can be provided through:

- The interactive prompt when running `ivy auth add`
- Connection string format: `Supabase:Url=...;Supabase:ApiKey=...;Supabase:LegacyJwtSecret=...`

If you don't need legacy JWT support, you can simply leave this field empty and continue using only the URL and API key as before.

### New App Command Aliases

Creating apps is now more intuitive with additional command aliases:

```bash
ivy app create MyApp
ivy app new MyApp
ivy app add MyApp
ivy app generate MyApp
```

### Silent Mode for CLI Commands

The `ivy app create` and `ivy fix` commands now support a `--silent` flag that suppresses audio feedback and output when you need quieter operations. This is particularly useful when creating multiple apps or running commands in automated workflows:

```bash
ivy app create MyApp --silent
ivy fix --silent
```

Additionally, the `ivy app create` command now supports a `--skip-debug` option that skips the automatic debugging step when creating multiple apps from entities.

## New Features

### Buttons with URLs

Buttons can now act as proper hyperlinks by providing a URL. When a button has a URL, clicking it will navigate to that URL in a new tab, and the button will support standard browser link actions like "Copy Link" and "Open in New Tab" (via right-click context menu).

**Usage:**

```csharp
// Simple link button
return new Button("Visit Ivy Docs", variant: ButtonVariant.Primary)
    .Url("https://github.com/Ivy-Interactive/Ivy-Framework");

// With icon
return new Button("External Link", variant: ButtonVariant.Secondary)
    .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
    .Icon(Icons.ExternalLink, Align.Right);

// Link style button
return new Button("Documentation", variant: ButtonVariant.Link)
    .Url("https://docs.example.com");
```

When a button has a URL configured:

- Clicking it navigates to the URL in a new tab
- Right-clicking provides standard browser link actions
- The button is rendered as a proper anchor (`<a>`) element

### Kanban Board Widget

A powerful new Kanban widget has been added to the framework, allowing you to visualize and manage data in a drag-and-drop board interface. The Kanban widget automatically groups your data into columns and displays items as draggable cards with full support for reordering, moving between columns, adding new items, and deleting cards.

**Basic Usage:**

```csharp
public record Task(string Status, string Id, string Title, string Description, int Priority)

var tasks = UseState<Task[]>(...);

return tasks.Value
    .ToKanban(
        groupBySelector: e => e.Status,
        idSelector: e => e.Id,
        titleSelector: e => e.Title,
        descriptionSelector: e => e.Description,
        orderSelector: e => e.Priority);
```

The Kanban widget supports events for user interactions:

```csharp
return tasks.Value
    .ToKanban(...)
    .HandleAdd(columnKey => {
        // Add a new task to the column
        var newTask = new Task { Status = columnKey, ... };
        tasks.Set(tasks.Value.Append(newTask).ToArray());
    })
    .HandleMove(moveData => {
        // Update task when moved
        var (cardId, fromColumn, toColumn, targetIndex) = moveData;
        // Update your data based on the move
    })
    .HandleDelete(cardId => {
        // Remove the task
        tasks.Set(tasks.Value.Where(t => t.Id != cardId).ToArray());
    });
```

### New Ivy.Abstractions Package

A new `Ivy.Abstractions` package has been introduced, providing core service interfaces that you can implement for common infrastructure patterns in your applications. This package includes:

**IBlobStorage** - A unified interface for blob/file storage operations:

```csharp
// Upload a file to blob storage
await blobStorage.UploadAsync("my-container", "file.txt", fileStream, "text/plain");

// Download a file
var stream = await blobStorage.DownloadAsync("my-container", "file.txt");

// List all blobs in a container
var blobs = await blobStorage.ListBlobsAsync("my-container", prefix: "documents/");

// Manage containers
await blobStorage.CreateContainerAsync("new-container");
var exists = await blobStorage.ContainerExistsAsync("my-container");
```

**IVolume** - Path management for persistent storage volumes:

```csharp
// Get absolute paths for file operations
var filePath = volume.GetAbsolutePath("uploads", "file.txt");
```

**IHaveSecrets** - Define services that require secret configuration:

```csharp
public class MyService : IHaveSecrets
{
    public Secret[] GetSecrets() => new[]
    {
        new Secret("ApiKey"),
        new Secret("DatabasePassword")
    };
}
```

**IDescribableService** - Export service configuration as YAML:

```csharp
public class MyService : IDescribableService
{
    public string ToYaml() => "..."; // Return service configuration
}
```

These abstractions make it easier to build portable applications with standardized interfaces for common infrastructure needs. You can install the package via NuGet:

```bash
dotnet add package Ivy.Abstractions
```

### AI-Powered DataTable Filtering

DataTables now support natural language filtering powered by AI. When you have an `IChatClient` registered in your dependency injection container (such as from Microsoft.Extensions.AI with OpenAI, Azure OpenAI, or other LLM providers), your DataTables automatically gain the ability to process natural language filter queries, for example:

- "Show me all users who registered last month"
- "Find products priced between $50 and $100"
- "Display orders from the last week that are still pending"

**How to enable it:**

First, register an `IChatClient` in your services:

```csharp
// Register your AI chat client (example with OpenAI)
server.Services.AddSingleton<IChatClient>(sp =>
    new OpenAIClient(apiKey).GetChatClient("gpt-4o").AsIChatClient()
);
```

Then enable AI filtering in your DataTable using the `Config` method:

```csharp
public override object? Build()
{
    var users = context.Users.AsQueryable();
    return users.ToDataTable()
        .Config(config => config.AllowLlmFiltering = true);
}
```

### Improved Authentication System Reliability

The authentication system has been significantly improved with better token management and automatic refresh capabilities. Key enhancements include:

- **Smart Token Refresh**: Authentication tokens are now validated and refreshed on a calculated schedule rather than on every user interaction, reducing overhead and improving performance
- **Better Session Management**: The framework now proactively monitors token expiration and automatically refreshes tokens before they expire, ensuring uninterrupted user sessions
- **Enhanced Timeout Handling**: All authentication operations now include proper timeout handling (30-second default) to prevent hanging operations
- **Improved Token Security**: Tokens are now validated more thoroughly using proper JWT verification with signing keys from providers' JWKS endpoints

**Cookie Name Changes:**

Authentication cookies have been renamed for clarity:

- `jwt` → `auth_token`
- `jwt_ext_refresh_token` → `auth_ext_refresh_token`

**API Changes:**

Several authentication API methods have been updated. If you're directly calling `IAuthProvider` methods, note these changes:

```csharp
// Old method names
await authProvider.ValidateJwtAsync(token);
await authProvider.RefreshJwtAsync(token);
var user = await authProvider.GetUserInfoAsync(token);

// New method names (all now require CancellationToken)
await authProvider.ValidateAccessTokenAsync(token, cancellationToken);
await authProvider.RefreshAccessTokenAsync(token, cancellationToken);
var user = await authProvider.GetUserInfoAsync(token, cancellationToken);
```

The `AuthToken` record has been simplified - the `ExpiresAt` property is now calculated dynamically when needed rather than stored. Additionally, the `Jwt` property has been renamed to `AccessToken` for clarity.

If you're using `IAuthService` (the recommended way to work with authentication in Ivy apps), the API remains largely unchanged, though all methods now support optional cancellation tokens.
