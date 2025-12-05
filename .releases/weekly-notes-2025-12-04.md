# Ivy Framework Weekly Notes - Week of 2025-12-04

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## Overview

This release introduces major improvements to form scaffolding with comprehensive DataAnnotations support, a new Stepper widget for multi-step workflows, enhanced Grid layout system with advanced control APIs, significant Kanban widget API simplifications, a major authentication security refactoring that moves tokens completely out of the frontend with a new `IAuthSession` interface and improved cross-tab authentication synchronization. The framework now includes better routing collision detection and custom 404 error page support.

## Improvements

### Stepper Widget

New `Stepper` widget for multi-step processes:

```csharp
var currentStep = UseState(0);

var steps = new[]
{
    new StepperItem("1", Icons.Check, "Company", "Basic info"),
    new StepperItem("2", null, "Raise", "Funding details"),
    new StepperItem("3", null, "Deck", "Presentation"),
    new StepperItem("4", null, "Founders", "Team info")
};

new Stepper(
    onSelect: async e => currentStep.Set(e.Value),
    selectedIndex: currentStep.Value,
    items: steps
)
.AllowSelectForward();
```

### Form Scaffolding

**Upload-Aware Form Submission:**

Forms automatically prevent submission while file uploads are in progress:

- Submit button disabled during uploads
- Toast notification: "File uploads are still in progress. Please wait for them to complete."
- Applies to standard forms, sheet forms, and dialog forms

**Enhanced Form Configuration API:**

Forms now offer more flexible configuration options for submit buttons, validation strategies, scaling, and comprehensive DataAnnotations support.

**Submit Button Customization:**

```csharp
// Simple title change
model.ToForm()
    .SubmitTitle("Create Account")
    .HandleSubmit(async (user) => await CreateAsync(user));

// Full button customization
model.ToForm()
    .SubmitBuilder(isLoading => new Button("Save Changes")
        .Variant(ButtonVariant.Primary)
        .Loading(isLoading)
        .Icon(Icons.Save))
    .HandleSubmit(async (user) => await SaveAsync(user));
```

**Default Submit Button:**

Forms now automatically get a default "Save" button if none specified.

**Async Submit Handler:**

`.HandleSubmit()` accepts async callback receiving validated model:

```csharp
model.ToForm()
    .HandleSubmit(async (user) =>
    {
        // Runs after validation passes
        await _database.SaveUserAsync(user);
        // Model state updated after callback completes
    });
```

**Validation Strategy:**

Chainable `.ValidationStrategy()` method:

```csharp
model.ToForm()
    .ValidationStrategy(FormValidationStrategy.OnSubmit)
    .SubmitTitle("Save");
```

- `Scale Configuration` – the `.Scale()` method is now part of the public API:

```csharp
model.ToForm()
    .Scale(Scale.Small)   // or .Medium(), .Large()
    .SubmitTitle("Save");
```

- `Improved Form Spacing and Typography` – Enhanced spacing and typography for better readability and visual hierarchy

- `Comprehensive DataAnnotations Support` - full support for DataAnnotations for automatic field configuration and validation

- `Display Attributes` - use `[Display]` attributes to control field labels, descriptions, placeholders, ordering, and grouping:

**Input Type Detection:**

Automatically detects input type from `[DataType]` attributes:

- `[DataType(DataType.EmailAddress)]` for Email input
- `[DataType(DataType.Password)]` for Password input
- `[DataType(DataType.Url)]` for URL input
- `[DataType(DataType.CreditCard)]` for Credit card input

**Validation Attributes:**

All major validation attributes supported:

- `[Required]`, `[StringLength]`, `[Range]`, `[RegularExpression]`
- `[EmailAddress]`, `[Phone]`, `[Url]`, `[CreditCard]`
- `[AllowedValues]`, `[Compare]`

**ScaffoldColumn Control:**

```csharp
[ScaffoldColumn(false)]
public Guid Id { get; set; }  // Hidden from form
```

**Fixed Label Generation:**

Improved logic for handling label generation when field names end with "Id":

- Custom labels specified with `[Display]` attributes are now correctly preserved without modification
- Only trims "Id" suffix from auto-generated labels (not from Display attribute names)
- Checks if the label itself ends with "Id" before trimming, preventing incorrect truncation
- Preserves labels like "User ID", "Government ID", and "Id" when specified via Display attributes

### Form Input Size Consistency

All form inputs now follow a unified sizing system.

Size variants simplified from enum-based to string literals for easier usage.

