# Ivy Framework Weekly Notes - Week of 2026-03-13

## Breaking Changes

### Input Variant Enums Renamed to Singular

To maintain consistency across the Ivy Framework, all input variant enums have been renamed from plural to singular form. This aligns them with other styling enums like `ButtonVariant`, `BadgeVariant`, and `CalloutVariant`.

**What Changed:**

| Old Name (Plural) | New Name (Singular) |
|---|---|
| `TextInputVariants` | `TextInputVariant` |
| `SelectInputVariants` | `SelectInputVariant` |
| `NumberInputVariants` | `NumberInputVariant` |
| `FileInputVariants` | `FileInputVariant` |
| `FeedbackInputVariants` | `FeedbackInputVariant` |
| `DateTimeInputVariants` | `DateTimeInputVariant` |
| `ColorInputVariants` | `ColorInputVariant` |
| `CodeInputVariants` | `CodeInputVariant` |
| `BoolInputVariants` | `BoolInputVariant` |

### Namespaces Flattened

Several core types such as `ExternalWidgetAttribute` and `TextAlignment` have been moved to the root `Ivy` namespace to simplify using directives and prevent refactoring issues (like the `RemoveInvalidIvyUsings` rule incorrectly stripping them).

**Migration:**

Since `using Ivy;` is typically already present in your files, you can simply remove `using Ivy.Core.ExternalWidgets;` and `using Ivy.Shared;`. Internal types like `ExternalWidgetRegistry` and `ExternalWidgetController` remain in `Ivy.Core.ExternalWidgets`.

### Removal of `.Value()` API from Input Widgets

The fluent `.Value()` extension method has been removed from all input widgets as it conflicted with Ivy's state-driven architecture.

**What Changed:**

The `.Value()` method was previously used to set initial values inline directly on the widget, but this approach doesn't align with Ivy's state management philosophy where values should be managed through state.

**Affected Widgets:**

All input widgets are affected: `TextInput`, `SelectInput`, `AsyncSelectInput`, `NumberInput`, `BoolInput`, `CodeInput`, `ColorInput`, `DateRangeInput`, `DateTimeInput`, `FeedbackInput`, `IconInput`, and `ReadOnlyInput`.

### Scale Renamed to Density

The `Scale` enum and all associated APIs have been renamed to `Density` to avoid ambiguity with chart scales, DPI scaling, and other scale-related concepts.

**What Changed:**

- `Ivy.Scale` enum → `Ivy.Density` enum
- `.Scale()` fluent method → `.Density()` method
- Enum values remain unchanged: `Small`, `Medium`, `Large`
- Shortcut methods `.Small()`, `.Medium()`, `.Large()` are unchanged

### Box.Color() Renamed to Box.Background()

The `Color()` method and property on the `Box` widget have been renamed to `Background()` to more clearly reflect what it controls—the background color of the box, not the foreground/text color.

### Text.InlineCode() Renamed to Text.Monospaced()

The `Text.InlineCode()` method and `TextVariant.InlineCode` enum value have been renamed to `Text.Monospaced()` and `TextVariant.Monospaced` to better reflect what they actually do—render text in a monospace font.

**What Changed:**

- `Text.InlineCode()` → `Text.Monospaced()`
- `TextVariant.InlineCode` → `TextVariant.Monospaced`

### Explicit Size API for Width, Height, and Size Methods

The implicit numeric overloads for `Width()`, `Height()`, and `Size()` methods have been removed. You now must explicitly use `Size.Units()` or `Size.Fraction()` to specify sizing.

**What Changed:**

- Removed `Width(int)`, `Width(float)`, `Height(int)`, `Height(float)`, and `Size(int)` overloads
- All sizing now requires explicit `Size` struct usage via `Size.Units()` or `Size.Fraction()`

### ToTextAreaInput() Renamed to ToTextareaInput()

The `ToTextAreaInput()` extension method has been renamed to `ToTextareaInput()` (lowercase 'a') to standardize naming across the codebase.

### Chart Data Syntax Changed from DataPoint Elements to JSON CDATA

The chart data format has been completely redesigned to use JSON arrays within CDATA sections instead of XML `<DataPoint>` elements.

**What Changed:**

- Removed: `<DataPoint>` XML elements with attributes
- Added: JSON arrays using `<Data><![CDATA[...]]></Data>` syntax
- Type handling now preserves strings, numbers, and booleans exactly as specified in JSON

### CreateSignal Renamed to UseSignal and ISignal Interface Unified

The signal creation API has been simplified and made more consistent with other Ivy hooks. `CreateSignal` has been renamed to `UseSignal`, and the separate `ISignalSender` and `ISignalReceiver` interfaces have been unified into a single `ISignal` interface.

**What Changed:**

- `context.CreateSignal<T, TInput, TOutput>()` → `context.UseSignal<T, TInput, TOutput>()`
- `ISignalSender<TInput, TOutput>` interface removed
- `ISignalReceiver<TInput, TOutput>` interface removed
- New unified `ISignal<TInput, TOutput>` interface with both `.Send()` and `.Receive()` methods

### ContentPipeline Renamed to HtmlPipeline with XDocument-Based Filters

The HTML processing pipeline has been refactored for better performance and maintainability. The namespace was renamed from `ContentPipeline` to `HtmlPipeline`, and filters now operate on parsed `XDocument` objects instead of raw HTML strings.

**What Changed:**

- **Namespace renamed:** `Ivy.Core.Server.ContentPipeline` → `Ivy.Core.Server.HtmlPipeline`
- **Filter interface updated:** `IHtmlFilter.Process` now returns `void` and takes an `XDocument` parameter instead of returning a modified HTML string
- **New pipeline customization API:** `Server.UseHtmlPipeline()` allows full control over the filter pipeline
- **New inspection methods:** `HtmlPipeline.Filters` (read-only list) and `HtmlPipeline.Clear()` for pipeline management

**New Customization API:**

You can now fully customize the HTML pipeline, including clearing and replacing all built-in filters:

```csharp
// Replace the entire pipeline
server.UseHtmlPipeline(pipeline =>
{
    pipeline.Clear();  // Remove all built-in filters
    pipeline.Use<MyCustomFilter>();
});

// Or append to the existing pipeline
server.UseHtmlPipeline(pipeline =>
{
    pipeline.Use<OpenGraphFilter>();
});
```

## New Features

### Terminal Emulator Widget with Xterm.js

Ivy now includes a powerful terminal emulator widget through the `Ivy.Widgets.Xterm` package, powered by xterm.js. Build interactive terminal UIs, display command output, or create full terminal experiences directly in your Ivy apps.

**Installation:**

```bash
dotnet add package Ivy.Widgets.Xterm
```

**Basic Usage:**

```csharp
using Ivy.Widgets.Xterm;

// Display terminal output
var terminal = new Terminal()
    .Write("Hello from the terminal!\r\n")
    .Write("This is a fully featured terminal emulator.\r\n");
```

**Interactive Terminal with PTY:**

For interactive shell sessions, use the `Ivy.Hooks.Pty` package to run real processes with PTY support:

```csharp
using Ivy.Hooks.Pty;

// Create an interactive bash/PowerShell terminal
var pty = UsePty("/bin/bash");  // or "powershell.exe" on Windows

return new Terminal()
    .OnInput(input => pty.Write(input))
    .OnResize((cols, rows) => pty.Resize(cols, rows))
    .Source(pty.Output);  // Stream process output to terminal
```

**Terminal with Process Output:**

Run a command and stream its output to the terminal:

```csharp
// Run a console app and display its output
var process = UsePty("dotnet", "run", "--project", "./MyConsoleApp");

return new Terminal()
    .Source(process.Output)
    .OnOutput(data => Console.WriteLine($"Terminal output: {data}"));
```

**Customization:**

```csharp
// Customize terminal appearance
new Terminal()
    .FontSize(14)
    .FontFamily("'Cascadia Code', 'Courier New', monospace")
    .Theme(new TerminalTheme
    {
        Background = "#1e1e1e",
        Foreground = "#d4d4d4",
        Cursor = "#ffffff"
    })
    .CursorBlink(true)
    .ScrollbackLimit(1000);
```

### Screenshot Feedback Widget

Ivy now includes a screenshot and annotation widget through the `Ivy.Widgets.ScreenshotFeedback` package. Capture screenshots of your Ivy app, annotate them with drawing tools, and upload them - perfect for bug reports, feedback forms, and documentation.

**Installation:**

```bash
dotnet add package Ivy.Widgets.ScreenshotFeedback
```

**Basic Usage:**

```csharp
using Ivy.Widgets.ScreenshotFeedback;

var screenshot = UseState<FileUpload<byte[]>?>();
var uploadCtx = UseUpload(MemoryStreamUploadHandler.Create(screenshot));
var isOpen = UseState(false);

return Layout.Vertical().Gap(4)
    | new Button("Take Screenshot", () => isOpen.Set(true), icon: Icons.Camera)
    | new ScreenshotFeedback()
        .UploadUrl(uploadCtx.Value.UploadUrl)
        .Open(isOpen.Value)
        .HandleSave(() => isOpen.Set(false))
        .HandleCancel(() => isOpen.Set(false))
    | (screenshot.Value?.Status == FileUploadStatus.Finished && screenshot.Value.Content != null
        ? new Image("data:image/png;base64," + Convert.ToBase64String(screenshot.Value.Content))
        : Text.Muted("No screenshot captured yet."));
```

**Features:**

- Captures full window screenshots using html2canvas
- Drawing tools: freehand, line, rectangle, circle, text
- Integrates with Ivy's `UseUpload` infrastructure
- Modal interface with save/cancel actions

### Server-to-Client Streaming with UseStream Hook

Ivy now supports efficient server-to-client streaming with the new `UseStream` hook. Stream real-time data from your backend to the frontend without triggering full state re-renders for every chunk. This is perfect for LLM text streaming, progress updates, or any scenario where you need to push data continuously to a single widget.

**Example: Streaming Rich Text from an LLM**

Attach the stream to widgets that support streaming (like `Text.Rich()`):

```csharp
public class StreamingApp : ViewBase
{
    protected override object? Build()
    {
        // 1. Create a stream for text runs
        var stream = Context.UseStream<TextRun>();

        return Layout.Vertical(
            Text.Rich()
                .Bold("🤖 ")
                // 2. Attach the stream to the widget
                .UseStream(stream),

            new Button("Generate").OnClick(async () =>
            {
                var words = new[] { "Hello", "world", "from", "the", "stream!" };

                foreach (var word in words)
                {
                    await Task.Delay(200);
                    // 3. Write data to the stream which gets pushed to the frontend in real-time
                    stream.Write(new TextRun(word) { Word = true });
                }
            })
        );
    }
}
```

**Buffering Behavior:**

By default (`buffer = true`), if you start writing to the stream before the frontend component has fully rendered and subscribed via WebSockets, the data will be buffered in memory on the server. Once the client establishes the subscription, all buffered data will be flushed immediately.

**Features:**

- No full state re-renders for each data chunk
- Automatic buffering until client subscribes
- Type-safe streaming with generics
- Works with widgets like `Text.Rich()` and `Terminal`
- No polling required - true push-based streaming

### Async Cleanup in UseEffect with IAsyncDisposable

`UseEffect` now supports asynchronous cleanup through `IAsyncDisposable`, making it easier to manage async resources like database connections, streams, and network sockets.

**Basic Usage:**

```csharp
UseEffect(() =>
{
    var subscription = SubscribeToWebSocket();

    // Return an async disposable for cleanup
    return AsyncDisposable.Create(async () =>
    {
        await subscription.UnsubscribeAsync();
        await subscription.DisposeAsync();
    });
}, []);
```

### DevTools for Visual Widget Inspection (Development Only)

Ivy now includes built-in DevTools for debugging and inspecting your widget tree during development. Enable it during local development to inspect widgets, view callsite information, and make live text edits.

**Enable DevTools:**

```csharp
var server = new Server()
    .EnableDevTools()  // Only in development builds
    .Run();
```

**Features:**

- Visual widget inspector with highlighting
- Widget callsite information (source file and line number)
- Live text editing for TextBlock and Markdown widgets
- Widget tree visualization

DevTools are designed for development environments only and should not be enabled in production.

### Enhanced Layout System with Figma-Style Options

Ivy's layout system now supports advanced Figma-style layout options, including space distribution, independent row/column gaps, wrapping, per-child alignment, and enhanced scroll control.

**New Alignment Options:**

The `Align` enum now includes space distribution options that work with both `StackLayout` and `GridLayout`:

```csharp
// Space between items
Layout.Horizontal()
    .Align(Align.SpaceBetween)
    | new Button("Left")
    | new Button("Middle")
    | new Button("Right");

// Space around items
Layout.Horizontal()
    .Align(Align.SpaceAround)
    | new Badge("Tag 1")
    | new Badge("Tag 2")
    | new Badge("Tag 3");

// Space evenly distributed
Layout.Vertical()
    .Align(Align.SpaceEvenly)
    | Text.Literal("First")
    | Text.Literal("Second")
    | Text.Literal("Third");
```

**Independent Row and Column Gaps:**

Control row and column spacing independently in both `StackLayout` and `GridLayout`:

```csharp
// StackLayout with different row/column gaps
Layout.Horizontal()
    .Wrap()
    .Gap(rowGap: 2, columnGap: 8)
    | new Badge("Tight rows")
    | new Badge("Wide columns")
    | new Badge("More tags");

// GridLayout with independent gaps
new GridLayout(new GridDefinition
{
    Columns = 3,
    RowGap = 4,      // Vertical spacing
    ColumnGap = 8    // Horizontal spacing
},
    child1, child2, child3
);

// GridView with fluent gap methods
new Grid()
    .Columns("1fr 1fr 1fr")
    .RowGap(4)       // Set only row gap
    .ColumnGap(8)    // Set only column gap
    | child1
    | child2
    | child3;

// Or set both gaps at once
new Grid()
    .Columns("1fr 1fr")
    .Gap(4)          // Sets both row and column gap to 4
    | item1
    | item2;
```

**Wrapping StackLayouts:**

`StackLayout` now supports wrapping, eliminating the need for a separate `WrapLayout` widget:

```csharp
// Horizontal layout that wraps to new lines
Layout.Horizontal()
    .Wrap()
    .Gap(2)
    | new Badge("React")
    | new Badge("Vue")
    | new Badge("Angular")
    | new Badge("Svelte")
    | new Badge("Next.js");

// Vertical layout that wraps to new columns
Layout.Vertical()
    .Wrap(Orientation.Vertical)
    | Text.Literal("Item 1")
    | Text.Literal("Item 2")
    | Text.Literal("Item 3");
```

**Per-Child Alignment with AlignSelf:**

Override alignment for individual children in `StackLayout` and `GridLayout`:

```csharp
Layout.Vertical()
    | new Box("Stretched Item").AlignSelf(Align.Stretch)
    | new Box("Centered Item").AlignSelf(Align.Center)
    | new Box("Left-Aligned Item").AlignSelf(Align.Left);
```

**Enhanced Scroll Options:**

The `Scroll` enum now supports directional scrolling:

```csharp
// Vertical scrolling only
Layout.Vertical()
    .Scroll(Scroll.Vertical)
    | /* content */;

// Horizontal scrolling only
Layout.Horizontal()
    .Scroll(Scroll.Horizontal)
    | /* content */;

// Both directions
Layout.Vertical()
    .Scroll(Scroll.Both)
    | /* content */;
```

**Enhanced Overflow Options:**

New `Overflow` values provide more control:

```csharp
// Allow content to overflow visibly
new Box("Content").Overflow(Overflow.Visible);

// Force scrollbars
new Box("Content").Overflow(Overflow.Scroll);
```

### Border Support for Layouts

LayoutView and StackLayout now support borders with full control over color, thickness, radius, and style.

**Adding Borders:**

```csharp
Layout.Horizontal()
    .Border(Colors.Red, new Thickness(top: 2, right: 1, bottom: 2, left: 1))
    | new Text("Custom border thickness");
```

**Fine-Grained Border Control:**

```csharp
// Full control over all border properties
Layout.Vertical()
    .BorderColor(Colors.Primary)
    .BorderThickness(2)
    .BorderStyle(BorderStyle.Solid)
    .BorderRadius(BorderRadius.Rounded)
    | new Text("Fully customized border");

// Different border styles
Layout.Horizontal()
    .BorderColor(Colors.Gray)
    .BorderThickness(1)
    .BorderStyle(BorderStyle.Dashed)
    .BorderRadius(BorderRadius.None)
    | new Text("Dashed border");

// Fully rounded border
Layout.Vertical()
    .BorderColor(Colors.Success)
    .BorderThickness(2)
    .BorderRadius(BorderRadius.Full)
    | new Badge("Pill-shaped layout");
```

### PWA (Progressive Web App) Manifest Support

Ivy now supports Progressive Web Apps with built-in manifest configuration. Configure your app's PWA settings using the new `UseManifest()` API.

**Basic Usage:**

```csharp
var server = new Server()
    .UseManifest(manifest =>
    {
        manifest.Name = "My Ivy App";
        manifest.ShortName = "MyApp";
        manifest.ThemeColor = "#4A90E2";
        manifest.BackgroundColor = "#ffffff";
        manifest.Icons = new List<ManifestIcon>
        {
            new() { Src = "/icon-192.png", Sizes = "192x192", Type = "image/png" },
            new() { Src = "/icon-512.png", Sizes = "512x512", Type = "image/png" }
        };
    });
```

The manifest is automatically served at `/manifest.json` and linked in your app's HTML `<head>`.

---

### AppBase - Semantic Base Class for Apps

Ivy now includes an `AppBase` class that provides a semantic foundation for building apps. While functionally equivalent to `ViewBase`, it offers clearer intent when defining app-level components.

**Usage:**

```csharp
[App(Title = "My Application", Icon = "🚀")]
public class MyApp : AppBase
{
    protected override Widget Build()
    {
        return new Page("Welcome")
        {
            new Text("Hello from AppBase!")
        };
    }
}
```

**Key Points:**

- `AppBase` extends `ViewBase` - no behavioral differences
- Provides semantic clarity that a class represents an app rather than a generic view
- Works seamlessly with the `[App]` attribute for metadata configuration
- Use `AppBase` for top-level application classes and `ViewBase` for reusable view components

### JavaScript Execution in Html Widget with DangerouslyAllowScripts

The `Html` widget now supports opt-in JavaScript execution through the new `DangerouslyAllowScripts` property. By default, the Html widget sanitizes all JavaScript for security, but you can now bypass this when rendering trusted HTML content that requires script execution.

**⚠️ Security Warning:** Only enable `DangerouslyAllowScripts` for HTML content you completely trust. Rendering untrusted or user-generated content with this flag enabled exposes your application to Cross-Site Scripting (XSS) attacks.

**Usage Example:**

```csharp
public class ScriptHtmlView : ViewBase
{
    public override object? Build()
    {
        var htmlWithScript =
            """
            <div id="target-div">Loading...</div>
            <script>
                document.getElementById('target-div').innerText = 'Script executed successfully!';
            </script>
            """;

        // Enable script execution using the fluent method
        return new Html(htmlWithScript).DangerouslyAllowScripts();
    }
}
```

### HtmlPipeline - XDocument-Based Filters and Full Customization

The HTML pipeline has been refactored to use `XDocument` for safer, more structured HTML manipulation. Filters now work with parsed XML instead of raw strings, and new APIs allow full pipeline customization.

**Creating a Custom Filter:**

```csharp
using System.Xml.Linq;
using Ivy.Core.Server.HtmlPipeline;

public class OpenGraphFilter : IHtmlFilter
{
    public void Process(HtmlPipelineContext context, XDocument document)
    {
        var head = document.Root?.Element("head");
        if (head == null) return;

        head.Add(new XElement("meta",
            new XAttribute("property", "og:title"),
            new XAttribute("content", "My App")));

        head.Add(new XElement("meta",
            new XAttribute("property", "og:description"),
            new XAttribute("content", "Built with Ivy")));
    }
}

// Register the filter
var server = new Server()
    .UseHtmlFilter(new OpenGraphFilter());
```

**Access Services in Filters:**

```csharp
public class ServiceBasedFilter : IHtmlFilter
{
    public void Process(HtmlPipelineContext context, XDocument document)
    {
        var myService = context.Services.GetService<IMyService>();
        var head = document.Root?.Element("head");

        // Use service data to add elements
        head?.Add(new XElement("meta",
            new XAttribute("name", "custom"),
            new XAttribute("content", myService.GetValue())));
    }
}
```

**Full Pipeline Customization:**

Use `Server.UseHtmlPipeline()` to access the full pipeline, allowing you to clear, reorder, or replace filters entirely:

```csharp
// Replace the entire pipeline with custom filters
server.UseHtmlPipeline(pipeline =>
{
    pipeline.Clear();
    pipeline.Use<OpenGraphFilter>();
    pipeline.Use<CustomAnalyticsFilter>();
});

// Append to the default pipeline
server.UseHtmlPipeline(pipeline =>
{
    pipeline.Use<ExtraFilter>();
});

// Inspect registered filters
server.UseHtmlPipeline(pipeline =>
{
    foreach (var filter in pipeline.Filters)
    {
        Console.WriteLine(filter.GetType().Name);
    }
});
```

The pipeline configurator runs after all built-in and custom filters have been added, so `Clear()` removes everything for complete control.

### XamlBuilder: DataPoint Support for Charts

XamlBuilder now supports defining chart data inline using `<DataPoint>` elements, making it easier to work with charts without needing to create separate data classes.

**Basic Usage:**

```csharp
var xaml = """
    <LineChart>
        <LineChart.Data>
            <DataPoint Month="Jan" Value="100" />
            <DataPoint Month="Feb" Value="200" />
            <DataPoint Month="Mar" Value="150" />
        </LineChart.Data>
    </LineChart>
    """;

var chart = builder.Build(xaml);
```

**Complete Chart Example:**

```csharp
var xaml = """
    <LineChart ColorScheme="Default">
        <LineChart.Data>
            <DataPoint Month="Jan" Revenue="100" Costs="80" />
            <DataPoint Month="Feb" Revenue="120" Costs="90" />
            <DataPoint Month="Mar" Revenue="140" Costs="85" />
        </LineChart.Data>
        <LineChart.Lines>
            <Line DataKey="Revenue" />
            <Line DataKey="Costs" />
        </LineChart.Lines>
        <LineChart.XAxis>
            <XAxis DataKey="Month" />
        </LineChart.XAxis>
    </LineChart>
    """;

var chart = builder.Build(xaml);
```

### Field - Horizontal Label Layout with LabelPosition

The `Field` widget now supports horizontal label layouts where labels appear beside inputs instead of above them. This is particularly useful for data-dense admin panels, settings pages, and compact form layouts.

**New API:**

```csharp
public enum LabelPosition
{
    Top,   // Default - label above input
    Left   // Label beside input (horizontal layout)
}
```

**Basic Usage:**

```csharp
// Default - label on top
var emailField = new Field(
    new TextInput("Email"),
    label: "Email Address"
);

// Horizontal layout - label on left
var emailField = new Field(
    new TextInput("Email"),
    label: "Email Address"
).LabelPosition(LabelPosition.Left);
```

### Form Submit Strategies

Forms now support different submit strategies that control when form state is committed back to your model. This is separate from validation timing and gives you fine-grained control over form behavior.

**Available Strategies:**

- `OnSubmit` (default) — State is committed only when the submit button is clicked
- `OnBlur` — State is committed when any field loses focus (submit button hidden)
- `OnChange` — State is committed on every field value change (submit button hidden)

**Auto-Save Settings Example:**

```csharp
public class SettingsPanel : ViewBase
{
    public record Settings(string Name, string Theme, int FontSize);

    public override object? Build()
    {
        var settings = UseState(() => new Settings("Default", "Light", 14));
        var client = UseService<IClientProvider>();

        // React to changes and auto-save
        UseEffect(() =>
        {
            if (!string.IsNullOrEmpty(settings.Value.Name))
            {
                client.Toast($"Settings auto-saved: {settings.Value.Name}");
            }
        }, settings);

        return Layout.Vertical()
            | settings.ToForm()
                .SubmitStrategy(FormSubmitStrategy.OnChange)  // Auto-save on every change
                .Label(m => m.Name, "Display Name")
                .Label(m => m.Theme, "Theme")
                .Label(m => m.FontSize, "Font Size")
            | Text.Block($"Current: {settings.Value.Name}, {settings.Value.Theme}, {settings.Value.FontSize}px");
    }
}
```

Use `OnChange` for settings panels where changes should apply immediately, and `OnBlur` for forms where you want to commit after the user finishes editing each field.

### Auto-Scaffolding SelectInput from [AllowedValues]

Forms now automatically detect the `[AllowedValues]` attribute on `string` and `string[]` properties and scaffold them as `SelectInput` widgets (single or multi-select). This eliminates the need for manual `.Builder()` calls to configure dropdowns.

### NumberInput: Min/Max Parameters

`ToNumberInput()` now accepts optional `min` and `max` parameters, making it easier to set value constraints directly when creating number inputs.

**Basic Usage:**

```csharp
var price = UseState(0.0);

return price.ToNumberInput()
    .Min(0)
    .Max(10000)
    .Placeholder("Enter price")
    .WithField()
    .Label("Product Price");
```

### TextInput OnSubmit Event

TextInput now includes an `OnSubmit` event that fires when the user presses Enter in a single-line text input. This enables common interaction patterns like search boxes, quick-add fields, and login forms without requiring a Form wrapper.

**Basic Usage:**

```csharp
public class SearchBox : ViewBase
{
    public override object Build()
    {
        var searchQuery = UseState("");
        var searchResult = UseState("");

        return Layout.Horizontal()
            | searchQuery.ToSearchInput()
                .Placeholder("Search...")
                .HandleSubmit(() => searchResult.Set($"Searched for: {searchQuery.Value}"))
            | searchResult;
    }
}
```

### Fluent API Enhancements

Several widgets have received new fluent API extensions to make configuration more concise and chainable:

- **Toast API**: `.Success()`, `.Destructive()`, `.Warning()`, `.Info()`
- **ListItem**: `.Title()`, `.Subtitle()`, `.Icon()`, `.Badge()`, `.Tag()`, `.OnClick()`, `.Content()`, `.Disabled()`
- **FeedbackInput**: Dedicated fluent methods for each variant type.
- **Chart Builders**: `.Height()` and `.Width()` (replaces `Polish` callback workaround).
- **DesktopWindow**: `.UseDpiScaling()`, `.UseDevTools()`, `.Resizable()`, `.Center()`, `.TopMost()` (booleans default to `true`).
- **Table Progress**: `.Min()`, `.Max()`, `.AutoColor()`, `.Color()`, `.Format()`.
- **Separator**: `.TextAlign(TextAlignment.Left | Center | Right)`.
- **Global**: `.Grow()` is now available on all widgets (shorthand for `.Width(Size.Grow())`).

### ColorInput: Alpha Channel Support

The `ColorInput` widget now supports transparency with the new `AllowAlpha()` method. When enabled, an opacity slider appears next to the color picker, and colors are stored in `#RRGGBBAA` format (8-digit hex with alpha channel).

**Basic Usage:**

```csharp
public class ColorAlphaDemo : ViewBase
{
    public override object? Build()
    {
        var colorState = UseState("#ff000080"); // Red with 50% opacity

        return Layout.Vertical()
            | colorState.ToColorInput().AllowAlpha()
            | Text.P($"Selected: {colorState.Value}");
    }
}
```

### ColorInput - Ghost Styling for Minimal Appearance

`ColorInput` now supports ghost styling through the `Ghost()` extension method. Ghost styling removes borders, background fill, and shadows from the text input portion, creating a minimal appearance ideal for embedding color pickers in cards or colored backgrounds.

**Basic Usage:**

```csharp
var themeColor = UseState("#4A90E2");

// Normal color input with borders
return themeColor.ToColorInput();

// Ghost color input - minimal styling
return themeColor.ToColorInput().Ghost();
```

### SelectInput and AsyncSelectInput - Ghost Styling for Minimal Appearance

Both `SelectInput` and `AsyncSelectInput` now support ghost styling through the `Ghost()` extension method. Ghost styling removes borders, background fill, and shadows, creating a minimal appearance ideal for embedding select inputs in cards, toolbars, or colored backgrounds.

**Basic Usage:**

```csharp
// SelectInput with ghost styling
var colorState = UseState(Colors.Red);
var colorOptions = typeof(Colors).ToOptions();

return Layout.Vertical()
    | Text.Label("Theme Color")
    | colorState.ToSelectInput(colorOptions).Ghost();
```

**AsyncSelectInput with ghost styling:**

```csharp
// AsyncSelectInput with ghost styling
var categoryState = UseState(default(Guid?));

return categoryState.ToAsyncSelectInput(
    QueryCategories,
    LookupCategory,
    placeholder: "Select Category"
).Ghost();
```



### Card: Disabled State

The `Card` widget now supports a disabled state to prevent user interaction. When disabled, cards are visually dimmed and click events are suppressed, making it easy to indicate unavailable options or read-only states.

**Basic Usage:**

```csharp
new Card("This card cannot be clicked.")
    .Title("Disabled Card")
    .Description("User interaction is disabled.")
    .OnClick(_ => client.Toast("This won't fire!"))
    .Disabled()
    .Width(Size.Units(100));
```

**Conditional Disable:**

```csharp
public class ProductCard : ViewBase
{
    public override object? Build()
    {
        var isOutOfStock = UseState(true);

        return new Card()
            .Title("Premium Product")
            .Content("This product is currently unavailable")
            .OnClick(_ => AddToCart())
            .Disabled(isOutOfStock.Value);  // Disabled when out of stock
    }
}
```

**Visual Feedback:**

When disabled, cards automatically:

- Display reduced opacity (50%)
- Show a `not-allowed` cursor on hover
- Suppress all `OnClick` events
- Remove hover effects and animations

This follows the same pattern as `Button` and `ListItem` disabled states, providing a consistent API across interactive widgets.

---

### DetailsBuilder: Custom Field Labels

