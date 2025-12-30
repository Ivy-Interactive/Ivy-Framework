# Ivy Framework Weekly Notes - Week of 2025-09-06

## Ivy Framework

### Simplified Terminal Widget ARIA Roles

Terminal widgets now use streamlined ARIA roles for better accessibility and content extraction.

**Key improvements:**
- Single `role="terminal"` attribute replaces complex `application` role
- Terminal text marked with `role="terminal-text"` for easy identification
- Individual lines use `role="log"` for semantic structure
- Command lines distinguished via `aria-label` attributes
- Simplified selectors for content extraction tools

This makes terminal widgets more maintainable while preserving full accessibility support.

### Expandable Content Support in Documentation

Documentation now supports expandable/collapsible content blocks for cleaner organization. Use the `<Details>` tag to create expandable sections.

**Usage:**

```markdown
<Details>
<Summary>Click to see implementation details</Summary>
<Body>
This content is hidden by default and can be expanded by clicking the summary.
You can include any markdown content here including:
- Lists
- Code blocks
- Tables
- Other nested markdown elements
</Body>
</Details>
```

Sections support full markdown rendering including code blocks, lists, and tables. The implementation handles both single and multiple content blocks automatically.

**Enhanced Animations:** Smooth 100ms animations with rotating chevron icons and fade effects provide responsive feedback.

Text input and WrapLayout documentation now use collapsible sections extensively, making pages cleaner while preserving detailed examples.

### Theme System for Custom Application Styling

Comprehensive theming system for customizing your application's visual appearance with custom colors, typography, and style properties.

**API Changes:**
- `Theme` class (was `ThemeConfig`)
- `ThemeMode` enum (was `Theme`)
- `ApplyTheme()` method (was `ApplyThemeCss()`)

**Light and Dark Mode Support:**

Define separate color palettes for light and dark modes:

**Configuring a Theme with Light and Dark Modes:**

```csharp
var server = new Server()
    .UseTheme(theme =>  // Now accepts Theme instead of ThemeConfig
    {
        theme.Name = "My Custom Theme";
        theme.Colors = new ThemeColorScheme
        {
            Light = new ThemeColors
            {
                Primary = "#0077BE",
                PrimaryForeground = "#FFFFFF",
                Secondary = "#5B9BD5",
                SecondaryForeground = "#FFFFFF",
                Background = "#F0F8FF",
                Foreground = "#1A1A1A",
                // ... configure other light mode colors
            },
            Dark = new ThemeColors
            {
                Primary = "#4A9EFF",
                PrimaryForeground = "#001122",
                Secondary = "#2D4F70",
                SecondaryForeground = "#E8F4FD",
                Background = "#001122",
                Foreground = "#E8F4FD",
                // ... configure other dark mode colors
            }
        };
    });
```

**Available Theme Properties:**
- Primary/Secondary colors with foregrounds
- Semantic colors (Success, Warning, Error, Info)
- UI elements (Background, Card, Border, Input, Accent, Muted)
- Popover colors for overlays
- Chart colors (Chart1-5) for data visualization
- Sidebar theming (primary, accent, border, ring)
- Typography (font family, size, border radius)

**Using Predefined Themes:**

```csharp
// Use the default theme with light and dark modes
server.UseTheme(Theme.Default);  // Changed from ThemeConfig.Default

// Or create and reuse theme objects
var oceanTheme = new Theme  // Changed from ThemeConfig
{
    Name = "Ocean",
    Colors = new ThemeColorScheme
    {
        Light = new ThemeColors
        {
            Primary = "#0077BE",
            Background = "#F0F8FF",
            // ... other light mode colors
        },
        Dark = new ThemeColors
        {
            Primary = "#4A9EFF",
            Background = "#001122",
            // ... other dark mode colors
        }
    }
};
server.UseTheme(oceanTheme);
```

**Dynamic Theme Application:**

Themes can now be applied dynamically at runtime without restarting your application or even refreshing the page! The enhanced theme system now supports instant CSS updates:

```csharp
// Get the theme service
var themeService = UseService<IThemeService>();
var client = UseClient();

// Apply a new theme dynamically
themeService.SetTheme(new Theme  // Changed from ThemeConfig
{
    Name = "My Theme",
    Colors = new ThemeColors
    {
        Primary = "#FF6347",
        // ... other colors
    }
});

// Apply CSS instantly without page refresh
var css = themeService.GenerateThemeCss();
client.ApplyTheme(css);  // Changed from ApplyThemeCss()
```

