# Ivy Framework Weekly Notes - Week of 2025-09-28

## CLI Project Initialization

### Protected Namespace Validation

The `ivy init` command now prevents you from accidentally using reserved namespaces that could conflict with the framework itself or its dependencies. The protected namespaces have been significantly expanded to include not just Ivy-specific namespaces, but also common .NET libraries and third-party packages used by the framework.

**Framework and System Namespaces:**
- Any namespace starting with `System.`, `Microsoft.`, `EntityFrameworkCore.`, or `Npgsql.`
- `Ivy` and any namespace starting with `Ivy.Auth.`
- Specific Ivy components: `Ivy.Airtable.EFCore`, `Ivy.Database.Generator.Toolkit`

**Third-Party Dependencies:**
- Popular libraries: `Newtonsoft.Json`, `YamlDotNet`, `Humanizer`, `Refit`
- Database providers: `Pomelo.EntityFrameworkCore.MySql`, `Oracle.EntityFrameworkCore`, `EFCore.Snowflake`, `Google.Cloud.EntityFrameworkCore.Spanner`
- Azure libraries: `Azure.Core`, `Azure.Identity`
- Other utilities: `DeepCloner`, `ExcelNumberFormat`, `SystemTextJson.JsonDiffPatch`, `Std.UriTemplate`, `Mono.TextTemplating`

If you attempt to use one of these namespaces, the CLI will display a helpful error message and prompt you to choose a different name. Additionally, if your current folder name would result in a reserved namespace, the CLI will automatically suggest "IvyApplication" instead as the default.

```bash
# If you're in a folder named "ivy" or "Ivy.Auth", the suggested namespace
# will be "IvyApplication" instead of the folder name
ivy init

# You'll see an error if you try to use a reserved namespace:
# "The namespace 'Ivy' is reserved by Ivy. Please choose a different namespace."

# Use --silent flag to disable the success sound
ivy init --silent
```

This change helps prevent potential naming conflicts and ensures your application code doesn't accidentally override framework components.

The CLI now plays a success sound when operations complete successfully. If you prefer to work without audio notifications, you can use the `--silent` flag to disable all sounds across CLI commands.

## CLI Remove Branding Command

### Remove Ivy Branding with Paid Subscriptions

The new `ivy remove-branding` command allows users with Ivy Pro or Team subscriptions to remove Ivy branding from their deployed projects. This command simplifies the process of configuring license tokens for white-label deployments.

```bash
# Remove branding (requires authentication and paid subscription)
ivy remove-branding

# Specify a custom project path
ivy remove-branding --project-path /path/to/your/project

# Enable verbose output for troubleshooting
ivy remove-branding --verbose
```

The command:
- Verifies your subscription status (Pro or Team required)
- Retrieves your license token from the Ivy billing service
- Configures the `Ivy:License` value in .NET user secrets
- Provides instructions for deployment configuration

When deploying your application, ensure the license configuration is included in your deployment environment:

```bash
# For manual deployments, set as environment variable
Ivy__License=your-unique-license-token

# Or as .NET user secret
Ivy:License=your-unique-license-token
```

When using `ivy deploy`, the license configuration is automatically transferred from your local user secrets to the deployment environment. For manual deployments, you'll need to configure the license token in your deployment platform's environment variables or secrets management system.

## Authentication Improvements

### JWT-Based Refresh Tokens for Basic Auth

The Basic Auth provider has been completely redesigned to use JWT-based refresh tokens instead of in-memory token storage. This enhancement provides better security, stateless operation, and improved user experience.

**Key improvements:**
- **JWT-based refresh tokens**: Both access and refresh tokens are now JWTs with proper validation and signing
- **Shorter access tokens**: Access tokens now expire in 15 minutes (reduced from 1 hour) for better security
- **24-hour refresh tokens**: Refresh tokens are valid for 24 hours with a maximum age of 365 days
- **Stateless operation**: No server-side token storage - all validation is done cryptographically
- **Session tracking**: Each refresh token includes a unique session ID and authentication timestamp
- **More accurate naming**: The `LoginAsync` method now uses `user` parameter instead of `email`, making it clearer that any username format is supported, not just email addresses

Here's how the improved authentication flow works:

```csharp
// When users log in, they receive both tokens as JWTs
var authToken = await authProvider.LoginAsync(user, password);
// authToken now contains:
// - JWT access token (expires in 15 minutes)
// - JWT refresh token (expires in 24 hours, max age 365 days)
// - Expiration time

// The refresh mechanism validates the JWT signature and checks:
// - Token hasn't expired
// - Max age hasn't been exceeded (365 days from initial auth)
// - Signature is valid
var newToken = await authProvider.RefreshJwtAsync(authToken);
```

The new implementation includes intelligent refresh logic - if the current access token is still valid, it returns it unchanged rather than generating a new one unnecessarily. This reduces server load and improves performance.

**Security benefits:**
- Refresh tokens include `auth_time` claim to track when authentication originally occurred
- Maximum age enforcement prevents indefinite token renewal
- Unique session IDs (`sid` claim) for each authentication session
- Different audience claims for access (`audience`) and refresh tokens (`oauth2/token`)
- All tokens are cryptographically signed and validated

## CLI Authentication Commands

### Connection String Format Updates

The `ivy auth add` command now uses a more consistent connection string format across all providers that aligns with .NET configuration conventions. The parameter names have been updated to use colon-separated namespace prefixes:

