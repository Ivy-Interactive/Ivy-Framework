# Ivy Framework Weekly Notes - Week of 2025-11-21

## CLI Improvements

### Headless Database Generation

The `ivy db generate` command now supports a console mode with the `--use-console` flag, enabling headless database generation without launching the UI.

**Requirements:**

- Must provide either `--prompt`, `--dbml`, or input via STDIN
- Must use `--yes-to-all` (no interactive prompts)
- Must use `--skip-debug` (no UI debugging)

### Automatic EF Migration Creation

The `ivy db generate` command now automatically creates an Entity Framework migration after building the database generator project.

### Database Generation Bug Fix

Fixed an issue where package references were being lost during database generation. The generator now properly preserves NuGet package references (like `Ivy.Database.Generator.Toolkit` and `Microsoft.EntityFrameworkCore.Design`) when creating the database generator project files, preventing build errors in generated projects.

### Improved Error Reporting

The `ivy db generate` command now provides better error handling and specific exit codes for different failure scenarios.

**Exit codes:**

- `20`: DBML validation error
- `30`: Database generator build error
- `40`: EF migration error
- `50`: Database generator run error
- `60`: Project build error

### Dashboard App No Longer Auto-Generated

The database generator no longer automatically adds a Dashboard app to your generated applications. Previously, when converting DBML to an app plan, the generator would always inject a dashboard app alongside the apps suggested by the AI agent. Now, only the apps explicitly returned by the agent are included in the generation plan..

### Custom AI Model Selection

You can now specify which AI model to use during database generation with the new `--model-id` argument.

**Usage:**

```bash
# Use a specific model for database generation
ivy db generate --model-id claude-3-5-sonnet-20241022
```

### Model Cache Control

For advanced scenarios, you can now control whether the AI model uses caching during generation and debugging with the `--model-disable-cache` flag. This hidden option is available in `ivy db generate`, `ivy app create`, and `ivy fix` commands.

### Parallel App Generation

The `ivy db generate` command now generates apps concurrently, significantly improving performance when generating multiple apps from your database schema.

### Better CLI Exit Codes for App Creation Failures

Both `ivy app create` and `ivy db generate` commands now properly return non-zero exit codes when app generation fails.

- `ivy app create` returns exit code `1` if any app fails to generate
- `ivy db generate` returns exit code `55` if any app fails to generate

## Framework Improvements

### Reactive State Support for Button Loading

Buttons now support binding their loading state directly to reactive state objects with the new `Loading(IState<bool>)` overload. This makes it easier to show loading indicators during async operations without manually managing the loading state.

**Usage:**

```csharp
public override object? Build()
{
    var loading = this.UseState(false);

    async ValueTask OnClick()
    {
        loading.Set(true);
        await Task.Delay(2000); // Simulate async operation
        loading.Set(false);
    }

    // The button automatically shows loading state when loading is true
    return new Button("Submit")
        .HandleClick(OnClick)
        .Loading(loading);
}
```

**API Changes:**

- Added `Button.Loading(IState<bool> loading)` extension method that automatically binds to the reactive state's value
- The existing `Loading(bool loading = true)` overload remains unchanged for static loading states

This enhancement simplifies async button interactions by letting you bind the loading state directly to a reactive state object, eliminating the need to manually set `Loading(true)` or `Loading(false)` after calling `Build()`.

### Breaking Change: Framework Routes Now Use `/ivy` Prefix

All Ivy framework endpoints have been reorganized under a single `/ivy` prefix for better namespace isolation and clearer separation between framework routes and your application routes.

**What Changed:**

All framework endpoints now use the `/ivy` prefix:

- **SignalR Hub:** `/messages` → `/ivy/messages`
- **Authentication:** `/auth/*` → `/ivy/auth/*`
- **Webhooks:** `/webhook` → `/ivy/webhook`
- **File Upload:** `/upload/*` → `/ivy/upload/*`
- **File Download:** `/download/*` → `/ivy/download/*`
- **Static Assets:** `/assets/*` → `/ivy/assets/*`
- **Health Check:** `/health` → `/ivy/health`

**What You Need to Update:**

1. **Authentication Callback URLs** - If you're using OAuth providers, update your redirect URIs:

   ```
   Before: http://localhost:5010/webhook
   After:  http://localhost:5010/ivy/webhook
   ```

   This applies to Auth0, Microsoft Entra, Supabase, and other OAuth providers. You'll need to update your application configuration in the provider's dashboard.

2. **Image Paths** - If you're referencing assets in markdown or code, update paths:

   ```csharp
   // Before
   new Image("/assets/logo.png")

   // After
   new Image("/ivy/assets/logo.png")
   ```

3. **Health Check Monitoring** - Update any health check URLs in your monitoring tools or load balancers:

   ```
   Before: http://localhost:5010/health
   After:  http://localhost:5010/ivy/health
   ```

### Tooltip Support for Menu Items and DataTable Row Actions

**MenuItem Tooltips:**

You can now add tooltips to menu items using the new `Tooltip()` extension method:

```csharp
// Add tooltip to a menu item
var menuItem = MenuItem.Default("Save", Icons.Save)
    .Tooltip("Save your changes to the server");

// Use in context menus, dropdowns, or sidebar navigation
var menu = new Menu(
    MenuItem.Default("Edit", Icons.Edit).Tooltip("Edit this record"),
    MenuItem.Default("Delete", Icons.Delete).Tooltip("Delete this record permanently"),
    MenuItem.Default("Share", Icons.Share).Tooltip("Share with other users")
);
```

The `MenuItem` record now includes a `Tooltip` property that you can set directly or via the fluent API extension method.

**DataTable Row Action Tooltips:**

Row actions in DataTables now also support tooltips through the `Tooltip` property on the `RowAction` class:

```csharp
data.ToDataTable()
    .Column(e => e.Name)
    .Column(e => e.Status)
    .RowActions(
        new RowAction
        {
            Id = "edit",
            Icon = "edit",
            EventName = "edit-clicked",
            Tooltip = "Edit this row"
        },
        new RowAction
        {
            Id = "delete",
            Icon = "trash",
            EventName = "delete-clicked",
            Tooltip = "Delete this row permanently"
        }
    )
    .OnRowAction(async e =>
    {
        // Handle actions...
    });
```

**API Changes:**

- Added `Tooltip` property to `MenuItem` record (nullable string)
- Added `MenuItem.Tooltip(string tooltip)` extension method in `MenuItemExtensions`
- Added `Tooltip` property to `RowAction` class (nullable string)
- `RowAction` changed from class to record for better immutability

### Simplified UseTrigger Hook

The `UseTrigger` hook now includes simpler overloads for common use cases where you don't need to pass data to the triggered view.

**New Simple Overload:**

```csharp
// New simple overload - no trigger value needed
var (dialogView, showDialog) = this.UseTrigger((open) =>
    new Dialog("Confirm", "Are you sure?")
        .OnClose(() => open.Set(false))
);

return Layout.Vertical()
    | Button.Primary("Show Dialog", () => showDialog())
    | dialogView;
```

**API Changes:**

- Added `UseTrigger(Func<IState<bool>, object?> factory)` overload on `IViewContext`
- Added `UseTrigger<TView>(Func<IState<bool>, object?> viewFactory)` extension method on `ViewBase`
- Existing generic `UseTrigger<T>` overloads remain unchanged for when you need to pass data

### Horizontal Field Placement in Forms

The `FormBuilder` now includes a new `PlaceHorizontal()` method that provides a cleaner API for arranging form fields side-by-side. This replaces the confusing `Place(bool row, ...)` overload.

**After:**

```csharp
// New API - clear intent
form.PlaceHorizontal(m => m.FirstName, m => m.LastName) // Side-by-side
    .Place(m => m.Email); // Stacked vertically (default)
```

### Form Input Size Consistency

All form input fields now have consistent sizing across different input types.

### Width and Height Support for Field Widgets

The `FieldWidget` component now supports explicit `width` and `height` properties, giving you precise control over form field dimensions.
**Usage:**

```csharp
// Set explicit width for a field
state.ToTextInput()
    .Width("300px")
    .Height("40px");

// Use with FormBuilder for custom field layouts
var form = model.ToForm()
    .Builder(m => m.Name, state => state.ToTextInput().Width("400px"))
    .Builder(m => m.Email, state => state.ToTextInput().Width("300px"));
```

### Fixed Table Column Width Handling

Table column widths using `Size.Units()` now work correctly when only some columns have explicit widths set.

### DataTable Row Actions with Menu Items and Nested Dropdowns

DataTable row actions have been significantly enhanced with support for nested menus, better styling, and improved event handling. Row actions now use `MenuItem` instead of the old `RowAction` class.

**New API with MenuItem:**

