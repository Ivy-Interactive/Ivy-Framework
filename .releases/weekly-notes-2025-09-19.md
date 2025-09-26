# Ivy Framework Weekly Notes - Week of 2025-09-19

## Database Connection Improvements

### Connection Type Support

Database connections in Ivy now include explicit connection type information, making it easier to work with different database providers. When you add a database connection to your project, the generated connection class now includes a `GetConnectionType()` method that returns the specific Entity Framework provider being used.

```csharp
public class MyDatabaseConnection : IConnection
{
    public string GetConnectionType() => "EntityFramework.SqlServer";
    // or "EntityFramework.PostgreSQL", "EntityFramework.MySQL", etc.
}
```

This enhancement improves database provider detection and configuration, especially when working with multiple database types in the same project.

### PostgreSQL Connection String Handling

The Ivy CLI now provides more robust PostgreSQL connection string parsing with improved validation and error handling. It automatically handles both ADO.NET-style and URI-style connection strings, converting between formats as needed. This means you can use either format when setting up your PostgreSQL database connections:

```bash
# Both connection string formats now work seamlessly:
# URI format (supports both postgresql:// and postgres:// schemes)
postgresql://user:password@host:5432/database
postgres://user:password@host/database  # Port defaults to 5432

# ADO.NET format
Host=host;Port=5432;Database=database;Username=user;Password=password
```

The enhanced parser now:
- Validates that connection strings are properly formatted, providing clearer error messages that show the actual URI scheme when validation fails (e.g., "Unrecognized URI scheme 'http'" instead of showing the entire connection string)
- Correctly handles URL-encoded usernames and passwords without double-decoding issues
- Defaults to port 5432 when no port is specified in URI-style strings
- Validates that required fields (username, password, database name) are present
- Supports both `postgresql://` and `postgres://` URI schemes

The CLI ensures that essential parameters for Npgsql and Supabase compatibility are always present, automatically setting `SSL Mode=Require` when SSL is not configured or disabled. This improvement makes it easier to connect to various PostgreSQL providers, including Supabase, without worrying about connection string format differences or encoding issues.

### Database Generator Script Generation

The Database Generator now automatically includes the correct run script in the generated README file, making it easier to create and seed your database. When you generate database code, the README will include the exact command you need to run, complete with your specific database provider and connection string.

```bash
# Generated README now includes the exact command to run:
dotnet run -- --data-provider SqlServer --connection-string "your-connection-string" --seed-database
```

The generated scripts now properly format command line arguments with correct flag prefixes (`--data-provider` instead of `data-provider`) and ensure connection strings are properly quoted to handle spaces and special characters. Additionally, the generated Program.cs template now correctly returns the exit code from the async Main method, ensuring proper error propagation in CI/CD pipelines.

This enhancement removes the guesswork from running your generated database creation scripts - you can simply copy and run the command from the README to set up your database with the correct provider and connection settings.

## Breaking Change: Configuration Key Format Update

All configuration keys have been standardized to use colon-separated format instead of underscore-separated format, aligning with .NET configuration best practices. This is a **breaking change** that requires updating your local secrets.

### Migration Required

Update your configuration keys using the new format:

```bash
# Auth0 (OLD → NEW)
AUTH0_DOMAIN → Auth0:Domain
AUTH0_CLIENT_ID → Auth0:ClientId
AUTH0_CLIENT_SECRET → Auth0:ClientSecret
AUTH0_AUDIENCE → Auth0:Audience

# Supabase
SUPABASE_URL → Supabase:Url
SUPABASE_API_KEY → Supabase:ApiKey

# Microsoft Entra
MICROSOFT_ENTRA_TENANT_ID → MicrosoftEntra:TenantId
MICROSOFT_ENTRA_CLIENT_ID → MicrosoftEntra:ClientId
MICROSOFT_ENTRA_CLIENT_SECRET → MicrosoftEntra:ClientSecret

# Authelia
AUTHELIA_URL → Authelia:Url

# Basic Auth
USERS → BasicAuth:Users
JWT_SECRET → BasicAuth:JwtSecret
JWT_ISSUER → BasicAuth:JwtIssuer
JWT_AUDIENCE → BasicAuth:JwtAudience

# License
IVY_LICENSE → Ivy:License

# Sliplane Deployment
SLIPLANE_API_KEY → Sliplane:ApiKey
SLIPLANE_ORG_ID → Sliplane:OrgId
```