**Supabase:**
```bash
# New format with namespace prefixes
ivy auth add --provider Supabase --connection-string "Supabase:Url=https://your-project.supabase.co;Supabase:ApiKey=your-anon-key"

# Previous format (deprecated)
# --connection-string "SUPABASE_URL=https://your-project.supabase.co;SUPABASE_API_KEY=your-anon-key"
```

**Microsoft Entra:**
```bash
# New format with namespace prefixes
ivy auth add --provider MicrosoftEntra --connection-string "MicrosoftEntra:TenantId=your-tenant-id;MicrosoftEntra:ClientId=your-client-id;MicrosoftEntra:ClientSecret=your-client-secret"

# Previous format (deprecated)
# --connection-string "MICROSOFT_ENTRA_TENANT_ID=your-tenant-id;MICROSOFT_ENTRA_CLIENT_ID=your-client-id;MICROSOFT_ENTRA_CLIENT_SECRET=your-client-secret"
```

**Authelia:**
```bash
# New format with namespace prefixes
ivy auth add --provider Authelia --connection-string "Authelia:Url=https://auth.yourdomain.com"

# Previous format (deprecated)
# --connection-string "AUTHELIA_URL=https://auth.yourdomain.com"
```

**Auth0:**
```bash
# New format with namespace prefixes
ivy auth add --provider Auth0 --connection-string "Auth0:Domain=your-domain.auth0.com;Auth0:ClientId=your-client-id;Auth0:ClientSecret=your-client-secret;Auth0:Audience=your-api-identifier"

# Previous format (deprecated)
# --connection-string "AUTH0_DOMAIN=your-domain.auth0.com;AUTH0_CLIENT_ID=your-client-id;AUTH0_CLIENT_SECRET=your-client-secret;AUTH0_AUDIENCE=your-api-identifier"
```

This change provides better consistency with how other configuration values are structured in the Ivy Framework and makes it clearer that these are provider-specific configuration parameters.

## CLI Database Commands

### Simplified Database Schema Selection

The `ivy db add` command now includes a `--use-default-schema` parameter that automatically uses the database's default schema without prompting. This is particularly useful for automation scenarios or when you want to quickly set up a database connection using standard conventions.

```bash
# Automatically use the default schema (e.g., 'public' for PostgreSQL, 'dbo' for SQL Server)
ivy db add --provider postgres --connection-string "..." --name MyDb --use-default-schema
```

The default schemas for each database provider are:
- **PostgreSQL/Supabase**: `public`
- **SQL Server**: `dbo`
- **Oracle**: Uses the connected username as the default schema
- **ClickHouse**: `default`
- **Snowflake**: `PUBLIC`

Note that you cannot use both `--schema` and `--use-default-schema` parameters together. Choose one based on whether you want to specify a custom schema or use the database default.

## Updater Improvements

### Better High-DPI Display Support

The Ivy Updater now automatically scales its window size on high-DPI and retina displays. Previously, the updater window would appear too small on these displays, making it difficult to read. The updater now detects your system's DPI settings and adjusts accordingly:

- **Windows**: Uses Win32 GDI APIs for accurate DPI detection
- **macOS**: Detects retina displays via NSScreen's backingScaleFactor
- **Linux**: Supports both X11 DPI calculation and Wayland environment variables

The base 800x600 window automatically scales based on your display settings. For example, on a 2x retina display, the window will render at 1600x1200 pixels, ensuring text and UI elements remain crisp and properly sized.

This improvement requires no configuration and works automatically across all supported platforms.

### Improved Error Handling

The Ivy Updater now properly detects and reports when update operations fail. Previously, the updater could show a success message even when the underlying CLI commands failed. Now, if either the update or unpack operations encounter errors, you'll see a clear "Update Failed" message along with instructions on how to repair your installation:

```bash
# If the update fails, you'll be prompted to repair by running:
dotnet tool install -g Ivy.Console
```

This ensures you're always aware when an update doesn't complete successfully and know exactly how to fix it.

## Database Generator Enhancements

### Entity Framework Migrations Support

The Database Generator now automatically creates a `DesignFactory` class when generating database projects. This enables full support for Entity Framework Core migrations, which is particularly important when working with cloud databases like Supabase that require proper migration tooling.

The generated `DesignFactory` implements `IDesignTimeDbContextFactory<T>` and allows you to use standard EF Core migration commands:

```bash
# Now works out of the box with generated database projects
dotnet ef migrations add InitialCreate
dotnet ef database update
```

The factory automatically detects your database provider and creates the appropriate context configuration. This enhancement means you no longer need to manually configure design-time database contexts when using the Ivy Database Generator - everything is set up automatically.

Additionally, the generator now includes the `Microsoft.EntityFrameworkCore.Design` package by default, ensuring all necessary tooling is available immediately after generation.

### Automatic Initial Migration

The Database Generator now automatically creates an initial Entity Framework migration after generating your database project. This streamlines the setup process by eliminating a manual step that was previously required.

When you generate a new database project, the generator will automatically run:

```bash
dotnet ef migrations add InitialCreate
```

This creates the initial migration files that capture your database schema, ready to be applied to your database. The migration is created with the connection string you provided during generation, ensuring it uses the correct database context.

