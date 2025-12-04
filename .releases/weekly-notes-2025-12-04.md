# Ivy Framework Weekly Notes - Week of 2025-12-04

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## Overview

This release introduces major improvements to form scaffolding with comprehensive DataAnnotations support, a new Stepper widget for multi-step workflows, enhanced Grid layout system with advanced control APIs, significant Kanban widget API simplifications, DataTable performance optimizations using Apache Arrow, improved authentication with cross-tab logout synchronization, and extensive UI refinements across buttons, inputs, tables, and charts. The framework now includes better routing collision detection and custom 404 error page support.

## Improvements

### Button Variants

**SkinnyGhost Variant:**

A new `SkinnyGhost` button variant provides ultra-compact button layouts for tight spaces:

```csharp
new Button("Edit")
    .SkinnyGhost()
    .Icon(Icons.Pencil);
```

Features minimal padding (`p-1`) with auto height, ideal for table rows, toolbars, or icon-only buttons.

### DateTime Input Visual Improvements

**Icon Positioning:**
- Calendar and clock icons now positioned inside input fields
- Consistent spacing across all date/time variants
- Clock icon appears inside Time input field

**Disabled State:**
- Consistent styling matching DateRange inputs
- Reduced opacity for icons in disabled inputs
- Proper `not-allowed` cursor on hover

**Clear and Error Icon Layout:**
- Absolute positioning for reliable placement
- Optimized spacing to prevent text overlap
- Automatic padding adjustment based on visible icons

### Form Input Size Consistency

All form inputs now follow a unified sizing system:
- **Small**: `h-7` with `px-2` padding
- **Medium**: `h-9` with `px-3` padding  
- **Large**: `h-11` with `px-4` padding

Size variants simplified from enum-based to string literals for easier usage.

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

**Visual Integration:**
- Chevron icon uses absolute positioning with optimized sizing
- Reduced opacity (50%) for subtle appearance
- Better text alignment without excessive margins

**Full-Width Dividers:**

Dropdown dividers now extend to full width for cleaner appearance.

**Option Descriptions:**

Options support optional descriptions appearing below labels:

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

### File Input Improvements

**Enhanced Event Handlers:**

**OnBlur Handler:**
Fires when file dialog closes (whether files selected or cancelled):

```csharp
files.ToFileInput(upload)
    .HandleBlur((Event<IAnyInput> e) =>
    {
        if (files.Value.Length > 0)
            Console.WriteLine($"{files.Value.Length} file(s) selected");
    });
```

**OnCancel Handler:**
Fires when user clicks X button on a file:

```csharp
files.ToFileInput(upload)
    .HandleCancel((Guid fileId) =>
    {
        upload.Value.Cancel(fileId);
        files.Set(list => list.Where(f => f.Id != fileId).ToImmutableArray());
    });
```

**Consolidated Documentation:**

File upload documentation merged into comprehensive FileInput widget documentation with unified examples and patterns.

### Field Widget

**Width and Height Support:**

FieldWidget now supports custom width and height properties:

```csharp
<FieldWidget
  label="Username"
  width="300px"
  height="auto"
>
  {/* Your input component */}
</FieldWidget>
```

When not specified, maintains default flexible behavior for backward compatibility.

### Kanban Improvements

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
    .Description(task.Description))
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

`.HandleClick()` and `.HandleDelete()` removed. Implement click/delete functionality within `.CardBuilder()` instead.

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
tasks.ToKanban(...)
    .CardOrder(e => e.DueDate)  // Sort by due date within each column