The `DetailsBuilder` now supports customizing field labels with the new `.Label()` method. By default, `ToDetails()` generates labels from property names using PascalCase splitting (e.g., `NetBurn` becomes "Net Burn"), but you can now override these auto-generated labels with custom text.

**Basic Usage:**

```csharp
public record RunwayData(decimal NetBurn, decimal GrossBurn, int Months, DateTime RunwayDate);

var data = new RunwayData(5000m, 10000m, 12, new DateTime(2027, 3, 1));
data.ToDetails()
    .Label(x => x.NetBurn, "Net Monthly Burn")
    .Label(x => x.RunwayDate, "Projected Runway End")
    .Build();
```

This provides more control over how field names are displayed to users, especially useful when property names don't naturally translate to readable labels or when you need specific terminology for your domain.

**Alternative Approach:**
For simple cases, you can use anonymous types where property names become the labels:

```csharp
new { NetBurn = "$5,000", GrossBurn = "$10,000" }.ToDetails()
```

**Note:** Use `.Builder(x => x.Field, b => ...)` to customize how a value is *rendered*, not to change the label text. The `.Label()` method is specifically for changing the displayed label.

---

### Ivy Studio (DevTools) - Visual Debugging and Interactive Development

Ivy Studio is a new integrated development tool that helps you debug and build UIs interactively within your running Ivy applications. It provides visual inspection, live editing, and widget callsite tracking for a faster development workflow.

**Key Features:**

**Widget Inspector:**

- Visually inspect widgets in your running application
- See widget hierarchy and properties
- Visual markers and overlays for proposed UI changes

**Widget Callsite Tracking (DEBUG builds):**

- Automatically captures where each widget is defined in your source code
- Maps UI elements back to code locations for easier debugging
- Exposed in DevTools overlay for quick navigation

**Text Edit Mode:**

- Live content overrides for `TextBlock` and `Markdown` widgets
- Edit text content in real-time without restarting your app
- Change request tracking for text content
- Inline editing with immediate visual feedback

**Enhanced UI:**

- Improved DevTools toolbar with better styling
- Widget overlay UX for easier interaction
- Speech-to-text change request interface

**Enabling DevTools:**

Use the `EnableDevTools` server argument to control visibility:

```csharp
var server = new Server(args)
    .EnableDevTools(isDevelopment);  // Enable in development only
```

Or enable it via desktop window:

```csharp
new DesktopWindow(server)
    .Title("My App")
    .UseDevTools(isDevelopment)
    .Run();
```

DevTools functionality is automatically detected in the frontend through meta tags. Note that callsite information is only captured in DEBUG builds to avoid performance overhead in production.

---

### Ivy.Widgets.ScreenshotFeedback - In-App Screenshot Annotation

The new `Ivy.Widgets.ScreenshotFeedback` package provides a powerful screenshot capture and annotation tool for collecting visual feedback directly within your Ivy applications. Users can capture the current window, annotate it with drawing tools, and upload the result.

**Installation:**

```bash
dotnet add package Ivy.Widgets.ScreenshotFeedback
```

**Basic Usage:**

```csharp
using Ivy;
using Ivy.Widgets.ScreenshotFeedback;

public class FeedbackDemo : ViewBase
{
    public override object? Build()
    {
        var screenshot = UseState<FileUpload<byte[]>?>();
        var uploadCtx = UseUpload(MemoryStreamUploadHandler.Create(screenshot));
        var isOpen = UseState(false);

        return Layout.Vertical().Gap(4)
            | new Button("Take Screenshot", () => isOpen.Set(true), icon: Icons.Camera)
            | new ScreenshotFeedback()
                .UploadUrl(uploadCtx.Value.UploadUrl)
                .Open(isOpen.Value)
                .HandleSave(() => isOpen.Set(false))
                .HandleCancel(() => isOpen.Set(false))
            | (screenshot.Value?.Status == FileUploadStatus.Finished
                ? new Image("data:image/png;base64," +
                    Convert.ToBase64String(screenshot.Value.Content))
                : Text.Muted("No screenshot captured yet."));
    }
}
```

**Features:**

- **Screenshot Capture**: Uses html2canvas to capture the current Ivy window
- **Annotation Tools**: Freehand drawing, lines, rectangles, circles, and text
- **Canvas-Based Overlay**: Intuitive drawing interface over the screenshot
- **Upload Integration**: Works with `UseUpload` and `MemoryStreamUploadHandler`
- **Modal Interface**: Opens in a modal for focused annotation workflow

**API:**

```csharp
new ScreenshotFeedback()
    .UploadUrl(url)              // Upload endpoint URL
    .Open(isOpen)                // Control visibility (boolean)
    .HandleSave(callback)        // Called when user saves
    .HandleCancel(callback);     // Called when user cancels
```

**Use Cases:**

- Bug reporting with visual annotations
- User feedback collection
- Visual documentation and issue tracking
- Design review and markup
- Support ticket screenshots

The widget handles all aspects of screenshot capture, annotation UI, and file upload, making it easy to add professional feedback collection to your application.

---

### Ivy.Widgets.Xterm - Terminal Emulator Widget

The new `Ivy.Widgets.Xterm` package provides a fully-featured terminal emulator widget built on xterm.js, enabling you to display and interact with terminal output directly in your Ivy web applications.

**Installation:**

```bash
dotnet add package Ivy.Widgets.Xterm
```

**Basic Usage:**

```csharp
using Ivy.Widgets.Xterm;

public class TerminalDemo : ViewBase
{
    public override object? Build()
    {
        var output = UseState("");

        return new Terminal()
            .Content(output.Value)
            .Width(Size.Percent(100))
            .Height(Size.Units(400));
    }
}
```

**Streaming Terminal Output:**

Combine with the `UseStream` hook for real-time output:

```csharp
var stream = UseStream<string>();

var terminal = new Terminal()
    .UseStream(stream)
    .Width(Size.Percent(100))
    .Height(Size.Units(400));

// In an async handler
await foreach (var line in ExecuteCommand())
{
    stream.Write(line);
}
```

**PTY Integration with Ivy.Hooks.Pty:**

For interactive shell sessions, use the new `Ivy.Hooks.Pty` hook:

```csharp
using Ivy.Hooks.Pty;

public class InteractiveShell : ViewBase
{
    public override object? Build()
    {
        var pty = UsePty("/bin/bash");  // Or "cmd.exe" on Windows

        return new Terminal()
            .UseStream(pty.Output)
            .OnInput(input => pty.Write(input))
            .Width(Size.Percent(100))
            .Height(Size.Percent(100));
    }
}
```

**Features:**

- **Unicode Support**: Full Unicode character rendering
- **Web Links**: Clickable URLs in terminal output
- **Clipboard Integration**: Copy/paste support
- **Theme-Aware**: Automatically adapts to your Ivy theme
- **Resizing**: Terminal resizes with widget dimensions
- **Base64 Encoding**: Preserves raw terminal data including control sequences
- **Streaming Support**: Progressive output rendering via `UseStream`

**Terminal Properties:**

```csharp
new Terminal()
    .Content("Hello, terminal!")           // Initial content
    .UseStream(streamInstance)              // Connect to stream
    .OnInput(data => HandleInput(data))    // Handle user input
    .Cols(80)                              // Column count
    .Rows(24)                              // Row count
    .Width(Size.Percent(100))              // Widget width
    .Height(Size.Units(400));              // Widget height
```

**Use Cases:**

- Command execution and output display
- Interactive shell sessions (SSH, local terminal)
- Build logs and CI/CD output
- Container logs and monitoring
- Interactive CLI tools within web apps
- Real-time log streaming

The widget handles all the complexity of terminal emulation, ANSI escape codes, and cursor positioning, providing a native terminal experience in the browser.

---

### UseStream - Server-to-Client Streaming

The new `UseStream<T>` hook enables server-to-client streaming, allowing you to progressively send data from the server to connected clients in real-time. This is perfect for LLM text streaming, long-running operations, real-time updates, and streaming content like AI responses or terminal output.

**API Reference:**

```csharp
/// <summary>
/// Creates a server-to-client stream that can push data to the frontend in real time.
/// </summary>
/// <param name="buffer">
/// When true (default), data written before the frontend subscribes is buffered
/// and automatically flushed once the subscription is established.
/// When false, data written before subscription is discarded.
/// </param>
public IWriteStream<T> UseStream<T>(bool buffer = true)
```

**Basic Usage:**

```csharp
public class StreamingDemo : ViewBase
{
    public override object? Build()
    {
        var stream = UseStream<string>();
        var client = UseService<IClientProvider>();

        return Layout.Vertical()
            | new Button("Start Streaming")
                .OnClick(async () => await StartStreaming(stream))
            | Text.Rich().UseStream(stream);
    }

    private async Task StartStreaming(IWriteStream<string> stream)
    {
        var words = "The quick brown fox jumps over the lazy dog".Split(' ');

        foreach (var word in words)
        {
            await Task.Delay(500);  // Simulate delay
            stream.Write(word + " ");
        }
    }
}
```

**Streaming with RichTextBlock:**

Perfect for streaming AI/LLM responses with styled text:

```csharp
var stream = UseStream<TextRun>();

var streamingText = Text.Rich()
    .Bold("🤖 AI: ")
    .UseStream(stream);

// Stream individual words or tokens
foreach (var token in aiResponse)
{
    stream.Write(new TextRun(token) { Word = true });
    await Task.Delay(50);
}
```

**Terminal Output Streaming:**

```csharp
var stream = UseStream<string>();

var terminal = new Terminal()
    .UseStream(stream);

// Stream command output line by line
await foreach (var line in ExecuteCommandAsync())
{
    stream.Write(line + "\n");
}
```

**Buffering Behavior:**

By default (`buffer = true`), if you start writing to the stream before the frontend component has fully rendered and subscribed via WebSockets, the data will be buffered in memory on the server. Once the client establishes the subscription, all buffered data will be flushed immediately. This ensures no data is lost even if you start streaming before the widget is ready.

If you don't care about missing early data or want to avoid memory overhead for large streams that might not be listened to, you can disable buffering:

```csharp
var stream = UseStream<byte[]>(buffer: false);
```

**How It Works:**

- Data is buffered on the server until a client subscribes (when `buffer = true`)
- Multiple clients can subscribe to the same stream
- Streams are automatically cleaned up when components unmount
- Type-safe streaming with full serialization support

**IWriteStream Interface:**

```csharp
public interface IWriteStream<T>
{
    void Write(T data);              // Write single item
    Task WriteAsync(T data);         // Async write
    void Complete();                 // Signal stream completion
}
```

**Supported Widgets with Streaming:**

- `RichTextBlock` via `.UseStream(stream)`
- `Terminal` via `.UseStream(stream)`
- Any custom widget that implements stream subscription

This feature works seamlessly with the serialization system, ensuring that complex objects (like `TextRun` with styling) are correctly formatted when received by clients.

---

### Dots Now Allowed in App IDs

App IDs can now include dots, enabling better namespacing and versioning patterns. Previously, app IDs like `app.v2` or `users.profile` were not allowed, but this restriction has been removed.

**New Capabilities:**

```csharp
// Version namespacing
[App(Id = "dashboard.v2")]
public class DashboardV2 : AppBase { }

// Feature namespacing
[App(Id = "users.profile")]
public class UserProfile : AppBase { }

// Domain-style naming
[App(Id = "com.mycompany.admin")]
public class AdminApp : AppBase { }

// Multi-level namespacing
[App(Id = "api.v1.users")]
public class ApiUsersV1 : AppBase { }
```

**Enhanced App ID Validation:**

The framework now provides comprehensive validation for app IDs with clear error messages:

- **Empty IDs**: "App ID is empty. Please provide a valid App ID."
- **Leading slash**: "App ID 'X' is invalid. App IDs should not start with '/'."
- **Unsafe characters**: "App ID 'X' is invalid. App IDs must be URL-friendly (alphanumeric, dashes, underscores, etc.)."
- **Reserved paths**: "App ID 'X' collides with a reserved path '/X'. Please choose a different App ID."
- **Static file extensions**: "App ID 'X' collides with a static file extension. Please choose a different App ID."

**What's Still Not Allowed:**

```csharp
// ❌ Static file extensions conflict
[App(Id = "app.js")]      // Conflicts with JavaScript files
[App(Id = "config.json")] // Conflicts with JSON files
[App(Id = "styles.css")]  // Conflicts with CSS files

// ❌ Unsafe characters
[App(Id = "app?query")]   // Contains query parameter syntax
[App(Id = "app:protocol")] // Contains protocol separator
[App(Id = "app#fragment")] // Contains URL fragment

// ❌ Reserved paths
[App(Id = "ivy")]         // Conflicts with /ivy system path
[App(Id = "api")]         // Conflicts with /api system path
```

Apps with invalid IDs will not be loaded, and clear error messages will be displayed during startup. This validation ensures your app routing remains predictable and conflict-free.

---

### Progress Builder for Table Cells

The Table widget now supports inline progress bars through the new `Progress()` builder, allowing you to render numeric values as visual progress indicators directly within table cells.

**Basic Usage:**

```csharp
var tasks = new[] {
    new { Name = "Design Review", Progress = 100 },
    new { Name = "Implementation", Progress = 75 },
    new { Name = "Testing", Progress = 45 },
    new { Name = "Documentation", Progress = 20 }
};

new Table(tasks)
    .Builder(t => t.Progress, f => f.Progress());
```

**Auto-Color Based on Thresholds:**

The progress bar automatically changes color based on completion percentage when using `.AutoColor()`:

- **Green** (Success): ≥75%
- **Yellow** (Warning): ≥50%
- **Orange**: ≥25%
- **Red** (Destructive): <25%

```csharp
new Table(tasks)
    .Builder(t => t.Progress, f => f.Progress().AutoColor());
```

**Custom Range and Format String:**

Configure custom min/max ranges and display the value alongside the progress bar:

```csharp
var downloads = new[] {
    new { File = "report.pdf", Downloaded = 750, Total = 1000 }
};

new Table(downloads)
    .Builder(d => d.Downloaded, f => f.Progress()
        .Min(0)
        .Max(1000)
        .AutoColor()
        .Format("%d bytes"));
```

**Explicit Colors:**

Set a specific color for all progress bars:

```csharp
new Table(tasks)
    .Builder(t => t.Progress, f => f.Progress()
        .Color(Colors.Blue)
        .Format("%d%"));
```

The Progress builder uses a fluent API, allowing you to chain configuration methods in any order. The percentage calculation automatically clamps values between 0-100%, ensuring consistent visual display even for out-of-range values.

---

### Text Alignment for Separator Labels

The `Separator` widget now supports text alignment control, allowing you to position labels at the left, center, or right of the separator line.

**Usage:**

```csharp
// Left-aligned label
new Separator("Left Aligned").TextAlign(TextAlignment.Left);

// Center-aligned label (default)
new Separator("Center Aligned").TextAlign(TextAlignment.Center);

// Right-aligned label
new Separator("Right Aligned").TextAlign(TextAlignment.Right);
```

**Example:**

```csharp
public override object? Build()
{
    return Layout.Vertical().Gap(4)
        | new Separator("Left Aligned").TextAlign(TextAlignment.Left)
        | new Separator("Center Aligned").TextAlign(TextAlignment.Center)
        | new Separator("Right Aligned").TextAlign(TextAlignment.Right);
}
```

The alignment property works seamlessly with all separator orientations and styling options, giving you precise control over label positioning in your layouts.

---

### Native Desktop Applications with Ivy.Desktop

Ivy apps can now run as native desktop applications across Windows, macOS, and Linux using the new `Ivy.Desktop` library. Built on Photino.NET, it provides a simple builder API for wrapping your Ivy web apps in native windows with automatic DPI detection and scaling.

**Getting Started:**

Add the `Ivy.Desktop` package to your project and use the `DesktopWindow` builder to configure and launch your app:

```csharp
using Ivy.Desktop;

var server = new Server(args);
var exitCode = new DesktopWindow(server)
    .Title("My Ivy App")
    .Size(1280, 800)
    .Run();

return exitCode;
```

**Configuration Options:**

The `DesktopWindow` builder supports fluent configuration for window properties:

```csharp
new DesktopWindow(server)
    .Title("My Application")
    .Size(1280, 800)
    .Resizable(true)
    .Center(true)
    .DpiScaling(true)  // Automatically adjusts for high-DPI displays
    .TopMost(false)
    .DevToolsEnabled(true)  // Enable for debugging
    .Run();
```

**Custom Window Icon:**

Set a custom icon from an embedded resource in your project:

```csharp
new DesktopWindow(server)
    .Title("My App")
    .Size(1280, 800)
    .Icon(typeof(Program), "MyApp.Resources.icon.ico")
    .Run();
```

**Cross-Platform DPI Support:**

The library includes automatic DPI detection and scaling for high-resolution displays on all platforms. The `DpiScaling(true)` option (enabled by default) automatically adjusts window dimensions for retina displays and high-DPI Windows monitors, ensuring your app looks sharp everywhere.

The native library files are automatically copied to your output directory via MSBuild targets, so no manual setup is required. Just add the package and start building native desktop apps with Ivy!

---

### Horizontal Label Layout for Field Widget

The `Field` widget now supports horizontal label positioning, enabling labels to appear beside inputs instead of above them. This is particularly useful for data-dense admin panels, settings pages, and compact form layouts.

**Basic Usage:**

```csharp
// Default vertical layout (label on top)
new Field(new TextInput("username"), "Username");

// Horizontal layout (label on left)
new Field(new TextInput("username"), "Username")
    .LabelPosition(LabelPosition.Left);
```

**In a Form:**

```csharp
new Column(
    new Field(new TextInput("name"), "Name")
        .LabelPosition(LabelPosition.Left),
    new Field(new TextInput("email"), "Email")
        .LabelPosition(LabelPosition.Left),
    new Field(new NumberInput("age"), "Age")
        .LabelPosition(LabelPosition.Left)
);
```

The `LabelPosition` enum has two values:

- `LabelPosition.Top` - Label appears above the input (default)
- `LabelPosition.Left` - Label appears to the left of the input

---

### Icons in Select Options

Select inputs now support optional icons for each option, making it easier to create visually rich select menus. Additionally, labels are now optional—if omitted, the option value will be displayed instead.

**What's New:**

- Added `icon` property to select options (supports any icon name from your icon library)
- Made `label` property optional (falls back to displaying the value)
- Icons are supported across all select variants: Toggle, Radio, Checkbox, and Dropdown

**Usage:**

```csharp
// Add icons to select options
var options = new List<SelectOption>
{
    new() { Value = "home", Label = "Home", Icon = "home" },
    new() { Value = "settings", Label = "Settings", Icon = "settings" },
    new() { Value = "profile", Label = "Profile", Icon = "user" }
};

var selected = UseState("home");
return selected.ToSelectInput()
    .Options(options)
    .Variant(SelectInputVariant.Dropdown);
```

**Icon-Only Options:**

You can omit labels entirely to create icon-only selects:

```csharp
// Icon-only toggle buttons
var theme = UseState("light");
return theme.ToSelectInput()
    .Options(new List<SelectOption>
    {
        new() { Value = "light", Icon = "sun" },
        new() { Value = "dark", Icon = "moon" }
    })
    .Variant(SelectInputVariant.Toggle);
```

Icons automatically scale with the density setting (Small, Medium, Large) and are positioned consistently across all select variants.

---



### Automatic Validation for Email, Password, Phone, and URL Inputs

Text inputs for common data types now include automatic validation that triggers on blur. New convenience methods like `ToEmailInput()`, `ToPasswordInput()`, `ToTelInput()`, and `ToUrlInput()` create validated inputs with appropriate input types and automatic validation.

**Basic Usage:**

```csharp
// Email input with automatic validation on blur
var email = UseState("");
return email.ToEmailInput()
    .Placeholder("e.g. user@example.com");

// Password input with minimum length validation (8 characters)
var password = UseState("");
return password.ToPasswordInput()
    .Placeholder("Min 8 characters");

// Phone number input with format validation
var phone = UseState("");
return phone.ToTelInput()
    .Placeholder("e.g. +1 234 567 8900");

// URL input with protocol validation (http/https)
var website = UseState("");
return website.ToUrlInput()
    .Placeholder("e.g. https://example.com");
```

**Using with Field Wrapper:**

Combine with `.WithField()` for complete form fields with labels, descriptions, and validation:

```csharp
var email = UseState("");
return email.ToEmailInput()
    .Placeholder("e.g. user@example.com")
    .WithField()
    .Label("Email Address")
    .Description("We'll use this for account recovery")
    .Required();
```

**Validation Triggers:**

Validation occurs automatically on blur (after the user has interacted with the field). Error messages are displayed below the input:

- **Email**: Validates proper email format (<username@domain.com> with valid domain)
- **Password**: Validates minimum length (8 characters by default)
- **Tel**: Validates phone number format (7-15 digits, allows spaces, dashes, parentheses)
- **URL**: Validates URL format and requires http:// or https:// protocol

**Using Variant Method:**

You can also apply validation using the `.Variant()` method on any `TextInput`:

```csharp
var email = UseState("");
return new TextInput(email)
    .Variant(TextInputVariant.Email)  // Automatically adds email validation
    .Placeholder("user@example.com");
```

**Form Integration:**

Form fields automatically get validation based on their type or naming conventions:

```csharp
public record UserModel(
    string Email,           // Automatically validated as email
    string Password,        // Automatically validated as password (min 8 chars)
    string? PhoneNumber,    // Automatically validated as phone number
    string? Website         // Automatically validated as URL
);

var model = UseState(() => new UserModel("", "", null, null));

var form = model.ToForm("Submit")
    .Builder(m => m.Email, s => s.ToEmailInput())
    .Builder(m => m.Password, s => s.ToPasswordInput())
    .Builder(m => m.PhoneNumber, s => s.ToTelInput())
    .Builder(m => m.Website, s => s.ToUrlInput());

return new Card(form);
```

**Validation Messages:**

Built-in validation provides user-friendly error messages:

- Email: "Please enter a valid email address"
- Password: "Password must be at least 8 characters"
- Tel: "Please enter a valid phone number"
- URL: "Please enter a valid URL (http or https)"

**Custom Validators:**

Built-in validation works alongside custom validators you define in forms. The framework runs both the variant-specific validation and any custom validators you provide.

This feature makes it much easier to build forms with proper validation without manually writing validator functions for common input types.

---

## Breaking Changes

### IHtmlFilter Interface - XDocument Instead of String Manipulation

The `IHtmlFilter.Process` method now takes an `XDocument` instead of a raw HTML string, and returns `void` instead of `string`. The namespace has also changed from `Ivy.Core.Server.ContentPipeline` to `Ivy.Core.Server.HtmlPipeline`. This provides safer, more structured HTML manipulation.

**Before (v1.2.17 and earlier):**

```csharp
using Ivy.Core.Server.ContentPipeline;

public class MyFilter : IHtmlFilter
{
    public string Process(HtmlPipelineContext context, string html)
    {
        html = html.Replace("</head>", "  <meta name=\"custom\" content=\"value\" />\n</head>");
        return html;
    }
}
```

**After (v1.2.18+):**

```csharp
using System.Xml.Linq;
using Ivy.Core.Server.HtmlPipeline;

public class MyFilter : IHtmlFilter
{
    public void Process(HtmlPipelineContext context, XDocument document)
    {
        var head = document.Root?.Element("head");
        head?.Add(new XElement("meta",
            new XAttribute("name", "custom"),
            new XAttribute("content", "value")));
    }
}
```

**Benefits:**

- Safer manipulation using structured XML instead of string replacement
- Single parse/serialize pass for better performance
- Eliminates potential HTML corruption from naive string replacement
- Access to LINQ to XML for powerful querying and manipulation

**Migration:**

1. Update `using` statements from `Ivy.Core.Server.ContentPipeline` to `Ivy.Core.Server.HtmlPipeline`
2. Add `using System.Xml.Linq;` to filter files
3. Change return type from `string` to `void`
4. Change second parameter from `string html` to `XDocument document`
5. Replace string manipulation with `XElement` operations on the document
6. Run `dotnet build` to verify

---

### IConnection.RegisterServices - Server Parameter Instead of IServiceCollection

The `IConnection.RegisterServices` method signature has changed to accept a `Server` instance instead of an `IServiceCollection`. This is a compile-time breaking change for all custom connection implementations.

**Before (v1.2.17 and earlier):**

```csharp
using Microsoft.Extensions.DependencyInjection;

public class MyConnection : IConnection
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IMyService, MyService>();
    }
}
```

**After (v1.2.18+):**

```csharp
using Ivy;

public class MyConnection : IConnection
{
    public void RegisterServices(Server server)
    {
        server.Services.AddScoped<IMyService, MyService>();

        // Or use Server-specific methods like:
        server.UseAuth();
    }
}
```

**Why this change:**
The new signature gives connection implementations access to the full `Server` API, not just the service collection. This enables connections to use server-level configuration methods like `UseAuth()`, `UseDatabase()`, and other framework features.

**Migration:**

1. Update the method signature from `RegisterServices(IServiceCollection services)` to `RegisterServices(Server server)`
2. Replace `services.` calls with `server.Services.`
3. Consider using server-level APIs (e.g., `server.UseAuth()`) instead of direct service registration where appropriate
4. Run `dotnet clean && dotnet build` to recompile

---

### WrapLayout Removed - Use StackLayout with Wrap Instead

The `WrapLayout` widget has been removed. Use `StackLayout` with the new `.Wrap()` method instead, which provides the same functionality with more flexibility and consistency.

**Before (v1.2.17 and earlier):**

```csharp
new WrapLayout([
    new Badge("React"),
    new Badge("Vue"),
    new Badge("Angular")
], gap: 4);
```

**After (v1.2.18+):**

```csharp
Layout.Horizontal()
    .Wrap()
    .Gap(4)
    | new Badge("React")
    | new Badge("Vue")
    | new Badge("Angular");

// Or using the constructor
new StackLayout([
    new Badge("React"),
    new Badge("Vue"),
    new Badge("Angular")
], wrap: true, gap: 4);
```

**Benefits:**

- Single layout system with consistent API
- Access to all `StackLayout` features (alignment, scroll, etc.)
- Support for both horizontal and vertical wrapping
- Independent row and column gap control

**Migration:** Replace `new WrapLayout(...)` with `Layout.Horizontal().Wrap()` or `new StackLayout(..., wrap: true)`. The `gap` parameter maps directly, and all other layout properties are now available.

---

### TextArea → Textarea API Standardization

The `ToTextAreaInput()` extension method has been renamed to `ToTextareaInput()` to align with HTML standards and framework conventions.

**Before (v1.2.16 and earlier):**

```csharp
var description = UseState("");
return description.ToTextAreaInput(placeholder: "Enter description...");
```

**After (v1.2.17+):**

```csharp
var description = UseState("");
return description.ToTextareaInput(placeholder: "Enter description...");
```

**Migration:** Search your codebase for `.ToTextAreaInput(` and replace with `.ToTextareaInput(`. Run `dotnet build` to find any remaining instances.

Alternatively, you can use the variant directly: `state.ToTextInput(variant: TextInputVariants.Textarea)`

---

### MultiLine → Multiline Property and Method Rename

The `MultiLine` property and method have been renamed to `Multiline` (lowercase 'l') across `Detail`, `TableCell`, `DetailsBuilder`, and `TableBuilder` for consistency with .NET naming conventions.

**Before (v1.2.16 and earlier):**

```csharp
// Detail widget
new Detail("Description", content, multiLine: true);

// DetailsBuilder
model.ToDetails()
    .MultiLine(e => e.Description, e => e.Notes);

// TableBuilder
records.ToTable()
    .MultiLine(e => e.Content);

// TableCell
new TableCell(content).MultiLine();
```

**After (v1.2.17+):**

```csharp
// Detail widget
new Detail("Description", content, multiline: true);

// DetailsBuilder
model.ToDetails()
    .Multiline(e => e.Description, e => e.Notes);

// TableBuilder
records.ToTable()
    .Multiline(e => e.Content);

// TableCell
new TableCell(content).Multiline();
```

**New Feature:** A `Multiline()` extension method has been added for `TextInputBase`:

```csharp
// New in v1.2.17
myTextInput.Multiline();       // Enable multiline
myTextInput.Multiline(true);   // Enable multiline
myTextInput.Multiline(false);  // Disable multiline
```

**Migration:** Search for `.MultiLine(` and replace with `.Multiline(`. Also replace constructor parameter `multiLine:` with `multiline:`. Run `dotnet build` to verify.

---

### Spacer Default Behavior Change

The `Spacer` widget now defaults to grow behavior (`flex-grow: 1`) without requiring explicit `.Width(Size.Grow())`. A bare `new Spacer()` will automatically fill available space in the parent layout's direction, matching the most common use case of pushing sibling elements apart.

**Before (v1.2.16 and earlier):**

```csharp
// Required explicit Width(Size.Grow()) to push elements apart
return Layout.Horizontal().Gap(4)
    | new Button("Left Button")
    | new Spacer().Width(Size.Grow())
    | new Button("Right Button");
```

**After (v1.2.17+):**

```csharp
// Spacer grows by default
return Layout.Horizontal().Gap(4)
    | new Button("Left Button")
    | new Spacer()
    | new Button("Right Button");
```

**Impact:** This change simplifies the common pattern of using spacers to push elements to opposite sides. Existing code with explicit `.Width(Size.Grow())` continues to work but is now redundant. If you were relying on `Spacer` taking minimal space by default (rare), you'll need to explicitly set `.Width(Size.Units(0))` or another fixed size.

**Migration:** Review your spacer usage. Most code will benefit from removing the now-redundant `.Width(Size.Grow())` calls, but verify that any spacers intended to take minimal space have explicit sizing.

---

### Button Icon API - Constructor Parameter Removed

The `Button` widget no longer accepts an `icon` constructor parameter. Use the fluent `.Icon()` method instead to add icons to buttons.

**Before (v1.2.14 and earlier):**