```csharp
data.ToDataTable()
    .Header(e => e.Name, "Name")
    .Header(e => e.Email, "Email")
    .RowActions(
        MenuItem.Default(Icons.Pencil, "edit").Tooltip("Edit employee"),
        MenuItem.Default(Icons.Trash, "delete").Tooltip("Delete employee"),
        MenuItem.Default(Icons.Eye, "view").Tooltip("View details"),
        MenuItem.Default(Icons.EllipsisVertical, "menu")
            .Children([
                MenuItem.Default(Icons.Archive, "archive").Label("Archive"),
                MenuItem.Default(Icons.Download, "export").Label("Export"),
                MenuItem.Default(Icons.Share2, "share").Label("Share")
            ])
    )
    .HandleRowAction(async e =>
    {
        var args = e.Value;
        var actionId = args.ActionId;        // ID from MenuItem tag or label
        var rowIndex = args.RowIndex;        // Zero-based row index
        var rowData = args.RowData;          // Dictionary keyed by column name

        // Access row data by column name
        var employeeName = rowData.TryGetValue("Name", out var name)
            ? name?.ToString()
            : "Unknown";

        client.Toast($"Action: {actionId} on {employeeName} (row {rowIndex})");
    });
```

**Key Features:**

- **Nested Menus** - Row actions can now have nested dropdowns by using `MenuItem.Children()`, perfect for organizing related actions under a "more actions" menu
- **Better Event Handling** - The new `HandleRowAction` method provides direct access to `ActionId`, `RowIndex`, and `RowData` (a dictionary keyed by column name)
- **Consistent API** - Row actions now use the same `MenuItem` API as menus, sidebars, and other navigation components
- **Improved Styling** - Row action buttons now use theme-aware colors with better hover effects, borders, and visual feedback

**Breaking Changes:**

The old `RowAction` class and `OnRowAction` method are replaced:

```csharp
// Old API (no longer supported)
.RowActions(
    new RowAction { Id = "edit", Icon = "Pencil", EventName = "OnEdit", Tooltip = "Edit" }
)
.OnRowAction(e => { /* ... */ })

// New API
.RowActions(
    MenuItem.Default(Icons.Pencil, "edit").Tooltip("Edit")
)
.HandleRowAction(e => { /* ... */ })
```

**Cell Actions:**

You can now attach click handlers to specific cells using the new `HandleCellAction` method:

```csharp
data.ToDataTable()
    .Header(e => e.Email, "Email")
    .Renderer(e => e.Email, new LinkDisplayRenderer { Type = LinkDisplayType.Url })
    .HandleCellAction(e => e.Email, (email) =>
    {
        client.Toast($"Email clicked: {email}");
    });
```

### DataTable Link Rendering Improvements

The DataTable now uses a dedicated custom link renderer instead of relying on the generic `GridCellKind.Uri` cell type.

**New Link Rendering API:**

```csharp
// New approach - explicit renderer
data.ToDataTable()
    .Header(e => e.ProfileUrl, "Profile")
    .Renderer(e => e.ProfileUrl, new LinkDisplayRenderer { Type = LinkDisplayType.Url })
```

**Features:**

- **Custom Link Styling** - Links now render with blue text and underlines using a custom cell renderer
- **Better Click Handling** - Improved Ctrl+Click / Cmd+Click support for opening links in new tabs
- **Focus on Open** - External links now automatically focus the new tab when opened
- **Auto Column Type** - Renderers automatically set the column type, so you don't need to specify both renderer and type hint

**Link Opening Behavior:**

- **External links** (http/https) open in a new focused tab
- **Relative URLs** navigate in the same tab
- **Ctrl+Click / Cmd+Click** opens links without triggering cell selection

### DataTable Visual Refinements

The DataTable widget has received several visual improvements for a more polished appearance:

**Sorted Column Headers:**

Sorted columns now have a subtle light gray background that matches the header hover effect

**Row Action Buttons:**

Row action buttons now feature refined styling with improved visual hierarchy:

- Clean white backgrounds with subtle borders (`bg-background` with `border-[var(--color-border)]`)
- Smooth hover effects using theme-aware muted colors (`hover:bg-[var(--color-muted)]`)
- Better spacing and alignment (positioned 5px from top instead of -1.5px)
- Support for dropdown menus with nested actions

**Row Hover Effect:**

Row hover backgrounds now use more subtle theme colors (`muted` instead of `accent`) for a lighter, less prominent hover effect that still provides clear visual feedback.

These refinements create a cleaner, more professional appearance while ensuring all colors properly support both light and dark themes through CSS variable integration.

### DataTable Configuration Improvements

Several improvements have been made to DataTable configuration:

**ShowColumnTypeIcons Default Changed:**

The `ShowColumnTypeIcons` property now defaults to `false` instead of `true`. This provides a cleaner, less cluttered table appearance out of the box. Column type icons (showing data types like text, number, date) are now opt-in:

```csharp
data.ToDataTable()
    .Config(config =>
    {
        config.ShowColumnTypeIcons = true; // Explicitly enable if needed
    });
```

**New Display Renderers:**

- Added `ButtonDisplayRenderer` for displaying buttons in DataTable columns
- This enables future support for clickable button cells in tables

**Internal Improvements:**

- Improved code formatting and consistency in DataTable configuration
- Added placeholder methods for future row action and cell action handlers (`HandleRowAction`, `RowActions2`, `HandleCellAction`)

### DataTable Footer Support

DataTable now supports footer content that overlays the bottom of the table. This is useful for displaying summary information, totals, or action buttons without disrupting the table layout.

**Key Features:**

- **Fixed Positioning** - The footer is positioned at the bottom of the DataTable container and scrolls horizontally with the table content
- **Smart Whitespace Handling** - When there's not enough data to fill the container, empty filler rows are automatically added to push the footer to the bottom, maintaining a polished appearance
- **Proper Z-Index Management** - Horizontal scrollbars appear above the footer overlay, ensuring smooth scrolling without visual conflicts
- **Empty Row Protection** - Empty filler rows are non-interactive - they cannot be selected, clicked, or edited, preventing confusion

**Technical Details:**

The DataTable now:

- Calculates whitespace needed to fill the container based on visible rows, header heights, and container dimensions
- Adds empty filler rows as needed to maintain footer positioning
- Prevents all interactions with empty filler rows (selection, hover, clicks)
- Ensures scrollbars have proper z-index layering to appear above the footer

This enhancement improves the visual consistency of DataTables, especially when displaying small datasets in large containers.

### Fixed DataTable Filter Query Syntax Highlighting

Fixed a visual issue in the DataTable filter query editor where syntax highlighting would remain active even when the query was invalid, making it harder to see that there was an error. Now, when a query is invalid, the syntax highlighting is disabled and the text appears in plain black, providing clearer visual feedback that something needs to be corrected.

This improves the user experience when working with complex DataTable filters by making invalid query states more obvious and easier to identify at a glance.

### Chart Toolboxes Now Opt-In

Chart toolboxes are now opt-in rather than automatically included by default. Previously, the default chart styles for Area, Bar, Line, and Pie charts would automatically add a toolbox with features like save-as-image, data view, and magic type converters. Now, you need to explicitly call `.Toolbox()` if you want these features.

**Breaking Change:**

If you were relying on the default toolbox functionality, you'll need to update your chart code:

```csharp
// Before - toolbox was automatic
data.ToAreaChart()
    .Dimension("Month", e => e.Month)
    .Measure("Desktop", e => e.Sum(f => f.Desktop))
    .Measure("Mobile", e => e.Sum(f => f.Mobile));

// After - explicitly add toolbox
data.ToAreaChart()
    .Dimension("Month", e => e.Month)
    .Measure("Desktop", e => e.Sum(f => f.Desktop))
    .Measure("Mobile", e => e.Sum(f => f.Mobile))
    .Toolbox(); // Add this line
```

**New API Methods:**

All chart builders now support three `.Toolbox()` overloads:

```csharp
// Default toolbox with standard options
.Toolbox()

// Custom toolbox configuration
.Toolbox(new Toolbox()
    .SaveAsImage(true)
    .DataView(false)
    .MagicType(true))

// Toolbox with configuration function
.Toolbox(tb => tb
    .Orientation(Toolbox.Orientations.Vertical)
    .SaveAsImage(true))
```

This applies to `AreaChartBuilder`, `BarChartBuilder`, `LineChartBuilder`, and `PieChartBuilder`.

**Why This Change:**

Making toolboxes opt-in gives you more control over chart features and reduces visual clutter when you don't need export/transform functionality. This is especially useful for embedded charts in dashboards or reports where toolbox controls would be distracting.

### Fixed Nullable Number Field Clearing

Fixed an issue where clicking the X button on nullable number fields (like `int?`, `double?`, or `decimal?`) would not properly clear the value. Previously, the framework would not create widget events when the value was `null`, preventing the clear action from being processed.

Now the framework correctly creates events even when the value is `null`, since `null` is a valid value for nullable types. This ensures that the X button works consistently across all field types, allowing users to clear optional numeric fields as expected.

This fix affects all widgets that use nullable number inputs, such as forms with optional quantity, price, or numeric identifier fields.

### Complete TabsLayout Redesign with Improved Performance

The TabsLayout widget has undergone a comprehensive redesign focused on performance, maintainability, and user experience. This refactoring brings significant improvements to drag-and-drop functionality and fixes several edge cases.

**Performance Improvements:**

