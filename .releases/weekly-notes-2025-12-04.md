# Ivy Framework Weekly Notes - Week of 2025-12-04

## UI Components

### Button Variants

#### SkinnyGhost Variant

A new `SkinnyGhost` button variant has been added for ultra-compact button layouts. This variant combines the subtle styling of the Ghost variant with minimal padding, perfect for tight spaces like table cells or toolbar actions:

```csharp
// Use SkinnyGhost for compact button layouts
new Button("Edit")
    .SkinnyGhost()
    .Icon(Icons.Pencil);

// Great for inline actions in tables
users.ToTable()
    .Column(e => e.Name)
    .Column(e => e.Actions, e => Layout.Horizontal()
        | new Button("Edit").SkinnyGhost().Icon(Icons.Pencil)
        | new Button("Delete").SkinnyGhost().Icon(Icons.Trash));
```

The SkinnyGhost variant features:
- **Minimal Padding**: Uses `p-1` with auto height for the smallest possible footprint
- **Subtle Hover**: Shows accent background on hover (same as Ghost variant)
- **Compact Design**: Ideal for icon-only buttons or actions where space is at a premium

This variant is particularly useful for:
- Action buttons in table rows
- Toolbar buttons with dense layouts
- Icon-only controls in compact interfaces
- Any scenario where you need a clickable button with minimal visual weight

### DateTime Input Visual Improvements

The DateTimeInput widget has received visual polish to improve consistency and usability:

**Icon Positioning:**
- The calendar and clock icons are now positioned inside the input field for a more compact, modern appearance
- Icons maintain consistent spacing and alignment across all date/time input variants (Date, DateTime, Time)
- The clock icon now appears inside the Time input field, matching the pattern used by other input types

**Disabled State:**
- Disabled date/time inputs now display with consistent styling that matches DateRange inputs
- Icons in disabled inputs show reduced opacity for clear visual feedback
- The cursor properly changes to `not-allowed` when hovering over disabled inputs

**Clear and Error Icon Layout:**
- Clear (X) and validation error icons are now positioned using absolute positioning for more reliable placement
- Icon spacing has been optimized to prevent overlap with input text
- The layout automatically adjusts padding based on which icons are visible (clear only, error only, or both)

These improvements are automatic - no code changes required. Your date and time inputs will now have a more polished, professional appearance that better matches modern UI design patterns.

### Form Input Size Consistency

We've improved the consistency of form input sizes across the framework. All input components (text inputs, date-time inputs) now follow a unified sizing system:

- **Small**: `h-7` with `px-2` padding
- **Medium**: `h-9` with `px-3` padding
- **Large**: `h-11` with `px-4` padding

This change ensures that form inputs align properly when mixed in the same form, providing a more polished and professional appearance. The size variants have also been simplified from enum-based to string literals for easier usage:

```typescript
// Before
<Input size={Sizes[Sizes.Medium]} />

// After
<Input size="Medium" />
```

If you're using form inputs in your application, you may notice slightly adjusted heights and padding that provide better visual consistency across your forms.

## AsyncSelectInput

### Scale Support

The `AsyncSelectInput` widget now fully supports the standard scale system, allowing you to adjust the size to match other form inputs:

```csharp
// Apply scale to async select inputs
var selectedOption = UseState<string?>(null);

selectedOption.ToAsyncSelectInput(QueryOptions, LookupOption, "Search...")
    .Small();   // Compact size with h-7

selectedOption.ToAsyncSelectInput(QueryOptions, LookupOption, "Search...")
    .Medium();  // Default size with h-9

selectedOption.ToAsyncSelectInput(QueryOptions, LookupOption, "Search...")
    .Large();   // Large size with h-11
```

The scale affects multiple aspects of the async select input:
- **Height**: Small (28px), Medium (36px), Large (44px)
- **Padding**: Proportional spacing that increases with scale
- **Text size**: Small (xs), Medium (sm), Large (base)
- **Icon size**: Chevron icon scales proportionally
- **Search sheet**: The search input in the selection sheet automatically inherits the same scale
- **Invalid icon positioning**: Error icons are now properly positioned for each scale variant

When you use AsyncSelectInput in forms with `.Scale()`, the component automatically matches the form's scale, ensuring visual consistency across all input types. The scale is also propagated to the search sheet that appears when selecting options, ensuring a consistent experience throughout the selection flow.

### Improved Visual Integration

The AsyncSelect component has received visual refinements that improve how it integrates with other form elements:

**Icon Positioning:**
- The chevron icon now uses absolute positioning with optimized sizing (w-5/6/8 instead of w-7/9/11)
- Icon spacing has been refined to prevent the button from appearing too wide compared to other inputs
- The chevron icon now displays with reduced opacity (50%) for a more subtle appearance

**Text Alignment:**
- Text content now has better alignment without excessive left margin
- The layout automatically adjusts to use available space more efficiently

These improvements ensure that AsyncSelect inputs blend seamlessly with other form inputs, maintaining consistent visual density and alignment. The changes are automatic - no code updates required.

### Full-Width Dividers

The dropdown list in AsyncSelectInput now displays dividers that extend to the full width of the dropdown menu, creating a cleaner and more modern appearance. Previously, dividers were inset from the edges, but they now span the entire width for better visual separation between options.

This improvement is automatic and requires no code changes - all AsyncSelectInput dropdowns now have this enhanced visual treatment.

### Option Descriptions

AsyncSelectInput options now support optional descriptions that appear below the main option label, providing additional context without cluttering the primary selection text:

```csharp
// Add descriptions to options for extra context
Task<Option<string>[]> QueryCountries(string query)
{
    var countries = new Dictionary<string, string>
    {
        { "Germany", "Europe" },
        { "France", "Europe" },
        { "Japan", "Asia" },
        { "USA", "North America" }
    };

    return Task.FromResult(countries
        .Where(c => c.Key.Contains(query, StringComparison.OrdinalIgnoreCase))
        .Select(c => new Option<string>(
            label: c.Key,
            value: c.Key,
            description: c.Value  // Description appears below the country name
        ))
        .ToArray());
}

// Descriptions work with complex types too
Task<Option<Guid>[]> QueryUsers(string query)
{
    return Task.FromResult(users
        .Select(u => new Option<Guid>(
            label: $"{u.Name} ({u.Department})",
            value: u.Id,
            description: u.Email  // Show email as secondary info
        ))
        .ToArray());
}
```

Descriptions appear in a smaller, muted font below the option label in both the dropdown list and the lookup result. This is particularly useful for:
- Showing categories or classifications (e.g., country regions)
- Displaying secondary identifiers (e.g., email addresses, IDs)
- Providing additional context without overwhelming the primary label

The description parameter is optional - options without descriptions continue to work exactly as before.

### Option Icons

Options now support icons, allowing you to add visual indicators to select inputs and async select inputs:

```csharp
// Add icons to options
var statusOptions = new[]
{
    new Option<string>("Active", "active", icon: Icons.CheckCircle),
    new Option<string>("Pending", "pending", icon: Icons.Clock),
    new Option<string>("Inactive", "inactive", icon: Icons.XCircle)
};

selectedStatus.ToSelectInput(statusOptions);

// Works with AsyncSelectInput too
Task<Option<string>[]> QueryCountries(string query)
{
    return Task.FromResult(new[]
    {
        new Option<string>("United States", "us", icon: Icons.Flag),
        new Option<string>("Canada", "ca", icon: Icons.Flag),
        new Option<string>("United Kingdom", "uk", icon: Icons.Flag)
    });
}

selectedCountry.ToAsyncSelectInput(QueryCountries, LookupCountry);
```

Icons appear next to the option label in both the dropdown list and the selected value display, providing quick visual identification of options. This is particularly useful for:
- Status indicators (completed, pending, failed)
- Category markers (documents, images, videos)
- Priority levels (high, medium, low)
- Any options where a visual icon enhances recognition

The icon parameter is optional - options without icons continue to work exactly as before.

**Optional Labels:**

The `Label` property on options is now nullable, allowing you to create options without explicit labels. When no label is provided, the value's string representation is used as a fallback:

```csharp
// Option with explicit label
new Option<int>("First Choice", 1, icon: Icons.Star)

// Option without label - uses value.ToString() as label
new Option<int>(null, 1, icon: Icons.Star)  // Label becomes "1"
```

This provides more flexibility when creating options programmatically or when the value itself is sufficient as the display label.

## File Input Improvements

### Enhanced Event Handlers

The `FileInput` component now supports two powerful event handlers for better user interaction control:

**OnBlur Handler:**
Fires when the file dialog closes, whether files were selected or the dialog was cancelled. This is particularly useful for tracking user interactions and implementing custom validation logic:

```csharp
files.ToFileInput(upload)
    .Placeholder("Choose files")
    .HandleBlur((Event<IAnyInput> e) =>
    {
        // Fires when file dialog closes or input loses focus
        if (files.Value.Length > 0)
            Console.WriteLine($"{files.Value.Length} file(s) selected");
        else
            Console.WriteLine("No file selected (dialog cancelled)");
    });
```

**OnCancel Handler:**
Fires when the user clicks the X button next to a file in the selected files list, allowing you to clean up upload state or implement custom cancellation logic:

```csharp
files.ToFileInput(upload)
    .HandleCancel((Guid fileId) =>
    {
        // Fires when user clicks X button on a file
        upload.Value.Cancel(fileId);
        files.Set(list => list.Where(f => f.Id != fileId).ToImmutableArray());
    });
```