### Database Connection Strings

Database connection strings now use the connection name directly instead of the uppercase underscore format:

```csharp
// Old format
"MYDATABASE_CONNECTION_STRING"

// New format
"MyDatabase"
```

### Deployment Compatibility

The Sliplane deployment provider automatically handles the format conversion for environment variables. When deploying to Sliplane, colons in configuration keys are converted to double underscores (`__`) for environment variable compatibility. This means your deployed applications will continue to work without manual intervention.

### Benefits

- **Consistency**: Aligns with standard .NET configuration patterns
- **Better organization**: Hierarchical configuration structure is more intuitive
- **Improved IntelliSense**: Better IDE support for configuration sections
- **Future-proof**: Matches ASP.NET Core and modern .NET conventions

When you run any `ivy` command that sets up authentication or connections, the CLI will automatically use the new format and display the updated key names in the success messages.

## Secrets Management

### New Secrets Management Foundation

The Ivy Framework now includes a foundation for comprehensive secrets management with the introduction of the `IHaveSecrets` interface and `Secret` record. This infrastructure enables compile-time tracking of required application secrets, making it easier to validate that all necessary configuration is in place before deployment.

```csharp
// Implement the IHaveSecrets interface in your classes
public class MyService : IHaveSecrets
{
    public Secret[] GetSecrets()
    {
        return
        [
            new Secret("ApiKey"),
            new Secret("ConnectionString"),
            new Secret("OAuth:ClientSecret")
        ];
    }
}
```

The `Secret` record provides a simple way to declare required configuration keys that your application needs to function. This foundation will be expanded with additional features like descriptions, default values, and validators in future updates, enabling:
- Automated secret validation during deployment
- Clear documentation of configuration requirements
- Prevention of runtime failures due to missing secrets
- Integration with the Ivy CLI for secrets management

### Database Connections with Built-in Secrets Declaration

Database connections now automatically declare their required secrets, making it easier to validate that all connection strings are properly configured before deployment. When you generate a database connection using the Ivy CLI, the generated connection class now implements the `IHaveSecrets` interface:

```csharp
public class MyDatabaseConnection : IConnection, IHaveSecrets
{
    // ... existing connection methods ...

    public Secret[] GetSecrets()
    {
        return
        [
            new("ConnectionStrings:MyDatabase")
        ];
    }
}
```

This integration ensures that your database connection strings are automatically included in secrets validation, preventing deployment failures due to missing database configuration. The connection string secret name follows the new colon-separated format (`ConnectionStrings:ConnectionName`) for consistency with .NET configuration standards.

### Automated Secrets Tracking and Validation

The Ivy CLI now includes a comprehensive secrets management system that helps you track, validate, and ensure all required secrets are properly configured in your application. This new infrastructure prevents deployment failures due to missing configuration.

The secrets management system provides:
- **Automatic Secret Discovery**: Scans your compiled assemblies to find all required secrets
- **Validation Against User Secrets**: Verifies that all required secrets are configured in your local development environment
- **Automated Secrets.cs File Management**: Creates and maintains a `Secrets.cs` file that implements the `IHaveSecrets` interface
- **Command-Level Secret Tracking**: CLI commands that store secrets now automatically register them for tracking

#### Using the SecretsManager

The `SecretsManager` class helps you maintain a centralized `Secrets.cs` file in your project:

```csharp
// Add a new secret to your project's Secrets.cs file
SecretsManager.Add(projectDirectory, "MyApp", "ConnectionStrings:Database");

// The generated Secrets.cs file looks like:
public class Secrets : IHaveSecrets
{
    public Secret[] GetSecrets()
    {
        return
        [
            new("ConnectionStrings:Database"),
            new("Auth0:ClientId"),
            new("Stripe:ApiKey")
        ];
    }
}
```

The system automatically:
- Creates the `Secrets.cs` file if it doesn't exist
- Adds new secrets without duplicating existing ones
- Maintains proper formatting and namespace structure

#### Server Description for Application Introspection

Ivy applications now support a `--describe` flag that outputs a comprehensive server description in YAML format. This powerful introspection feature provides detailed information about your application's structure and requirements, making it invaluable for deployment, documentation, and configuration management.