**Theme Customizer Sample App:**
- Instant theme application without page refresh
- Live CSS injection for immediate updates
- Real-time color palette preview
- C# and JSON export options
- Built-in presets: Ocean, Forest, Sunset, Midnight (with light/dark modes)
- Side-by-side light/dark mode preview
- Improved color preview using `Box` widgets with semantic colors

Themes inject CSS variables to override defaults, with all Ivy components automatically respecting theme settings.

### Enhanced Code Editor Selection Highlighting

Improved text selection visibility in code editors with theme-aware custom styling.

**Features:**
- Clear visual distinction using theme colors
- Smooth selection updates while dragging
- CSS variable integration with Ivy themes
- Robust edge case handling with range validation
- Batched state updates for better performance

### New Pagination Widget

Ivy now includes a powerful `Pagination` widget for navigating through large sets of data. This new component provides an intuitive way for users to browse through multiple pages of content with customizable controls and appearance.

**Basic Usage:**

```csharp
public class BasicPaginationApp : ViewBase
{
    public override object? Build() {
        var page = UseState(5);

        return new Pagination(
            page: page.Value, 
            numPages: 10, 
            onChange: newPage => page.Set(newPage.Value)
        );
    }
}
```

**Key Features:**
- **Previous/Next Navigation**: Built-in buttons for moving between pages
- **Page Numbers**: Direct access to specific pages with visual indication of the current page
- **Smart Ellipsis**: Automatically shows ellipsis (...) for page ranges when there are many pages
- **Customizable Display**: Control how many page numbers appear using `Siblings()` and `Boundaries()` methods
- **Disabled State**: Support for disabling the pagination controls when needed

**Advanced Configuration:**

```csharp
// Control visible page numbers
new Pagination(page.Value, 20, onChange)
    .Siblings(2)  // Pages on each side of current
    .Boundaries(1) // Pages at start/end

// Disable interaction
new Pagination(page.Value, 10, onChange)
    .Disabled(true)
```

`Siblings()` controls adjacent page numbers, `Boundaries()` controls start/end pages. Smart edge case handling shows page numbers instead of ellipsis when appropriate. The sample app features synchronized pagination instances for easy comparison of different configurations.

### Improved Sidebar Layout Visual Design

Cleaner visual separators in SidebarLayout headers and footers create a more modern, streamlined appearance with reduced visual clutter.

### Hidden Apps with URI Navigation

Ivy now supports creating hidden applications that don't appear in the main navigation but can be accessed through URI-based navigation. This is useful for admin panels, developer tools, or features that should only be accessible through specific entry points.

**Creating a Hidden App:**

```csharp
[App(icon: Icons.EyeOff, isVisible: false)]
public class HiddenArgsApp : SampleBase
{
    protected override object? BuildSample()
    {
        var args = UseArgs<HiddenArgsAppArgs>();
        var navigator = this.UseNavigation();

        if (args != null)
        {
            return Layout.Vertical(
                Text.H2("Hidden App with Arguments"),
                Text.P($"Name: {args.Name}"),
                Text.P($"Value: {args.Value}"),
                new Button("Back").HandleClick(() => 
                {
                    navigator.Navigate("app://concepts/links");
                })
            );
        }

        return Text.P("This hidden app should only be accessed with arguments.");
    }
}
```

**Navigating to Hidden Apps via URI:**

```csharp
// Navigate using app URI scheme with arguments
navigator.Navigate("app://hidden/hidden-args", 
    new HiddenArgsAppArgs("John", 42));

// Navigate back using URI
navigator.Navigate("app://concepts/links");
```

The `isVisible: false` attribute hides apps from navigation menus while keeping them accessible via URI. Perfect for admin panels, developer tools, or role-based features.

### Namespace Reorganization

`Ivy.Alerts` and `Ivy.Builders` moved to `Ivy.Views` namespace:

```csharp
// Old
global using Ivy.Alerts;
global using Ivy.Builders;

// New
global using Ivy.Views.Alerts;
global using Ivy.Views.Builders;
```

Update your using statements to reflect the new structure.

### Namespace Transformation Utilities

New utility for changing namespaces across your codebase:

```csharp
// Change namespace in files
Utils.ChangeNamespace("MyFile.cs", "OldNamespace", "NewNamespace");
Utils.ChangeNamespace("MyProject.csproj", "OldNamespace", "NewNamespace");
```

Handles namespace declarations, using statements, qualified types, and project file elements. Powers automatic namespace renaming in project templates.

## Ivy CLI

### Enhanced Project Initialization with Templates

The `ivy init` command now supports project templates, allowing you to quickly scaffold new projects with pre-configured setups. You can select templates interactively or specify them directly via command line options.

