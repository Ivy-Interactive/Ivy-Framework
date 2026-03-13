# Ivy Framework Weekly Notes - Week of 2026-03-13

## Breaking Changes

### IHtmlFilter Interface - XDocument Instead of String Manipulation

The `IHtmlFilter.Process` method now takes an `XDocument` instead of a raw HTML string, and returns `void` instead of `string`. The namespace has also changed from `Ivy.Core.Server.ContentPipeline` to `Ivy.Core.Server.HtmlPipeline`.

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

### IConnection.RegisterServices - Server Parameter Instead of IServiceCollection

The `IConnection.RegisterServices` method signature has changed to accept a `Server` instance instead of an `IServiceCollection`.

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

### WrapLayout Removed - Use StackLayout with Wrap Instead

The `WrapLayout` widget has been removed. Use `StackLayout` with the new `.Wrap()` method instead.

```csharp
Layout.Horizontal()
    .Wrap()
    .Gap(4)
    | new Badge("React")
    | new Badge("Vue")
    | new Badge("Angular");
```

### Button Icon API - Constructor Parameter Removed

The `Button` widget no longer accepts an `icon` constructor parameter. Use the fluent `.Icon()` method instead to add icons to buttons.

```csharp
// Icon via fluent method
new Button("Save").Icon(Icons.Save)
```

### OAuth Callback URL Path Change

The OAuth authentication callback URL has changed from `/ivy/webhook` to `/ivy/auth/callback`.

- Local development: `http://localhost:5010/ivy/auth/callback`
- Production: `https://your-app.com/ivy/auth/callback`

### DesktopWindow API Improvements

Two `DesktopWindow` methods have been renamed to follow the `Use*` pattern:

**DpiScaling → UseDpiScaling and DevToolsEnabled → UseDevTools:**

```csharp
new DesktopWindow(server)
    .Title("My App")
    .Size(1280, 720)
    .UseDpiScaling(true)
    .UseDevTools(true)
    .Run();
```

### Chart Data Syntax - XML DataPoint Replaced by JSON

Chart data in XAML now uses JSON arrays inside CDATA sections instead of `<DataPoint>` XML elements.

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

### CreateSignal Renamed to UseSignal and ISignal Unified

The `CreateSignal<T, TInput, TOutput>()` method has been removed and replaced by `UseSignal<T, TInput, TOutput>()`. Additionally, the separate `ISignalSender` and `ISignalReceiver` interfaces have been unified into a single `ISignal` interface that provides both sending and receiving capabilities.

```csharp
// Both sending and receiving are now handled by ISignal
ISignal<string, bool> mySignal = context.UseSignal<MySignal, string, bool>();

// The same instance provides both methods:
mySignal.Send(input);           // Send data through the signal
mySignal.Receive(callback);     // Register a callback to receive data
```

### Input Variant Enums Renamed to Singular

All input variant enums have been renamed from plural to singular form.

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

### Removal of `.Value()` API from Input Widgets

The fluent `.Value()` extension method has been removed from all input widgets. All input widgets are affected (`TextInput`, `SelectInput`, `AsyncSelectInput`, `NumberInput`, `BoolInput`, `CodeInput`, `ColorInput`, `DateRangeInput`, `DateTimeInput`, `FeedbackInput`, `IconInput`, and `ReadOnlyInput`).

### Scale Renamed to Density

The `Scale` enum and all associated APIs have been renamed to `Density`.

- `Ivy.Scale` enum → `Ivy.Density` enum
- `.Scale()` fluent method → `.Density()` method
- Enum values remain unchanged: `Small`, `Medium`, `Large`
- Shortcut methods `.Small()`, `.Medium()`, `.Large()` are unchanged

### Box.Color() Renamed to Box.Background()

The `Color()` method and property on the `Box` widget have been renamed to `Background()`.

### Text.InlineCode() Renamed to Text.Monospaced()

The `Text.InlineCode()` method and `TextVariant.InlineCode` enum value have been renamed to `Text.Monospaced()` and `TextVariant.Monospaced`.

### Explicit Size API for Width, Height, and Size Methods