- **Eliminated Content Re-renders on Tab Reorder** - Previously, dragging tabs to reorder them would cause all tab content to re-render, resulting in apps remounting and unnecessary data fetching. The system now uses a hash-based children stability check to only update tab content when tabs are actually added or removed, not just reordered.

- **Smooth Drag-and-Drop** - Fixed lag and zoom issues during tab dragging by switching from CSS scale transforms to `translate3d()` and disabling transitions during drag operations. Tabs now track the cursor smoothly without compression or scaling artifacts.

- **Better Responsive Overflow Calculation** - The tab visibility calculation system has been completely rewritten with proper debouncing, resize observers, and mutation observers. This ensures accurate detection of which tabs fit in the available space without performance issues.

**UX Enhancements:**

- **Dropdown Tab Selection** - When selecting a tab from the overflow dropdown menu, it now swaps positions with the last visible tab, bringing the selected tab into view. This provides a more intuitive experience when working with many open tabs.

- **Improved Tab Styling** - Inactive tabs now display with a light gray background (`bg-muted`) and show a darker hover effect (`hover:bg-muted-foreground/20`), providing better visual feedback. Active tabs use the card background color (`bg-card`) for better contrast and consistency across the theme. In dark mode, inactive tabs use pure black backgrounds (`dark:bg-background`) for improved visual separation. The close button on inactive tabs is now only visible on hover, reducing visual clutter. Tab action buttons (refresh and close) now use fully circular styling (`rounded-full`) for a more polished, modern appearance.

- **Updated Tab Spacing** - Tabs now use updated padding (`px-4 py-2` instead of `px-3 py-1.5`) and negative margins (`-ml-px -mt-px`) to create overlapping borders for a more polished appearance.

**Code Architecture:**

The monolithic `TabsLayoutWidget.tsx` (1080 lines) has been split into a well-organized module structure:

```
tabs/
├── TabWidget.tsx              # Simple tab content wrapper
├── TabsLayoutWidget.tsx       # Main orchestration component
├── types.ts                   # TypeScript interfaces
├── components/
│   ├── DropdownMenu.tsx       # Overflow dropdown UI
│   ├── Sortable.tsx          # Drag-and-drop components
│   ├── TabContent.tsx        # Tab content renderer
│   └── Variants.tsx          # Content & Tabs variant renderers
├── hooks/
│   ├── useAnimation.ts       # Animated underline for Content variant
│   ├── useDrag.ts           # Drag-and-drop logic
│   ├── useTabCalculation.ts # Responsive overflow calculations
│   └── useTabManagement.ts  # State management & synchronization
└── utils/
    └── tabUtils.ts           # Helper functions for ordering, sizing, etc.
```

Each hook includes comprehensive unit tests ensuring reliability. This modular architecture makes the code more maintainable and easier to extend with new features.

**Browser Compatibility:**

The component now includes `happy-dom` as a dev dependency for improved testing support in the Vitest environment.

These improvements make the TabsLayout widget more responsive, performant, and maintainable while providing a smoother user experience when working with multiple tabs.

### DBML Editor UI Improvements

The DBML canvas editor has received several UI improvements for a better table editing experience:

**Fixed Table Width** - All database tables in the canvas now use a consistent fixed width of 240px instead of dynamic sizing. This creates a more uniform and predictable layout when working with database schemas.

**Field Name Tooltips** - Long field names and types now display in tooltips when hovered, making it easier to work with descriptive column names that might overflow the table cell. The field names are properly truncated with ellipsis, and hovering reveals the full name.

**Improved Connection Handles** - Connection handles (the dots on the sides of tables for creating relationships) now have better visibility and z-index handling. The system now correctly shows handles only for source fields (fields that have relationships pointing out), removing redundant handles on primary key fields that don't have outgoing relationships.

**Better Interaction** - The canvas now has improved pointer event handling, preventing accidental edge interactions and ensuring smooth dragging behavior with proper z-index management for overlapping nodes.

These improvements make the DBML editor more polished and easier to use when designing database schemas visually.

### Sidebar UI Refinements

The default sidebar has received several subtle UI improvements for a more polished appearance:

**Toggle Button Position** - The sidebar toggle button (when using `showToggleButton: true`) has been repositioned for better alignment. The button now sits flush with the top edge (`marginTop: '3px'`) and uses tighter spacing (`left: calc(16rem + 4px)` instead of `8px`), creating a more compact and professional look.

**Scrollbar Visibility** - The sidebar's scrollbar now has improved z-index handling (`z-20`) to ensure it's always visible above other elements, preventing it from being hidden behind sidebar content during scrolling.

These refinements create a more polished visual experience in the sidebar with better interaction feedback and improved scroll behavior.

### User Avatar Display in Sidebar

The default sidebar chrome now properly displays user avatar images. Previously, the sidebar only showed user initials in the avatar component. Now, if a user has an `AvatarUrl` configured, their actual avatar image will be displayed in the sidebar's user menu trigger.

**Implementation:**

```csharp
// The Avatar component now receives the user's avatar URL
new Avatar(user.Value.Initials, user.Value.AvatarUrl)
```

This provides a more polished and personalized user experience in applications using the default sidebar chrome.

### Footer Menu Items Transformer

You can now dynamically customize the footer menu items (links shown at the bottom of the sidebar) using the new `UseFooterMenuItemsTransformer` method on `ChromeSettings`. This powerful feature lets you add, remove, reorder, or filter footer links based on runtime context like user roles or navigation state.

**Basic Usage:**

```csharp
var chromeSettings = ChromeSettings.Default()
    .UseFooterMenuItemsTransformer((items, navigator) =>
    {
        var list = items.ToList();

        // Append a custom logout link
        list.Add(new MenuItem("Logout", _ => navigator.Navigate("app://logout"), Icons.Logout));

        // Move "Settings" to the top
        var settings = list.FirstOrDefault(i => i.Id == "app://settings");
        if (settings != null)
        {
            list.Remove(settings);
            list.Insert(0, settings);
        }

        return list;
    });
```

**Role-Based Filtering:**

```csharp
var chromeSettings = ChromeSettings.Default()
    .UseFooterMenuItemsTransformer((items, navigator) =>
    {
        var user = AuthContext.CurrentUser;

        // Hide admin-only links for non-admins
        return items.Where(i =>
            !i.Tags.Contains("admin") || user?.IsInRole("admin") == true);
    });
```

The transformer receives:

- **items** - The menu items produced by Ivy from discovered apps
- **navigator** - A helper for building MenuItem actions that navigate to URIs or apps
- **return value** - The new collection to render (reordered, filtered, or with additions)

This makes it easy to inject custom links like "Docs", "Logout", or "Change theme" without updating individual apps.

### Alert Dialog Button Positioning

Alert dialogs now display buttons with improved positioning that follows platform UI conventions. Primary action buttons (Ok, Yes) now appear on the right side, while secondary and cancel buttons appear on the left. All buttons are right-aligned within the dialog footer.

**What Changed:**

The button order in alert dialogs has been adjusted:

- **Ok/Cancel** - Cancel button on left, Ok button on right
- **Yes/No** - No button on left, Yes button on right
- **Yes/No/Cancel** - Cancel on left, No in middle, Yes on right

This creates a more intuitive dialog experience where the primary action is consistently positioned on the right side, matching common desktop application patterns. The entire button group is aligned to the right side of the dialog footer for a cleaner, more polished appearance.

This improvement applies automatically to all alert dialogs created with `AlertButtonSet` - no code changes needed in your applications.

### Supabase Auth: Optional User Metadata Fields

The Supabase authentication provider now gracefully handles missing user metadata fields. Previously, the provider would throw an exception if `full_name` or `avatar_url` were not present in the user's metadata JSON. This could cause authentication failures when users didn't have these optional fields set.

Now the provider safely checks for the existence of these fields before accessing them:

```csharp
// The provider now uses TryGetProperty for optional metadata
if (root.TryGetProperty("full_name", out var fullNameProperty))
{
    name = fullNameProperty.GetString();
}
if (root.TryGetProperty("avatar_url", out var avatarUrlProperty))
{
    avatarUrl = avatarUrlProperty.GetString();
}
```

This ensures authentication works smoothly regardless of which metadata fields are present in your Supabase user profiles.

### Fixed Infinite Loop in Authentication Refresh

Fixed a critical bug in the `AuthRefreshLoop` that could cause an infinite loop when an authentication provider returned the same invalid token during refresh attempts. This issue would occur if an auth provider implementation had a bug where it returned an unchanged token when calling `RefreshAccessTokenAsync()`.

**What was fixed:**

The system now detects when a token is invalid AND unchanged after a refresh attempt:

```csharp
// New state for tracking invalid tokens
enum AuthRefreshState {
    HasToken,
    HasNoToken,
    TokenExpired,
    TokenInvalid  // New state
}

// Detection logic prevents infinite refresh loops
if (state == AuthRefreshState.TokenInvalid && token == newToken)
{
    // Auth provider returned the same invalid token - break the loop
    logger.LogInformation("AuthRefreshLoop: Invalid token object unchanged after refresh for {ConnectionId}.", connectionId);
    newToken = null;
}
```