## Input Widgets

### AsyncSelectInput Enhancements

**Scale Support:**

AsyncSelectInput now fully supports the standard scale system:

```csharp
selectedOption.ToAsyncSelectInput(QueryOptions, LookupOption, "Search...")
    .Small();   // h-7
    .Medium();  // h-9 (default)
    .Large();   // h-11
```

Scale affects height, padding, text size, icon size, and search sheet styling.

**Additional Enhancements:**

- **Visual Integration:** Styling now matches other form inputs with consistent borders, shadows, and hover states for seamless integration in forms
- **Full-Width Dividers:** Search sheet list items use full-width dividers for better visual separation
- **Option Descriptions:** Options support optional descriptions appearing below labels:

```csharp
new Option<string>(
    label: "Germany",
    value: "DE",
    description: "Europe"  // Appears below label
)
```

**Option Icons:**

Options now support icons for visual indicators:

```csharp
new Option<string>("Active", "active", icon: Icons.CheckCircle)
```

**Optional Labels:**

The `Label` property is now nullable - when omitted, uses `value.ToString()` as fallback.

### DateTime Input Visual Improvements

- Calendar and clock icons positioned inside input fields
- Consistent disabled state styling matching DateRange inputs
- Clear and error icons use absolute positioning with optimized spacing

### File Input Improvements

**Event Handlers:**

```csharp
files.ToFileInput(upload)
    .HandleBlur((Event<IAnyInput> e) => {
        // Fires when file dialog closes (selected or cancelled)
    })
    .HandleCancel((Guid fileId) => {
        // Fires when user clicks X button on a file
        upload.Value.Cancel(fileId);
    });
```

### Field Widget

FieldWidget now supports custom width and height properties. Field widgets also support explicit width and height directly:

```csharp
state.ToTextInput()
    .Width("300px")
    .Height("40px");
```

### Kanban Widget

**CardBuilder Now Required:**

The Kanban widget now requires `.CardBuilder()` - simple `titleSelector` and `descriptionSelector` parameters removed:

```csharp
// Before - no longer supported
tasks.ToKanban(
    groupBySelector: e => e.Status,
    titleSelector: e => e.Title,
    descriptionSelector: e => e.Description)

// After - CardBuilder required
tasks.ToKanban(
    groupBySelector: e => e.Status,
    idSelector: e => e.Id,
    orderSelector: e => e.Priority)
.CardBuilder(task => new Card()
    .Title(task.Title)
    .Description(task.Description)
    .HandleClick(() => showTaskSheet(task.Id)))  // Card click example
```

**HandleMove Renamed:**

`.HandleCardMove()` renamed to `.HandleMove()` for consistency:

```csharp
tasks.ToKanban(...)
    .HandleMove(moveData => {
        // Handle card movement
    });
```

**Removed Event Handlers:**

`.HandleClick()` and `.HandleDelete()` removed from Kanban API. Implement click/delete functionality within `.CardBuilder()` instead using Card's `.HandleClick()` method.

**Column Width Changes:**

Column width configuration simplified - use `.ColumnWidth()` for uniform width across all columns:

```csharp
tasks.ToKanban(...)
    .Width(Size.Full())              // Overall board width
    .ColumnWidth(Size.Rem(20))       // Uniform column width (enables horizontal scroll)
```

**Custom Card Ordering:**

Use `.CardOrder()` to sort cards within columns independently of global `orderSelector`:

```csharp
tasks.ToKanban(
    groupBySelector: e => e.Status,
    idSelector: e => e.Id,
    orderSelector: e => e.Priority)  // Global ordering
.CardBuilder(task => new Card()
    .Title(task.Title)
    .Description(task.Description))
.CardOrder(e => e.DueDate)  // Order cards by due date within each column
```

**Drag-and-Drop Improvements:**

- Cards correctly reorder when dragged within same column or between columns
- Column highlights properly clear after drag operations complete
- Drop position indicators show exact insertion point
- Smooth animations when cards shift
- Improved scroll bar padding and rounded corners

**Simplified Width and Height Methods:**

Methods now accept only `Size` parameters - use `Size.Units()`, `Size.Fraction()`, etc.:

```csharp
// Before
tasks.ToKanban(...).Width(800).Height(600);

// After
tasks.ToKanban(...).Width(Size.Units(800)).Height(Size.Units(600));
```

### HeaderLayout Widget

Disable automatic ScrollArea wrapper for custom scrolling:

```csharp
new HeaderLayout(header, content)
    .Scroll(Scroll.None);  // Content handles its own scrolling
```

Kanban boards can now be used in HeaderLayout with scrolling disabled.

### Table Widget

**API Change: Width to ColumnWidth:**

The `.Width()` method for setting column widths renamed to `.ColumnWidth()`:

```csharp
// Before
products.ToTable()
    .Width(e => e.Sku, Size.Fraction(0.15f));

// After
products.ToTable()
    .ColumnWidth(e => e.Sku, Size.Fraction(0.15f));
```

**Column Width and Alignment:**

The `.Align()` method properly aligns content within both header and data cells:

```csharp
records.ToTable()
    .ColumnWidth(e => e.Views, Size.Fit())
    .Align(e => e.Views, Align.Right);
```

### DataTable Widget

**Row Action Improvements:**

Row actions enhanced with better event handling. **Important**: You must specify `idSelector` when using row actions to properly identify rows:

```csharp
users.ToDataTable(idSelector: e => e.Id)
    .Column(e => e.Name)
    .RowActions(
        MenuItem.Default(Icons.Pencil, "edit").Tooltip("Edit employee"),
        MenuItem.Default(Icons.EllipsisVertical, "menu")
            .Children([
                MenuItem.Default(Icons.Archive, "archive").Label("Archive")
            ])
    )
    .HandleRowAction(async e =>
    {
        var userId = e.Value.Id;   // Direct access to row ID
        var action = e.Value.Tag;  // Menu item tag
    });
```

**Key Changes:**

`RowActionClickEventArgs` simplified:

- **New**: `Id` (row's unique identifier), `Tag` (menu item's tag)
- **Removed**: `ActionId`, `EventName`, `RowIndex`, `RowData`

**Event Handling Improvements:**

DataTable event handling now uses row IDs throughout, replacing less reliable row index and data dictionary access. Toast messages and event arguments now consistently use row IDs for more robust event tracking.

**Improved Performance with Arrow Tables:**

DataTable now uses Apache Arrow's columnar storage format internally for better memory efficiency and performance with large datasets. Transparent to existing code - no changes required.

**Column Resizing:**

DataTable now supports column resizing out of the box. Users can drag column borders to adjust widths. Column widths preserved during session.

To disable:

```csharp
users.ToDataTable()
    .Config(c => c.AllowColumnResizing = false);
```

### Charts

- Chart toolbox controls now only appear when hovering over chart (if enabled)
- Charts now correctly handle negative values by automatically adjusting Y-axis minimum to include negative ranges

### Grid Layout

**Improved Dark Mode Contrast:**

Grid layouts now have improved text contrast in dark mode when using opacity. The fix uses a CSS variable `--opacity-mix-color` that switches between white (light mode) and black (dark mode), ensuring text stays readable in both themes.

**Enhanced Grid API:**

**Column and Row Sizing:**

```csharp
Layout.Grid()
    .Columns(3)
    .ColumnWidths(Size.Px(100), Size.Fraction(1), Size.Px(150))
    .RowHeights(Size.Px(60), Size.Fraction(1), Size.Fraction(1))
```

**Header and Footer Builders:**

```csharp
Layout.Grid()
    .Columns(4)
    .HeaderBuilder((columnIndex, cell) =>
        cell.WithCell().Color(Colors.Green).Content($"Header {columnIndex}"))
    .FooterBuilder((columnIndex, cell) =>
        cell.WithCell().Color(Colors.Blue).Content($"Total: {cell}"))
```

**Cell Builder:**

```csharp
Layout.Grid()
    .Columns(3)
    .CellBuilder(cell => cell.WithCell().Color(Colors.Gray))
```

**WithCell() Extension:**

Creates borderless boxes that fill entire grid cell:

```csharp
"Fills cell".WithCell()
"No borders".WithCell().Color(Colors.Blue)
```

**Color Opacity Support:**

Box widget now supports color opacity (0.0 to 1.0):

```csharp
new Box("50% opacity").Color(Colors.Blue, 0.5f)
```

Perfect for heatmaps, cohort analysis, and visual hierarchies.

### Component Sizing

**Sizes Renamed to Scale:**

The `Sizes` enum renamed to `Scale` throughout framework:

```csharp
// Before
button.Size(Sizes.Medium);

// After
button.Scale(Scale.Medium);
// Or use convenience methods
button.Small();
button.Medium();
button.Large();
```

**Nullable Scale Property:**

