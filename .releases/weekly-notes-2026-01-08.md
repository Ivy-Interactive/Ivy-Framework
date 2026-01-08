# Ivy Framework Weekly Notes - Week of 2026-01-08

## Breaking Changes

### UseBuilder Renamed to UseWebApplicationBuilder

The `Server.UseBuilder()` method has been renamed to `UseWebApplicationBuilder()` for better clarity and to distinguish it from the new `UseWebApplication()` method.

### NavigationPurpose Renamed to HistoryOp

- `NavigationPurpose` - `HistoryOp`
- `NavigationPurpose.NewDestination` - `HistoryOp.Push`
- `NavigationPurpose.HistoryTraversal` - `HistoryOp.Pop`

### Audio Widget Renamed to AudioPlayer

The `Audio` widget has been renamed to `AudioPlayer` for better clarity and consistency. All references throughout the codebase have been updated.

### Icons Enum Updated to Match Lucide React 0.562.0

The `Icons` enum has been updated to align with the latest version of lucide-react (0.562.0).

## Improvements

### Spacer and Loading Widgets Now Public

The `Spacer` and `Loading` widget constructors are now public, allowing you to create instances directly in your layouts.

**Usage:**

```csharp
// You can now create spacers and loading indicators directly
var spacer = new Spacer();
var loading = new Loading();

// Use spacers in layouts
var layout = Layout.Horizontal()
    | new Button("Left")
    | new Spacer()  // Takes up remaining space
    | new Button("Right");

// Use loading indicators conditionally
var content = isLoading
    ? new Loading()
    : new Text("Content loaded!");
```

### ReadOnlyInput Simplified Constructor

The `ReadOnlyInput` widget now includes a non-generic constructor for string values, making it more convenient to use for common scenarios where you're displaying text-based read-only data.

```csharp
var readOnly = new ReadOnlyInput("User ID: 12345");
```

The generic version `ReadOnlyInput<T>` is still available when you need to work with other data types.

### LLM-Friendly Documentation

- `UseSitemap()` - Automatically generates `/robots.txt` and `/sitemap.xml` based on your visible apps
- `UseSsrMarkdown()` - Detects bot user agents (ChatGPT, Claude, Perplexity, etc.) and serves simplified HTML with markdown content
- `UseMarkdownFiles()` - Serves embedded `.md` files directly at URLs like `/api/button.md`, with caching for performance

### Enhanced Calendar Navigation

Calendar widgets (`DateInput`, `DateTimeInput`, `DateRangeInput`) now include dropdown selectors for month and year, making it much easier to navigate to dates far in the past or future. Instead of clicking through months one-by-one, you can now:

- **Select year from dropdown** - Choose any year from 1900 to 2100 directly
- **Select month from dropdown** - Jump to any month instantly
- **Improved date visibility** - Fixed an issue where some dates could appear invisible in certain scenarios

### DataTable Service Lazy Initialization

The `GrpcTableService` is now initialized lazily, only when DataTables are actually used in your application. This provides significant performance improvements for applications that don't use DataTables:

### Font Loading Performance

Font flickering during page load has been eliminated by migrating to the Ivy Design System package. Fonts now load more reliably and smoothly, providing a better visual experience when your application first renders.

### Better Number Input Validation and Formatting

The `NumberInput` widget has been enhanced with improved null value handling and better format support. The input now properly handles null values and provides more reliable formatting:

- **Better null value handling** - Default null values are now properly managed, with nullable inputs defaulting to null and non-nullable inputs defaulting to 0
- **Improved format fallback** - When formatted display fails, the widget gracefully falls back to the raw number string instead of showing blank

### Automatic Color Input Validation

The `ColorInput` widget now automatically validates color values and displays an error state when invalid formats are entered.

**What's validated:**

- **Hex colors** - Must match valid formats: `#RGB`, `#RRGGBB`, or `#RRGGBBAA`
- **Color enums** - Must be a valid value from the `Colors` enum
- **Invalid entries** - Automatically marked with "Invalid color format" error message

**Example:**

```csharp
// Automatic validation - no manual Invalid() calls needed
var colorState = UseState("#ff0000");  // Valid - works fine
var badColor = UseState("#invalid");    // Invalid - automatically shows error

return new VStack()
    .Add(colorState.ToColorInput())     // Shows valid state
    .Add(badColor.ToColorInput());      // Shows error state automatically
```

The validation happens automatically when you use `ToColorInput()`, so you no longer need to manually check color formats or call the `Invalid()` method for format errors. The input will update its error state in real-time as the user types.

### Widget Serialization Optimization

### Scale Inheritance for Nested Widgets

Widgets now properly inherit scale settings from their parent widgets, ensuring consistent sizing throughout nested component hierarchies. When you set a scale on a parent widget, all children automatically inherit that scale unless explicitly overridden.