**Important**: The Database Generator now uses `MigrateAsync()` instead of `EnsureCreatedAsync()` when creating database tables. This change ensures that:
- Your database schema is created using the migrations system, maintaining full compatibility with future schema changes
- You can use Entity Framework migrations to evolve your database schema over time
- The database structure matches exactly what's defined in your migration files

This enhancement works seamlessly with the newly added `DesignFactory` support, providing a complete, migration-ready database project right out of the box.

### Enhanced SQLite Database Initialization

The Database Generator for SQLite projects now includes automatic database initialization and improved logging capabilities. When you generate a SQLite database project, the generated `DbContextFactory` now:

- **Automatic Database Creation**: The factory automatically creates the SQLite database file from a template on first use. This ensures your application always has a properly initialized database without manual setup.

- **Thread-Safe Initialization**: Uses semaphore locking to ensure the database is created safely even when multiple threads attempt to access it simultaneously.

- **Flexible Storage Locations**: Supports custom storage volumes through dependency injection, with a sensible default location in the user's local application data folder (`%LOCALAPPDATA%/Ivy-Data/{ProjectName}` on Windows).

- **Better Logging**: When verbose mode is enabled, Entity Framework logs are now properly routed through the application's logging infrastructure instead of directly to the console:

```csharp
// The generated factory now accepts optional volume and logger parameters
public MyDbContextFactory(
    ServerArgs args,
    IVolume? volume = null,
    ILogger? logger = null
)
```

This improvement makes SQLite-based projects more production-ready by handling database initialization automatically and providing better observability through proper logging integration.

## Application Debugging and Introspection

### New `--describe` Command

The Ivy Framework now includes a powerful new debugging command that provides a complete overview of your application's configuration. Running `dotnet run --describe` outputs a YAML-formatted description of your application's structure, making it easy to verify your setup and troubleshoot configuration issues.

```bash
dotnet run --describe
```

The command outputs comprehensive information about your application:

```yaml
apps:
  - name: Dashboard
    id: dashboard
    isVisible: true
  - name: Settings
    id: settings
    isVisible: true
connections:
  - name: MainDatabase
    connectionType: EntityFramework.SqlServer
    namespace: MyApp.Data
  - name: ExternalApi
    connectionType: OpenAPI
    namespace: MyApp.Services
secrets:
  - ConnectionStrings:Database
  - Auth0:ClientId
  - Stripe:ApiKey
services:
  - serviceType: IUserService
    implementationType: UserService
    lifetime: Scoped
```

This feature is particularly useful for:
- **Debugging Configuration Issues**: Quickly verify that all your apps, connections, and services are properly registered
- **Documentation**: Generate up-to-date documentation of your application's architecture
- **Deployment Verification**: Ensure your production deployments have the correct configuration
- **Team Collaboration**: Share application structure with team members without diving into code

The YAML format makes the output easy to read and can be piped to other tools for further processing or validation.

## UI Component Enhancements

### FeedbackInput Size Variants

The FeedbackInput widget now supports size variants (Small, Medium, and Large), providing better control over the visual prominence of star ratings, emoji ratings, and thumbs up/down feedback components.

```csharp
// Create feedback inputs with different sizes
var rating = UseState(0);

// Small size for compact layouts
rating.ToFeedbackInput(FeedbackInputs.StarRating)
    .Small();

// Medium size (default)
rating.ToFeedbackInput(FeedbackInputs.StarRating);

// Large size for prominent feedback areas
rating.ToFeedbackInput(FeedbackInputs.StarRating)
    .Large();

// Or set size explicitly
rating.ToFeedbackInput(FeedbackInputs.EmojiRating)
    .Size(Sizes.Large);
```

This enhancement applies to all feedback input variants:
- **Star Rating**: Adjusts star icon dimensions (Small: 16x16, Medium: 24x24, Large: 32x32)
- **Emoji Rating**: Scales emoji text size (Small: text-lg, Medium: text-2xl, Large: text-4xl)
- **Thumbs Rating**: Resizes thumb icons (Small: 16px, Medium: 24px, Large: 32px)

Use smaller sizes in tables or compact forms, and larger sizes for prominent feedback collection points where you want to encourage user interaction.

The sample application now includes a comprehensive demonstration of all size variants across the different feedback types, making it easy to see how each size looks with Stars, Emojis, and Thumbs variants.

### New Semantic Button Variants

The Button widget now includes three new semantic variants that provide better contextual communication for different types of actions: **Success**, **Warning**, and **Info**. These complement the existing Primary, Secondary, Destructive, Outline, Ghost, and Link button variants.

```csharp
// Using the new semantic button variants
Layout.Horizontal()
    | new Button("Success", variant: ButtonVariant.Success)
    | new Button("Warning", variant: ButtonVariant.Warning)
    | new Button("Info", variant: ButtonVariant.Info)
```

These semantic variants help users quickly understand the nature of an action:
- **Success**: Use for positive actions like saving, confirming, or completing operations
- **Warning**: Use for actions that require caution or may have significant consequences
- **Info**: Use for neutral informational actions or secondary operations

The semantic button variants work with all existing button features including icons, sizes, and states:

```csharp
// Semantic buttons with icons
new Button("Save Changes", variant: ButtonVariant.Success).Icon(Icons.Check)
new Button("Delete All", variant: ButtonVariant.Warning).Icon(Icons.Trash)
new Button("View Details", variant: ButtonVariant.Info).Icon(Icons.Info)

// Semantic buttons with different sizes
new Button("Confirm", variant: ButtonVariant.Success).Large()
new Button("Cancel", variant: ButtonVariant.Warning).Small()
```

