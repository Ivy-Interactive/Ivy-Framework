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

- **Better rendering performance** - Faster rendering for large datasets with the new v6 engine
- **Enhanced toolbox theming** - Toolbox buttons, data view dialog, and icons now properly respect your theme colors (muted-foreground for borders, card colors for backgrounds)
- **Fixed legend key casing** - Resolved an issue where chart legend keys had inconsistent casing by removing dictionary key policy from the serializer
- **Improved tooltip and crosshair styling** - Tooltips, crosshair lines, and axis pointers now use theme colors (muted-foreground) for better visual consistency
- **Better data view dialog** - The data view feature now uses proper theme colors for background, text, textarea, and buttons
- **Improved toolbox positioning** - Toolbox now positions correctly at -10px from top when vertical alignment is set to "top"

**No code changes required** - all existing chart code continues to work as before. The upgrade is fully backward compatible.

### PieChart Toolbox Refinement

The `PieChart` widget's toolbox has been refined to remove the chart type switcher (magicType) option, which was not applicable to pie charts. This provides a cleaner, more focused toolbox with only the relevant tools:

- **Save as Image** - Export your chart
- **Data View** - View the raw data
- **Restore** - Reset zoom/pan

This change eliminates confusion from having a non-functional chart type switcher in the pie chart toolbox, making the UI more intuitive.

### Card Icon Color Refinement

Card icons now use `Colors.Neutral` instead of `Colors.Muted` for a more balanced visual appearance. This subtle change provides better contrast and readability for card icons while maintaining a clean, professional look.

**Example:**

```csharp
// Icons passed to Card headers now automatically use Neutral color
var card = new Card()
    .Title("Settings")
    .Description("Manage your preferences")
    .Header(Icons.Settings); // Icon will use Neutral color
```

This is handled automatically when you use the `Header()` extension method with an icon - no changes needed to your existing code.

### GitHub Contributors Display Enhancement

The contributor display in documentation pages now shows proper display names for team members instead of only GitHub usernames. This provides a more professional appearance in the built-in `GitHubContributors` widget used for article pages.

**What's improved:**

- **Display names for team members** - Team members can now have custom display names (e.g., "Niels Bosma" instead of "nielsbosma")
- **Smart name fallback** - If no display name is configured, the widget shows the commit author name when it differs from the username
- **More accurate contribution tracking** - Improved logic for counting contributions per contributor
- **Better data structure** - Team member information now includes both role and optional display name

This makes it easier to identify core team members by their full names while still showing contributor statistics accurately for both team members and open source contributors.

### Pivot Table Support for Data Aggregation

The framework now includes `ToPivotTable()` extension method for aggregating and summarizing data by grouping on dimensions and calculating measures. This powerful feature allows you to transform raw data into aggregated results perfect for reporting and analytics dashboards.

**Example:**

```csharp
record SalesData(string Browser, string Region, int Sessions, decimal Revenue);
record BrowserSummary(string Browser, int TotalSessions, decimal TotalRevenue, decimal AverageRevenue);

var rawData = new[] {
    new SalesData("Chrome", "North", 150, 4500m),
    new SalesData("Chrome", "South", 120, 3600m),
    new SalesData("Firefox", "North", 80, 2400m),
    // ... more data
};

// Simple pivot by browser
var pivotByBrowser = await rawData.ToPivotTable()
    .Dimension("Browser", d => d.Browser)
    .Measure("Total Sessions", g => g.Sum(s => s.Sessions))
    .Measure("Total Revenue", g => g.Sum(s => s.Revenue))
    .Measure("Average Revenue", g => g.Average(s => s.Revenue))
    .ExecuteAsync();

// Multi-dimensional pivot
var pivotByBrowserAndRegion = await rawData.ToPivotTable()
    .Dimension("Browser", d => d.Browser)
    .Dimension("Region", d => d.Region)
    .Measure("Sessions", g => g.Sum(s => s.Sessions))
    .Measure("Revenue", g => g.Sum(s => s.Revenue))
    .ExecuteAsync();

// Strongly-typed results
var typedResults = new List<BrowserSummary>();
await foreach (var item in rawData.ToPivotTable()
    .Dimension("Browser", d => d.Browser)
    .Measure("TotalSessions", g => g.Sum(s => s.Sessions))
    .Measure("TotalRevenue", g => g.Sum(s => s.Revenue))
    .Measure("AverageRevenue", g => g.Average(s => s.Revenue))
    .Produces<BrowserSummary>()
    .ExecuteAsync())
{
    typedResults.Add(item);
}

// Display in a table
return pivotByBrowser.ToExpando().ToTable().Width(Size.Full());
```