**Why it matters:**

Without this fix, poorly implemented auth providers could cause the authentication refresh loop to run indefinitely, consuming resources and potentially crashing the application. The fix ensures that even with a buggy auth provider, the system degrades gracefully by treating an unchanged invalid token as a failed refresh.

This protection applies to all authentication providers, including Supabase, Authelia, Microsoft Entra, Auth0, and custom implementations.

### Routing and Connection Improvements

Several edge cases in the routing and connection logic have been fixed, improving the reliability of app navigation and reconnection scenarios:

- **Default app selection** - Fixed issues where special apps (`$auth` or `$chrome`) could be incorrectly selected as the default app
- **Chrome parameter handling** - Improved handling when `chrome=false` is specified, ensuring the chrome UI is properly disabled
- **Reconnection robustness** - Enhanced reconnection logic to better handle connection failures and state synchronization
- **Auth failure recovery** - The framework now automatically refreshes the page when authentication fails in a child connection, ensuring users get a clean slate to re-authenticate

These fixes make the app navigation and connection experience more predictable and resilient, especially in scenarios involving authentication, chrome navigation, and connection recovery.

### Entity Framework Core 9.0.11 Update

The framework has been updated to use Entity Framework Core 9.0.11 across all database-related packages. This update includes:

- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Sqlite
- Microsoft.EntityFrameworkCore.Design
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.EntityFrameworkCore.InMemory

This ensures you're running the latest stable version of EF Core with the most recent bug fixes and improvements. If you're using the database generator toolkit or working with EF Core in your Ivy applications, this update will be included automatically when you reference the framework packages.

## Documentation Improvements

### Consolidated FileInput and Upload Documentation

The file upload documentation has been reorganized and consolidated for better discoverability. The separate "Uploads" concept page has been merged into the FileInput widget documentation, providing a single comprehensive resource for file uploads.

The enhanced FileInput documentation now includes:

- Complete upload workflow explanation (state → handler → widget)
- File validation configuration (`Accept()`, `MaxFileSize()`, `MaxFiles()`)
- Event handler examples for `OnBlur` and `OnCancel` events
- Integration patterns with dialogs and forms
- Multiple file upload examples with progress tracking
- Content type handling (binary vs text files)

**Event Handler Improvements:**

The FileInput widget now properly supports the `OnBlur` event handler, which fires when the file dialog closes (either after file selection or cancellation). This makes it easier to respond to user interactions:

```csharp
files.ToFileInput(upload)
    .Placeholder("Choose files")
    .HandleBlur((Event<IAnyInput> e) =>
    {
        if (files.Value.Length > 0)
            client.Toast($"{files.Value.Length} file(s) selected");
        else
            client.Toast("No file selected");
    })
    .HandleCancel((Guid fileId) =>
    {
        upload.Value.Cancel(fileId);
        files.Set(list => list.Where(f => f.Id != fileId).ToImmutableArray());
    });
```

The `OnBlur` event is particularly useful for tracking when users cancel the file selection dialog or for triggering validation after file selection completes.

### JobScheduler Documentation

Comprehensive documentation has been added for the `JobScheduler` API in `Ivy.Helpers`, covering how to coordinate complex async work with declarative job graphs, dependency-aware scheduling, and built-in UI status reporting.

The new documentation includes:

**Core Concepts:**

- Job lifecycle and state transitions (Waiting → Running → Finished/Failed/Cancelled)
- Dependency management with `DependsOn()` for prerequisite enforcement
- Real-time UI rendering with `ToView()` extension method
- Progress reporting with `IProgress<double>`

**Common Patterns:**

*Basic job creation:*

```csharp
var scheduler = new JobScheduler(maxParallelJobs: 2);

var initialize = scheduler.CreateJob("Initialize")
    .WithAction(async (_, _, progress, token) =>
    {
        await Task.Delay(300, token);
        progress.Report(1);
    })
    .Build();

scheduler.CreateJob("Load Data")
    .DependsOn(initialize)
    .WithAction(async (_, _, progress, token) =>
    {
        await Task.Delay(500, token);
        progress.Report(1);
    })
    .Build();
```

*Fluent job chaining with `.Then()`:*

```csharp
scheduler.CreateJob("Step 1: Extract")
    .WithAction(async (_, _, progress, token) => { /* ... */ })
    .Then("Step 2: Transform", async (_, _, progress, token) => { /* ... */ })
    .Then("Step 3: Load", async (_, _, progress, token) => { /* ... */ })
    .Build();
```

*Dynamic child jobs:*

```csharp
scheduler.CreateJob("Generate Reports")
    .WithAction(async (job, sched, _, token) =>
    {
        for (int i = 1; i <= 3; i++)
        {
            var child = sched.CreateJob($"Report {i}")
                .WithAction(async (_, _, progress, childToken) =>
                {
                    await Task.Delay(200, childToken);
                    progress.Report(1);
                })
                .Build();

            sched.AddChild(job, child);
        }
        await Task.Delay(600, token);
    })
    .Build();
```

The documentation includes interactive examples showing linear dependencies, complex job hierarchies, and integration with Ivy's reactive UI system via `Subscribe()`. Each example demonstrates progress tracking with the scheduler's built-in UI view.

### Fixed App Protocol URL Parsing

Fixed a critical bug in the `app://` URL parsing logic that was incorrectly truncating app identifiers. The framework was removing 7 characters instead of 6 when stripping the `app://` protocol prefix, causing app:// links to lose their first character.

**What was fixed:**

```csharp
// Before (incorrect)
const appId = safeHref.substring(7); // Would strip 7 chars from "app://MyApp"
// Result: "yApp" (missing first 'M')

// After (correct)
const appId = safeHref.substring(6); // Correctly strips 6 chars from "app://"
// Result: "MyApp" (correct)
```

This bug affected:

- Navigation using `app://` links in markdown and UI components
- The MarkdownRenderer component when processing app protocol links
- URL validation logic for app:// URLs

If you were experiencing issues with app:// links not working correctly or navigating to the wrong apps, this fix resolves those problems.

### Security Policy Documentation

