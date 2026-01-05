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

**Configuration & Submission:**

Forms now support async submit handlers, upload awareness (auto-disabling submit during uploads), and flexible button customization.

```csharp
model.ToForm()
    .ValidationStrategy(FormValidationStrategy.OnSubmit)
    .SubmitBuilder(isLoading => new Button("Save Changes").Loading(isLoading))
    .HandleSubmit(async (user) => await SaveAsync(user));
```

**Comprehensive DataAnnotations Support:**

Full support for standard attributes to configure fields automatically:

- **Display & Labels:** `[Display]` for labels/grouping. Smart generation logic preserves explicit names like "User ID" while correctly trimming suffixes for others.
- **Input Types:** `[DataType]` automatically selects widgets (Email, Password, Url, CreditCard).
- **Validation:** Supports `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[AllowedValues]`, and more.
- **Control:** `[ScaffoldColumn(false)]` to hide fields.

See the [Forms documentation](https://docs.ivy.app/onboarding/concepts/forms) for complete details.

## Input Widgets

### AsyncSelectInput Enhancements

- **Option Descriptions:** Options support optional descriptions appearing below labels:

```csharp
new Option<string>(
    label: "Germany",
    value: "DE",
    description: "Europe"  // Appears below label
)
```

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

Row actions are enhanced with better event handling. **Important**: You must specify `idSelector` when using row actions to properly identify rows:

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

### Standardized Scale API

The `Sizes` enum has been renamed to `Scale` throughout the framework. All components (Forms, Inputs, Tables, Expandables) now support consistent `.Small()`, `.Medium()`, and `.Large()` configuration methods. Form inputs default to `Medium` if unspecified.

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

## What's Changed

- (release): quick fixes by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1579
- (kanban): fix build error in docs by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1595
- feat: enhance FormBuilder with comprehensive DataAnnotations support by @nielsbosma in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1518
- [FileInput]: update implementation based on Copilot review by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1581
- [Chrome]: for ?chrome=false fix redirect to prev and next page links by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1583
- (routing): resolve custom route conflicts and prevent app ID collisions by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1580
- (codex): Cleanup unnecessary logging statements by @zachwolfe in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1591
- [HotFix]: update renamed API methods in kanban and table by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1601
- [Expandable]: implement scale by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1608
- [Time]: style improvements by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1606
- [Scale]: set scale medium by default by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1604
- fix: adjust chart yaxis min when negative values are present by @ViktorWb in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1600
- fix: don't vary logging message templates by @zachwolfe in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1602
- (kanban): API updates, simplification and fix of Header Layout issues by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1612
- Add Stepper widget with selectable steps by @nielsbosma in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1615
- (kanban): add example with card click by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1619
- (form): size api fixes by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1621
- [AsyncSelect]: make dividers go all the width by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1568
- [Security]: validate urls to avoid Redirecting to a URL that is constructed from parts of the DOM by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1544
- (form): FE api refactoring by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1625
- (DemoBox): (removal): replace with Box.Plain and smart TOC by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1623
- (async select): new styles that look good when mixed with other elements by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1628
- [Grid]: Implement new Grid API and Color Opacity by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1624
- New grid and cohort implementation by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1632
- (expandable): ui updates to match everything in forms by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1636
- (expandable): chevron icon updates by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1637
- (expandable): handle edge cases with switches by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1638
- [Kanban]: clear column highlight after drag ends by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1640
- [Charts]: show tools only on hover if enabled by @ArtemLazarchuk in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1616
- [Docs]: Sync docs with Release v1.1.1 by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1645
- (codex): format main readme by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1655
- (blades): fix misleading view name by @joshuauaua in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1658
- (onboarding): remove unneccessary numbers in list in docs by @joshuauaua in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1648
- (kanban): Kanban drag interactions and styling by @dcrjodle in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1657
- (selects): refactor docs for select and async select by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1661
- (kanban): scroll bar padding and rounded corners by @dcrjodle in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1666
- (forms): better logic for handling cutoff by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1665
- (tables): actually work with new API by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1670
- (docs): made alert buttons horizontal layout by @joshuauaua in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1668
- (routing): Implement 404 error for unrecognized apps by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1560
- (table): propery implement align api by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1674
- (codex): fix remove parent padding by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1673
- (tooltip): multiline text in tooltip if it's long by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1688
- (kanban): demo card ordering by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1685
- (codex): implement proper usage of remove parent padding for footer and async select by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1683
- [Grid]: improve text contrast with opacity in dark mode by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1660
- (datatable): complete FE solution rework by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1656
- [Docs]: fix dialog form width in few app examples in Forms by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1698
- (options): fix missed param in ctor by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1701
- (auth): get tokens out of frontend, add `IAuthSession` interface with checked access by @zachwolfe in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1607
- (fix): Removed focus:ring (green color) from git codespaces button by @KaiserReich95 in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1699
- (chore): Refactor of TextInput by @KaiserReich95 in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1696
- (stepper): implement docs by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1689
- [Sheet]: Implement use case in docs to show sheet creation algorithm in complete layouts by @ArtemKhvorostianyi in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1582
- [tabs]: move underline inside container bounds by @defymecobra in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1702
- fix: errors sent by AppHub during connection are masked by @zachwolfe in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1710
- (tabs): adjust colors by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1714
- (docs): add conceptual documentation for Apps and the `[App]` attribute by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1711
- Remove nullable Scale prop from input widgets by @nielsbosma in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1718
- (fix): fix semantic structure and styling for cards by @dcrjodle in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1700
- feat: prepare for patchnotes by @rorychatt in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1697

## New Contributors

- @joshuauaua made their first contribution in https://github.com/Ivy-Interactive/Ivy-Framework/pull/1658

**Full Changelog**: https://github.com/Ivy-Interactive/Ivy-Framework/compare/v1.1.1...v1.1.2