```bash
# Get your application's complete server description
dotnet run -- --describe

# Output (YAML format):
# apps:
#   - name: Dashboard
#     id: dashboard
#     isVisible: true
#   - name: Settings
#     id: settings
#     isVisible: true
# connections:
#   - name: MainDatabase
#     connectionType: EntityFramework.SqlServer
#     namespace: MyApp.Data
#   - name: ExternalApi
#     connectionType: OpenAPI
#     namespace: MyApp.Services
# secrets:
#   - ConnectionStrings:Database
#   - Auth0:ClientId
#   - Stripe:ApiKey
# services:
#   - serviceType: IUserService
#     implementationType: UserService
#     lifetime: Scoped
```

The server description includes:
- **Apps**: All registered applications with their IDs and visibility settings
- **Connections**: Database and API connections with their types and namespaces
- **Secrets**: All required configuration keys discovered from IHaveSecrets implementations
- **Services**: Registered DI services with their lifetimes

This new approach:
- Eliminates dependency version conflicts that could occur with reflection-based discovery
- Provides a standard way for applications to declare their configuration requirements
- Works seamlessly with the CLI's secrets validation during deployment
- Makes it easier to understand and document your application's structure
- Enables automated deployment tooling to understand application requirements

#### Validating Secrets with SecretsReviewer

The `SecretsReviewer` class ensures all required secrets are configured by using the new server description feature:

```csharp
// Review and find missing secrets in your project
var missingSecrets = await SecretsReviewer.ReviewAsync(projectDirectory);
// Returns: ["ConnectionStrings:Database", "Auth0:ClientId"] if these are missing
```

The reviewer now:
- Runs your application with the `--describe` flag to get the server configuration
- Parses the YAML output to extract required secrets
- Compares required secrets against your configured user secrets
- Returns a list of any missing configuration keys

This simpler approach is more reliable and doesn't require complex assembly loading or reflection, making secrets validation faster and less error-prone.

This feature helps catch configuration issues early in development, preventing runtime failures when deploying your application.

#### Extended Secret Tracking for CLI Commands

The following CLI commands now automatically register their secrets with the SecretsManager when they store configuration:

- **OpenAPI Connections**: When adding an API connection, both `ConnectionName:HostUrl` and `ConnectionName:ApiKey` are tracked
- **Sliplane Deployment**: The `Sliplane:ApiKey` and `Sliplane:OrgId` are registered when configuring deployment
- **License Management**: The `Ivy:License` key is tracked when removing branding

This means that every time you use these commands to configure your application, the corresponding secrets are automatically added to your project's `Secrets.cs` file. You no longer need to manually track which configuration keys your application requires - the CLI handles this for you.

## CLI Improvements

### Enhanced Progress Reporting for App Creation

The `ivy create app` command now provides more detailed progress updates, showing exactly which files are being generated as the AI works on your project. Instead of just seeing a generic "working" message, you'll now see the specific file being created:

```bash
# Progress table now shows file-level details:
# "Creating LoginPage.cs..."
# "Creating UserService.cs..."
# "Creating AppDbContext.cs..."
```

This enhancement makes it easier to track what's happening during app generation, especially for larger projects with many components. The status updates automatically extract and display the filename from the generation messages, giving you real-time insight into the creation process.

### Token Usage Display

The Ivy CLI now displays real-time token usage when running commands. This helps you track resource consumption during AI-assisted development tasks. The token count appears in the progress table and status messages, updating as the AI processes your request.

```csharp
// Token counts now appear in the progress table
// Format: "Building components... (12,345)"
```

This enhancement provides better visibility into AI resource usage during `ivy create` and `ivy fix` commands.

### Improved Telemetry Control

The `ivy fix` command now properly respects the `--disable-telemetry` flag even when the `IVY_FIX_UPLOAD_TELEMETRY` environment variable is set. This gives you more granular control over telemetry data collection.

```bash
# Telemetry will be disabled even if IVY_FIX_UPLOAD_TELEMETRY is set
ivy fix --disable-telemetry
```

### Bug Fix: App Remove Command

Fixed an issue where the `ivy app remove` command was looking for an incorrect directory name. The command now correctly looks for the "Apps" directory instead of "App", ensuring that app removal operations work as expected.

## Deployment Enhancements

### Pre-deployment Secrets Validation

The Ivy CLI now automatically validates that all required secrets are configured before allowing deployment. This prevents deployment failures due to missing configuration and provides clear feedback about what needs to be set up.