The implicit numeric overloads for `Width()`, `Height()`, and `Size()` methods have been removed. You now must explicitly use `Size.Units()` or `Size.Fraction()` to specify sizing.

## New Features

### Terminal Emulator Widget with Xterm.js

Ivy now includes a terminal emulator widget through the `Ivy.Widgets.Xterm` package, powered by xterm.js.

**Installation:**

```bash
dotnet add package Ivy.Widgets.Xterm
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

Ivy now includes a screenshot and annotation widget through the `Ivy.Widgets.ScreenshotFeedback` package.

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

### Server-to-Client Streaming with UseStream Hook

Ivy now supports efficient server-to-client streaming with the new `UseStream` hook.

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

### Async Cleanup in UseEffect with IAsyncDisposable

`UseEffect` now supports asynchronous cleanup through `IAsyncDisposable`.

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

Ivy now includes built-in DevTools for debugging and inspecting your widget tree during development.

**Enable DevTools:**

```csharp
var server = new Server()
    .EnableDevTools()  // Only in development builds
    .Run();
```

### Enhanced Layout System with Figma-Style Options

Ivy's layout system now supports advanced Figma-style layout options, including space distribution, independent row/column gaps, wrapping, per-child alignment, and enhanced scroll control.

**New Alignment Options:**

The `Align` enum now includes space distribution options that work with both `StackLayout` and `GridLayout`:

```csharp
// Space distribution
Layout.Horizontal()
    .Align(Align.SpaceBetween)
    | new Button("Left")
    | new Button("Middle")
    | new Button("Right");
```

**Independent Row and Column Gaps:**

Control row and column spacing independently in both `StackLayout` and `GridLayout`:

```csharp
new Grid()
    .Columns("1fr 1fr 1fr")
    .RowGap(4)
    .ColumnGap(8)
    | child1
    | child2
    | child3;
```

**Wrapping StackLayouts:**

`StackLayout` now supports wrapping, eliminating the need for a separate `WrapLayout` widget:

```csharp
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

### AppBase - Semantic Base Class for Apps

Ivy now includes an `AppBase` class that provides a base foundation for building apps.

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

### HtmlPipeline - XDocument-Based Filters and Full Customization

The HTML pipeline has been refactored to use `XDocument`. Filters now work with parsed XML instead of raw strings, and new APIs allow full pipeline customization.

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
```

The pipeline configurator runs after all built-in and custom filters have been added, so `Clear()` removes everything for complete control.

### Field - Horizontal Label Layout with LabelPosition

The `Field` widget now supports horizontal label layouts where labels appear beside inputs instead of above them.

```csharp
// Horizontal layout - label on left
var emailField = new Field(
    new TextInput("Email"),
    label: "Email Address"
).LabelPosition(LabelPosition.Left);
```

### Form Submit Strategies

Forms now support different submit strategies that control when form state is committed back to your model.

**Available Strategies:**

- `OnSubmit` (default) — State is committed only when the submit button is clicked
- `OnBlur` — State is committed when any field loses focus (submit button hidden)
- `OnChange` — State is committed on every field value change (submit button hidden)

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

### Fluent API Enhancements

Several widgets have received new fluent API extensions:

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

### DetailsBuilder: Custom Field Labels

The `DetailsBuilder` now supports customizing field labels with the new `.Label()` method. By default, `ToDetails()` generates labels from property names using PascalCase splitting (e.g., `NetBurn` becomes "Net Burn"), but you can now override these auto-generated labels with custom text.

```csharp
public record RunwayData(decimal NetBurn, decimal GrossBurn, int Months, DateTime RunwayDate);

var data = new RunwayData(5000m, 10000m, 12, new DateTime(2027, 3, 1));
data.ToDetails()
    .Label(x => x.NetBurn, "Net Monthly Burn")
    .Label(x => x.RunwayDate, "Projected Runway End")
    .Build();