**Key Features:**

- **Dimensions**: Group data by one or more fields (like SQL GROUP BY)
- **Measures**: Perform aggregations (Sum, Count, Average, Max, Min)
- **Strongly-typed results**: Use `.Produces<T>()` to get typed objects
- **Dynamic results**: Use `.ToExpando()` for flexible dynamic objects
- **Async streaming**: Results stream asynchronously for better performance

This makes building analytics dashboards and summary reports much simpler, eliminating the need for manual grouping and aggregation logic.

### Text.ExtraLarge() Variant Added

A new text size variant has been added to the `Text` widget for even more prominent text display. The hierarchy now includes:

```csharp
Text.ExtraLarge("Extra Large text")  // New variant
Text.Large("Large text")
Text.Lead("Lead text for prominent display")
Text.Default("Default text")
Text.Small("Small text")
```

Use `Text.ExtraLarge()` when you need text larger than `Lead` but smaller than heading styles, perfect for call-out sections or featured content.

### H5 and H6 Heading Variants Added

The `Text` API now includes support for H5 and H6 heading levels, completing the full HTML heading hierarchy:

```csharp
// All heading levels now supported
Text.H1("Main Title")
Text.H2("Section Heading")
Text.H3("Subsection Heading")
Text.H4("Minor Heading")
Text.H5("Small Heading")      // New variant
Text.H6("Smallest Heading")   // New variant
```

These smaller heading variants are useful for deeply nested documentation structures or when you need more granular heading hierarchy in your content.

### Typography and Spacing Improvements

The framework's typography system has been refined with improved spacing consistency across all text elements. This comprehensive update improves readability and visual hierarchy across documentation, content pages, and all text-based widgets.

**Key improvements:**

- **Smart heading spacing** - Headings now have contextual spacing that adjusts based on following elements (headings following headings have reduced top margin, paragraphs immediately after headings have no top margin)
- **Improved visual hierarchy** - Consistent margins and padding across all text variants for better content flow
- **Enhanced component spacing** - Separators, callouts, and code blocks now have better default spacing
- **Terminal widget polish** - Added bottom margin for better visual separation in documentation
- **Separator refinements** - Horizontal separators now include proper vertical margins (my-6)
- **Callout improvements** - Better padding structure for more balanced appearance

**Typography consolidation:**

The internal typography system has been consolidated and refactored for better maintainability. All typography styles now live in a single, unified system that ensures consistency across the framework. This includes:

- Unified heading styles (H1 through H6) with consistent font weights and spacing
- Standardized body text, lead text, and size variants
- Semantic variants (danger, warning, success) with consistent styling
- Proper list, table, blockquote, and code formatting

These improvements provide a more polished and consistent reading experience across documentation and content-heavy pages. No code changes are required - the improvements are automatically applied to all text widgets.

### Terminal Widget Copy Button

The `Terminal` widget now includes a convenient copy-to-clipboard button that automatically extracts and copies all command lines from the terminal display. This is especially useful in documentation and tutorials where users need to copy commands.

**Features:**

- Automatically positioned in the top-right corner of the terminal
- Only copies command lines (ignoring output text)
- Can be disabled if needed using `.ShowCopyButton(false)`

**Example:**

```csharp
// Default behavior - copy button is shown
var terminal = new Terminal()
    .Title("Installation")
    .ShowHeader(true);

// Optionally hide the copy button
var terminalNoCopy = new Terminal()
    .Title("Output Only")
    .ShowCopyButton(false);
```

When users click the copy button, only the actual commands (lines prefixed with `>`) are copied to the clipboard, making it easy to paste multiple commands into their own terminal without needing to manually select and copy each line individually.

### Code Snippet Copy Button Enhancement

The copy-to-clipboard button for code snippets has been enhanced with better sizing and improved visual feedback:

**What's improved:**

- **Larger buttons** - Copy buttons are now more accessible across all scales (Small: 24px, Medium: 32px, Large: 36px, up from 20px/24px/28px)
- **Better padding** - Increased from 4px to 8px for easier clicking
- **Refined styling** - Icon-only buttons now have transparent background by default with smooth hover transitions
- **Improved hover states** - More intuitive color transitions when hovering over copy buttons
- **Better visual feedback** - Copied state uses primary color for clearer confirmation