**Example:**

```csharp
// The Details widget and all its children will use the Large scale
new Details(Scales.Large)
    .Add(new Text("Title"))           // Inherits Large scale
    .Add(new Button("Click Me"))      // Inherits Large scale
    .Add(new Input().Placeholder("Enter text")); // Inherits Large scale
```

### ECharts Upgraded to v6

The charting library has been upgraded from ECharts v5.6.0 to v6.0.0, bringing performance improvements and new features to all chart widgets (`AreaChart`, `BarChart`, `LineChart`, `PieChart`).

**What's improved:**

- **Enhanced toolbox theming** - Toolbox buttons, data view dialog, and icons now properly respect your theme colors
- **Fixed legend key casing** - Resolved an issue where chart legend keys had inconsistent casing by removing dictionary key policy from the serializer
- **Improved tooltip and crosshair styling** - Tooltips, crosshair lines, and axis pointers now use theme colors (muted-foreground) for better visual consistency
- **Better data view dialog** - The data view feature now uses proper theme colors for background, text, textarea, and buttons

### PieChart Toolbox Refinement

- **Save as Image** - Export your chart
- **Data View** - View the raw data
- **Restore** - Reset zoom/pan

### Typography and Spacing Improvements

The framework's typography system has been refined with improved spacing consistency across all text elements. This comprehensive update improves readability and visual hierarchy across documentation, content pages, and all text-based widgets.

**Key improvements:**

- **Smart heading spacing**
- **Improved visual hierarchy**
- **Enhanced component spacing**

### Terminal Widget Copy Button

The `Terminal` widget now includes a convenient copy-to-clipboard button that automatically extracts and copies all command lines from the terminal display. This is especially useful in documentation and tutorials where users need to copy commands.

### Code Snippet Copy Button Enhancement

## Bug Fixes

### Fixed Page Padding in Non-Chrome Applications

### Improved Tooltip Text Handling

### Removed Focus Outline on Sheet Container

### Database Generator Migration Cleanup

### Fixed Root Widget Replacement

### Fixed Authentication Logout When User Info Unavailable

### Fixed OAuth Popup Blocking in Safari

### Fixed TabsLayout Width Rendering

### Fixed Sidebar Arrow Rotation for Nested Grouped Items

### Chart Configuration Consistency

## New Features

### Clerk Authentication Provider

A new authentication provider for Clerk (<https://clerk.com>) has been added to the framework, allowing you to leverage Clerk's complete user management platform in your Ivy applications.

```bash
dotnet add package Ivy.Auth.Clerk
```

1. Create a Clerk application at [clerk.com](https://clerk.com)
2. Configure your Clerk keys using .NET user secrets (development) or environment variables (production):

```terminal
dotnet user-secrets set "Clerk:SecretKey" "your_secret_key"
dotnet user-secrets set "Clerk:PublishableKey" "your_publishable_key"
```

```csharp
var server = new Server();

var authProvider = new ClerkAuthProvider()
    .UseEmailPassword()
    .UseGoogle()
    .UseGithub()
    .UseMicrosoft();

server.UseAuth(authProvider);

await server.RunAsync();
```

### GitHub OAuth Authentication Provider

A new authentication provider for GitHub OAuth 2.0 has been added to the framework, allowing users to sign in to your Ivy applications using their GitHub accounts.

```bash
dotnet add package Ivy.Auth.GitHub
```

```csharp
var server = new Server();

// Register HttpClient for GitHub API
server.Services.AddHttpClient("GitHubAuth", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Ivy-Framework");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
});
server.Services.AddSingleton(server.Configuration);
server.UseAuth<GitHubAuthProvider>(c => c.UseGitHub());
await server.RunAsync();
```

Configure your GitHub OAuth App credentials using .NET user secrets (development) or environment variables (production):

```terminal
dotnet user-secrets set "GitHub:ClientId" "your_client_id"
dotnet user-secrets set "GitHub:ClientSecret" "your_client_secret"
dotnet user-secrets set "GitHub:RedirectUri" "http://localhost:5010/ivy/webhook"
```

### Dynamic Page Titles

The framework now automatically updates the browser page title to reflect your current application route.

When you define an `AppDescriptor`, the framework automatically uses its `Title` property to set the browser page title:

```csharp
yield return new AppDescriptor(
    Id: "dashboard",
    Title: "Dashboard",  // This becomes the browser page title
    Component: typeof(DashboardView),
    MenuItems: [/* ... */]
)
```

### Custom Login Views

You now have full control over the authentication login.

```csharp
// Replace the entire login UI with your custom implementation
server.UseAuth<BasicAuthProvider>(viewFactory: () => new MyCustomLoginApp());
```
