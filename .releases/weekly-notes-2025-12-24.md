# Ivy Framework Weekly Notes - Week of 2025-12-24

## API Changes

### PivotTable Sorting Behavior

The `PivotTable` widget no longer automatically sorts results alphabetically by the first dimension. Previously, results were always sorted by the first dimension column, which could be unexpected and made it difficult to preserve custom ordering or natural data order.

**What Changed:**

- Removed automatic `Sort()` call on pivot table results
- Results now maintain the order they appear in the source data
- You can now control sorting explicitly if needed

**Example:**

```csharp
// Before: Results were always sorted alphabetically by first dimension
var pivot = new PivotTable<SalesData>()
    .AddDimension(x => x.Category)  // Was automatically sorted A-Z
    .AddCalculation(x => x.Amount.Sum());

// After: Results maintain natural order from your data
var pivot = new PivotTable<SalesData>()
    .AddDimension(x => x.Category)  // Preserves order from source
    .AddCalculation(x => x.Amount.Sum());
// Sort explicitly if needed: results.OrderBy(r => r["Category"])
```

This change gives you more control over how your pivot table data is presented.

### GridView Height Control

The `GridView` widget now supports explicit height control through a new `Height()` method. Previously, grid views would automatically size based on their content, but you can now set a specific height when needed.

**What Changed:**

- Added `Height(Size height)` method to `GridView`
- Allows explicit height control for grid layouts
- Works with Ivy's `Size` type for consistent sizing across components

**Example:**

```csharp
// Set explicit height for a grid view
var productGrid = new GridView()
    .Columns(3)
    .Height(Size.Pixels(400))  // Fixed height grid
    .Gap(Size.Pixels(16))
    | productCards;

// Useful for scrollable grids with many items
var imageGallery = new GridView()
    .Columns(4)
    .Height(Size.ViewportHeight(80))  // 80% of viewport height
    .ColumnWidths(Size.Fr(1), Size.Fr(1), Size.Fr(1), Size.Fr(1))
    | images;
```

This is particularly useful when creating scrollable grid containers or when you need consistent sizing across different views.

### TextInput Prefix/Suffix Refactoring

The `TextInput` widget's prefix and suffix API has been refactored for better simplicity. The previous discriminated union type `PrefixSuffix` has been replaced with a simpler `Affix` record that supports both text and icons.

**What Changed:**

- `PrefixSuffix.Text` and `PrefixSuffix.Icon` → `Affix` with `Text` and `Icon` properties
- New extension methods: `ToAffix()` for both strings and Icons
- Sample code updated from `TextInputPrefixSuffix` → `TextInputAffixes`

**Migration Example:**

```csharp
// Before
widget.Prefix(new PrefixSuffix.Text("$"))
widget.Suffix(new PrefixSuffix.Icon(Icons.Search))

// After - Same API, simplified internally
widget.Prefix("$")
widget.Suffix(Icons.Search)
```

The extension methods remain unchanged, so your existing code using `Prefix(string)` and `Suffix(Icons)` continues to work without modification.

### Article Widget - Table of Contents

The `Article` widget now supports passing table of contents headings directly as a property, enabling compile-time TOC generation instead of runtime extraction from the DOM.

**What Changed:**

- Added `Headings` property to `Article` widget accepting `List<ArticleHeading>`
- New `ArticleHeading` record: `ArticleHeading(string Id, string Text, int Level)`
- Added `Headings()` extension method for fluent configuration
- Ivy.Docs.Tools `MarkdownConverter` now extracts headings at compile-time during documentation generation

**Example:**

```csharp
// Manually provide headings for faster TOC rendering
var article = new Article()
    .ShowToc(true)
    .Headings(new List<ArticleHeading>
    {
        new("introduction", "Introduction", 2),
        new("getting-started", "Getting Started", 2),
        new("installation", "Installation", 3),
        new("configuration", "Configuration", 3)
    })
    | content;
```

This change significantly improves performance by eliminating runtime DOM parsing, loading states, and retry logic in the frontend. For documentation sites using Ivy.Docs.Tools, TOC headings are now automatically extracted during markdown compilation.

## Framework Improvements

### AOT (Ahead-Of-Time) Compilation Support

Ivy Framework now supports AOT compilation for better performance and faster startup times. This enables native compilation of your Ivy apps, resulting in smaller deployments and improved runtime performance.

**What Changed:**