The `Scale` property on `WidgetBase` is now nullable, allowing widgets to inherit size from parent components.

**Medium Scale as Default:**

All form inputs and tables now default to `Scale.Medium` when no scale explicitly specified.

### Expandable Widget

**Scale Support:**

```csharp
new Expandable(header, content)
    .Small()   // Compact
    .Medium()  // Default
    .Large()   // Emphasized
```

**Improvements:**

- Chevron icon uses absolute positioning with optimized sizing
- Interactive elements (buttons, switches, links) in header remain clickable when expandable is disabled
- Click handling properly distinguishes between interactive elements and expandable toggle

### Box Widget

**Plain() Extension Method:**

New `Box.Plain()` extension method provides a reusable preset for demo/documentation styling:

```csharp
new Box().Plain().Content(content)
```

Applies DemoBox-style settings: 1px border, 16px padding, neutral color, top-left alignment. This replaces the removed `DemoBox` widget for a more consistent API.

### Button Variants

**SkinnyGhost Variant:**

A new `SkinnyGhost` button variant provides ultra-compact button layouts for tight spaces:

```csharp
new Button("Edit")
    .SkinnyGhost()
    .Icon(Icons.Pencil);
```

Features minimal padding (`p-1`) with auto height, ideal for table rows, toolbars, or icon-only buttons.

### Alert Dialogs

**Improved Button Layout:**

Alert dialog buttons now follow standard UI conventions:

- All buttons right-aligned in footer
- Button order: Cancel (secondary) | No | Yes (primary)
- Primary actions consistently on the right

### Tooltips

- Multiline text support with maximum width constraint
- Long strings without spaces use `break-all` for proper wrapping
- Table cell tooltips use `whitespace-pre-wrap` for proper formatting

### List Widget

**Full-Width Dividers:**

List widget dividers now extend the full width of the container for better visual separation.

### Loading Widget

- Fixed, full-screen overlay with semi-transparent dark background
- 200ms display delay to prevent jarring flashes for quick operations
- Can be conditionally rendered based on state: `isLoading.True(() => new Loading())!`

### Layout System

**TopCenter Alignment:**

New `Layout.TopCenter()` method for horizontally-aligned layouts with top-center alignment:

```csharp
Layout.TopCenter(
    new Button("Action 1"),
    new Button("Action 2"),
    new Button("Action 3")
)
```

### State Management

New convenience methods for state:

```csharp
var counter = UseState(0);
counter.Incr(); // Increment by 1
counter.Decr(); // Decrement by 1

var isLoading = UseState(false);
return isLoading.True(() => new Loading())!;  // Show when true
return isLoading.False(() => new Button("Load Data"))!; // Show when false
```

### Utilities

New `Utils.FormatNumber()` utility for formatting large numbers:

```csharp
Utils.FormatNumber(1500);      // "1.5K"
Utils.FormatNumber(2500000);    // "2.5M"
Utils.FormatNumber(3800000000); // "3.8B"
```

### Authentication

**Tokens Moved Out of Frontend:**

Authentication tokens are now completely managed server-side and no longer exposed to the frontend. New `IAuthSession` interface encapsulates authentication state. All `IAuthProvider` methods now accept `IAuthSession` instead of token strings or `AuthToken` objects. New cookie registry system provides secure server-side cookie management with automatic cleanup. Token registry IDs now use ~256 bits of strong entropy (up from 122 bits).

**Cross-Tab Logout Synchronization:**

Logout events are synchronized across browser tabs using the Broadcast Channel API. When a user logs out in one tab, all other tabs automatically reload to reflect the logout state.

**Cross-Tab Login Synchronization:**

When a user logs in one tab, all other tabs with the same `machineId` automatically reload to pick up the new authentication state.

### Routing

**404 Not Found Page:**

When users navigate to non-existent app, Ivy displays proper 404 error page. Customize with `server.UseErrorNotFound<MyCustomNotFoundApp>()`.

**App ID Collision Detection:**

Ivy automatically detects and prevents routing conflicts between app IDs and framework routes. Reserve custom paths:

```csharp
server.ReservePaths("/admin", "/reports", "/dashboard")
    .RegisterApp<MyApp>("users")
    .Start();
```

### Chrome Customization

Simpler generic syntax for custom chrome:

```csharp
server.UseChrome<MyCustomChrome>();
```

### Article Widget

Previous/next navigation links now preserve `chrome=false` parameter when navigating between articles.

### Theming System