```csharp
// Icon via constructor parameter
new Button("Save", icon: Icons.Save)
new Button("Hover", icon: Icons.Info).WithTooltip("This is a tooltip")

// Icon-only button
new Button(null, icon: Icons.Settings).WithTooltip("Open settings")
```

**After (v1.2.15+):**

```csharp
// Icon via fluent method
new Button("Save").Icon(Icons.Save)
new Button("Hover").Icon(Icons.Info).WithTooltip("This is a tooltip")

// Icon-only button
new Button().Icon(Icons.Settings).WithTooltip("Open settings")
```

**Migration:** Search your codebase for `new Button(` with `icon:` parameter and convert to use `.Icon()` method. Icon-only buttons should use `new Button()` without a text parameter, then call `.Icon()`.

---

### Event Handler Naming Standardization

Several event handler methods have been renamed for consistency across the framework, moving from `Handle*` to `On*` naming pattern to better match .NET conventions.

**HandleRowAction → OnRowAction** (DataTable):

```csharp
// Before (v1.2.14 and earlier)
dataTable.HandleRowAction("edit", e => EditItem(e.Value))
    .HandleRowAction("delete", async e => await DeleteItem(e.Value))

// After (v1.2.15+)
dataTable.OnRowAction("edit", e => EditItem(e.Value))
    .OnRowAction("delete", async e => await DeleteItem(e.Value))
```

**HandleClick → OnClick** (Card):

```csharp
// Before (v1.2.14 and earlier)
new Card("Content").HandleClick(() => DoSomething())

// After (v1.2.15+)
new Card("Content").OnClick(() => DoSomething())
```

**HandleSelect → OnSelect** (Tree, MenuItem):

```csharp
// Before (v1.2.17 and earlier)
new Tree(items).HandleSelect(e => ProcessSelection(e))

MenuItem.Default("Edit").HandleSelect(() => EditItem())
MenuItem.Default("Delete").HandleSelect(item => DeleteItem(item))

// After (v1.2.18+)
new Tree(items).OnSelect(e => ProcessSelection(e))

MenuItem.Default("Edit").OnSelect(() => EditItem())
MenuItem.Default("Delete").OnSelect(item => DeleteItem(item))
```

**HandleSubmit → OnSubmit** (TextInput):

```csharp
// Before (v1.2.17 and earlier)
searchQuery.ToSearchInput()
    .Placeholder("Search...")
    .HandleSubmit(() => PerformSearch())

tag.ToTextInput()
    .Placeholder("Add a tag...")
    .HandleSubmit(() => AddTag(tag.Value))

// After (v1.2.18+)
searchQuery.ToSearchInput()
    .Placeholder("Search...")
    .OnSubmit(() => PerformSearch())

tag.ToTextInput()
    .Placeholder("Add a tag...")
    .OnSubmit(() => AddTag(tag.Value))
```

**HandleMove → OnMove** (Kanban):

```csharp
// Before (v1.2.17 and earlier)
tasks.ToKanban(
    columnSelector: t => t.Status,
    orderSelector: t => t.Priority)
    .CardBuilder(task => new Card(task.Title, task.Description))
    .HandleMove(moveData => {
        UpdateTaskStatus(moveData.CardId, moveData.ToColumn);
    })

// After (v1.2.18+)
tasks.ToKanban(
    columnSelector: t => t.Status,
    orderSelector: t => t.Priority)
    .CardBuilder(task => new Card(task.Title, task.Description))
    .OnMove(moveData => {
        UpdateTaskStatus(moveData.CardId, moveData.ToColumn);
    })
```

**HandleCellAction → OnCellAction** (DataTable):

```csharp
// Before (v1.2.17 and earlier)
records.ToTable()
    .Column(e => e.Id)
    .Column(e => e.Name)
    .HandleCellAction(e => e.Name, value => OpenDetails(value))

// After (v1.2.18+)
records.ToTable()
    .Column(e => e.Id)
    .Column(e => e.Name)
    .OnCellAction(e => e.Name, value => OpenDetails(value))
```

**Migration:** Search for `.HandleRowAction(`, `.HandleClick(`, `.HandleSelect(`, `.HandleSubmit(`, `.HandleMove(`, and `.HandleCellAction(` and replace with `.OnRowAction(`, `.OnClick(`, `.OnSelect(`, `.OnSubmit(`, `.OnMove(`, and `.OnCellAction(` respectively. Run `dotnet build` to find any remaining instances.

---

### OAuth Callback URL Path Change

The OAuth authentication callback URL has changed from `/ivy/webhook` to `/ivy/auth/callback` to better reflect its purpose and avoid confusion with actual webhook endpoints.

**Before (v1.2.14 and earlier):**

- Local development: `http://localhost:5010/ivy/webhook`
- Production: `https://your-app.com/ivy/webhook`

**After (v1.2.15+):**

- Local development: `http://localhost:5010/ivy/auth/callback`
- Production: `https://your-app.com/ivy/auth/callback`

**Migration:**

1. Update your OAuth provider settings (GitHub, Google, etc.) to use the new callback URL
2. For GitHub OAuth Apps: Navigate to Settings → Developer settings → OAuth Apps → Your App → Update "Authorization callback URL"
3. No code changes required - the framework handles this automatically
4. Both old and new URLs may work temporarily during migration, but update to the new URL to ensure future compatibility

**Affected authentication providers:**

- GitHub OAuth
- Google OAuth
- Microsoft OAuth
- Any custom OAuth providers

---

### DesktopWindow API Improvements

The `DesktopWindow` fluent API has been improved with better naming consistency and simpler default parameters. Two methods have been renamed to follow the `Use*` pattern:

**DpiScaling → UseDpiScaling and DevToolsEnabled → UseDevTools:**

```csharp
// Before (v1.2.16 and earlier)
new DesktopWindow(server)
    .Title("My App")
    .Size(1280, 720)
    .DpiScaling(true)
    .DevToolsEnabled(true)
    .Run();

// After (v1.2.17+)
new DesktopWindow(server)
    .Title("My App")
    .Size(1280, 720)
    .UseDpiScaling(true)
    .UseDevTools(true)
    .Run();
```

**New Default Parameters:** All boolean fluent methods now default to `true`, allowing for simpler method calls:

```csharp
// Simplified calls (v1.2.17+)
new DesktopWindow(server)
    .Title("My App")
    .Size(1280, 720)
    .Resizable()        // Same as .Resizable(true)
    .Center()           // Same as .Center(true)
    .TopMost()          // Same as .TopMost(true)
    .UseDpiScaling()    // Same as .UseDpiScaling(true)
    .UseDevTools()      // Same as .UseDevTools(true)
    .Run();
```

**Migration:** Search your codebase for `.DpiScaling(` and replace with `.UseDpiScaling(`. Search for `.DevToolsEnabled(` and replace with `.UseDevTools(`. Run `dotnet build` to verify.

---

### Chart Data Syntax - XML DataPoint Replaced by JSON

Chart data in XAML now uses JSON arrays inside CDATA sections instead of `<DataPoint>` XML elements. This provides native type preservation for strings, numbers, and booleans, eliminating ambiguity from automatic type parsing.

**Before (v1.2.17 and earlier):**

```csharp
var xml = """
    <LineChart>
        <LineChart.Data>
            <DataPoint Month="Jan" Revenue="100" Costs="80" />
            <DataPoint Month="Feb" Revenue="120" Costs="90" />
        </LineChart.Data>
        <LineChart.Lines>
            <Line DataKey="Revenue" />
            <Line DataKey="Costs" />
        </LineChart.Lines>
    </LineChart>
    """;
var chart = builder.Build(xml);
```

**After (v1.2.18+):**

```csharp
var xml = """
    <LineChart>
        <Data><![CDATA[
            [
                {"Month": "Jan", "Revenue": 100, "Costs": 80},
                {"Month": "Feb", "Revenue": 120, "Costs": 90}
            ]
        ]]></Data>
        <LineChart.Lines>
            <Line DataKey="Revenue" />
            <Line DataKey="Costs" />
        </LineChart.Lines>
    </LineChart>
    """;
var chart = builder.Build(xml);
```

**Type Preservation Benefits:**

The new JSON format preserves exact types, which is especially important for:

```csharp
// Strings: Leading zeros preserved
<Data><![CDATA[
    [{"Id": "007", "Code": "00123"}]
]]></Data>

// Booleans: True boolean values, not strings
<Data><![CDATA[
    [{"Active": true, "Visible": false}]
]]></Data>

// Numbers: Explicit numeric types
<Data><![CDATA[
    [{"Count": 42, "Price": 19.99}]
]]></Data>
```

**Migration:**

1. Replace `<ChartType.Data>` wrapper with `<Data>`
2. Convert `<DataPoint>` elements to JSON array objects inside `<![CDATA[...]]>`
3. Use JSON syntax: string values in quotes, numbers without quotes, booleans as `true`/`false`
4. Ensure valid JSON formatting (commas between properties and array items)

**Works with all chart types:** `LineChart`, `BarChart`, `AreaChart`, `PieChart`, and custom chart implementations.

---

### CreateSignal Renamed to UseSignal and ISignal Unified

The `CreateSignal<T, TInput, TOutput>()` method has been removed and replaced by `UseSignal<T, TInput, TOutput>()`. Additionally, the separate `ISignalSender` and `ISignalReceiver` interfaces have been unified into a single `ISignal` interface that provides both sending and receiving capabilities.

**Before (v1.2.17 and earlier):**

```csharp
// Create signal sender
ISignalSender<string, bool> mySender = context.CreateSignal<MySignal, string, bool>();

// Get signal receiver
ISignalReceiver<string, bool> myReceiver = context.UseSignal<MySignal, string, bool>();
```

**After (v1.2.18+):**

```csharp
// Both sending and receiving are now handled by ISignal
ISignal<string, bool> mySignal = context.UseSignal<MySignal, string, bool>();

// The same instance provides both methods:
mySignal.Send(input);           // Send data through the signal
mySignal.Receive(callback);     // Register a callback to receive data
```

**Why this change:**
The previous separation between sender and receiver interfaces was unnecessarily complex for most use cases. The unified `ISignal` interface simplifies the API while maintaining full functionality. Components that need to both send and receive no longer need two separate references.

**Migration:**

1. Replace all calls to `context.CreateSignal<...>()` with `context.UseSignal<...>()`
2. Change variable types from `ISignalSender<TInput, TOutput>` or `ISignalReceiver<TInput, TOutput>` to `ISignal<TInput, TOutput>`
3. The `.Send()` and `.Receive()` methods work identically, so no changes to usage code are required
4. Run `dotnet build` to verify all references are updated

---

## New Features

### RichTextBlock - Styled Text with Links and Streaming

A new `RichTextBlock` widget enables rich text formatting with support for text styling, hyperlinks, and real-time streaming content.

**Text Styling Options:**

- **Bold**, *italic*, and ~~strikethrough~~ text
- Custom text and highlight colors
- Word-by-word spacing control

**Hyperlinks:**

```csharp
// Create a RichTextBlock with styled text and links
var richText = new RichTextBlock
{
    Runs = new List<TextRun>
    {
        new() { Content = "Visit our ", Word = true },
        new()
        {
            Content = "documentation",
            Link = "https://docs.example.com",
            LinkTarget = LinkTarget.Blank,  // Opens in new tab
            Word = true
        },
        new() { Content = " for more info.", Word = true }
    }
};
```

**Text Styling:**

```csharp
var richText = new RichTextBlock
{
    Runs = new List<TextRun>
    {
        new() { Content = "Bold text", Bold = true, Word = true },
        new() { Content = "Italic text", Italic = true, Word = true },
        new() { Content = "Colored text", Color = "Red", Word = true },
        new()
        {
            Content = "Highlighted text",
            HighlightColor = "Yellow",
            Word = true
        }
    }
};
```

**Link Click Events:**
Handle link clicks programmatically instead of navigating:

```csharp
var richText = new RichTextBlock
{
    Runs = new List<TextRun>
    {
        new()
        {
            Content = "Click me",
            Link = "/action",
            Word = true
        }
    },
    OnLinkClick = (url) =>
    {
        // Handle the link click
        Console.WriteLine($"User clicked: {url}");
    }
};
```

**Streaming Support:**
Stream text runs in real-time for dynamic content, perfect for LLM responses or real-time updates:

Using `UseStream` with the builder API:

```csharp
var stream = Context.UseStream<TextRun>();

var streamingText = Text.Rich()
    .Bold("🤖 AI: ")
    .UseStream(stream);

// In an async handler (e.g., button click)
var words = "The meaning of life is 42.".Split(' ');
foreach (var word in words)
{
    await Task.Delay(100);  // Simulate LLM token delay
    stream.Write(new TextRun(word) { Word = true });
}
```

Using `Stream` property with stream ID:

```csharp
var streamId = "chat-response";
var richText = new RichTextBlock
{
    Stream = streamId,
    Runs = new List<TextRun>
    {
        new() { Content = "AI Response: ", Bold = true }
    }
};

// Stream additional runs as they become available
await Stream(streamId, new TextRun { Content = "Hello ", Word = true });
await Stream(streamId, new TextRun { Content = "world!", Word = true });
```

**Additional Properties:**

- `TextAlignment` - Control text alignment (Left, Center, Right, Justify)
- `NoWrap` - Prevent text wrapping
- `Overflow` - Control overflow behavior
- `Scale` - Set text size (Small, Large)

---

### ReadOnlyInput - Copy Button and Placeholder Support

The `ReadOnlyInput` widget now supports two new fluent extension methods for enhanced functionality:

**ShowCopyButton()** - Control visibility of the copy button:

```csharp
var apiKey = UseState("sk-1234567890abcdef");

// Show copy button (default)
return apiKey.ToReadOnlyInput()
    .ShowCopyButton();

// Hide copy button
return apiKey.ToReadOnlyInput()
    .ShowCopyButton(false);
```

**Placeholder()** - Set placeholder text for empty read-only inputs:

```csharp
var result = UseState("");

return result.ToReadOnlyInput()
    .Placeholder("No data available");
```

These methods can be chained together:

```csharp
var token = UseState("");

return token.ToReadOnlyInput()
    .Placeholder("Token will appear here")
    .ShowCopyButton();
```

---

### BoolInput - Loading State Support

The `BoolInput` widget now supports a loading state across all variants (Checkbox, Switch, and Toggle). When in loading state, the widget displays a spinner overlay and is automatically disabled to prevent user interaction during async operations.

**Basic usage:**

```csharp
var isEnabled = UseState(true);
var isLoading = UseState(true);

return isEnabled.ToSwitchInput()
    .Label("Enable Feature")
    .Loading(isLoading.Value);
```

**Works with all variants:**

```csharp
// Checkbox variant
var checkboxValue = UseState(false);
return checkboxValue.ToBoolInput()
    .Label("Accept Terms")
    .Loading(isProcessing.Value);

// Switch variant
var switchValue = UseState(true);
return switchValue.ToSwitchInput()
    .Label("Notifications")
    .Loading(isUpdating.Value);

// Toggle variant
var toggleValue = UseState(false);
return toggleValue.ToToggleInput(Icons.Bell)
    .Label("Alerts")
    .Loading(isSaving.Value);
```

The loading state can be combined with other input states like descriptions and validation:

```csharp
var value = UseState(false);
var isLoading = UseState(true);

return value.ToSwitchInput()
    .Label("Enable API Access")
    .Description("Toggle API access for your application")
    .Loading(isLoading.Value);
```

---

### TextInput - OnSubmit Event for Enter Key Handling

The `TextInput` widget now supports an `OnSubmit` event that fires when the user presses Enter in a single-line text input. This enables common interaction patterns like search boxes, quick-add fields, and login forms without requiring a Form wrapper.

**Basic usage:**

```csharp
var searchQuery = UseState("");

return searchQuery.ToTextInput()
    .Placeholder("Search...")
    .HandleSubmit(() => PerformSearch(searchQuery.Value));
```

**Multiple overloads available:**

```csharp
// Simple Action
textInput.HandleSubmit(() => DoSomething());

// Action with Event<IAnyInput> parameter
textInput.HandleSubmit(e => ProcessInput(e));

// Async ValueTask
textInput.HandleSubmit(async () => await SaveAsync());

// Async with Event<IAnyInput>
textInput.HandleSubmit(async e => await ProcessAsync(e));
```

**Practical example - Search input:**

```csharp
var searchQuery = UseState("");
var searchResult = UseState("");

return Layout.Horizontal()
    | searchQuery.ToSearchInput()
        .Placeholder("Search...")
        .HandleSubmit(() => searchResult.Set($"Searched for: {searchQuery.Value}"))
    | searchResult;
```

**Quick-add pattern example:**

```csharp
var tag = UseState("");
var tags = UseState<List<string>>(new List<string>());

return Layout.Horizontal()
    | tag.ToTextInput()
        .Placeholder("Add a tag...")
        .HandleSubmit(() => {
            if (!string.IsNullOrWhiteSpace(tag.Value))
            {
                tags.Set(new List<string>(tags.Value) { tag.Value });
                tag.Set("");  // Clear input after adding
            }
        })
    | Layout.Horizontal().Gap(2) | tags.Value.Select(t => new Badge(t));
```

**Important notes:**

- Only fires for single-line text inputs (not Textarea variant)
- In Textarea, Enter inserts a newline as expected
- Works with all TextInput variants: Default, Password, and Search
- The input automatically blurs after the event fires

---

### NumberInput - Prefix and Suffix Support

The `NumberInput` widget now supports `Prefix` and `Suffix` properties, allowing you to display contextual visual cues inside the input field such as currency symbols, unit labels, or icons. This matches the existing pattern on `TextInput` and makes it easier to build forms with clear input context.

**Basic usage with text affixes:**

```csharp
var price = UseState(99.99m);

return price.ToNumberInput()
    .Prefix("$")
    .Precision(2);
```

**Using icons and units:**

```csharp
var weight = UseState(5.5);
var temperature = UseState(22);

return Layout.Vertical()
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

**Complete example:**

```csharp
public class NumberPrefixSuffixDemo : ViewBase
{
    public override object? Build()
    {
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
    }
}
```

**Extension methods:**

Both `Prefix()` and `Suffix()` accept either a `string` or an `Icons` value:

```csharp
// String prefix/suffix
numberInput.Prefix("$")
numberInput.Suffix("kg")

// Icon prefix/suffix
numberInput.Prefix(Icons.Thermometer)
numberInput.Suffix(Icons.Calendar)
```

**Visual styling:**

Prefix and suffix elements appear with a muted background and border separator, clearly distinguishing them from the editable input area while maintaining visual cohesion with the input field.

---

### DateTimeInput - Month, Week, and Year Pickers

The `DateTimeInput` widget now supports three additional variants for selecting time periods: Month, Week, and Year. These variants are perfect for reporting periods, fiscal years, project planning, and any scenario where you need to select a specific time granularity beyond dates and times.

**New Variants:**

- **Month** - Month picker with year navigation; selects the 1st of the chosen month
- **Week** - Calendar with week numbers; selects the Monday of the chosen week
- **Year** - Year picker with decade navigation; selects January 1st of the chosen year

**Convenient Extension Methods:**

```csharp
// Month input
var billingPeriod = UseState(DateTime.Today);
return billingPeriod.ToMonthInput()
    .Placeholder("Select billing month")
    .WithField()
    .Label("Billing Period");

// Week input
var projectWeek = UseState(DateTime.Today);
return projectWeek.ToWeekInput()
    .Placeholder("Select project week")
    .WithField()
    .Label("Project Week");

// Year input
var fiscalYear = UseState(DateTime.Today);
return fiscalYear.ToYearInput()
    .Placeholder("Select fiscal year")
    .WithField()
    .Label("Fiscal Year");
```

**All Variants Together:**

```csharp
public class DateTimeVariantsDemo : ViewBase
{
    public override object? Build()
    {
        var dateState = UseState(DateTime.Today.Date);
        var timeState = UseState(DateTime.Now);
        var dateTimeState = UseState(DateTime.Today);
        var monthState = UseState(DateTime.Today);
        var weekState = UseState(DateTime.Today);
        var yearState = UseState(DateTime.Today);

        return Layout.Vertical()
            | dateState.ToDateInput()
                .WithField()
                .Label("Date")
            | dateTimeState.ToDateTimeInput()
                .WithField()
                .Label("DateTime")
            | timeState.ToTimeInput()
                .WithField()
                .Label("Time")
            | monthState.ToMonthInput()
                .WithField()
                .Label("Month")
            | weekState.ToWeekInput()
                .WithField()
                .Label("Week")
            | yearState.ToYearInput()
                .WithField()
                .Label("Year");
    }
}
```

**Use Cases:**

- Monthly billing periods and subscriptions
- Weekly sprint planning and project timelines
- Fiscal year selection for financial reports
- Annual performance reviews and planning cycles
- Historical data filtering by time period

All variants support the same features as the existing DateTimeInput: nullable values, validation, disabled state, placeholder text, and all standard sizing options (Small, Medium, Large).

---

### Badge - Clickable Badges with OnClick Event

The `Badge` widget now supports click events through the `OnClick` extension methods, enabling interactive badges for common UI patterns like filter chips, tag management, and toggle states.

**Basic usage:**

```csharp
new Badge("Click Me", icon: Icons.MousePointer)
    .OnClick(_ => client.Toast("Badge clicked!"));
```

**Multiple overloads available:**

```csharp
// Action with Event<Badge> parameter
new Badge("Filter")
    .OnClick(e => client.Toast("Badge clicked!"));

// Simple Action
new Badge("Remove", variant: BadgeVariant.Destructive)
    .OnClick(() => RemoveItem());

// Async ValueTask
new Badge("Save")
    .OnClick(async () => await SaveAsync());

// Async with Event<Badge>
new Badge("Process")
    .OnClick(async e => await ProcessAsync());
```

**Practical example - Filter badges:**

```csharp
var activeFilters = UseState<List<string>>(new());

return activeFilters.Value
    .Select(filter =>
        new Badge(filter, icon: Icons.X, variant: BadgeVariant.Secondary)
            .OnClick(() => {
                var updated = activeFilters.Value.Where(f => f != filter).ToList();
                activeFilters.Set(updated);
            })
    )
    .ToArray();
```

---

### Box - Interactive Regions with OnClick and Hover Effects

The `Box` widget now supports click events and hover effects, enabling you to create interactive UI regions without using the heavier `Card` widget. This follows the same pattern as `Card`, making it easier to build consistent interactive experiences.

**OnClick Event Handler**

Add click functionality to any Box with multiple overload options:

```csharp
var client = UseService<IClientProvider>();

// Simple Action
new Box("Click Me")
    .OnClick(() => client.Toast("Box clicked!"));

// Action with Event<Box> parameter
new Box("Clickable Region")
    .OnClick(e => client.Toast("Box clicked!"));

// Async ValueTask
new Box("Save")
    .OnClick(async () => await SaveAsync());

// Async with Event<Box>
new Box("Process")
    .OnClick(async e => await ProcessAsync());
```

**Hover Effects**

Control hover behavior using the `Hover()` extension method with `CardHoverVariant`:

```csharp
// Pointer cursor only
new Box("Hover Me")
    .Hover(CardHoverVariant.Pointer)
    .OnClick(() => DoSomething());

// Pointer + translate animation
new Box("Interactive Card")
    .Hover(CardHoverVariant.PointerAndTranslate)
    .OnClick(() => DoSomething());

// No hover effect (default)
new Box("Static Box")
    .Hover(CardHoverVariant.None);
```

**Practical Example - Interactive Selection**

```csharp
var selected = UseState("Option A");
var client = UseService<IClientProvider>();

return Layout.Horizontal().Gap(4)
    | CreateOption("Option A", selected, client)
    | CreateOption("Option B", selected, client)
    | CreateOption("Option C", selected, client);

Box CreateOption(string label, IState<string> selected, IClientProvider client)
{
    var isSelected = selected.Value == label;
    return new Box(label)
        .Color(isSelected ? Colors.Primary : Colors.Muted)
        .BorderThickness(isSelected ? 2 : 1)
        .Hover(CardHoverVariant.Pointer)
        .OnClick(() => {
            selected.Set(label);
            client.Toast($"Selected: {label}");
        })
        .Padding(8)
        .Width(Size.Fraction(1/3f));
}
```

**Note:** When using `OnClick()` without specifying a hover variant, the Box automatically applies `CardHoverVariant.PointerAndTranslate` to provide visual feedback.

**Grow Extension Method**

The `Box` widget now includes a convenient `Grow()` extension method for making boxes expand to fill available width. This is a shorthand for `.Width(Size.Grow())`.

```csharp
// Before: using Size.Grow() explicitly
new Box("Content").Width(Size.Grow());

// After: using the convenient Grow() method
new Box("Content").Grow();

// Practical example - flexible layout
Layout.Horizontal().Gap(4)
    | new Box("Fixed width").Width(Size.Units(50))
    | new Box("Grows to fill space").Grow()
    | new Box("Another fixed").Width(Size.Units(50));
```

This makes your code more concise and expressive when creating flexible layouts where certain boxes should expand to fill available space.

---

### Callout - Closable Callouts with OnClose Event

The `Callout` widget now supports closable behavior through the `OnClose` event handler. When an `OnClose` handler is set, the callout displays a close (X) button in the top-right corner, enabling users to dismiss notifications, banners, and temporary alerts.

**Basic usage with UseTrigger:**

```csharp
var (calloutView, showCallout) = UseTrigger((IState<bool> isOpen) =>
    isOpen.Value
        ? Callout.Info("A new version is available. Refresh to update.", "Update Available")
            .OnClose(() => isOpen.Set(false))
        : null);

return Layout.Vertical().Gap(6)
    | new Button("Show callout", onClick: _ => showCallout())
    | calloutView;
```

**Using with UseState:**

```csharp
var showUpdateBanner = UseState(true);
var showTrialBanner = UseState(true);

return Layout.Vertical().Gap(6)
    | (showUpdateBanner.Value
        ? Callout.Info("A new version is available. Refresh to update.", "Update Available")
            .OnClose(() => showUpdateBanner.Set(false))
        : null)
    | (showTrialBanner.Value
        ? Callout.Warning("Your trial expires in 3 days.")
            .OnClose(() => showTrialBanner.Set(false))
        : null);
```

**Use cases:**

- Dismissible notification banners
- Temporary alerts and warnings
- User-controlled information messages
- Feature announcements that can be closed
- Session-based notices

The `OnClose()` extension method provides a convenient way to handle simple dismissal logic, while the `OnClose` property supports full `EventHandler<Event<Callout>>` handlers for more complex scenarios.

---

### Card - Disabled State for Preventing Interaction

The `Card` widget now supports a `Disabled` property and extension method to prevent user interaction. When disabled, the card will not trigger `OnClick` events and displays visual feedback (reduced opacity, no hover effects) to indicate its unavailable state.

**Basic usage:**

```csharp
new Card("This card cannot be clicked.")
    .Title("Disabled Card")
    .Description("User interaction is disabled.")
    .OnClick(_ => client.Toast("This won't fire!"))
    .Disabled()
    .Width(Size.Units(100));
```

**Control disabled state dynamically:**

```csharp
var isProcessing = UseState(false);

return new Card("Click to process")
    .Title("Processing Card")
    .OnClick(async _ => {
        isProcessing.Set(true);
        await ProcessDataAsync();
        isProcessing.Set(false);
    })
    .Disabled(isProcessing.Value);  // Disable while processing
```

**Use cases:**

- Indicate unavailable options (e.g., locked features, insufficient permissions)
- Show read-only states in dashboards
- Prevent interaction during async operations
- Display inactive or archived items

This follows the same pattern as the existing `Button` and `Expandable` widgets, providing a consistent API across interactive components.

---

### FileInput - Minimum File Size Validation

The `FileInput` widget now supports minimum file size validation through the `MinFileSize` property and extension method, allowing you to reject empty or trivially small files that are likely erroneous. This complements the existing `MaxFileSize` validation.

**Basic usage with UploadContext:**

```csharp
var file = UseState<FileUpload<byte[]>?>();
var upload = UseUpload(MemoryStreamUploadHandler.Create(file))
    .MinFileSize(1024)  // Minimum 1 KB
    .MaxFileSize(FileSize.FromMegabytes(10));

return file.ToFileInput(upload)
    .Placeholder("Min 1 KB, Max 10 MB");
```

**Using FileSize helpers for clarity:**

```csharp
var upload = UseUpload(handler)
    .MinFileSize(FileSize.FromKilobytes(1))   // Minimum 1 KB
    .MaxFileSize(FileSize.FromMegabytes(5));  // Maximum 5 MB
```

**Direct widget configuration:**

```csharp
var file = UseState<FileUpload<byte[]>?>();

return file.ToFileInput()
    .MinFileSize(1024)  // 1 KB minimum
    .MaxFileSize(5 * 1024 * 1024)  // 5 MB maximum
    .Placeholder("Between 1 KB and 5 MB");
```

**Complete example with file size range:**

```csharp
public class FileSizeRangeDemo : ViewBase
{
    public override object? Build()
    {
        var file = UseState<FileUpload<byte[]>?>();
        var upload = UseUpload(MemoryStreamUploadHandler.Create(file))
            .MinFileSize(FileSize.FromKilobytes(1))    // Reject files < 1 KB
            .MaxFileSize(FileSize.FromMegabytes(10));  // Reject files > 10 MB

        return Layout.Vertical()
            | Text.H2("Upload File (1 KB - 10 MB)")
            | file.ToFileInput(upload)
                .Placeholder("Select a file between 1 KB and 10 MB")
            | (file.Value != null
                ? Text.P($"Selected: {file.Value.FileName} ({Utils.FormatBytes(file.Value.Length)})")
                : null);
    }
}
```

**Validation behavior:**

- Client-side validation provides immediate feedback before upload
- Server-side validation ensures security even if client validation is bypassed
- Error messages automatically format file sizes (e.g., "File is too small (512 B). Minimum required size is 1.00 KB.")
- Works with both single and multiple file uploads

---

### CodeBlock - Starting Line Number for Code Excerpts

The `CodeBlock` widget now supports `StartingLineNumber` to offset line numbering when displaying code excerpts. This is useful when you want to show a snippet from a larger file while preserving the original line numbers from the source.

**Basic usage:**

```csharp
new CodeBlock(@"    private static int Calculate(int input)
    {
        return input * 2 + 1;
    }
}")
    .ShowLineNumbers()
    .StartingLineNumber(18)  // Line numbering starts at 18
    .Language(Languages.Csharp);
