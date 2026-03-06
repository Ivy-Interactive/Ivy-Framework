# Ivy Framework Weekly Notes - Week of 2026-03-06

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## New Widgets

### Tree Widget for Hierarchical Data

`Tree` widget got event handling, support for optional Icon and nested menus, as well as RowActions API like it is done in DataTables

**Basic usage:**

```csharp
new Tree(
    new MenuItem("src")
        .Icon(Icons.Folder)
        .Expanded()
        .Children(
            new MenuItem("components")
                .Icon(Icons.Folder)
                .Children(
                    new MenuItem("Button.tsx").Icon(Icons.Code),
                    new MenuItem("Card.tsx").Icon(Icons.Code)
                ),
            new MenuItem("App.tsx").Icon(Icons.Code)
        )
)
```

**Handling click events:**

```csharp
var selected = UseState("");

return new Tree(
    new MenuItem("src")
        .Icon(Icons.Folder)
        .Expanded()
        .Children(
            new MenuItem("App.tsx").Icon(Icons.Code).Tag("App.tsx"),
            new MenuItem("index.ts").Icon(Icons.Code).Tag("index.ts")
        )
).HandleSelect(e => selected.Set(e.Value?.ToString() ?? ""));
```

Individual items can be marked as `.Disabled()` to prevent interaction.

**Row Actions:**
The Tree widget now supports row actions - contextual actions that appear for each tree item.

```csharp
var lastAction = UseState("None");

return new Tree(
    new MenuItem("src")
        .Icon(Icons.Folder)
        .Children(
            new MenuItem("App.tsx").Icon(Icons.Code).Tag("App.tsx")
        )
)
.RowActions(
    new MenuItem("Edit").Icon(Icons.Pencil).Tag("edit"),
    new MenuItem("More").Icon(Icons.Ellipsis).Children(
        new MenuItem("Duplicate").Icon(Icons.Copy).Tag("duplicate"),
        new MenuItem("Delete").Icon(Icons.Trash).Tag("delete")
    )
)
.OnRowAction(e => lastAction.Set($"{e.Value.ActionTag} on {e.Value.ItemValue}"));
```

## Authentication

### New Sliplane OAuth Provider