### New Semantic Badge Variants

The Badge widget now includes semantic variants that match the new button variants: **Success**, **Warning**, and **Info**. These provide consistent visual language across your application's status indicators and labels.

```csharp
// Using the new semantic badge variants
Layout.Horizontal()
    | new Badge("Success", variant: BadgeVariant.Success)
    | new Badge("Warning", variant: BadgeVariant.Warning)
    | new Badge("Info", variant: BadgeVariant.Info)
```

These semantic badge variants complement the existing Primary, Destructive, Outline, and Secondary variants, making it easier to communicate status and context throughout your interface:

```csharp
// Status badges in a data table
new Badge("Active", variant: BadgeVariant.Success)
new Badge("Pending", variant: BadgeVariant.Warning)
new Badge("Draft", variant: BadgeVariant.Info)
new Badge("Cancelled", variant: BadgeVariant.Destructive)
```

### Enhanced Table Column Width Control

The Table widget now supports fractional (percentage-based) column widths, giving you more flexible control over table layouts. Additionally, long text in cells now automatically truncates with ellipsis and displays the full content in a tooltip on hover.

```csharp
// Set column widths as fractions of available space
products.ToTable()
    .Width(p => p.Sku, Size.Fraction(0.15f))          // 15% for SKU
    .Width(p => p.Name, Size.Fraction(0.25f))         // 25% for Name
    .Width(p => p.Price, Size.Fraction(0.15f))        // 15% for Price
    .Width(p => p.Url, Size.Fraction(0.2f))           // 20% for URL
    .Width(p => p.Description, Size.Fraction(0.25f))  // 25% for Description
```

The fractional widths work alongside existing fixed pixel widths (`Size.Units(100)`), allowing you to mix responsive and fixed column sizing:

```csharp
products.ToTable()
    .Width(p => p.Price, Size.Units(100))           // Fixed 100px width
    .Width(p => p.Name, Size.Fraction(0.3f))        // 30% of remaining space
    .Width(p => p.Description, Size.Fraction(0.7f)) // 70% of remaining space
```

When text content exceeds the column width, it automatically displays with an ellipsis (...). Users can hover over truncated cells to see the full content in a tooltip, ensuring data remains accessible even in constrained layouts.

### Simplified DropDownMenu Positioning

The DropDownMenu widget now handles positioning offsets automatically, removing the need for manual configuration of `SideOffset` and `SubSideOffset` properties. The framework now uses consistent CSS margin classes (`m-2` which equals 8px) for all dropdown positioning, resulting in cleaner code and more predictable layouts.

Previously, you might have customized dropdown positioning like this:
```csharp
// Previous approach (no longer supported)
new DropDownMenu(@evt => HandleSelection(@evt.Value),
    new Button("Options"),
    MenuItem.Default("Item 1"),
    MenuItem.Default("Item 2"))
    .SideOffset(15)      // Removed
    .SubSideOffset(10)   // Removed
    .AlignOffset(8);     // Still available
```

Now the framework handles these offsets automatically:
```csharp
// Simplified approach - offsets handled automatically via CSS margins
new DropDownMenu(@evt => HandleSelection(@evt.Value),
    new Button("Options"),
    MenuItem.Default("Item 1"),
    MenuItem.Default("Item 2"))
    .AlignOffset(8);  // Only alignment offset remains configurable
```

This change:
- **Reduces API complexity** by removing rarely-used configuration options
- **Improves consistency** with standard CSS margin classes (`m-2`) across all dropdowns
- **Simplifies frontend implementation** by using CSS classes instead of props, making styling more flexible and maintainable
- **Maintains flexibility** through the `AlignOffset` property for horizontal positioning adjustments

The positioning options that remain available are `Side` (Top, Right, Bottom, Left), `Align` (Start, Center, End), and `AlignOffset` for fine-tuning horizontal alignment when needed.

### New Semantic Colors

The Ivy Framework's color system has been expanded with three new semantic colors that provide more expressive options for status indicators and UI feedback:

- **`Colors.Success`**: A dedicated success color for positive feedback and successful operations
- **`Colors.Warning`**: A warning color for alerts and important notifications
- **`Colors.Info`**: An informational color for neutral status messages

These semantic colors automatically adapt to light and dark themes and meet WCAG accessibility standards. They replace the previous approach of using chromatic colors directly:

```csharp
// Previous approach (still works but not recommended)
new Box("Success").Color(Colors.Green)
new Box("Warning").Color(Colors.Amber)
new Box("Info").Color(Colors.Blue)

// New semantic approach (recommended)
new Box("Success").Color(Colors.Success)
new Box("Warning").Color(Colors.Warning)
new Box("Info").Color(Colors.Info)
```

The semantic colors integrate seamlessly with existing components:

```csharp
// Status indicators with semantic colors
Layout.Vertical(
    new Box("Operation completed").Color(Colors.Success),
    new Box("Low disk space").Color(Colors.Warning),
    new Box("System update available").Color(Colors.Info),
    new Box("Connection failed").Color(Colors.Destructive)
).Gap(5);
```

Using semantic colors instead of chromatic colors directly ensures consistency across your application and makes it easier to maintain a cohesive design system.

### Custom Emojis in Markdown

The Markdown renderer now supports custom emojis, allowing you to use branded or application-specific icons in your markdown content. The framework includes an Ivy-branded star emoji as an example, and you can easily add your own custom emojis.