```

### Dots Now Allowed in App IDs

App IDs can now include dots. Previously, app IDs like `app.v2` or `users.profile` were not allowed.

**New Capabilities:**

```csharp
// Version namespacing
[App(Id = "dashboard.v2")]
public class DashboardV2 : AppBase { }
```

### Icons in Select Options

Select inputs now support optional icons for each option. Additionally, labels are now optional—if omitted, the option value will be displayed instead.

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
Stream text runs in real-time for dynamic content:

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
- `Density` - Set text size (Small, Large)

### ReadOnlyInput - Copy Button and Placeholder Support

The `ReadOnlyInput` widget now supports two new fluent extension methods:

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

### TextInput - OnSubmit Event for Enter Key Handling

The `TextInput` widget now supports an `OnSubmit` event that fires when the user presses Enter in a single-line text input.

```csharp
var searchQuery = UseState("");

return searchQuery.ToTextInput()
    .Placeholder("Search...")
    .OnSubmit(() => PerformSearch(searchQuery.Value));
```

### DateTimeInput - Month, Week, and Year Pickers

The `DateTimeInput` widget now supports three additional variants for selecting time periods: Month, Week, and Year.

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

### Box Grow Extension Method

The `Box` widget now includes a convenient `Grow()` extension method for making boxes expand to fill available width. This is a shorthand for `.Width(Size.Grow())`.

```csharp
new Box("Content").Grow();
```

### Callout - Closable Callouts with OnClose Event

The `Callout` widget now supports closable behavior through the `OnClose` event handler. When an `OnClose` handler is set, the callout displays a close (X) button in the top-right corner.

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

### Forms - Auto-Scaffold [AllowedValues] as SelectInput

String and string array properties with the `[AllowedValues]` attribute are now automatically scaffolded as `SelectInput` widgets (single or multi-select) when using `.ToForm()`.

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
    return settings.ToForm();
}
```

### Ivy.Desktop - Run Ivy Apps as Native Desktop Applications

The new `Ivy.Desktop` library enables you to wrap your Ivy web applications as native desktop applications using Photino, providing cross-platform support for Windows, macOS, and Linux.

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

### Server Configuration - External Configuration Providers

The `Server` class now supports extending the default configuration pipeline with external configuration sources through the `UseConfiguration` method. This enables you to add custom configuration providers like Azure Key Vault, AWS Secrets Manager, or any other configuration source while preserving the built-in defaults (environment variables, appsettings.json, user secrets).

```csharp
var server = new Server(args);

server.UseConfiguration(config => {
    config.AddJsonFile("custom-config.json", optional: true);
});
```

## Improvements

### DataTableBuilder - Remove() Method for API Consistency

The `DataTableBuilder` now supports the `.Remove()` method. This method allows you to completely exclude columns from your data tables.

### Clerk Auth: Graceful Handling of Existing Sessions

The Clerk authentication provider now gracefully handles scenarios where a session already exists during sign-in, making the authentication flow more robust and user-friendly.

- When signing in with a session already active, the provider now attempts to restore and reuse the existing session. If restoration fails, it automatically cleans up stale sessions and retries the sign-in. This eliminates sign-in failures that could occur in edge cases like browser back/forward navigation or concurrent sign-in attempts.

### WithConfirm: Customizable Button Labels and Destructive Styling

The `WithConfirm` helper method now supports customizable confirm button labels and destructive styling.

### Desktop Apps: Default Ivy Icon for Windows

Desktop applications now automatically display the Ivy logo in the taskbar and title bar when no custom icon is explicitly set.

- When you create a desktop window without calling `.Icon()`, it now automatically uses the embedded Ivy icon
- The ivy.ico resource is embedded directly in the `Ivy.Desktop` package
- Your app's window will show the Ivy logo in the taskbar and window title bar by default

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

### DefaultSidebarChrome: Auto-Open Sidebar When Last Tab Closes

The `DefaultSidebarChrome` now automatically opens the sidebar when you close the last tab, preventing an empty state where both the sidebar and all tabs are closed.

- When closing the final tab (which redirects to the home page), the sidebar automatically opens
- This ensures users always have navigation options visible
- The sidebar state is now dynamically managed to respond to tab closure events

### Nested App Streaming Support