When you run `ivy deploy`, the CLI will:
- Automatically scan your project for all required secrets
- Block deployment if any secrets are missing
- Display a clear list of missing configuration keys
- Provide guidance on how to configure them using `dotnet user-secrets`

```bash
# Example output when secrets are missing:
# Deployment Blocked: 3 required secret(s) are missing:
# - ConnectionStrings:Database
# - Auth0:ClientId
# - Stripe:ApiKey
# Please configure the missing secrets using 'dotnet user-secrets' before deploying.
```

This validation ensures deployments only proceed when all required configuration is in place, preventing runtime failures in production. Once all secrets are configured, you'll see a success message and deployment will continue normally.

### Interactive Service Naming

The `ivy deploy` command for Sliplane deployments now lets you choose a custom service name during deployment. This is especially useful when deploying multiple services to the same project.

```bash
# During deployment, you'll be prompted for a service name
ivy deploy --provider sliplane

# Enter Service Name: my-custom-service-name
```

The CLI validates that your service name:
- Starts and ends with alphanumeric characters
- Contains only letters, numbers, and hyphens
- Isn't already taken in your project

### Git Status Checking Before Deployment

The Ivy CLI now checks your Git repository status before deploying to Sliplane, helping you avoid accidentally deploying outdated code. When you run `ivy deploy`, the CLI will:

- Check for uncommitted or unstaged changes in your repository
- Count any commits that haven't been pushed to the remote repository
- Warn you if there are local changes that won't be included in the deployment

```bash
# Example warnings you might see:
# "You have uncommitted or unstaged changes."
# "You have 3 commits not pushed to remote."
# "Sliplane deploys from your public GitHub repository."

# You'll be prompted to continue or cancel the deployment
# Continue anyway? (Y/n)
```

Since Sliplane deploys directly from your public GitHub repository, this feature ensures you're aware when your local changes won't be included in the deployment. You can choose to continue anyway or push your changes first for a complete deployment.

### Automated Deployment Monitoring

Deployments to Sliplane now wait for completion automatically, providing real-time status updates. The CLI monitors your deployment for up to 5 minutes and displays:
- Current deployment status
- Completion notification when your app is live
- Error logs if deployment fails
- Your application's public URL once available

This eliminates the need to manually check the Sliplane dashboard for deployment status - you'll know immediately if your deployment succeeded and can access your app right away.

## Authentication Improvements

### Basic Auth Password Support Enhancement

The Basic Auth provider now supports passwords with special characters. Previously, passwords were restricted to only alphanumeric characters, which limited password complexity. The validation regex has been updated to allow any characters except colons (`:`) and semicolons (`;`), which are reserved as delimiters in the user string format.

```bash
# Now you can use complex passwords with special characters:
ivy auth basic --users "admin:P@ssw0rd!123;user:My$ecure#Pass"

# The only restrictions are:
# - No colons (:) - used to separate username from password
# - No semicolons (;) - used to separate user pairs
```

This enhancement improves security by allowing users to create stronger, more complex passwords while maintaining the simple `username:password;username:password` format for configuring multiple users.

## UI Component Enhancements


### Enhanced Semantic Colors

The framework now includes additional semantic color definitions to better support contextual UI elements. New color values have been added to the `Colors` enum to provide consistent theming across components:

```csharp
// New semantic colors available
Colors.Success   // For positive states and confirmations
Colors.Warning   // For cautionary states and alerts
Colors.Info      // For informational states and messages
```

These semantic colors work alongside existing colors like Primary, Secondary, and Destructive. They're integrated throughout the framework in components like badges, buttons, and other UI elements, providing:
- Consistent color semantics across your application
- Better visual communication of component states
- Improved accessibility through standardized color meanings
- Automatic theme adaptation for light and dark modes

### Enhanced Input Size Variants

Number inputs and sliders now feature improved size variants that provide better visual hierarchy and spacing in your forms. The three size options (Small, Default, and Large) have been refined with more distinct differences to better suit different UI contexts.

```csharp
// Use size variants on NumberInput widgets
var compactInput = new NumberInput().Small();    // Compact size for dense forms
var standardInput = new NumberInput();            // Default size for regular use
var prominentInput = new NumberInput().Large();   // Large size for important inputs

// Or use the Size method directly
var customSizedInput = new NumberInput().Size(Sizes.Large);
```