**Using Templates with Init Command:**

```bash
# Initialize with a specific template
ivy init MyProject -t webapi

# Interactively select from available templates
ivy init MyProject --select-template

# Use the hello world template (shorthand)
ivy init MyProject --hello

# Initialize with prerelease version support
ivy init MyProject --prerelease
```

**Prerelease Version Support:**

Use `--prerelease` flag to initialize projects with prerelease Ivy versions:
- Configures NuGet to accept prerelease packages
- Uses version range `[1.0.0-*,999.0.0)` for latest builds
- Perfect for testing upcoming features

Templates automatically:
- Download from Ivy Templates repository
- Extract files to your project
- Update namespaces to match project name
- Rename project/solution files

**Template Management API:**

```csharp
// The TemplateManager service can fetch available templates
var templateManager = new TemplateManager();

// Get all available templates
var templates = await templateManager.GetTemplatesAsync();
foreach (var template in templates)
{
    Console.WriteLine($"Template: {template.Name} - {template.Url}");
}

// Get a specific template by name
var webApiTemplate = await templateManager.GetTemplateByName("WebApi");
if (webApiTemplate != null)
{
    // Use the template URL to download and scaffold your project
    Console.WriteLine($"Found template: {webApiTemplate.Url}");
}
```

Templates fetched from latest GitHub release. All `.zip` files in releases are recognized as available templates.

### Enhanced JWT Configuration in Basic Authentication

Configurable JWT issuer and audience settings via connection string:

```csharp
var connectionString = "JWT_ISSUER=my-app;JWT_AUDIENCE=my-api;JWT_SECRET=...;USERS=...";
```

Defaults to `ivy` (issuer) and `ivy-app` (audience) if not specified. Enables proper token scoping for multi-tenant scenarios.

### Improved `ivy fix` Command with Build Validation

Smart build validation before attempting fixes:
- Builds project first to check for errors
- Exits early if build succeeds
- Only requires authentication when fixes are needed
- More efficient for quick build checks

### New `--ignore-git` Option for CLI Commands

The Ivy CLI now supports a `--ignore-git` flag across multiple commands, allowing you to run operations without Git interactions. This is particularly useful in CI/CD environments or when working with projects outside of Git repositories.

The flag has been added to the following commands:
- `ivy connection add`
- `ivy app` (when generating apps)
- `ivy fix`

**Example usage:**

```bash
# Add a database connection without Git operations
ivy connection add --provider SqlServer \
  --connection-string "Server=localhost;Database=MyDb" \
  --name MyConnection \
  --ignore-git

# Generate an app without Git operations
ivy app -p "Create a user management system" \
  --connection MyConnection \
  --ignore-git

# Run fix command without Git operations
ivy fix --debug-agent-server http://localhost:5000 \
  --ignore-git
```

Provides flexibility for CI/CD environments and non-Git scenarios.

### Configurable Timeout for App Creation

The `ivy app` command now respects the global `--timeout` parameter when creating applications. This allows you to control how long the app generation process can run before timing out, which is especially useful for complex applications or slower network connections.

**Example usage:**

```bash
# Set a custom timeout of 5 minutes (300 seconds)
ivy app -p "Create a complex inventory management system" \
  --connection MyConnection \
  --timeout 300

# Use the default timeout (now 120 seconds)
ivy app -p "Create a simple CRUD app" \
  --connection MyConnection
```

Default timeout adjusted to 120 seconds (was 300). Override with `--timeout` for complex applications.

### Improved Database Generation with Provider Support

Database generation now includes provider information for optimized code generation. Automatically detects SQL Server, PostgreSQL, MySQL, etc., and generates provider-specific code.

### Simplified Release Process

The Ivy Framework now includes automated release creation with a new PowerShell script. This streamlines the process of publishing new versions of the framework and its packages.

**Creating a New Release:**

```bash
# Auto-increment version and create release
./CreateRelease.ps1

# Create a prerelease version
./CreateRelease.ps1 -Pre

# Specify a custom version
./CreateRelease.ps1 -Version "2.0.0"
```

Automated release features:
- Version detection from `Directory.Build.props` or Git tags
- Auto-increment version numbers
- Tagged repository releases
- Prerelease versions with timestamps

**Working with Prereleases:**

For testing upcoming features, you can now easily work with prerelease versions:

```bash
# Install prerelease version of Ivy CLI
dotnet tool install -g Ivy.Console --prerelease
```

In your projects, reference prerelease packages using wildcards:

```xml
<PackageReference Include="Ivy" Version="1.*-*" />
```

Enables faster iteration and testing across different release channels.