The OnBlur handler uses window focus detection with precise timing control to ensure it fires reliably when the file dialog closes, without double-firing when files are selected. The implementation intelligently distinguishes between file selection and dialog cancellation scenarios. The OnCancel handler provides the file's unique ID, making it easy to remove the specific file from your state.

### Consolidated Documentation

The file upload documentation has been significantly improved and consolidated. The separate "Uploads" concept page has been merged into the comprehensive FileInput widget documentation, providing a single source of truth for all file upload functionality.

**What's improved:**

- **Unified Documentation**: All file upload concepts, patterns, and examples are now in one place
- **Better Organization**: Examples are now categorized into logical sections (Basic Usage, Validation, Integration, Event Handlers)
- **Clearer Flow**: The documentation follows a natural progression from basic to advanced usage
- **Enhanced Examples**: Dialog and form integration examples are more comprehensive and practical
- **Real-World Patterns**: Validation examples demonstrate common use cases like file type restrictions and size limits

The consolidated documentation covers:
- How the upload system works (`UseState` → `UseUpload` → `MemoryStreamUploadHandler`)
- Single and multiple file uploads with progress tracking
- File validation (type, size, count limits)
- Integration patterns for dialogs and forms
- Event handlers for blur and cancel actions
- Binary and text content handling with encoding options

All file upload functionality is now documented under the FileInput widget documentation, making it easier to find and understand the complete upload system.

## Field Widget

### Width and Height Support

The `FieldWidget` component now supports custom width and height properties, giving you more control over field dimensions in your forms:

```typescript
<FieldWidget
  label="Username"
  width="300px"
  height="auto"
>
  {/* Your input component */}
</FieldWidget>
```

Previously, field widgets would always take up the full available width (`flex-1`). Now you can specify exact dimensions when needed:

- **width**: Set a specific width (e.g., `"300px"`, `"50%"`, `"20rem"`)
- **height**: Set a specific height (e.g., `"200px"`, `"auto"`)

When width or height are not specified, the field widget maintains its default flexible behavior, ensuring backward compatibility with existing code.

## Kanban Widget

### API Simplification and Breaking Changes

The Kanban widget has received a major API simplification to improve consistency and remove rarely-used features. **This contains breaking changes if you're using the Kanban widget.**

#### CardBuilder Now Required

The Kanban widget now **requires** you to specify a `.CardBuilder()` to define how cards are rendered. The simple `titleSelector` and `descriptionSelector` parameters have been removed from the `.ToKanban()` method:

```csharp
// ❌ Old API - simple title/description selectors
tasks.ToKanban(
    groupBySelector: e => e.Status,
    idSelector: e => e.Id,
    orderSelector: e => e.Priority,
    titleSelector: e => e.Title,              // Removed
    descriptionSelector: e => e.Description)  // Removed

// ✅ New API - CardBuilder is required
tasks.ToKanban(
    groupBySelector: e => e.Status,
    idSelector: e => e.Id,
    orderSelector: e => e.Priority)
.CardBuilder(task => new Card()
    .Title(task.Title)
    .Description(task.Description))
```

This change makes the API more explicit and consistent - all cards are now rendered through `.CardBuilder()`, giving you full control over card appearance:

```csharp
// Custom card with complex content
tasks.ToKanban(
    groupBySelector: e => e.Status,
    idSelector: e => e.Id,
    orderSelector: e => e.Priority)
.CardBuilder(task => new Card(
    content: task.ToDetails()
        .Remove(x => x.Id)
        .MultiLine(x => x.Description)
))
```

#### HandleCardMove Renamed to HandleMove

The `.HandleCardMove()` method has been renamed to `.HandleMove()` for better consistency and brevity:

```csharp
// Before
tasks.ToKanban(...)
    .HandleCardMove(moveData => {
        // Handle card movement
    });

// After
tasks.ToKanban(...)
    .HandleMove(moveData => {
        // Handle card movement
    });
```

This rename provides a cleaner, more concise API while maintaining the same functionality. The method still receives the same event data containing `CardId`, `ToColumn`, and `TargetIndex`.

#### Removed Event Handlers

The `.HandleClick()` and `.HandleDelete()` methods have been removed from the Kanban widget to simplify the API:

```csharp
// ❌ No longer available
tasks.ToKanban(...)
    .HandleClick(cardId => { /* ... */ })     // Removed
    .HandleDelete(cardId => { /* ... */ })    // Removed

// ✅ Implement click/delete in your CardBuilder instead
tasks.ToKanban(...)
    .CardBuilder(task =>
        new Card()
            .Title(task.Title)
            | new Button("Delete")
                .Variant(ButtonVariant.Destructive)
                .HandleClick(() => DeleteTask(task.Id))
    )
```

If you need click or delete functionality, add buttons or handle events within your custom card content using `.CardBuilder()`.

**Card Click Example with UseTrigger Hook:**

You can easily implement card click functionality by combining `.CardBuilder()` with the `UseTrigger()` hook pattern. This provides more flexibility and integrates well with sheets, dialogs, or any other UI response:

```csharp
// Define a trigger that shows a sheet with task details
var (taskSheetView, showTaskSheet) = this.UseTrigger((IState<bool> isOpen, string taskId) =>
{
    var task = tasks.Value.FirstOrDefault(t => t.Id == taskId);
    if (task == null) return new Fragment();

    return new Sheet(
        onClose: () => isOpen.Set(false),
        content: Layout.Vertical()
            | new Card().Title(task.Title).Description(task.Description),
        title: task.Title,
        description: "Task Details"
    ).Width(Size.Rem(32));
});

// Use HandleClick on the card in CardBuilder
var kanban = tasks.Value
    .ToKanban(
        groupBySelector: e => e.Status,
        idSelector: e => e.Id,
        orderSelector: e => e.Priority)
    .CardBuilder(task => new Card(
        content: task.ToDetails()
            .Remove(x => x.Id)
            .MultiLine(x => x.Description)
    )
    .HandleClick(() => showTaskSheet(task.Id)))  // Click opens the sheet
    .Width(Size.Full());

// Don't forget to render the trigger view
return new Fragment(
    kanban,
    taskSheetView
);
```

This pattern gives you complete control over what happens when a card is clicked - open a sheet, show a dialog, navigate to a detail page, or any other interaction your application needs.

#### Column Width Changes

Column width configuration has been simplified. Instead of setting widths per column, use `.ColumnWidth()` to set a uniform width for all columns:

```csharp
// ❌ Old API - per-column widths
tasks.ToKanban(...)
    .Width(e => e.Status, Size.Fraction(0.33f))  // Removed
    .Width("Todo", Size.Rem(20))                 // Removed

// ✅ New API - uniform column width
tasks.ToKanban(...)
    .Width(Size.Full())              // Width of entire kanban board
    .ColumnWidth(Size.Rem(20))       // Width of each column (enables horizontal scroll)
```

When you set `.ColumnWidth()`, all columns get the same width, and the kanban board enables horizontal scrolling if columns exceed the container width. The `.Width()` method now controls only the overall board width, not individual columns.

### Custom Card Ordering

The Kanban widget supports custom card ordering within columns using the `.CardOrder()` method. This allows you to sort cards by any property, independent of the global `orderSelector` passed to `.ToKanban()`:

```csharp
// Define a task with date properties
public class Task
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Status { get; set; }
    public required int Priority { get; set; }
    public required string Description { get; set; }
    public required DateTime DueDate { get; set; }
    public required DateTime CreatedDate { get; set; }
}

// Order cards by due date - upcoming deadlines appear first
tasks.ToKanban(
    groupBySelector: e => e.Status,
    idSelector: e => e.Id,
    orderSelector: e => e.Priority)  // Initial order from data
.CardBuilder(task => new Card(
    content: task.ToDetails()
        .Remove(x => x.Id)
        .MultiLine(x => x.Description)
        .Builder<DateTime>(d => d.ToString("MMM dd, yyyy"))  // Format dates nicely
))
.CardOrder(e => e.DueDate)  // Override: sort by due date within each column
.Width(Size.Full())
.ColumnWidth(Size.Fraction(0.33f));
```

**How it works:**

- The `orderSelector` in `.ToKanban()` provides the initial card ordering
- Use `.CardOrder()` to override or refine how cards are sorted within each column
- This is particularly useful for displaying cards by deadline, created date, or any custom sorting logic
- Combine with `.Builder<DateTime>()` in your card details to format dates as readable strings (e.g., "Dec 04, 2025")

This makes it easy to create task boards where cards are sorted by urgency, priority, or timeline within their status columns, helping users quickly identify high-priority or time-sensitive items.

### Fixed Card Reordering Logic

We've fixed an issue with the Kanban widget's drag-and-drop reordering functionality. Cards now correctly reorder when dragged to different positions within the same column or moved between columns.

The fix addresses several edge cases:
- Cards dropped at the end of a column are now properly positioned
- Moving cards between columns maintains the correct order
- Insertion positions are calculated more accurately based on the target drop location
- List operations have been optimized for better performance during drag-and-drop

The reordering logic now creates a new task instance when moving cards (ensuring immutability) and uses more efficient list operations (`Remove()` instead of `RemoveAll()`). The insertion index calculation has also been improved to handle edge cases where cards are dropped at the beginning, middle, or end of a column.

If you're using the Kanban widget and previously experienced issues with cards not appearing in the correct position after dragging, this update resolves those problems. No code changes are required on your end - the fix is transparent to existing implementations.