```

**Fixed Card Reordering Logic:**

Cards now correctly reorder when dragged within same column or between columns. Fix addresses edge cases with end-of-column positioning and insertion index calculation.

**Improved Drag Visual Feedback:**

Column highlights properly clear after drag operations complete. Drag-over state centralized in Kanban context.

**Enhanced Drag-and-Drop Interactions:**

- Drop position indicators show exact insertion point
- Smooth animations (0.2s ease) when cards shift
- Improved column styling with accent background on drag-over
- Refined scrollbars (1.5 units instead of 2.5)

**Simplified Width and Height Methods:**

Methods now accept only `Size` parameters - use `Size.Units()`, `Size.Fraction()`, etc.:

```csharp
// Before
tasks.ToKanban(...).Width(800).Height(600);

// After
tasks.ToKanban(...).Width(Size.Units(800)).Height(Size.Units(600));
```

### HeaderLayout Widget

**Scroll Control:**

Disable automatic ScrollArea wrapper for custom scrolling:

```csharp
new HeaderLayout(header, content)
    .Scroll(Scroll.None)  // Content handles its own scrolling
```

When `.Scroll(Scroll.None)` is set, HeaderLayout automatically sets height to `Size.Full()` if no explicit height provided.

### Alert Dialogs

**Improved Button Layout:**

Alert dialog buttons now follow standard UI conventions:
- All buttons right-aligned in footer
- Button order: Cancel (secondary) | No | Yes (primary)
- Primary actions consistently on the right

### Theming System

**Streamlined Color Palette:**

Documentation updated to reflect actual color variables in Ivy Design System. Removed documentation for unused variables (`Chart1-5`, `Sidebar`, `SidebarForeground`).

Current supported theme colors focus on Main, Semantic, and UI Elements categories.

### Article Widget

**Fixed Navigation in Chrome=False Mode:**

Previous/next navigation links now preserve `chrome=false` parameter when navigating between articles, preventing unexpected chrome mode toggling.

### Tooltips

**Multiline Text Support:**

- Maximum width constrained to `max-w-sm` (24rem/384px)
- Long strings without spaces use `break-all` for proper wrapping
- Table cell tooltips use `whitespace-pre-wrap` for proper formatting

### Utilities

**Number Formatting:**

New `Utils.FormatNumber()` utility for formatting large numbers:

```csharp
Utils.FormatNumber(1500);           // "1.5K"
Utils.FormatNumber(2500000);        // "2.5M"
Utils.FormatNumber(3800000000);     // "3.8B"
Utils.FormatNumber(1234567, 1);     // "1.2M" (1 decimal place)
```

### Authentication

**Cross-Tab Logout Synchronization:**

When logging out in one browser tab, all other tabs automatically reload and reflect logged-out state using Broadcast Channel API. Works in all modern browsers (Chrome, Firefox, Edge, Safari 15.4+).

### Form Scaffolding

**Upload-Aware Form Submission:**

Forms automatically prevent submission while file uploads are in progress:
- Submit button disabled during uploads
- Toast notification: "File uploads are still in progress. Please wait for them to complete."
- Applies to standard forms, sheet forms, and dialog forms

**Enhanced Form Configuration API:**

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

**Scale Configuration:**

`.Scale()` method now part of public API.

**Improved Form Spacing and Typography:**

Scale-appropriate spacing between fields and submit button:
- Small: 4px gap, `text-xs`
- Medium: 6px gap, `text-sm` (default)
- Large: 8px gap, `text-base`

**Comprehensive DataAnnotations Support:**

**Display Attributes:**

```csharp
[Display(Name = "Full Name", Description = "Enter your name",
         Prompt = "John Doe", Order = 1, GroupName = "Personal Info")]
public string Name { get; set; }
```

**Input Type Detection:**

Automatically detects input type from `[DataType]` attributes:
- `[DataType(DataType.EmailAddress)]` → Email input
- `[DataType(DataType.Password)]` → Password input
- `[DataType(DataType.Url)]` → URL input
- `[DataType(DataType.CreditCard)]` → Credit card input

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

**Universal Placeholder Support:**

All input widgets support `.Placeholder()` method:

```csharp
textState.ToTextInput().Placeholder("Enter your name");
numberState.ToNumberInput().Placeholder("0.00");
dateState.ToDateTimeInput().Placeholder("Select a date");
```

**Fixed Label Generation:**

Custom labels specified with `[Display]` attributes now correctly preserved when field names end with "Id".

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

Expandable now supports standard scale system:

```csharp
new Expandable(header, content)
    .Small()   // Compact (h-7)
    .Medium()  // Default (h-9)
    .Large()   // Emphasized (h-11)
