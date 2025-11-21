# Ivy Framework Weekly Notes - Week of 2025-11-21

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## Overview

This release introduces comprehensive URL validation and security improvements, a complete TabsLayout redesign with performance enhancements, major DataTable improvements (nested menus, cell actions, footer support), Kanban card builder API, integrated Ivy Design System for centralized theming, and extensive documentation updates including new architecture guides. The framework routes have been reorganized under the `/ivy` prefix for better namespace isolation.

## Improvements

### Button Loading State

Buttons now support binding loading state directly to reactive state objects:

```csharp
var loading = this.UseState(false);

async ValueTask OnClick()
{
    loading.Set(true);
    await Task.Delay(2000);
    loading.Set(false);
}

return new Button("Submit")
    .HandleClick(OnClick)
    .Loading(loading); // Automatically shows loading when true
```

### Tooltip Support

**Menu Items:**

```csharp
MenuItem.Default("Save", Icons.Save)
    .Tooltip("Save your changes to the server");
```

**DataTable Row Actions:**

Row actions support tooltips through the `Tooltip` property. The `RowAction` class has been changed to a record for better immutability.

### Simplified UseTrigger Hook

New simple overload for cases where you don't need to pass data:

```csharp
var (dialogView, showDialog) = this.UseTrigger((open) =>
    new Dialog("Confirm", "Are you sure?")
        .OnClose(() => open.Set(false))
);

return Layout.Vertical()
    | Button.Primary("Show Dialog", () => showDialog())
    | dialogView;
```

### Form Improvements

**Horizontal Field Placement:**

```csharp
form.PlaceHorizontal(m => m.FirstName, m => m.LastName) // Side-by-side
    .Place(m => m.Email); // Stacked vertically
```

**Form Input Size Consistency:**

All form input fields now have consistent sizing across different input types.

**Field Widget Dimensions:**

```csharp
state.ToTextInput()
    .Width("300px")
    .Height("40px");
```

**Form Groups Open by Default:**

```csharp
var form = model.ToForm()
    .Group("Personal Information", open: true, m => m.Name, m => m.Email)
    .Group("Contact Details", m => m.PhoneNumber); // Collapsed by default
```

### DataTable Enhancements

**Row Actions with MenuItem API:**

Row actions now use `MenuItem` instead of `RowAction` class, supporting nested menus:

```csharp
data.ToDataTable()
    .Header(e => e.Name, "Name")
    .RowActions(
        MenuItem.Default(Icons.Pencil, "edit").Tooltip("Edit employee"),
        MenuItem.Default(Icons.EllipsisVertical, "menu")
            .Children([
                MenuItem.Default(Icons.Archive, "archive").Label("Archive"),
                MenuItem.Default(Icons.Download, "export").Label("Export")
            ])
    )
    .HandleRowAction(async e =>
    {
        var actionId = e.Value.ActionId;
        var rowIndex = e.Value.RowIndex;
        var rowData = e.Value.RowData; // Dictionary keyed by column name
    });
```

**Cell Actions:**

```csharp
data.ToDataTable()
    .Header(e => e.Email, "Email")
    .Renderer(e => e.Email, new LinkDisplayRenderer { Type = LinkDisplayType.Url })
    .HandleCellAction(e => e.Email, (email) =>
    {
        client.Toast($"Email clicked: {email}");
    });
```

**Link Rendering:**

```csharp
data.ToDataTable()
    .Header(e => e.ProfileUrl, "Profile")
    .Renderer(e => e.ProfileUrl, new LinkDisplayRenderer { Type = LinkDisplayType.Url })
```

- Custom link styling with blue text and underlines
- External links open in new focused tab
- Relative URLs navigate in same tab
- Ctrl+Click / Cmd+Click support

**Visual Refinements:**

- Sorted column headers have subtle light gray background
- Row action buttons with refined styling and hover effects
- Row hover uses muted colors for subtler effect
- Column type icons now default to `false` (opt-in)

**Footer Support:**

DataTable now supports footer content that overlays the bottom of the table, with smart whitespace handling and proper z-index management.

**Configuration:**