These improvements make it easier to copy code examples from documentation and code snippets, especially on touch devices and for users who need larger click targets for accessibility.

## Bug Fixes

### Fixed AsyncSelectInput Styling Issues

The `AsyncSelectInput` widget now displays correctly with proper icon positioning and spacing. Previous versions had layout issues where:

- The invalid icon could overlap with the selected value text
- The chevron icon positioning was inconsistent across different scales
- Content padding didn't account for icon space

All these issues have been resolved, providing a cleaner and more professional appearance for async select inputs across all scales (Small, Medium, Large).

### Fixed Page Padding in Non-Chrome Applications

Resolved an issue where page padding was not rendering correctly in applications without Chrome (the outer application shell). Pages in non-Chrome apps now properly display their padding, ensuring consistent layout spacing across all application types.

### Improved Tooltip Text Handling

Tooltips now intelligently handle word breaking and have a maximum height constraint to prevent oversized tooltips. The framework automatically detects when content contains long unbroken strings (like URLs or technical identifiers) and applies appropriate word-breaking behavior:

- **Automatic detection** - Content without spaces uses `break-all` to prevent overflow
- **Normal text** - Content with spaces uses `break-normal` for natural word wrapping
- **Maximum height** - Tooltips are capped at 20% of viewport height to prevent screen overflow
- **Manual control** - Override automatic behavior with the `breakType` prop (`'auto'`, `'normal'`, `'all'`, or `'words'`)

**Example:**

```typescript
// Automatic behavior (default)
<TooltipContent>
  Long unbroken text here
</TooltipContent>

// Force specific break behavior
<TooltipContent breakType="words">
  Custom content with controlled word breaking
</TooltipContent>
```

This ensures tooltips remain readable and properly contained, regardless of content type. The improvement is particularly noticeable with DataTable cell tooltips that display long text values.

### Removed Focus Outline on Sheet Container

The Sheet component no longer displays a focus outline on its container element, providing a cleaner visual appearance. This subtle polish improvement removes an unnecessary visual indicator that could appear when sheets were opened, resulting in a more professional look for dialog and drawer components.

### Database Generator Migration Cleanup

The Ivy CLI's `DatabaseGenerator` now automatically cleans up the initial migration entry from the `__EFMigrationsHistory` table after applying it. This prevents issues where EF Core would look for a migration file (`InitialCreate`) that only exists in the temporary database generator project, not in your actual application.

**What this fixes:**

- Eliminates errors when EF Core tries to find the `InitialCreate` migration in your project
- Ensures a clean migration history that only references migrations in your codebase
- No manual cleanup required after database generation

This happens automatically when you run the database generator - no changes needed to your workflow. The cleanup is handled gracefully, with warnings logged in verbose mode if any issues occur.

### Fixed Root Widget Replacement

Resolved an issue where completely replacing the root widget of your application could fail to render correctly. The framework now properly handles scenarios where the entire widget tree needs to be replaced (rather than just patched), ensuring smooth updates even when the top-level widget changes completely.

This fix improves reliability for applications that dynamically swap out their entire UI structure, such as navigation between completely different page layouts or application modes.

### Fixed Authentication Logout When User Info Unavailable

Resolved an issue where users couldn't logout from applications using `DefaultSidebarChrome` if their user information failed to load. The logout functionality now works reliably based on authentication session state rather than user info availability.

**What this fixes:**

- Users can now logout even if `GetUserInfoAsync()` returns null or fails
- The logout menu item appears correctly when authenticated, regardless of user info state
- Prevents users from being stuck in an authenticated state when profile data is unavailable

This is particularly helpful in scenarios where user profile services are temporarily unavailable or experiencing issues, ensuring users always have a way to clear their authentication session.

### Fixed OAuth Popup Blocking in Safari

OAuth authentication now works reliably in Safari and other browsers with strict popup blocking policies. Previously, OAuth login buttons would open popups directly, which Safari would block as unauthorized popup windows.

**What changed:**

- OAuth login buttons now navigate to a server endpoint (`/ivy/auth/oauth-login`) which redirects to the OAuth provider
- This server-side redirect approach bypasses Safari's popup blocker
- Better error handling with more concise error messages
- Enhanced parameter validation to prevent malformed OAuth requests

