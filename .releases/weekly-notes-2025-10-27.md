# Ivy Framework Weekly Notes - Week of 2025-10-27

## Bug Fixes

### DataTable Theme Support
Fixed an issue where the DataTable widget wasn't responding to theme changes in your application. The DataTable now properly respects your app's theme settings (light, dark, or system preference) and updates its colors accordingly. The widget now uses the theme provider context to dynamically apply the correct color scheme, including background colors, text colors, borders, and hover states.

This fix ensures that DataTables seamlessly integrate with your app's appearance, automatically switching between light and dark modes when your theme changes. Additionally, DataTable column header icons now render properly in both light and dark modes with appropriate colors and theming.

### Database Code Generator
Fixed an issue in the database connection code generator where ambiguous type references could cause compilation errors in generated code. All `System.*` types are now fully qualified in the generated connection files, making the generated code more robust and preventing potential naming conflicts.

This improvement ensures that when you use the Ivy CLI to generate database connections, the resulting code will compile reliably without ambiguous reference errors.

### Cross-Platform MCP Installation
Fixed an issue where the `ivy mcp install` command would fail on Mac and Linux systems. The CLI now correctly uses the platform-appropriate command (`which` on Mac/Linux, `where` on Windows) when locating executables in your system PATH. This ensures MCP server installation works reliably across all supported platforms.

## Improvements

### Form Size Variants

Forms now support three size variants to give you better control over the visual density and spacing in your forms. You can set forms to Small, Medium (default), or Large sizes using fluent API methods:

```csharp
// Small form - compact sizing for dense layouts
return userModel.ToForm()
    .Small()
    .Builder(m => m.Description, s => s.ToTextAreaInput())
    .Builder(m => m.Password, s => s.ToPasswordInput());

// Medium form - default size, balanced layout
return userModel.ToForm()
    .Medium()
    .Builder(m => m.Description, s => s.ToTextAreaInput())
    .Builder(m => m.Password, s => s.ToPasswordInput());

// Large form - spacious layout for prominent forms
return userModel.ToForm()
    .Large()
    .Builder(m => m.Description, s => s.ToTextAreaInput())
    .Builder(m => m.Password, s => s.ToPasswordInput());
```

The size variants affect:
- **Field spacing**: Small forms use 4px gaps, Medium uses 6px, Large uses 8px between fields
- **Label sizing**: Labels scale from text-xs (small) to text-sm (medium) to text-base (large)
- **Description text**: Scales proportionally with the form size
- **Input fields**: All input controls automatically adopt the form's size
- **Submit buttons**: Form buttons automatically match the form size

This is particularly useful when creating dialogs with compact forms, dashboard settings panels, or prominent registration forms where you want to control the visual hierarchy and information density.

### Card Size Variants

Cards now support three size variants to give you more control over the visual hierarchy and information density in your layouts. You can set cards to Small, Medium (default), or Large sizes using fluent API methods:

```csharp
// Small card - compact sizing, perfect for metric summaries
return new Card("Compact content")
    .Title("Small Card")
    .Description("Less padding, smaller text")
    .Icon(Icons.Info)
    .Small();

// Medium card - default size, balanced layout
return new Card("Standard content")
    .Title("Medium Card")
    .Description("Default padding and text sizing")
    .Icon(Icons.Info)
    .Medium();

// Large card - spacious layout for detailed content
return new Card("Detailed content")
    .Title("Large Card")
    .Description("More padding, larger text")
    .Icon(Icons.Info)
    .Large();
```

The size variants affect:
- **Padding**: Small cards use 12px, Medium uses 24px, Large uses 32px
- **Text sizing**: Content text scales appropriately for each size
- **Icon dimensions**: Icons automatically scale to match the card size
- **Spacing**: Gaps between title, description, and icon adjust proportionally

This is particularly useful when creating dashboards with metric cards, where you can use different sizes to emphasize important metrics or create visual groupings.

### Table Size Variants

Tables now support three size variants to give you better control over the visual density and spacing in your tables. You can set tables to Small, Medium (default), or Large sizes using fluent API methods:

```csharp
// Small table - compact sizing for dense data displays
return products
    .ToTable()
    .Small()
    .Builder(e => e.Name, e => e.Name)
    .Builder(e => e.Price, e => e.Price);

// Medium table - default size, balanced layout
return products
    .ToTable()
    .Medium()
    .Builder(e => e.Name, e => e.Name)
    .Builder(e => e.Price, e => e.Price);

// Large table - spacious layout for prominent tables
return products
    .ToTable()
    .Large()
    .Builder(e => e.Name, e => e.Name)
    .Builder(e => e.Price, e => e.Price);
```

The size variants affect:
- **Cell padding**: Small tables use 4px, Medium uses 8px, Large uses 12px
- **Header height**: Headers scale from 32px (small) to 40px (medium) to 48px (large)
- **Text sizing**: Content text scales from text-xs (small) to text-sm (medium) to text-base (large)
- **All table cells**: Text sizing automatically inherits throughout the table structure

This is particularly useful when creating data-dense dashboards where you want to fit more information in a compact space, or when you want to emphasize important tables with larger, more readable text.

### DataTable Column Header Icons

DataTables now support visual icons in column headers to help users quickly identify the type of data in each column. You can add icons to your columns using the `.Icon()` method with predefined icon constants from the `DataTableIcons` class.

**Available Icons:**

```csharp
// Add icons to column headers
return users.ToDataTable()
    .Icon(u => u.Name, DataTableIcons.User)
    .Icon(u => u.Email, DataTableIcons.Mail)
    .Icon(u => u.Age, DataTableIcons.Hash)
    .Icon(u => u.CreatedAt, DataTableIcons.Calendar)
    .Icon(u => u.LastLogin, DataTableIcons.Clock)
    .Icon(u => u.Status, DataTableIcons.Activity)
    .Icon(u => u.Priority, DataTableIcons.Flag)
    .Icon(u => u.IsActive, DataTableIcons.Zap);
```