- `ShowColumnTypeIcons` defaults to `false`
- Added `ButtonDisplayRenderer` for future button cell support
- Improved code formatting and consistency

### Chart Toolboxes Now Opt-In

Chart toolboxes are now opt-in rather than automatically included:

```csharp
// Before - toolbox was automatic
data.ToAreaChart()
    .Dimension("Month", e => e.Month)
    .Measure("Desktop", e => e.Sum(f => f.Desktop));

// After - explicitly add toolbox
data.ToAreaChart()
    .Dimension("Month", e => e.Month)
    .Measure("Desktop", e => e.Sum(f => f.Desktop))
    .Toolbox(); // Add this line
```

All chart builders support three `.Toolbox()` overloads for customization.

### Complete TabsLayout Redesign

**Performance Improvements:**

- Eliminated content re-renders on tab reorder using hash-based children stability check
- Smooth drag-and-drop with `translate3d()` instead of CSS scale transforms
- Better responsive overflow calculation with debouncing, resize observers, and mutation observers

**UX Enhancements:**

- Dropdown tab selection swaps positions with last visible tab
- Improved tab styling with better visual hierarchy
- Updated tab spacing with overlapping borders
- Close button on inactive tabs only visible on hover

**Code Architecture:**

The monolithic `TabsLayoutWidget.tsx` (1080 lines) has been split into a well-organized module structure with hooks, components, and utilities, each with comprehensive unit tests.

### DBML Editor UI Improvements

- Fixed table width (240px) for consistent layout
- Field name tooltips for long names/types
- Improved connection handles visibility and z-index handling
- Better pointer event handling for smooth dragging

### Sidebar Improvements

- Toggle button repositioned for better alignment
- Scrollbar visibility improved with z-index handling (`z-20`)
- User avatar images now properly displayed (previously only showed initials)

### Footer Menu Items Transformer

Dynamically customize footer menu items:

```csharp
var chromeSettings = ChromeSettings.Default()
    .UseFooterMenuItemsTransformer((items, navigator) =>
    {
        var list = items.ToList();
        list.Add(new MenuItem("Logout", _ => navigator.Navigate("app://logout"), Icons.Logout));
        return list;
    });
```

### Alert Dialog Button Positioning

Alert dialogs now display buttons with improved positioning following platform UI conventions. Primary actions (Ok, Yes) appear on the right, secondary/cancel buttons on the left.

### Supabase Auth Improvements

The Supabase authentication provider now gracefully handles missing user metadata fields (`full_name`, `avatar_url`), preventing authentication failures when these optional fields are not present.

### Entity Framework Core Update

Updated to Entity Framework Core 9.0.11 across all database-related packages.

### Select Input Improvements

**Ellipsis and Tooltips:**

Select inputs now handle long option labels with text truncation and automatic tooltips. Same functionality applied to `AsyncSelectInput`.

**Option Descriptions:**

```csharp
var options = new[]
{
    new Option<string>("Standard Shipping", "standard", description: "Delivery in 5-7 business days"),
    new Option<string>("Express Shipping", "express", description: "Delivery in 2-3 business days")
};

state.ToAsyncSelectInput()
    .Placeholder("Select shipping method")
    .Load(async () => options);
```

**AsyncSelectInput HeaderLayout:**

The widget now uses `HeaderLayout` internally for cleaner visual structure. `HeaderLayout` includes `ShowHeaderDivider` property (defaults to `true`).

### Kanban Improvements

**Card Builder API:**

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
    // Handle drag-and-drop
});
```

**Default Sizing:**

- Width defaults to `Size.Full()`
- Height defaults to `Size.Full()`

### Callout Links

Callout widgets now support clickable links in their content. Links are automatically detected and converted to clickable elements that trigger navigation events.

### Card Header Slot System

Card widget refactored to use slot-based header system:

```csharp
// Existing API still works
var card = new Card(content: "Card body content")
    .Title("My Card")
    .Description("Card description")
    .Icon(Icons.Star);

