# Ivy Framework Weekly Notes - Week of 2026-03-26

## Features

### DataTable Footer Aggregates

Display aggregate calculations (sum, average, count, etc.) directly in your DataTable footers. The new `.Footer()` method lets you add calculated summaries at the bottom of columns:

```csharp
var invoiceLines = GetInvoiceLines();

invoiceLines.ToDataTable()
    .Header(x => x.Product, "Product / Service")
    .Header(x => x.Qty, "Quantity")
        .Footer(x => x.Qty, "Total", values => values.Sum())
    .Header(x => x.UnitPrice, "Unit Price")
        .Footer(x => x.UnitPrice, "Avg", values => values.Average())
    .Header(x => x.Amount, "Amount")
        .Footer(x => x.Amount, "Total", values => values.Sum())
    .Height(Size.Units(80))
```

**Multiple aggregates per column** are supported with an array syntax:

```csharp
salesData.ToDataTable()
    .Header(x => x.Sales, "Actual Sales")
        .Footer(x => x.Sales, new[]
        {
            ("Total", (Func<IEnumerable<decimal>, object>)(values => values.Sum())),
            ("Avg", (Func<IEnumerable<decimal>, object>)(values => values.Average()))
        })
```

When multiple aggregates are present, a dropdown selector appears in the footer cell, letting users switch between views. Footer widths automatically sync with column widths, and the footer scrolls horizontally along with the table content.

**Format numeric values** with the `.Format()` method to control how numbers display in both column cells and footers:

```csharp
salesData.ToDataTable()
    .Header(x => x.Revenue, "Revenue")
        .Format(x => x.Revenue, NumberFormatStyle.Currency, precision: 2, currency: "USD")
        .Footer(x => x.Revenue, "Total", values => values.Sum())
    .Header(x => x.GrowthRate, "Growth %")
        .Format(x => x.GrowthRate, NumberFormatStyle.Percent, precision: 1)
```

Available format styles: `Decimal`, `Currency`, `Accounting`, and `Percent`. Currency and Accounting formats default to USD if no currency is specified.

### URL Query Parameters Now Work with UseArgs

You can now pass arguments to your Ivy app directly through URL query parameters. Previously, `UseArgs<T>()` only worked with a pre-serialized `appArgs` query parameter. Now, individual query parameters are automatically collected and made available to your app.

```csharp
// Define your args model
public class MyArgs
{
    public string? NoteId { get; set; }
    public int? PageNumber { get; set; }
}

// In your app
var args = UseArgs<MyArgs>();
// Accessing: /myapp?noteId=abc123&pageNumber=5
// args.NoteId will be "abc123"
// args.PageNumber will be 5
```

Reserved parameters (`appId`, `machineId`, `parentId`, `shell`, `appArgs`, `oauthLogin`, `id`) are excluded from automatic arg parsing.

### New Skeleton Loading Variants

Added three new skeleton placeholder methods to improve loading UX in your apps. Previously, only `Skeleton.Card()` and `Skeleton.Form()` were available. Now you can also use:

```csharp
// Show a text skeleton with multiple lines
Skeleton.Text(lines: 3)  // Default is 3 lines, last line is shorter

// Show a data table skeleton with header and rows
Skeleton.DataTable(rows: 5)  // Default is 5 rows

// Show a feed/list skeleton with avatar and content
Skeleton.Feed(items: 3)  // Default is 3 items, includes avatar + text layout
```

**Best Practice Example with UseQuery:**

```csharp
var query = UseQuery("key", async ct => await FetchData(ct));

if (query.Loading)
    return Skeleton.DataTable(); // Or Skeleton.Text(), Skeleton.Feed(), etc.

if (query.Error is { } error)
    return Layout.Vertical()
        | Callout.Error($"Failed to load: {error.Message}")
        | new Button("Retry", _ => query.Mutator.Revalidate());

return new Card(query.Value);
```

Use these skeleton placeholders instead of plain "Loading..." text to maintain visual layout and provide better user experience during data fetching.

### Local File Images in Markdown