- Added AOT compatibility to core framework and Ivy.Filters
- Implemented static YAML context for AOT-safe serialization
- Updated project files with `IsAotCompatible` and `IsTrimmable` properties
- Switched to AOT-compatible JSON serialization
- Refactored `FilterParserAgent` to use `FilterYamlContext`

**Benefits:**

- Faster application startup
- Smaller deployment size through trimming
- Better performance with native compilation
- Future-ready for .NET AOT scenarios

AOT support is transparent - your existing Ivy code continues to work without changes. When you publish with AOT enabled, you'll automatically benefit from the optimizations.

## New Features

### Nullable Input Support

All input widgets now support nullable values with a dedicated clear button. When an input is marked as nullable and has a value, a clear (X) button appears, allowing users to reset the field to null.

**What Changed:**

- Added `.Nullable()` method to all input widgets
- Clear button automatically appears when nullable inputs have values
- Supports all input types: Text, Number, DateTime, Select, Color, Code, Boolean, and Feedback
- Automatically detects nullable types (e.g., `string?`, `int?`, `DateTime?`)
- Clear button scales appropriately with input size (Small, Medium, Large)

**Example:**

```csharp
// String inputs
var optionalText = UseState((string?)null);
var textInput = optionalText.ToTextInput()
    .Placeholder("Optional field...")
    .Nullable();

// Number inputs
var optionalAge = UseState((int?)null);
var ageInput = optionalAge.ToNumberInput()
    .Placeholder("Enter age...")
    .Nullable();

// DateTime inputs
var optionalDate = UseState((DateOnly?)null);
var dateInput = optionalDate.ToDateInput()
    .Placeholder("Select date...")
    .Nullable();

// Select inputs
var optionalCategory = UseState((string?)null);
var selectInput = optionalCategory.ToSelectInput(
    new[] {
        new Option<string>("tech", "Technology"),
        new Option<string>("design", "Design"),
        new Option<string>("business", "Business")
    },
    "Select category..."
).Nullable();

// Multi-select inputs
var optionalTags = UseState((string[]?)null);
var multiSelect = optionalTags.ToSelectInput(
    new[] {
        new Option<string>("tag1", "Tag 1"),
        new Option<string>("tag2", "Tag 2")
    },
    "Select tags..."
).Nullable();

// Boolean inputs
var optionalFlag = UseState((bool?)null);
var boolInput = optionalFlag.ToBoolInput("Accept terms").Nullable();

// Code inputs
var optionalCode = UseState((string?)null);
var codeInput = optionalCode.ToCodeInput()
    .Placeholder("Enter code...")
    .Nullable();

// Color inputs
var optionalColor = UseState((string?)null);
var colorInput = optionalColor.ToColorInput()
    .Placeholder("Pick color...")
    .Nullable();
```

**UI Enhancements:**

- Clear button positioned to the right of the input, before the invalid icon (if present)
- Proper spacing between clear button, invalid icons, and other input controls
- Hover states and focus management for accessibility
- Responsive scaling across Small, Medium, and Large input sizes

This feature makes it easy to create forms with truly optional fields, where users can distinguish between "empty" and "not provided" states.

### DataTable AI-Powered Filtering

DataTable now supports natural language filtering powered by Large Language Models (LLMs). Users can type conversational queries instead of writing formal filter expressions, and the AI will convert them to the appropriate filter syntax.

**What Changed:**

- Added `AllowLlmFiltering` configuration option to enable AI-powered filtering
- Natural language queries are automatically converted to structured filter expressions
- Smart interpretation with typo tolerance and concept mapping

**Example:**

```csharp
public record Employee(int Id, string Name, decimal Salary, bool IsActive);

// Enable AI filtering
var employees = GetEmployees().AsQueryable();
return employees.ToDataTable(e => e.Id)
    .Header(e => e.Name, "Employee Name")
    .Header(e => e.Salary, "Salary")
    .Header(e => e.IsActive, "Active")
    .Config(config =>
    {
        config.AllowSorting = true;
        config.AllowFiltering = true;
        config.AllowLlmFiltering = true;  // Enable AI filtering
    });
```

**Natural Language Queries:**

Users can now filter using conversational phrases:

- "employees older than 30"
- "salary above 100000"
- "active managers"
- "hired in 2023"

