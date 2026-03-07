# Ivy Framework Weekly Notes - Week of 2026-03-06

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## Breaking Changes

### Event Handler Naming: Handle*→ On*

All event handler extension methods have been renamed from `Handle*` to `On*` to provide a more intuitive API. This affects all widgets with event handlers including [Button](https://docs.ivy.app/widgets/common/button), [Card](https://docs.ivy.app/widgets/common/card), [Form](https://docs.ivy.app/onboarding/concepts/forms), [Tree](https://docs.ivy.app/widgets/common/tree), [DataTable](https://docs.ivy.app/widgets/advanced/data-table), and input widgets.

**Common renames:**

- `.HandleClick()` – `.OnClick()`
- `.HandleSubmit()` – `.OnSubmit()`
- `.HandleChange()` – `.OnChange()`
- `.HandleSelect()` – `.OnSelect()`
- `.HandleBlur()` – `.OnBlur()`
- `.HandleRowAction()` – `.OnRowAction()`

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
    .Placeholder("Enter description...");
```

**After:**

```csharp
var description = UseState("");
return description.ToTextareaInput()
    .Placeholder("Enter description...");
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

- `TextInputs` – `TextInputVariants`
- `SelectInputs` – `SelectInputVariants`
- `NumberInputs` – `NumberInputVariants`
- `ColorInputs` – `ColorInputVariants`
- `DateTimeInputs` – `DateTimeInputVariants`
- `BoolInputs` – `BoolInputVariants`
- `FileInputs` – `FileInputVariants`
- `CodeInputs` – `CodeInputVariants`
- `FeedbackInputs` – `FeedbackInputVariants`

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

Both the [Text](https://docs.ivy.app/widgets/primitives/text-block) and [Markdown](https://docs.ivy.app/widgets/primitives/markdown) widgets now support text alignment with new fluent methods for controlling how content is aligned within its container. You can align text left (default), center, right, or justify.

**Text widget:**

```csharp
Text.P("Left-aligned paragraph").Left()
Text.P("Centered title or callout").Center()
Text.P("Right-aligned numbers or dates").Right()
Text.P("Justified text that stretches to fill the full width").Justify()
```

**Markdown widget:**

```csharp
new Markdown("# Centered Title").Center()
new Markdown("Right-aligned content").Right()
new Markdown("Justified paragraph text").Justify()
```

<img width="1154" height="478" alt="textAlighnment" src="https://github.com/user-attachments/assets/48db1f85-0d48-49de-8bfc-26700022b50f" />

### Fluent Value Setters for Input Widgets

All input widgets now support fluent `.Value()` setters, making it easier to set initial values or update input values programmatically. This works with all input types including [TextInput](https://docs.ivy.app/widgets/inputs/text-input), [NumberInput](https://docs.ivy.app/widgets/inputs/number-input), [BoolInput](https://docs.ivy.app/widgets/inputs/bool-input), SelectInput, [DateTimeInput](https://docs.ivy.app/widgets/inputs/date-time-input), [ColorInput](https://docs.ivy.app/widgets/inputs/color-input), and more.

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

<img width="707" height="282" alt="textValue" src="https://github.com/user-attachments/assets/87134269-e853-4ef3-b17b-c62c851971a3" />

### Separator Text Alignment

The [Separator](https://docs.ivy.app/widgets/primitives/separator) widget now supports positioning label text along the separator line with the new `.TextAlign()` method. Text can be positioned at Left, Center (default), or Right.

```csharp
Layout.Vertical().Gap(4)
    | new Separator("Left Aligned").TextAlign(TextAlignment.Left)
    | new Separator("Center Aligned").TextAlign(TextAlignment.Center)
    | new Separator("Right Aligned").TextAlign(TextAlignment.Right)
```

### NumberInput Prefix and Suffix

The NumberInput widget now supports prefix and suffix properties, matching the existing pattern on TextInput.

```csharp
var temperature = UseState(22);

return Layout.Vertical()
    | temperature.ToNumberInput()
        .Prefix(Icons.Thermometer)
        .Suffix("°C")
        .WithField()
        .Label("Temperature");
```

<img width="661" height="118" alt="numberPrefixSuffix" src="https://github.com/user-attachments/assets/ec6d0a15-9f3f-49a8-8e23-0c07e581d9de" />

### TextInput OnSubmit Event

The TextInput widget now supports an `OnSubmit` event that fires when the user presses Enter in single-line text inputs.

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

The TextInput widget and all its variants (Password, Search, Textarea) now support minimum length validation with the new `.MinLength()` method.

```csharp
var usernameState = UseState("");

// Combine with MaxLength for range constraints
usernameState.ToTextInput()
    .Placeholder("Between 5 and 10 characters")
    .MinLength(5)
    .MaxLength(10)
```

![textMinSize](https://github.com/user-attachments/assets/ae40c173-a7ce-42f2-81ef-d61c9af5aaa9)

### TextInput Multiline Helper Method

A new `.Multiline()` extension method has been added to `TextInputBase` for quickly converting any TextInput into a textarea.

```csharp
var notes = UseState("");

// New convenient method
notes.ToTextInput()
    .Placeholder("Enter notes...")
    .Multiline()

// Equivalent to
notes.ToTextareaInput()
    .Placeholder("Enter notes...")
```

<img width="230" height="121" alt="TextMultiline" src="https://github.com/user-attachments/assets/4c4cfedf-db03-4678-81dc-4ccea0ebe5b6" />

### FileInput Minimum Size Validation

The [FileInput](https://docs.ivy.app/widgets/inputs/file-input) widget now supports minimum file size validation with the new `.MinFileSize()` method.

```csharp
var file = UseState<FileUpload<byte[]>?>();
var upload = UseUpload(MemoryStreamUploadHandler.Create(file))
    .MinFileSize(FileSize.FromKilobytes(1))   // Minimum 1 KB
    .MaxFileSize(FileSize.FromMegabytes(10)); // Maximum 10 MB

return file
    .ToFileInput(upload)
    .Placeholder("Min 1 KB, Max 10 MB");
```

<img width="641" height="191" alt="FileMinSize" src="https://github.com/user-attachments/assets/dec405f6-3e21-462c-89b4-7b4458e29731" />

### CodeBlock Line Wrapping

The [CodeBlock](https://docs.ivy.app/widgets/primitives/code-block) widget now supports line wrapping with the new `.WrapLines()` method. When enabled, long lines wrap within the code block instead of requiring horizontal scrolling

```csharp
new CodeBlock(@"public class Example {
    public void VeryLongMethodName(string parameter1, int parameter2, bool parameter3) {
        Console.WriteLine(""This is a very long line that will wrap instead of requiring horizontal scrolling."");
    }
}")
    .WrapLines()
    .Language(Languages.Csharp)
```

<img width="714" height="393" alt="codeBlockWrapLines" src="https://github.com/user-attachments/assets/8071e113-933d-4a49-aff5-45c862766626" />

### CodeBlock Starting Line Numbers

The CodeBlock widget now supports custom starting line numbers with the new `.StartingLineNumber()` method. This is useful when displaying code excerpts where you want to preserve the original line numbers from the source file.

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

<img width="691" height="121" alt="codeBlockStart" src="https://github.com/user-attachments/assets/452fa226-aebf-4eaf-943b-4d76119d1bae" />

### Expandable Icon Support

The [Expandable](https://docs.ivy.app/widgets/common/expandable) widget now supports icons with the new `.Icon()` extension method, following the same pattern used by Button and Badge widgets.

```csharp
Layout.Vertical().Gap(2)
    | new Expandable("Settings", "Configure your application preferences here.")
        .Icon(Icons.Settings)
```

<img width="664" height="86" alt="expandableIcon" src="https://github.com/user-attachments/assets/28919b73-f84a-47db-897c-7e9d588201d0" />

### SelectInput Advanced Features

The [SelectInput](https://docs.ivy.app/widgets/inputs/select-input) widget has three powerful new features:

**Search Support:**

```csharp
options.ToSelectInput(options)
    .Searchable(true)
    .SearchMode(SearchMode.Fuzzy)  // or CaseInsensitive, CaseSensitive
    .EmptyMessage("No items found")
```

![selectSearch](https://github.com/user-attachments/assets/cb12a8ae-2fdd-46c7-adf1-bb4b6c4610cb)

**Selection Limits:**

```csharp
// For multi-select variants
colors.ToSelectInput(options)
    .Variant(SelectInputVariants.List)
    .MinSelections(1)  // Must select at least 1
    .MaxSelections(3)  // Can't select more than 3
```

![selectLimits](https://github.com/user-attachments/assets/0aba4221-d443-4e9d-aeaf-ec629db51673)

**Loading State:**

```csharp
var isLoading = UseState(true);

options.ToSelectInput(options)
    .Loading(isLoading.Value)
```

![selectLoading](https://github.com/user-attachments/assets/4d971aa7-e222-4156-a612-1b5feb8306d3)

All three features work across all SelectInput variants (Select, List, Toggle).

### SelectInput Disabled Options

Individual options in SelectInput can now be disabled using the `.Disabled()` method on `Option<T>`. Disabled options appear greyed out and cannot be selected, but remain visible in the list.

```csharp
var fruit = UseState("apple");

var fruitOptions = new IAnyOption[]
{
    new Option<string>("Banana", "banana"),
    new Option<string>("Mango (Coming Soon)", "mango").Disabled(),
};

fruit.ToSelectInput(fruitOptions)
    .Placeholder("Select a fruit...")
```

<img width="2140" height="242" alt="selectDisabled" src="https://github.com/user-attachments/assets/9579f7c0-abb2-4467-a620-b872a82755f3" />

### SelectInput Ghost Styling

All SelectInput and [AsyncSelectInput](https://docs.ivy.app/widgets/inputs/async-select-input) variants now support ghost styling with the new `.Ghost()` extension method. Ghost styling removes borders and background fill, making the select blend into its surroundings.

```csharp
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

<img width="2140" height="438" alt="selectGhost" src="https://github.com/user-attachments/assets/68c1aee0-d05e-4632-a55f-a25c05b07d1f" />

### Card Disabled State

The Card widget now supports a disabled state using the `.Disabled()` extension method.

```csharp
new Card("This card cannot be clicked")
    .Title("Disabled Card")
    .Description("User interaction is disabled")
    .OnClick(_ => client.Toast("This won't fire!"))
    .Disabled()
    .Width(Size.Units(100))
```

<img width="627" height="466" alt="cardDisabled" src="https://github.com/user-attachments/assets/6f76bb93-c2fb-4d23-b317-1ae37f5e206d" />

### Spacer Default Behavior Change

The [Spacer](https://docs.ivy.app/widgets/primitives/spacer) widget now defaults to grow behavior (`flex-grow: 1`), automatically filling available space in the parent layout's direction. This matches the common use case of pushing sibling elements apart without requiring explicit `.Width(Size.Grow())`.

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

The [Html](https://docs.ivy.app/widgets/primitives/html) widget now supports JavaScript execution with the new `DangerouslyAllowScripts()` option. This allows rendering raw HTML that includes `<script>` tags when you trust the source completely.

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

The [Sheet](https://docs.ivy.app/widgets/advanced/sheet) widget now supports sliding in from any edge of the screen with the new `.Side()` API and `SheetSide` enum. Previously sheets only slid from the right; now they can come from Left, Right, Top, or Bottom.

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

https://github.com/user-attachments/assets/71c46968-c23c-4408-b706-65a1107d0683

### SidebarLayout Resizable Width

The [SidebarLayout](https://docs.ivy.app/widgets/layouts/sidebar-layout) widget now supports drag-to-resize functionality with the new `.Resizable()` extension method. Users can drag the sidebar border to adjust its width at runtime, and configure constraints with the [Size API](https://docs.ivy.app/api-reference/ivy-shared/size).

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

### Progress Indeterminate Mode

The [Progress](https://docs.ivy.app/widgets/common/progress) widget now has an explicit `Indeterminate` property for displaying animated progress bars when completion percentage is unknown.

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

https://github.com/user-attachments/assets/8b83063b-8d4d-41cb-8ce8-df3168b432a8

### Table Progress Builder

The [Table](https://docs.ivy.app/widgets/common/table) widget now supports rendering progress bars in cells with the new `.Progress()` builder.

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

<img width="1065" height="296" alt="tableProgress" src="https://github.com/user-attachments/assets/088b82d9-cf8f-49e3-b2ca-e1803eedbc21" />

### DataTable Programmatic Refresh

The DataTable widget now supports programmatic refreshing with the new [UseRefreshToken](https://docs.ivy.app/hooks/core/use-refresh-token) hook and `.RefreshToken()` fluent API. This feature is particularly useful for reloading table data after CRUD operations like creating, updating, or deleting records.

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

### Default Theme Changed to System

The default theme has been changed from 'light' to 'system', so the application now respects the user's system-wide dark/light mode preference by default.

### Badge Click Events

The [Badge](https://docs.ivy.app/widgets/common/badge) widget now supports click events with the new `.OnClick()` extension method.

```csharp
new Badge("Click Me", icon: Icons.MousePointer)
    .OnClick(_ => client.Toast("Badge clicked!"))
```

![badgeClick](https://github.com/user-attachments/assets/46c8f89b-0b7f-421a-9313-e634de27d61e)

### Box Widget Interactivity

The [Box](https://docs.ivy.app/widgets/primitives/box) widget now supports click events and hover effects, making it easy to create interactive regions without using the heavier Card widget.

```csharp
new Box("Interactive box")
    .Hover(CardHoverVariant.PointerAndTranslate)
    .OnClick(() => HandleSelection())
    .Padding(8)
```

When you add `.OnClick()` to a Box, it automatically applies `CardHoverVariant.PointerAndTranslate` for visual feedback. You can customize the hover behavior using `.Hover()` to choose between `None`, `Pointer`, or `PointerAndTranslate`.

![boxHover](https://github.com/user-attachments/assets/b18ffc9f-7db1-458d-89ee-0ef6db883d35)

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

### MCP Server Configuration

The Ivy CLI now includes commands to easily configure the Ivy MCP (Model Context Protocol) Server with your AI-powered IDE. This enables AI assistants like Claude Code, Cursor, VS Code, Copilot, and others to directly interact with the Ivy Framework, providing them with access to documentation, widget properties, and framework-specific knowledge.

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

## What's Changed

* [ErrorSheet]: scope scrolling to error content area by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2310>
- [DataTable]: show scrollbars only when needed by @ArtemLazarchuk in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2315>
- [TextBlock]: Add TextAlignment support and improve Text sample by @ArtemLazarchuk in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2404>
- [TextInput]: add MinLength support and improve samples by @ArtemLazarchuk in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2408>
- [Progress]: standardize Color property by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2417>
- [Tree]: expand nodes on label click by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2419>
- [Security]: fix warning by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2418>
- [Security]: fix bag of warnings by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2424>
- [ListWidget]: remove parent padding hack by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2422>
- [Markdown]:  Added TextAlignment property by @ArtemLazarchuk in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2409>
- [CLI]: add Upgrade docs page and update links by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2427>
- [ButtonWidget]: fix AI variant gradient clipping for Full rounded shape by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2426>
- [OAuth]: Add Sliplane Auth Pprovider and example app by @ArtemLazarchuk in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2414>
- [DataTables]: rename OnRowAction to HandleRowAction by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2423>
- [Inputs]: add fluent setters for Value and OnChange properties by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2406>
- (docs)getting-started-mcp by @joshuauaua in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2405>
- datatables: refresh token support by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2425>
- (async select): fix paddings for list items only in async select by @ArtemKhvorostianyi in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2441>
- refactor: rename input type enums to a consistent `*InputVariant` nam… by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2442>
- (tree): item action menu by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2443>
- (Tree): Remove empty space in nodes with no children by @dcrjodle in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2445>
- [OAuth]: fix callback redirect in Sliplane Auth Provider by @ArtemLazarchuk in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2446>
- (tree):  add a doc section about handling raw actions by @ArtemKhvorostianyi in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2448>
- feat: avoid overlaying of kanban cards by @ArtemKhvorostianyi in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2449>
- [List] Safe full‑bleed mode via remove-parent-padding by @ArtemKhvorostianyi in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2454>
- [OAuth]: Improve Sliplane auth flow and user info by @ArtemLazarchuk in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2461>
- (audio)  refactor AudioRecorder into AudioInput by @joshuauaua in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2456>
- refactor: EventHandler wrapper and Handle*→ On* rename by @ivy-interactive-claude-code[bot] in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2460>
- docs: list all widgets in Widget Library table by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2473>
- refactor: standardize TextArea → Textarea naming by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2472>
- feat: rename MultiLine to Multiline & add Multiline() extension for TextInputBase by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2471>
- feat(badge): add OnClick event handler by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2475>
- feat(box): add OnClick event and HoverVariant support by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2474>
- feat(file-input): add minimum file size validation by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2476>
- feat(Card): add Disabled property to prevent interaction by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2477>
- feat(CodeBlock): add StartingLineNumber property by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2478>
- feat(expandable): add icon support by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2479>
- feat: Add search, loading, and selection limit features to SelectInput. by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2484>
- feat(spacer): default to grow behavior by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2482>
- feat(html): add DangerouslyAllowScripts option to Html widget by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2485>
- docs: remove unnecessary `this.` prefix from hooks in documentation by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2486>
- feat(Sheet): Add Side API for slide direction by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2489>
- feat(progress): add explicit Indeterminate property by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2488>
- feat(select): add Ghost() API to all select variants by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2491>
- feat(table): add Progress() builder renderer by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2492>
- fix(markdown): add missing border to code blocks by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2494>
- feat(SidebarLayout): Add Resizable drag-to-resize support by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2487>
- feat(separator): add TextAlign property with fluent API by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2495>
- feat(Option): Add per-item Disabled support to SelectInput options by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2496>
- fix(themes): change default theme to system for auto dark/light mode by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2497>
- feat(TextInput): add OnSubmit event for Enter key handling by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2498>
- feat(NumberInput): Add Prefix and Suffix support by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2499>
- feat(CodeBlock): add WrapLines option for wrapping long lines by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2500>

## New Contributors

* @ivy-interactive-claude-code[bot] made their first contribution in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2460>

**Full Changelog**: <https://github.com/Ivy-Interactive/Ivy-Framework/compare/v1.2.16...v1.2.17>