// Custom header content
var card = new Card(
    content: "Card body content",
    header: Layout.Horizontal()
        | Text.Block("Custom Header")
        | Button.Default("Action", Icons.Plus)
);
```

The `Card.Icon()` method now accepts any object, not just `Icons` enums.

### Integrated Ivy Design System

The framework now uses the **Ivy Design System** - a centralized, token-based theming solution:

**Backend:**

- NuGet package reference to `Ivy.DesignSystem` (v1.1.5)
- Hardcoded colors replaced with semantic token references
- Theme token paths simplified (`LightThemeTokens.Color.*`, `DarkThemeTokens.Color.*`)

**Frontend:**

- Imports `ivy-design-system` npm package
- CSS variables from flat CSS files provided by design system

**Breaking Change:**

Removed `Chart` (Chart1-5) and `Sidebar` color properties from `ThemeColors`. These were framework-specific and not part of the core design system. Existing sidebar colors now use muted color tokens.

### UI Refinements

- **Sheet Widget**: Improved vertical spacing
- **Copy-to-Clipboard Button**: Refined to better integrate with shadcn/ui design system
- **Audio Recorder**: Automatic MIME type fallback with browser compatibility detection
- **Blade Widget**: Optional `title` prop for custom header slots
- **Optional Blade Title**: Prevents rendering empty `<h2>` tag when no title provided

## New Features

### DataTable Row Actions with Nested Menus

Row actions now support nested dropdowns using `MenuItem.Children()`, providing better organization for related actions under a "more actions" menu.

### DataTable Cell Actions

Attach click handlers to specific cells using `HandleCellAction` method.

### DataTable Footer Support

Footer content overlays the bottom of the table with fixed positioning, smart whitespace handling, and proper z-index management.

### Kanban Card Builder

Custom card content through `.CardBuilder()` API, enabling rich card layouts with any widgets.

### Option Descriptions in Select Inputs

Options can include descriptions that display as subtitles in dropdown lists.

### Clickable Links in Callouts

Callout widgets automatically detect and convert markdown links to clickable navigation elements.

### Card Header Slot System

Flexible header system supporting custom layouts while maintaining backward compatibility.

## Breaking Changes

### Framework Routes Now Use `/ivy` Prefix

All Ivy framework endpoints have been reorganized under a single `/ivy` prefix:

- **SignalR Hub:** `/messages` → `/ivy/messages`
- **Authentication:** `/auth/*` → `/ivy/auth/*`
- **Webhooks:** `/webhook` → `/ivy/webhook`
- **File Upload:** `/upload/*` → `/ivy/upload/*`
- **File Download:** `/download/*` → `/ivy/download/*`
- **Static Assets:** `/assets/*` → `/ivy/assets/*`
- **Health Check:** `/health` → `/ivy/health`

**What You Need to Update:**

1. **Authentication Callback URLs** - Update redirect URIs in OAuth provider dashboards:

   ```
   Before: http://localhost:5010/webhook
   After:  http://localhost:5010/ivy/webhook
   ```

2. **Image Paths** - Update asset references:

   ```csharp
   // Before
   new Image("/assets/logo.png")
   
   // After
   new Image("/ivy/assets/logo.png")
   ```

3. **Health Check Monitoring** - Update URLs in monitoring tools:

   ```
   Before: http://localhost:5010/health
   After:  http://localhost:5010/ivy/health
   ```

### Chart Toolboxes Now Opt-In

Chart toolboxes are now opt-in. Add `.Toolbox()` explicitly if you need export/transform functionality.

### DataTable Row Actions API Change

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

**Breaking Changes:**

- `HandleMove()` renamed to `HandleCardMove()`
- `FromColumn` removed from move event
- `HandleAdd()` removed
- `ColumnTitle()` removed
- `KanbanColumn` widget removed

### Integrated Ivy Design System

**Removed Properties:**

If you have custom theme configurations, remove these properties:

- `Chart1` through `Chart5`
- `Sidebar` and `SidebarForeground`

The framework will work without them, using design system defaults.

## Security Improvements

### Link URL Validation and Sanitization

Comprehensive URL validation and sanitization to protect against XSS attacks, open redirect vulnerabilities, and injection attacks.

**What's Protected:**

- Button Links - `Button.Url()` validates URLs and throws `ArgumentException` for dangerous protocols
- Link Builder - Creates disabled buttons when URLs are invalid
- Navigation - `Navigate()` and `Redirect()` validate destination URLs
- Markdown Links - All markdown links are sanitized before rendering
- DataTable Links - Link cells validate URLs before opening
- External URL Opening - `OpenUrl()` validates before opening new windows

**Allowed URL Types:**

- HTTP/HTTPS URLs
- Relative paths (`/dashboard`, `/users/profile`)
- App Protocol (`app://dashboard`, `app://MyApp?param=value`)
- Anchor links (`#section1`, `#section:value`)

**Blocked Patterns:**

- `javascript:alert('xss')` - JavaScript protocol injection
- `data:text/html,<script>` - Data URI XSS
- `file:///etc/passwd` - File protocol access
- `vbscript:msgbox('xss')` - VBScript execution
- `/path:with:colons` - Relative paths with protocol injection attempts

**Validation API:**

```csharp
// Validate redirect URLs
string? validatedUrl = Utils.ValidateRedirectUrl(url, allowExternal: false);

// Validate link URLs
string? validatedUrl = Utils.ValidateLinkUrl(url);

// Validate app IDs
bool isSafe = Utils.IsSafeAppId(appId);
```

**Breaking Changes:**

- `Button.Url()` now throws `ArgumentException` for invalid URLs
- `LinkBuilder` returns disabled buttons for invalid URLs
- `client.Redirect()` validates URLs and throws for invalid destinations
- `NavigateArgs.AppId` validates app IDs and throws for unsafe characters

## CLI & Tooling

### Headless Database Generation

The `ivy db generate` command now supports console mode with `--use-console` flag for headless operation. Requires `--prompt`, `--dbml`, or STDIN input, `--yes-to-all`, and `--skip-debug`.

### Automatic EF Migration Creation

The command now automatically creates Entity Framework migrations after building the database generator project.

### Database Generation Bug Fix

Fixed issue where NuGet package references were being lost during database generation. The generator now properly preserves references like `Ivy.Database.Generator.Toolkit` and `Microsoft.EntityFrameworkCore.Design`.

### Improved Error Reporting

Better error handling with specific exit codes:

- `20`: DBML validation error
- `30`: Database generator build error
- `40`: EF migration error
- `50`: Database generator run error
- `60`: Project build error

### Dashboard App No Longer Auto-Generated

The database generator no longer automatically adds a Dashboard app. Only apps explicitly returned by the AI agent are included.

### Custom AI Model Selection

```bash
ivy db generate --model-id claude-3-5-sonnet-20241022
```

### Model Cache Control

Hidden `--model-disable-cache` flag available in `ivy db generate`, `ivy app create`, and `ivy fix` commands for advanced scenarios.

### Parallel App Generation

Apps are now generated concurrently, significantly improving performance.

### Better CLI Exit Codes for App Creation Failures

Both `ivy app create` and `ivy db generate` now properly return non-zero exit codes when app generation fails:

- `ivy app create` returns exit code `1` if any app fails
- `ivy db generate` returns exit code `55` if any app fails

## Bug Fixes

- **Nullable Number Field Clearing**: Fixed issue where clicking X button on nullable number fields (`int?`, `double?`, `decimal?`) would not properly clear the value
- **DataTable Filter Syntax Highlighting**: Fixed visual issue where syntax highlighting remained active even when query was invalid
- **Authentication Refresh Loop**: Fixed critical bug that could cause infinite loop when auth provider returned same invalid token during refresh attempts
- **App Protocol URL Parsing**: Fixed bug where `app://` URLs were incorrectly truncating app identifiers (removed 7 chars instead of 6)
- **Kanban Card Reordering**: Fixed bug causing cards to be inserted at incorrect positions when dragging between columns
- **Table Column Widths**: Fixed handling of `Size.Units()` when only some columns have explicit widths set
- **Routing and Connection**: Fixed edge cases in default app selection, chrome parameter handling, reconnection robustness, and auth failure recovery