The Markdown widget can now display images from local file paths - useful for desktop applications or development scenarios where you need to reference files on disk. This feature requires a two-layer opt-in for security:

```csharp
// 1. Enable on the server in Program.cs
var server = new Server()
    .DangerouslyAllowLocalFiles();

// 2. Enable on the widget
var markdown = new Markdown("""
    # Documentation

    ![Screenshot](C:/Screenshots/example.png)
    ![Diagram](file:///Users/me/diagrams/architecture.png)
    """)
    .DangerouslyAllowLocalFiles();
```

**Supported file formats:** The local file endpoint now properly handles modern image formats (WebP, AVIF), video formats (WebM), markdown files (`.md`), and JSONL data files. Files are served with proper Content-Disposition headers so they display with their actual filename rather than a generic proxy URL.

**Supported path formats:**
- Forward slash paths: `C:/Screenshots/example.png`
- File URL protocol: `file:///Users/me/diagrams/architecture.png`

**Note:** Backslash paths (`C:\Screenshots\example.png`) don't work because the markdown parser interprets backslashes as escape characters. Always use forward slashes in your image paths, even on Windows.

**Important notes:**
- Images are served through a proxy endpoint (`/ivy/local-file`) rather than directly accessing `file://` URLs, which browsers block from HTTP pages
- Local file access is disabled by default. Only enable it in trusted contexts like desktop apps or development tools where displaying local images is the intended behavior. Don't enable this for web applications that render user-provided markdown.

### Programmatic Sheet Closing

Sheets can now be closed programmatically from within their content using a close callback. The `WithSheet` method now supports an overload where the content factory receives a close action, enabling forms and actions within sheets to automatically close after completion:

```csharp
var client = UseService<IClientProvider>();

return new Button("Add Item").WithSheet(
    close => {
        var itemName = UseState("");

        async ValueTask HandleSubmit()
        {
            await SaveItem(itemName.Value);
            client.Toast($"Item '{itemName.Value}' added!").Success();
            close(); // Close the sheet after successful submission
        }

        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                | new Button("Submit").OnClick(_ => HandleSubmit())
                | new Button("Cancel").Variant(ButtonVariant.Outline)
                    .OnClick(_ => close()),
            new Card(
                new TextInput("Item Name").Bind(itemName)
            ).Title("Item Details")
        );
    },
    title: "Add New Item",
    description: "Fill out the form below"
);
```

This pattern eliminates the need for manual sheet state management in common scenarios like form submissions, creating a smoother user experience where sheets dismiss automatically after successful actions.

### Toolbar Widget for Action Buttons and Controls

A new Toolbar widget provides a flexible way to organize action buttons, editor controls, and floating action bars. The widget uses `MenuItem` objects for toolbar items and supports grouping, separators, and active states:

```csharp
var client = UseService<IClientProvider>();

new Toolbar()
    | new MenuItem(Label: "Save", Icon: Icons.Save, Tag: "save")
        .OnSelect(_ => client.Toast("Saved!"))
    | new MenuItem(Label: "Undo", Icon: Icons.Undo, Tag: "undo")
        .OnSelect(_ => client.Toast("Undo"))
    | MenuItem.Separator()
    | MenuItem.Default(Icons.ZoomIn, tag: "zoom-in")
        .Tooltip("Zoom In")
        .OnSelect(_ => client.Toast("Zoom In"))
```

**Group related actions** using `MenuItem` with `MenuItemVariant.Group`:

```csharp
var isBold = UseState(false);
var isItalic = UseState(false);

new Toolbar(
    new MenuItem(
        Variant: MenuItemVariant.Group,
        Children: [
            MenuItem.Default(Icons.Bold, tag: "bold")
                .Tooltip("Bold")
                .Checked(isBold.Value)
                .OnSelect(_ => isBold.Set(!isBold.Value)),
            MenuItem.Default(Icons.Italic, tag: "italic")
                .Tooltip("Italic")
                .Checked(isItalic.Value)
                .OnSelect(_ => isItalic.Set(!isItalic.Value))
        ]
    ),
    MenuItem.Separator(),
    new MenuItem(
        Variant: MenuItemVariant.Group,
        Children: [
            MenuItem.Default(Icons.AlignLeft, tag: "left").Tooltip("Left"),
            MenuItem.Default(Icons.AlignCenter, tag: "center").Tooltip("Center")
        ]
    )
)
```