### Improved Drag Visual Feedback

The Kanban widget now properly clears column highlights after drag-and-drop operations complete. Previously, when you dragged a card over a column, the highlight effect could persist even after you finished dragging or dropped the card.

**What's fixed:**

- Column highlights are now properly cleared when a drag operation ends
- The drag-over state is centralized in the Kanban context, preventing stale highlight states
- Visual feedback is more reliable and predictable throughout the entire drag operation
- The `dragOverColumn` state is now properly reset in the `handleDragEnd` callback, ensuring no lingering highlights

This improvement ensures that the visual feedback during drag-and-drop operations is clean and accurate, without lingering highlight effects. The fix is automatic - your existing Kanban boards will immediately benefit from cleaner drag-and-drop interactions.

### Enhanced Drag-and-Drop Interactions

The Kanban widget now provides precise, visual feedback during drag-and-drop operations with line indicators and smooth animations:

**Drop Position Indicators:**

When dragging a card, you'll now see a visual line indicating exactly where the card will be inserted when you drop it. This works for both:
- Moving cards within the same column (reordering)
- Moving cards between different columns

The line indicator appears between cards at the exact insertion point, making it clear where your card will land before you release the mouse button.

**Smooth Animations:**

Cards automatically shift smoothly out of the way when you drag a card from another column over them, creating space for the incoming card. The animation uses a 0.2s ease transition for a polished, professional feel.

**Improved Column Styling:**

When dragging a card over a different column:
- The target column now shows a subtle accent background (rather than a dashed border)
- Columns have better spacing with a consistent 3-unit gap between them
- The visual feedback is cleaner and less intrusive

**Refined Scrollbars:**

The scroll areas within Kanban columns now have slimmer, more modern scrollbars (1.5 units instead of 2.5 units), providing a cleaner appearance without sacrificing usability.

All of these improvements work automatically - your existing Kanban implementations will immediately benefit from the enhanced drag-and-drop experience without any code changes.

### Simplified Width and Height Methods

The Kanban widget's `Width()` and `Height()` methods have been simplified to accept only `Size` parameters. If you were using convenience overloads, you'll need to update your code:

```csharp
// Before - convenience overloads
tasks.ToKanban(...)
    .Width(800)           // int overload
    .Width(0.8f)          // float overload
    .Width("80%")         // string overload
    .Height(600);

// After - use Size factory methods
tasks.ToKanban(...)
    .Width(Size.Units(800))
    .Width(Size.Fraction(0.8f))
    .Width(Size.Fraction(0.8f))  // For percentages, convert to decimal
    .Height(Size.Units(600));
```

This change centralizes size handling logic and provides a more consistent API across the framework. The `Size` class offers:
- `Size.Units(int)` - Fixed pixel values
- `Size.Fraction(float)` - Percentage-based sizing (0.0 to 1.0)

## HeaderLayout Widget

### Scroll Control for Custom Scrolling

The HeaderLayout widget now supports disabling the automatic ScrollArea wrapper, which is essential when your content needs to handle its own scrolling (like Kanban boards):

```csharp
// Default behavior - content is wrapped in ScrollArea
new HeaderLayout(
    header: new Button("Actions"),
    content: myContent
)  // Content area scrolls automatically

// Disable scroll wrapper for custom scrolling
new HeaderLayout(
    header: new Button("Actions"),
    content: myKanbanBoard
)
.Scroll(Scroll.None)  // Content handles its own scrolling
```

**When to use `.Scroll(Scroll.None)`:**
- When content widgets (like Kanban, complex layouts) need to manage their own scrolling behavior
- When you want to prevent nested scroll containers
- When building full-height layouts where the content should fill available space without a scroll wrapper

**Auto-height behavior:**
When you set `.Scroll(Scroll.None)`, the HeaderLayout automatically sets its height to `Size.Full()` if no explicit height is provided. This ensures your content fills the available vertical space properly.

```csharp
// Example: Kanban board in HeaderLayout
var kanban = tasks.ToKanban(...)
    .Width(Size.Full())
    .Height(Size.Full());

var header = Layout.Horizontal() | new Button("Add Task");

var layout = new HeaderLayout(header, kanban)
    .Scroll(Scroll.None);  // Height automatically becomes Size.Full()
```

This is particularly useful for dashboard layouts and applications where the main content area needs precise control over scrolling behavior.

## Alert Dialogs

### Improved Button Layout

Alert dialog buttons now follow standard UI conventions with right-aligned buttons and improved button ordering:

**Button Position:**
All alert buttons are now right-aligned in the dialog footer, following common dialog design patterns. This provides a more familiar and professional appearance.

**Button Order:**
The button order has been updated to follow the "safe action on the right" convention:

- **Ok/Cancel**: Cancel (secondary) | Ok (primary)
- **Yes/No**: No (secondary) | Yes (primary)
- **Yes/No/Cancel**: Cancel (secondary) | No | Yes (primary)

```csharp
// Usage remains the same - the improved layout is automatic
await AlertService.ShowAsync(new AlertOptions
{
    Title = "Confirm Action",
    Message = "Are you sure you want to proceed?",
    ButtonSet = AlertButtonSet.YesNoCancel
});
// Buttons now appear as: [Cancel] [No] [Yes]
// With all buttons right-aligned in the footer
```

This change affects all alert dialogs in your application automatically - no code changes are required. The primary action (Ok, Yes) is now consistently positioned on the right, making it easier for users to quickly locate the action they want to take.

## Theming System

### Streamlined Color Palette

The theming system documentation has been updated to reflect the actual color variables available in the Ivy Design System. We've removed documentation for unused color variables that were previously listed but not actively used by the framework:

**Removed variables:**
- Chart colors (`Chart1` through `Chart5`)
- Sidebar-specific colors (`Sidebar`, `SidebarForeground`, `SidebarPrimary`, etc.)

**Current supported theme colors:**

| Category | Variables |
|----------|-----------|
| Main | `Primary`, `PrimaryForeground`, `Secondary`, `SecondaryForeground`, `Background`, `Foreground` |
| Semantic | `Destructive`, `Success`, `Warning`, `Info` (with `Foreground` variants) |
| UI Elements | `Border`, `Input`, `Ring`, `Muted`, `Accent`, `Card`, `Popover` (with `Foreground` variants) |

If you're using `IThemeService.SetTheme()` to customize your application's theme, you can now refer to a cleaner, more focused set of color variables:

```csharp
var theme = new Theme
{
    Light = new ThemeColors
    {
        Primary = "#0077BE",
        PrimaryForeground = "#FFFFFF",
        Secondary = "#87CEEB",
        SecondaryForeground = "#1A1A1A",
        Background = "#FFFFFF",
        Foreground = "#1A1A1A",
        Destructive = "#DC143C",
        DestructiveForeground = "#FFFFFF",
        // ... other supported colors
        Card = "#FFFFFF",
        CardForeground = "#1A1A1A",
        Popover = "#FFFFFF",
        PopoverForeground = "#1A1A1A"
    },
    Dark = new ThemeColors
    {
        // Dark mode color definitions
    }
};
```

This change simplifies theming by focusing on the colors that are actually used throughout the framework, making it easier to understand which variables to customize for your application's look and feel.

## Article Widget

### Fixed Navigation in Chrome=False Mode

The Article widget's previous/next navigation links now work correctly when your application runs in `chrome=false` mode (without the Ivy chrome wrapper). Previously, clicking these navigation links would lose the `chrome=false` query parameter, causing the page to reload with chrome enabled.

This fix ensures that:
- Navigation links preserve the `chrome=false` parameter when navigating between articles
- Browser-native navigation is used in `chrome=false` mode for better compatibility
- Backend event handlers are still used for navigation in normal chrome mode

If you're building documentation or article-based applications that run without the Ivy chrome wrapper, navigation between articles will now work seamlessly without toggling chrome mode unexpectedly.

## Tooltips

### Multiline Text Support

Tooltips now properly handle long text content with improved wrapping and width constraints:

**Maximum Width:**
- Tooltips are now constrained to a maximum width of `max-w-sm` (24rem/384px)
- Long text automatically wraps to multiple lines instead of creating extremely wide tooltips

**Long String Handling:**
- Very long strings without spaces (like URLs or file paths) now use `break-all` to wrap properly
- This prevents horizontal overflow and ensures tooltips remain readable regardless of content

```csharp
// Tooltips now handle long content gracefully
table.ToTable()
    .Column(e => e.VeryLongUrlWithoutSpaces)  // Wraps properly in tooltip
    .Column(e => e.LongDescription);          // Multi-line text wraps nicely
```

**Table Cell Tooltips:**
- Table cell tooltips specifically use `break-all` for consistent wrapping behavior
- Multi-line content is displayed with proper whitespace preservation using `whitespace-pre-wrap`

These improvements ensure tooltips remain readable and well-formatted, even when displaying long URLs, file paths, or text content that doesn't contain natural break points. The changes apply automatically to all tooltips throughout the framework.

## Utilities

### Number Formatting

A new `Utils.FormatNumber()` utility has been added for formatting large numbers with K/M/B suffixes:

```csharp
// Format large numbers into readable strings
Utils.FormatNumber(1500);           // "1.5K"
Utils.FormatNumber(2500000);        // "2.5M"
Utils.FormatNumber(3800000000);     // "3.8B"
Utils.FormatNumber(999);            // "999"

// Control decimal places (default is 2)
Utils.FormatNumber(1234567, 1);     // "1.2M"
Utils.FormatNumber(1234567, 3);     // "1.235M"
```