The Ivy Framework now supports Sliplane OAuth 2.0 authentication, enabling users to sign in with their Sliplane accounts. This is particularly useful for applications deployed on or integrated with [Sliplane](https://sliplane.io).

**Setup:**

```csharp
using Ivy.Auth.Sliplane;

var server = new Server();
server.UseAuth<SliplaneAuthProvider>();
await server.RunAsync();
```

**Configuration** (via user secrets or environment variables):

```terminal
>dotnet user-secrets set "Sliplane:ClientId" "your_client_id"
>dotnet user-secrets set "Sliplane:ClientSecret" "your_client_secret"
```

Or use CLI

```terminal
ivy auth add
```

## API Improvements

### Fluent Value Setters for Input Widgets

All input widgets now support fluent `.Value()` setters, making it easier to set initial values or update input values programmatically. This works with all input types including TextInput, NumberInput, BoolInput, SelectInput, DateTimeInput, ColorInput, and more.

**Usage:**

```csharp
// Set initial value on a text input
var username = UseState("");
username.ToTextInput()
    .Placeholder("Enter username")
    .Value("john_doe")

// Set value on a number input
var age = UseState(0);
age.ToNumberInput()
    .Placeholder("Enter age")
    .Value(25)

// Set value on a select input
var country = UseState("");
country.ToSelectInput(countries)
    .Value("US")
```

## Breaking Changes

### Event Handler Naming: Handle*→ On*

All event handler extension methods have been renamed from `Handle*` to `On*` to provide a more intuitive API. This affects all widgets with event handlers including Button, Card, Form, Tree, DataTable, and input widgets.

**Common renames:**

- `.HandleClick()` → `.OnClick()`
- `.HandleSubmit()` → `.OnSubmit()`
- `.HandleChange()` → `.OnChange()`
- `.HandleSelect()` → `.OnSelect()`
- `.HandleBlur()` → `.OnBlur()`
- `.HandleRowAction()` → `.OnRowAction()`

**Before:**

```csharp
new Button("Save")
    .HandleClick(async () => await SaveAsync());

model.ToForm()
    .HandleSubmit(async (data) => await SaveAsync(data));

new Tree(items)
    .HandleSelect(e => selectedItem.Set(e.Value));
```

**After:**

```csharp
new Button("Save")
    .OnClick(async () => await SaveAsync());

model.ToForm()
    .OnSubmit(async (data) => await SaveAsync(data));

new Tree(items)
    .OnSelect(e => selectedItem.Set(e.Value));
```

### AudioRecorder Widget Renamed to AudioInput

The `AudioRecorder` widget has been renamed to `AudioInput` for better consistency with other input widgets in the framework.

**Before:**

```csharp
new AudioRecorder(upload.Value, "Start recording", "Recording...")
```

**After:**

```csharp
new AudioInput(upload.Value, "Start recording", "Recording...")
```

### TextArea Input Method Renamed to Textarea

The `ToTextAreaInput()` extension method has been renamed to `ToTextareaInput()` (lowercase 'a') to align with the HTML `<textarea>` element specification and match the `TextInputVariants.Textarea` enum value.

**Before:**

```csharp
var description = UseState("");
return description.ToTextAreaInput()
    .Placeholder("Enter description...")
    .Rows(4);
```

**After:**

```csharp
var description = UseState("");
return description.ToTextareaInput()
    .Placeholder("Enter description...")
    .Rows(4);
```

### MultiLine Property and Methods Renamed to Multiline

The `MultiLine` property and methods have been renamed to `Multiline` (lowercase 'l') across the framework for consistency with .NET naming conventions. This affects `Detail`, `TableCell`, and their respective builders.

**Before:**

```csharp
// In Details
new Detail("Notes", notes, multiLine: true);
model.ToDetails().MultiLine(e => e.Description);

// In Tables
records.ToTable().MultiLine(e => e.Content);
new TableCell(content).MultiLine();
```

**After:**

```csharp
// In Details
new Detail("Notes", notes, multiline: true);
model.ToDetails().Multiline(e => e.Description);

// In Tables
records.ToTable().Multiline(e => e.Content);
new TableCell(content).Multiline();
```

### Input Widget Enum Naming Convention

All input widget variant enums have been renamed to follow a consistent `*InputVariants` (plural) naming pattern. This provides better consistency across the framework and aligns with the standard `{Widget}Variant` convention used by other widgets.

**Updated enum names:**

- `TextInputs` → `TextInputVariants`
- `SelectInputs` → `SelectInputVariants`
- `NumberInputs` → `NumberInputVariants`
- `ColorInputs` → `ColorInputVariants`
- `DateTimeInputs` → `DateTimeInputVariants`
- `BoolInputs` → `BoolInputVariants`
- `FileInputs` → `FileInputVariants`
- `CodeInputs` → `CodeInputVariants`
- `FeedbackInputs` → `FeedbackInputVariants`

**Before:**

```csharp
myState.ToTextInput().Variant(TextInputs.Email);
myState.ToColorInput().Variant(ColorInputs.Swatch);
myState.ToBoolInput().Variant(BoolInputs.Switch);
```

**After:**

```csharp
myState.ToTextInput().Variant(TextInputVariants.Email);
myState.ToColorInput().Variant(ColorInputVariants.Swatch);
myState.ToBoolInput().Variant(BoolInputVariants.Switch);
```

Simply replace all instances of the old enum names with their new `*Variants` counterparts in your codebase. Running `dotnet build` will highlight all locations that need updating.

## Widget Improvements

### Text Alignment Support

Both the `Text` and `Markdown` widgets now support text alignment with new fluent methods for controlling how content is aligned within its container. You can align text left (default), center, right, or justify.

**Text widget usage:**

```csharp
Text.P("Left-aligned paragraph").Left()
Text.P("Centered title or callout").Center()
Text.P("Right-aligned numbers or dates").Right()
Text.P("Justified text that stretches to fill the full width").Justify()
```

**Markdown widget usage:**

```csharp
new Markdown("# Centered Title").Center()
new Markdown("Right-aligned content").Right()
new Markdown("Justified paragraph text").Justify()
```

You can also use the generic `.Align()` method on both widgets:

```csharp
Text.P("Custom alignment").Align(TextAlignment.Center)
new Markdown("Custom alignment").Align(TextAlignment.Right)
```

### DataTable Programmatic Refresh

The `DataTable` widget now supports programmatic refreshing with the new `UseRefreshToken()` hook and `.RefreshToken()` fluent API. This feature is particularly useful for reloading table data after CRUD operations like creating, updating, or deleting records.

**Usage:**

```csharp
public class EmployeeTable : ViewBase
{
    public override object? Build()
    {
        var refreshToken = UseRefreshToken();
        var employees = GetEmployees().AsQueryable();

        var table = employees
            .ToDataTable(e => e.Id)
            .RefreshToken(refreshToken)
            .Header(e => e.Name, "Name")
            .Height(Size.Units(100));

        var refreshButton = new Button("Reload Table").OnClick(e =>
        {
            // Trigger a refresh of the DataTable
            refreshToken.Refresh();
        });

        return new Fragment(refreshButton, table);
    }
}
```

### CodeBlock Line Wrapping

The `CodeBlock` widget now supports line wrapping with the new `.WrapLines()` method. When enabled, long lines wrap within the code block instead of requiring horizontal scrolling

**Usage:**

```csharp
new CodeBlock(@"public class Example {
    public void VeryLongMethodName(string parameter1, int parameter2, bool parameter3) {
        Console.WriteLine(""This is a very long line that will wrap instead of requiring horizontal scrolling."");
    }
}")
    .WrapLines()
    .Language(Languages.Csharp)
```

### CodeBlock Starting Line Numbers

The `CodeBlock` widget now supports custom starting line numbers with the new `.StartingLineNumber()` method. This is useful when displaying code excerpts where you want to preserve the original line numbers from the source file.

**Usage:**

```csharp
new CodeBlock(@"    private static int Calculate(int input)
    {
        return input * 2 + 1;
    }
}")
    .ShowLineNumbers()
    .StartingLineNumber(18)  // Start numbering from line 18
    .Language(Languages.Csharp)
```

### Expandable Icon Support

The `Expandable` widget now supports icons with the new `.Icon()` extension method, following the same pattern used by Button and Badge widgets.
**Usage:**

```csharp
Layout.Vertical().Gap(2)
    | new Expandable("Settings", "Configure your application preferences here.")
        .Icon(Icons.Settings)
    | new Expandable("User Profile", "View and edit your profile information.")
        .Icon(Icons.User)
    | new Expandable("Notifications", "Manage your notification preferences.")
        .Icon(Icons.Bell)
```

### SelectInput Advanced Features

The `SelectInput` widget has three powerful new features:

**Search Support:**

```csharp
options.ToSelectInput(options)
    .Searchable(true)
    .SearchMode(SearchMode.Fuzzy)  // or CaseInsensitive, CaseSensitive
    .EmptyMessage("No items found")
```

**Selection Limits:**

```csharp
// For multi-select variants
colors.ToSelectInput(options)
    .Variant(SelectInputVariants.List)
    .MinSelections(1)  // Must select at least 1
    .MaxSelections(3)  // Can't select more than 3
```

**Loading State:**

```csharp
var isLoading = UseState(true);

options.ToSelectInput(options)
    .Loading(isLoading.Value)
```

All three features work across all SelectInput variants (Select, List, Toggle).

### Spacer Default Behavior Change

The `Spacer` widget now defaults to grow behavior (`flex-grow: 1`), automatically filling available space in the parent layout's direction. This matches the common use case of pushing sibling elements apart without requiring explicit `.Width(Size.Grow())`.

**Before:**

```csharp
Layout.Horizontal().Gap(4)
    | new Button("Left Button").Variant(ButtonVariant.Outline)
    | new Spacer().Width(Size.Grow())  // Had to specify explicitly
    | new Button("Right Button").Variant(ButtonVariant.Primary)
```

**After:**

```csharp
Layout.Horizontal().Gap(4)
    | new Button("Left Button").Variant(ButtonVariant.Outline)
    | new Spacer()  // Automatically grows to fill space
    | new Button("Right Button").Variant(ButtonVariant.Primary)
```

### Html Widget Script Execution

The `Html` widget now supports JavaScript execution with the new `DangerouslyAllowScripts()` option. This allows rendering raw HTML that includes `<script>` tags when you trust the source completely.

**Usage:**

```csharp
var htmlWithScript = """
    <div id="target-div">Loading...</div>
    <script>
        document.getElementById('target-div').innerText = 'Script executed successfully!';
    </script>
    """;

new Html(htmlWithScript).DangerouslyAllowScripts()
```

### Sheet Slide Directions

The `Sheet` widget now supports sliding in from any edge of the screen with the new `.Side()` API and `SheetSide` enum. Previously sheets only slid from the right; now they can come from Left, Right, Top, or Bottom.

**Usage:**

```csharp
// Slide from left (great for navigation)
new Button("Left Sheet").WithSheet(
    () => new Card("Navigation Panel").Title("Menu"),
    title: "Navigation",
    side: SheetSide.Left
)

// Or a Sheet directly from the bottom
new Sheet().Side(SheetSide.Bottom)
```

### Progress Indeterminate Mode

The `Progress` widget now has an explicit `Indeterminate` property for displaying animated progress bars when completion percentage is unknown.

**Usage:**

```csharp
// Basic indeterminate progress
new Progress().Indeterminate().Goal("Loading...")

// Toggle between indeterminate and determinate
var isLoading = UseState(true);
var progress = UseState(0);

new Progress(progress.Value)
    .Indeterminate(isLoading.Value)
    .Goal(isLoading.Value ? "Syncing..." : $"{progress.Value}% Complete")
```

### Table Progress Builder

The `Table` widget now supports rendering progress bars in cells with the new `.Progress()` builder.

**Usage:**

```csharp
var tasks = new[] {
    new {Name = "Design Review", Progress = 100},
    new {Name = "Implementation", Progress = 75},
    new {Name = "Testing", Progress = 45},
    new {Name = "Documentation", Progress = 20}
};

tasks.ToTable()
    .Width(Size.Full())
    .Builder(t => t.Progress, f => f.Progress().AutoColor().Format("%d%"))
```

**Features:**

- `.AutoColor()` - Automatically colors progress bars based on value (green ≥75%, yellow ≥50%, orange ≥25%, red <25%)
- `.Color(Colors.Blue)` - Set a specific color for all progress bars
- `.Format("%d%")` - Display value alongside progress bar

### SidebarLayout Resizable Width

The `SidebarLayout` widget now supports drag-to-resize functionality with the new `.Resizable()` extension method. Users can drag the sidebar border to adjust its width at runtime.

**Usage:**

```csharp
// Basic resizable sidebar
new SidebarLayout(
    mainContent: new Card("Main Content"),
    sidebarContent: Layout.Vertical()
        | Text.P("Drag the right edge to resize")
).Resizable()

// Custom constraints using Size API
new SidebarLayout(
    mainContent: new Card("Main Content"),
    sidebarContent: Text.P("150px min, 400px max")
)
.Width(Size.Px(250).Min(Size.Px(150)).Max(Size.Px(400)))
.Resizable()
```

### Separator Text Alignment

The `Separator` widget now supports positioning label text along the separator line with the new `.TextAlign()` method. Text can be positioned at Left, Center (default), or Right.

**Usage:**

```csharp
Layout.Vertical().Gap(4)
    | new Separator("Left Aligned").TextAlign(TextAlignment.Left)
    | new Separator("Center Aligned").TextAlign(TextAlignment.Center)
    | new Separator("Right Aligned").TextAlign(TextAlignment.Right)
```

### SelectInput Disabled Options

Individual options in `SelectInput` can now be disabled using the `.Disabled()` method on `Option<T>`. Disabled options appear greyed out and cannot be selected, but remain visible in the list.

**Usage:**

```csharp
var fruit = UseState("apple");

var fruitOptions = new IAnyOption[]
{
    new Option<string>("Apple", "apple"),
    new Option<string>("Orange", "orange"),
    new Option<string>("Grape (Out of Stock)", "grape").Disabled(),
    new Option<string>("Banana", "banana"),
    new Option<string>("Mango (Coming Soon)", "mango").Disabled(),
};

fruit.ToSelectInput(fruitOptions)
    .Placeholder("Select a fruit...")
```

### SelectInput Ghost Styling

All `SelectInput` and `AsyncSelectInput` variants now support ghost styling with the new `.Ghost()` extension method. Ghost styling removes borders and background fill, making the select blend into its surroundings.

**Usage:**

```csharp
// Normal select with borders
colorState.ToSelectInput(colorOptions)

// Ghost select without borders
colorState.ToSelectInput(colorOptions).Ghost()

// Works with all variants
colorArrayState.ToSelectInput(colorOptions)
    .Variant(SelectInputVariants.List)
    .Ghost()

// Also works with AsyncSelectInput
guidState.ToAsyncSelectInput(QueryCategories, LookupCategory)
    .Placeholder("Select Category")
    .Ghost()
```

### Default Theme Changed to System

The default theme has been changed from 'light' to 'system', so the application now respects the user's system-wide dark/light mode preference by default.

### NumberInput Prefix and Suffix

The `NumberInput` widget now supports prefix and suffix properties, matching the existing pattern on `TextInput`.

**Usage:**

```csharp
var price = UseState(99.99m);
var weight = UseState(5.5);
var temperature = UseState(22);

return Layout.Vertical()
    | price.ToNumberInput()
        .Prefix("$")
        .Precision(2)
        .WithField()
        .Label("Price")
    | weight.ToNumberInput()
        .Suffix("kg")
        .Precision(1)
        .WithField()
        .Label("Weight")
    | temperature.ToNumberInput()
        .Prefix(Icons.Thermometer)
        .Suffix("°C")
        .WithField()
        .Label("Temperature");
```

### TextInput OnSubmit Event

The `TextInput` widget now supports an `OnSubmit` event that fires when the user presses Enter in single-line text inputs.
**Usage:**

```csharp
var searchQuery = UseState("");
var searchResult = UseState("");

// Search example
searchQuery.ToSearchInput()
    .Placeholder("Search...")
    .OnSubmit(() => searchResult.Set($"Searched for: {searchQuery.Value}"))

// Quick-add example
var tag = UseState("");
var tags = UseState<List<string>>(new List<string>());

tag.ToTextInput()
    .Placeholder("Add a tag...")
    .OnSubmit(() =>
    {
        if (!string.IsNullOrWhiteSpace(tag.Value))
        {
            tags.Set(new List<string>(tags.Value) { tag.Value });
            tag.Set("");
        }
    })
```

### TextInput MinLength Validation

The `TextInput` widget and all its variants (Password, Search, Textarea) now support minimum length validation with the new `.MinLength()` method.
**Usage:**

```csharp
var usernameState = UseState("");

// Require at least 3 characters
usernameState.ToTextInput()
    .Placeholder("At least 3 characters")
    .MinLength(3)

// Combine with MaxLength for range constraints
usernameState.ToTextInput()
    .Placeholder("Between 5 and 10 characters")
    .MinLength(5)
    .MaxLength(10)
```

### Badge Click Events

The `Badge` widget now supports click events with the new `.OnClick()` extension method.

**Usage:**

```csharp
new Badge("Click Me", icon: Icons.MousePointer)
    .OnClick(_ => client.Toast("Badge clicked!"))

new Badge("Filter", icon: Icons.ListFilterPlus, variant: BadgeVariant.Secondary)
    .OnClick(_ => ApplyFilter())

new Badge("Remove", icon: Icons.X, variant: BadgeVariant.Destructive)
    .OnClick(_ => RemoveItem())
```

### Box Widget Interactivity

The `Box` widget now supports click events and hover effects, making it easy to create interactive regions without using the heavier Card widget.

**Click events:**

```csharp
new Box("Clickable Box")
    .OnClick(() => client.Toast("Box clicked!"))
    .Padding(8)
```

**Hover effects:**

```csharp
new Box("Hover over me")
    .Hover(CardHoverVariant.Pointer)
    .Padding(8)

new Box("Interactive box")
    .Hover(CardHoverVariant.PointerAndTranslate)
    .OnClick(() => HandleSelection())
    .Padding(8)
```

When you add `.OnClick()` to a Box, it automatically applies `CardHoverVariant.PointerAndTranslate` for visual feedback. You can customize the hover behavior using `.Hover()` to choose between `None`, `Pointer`, or `PointerAndTranslate`.

### Card Disabled State

The `Card` widget now supports a disabled state using the `.Disabled()` extension method.

**Usage:**

```csharp
new Card("This card cannot be clicked")
    .Title("Disabled Card")
    .Description("User interaction is disabled")
    .OnClick(_ => client.Toast("This won't fire!"))
    .Disabled()
    .Width(Size.Units(100))
```

### FileInput Minimum Size Validation

The `FileInput` widget now supports minimum file size validation with the new `.MinFileSize()` method.

**Usage:**

```csharp
var file = UseState<FileUpload<byte[]>?>();
var upload = UseUpload(MemoryStreamUploadHandler.Create(file))
    .MinFileSize(FileSize.FromKilobytes(1))   // Minimum 1 KB
    .MaxFileSize(FileSize.FromMegabytes(10)); // Maximum 10 MB

return file
    .ToFileInput(upload)
    .Placeholder("Min 1 KB, Max 10 MB");
```

### TextInput Multiline Helper Method

A new `.Multiline()` extension method has been added to `TextInputBase` for quickly converting any text input into a textarea.

**Usage:**

```csharp
var notes = UseState("");

// New convenient method
notes.ToTextInput()
    .Placeholder("Enter notes...")
    .Multiline()

// Equivalent to
notes.ToTextareaInput()
    .Placeholder("Enter notes...")

// Or
notes.ToTextInput()
    .Variant(TextInputVariants.Textarea)
    .Placeholder("Enter notes...")
```

## Bug Fixes

### MCP Server Configuration

The Ivy CLI now includes commands to easily configure the Ivy MCP (Model Context Protocol) Server with your AI-powered IDE. This enables AI assistants like Claude Code, Cursor, VS Code Copilot, and others to directly interact with the Ivy Framework, providing them with access to documentation, widget properties, and framework-specific knowledge.

**Quick setup with IDE-specific configuration:**

```terminal
# Set up for Claude Code
>ivy init --hello --claude

# Set up for Cursor
>ivy init --hello --cursor
```

The `--hello` flag scaffolds a sample project and automatically configures the IDE-specific MCP settings in one command.

**Manual MCP configuration:**

```terminal
# Initialize your project
>ivy init

# Generate MCP server configuration
>ivy mcp config
```

The `ivy mcp config` command generates the appropriate MCP server configuration file for your IDE, making it easy to connect your AI tools to the Ivy ecosystem.