The AI agent intelligently handles typos, maps concepts (like "retirement age" to `[Age] >= 65`), and resolves type mismatches by suggesting appropriate alternative fields. The AI converts queries to structured filter expressions using comparisons (`=`, `>`, `<`), text operations (`contains`, `starts with`), existence checks (`IS BLANK`), and logical operators (`AND`, `OR`, `NOT`).

## UI/UX Improvements

### Layout Padding for Non-Chrome Apps

Apps running without the chrome UI (navigation and header) now have consistent padding applied automatically. Previously, padding behavior was inconsistent when using `?chrome=false` in the URL, which could cause content to touch the edges of the viewport.

**What Changed:**

- Non-chrome apps now automatically wrap content in a padded container (`p-4` with full width/height and scroll)
- `HeaderLayout` widget now applies padding to content area consistently
- Simplified padding logic by removing conditional chrome checks

This ensures your app content has proper spacing from viewport edges when embedded or displayed without the standard Ivy chrome UI.

### SelectInput Toggle with Icons

The `SelectInput` widget's toggle variant now supports icons, making it perfect for formatting toolbars and icon-based option selectors. This is ideal for rich text editors, toolbar buttons, and visual selection interfaces.

**Example:**

```csharp
var textFormat = UseState("bold");

// Icon-based toggle buttons
var formatSelector = textFormat.ToSelectInput(new[]
{
    new Option<string>(null, "bold", icon: Icons.Bold),
    new Option<string>(null, "italic", icon: Icons.Italic),
    new Option<string>(null, "underline", icon: Icons.Underline)
})
.Variant(SelectInputs.Toggle);
```

This feature makes it easy to create formatting toolbars, view mode switchers, and other icon-based selection interfaces.

### MetricView and Card Design Updates

The `MetricView` and `Card` widgets have been redesigned for better visual hierarchy and readability. This update includes several improvements to how metric cards display data and trends.

**What Changed:**

**New Text Size:**

- Added `Text.ExtraLarge()` for larger text displays, perfect for highlighting key metrics

**Metric View Redesign:**

- Metric values now display in extra-large text for better visibility
- Trend indicators (↑ ↗ ↓) and percentages moved to top-right corner of the card header
- Progress bars positioned at the bottom as footer element
- Improved layout with better use of space

**Card Widget Improvements:**

- Header layout now uses center alignment with title on left and optional icon on right
- Title and icon parameters now properly support nullable values
- Better spacing control with reduced gaps between sections
- Icon colors changed from black to neutral/muted tones for softer appearance
- Enhanced responsive behavior and text wrapping

**Example:**

```csharp
// MetricView with the new design
var revenueMetric = new MetricView(
    title: "Total Revenue",
    metricData: async () => new MetricRecord(
        MetricFormatted: "$84,200",
        TrendComparedToPreviousPeriod: 0.12, // 12% increase
        GoalAchieved: 0.84,
        GoalFormatted: "$100,000 target"
    )
);

// Custom card with header, content, and footer
var card = new Card(
    content: Text.ExtraLarge("1,234").Color(Colors.Primary),
    header: Layout.Horizontal().Align(Align.Center)
        | Text.H4("Active Users").WithLayout().Grow()
        | Icons.Users.ToIcon().Color(Colors.Muted),
    footer: new Progress(75).Goal("2,000 target")
).Medium();
```

The redesigned metric cards now provide clearer visual hierarchy, making it easier to scan dashboards and focus on key numbers at a glance.

## Framework Requirements

### .NET 10.0 Required

Ivy Framework now requires .NET 10.0 as the target framework. Previously, Ivy required .NET 9.0. All Ivy projects and packages are now built against .NET 10.0.

**What this means for you:**

- Install the latest .NET 10.0 SDK to build and run Ivy applications
- Update your project files to target .NET 10.0
- Existing projects on .NET 9.0 should migrate to .NET 10.0