### Enum Entity Seeding Support for Database Generation

The database generation toolkit now includes utilities for automatically seeding enum-based entity tables. This feature simplifies the common pattern of creating database tables that mirror enum values, ensuring your database stays in sync with your application's enum definitions.

**Using the Enum Entity Generator:**

```csharp
// Define your enum
public enum UserRole
{
    [EnumMember(Value = "admin")]
    Administrator = 1,
    
    [EnumMember(Value = "user")]
    StandardUser = 2,
    
    [EnumMember(Value = "guest")]
    Guest = 3
}

// Create an entity that maps to the enum
public class UserRoleEntity : IEnumEntity<UserRole>
{
    public int Id { get; set; }
    public string DescriptionText { get; set; }
}

// Seed the database with enum values
var seedData = EnumEntity.Seed<UserRoleEntity, UserRole>(
    EnumMetadata.GetValues<UserRole>()
);
```

`EnumEntity.Seed` automatically:
- Maps enum values to entity IDs
- Extracts friendly names (underscores → dots)
- Preserves `EnumMember` attributes
- Supports metadata (descriptions, display order, resource types)

Ensures consistency between application enums and database lookup tables.

### Enhanced Database Generation Options

The database generator now includes two powerful new options for managing your database state during the generation process. These options provide more control over how your database is initialized, particularly useful during development and testing phases.

**New Command Line Options:**

```bash
# Delete existing database and create fresh
ivy generate --delete-database

# Seed database with initial data after generation
ivy generate --seed-database

# Combine both for a clean start with test data
ivy generate --delete-database --seed-database
```

`--delete-database`: Remove existing database for clean slate (use with caution)
`--seed-database`: Populate with initial data after generation

Integrates with `--yes-to-all` for CI/CD automation.

### Improved Update Check Reliability

Robust update checking that silently continues on failure:
- Network connectivity issues
- NuGet API downtime
- Parse errors

Works reliably in restricted or offline environments.

### Enhanced Session Logging with Version and OS Information

The CLI now captures additional diagnostic information in session logs, including the Ivy version and operating system details. This enhancement helps with debugging and support by providing crucial environment information alongside each logged command.

Session logs now capture:
- Ivy version with prerelease tags
- OS description and architecture

Helps diagnose version-specific and platform-specific issues.

### Improved Error Handling and Application Stability

The Ivy Framework server has been significantly hardened with comprehensive error handling to prevent application crashes from unhandled exceptions. This enhancement ensures your Ivy applications remain stable even when unexpected errors occur.

**Key improvements:**
- Connection error recovery with detailed logging
- Unobserved task exception handling
- Global exception handler with JSON error responses
- Domain exception monitoring
- `/health` endpoint for monitoring
- Safe app repository fallback for invalid IDs

Applications are now more resilient with graceful error handling and detailed diagnostics.

### Improved File Upload Handling

Modern FileReader API replaces legacy `btoa` for base64 conversion:
- Better binary data handling
- Efficient memory usage for large files
- Cleaner data URL extraction
- Cross-browser compatibility

### Direct File Upload Support

The FileInput widget now supports automatic file uploads to a server endpoint, enabling more efficient handling of large files without passing content through your Ivy application.

**Using the Upload URL Feature:**

```csharp
// Configure file input with an upload endpoint
var fileState = UseState<FileInput?>(null);
var uploadUrl = UseState<string?>("/api/upload");

return fileState.ToFileInput(uploadUrl)
    .Placeholder("Drop files here or click to select")
    .Multiple(true);

// Or set directly
new FileInput()
    .UploadUrl("/api/upload")
    .HandleChange(file => {
        // File has already been uploaded to the server
        Console.WriteLine($"File {file.Name} uploaded successfully");
    });
```

With upload URL configured:
- Automatic multipart/form-data upload on selection
- FileInput stores only metadata
- Direct server handling reduces memory usage
- Proper relative path resolution via `ivy-host` meta tag

Ideal for large files and separate upload processing.

### Automatic Port Discovery for Development

The Ivy Framework server now includes automatic port discovery functionality, making development smoother when your preferred port is already in use. This feature is especially helpful when running multiple Ivy applications simultaneously or when other services occupy common ports.

```csharp
// Automatic in DEBUG builds
var server = new Server(new ServerArgs 
{
    Port = 5000,
    FindAvailablePort = true  // Auto-enabled in DEBUG
});
```

Server automatically finds next available port (5001, 5002...) if requested port is in use. Checks up to 100 consecutive ports.

**Defaults:**
- Debug: Enabled for development convenience
- Release: Disabled for production predictability

Eliminates port conflicts during development.