The Toolbar supports icon-only buttons (always provide tooltips for accessibility), active/checked state for toggle buttons, and disabled state for the entire toolbar or individual items. Perfect for text editors, image editors, or any interface that needs organized action buttons.

### Navigation Beacons for Type-Safe App Navigation

Navigate between apps without hard-coding app IDs using the Navigation Beacon system. Apps can advertise their ability to handle specific entity types, enabling dynamic discovery and type-safe contextual navigation.

**Register a beacon** using the `[NavigationBeacon]` attribute and a static factory method:

```csharp
public class Product { public int Id { get; set; } }

[App(icon: Icons.Package)]
[NavigationBeacon(typeof(Product), nameof(GetProductBeacon))]
public class ProductDetailsApp : ViewBase
{
    public static NavigationBeacon<Product> GetProductBeacon() => new(
        AppId: "product-details",
        ArgsBuilder: product => new { ProductId = product.Id }
    );

    public override object? Build()
    {
        var args = UseArgs<dynamic>();
        return Text.Heading($"Product #{args?.ProductId}");
    }
}
```

**Navigate using beacons** without knowing the target app's ID:

```csharp
public class ProductListApp : ViewBase
{
    public override object? Build()
    {
        var navigator = UseNavigation();
        var productBeacon = UseNavigationBeacon<Product>();

        var products = GetProducts();

        return Layout.Vertical(
            products.Select(product =>
                new Button($"View Product {product.Id}")
                    .OnClick(() => {
                        if (productBeacon != null)
                            navigator.Navigate(productBeacon, product);
                    })
            )
        );
    }
}
```

**Check beacon availability** before navigating:

```csharp
var productBeacon = UseNavigationBeacon<Product>();

if (productBeacon != null)
{
    // Product detail app is available
    navigator.Navigate(productBeacon, product);
}
else
{
    // No app handles Product entities
    UseToast().Show("Product details not available", ToastType.Warning);
}
```

**Key benefits:**
- Apps don't need to know each other's IDs
- Type-safe navigation with compile-time checking
- Dynamic discovery of navigation capabilities
- Graceful handling when target app is unavailable

**Important:** Only one app can register a beacon for each entity type. Use meaningful entity classes rather than reusing primitives.

## Breaking Changes

### AppContext.PathBase Renamed to AppContext.BasePath

The `AppContext.PathBase` property has been renamed to `AppContext.BasePath` to align with .NET naming conventions. If you're accessing this property in your code, update your references:

```csharp
// Before
var context = GetAppContext();
var path = context.PathBase;

// After
var context = GetAppContext();
var path = context.BasePath;
```

The CLI flag `--path-base` remains unchanged.

## Improvements

### Contextual Hints in Error Dialogs

Error dialogs now include helpful troubleshooting hints for common .NET exceptions. When an error occurs, Ivy automatically detects the exception type and displays an info callout with actionable guidance specific to that error:

```csharp
// When a NullReferenceException occurs in your app, users will see:
// Error message + helpful hint like:
// "An object reference was not set. In Ivy apps, common causes:
//  - Accessing UseState<T>().Value before it has been initialized
//  - Calling UseArgs<T>() when no args were passed to the view
//  - A UseQuery result accessed before loading completes (check .Loading first)
//  - A service not registered in Program.cs"
```

The framework provides contextual hints for common exceptions including `NullReferenceException`, `BadImageFormatException`, `FileNotFoundException`, `TypeLoadException`, `TimeoutException`, and `InvalidOperationException`. Each hint includes Ivy-specific causes and remediation steps, helping you debug issues faster without leaving your application.

### SelectInput Bulk Selection Actions