The improvements include:
- **Better text scaling**: Small uses `text-xs`, Default uses `text-base`, and Large uses `text-xl` for clearer hierarchy
- **Refined padding and heights**: More appropriate spacing for each size variant
- **Improved slider tooltips**: Better positioned and sized tooltips that match the slider size
- **Enhanced line height**: Small inputs use tight line spacing while large inputs use relaxed spacing for better readability

These refinements make it easier to create forms with clear visual hierarchy and improve the user experience when working with numeric inputs and sliders.

### Improved Form Field Layout Documentation

The `FormBuilder.Place` method documentation has been clarified to better explain how fields are arranged in forms. The updated documentation makes the distinction between vertical and horizontal placement more explicit:

```csharp
// Place fields vertically in a column (stacked)
form.Place(m => m.Street)                        // Single field in first column
    .Place(1, m => m.City, m => m.State);       // Two fields stacked in second column

// Place fields horizontally side-by-side (sharing row width)
form.Place(true, m => m.City, m => m.State)     // Two fields side-by-side
    .Place(true, m => m.ZipCode, m => m.Country); // Two more fields side-by-side
```

The key improvements in the documentation:
- **Clearer parameter descriptions**: The `row` parameter now explicitly states "True to arrange fields side-by-side in the same row; false to stack vertically"
- **Better method summaries**: Each overload clearly indicates whether fields will be placed vertically or horizontally
- **Consistent terminology**: Uses "side-by-side" for horizontal placement and "vertically/stacked" for vertical placement
- **Width distribution note**: When fields share a row, they're distributed evenly across the row width

Additionally, the frontend form field component now includes `flex-1 min-w-0` classes to ensure proper flex behavior when fields are arranged horizontally, preventing layout issues with long content.

### New Button Variants

The Button widget now includes three new contextual variants to help communicate different types of actions to users: **Success**, **Warning**, and **Info**. These variants complement the existing Primary, Secondary, Destructive, Outline, Ghost, and Link options.

```csharp
// New button variants for better semantic messaging
new Button("Confirm", handler, variant: ButtonVariant.Success);   // Green styling for positive actions
new Button("Caution", handler, variant: ButtonVariant.Warning);   // Yellow/orange for cautionary actions
new Button("Learn More", handler, variant: ButtonVariant.Info);   // Blue for informational actions
```

Each new variant comes with:
- Dedicated color theming that adapts to light/dark modes
- Consistent hover states with brightness adjustments
- Full support for disabled and loading states
- Icon placement on either side of the button text

The button styling has also been refined with tighter spacing between icons and text (gap reduced from 2 to 1) for a more compact, polished appearance. These enhancements make it easier to create intuitive interfaces where button colors clearly communicate the nature and importance of different actions.

### Expanded Badge Variants

The Badge widget has been enhanced with the same three new semantic variants as buttons: **Success**, **Warning**, and **Info**. These additions provide more contextual options for status indicators and labels in your UI.

```csharp
// New badge variants match button semantics for consistency
new Badge("Complete", variant: BadgeVariant.Success);   // Positive states and confirmations
new Badge("Attention", variant: BadgeVariant.Warning);  // Alerts and important notices
new Badge("Note", variant: BadgeVariant.Info);         // General information

// Or use the convenient extension methods
new Badge("Active").Success();
new Badge("Pending").Warning();
new Badge("Details").Info();

// Icon badges also support the new variants
new Badge(icon: Icons.CircleCheck, variant: BadgeVariant.Success);
new Badge(icon: Icons.CircleAlert, variant: BadgeVariant.Warning);
new Badge(icon: Icons.Info, variant: BadgeVariant.Info);
```

These new semantic badge variants provide:
- Consistent color theming with the corresponding button variants
- Clear visual communication of status and information types
- Better accessibility through semantic color usage
- Full support for both text and icon-only badges

The expanded badge variants make it easier to create comprehensive status systems and information hierarchies that are visually consistent across your entire application.

### Smart Blade Width Adaptation

The Blade widget now features intelligent width adaptation that automatically adjusts to its content. Instead of a fixed default width, blades now use `fit-content` sizing with smart constraints, providing a more responsive and content-aware layout.