Documentation updated to reflect actual color variables in Ivy Design System. Removed documentation for unused variables (`Chart1-5`, `Sidebar`, `SidebarForeground`).

## Breaking Changes

### Kanban Widget API Simplification

**CardBuilder Now Required:**

The simple `titleSelector` and `descriptionSelector` parameters have been removed. You must now use `.CardBuilder()`:

```csharp
// Old API - no longer supported
tasks.ToKanban(
    groupBySelector: e => e.Status,
    titleSelector: e => e.Title,
    descriptionSelector: e => e.Description)

// New API - CardBuilder required
tasks.ToKanban(
    groupBySelector: e => e.Status,
    idSelector: e => e.Id,
    orderSelector: e => e.Priority)
.CardBuilder(task => new Card()
    .Title(task.Title)
    .Description(task.Description))
```

**HandleCardMove Renamed to HandleMove:**

```csharp
// Before
tasks.ToKanban(...).HandleCardMove(moveData => { /* ... */ });

// After
tasks.ToKanban(...).HandleMove(moveData => { /* ... */ });
```

**Removed Event Handlers:**

The `.HandleClick()` and `.HandleDelete()` methods have been removed from Kanban API. Implement click/delete functionality within your `.CardBuilder()` instead using Card's `.HandleClick()` method.

**Column Width Changes:**

Per-column width configuration removed. Use `.ColumnWidth()` for uniform width:

```csharp
// Old API - per-column widths
tasks.ToKanban(...)
    .Width(e => e.Status, Size.Fraction(0.33f))

// New API - uniform column width
tasks.ToKanban(...)
    .ColumnWidth(Size.Rem(20))
```

**Simplified Width and Height Methods:**

Methods now accept only `Size` parameters:

```csharp
// Before
tasks.ToKanban(...).Width(800).Height(600);

// After
tasks.ToKanban(...).Width(Size.Units(800)).Height(Size.Units(600));
```

### Table Widget API Change

**Width to ColumnWidth:**

The `.Width()` method for setting column widths has been renamed to `.ColumnWidth()`:

```csharp
// Before
products.ToTable()
    .Width(e => e.Sku, Size.Fraction(0.15f));

// After
products.ToTable()
    .ColumnWidth(e => e.Sku, Size.Fraction(0.15f));
```

### DataTable Row Actions API Change

**Simplified Event Arguments:**

The `RowActionClickEventArgs` structure has been simplified:

```csharp
// Before
.HandleRowAction(async e => {
    var userId = e.Value.RowData["Id"];
    var action = e.Value.ActionId;
});

// After
.HandleRowAction(async e => {
    var userId = e.Value.Id;   // Direct access
    var action = e.Value.Tag;   // Menu item tag
});
```

**Removed Properties:**

- `ActionId` - Use `Tag` instead
- `EventName` - Removed
- `RowIndex` - Removed
- `RowData` - Use `Id` instead

**idSelector Required for Row Actions:**

When using row actions, you must specify `idSelector` to properly identify rows:

```csharp
users.ToDataTable(idSelector: e => e.Id)  // Required when using row actions
    .RowActions(...)
```

Without `idSelector`, the `Id` property in `RowActionClickEventArgs` will be null.

### Component Sizing Changes

**Sizes Renamed to Scale:**

The `Sizes` enum has been renamed to `Scale`:

```csharp
// Before
button.Size(Sizes.Medium);

// After
button.Scale(Scale.Medium);
// Or use convenience methods
button.Medium();
```

**Medium Scale as Default:**

All form inputs and tables now default to `Scale.Medium` when no scale explicitly specified. If you previously relied on undefined scale behavior, components will now render at Medium scale.

### Authentication API Changes

**IAuthProvider Interface Breaking Changes:**

All `IAuthProvider` implementations must be updated to use the new `IAuthSession` interface. All methods now accept `IAuthSession` instead of token strings or `AuthToken` objects:

```csharp
public async Task<AuthToken?> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken)
public Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken)
public async Task<AuthToken?> RefreshAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken)
public Task<bool> ValidateAccessTokenAsync(IAuthSession authSession, CancellationToken cancellationToken)
public Task<UserInfo?> GetUserInfoAsync(IAuthSession authSession, CancellationToken cancellationToken)
public Task<Uri> GetOAuthUriAsync(IAuthSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
public Task<AuthToken?> HandleOAuthCallbackAsync(IAuthSession authSession, HttpRequest request, CancellationToken cancellationToken)
```