The function automatically:
- Formats numbers 1,000+ with "K" suffix
- Formats numbers 1,000,000+ with "M" suffix
- Formats numbers 1,000,000,000+ with "B" suffix
- Trims trailing zeros from decimal places
- Uses invariant culture formatting for consistency

This is particularly useful for displaying metrics, counts, or any large numbers in a compact, user-friendly format.

## Authentication

### Cross-Tab Logout Synchronization

When you log out in one browser tab, all other tabs running your Ivy application will now automatically reload and reflect the logged-out state. This is achieved using the Broadcast Channel API, which enables seamless communication between browser tabs.

**How it works:**

When a logout occurs (the auth token is cleared), the framework broadcasts a logout event to all other tabs. Each tab receives this event and automatically reloads, ensuring a consistent authentication state across your entire application session.

**Browser support:**

This feature works automatically in all modern browsers that support the Broadcast Channel API (Chrome, Firefox, Edge, Safari 15.4+). For browsers without support, the feature gracefully degrades - each tab will still function independently, but won't automatically synchronize logout events.

**No code changes required:**

This enhancement is built into the framework's authentication system and works automatically. Your application will now provide a better user experience when users have multiple tabs open, preventing stale authentication states and potential security issues.

## Form Scaffolding

### Upload-Aware Form Submission

Forms now intelligently prevent submission while file uploads are still in progress, providing clear user feedback:

**Automatic Upload Detection:**

When you submit a form that contains file uploads, the framework automatically checks if any uploads are still processing. If uploads are in progress, the submit button is disabled and a toast notification alerts the user:

```
"File uploads are still in progress. Please wait for them to complete."
```

This applies to all form contexts:
- Standard forms created with `.ToForm()`
- Sheet forms created with `.ToSheet()`
- Dialog forms created with `.ToDialog()`

**Example:**

```csharp
var files = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
var upload = this.UseUpload(MemoryStreamUploadHandler.Create(files));

var model = UseState(new ProductForm());

// The form automatically handles upload state
model.ToForm()
    .Field(m => m.Images, state => files.ToFileInput(upload))
    .Field(m => m.Name, state => state.ToTextInput())
    .HandleSubmit(async () =>
    {
        // This only executes after all file uploads complete
        await SaveProduct(model.Value);
    });
```

**Visual Feedback:**

While uploads are in progress:
- Submit button shows a loading state (spinner)
- Submit button is disabled to prevent premature submission
- Clicking the disabled submit button shows a toast notification explaining the wait
- Once all uploads complete, the submit button becomes active automatically

This enhancement prevents data loss and improves user experience by ensuring all file content is properly uploaded before the form is submitted. No code changes are required - the feature works automatically with any form that includes file uploads.

### Enhanced Form Configuration API

FormBuilder now provides a more flexible and consistent API for configuring forms. Several configuration options have been converted from properties to chainable methods:

**Submit Button Customization:**

You can now fully customize the submit button using `.SubmitBuilder()`, or simply change the text with `.SubmitTitle()`:

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
        .Disabled(isLoading)
        .Icon(Icons.Save))
    .HandleSubmit(async (user) => await SaveAsync(user));
```

The `SubmitBuilder` receives a boolean indicating whether the form is currently submitting, allowing you to customize the button's loading and disabled states. This gives you complete control over the submit button's appearance and behavior while maintaining proper form state management.

**Default Submit Button:**

If you don't specify a custom submit button, forms now automatically get a default "Save" button. This ensures all forms have a working submit button out of the box, even without explicit configuration.

**Async Submit Handler:**

The `.HandleSubmit()` method now accepts an async callback that receives the validated model. This callback is invoked after validation passes but before the model state is updated, making it perfect for saving to a database or calling APIs:

```csharp
var model = UseState(new User());

model.ToForm()
    .Field(m => m.Name, state => state.ToTextInput())
    .Field(m => m.Email, state => state.ToTextInput())
    .HandleSubmit(async (user) =>
    {
        // This runs after validation passes
        await _database.SaveUserAsync(user);

        // After this callback completes, the model state is updated
        // and invalid field count is reset
    });
```

**Key behaviors:**

- The callback only runs if all validations pass
- You receive the validated model as a parameter
- Any exceptions thrown will prevent the model state from being updated
- After successful completion, the form's internal state is synchronized with your model

This makes form submission workflows much more straightforward - you no longer need to manually check validation state or worry about updating the model at the right time.

**Validation Strategy:**

The validation strategy can now be set via a chainable method:

```csharp
// Before (initialization only)
var form = new FormBuilder<User>(model, "Save", FormValidationStrategy.OnBlur);

// After (chainable configuration)
model.ToForm()
    .ValidationStrategy(FormValidationStrategy.OnSubmit)
    .SubmitTitle("Save")
    .HandleSubmit(async () => await SaveAsync());
```

**Scale Configuration:**

The `.Scale()` method is now part of the public API, complementing the existing `.Small()`, `.Medium()`, and `.Large()` convenience methods:

```csharp
model.ToForm()
    .Scale(Scale.Large)  // Now public API
    .SubmitTitle("Submit")
    .HandleSubmit(async () => await SaveAsync());
```

These changes make form configuration more flexible and consistent with other widget configuration patterns in the framework.

### Improved Form Spacing and Typography

Forms now apply scale-appropriate spacing between the form fields and the submit button section, along with improved button text sizing:

**Spacing:**
- **Small**: 4px gap
- **Medium**: 6px gap (default)
- **Large**: 8px gap

**Button Text Sizes:**
- **Small**: `text-xs` (extra small text)
- **Medium**: `text-sm` (small text, default)
- **Large**: `text-base` (base text size)

These improvements ensure that forms at different scales maintain proper visual balance, with breathing room between form content and action buttons that matches the overall form density. The typography now scales harmoniously with button sizes for better readability:

```csharp
model.ToForm()
    .Small()      // 4px gap, smaller text for compact layouts
    .HandleSubmit(async () => await SaveAsync());

model.ToForm()
    .Large()      // 8px gap, larger text for comfortable spacing
    .HandleSubmit(async () => await SaveAsync());
```

These changes create a more cohesive visual hierarchy across different form scales, making it easier to create forms that feel balanced at any size.

**Note:** A bug fix was applied to ensure the scale-appropriate spacing works correctly across all form scales.

### Comprehensive DataAnnotations Support

FormBuilder now provides extensive support for C# DataAnnotations, making it easier to generate forms from your model classes with built-in validation and metadata.

#### Display Attributes

Use the `[Display]` attribute to control how your form fields are rendered:

```csharp
public class UserProfile
{
    [Display(Name = "Full Name", Description = "Enter your first and last name",
             Prompt = "John Doe", Order = 1, GroupName = "Personal Info")]
    public string Name { get; set; }

    [Display(Name = "Email Address", Description = "We'll never share your email",
             Prompt = "user@example.com", Order = 2, GroupName = "Contact")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
}

// The FormBuilder automatically uses these attributes
var model = UseState(new UserProfile());
model.ToForm()
    .HandleSubmit(async () => {
        await SaveProfile(model.Value);
    });
```

The FormBuilder extracts:
- **Name**: Custom label for the field
- **Description**: Help text shown below the field
- **Prompt**: Placeholder text in the input
- **Order**: Display order of fields
- **GroupName**: Groups fields into sections

#### Input Type Detection

The FormBuilder now automatically detects the appropriate input type based on `[DataType]` attributes:

```csharp
public class ContactForm
{
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }  // → Email input with validation

    [DataType(DataType.PhoneNumber)]
    public string Phone { get; set; }  // → Tel input

    [DataType(DataType.Url)]
    public string Website { get; set; }  // → URL input with validation

    [DataType(DataType.Password)]
    public string Password { get; set; }  // → Password input (masked)

    [DataType(DataType.CreditCard)]
    public string Card { get; set; }  // → Credit card input with validation
}
```

#### Validation Attributes

All major validation attributes are now automatically enforced with appropriate error messages:

```csharp
public class Product
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, MinimumLength = 3,
                  ErrorMessage = "Name must be between 3 and 100 characters")]
    public string Name { get; set; }

    [Range(0.01, 10000, ErrorMessage = "Price must be between $0.01 and $10,000")]
    public decimal Price { get; set; }

    [RegularExpression(@"^[A-Z]{3}\d{3}$",
                       ErrorMessage = "SKU must be 3 letters followed by 3 digits")]
    public string SKU { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }  // Enforces character limit on input

    [EmailAddress(ErrorMessage = "Please enter a valid email")]
    public string ContactEmail { get; set; }

    [Url(ErrorMessage = "Please enter a valid URL")]
    public string ProductUrl { get; set; }

    [CreditCard]
    public string PaymentCard { get; set; }

    [AllowedValues("Small", "Medium", "Large")]
    public string Size { get; set; }
}
```

**Supported validation attributes:**
- `[Required]` - Makes field mandatory
- `[StringLength]` / `[MaxLength]` / `[MinLength]` - String length limits with frontend enforcement
- `[Range]` - Numeric min/max values (enforced on number inputs)
- `[RegularExpression]` - Pattern matching
- `[EmailAddress]` - Email format validation
- `[Phone]` - Phone number validation
- `[Url]` - URL format validation
- `[CreditCard]` - Credit card number validation
- `[AllowedValues]` - Restricts to specific values
- `[Compare]` - Compares two fields (e.g., password confirmation)

#### ScaffoldColumn Control

Use `[ScaffoldColumn(false)]` to exclude fields from form generation:

```csharp
public class User
{
    [ScaffoldColumn(false)]
    public Guid Id { get; set; }  // Hidden from form