Multi-select inputs now support bulk actions with the `.ShowActions()` modifier. This adds "Select All" and "Clear All" buttons in the dropdown footer, making it easy to manage large lists:

```csharp
var languages = UseState<string[]>([]);
var options = new[] { "C#", "Java", "Python", "JavaScript", "Go", "Rust" }.ToOptions();

languages.ToSelectInput(options)
    .Variant(SelectInputVariant.Select)
    .ShowActions()
    .Searchable(true)
    .Placeholder("Select languages...")
```

The actions work with all multi-select variants (Select, List, Toggle) and respect your selection constraints:
- "Select All" is disabled when all visible options are already selected
- "Clear All" respects `MinSelections` constraints and won't clear below the minimum
- When searching, "Select All" only selects matching filtered options
- For nullable inputs, "Clear All" can clear to null when at zero selections

### RichText.Muted() Convenience Method

Added a new `.Muted()` method to `RichTextBuilder` for easily creating secondary/muted colored text. This convenience method saves you from manually specifying `Colors.Muted` each time:

```csharp
Text.Rich()
    .Text("Status: ")
    .Success("Active")
    .Text(" • ")
    .Muted("Last updated 5 minutes ago")
```

The method supports all standard text formatting options including `bold`, `italic`, `strikeThrough`, `highlightColor`, and `link`.

### Better List Spacing in Markdown Content

Markdown lists now render with improved vertical spacing between items and nested lists. Lists (`ul` and `ol` elements) automatically include consistent gaps between items, and nested lists within list items now have proper top margin spacing for better readability throughout your Ivy apps.

### Cleaner Code Block Comment Styling

Comments in code blocks now render in regular font instead of italic. This improves readability and provides a cleaner, more modern appearance for syntax-highlighted code throughout the framework.

### Smoother DataTable Refresh Experience

DataTables no longer flicker or show loading indicators during background data refreshes. After the initial load, subsequent refreshes happen seamlessly without resetting the view state, providing a much smoother user experience when working with live or frequently updated data.

### Better DataTable Row Action Button Alignment

Row action buttons in DataTables now align perfectly with their rows at any zoom level. The framework uses device pixel ratio and precise border calculations to ensure pixel-perfect positioning, eliminating any visual misalignment or jitter when hovering over rows with action buttons.

### Smarter DataTable Column Width Auto-Sizing

DataTable columns without explicit widths now auto-size based on header text length instead of defaulting to 150px. This prevents header text from being truncated with ellipsis, ensuring your column headers display fully. The framework estimates the minimum width needed (approximately 8 pixels per character plus padding for icons and sort indicators), with a minimum of 60px and a maximum auto-width of 300px to prevent excessively wide columns.

### Wider Dialog Layout

Dialogs now have a maximum width of 576px (up from 512px), providing more comfortable spacing for content and buttons. This gives your dialog layouts more breathing room without feeling cramped, especially when working with multiple buttons or form fields.

### BaseUrl Now Includes Base Path

`AppContext.BaseUrl` now automatically includes the base path when your app is deployed behind a reverse proxy with a path prefix. Previously, you needed to manually append the base path when constructing URLs. Now it's handled automatically:

```csharp
// When deployed at https://example.com/myapp/
var context = GetAppContext();
var url = context.BaseUrl;
// Returns: "https://example.com/myapp/" (includes trailing slash)
```

This simplifies URL construction for OAuth callbacks, webhooks, and shareable links in reverse proxy deployments.

### Semantic Colors for MenuItem

MenuItem now supports semantic color styles beyond the basic Default and Destructive options. You can now use Primary, Secondary, Success, Warning, and Info colors to better communicate the meaning and importance of menu items:

```csharp
DropDownMenu.Default()
    .Trigger(new Button("Actions"))
    .Items(
        MenuItem.Default("Save").Primary()
        | MenuItem.Default("Draft").Secondary()
        | MenuItem.Default("Publish").Success()
        | MenuItem.Default("Review").Warning()
        | MenuItem.Default("Info").Info()
        | MenuItem.Separator()
        | MenuItem.Default("Delete").Destructive()
    )
```