```csharp
// Blade now automatically adapts to content width
new Blade(view, index, title: "Adaptive Blade");
// Width defaults to: Size.Fit().Min(Size.Units(120)).Max(Size.Units(300))

// The blade will:
// - Shrink to fit narrow content (minimum 120 units)
// - Expand for wider content like tables (maximum 300 units)
// - Automatically adjust when content changes

// You can still specify custom widths when needed
new Blade(view, index, title: "Fixed Width", width: Size.Units(400));
```

This enhancement is particularly useful for blades containing:
- **Tables with varying column counts**: The blade automatically expands to accommodate wide tables without horizontal scrolling
- **Forms with different field layouts**: Narrow forms stay compact while complex forms get more space
- **Dynamic content**: As content changes, the blade width adjusts accordingly

The new smart sizing provides a better user experience by eliminating unnecessary whitespace for simple content while ensuring complex content like wide tables remains readable without manual width adjustments. The maximum width has been increased from 240 to 300 units to better accommodate wider content like tables with many columns.

### Smarter Table Column Widths with Overflow Handling

Tables now feature improved handling of long content in cells, with automatic text truncation and tooltips for better readability. When table cells contain text that exceeds the available column width, the framework now:

- **Automatically truncates text with ellipsis**: Long text is clipped with "..." to maintain clean table layouts
- **Shows full content in tooltips**: Hover over truncated cells to see the complete text in a tooltip
- **Supports explicit column widths**: Use the `Width()` method on TableBuilder to control column proportions

```csharp
// Configure column widths using fractional sizing
products.ToTable()
    .Width(e => e.Sku, Size.Fraction(0.15f))      // 15% for SKU
    .Width(e => e.Name, Size.Fraction(0.3f))      // 30% for Name
    .Width(e => e.Price, Size.Fraction(0.15f))    // 15% for Price
    .Width(e => e.Description, Size.Fraction(0.4f)) // 40% for Description

// Long text in cells automatically gets truncated with tooltips
// For example: "This is a very long description..." [hover to see full text]
```

The improvements include:
- **Smart overflow detection**: Only cells with overflowing content get tooltips, reducing UI clutter
- **Preserved multiline support**: Cells marked with `MultiLine()` still wrap text normally without truncation
- **Better table layout**: Tables now use `table-layout: auto` for more intelligent column sizing
- **Responsive tooltips**: Tooltips display with proper formatting and word wrapping for readability

This enhancement is particularly useful when displaying:
- **Tables with long descriptions or file paths**: Content remains readable without breaking table layouts
- **Data grids with mixed content lengths**: Short and long content coexist cleanly
- **Headers with verbose column names**: Long column headers are truncated with full text in tooltips

The system works seamlessly with both table headers and data cells, ensuring consistent behavior throughout your tables.

## Audio Feedback

### Success Sound Notifications

The Ivy CLI now includes audio feedback functionality that can play a success sound after completing certain operations. This provides an audible notification when long-running tasks finish, which is especially helpful when you're multitasking or running commands in the background.

The audio playback system:
- Works cross-platform (Windows, macOS, and Linux)
- Plays a built-in success sound
- Runs asynchronously without blocking your workflow
- Automatically uses the appropriate audio player for your operating system
- Can be disabled with the `--silent` flag

```bash
# Run commands without audio notifications
ivy create app MyApp --silent
ivy fix --silent
```

The success sound helps you stay informed about task completion without needing to constantly watch the terminal output. If you prefer to work without audio notifications, simply add the `--silent` flag to any command.

## Form and Table Enhancements

### Improved Field Label Generation

The framework now includes a smarter field label generation system that creates more user-friendly labels from property names. The new `Utils.LabelFor` method provides intelligent label formatting that:

- **Removes 'At' suffix for date fields**: When a property name ends with 'At' and is a date type (DateTime, DateTimeOffset, or the newly supported DateOnly), the framework automatically removes the 'At' suffix for cleaner labels
- **Supports DateOnly type**: The date detection now includes support for DateOnly fields, treating them the same as DateTime for label generation purposes

```csharp
// Property names are now converted to cleaner labels:
public DateTime CreatedAt { get; set; }  // Label: "Created" (not "Created At")
public DateTime UpdatedAt { get; set; }  // Label: "Updated" (not "Updated At")
public DateOnly BirthDate { get; set; }  // Label: "Birth Date"
public DateTime LastLogin { get; set; }  // Label: "Last Login" (no 'At' to remove)
```