    public string Username { get; set; }  // Included in form

    [ScaffoldColumn(false)]
    public DateTime CreatedAt { get; set; }  // Hidden from form
}
```

### Universal Placeholder Support

All input widgets now support the `.Placeholder()` method for consistent placeholder text across the framework:

```csharp
// Works on all input types
textState.ToTextInput()
    .Placeholder("Enter your name");

numberState.ToNumberInput()
    .Placeholder("0.00");

dateState.ToDateTimeInput()
    .Placeholder("Select a date");
```

This provides a unified API for setting placeholder text, making it easier to provide helpful hints to users across different input types.

### Fixed Label Generation with Display Attributes

The form scaffolding system now correctly preserves custom labels specified with `[Display]` attributes when field names end with "Id". Previously, the framework would sometimes incorrectly trim custom labels:

```csharp
public class UserForm
{
    // Custom labels are now preserved exactly as written
    [Display(Name = "User ID")]
    public int UserId { get; set; }  // Label: "User ID" (not "User")

    [Display(Name = "Government ID")]
    public int GovId { get; set; }  // Label: "Government ID" (not "Government")

    // Auto-generated labels still have "Id" trimmed
    public int CountryId { get; set; }  // Label: "Country" (auto-generated)
}
```

**The fix:**
- If you explicitly set a label with `[Display(Name = "...")]`, that label is now used exactly as written
- Only auto-generated labels (from property names) have the "Id" suffix trimmed
- This ensures your carefully chosen display names are respected by the form scaffolding system

This improvement makes the form scaffolding behavior more predictable and gives you full control over field labels when you need it.

## Component Sizing

### Sizes Renamed to Scale

The `Sizes` enum has been renamed to `Scale` throughout the framework for better clarity. The API now uses a more intuitive sizing system:

```csharp
// Old approach (no longer available)
button.Size(Sizes.Medium);

// New approach
button.Scale(Scale.Medium);

// Or use convenience methods
button.Small();
button.Medium();
button.Large();
```

**Migration:** If you're using the `Sizes` enum in your code, update references to `Scale`. The enum values remain the same (Small, Medium, Large), only the type name has changed.

### Nullable Scale Property

The `Scale` property on `WidgetBase` is now nullable, allowing widgets to inherit their size from parent components or use default sizing when no scale is explicitly set. This provides more flexibility in widget configuration and better supports responsive designs.

### Medium Scale as Default

All form inputs and tables now default to `Scale.Medium` when no scale is explicitly specified. This provides a more balanced and comfortable default appearance across your application:

```csharp
// These now all default to Medium scale
textState.ToTextInput();     // Medium scale by default
selectState.ToSelectInput(); // Medium scale by default
items.ToTable();             // Medium scale by default

// You can still override the default
textState.ToTextInput().Small();
```

**Affected components:**
- TextInput and all text input variants (email, password, search, tel, url, textarea)
- SelectInput (both single and multi-select)
- Table widget

This change ensures a consistent default experience without requiring explicit scale configuration. If you previously relied on the undefined scale behavior, your components will now render at Medium scale, which provides better visual consistency and spacing.

## Expandable Widget

### Scale Support

The Expandable widget now supports the standard scale system, allowing you to adjust the size and density to match your layout needs:

```csharp
// Compact layout - ideal for space-constrained areas like sidebars
new Expandable(
    Text.Block("Small scale (compact task list)"),
    Text.Block("Tighter padding keeps related details visible.")
).Small();

// Default balanced layout - works well for most content
new Expandable(
    Text.Block("Medium scale (default)"),
    Text.Block("Comfortable spacing for mixed content like text, lists or buttons.")
).Medium();

// Emphasized layout - great for primary content areas
new Expandable(
    Text.Block("Large scale (emphasis)"),
    Text.Block("Generous spacing improves readability.")
).Large();
```

The scale affects multiple aspects of the expandable:
- **Height**: Small (28px), Medium (36px), Large (44px)
- **Padding**: Proportional spacing that increases with scale
- **Text size**: Small (xs), Medium (sm), Large (base)
- **Chevron icon size**: Scales proportionally with the overall size
- **Content spacing**: Internal gaps adjust to maintain visual harmony

Scale also automatically propagates to expandables within forms when using FormBuilder with `.Scale()`, ensuring consistent sizing throughout your form groups.

**Note:** The default scale is Medium, providing backward compatibility with existing code.

### Improved Icon Positioning

The Expandable widget's chevron icon now uses absolute positioning to match the visual pattern established by AsyncSelectInput and other form inputs:

**Icon Positioning:**
- The chevron icon now uses absolute positioning with right-alignment
- Icon widths have been optimized (w-5/6/8 instead of w-7/9/11) for better visual integration
- The chevron displays with reduced opacity (50%) for a more subtle appearance
- The vertical border line that previously separated the chevron from the header has been removed for a cleaner look

**Text Layout:**
- Header content now has proper right padding (pr-8/9/11) to prevent text overlap with the chevron
- The layout automatically adjusts spacing based on scale
- Text content has better alignment without excessive margins

These improvements ensure that Expandable widgets blend seamlessly with other form inputs, maintaining consistent visual density and alignment. The changes are automatic - no code updates required.

### Interactive Elements in Disabled Expandables

When an Expandable is disabled, interactive elements within the header (like buttons, switches, or links) now remain clickable. This allows you to include action buttons or controls in expandable headers without them being blocked by the expandable's disabled state:

```csharp
// Example: Expandable section with action buttons in the header
var sectionExpanded = UseState(false);

new Expandable(
    header: Layout.Horizontal()
        | Text.Block("Section Title")
        | new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .HandleClick(() => EditSection())
        | new Button("Delete")
            .Variant(ButtonVariant.Destructive)
            .HandleClick(() => DeleteSection()),
    content: Text.Block("Section content goes here")
)
.Disabled(true)  // Expandable can't be toggled, but buttons still work
```

Interactive elements that remain functional in disabled expandables include:
- Buttons and action buttons
- Switches and checkboxes
- Input fields and selects
- Links (`<a>` tags)
- Custom interactive elements with `role="button"`, `role="switch"`, `role="checkbox"`, etc.

The expandable header itself remains non-clickable when disabled (preventing toggling), but embedded controls continue to work normally. This is particularly useful for expandable sections that contain actions or settings that should always be accessible, regardless of whether the section can be collapsed.

### Improved Click Handling with Interactive Elements

The Expandable widget now has more intelligent click handling that properly distinguishes between clicking interactive elements (buttons, switches, inputs) and clicking the expandable header itself:

**How it works:**

When you click on the expandable header:
- Clicking blank areas of the header toggles the expandable open/closed as expected
- Clicking interactive elements (buttons, switches, inputs, links) triggers those elements without toggling the expandable
- The toggle behavior is prevented when the expandable is disabled, showing the proper `not-allowed` cursor

This fix ensures that complex headers with multiple interactive controls work intuitively. For example, if you have a switch in an expandable header, you can click the switch to toggle it without also expanding/collapsing the section. Clicking anywhere else in the header still toggles the expandable normally.

**Visual refinements:**

The click handling improvements also come with refined visual alignment:
- Content padding now properly aligns with the header text across all scale variants
- The chevron icon is positioned absolutely at the far right with proper z-indexing
- Header text has appropriate right padding to prevent overlap with the chevron
- Cursors correctly reflect the interaction state (pointer for clickable areas, not-allowed when disabled)

These improvements make expandables with complex headers (containing switches, buttons, or other controls) work more predictably and feel more polished. No code changes are required - your existing expandables will automatically benefit from the improved behavior.

## Table Widget

### API Change: Width → ColumnWidth

The `.Width()` method for setting column widths in TableBuilder has been renamed to `.ColumnWidth()` for better clarity and consistency. This is a breaking change if you're customizing column widths in your tables:

```csharp
// Before
products.ToTable()
    .Width(e => e.Sku, Size.Fraction(0.15f))
    .Width(e => e.Name, Size.Fraction(0.3f))
    .Width(e => e.Price, Size.Fraction(0.15f));

// After
products.ToTable()
    .ColumnWidth(e => e.Sku, Size.Fraction(0.15f))
    .ColumnWidth(e => e.Name, Size.Fraction(0.3f))
    .ColumnWidth(e => e.Price, Size.Fraction(0.15f));
```

The method signature and functionality remain the same - only the name has changed. The new name makes it clearer that you're setting the width of a specific column, not the entire table width (which is controlled by the `.Width()` method on the table itself).

### Improved Column Width and Text Wrapping

Tables now handle column widths and text wrapping more intelligently, providing better control over layout and preventing unwanted horizontal scrolling:

**Column Width Behavior:**

- **Full Width Tables**: Use `table-layout: fixed` to strictly respect column width constraints and prevent overflow
- **Fixed Width Tables**: Use `table-layout: auto` to allow natural column sizing based on content
- Tables with `Size.Full()` now properly constrain themselves to the container width (max-width: 100%) without causing horizontal scroll
- Tables with fixed widths (`Size.Units()`, `Size.Px()`, `Size.Rem()`) remove max-width constraints to allow natural expansion

**Text Wrapping Improvements:**

- Multi-line cells now use `wrap-break-word` for better word breaking behavior with proper wrapping
- Data cells without explicit widths can now size naturally based on content (no forced truncation)
- Header cells always truncate with tooltips for better consistency
- Cells with explicit widths apply proper truncation with overflow controls
- Tooltips now only appear for text content, preventing "[object Object]" display issues

**Example:**

```csharp
// Fixed width table with mixed column sizing
records.ToTable()
    .Width(Size.Units(100))
    .ColumnWidth(e => e.Reference, Size.Units(20))      // Fixed width
    .ColumnWidth(e => e.Contact, Size.Fraction(1.0f))   // Takes remaining space
    .ColumnWidth(e => e.Actions, Size.Fit())            // Fits content