Each semantic color applies consistent styling that matches the rest of the Ivy design system, making it easier to create visually cohesive menus that clearly indicate the purpose of each action.

### Automatic Enum Formatting in SelectInput

Enums now display with readable, space-separated names in SelectInput dropdowns. PascalCase enum values are automatically formatted with spaces, so `BlueToRed` displays as "Blue To Red" and `LeastRecentlyUsed` displays as "Least Recently Used":

```csharp
// Define your enum
public enum CacheStrategy
{
    LeastRecentlyUsed,      // Displays as "Least Recently Used"
    LeastFrequentlyUsed,    // Displays as "Least Frequently Used"

    [Description("FIFO")]   // Custom display name
    FirstInFirstOut         // Without attribute: "First In First Out"
}

// Use in SelectInput - formatting happens automatically
var strategy = UseState(CacheStrategy.LeastRecentlyUsed);
return strategy.ToSelectInput()
    .WithField()
    .Label("Cache Strategy");
```

The formatting applies to all enums passed through `.ToOptions()`. You can override the automatic formatting by adding a `[Description]` attribute from `System.ComponentModel` to any enum value that needs precise control over its display name.

### Callout Density Control

Callouts now support three density levels to match your layout needs. Use `.Small()` for compact layouts, `.Medium()` (default) for standard spacing, or `.Large()` for high-impact messages:

```csharp
Layout.Vertical().Gap(4)
    | Callout.Info("Compact callout for tight layouts").Small()
    | Callout.Warning("Standard callout with default spacing").Medium()
    | Callout.Error("High-impact callout for critical messages").Large()
```

Density controls icon size (20px/24px/28px), padding, and vertical alignment automatically. Small callouts work well in sidebars or lists, while large callouts are ideal for prominent notifications or error states. Icons now align perfectly with text content across all densities, with refined spacing and line height for optimal readability.

### Ghost Variant for Expandable Widget

The Expandable widget now supports a Ghost variant for an ultra-minimal, understated appearance. Use `.Ghost()` to create expandables with no border, transparent background, and no shadow - perfect for secondary or less prominent collapsible sections:

```csharp
var notes = new Expandable("Additional Notes", notesContent)
    .Ghost();

// Works great with icons too
var advancedOptions = new Expandable("Advanced Settings", settingsContent)
    .Ghost()
    .Icon(Icons.Settings);
```

The Ghost variant reduces visual chrome to the absolute minimum (no border, transparent background, no shadow, minimal padding) while maintaining full expandable functionality. This provides the lightest possible visual weight that won't compete with primary content on your page.

### Horizontal Layouts Now Center Children by Default

Horizontal layouts (`Layout.Horizontal()`) now automatically vertically center their child elements. Previously, children would align to the top of the container. This new default provides better visual alignment when combining elements of different heights:

```csharp
// Text, badges, and buttons now align nicely without extra styling
Layout.Horizontal().Gap(2)
    | Text.Block("Premium Plan").Bold()
    | new Badge("Active").Variant(BadgeVariant.Success)
    | new Button("Edit", Icons.Pencil)
```

This applies when no explicit alignment is specified. You can still override this behavior using `.Align()` if needed.

### Automatic Frontend Build with `dotnet build`

If you're working with the Ivy Framework source code, `dotnet build` now automatically handles frontend compilation. MSBuild targets detect when frontend source files change and trigger incremental builds - no manual commands needed. This streamlines the development workflow when contributing to the framework.

### Vite+ CLI Migration

The Ivy Framework has migrated from `pnpm` to the Vite+ CLI (`vp`) for frontend tooling. Contributors working with the Ivy source code should install it globally with `npm install -g vite-plus`. Common commands: `vp install`, `vp dev`, `vp run build`, `vp test`. This change only affects framework contributors - if you're building apps with Ivy, your workflow remains unchanged.

### Concise Chart Creation with Array-Based API

For quick, pre-styled chart creation, `ToLineChart()` and `ToBarChart()` support a concise array-based syntax where you pass parameters directly instead of using the fluent API:

```csharp
var salesData = new[]
{
    new { Month = "Jan", Sales = 186 },
    new { Month = "Feb", Sales = 305 },
    new { Month = "Mar", Sales = 237 }
};

// ToLineChart(dimension, measures[], style?)
return salesData.ToLineChart(
    e => e.Month,
    [e => e.Sum(f => f.Sales)],
    LineChartStyles.Dashboard);
```

**Important:** The parameter order matters - the optional style parameter must come *after* the measures array, not before it. Use this syntax when you want pre-styled charts with minimal configuration. For custom styling and additional options like sorting or toolbox configuration, continue using the fluent API.

## Bug Fixes

### Fixed Asset Loading Behind Reverse Proxies

Ivy apps now load correctly when deployed behind a reverse proxy with a path prefix (e.g., `/test/studio/`). Previously, Vite would generate absolute asset paths (`/assets/...`) that didn't respect the base path, causing 404 errors. The Vite config now uses relative paths (`./`) which work correctly with the `BasePathFilter.cs` in all deployment contexts.

### Improved Focus/Blur Event Handling for SelectInput

The `SelectInput` widget now handles `OnFocus` and `OnBlur` events more accurately. These events now only fire when focus truly enters or leaves the select component, rather than when focus moves between internal elements. Additionally, disabled select inputs no longer fire these events.

### Fixed Shell-less Mode to Auto-Select First App

When using `?shell=false` to bypass the AppShell, Ivy now automatically selects the first visible app if no default app is configured. Previously, this would result in no app being loaded. The framework now intelligently selects the first available app based on order and title.

### Fixed Accessibility Warning in Sheet Components

All Sheet components now include the required `SheetDescription` element to satisfy accessibility requirements. Radix UI Dialog (which Sheet is built on) requires both Title and Description for proper accessibility compliance. Previously, sheets without a description would generate a browser console warning about a missing `aria-describedby` reference. The framework now always renders a SheetDescription element - either with the provided description text, or with a screen reader-only fallback ("Sheet content") to ensure accessibility compliance.

### Fixed UseDownload Method Overload Ambiguity

Resolved a compiler ambiguity error (CS0121) that could occur when calling `UseDownload` with synchronous factory methods. The framework now properly prioritizes the synchronous overloads, preventing ambiguous method call errors.

### Fixed SignalR Connection Parameter Handling

Ivy apps using SignalR now properly maintain their `BaseUrl` and `ProjectDirectory` properties. Previously, SignalR's internal `id` query parameter could interfere with app argument handling, causing these values to be empty.

### Fixed Decimal Value Rendering in DataTable Columns

DataTable columns displaying decimal and currency values now render correctly. Previously, decimal columns could display as long strings of zeros (e.g., "00000000000000000000") instead of properly formatted numbers (e.g., "129.99" or "$129.99"). The framework now properly handles Arrow Decimal128 objects by converting bigint and string representations to JavaScript numbers with correct decimal scaling.

### Fixed .NET 10 Compatibility Issue

Resolved a runtime error affecting .NET 10 applications where `FileNotFoundException` would be thrown for `Microsoft.Extensions.Configuration.Abstractions`. The framework now includes an explicit package reference to this assembly, ensuring it's available at runtime. Previously, Ivy relied on a transitive dependency that wasn't automatically copied to consuming applications in .NET 10.

### Fixed Callout Icon Alignment for Multi-line Content

The Callout widget icon now aligns to the top of the content instead of being vertically centered. This provides better visual consistency when callouts contain multiple lines of text, keeping the icon positioned with the first line rather than floating in the middle of longer messages. Additionally, callouts without titles now have pixel-perfect icon and text alignment across all density sizes (small, medium, large).

### Fixed SpacerWidget to Grow by Default

The `SpacerWidget` now properly grows to fill available space in flex layouts. Previously, spacers would not expand automatically, requiring manual sizing. Now spacers will naturally fill remaining space in horizontal and vertical layouts, making it easier to create balanced layouts and push elements apart without explicitly calculating dimensions.