```

**Practical example - showing error context:**

```csharp
// Display code around an error on line 42
var errorContext = GetCodeLinesAround(errorLine: 42);

return Layout.Vertical()
    | Text.H3($"Error at line {errorLine}")
    | new CodeBlock(errorContext)
        .ShowLineNumbers()
        .StartingLineNumber(40)  // Show lines 40-44
        .Language(Languages.Csharp);
```

**Default behavior:**

```csharp
// Without StartingLineNumber, numbering starts at 1 (default)
new CodeBlock(code)
    .ShowLineNumbers()
    .Language(Languages.Javascript);
```

The `StartingLineNumber` property defaults to 1, maintaining backward compatibility with existing code.

---

### Expandable - Icon Support

The `Expandable` widget now supports icons in the header, following the same pattern as `Button` and `Badge` widgets. Icons automatically scale based on the expandable's size (Small/Medium/Large).

**Basic usage:**

```csharp
new Expandable("Settings", "Configure your application preferences here.")
    .Icon(Icons.Settings);
```

**Multiple examples:**

```csharp
Layout.Vertical().Gap(2)
    | new Expandable("Settings", "Configure your application preferences here.").Icon(Icons.Settings)
    | new Expandable("User Profile", "View and edit your profile information.").Icon(Icons.User)
    | new Expandable("Notifications", "Manage your notification preferences.").Icon(Icons.Bell);
```

**With scale variations:**

```csharp
// Small expandable with icon
new Expandable("Small Settings", "Configure preferences.")
    .Icon(Icons.Settings)
    .Small();

// Medium expandable with icon (default)
new Expandable("Medium Settings", "Configure preferences.")
    .Icon(Icons.Settings)
    .Medium();

// Large expandable with icon
new Expandable("Large Settings", "Configure preferences.")
    .Icon(Icons.Settings)
    .Large();
```

The icon appears at the start of the header content and automatically adjusts its size to match the expandable's scale for consistent visual hierarchy.

---

### Progress - Indeterminate State for Unknown Progress

The `Progress` widget now supports an explicit `Indeterminate` property to display an animated progress bar when the completion percentage is unknown. This is more expressive than passing `null` as the value and enables showing indeterminate state while preserving the last known progress value.

**Basic usage:**

```csharp
// Basic indeterminate progress
new Progress()
    .Indeterminate()
    .Goal("Loading...");
```

**Toggle between determinate and indeterminate:**

```csharp
var isLoading = UseState(true);
var progress = UseState(0);

return Layout.Vertical()
    | new Progress(progress.Value)
        .Indeterminate(isLoading.Value)
        .Goal(isLoading.Value ? "Syncing..." : $"{progress.Value}% Complete")
    | Layout.Horizontal(
        new Button("Toggle Loading", _ => isLoading.Set(!isLoading.Value)),
        new Button("Set 50%", _ => progress.Set(50))
    );
```

**Key features:**

- The indeterminate animation respects `prefers-reduced-motion` for accessibility—users with motion sensitivity see a static appearance instead
- Works alongside existing progress values (e.g., show indeterminate state while keeping the progress bar at 50%)
- More explicit than passing `null`, making code intent clearer
- Backward compatible: passing `null` as the value still works

**Common use cases:**

- File uploads or downloads where progress isn't immediately available
- API calls with unknown duration
- Background sync operations
- Initial loading states before progress can be calculated

---

### SelectInput - Search, Loading, and Selection Limits

The `SelectInput` widget now supports advanced features for all variants (Select, List, and Toggle), including search functionality, loading states, and selection count limits. These features work seamlessly across single and multi-select modes.

**Search Functionality**

Enable search with customizable search modes:

```csharp
var framework = UseState<Frameworks?>(null);

return framework.ToSelectInput(options)
    .Variant(SelectInputVariants.Select)
    .Searchable()
    .SearchMode(SearchMode.Fuzzy)  // CaseInsensitive, CaseSensitive, or Fuzzy
    .EmptyMessage("No frameworks found");
```

**Loading State**

Show a loading indicator while fetching data:

```csharp
var isLoading = UseState(true);
var data = UseState<string[]>([]);

// Fetch data asynchronously
UseEffect(async () => {
    data.Set(await FetchDataAsync());
    isLoading.Set(false);
});

return data.ToSelectInput()
    .Loading(isLoading.Value)
    .Searchable();
```

**Selection Limits**

Enforce minimum and maximum selection counts for multi-select:

```csharp
var frameworks = UseState<Frameworks[]>([Frameworks.React]);

return frameworks.ToSelectInput(options)
    .Variant(SelectInputVariants.Select)
    .MinSelections(1)   // Must have at least 1 selected
    .MaxSelections(3);  // Can't select more than 3
```

**Complete Example**

Combining all features:

```csharp
public class SelectInputAdvancedDemo : ViewBase
{
    private enum Frameworks { React, Angular, Vue, Svelte, Ember, Backbone }

    public override object? Build()
    {
        var selected = UseState<Frameworks[]>([Frameworks.React]);
        var isLoading = UseState(false);
        var isSearchable = UseState(true);

        var options = typeof(Frameworks).ToOptions();

        return Layout.Vertical()
            | Text.H3("Framework Selection")
            | (Layout.Horizontal()
                | isLoading.ToSwitchInput().Label("Loading State")
                | isSearchable.ToSwitchInput().Label("Searchable"))
            | selected.ToSelectInput(options)
                .Variant(SelectInputVariants.Select)
                .Searchable(isSearchable.Value)
                .SearchMode(SearchMode.Fuzzy)
                .Loading(isLoading.Value)
                .MinSelections(1)
                .MaxSelections(3)
                .EmptyMessage("No frameworks found")
                .Width(Size.Grow());
    }
}
```

**Search Modes:**

- `SearchMode.CaseInsensitive` (default) - Case-insensitive substring matching
- `SearchMode.CaseSensitive` - Case-sensitive substring matching
- `SearchMode.Fuzzy` - Fuzzy matching (finds "rct" in "React")

**Behavior Notes:**

- When max selections is reached, remaining options become disabled
- When at min selections, selected items cannot be deselected
- Search works across all variants: Select, List, and Toggle
- Loading state disables interaction and shows a spinner
- Empty message displays when search returns no results

---

### SelectInput - Ghost Styling for Minimal Appearance

All select input variants now support ghost styling through the `Ghost()` extension method. Ghost styling removes borders, background fill, and shadows, creating a minimal appearance that blends seamlessly into its surroundings while maintaining full functionality.

**Basic usage with SelectInput:**

```csharp
var color = UseState(Colors.Red);
var colorOptions = typeof(Colors).ToOptions();

// Normal select
return color.ToSelectInput(colorOptions);

// Ghost select - minimal styling
return color.ToSelectInput(colorOptions).Ghost();
```

**Works with all select variants:**

```csharp
var frameworks = UseState<Frameworks[]>([Frameworks.React]);
var options = typeof(Frameworks).ToOptions();

// Ghost with List variant
return frameworks.ToSelectInput(options)
    .Variant(SelectInputVariants.List)
    .Ghost();

// Ghost with Toggle variant
return frameworks.ToSelectInput(options)
    .Variant(SelectInputVariants.Toggle)
    .Ghost();
```

**AsyncSelectInput also supports Ghost:**

```csharp
var category = UseState<Guid?>(null);

return category.ToAsyncSelectInput(
        QueryCategories,
        LookupCategory,
        placeholder: "Select Category"
    )
    .Ghost();
```

**Practical example - blending into card backgrounds:**

```csharp
return new Card(
    Layout.Vertical().Gap(2)
        | Text.Label("Select Framework")
        | framework.ToSelectInput(options).Ghost()
)
.Title("Settings")
.Width(Size.Units(100));
```

**When to use Ghost styling:**

- Inside cards or colored backgrounds where borders would create visual clutter
- In compact UIs where minimal chrome is preferred
- For inline selection controls that should feel lightweight
- When you want the select to blend with surrounding content

The ghost style maintains hover effects for interactivity feedback while removing the default border and background treatment.

---

### SelectInput - Disabled Options

Individual options within a SelectInput can now be disabled using the fluent `.Disabled()` method on `Option<T>`. Disabled options appear greyed out and cannot be selected, but remain visible in the list. This works across all SelectInput variants: Select (dropdown), List (checkboxes), Toggle, Radio, and MultiSelect.

**Basic usage:**

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

return fruit.ToSelectInput(fruitOptions)
    .Placeholder("Select a fruit...");
```

**Works with all variants:**

```csharp
var colors = UseState<string[]>([]);

var colorOptions = new IAnyOption[]
{
    new Option<string>("Red", "red"),
    new Option<string>("Green", "green"),
    new Option<string>("Blue (Premium)", "blue").Disabled(),
    new Option<string>("Yellow", "yellow"),
};

// Toggle variant
return colors.ToSelectInput(colorOptions)
    .Variant(SelectInputVariants.Toggle);

// List variant with checkboxes
return colors.ToSelectInput(colorOptions)
    .Variant(SelectInputVariants.List);
```

**Common use cases:**

- Indicate temporarily unavailable options (out of stock, coming soon)
- Show premium or restricted features that require upgrades
- Display items that don't meet current selection criteria
- Maintain context by showing all options while preventing selection of invalid ones

Disabled options remain visible to provide context to users about what might be available under different circumstances, while preventing selection when those options aren't currently valid.

---

### SelectInput - Optional Icons and Labels

Select options now support optional icons and labels, making it easier to create visually rich select inputs with icons and allowing icon-only options for compact interfaces.

**Adding Icons to Options:**

You can now add icons to any select option by specifying the `icon` property when creating options manually:

```csharp
var theme = UseState("light");

var themeOptions = new IAnyOption[]
{
    new Option<string>("Light", "light") { Icon = "sun" },
    new Option<string>("Dark", "dark") { Icon = "moon" },
    new Option<string>("Auto", "auto") { Icon = "laptop" },
};

return theme.ToSelectInput(themeOptions)
    .Placeholder("Select theme...");
```

**Icon-Only Options:**

Labels are now optional - when omitted, the option's value is used as the display text. This is perfect for icon-only interfaces:

```csharp
var alignment = UseState("left");

var alignmentOptions = new IAnyOption[]
{
    new Option<string>(null, "left") { Icon = "align-left" },
    new Option<string>(null, "center") { Icon = "align-center" },
    new Option<string>(null, "right") { Icon = "align-right" },
    new Option<string>(null, "justify") { Icon = "align-justify" },
};

return alignment.ToSelectInput(alignmentOptions)
    .Variant(SelectInputVariant.Toggle);
```

**Works Across All Variants:**

Icons work seamlessly with all SelectInput variants:

```csharp
var status = UseState("active");

var statusOptions = new IAnyOption[]
{
    new Option<string>("Active", "active") { Icon = "check-circle" },
    new Option<string>("Pending", "pending") { Icon = "clock" },
    new Option<string>("Inactive", "inactive") { Icon = "x-circle" },
};

// Dropdown variant
return status.ToSelectInput(statusOptions)
    .Variant(SelectInputVariant.Select);

// Radio variant
return status.ToSelectInput(statusOptions)
    .Variant(SelectInputVariant.Radio);

// Toggle variant
return status.ToSelectInput(statusOptions)
    .Variant(SelectInputVariant.Toggle);

// List variant (checkboxes for multi-select)
return statusList.ToSelectInput(statusOptions)
    .Variant(SelectInputVariant.List);
```

**Common Use Cases:**

- Theme selectors with sun/moon icons
- Text alignment toolbars with alignment icons
- Status indicators with visual state icons
- Language/region pickers with flag icons
- File type filters with document icons
- Compact toolbars where space is limited

Icons automatically scale to match the density setting (Small, Medium, Large) of the select input.

---

### Forms - Auto-Scaffold [AllowedValues] as SelectInput

String and string array properties with the `[AllowedValues]` attribute are now automatically scaffolded as `SelectInput` widgets (single or multi-select) when using `.ToForm()`. This eliminates the need for manual `.Builder()` calls and makes forms more declarative.

**Before (manual Builder required):**

```csharp
public class SettingsModel
{
    [AllowedValues("Light", "Dark", "Auto")]
    public string Theme { get; set; } = "Auto";

    [AllowedValues("Technology", "Sports", "Music", "Art", "Travel")]
    public string[] Interests { get; set; } = [];
}

public override object? Build()
{
    var settings = UseState(() => new SettingsModel());

    var themeOptions = new[] { "Light", "Dark", "Auto" }.ToOptions();
    var interestOptions = new[] { "Technology", "Sports", "Music", "Art", "Travel" }.ToOptions();

    return settings.ToForm()
        .Builder(m => m.Theme, s => s.ToSelectInput(themeOptions))
        .Builder(m => m.Interests, s => s.ToSelectInput(interestOptions).List());
}
```

**After (automatic scaffolding):**

```csharp
public class SettingsModel
{
    [AllowedValues("Light", "Dark", "Auto")]
    public string Theme { get; set; } = "Auto";

    [AllowedValues("Technology", "Sports", "Music", "Art", "Travel")]
    public string[] Interests { get; set; } = [];
}

public override object? Build()
{
    var settings = UseState(() => new SettingsModel());

    // No Builder calls needed - automatically creates SelectInput!
    return settings.ToForm();
}
```

**How it works:**

- `string` properties with `[AllowedValues]` → Single-select dropdown
- `string[]` properties with `[AllowedValues]` → Multi-select dropdown
- Respects nullable types and `[Required]` attributes
- Works seamlessly with other form scaffolding features

**Complete example with mixed field types:**

```csharp
public class UserPreferencesModel
{
    [Required]
    [Display(Name = "Full Name")]
    public string Name { get; set; } = "";

    [AllowedValues("USA", "Canada", "UK", "Germany", "France")]
    [Display(Name = "Country")]
    public string Country { get; set; } = "USA";

    [AllowedValues("Light", "Dark", "Auto")]
    [Display(Name = "Theme Preference")]
    public string Theme { get; set; } = "Auto";

    [AllowedValues("Technology", "Sports", "Music", "Art", "Travel")]
    [Display(Name = "Interests", Description = "Select your interests")]
    public string[] Interests { get; set; } = [];
}

public override object? Build()
{
    var preferences = UseState(() => new UserPreferencesModel());

    // All fields automatically scaffolded with appropriate inputs
    return preferences.ToForm("Save Preferences");
}
```

This feature reduces boilerplate code while maintaining type safety and leveraging C#'s built-in `[AllowedValues]` attribute for compile-time validation.

---

### Table - Progress Builder for Inline Progress Bars

The `Table` widget now supports rendering numeric values as inline progress bars through the new `Progress()` builder. This enables visual representation of completion percentages, download progress, scores, and other numeric data directly within table cells.

**Basic usage:**

```csharp
var tasks = new[]
{
    new { Name = "Design Review", Progress = 100 },
    new { Name = "Implementation", Progress = 75 },
    new { Name = "Testing", Progress = 45 },
    new { Name = "Documentation", Progress = 20 }
};

return tasks.ToTable()
    .Builder(t => t.Progress, f => f.Progress());
```

**Fluent API for customization:**

```csharp
// Auto-color based on percentage thresholds
tasks.ToTable()
    .Builder(t => t.Progress, f => f.Progress().AutoColor());
// Green (≥75%), Yellow (≥50%), Orange (≥25%), Red (<25%)

// Custom range for non-percentage values
downloads.ToTable()
    .Builder(d => d.BytesDownloaded, f => f
        .Progress()
        .Min(0)
        .Max(1000000)
        .AutoColor());

// Format string to show value alongside progress bar
tasks.ToTable()
    .Builder(t => t.Progress, f => f.Progress().Format("%d%"));

// Explicit color for all progress bars
tasks.ToTable()
    .Builder(t => t.Progress, f => f.Progress().Color(Colors.Blue));
```

**Complete example with download tracking:**

```csharp
var downloads = new[]
{
    new { File = "report.pdf", Downloaded = 1024, Total = 1024 },
    new { File = "data.csv", Downloaded = 750, Total = 1000 },
    new { File = "images.zip", Downloaded = 250, Total = 500 }
};

return downloads
    .Select(d => new {
        d.File,
        d.Downloaded,
        d.Total,
        Percent = (double)d.Downloaded / d.Total * 100
    })
    .ToTable()
    .Builder(e => e.Percent, f => f
        .Progress()
        .Min(0)
        .Max(100)
        .AutoColor()
        .Format("%d%"))
    .Header(e => e.Percent, "Progress");
```

**Configuration options:**

- **`.Min(double)`** - Minimum value for percentage calculation (default: 0)
- **`.Max(double)`** - Maximum value for percentage calculation (default: 100)
- **`.AutoColor()`** - Automatic color based on thresholds
- **`.Color(Colors)`** - Explicit color for the progress bar
- **`.Format(string)`** - Display formatted value alongside the bar (`%d` for integer, `%f` for decimal)

**Common use cases:**

- Task completion tracking in project management dashboards
- Download/upload progress monitoring
- Score or rating visualization
- Capacity utilization (storage, memory, CPU)
- Goal achievement tracking

---

### Html - JavaScript Execution with DangerouslyAllowScripts

The `Html` widget now supports executing JavaScript through the `DangerouslyAllowScripts` extension method. By default, the Html widget sanitizes all content and strips script tags for security. This new option allows you to bypass sanitization when you have complete trust in the HTML source.

**⚠️ Security Warning:** Only use this feature with completely trusted HTML sources. Enabling script execution with untrusted content exposes your application to Cross-Site Scripting (XSS) attacks. **Never use this with user-generated content.**

**Basic usage:**

```csharp
var htmlWithScript = """
    <div id="target-div">Loading...</div>
    <script>
        document.getElementById('target-div').innerText = 'Script executed successfully!';
    </script>
    """;

return new Html(htmlWithScript)
    .DangerouslyAllowScripts();
```

**Default secure behavior (scripts stripped):**

```csharp
// Without DangerouslyAllowScripts, script tags are removed during sanitization
var htmlWithScript = "<script>alert('Hello');</script><p>Content</p>";

return new Html(htmlWithScript);  // Only <p>Content</p> is rendered
```

**When to use:**

- Embedding trusted third-party widgets (analytics, chat, etc.)
- Rendering HTML from your own backend templates
- Displaying documentation with interactive code examples
- Loading content from trusted CMS systems you control

**When NOT to use:**

- User-generated content (comments, forum posts, user profiles)
- HTML from external APIs you don't control
- Any content that could be modified by users
- Dynamic HTML where you're unsure of the source

The method signature supports both explicit and implicit enabling:

```csharp
new Html(trustedHtml).DangerouslyAllowScripts()       // Enable
new Html(trustedHtml).DangerouslyAllowScripts(true)   // Enable explicitly
new Html(trustedHtml).DangerouslyAllowScripts(false)  // Disable (use default sanitization)
```

---

### SidebarLayout - Resizable Drag-to-Resize Support

The `SidebarLayout` widget now supports drag-to-resize functionality, allowing users to adjust the sidebar width at runtime by dragging the sidebar border. This provides a more flexible and customizable user experience for applications with sidebar navigation.

**Basic usage:**

```csharp
return new SidebarLayout(
    mainContent: new Card("Your main content here").Title("Main Content"),
    sidebarContent: Layout.Vertical().Gap(2)
        | Text.P("Sidebar Content")
        | Text.P("Drag the right edge to resize this sidebar.").Small()
).Resizable();
```

**Custom width constraints:**

By default, the sidebar can be resized between 200px and 600px. You can customize these constraints using the `Size` API with `.Min()` and `.Max()`:

```csharp
return new SidebarLayout(
    mainContent: new Card("Main content").Title("Content"),
    sidebarContent: Layout.Vertical().Gap(2)
        | Text.P("Custom Width Sidebar")
)
.Width(Size.Px(250).Min(Size.Px(150)).Max(Size.Px(400)))
.Resizable();
```

**Key features:**

- **Mouse drag**: Click and drag the sidebar border to resize
- **Touch gestures**: Full touch support for mobile and tablet devices
- **Keyboard navigation**: Use arrow keys on the resize handle for accessibility
- **Default constraints**: 200px minimum, 600px maximum (customizable)
- **Smooth animations**: Visual feedback during resize operations
- **Persistent state**: Width persists during the user's session

**When to use:**

- Applications where users want to customize their workspace layout
- Dashboards with variable amounts of navigation or filtering content
- Document editors with collapsible tool panels
- Admin interfaces with configurable sidebars

---

### Sheet - Slide from Any Edge with Side API

The `Sheet` widget now supports sliding in from any edge of the screen through the new `Side` API. Previously, sheets could only slide in from the right side—now you can choose from left, right, top, or bottom using the `SheetSide` enum.

**Basic usage:**

```csharp
// Slide from left
new Button("Left Menu").WithSheet(
    () => new Card("Navigation menu").Title("Menu"),
    title: "Navigation",
    side: SheetSide.Left
);

// Slide from right (default)
new Button("Right Panel").WithSheet(
    () => new Card("Settings panel").Title("Settings"),
    title: "Settings",
    side: SheetSide.Right
);

// Slide from top
new Button("Top Notification").WithSheet(
    () => new Card("Important message").Title("Alert"),
    title: "Notification",
    width: Size.Rem(16),  // Controls height for top/bottom
    side: SheetSide.Top
);

// Slide from bottom
new Button("Bottom Menu").WithSheet(
    () => new Card("Action menu").Title("Actions"),
    title: "Actions",
    width: Size.Rem(16),  // Controls height for top/bottom
    side: SheetSide.Bottom
);
```

**Using fluent extension method:**

```csharp
var isOpen = UseState(false);

return content.ToSheet(isOpen,
    title: "My Sheet",
    width: Size.Rem(24),
    side: SheetSide.Left);
```

**Important notes:**

- The `width` parameter controls **height** for top/bottom sheets and **width** for left/right sheets
- Default side is `SheetSide.Right` for backward compatibility
- Each direction has smooth slide-in animations
- Top and bottom sheets default to `Size.Rem(16)` height, while left/right default to `Size.Rem(24)` width

**Common use cases:**

- **Left**: Navigation panels, sidebars, menu drawers
- **Right**: Settings panels, detail views, property inspectors (default)
- **Top**: Notifications, announcements, banners
- **Bottom**: Action menus, mobile-style selection sheets

---

### Separator - Text Alignment Control

The `Separator` widget now supports text alignment for separator labels through the new `TextAlign` property and fluent API. You can position label text at the left, center, or right along the separator line.

**Basic usage:**

```csharp
// Left aligned label
new Separator("Left Aligned").TextAlign(TextAlignment.Left);

// Center aligned label (default)
new Separator("Center Aligned").TextAlign(TextAlignment.Center);

// Right aligned label
new Separator("Right Aligned").TextAlign(TextAlignment.Right);
```

**Practical example - section separators with varied alignment:**

```csharp
return Layout.Vertical().Gap(4)
    | new Separator("Section Start").TextAlign(TextAlignment.Left)
    | Text.P("Content for this section goes here...")
    | new Separator("Important Note").TextAlign(TextAlignment.Center)
    | Text.P("This is an important message.")
    | new Separator("Page End").TextAlign(TextAlignment.Right);
```

**Default behavior:**

When no alignment is specified, separator labels default to center alignment for backward compatibility with existing code.

**Common use cases:**

- Left alignment for section headers that read like chapter titles
- Center alignment for emphasized dividers or callouts
- Right alignment for end-of-section markers or footnotes

---

### CodeBlock - WrapLines Option for Long Lines

The `CodeBlock` widget now supports a `WrapLines` option that allows long lines to wrap within the code block instead of requiring horizontal scrolling. This improves readability when displaying code with long lines in constrained layouts.

**Basic usage:**

```csharp
var longCode = @"public class VeryLongClassName {
    public void VeryLongMethodName(string veryLongParameterName, int anotherVeryLongParameterName) {
        Console.WriteLine(""This is a very long line that will wrap when WrapLines is enabled."");
    }
}";

return new CodeBlock(longCode, Languages.Csharp)
    .WrapLines();
```

**Default behavior vs. WrapLines:**

```csharp
// Without WrapLines (default) - requires horizontal scrolling
new CodeBlock(longCode, Languages.Csharp);

// With WrapLines - wraps long lines for better readability
new CodeBlock(longCode, Languages.Csharp)
    .WrapLines();
```

**Combining with other options:**

```csharp
// Works well with line numbers and other CodeBlock features
new CodeBlock(longCode, Languages.Csharp)
    .WrapLines()
    .ShowLineNumbers()
    .ShowCopyButton();
```

**When to use:**

- Displaying code in narrow panels or responsive layouts
- Code with very long lines (URLs, long strings, etc.)
- When horizontal scrolling would disrupt the user experience
- Documentation or tutorial content where readability is paramount

By default, `WrapLines` is `false`, preserving the original formatting with horizontal scroll for code that depends on strict line structure.

---

### Ivy.Desktop - Run Ivy Apps as Native Desktop Applications

The new `Ivy.Desktop` library enables you to wrap your Ivy web applications as native desktop applications using Photino. This provides a seamless desktop experience with native window management, system tray integration, and cross-platform support for Windows, macOS, and Linux.

**Installation:**

```bash
dotnet add package Ivy.Desktop
```

**Basic usage:**

```csharp
using Ivy.Desktop;

var server = new Server(args);
server.MapGet("/", () => new Page("My App") { new Text("Hello Desktop!") });

var window = new DesktopWindow(server)
    .Title("My Desktop App")
    .Size(1280, 800)
    .Run();
```

**Window customization with fluent API:**

```csharp
var window = new DesktopWindow(server)
    .Title("My Desktop App")
    .Size(1280, 800)
    .Resizable(true)
    .Center()
    .DpiScaling(true)  // Automatically adjusts for high-DPI displays
    .Run();
```

**Adding a custom icon:**

```csharp
var window = new DesktopWindow(server)
    .Title("My Desktop App")
    .Size(1280, 800)
    .Icon(typeof(Program), "MyApp.icon.ico")  // Embedded resource
    .Run();
```

**Advanced configuration:**

```csharp
var window = new DesktopWindow(server)
    .Title("Production Dashboard")
    .Size(1600, 1000)
    .Resizable(true)
    .TopMost(false)
    .Center()
    .DpiScaling(true)
    .DevToolsEnabled(isDevelopment)  // Enable dev tools in development
    .Run();
```

**Key features:**

- **Cross-platform**: Works on Windows, macOS, and Linux with automatic native library handling
- **DPI awareness**: Automatic scaling for Retina and high-DPI displays
- **Builder pattern**: Clean, fluent API for window configuration
- **Native integration**: Uses Photino for true native window management
- **MSBuild integration**: Automatically copies required native libraries during build and publish

The library automatically handles platform-specific details like native library copying via MSBuild targets, so your desktop app "just works" across all supported platforms.

**Default Icon:**

Desktop applications now automatically include the Ivy icon embedded in the executable. If you don't specify a custom icon via `.Icon()`, your exported desktop app will display the Ivy Framework icon in the taskbar, dock, and title bar. You can still override this with your own custom icon using the `.Icon()` method as shown above.

---

### Server Configuration - External Configuration Providers

The `Server` class now supports extending the default configuration pipeline with external configuration sources through the `UseConfiguration` method. This enables you to add custom configuration providers like Azure Key Vault, AWS Secrets Manager, or any other configuration source while preserving the built-in defaults (environment variables, appsettings.json, user secrets).

**Basic usage:**

```csharp
var server = new Server(args);

server.UseConfiguration(config => {
    config.AddJsonFile("custom-config.json", optional: true);
});
```

**Azure Key Vault integration:**

```csharp
var server = new Server(args);

server.UseConfiguration(config => {
    var keyVaultUrl = Environment.GetEnvironmentVariable("KEY_VAULT_URL");
    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        config.AddAzureKeyVault(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential());
    }
});
```

**AWS Secrets Manager:**

```csharp
var server = new Server(args);

server.UseConfiguration(config => {
    config.AddSecretsManager(region: RegionEndpoint.USEast1);
});
```

**Multiple custom sources:**

```csharp
var server = new Server(args);

server.UseConfiguration(config => {
    // Add custom JSON configuration
    config.AddJsonFile("overrides.json", optional: true);

    // Add cloud-based secrets
    if (server.IsProduction())
    {
        config.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());
    }

    // Add in-memory overrides for testing
    if (server.IsDevelopment())
    {
        config.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["FeatureFlags:NewUI"] = "true",
            ["DebugMode"] = "true"
        });
    }
});
```

**Key features:**

- External providers are applied **after** built-in sources (environment variables, appsettings.json, user secrets)
- The configuration action receives an `IConfigurationBuilder` to extend
- Multiple configuration sources can be chained within the action
- Preserves all default framework configuration behavior
- Compatible with any .NET configuration provider

This makes it easy to integrate enterprise configuration systems while maintaining the framework's convention-based defaults.

---

## Improvements

### Spacer Now Defaults to Grow Behavior

A bare `new Spacer()` now defaults to grow behavior (`flex-grow: 1`), automatically filling available space in the parent layout's direction. This eliminates the need to explicitly specify `.Width(Size.Grow())` for the common use case of pushing sibling elements apart.

**What changed:**

```csharp
// Before: Had to explicitly use Size.Grow()
Layout.Horizontal()
    | new Button("Left")
    | new Spacer().Width(Size.Grow())
    | new Button("Right");

// After: Spacer grows by default
Layout.Horizontal()
    | new Button("Left")
    | new Spacer()  // Automatically grows to fill space
    | new Button("Right");
```

**Why this matters:**

This matches the overwhelmingly common use case for `Spacer` - creating flexible space that pushes elements to opposite sides of a layout. The new default makes layouts cleaner and more intuitive, while still allowing explicit sizing when needed via `.Width()` or `.Height()`.