This enhancement affects all form builders, table builders, and detail views throughout your application:
- **FormBuilder**: Automatically generates cleaner labels for form fields
- **TableBuilder**: Creates better column headers in tables
- **DetailsBuilder**: Shows more readable field names in detail views
- **DateTimeInput**: Uses improved placeholders based on the field name

The label generation is consistent across all these components, ensuring a uniform and professional appearance in your application's UI without requiring manual label configuration for common date field patterns.

## Charts

### Enhanced Pie Chart Label Customization

The Pie Chart widget now supports multiple label layers with independent positioning and styling, allowing you to overlay different types of information on your charts. You can now add multiple `LabelList` configurations to a single pie chart, each with its own positioning, formatting, and styling options.

```csharp
new PieChart(data)
    .Pie(new Pie(nameof(PieChartData.Measure), nameof(PieChartData.Dimension))
        .InnerRadius("40%")  // Create a donut chart
        .OuterRadius("90%")
        // First label layer - values outside with currency formatting
        .LabelList(new LabelList(nameof(PieChartData.Measure))
            .Position(Positions.Outside)
            .Fill(Colors.Blue)
            .FontSize(11)
            .NumberFormat("$0,0"))
        // Second label layer - dimension names inside
        .LabelList(new LabelList(nameof(PieChartData.Dimension))
            .Position(Positions.Inside)
            .Fill(Colors.White)
            .FontSize(9)
            .FontFamily("Arial"))
    )
```

This enhancement enables more sophisticated chart labeling strategies:
- Display both values and percentages simultaneously
- Show dimension names inside the chart while keeping values outside
- Use different colors and fonts for different label types
- Create cleaner, more informative visualizations without cluttering

The feature is particularly useful for donut charts where the inner space can display one type of label while the outer area shows another, making complex data presentations more readable and professional.

## File System and Storage

### Volume Management for Application Data

Ivy now includes a standardized way to manage application data storage through the new `IVolume` interface and `FolderVolume` implementation. This provides a consistent approach to handling file paths and ensuring proper directory structure for your application's data files.

```csharp
// Configure a volume for your application
var volume = new FolderVolume("/data/myapp");
server.UseVolume(volume);

// Inject and use the volume in your services
public class FileService(IVolume volume)
{
    public void SaveUserData(string userId, byte[] data)
    {
        // Automatically creates the full path: /data/myapp/Ivy/YourAppName/users/123/profile.json
        var path = volume.GetAbsolutePath("users", userId, "profile.json");
        File.WriteAllBytes(path, data);
    }
}
```

The `FolderVolume` implementation provides:
- **Automatic directory creation**: Parent directories are created automatically when you request a path
- **Namespace isolation**: Files are organized under `Ivy/{YourAppName}/` to prevent conflicts between applications
- **Fallback to local app data**: If the configured root directory doesn't exist, it falls back to the system's local application data folder
- **Clean path composition**: Use params array for path parts instead of manual string concatenation

This is particularly useful for:
- Storing user uploads and generated files
- Managing application cache and temporary files
- Organizing configuration and data files in a consistent structure
- Deploying applications with predictable file storage locations

## Framework Stability

### Enhanced Error Handling and Logging

The Ivy Framework has improved its internal error handling and logging mechanisms for better stability and debugging. The AppHub and ClientNotifier components now feature:

- **Structured logging** throughout the connection lifecycle, making it easier to diagnose issues in production
- **Graceful error recovery** when clients disconnect unexpectedly or fail to connect
- **Better resilience** against unhandled exceptions that could previously crash the server process
- **Detailed error reporting** in verbose mode for development and debugging

These improvements ensure that your Ivy applications are more stable and provide better diagnostic information when issues occur, without requiring any changes to your existing code.

## API Changes

### Event Handler Extensions Deprecated

The `ToEventHandler` extension methods in the Event API have been marked as obsolete in favor of using the modern ValueTasks pattern. If you're using these methods in your code, you'll see deprecation warnings:

```csharp
// These methods are now deprecated:
Action<Event<MySender>> handler = myAction.ToEventHandler<MySender>();
Action<Event<MySender, int>> valueHandler = myValueAction.ToEventHandler<MySender, int>();

// Consider migrating to ValueTasks pattern for better async support and performance
```

This change encourages the use of more efficient async patterns in event handling, providing better performance and more flexible asynchronous operations in your Ivy applications.
