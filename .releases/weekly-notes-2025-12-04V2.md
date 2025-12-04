# Ivy Framework Weekly Notes - Week of 2025-12-04

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## Overview

This release introduces major Kanban API improvements with simplified card builder requirements, a new Stepper widget for progress tracking, comprehensive form enhancements including upload-aware submission and async submit handlers, expanded URL validation security, cross-tab logout synchronization, and numerous UI refinements including scale support for expandable widgets and improved date-time input styling.

## Improvements

### Form Input Size Consistency

All form input fields now have consistent sizing across different input types. Date-time inputs have been updated to match standard input sizes:

```csharp
// All inputs now use consistent sizing
form.Small()  // All inputs use small scale
form.Medium() // All inputs use medium scale (default)
form.Large()  // All inputs use large scale
```

### Form Improvements

**Upload-Aware Form Submission:**

Forms now automatically detect and prevent submission while file uploads are in progress, providing user feedback via toast notifications:

```csharp
var form = model.ToForm()
    .HandleSubmit(async (model) => {
        // This is called after validation passes
        await SaveToDatabase(model);
    });
```

The form will automatically show a toast message if users try to submit while uploads are still in progress.

**Async Submit Handler:**

Forms now support async submit handlers that are invoked after validation passes but before model state is updated:

```csharp
var form = model.ToForm()
    .HandleSubmit(async (model) => {
        // Save to database, call API, etc.
        await api.SaveAsync(model);
    });
```

**Form Submit Button Customization:**

Customize the submit button using a builder function:

```csharp
var form = model.ToForm()
    .SubmitBuilder(isLoading => 
        new Button("Custom Submit")
            .Loading(isLoading)
            .Disabled(isLoading)
            .Icon(Icons.Save)
    );
```

**Form Scale Defaults:**

All form inputs now default to `Medium` scale for consistency. Scale is properly applied to async select inputs and form buttons.

### Kanban API Improvements

**Simplified Card Builder:**

Card builder is now required and simplified - no more title/description selectors:

```csharp
data.ToKanban(
    groupBySelector: e => e.Status,
    idSelector: e => e.Id,
    orderSelector: e => e.Priority
)
.CardBuilder(task => new Card()
    .Title(task.Title)
    .Description(task.Description))
```

**Column Width API:**

Simplified column width API - set uniform width for all columns:

```csharp
kanban
    .Width(Size.Full())           // Overall kanban width
    .ColumnWidth(Size.Units(300))  // All columns same width
```

**Removed Methods:**

- `HandleClick()` - Use card click handlers directly in CardBuilder
- `HandleDelete()` - Removed from API
- `Width(groupKeySelector, width)` - Use `.ColumnWidth()` instead
- `titleSelector` and `descriptionSelector` parameters - Use `.CardBuilder()` instead

**HeaderLayout Integration:**

Kanban boards can now be used in HeaderLayout with scrolling disabled:

```csharp
var header = Layout.Horizontal() | new Button("Add Task");
var kanban = tasks.ToKanban(...).CardBuilder(...);

return new HeaderLayout(header, kanban)
    .Scroll(Scroll.None); // Disable HeaderLayout scrolling for Kanban
```

### New Stepper Widget

Introducing the `Stepper` widget for displaying and selecting progress steps:

```csharp
var selectedIndex = UseState(0);
var items = new[]
{
    new StepperItem("1", Icons.Check, "Company", "Some description"),
    new StepperItem("2", null, "Raise", "Some description"),
    new StepperItem("3", null, "Deck", "Some description"),
};

return new Stepper(OnSelect, selectedIndex.Value, items)
    .Width(200)
    .AllowSelectForward(); // Allow selecting future steps

ValueTask OnSelect(Event<Stepper, int> e)
{
    selectedIndex.Set(e.Value);
    return ValueTask.CompletedTask;
}
```

**Features:**
- Visual progress indication with completed, current, and upcoming states
- Optional forward selection with `.AllowSelectForward()`
- Custom icons for each step
- Labels and descriptions for each step

### Expandable Widget Scale Support

Expandable widgets now support scale variants:

```csharp
new Expandable("Header", "Content")
    .Small()   // Compact spacing
    .Medium()  // Default spacing
    .Large()   // Generous spacing
```

Scale affects padding, text sizes, and spacing throughout the expandable widget.

### Date-Time Input Styling Improvements

**Time Input Icon Placement:**

Clock icon is now positioned inside the input field for better visual integration.

**Disabled State:**

Date-time inputs now handle disabled state consistently with other input types.

**Clear Button Positioning:**

Clear and invalid icons are now properly positioned outside the input button for better interaction.

### Table Column Width API

Table column width method renamed for clarity:

```csharp
// Before
table.Width(p => p.ColumnName, Size.Units(100))

// After
table.ColumnWidth(p => p.ColumnName, Size.Units(100))
```

### Field Widget Dimensions

Field widgets now support explicit width and height:

```csharp
state.ToTextInput()
    .Width("300px")
    .Height("40px");
```

### State Helpers

New convenience methods for integer state:

```csharp
var count = UseState(0);
count.Incr(); // Increment by 1
count.Decr(); // Decrement by 1
```

Conditional rendering helpers:

```csharp
var isLoading = UseState(false);

return isLoading.True(() => new Loading())!;  // Show when true
return isLoading.False(() => new Text("Ready"))!; // Show when false
```

### Layout Improvements

**TopCenter Layout:**

New layout helper for top-center alignment:

```csharp
Layout.TopCenter(
    new Button("Action 1"),
    new Button("Action 2")
);
```

### Server API Improvements

**Custom Chrome Type:**

Generic method for custom chrome types:

```csharp
server.UseChrome<MyCustomChrome>();
```

**Reserved Paths:**

Prevent app ID collisions with reserved paths:

```csharp
server.ReservePaths("/api", "/admin", "/custom-route");
```

The server now automatically detects controller routes and validates app IDs don't collide with reserved paths.

### Cross-Tab Logout Synchronization

Logout events are now synchronized across browser tabs using the Broadcast Channel API. When a user logs out in one tab, all other tabs automatically reload to reflect the logout state.

### URL Validation Security Enhancements

Comprehensive URL validation has been added to prevent open redirect vulnerabilities and XSS attacks:

**What's Protected:**
- Button URLs - Validates and shows error for invalid URLs
- Image URLs - Validates before rendering
- Audio/Video URLs - Validates media URLs
- Markdown links - All links are sanitized
- Media widgets - Images, audio, and video players validate URLs

**Validation Functions:**
- `validateLinkUrl()` - For anchor tags and navigation
- `validateImageUrl()` - For image sources
- `validateAudioUrl()` - For audio sources
- `validateVideoUrl()` - For video sources
- `validateRedirectUrl()` - For redirect operations

Invalid URLs are rejected and widgets show appropriate error messages instead of rendering unsafe content.

### FileInput Event Handlers

FileInput now supports `OnBlur` event handler that fires when the file dialog closes:

```csharp
files.ToFileInput(upload)
    .HandleBlur((Event<IAnyInput> e) => {
        client.Toast($"Files selected: {files.Value.Length}");
    })
    .HandleCancel((Guid fileId) => {
        upload.Value.Cancel(fileId);
    });
```

The blur event fires when:
- User selects files and closes the dialog
- User cancels the file dialog without selecting files

### Loading Widget State-Based Visibility

Loading widget can now be conditionally rendered based on state:

```csharp
var isLoading = UseState(false);

return isLoading.True(() => new Loading())!;
```

The loading widget includes a 200ms delay before showing to prevent flicker for fast operations.

### Utils.FormatNumber

New utility method for formatting numbers with K/M/B suffixes:

```csharp
Utils.FormatNumber(1500)      // "1.5K"
Utils.FormatNumber(2500000)   // "2.5M"
Utils.FormatNumber(3000000000) // "3B"
```

### Chart Y-Axis Improvements

Charts now properly adjust Y-axis minimum when negative values are present, ensuring all data points are visible.

### AsyncSelectInput Scale Support

AsyncSelectInput now properly supports scale variants and applies scale to the search input in the sheet.