**Method Signature Changes:**

- `GetTokenExpiration(AuthToken, ...)` - `GetAccessTokenExpirationAsync(IAuthSession, ...)`
- `SetHttpContext(HttpContext)` - `InitializeAsync(IAuthSession, string requestScheme, string requestHost, ...)`

**AuthService Constructor:**

`AuthService` constructor now requires `IAuthSession`, `IClientProvider`, and `AppSessionStore`:

```csharp
var authSession = AuthHelper.GetAuthSession(httpContext);
var authService = new AuthService(authProvider, authSession, clientProvider, sessionStore);
```

## Security Improvements

### Enhanced URL Validation

Ivy Framework now includes comprehensive URL validation across all components to prevent open redirect vulnerabilities and other URL-based security issues.

**What's Protected:**

All URL-accepting components validate and sanitize URLs automatically:

- Links in markdown content
- Images, audio, and video players
- Button widgets with URL targets
- Any redirect or navigation URLs

**Validation Rules:**

- **Allowed**: Relative paths, http/https URLs, data URLs (for appropriate media types), blob URLs (with origin validation), `app://` protocol URLs, anchor links
- **Blocked**: `javascript:` protocol, malformed URLs, protocol injection attempts, dangerous URL patterns

**Validation Functions:**

The framework uses centralized validation functions for different URL types:

- `validateLinkUrl()` - For anchor tags and navigation
- `validateImageUrl()` - For image sources
- `validateAudioUrl()` - For audio sources
- `validateVideoUrl()` - For video sources
- `validateRedirectUrl()` - For redirect operations

**Blob URL Security:**

Blob URLs validated to ensure they match current origin, preventing attacks where malicious blob URLs from other origins could be injected. Properly handles default ports and localhost development scenarios.

**Error Handling:**

Invalid URLs show clear, user-friendly error messages:

- Images/Audio/Video: Bordered error box with message
- Buttons: "Invalid button URL" in destructive-styled container
- Links: Converted to safe anchor links (`#`)

## Bug Fixes

- **Kanban Card Reordering**: Fixed bug causing cards to be inserted at incorrect positions when dragging between columns
- **Kanban Drag Visual Feedback**: Fixed column highlights persisting after drag operations complete
- **Kanban Build Error**: Fixed build error in documentation examples
- **Table Column Widths**: Fixed handling of `Size.Units()` when only some columns have explicit widths set
- **Table Layout**: Improved table layout logic - fixed layout for Full() width tables, auto layout for fixed width tables to allow natural expansion
- **Table Cell Truncation**: Fixed truncation logic to only apply when explicit column width is set or for header cells, allowing natural sizing for data cells without widths
- **DataTable Row Actions**: Fixed event handling requiring `idSelector` for proper row identification
- **Article Navigation**: Fixed navigation links losing `chrome=false` parameter when navigating between articles
- **Tooltip Wrapping**: Fixed tooltips not properly wrapping long strings without spaces
- **Chart Y-Axis**: Fixed Y-axis always starting at 0, cutting off negative data points
- **Form Label Generation**: Fixed label generation logic - now only trims "Id" suffix from auto-generated labels, preserves custom Display attribute names, and checks if label itself ends with "Id" before trimming
- **Grid Dark Mode**: Fixed text contrast issues in dark mode when using opacity for proper text readability
- **Loading Widget**: Fixed missing overlay and delay timing for better UX
- **Logging Templates**: Fixed inconsistent logging message templates that caused warnings
- **Codex Logging**: Cleaned up unnecessary logging statements
- **FileInput OnBlur**: Fixed double-firing of blur events when files are selected. Blur now fires correctly when dialog closes
- **Form Scale Application**: Fixed issue where form scale wasn't being applied to async select inputs and submit buttons
- **List Widget Dividers**: Fixed dividers not extending full width of container
- **Option Enum Description**: Fixed missing null parameter in Option enum extension method
- **Field Widget Dimensions**: Fixed width and height not being applied to field widgets
- **Routing Collisions**: Fixed app ID collisions with custom routes. Server now validates app IDs don't conflict with reserved paths
- **URL Validation**: Fixed various edge cases in URL validation for images, audio, video, and links
- **Padding Removal**: Updated padding removal class from `remove-ancestor-padding` to `remove-parent-padding` for more predictable and maintainable padding behavior across widgets
- **Option Constructor**: Fixed missing parameter in Option constructor when creating enum options - now properly passes all 5 parameters (label, value, group, description, icon)