// Full width table with fractions
records.ToTable()
    .Width(Size.Full())                                 // Fills container
    .ColumnWidth(e => e.Reference, Size.Fraction(0.3f))
    .ColumnWidth(e => e.Contact, Size.Fraction(0.4f))
    .ColumnWidth(e => e.Views, Size.Fraction(0.3f))
```

These improvements make tables more predictable and easier to work with, especially when mixing fixed-width, fractional, and fit-content columns.

### Enhanced Column Alignment API

The Table widget's `.Align()` method has been improved to properly align content within both header and data cells. The alignment now correctly applies text alignment to cell content rather than just flex positioning:

```csharp
// Align numbers to the right for better readability
records.ToTable()
    .ColumnWidth(e => e.Views, Size.Fit())
    .Align(e => e.Views, Align.Right);

// Center align text in a wide column
records.ToTable()
    .ColumnWidth(e => e.Contact, Size.Fraction(0.6f))
    .Align(e => e.Contact, Align.Center);

// Mix different alignments for different column types
records.ToTable()
    .Width(Size.Full())
    .ColumnWidth(e => e.Reference, Size.Fraction(0.2f))
    .ColumnWidth(e => e.Contact, Size.Fraction(0.6f))
    .ColumnWidth(e => e.Views, Size.Fraction(0.2f))
    .Align(e => e.Contact, Align.Center)
    .Align(e => e.Views, Align.Right);
```

**How it works:**

The alignment applies consistently to:
- Header cells (column names)
- All data cells in that column
- Both truncated and multi-line content

The implementation now uses proper `text-align` CSS properties, ensuring text content aligns correctly within cells regardless of the column width. This is particularly useful for:
- Right-aligning numeric columns (prices, counts, percentages)
- Center-aligning status indicators or badges
- Creating visually balanced tables with mixed content types

The `.Align()` method accepts any of the standard `Align` enum values (Left, Center, Right, TopLeft, TopCenter, TopRight, etc.), but only the horizontal component affects table cell alignment.

## DataTable Widget

### Row Action Improvements

DataTable row actions have been enhanced with better event handling and menu support. **Important**: When using row actions, you must now specify an ID selector to identify rows:

```csharp
// Specify ID selector when creating the DataTable
users.ToDataTable(idSelector: e => e.Id)
    .Column(e => e.Name)
    .Column(e => e.Email)
    .RowActions(
        MenuItem.Default(Icons.Pencil, "edit").Tooltip("Edit employee"),
        MenuItem.Default(Icons.EllipsisVertical, "menu")
            .Children([
                MenuItem.Default(Icons.Archive, "archive").Label("Archive"),
                MenuItem.Default(Icons.Download, "export").Label("Export"),
                MenuItem.Default(Icons.Mail, "email").Label("Send Email")
            ])
    )
    .HandleRowAction(async e =>
    {
        // Access row ID and menu item tag
        var userId = e.Value.Id;  // The ID from the row (e.Id from the idSelector)
        var action = e.Value.Tag; // The tag from the MenuItem

        switch (action)
        {
            case "edit":
                // Handle edit for user with ID: userId
                break;
            case "archive":
                // Handle archive
                break;
            case "export":
                // Handle export
                break;
            case "email":
                // Handle email
                break;
        }
    });
```

**Key Changes:**

The `RowActionClickEventArgs` structure has been significantly simplified to improve usability:

- **New**: `Id` (the row's unique identifier from `idSelector`), `Tag` (the menu item's tag)
- **Removed**: `ActionId`, `EventName`, `RowIndex`, `RowData`

**Why this change?**

The new structure makes it easier to handle row actions by providing direct access to the row's unique ID and the specific menu item that was clicked. Instead of manually extracting values from a `RowData` dictionary, you now get the row's ID directly, making it more type-safe and cleaner to work with.

**Migration example:**

```csharp
// Before
.HandleRowAction(async e => {
    var userId = e.Value.RowData["Id"];  // Dictionary lookup
    var action = e.Value.ActionId;
    // Handle action based on userId and action
});

// After
.HandleRowAction(async e => {
    var userId = e.Value.Id;   // Direct access to ID
    var action = e.Value.Tag;  // Menu item tag
    // Handle action based on userId and action
});
```

**Features:**

- **Nested Menus**: Use `.Children()` to create dropdown menus with sub-items
- **Icon Support**: Each menu item (parent and child) can have its own icon
- **Labels and Tooltips**: Parent items can show tooltips on hover, child items show labels
- **ID-based Row Identification**: Use the `idSelector` parameter to specify which property uniquely identifies each row
- **Simplified Event Handling**: Direct access to row ID and menu tag instead of dictionary lookups

This is particularly useful when you have many row actions and want to:
- Group related actions together (e.g., all export options under one menu)
- Keep the row actions column compact
- Provide better organization for complex workflows

The ellipsis icon (vertical three dots) is a common pattern for indicating additional menu options, but you can use any icon for parent menu items.

### Improved Performance with Arrow Tables

The DataTable widget now uses Apache Arrow's columnar storage format internally for better memory efficiency and performance, especially when working with large datasets. This change is transparent to you - your existing DataTable code will work exactly as before, but you'll notice:

- **Better Memory Efficiency**: Large tables use significantly less memory
- **Faster Load Times**: Initial data loading and pagination are more responsive
- **Smoother Scrolling**: Scrolling through large datasets is now smoother

This optimization is particularly noticeable when working with tables containing thousands of rows or more. The frontend no longer stores millions of rows in React state - instead, data is accessed efficiently from the Arrow table via gRPC.

### Column Resizing

DataTable now supports column resizing out of the box. Users can click and drag column borders to adjust widths interactively. Column widths are preserved during the session, providing a more flexible data viewing experience.

To disable column resizing:

```csharp
users.ToDataTable()
    .Config(c => c.AllowColumnResizing = false);
```

This feature is enabled by default and requires no code changes to use.

## Chrome Customization

### Generic UseChrome Method

You can now specify custom chrome implementations using a simpler generic syntax with the new `UseChrome<T>()` method:

```csharp
// Before - using a factory function
var server = new Server();
server.UseChrome(() => new MyCustomChrome());

// After - using the generic method
var server = new Server();
server.UseChrome<MyCustomChrome>();
```

The generic method automatically instantiates your custom chrome class (which must inherit from `ViewBase`). This provides a cleaner, more type-safe way to configure custom chrome for your application.

Both approaches are still supported, but the generic version is recommended for simple cases where your chrome class has a parameterless constructor.

## Charts

### Toolbox on Hover

Chart toolbox controls (save, zoom, data view, restore) now only appear when you hover over the chart, providing a cleaner, less cluttered appearance:

**Before:**
- Toolbox controls were always visible in the top-right corner of charts
- This could be distracting, especially for dashboard views with multiple charts

**After:**
- Toolbox controls appear only when you hover over the chart area
- Charts have a cleaner look by default
- Controls remain fully functional when you need them

This is particularly useful when displaying multiple charts on a dashboard or report page. The reduced visual clutter helps users focus on the data while keeping powerful tools accessible on demand.

**Example:**

```csharp
// Chart toolboxes now automatically show on hover
var data = UseState(ImmutableArray.Create(
    new SalesRecord { Month = "Jan", Desktop = 100, Mobile = 50 },
    new SalesRecord { Month = "Feb", Desktop = 150, Mobile = 75 }
));

data.ToLineChart()
    .Dimension("Month", e => e.Month)
    .Measure("Desktop", e => e.Sum(f => f.Desktop))
    .Measure("Mobile", e => e.Sum(f => f.Mobile))
    .Toolbox();  // Toolbox controls appear only on hover
```

No code changes required - this enhancement applies automatically to all chart types (Line, Area, Bar, Pie) when the toolbox is enabled.

### Fixed Y-Axis Rendering with Negative Values

Charts now correctly handle negative values by automatically adjusting the Y-axis minimum to include negative ranges. Previously, when data contained negative values without a large spread, the Y-axis would always start at 0, cutting off negative data points.

**What changed:**

The Y-axis now dynamically adjusts its minimum value based on your data:
- If your data has a large spread, the axis min/max is calculated from the data range (as before)
- If your data has a small spread, the axis minimum is now `Math.min(minValue, 0)` instead of always `0`

This means:
- Charts with all positive values: Y-axis still starts at 0 (no change)
- Charts with negative values: Y-axis now extends below 0 to show all data points
- Charts with mixed positive/negative values: Y-axis spans the full range

```csharp
// Example: This data will now display correctly
var salesData = UseState(ImmutableArray.Create(
    new SalesRecord { Month = "Jan", Profit = 100 },
    new SalesRecord { Month = "Feb", Profit = -50 },  // Previously cut off
    new SalesRecord { Month = "Mar", Profit = 75 }
));