### List Widget Dividers

List widget dividers now extend the full width of the container for better visual separation.

### Article Footer Navigation

Article footer navigation now properly preserves `chrome=false` parameter when navigating between articles.

### Logging Improvements

Logging message templates are now consistent to avoid warnings. Unnecessary logging statements have been cleaned up.

## Breaking Changes

### Kanban API Changes

**Card Builder Now Required:**

The `CardBuilder()` method is now required. The old API with `titleSelector` and `descriptionSelector` is removed:

```csharp
// Old API (no longer supported)
data.ToKanban(
    groupBySelector: e => e.Status,
    idSelector: e => e.Id,
    titleSelector: e => e.Title,
    descriptionSelector: e => e.Description
)

// New API (required)
data.ToKanban(
    groupBySelector: e => e.Status,
    idSelector: e => e.Id,
    orderSelector: e => e.Priority
)
.CardBuilder(task => new Card()
    .Title(task.Title)
    .Description(task.Description))
```

**Removed Methods:**

- `HandleCardMove()` → Use `HandleMove()` instead
- `HandleClick()` - Removed, use card click handlers in CardBuilder
- `HandleDelete()` - Removed
- `Width(groupKeySelector, width)` - Use `.ColumnWidth(width)` instead

**Column Width API:**

```csharp
// Old API (no longer supported)
kanban.Width(e => e.Status, Size.Fraction(0.33f))

// New API
kanban.ColumnWidth(Size.Fraction(0.33f)) // All columns same width
```

### Table Column Width Method Rename

```csharp
// Old API
table.Width(p => p.ColumnName, Size.Units(100))

// New API
table.ColumnWidth(p => p.ColumnName, Size.Units(100))
```

### Kanban Width/Height Methods

Kanban `Width()` and `Height()` methods now only accept `Size` parameter. Removed overloads for `int`, `float`, and `string`:

```csharp
// Old API (no longer supported)
kanban.Width(800)
kanban.Width(0.8f)
kanban.Width("80%")

// New API
kanban.Width(Size.Units(800))
kanban.Width(Size.Fraction(0.8f))
kanban.Height(Size.Full())
```

## Bug Fixes

- **Kanban Card Reordering**: Fixed bug causing cards to be inserted at incorrect positions when dragging between columns. Cards now properly insert at the target index.
- **FileInput OnBlur**: Fixed double-firing of blur events when files are selected. Blur now fires correctly when dialog closes.
- **Form Scale Application**: Fixed issue where form scale wasn't being applied to async select inputs and submit buttons.
- **Chart Y-Axis**: Fixed Y-axis minimum calculation when negative values are present.
- **List Widget Dividers**: Fixed dividers not extending full width of container.
- **Article Footer Navigation**: Fixed `chrome=false` parameter not being preserved when navigating between articles.
- **Logging Templates**: Fixed inconsistent logging message templates that caused warnings.
- **Option Enum Description**: Fixed missing null parameter in Option enum extension method.
- **Field Widget Dimensions**: Fixed width and height not being applied to field widgets.
- **Routing Collisions**: Fixed app ID collisions with custom routes. Server now validates app IDs don't conflict with reserved paths.
- **URL Validation**: Fixed various edge cases in URL validation for images, audio, video, and links.

## Security Improvements

### Comprehensive URL Validation

All URL handling throughout the framework now uses centralized validation functions to prevent:
- Open redirect vulnerabilities
- XSS attacks via `javascript:` protocol
- Protocol injection attacks
- Unsafe blob URL origins
- Invalid data URL types

**Media URL Validation:**

Images, audio, and video widgets now validate URLs before rendering. Invalid URLs show error messages instead of attempting to load unsafe content.

**Blob URL Security:**

Blob URLs are validated to ensure they originate from the current origin, preventing attacks using blob URLs from external origins.

## Documentation Updates

- Updated Kanban documentation to reflect new CardBuilder API
- Updated Table documentation to use `ColumnWidth` instead of `Width`
- Added HeaderLayout documentation for scroll control
- Added troubleshooting section for Apple Silicon Mac Protobuf issues