```

Scale affects height, padding, text size, chevron icon size, and content spacing.

**Improved Icon Positioning:**

- Chevron icon uses absolute positioning with right-alignment
- Optimized icon widths (w-5/6/8 instead of w-7/9/11)
- Reduced opacity (50%) for subtle appearance
- Removed vertical border line separating chevron

**Interactive Elements in Disabled Expandables:**

When Expandable is disabled, interactive elements within header (buttons, switches, links) remain clickable. Only the expandable toggle is disabled.

**Improved Click Handling:**

Click handling properly distinguishes between clicking interactive elements and clicking expandable header itself.

### Table Widget

**API Change: Width → ColumnWidth:**

The `.Width()` method for setting column widths renamed to `.ColumnWidth()`:

```csharp
// Before
products.ToTable()
    .Width(e => e.Sku, Size.Fraction(0.15f));

// After
products.ToTable()
    .ColumnWidth(e => e.Sku, Size.Fraction(0.15f));
```

**Improved Column Width and Text Wrapping:**

- Full width tables use `table-layout: fixed` to respect column constraints
- Fixed width tables use `table-layout: auto` for natural sizing
- Multi-line cells use `wrap-break-word` for better wrapping
- Header cells always truncate with tooltips

**Enhanced Column Alignment API:**

The `.Align()` method now properly aligns content within both header and data cells using `text-align` CSS properties:

```csharp
records.ToTable()
    .ColumnWidth(e => e.Views, Size.Fit())
    .Align(e => e.Views, Align.Right);  // Right-align numbers
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

**Improved Performance with Arrow Tables:**

DataTable now uses Apache Arrow's columnar storage format internally for better memory efficiency and performance with large datasets. Transparent to existing code - no changes required.

**Column Resizing:**

DataTable now supports column resizing out of the box. Users can drag column borders to adjust widths. Column widths preserved during session.

To disable:
```csharp
users.ToDataTable()
    .Config(c => c.AllowColumnResizing = false);
```

### Chrome Customization

**Generic UseChrome Method:**

Simpler generic syntax for custom chrome:

```csharp
// Before
server.UseChrome(() => new MyCustomChrome());

// After
server.UseChrome<MyCustomChrome>();
```

### Charts

**Toolbox on Hover:**

Chart toolbox controls now only appear when hovering over chart, providing cleaner appearance by default.

**Fixed Y-Axis Rendering with Negative Values:**

Charts now correctly handle negative values by automatically adjusting Y-axis minimum to include negative ranges. Previously, Y-axis would always start at 0, cutting off negative data points.

### Stepper Widget

**New Component:**

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
.AllowSelectForward();  // Allow jumping ahead
```

**Features:**
- Visual states: completed (checkmark), current (highlighted), upcoming (muted)
- Selective navigation: by default, only completed steps clickable
- Forward navigation: use `.AllowSelectForward()` to allow jumping ahead
- Icons, labels, and descriptions per step
- Connected design with highlighted progress lines

### State Management

**Increment and Decrement Helpers:**

New `Incr()` and `Decr()` extension methods for `IState<int>`:

```csharp
var counter = UseState(0);
new Button("Increment").HandleClick(() => counter.Incr());
new Button("Decrement").HandleClick(() => counter.Decr());
```

**Conditional Rendering with Boolean State:**

New `True()` and `False()` extension methods for `IState<bool>`:

```csharp
var isLoading = UseState(false);

return new Fragment()
    | isLoading.True(() => new Loading())
    | isLoading.False(() => new Button("Load Data"));