The `UseStream` hook now works seamlessly in nested apps hosted via `AppHostWidget`. Previously, streaming functionality might not have worked correctly when using `UseStream` within an app that's hosted inside another app.

- `AppHostWidget` now provides the necessary streaming infrastructure to child apps
- Stream subscriptions are properly propagated through nested app boundaries
- Both `RichTextBlock.UseStream()` and `Terminal.UseStream()` work correctly in nested contexts

No code changes needed - this improvement applies automatically to applications using `DefaultSidebarChrome`.

### Desktop Apps: Error Dialog for Unhandled Exceptions

Desktop applications now show a proper error dialog when unhandled exceptions occur, instead of silently failing or writing to an invisible console.

- The `DesktopWindow.Run()` method now wraps execution in error handling
- Unhandled exceptions trigger a native error dialog window with the error message and full stack trace
- The error dialog features a clean, modern design using the **Geist font** and light theme
- Light background with a subtle bordered code block for stack traces
- This replaces the previous `Console.WriteLine` approach which was invisible in Windows GUI apps

**Example error scenarios now handled:**

- Server fails to start on the expected port
- Initialization errors during app startup
- Any unhandled exceptions during the window lifecycle

No code changes needed - this protection applies automatically to all Ivy desktop applications.

### Desktop Apps: WebView2 Threading Fix for Windows

Desktop applications now automatically handle threading requirements for WebView2 on Windows, fixing an issue where windows would open but display blank content.

- `DesktopWindow.Run()` automatically detects the thread apartment state on Windows
- If not on an STA thread, it creates and switches to an STA thread automatically
- Works seamlessly without any code changes or performance impact

No code changes needed - this fix applies automatically to all Ivy desktop applications.

### Desktop Apps: Automatic Ivy Icon Embedding

Desktop applications now automatically include the Ivy icon in their executable files.

- The Ivy.Desktop package now includes an `ivy.ico` icon file
- MSBuild automatically sets the `ApplicationIcon` property during build if you haven't specified a custom icon
- The icon is embedded in your compiled `.exe` file on all platforms

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

### Desktop Apps: Server Readiness Check Prevents Premature Loading

Desktop applications now wait for the server to be fully ready before loading the UI.

- The port was read before the server had actually bound to it, potentially using the wrong port number
- The WebView could navigate to the URL before the server was accepting requests, causing connection failures

- The actual bound port is now read after `RunAsync()` returns, ensuring the correct port is used
- A new `WaitForServerReady()` method polls the server's `/ivy/health` endpoint before the window loads
- Detects if the server task faults or exits early (e.g., missing secrets, port conflicts)
- 30-second timeout with clear error message if the server doesn't become ready

- The server takes a moment to initialize and bind to a port
- The server fails early due to configuration issues (missing secrets, invalid configuration)
- Port conflicts or other startup issues occur

The window will only display your app once the server is confirmed to be accepting HTTP requests, eliminating "connection refused" errors during startup.

**Technical details:**
The health check polls `/ivy/health` every 250ms with a 2-second request timeout. The check runs on the main thread before the WebView is navigated, ensuring synchronous startup flow with proper error handling.

No code changes needed - this reliability improvement applies automatically to all Ivy desktop applications.

### Graceful Handling of Missing Assembly References

The framework now gracefully handles situations where your application references assemblies that aren't deployed, preventing crashes during assembly scanning operations.

- Assembly scanning operations (for apps, widgets, connections, and extensions) now use a new `GetLoadableTypes()` extension method
- When an assembly references other assemblies that aren't available (e.g., optional packages like `Ivy.Filters`), the framework loads only the types that are available
- Previously, these situations would throw a `ReflectionTypeLoadException` and crash your application
- This affects automatic discovery of: app classes, external widgets, database connections, and extension methods

This is particularly useful for:

- Deploying minimal production builds without optional dependencies
- Development scenarios where not all packages are installed
- Modular applications where certain features are conditionally deployed

No code changes needed - this protection applies automatically throughout the framework.

### SelectInput: Auto-Flip Dropdown Near Viewport Edge