**Standard Icons:**
- `DataTableIcons.User` - Person silhouette
- `DataTableIcons.Mail` - Email envelope
- `DataTableIcons.Hash` - Number symbol (#)
- `DataTableIcons.Calendar` - Date picker
- `DataTableIcons.Clock` - Time indicator
- `DataTableIcons.Activity` - Line chart/heartbeat
- `DataTableIcons.Flag` - Priority marker
- `DataTableIcons.Zap` - Lightning bolt
- `DataTableIcons.Info` - Information circle
- `DataTableIcons.ChevronUp` / `DataTableIcons.ChevronDown` - Arrow indicators
- `DataTableIcons.Filter` - Data filtering funnel
- `DataTableIcons.Search` - Magnifying glass
- `DataTableIcons.Settings` - Configuration gear
- `DataTableIcons.MoreVertical` - Three-dot menu
- `DataTableIcons.HelpCircle` - Question mark

Icons automatically adapt to your application's theme, rendering with proper colors in both light and dark modes. When no icon is specified, columns display default type-based icons (numbers, text, dates, etc.).

## More Improvements

### Simplified TextInput Labeling with .WithField()

TextInput widgets now have a more streamlined way to add labels using the `.WithField().Label()` pattern. Instead of manually creating horizontal layouts with separate text blocks, you can now add labels directly to your text inputs:

**New simplified approach:**

```csharp
return new TextInput(password)
    .Placeholder("Password")
    .Variant(TextInputs.Password)
    .WithField()
    .Label("Enter Password");
```

**Old approach (still works but not recommended):**

```csharp
return Layout.Horizontal()
    | Text.Block("Enter Password")
    | new TextInput(password)
        .Placeholder("Password")
        .Variant(TextInputs.Password);
```

This pattern works with all TextInput variants including password, email, tel, URL, search, and textarea inputs. The `.WithField()` method provides consistent styling and layout for your form fields, making your code cleaner and more maintainable. All the extension methods like `.ToTextInput()`, `.ToPasswordInput()`, `.ToEmailInput()`, etc. also support this pattern.

### DataTable Production Deployment Improvements
DataTables now use a simplified and more reliable backend URL resolution strategy. The widget consistently uses the framework's `getIvyHost()` function to determine the correct backend URL, eliminating inconsistencies in URL construction. This fix resolves issues with malformed URLs in DataTable operations and ensures DataTables work reliably across all deployment environments - development, production, containerized deployments, and behind load balancers or reverse proxies.

### DataTable Performance Configuration for Large Datasets

DataTables now support configurable data loading strategies to optimize performance for datasets of any size, including millions of rows. You can control how data is fetched and rendered using two new methods:

```csharp
// Load all rows at once for maximum performance with very large datasets
var largeDataset = Enumerable.Range(1, 1_000_000)
    .Select(i => new { Id = i, Value = $"Row {i}" })
    .AsQueryable()
    .ToDataTable()
    .Header(x => x.Id, "ID")
    .Header(x => x.Value, "Value")
    .LoadAllRows(true);  // Fetch all 1 million rows in one request

// Or customize batch size for incremental loading
var incrementalDataset = products.ToDataTable()
    .Header(p => p.Name, "Product")
    .Header(p => p.Price, "Price")
    .BatchSize(50);  // Load 50 rows at a time as user scrolls
```

**Configuration options:**

- **LoadAllRows(true)** - Fetches all rows in a single request, providing optimal performance when you need immediate access to the entire dataset. Tested and verified with datasets containing 1 million+ rows.
- **BatchSize(n)** - Loads data progressively in batches of n rows as the user scrolls. The default batch size is 20 rows. Use larger batch sizes (100-1000) for better performance with large datasets, or smaller batch sizes for faster initial page rendering.

When `LoadAllRows(true)` is enabled, infinite scroll is disabled and all data is loaded upfront. When using `BatchSize(n)`, the table loads additional batches automatically as you scroll down, keeping initial page load fast while still supporting very large datasets.

### Performance and Stability Improvements
The framework's frontend has received significant performance optimizations, particularly around state management and rendering. These improvements reduce unnecessary re-renders and fix React state update violations, resulting in a smoother and more responsive user interface across all widgets, especially in forms, data tables, and interactive components like the sidebar navigation.

### Chart Widgets Migrated to Apache ECharts
The chart widgets (BarChart, LineChart, AreaChart, and PieChart) have been upgraded to use Apache ECharts instead of the previous charting library. This migration brings several improvements:

- **Better Animation Performance**: Charts now animate smoothly without flickering, especially in dark mode
- **Smoother Rendering**: Charts have been optimized to eliminate visual stuttering and jank during rendering and resizing
- **Improved Color Support**: Charts now use standard hex color codes instead of OKLCH, ensuring consistent rendering across all themes
- **Enhanced Dark Mode**: Charts properly render in dark mode with correct colors and gradients
- **Real-Time Theme Switching**: Charts now dynamically respond to theme changes in your application, automatically updating all text colors, borders, tooltips, and backgrounds when you switch between light and dark modes
- **Better Visual Integration**: All chart elements including axis labels, legends, and tooltips now properly respect your application's theme colors for seamless visual consistency
- **Improved Layout and Spacing**: Charts now use flexbox layouts that adapt better to different container sizes and properly allocate space for legends and labels. Pie charts have been refined with better vertical centering when displaying totals or legends, and the center value display has been removed for a cleaner appearance

The API for creating charts remains the same, so your existing chart code will continue to work without changes. The visual improvements and better performance are applied automatically.

### Code Widget Copy Button Enhancement
The Code widget's copy button has been redesigned for better visibility and usability. The button now features improved styling with:
- Better contrast and visibility when hovering over code blocks
- A cleaner appearance with the copied state now using primary colors for clear feedback
- Improved positioning with proper z-index to ensure the button is always clickable
- Additional padding to prevent text from being obscured by the button

The copy button now provides clearer visual feedback when you successfully copy code to your clipboard, making it easier to work with code examples in your applications.

### Supabase Legacy JWT Secret Support
The Supabase authentication provider now supports optional legacy JWT secrets. When configuring Supabase authentication with `ivy auth add`, you'll be prompted to provide a legacy JWT secret in addition to your URL and API key. This is useful if you're working with older Supabase projects that require legacy JWT authentication.

The legacy JWT secret can be provided through:
- The interactive prompt when running `ivy auth add`
- Connection string format: `Supabase:Url=...;Supabase:ApiKey=...;Supabase:LegacyJwtSecret=...`

If you don't need legacy JWT support, you can simply leave this field empty and continue using only the URL and API key as before.

### Version Header in HTTP Responses
The Ivy Framework now includes an `ivy-version` header in HTTP responses from the root endpoint (`/`). This allows you to programmatically check which version of the framework your application is running, which can be helpful for debugging, monitoring deployments, or ensuring the correct version is deployed in production.

You can inspect this header using browser developer tools, curl, or any HTTP client to verify your application's framework version.

### Automatic Global Using Statement Sorting
The Ivy CLI now automatically sorts global using statements alphabetically when adding new ones to your files. This ensures consistent code organization and improved readability across your project. Both existing and newly added global using statements will be kept in alphabetical order at the top of your files.

### Bulk App Removal
You can now remove all Ivy apps at once when using the interactive `ivy app remove` command. When prompted to select an app to remove, a new `<All>` option appears at the top of the list, allowing you to clean up all apps in your project with a single command.

### New App Command Aliases
Creating apps is now more intuitive with additional command aliases. You can use any of these commands to create a new app:

```bash
ivy app create MyApp
ivy app new MyApp
ivy app add MyApp
ivy app generate MyApp
```

All four commands work identically, so you can use whichever feels most natural to you.

### Statistics for App Operations
The `ivy app create` and `ivy fix` commands now display statistics after completing their operations, giving you insight into token usage and operation metrics during AI-assisted app generation and debugging.

### Silent Mode for CLI Commands
The `ivy app create` and `ivy fix` commands now support a `--silent` flag that suppresses audio feedback and output when you need quieter operations. This is particularly useful when creating multiple apps or running commands in automated workflows:

```bash
ivy app create MyApp --silent
ivy fix --silent
```

Additionally, the `ivy app create` command now supports a `--skip-debug` option that skips the automatic debugging step when creating multiple apps from entities, giving you more control over the app creation workflow.

## New Features

### Buttons with URLs

Buttons can now act as proper hyperlinks by providing a URL. When a button has a URL, clicking it will navigate to that URL in a new tab, and the button will support standard browser link actions like "Copy Link" and "Open in New Tab" (via right-click context menu).

This provides a better user experience than programmatic navigation, as users get all the standard browser link behaviors they expect.

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
- The `OnClick` event handler is not triggered
- The button is rendered as a proper anchor (`<a>`) element

This is particularly useful for documentation links, external resources, or any scenario where you want to provide users with proper link semantics and browser navigation features.

### Kanban Board Widget

A powerful new Kanban widget has been added to the framework, allowing you to visualize and manage data in a drag-and-drop board interface. The Kanban widget automatically groups your data into columns and displays items as draggable cards with full support for reordering, moving between columns, adding new items, and deleting cards.

Comprehensive documentation has been added for the Kanban widget, including complete examples of project management boards, simple status trackers, and detailed explanations of all features and configuration options.

**Basic Usage:**

```csharp
public class Task
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }
    public int Priority { get; set; }
    public string Description { get; set; }
}

public override object? Build()
{
    var tasks = UseState<Task[]>(...);

    return tasks.Value
        .ToKanban(
            groupBySelector: e => e.Status,
            idSelector: e => e.Id,
            titleSelector: e => e.Title,
            descriptionSelector: e => e.Description,
            orderSelector: e => e.Priority);
}
```

**Advanced Features:**

```csharp
// Customize column titles and ordering
return tasks.Value
    .ToKanban(
        groupBySelector: e => e.Status,
        idSelector: e => e.Id,
        titleSelector: e => e.Title,
        descriptionSelector: e => e.Description)
    .ColumnTitle(status => status switch
    {
        "Todo" => "To Do",
        "InProgress" => "In Progress",
        "Done" => "Completed",
        _ => status
    })
    .ColumnOrder(e => e.Status)
    .Height(Size.Units(200));
```

**Event Handlers:**

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

The Kanban widget provides:
- **Drag-and-Drop**: Intuitive card reordering within and between columns with precise positioning - drop cards exactly where you want them
- **Custom Rendering**: Use `.CardBuilder()` to render custom card content
- **Flexible Sizing**: Kanban boards now default to full width and height for better layout integration
- **Column Customization**: Control column titles, ordering, and appearance
- **Event Handling**: Respond to add, move, and delete operations with accurate target index information
- **Empty State**: Display custom content when the board is empty with `.Empty()`
- **Independent Priority Management**: The `orderSelector` now controls visual priority badges without forcing automatic card sorting, giving you full control over card ordering through drag-and-drop

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
DataTables now support natural language filtering powered by AI! When you have an `IChatClient` registered in your dependency injection container (such as from Microsoft.Extensions.AI with OpenAI, Azure OpenAI, or other LLM providers), your DataTables automatically gain the ability to process natural language filter queries.

This means users can filter data using plain English instead of complex filter syntax. For example:
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

When enabled, the AI analyzes your filter expression and converts it into proper filter syntax that works with your data source. Users can type natural language queries in the filter box, press Enter, and the AI will translate them into structured filters.

### Improved Authentication System Reliability

The authentication system has been significantly improved with better token management and automatic refresh capabilities. Key enhancements include:

- **Smart Token Refresh**: Authentication tokens are now validated and refreshed on a calculated schedule rather than on every user interaction, reducing overhead and improving performance
- **Better Session Management**: The framework now proactively monitors token expiration and automatically refreshes tokens before they expire, ensuring uninterrupted user sessions
- **Enhanced Timeout Handling**: All authentication operations now include proper timeout handling (30-second default) to prevent hanging operations
- **Improved Token Security**: Tokens are now validated more thoroughly using proper JWT verification with signing keys from providers' JWKS endpoints

**For Supabase Users:**

The Supabase authentication provider now validates tokens using the provider's JWKS endpoint for enhanced security. Tokens signed with legacy JWT secrets (HS256) are still supported for backward compatibility.

**Cookie Name Changes:**

Authentication cookies have been renamed for clarity:
- `jwt` → `auth_token`
- `jwt_ext_refresh_token` → `auth_ext_refresh_token`

These changes are handled automatically by the framework - no action required on your part.

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

## Documentation

### Event Handlers Guide

A comprehensive new documentation guide has been added for event handlers in Ivy, with a deep focus on the `HandleBlur` event handler. The guide covers how to respond to user interactions with input widgets and includes practical patterns for common scenarios.

**Key concepts covered:**

The `HandleBlur` event handler is triggered when an input widget loses focus. It's available on all input widgets that implement the `IAnyInput` interface, including TextInput, NumberInput, SelectInput, AsyncSelectInput, BoolInput, DateTimeInput, DateRangeInput, FileInput, ColorInput, CodeInput, FeedbackInput, and ReadOnlyInput.

**Common patterns with examples:**

```csharp
// Validation pattern - validate when user finishes editing
var email = UseState("");
var error = UseState(() => (string?)null);

return email.ToTextInput()
    .Placeholder("your.email@example.com")
    .HandleBlur(() =>
    {
        error.Set(string.IsNullOrWhiteSpace(email.Value) ? "Required"
            : !email.Value.Contains("@") ? "Invalid email"
            : null);
    })
    .Invalid(error.Value);

// Auto-save pattern - save changes when focus is lost
return title.ToTextInput()
    .Placeholder("Document title")
    .HandleBlur(async () =>
    {
        await SaveToDatabase();
        lastSaved.Set(DateTime.Now);
        client.Toast("Saved!");
    });

// Formatting pattern - format input when user finishes
return phoneNumber.ToTextInput()
    .Placeholder("Enter 10-digit phone")
    .HandleBlur(() =>
    {
        var digits = new string(phoneNumber.Value.Where(char.IsDigit).ToArray());
        if (digits.Length == 10)
            phoneNumber.Set($"({digits.Substring(0, 3)}) {digits.Substring(3, 3)}-{digits.Substring(6, 4)}");
    });

// Async operations - like API validation
return username.ToTextInput()
    .HandleBlur(async () =>
    {
        if (string.IsNullOrWhiteSpace(username.Value)) return;

        message.Set("Checking...");
        var isAvailable = await CheckUsernameAvailability(username.Value);
        message.Set(isAvailable ? "Available" : "Taken");
    });
```

The documentation includes working examples for validation, auto-saving, formatting, and async operations, making it easy to implement these common patterns in your applications.

### UseDefaultApp() for Single-Purpose Applications

The Program.md documentation now includes guidance on when to use `UseDefaultApp()` as an alternative to `UseChrome()`. For single-purpose applications, embedded views, or minimal interfaces where sidebar navigation isn't needed, you can use:

```csharp
server.UseDefaultApp(typeof(AppName));
```

This approach bypasses the Chrome UI entirely, displaying only your app without tabs or sidebar navigation. This is particularly useful for:
- Single-page applications focused on one specific task
- Embedded Ivy applications within other systems
- Minimal UIs where the full Chrome navigation would be unnecessary overhead

Use `UseChrome()` when you want the full navigation experience with sidebar, tabs, and multiple apps. Use `UseDefaultApp()` when you want to display a single app directly without any navigation chrome.

### UseNavigation Hook Documentation

New comprehensive documentation has been added for the `UseNavigation()` hook, which provides a powerful way to navigate between apps and external URLs in Ivy applications. The documentation covers:

**Navigation Patterns:**

```csharp
[App(icon: Icons.Navigation)]
public class MyNavigationApp : ViewBase
{
    public override object? Build()
    {
        var navigator = this.UseNavigation();

        // Type-safe navigation to other apps
        return new Button("Navigate to Another App")
            .HandleClick(() => navigator.Navigate(typeof(AnotherApp)));
    }
}
```

**Navigation with Arguments:**

```csharp
public record UserProfileArgs(int UserId, string Tab = "overview");

// Navigate with strongly-typed arguments
navigator.Navigate(typeof(UserProfileApp), new UserProfileArgs(123, "details"));

// Receive arguments in target app
public class UserProfileApp : ViewBase
{
    public override object? Build()
    {
        var args = UseArgs<UserProfileArgs>();
        return Text.Heading($"User Profile: {args?.UserId}");
    }
}
```

**External URL Navigation:**

```csharp
// Open external websites
navigator.Navigate("https://docs.ivy-framework.com");
navigator.Navigate("mailto:support@example.com");
```

The documentation includes:
- Overview of navigation concepts and the `INavigator` interface
- Type-safe navigation patterns for compile-time safety
- URI-based navigation for dynamic scenarios
- External URL navigation examples
- Navigation with strongly-typed arguments
- Integration with Chrome settings (tabs mode, pages mode, prevent duplicates)
- Master-detail navigation patterns
- Conditional navigation based on user permissions
- Performance best practices using `UseCallback`
- Troubleshooting common navigation issues

The documentation provides a complete guide to building navigation experiences in Ivy applications with deep linking capabilities and type-safe routing between apps.

### Enhanced Form Validation Examples
The form validation documentation has been updated with clearer, more comprehensive examples showing how to use both standard .NET DataAnnotations and custom validation logic together. The new examples demonstrate:

- Using DataAnnotations like `[Required]`, `[MinLength]`, `[EmailAddress]`, and `[Range]` for standard validation
- Adding custom `.Validate()` calls for business-specific validation rules
- A complete list of supported DataAnnotations attributes
- How validation errors appear when forms are submitted

**Example combining DataAnnotations with custom validation:**

```csharp
public class UserModel
{
    [Required, MinLength(3)]
    public string Username { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(8)]
    public string Password { get; set; } = "";

    [Range(13, 120)]
    public int Age { get; set; } = 18;
}

public override object? Build()
{
    var user = UseState(() => new UserModel());

    return user.ToForm("Create Account")
        // Custom validation for business rules
        .Validate<DateTime>(m => m.BirthDate, birthDate =>
            (birthDate <= DateTime.Now, "Birth date cannot be in the future"))
        .Validate<string>(m => m.Username, username =>
            (!username.Contains(' '), "Username cannot contain spaces"));
}
```

The updated documentation also notes that fields ending with "Email" automatically receive email validation, even without the `[EmailAddress]` attribute.

### AI Integration with OpenAI and IChatClient
The Lucide Icon Agent demo has been updated to showcase the recommended approach for integrating AI capabilities in Ivy apps using `IChatClient` from Microsoft.Extensions.AI with OpenAI. This demonstrates how to move from legacy Semantic Kernel implementations to the modern, standardized AI abstraction.

**Configuring OpenAI with IChatClient:**

```csharp
// In your server configuration
if (server.Configuration.GetValue<string>("OpenAi:ApiKey") is { } openAiApiKey &&
    server.Configuration.GetValue<string>("OpenAi:Endpoint") is { } openAiEndpoint)
{
    var openAiClient = new OpenAIClient(
        new System.ClientModel.ApiKeyCredential(openAiApiKey),
        new OpenAIClientOptions
        {
            Endpoint = new Uri(openAiEndpoint)
        });

    var openAiChatClient = openAiClient.GetChatClient("gpt-4o");
    var chatClient = openAiChatClient.AsIChatClient();
    server.Services.AddSingleton<IChatClient>(chatClient);
}
```

**Using IChatClient in your apps:**

```csharp
public override object? Build()
{
    var chatClient = UseService<IChatClient?>();

    if (chatClient == null)
    {
        return Callout.Error("IChatClient is not configured.");
    }

    var messages = new List<ChatMessage>
    {
        new(ChatRole.System, "You are a helpful assistant."),
        new(ChatRole.User, userInput)
    };

    var response = await chatClient.GetResponseAsync(messages);
    return response.Text;
}
```

Store your OpenAI credentials using .NET user secrets or configuration:
```json
{
  "OpenAi:ApiKey": "your-api-key",
  "OpenAi:Endpoint": "https://api.openai.com/v1"
}
```

### Refresh Tokens Guide

A comprehensive new documentation guide has been added explaining Refresh Tokens - a powerful mechanism for manually triggering UI updates and effect executions in Ivy applications. This feature enables you to reload data, refresh components, or trigger actions on demand.

**Creating a Refresh Token:**

```csharp
public override object? Build()
{
    var refreshToken = this.UseRefreshToken();
    var timestamp = UseState(DateTime.Now);

    // Effect runs when refresh token changes
    UseEffect(() =>
    {
        timestamp.Set(DateTime.Now);
    }, [refreshToken]);

    return Layout.Vertical()
        | Text.Muted("Click the button to manually trigger a refresh")
        | new Button("Refresh", onClick: _ => refreshToken.Refresh())
        | Text.P($"Last refreshed: {timestamp.Value:HH:mm:ss.fff}");
}
```

**Passing Data with Return Values:**

Refresh tokens can carry return values to pass data from user actions to effects:

```csharp
public override object? Build()
{
    var refreshToken = this.UseRefreshToken();
    var selectedColor = UseState("No color selected");

    UseEffect(() =>
    {
        if (refreshToken.IsRefreshed && refreshToken.ReturnValue is string color)
        {
            selectedColor.Set($"Selected: {color}");
        }
    }, [refreshToken]);

    return Layout.Vertical()
        | Layout.Horizontal(
            new Button("Red", onClick: _ => refreshToken.Refresh("Red")),
            new Button("Green", onClick: _ => refreshToken.Refresh("Green")),
            new Button("Blue", onClick: _ => refreshToken.Refresh("Blue")))
        | Text.P(selectedColor.Value);
}
```

**When to Use Refresh Tokens:**

- Trigger effects after async operations complete
- Pass data from background operations to trigger UI updates
- Coordinate updates across different parts of your view
- Refresh external content like iframes
- Combine with `AfterInit` trigger to load data on initialization AND manual refresh

**Token Properties:**

- `Token` (Guid): A unique identifier that changes with each refresh
- `IsRefreshed` (bool): `true` if the token has been refreshed at least once
- `ReturnValue` (object?): The value passed to the last `Refresh()` call

The documentation includes working examples showing basic usage, return value patterns, and best practices for using refresh tokens effectively in your applications.