salesData.ToLineChart(...)
    // Y-axis will now range from -50 to 100, showing the loss in February
```

This fix ensures that all your data is visible on the chart, especially important for financial data, deltas, or any metrics that can be negative. No code changes are required - your existing charts will automatically benefit from this improvement.

## Stepper Widget

### New Component for Step-by-Step Flows

A new `Stepper` widget has been added for displaying and navigating multi-step processes like wizards, onboarding flows, or progress tracking:

```csharp
// Define your steps with symbols, icons, labels, and descriptions
var currentStep = UseState(0);

var steps = new[]
{
    new StepperItem("1", Icons.Check, "Company", "Basic info"),
    new StepperItem("2", null, "Raise", "Funding details"),
    new StepperItem("3", null, "Deck", "Presentation"),
    new StepperItem("4", null, "Founders", "Team info")
};

// Create the stepper with step selection handling
new Stepper(
    onSelect: async e => currentStep.Set(e.Value),
    selectedIndex: currentStep.Value,
    items: steps
)
.Width(200);
```

**Features:**

- **Visual States**: Steps automatically display as completed (with checkmark), current (highlighted), or upcoming (muted)
- **Selective Navigation**: By default, users can only click to go back to completed steps
- **Forward Navigation**: Use `.AllowSelectForward()` to let users jump ahead to any step
- **Icons and Labels**: Each step can have a symbol (number/letter), optional icon, label, and description
- **Connected Design**: Steps are visually connected with lines that highlight as you progress

```csharp
// Allow jumping forward to any step
new Stepper(OnSelect, currentStep.Value, steps)
    .AllowSelectForward();

// Programmatically control the current step
new Button("Previous").HandleClick(() =>
    currentStep.Set(Math.Clamp(currentStep.Value - 1, 0, steps.Length - 1)));

new Button("Next").HandleClick(() =>
    currentStep.Set(Math.Clamp(currentStep.Value + 1, 0, steps.Length - 1)));
```

**Step Items:**

The `StepperItem` record lets you configure each step:

```csharp
new StepperItem(
    symbol: "1",                    // Number, letter, or text shown in circle
    icon: Icons.Check,               // Optional icon (replaces symbol when step is completed)
    label: "Company Details",        // Optional label below the step
    description: "Basic information" // Optional description text
);
```

The Stepper widget automatically handles:
- Hover effects for clickable steps
- Smooth transitions between states
- Responsive layout with proper text alignment
- Accessibility with button semantics

This is ideal for guided workflows where users need to complete steps in sequence, such as multi-page forms, setup wizards, or onboarding experiences.

## State Management

### Increment and Decrement Helpers

Working with integer state just got more convenient with new `Incr()` and `Decr()` extension methods for `IState<int>`:

```csharp
// Before - verbose counter updates
var counter = UseState(0);
new Button("Increment").HandleClick(() => counter.Set(counter.Value + 1));
new Button("Decrement").HandleClick(() => counter.Set(counter.Value - 1));

// After - clean and concise
var counter = UseState(0);
new Button("Increment").HandleClick(() => counter.Incr());
new Button("Decrement").HandleClick(() => counter.Decr());
```

These helper methods make your code cleaner and more expressive when working with counters, pagination indices, or any integer state that needs simple increment/decrement operations.

### Conditional Rendering with Boolean State

New `True()` and `False()` extension methods for `IState<bool>` make conditional rendering more elegant and expressive:

```csharp
var isLoading = UseState(false);
var showError = UseState(false);

// Render content only when state is true
return new Fragment()
    | isLoading.True(() => new Loading())
    | showError.True(() => new Alert("An error occurred"))
    | new Button("Load Data", () => isLoading.Set(true));

// Render content only when state is false
var isLoggedIn = UseState(false);
return new Fragment()
    | isLoggedIn.False(() => new Button("Log In", () => Login()))
    | isLoggedIn.True(() => new Dashboard());
```

**How it works:**
- `.True(func)` returns the result of `func()` when the state is `true`, otherwise returns `null`
- `.False(func)` returns the result of `func()` when the state is `false`, otherwise returns `null`

This pattern is particularly useful for:
- Toggling loading indicators or spinners
- Showing/hiding error messages or alerts
- Conditional rendering based on feature flags
- Simple show/hide logic without explicit if statements

The methods integrate seamlessly with the pipe operator and Fragment pattern, keeping your component structure clean and declarative.

## Loading Widget

### Enhanced Visual Presentation

The `Loading` widget now displays with a professional overlay design and smart delay timing:

**Overlay Background:**
The loading indicator now appears in a fixed, full-screen overlay with a semi-transparent dark background (`bg-black/30`), providing better visual context and preventing user interaction with content underneath. This ensures users clearly understand that the application is processing.

**200ms Display Delay:**
To prevent jarring flashes for quick operations, the loading spinner now has a 200ms delay before becoming visible. This means:
- Operations completing in under 200ms won't show a loading indicator at all
- Longer operations will show smooth loading feedback without flickering

```csharp
var isLoading = UseState(false);

return new Fragment()
    | isLoading.True(() => new Loading())  // Shows with overlay after 200ms
    | new Button("Load Data", async () =>
    {
        isLoading.Set(true);
        await LoadDataAsync();
        isLoading.Set(false);
    });
```

These improvements provide a more polished user experience, particularly for data-heavy operations or network requests. The overlay prevents accidental interactions during loading, while the delay prevents unnecessary loading flashes for quick operations.

## Layout System

### TopCenter Alignment

A new `Layout.TopCenter()` method provides a convenient way to create horizontally-aligned layouts with top-center alignment:

```csharp
// Create a horizontal layout aligned to the top-center
Layout.TopCenter(
    new Button("Action 1"),
    new Button("Action 2"),
    new Button("Action 3")
)
```

This is particularly useful for:
- Header navigation elements that need to be centered but aligned to the top of their container
- Action buttons or controls in top bars
- Any horizontal layout where you need top-center positioning

The method automatically removes parent padding and applies `Align.TopCenter`, giving you a clean layout that's ready to use without additional configuration.

## Security

### Enhanced URL Validation

Ivy Framework now includes comprehensive URL validation across all components to prevent open redirect vulnerabilities and other URL-based security issues. This security enhancement validates all URLs before they're used in navigation, media rendering, or redirects.

**What's Protected:**

All URL-accepting components now validate and sanitize URLs automatically:
- Links in markdown content
- Images, audio, and video players
- Button widgets with URL targets
- Any redirect or navigation URLs

**Validation Rules:**

The framework now enforces strict URL validation:
- ✅ **Allowed**: Relative paths (`/path/to/resource`), http/https URLs, data URLs (for appropriate media types), blob URLs (with origin validation), `app://` protocol URLs, and anchor links (`#section`)
- ❌ **Blocked**: `javascript:` protocol, malformed URLs, protocol injection attempts, and other dangerous URL patterns

**Blob URL Security:**

Blob URLs are now validated to ensure they match the current origin, preventing attacks where malicious blob URLs from other origins could be injected. The validation properly handles default ports (443 for HTTPS, 80 for HTTP) and supports localhost development scenarios where the frontend and backend may run on different ports.

**Error Handling:**

Invalid URLs now show clear, user-friendly error messages:
- **Images/Audio/Video**: Display a bordered error box with the message "Invalid [media type] URL" or "No [media type] source provided"
- **Buttons**: Show "Invalid button URL" in a destructive-styled container
- **Links**: Convert to safe anchor links (`#`) that won't navigate

**No Code Changes Required:**

This is a transparent security enhancement. Your existing code will continue to work exactly as before, but with added protection against URL-based vulnerabilities. The framework now includes:
- Dedicated `urlValidation.ts` module with comprehensive validation utilities
- Helper functions for URL type detection (`isExternalUrl`, `isAnchorLink`, `isAppProtocol`, `isRelativePath`, etc.)
- Extensive test coverage for all validation scenarios including localhost development, production environments, and edge cases

If you notice any legitimate URLs being blocked, they may be violating the security rules and should be reviewed for potential security issues.

## Grid Layout

### Improved Dark Mode Contrast

Grid layouts now have improved text contrast in dark mode. Previously, when using color mixing to adjust color intensities in the grid, the framework would blend colors with pure white or black. Now, colors are blended with the background color instead, ensuring better readability and more consistent visual appearance across light and dark themes.

This improvement is automatic - your existing grid layouts will immediately benefit from better contrast in dark mode without any code changes. The enhancement applies to all uses of the `getColor()` utility function with percentage-based color mixing.

### Enhanced Grid API with Advanced Control

The Grid layout system has received a major upgrade with powerful new APIs for building complex grid-based layouts like data tables, dashboards, and cohort analyses:

#### Column and Row Sizing

You can now specify individual widths for columns and heights for rows using the new `ColumnWidths()` and `RowHeights()` methods:

```csharp
// Different column widths
Layout.Grid()
    .Columns(3)
    .ColumnWidths(Size.Px(100), Size.Fraction(1), Size.Px(150))
    | "100px wide" | "Takes remaining space (1fr)" | "150px wide"

// Different row heights (e.g., header + content rows)
Layout.Grid()
    .Columns(3)
    .RowHeights(Size.Px(60), Size.Fraction(1), Size.Fraction(1))
    | "Header (60px tall)" | "Header" | "Header"
    | "Content row (1fr)" | "Content" | "Content"
    | "Content row (1fr)" | "Content" | "Content"
```