```csharp
// Use custom emojis in markdown content
new Markdown(@"
Welcome to Ivy Framework :ivy-branded-star:
Build amazing apps with our tools!
");

// The Hello sample app now showcases this feature
Text.Markdown("You'd be a hero to us if you could :ivy-branded-star: us on [Github](https://github.com/Ivy-Interactive/Ivy-Framework)")
```

Custom emojis are defined using a simple mapping system and render as inline images that scale with your text:

```csharp
// The emoji system automatically replaces :emoji-name: patterns
// with corresponding images at the appropriate size
var content = "Check out our new feature :ivy-branded-star:";
```

The custom emoji system:
- **Automatic Parsing**: Detects and replaces emoji patterns (`:emoji-name:`) in markdown content
- **Inline Display**: Emojis render inline with text using appropriate vertical alignment
- **Scalable**: Emoji size adjusts based on the surrounding text (default 16px)
- **Extensible**: Add your own custom emojis by extending the emoji map
- **Multiple Emoji Support**: The parser now correctly handles multiple custom emoji types in the same content using proper OR separators in the regex pattern
- **XSS Protection**: Custom emoji HTML is automatically sanitized using DOMPurify to prevent cross-site scripting attacks, ensuring only safe SVG and image tags are rendered

To add your own custom emojis, place your SVG or image files in the public directory and register them in the emoji map. This feature is perfect for adding brand-specific icons, status indicators, or fun visual elements to your application's content.

### Data URI Support for Image Widget

The Image widget now supports data URIs as image sources, enabling you to embed images directly in your application without external file references. This is particularly useful for dynamically generated images, small icons, or when you need to display images without hosting them separately.

```csharp
// Use data URI for inline images
var dataUri = "data:image/png;base64,iVBORw0KGgoAAAANS...";
new Image(dataUri);

// Works alongside regular URLs
new Image("https://example.com/image.jpg");  // External URL
new Image("/images/logo.png");               // Local file
new Image(dataUri);                          // Data URI
```

This enhancement allows for:
- **Dynamic Image Generation**: Display images generated on-the-fly without saving to disk
- **Embedded Icons**: Include small icons directly in your code as base64
- **Offline Functionality**: Display images without requiring network access
- **Simplified Deployment**: No need to manage separate image files for small assets

### DateTimeInput Size Variants

The DateTimeInput widget now supports three size variants (Small, Medium, and Large) for better visual consistency and flexibility across different layouts. This enhancement applies to all DateTimeInput variants: Date, DateTime, and Time.

```csharp
// Create date/time inputs with different sizes
var dateState = UseState(() => DateOnly.FromDateTime(DateTime.Now));
var dateTimeState = UseState(() => DateTime.Now);
var timeState = UseState(() => TimeOnly.FromDateTime(DateTime.Now));

// Small size for compact layouts
dateState.ToDateTimeInput()
    .Variant(DateTimeInputs.Date)
    .Small()
    .Placeholder("Select date");

// Medium size (default)
dateTimeState.ToDateTimeInput()
    .Variant(DateTimeInputs.DateTime)
    .Placeholder("Select date and time");

// Large size for prominent date/time selection
timeState.ToDateTimeInput()
    .Variant(DateTimeInputs.Time)
    .Large()
    .Placeholder("Select time");

// You can also set size explicitly
dateState.ToDateTimeInput()
    .Size(Sizes.Large)
    .Placeholder("Select date");
```

The size variants adjust several visual aspects:
- **Small**: Compact height (32px), smaller icons (12x12), and smaller text (12px)
- **Medium**: Standard height (36px), medium icons (16x16), and regular text (14px)
- **Large**: Increased height (40px), larger icons (20x20), and larger text (16px)

The calendar component within the date picker now also scales appropriately with the size variant:
- **Calendar Cell Size**: Adjusts to match the input size (Small: 24px, Medium: 32px, Large: 40px cells)
- **Calendar Text**: Font sizes scale with the input size for better readability
- **Navigation Buttons**: Previous/next month buttons match the selected size

This enhancement ensures date and time inputs maintain consistent sizing with other form elements:

```csharp
Layout.Grid().Columns(3)
    | dateState.ToDateTimeInput().Small().Placeholder("Small date")
    | dateTimeState.ToDateTimeInput().Placeholder("Medium datetime")
    | timeState.ToDateTimeInput().Large().Placeholder("Large time");
```

The size variants work seamlessly across all date/time input types, including the calendar popover, time selection fields, and clear buttons, ensuring a cohesive visual experience throughout your forms.

### ColorInput Size Variants

The ColorInput widget now supports three size variants (Small, Medium, and Large) for better visual consistency across all input variants: Text, Picker, and TextAndPicker. The widget now defaults to `Medium` size when no size is specified, ensuring consistent behavior with other input components.

```csharp
// Create color inputs with different sizes
var colorState = UseState("#ff6b6b");

// Small size for compact layouts
colorState.ToColorInput()
    .Variant(ColorInputs.Text)
    .Small();

// Medium size (default)
colorState.ToColorInput()
    .Variant(ColorInputs.Picker);

// Large size for prominent color selection
colorState.ToColorInput()
    .Variant(ColorInputs.TextAndPicker)
    .Large();

// You can also set size explicitly
colorState.ToColorInput()
    .Size(Sizes.Large);
```