**Note:** If you need a fixed-size spacer, you can still explicitly set dimensions:

```csharp
new Spacer().Width(Size.Units(20))  // Fixed 20-unit spacer
```

---

### Grow() Method Now Available on All Widgets

The convenient `.Grow()` extension method is now available on all widgets, not just `Box`. Previously, only boxes could use `.Grow()` as a shorthand for `.Width(Size.Grow())`. Now every widget type can use this fluent API.

**What changed:**

```csharp
// Before: Only worked on Box
new Box(content).Grow()

// Before: Other widgets required the verbose syntax
new TextInput("Name").Width(Size.Grow())

// After: All widgets support .Grow()
new TextInput("Name").Grow()
new Button("Submit").Grow()
new Card(content).Grow()
```

**Why this matters:**

This makes the API more consistent and ergonomic across the framework. The `.Grow()` method is a common pattern for making widgets fill available space, and now it works uniformly regardless of widget type.

**Technical details:**

The method has been promoted from a `Box`-specific extension to a generic `WidgetBase<T>` extension, making it available to all widget types that inherit from `WidgetBase<T>`.

---

### GridView: Separate RowGap and ColumnGap Methods

GridView now offers granular control over grid spacing with dedicated `RowGap()` and `ColumnGap()` methods. The existing `Gap()` method now sets both row and column gaps simultaneously, while the new methods let you control each axis independently.

**What changed:**

```csharp
// Before: Gap() only set a single gap value
new GridView()
    .Gap(16);

// Now: Gap() sets both row and column gaps
new GridView()
    .Gap(16);  // Sets both to 16

// New: Control row and column gaps independently
new GridView()
    .RowGap(20)      // Vertical spacing between rows
    .ColumnGap(12);  // Horizontal spacing between columns
```

**Why this matters:**

Different spacing for rows vs. columns is a common design pattern, especially in responsive grids. You might want tighter horizontal spacing but more breathing room vertically, or vice versa. Previously, you'd need custom CSS or workarounds to achieve this.

**Use cases:**

```csharp
// Card grid with more vertical space
new GridView()
    .Columns("repeat(auto-fill, minmax(200px, 1fr))")
    .RowGap(32)
    .ColumnGap(16)
    | cards;

// Form layout with compact horizontal, spacious vertical
new GridView()
    .Columns("1fr 2fr")
    .RowGap(24)      // Space between form rows
    .ColumnGap(12);  // Tight label-to-input spacing
```

---

### Theme Defaults to System Preference

The framework now defaults to `system` theme instead of `light` theme, automatically respecting users' system-wide dark/light mode preferences. Apps will now adapt to the operating system's appearance settings by default, providing a better out-of-the-box experience.

**Previous behavior:**

- Apps defaulted to light mode regardless of system preferences
- Users had to manually switch to dark mode even if their OS was set to dark

**New behavior:**

- Apps automatically match the system's light/dark mode preference
- Seamlessly switches between light and dark as the user changes their system settings
- Manual theme selection still available and takes precedence when set

No code changes required in your apps - this improvement applies automatically to all Ivy Framework applications.

---

### AddConnectionsFromAssembly - Optional Assembly Parameter

The `Server.AddConnectionsFromAssembly()` method now accepts an optional `Assembly` parameter, giving you explicit control over which assembly to scan for `IConnection` types.

**What changed:**

```csharp
// Before: always scanned the entry assembly
server.AddConnectionsFromAssembly();

// Now: optionally specify which assembly to scan
server.AddConnectionsFromAssembly(typeof(MyConnection).Assembly);

// Or use the default behavior (entry assembly)
server.AddConnectionsFromAssembly();
```

**Why this matters:**
This enhancement is particularly useful when your entry assembly differs from the one containing your connection definitions. Common scenarios include:

- Plugin architectures where connections are defined in separate assemblies
- Class libraries containing reusable connections
- Testing scenarios where you want to load specific assemblies

**API consistency:**
This change matches the existing `AddAppsFromAssembly(Assembly? assembly = null)` signature, providing a consistent API surface across the framework.

**Note on recompilation:**
This is a binary breaking change. If you see a `MissingMethodException` at runtime, run `dotnet clean && dotnet build` to recompile your project against the updated method signature.

---

### DataTableBuilder - Remove() Method for API Consistency

The `DataTableBuilder` now supports the `.Remove()` method, bringing it in line with other builders like `FormBuilder`, `TableBuilder`, and `DetailsBuilder`. This method allows you to completely exclude columns from your data tables.

**What's the difference?**

- **`.Remove()`** - Fully excludes columns from the data (column is not sent to the client at all)
- **`.Hidden()`** - Keeps columns in the data but hides them visually (data still exists on the client side)

**Basic usage:**

```csharp
var users = GetUsers();

return users.ToDataTable()
    .Remove(u => u.PasswordHash)      // Exclude sensitive data completely
    .Remove(u => u.InternalNotes);    // Not sent to client
```

**Why this matters:**

Use `.Remove()` when you want to completely exclude data for security or performance reasons. This is especially important for:

- Sensitive fields like password hashes, API keys, or internal notes
- Large binary data that's not needed for display
- Computed or redundant fields that bloat the payload

Use `.Hidden()` when you want to keep the data available for client-side operations (filtering, sorting, exports) but don't want to display it in the visible columns.

**Example with both:**

```csharp
return users.ToDataTable()
    .Remove(u => u.PasswordHash)      // Security: never send to client
    .Remove(u => u.InternalNotes)     // Privacy: internal only
    .Hidden(u => u.CreatedAt)         // Keep for sorting, but hide column
    .Hidden(u => u.UserId);           // Keep for row actions, but hide column
```

This enhancement completes the API surface for `DataTableBuilder`, making it consistent with other builder classes in the framework.

---

### Clerk Auth: Graceful Handling of Existing Sessions

The Clerk authentication provider now gracefully handles scenarios where a session already exists during sign-in, making the authentication flow more robust and user-friendly.

**What changed:**

- When signing in with a session already active, the provider now attempts to restore and reuse the existing session
- If restoration fails, it automatically cleans up stale sessions and retries the sign-in
- This eliminates sign-in failures that could occur in edge cases like browser back/forward navigation or concurrent sign-in attempts

**Impact:**
Users will experience more reliable sign-in behavior without encountering "session already exists" errors. The improvement works automatically - no code changes needed in your authentication logic.

**Affected methods:**

- Password-based sign-in (email/password)
- OAuth sign-in (Google, GitHub, etc.)

---

### WithConfirm: Customizable Button Labels and Destructive Styling

The `WithConfirm` helper method now supports customizable confirm button labels and destructive styling, making confirmation dialogs more appropriate for delete operations and other critical actions.

**What changed:**

```csharp
// Before: Always showed "Ok" button with default styling
new Button("Delete")
    .Variant(ButtonVariant.Destructive)
    .WithConfirm("Are you sure you want to delete this item?", "Delete Item");

// After: Customize button label and use destructive styling
new Button("Delete")
    .Variant(ButtonVariant.Destructive)
    .WithConfirm(
        message: "Are you sure you want to delete this item?",
        title: "Delete Item",
        confirmLabel: "Delete",    // Custom label instead of "Ok"
        destructive: true          // Red destructive button styling
    );
```

**New parameters:**

- `confirmLabel` (optional) - Customizes the confirm button text (defaults to "Ok")
- `destructive` (optional) - Applies destructive styling (`ButtonVariant.Destructive`) to the confirm button for dangerous actions

**Why this matters:**

Confirmation dialogs for destructive actions should clearly communicate the severity of the action. A delete dialog with a "Delete" button styled in red provides much clearer intent than a generic "Ok" button. This enhancement makes your confirmation dialogs more user-friendly and reduces the chance of accidental destructive operations.

**Real-world usage:**

```csharp
// Delete confirmation with appropriate styling
deleteButton
    .WithConfirm(
        $"Are you sure you want to delete '{category.Name}'?",
        "Delete Category",
        confirmLabel: "Delete",
        destructive: true
    );

// Standard confirmation keeps default "Ok" button
saveButton
    .WithConfirm(
        "This will overwrite the existing data. Continue?",
        "Confirm Save"
    );
```

The method remains backward compatible - existing code without the new parameters will continue to work with the default "Ok" button and primary styling.

---

### App IDs Now Support Dots for Namespacing and Versioning

App IDs can now contain dots, enabling more flexible naming schemes for versioning, namespacing, and organizing your apps. Previously restricted, dots are now fully supported and encouraged for creating hierarchical app identifiers.

**What changed:**

```csharp
// Now valid: versioned app IDs
"dashboard.v2"
"api.v1"
"app.v3.beta"

// Now valid: namespaced app IDs
"users.profile"
"admin.settings"
"com.example.app"

// Now valid: gRPC-style service names
"datatable.DataTableService"
"grpc.ServiceName"
```

**Why this matters:**

Dots in app IDs enable cleaner organization patterns:

- **Versioning:** Run multiple versions side-by-side (`api.v1`, `api.v2`)
- **Namespacing:** Group related apps (`users.profile`, `users.settings`)
- **Domain-style naming:** Use reverse domain notation (`com.company.product`)
- **gRPC compatibility:** Match your gRPC service naming conventions

**Improved validation:**

Along with dot support, app ID validation has been significantly enhanced with clear error messages:

```csharp
// ❌ Invalid: starts with slash
"/dashboard"  // Error: "App IDs should not start with '/'"

// ❌ Invalid: unsafe characters
"app?query"   // Error: "App IDs must be URL-friendly"
"app%20name"  // Error: "App IDs must be URL-friendly"

// ❌ Invalid: reserved paths
"api"         // Error: "App ID 'api' collides with reserved path '/api'"

// ❌ Invalid: static file extensions
"app.js"      // Error: "App ID 'app.js' collides with static file extension"
"config.json" // Error: "App ID 'config.json' collides with static file extension"

// ✅ Valid: dots without static extensions
"app.v2"      // Valid - .v2 is not a static file extension
"my.app"      // Valid - .app is not a static file extension
```

**Protected static file extensions:**

The framework now prevents app IDs that could conflict with static files: `.js`, `.jsx`, `.css`, `.html`, `.json`, `.png`, `.jpg`, `.ico`, `.svg`, `.woff`, `.woff2`, `.ttf`, `.map`

**Error handling:**

Invalid app IDs are now caught at startup and during hot-reload with clear, actionable error messages. Apps with invalid IDs won't be added to the repository, preventing runtime routing issues.

No migration needed - this is purely an enhancement that expands what's possible with app IDs while maintaining backward compatibility with existing dash and underscore-based naming.

---

### Desktop Apps: Instant Window Display with Loading Screen

Desktop applications now show the window immediately with a clean loading screen, eliminating the startup delay users previously experienced.

**What changed:**

- The window appears instantly when you launch your app with a light, modern design
- Light theme with clean colors: `#ffffff` (white background), `#00cc92` (primary green spinner), `#dd5860` (error red)
- Uses the modern **Geist font** from Vercel for a polished, contemporary look
- **Smart loading spinner** that only appears after 4 seconds to avoid flickering on fast startups
- Animated spinner with "Connecting to server..." message once it appears
- Client-side JavaScript polling checks when the server is ready and automatically navigates to your app
- 30-second timeout with clear error messaging if the server fails to start
- Error dialogs also updated to use the light theme and Geist font for a consistent look

**Impact:**
Your desktop apps feel significantly more responsive and polished with immediate window display and a professional loading experience. Fast-loading apps won't show unnecessary loading indicators, while slower startups still provide clear feedback to users.

**Technical details:**
The loading page polls the server every 500ms using `fetch()` with `mode: 'no-cors'`. The loading spinner is initially hidden and only becomes visible after 4 seconds of elapsed time to prevent flashing on quick startups. Once the server responds, it automatically redirects to your app URL. If the server doesn't respond within 30 seconds, users see a clear error message.

No code changes needed - this enhancement applies automatically to all Ivy desktop applications.

---

### Desktop Apps: Default Ivy Icon for Windows

Desktop applications now automatically display the Ivy logo in the taskbar and title bar when no custom icon is explicitly set, giving your apps a polished appearance out of the box.

**What changed:**

- When you create a desktop window without calling `.Icon()`, it now automatically uses the embedded Ivy icon
- The ivy.ico resource is embedded directly in the `Ivy.Desktop` package
- Your app's window will show the Ivy logo in the taskbar and window title bar by default

**Before:**

```csharp
// No icon set = blank/default system icon in taskbar
new DesktopWindow(server)
    .Title("My App")
    .Run();
```

**After:**

```csharp
// No icon set = Ivy logo appears in taskbar automatically
new DesktopWindow(server)
    .Title("My App")
    .Run();
```

**Setting a custom icon still works as before:**

```csharp
// Custom icon overrides the default
new DesktopWindow(server)
    .Title("My App")
    .Icon<MyApp>("MyApp.Resources.custom-icon.ico")
    .Run();
```

**Impact:**
Your desktop applications now have a professional appearance by default without needing to configure an icon. This is especially useful during development and prototyping when you haven't yet created a custom icon. When you're ready to brand your app, simply call `.Icon()` to override the default.

---

### DefaultSidebarChrome: Auto-Open Sidebar When Last Tab Closes

The `DefaultSidebarChrome` now automatically opens the sidebar when you close the last tab, preventing an empty state where both the sidebar and all tabs are closed.

**What changed:**

- When closing the final tab (which redirects to the home page), the sidebar automatically opens
- This ensures users always have navigation options visible
- The sidebar state is now dynamically managed to respond to tab closure events

**Impact:**
Users experience better navigation flow - closing all tabs no longer leaves them without visible navigation options. The sidebar intelligently opens to provide immediate access to navigation, improving the overall user experience in multi-tab applications.

---

### Nested App Streaming Support

The `UseStream` hook now works seamlessly in nested apps hosted via `AppHostWidget`. Previously, streaming functionality might not have worked correctly when using `UseStream` within an app that's hosted inside another app.

**What changed:**

- `AppHostWidget` now provides the necessary streaming infrastructure to child apps
- Stream subscriptions are properly propagated through nested app boundaries
- Both `RichTextBlock.UseStream()` and `Terminal.UseStream()` work correctly in nested contexts

**Impact:**
If you're building applications that host other apps (e.g., a dashboard that embeds multiple sub-apps, or a plugin system), streaming features like real-time updates, AI response streaming, and terminal output now work correctly in all nested apps without any code changes needed.

No code changes needed - this improvement applies automatically to applications using `DefaultSidebarChrome`.

---

### Desktop Apps: Error Dialog for Unhandled Exceptions

Desktop applications now show a proper error dialog when unhandled exceptions occur, instead of silently failing or writing to an invisible console.

**What changed:**

- The `DesktopWindow.Run()` method now wraps execution in error handling
- Unhandled exceptions trigger a native error dialog window with the error message and full stack trace
- The error dialog features a clean, modern design using the **Geist font** and light theme
- Light background with a subtle bordered code block for stack traces
- This replaces the previous `Console.WriteLine` approach which was invisible in Windows GUI apps

**Impact:**
When your desktop app encounters an error (e.g., server startup failure, unhandled exception), users and developers will see a clear, professionally styled error dialog instead of the app silently crashing or appearing to hang. This significantly improves debugging and troubleshooting desktop applications.

**Example error scenarios now handled:**

- Server fails to start on the expected port
- Initialization errors during app startup
- Any unhandled exceptions during the window lifecycle

No code changes needed - this protection applies automatically to all Ivy desktop applications.

---

### Desktop Apps: WebView2 Threading Fix for Windows

Desktop applications now automatically handle threading requirements for WebView2 on Windows, fixing an issue where windows would open but display blank content.

**What was the problem:**
WebView2 requires STA (Single-Threaded Apartment) threading for its COM message pump. If your app started on the wrong thread type, Photino windows would open but show blank white content instead of your UI.

**What changed:**

- `DesktopWindow.Run()` automatically detects the thread apartment state on Windows
- If not on an STA thread, it creates and switches to an STA thread automatically
- Works seamlessly without any code changes or performance impact

**Impact:**
Your desktop apps now work reliably regardless of how the application thread is initialized. This particularly helps when launching from different contexts (console apps, test runners, or other host applications).

No code changes needed - this fix applies automatically to all Ivy desktop applications.

---

### Desktop Apps: Automatic Ivy Icon Embedding

Desktop applications now automatically include the Ivy icon in their executable files, giving your apps a professional appearance in taskbars, file explorers, and window title bars without any additional configuration.

**What changed:**

- The Ivy.Desktop package now includes an `ivy.ico` icon file
- MSBuild automatically sets the `ApplicationIcon` property during build if you haven't specified a custom icon
- The icon is embedded in your compiled `.exe` file on all platforms

**Impact:**
Your desktop applications now have a polished, branded appearance by default. The Ivy leaf icon will appear:

- In the Windows taskbar when your app is running
- In the file explorer alongside your `.exe` file
- In the window title bar (on supported platforms)
- In Alt+Tab and other OS-level application switchers

**Using a custom icon:**
If you want to use your own icon instead, simply set the `ApplicationIcon` property in your `.csproj` file - the automatic Ivy icon will be skipped in favor of your custom one:

```xml
<PropertyGroup>
  <ApplicationIcon>path/to/your/icon.ico</ApplicationIcon>
</PropertyGroup>
```

No code changes needed - this enhancement applies automatically to all new and existing Ivy desktop applications.

---

### Desktop Apps: Server Readiness Check Prevents Premature Loading

Desktop applications now wait for the server to be fully ready before loading the UI, eliminating race conditions and catching early server failures.

**What was the problem:**
Previously, the desktop window would immediately try to load the app URL as soon as `server.RunAsync()` was called. This created two issues:

- The port was read before the server had actually bound to it, potentially using the wrong port number
- The WebView could navigate to the URL before the server was accepting requests, causing connection failures

**What changed:**

- The actual bound port is now read after `RunAsync()` returns, ensuring the correct port is used
- A new `WaitForServerReady()` method polls the server's `/ivy/health` endpoint before the window loads
- Detects if the server task faults or exits early (e.g., missing secrets, port conflicts)
- 30-second timeout with clear error message if the server doesn't become ready

**Impact:**
Desktop apps now start more reliably, especially in scenarios where:

- The server takes a moment to initialize and bind to a port
- The server fails early due to configuration issues (missing secrets, invalid configuration)
- Port conflicts or other startup issues occur

The window will only display your app once the server is confirmed to be accepting HTTP requests, eliminating "connection refused" errors during startup.

**Technical details:**
The health check polls `/ivy/health` every 250ms with a 2-second request timeout. The check runs on the main thread before the WebView is navigated, ensuring synchronous startup flow with proper error handling.

No code changes needed - this reliability improvement applies automatically to all Ivy desktop applications.

---

### Graceful Handling of Missing Assembly References

The framework now gracefully handles situations where your application references assemblies that aren't deployed, preventing crashes during assembly scanning operations.

**What changed:**

- Assembly scanning operations (for apps, widgets, connections, and extensions) now use a new `GetLoadableTypes()` extension method
- When an assembly references other assemblies that aren't available (e.g., optional packages like `Ivy.Filters`), the framework loads only the types that are available
- Previously, these situations would throw a `ReflectionTypeLoadException` and crash your application
- This affects automatic discovery of: app classes, external widgets, database connections, and extension methods

**Impact:**
You can now deploy applications without including every referenced assembly. For example, if your project references `Ivy.Filters` during development but doesn't deploy it to production, the app will start successfully instead of crashing on startup.

This is particularly useful for:

- Deploying minimal production builds without optional dependencies
- Development scenarios where not all packages are installed
- Modular applications where certain features are conditionally deployed

No code changes needed - this protection applies automatically throughout the framework.

---

### NumberInput Min/Max Validation

You can now set minimum and maximum values on `NumberInput` widgets directly through optional parameters or fluent methods, providing built-in validation for numeric inputs.

**Direct parameters:**

```csharp
var count = UseState(1);
count.ToNumberInput(min: 1, max: 100).Placeholder("Enter count");
```

**Fluent methods:**

```csharp
var count = UseState(1);
count.ToNumberInput()
    .Min(1)
    .Max(100)
    .Placeholder("Enter count");
```

**What changed:**

- `ToNumberInput()` now accepts optional `min` and `max` parameters
- The `.Min()` and `.Max()` fluent extension methods provide an alternative syntax
- Works with all number input variants (standard, slider, etc.)

**Impact:**
Setting validation bounds on number inputs is now more convenient and requires less code. The HTML `<input type="number">` element's native min/max attributes will be set automatically, providing client-side validation.

---

### Badge Hover Effects Only Show for Clickable Badges

Badge widgets now only display hover effects when they're actually clickable, preventing confusing UI feedback on non-interactive badges.

**What changed:**

- Badges without an `OnClick` handler no longer show hover effects (color changes on mouse-over)
- Clickable badges continue to show hover feedback with a subtle opacity change and pointer cursor
- The behavior is automatically determined based on whether you've registered a click handler

**Impact:**
Your badge UI will now be more consistent and intuitive - users won't see visual feedback suggesting interactivity on badges that don't respond to clicks. This is a visual polish improvement that makes the UI feel more professional and predictable.

**Example:**

```csharp
// Non-clickable badge - no hover effect
Badge("Status: Active").Variant(BadgeVariant.Success);

// Clickable badge - shows hover effect
Badge("Click me").OnClick(() => DoSomething());
```

No code changes needed - this improvement applies automatically to all Badge widgets.

---

### SelectInput: Auto-Flip Dropdown Near Viewport Edge

The `SelectInput` dropdown now automatically detects when it would extend beyond the bottom of the viewport and intelligently flips to open upward instead. This prevents the dropdown from being cut off when the input is positioned near the bottom of the screen.

**What changed:**

- The dropdown calculates available space below the trigger element when opening
- If insufficient space exists (less than the dropdown height + 8px), it automatically opens upward
- The dropdown smoothly positions itself above the input with appropriate spacing
- Works seamlessly across all SelectInput variants (Select, List, Toggle, etc.)

**Impact:**
Users can now reliably interact with SelectInput dropdowns regardless of where they're positioned on the page. Previously, dropdowns near the bottom of the viewport would extend off-screen, forcing users to scroll to see all options. The dropdown now intelligently adapts to available space, ensuring all options remain visible and accessible.

**Example scenarios:**

- SelectInput in a modal dialog near the bottom
- Form fields at the end of a long page
- Dropdowns in fixed-position sidebars or toolbars
- Any SelectInput where the trigger is close to the viewport bottom

No code changes needed - this improvement applies automatically to all SelectInput widgets.

---

### Dynamic Metric Progress Colors

The `MetricView` component now intelligently colors its progress bar based on achievement percentage, providing instant visual feedback about goal performance.

**What changed:**

- Progress bars now use color-coded thresholds based on goal achievement
- Replaces the previous fixed amber color with dynamic colors that reflect performance

**Color thresholds:**

```csharp
// The colors automatically adjust based on goal achievement:
// ≥ 75% → Green (Success)    - Excellent progress
// ≥ 50% → Yellow (Warning)   - Good progress
// ≥ 25% → Orange             - Needs attention
// < 25% → Red (Destructive)  - Urgent attention needed
```

**Impact:**
Dashboard metrics now provide immediate, intuitive visual feedback. Users can instantly identify which metrics are performing well (green), need attention (yellow/orange), or require urgent focus (red) without reading the exact percentage. This makes scanning multiple metrics significantly faster and more effective.

No code changes needed - this improvement applies automatically to all `MetricView` components in your dashboards.

---



### Customizable Confirm Dialogs with Destructive Styling

The `WithConfirm` method now supports customizable button labels and destructive styling, allowing you to create more appropriate confirmation dialogs for delete operations and other destructive actions.

**New parameters:**

```csharp
// Basic confirmation (default "Ok" button)
new Button("Delete")
    .Variant(ButtonVariant.Destructive)
    .WithConfirm("Are you sure you want to delete this item?");

// Custom label with destructive styling
new Button("Delete")
    .Variant(ButtonVariant.Destructive)
    .WithConfirm(
        message: "Are you sure you want to delete this category?",
        title: "Delete Category",
        confirmLabel: "Delete",
        destructive: true
    );

// Custom label without destructive styling
new Button("Proceed")
    .WithConfirm(
        message: "This will update all records. Continue?",
        title: "Confirm Update",
        confirmLabel: "Update All"
    );
```

**What changed:**

- Added optional `confirmLabel` parameter to customize the confirm button text (defaults to "Ok")
- Added optional `destructive` parameter to style the confirm button with destructive (red) styling
- When `destructive: true`, the confirm button uses `ButtonVariant.Destructive` for visual emphasis
- When `destructive: false` or omitted, the confirm button uses the standard `ButtonVariant.Primary`

**Impact:**
Confirmation dialogs can now better match their purpose. Delete operations can show a red "Delete" button instead of a generic "Ok" button, making the UI more intuitive and reducing the risk of accidental destructive actions through clearer visual communication.

---

### ListItem Improved Vertical Spacing

The `ListItem` widget now includes vertical padding for better content spacing and visual comfort.

**What changed:**

- Added `py-2` (vertical padding) to the ListItem component's styling
- Content within list items now has more breathing room
- The minimum height of 60px is maintained for consistent sizing

**Impact:**
List items now have improved visual hierarchy and readability. The additional vertical padding creates better separation between the content and the list item boundaries, making lists feel less cramped and more polished. This applies automatically to all `ListItem` widgets without requiring any code changes.

---

### DefaultSidebarChrome Auto-Opens When Last Tab is Closed

The `DefaultSidebarChrome` now automatically opens the sidebar when you close the last tab, preventing users from being left without visible content or navigation options.

**What changed:**

- When the last tab is closed, the sidebar automatically expands
- The sidebar state is now managed dynamically instead of using a fixed initial value
- Users always have access to navigation after closing all tabs

**Impact:**
This improvement prevents a confusing state where closing the last tab would leave the application with no visible UI elements. The sidebar automatically opens to provide navigation options, making the user experience more intuitive and consistent. This behavior works automatically in applications using `DefaultSidebarChrome`.

---

### Size.Fraction and Size.FractionGap Now Accept Decimal and Double

The `Size.Fraction()` and `Size.FractionGap()` methods now accept `decimal` and `double` values in addition to `float`, eliminating common type mismatch errors when using numeric literals or variables.

**What changed:**

```csharp
// Before: Required float literals or explicit casting
new Box()
    .Width(Size.Fraction(0.5f));  // Had to use 'f' suffix

decimal ratio = 0.75m;
new Box()
    .Width(Size.Fraction((float)ratio));  // Had to cast

// After: Works with double and decimal directly
new Box()
    .Width(Size.Fraction(0.5));  // Double literal works

decimal ratio = 0.75m;
new Box()
    .Width(Size.Fraction(ratio));  // No casting needed

// Same applies to FractionGap
Layout.Horizontal()
    .Gap(Size.FractionGap(0.25));  // Double works
```

**Why this matters:**

This removes a common friction point when working with fractional sizes. Since C# numeric literals default to `double` (not `float`), developers previously had to either add the `f` suffix to every literal or explicitly cast their values. The new overloads handle the conversion automatically, making the API more intuitive and reducing boilerplate code.

**Technical details:**

The overloads internally convert `decimal` and `double` to `float` before creating the `Size` instance, maintaining the existing behavior while accepting more input types.

---

### Desktop Windows Now Show Ivy Icon by Default

Desktop applications built with Ivy.Desktop now automatically display the Ivy framework icon in the taskbar and title bar when no custom icon is specified. This gives your desktop apps a professional, polished appearance out of the box.

**What changed:**

The Ivy icon (`ivy.ico`) is now embedded as a resource in Ivy.Desktop and automatically applied when you call `Run()` on a `DesktopWindow`, unless you've explicitly set a custom icon using `.Icon()`.

```csharp
// Your desktop app now shows the Ivy icon automatically
new DesktopWindow(server)
    .Title("My App")
    .Size(1200, 800)
    .Run();  // Ivy icon appears in taskbar and title bar
```

**Custom icons still work:**

If you want to use your own icon, the `.Icon()` method works exactly as before:

```csharp
new DesktopWindow(server)
    .Title("My App")
    .Icon<Program>("MyApp.icon.ico")  // Your custom icon
    .Run();
```

**Why this matters:**

Previously, desktop apps would show a generic system icon unless you explicitly embedded and configured a custom icon. Now, your Ivy desktop applications have a branded, professional appearance by default, making the framework immediately recognizable while still allowing full customization when needed.

---

## Bug Fixes

### CLI Commands Port Conflict Resolution

Fixed a port conflict issue where CLI-only commands (`--describe`, `--describe-connection`, `--test-connection`) would fail if an Ivy application was already running on the configured port. These commands need dependency injection but never actually start the web host, making port binding unnecessary.

**What was fixed:**

- CLI commands now bind to port 0 (ephemeral port) instead of the configured application port
- Port-in-use checks are completely skipped for CLI commands
- No more conflicts when running CLI commands alongside a running instance

**Impact:**
If you use CLI commands like `--describe` to inspect your database schema, `--describe-connection` to test connection strings, or `--test-connection` to verify database connectivity, these commands will now work seamlessly even when your Ivy application is running. Previously, you would get port conflict errors forcing you to stop your running application before using these diagnostic commands.

---

### Form Submit Strategy Hook Ordering

Fixed a critical bug in `FormBuilder` where changing the form submit strategy at runtime would cause an `InvalidOperationException`. The issue occurred when using `OnBlur` or `OnChange` strategies, as internal hooks were called conditionally, violating hook ordering rules.

**What was fixed:**

- Hook indices no longer shift when the form submit strategy changes
- Hooks are now called consistently regardless of the submit strategy
- Forms with dynamic strategy switching now work reliably without crashes

**Impact:**
If you use `FormBuilder` with `.SubmitStrategy(FormSubmitStrategy.OnBlur)` or `.SubmitStrategy(FormSubmitStrategy.OnChange)`, or if you dynamically switch between strategies, your forms will now work correctly without encountering hook ordering exceptions.

---