You can download .NET 10.0 from the [official .NET download page](https://dotnet.microsoft.com/download).

## Documentation

### Introduction Guide Revamp

The [Getting Started documentation](https://docs.ivy.app/onboarding/getting-started/introduction) has been significantly improved to provide clearer onboarding paths for new users. The introduction now focuses on practical setup methods and includes comprehensive code examples.

**What Changed:**

- Added detailed section on file-based apps using .NET 10's single-file feature
- Complete walkthrough with line-by-line code explanations
- Improved project structure documentation for CLI-based projects
- Better organization of framework capabilities and resources
- Enhanced links to samples, examples, and community resources

The new documentation makes it easier for developers to understand Ivy's approach and get started quickly with either file-based scripts or full CLI-initialized projects.

### Kanban and Sheet Widget Example Improvements

The documentation examples for Kanban and Sheet widgets have been updated with improved card movement logic. The `HandleMove` examples now properly handle card insertion at specific positions when dragging and dropping cards between columns.

**What Changed:**

- Fixed `HandleMove` logic to properly calculate insertion index when moving cards
- Added null checks for task ID and task object
- Improved card positioning logic to insert cards at the correct index within target columns
- Better handling of edge cases when moving cards to empty columns or end of lists
- Removed redundant `CardOrder` configuration in Kanban custom cards example

**Updated Example Pattern:**

```csharp
.HandleMove(moveData =>
{
    var taskId = moveData.CardId?.ToString();
    if (string.IsNullOrEmpty(taskId)) return;

    var updatedTasks = tasks.Value.ToList();
    var taskToMove = updatedTasks.FirstOrDefault(t => t.Id == taskId);
    if (taskToMove == null) return;

    var updated = taskToMove with { Status = moveData.ToColumn };
    updatedTasks.RemoveAll(t => t.Id == taskId);

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
        var lastTaskInColumn = updatedTasks.LastOrDefault(t => t.Status == moveData.ToColumn);
        if (lastTaskInColumn != null)
        {
            insertIndex = updatedTasks.IndexOf(lastTaskInColumn) + 1;
        }
    }

    updatedTasks.Insert(insertIndex, updated);
    tasks.Set(updatedTasks.ToArray());
});
```

This improved pattern ensures that cards are inserted at the exact position where they're dropped, respecting the `TargetIndex` provided by the move event and handling edge cases gracefully.

### Markdown Rendering - H5 and H6 Support

The markdown renderer now supports H5 and H6 heading levels for more granular content hierarchy. Previously, only H1-H4 headings were styled and included in the table of contents.

**What Changed:**

- Added H5 and H6 heading components to `MarkdownRenderer`
- New CSS classes for H5 (`text-lg`) and H6 (`text-base`) heading styles
- Table of contents now includes H5 and H6 headings with proper indentation
- Improved indentation logic using explicit padding levels instead of dynamic calculation
- All heading levels now support prop passthrough for better extensibility

**Example:**

```markdown
## Main Section (H2)

### Subsection (H3)

#### Detail (H4)

##### Fine Detail (H5)

###### Finest Detail (H6)
```

The table of contents will now display all six heading levels with appropriate visual hierarchy:

- H1: No indentation
- H2: Small indent (pl-2)
- H3: Medium indent (pl-4)
- H4: Larger indent (pl-6)
- H5: Extra indent (pl-8)
- H6: Maximum indent (pl-10)

This enhancement is particularly useful for detailed technical documentation, API references, and comprehensive guides that require deep content hierarchy.

## What's Changed

- deps: Update React and ReactDOM to 19.2.3. (#1865) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1865>
- (Card): Update Design of Metric Views (#1860) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1860>
- feat: remove redundant theming system (#1864) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1864>
- Refactored Affixes (#1802) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1802>
- [charts]: remove automatic alphabetical sorting in PivotTable (#1871) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1871>
- Feat/docs updates mikael (#1867) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1867>
- [Kanban]: fix examples in docs (#1877) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1877>
- [CodeInput]: fix example in docs (#1878) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1878>
- [GridView]: add Height (#1876) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1876>
- feat(filters,aot): add AOT support and YAML serialization (#18xx)
- docs improvements batch 2 (#1882) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1882>
- refactor(aot): improve trimming/AOT support and simplify config (#18xx)
- (toc): generate compile time instead of frontend runtime (#1883) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1883>
- fix(card,frontend): handle nullable Card title and clean peer flags (#18xx)
- Simplify bug report template by removing fields (#18xx)
- feat(core): add widget tree hashing and duplicate ID checks (#18xx)
- feat(samples): update Kanban and product demos with interactivity (#18xx)
- Added description for allowing LLMs in for querying (#1899) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1899>
- (IAnyInput): handle nullable (#1808) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1808>
- docs: improvements by Mikael vol3 (#1896) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1896>
- (chrome): fix padding rendering for none-chrome apps (#1873) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1873>
- chore: Reduce commit history download to 7 days and update example paths in prompt.