```

### Loading Widget

**Enhanced Visual Presentation:**

- Fixed, full-screen overlay with semi-transparent dark background
- 200ms display delay to prevent jarring flashes for quick operations
- Operations completing in under 200ms won't show loading indicator

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

### Grid Layout

**Improved Dark Mode Contrast:**

Grid layouts now have improved text contrast in dark mode. Colors blended with background color instead of pure white/black.

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

### Routing

**404 Not Found Page for Invalid Apps:**

When users navigate to non-existent app, Ivy displays proper 404 error page instead of failing silently.

**Default Behavior:**

Shows friendly error message: "Ouch! :| Apologies, the app you were looking for was not found." Returns HTTP 404 status code.

**Customizing the 404 Page:**

```csharp
var server = new Server();
server.UseErrorNotFound<MyCustomNotFoundApp>();
// Or use factory function
server.UseErrorNotFound(() => new MyCustomNotFoundApp());
```

**Framework Routes Now Use /ivy Prefix:**

All framework-provided routes use `/ivy` prefix:
- SignalR hub: `/ivy/messages`
- Health checks: `/ivy/health`
- Static resources: `/ivy/img/`, `/ivy/css/`, etc.

**App ID Collision Detection:**

Ivy automatically detects and prevents routing conflicts between app IDs and framework routes. Checks for collisions with:
- System paths (`/_framework`, `/api`, `/ivy`)
- Controller routes (auto-discovered from ASP.NET Core controllers)
- Explicitly reserved paths

**Reserving Custom Paths:**

```csharp
var server = new Server();
server.ReservePaths("/admin", "/reports", "/dashboard")
    .RegisterApp<MyApp>("users")  // ✅ Works fine
    .Start();
```

Path comparison is case-insensitive.

## Breaking Changes

### Kanban Widget API Simplification

**CardBuilder Now Required:**

The simple `titleSelector` and `descriptionSelector` parameters have been removed. You must now use `.CardBuilder()`:

```csharp
// ❌ Old API - no longer supported
tasks.ToKanban(
    groupBySelector: e => e.Status,
    titleSelector: e => e.Title,
    descriptionSelector: e => e.Description)

// ✅ New API - CardBuilder required
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

The `.HandleClick()` and `.HandleDelete()` methods have been removed. Implement click/delete functionality within your `.CardBuilder()` instead.

**Column Width Changes:**

Per-column width configuration removed. Use `.ColumnWidth()` for uniform width:

```csharp
// ❌ Old API - per-column widths
tasks.ToKanban(...)
    .Width(e => e.Status, Size.Fraction(0.33f))

// ✅ New API - uniform column width
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

**Width → ColumnWidth:**

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
- `ActionId` → Use `Tag` instead
- `EventName` → Removed
- `RowIndex` → Removed
- `RowData` → Use `Id` instead

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

- ✅ **Allowed**: Relative paths, http/https URLs, data URLs (for appropriate media types), blob URLs (with origin validation), `app://` protocol URLs, anchor links
- ❌ **Blocked**: `javascript:` protocol, malformed URLs, protocol injection attempts, dangerous URL patterns

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
- **Table Column Widths**: Fixed handling of `Size.Units()` when only some columns have explicit widths set
- **DataTable Row Actions**: Fixed event handling requiring `idSelector` for proper row identification
- **Article Navigation**: Fixed navigation links losing `chrome=false` parameter when navigating between articles
- **Tooltip Wrapping**: Fixed tooltips not properly wrapping long strings without spaces
- **Chart Y-Axis**: Fixed Y-axis always starting at 0, cutting off negative data points
- **Form Label Generation**: Fixed custom labels specified with `[Display]` attributes being incorrectly trimmed when field names end with "Id"
- **Grid Dark Mode**: Fixed text contrast issues in dark mode when using color mixing
- **Loading Widget**: Fixed missing overlay and delay timing for better UX