The `SelectInput` dropdown now automatically detects when it would extend beyond the bottom of the viewport and intelligently flips to open upward instead.

- The dropdown calculates available space below the trigger element when opening
- If insufficient space exists (less than the dropdown height + 8px), it automatically opens upward
- The dropdown smoothly positions itself above the input with appropriate spacing
- Works seamlessly across all SelectInput variants (Select, List, Toggle, etc.)

No code changes needed - this improvement applies automatically to all SelectInput widgets.

### Dynamic Metric Progress Colors

The `MetricView` component now colors its progress bar based on achievement percentage.

### Size.Fraction and Size.FractionGap Now Accept Decimal and Double

The `Size.Fraction()` and `Size.FractionGap()` methods now accept `decimal` and `double` values in addition to `float`.

- Form Submit Strategy Hook Ordering
- RichTextBlock Stream Subscription Fix
- NumberInput Currency Format Default
- Stream Data Serialization Fix
- Stream Data Buffering - Preventing Dropped Messages
- HtmlPipeline XML Parsing Fix for Vite-Generated HTML
- ClientSender Disposal Race Condition
- Chart Toolbox Overlap Fix
- Missing HttpClient Dependency Fix
- Table of Contents - Smooth Scrolling Without Visual Glitches
- `IState<T>.Set(null)` Ambiguity Resolved
- Desktop Window Title Default
- Hook Usage Analyzer: FuncView and MemoizedFuncView Lambda Support
- Chart Legend Title-Casing Fix
- Semantic Color Mapping for Text
- SidebarLayout - Respect Open Property on Mount
- MarkdownRenderer Code Block Borders
- Desktop Error Dialog Display Fix
- Desktop WebView2 Blank Window Fix
- Better Desktop Startup Error Messages
- Assembly Scanning - Missing Reference Resilience
- Outline Button Missing Background
- Semantic Color Text Readability Fix

## Developer Experience Improvements

### Compile-Time Analyzer for App Constructor Requirements

A new Roslyn analyzer (`IVYAPP001`) now provides compile-time feedback when `[App]`-attributed classes don't have a parameterless constructor.

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

### Compile-Time Analyzer for Widget Child Misuse

New Roslyn analyzers (`IVYCHILD001`, `IVYCHILD002`, and `IVYCHILD003`) now catch widget child misuse at compile time.

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

### Compile-Time Analyzer for Hook Results Stored in Class Members

A new Roslyn analyzer (`IVYHOOK006`) now detects when hook results are incorrectly stored in class fields or properties.

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

### Hook Usage Analyzer - Clearer Error Messages with Sub-Types

The hook usage analyzer now provides more specific error messages by splitting `IVYHOOK001` into sub-types.

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

### Size.Fraction and Size.FractionGap - Decimal/Double Overloads Removed

The `decimal` and `double` overloads for `Size.Fraction()` and `Size.FractionGap()` have been removed to fix ambiguous call compilation errors (CS0121). You must now use `float` values with the `f` suffix.

```csharp
// Use explicit float literals with 'f' suffix
.Width(Size.Fraction(0.5f))
.Height(Size.FractionGap(0.25f))

// Or cast explicitly if using decimal/double variables
decimal ratio = 0.333m;
.Width(Size.Fraction((float)ratio))
```

### Size.Percent() - Intuitive Percentage-Based Sizing

New `Size.Percent()` overloads allow you to specify percentage-based sizes.

**New overloads:**

```csharp
// Integer percentage
.Width(Size.Percent(50))    // 50% width
.Height(Size.Percent(100))  // 100% height
```

### Connection Name Error Messages

When using `--test-connection` or `--describe-connection` command-line arguments with a connection name that doesn't exist, the error message now lists all available connections.

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

### CLI Commands Work Alongside Running Instances

CLI diagnostic commands (`--describe`, `--describe-connection`, `--test-connection`) now run successfully even when an Ivy app instance is already running on the configured port.

### Server Binds to Localhost - No More Windows Firewall Prompts

Ivy apps now bind to `localhost` instead of the wildcard address (`*`), eliminating Windows Firewall prompts during development.