The framework now includes a comprehensive [SECURITY.md](https://github.com/Ivy-Interactive/Ivy-Framework/security) file that documents:

- **Vulnerability reporting process** - How to responsibly report security issues through GitHub Security Advisories or direct contact (not through public issues)
- **Coordinated disclosure policy** - What to expect when reporting vulnerabilities, including acknowledgment within 48 hours and initial assessment within 7 days
- **Security scope** - What's covered (core library, widgets, authentication providers, database connectors, CLI tool) and what's out of scope
- **Security best practices** - Recommendations for keeping your Ivy applications secure
- **Built-in security features** - Documentation of the framework's security-focused design including server-side state management, handcrafted authentication integrations, and secure secrets handling

If you discover a security vulnerability in Ivy Framework, please report it via the [Security tab](https://github.com/Ivy-Interactive/Ivy-Framework/security) in the GitHub repository rather than creating a public issue.

### Enhanced Chat Widget Examples

The Chat widget documentation has been polished with several improvements to make the examples clearer and more practical:

**Layout improvements:**

- **Removed artificial width constraints** - All chat examples now use `.Width(Size.Full())` instead of `.Width(Size.Full().Max(400))`, allowing the chat widget to adapt naturally to its container without arbitrary maximum widths. This makes the examples more flexible and realistic for real-world usage.

- **Better visibility for embedded widgets** - The advanced chat example now includes explicit sizing for charts (`.Width(Size.Units(80))`) and tables (`.Width(Size.Units(100))`), ensuring these widgets render properly within chat messages without layout issues.

- **More realistic table data** - The table example in the advanced chat now includes 5 rows with 4 columns of realistic employee data, better demonstrating how tables render in chat contexts.

**Documentation cleanup:**

- **Removed redundant example** - The "Chat with Custom Placeholder" example has been removed since the `Placeholder()` method is now demonstrated directly in the advanced example, reducing duplication and keeping the documentation focused.

**Example of embedded table in chat:**

```csharp
"table data" => new Table(
    new TableRow(
        new TableCell("Name"),
        new TableCell("Age"),
        new TableCell("Role"),
        new TableCell("Department")
    ).IsHeader(),
    new TableRow(new TableCell("John Doe"), new TableCell("30"), new TableCell("Developer"), new TableCell("Engineering")),
    new TableRow(new TableCell("Jane Smith"), new TableCell("25"), new TableCell("Designer"), new TableCell("Design")),
    new TableRow(new TableCell("Bob Johnson"), new TableCell("35"), new TableCell("Manager"), new TableCell("Product")),
    new TableRow(new TableCell("Alice Williams"), new TableCell("28"), new TableCell("Developer"), new TableCell("Engineering")),
    new TableRow(new TableCell("Charlie Brown"), new TableCell("32"), new TableCell("QA Engineer"), new TableCell("Quality Assurance"))
).Width(Size.Units(100))
```

These refinements make the Chat widget documentation more practical and easier to follow when building chat interfaces in your Ivy applications.

### Redesigned Authentication Login Form

The default authentication login UI has been completely redesigned with improved UX and modern form handling. The new design provides a cleaner, more welcoming experience with better validation and error handling.

**Key improvements:**

- **Modern welcome screen** - The login page now displays the application name (from `ServerArgs.MetaTitle` or assembly name) with an Ivy logo and welcoming message: "Welcome to {AppName}!"

- **FormBuilder integration** - The password/email flow now uses the FormBuilder API for proper validation, field-level error handling, and form state management. This provides a more robust and user-friendly form experience.

- **Improved layout** - Better spacing, responsive card sizing (120 units width, max 500px), and clearer visual hierarchy with the new gap-based layout system

- **Better error states** - Error messages are now displayed in a Callout component and properly cleared when retrying login. The form also prevents submission while validation is running or login is in progress.

**What it looks like:**

The new login form follows a cleaner structure with proper form validation:

```csharp
// Form model with validation
var credentials = this.UseState(() => new LoginFormModel("", ""));

var formBuilder = credentials.ToForm("Login")
    .Required(m => m.User, m => m.Password)
    .Label(m => m.User, "User")
    .Label(m => m.Password, "Password")
    .Builder(m => m.User, state => state.ToTextInput())
    .Builder(m => m.Password, state => state.ToPasswordInput());

var (submitForm, formView, _, submitting) = formBuilder.UseForm(this.Context);

// Form submission validates before attempting login
async ValueTask HandleSubmit()
{
    var isValid = await submitForm(); // Runs validation
    if (!isValid) return;

    await HandleLoginAsync(); // Only proceeds if valid
}
```

The login page now provides a more polished first impression for users accessing your Ivy applications, with proper validation feedback and a cleaner visual design that matches modern authentication experiences.

### Select Input Ellipsis and Tooltips

Select inputs now properly handle long option labels with text truncation and automatic tooltips. When a selected option's label is too long to fit in the select trigger, it will be truncated with an ellipsis (`...`), and hovering over the truncated text reveals the full label in a tooltip.

**What's Improved:**

- **Smart Ellipsis Detection** - The component automatically detects when text overflows and needs truncation, adjusting in real-time as you resize the window
- **Automatic Tooltips** - Tooltips only appear when text is actually truncated, avoiding unnecessary tooltips on short labels
- **Proper Layout** - The select trigger now uses flex layout with proper shrinking behavior to ensure the chevron icon stays visible while the label truncates
- **Better UX** - Tooltips are hidden when the dropdown is open, preventing tooltip/dropdown overlap

**Technical Details:**

The select component now:

- Uses responsive CSS classes (`[&>span:first-child]:flex-1 [&>span:first-child]:min-w-0 [&>span:first-child]:truncate`) to enable proper text truncation
- Applies `flex-shrink-0` to the chevron icon to prevent it from being compressed
- Monitors DOM changes with ResizeObserver to detect when ellipsis becomes necessary
- Only renders tooltips for single-select inputs (multi-select displays selected values differently)

This improvement ensures select inputs work gracefully with long option labels without breaking the layout or hiding important information from users.

The same ellipsis and tooltip functionality has also been applied to the **AsyncSelectInput** widget, ensuring consistent behavior across both select input types. When the selected value display is too long, it will be truncated with an ellipsis and show the full value in a tooltip on hover. The implementation uses the same smart detection logic with ResizeObserver to handle window resizing and dynamically show/hide tooltips only when necessary.

### Optional Blade Title

The `BladeWidget` component now has an optional `title` prop, making it more flexible when using custom header slots. Previously, you always had to provide a title even when using the `BladeHeader` slot, which could result in an empty header being rendered.

**Usage:**

```typescript
// Now you can omit the title when using a custom header slot
<BladeWidget id="custom-blade" width="400px" index={0}>
  <BladeHeader>
    <CustomHeaderComponent />
  </BladeHeader>
  <BladeContent>
    {/* Your blade content */}
  </BladeContent>
</BladeWidget>

// Or continue using the default header with a title
<BladeWidget id="default-blade" title="My Blade" width="400px" index={0}>
  <BladeContent>
    {/* Your blade content */}
  </BladeContent>
</BladeWidget>
```

This fix prevents rendering an empty `<h2>` tag when no title is provided, resulting in cleaner markup and better support for fully custom blade headers.

### Form Groups Can Now Open by Default

The `Group()` method on `FormBuilder` now supports an optional `open` parameter, allowing you to control whether grouped fields start expanded or collapsed. Previously, all grouped fields were collapsed by default, requiring users to manually expand them to see the content.

**Usage:**

```csharp
// Create a form with groups that are open by default
var form = model.ToForm()
    .Group("Personal Information", open: true, m => m.Name, m => m.Email, m => m.Age)
    .Group("Contact Details", m => m.PhoneNumber, m => m.Website) // Collapsed by default
    .Group("Account Settings", open: true, m => m.Username, m => m.Password);
```

**API Overloads:**

```csharp
// Simple group (collapsed by default)
.Group("Section Name", m => m.Field1, m => m.Field2)

// Group with open state
.Group("Section Name", open: true, m => m.Field1, m => m.Field2)

// Group with column index
.Group("Section Name", column: 0, m => m.Field1, m => m.Field2)

// Group with column and open state
.Group("Section Name", column: 0, open: true, m => m.Field1, m => m.Field2)
```

This is particularly useful when you have important fields in a group that users should see immediately, like primary contact information or required settings. The groups render as `Expandable` widgets with the `Open()` method set appropriately based on your configuration.

**Example from the samples:**

```csharp
// Form with mixed open/closed groups
var form = model.ToForm()
    .Medium()
    .Group("Account", open: true, m => m.Name, m => m.Email, m => m.Password) // Open by default
    .Group("Profile", m => m.Age, m => m.BirthDate, m => m.Role) // Collapsed by default
    .Builder(m => m.Description, s => s.ToTextAreaInput());
```

The underlying `Expandable` widget has also been updated with a new `Open` property and corresponding `Open()` extension method, which the form system uses internally to control the default state of grouped fields.

### New Architecture Documentation Section

Comprehensive architecture documentation has been added to the Getting Started guide, providing deep technical insights into how Ivy Framework works internally. This new section includes three detailed guides:

**[Frontend Architecture](https://docs.ivyframework.dev/onboarding/getting-started/architecture/frontend-architecture)** - Deep dive into the React/TypeScript frontend:

- Technology stack (React 19, Vite, TypeScript, Tailwind CSS, Radix UI)
- Build system and development environment configuration
- Real-time communication with SignalR and the `useBackend` hook
- Widget rendering system and the `renderWidgetTree` function
- Theming system with CSS custom properties and runtime theming via `IThemeService`
- Development tools including hot reload and XML debugging

**[Backend Architecture](https://docs.ivyframework.dev/onboarding/getting-started/architecture/backend-architecture)** - Server-side C# framework:

- Core `Server` class and startup flow
- Application system with `ViewBase` and `AppDescriptor`
- Widget system architecture and serialization with `[Prop]` attributes
- State management and widget binding patterns
- Service container and dependency injection
- Real-time communication infrastructure with `AppHub`

**[Communication](https://docs.ivyframework.dev/onboarding/getting-started/architecture/communication)** - Frontend-backend protocol:

- SignalR connection management and lifecycle
- Message types (Refresh, Update, Error, Auth tokens, Widget events)
- Widget event system for user interactions
- State synchronization using JSON patches
- Client commands (clipboard, URL navigation, JWT tokens, themes, toasts)
- Development features including hot reload support and connection state monitoring

These guides are perfect for developers who want to understand:

- How Ivy achieves real-time updates without writing frontend code
- The widget tree synchronization mechanism using JSON patches
- How the frontend rendering pipeline works
- The role of SignalR in maintaining state consistency
- How to extend or customize the framework

The documentation includes code examples, architecture diagrams (using Mermaid), and detailed explanations of key concepts like the widget rendering system, state synchronization, and development workflows.

### Updated Supabase Authentication Guide

The Supabase authentication documentation has been updated to reflect recent changes in the Supabase UI. When configuring authentication settings in your Supabase project, you now need to click **"URL Configuration"** instead of "Settings" to access the redirect URL and site URL configuration.

**Updated Steps:**

1. Go to Authentication in the sidebar
2. Click **"URL Configuration"** (previously "Settings")
3. Configure your Site URL and Redirect URLs

This documentation update ensures the setup instructions match the current Supabase dashboard interface, preventing confusion when configuring OAuth callbacks for your Ivy applications.

### Consolidated Chrome Configuration Documentation

The documentation for configuring the application chrome (sidebar, header, footer, navigation) has been consolidated and expanded in the new [Chrome Configuration](https://docs.ivyframework.dev/onboarding/concepts/chrome) guide. This replaces the scattered documentation previously found in separate Program.md, Wallpaper.md, and FooterMenuItemsTransformer.md pages.

The unified Chrome guide now covers all chrome-related configuration in one place:

**ChromeSettings Options:**

- `DefaultAppId()` / `DefaultApp<T>()` - Set the default app to load
- `UseTabs()` / `UsePages()` - Configure navigation mode
- `Header()` / `Footer()` - Customize sidebar sections
- `WallpaperAppId()` / `WallpaperApp<T>()` - Set background app for empty tabs
- `UseFooterMenuItemsTransformer()` - Dynamically transform footer menu items

**Wallpaper Configuration:**

```csharp
// Set a welcome screen that appears when no tabs are open
var chromeSettings = ChromeSettings.Default()
    .WallpaperApp<WelcomeScreenApp>()
    .UseTabs();

server.UseChrome(() => new DefaultSidebarChrome(chromeSettings));
```

**Footer Transformer:**

```csharp
// Dynamically customize footer links based on user roles
var chromeSettings = ChromeSettings.Default()
    .UseFooterMenuItemsTransformer((items, navigator) =>
    {
        var list = items.ToList();
        list.Add(new MenuItem("Logout", _ => navigator.Navigate("app://logout"), Icons.Logout));
        return list;
    });
```

This consolidation makes it much easier to find all chrome-related configuration options in one place, with clear examples and use cases for each feature.

### Expanded Installation and Project Setup Guide

The [Installation](https://docs.ivyframework.dev/onboarding/getting-started/installation) documentation has been significantly expanded from a simple CLI installation guide to a comprehensive project setup reference. The updated guide now covers:

**Prerequisites:**

- .NET 9.0 SDK requirement
- Optional dependencies (Git, database systems)
- Code editor recommendations

**Installation Methods:**

- **Quick Start with CLI** - Using `ivy init` for automatic project scaffolding
- **Manual Setup** - Step-by-step instructions for creating projects without the CLI

**Package Management:**

- Core Ivy package installation
- Optional extension packages (authentication providers, database tools)
- Package dependency overview with diagrams

**Project Structure:**

- Basic project layout for single-app projects
- Multi-project solution structure for larger applications
- File organization best practices

**Server Configuration:**

- Basic server setup in `Program.cs`
- Development vs production environment differences
- Configuration options overview with links to detailed guides

**Manual Setup Example:**

```csharp
// Minimal Program.cs for manual setup
using Ivy;

var server = new Server();
server.UseHotReload();
server.AddAppsFromAssembly();
server.UseChrome();

await server.RunAsync();
```

The guide includes visual diagrams (using Mermaid) showing prerequisite relationships, project structures, and environment configurations. This makes the installation process clearer for developers at all experience levels, whether they prefer the quick CLI approach or want more control with manual setup.

### Refined Getting Started Documentation

Several improvements have been made to the Getting Started documentation to better guide new users:

**Reorganized Content:**

- Tutorial files have been renumbered to accommodate the new Architecture section
- `05_TodoTutorial.md` moved to `06_TodoTutorial.md`
- `06_ChatTutorial.md` moved to `07_ChatTutorial.md`
- New Architecture section added as `05_Architecture/`

**Enhanced How Ivy Works Guide:**

- Moved the "Why This Approach Works" section content to the Introduction page where it fits better contextually
- Added prominent note emphasizing that in production, you only work with the backend - the frontend is pre-built and embedded
- Added links to the new detailed Architecture documentation for developers who want deeper technical understanding
- Improved section headings for better scanability

**Better Cross-Linking:**

- Installation guide now links to CLI Tools, Core Concepts, and Basics documentation
- Basics guide includes links to Widgets documentation
- How Ivy Works guide links to all three Architecture guides
- Program guide redirects chrome configuration to the new consolidated Chrome documentation

These refinements create a clearer learning path for new Ivy developers, from quick start through basic concepts to deep architectural understanding.

### Introduction Documentation Updates

The [Introduction](https://docs.ivyframework.dev/onboarding/getting-started/introduction) page has been refined with better content organization and more accurate descriptions:

**Repositioned Content:**

- The "Why Ivy Exists" section now opens with a clear positioning statement: "The Ivy Framework is a comprehensive solution for building internal business applications. The framework targets scenarios where rapid development, maintainability, and integration with existing enterprise systems are prioritized."
- Removed the separate "Open-Source & Cloud-Native" section, consolidating this information into other areas

**Enhanced Security & Architecture Section:**

- Updated to mention specific authentication providers: "Multiple authentication providers (Supabase, Authelia, Basic Auth) with RBAC"
- Updated to mention specific database support: "Database integration (SQL Server, PostgreSQL, SQLite, MySQL) via Entity Framework Core"

**Improved Deployment & Tooling Section:**

- Reordered bullet points to emphasize rich CLI tooling before deployment
- Updated deployment description to be more specific: "One-command container deployment to AWS, Azure, GCP, or your own infrastructure"

These updates provide a more accurate and informative introduction to Ivy Framework, highlighting the specific technologies and providers supported while maintaining clarity about the framework's target audience and use cases.

### Sheet Widget Layout Improvements

The `SheetWidget` component now has improved vertical spacing for better visual appearance. The content area now includes a small top padding (`pt-1` instead of `pt-0`), providing better spacing when using sheets with async select components and other content. This creates a more polished look with proper breathing room for sheet content.

### Copy-to-Clipboard Button Styling Update

The `CopyToClipboardButton` component has been refined to better integrate with the shadcn/ui design system. The button now uses the `buttonVariants` helper for more consistent styling and properly adapts its appearance based on whether it's icon-only or includes a label.

**What changed:**

- **Icon-only mode** - When no label is provided, the button now uses the ghost variant with icon sizing from shadcn/ui (`buttonVariants({ variant: 'ghost', size: 'icon' })`), ensuring consistency with other icon buttons in your application
- **Standardized icon sizing** - Icon sizes now use Tailwind classes (`h-4 w-4`) instead of explicit size props, following the framework's design system conventions
- **Better active state styling** - When copied, icon-only buttons now properly show the primary background color to indicate the action succeeded

These refinements ensure the copy button follows the same design patterns as other UI components in the framework, providing a more cohesive visual experience.

### Audio Recorder: Automatic MIME Type Fallback

The `AudioRecorderWidget` now automatically detects and uses browser-supported audio formats, eliminating audio recording failures due to unsupported MIME types. Previously, if your specified MIME type wasn't supported by the browser, recording would silently fail. Now the widget intelligently tests multiple formats and falls back to the first supported option.

**What Changed:**

The widget now maintains a prioritized list of common audio formats and automatically probes browser support:

```csharp
// The widget tries these formats in order:
// 1. Your specified mimeType (if provided)
// 2. audio/webm (Chromium/Firefox)
// 3. audio/mp4 (Safari/iOS)
// 4. audio/ogg (Older Firefox/desktop)
// 5. audio/aac (Safari/iOS)
// 6. audio/webm;codecs=opus
// 7. audio/ogg;codecs=opus
// 8. audio/wav (Uncompressed fallback, always supported)
```

**Browser Compatibility:**

- **Chrome/Edge** - Prefers `audio/webm` or `audio/webm;codecs=opus`
- **Firefox** - Uses `audio/webm` or `audio/ogg`
- **Safari/iOS** - Falls back to `audio/mp4` or `audio/aac`
- **Legacy Browsers** - Uses `audio/wav` as the universal fallback

**Improved Error Handling:**

If no supported format is found, the widget now displays a clear, specific error message:

```
"Recording format not supported in this browser."
```

Instead of the generic:

```
"Failed to record. Check your settings."
```

**Usage:**

No code changes required - existing audio recorder widgets automatically benefit from the fallback logic:

```csharp
// Your existing code continues to work
state.ToAudioRecorder()
    .UploadUrl("/api/upload-audio")
    .MimeType("audio/webm") // Optional - widget will fallback if unsupported
    .ChunkInterval(1000);
```

**Technical Details:**

The widget now:

- Uses `MediaRecorder.isTypeSupported()` to probe format compatibility before recording
- Tracks the selected MIME type and includes it in upload requests via the `mimeType` form field
- Prevents infinite loops when MIME types are invalid by detecting unchanged tokens
- Properly resets MIME type selection on component unmount

This enhancement ensures audio recording works reliably across all browsers without manual format detection or browser-specific code in your applications.

### Option Descriptions in Select Inputs

Select inputs now support optional descriptions on options, providing additional context to help users make informed choices. The `Option<T>` class includes a new `Description` property that displays as a subtitle in select lists.

**Usage:**

```csharp
// Create options with descriptions
var options = new[]
{
    new Option<string>("Standard Shipping", "standard", description: "Delivery in 5-7 business days"),
    new Option<string>("Express Shipping", "express", description: "Delivery in 2-3 business days"),
    new Option<string>("Overnight Shipping", "overnight", description: "Next business day delivery")
};

// Use in AsyncSelectInput - descriptions automatically display
state.ToAsyncSelectInput()
    .Placeholder("Select shipping method")
    .Load(async () => options);
```

**API Changes:**

The `Option<TValue>` constructor now accepts an optional `description` parameter:

```csharp
// New constructor signature
public class Option<TValue>(
    string label,
    TValue value,
    string? group = null,
    string? description = null)
```

Descriptions appear as subtle subtitles below the option label in dropdown lists, making it easier to explain complex choices without cluttering the main label. This is particularly useful for:

- Explaining pricing tiers or subscription plans
- Clarifying configuration options with technical details
- Providing context for shipping methods, payment options, or delivery windows
- Displaying additional metadata like counts, dates, or status information

The `AsyncSelectInput` widget automatically renders descriptions when present, requiring no changes to your existing select input code beyond adding the optional description to your options.

### AsyncSelectInput Now Uses HeaderLayout

The `AsyncSelectInput` widget has been improved with a cleaner visual structure by adopting the `HeaderLayout` widget internally. This separates the search filter from the results list, providing better visual organization.

**What Changed:**

The search input now appears in a dedicated header section, with the loading indicator and results list displayed below in the content area. The header divider is disabled (`ShowHeaderDivider = false`) for a seamless appearance.

**HeaderLayout Enhancement:**

The `HeaderLayout` widget now includes a `ShowHeaderDivider` property that allows you to control whether the divider line between header and content is displayed. This defaults to `true` to maintain existing behavior, but can be set to `false` for cleaner layouts:

```csharp
// Use HeaderLayout without the divider
return new HeaderLayout(header, content)
{
    ShowHeaderDivider = false
};
```

This improvement creates a more polished appearance for async select inputs while making the HeaderLayout widget more flexible for custom layouts throughout your application.

### Kanban Card Builder for Custom Card Layouts

The Kanban widget now supports custom card content through the new `.CardBuilder()` API, giving you complete control over card appearance beyond the default title and description. This enables rich, custom card layouts with additional fields, formatting, or any widgets you need.

**Basic Custom Cards:**

```csharp
data.ToKanban(
    groupBySelector: e => e.Status,
    idSelector: e => e.Id,
    titleSelector: e => e.Title,
    descriptionSelector: e => e.Description)
.CardBuilder(factory => factory.Func<Task, Task>(task => new Card(
    content: task.ToDetails()
        .Remove(x => x.Id)
        .MultiLine(x => x.Description)
)))
.HandleCardMove(moveData => {
    // Handle drag-and-drop between columns
});
```

**Key Features:**

- **Custom Content** - Use any widget as card content, not just title/description
- **Details View Integration** - Automatically generate card layouts with `.ToDetails()`
- **Full Widget Support** - Cards can contain forms, charts, buttons, badges, or any other widget
- **Flexible Builders** - Use the builder factory to create cards from your data models

**API Changes:**

The Kanban API has been significantly refactored for better usability:

- **Simplified Structure** - `KanbanColumn` has been removed; columns are now automatically generated from card grouping
- **Card-Centric API** - `Kanban` now takes `KanbanCard[]` directly instead of nested columns
- **Event Naming** - `HandleMove` renamed to `HandleCardMove` for clarity
- **Removed OnAdd** - The column "add" button feature has been removed; create custom add workflows instead
- **Column Configuration** - Set column widths with `.Width(e => e.Status, Size.Fraction(0.33f))`

**Migration Guide:**

```csharp
// Old API (before)
data.ToKanban(groupBySelector: e => e.Status, ...)
    .HandleMove(moveData => {
        // Had FromColumn parameter
        var from = moveData.FromColumn;
        var to = moveData.ToColumn;
    })
    .HandleAdd(columnKey => {
        // Column add button clicked
    })
    .ColumnTitle(status => GetCustomTitle(status));

// New API (after)
data.ToKanban(groupBySelector: e => e.Status, ...)
    .CardBuilder(factory => factory.Func<T, T>(item =>
        new Card(content: item.ToDetails())
    ))
    .HandleCardMove(moveData => {
        // FromColumn parameter removed - derive from card data
        var to = moveData.ToColumn;
    })
    .HandleClick(cardId => {
        // Handle card clicks
    });
```

**Breaking Changes:**

- **`HandleMove()` renamed to `HandleCardMove()`** - Update all event handlers
- **`FromColumn` removed from move event** - Derive source column from the card being moved
- **`HandleAdd()` removed** - Create custom "add card" workflows instead
- **`ColumnTitle()` removed** - Column titles now use the group key's `ToString()` value
- **`KanbanColumn` widget removed** - Columns are generated automatically from card grouping

**Frontend Improvements:**

The frontend Kanban implementation has been completely rewritten:

- Improved drag-and-drop performance and reliability
- Better card ordering and positioning
- Fixed edge cases with card movement between columns
- Cleaner component architecture with proper separation of concerns

**Default Sizing:**

The `KanbanBuilder` now includes sensible default sizing:

- **Width** defaults to `Size.Full()` - Takes up available horizontal space
- **Height** defaults to `Size.Full()` - Takes up available vertical space

You can still override these defaults with `.Width()` or `.Height()` as needed.

### Fixed Kanban Card Reordering Logic

Fixed a critical bug in the Kanban widget's card reordering logic that could cause cards to be inserted at incorrect positions when dragging between columns or reordering within the same column. The fix optimizes list operations and ensures cards always land in the exact position where they're dropped.

**What was fixed:**

The Kanban `HandleCardMove` handler now:

- **Correctly calculates insertion index** - Finds the actual index of the task at the target position, rather than trying to calculate it from column-relative indices
- **Optimized list operations** - Uses `Remove()` instead of `RemoveAll()` for better performance
- **Creates new task instances** - Properly clones tasks when moving them to prevent reference mutation issues
- **Handles edge cases** - Correctly inserts cards when dropped at the end of a column or in empty columns

**In your HandleCardMove handler:**

```csharp
.HandleCardMove(moveData =>
{
    var updatedTasks = tasks.Value.ToList();
    var taskToMove = updatedTasks.FirstOrDefault(t => t.Id == moveData.CardId);
    if (taskToMove == null) return;

    // Create new task with updated status
    var newTask = new Task
    {
        Id = taskToMove.Id,
        Title = taskToMove.Title,
        Status = moveData.ToColumn,
        // ... other properties
    };

    // Remove old task reference
    updatedTasks.Remove(taskToMove);

    // Find correct insertion index
    int insertIndex = updatedTasks.Count;
    var taskAtTargetIndex = updatedTasks
        .Where(t => t.Status == moveData.ToColumn)
        .ElementAtOrDefault(moveData.TargetIndex ?? -1);

    if (taskAtTargetIndex != null)
    {
        insertIndex = updatedTasks.IndexOf(taskAtTargetIndex);
    }
    else
    {
        // Append to end of column
        var lastTaskInColumn = updatedTasks.LastOrDefault(t => t.Status == moveData.ToColumn);
        if (lastTaskInColumn != null)
        {
            insertIndex = updatedTasks.IndexOf(lastTaskInColumn) + 1;
        }
    }

    updatedTasks.Insert(insertIndex, newTask);
    tasks.Set(updatedTasks.ToArray());
})
```

This fix ensures smooth, predictable drag-and-drop behavior in Kanban boards, with cards always landing exactly where users drop them.

### Clickable Links in Callouts

Callout widgets now support clickable links in their content. Previously, if you included markdown links in a callout, they would render as plain text. Now, links are automatically detected and converted to clickable elements that trigger navigation events.

**How it works:**

When you create a callout with markdown links, the framework automatically:

1. Converts markdown links to `app://` format during documentation generation
2. Attaches a link click handler using the new `HandleLinkClick()` extension method
3. Triggers navigation when users click the links

**Usage:**

```csharp
// In your documentation markdown, links in callouts now work:
// <Callout Type="info">
// Check out the [installation guide](docs/installation) for details.
// </Callout>

// The generated code automatically includes link handling:
new Callout("Check out the installation guide for details.", icon: Icons.Info)
    .HandleLinkClick(onLinkClick)
```

**API Changes:**

- Added `HandleLinkClick(Action<string> onLinkClick)` extension method on `Callout`
- The method wraps markdown content with a link click handler that invokes the provided callback
- Callouts in documentation now support the `Type` attribute (`tip`, `info`, `warning`, `error`, `success`) that automatically maps to appropriate icons

This makes callouts more interactive and useful in documentation apps, allowing you to guide users to related resources without leaving the callout as passive text.

### Card Header Slot System

The Card widget has been refactored to use a slot-based header system, providing more flexibility for custom header layouts. Instead of fixed `Title`, `Description`, and `Icon` properties, the Card now uses a `Header` slot that can contain any content.

**What Changed:**

The `Title`, `Description`, and `Icon` properties are now internal implementation details. The public API uses extension methods that create layouts internally:

```csharp
// The API remains the same - this still works
var card = new Card(content: "Card body content")
    .Title("My Card")
    .Description("Card description")
    .Icon(Icons.Star);

// But now you can also provide custom header content
var card = new Card(
    content: "Card body content",
    header: Layout.Horizontal()
        | Text.Block("Custom Header")
        | Button.Default("Action", Icons.Plus)
);
```

**Header Layout:**

When using the convenience methods (`Title()`, `Description()`, `Icon()`), the framework creates this layout internally:

```csharp
Layout.Horizontal()
    | (Layout.Vertical().Gap(0) | title | description)
    | icon
```

The description text automatically uses the new `Colors.Muted` color for subtle styling.

**Flexible Icon Support:**

The `Card.Icon()` method now accepts any object, not just `Icons` enums. If you pass an `Icons` value, it's automatically converted to an icon widget with black coloring. If you pass any other widget, it's used directly as the header icon:

```csharp
// Using Icons enum (automatically converted)
card.Icon(Icons.Star);

// Using custom widget
card.Icon(new Badge("New"));

// Using custom icon with different styling
card.Icon(Icons.Alert.ToIcon().Color(Colors.Red));
```

**Benefits:**

- **Backward Compatible** - Existing code using `Title()`, `Description()`, and `Icon()` continues to work unchanged
- **More Flexible** - You can now provide fully custom header content via the `header` parameter, or pass any widget as the icon
- **Better Architecture** - The slot system is consistent with how other Ivy widgets handle customizable sections

This change lays the groundwork for more advanced card headers, such as headers with action buttons, badges, or custom layouts, while maintaining a simple API for common cases.

### Integrated Ivy Design System for Centralized Theming

The framework has been enhanced with the **Ivy Design System** - a centralized, token-based theming solution that replaces hardcoded color values across both backend and frontend. This integration provides a single source of truth for design tokens, making theme customization easier and ensuring perfect synchronization between backend theme generation and frontend CSS.

**What Changed:**

**Backend:**

- The framework now includes a NuGet package reference to `Ivy.DesignSystem` (updated to v1.1.5)
- All color definitions in `ThemeConfig.cs` now use design system tokens instead of hardcoded hex values
- 47+ hardcoded colors replaced with semantic token references
- The API remains backward compatible - your existing `ThemeColors` configuration still works
- Theme token paths have been simplified to use shorter, cleaner references (`LightThemeTokens.Color.*` and `DarkThemeTokens.Color.*`)

**Before:**

```csharp
Primary = "#00cc92",
PrimaryForeground = "#000000",
Secondary = "#dfe7e3",
// ... more hardcoded colors
```

**After:**

```csharp
Primary = LightThemeTokens.Color.Primary,
PrimaryForeground = LightThemeTokens.Color.PrimaryForeground,
Secondary = LightThemeTokens.Color.Secondary,
// ... using design system tokens
```

**Frontend:**

- The frontend now imports the `ivy-design-system` npm package
- Removed ~150 lines of hardcoded CSS variables from `index.css`
- CSS variables now come from flat CSS files (`ivy-framework-flat.css`, `dark-flat.css`) provided by the design system
- Only framework-specific variables (shadows, spacing, toolbox colors) remain in the codebase

**Chart and Sidebar Colors Removed:**

- Simplified the theme system by removing `Chart` (Chart1-5) and `Sidebar` color properties from `ThemeColors`
- These properties were framework-specific and not part of the core design system
- Existing sidebar colors now use the muted color tokens from the design system
- This change is breaking if you were customizing chart or sidebar colors directly

**Benefits:**

- **Consistency** - Backend-generated themes and frontend CSS are always in sync
- **Maintainability** - Change a color once in the design system, and it updates everywhere
- **Customization** - Easier to create custom themes by overriding design tokens
- **Brand Alignment** - All Ivy-based applications share consistent design language

**Migration Notes:**

If you have custom theme configurations that reference the removed properties (`Chart1-5`, `Sidebar*` colors), you'll need to remove these from your `ThemeColors` definitions. The framework will work without them, using the design system defaults instead.

**Example - Remove these properties:**

```csharp
// Remove these from your ThemeColors configuration:
Chart1 = "#0077BE",
Chart2 = "#DC143C",
Sidebar = "#f8f8f8",
SidebarForeground = "#000000",
// ... etc
```

The integration includes proper CI/CD support with NuGet and npm package references, ensuring smooth builds in both development and production environments.

## Security Improvements

### Link URL Validation and Sanitization

The framework now includes comprehensive URL validation and sanitization to protect against XSS attacks, open redirect vulnerabilities, and other injection attacks. All URLs used in links, buttons, redirects, and navigation are now validated before use.

**What's Protected:**

The following components now validate URLs before using them:

- **Button Links** - `Button.Url()` validates URLs and throws `ArgumentException` for dangerous protocols
- **Link Builder** - `LinkBuilder` creates disabled buttons when URLs are invalid
- **Navigation** - `Navigate()` and `Redirect()` validate destination URLs
- **Markdown Links** - All markdown links are sanitized before rendering
- **DataTable Links** - Link cells validate URLs before opening
- **External URL Opening** - `OpenUrl()` validates before opening new windows

**Allowed URL Types:**

The validation system allows these safe URL patterns:

- **HTTP/HTTPS URLs** - `https://example.com`, `http://localhost:5000`
- **Relative Paths** - `/dashboard`, `/users/profile`
- **App Protocol** - `app://dashboard`, `app://MyApp?param=value`
- **Anchor Links** - `#section1`, `#section:value`

**Blocked Patterns:**

Dangerous URL patterns are rejected:

- `javascript:alert('xss')` - JavaScript protocol injection
- `data:text/html,<script>` - Data URI XSS
- `file:///etc/passwd` - File protocol access
- `vbscript:msgbox('xss')` - VBScript execution
- `/path:with:colons` - Relative paths with protocol injection attempts

**Usage Examples:**

```csharp
// Button URL validation - throws ArgumentException for dangerous URLs
try
{
    var button = new Button("Click Me").Url("javascript:alert('xss')");
}
catch (ArgumentException ex)
{
    // ex.Message: "Invalid URL: javascript:alert('xss'). Only safe URLs are allowed."
}

// Safe URLs work normally
var safeButton = new Button("Go Home").Url("/dashboard"); // ✓ Valid
var externalButton = new Button("GitHub").Url("https://github.com"); // ✓ Valid
var appButton = new Button("Settings").Url("app://settings"); // ✓ Valid

// Link Builder creates disabled buttons for invalid URLs
var linkBuilder = new LinkBuilder<string>("javascript:alert('xss')", "Dangerous");
var result = linkBuilder.Build("dummy", context);
// Result: Button with Disabled=true and no URL

// Navigation validates URLs
client.Redirect("javascript:alert('xss')"); // Throws ArgumentException
client.Redirect("/dashboard"); // ✓ Valid
client.OpenUrl("https://example.com"); // ✓ Valid
```

**Validation API:**

Three new utility methods have been added to the `Utils` class:

```csharp
// Validate redirect URLs (for server-side redirects)
string? validatedUrl = Utils.ValidateRedirectUrl(url, allowExternal: false);
if (validatedUrl == null)
{
    // URL is invalid or external when not allowed
}

// Validate link URLs (for buttons, markdown, etc.)
string? validatedUrl = Utils.ValidateLinkUrl(url);
if (validatedUrl == null)
{
    // URL uses dangerous protocol or invalid format
}

// Validate app IDs (for app:// URLs)
bool isSafe = Utils.IsSafeAppId(appId);
if (!isSafe)
{
    // App ID contains unsafe characters like :, ?, #, etc.
}
```

**Frontend Protection:**

The frontend also includes URL validation:

```typescript
// TypeScript validation functions
import { validateRedirectUrl, validateLinkUrl } from '@/lib/utils';

// Validate before redirecting
const safeUrl = validateRedirectUrl(url, false);
if (safeUrl) {
    window.location.href = safeUrl;
}

// Validate before rendering links
const safeHref = validateLinkUrl(href);
// Returns '#' for invalid URLs instead of null
```

**App Protocol Query Parameters:**

The `app://` protocol now supports query parameters for passing data to apps:

```csharp
// Query parameters are now allowed and validated
var button = new Button("Open Report")
    .Url("app://Reports?reportId=123&format=pdf"); // ✓ Valid

// Fragments and protocol injection are still blocked
var button = new Button("Dangerous")
    .Url("app://Reports#fragment"); // ✗ Throws ArgumentException
```

**Anchor Links with Colons:**

Anchor links can now contain colons (HTML5 allows this):

```csharp
// Colons in anchor IDs are now allowed
var button = new Button("Jump to Section")
    .Url("#section:subsection:detail"); // ✓ Valid
```

**Breaking Changes:**

- **`Button.Url()`** now throws `ArgumentException` for invalid URLs instead of silently accepting them
- **`LinkBuilder`** now returns disabled buttons for invalid URLs instead of rendering them as clickable
- **`client.Redirect()`** now validates URLs and throws for invalid destinations
- **`NavigateArgs.AppId`** now validates app IDs and throws for unsafe characters

**Security Best Practices:**

1. **Always use the validation APIs** when constructing URLs from user input
2. **Never bypass validation** by concatenating strings to build URLs
3. **Test with attack vectors** like `javascript:`, `data:`, and `file:` protocols
4. **Use relative paths** when linking within your application
5. **Validate external URLs** before opening them with `client.OpenUrl()`

This security enhancement protects your Ivy applications from common web vulnerabilities without requiring changes to your existing code - dangerous URLs that previously would have worked will now be blocked with clear error messages.