The size variants adjust several visual aspects:
- **Small**: Compact height (32px), smaller picker (32x32), and smaller text (12px)
- **Medium**: Standard height (36px), medium picker (40x40), and regular text (14px)
- **Large**: Increased height (40px), larger picker (48x48), and larger text (16px)

This enhancement ensures color inputs maintain consistent sizing with other form elements:

```csharp
Layout.Grid().Columns(3)
    | smallColor.ToColorInput().Variant(ColorInputs.Text).Small()
    | mediumColor.ToColorInput().Variant(ColorInputs.Picker)
    | largeColor.ToColorInput().Variant(ColorInputs.TextAndPicker).Large();
```

The size variants work seamlessly across all color input types (text field, color picker, or combined), ensuring a cohesive visual experience throughout your forms.

### CodeInput Size Variants

The CodeInput widget now supports three size variants (Small, Medium, and Large) for better visual hierarchy and layout flexibility. This enhancement allows you to adjust the font size of code snippets based on their importance and context.

```csharp
// Create code inputs with different sizes
var csharpCode = UseState(@"public class Example
{
    public string Name { get; set; }
}");

// Small size for compact code snippets
csharpCode.ToCodeInput()
    .Language(Languages.Csharp)
    .Small();  // 12px font size

// Medium size (default)
csharpCode.ToCodeInput()
    .Language(Languages.Csharp);  // 14px font size

// Large size for prominent code display
csharpCode.ToCodeInput()
    .Language(Languages.Csharp)
    .Large();  // 16px font size

// You can also set size explicitly
csharpCode.ToCodeInput()
    .Language(Languages.Json)
    .Size(Sizes.Large);
```

The size variants adjust the font size of the code text:
- **Small**: 12px (0.75rem) - Ideal for inline code references or compact layouts
- **Medium**: 14px (0.875rem) - Default size for standard code display
- **Large**: 16px (1.0rem) - Perfect for teaching materials or featured code examples

This enhancement works across all supported languages and integrates seamlessly with other CodeInput features:

```csharp
// Create a grid showing different sizes for various languages
Layout.Grid().Columns(3)
    | csharpCode.ToCodeInput().Language(Languages.Csharp).Small()
    | jsonCode.ToCodeInput().Language(Languages.Json)
    | sqlCode.ToCodeInput().Language(Languages.Sql).Large();

// Combine size with other features
htmlCode.ToCodeInput()
    .Language(Languages.Html)
    .Large()
    .ShowCopyButton(true)
    .ShowLineNumbers(true);
```

### FileInput Size Variants and Form Integration

The FileInput widget now supports three size variants (Small, Medium, and Large) for better visual hierarchy and layout flexibility. This enhancement makes it easier to match file inputs with other form elements and adjust their prominence based on context.

```csharp
// Create file inputs with different sizes
var fileState = UseState<FileInput?>(null);

// Small size for compact layouts
fileState.ToFileInput()
    .Small()
    .Placeholder("Upload document");

// Medium size (default)
fileState.ToFileInput()
    .Placeholder("Upload document");

// Large size for prominent file upload areas
fileState.ToFileInput()
    .Large()
    .Placeholder("Upload document");

// You can also set size explicitly
fileState.ToFileInput()
    .Size(Sizes.Large)
    .Placeholder("Upload document");
```

The size variants adjust several visual aspects:
- **Small**: Reduced padding (8px), smaller upload icon (16x16), and smaller text (12px)
- **Medium**: Standard padding (16px), medium upload icon (24x24), and regular text (14px)
- **Large**: Increased padding (24px), larger upload icon (32x32), and larger text (16px)

This makes it easy to create consistent file upload experiences that scale appropriately with your form design:

```csharp
Layout.Vertical()
    | Text.H2("Upload Files")
    | (Layout.Grid().Columns(3)
       | singleFile.ToFileInput().Small().Placeholder("Small file input")
       | multipleFiles.ToFileInput().Placeholder("Medium file input")
       | largeFile.ToFileInput().Large().Placeholder("Large file input")
    );
```

The FileInput widget now integrates beautifully with the Form builder, allowing you to use different sizes and configurations in form contexts:

```csharp
public record FileModel(
    FileInput? ProfilePhoto,
    FileInput? Document,
    FileInput? Certificate
);

var fileModel = UseState(() => new FileModel(null, null, null));

// Create a form with different file input sizes
fileModel.ToForm()
    .Builder(m => m.ProfilePhoto, s => s.ToFileInput().Large().Accept("image/*"))
    .Builder(m => m.Document, s => s.ToFileInput().Accept(".pdf,.doc,.docx"))
    .Builder(m => m.Certificate, s => s.ToFileInput().Small().Accept(".pdf"))
    .Label(m => m.ProfilePhoto, "Profile Photo")
    .Label(m => m.Document, "Document")
    .Label(m => m.Certificate, "Certificate")
    .Description(m => m.ProfilePhoto, "Upload your profile picture")
    .Description(m => m.Document, "Upload your resume or document")
    .Description(m => m.Certificate, "Upload certificate (optional)")
    .Required(m => m.ProfilePhoto, m => m.Document);
```

This form integration allows you to create sophisticated file upload forms with proper validation, descriptions, and visual hierarchy using the different size variants.

## Chart Enhancements

### Pie Chart Label Lists