**No code changes required** - this fix is handled automatically by the framework. OAuth authentication buttons continue to work exactly as before, but now function correctly in Safari and other browsers with aggressive popup blocking.

This is particularly important for applications where Safari users represent a significant portion of your user base, as OAuth authentication was previously non-functional in that browser.

### Fixed TabsLayout Width Rendering

Resolved an issue where `TabsLayout` components were not rendering at full width in some scenarios. The tabs container now properly expands to fill its parent container's width, ensuring consistent layout behavior across different use cases.

**Responsive Overflow:**

When you have many tabs that don't fit in the available width, the `TabsLayout` component automatically shows a dropdown menu for the hidden tabs. This provides a clean way to handle large numbers of tabs without horizontal scrolling or layout breaks:

```csharp
// Tabs automatically overflow to dropdown when space is limited
Layout.Tabs(
    new Tab("Home", homeContent),
    new Tab("Products", productsContent),
    new Tab("Services", servicesContent),
    new Tab("About", aboutContent),
    new Tab("Contact", contactContent),
    new Tab("Blog", blogContent)
    // ... more tabs
)
```

The component intelligently manages available space and provides a seamless experience regardless of how many tabs you add or how users resize their browser windows.

### Fixed Sidebar Arrow Rotation for Nested Grouped Items

Resolved an issue where the chevron arrow indicator for collapsible sidebar menu items with nested groups was not rotating correctly when expanded/collapsed. The arrow now properly rotates 90 degrees when toggling nested menu items, providing consistent visual feedback across all levels of menu nesting.

**What changed:**

- Fixed CSS class selector from `group-data-[state=open]/collapsible` to `group-data-[state=open]`
- Added proper null coalescing for the `expanded` state to ensure boolean values
- Cleaned up redundant group class definitions

This fix ensures that nested sidebar menu items (like grouped navigation sections) have proper visual indicators when expanded or collapsed, making the sidebar navigation more intuitive and user-friendly.

### Chart Configuration Consistency

Fixed an issue where chart properties weren't being applied consistently between the backend and frontend. The framework now ensures that default values for chart configurations (lines, bars, pies, legends, grids, tooltips, toolboxes, and reference lines) are properly applied on the frontend, matching the C# backend behavior.

**What this fixes:**

- Charts now render more consistently with expected default values
- Properties that equal backend defaults are now properly applied even when not explicitly serialized
- Eliminates subtle rendering inconsistencies in chart widgets (`AreaChart`, `BarChart`, `PieChart`)

This improvement ensures that charts behave predictably and consistently, particularly when relying on default property values rather than explicitly setting every configuration option.

## New Features

### Clerk Authentication Provider