### Fixed Charts with Non-String Dimension Values

Charts (Line, Area, Bar, Pie, Radar, Funnel) now render correctly when using non-string dimension values like integer years or dates. Previously, charts would display empty when the dimension column contained numeric values instead of strings:

```csharp
// This now works correctly - years as integers
var salesByYear = new[]
{
    new { Year = 2022, Revenue = 100000 },
    new { Year = 2023, Revenue = 150000 },
    new { Year = 2024, Revenue = 180000 }
};

salesByYear.ToLineChart()
    .Dimension(x => x.Year)
    .Value(x => x.Revenue, "Revenue");
```

The framework now intelligently detects dimension columns by prioritizing string-typed keys but falling back to the first available key when no string columns exist. All dimension values are automatically converted to strings for display, ensuring charts work with any data type in the dimension column.

### Fixed Case-Sensitive Matching in LineChart and BarChart Series

LineCharts and BarCharts now correctly display data when using explicitly configured series. Previously, charts would show axes but no data lines due to a case sensitivity mismatch - the JSON serializer camelCased data keys (e.g., "count") while the chart configuration preserved PascalCase measure names (e.g., "Count"). The framework now uses case-insensitive matching for dataKey configuration:

```csharp
// This now works correctly regardless of casing
data.ToLineChart()
    .Dimension(x => x.Month)
    .Value(x => x.Sales, "Sales")    // Works with "Sales", "sales", or any casing
    .Value(x => x.Target, "Target");  // Works with "Target", "target", or any casing
```

This fix aligns LineChart and BarChart behavior with other chart types (AreaChart, PieChart, FunnelChart) which already used case-insensitive matching.

### Fixed NumberInput Display with Programmatic Value Changes

NumberInput now correctly displays values when they are set programmatically from code. Previously, changing a NumberInput's value via state binding or code could result in malformed display values (e.g., "20,001,503,000" instead of "2000") because the internal display state wasn't syncing with external value changes. The component now properly updates its display formatting whenever the bound value changes while the input is not focused.

### Fixed Markdown OnLinkClick to Intercept All URLs

The Markdown widget's `OnLinkClick` handler now fires for all link clicks, including standard http/https URLs. Previously, `OnLinkClick` would only be called for non-standard URLs, while standard web links would open directly in the browser. Now when you register an `OnLinkClick` handler, it intercepts all link clicks, giving you complete control over link navigation:

```csharp
var markdown = new Markdown("""
    [Internal Page](/docs/guide)
    [External Site](https://example.com)
    """)
    .OnLinkClick(url => {
        // This now fires for both internal AND external URLs
        if (url.StartsWith("http"))
        {
            // Handle external links your way
            UseToast().Show($"Opening: {url}", ToastType.Info);
        }
        UseNavigation().NavigateTo(url);
    });
```

This fix ensures consistent behavior across all link types and allows you to implement custom routing, analytics tracking, or confirmation dialogs for external links.

### Fixed BarChart Gap Property Double-Percent Bug

BarCharts now correctly handle `barGap` and `barCategoryGap` properties when set to string values. Previously, the framework would unconditionally append '%' to gap values, causing string values like `"10%"` to become invalid `"10%%"` values. The framework now checks the value type - if numeric, it appends '%'; if already a string, it uses the value as-is. This ensures both numeric gap values (e.g., `10`) and pre-formatted string values (e.g., `"10%"`) work correctly.

### Fixed Empty CodeBlock Minimum Height

CodeBlock widgets now render with proper minimum height even when they contain no content. Previously, empty code blocks could collapse to zero height, making them invisible or difficult to interact with. The framework now calculates a minimum height based on font size, line height, and padding to ensure empty code blocks always display at least one line of visible space.

### Fixed Badge Widget Flexbox Layout

Badge widgets now render correctly in all layout contexts. Previously, badges used `inline-flex` with `w-min` (minimum width) constraints that could cause layout issues in certain flex containers. The framework now uses standard `flex` display without width constraints, ensuring badges layout properly within any parent container while maintaining their compact, content-hugging appearance.