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

Learn more about the Stepper widget in the [documentation](https://docs.ivy.app/widgets/primitives/stepper).

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

See the [Forms documentation](https://docs.ivy.app/onboarding/concepts/forms) for complete details.

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

Explore the [FileInput documentation](https://docs.ivy.app/widgets/inputs/file) for complete API reference.

### Field Widget

FieldWidget now supports custom width and height properties. Field widgets also support explicit width and height directly:

```csharp
state.ToTextInput()
    .Width("300px")
    .Height("40px");
```

Refer to the [Field Widget documentation](https://docs.ivy.app/widgets/inputs/field) for additional details.

### Kanban Widget

**CardBuilder Now Required:**

The Kanban widget now requires `.CardBuilder()` - simple `titleSelector` and `descriptionSelector` parameters removed:

```csharp
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

`.HandleCardMove()` renamed to `.HandleMove()` for consistency.

**Removed Event Handlers:**

`.HandleClick()` and `.HandleDelete()` removed from Kanban API. Implement click/delete functionality within `.CardBuilder()` instead using Card's `.HandleClick()` method.

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

See the [Kanban documentation](https://docs.ivy.app/widgets/advanced/kanban) for a complete guide.

### HeaderLayout Widget

Disable automatic ScrollArea wrapper for custom scrolling:

```csharp
new HeaderLayout(header, content)
    .Scroll(Scroll.None);  // Content handles its own scrolling
```

Documentation: [HeaderLayout](https://docs.ivy.app/widgets/layouts/header-layout).

### Table Widget

**Column Width and Alignment:**

The `.Align()` method properly aligns content within both header and data cells:

```csharp
records.ToTable()
    .ColumnWidth(e => e.Views, Size.Fit())
    .Align(e => e.Views, Align.Right);
```

More information in the [Table documentation](https://docs.ivy.app/widgets/common/table).

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

**Column Resizing:**

DataTable now supports column resizing out of the box. Users can drag column borders to adjust widths. Column widths preserved during session.

To disable:

```csharp
users.ToDataTable()
    .Config(c => c.AllowColumnResizing = false);
```

Complete API reference: [DataTable documentation](https://docs.ivy.app/widgets/advanced/data-table).

### Grid Layout

**Column and Row Sizing:**

```csharp
Layout.Grid()
    .Columns(3)
    .ColumnWidths(Size.Px(100), Size.Fraction(0.5f), Size.Px(150))
    .RowHeights(Size.Px(60), Size.Fraction(0.5f), Size.Fraction(1))
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

Learn more: [Grid Layout documentation](https://docs.ivy.app/widgets/layouts/grid-layout).

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

### Expandable Widget

**Scale Support:**

```csharp
new Expandable(header, content)
    .Small()   // Compact
    .Medium()  // Default
    .Large()   // Emphasized
```

Check the [Expandable documentation](https://docs.ivy.app/widgets/common/expandable) for more examples.

### Box Widget

**Plain() Extension Method:**

New `Box.Plain()` extension method provides a reusable preset for demo/documentation styling:

```csharp
new Box().Plain().Content(content)
```

Documentation: [Box widget](https://docs.ivy.app/widgets/primitives/box).

### Button Variants

**SkinnyGhost Variant:**

A new `SkinnyGhost` button variant provides ultra-compact button layouts for tight spaces:

```csharp
new Button("Edit")
    .SkinnyGhost()
    .Icon(Icons.Pencil);
```

Read the [Button documentation](https://docs.ivy.app/widgets/common/button) for all variants and options.

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

See the [State Management documentation](https://docs.ivy.app/onboarding/concepts/state) for best practices.

### Utilities

New `Utils.FormatNumber()` utility for formatting large numbers:

```csharp
Utils.FormatNumber(1500);      // "1.5K"
Utils.FormatNumber(2500000);    // "2.5M"
Utils.FormatNumber(3800000000); // "3.8B"
```

### Authentication

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

See [Chrome Customization documentation](https://docs.ivy.app/onboarding/concepts/chrome) for examples.

## Breaking Changes

### Component Sizing API Changes

**Size Parameters Required:**

All width/height methods now require `Size` parameters instead of numeric values:

```csharp
widget.Width(Size.Units(800)).Height(Size.Units(600));
kanban.ColumnWidth(Size.Rem(20));  // Uniform width
```

**Method Renames:**

- **Table**: `.Width()` → `.ColumnWidth()` for column-specific widths
- **Kanban**: Per-column `.Width()` removed, use `.ColumnWidth()` for uniform width

```csharp
// Table
products.ToTable().ColumnWidth(e => e.Sku, Size.Fraction(0.15f));  // Was .Width()

// Kanban
tasks.ToKanban(...).ColumnWidth(Size.Rem(20));  // Uniform width
```

### Kanban Widget API Simplification

- **CardBuilder required**: `titleSelector`/`descriptionSelector` removed. Use `.CardBuilder()` instead
- **`.HandleCardMove()` - `.HandleMove()`**: Method renamed
- **Removed handlers**: `.HandleClick()` and `.HandleDelete()` removed. Use Card's `.HandleClick()` within `.CardBuilder()`

```csharp
tasks.ToKanban(..., idSelector: e => e.Id, orderSelector: e => e.Priority)
    .CardBuilder(task => new Card().Title(task.Title).Description(task.Description))
    .HandleMove(...);
```

### DataTable Row Actions API Change

- **Simplified event args**: `e.Value.Id` and `e.Value.Tag` instead of `e.Value.RowData["Id"]` and `e.Value.ActionId`
- **Removed**: `ActionId` (use `Tag`), `EventName`, `RowIndex`, `RowData` (use `Id`)
- **`idSelector` required**: Must specify when using row actions

```csharp
users.ToDataTable(idSelector: e => e.Id)  // Required
    .RowActions(...)
    .HandleRowAction(async e => {
        var userId = e.Value.Id;   // Direct access
        var action = e.Value.Tag;   // Was ActionId
    });
```

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

Comprehensive URL validation across all components to prevent open redirect vulnerabilities and XSS attacks.

- **Protected components**: Links, images, audio/video players, buttons, redirects
- **Allowed**: Relative paths, http/https URLs, data URLs, blob URLs (origin-validated), `app://` protocol, anchor links
- **Blocked**: `javascript:` protocol, malformed URLs, protocol injection attempts
- **Blob URL security**: Validates origin matching to prevent cross-origin attacks
- **Error handling**: Invalid URLs show user-friendly error messages or are converted to safe fallbacks

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