The PieChart widget now supports label lists for displaying text directly on chart segments. This allows you to show values, labels, or both inside or outside the pie slices, making charts more informative without requiring users to hover for tooltips.

```csharp
new PieChart(data)
    .Pie(new Pie("Share", "Browser")
        .OuterRadius(150)
        .InnerRadius(90)
        // Display percentage values outside the pie
        .LabelList(new LabelList("Share")
            .Position(Positions.Outside)
            .Fill(Colors.Black)
            .FontSize(12)
            .NumberFormat("0%"))
        // Display browser names inside the donut
        .LabelList(new LabelList("Browser")
            .Position(Positions.Inside)
            .Fill(Colors.White)
            .FontSize(10)
            .FontFamily("Arial")))
```

Label lists support full customization:
- **Position**: Place labels inside or outside the segments
- **Formatting**: Use number formats for percentages, currency, or custom formats
- **Styling**: Control font size, family, and color
- **Multiple Labels**: Add both value and category labels to the same chart

This enhancement makes pie and donut charts more readable, especially when presenting data to stakeholders who prefer seeing values directly on the visualization.

## Volume Management for Application Data

### Standardized File Storage with IVolume

The Ivy Framework now provides a comprehensive volume management system through the `IVolume` interface and `FolderVolume` implementation. This feature standardizes how applications handle file storage, ensuring consistent path management and proper isolation between different applications.

Key features of the Volume Management system:

- **Automatic Directory Creation**: Parent directories are created automatically when requesting file paths
- **Namespace Isolation**: Files are organized under `Ivy/{YourAppName}/` to prevent conflicts between applications
- **Intelligent Fallback**: If the configured root directory doesn't exist, the system automatically falls back to the local application data folder
- **Clean Path Composition**: Use params arrays for path parts instead of manual string concatenation
- **Full DI Support**: Volumes are registered as services and can be injected throughout your application

### Basic Usage

Configure a volume during server startup:

```csharp
using Ivy.Services;

var server = new Server();

// Configure a volume for your application
var volume = new FolderVolume("/data/myapp");
server.UseVolume(volume);

await server.RunAsync();
```

Use the volume in your services or views:

```csharp
public class FileService(IVolume volume)
{
    public void SaveUserData(string userId, byte[] data)
    {
        // Creates: /data/myapp/Ivy/YourAppName/users/123/profile.json
        var path = volume.GetAbsolutePath("users", userId, "profile.json");
        File.WriteAllBytes(path, data);
    }

    public byte[] LoadUserData(string userId)
    {
        var path = volume.GetAbsolutePath("users", userId, "profile.json");
        return File.ReadAllBytes(path);
    }
}
```

### Path Structure and Fallback

The `FolderVolume` creates paths following this structure:
```
{root}/Ivy/{YourAppName}/{pathParts}
```

If the configured root directory doesn't exist, the system automatically falls back to platform-specific locations:
- **Windows**: `%LOCALAPPDATA%`
- **macOS**: `~/Library/Application Support`
- **Linux**: `~/.local/share`

### Integration with Upload and Download Services

Volumes integrate seamlessly with Ivy's upload and download services:

```csharp
public class FileHandler(IVolume volume, IUploadService uploadService)
{
    public void SetupFileUpload()
    {
        var (cleanup, url) = uploadService.AddUpload(
            async (fileData) =>
            {
                var path = volume.GetAbsolutePath("uploads", "user-file.dat");
                await File.WriteAllBytesAsync(path, fileData);
            },
            "application/octet-stream",
            "uploaded-file.dat"
        );
    }
}
```

This volume management system ensures your application data is properly organized, isolated from other applications, and accessible through a clean, consistent API.

## Chart Improvements

### Enhanced Chart Legend Layout

The chart legend for donut and pie charts now has improved scrolling behavior when displaying many items. The legend items are properly configured with:

- **Consistent spacing**: Fixed padding and gaps between legend items ensure proper alignment
- **Non-wrapping labels**: Legend text no longer wraps unexpectedly, maintaining clean horizontal layouts
- **Better overflow handling**: When there are many legend items displayed in two rows, both rows now scroll together horizontally as a unit, preventing misalignment between the top and bottom rows
- **Flexible wrapping**: For smaller legend sets, items wrap naturally without forcing horizontal scrolling

This fix is particularly noticeable in dashboard views with donut charts containing numerous categories, where the legend previously could become misaligned or difficult to navigate.

## Advanced Filter Parsing for Data Tables

### New Filter Parsing Library

The Ivy Framework now includes a new `Ivy.Filters` library that provides advanced filter parsing capabilities for data tables. This feature enables users to write complex filter expressions using a natural, SQL-like syntax that gets parsed and validated before being applied to your data.

The filter parser supports:

- **Column References**: Use square brackets to reference columns by their display names: `[Age]`, `[Country]`, `[Start Date]`
- **Logical Operations**: Combine filters with `AND`, `OR`, and parentheses for complex logic
- **Text Operators**: `contains`, `not contains`, `equals`, `not equal`, `starts with`, `ends with`, `blank`, `not blank`
- **Number Operators**: `=`, `!=`, `>`, `>=`, `<`, `<=`, plus word forms like `greater than`, `less than or equal`
- **Range Operations**: `in range` for both numbers and dates: `[Age] in range 18 AND 30`
- **Date Comparisons**: `before`, `after`, `equals` with string date inputs