### RichTextBlock Stream Subscription Fix

Fixed a bug in `RichTextBlock` where streaming text content would fail to display. The frontend component expected the `stream` property to be a plain string, but the backend serializer was producing an object with an `id` property (`{ id: "..." }`), causing stream subscriptions to silently fail.

**What was fixed:**

- The `RichTextBlockWidget` now correctly handles the stream object format from the serializer
- Stream subscriptions now work reliably for real-time text updates

**Impact:**
If you're using the `RichTextBlock` streaming feature (documented above), streams will now properly display incoming text chunks in real-time.

---

### NumberInput Currency Format Default

Fixed a runtime `TypeError` that occurred when using `.FormatStyle(NumberFormatStyle.Currency)` on `NumberInput` without explicitly setting a currency code. The framework now automatically defaults to "USD" when no currency is specified.

**What was fixed:**

- `NumberInput` with `Currency` format style no longer crashes at runtime
- Currency code automatically defaults to "USD" if not explicitly set
- Works seamlessly with method chaining after `ToNumberInput()`

**Example:**

```csharp
// This now works without errors
var input = new NumberInput()
    .Value(100.50)
    .FormatStyle(NumberFormatStyle.Currency); // Automatically uses USD

// You can still override with a specific currency
var euroInput = new NumberInput()
    .Value(100.50)
    .FormatStyle(NumberFormatStyle.Currency)
    .Currency("EUR");
```

**Impact:**
If you were experiencing crashes when using currency formatting on number inputs, this fix resolves the issue. The default USD currency provides sensible behavior while still allowing you to specify other currencies when needed.

---

### Stream Data Serialization Fix

Fixed a bug where streamed data (like `TextRun` objects in `RichText`) was not properly serialized when sent to the client. Stream data was passed as raw objects through SignalR, bypassing the camelCase naming policy and enum converters used by `WidgetSerializer`, causing property names and enum values to be incorrectly formatted on the client side.

**What was fixed:**

- Stream data is now pre-serialized using `WidgetSerializer.SerializerOptions`
- OnSubscribed buffer flush now uses the same serialization as regular Write() calls
- Property names are correctly converted to camelCase
- Enum values are properly formatted for the client
- Streamed complex objects now have consistent serialization with other widget data

**Impact:**
If you use streaming features (like `UseStream`) with complex objects, particularly `RichText` with `TextRun` objects, the data will now be correctly formatted when received by the client. This fix is especially important for buffered data - if stream data arrives before your frontend component subscribes (a common scenario during component mounting), it will now be properly serialized when flushed to your handler. This ensures consistent behavior between streamed and non-streamed widget data, and between regular stream writes and buffered data.

---

### Stream Data Buffering - Preventing Dropped Messages

Fixed a race condition where `StreamData` messages could be dropped if they arrived before the frontend stream handler was ready. This could happen during React re-render cycles when stream data arrives while the component is still mounting or updating.

**What was fixed:**

- Frontend now buffers stream data that arrives before the handler registers
- When `useStream` subscribes, any buffered data is immediately flushed to the handler
- Stream data is no longer lost during component lifecycle events
- Mirrors the backend buffering behavior for consistent reliability

**Impact:**
If you're using `UseStream` for real-time data streaming (like chat messages, AI responses, or live updates), you'll no longer experience dropped messages during component re-renders or initial mounting. All stream data is now guaranteed to be delivered to your handlers, even if it arrives slightly before the component is ready to receive it.

---

### HtmlPipeline XML Parsing Fix for Vite-Generated HTML

Fixed a parsing issue in `HtmlPipeline` where void HTML elements (like `<link>`, `<meta>`, `<br>`, etc.) generated by Vite without self-closing slashes would cause `XDocument.Parse` to fail. While these elements are valid HTML5, XML parsing requires them to be self-closed.

**What was fixed:**

- Added automatic normalization that converts void HTML elements to self-closing form before XML parsing
- Supports all standard void elements: `link`, `meta`, `br`, `hr`, `img`, `input`, `source`, `track`, `wbr`, `col`, `area`, `base`, `embed`
- Mixed self-closed and non-self-closed elements are now handled correctly

**Impact:**
If you're using Vite as your build tool with Ivy, you'll no longer encounter parsing errors when the framework processes your `index.html` file. Vite's default HTML output with non-self-closed `<link rel="modulepreload">` and `<link rel="stylesheet">` tags will now work seamlessly with Ivy's HtmlPipeline.

---

### HtmlPipeline XML Parsing with Modern Build Tools

Fixed a bug in `HtmlPipeline` where HTML generated by modern build tools like Vite would cause XML parsing failures. Vite and other tools generate void HTML elements (`<link>`, `<meta>`, `<br>`, `<hr>`, `<img>`, etc.) without self-closing slashes, which is valid HTML5 but incompatible with `XDocument.Parse`.

**What was fixed:**

- The `HtmlPipeline` now automatically normalizes void HTML elements to self-closing form before XML parsing
- Works with HTML generated by Vite, including `<link rel="modulepreload">` and `<link rel="stylesheet">` tags
- Handles mixed self-closed and non-self-closed void elements correctly

**Impact:**
If you're using Vite or other modern build tools that generate HTML5-compliant void elements, the `HtmlPipeline` will now process your HTML without parsing errors. This fix ensures compatibility with standard build tool output without requiring manual HTML modifications.

---

### ClientSender Disposal Race Condition

Fixed a race condition that could occur when a client connection is closed while event handlers are still processing. Previously, the `ClientSender` could be torn down before in-flight event handlers finished executing, potentially causing errors or lost messages during disconnection.

**What was fixed:**

- App state is now disposed before the `ClientSender` is torn down, ensuring in-flight event handlers complete first
- Added a disposal guard in `ClientSender.Send` to prevent attempts to write after disposal
- Eliminated race conditions during connection cleanup

**Impact:**
If you experienced errors or warnings in your logs when users disconnect (especially during active event processing), these should now be resolved. The framework now ensures a clean shutdown sequence where all pending work completes before communication channels are closed.

---

### Chart Toolbox Overlap Fix

Fixed a visual bug where chart content (area charts, bar charts, and line charts) would overlap with the toolbox controls when the toolbox was enabled. The chart grid now properly adjusts its top spacing to accommodate the toolbox.

**What was fixed:**

- Charts now reserve appropriate space at the top when a toolbox is present
- Toolbox positioning is more precise to prevent overlap
- All chart types (AreaChart, BarChart, LineChart) now handle toolbox spacing consistently

**Impact:**
If you're using charts with toolboxes enabled (for features like data zoom, download, or restore), the chart content will no longer visually overlap with the toolbox controls, providing a cleaner and more professional appearance.

---

### Missing HttpClient Dependency Fix

Fixed a compilation error (CS1061) that occurred when using `server.Services.AddHttpClient()`. The Ivy package now includes `Microsoft.Extensions.Http` as a transitive dependency.

**What was fixed:**

- Added `Microsoft.Extensions.Http` package reference to Ivy
- `AddHttpClient()` extension method is now accessible without manual package installation

**Impact:**
If you're using `AddHttpClient()` to register HTTP client services in your Ivy application, you no longer need to manually add the `Microsoft.Extensions.Http` package to your project. This dependency is now automatically included.

---

### XAML Builder Sample - StackLayout Gap Property

Fixed incorrect property usage in the XAML Builder sample application where `StackLayout` examples were using a non-existent `Gap` property. The examples now correctly use `RowGap` and `ColumnGap` properties.

**What was fixed:**

- "Buttons" example now uses `RowGap="2" ColumnGap="2"` instead of `Gap="2"`
- "Nested Layout" example now uses proper `RowGap` and `ColumnGap` properties throughout

**Impact:**
If you were learning from the XAML Builder sample or copying its code, the examples now demonstrate the correct property names for spacing in `StackLayout`. This aligns with the framework's actual API where row and column gaps are controlled independently.

---

### TableBuilder.Remove() - Non-Displayable Property Types

Fixed a `KeyNotFoundException` that occurred when calling `.Remove()` on `TableBuilder` for properties that cannot be displayed in tables, such as `byte[]` arrays. These properties are never registered as columns in the table, so attempting to remove them would throw an exception.

**What was fixed:**

- `TableBuilder.Remove()` now silently skips properties that aren't registered as columns
- No longer throws `KeyNotFoundException` for non-displayable property types like `byte[]`
- Uses `TryGetValue` internally to gracefully handle missing column entries

**Example:**

```csharp
// This no longer throws an exception
var table = new DataTableBuilder<User>()
    .Remove(u => u.ProfilePicture); // byte[] property - silently skipped
```

---

### Table of Contents - Smooth Scrolling Without Visual Glitches

Fixed visual glitches in the `TableOfContents` widget that would cause incorrect highlighting and "junk" to appear when users scroll quickly through content. The component now uses a debounced update mechanism to ensure smooth, accurate highlighting even during rapid scrolling.

**What was fixed:**

- Implemented 120ms debounce delay before updating the active TOC heading
- TOC now correctly identifies and listens to the actual scroll container instead of assuming `window`
- Eliminated visual jitter and incorrect highlighting during fast scroll operations
- Fixed auto-scroll behavior to keep the active TOC item visible

**Impact:**
If you're using the `TableOfContents` widget in your Ivy applications (commonly used for documentation, articles, or long-form content), users will now experience smooth, accurate highlighting as they scroll through content. Fast scrolling no longer causes the TOC to flicker between multiple active states or show incorrect highlights.

**Impact:**
If you're using `TableBuilder.Remove()` to exclude certain properties from your tables, you can now safely call it on any property without worrying about whether it's displayable. Properties that weren't going to show up anyway (like byte arrays) are now gracefully ignored instead of throwing exceptions.

---

### IState<T>.Set(null) Ambiguity Resolved

Fixed a compiler ambiguity error that occurred when calling `.Set(null)` on state objects with nullable reference types. Previously, passing `null` to `.Set()` would match both the `Set(T value)` and `Set(Func<T,T> setter)` overloads, causing a compilation error.

**What was fixed:**

- Added `[OverloadResolutionPriority(1)]` attribute to `Set(T value)` overload in `IState<T>`, `State<T>`, `ConvertedState<TFrom,TTo>`, and `MockState<T>`
- The compiler now correctly prefers `Set(T value)` over `Set(Func<T,T>)` when `null` is passed
- No more ambiguity errors when setting nullable state values to null

**Example:**

```csharp
// This now works without compiler errors or explicit casting
var selectedItem = UseState<InventoryItem?>(null);
selectedItem.Set(null); // ✅ Compiles cleanly

// Previously required casting to resolve ambiguity
// selectedItem.Set((InventoryItem?)null);
```

**Impact:**
If you're managing nullable state values and need to reset them to `null`, you can now call `.Set(null)` directly without casting or wrapping in a lambda. This makes state management code cleaner and more intuitive.

---

### Desktop Window Title Default

Fixed the default window title for Ivy desktop applications. Instead of showing a generic "Ivy App" title, desktop windows now automatically display the application's assembly name, providing a more professional and context-appropriate default.

**What was fixed:**

- Desktop window title now defaults to the entry assembly name (e.g., "MyApplication" for MyApplication.exe)
- Falls back to "Ivy App" only if the assembly name cannot be determined
- No code changes required - existing applications automatically benefit from this improvement

**Impact:**
If you're building desktop applications with `Ivy.Desktop`, your application windows will now show a meaningful title by default without needing to explicitly call `.Title()`. This provides a better out-of-the-box experience, though you can still customize the title using the `.Title()` method when needed:

```csharp
// Automatically shows your assembly name as the window title
new DesktopWindow(server);

// Or customize it explicitly
new DesktopWindow(server).Title("My Custom App Name");
```

---

### Hook Usage Analyzer: FuncView and MemoizedFuncView Lambda Support

Fixed the hook usage analyzer (`IVYHOOK001`) to correctly recognize lambdas passed to `FuncView` and `MemoizedFuncView` constructors as valid locations for hooks. Previously, the analyzer would incorrectly flag these as errors, even though these lambdas function as Build methods.

**What was fixed:**

- `FuncView` and `MemoizedFuncView` lambdas are now treated as valid Build method contexts
- Hooks can be called inside these lambdas without triggering false analyzer errors
- All other hook rules (no conditionals, no loops, must be at top) still apply within the lambdas

**Example:**

```csharp
// ✅ This now works without analyzer errors
public override object? Build()
{
    return new FuncView(context =>
    {
        var state = UseHelper(context);  // No longer flagged as IVYHOOK001
        return new Text($"Value: {state}");
    });
}

private static object UseHelper(IViewContext context) => context.UseState(0);
```

**Impact:**
If you're using `FuncView` or `MemoizedFuncView` with hooks inside their lambdas, you'll no longer get false positive analyzer errors. This enables cleaner inline widget composition with state management, particularly useful for functional-style widget building where you want to encapsulate state logic within the view factory.

---

### Table of Contents - Smooth Scrolling Fix

Fixed a visual bug in the `TableOfContents` widget where the active heading indicator would flicker or show incorrect values during fast scrolling. The TOC now updates smoothly with a debounced approach that prevents jittery behavior.

**What was fixed:**

- Added 120ms debounce to prevent excessive updates during rapid scrolling
- Improved scroll container detection to listen on the correct scrolling element
- Fixed auto-scrolling of the TOC itself to keep the active item visible
- Eliminated visual "junk" or flickering that appeared when quickly scrolling through long articles

**Impact:**
If you're using the `TableOfContents` widget in your articles or documentation pages, you'll notice much smoother behavior when scrolling through content. The active heading indicator now updates cleanly without flickering, providing a better reading experience for your users.

---

### Chart Legend Title-Casing Fix

Fixed improper capitalization in chart legends when using camelCase property names. The `SplitPascalCase` utility now properly title-cases each word, producing professional-looking chart labels.

**What was fixed:**

- `SplitPascalCase` now capitalizes the first letter of each word
- Chart legends now display "Avg Salary" instead of "avg Salary" for properties like `avgSalary`
- Removed redundant frontend capitalize hack in `PieChartWidget`

**Example:**

```csharp
// Property names in your data classes
public class SalesData
{
    public decimal avgSalary { get; set; }     // Now displays as "Avg Salary"
    public int totalCount { get; set; }        // Now displays as "Total Count"
    public decimal minValue { get; set; }      // Now displays as "Min Value"
}
```

**Impact:**
If you're using pie charts, bar charts, or any other charts that automatically generate labels from property names, the labels will now be properly title-cased, providing a more polished and professional appearance. This fix ensures consistent capitalization across all chart types without requiring manual label customization.

---

### Semantic Color Mapping for Text

Fixed incorrect color mapping when using semantic surface colors (like `Colors.Muted`, `Colors.Background`, `Colors.Card`) for text. These colors were previously mapping to their base CSS variables, which are designed for backgrounds, resulting in poor readability and contrast issues.

**What was fixed:**

- Surface theme colors (`Background`, `Card`, `Popover`, `Muted`, `Accent`) now automatically map to their foreground variants when used for text
- `.Color(Colors.Muted)` now correctly applies `muted-foreground` instead of `muted`
- Brand/state colors (`Primary`, `Secondary`, `Destructive`) continue to use their base variables as intended

**Example:**

```csharp
// This now applies the correct foreground color with proper contrast
Text.P("Muted text")
    .Color(Colors.Muted);  // Uses muted-foreground, not muted

// Other semantic colors work correctly too
Text.P("Subtle text")
    .Color(Colors.Accent);  // Uses accent-foreground
```

**Impact:**
If you're using semantic surface colors for text styling, your text will now have proper contrast and readability. The framework automatically selects the appropriate foreground variant to ensure text remains legible against the theme's background.

---

### SidebarLayout - Respect Open Property on Mount

Fixed a bug in `SidebarLayoutWidget` where the `.Open(false)` property was being overridden by the media query handler on component mount. When a sidebar was explicitly set to closed, the auto-collapse media query would incorrectly force it open if the viewport was wide enough.

**What was fixed:**

- Media query handler now respects the `open` property when determining sidebar state
- Sidebar stays closed when `.Open(false)` is set, regardless of viewport width
- The `openProp` is now included in the useEffect dependency array for correct reactivity

**Example:**

```csharp
// The sidebar now correctly stays closed, even on wide viewports
new SidebarLayoutWidget()
    .Open(false)  // This is now properly respected
    .Sidebar(new Text("Sidebar content"))
    .MainContent(new Text("Main content"));
```

**Impact:**
If you're using `SidebarLayoutWidget` with `.Open(false)` to start with a closed sidebar, it will now correctly remain closed on mount, regardless of the viewport size. Previously, wide viewports would override this setting and show the sidebar anyway.

---

### MarkdownRenderer Code Block Borders

Fixed a visual bug in the `MarkdownRenderer` where code blocks were missing their borders. The `ScrollArea` component wrapping the syntax highlighter was missing the border styling classes that were present in the fallback block, causing rendered code blocks to appear without visible borders.

**What was fixed:**

- Code blocks in markdown now display with proper borders and rounded corners
- Consistent styling between scrollable and non-scrollable code blocks
- Visual appearance matches the intended design

**Impact:**
If you're using markdown rendering in your application (via `MarkdownRenderer` component), code blocks will now properly display with borders, providing better visual separation from surrounding content and a more polished appearance.

---

### Clerk Authentication - Graceful Existing Session Handling

Fixed an issue in the Clerk authentication provider where sign-in attempts would fail if an active session already existed. This commonly occurred when users tried to sign in while already having a valid but undetected session, causing authentication errors instead of seamlessly restoring or replacing the existing session.

**What was fixed:**

- Password sign-in now attempts to restore existing sessions before failing
- If session restoration fails, all sessions are automatically cleared and sign-in proceeds
- OAuth sign-in gracefully clears conflicting sessions and retries
- No more `session_exists` errors blocking user authentication

**Impact:**
If you're using Clerk authentication (`Ivy.Auth.Clerk`) in your application, users will no longer encounter authentication failures when attempting to sign in with an existing session. The framework now intelligently handles these situations by either restoring the existing session or clearing it to allow fresh sign-in, providing a smoother authentication experience without requiring manual session cleanup.

---

### Desktop Error Dialog Display Fix

Fixed a bug in `Ivy.Desktop` where application error dialogs would display as blank white windows instead of showing the formatted error message and stack trace. The issue was caused by using `LoadRawString()` to load the error HTML, which doesn't render properly in Photino.

**What was fixed:**

- Error dialogs now write HTML to a temporary file and load it via `Load(Uri)` instead of `LoadRawString()`
- This matches the approach used for the main application window, ensuring consistent rendering
- Temporary error HTML files are automatically cleaned up after the error dialog closes
- Error dialogs now correctly display the formatted error message and stack trace

**Impact:**
If you're building desktop applications with `Ivy.Desktop`, unhandled exceptions will now display properly formatted error dialogs with readable error messages and stack traces, instead of showing blank white windows. This makes debugging and error reporting significantly easier during development and for end users.

---

### Desktop WebView2 Blank Window Fix

Fixed a critical bug in `Ivy.Desktop` on Windows where desktop applications using WebView2 would open but display completely blank content. The issue was caused by WebView2's COM message pump requiring Single-Threaded Apartment (STA) threading, which wasn't guaranteed when `DesktopWindow.Run()` was called.

**What was fixed:**

- `DesktopWindow.Run()` now automatically checks the current thread's apartment state on Windows
- If not already on an STA thread, the window automatically starts on a new STA thread
- WebView2 now receives the proper threading context it needs for rendering
- Desktop applications now display content correctly instead of blank windows

**Impact:**
If you're building Windows desktop applications with `Ivy.Desktop`, your applications will now render properly instead of showing blank white windows. This fix is automatic and requires no code changes - simply update to this version and your existing desktop apps will work correctly on Windows.

---

### Better Desktop Startup Error Messages

Fixed a bug in `Ivy.Desktop` where server startup failures would display a generic "Unable to connect" error message instead of showing the actual exception that caused the server to fail. This made debugging startup issues difficult, as the root cause was hidden.

**What was fixed:**

- `CheckIfPortIsListening` now monitors the server task status during port polling
- If the server task faults during startup, the actual exception is now thrown and displayed
- Error dialogs now show the real cause of server failures (e.g., port already in use, configuration errors)
- Unwraps `AggregateException` to expose the underlying startup exception

**Impact:**
If you're building desktop applications with `Ivy.Desktop`, you'll now see clear, actionable error messages when the server fails to start, rather than a vague "Unable to connect" message. This makes debugging server configuration issues, port conflicts, and other startup problems much easier during development.

---

### Assembly Scanning - Missing Reference Resilience

Fixed a crash that occurred during application startup when the framework scanned assemblies that referenced optional dependencies that weren't deployed. Previously, calling `Assembly.GetTypes()` on assemblies with missing references would throw a `ReflectionTypeLoadException`, causing the entire application to crash.

**What was fixed:**

- Added `GetLoadableTypes()` extension method that gracefully handles missing assembly references
- Framework now loads only the types that are available, skipping types with unresolved dependencies
- Applied to all assembly scanning operations: app discovery, external widgets, connections, secret providers, and extension methods
- Applications no longer crash when optional assemblies like `Ivy.Filters` aren't deployed

**Impact:**
Your Ivy applications are now more resilient to partial deployments where not all referenced assemblies are present. This is particularly useful for modular applications where certain features (and their dependencies) may be optionally deployed. The framework will successfully discover and use all available types while gracefully skipping any that depend on missing assemblies, allowing your application to start and run normally instead of crashing at startup.

---

### Desktop Window Server Readiness Check

Fixed race conditions in `Ivy.Desktop` where the desktop window could attempt to navigate to the server before it was ready to accept requests. This could lead to blank windows, connection errors, or missed error messages when the server failed to start properly.

**What was fixed:**

- Desktop window now polls the `/ivy/health` endpoint before navigating, ensuring the server is accepting requests
- Port is read after `RunAsync()` returns, guaranteeing the actual bound port is used (important when using port 0 for auto-assignment)
- Early server faults are now detected and surfaced immediately instead of appearing as connection timeouts
- Server startup failures (missing secrets, configuration errors, etc.) are caught before the window loads

**Impact:**
If you're building desktop applications with `Ivy.Desktop`, your applications will start more reliably. The desktop window will wait for the server to be fully ready before attempting to load, preventing blank windows or "connection refused" errors during startup. If the server fails to start for any reason, you'll see the actual error immediately rather than waiting for a timeout. This is especially helpful during development when configuration issues or missing secrets might prevent server startup.

---

### Badge Hover Effect Only for Clickable Badges

Fixed misleading hover effects on badges by only showing hover feedback when a badge has an `OnClick` handler registered. Previously, all badges displayed hover effects regardless of whether they were interactive, creating a confusing user experience.

**What was fixed:**

- Hover effects (darkening on mouse over) now only appear when `.OnClick()` is registered
- Non-interactive badges no longer show visual feedback suggesting they're clickable
- The `onClick` handler is now properly conditional based on whether the badge is clickable

**Example:**

```csharp
// This badge is interactive and shows hover effect
new Badge("Click me")
    .OnClick(() => { /* action */ });  // ✅ Has hover effect

// This badge is static and no hover effect
new Badge("Status: Active");  // ✅ No hover effect
```

**Impact:**
If you're using badges in your application, the visual feedback now accurately reflects whether a badge is interactive or purely informational. This improves user experience by preventing users from attempting to click non-interactive badges that displayed misleading hover states.

---

### Outline Button Missing Background

Fixed a visual bug where outline variant buttons were missing their background color, causing transparency issues and inconsistent appearance depending on the content behind them.

**What was fixed:**

- Outline buttons now have an explicit `bg-background` color applied
- Buttons maintain proper opacity and visual consistency across all contexts
- No more unintended transparency showing through outline buttons

**Example:**

```csharp
// Outline buttons now have proper background styling
new Button("Submit")
    .Variant(ButtonVariant.Outline);
```

**Impact:**
If you're using outline variant buttons in your application, they will now display with the correct background color, ensuring they remain visible and properly styled regardless of the content positioned behind them. This provides more consistent and reliable button styling across your UI.

---

### Semantic Color Text Readability Fix

Fixed a bug where using semantic surface colors (like `Colors.Muted`, `Colors.Background`, `Colors.Card`) as text colors would result in poor readability. These colors are designed as background layers, but when applied to text, they should automatically map to their foreground variants.

**What was fixed:**

- Text colored with `Colors.Muted` now properly uses `--muted-foreground` instead of `--muted`
- Similarly, `Colors.Background`, `Colors.Card`, `Colors.Popover`, and `Colors.Accent` all map to their foreground variants when used as text color
- Brand and state colors (`Colors.Primary`, `Colors.Secondary`, `Colors.Destructive`) remain unchanged as their base variables are already intended for text
- Ensures consistent readability across all semantic color usage

**Example:**

```csharp
// Now properly maps to readable foreground color
Text.P("Muted text").Color(Colors.Muted);

// Background colors remain unchanged
new Box("Content").Background(Colors.Muted);
```

**Impact:**
If you're using semantic surface colors like `Colors.Muted` on text elements, they will now display with proper contrast and readability. The framework automatically selects the appropriate foreground variant, ensuring your text is visible against its background without requiring manual color adjustments.

---

## Developer Experience Improvements

### Compile-Time Analyzer for App Constructor Requirements

A new Roslyn analyzer (`IVYAPP001`) now provides compile-time feedback when `[App]`-attributed classes don't have a parameterless constructor, catching this common mistake before runtime.

**What It Catches:**

The analyzer flags App classes that use constructor injection, which isn't supported because Ivy instantiates apps via `Activator.CreateInstance`:

```csharp
// ❌ This now shows a compile error
[App]
public class MyApp : AppBase
{
    private readonly IClientProvider _client;

    public MyApp(IClientProvider client)  // IVYAPP001 error here
    {
        _client = client;
    }

    public override object Build() => ...;
}
```

**The Fix:**

The error message guides you to use `UseService<T>()` inside the `Build()` method instead:

```csharp
// ✅ Correct approach
[App]
public class MyApp : AppBase
{
    public override object Build()
    {
        var client = UseService<IClientProvider>();
        // Use client...
    }
}
```

**Additional Safety:**

- Generic methods `Server.UseChrome<T>()` and `Server.UseErrorNotFound<T>()` now have `new()` constraints for compile-time safety
- Runtime validation in `AppDescriptor.CreateApp()` provides a clear error message if issues are missed

This change helps you follow Ivy's dependency injection patterns correctly and catch mistakes early in development.

---

### Compile-Time Analyzer for Widget Child Misuse

New Roslyn analyzers (`IVYCHILD001`, `IVYCHILD002`, and `IVYCHILD003`) now catch widget child misuse at compile time, preventing runtime `NotSupportedException` errors when adding children to incompatible widgets.

**IVYCHILD001 - Leaf Widgets Don't Support Children:**

The analyzer flags attempts to add children to leaf widgets that don't support any children:

```csharp
// ❌ Compile error - Button doesn't support children
var result = new Button("Click") | new Text("child");

// ❌ Compile error - Badge doesn't support children
var result = new Badge("Status") | "child";

// ❌ Compile error - Input widgets don't support children
var input = new TextInput();
var result = input | "child";
```

**Affected Leaf Widgets:**

- **UI Components:** `Button`, `Badge`, `Progress`, `Field`, `Detail`, `Tooltip`
- **Inputs:** All widgets implementing `IInput<T>` (TextInput, SelectInput, NumberInput, etc.)
- **Layouts:** `Dialog`, `DialogHeader`, `HeaderLayout`, `SidebarLayout`, `FooterLayout`, `SidebarMenu`
- **Charts:** `DataTable`, `LineChart`, `PieChart`, `BarChart`, `AreaChart`

**IVYCHILD002 - Single-Child Widgets (Warning):**

The analyzer warns when adding multiple children to widgets that only support one child:

```csharp
// ⚠️ Warning - Card only supports a single child
var card = new Card()
    | new Text("First child")
    | new Text("Second child");  // Warning here

// ✅ Correct - wrap multiple children in a layout
var card = new Card()
    | Layout.Vertical()
        | new Text("First child")
        | new Text("Second child");
```

**Single-Child Widgets:** `Card`, `Sheet`, `Confetti`, `FloatingPanel`

**IVYCHILD003 - Type-Restricted Children:**

The analyzer enforces type restrictions on widgets that only accept specific child types via the new `[ChildType]` attribute. This prevents mismatches at compile time rather than runtime.

```csharp
// ❌ Compile error - DropDownMenu only accepts MenuItem children
var menu = new DropDownMenu() | new Text("Invalid");

// ❌ Compile error - string children not allowed
var menu = new DropDownMenu() | "Invalid";

// ✅ Correct - MenuItem is the allowed type
var menu = new DropDownMenu()
    | new MenuItem("Option 1")
    | new MenuItem("Option 2");

// ✅ Also works with arrays and IEnumerable
var items = new MenuItem[] {
    new MenuItem("Option 1"),
    new MenuItem("Option 2")
};
var menu = new DropDownMenu() | items;
```

**The ChildType Attribute:**

Widget authors can now use `[ChildType(typeof(T))]` to specify allowed child types. The analyzer checks direct children, arrays, and `IEnumerable<T>` collections, supporting both exact type matches and derived types.

These analyzers work with direct widget instantiation, variables, method returns, and derived widget types, providing comprehensive compile-time safety for widget composition.

---

### Compile-Time Analyzer for Hook Results Stored in Class Members

A new Roslyn analyzer (`IVYHOOK006`) now detects when hook results are incorrectly stored in class fields or properties, preventing a subtle but critical bug where the reactive system breaks due to cached state.

**What It Catches:**

The analyzer flags when `Use*` hook results (like `UseState`, `UseMemo`, etc.) are assigned to class fields or properties instead of local variables:

```csharp
// ❌ This now shows a compile error
public class TestView : ViewBase
{
    private object? _count;  // Field

    public override object? Build()
    {
        _count = UseState(0);  // IVYHOOK006 error here
        return new Button("Click");
    }
}

// ❌ Also catches assignments with 'this.'
public class TestView : ViewBase
{
    private object? _count;

    public override object? Build()
    {
        this._count = UseState(0);  // IVYHOOK006 error here
        return new Button("Click");
    }
}

// ❌ Also catches property assignments
public class TestView : ViewBase
{
    public object? Count { get; set; }

    public override object? Build()
    {
        Count = UseState(0);  // IVYHOOK006 error here
        return new Button("Click");
    }
}
```

**Why This Matters:**

Storing hook results in class members breaks Ivy's reactive hook indexing system. The state object gets cached once and reused across renders, causing hooks to receive wrong indices and leading to unpredictable behavior.

**The Fix:**

Always store hook results in local variables:

```csharp
// ✅ Correct approach
public class TestView : ViewBase
{
    public override object? Build()
    {
        var count = UseState(0);  // Store in local variable
        return new Button("Click");
    }
}

// ✅ Or discard the result if not needed
public class TestView : ViewBase
{
    public override object? Build()
    {
        UseState(0);  // Discarded - also fine
        return new Button("Click");
    }
}
```

This analyzer helps enforce the correct hook usage pattern and catches mistakes that would otherwise cause hard-to-debug reactive system issues.

---

### Hook Usage Analyzer - Clearer Error Messages with Sub-Types

The hook usage analyzer now provides more specific error messages by splitting `IVYHOOK001` into sub-types, making it easier to understand and fix hook placement issues.

**What Changed:**

- `IVYHOOK001` now only fires for hooks called outside `Build()` entirely (e.g., in helper methods)
- New `IVYHOOK001B` fires for hooks nested in lambdas, local functions, or anonymous methods within `Build()`
- Error messages now explain the "same order on every render" constraint
- `IVYHOOK001B` messages specifically name the closure type causing the issue

**Examples:**

```csharp
// IVYHOOK001 - Hook in helper method (outside Build)
public class TestView : ViewBase
{
    public override object Build()
    {
        Helper();
        return new Button("Click");
    }

    private void Helper()
    {
        var state = UseState(0);  // IVYHOOK001: Must be at top level of Build()
    }
}

// IVYHOOK001B - Hook in lambda (inside Build)
public class TestView : ViewBase
{
    public override object Build()
    {
        var handler = (Event<Button> e) =>
        {
            var state = UseState(false);  // IVYHOOK001B: inside a lambda
        };
        return new Button().OnClick(handler);
    }
}

// IVYHOOK001B - Hook in local function (inside Build)
public class TestView : ViewBase
{
    public override object Build()
    {
        void LocalFunction()
        {
            var state = UseState(false);  // IVYHOOK001B: inside a local function
        }

        LocalFunction();
        return new Button("Click");
    }
}

// IVYHOOK001B - Hook in anonymous method (inside Build)
public class TestView : ViewBase
{
    public override object Build()
    {
        Action action = delegate()
        {
            var state = UseState(false);  // IVYHOOK001B: inside an anonymous method
        };

        return new Button("Click");
    }
}
```

This improvement helps developers quickly identify the exact nature of the hook placement issue and understand why hooks must execute in the same order on every render.

---

### CLI Documentation Commands - Query and Browse Framework Docs

New CLI commands provide instant access to framework documentation and AI-powered answers directly from your terminal.

**ivy question - Natural Language Framework Queries:**

Ask questions about Ivy using natural language and get answers powered by Local RAG (Retrieval Augmented Generation) tailored to your specific framework version:

```bash
# Ask architectural questions
ivy question "How do I implement a new Application Shell in Ivy?"

# Get specific command help
ivy question "What is the command to create an auto-incrementing migration in Ivy?"
```

The command searches the comprehensive framework knowledge base and provides contextually relevant answers based on the latest indexed state of the documentation.

**ivy docs - Access Documentation from Terminal:**

Browse and retrieve framework documentation without leaving your terminal:

```bash
# List all available documentation
ivy docs list

# Get the raw Markdown content for a specific page
ivy docs "docs/ApiReference/IvyShared/Colors.md"
```

The `ivy docs list` command outputs a structured YAML representation of all documentation paths, making it easy to discover available topics. Use these paths with `ivy docs [path]` to retrieve the actual documentation content.

These commands are particularly useful when:

- Working in terminal-focused workflows
- Building AI agents or automation that needs framework context
- Quickly checking API details without opening a browser
- Integrating framework documentation into other tools

---

### Smart Search - AI-Powered Documentation Assistant

The Ivy documentation now features an intelligent search assistant that answers your questions about the framework using natural language. Instead of manually browsing through documentation, you can ask questions directly and get contextual answers with follow-up conversation support.

**How to use it:**

Click on the search bar in the documentation sidebar or press it to activate the smart search overlay. Type your question and click "Get an answer from Ivy Agent" to receive an AI-generated response based on the Ivy Framework documentation.

**Key features:**

- **Natural language queries**: Ask questions like "How do I create a form?" or "What's the difference between UseState and UseSignal?"
- **Contextual answers**: Get relevant answers pulled directly from the official Ivy documentation
- **Follow-up conversations**: Continue the dialogue with follow-up questions to dive deeper into topics
- **Search suggestions**: Browse suggested documentation pages while typing
- **Keyboard navigation**: Use arrow keys to navigate suggestions and Enter to select or submit questions

**Example workflow:**

1. Click the sidebar search input
2. Type "How do I validate form inputs?"
3. Click "Get an answer from Ivy Agent" (or press Enter)
4. Read the AI-generated answer with code examples
5. Ask follow-up questions like "Can I add custom validation rules?"

This feature makes it faster to find information and learn the framework, especially when you're not sure which documentation page to check or what the exact API name is.

---

### Size.Fraction and Size.FractionGap - Decimal/Double Overloads Removed

The `decimal` and `double` overloads for `Size.Fraction()` and `Size.FractionGap()` have been removed to fix ambiguous call compilation errors (CS0121). You must now use `float` values with the `f` suffix.

**Before (caused ambiguous call errors):**

```csharp
// These overloads caused compiler ambiguity issues
.Width(Size.Fraction(0.5))
.Height(Size.FractionGap(0.25))
```

**After:**

```csharp
// Use explicit float literals with 'f' suffix
.Width(Size.Fraction(0.5f))
.Height(Size.FractionGap(0.25f))

// Or cast explicitly if using decimal/double variables
decimal ratio = 0.333m;
.Width(Size.Fraction((float)ratio))
```

This change ensures compilation succeeds without ambiguous method call errors.

---

### Size.Percent() - Intuitive Percentage-Based Sizing

New `Size.Percent()` overloads make it easier to specify percentage-based sizes without manually converting to fractions or using float literals.

**New overloads:**

```csharp
// Integer percentage
.Width(Size.Percent(50))    // 50% width
.Height(Size.Percent(100))  // 100% height

// String percentage (useful when parsing from config/user input)
.Width(Size.Percent("75%"))  // 75% width
.Height(Size.Percent("33%")) // 33% height
```

**Before (still works):**

```csharp
.Width(Size.Fraction(0.5f))   // 50% width
.Height(Size.Fraction(1.0f))  // 100% height
```

**After (more intuitive):**

```csharp
.Width(Size.Percent(50))   // Much clearer intent
.Height(Size.Percent(100)) // Obvious it's a percentage
```

This is especially helpful when working with percentage values, making the code more readable and eliminating the need to mentally convert percentages to decimal fractions.

---

### Connection Name Error Messages

When using `--test-connection` or `--describe-connection` command-line arguments with a connection name that doesn't exist, the error message now lists all available connections to help you quickly identify and fix typos or discover the correct connection names.

**Before:**

```
Connection 'mytypo' not found.
```

**After:**

```
Connection 'mytypo' not found. Available connections: postgres, mysql, redis
```

**Usage example:**

```bash
# Test a connection
dotnet run --test-connection postgres

# Describe a connection (shows connection details)
dotnet run --describe-connection mysql

# Typo in connection name now shows helpful error
dotnet run --test-connection postgress
# Output: Connection 'postgress' not found. Available connections: postgres, mysql, redis
```

This improvement makes it easier to work with database connections during development and deployment, eliminating the need to search through your code or documentation to find the correct connection names.

---

### AddConnectionsFromAssembly - Optional Assembly Parameter

The `AddConnectionsFromAssembly()` method now accepts an optional `Assembly` parameter, matching the existing `AddAppsFromAssembly` signature and providing more flexibility when registering connections.

**What Changed:**

You can now specify which assembly to scan for `IConnection` types, which is useful when the entry assembly differs from the one containing your connections.

**Usage:**

```csharp
// Scan the entry assembly (default behavior)
server.AddConnectionsFromAssembly();

// Scan a specific assembly
server.AddConnectionsFromAssembly(typeof(MyConnection).Assembly);

// Scan a loaded assembly
var assembly = Assembly.Load("MyConnectionLibrary");
server.AddConnectionsFromAssembly(assembly);
```

**When is this useful?**

- Your connections are defined in a separate library from your main entry point
- You're building modular applications with connections split across multiple assemblies
- You need explicit control over which assembly is scanned for connections

This enhancement makes connection registration more flexible and consistent with how apps are registered using `AddAppsFromAssembly`.

---

### CLI Commands Work Alongside Running Instances

CLI diagnostic commands (`--describe`, `--describe-connection`, `--test-connection`) now run successfully even when an Ivy app instance is already running on the configured port.

**What was the problem:**
These commands need dependency injection but don't actually start a web server. Previously, they would fail with port-in-use errors if you tried to run them while your app was already running, even though they never needed the port.

**What changed:**

- CLI-only commands now use port 0 internally and skip port-in-use checks entirely
- You can now run diagnostic commands at any time without stopping your running app
- The commands initialize the DI container but never bind to the actual port

**Example scenario:**

```bash
# Your app is running on port 5000
dotnet run

# In another terminal, you can now run diagnostic commands without conflicts
dotnet run --test-connection postgres
dotnet run --describe-connection mysql
dotnet run --describe
```

This eliminates the frustration of having to stop your running app just to test a database connection or inspect your app's configuration.

---

### Server Binds to Localhost - No More Windows Firewall Prompts

Ivy apps now bind to `localhost` instead of the wildcard address (`*`), eliminating Windows Firewall prompts during development.

**What changed:**

- Server now listens on `http://localhost:{port}` instead of `http://*:{port}`
- Since Ivy apps are local development tools, they don't need to listen on external network interfaces
- This change prevents Windows from showing firewall permission dialogs every time you run your app

**Impact:**

- Smoother development experience on Windows - no more clicking through firewall dialogs
- Your app is still fully accessible on localhost as before
- No action needed from developers - the change is automatic

This makes the development workflow more seamless, especially for Windows users who were repeatedly prompted to allow firewall access.

---

### Better Desktop Startup Error Messages

Desktop applications now show the actual server exception in error dialogs instead of a generic "Unable to connect" message. When your app fails to start due to configuration errors, missing dependencies, or other issues, you'll immediately see the root cause without needing to check log files.

**Example:** Instead of "Unable to connect to the Ivy server", you now see the specific error like:

```
System.InvalidOperationException: Configuration error: AppSettings:DatabaseConnection is missing
```

---

### Ivy.Desktop Now Available on NuGet

The `Ivy.Desktop` package is now published to NuGet, making it easier to add desktop application support to your Ivy projects.

**Installation:**

```bash
dotnet add package Ivy.Desktop
```

This streamlines the setup process for building native desktop applications - no need to manually reference project files or build from source.

---

### XamlBuilder Live Preview Sample

A new interactive sample app has been added to demonstrate the `XamlBuilder` API. This sample provides a live XAML editor with instant preview, making it easy to experiment with XAML-based layouts and learn the XamlBuilder syntax.

**Features:**

- Pre-built examples including layouts, buttons, cards, and nested structures
- Live code editor with syntax highlighting for XAML
- Real-time preview of rendered components
- Error feedback for invalid XAML syntax

**Code example from the sample:**

```csharp
var xml = UseState(DefaultXaml);
var builder = new XamlBuilder();

try
{
    var preview = builder.Build(xml.Value);
    // Display the rendered component
}
catch (Exception ex)
{
    // Show error feedback
    preview = Callout.Error(ex.Message);
}
```

This sample is particularly useful for learning how to:

- Parse and render XAML at runtime
- Build interactive editing interfaces
- Handle dynamic UI generation with error handling
- Structure complex layouts with minimal code

You can find this sample in the Advanced section of the sample apps.

---

### Launcher Scripts for Samples and Documentation

New PowerShell launcher scripts have been added to make it easier to run the Ivy Samples and Documentation projects. These scripts provide a streamlined way to start these projects with optional frontend building.

**IvySamples.ps1:**

```powershell
# Run the samples project
./IvySamples.ps1

# Build frontend first, then run samples
./IvySamples.ps1 -BuildFrontend
```

**IvyDocs.ps1:**

```powershell
# Run the documentation project
./IvyDocs.ps1

# Build frontend first, then run documentation
./IvyDocs.ps1 -BuildFrontend
```

When you use the `-BuildFrontend` switch, the scripts automatically:

1. Navigate to the frontend directory
2. Run `npm install` to ensure dependencies are up to date
3. Run `npm run build` to compile the frontend
4. Return and launch the .NET project

This eliminates the need to manually build the frontend in a separate terminal or remember the build steps, making it faster to get started with development or documentation work.

---

## Documentation Improvements

### Sidebar State Control Documentation

New documentation has been added for controlling sidebar state in your Ivy applications, both at the application chrome level and at the widget level.

**ChromeSettings Configuration**

You can now configure whether the sidebar starts expanded or collapsed when using chrome:

```csharp
var chromeSettings = new ChromeSettings()
    .Logo(
        Image.Src("/logo.svg").Width(40)
        | Text.P("Enterprise Application Framework").Small()
    )
    .DefaultApp<MyApp>()
    .UseTabs(preventDuplicates: true)
    .SidebarOpen(false); // Start with sidebar collapsed

server.UseChrome(() => new DefaultSidebarChrome(chromeSettings));
```

The `SidebarOpen(bool open)` method controls the initial state of the sidebar and defaults to `true` (expanded).

**SidebarLayout Widget APIs**

When building custom sidebars with the `SidebarLayout` widget, two key APIs control the behavior:

- **`.MainAppSidebar(bool isMainApp = true)`** - Configures the layout as the primary application menu. This automatically adds a collapse/expand toggle button and enables responsive behavior on smaller screens.
- **`.Open(bool open = true)`** - Controls whether the layout starts in an expanded or collapsed state.

```csharp
new SidebarLayout(
    sidebar: Menu.Sidebar()
        | new MenuItem("Dashboard", Icon.Name("home"))
        | new MenuItem("Settings", Icon.Name("settings")),
    sidebarHeader: Layout.Vertical().Gap(2)
        | Text.Lead("Workspace")
        | new TextInput(placeholder: "Search...", variant: TextInputVariant.Search)
).Open(false).MainAppSidebar(true);
```

This gives you full control over the sidebar's initial state whether you're using the built-in chrome or building custom layouts.

---

### New CLI Commands for Documentation Access

The Ivy CLI now includes powerful commands for querying and accessing framework documentation directly from your terminal.

**`ivy question` - Semantic Documentation Search**

Query the Ivy Framework documentation using natural language with Local RAG (Retrieval-Augmented Generation). The command uses semantic search to find relevant information from the framework's knowledge base, tailored to your specific Ivy version.

```terminal
>ivy question "How do I implement a new Application Shell in Ivy?"
```

```terminal
>ivy question "What is the command to create an auto-incrementing migration in Ivy?"
```

The query is wrapped in double quotes and can be any natural language question about Ivy's architecture, APIs, or best practices.

**`ivy docs list` - List Available Documentation**

List all available documentation paths in the framework:

```terminal
>ivy docs list
```

This outputs a structured YAML representation of all documentation titles and relative paths, making it easy to discover what documentation is available.

**`ivy docs [path]` - Retrieve Raw Documentation**

Fetch the raw Markdown content of a specific documentation page:

```terminal
>ivy docs "docs/ApiReference/IvyShared/Colors.md"
```

This retrieves the full Markdown source for the specified page, version-matched to your framework installation. Use paths discovered via `ivy docs list`.

These commands make it faster to find answers and reference documentation without leaving your terminal or opening a browser.

---

### Hook Syntax Simplification

The documentation has been updated to reflect that the `this.` prefix is no longer required when calling hooks. All framework documentation examples now use the cleaner, simplified syntax.

**Old style (still supported):**

```csharp
var state = this.UseState(0);
var client = this.UseService<IClientProvider>();
```

**New recommended style:**

```csharp
var state = UseState(0);
var client = UseService<IClientProvider>();
```

Both syntaxes work, but the documentation now consistently uses the simpler form without `this.` for better readability.

---

### AI-Powered Smart Search

The Ivy documentation site now features an intelligent AI-powered search system that helps you find answers faster and more accurately.

**How It Works:**

When you interact with the search box in the documentation sidebar, the Smart Search dialog opens with:

1. **Instant search suggestions** - As you type, see relevant documentation pages filtered by your query with smart scoring
2. **AI-powered answers** - Click the "Get an answer from Ivy Agent" button to ask natural language questions and receive contextual answers from the Ivy documentation
3. **Interactive chat** - Ask follow-up questions to refine your understanding without starting a new search
4. **Keyboard navigation** - Use `Arrow Up`/`Arrow Down` to navigate results, `Enter` to select or submit your question

**Key Features:**

- **Natural language questions** - Ask "How do I create a new input widget?" instead of searching for keywords
- **Context-aware answers** - Responses are generated from the official Ivy Framework documentation and tailored to your version
- **Follow-up conversation** - Continue the dialogue with additional questions to drill deeper into a topic
- **Seamless integration** - The search dialog opens automatically when you click or focus the sidebar search input
- **Smart error handling** - Clear error messages with retry options if searches fail

The Smart Search feature is powered by the new `IvyDocsQuestionsClient` service that communicates with the Ivy documentation AI API, providing accurate, up-to-date answers based on the framework's official documentation.

**Example Questions:**

```
"How do I implement a custom ViewBase component?"
"What's the difference between UseState and UseMemo?"
"How do I style a Button widget?"
```

This makes finding documentation faster and more intuitive, especially for developers new to the framework or looking for specific implementation patterns.

---

### Common API Mistake Documented: LayoutView.SpaceBetween()

A new entry has been added to the Hallucinations documentation page to help users avoid a common API mistake with layout spacing.

**Common Mistake:**

```csharp
Layout.Horizontal().SpaceBetween()  // ❌ Error: 'LayoutView' does not contain a definition for 'SpaceBetween'
```

**Correct Usage:**

```csharp
Layout.Horizontal(Align.SpaceBetween)  // ✅ Correct
```

`SpaceBetween` is an `Align` enum value passed to the layout constructor, not a fluent method. The same applies to `SpaceAround` and `SpaceEvenly`. This documentation helps clarify the correct API usage, especially for users working with AI coding assistants.

---

### Common API Mistake Documented: InputBase.Label()

The hallucinations documentation has been updated to clarify that the `.Label()` mistake applies to **all input types**, not just `NumberInputBase`.

**Common Mistake:**

```csharp
// Any input type - all fail the same way
stockAdjustment.ToNumberInput().Label("Adjustment amount")  // ❌ CS0311 error
dateState.ToDateInput().Label("Birthdate")                   // ❌ CS0311 error
```

**Error:** `The type 'Ivy.NumberInputBase' cannot be used as type parameter 'T' in the generic type or method 'AxisExtensions.Label<T>(T, string)'` (same error for `DateTimeInputBase`, `TextInputBase`, `SelectInputBase`, `BoolInputBase`, etc.)

**Correct Usage:**

The **preferred way** is to use `.WithField().Label()`:

```csharp
stockAdjustment.ToNumberInput().WithField().Label("Adjustment amount")
dateState.ToDateInput().WithField().Label("Birthdate")
```

Alternatively, use `Text.Label()` as a separate element:

```csharp
Layout.Vertical()
    | Text.Label("Adjustment amount")
    | stockAdjustment.ToNumberInput()
```

Or use the form builder's `.Label()` method:

```csharp
state.ToForm().Label(m => m.Amount, "Adjustment amount")
```

The issue is that `.Label()` is an `AxisExtensions` method for chart axes, not for inputs. This mistake affects all `InputBase` types, and the documentation now includes examples for multiple input types to make this clear.

---

### App Attribute - Comprehensive Usage Guide

The FAQ documentation now includes a comprehensive guide for the `[App]` attribute, explaining all available parameters and how to create Ivy applications.

**Available parameters:**

- `title` - Display name (optional, defaults to class name)
- `icon` - Icon from the Icons enum (optional)
- `path` - Navigation path/group as an array of strings (optional)
- `description` - Description text (optional)
- Additional parameters: `id`, `isVisible`, `order`, `groupExpanded`, `documentSource`, `searchHints`

**Example:**

```csharp
[App(
    title: "My App",           // Display name (optional, defaults to class name)
    icon: Icons.Layout,        // Icon from the Icons enum (optional)
    path: ["Category"],        // Navigation path/group (optional, array of strings)
    description: "My app desc" // Description text (optional)
)]
public class MyApp : ViewBase
{
    public override object? Build()
    {
        return Text.H1("Hello World");
    }
}
```

---

### App Attribute Layout Parameter Clarification

A new FAQ entry has been added clarifying that the `[App]` attribute does not have a layout parameter. This addresses a common misconception where developers might try to configure layout through the attribute itself.

**The Question:** Does the `[App]` attribute have a layout parameter?

**The Answer:** No. The `[App]` attribute only supports `title`, `icon`, `group`, and `connection` named parameters. Layout is controlled within the `Build()` method using layout helpers, not as an attribute parameter.

**Example:**

```csharp
[App(title: "My App", icon: Icons.Layout)]
public class MyApp : ViewBase
{
    public override object? Build()
    {
        // Layout is set here, not in the attribute
        return Layout.TopCenter()
            | (Layout.Vertical().Width(Size.Full().Max(200))
                | Text.H1("My App")
                | ...
            );
    }
}
```

This clarification helps developers understand that layout configuration is part of the component's render logic, not part of the app metadata defined in the attribute.

---

### Enhanced Ivy.Desktop README

The Ivy.Desktop package now has a comprehensive README with quick start instructions, code examples, and visual branding to help developers get started with building native desktop applications.

**What's New:**

The README now includes:

- Logo and NuGet badges showing package version, downloads, and license information
- Clear description of Ivy.Desktop's purpose: running Ivy apps natively on Windows, macOS, and Linux
- Benefits explanation: lightweight alternative to Electron using [Photino](https://tryphotino.io) without shipping a heavy Chromium instance
- Complete Quick Start guide with installation and initialization examples

**Quick Start Example:**

```csharp
using Ivy;
using Ivy.Desktop;

public class MyDesktopApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical(
            Text.Title("Hello from Ivy.Desktop!"),
            Text.Subtitle("Native desktop UI powered by C#")
        );
    }
}

// In your Program.cs
public class Program
{
    public static void Main(string[] args)
    {
        var appDescriptor = new AppDescriptor()
        {
            RootComponent = typeof(MyDesktopApp),
            InitialTitle = "My Desktop App",
        };

        DesktopWindow.Run(appDescriptor, args);
    }
}
```

This improved documentation makes it easier for developers to discover and start building desktop applications with Ivy, with clear instructions and working code examples right in the package README.

---

### AI Agent Hallucinations Reference

A new documentation file has been added that catalogs common API mistakes made by AI coding assistants when generating Ivy Framework code. This reference helps developers quickly identify and fix build errors that result from AI-generated code.

The documentation covers patterns like:

- Using non-existent fluent methods (e.g., `.Options()`, `.OnEnter()`, `.Color()`)
- Wrong enum names (e.g., `TextInputVariant` instead of `TextInputVariants`)
- Incorrect constructor signatures (e.g., `new Table<T>()` instead of `.ToTable()`)
- Confused property names (e.g., `.Data` instead of `.Value`, `.IsLoading` instead of `.Loading`)
- Non-existent lifecycle callbacks (e.g., `Server.OnReady()`, `Server.OnStartup()`)

Each entry includes the hallucinated API, the actual error message, and the correct way to use the API. This is particularly useful when working with AI assistants like Claude, GPT-4, or Copilot to build Ivy applications.

**New additions:**

The reference now documents the common hallucination where AI assistants suggest `server.OnReady()` or `server.OnStartup()` lifecycle callbacks for initializing data or seeding databases. The `Server` class doesn't have these methods. Instead, use the connection's context factory pattern to seed data:

```csharp
// Correct: Use context factory for database seeding
var connection = server.UseConnection<MyDbContext>(options =>
    options.ContextFactory = () =>
    {
        var ctx = new MyDbContext();
        ctx.Database.EnsureCreated();
        SeedData(ctx);
        return ctx;
    });

// Or resolve services directly in Program.cs:
var myService = server.Services.GetRequiredService<IMyService>();
myService.Initialize();
```

The reference also now includes the `LayoutView.SpaceBetween()` hallucination, where AI assistants suggest using `.SpaceBetween()` as a fluent method when it's actually an `Align` enum value:

```csharp
// Hallucinated (incorrect):
Layout.Horizontal().SpaceBetween()

// Correct:
Layout.Horizontal(Align.SpaceBetween)
```

The same applies to `SpaceAround` and `SpaceEvenly` — they are alignment options passed to the layout constructor, not fluent methods.

The documentation now includes an improved workaround for the `UseState<T?>(null)` ambiguity issue. When initializing nullable state with `null`, the simplest approach is to omit the argument entirely:

```csharp
// Best: omit the null argument — the default is already null
var selectedItem = UseState<InventoryItem?>();

// Alternative: cast null to the explicit type
var selectedItem = UseState<InventoryItem?>((InventoryItem?)null);

// Alternative: use a lambda
var selectedItem = UseState(() => (InventoryItem?)null);
```

**Technical note:** Unlike `IState<T>.Set(null)` (which was fixed using `[OverloadResolutionPriority]`), `UseState` cannot use the same solution. C# 10+ lambda natural types would cause the `T?` overload to steal all lambda calls when given higher priority, breaking `UseState(() => expr)` patterns throughout the codebase.

The reference now documents the `Fragment.Empty` hallucination, where AI assistants suggest using a non-existent static member to return an empty view:

```csharp
// Hallucinated (incorrect):
return Fragment.Empty;
// Error: 'Fragment' does not contain a definition for 'Empty'

// Correct alternatives:
return ViewBase.Empty;  // Use ViewBase.Empty
return new Fragment();  // Or return an empty Fragment
return null;            // Or just return null
```

`Fragment` does not have an `Empty` static member. To return nothing from a view, use `ViewBase.Empty`, `new Fragment()`, or `null`.

**Key points:**

- The class must inherit from `ViewBase` and override `Build()`
- `path` controls navigation grouping (e.g., `["Settings", "Advanced"]` creates nested groups)
- `icon` uses the `Icons` enum (e.g., `Icons.Settings`, `Icons.Users`, `Icons.Database`)
- All parameters are optional — `[App]` with no arguments is valid
- Layout is controlled within the `Build()` method, not as an attribute parameter

---

### UseAlert Hook FAQ Entry

The FAQ documentation now includes guidance on using the `UseAlert` hook for showing alert dialogs, addressing a common question about displaying alerts and confirmations to users.

**Basic usage:**

```csharp
var (alertView, showAlert) = UseAlert();

return Layout.Vertical()
    | new Button("Show Alert", _ =>
        showAlert("Are you sure?", result =>
        {
            // result is the AlertResult (Ok, Cancel, Yes, No)
        }, "Confirmation", AlertButtonSet.OkCancel))
    | alertView; // IMPORTANT: alertView must be included in the view tree
```

**Key points:**

- `UseAlert()` returns a **tuple**, not an object — always destructure it with `var (alertView, showAlert)`
- `showAlert` is a delegate with signature: `showAlert(message, callback, title?, buttonSet?)`
- `alertView` must be rendered somewhere in your view tree for the dialog to appear
- Available button sets: `AlertButtonSet.Ok`, `AlertButtonSet.OkCancel`, `AlertButtonSet.YesNo`, `AlertButtonSet.YesNoCancel`
- For simple toast notifications without callbacks, use `client.Toast("message")` or `client.Error("message")` via `IClientProvider`

---

### Delete Confirmation FAQ Entry

The FAQ documentation now includes a complete example of implementing delete confirmations, demonstrating a common pattern for confirming destructive actions before executing them.

**Example - Delete with confirmation:**

```csharp
var (alertView, showAlert) = UseAlert();
var client = UseService<IClientProvider>();

void DeleteItem(int id)
{
    showAlert("Are you sure you want to delete this item?", async result =>
    {
        if (result == AlertResult.Ok)
        {
            await using var db = dbFactory.CreateDbContext();
            var item = await db.Items.FindAsync(id);
            if (item != null)
            {
                db.Items.Remove(item);
                await db.SaveChangesAsync();
                client.Toast("Item deleted");
                refreshToken.Refresh();
            }
        }
    }, "Confirm Delete", AlertButtonSet.OkCancel);
}

return Layout.Vertical()
    | new Button("Delete", _ => DeleteItem(itemId)).Destructive()
    | alertView; // IMPORTANT: must include alertView in the view tree
```

**Key patterns demonstrated:**

- Using `UseAlert()` for confirmation dialogs
- Destructive button styling with `.Destructive()`
- Async database operations inside the callback
- Toast notifications for user feedback
- Refreshing the UI after data changes
- Proper disposal with `await using`

---

### UseQuery with EF Core FAQ Entry

The FAQ documentation now includes comprehensive guidance on using `UseQuery` with Entity Framework Core, covering the proper patterns for reactive database queries in Ivy applications.

**Key topics covered:**

- **DbContext Factory Pattern**: Always use `IDbContextFactory<T>` (never inject `DbContext` directly) and create a scoped instance with `CreateDbContextAsync()` inside the query lambda
- **QueryResult Properties**: The result has `.Value`, `.Loading`, and `.Error` properties (not `.Data` or `.IsLoading`)
- **Reactive Re-fetching**: Pass a `RefreshToken` as a dependency to automatically re-run queries after data mutations
- **Query Execution**: Always call `.ToListAsync()` inside the query lambda — never return `IQueryable` directly
- **Mutation Pattern**: Use separate methods that create their own `DbContext` from the factory, call `SaveChangesAsync()`, then trigger `refreshToken.Refresh()`

**Example:**

```csharp
var dbFactory = UseService<IDbContextFactory<MyDbContext>>();
var refreshToken = UseRefreshToken();

var query = UseQuery(async () =>
{
    await using var db = await dbFactory.CreateDbContextAsync();
    return await db.Items.OrderBy(i => i.Name).ToListAsync();
}, refreshToken);

if (query.Loading) return Text.P("Loading...");
if (query.Value is not { } items) return Callout.Info("No data.");

return items.ToDataTable();
```

This pattern ensures proper lifetime management, prevents concurrency issues, and integrates seamlessly with Ivy's reactive system.

---

### DataTable Navigation Properties FAQ Entry

The FAQ documentation now includes guidance on working with navigation properties in DataTable, explaining a common limitation and the recommended solution.

**The Problem:** `DataTableBuilder` only supports top-level properties of the model type. Attempting to access nested properties like `p.Author.Username` will throw a `KeyNotFoundException` at runtime because only direct properties are scaffolded as columns.

**The Solution:** Project your query into a flat DTO with all needed fields as direct properties:

```csharp
// ❌ BAD - nested property access will fail at runtime
var posts = db.Posts.Include(p => p.Author).AsQueryable();
posts.ToDataTable()
    .Header(p => p.Author.Username, "Author"); // KeyNotFoundException!

// ✅ GOOD - project into a flat DTO
record PostListItem(int Id, string Title, string AuthorName, string Status);

var posts = db.Posts
    .Include(p => p.Author)
    .Select(p => new PostListItem(p.Id, p.Title, p.Author.Username, p.Status.ToString()))
    .AsQueryable();

posts.ToDataTable()
    .Header(p => p.AuthorName, "Author"); // Works!
```

**Benefits of this approach:**

- Avoids runtime exceptions from nested property access
- Simplifies DataTable configuration
- No need to use `.Hidden()` to hide navigation properties or unwanted fields
- Better performance by only selecting the data you need

---

### Chart Examples in XamlBuilderApp

The XamlBuilderApp sample now includes comprehensive chart examples demonstrating how to use JSON CDATA data with charts. New examples include Line Chart, Bar Chart, Area Chart, and Pie Chart presets.

**Line Chart Example:**

```csharp
["Line Chart"] = """
    <LineChart>
      <Data><![CDATA[
        [
          { "Month": "Jan", "Revenue": 4000, "Costs": 2400 },
          { "Month": "Feb", "Revenue": 3000, "Costs": 1398 },
          { "Month": "Mar", "Revenue": 5000, "Costs": 3200 },
          { "Month": "Apr", "Revenue": 4780, "Costs": 3908 },
          { "Month": "May", "Revenue": 5890, "Costs": 4800 },
          { "Month": "Jun", "Revenue": 6390, "Costs": 3800 }
        ]
      ]]></Data>
      <LineChart.Lines>
        <Line DataKey="Revenue" />
        <Line DataKey="Costs" />
      </LineChart.Lines>
      <LineChart.XAxis>
        <XAxis DataKey="Month" />
      </LineChart.XAxis>
      <LineChart.YAxis>
        <YAxis />
      </LineChart.YAxis>
      <LineChart.CartesianGrid StrokeDashArray="3 3" />
      <LineChart.Tooltip />
      <LineChart.Legend Layout="Horizontal" Align="Center" />
    </LineChart>
    """
```

These examples are available in the XamlBuilder sample app and provide ready-to-use templates for implementing data visualizations in your Ivy applications.

---

### Table Creation FAQ Entry

The FAQ documentation now includes comprehensive guidance on creating Tables in Ivy, covering both simple data-driven tables and manual table construction.

**Simple data-driven tables:**

For displaying collections as tables, use `.ToTable()` to automatically scaffold columns from your data:

```csharp
var data = new[]
{
    new { Name = "Alice", Age = 30, City = "NYC" },
    new { Name = "Bob", Age = 25, City = "LA" },
};
return data.ToTable();
```

**Manual table construction:**

For fine-grained control, construct tables explicitly using `TableRow` and `TableCell`:

```csharp
new Table(
    new TableRow(new TableCell("Name"), new TableCell("Age")).IsHeader(),
    new TableRow(new TableCell("Alice"), new TableCell("30")),
    new TableRow(new TableCell("Bob"), new TableCell("25"))
)
```

**Important API notes:**

- `Table` takes `TableRow[]` as its constructor parameter — **not** `string[]`
- There is **no** `.Row()` method on Table
- For data-heavy tables with sorting, filtering, and pagination, use `.ToDataTable()` on `IQueryable<T>` instead of `.ToTable()`

---

### UseState Immutable Type Guidance

The `UseState` documentation now includes important guidance about using immutable types to prevent silent re-render failures. This addresses a common pitfall where UI doesn't update as expected.

**The Problem:** When you mutate an object in-place and pass the same reference to `.Set()`, the UI will not re-render because Ivy's change detection is based on reference equality. If the reference hasn't changed, no re-render occurs.

**The Solution:** Always use immutable types (like records) with `UseState`, and create new instances when updating state:

```csharp
// ❌ BAD - mutating in place won't trigger re-render
public record UserSettings(string Theme, bool Notifications);
var settings = UseState(new UserSettings("light", true));

// This won't work - same reference!
settings.Value.Theme = "dark";
settings.Set(settings.Value);

// ✅ GOOD - create a new instance with 'with' expression
var settings = UseState(new UserSettings("light", true));

// This works - new reference!
settings.Set(settings.Value with { Theme = "dark" });
```

**Key takeaways:**

- Use C# records for state objects (they're immutable by default)
- Use `with` expressions to create modified copies
- Never mutate properties directly on state objects
- This applies to complex objects — primitives like `int`, `string`, etc. work fine since they're immutable by nature

---

### NumberInput Format Styles FAQ Entry

The FAQ documentation now includes guidance on formatting `NumberInput` controls as currency, percent, or decimal using the `NumberFormatStyle` enum.

**Example usage:**

```csharp
var price = UseState(99.99m);
var taxRate = UseState(0.08);

// Currency formatting
price.ToNumberInput().FormatStyle(NumberFormatStyle.Currency).Currency("USD")

// Percent formatting
taxRate.ToNumberInput().FormatStyle(NumberFormatStyle.Percent)

// Decimal formatting (default)
price.ToNumberInput().FormatStyle(NumberFormatStyle.Decimal)
```

**Available format styles:**

- `NumberFormatStyle.Decimal` - Standard decimal formatting (default)
- `NumberFormatStyle.Currency` - Currency formatting with symbol
- `NumberFormatStyle.Percent` - Percentage formatting with % symbol

**Key points:**

- Use `.FormatStyle()` to set the formatting style
- For currency inputs, chain `.Currency("USD")` to specify the currency code
- Recommended state type for currency is `decimal`

---

### Clipboard Access Clarification

The FAQ documentation now clarifies that there is no `UseClipboard` hook in Ivy. Clipboard functionality is accessed through `IClientProvider`, not through a dedicated hook.

**How to access the clipboard:**

```csharp
var client = UseService<IClientProvider>();

// Copy text to clipboard
client.CopyToClipboard("Text to copy");
client.Toast("Copied to clipboard!");
```

**Key points:**

- There is **no** `UseClipboard` hook
- Use `UseService<IClientProvider>()` to get the client provider
- Call `client.CopyToClipboard(content)` to copy text
- The client provider offers other methods like `Toast()`, `Error()`, and navigation methods

---

### Port Configuration Guide

The documentation now includes comprehensive guidance on all the ways to configure the server port in Ivy applications, making it easier to run multiple instances or integrate with different deployment environments.

**Three ways to configure the port:**

1. **CLI Flag** - The simplest approach when using `ivy run`:

```terminal
>ivy run --port 5011
```

1. **Server Configuration in Code** - Recommended for `dotnet run` and file-based apps:

```csharp
var server = new Server(new ServerArgs { Port = 5011 });
```

1. **Environment Variable** - Works with any launch method:

```terminal
>set PORT=5011
>dotnet run
```

The documentation clarifies that by default, Ivy starts on port **5010**, and explains when each configuration method is most appropriate. This is particularly useful when running multiple Ivy instances simultaneously or deploying to environments with specific port requirements.

---

### RichTextBlock Streaming Documentation

The `RichTextBlock` streaming documentation has been significantly expanded with practical examples and technical details about buffering behavior, making it easier to build streaming text features like LLM responses.

**New Simulated LLM Example:**

The documentation now includes a realistic example showing how to stream text word-by-word with automatic spacing, perfect for AI chat interfaces:

```csharp
public class RichTextLLMStreamDemo : ViewBase
{
    public override object? Build()
    {
        var stream = Context.UseStream<TextRun>();
        var cts = new CancellationTokenSource();

        return Layout.Vertical()
            | Text.Rich()
                .Bold("🤖 ")
                .UseStream(stream)
            | new Button("Generate response").OnClick(async () =>
            {
                await cts.CancelAsync();
                cts = new CancellationTokenSource();
                var token = cts.Token;

                var words = "The meaning of life is to build great software.".Split(' ');
                try
                {
                    foreach (var word in words)
                    {
                        await Task.Delay(100, token);
                        stream.Write(new TextRun(word) { Word = true });
                    }
                }
                catch (OperationCanceledException) { }
            });
    }
}
```

**Key Feature - Word Spacing:**
Setting `Word = true` on each `TextRun` automatically inserts spaces between words, eliminating the need to manually add spacing when streaming word-by-word.

**Buffering Behavior:**

The documentation now explains that `UseStream<T>()` buffers data by default until the frontend subscribes. This means you can start writing to the stream immediately — any data written before the frontend is ready will be automatically flushed once the connection is established.

To disable buffering:

```csharp
var stream = Context.UseStream<TextRun>(buffer: false);
```

**Use Cases:**
The enhanced documentation highlights ideal use cases for streaming:

- LLM/AI chat responses
- Live log tailing
- Real-time data feeds
- Any incremental text output that arrives over time

---

### Full-Height Layout Sample - Header, Content, Footer Pattern

A new sample app has been added demonstrating the correct way to build full-height layouts with a header, scrollable content area, and footer using `Layout.Vertical().Height(Size.Full())`.

**Key Pattern:**

The sample shows how to properly structure a full-height layout where only the content area scrolls while the header and footer remain fixed:

```csharp
public class FullHeightLayoutView : ViewBase
{
    public override object? Build()
    {
        var filter = UseState("");

        var header = Layout.Horizontal().Align(Align.Center)
            | Text.H2("Items")
            | filter.ToTextInput().Placeholder("Filter...").Variant(TextInputVariant.Search);

        var content = Layout.Vertical()
            | Enumerable.Range(1, 30)
                .Select(i => new Card($"Item {i}").Description($"Description for item {i}"))
                .Cast<object>()
                .ToArray();

        var footer = Layout.Horizontal().Align(Align.Center)
            | Text.Muted("30 items")
            | new Spacer()
            | new Button("Export").Variant(ButtonVariant.Outline).Icon(Icons.Download);

        return Layout.Vertical().Height(Size.Full())
            | header
            | (Layout.Vertical().Height(Size.Full())
                | content)
            | footer;
    }
}
```

**Important Details:**

- The root layout uses `Layout.Vertical().Height(Size.Full())` to fill the viewport
- Header and footer auto-size to their content (no explicit height)
- Only the content layout uses `.Height(Size.Full())` to fill remaining space
- The content area automatically becomes scrollable when it overflows

This pattern is ideal for dashboard layouts, list views, and any interface where you need fixed header/footer sections with a scrollable main content area.

---

### UseEffect Resource Cleanup FAQ Entry

The FAQ documentation now includes comprehensive guidance on cleaning up resources like timers and subscriptions in UseEffect hooks. Proper cleanup is essential to prevent memory leaks and ensure resources are disposed when components unmount or dependencies change.

**Simple cleanup - return the disposable directly:**

For resources that implement `IDisposable`, you can return them directly from the UseEffect callback:

```csharp
UseEffect(() =>
{
    var timer = new System.Threading.Timer(_ =>
    {
        counter.Set(counter.Value + 1);
    }, null, 0, 1000);

    return timer; // Timer implements IDisposable — returned for cleanup
}, dependencies);
```

**Custom cleanup logic - use Disposable.Create():**

For more complex cleanup scenarios, use `Disposable.Create()` from `System.Reactive.Disposables`:

```csharp
using System.Reactive.Disposables;

UseEffect(() =>
{
    var timer = new System.Threading.Timer(_ =>
    {
        counter.Set(counter.Value + 1);
    }, null, 0, 1000);

    return Disposable.Create(() =>
    {
        timer?.Dispose();
        // additional cleanup logic here
    });
}, dependencies);
```

**Important:** `Disposable.Create()` requires `using System.Reactive.Disposables;`. System.Reactive is included as a transitive dependency of Ivy Framework — you do NOT need to add a NuGet package, just add the using statement.

**Cancellation-based cleanup:**

For operations that support cancellation, use `CancellationTokenSource`:

```csharp
UseEffect(() =>
{
    var cts = new CancellationTokenSource();
    StartBackgroundWork(cts.Token);
    return cts; // CancellationTokenSource implements IDisposable
}, dependencies);
```

**AI Assistant Hallucination Warning:**

The hallucinations reference has been updated to note that AI assistants often suggest `Disposable.Create()` but forget to include the required `using System.Reactive.Disposables;` statement. If you see the error `CS0103: The name 'Disposable' does not exist in the current context`, simply add the using directive — the package is already available as a transitive dependency.

---

### UseEffect Multiple Firing FAQ Entry

The FAQ documentation now includes guidance on why `UseEffect` fires multiple times and how to prevent duplicate processing. This addresses the common question: "Why does my UseEffect fire multiple times even though I only expect it to run once?"

**The behavior:**

`UseEffect` with `AfterChange` triggers (state dependencies) fires once per `Set()` call on the watched state. If the state is updated multiple times in quick succession (e.g., file upload status transitions), the effect runs for each update.

**Solution - Use a guard pattern:**

```csharp
var processedFile = UseRef<string?>(null);
var uploadedFile = UseState<FileUpload?>(null);

UseEffect(() =>
{
    var file = uploadedFile.Value;
    if (file == null) return;
    if (processedFile.Value == file.FileName) return; // Guard: already processed
    processedFile.Value = file.FileName;

    // Process file and show toast
    alert.Toast($"Loaded {file.FileName}");
}, uploadedFile);
```

**Key points:**

- Use `UseRef` to track processed state without triggering re-renders
- Always check if the meaningful value actually changed before taking action
- For file uploads, guard on the file name or a unique identifier

---

### Displaying Dictionary Data in DataTable

The FAQ documentation now includes guidance on displaying dictionary or dynamic data in a DataTable, addressing the common scenario where data has dynamic or variable columns.

**The challenge:**

`ToDataTable()` uses reflection to discover columns from the model type's top-level properties. It does not automatically expand `Dictionary<TKey, TValue>` properties into separate columns — the dictionary itself appears as a single column.

**Solution - Project into a flat structure first:**

```csharp
// Instead of this:
record DataRow(int Id, Dictionary<string, string> Values);
rows.AsQueryable().ToDataTable(); // Shows "Id" and "Values" columns

// Do this — project into a flat anonymous type or record:
record FlatRow(string Name, int Age, string City);
var flat = rows.Select(r => new FlatRow(
    r.Values["Name"],
    int.Parse(r.Values["Age"]),
    r.Values["City"]
));
flat.AsQueryable().ToDataTable(); // Shows Name, Age, City columns
```

**For truly dynamic columns:**

If the columns are unknown at compile time, consider building a `List<Dictionary<string, object>>` and using `.ToTable()` with explicit column definitions instead of relying on reflection.

---

### Dynamic Form Fields FAQ Entry

The FAQ documentation now includes guidance on creating forms with a dynamic number of fields, addressing the common scenario where you need to generate inputs based on runtime data (e.g., dictionary input, configurable forms).

**The Problem:**

Since hooks cannot be called inside loops (violates rule IVYHOOK003), you cannot use `UseState` in a `for`/`foreach`/LINQ loop. This means you can't create a separate state variable for each dynamic field.

**The Solution:**

Use **one state variable** that holds all field values in a single collection (like a `Dictionary<string, string>`):

```csharp
public override object Build()
{
    var columns = GetColumnNames(); // e.g. ["Name", "Age", "City"]
    var values = UseState(new Dictionary<string, string>());

    var layout = Layout.Vertical();
    foreach (var col in columns)
    {
        var currentValue = values.Value.GetValueOrDefault(col, "");
        layout.Add(
            new TextInput(currentValue, e =>
            {
                var updated = new Dictionary<string, string>(values.Value) { [col] = e.Value };
                values.Set(updated);
            })
            .Placeholder(col)
            .WithField()
            .Label(col)
        );
    }
    return layout;
}
```

**Key points:**

- Only one `UseState` call at the top level — no hook rule violations
- The dictionary keys map to column names, values map to user input
- Create a new dictionary on each update to trigger a re-render
- This pattern works for any dynamic input scenario (forms, dialogs, configuration editors, etc.)

---

### Application Chrome and SidebarLayout Configuration

The documentation has been enhanced with comprehensive guidance on configuring application sidebars, including how to control the initial sidebar state and enable toggle functionality.

**ChromeSettings.SidebarOpen() - Initial Sidebar State**

A new `SidebarOpen(bool open)` method is now documented for `ChromeSettings`, allowing you to control whether the application sidebar starts expanded or collapsed. This is particularly useful for applications where screen real estate is valuable or when you want to default to a focused, distraction-free view.

```csharp
var chromeSettings = new ChromeSettings()
    .Logo(
        Icons.LayoutSidebar,
        Text.P("My Application").Large()
        | Text.P("Enterprise Application Framework").Small()
    )
    .DefaultApp<MyApp>()
    .UseTabs(preventDuplicates: true)
    .SidebarOpen(false); // Start with sidebar collapsed

server.UseChrome(() => new DefaultSidebarChrome(chromeSettings));
```

**SidebarLayout.MainAppSidebar() and .Open() APIs**

The `SidebarLayout` documentation now clearly explains the `.MainAppSidebar()` and `.Open()` APIs:

- **`.MainAppSidebar(bool isMainApp = true)`** - Configures the layout as the primary application menu, automatically adding a collapse/expand toggle button and enabling responsive behavior on smaller screens
- **`.Open(bool open = true)`** - Controls whether the layout starts in an expanded or collapsed state (defaults to `true`)

These APIs can be combined to create a toggleable sidebar that starts collapsed:

```csharp
return new SidebarLayout(
    mainContent: Text.H1("Main Content"),
    sidebarContent: Layout.Vertical().Gap(2)
        | Text.Lead("Workspace")
        | new TextInput(placeholder: "Search...", variant: TextInputVariant.Search)
).Open(false).MainAppSidebar(true);
```

**Integration with ChromeSettings**

When using `ChromeSettings` to define the main application chrome, you can inject the initial sidebar state through `ChromeSettings.SidebarOpen(false)`, which will be respected by the chrome implementation. This provides a centralized way to configure the default sidebar behavior for your entire application.

---

### Database Schema Generator Documentation Removed

The documentation for the `ivy db generate` command has been removed from the framework documentation. This CLI command, which previously provided a GUI for generating database schemas using AI, is no longer available.

**What changed:**

- Removed references to `ivy db generate` from the getting started documentation
- Removed the complete "Generating A Database Schema" section including usage examples and screenshots

**Alternative:**

For generating applications from database schemas, use `ivy app create` which remains the recommended approach for creating back-office applications based on your existing database structure.

---

### Form Samples Consolidated

The sample applications have been reorganized to consolidate form-related examples in one place. The `AllowedValuesFormApp`, which demonstrates how to use select inputs and dropdowns with allowed values, is no longer a standalone app in the Concepts section. Instead, it's now integrated as a tab within the main `FormApp`.

**What this means for users:**

When exploring form examples in the framework samples, you'll now find all form-related concepts organized together:

- **Form** - Basic form building
- **Scaffolding** - Automatic form generation
- **Validation** - Form validation examples
- **Allowed Values** - Select inputs and dropdowns (previously a separate app)

This consolidation makes it easier to discover all form-related features without navigating between multiple apps. The search functionality has been updated accordingly, so searching for "allowed-values", "select", or "dropdown" will still lead you to the right place.

---

### Creating Circular Shapes FAQ Entry

The FAQ documentation now includes guidance on how to create circular shapes and circles in Ivy. Since there is no dedicated Shape or Circle widget, the documentation explains how to use a `Box` with `BorderRadius.Full` and equal width and height.

**Creating a colored circle:**

```csharp
new Box()
    .Color(Colors.Primary)
    .Width(Size.Px(36))
    .Height(Size.Px(36))
    .BorderRadius(BorderRadius.Full)
```

**Creating a circle with centered content:**

```csharp
new Box(Text.P("A"))
    .Color(Colors.Slate)
    .Width(Size.Px(48))
    .Height(Size.Px(48))
    .ContentAlign(Align.Center)
    .BorderRadius(BorderRadius.Full)
```

**Key points:**

- `BorderRadius.Full` makes the box fully rounded
- When width and height are equal, this produces a perfect circle
- Use `BorderRadius.Rounded` for rounded corners instead of a complete circle

---

### UseState Immutable Type Guidance

The `UseState` hook documentation and AI agent guidelines have been updated with critical guidance about using immutable types to prevent silent re-render failures.

**The Problem:**

When you mutate an object in place and pass the same reference to `.Set()`, Ivy won't detect the change because the reference hasn't changed, resulting in the UI not re-rendering:

```csharp
// ❌ WRONG - This won't trigger a re-render
var userState = UseState(new User { Name = "Alice" });
var user = userState.Value;
user.Name = "Bob";           // Mutating in place
userState.Set(user);          // Same reference - no re-render!
```

**The Solution:**

Always use immutable types like records with `UseState`. When you need to update the state, create a new instance using the `with` expression:

```csharp
// ✅ CORRECT - Using a record with 'with' expression
public record User(string Name, int Age);

var userState = UseState(new User("Alice", 30));

// Create a new instance when updating
userState.Set(userState.Value with { Name = "Bob" });
```

**Key points:**

- Use `record` types instead of `class` types for state objects
- The `with` expression creates a new instance with modified properties
- Creating a new instance ensures the reference changes, triggering a re-render
- This guidance has been added to both the UseState documentation and the AGENTS.md file

---

### Icon Sizing FAQ Entry

The FAQ documentation now includes guidance on how to change the size of icons, addressing a common question about icon sizing.

**How to change icon size:**

```csharp
new Icon(Icons.Star).Small()   // small icon
new Icon(Icons.Star)            // default size
new Icon(Icons.Star).Large()   // large icon
```

**Important:** There is no `.WithIconSize()` method or `IconSize` enum. Use the simple `.Small()` and `.Large()` fluent modifiers for resizing icons.

---

### Text Sizing FAQ Entry

The FAQ documentation now includes guidance on how to change the font size of text, addressing a common question about text sizing.

**How to change text size:**

```csharp
Text.P("Large text").Large()
Text.P("Normal text").Medium()
Text.P("Small text").Small()
Text.P("Small muted text").Small().Muted()
```

These modifiers work with all text factory methods (`Text.P()`, `Text.H1()`, `Text.Block()`, `Text.Label()`, etc.).

**Important:** There is no `.WithFontSize()` method or `FontSize` enum. Use the simple `.Large()`, `.Medium()`, and `.Small()` fluent modifiers for text sizing.

---

### RichTextBlock Streaming Documentation Expanded

The `RichTextBlock` streaming documentation has been significantly enhanced with more comprehensive examples and explanations, making it easier to implement real-time text streaming scenarios like LLM responses.

**What's New:**

The documentation now includes a realistic LLM response streaming example that demonstrates word-by-word streaming with delays. This example shows how to use the `Word = true` property to automatically insert spaces between words:

```csharp
public class RichTextLLMStreamDemo : ViewBase
{
    public override object? Build()
    {
        var stream = Context.UseStream<TextRun>();
        var cts = new CancellationTokenSource();

        return Layout.Vertical()
            | Text.Rich()
                .Bold("🤖 ")
                .UseStream(stream)
            | new Button("Generate response").OnClick(async () =>
            {
                await cts.CancelAsync();
                cts = new CancellationTokenSource();
                var token = cts.Token;

                var words = "The meaning of life is to build great software.".Split(' ');
                try
                {
                    foreach (var word in words)
                    {
                        await Task.Delay(100, token);
                        stream.Write(new TextRun(word) { Word = true });
                    }
                }
                catch (OperationCanceledException) { }
            });
    }
}
```

**Buffering Control:**

The documentation now explains that `UseStream<T>()` buffers data by default until the frontend subscribes. This means you can start writing immediately, and any data written before the frontend is ready will be automatically flushed once the connection is established:

```csharp
// Default: buffering enabled (recommended)
var stream = Context.UseStream<TextRun>();

// Disable buffering if needed
var stream = Context.UseStream<TextRun>(buffer: false);
```

These improvements make it much clearer how to implement streaming text scenarios, particularly useful for AI/LLM integrations, live logs, or any incremental text output.

---

### Complete Port Configuration Guide

The documentation now includes a comprehensive guide for all the ways to configure the port when running Ivy applications. Whether you're using the CLI, running with `dotnet run`, or working with file-based apps, you now have clear examples for each scenario.

**Three Ways to Configure the Port:**

**1. CLI Flag (simplest for `ivy run`):**

```terminal
>ivy run --port 5011
```

**2. Server Configuration in Code:**

```csharp
var server = new Server(new ServerArgs { Port = 5011 });
```

This is the recommended approach when running with `dotnet run` or `dotnet watch` directly, since those commands do not support the `--port` flag. It also works for file-based apps.

**3. Environment Variable:**

```terminal
>set PORT=5011
>dotnet run
```

This works with any launch method (`ivy run`, `dotnet run`, file-based apps).

The documentation now clearly explains when to use each method and includes cross-references between the CLI documentation and the file-based apps guide, making it easier to find the right approach for your workflow.

---

### Full-Height Layout Sample

A new sample has been added demonstrating the correct way to build a full-height layout with header, content, and footer using `Layout.Vertical().Height(Size.Full())`.

The `FullHeightLayoutApp` sample shows how to create a layout that fills the entire viewport with three sections: a header with title and search input, scrollable content in the middle, and a footer with action buttons.

**Key Pattern:**

```csharp
return Layout.Vertical().Height(Size.Full())
    | header
    | (Layout.Vertical().Height(Size.Full())
        | content)
    | footer;
```

**Important Details:**

- The outer container uses `.Height(Size.Full())` to fill the available space
- Only the content row (middle section) should have `.Height(Size.Full())` applied
- Header and footer rows auto-size to their content
- The content section becomes scrollable when it exceeds available space

This pattern is essential for building app-like layouts where you need a fixed header and footer with scrollable content in between. The sample can be found in the framework's sample browser under the "Layouts" category with search hints including "full height", "header", "footer", "content", "stretch", and "fill".

---

### Component Styling Guide - FAQ Entry

A new FAQ entry has been added explaining how to apply styling (width, height, color, padding) to Ivy components. This addresses a common question from developers trying to apply custom styles to widgets.

**The Question:** How do I apply styling (width, height, color, padding) to Ivy components?

**The Answer:** Ivy uses a fluent API for styling - there is no `.Style()` method for arbitrary CSS. Instead, use the built-in extension methods:

```csharp
new Box(content)
    .Width(Size.Px(200))
    .Height(Size.Px(100))
    .Color(Colors.Blue)
    .Padding(16)
    .Margin(8)
    .BorderRadius(BorderRadius.Rounded)
    .BorderStyle(BorderStyle.Solid)
```

**For Advanced CSS Effects:**

For CSS transforms, rotations, or complex visual effects that can't be expressed with Ivy's styling API, use the `Html` widget with inline styles:

```csharp
new Html($"<div style='transform: rotate({degrees}deg); width: 100px; height: 2px; background: #000;'></div>")
    .DangerouslyAllowScripts()
```

**Important Note:** The `Html` widget renders in an iframe, so CSS variables like `var(--primary)` do not resolve - use hardcoded color values instead.

This documentation helps clarify the proper way to style components and provides clear examples for both standard styling and advanced CSS use cases.

---

### Dynamic Form Fields - FAQ Entry

A new FAQ entry has been added to the TextInput documentation explaining how to create forms with a dynamic number of fields, such as dictionary-style inputs. This addresses a common challenge developers face when trying to use hooks inside loops.

**The Question:** How to create a form with a dynamic number of fields (e.g. dictionary input)?

**The Problem:** Since hooks cannot be called inside loops (IVYHOOK003), you cannot use `UseState` in a `for`/`foreach`/LINQ loop.

**The Solution:** Use **one state variable** that holds all field values in a dictionary:

```csharp
public override object Build()
{
    var columns = GetColumnNames(); // e.g. ["Name", "Age", "City"]
    var values = UseState(new Dictionary<string, string>());

    var layout = Layout.Vertical();
    foreach (var col in columns)
    {
        var currentValue = values.Value.GetValueOrDefault(col, "");
        layout.Add(
            new TextInput(currentValue, e =>
            {
                var updated = new Dictionary<string, string>(values.Value) { [col] = e.Value };
                values.Set(updated);
            })
            .Placeholder(col)
            .WithField()
            .Label(col)
        );
    }
    return layout;
}
```

**Key Points:**

- Only one `UseState` call at the top level — no hook rule violations
- The dictionary keys map to column names, values map to user input
- Create a new dictionary on each update to trigger a re-render
- This pattern works for any dynamic input scenario (forms, dialogs, etc.)

This pattern is essential for building flexible forms where the number of fields is determined at runtime, such as database column editors, custom property panels, or dynamic configuration screens.