A new authentication provider for Clerk (<https://clerk.com>) has been added to the framework, allowing you to leverage Clerk's complete user management platform in your Ivy applications. Clerk provides a modern, secure, and flexible authentication solution with support for multiple authentication methods.

**Installation:**

The provider is available in the new `Ivy.Auth.Clerk` package:

```bash
dotnet add package Ivy.Auth.Clerk
```

**Setup:**

1. Create a Clerk application at [clerk.com](https://clerk.com)
2. Configure your Clerk keys using .NET user secrets (development) or environment variables (production):

```terminal
dotnet user-secrets set "Clerk:SecretKey" "your_secret_key"
dotnet user-secrets set "Clerk:PublishableKey" "your_publishable_key"
```

> **Note:** Both keys must be for the same environment (either both `test` or both `live`).

1. Configure the provider in your application:

```csharp
var server = new Server();

// Configure Clerk Auth Provider with desired authentication methods
var authProvider = new ClerkAuthProvider()
    .UseEmailPassword()
    .UseGoogle()
    .UseGithub()
    .UseMicrosoft();

server.UseAuth(authProvider);

await server.RunAsync();
```

**Supported Authentication Methods:**

- **Email/Password** - Traditional email and password authentication
- **Google OAuth** - Sign in with Google accounts
- **GitHub OAuth** - Sign in with GitHub accounts
- **Microsoft OAuth** - Sign in with Microsoft accounts
- **Twitter OAuth** - Sign in with Twitter accounts
- **Apple OAuth** - Sign in with Apple ID

**Features:**

- **Multi-provider support** - Mix and match authentication methods based on your needs
- **Complete user management** - Leverages Clerk's full user management platform
- **Security best practices** - Built-in security features, session management, and user verification
- **Customizable** - Configure only the authentication methods your application needs

The Clerk provider integrates seamlessly with Ivy's authentication system, providing a production-ready authentication solution with minimal configuration.

### GitHub OAuth Authentication Provider

A new authentication provider for GitHub OAuth 2.0 has been added to the framework, allowing users to sign in to your Ivy applications using their GitHub accounts. This is perfect for developer-focused applications and internal tools.

**Installation:**

The provider is available in the new `Ivy.Auth.GitHub` package:

```bash
dotnet add package Ivy.Auth.GitHub
```

**Setup:**

1. Create a GitHub OAuth App in your [GitHub Developer settings](https://github.com/settings/developers)
2. Register the HttpClient factory and configure the provider:

```csharp
var server = new Server();

// Register HttpClient for GitHub API
server.Services.AddHttpClient("GitHubAuth", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Ivy-Framework");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
});

// Ensure IConfiguration is registered
server.Services.AddSingleton(server.Configuration);

// Configure GitHub Auth Provider
server.UseAuth<GitHubAuthProvider>(c => c.UseGitHub());

await server.RunAsync();
```

1. Configure your GitHub OAuth App credentials using .NET user secrets (development) or environment variables (production):

```terminal
dotnet user-secrets set "GitHub:ClientId" "your_client_id"
dotnet user-secrets set "GitHub:ClientSecret" "your_client_secret"
dotnet user-secrets set "GitHub:RedirectUri" "http://localhost:5010/ivy/webhook"
```

**Features:**

- **Standard OAuth 2.0 flow** - Secure authorization code flow
- **Automatic user info retrieval** - Fetches user ID, email, display name, and avatar from GitHub
- **Smart email handling** - Automatically selects primary or first verified email
- **Long-lived tokens** - GitHub OAuth tokens don't expire
- **User scope** - Requests `user:email` scope for user identification

The authentication flow is seamless: users click the GitHub login button, authorize your app on GitHub, and are redirected back to your application authenticated. The provider handles all token exchange and user info retrieval automatically.

### Dynamic Page Titles

The framework now automatically updates the browser page title to reflect your current application route. This provides better browser history, bookmarking, and tab identification for users.

**How it works:**

When you define an `AppDescriptor`, the framework automatically uses its `Title` property to set the browser page title:

```csharp
public class MyAppRepository : AppRepository
{
    public override IEnumerable<AppDescriptor> GetApps()
    {
        yield return new AppDescriptor(
            Id: "dashboard",
            Title: "Dashboard",  // This becomes the browser page title
            Component: typeof(DashboardView),
            MenuItems: [/* ... */]
        );

        yield return new AppDescriptor(
            Id: "settings",
            Title: "Settings",  // Browser title updates when navigating here
            Component: typeof(SettingsView),
            MenuItems: [/* ... */]
        );
    }
}
```

**For applications using `DefaultSidebarChrome` with tabs:**

- Opening a new tab sets the page title to match the tab's application
- Switching between tabs updates the page title dynamically
- Closing all tabs resets the title to your application's default `MetaTitle`

**Title formatting:**
If you've configured a `MetaTitle` in your server configuration, page titles are automatically formatted as:

```
{AppTitle} - {MetaTitle}
```

For example, if your app has `MetaTitle = "My Company"` and you navigate to a page with `Title = "Dashboard"`, the browser title becomes: **Dashboard - My Company**

**No code changes required** - the framework automatically manages page titles based on your existing `AppDescriptor` definitions. This enhancement improves SEO, browser history clarity, and overall user experience when managing multiple tabs or bookmarks.

### Custom Login Views

You now have full control over the authentication login experience. Instead of using the default login interface, you can replace it with a completely custom view tailored to your application's needs.

**Complete Custom Login View:**

```csharp
// Replace the entire login UI with your custom implementation
server.UseAuth<BasicAuthProvider>(viewFactory: () => new MyCustomLoginApp());
```

This allows you to:

- Design a completely custom login experience that matches your brand
- Add custom elements like marketing content, help text, or additional authentication options
- Implement complex multi-step login flows
- Integrate with custom UI libraries or design systems

Your custom login view receives all the necessary authentication infrastructure while giving you complete control over the presentation and user experience.