Example usage:
```csharp
using Ivy.Filters;

var columns = new[]
{
    new FieldMeta("Age", "age", FieldType.Number),
    new FieldMeta("Country", "country", FieldType.Text),
    new FieldMeta("Is Premium", "isPremium", FieldType.Boolean)
};

var parser = new FilterParser(columns);
var result = parser.Parse("[Age] > 23 AND [Country] contains \"united\"");

// Use the parsed result to filter your data
if (!result.HasErrors)
{
    // Apply the filter to your data source
}
```

Example filter expressions:
```
[Age] > 23 AND [Country] contains "united"
([Sport] ends with "ing" OR [Age] < 30) AND [Active] = true
[Price] in range 10 AND 100
[Start Date] in range "2024-01-01" AND "2024-12-31"
[Is Premium] = true
```

The parser validates expressions against your column schema, ensuring type safety (e.g., preventing text operations on numeric columns) and providing detailed error messages for invalid syntax or semantics. This feature integrates seamlessly with AG Grid's advanced filter model, making it easy to build powerful, user-friendly data filtering interfaces.

## Deployment Documentation Updates

### .NET 9.0 Docker Configuration

The deployment documentation has been updated to use .NET 9.0 runtime and SDK images in the generated Dockerfiles. This ensures your deployments use the latest .NET version with improved performance and features. The Dockerfile structure has also been enhanced with:

- **Build configuration arguments**: Use `ARG BUILD_CONFIGURATION=Release` for flexible build configurations
- **Improved layering**: Better separation between restore, build, and publish stages for optimized Docker layer caching
- **Explicit environment setup**: Clear configuration of `PORT` and `ASPNETCORE_URLS` environment variables
- **Enhanced publish settings**: Added `/p:UseAppHost=true` flag for better executable generation

Example of the updated Dockerfile structure:
```dockerfile
# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Build stage with configurable build type
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
# ... build steps ...

# Final runtime image with environment configuration
ENV PORT=80
ENV ASPNETCORE_URLS="http://+:80"
ENTRYPOINT ["dotnet","./YourProject.dll"]
```

This update ensures your Ivy applications are deployed with the latest .NET runtime optimizations and follow Docker best practices for production deployments.

## UI Component Refinements

### Terminal Widget Window Controls

The Terminal widget's window control buttons (the familiar red, yellow, and green circles) have been reordered to match macOS conventions. The controls now appear in the correct order: green (close/expand), yellow (minimize), and red (close) from left to right. This small but important change ensures the Terminal widget feels more native and familiar to users accustomed to standard window controls.

### SelectInput Enum Serialization Fix

The SelectInput widget now correctly handles enum serialization when working with enums that have `Description` attributes. Previously, when serializing enum values to JSON, the framework incorrectly sent the description text instead of the actual enum value name, causing mismatches between frontend and backend.

For example, with an enum like:
```csharp
public enum DatabaseNamingConvention
{
    [Description("PascalCase")]
    PascalCase,
    [Description("snake_case")]
    SnakeCase
}
```

The framework would previously serialize `SnakeCase` as `"snake_case"` (the description), but the frontend expected `"SnakeCase"` (the enum name). This mismatch caused "value not found" errors when the frontend sent selections back to the backend.

The fix ensures that:
- Enum values are serialized using their actual enum names (e.g., `"SnakeCase"`)
- The `JsonEnumConverter` maintains backward compatibility by accepting both enum names and descriptions during deserialization
- SelectInput widgets now work correctly with enums that have underscores or special characters in their descriptions

This fix is particularly important for database-related enums where naming conventions like `snake_case` are commonly used in descriptions while maintaining proper C# enum naming conventions.

## Performance Optimizations

### Lazy Loading for Code Syntax Highlighting

The CodeWidget now uses lazy loading for the Prism syntax highlighter, significantly improving initial page load performance. Instead of bundling the entire syntax highlighting library upfront, it's now loaded only when a code block is actually rendered.

```jsx
// The syntax highlighter is now loaded on-demand
const SyntaxHighlighter = lazy(() =>
  import('react-syntax-highlighter').then(mod => ({ default: mod.Prism }))
);

// Users see a simple preformatted fallback while the highlighter loads
<Suspense fallback={<pre className="p-4 bg-muted rounded-md font-mono text-sm">{content}</pre>}>
  <SyntaxHighlighter language={language} style={theme}>
    {content}
  </SyntaxHighlighter>
</Suspense>
```

This change reduces the initial JavaScript bundle size and improves time-to-interactive for pages without code blocks.

### Optimized Build Configuration

The frontend build process has been significantly optimized with intelligent code splitting and vendor chunking. Libraries are now grouped into logical chunks based on their functionality:

- **vendor-react**: React and Radix UI components
- **vendor-codemirror**: Code editor components
- **vendor-markdown**: Markdown processing libraries
- **vendor-mermaid**: Diagram rendering
- **vendor-charts**: Recharts and D3 visualization libraries
- **vendor-reactflow**: Flow diagram components
- **vendor-motion**: Framer Motion animations
- **vendor-katex**: Math formula rendering

This granular chunking strategy:
- Reduces initial page load by loading only required chunks
- Improves browser caching as vendor chunks change infrequently
- Enables parallel downloading of independent features

Additionally, the build now:
- Uses ES2020 target for modern JavaScript features
- Removes console statements and debuggers in production builds
- Aliases `lodash` to `lodash-es` for better tree-shaking
- Disables source maps in production for smaller bundle sizes