This gives you precise control over grid dimensions, perfect for creating data tables with fixed-width ID columns, flexible content columns, and action columns.

#### Header and Footer Builders

Automatically style the first row as a header or the last complete row as a footer using builder functions:

```csharp
// Header builder - automatically styles the first row
Layout.Grid()
    .Columns(4)
    .HeaderBuilder((columnIndex, cell) =>
        cell.WithCell().Color(Colors.Green).Content($"Header {columnIndex}"))
    | "Cell 1" | "Cell 2" | "Cell 3" | "Cell 4"
    | "Data 1" | "Data 2" | "Data 3" | "Data 4"

// Footer builder - automatically styles the last complete row
Layout.Grid()
    .Columns(3)
    .FooterBuilder((columnIndex, cell) =>
        cell.WithCell().Color(Colors.Blue).Content($"Total: {cell}"))
    | "Item 1" | "Item 2" | "Item 3"
    | "Item 4" | "Item 5" | "Item 6"
    | "100" | "200" | "300"  // Styled as footer automatically
```

The builders receive the column index and the cell content, allowing you to transform cells based on their position.

#### Cell Builder

Apply consistent styling to all cells (excluding header/footer) using `CellBuilder()`:

```csharp
Layout.Grid()
    .Columns(3)
    .CellBuilder(cell => cell.WithCell().Color(Colors.Gray))
    | "All cells" | "get the same" | "gray styling"
    | "Unless" | "you override" | "like this".WithCell().Color(Colors.Rose)
```

This is particularly useful for applying default styling while allowing individual cells to override when needed.

#### WithCell() Extension

A new `.WithCell()` extension method creates borderless boxes that fill the entire grid cell, perfect for creating clean, modern grid layouts:

```csharp
Layout.Grid()
    .Columns(3)
    .Gap(2)  // Small gap between cells
    | "Fills cell".WithCell()
    | "No borders".WithCell().Color(Colors.Blue)
    | "Custom content".WithCell().Color(Colors.Green)
```

The method automatically removes borders, border radius, and padding, and sets the box to fill the cell completely.

#### Complete Example: Data Table

Here's a complete example combining all the features to create a styled data table:

```csharp
var users = new[] {
    new { Id = 1, Name = "Alice", Email = "alice@example.com", Role = "Admin" },
    new { Id = 2, Name = "Bob", Email = "bob@example.com", Role = "User" },
    new { Id = 3, Name = "Charlie", Email = "charlie@example.com", Role = "User" }
};

Layout.Grid()
    .Columns(4)
    .ColumnWidths(Size.Px(50), Size.Fraction(1), Size.Fraction(1), Size.Px(100))
    .Gap(2)
    .HeaderBuilder((i, _) =>
        (i == 0 ? "ID" : i == 1 ? "Name" : i == 2 ? "Email" : "Role")
            .WithCell().Color(Colors.Green))
    .CellBuilder(cell => cell.WithCell())
    | users[0].Id.ToString() | users[0].Name | users[0].Email | users[0].Role
    | users[1].Id.ToString() | users[1].Name | users[1].Email | users[1].Role
    | users[2].Id.ToString() | users[2].Name | users[2].Email | users[2].Role
```

#### Cohort Analysis & Heatmaps

The new grid API combined with color opacity support makes it easy to create heatmaps and cohort analysis visualizations:

```csharp
Layout.Grid()
    .Columns(6)
    .ColumnWidths(Size.Px(100), Size.Fraction(1), Size.Fraction(1), Size.Fraction(1), Size.Fraction(1), Size.Fraction(1))
    .Gap(2)
    // Header
    | "Cohort".WithCell() | "Month 0".WithCell() | "Month 1".WithCell() | "Month 2".WithCell() | "Month 3".WithCell() | "Month 4".WithCell()
    // Data with opacity representing retention rates
    | "Jan 2024".WithCell()
    | "100%".WithCell().Color(Colors.Orange, 1.0f)   // 100% opacity
    | "85%".WithCell().Color(Colors.Orange, 0.85f)   // 85% opacity
    | "70%".WithCell().Color(Colors.Orange, 0.7f)    // 70% opacity
    | "55%".WithCell().Color(Colors.Orange, 0.55f)   // 55% opacity
    | "40%".WithCell().Color(Colors.Orange, 0.4f)    // 40% opacity
```

The opacity parameter (0.0 to 1.0) controls color intensity, perfect for visualizing data magnitude in heatmaps.

## Box Widget

### Color Opacity Support

The Box widget now supports color opacity, allowing you to create subtle color variations and data visualizations:

```csharp
// Basic color
new Box("Solid color").Color(Colors.Blue)

// Color with opacity (0.0 = transparent, 1.0 = solid)
new Box("50% opacity").Color(Colors.Blue, 0.5f)
new Box("20% opacity").Color(Colors.Blue, 0.2f)
```

The opacity value controls the transparency of the box's background color. This is particularly useful for:
- Creating visual hierarchies with subtle color variations
- Building heatmaps where color intensity represents data values
- Designing layered UI elements with semi-transparent backgrounds
- Implementing cohort analysis and retention grids

The opacity parameter works seamlessly with the `.WithCell()` extension, making it easy to create data-rich grid visualizations.

## Routing

### 404 Not Found Page for Invalid Apps

When users navigate to a non-existent app, Ivy now displays a proper 404 error page instead of failing silently or showing confusing errors:

**Default Behavior:**

If you navigate to an invalid app ID (e.g., `/app?appId=nonexistent`), you'll see a friendly error message:

```
Ouch! :|
Apologies, the app you were looking for was not found.
```

The framework also returns the proper HTTP 404 status code, which is important for SEO and error tracking tools.

**Customizing the 404 Page:**

You can override the default 404 error page with your own custom app using the new `UseErrorNotFound()` method:

```csharp
var server = new Server();

// Use a custom 404 app
server.UseErrorNotFound<MyCustomNotFoundApp>();

// Or use a factory function
server.UseErrorNotFound(() => new MyCustomNotFoundApp());
```

Your custom 404 app can include:
- Branded error messaging that matches your application's tone
- Search functionality to help users find what they were looking for
- Navigation links to popular sections of your app
- Contact information or support links

**How it works:**

The routing system now:
- Detects when an app ID doesn't exist in the app repository
- Displays the 404 app (default or custom) in place of the missing app
- Preserves the invalid URL in the address bar (no redirects)
- Returns HTTP 404 status codes for proper error handling
- Works correctly both with and without Chrome enabled

This improvement provides a better user experience when navigation errors occur, making it clear what went wrong rather than showing cryptic errors or blank pages.

### Framework Routes Now Use /ivy Prefix

All framework-provided routes and resources now use the `/ivy` prefix for better organization and to prevent conflicts with your application routes:

**Updated Routes:**
- SignalR hub: `/ivy/messages` (previously `/messages`)
- Health checks: `/ivy/health` (previously `/health`)
- Static resources: `/ivy/img/`, `/ivy/css/`, etc.

**What This Means:**

If you're referencing framework resources directly (like images or stylesheets), update your paths to use the `/ivy` prefix:

```csharp
// Before
new Image("/img/brand-logo.svg")

// After
new Image("/ivy/img/brand-logo.svg")
```

**Health Check Endpoint:**

The health check endpoint is now available at `/ivy/health` and can be used by monitoring tools and load balancers to verify application availability:

```bash
curl https://your-app.com/ivy/health
```

This change provides a cleaner separation between framework routes and your application's custom routes, reducing the likelihood of routing conflicts.

### App ID Collision Detection

Ivy now automatically detects and prevents routing conflicts between your app IDs and framework routes. When you start your server, the framework will check if any registered app IDs collide with:

- System paths (like `/_framework`, `/api`, `/ivy`)
- Controller routes you've defined (auto-discovered from your ASP.NET Core controllers)
- Paths you've explicitly reserved

If a collision is detected, you'll receive a clear error message at startup:

```
App ID 'api' collides with a reserved path '/api'. Please choose a different App ID.
```

This prevents subtle routing bugs where your app might not load because its ID conflicts with an existing route.

**Auto-Discovery of Controller Routes:**

The framework now automatically discovers routes from your ASP.NET Core controllers and treats them as reserved paths. This means you don't need to manually reserve paths for your API controllers - the framework detects them automatically:

```csharp
// These controller routes are automatically detected and reserved
[Route("api/[controller]")]
public class UsersController : ControllerBase { }

[Route("admin/settings")]
public class SettingsController : ControllerBase { }

// App IDs "api" and "admin" would now cause a collision error at startup
```

Dynamic route segments (like `{id}` or `user-{id}`) are intelligently ignored during collision detection, so only static path segments are considered reserved.

### Reserving Custom Paths

You can now explicitly reserve path segments to prevent them from being used as app IDs using the new `ReservePaths()` method:

```csharp
var server = new Server();

server.ReservePaths("/admin", "/reports", "/dashboard")
    .RegisterApp<MyApp>("admin")  // ❌ This would now throw an error
    .RegisterApp<MyApp>("users")  // ✅ This works fine
    .Start();
```

This is particularly useful when:
- You're planning to add routes in the future and want to reserve the paths ahead of time
- You have static file directories that should never be treated as app IDs
- You want to prevent apps from being registered with conflicting names

**Note:** Path comparison is now case-insensitive, so `/API`, `/api`, and `/Api` are all treated as the same path. This prevents cross-platform routing issues and makes the routing system more predictable.
