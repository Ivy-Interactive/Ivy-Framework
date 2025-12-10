> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## Protected Namespace Validation

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

This change helps prevent potential naming conflicts and ensures your application code doesn't accidentally override framework components.

## JWT-Based Refresh Tokens for Basic Auth

The Basic Auth provider has been completely redesigned to use JWT-based refresh tokens instead of in-memory token storage. This enhancement provides better security, stateless operation, and improved user experience.

## Simplified Database Schema Selection

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

## UI Component Enhancements

### Input Size Variants

Many input widgets now support a `Size` property that allows you to choose between `Small`, `Medium`, and `Large` sizes. This provides greater flexibility in designing your application's UI by allowing you to adjust the size of input elements to better fit your layout and user experience needs.

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

### Enhanced Chart Legend Layout

The chart legend for donut and pie charts now has improved scrolling behavior when displaying many items.

### Terminal Widget Window Controls

The Terminal widget's window control buttons (the familiar red, yellow, and green circles) have been reordered to match macOS conventions. The controls now appear in the correct order: green (close/expand), yellow (minimize), and red (close) from left to right. This small but important change ensures the Terminal widget feels more native and familiar to users accustomed to standard window controls.

### SelectInput Enum Serialization Fix

The SelectInput widget now correctly handles enum serialization when working with enums that have `Description` attributes. Previously, when serializing enum values to JSON, the framework incorrectly sent the description text instead of the actual enum value name, causing mismatches between frontend and backend.

## Performance Optimizations

### Lazy Loading for Code Syntax Highlighting

The CodeWidget now uses lazy loading for the Prism syntax highlighter, significantly improving initial page load performance. Instead of bundling the entire syntax highlighting library upfront, it's now loaded only when a code block is actually rendered.

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

## What's Changed
* Revert "(ui/ux): (tabs): better align toggle button" by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/888
* (sidebar): remove borders around main content by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/889
* (inputs): resolve font size inconsistency between text input and text area by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/890
* chore: remove bg for currently active line by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/892
* (feat): implement Size for Select Input type by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/864
* [Feature]: VideoWidget by @id-pm in https://github.com/Ivy-Interactive/Ivy-Framework/pull/865
* (blade): smaller min default width; resolve issues for cards with details inside of min width blades by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/894
* (codex): use react swc by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/872
* [Docs] Sync docs with release v1.0.106 by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/895
* Add Language.Markdown support to CodeInputWidget by @nielsbosma in https://github.com/Ivy-Interactive/Ivy-Framework/pull/898
* Allow custom submit title in toForm() method by @ShogunFire in https://github.com/Ivy-Interactive/Ivy-Framework/pull/901
* Unwrap `AggregateException`s in `Error{Teaser}View` by @zachwolfe in https://github.com/Ivy-Interactive/Ivy-Framework/pull/904
* [Docs]: sync docs with Release v1.0.112 by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/905
* Fix: Take into account the width for all variants of the select input by @ShogunFire in https://github.com/Ivy-Interactive/Ivy-Framework/pull/903
* [Program]: test program docs by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/913
* (dbml): improve DBML editor table layout algorithm by @nielsbosma in https://github.com/Ivy-Interactive/Ivy-Framework/pull/902
* (html): removing the Rendered result part and fix security formating by @ShogunFire in https://github.com/Ivy-Interactive/Ivy-Framework/pull/915
* [Docs]: fix missing brackets in horizontal layout usage in forms by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/917
* [Docs]: add --silent flag to Ivy Init docs (missed from release sync) by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/923
* (audio): remove color from fonts and use regular Text.P by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/930
* (video): add missing XML documentation for poster parameter by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/929
* (image) Fix Image frontend to allow data uri src by @ShogunFire in https://github.com/Ivy-Interactive/Ivy-Framework/pull/934
* (feat): implement Size for File Input type by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/916
* [Docs]: implement Secret docs in new Concept section by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/925
* [Docs]: fix Secrets doc placement in sidebar with apps by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/936
* (forms): better auto-fill colors by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/928
* [DropDownMenu]: fix sub content (nested menu) overlaying of main content by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/906
* (fix): Use `MigrateAsync()` instead of `EnsureCreatedAsync()` by @zachwolfe in https://github.com/Ivy-Interactive/Ivy-Framework/pull/940
* (codex): fe unit tests support and workflows by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/943
* (docs): Fix incorrect Dockerfile in deployment docs by @zachwolfe in https://github.com/Ivy-Interactive/Ivy-Framework/pull/949
* (docs): Add docs for `ivy remove-branding` by @zachwolfe in https://github.com/Ivy-Interactive/Ivy-Framework/pull/947
* (emoji): add a custom emojis functionality in the markup renderer.  by @ShogunFire in https://github.com/Ivy-Interactive/Ivy-Framework/pull/918
* (feat): implement Size for Feedback Input type  by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/931
* [Docs]: implement Volume docs by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/937
* (git) Fix: In the precommit script, remove git add . by @ShogunFire in https://github.com/Ivy-Interactive/Ivy-Framework/pull/938
* (feat): Implement Size for DateTime Input type by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/945
* Reverse order of TerminalWidget stoplight buttons by @zachwolfe in https://github.com/Ivy-Interactive/Ivy-Framework/pull/948
* Add 2 new area charts, enhance pie charts with 3 additional examples, and fix chart label responsiveness by @Jeelislive in https://github.com/Ivy-Interactive/Ivy-Framework/pull/822
* (feat): fix bug on latest vite with missing packages on Linux by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/951
* (docs): fix UseMemo dependency array example by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/952
* (feat): Implement Size for Code Input type by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/955
* (fe): FE load performance: split heavy vendor chunks, lazy-load Prism, and polish Vite config Fixes : (#935) by @Jeelislive in https://github.com/Ivy-Interactive/Ivy-Framework/pull/956
* (feat): implement Size for Color Input type by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/946
* (rechart): fix support for react 19 by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/965
* (blade): scroll fix for radix scrollarea with nested elements by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/966
* (select): resolve value mismatch  by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/967
* feat: Add refresh token support for Basic Auth by @zachwolfe in https://github.com/Ivy-Interactive/Ivy-Framework/pull/950

## New Contributors
* @id-pm made their first contribution in https://github.com/Ivy-Interactive/Ivy-Framework/pull/865
* @ShogunFire made their first contribution in https://github.com/Ivy-Interactive/Ivy-Framework/pull/901

**Full Changelog**: https://github.com/Ivy-Interactive/Ivy-Framework/compare/v1.0.112...v1.0.114