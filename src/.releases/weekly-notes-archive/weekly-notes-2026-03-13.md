# Ivy Framework Weekly Notes - Week of 2026-03-13

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

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

The `WrapLayout` widget has been removed. Use [StackLayout](https://docs.ivy.app/onboarding/concepts/layout) with the new `.Wrap()` method instead.

```csharp
Layout.Horizontal()
    .Wrap()
    .Gap(4)
    | new Badge("React")
    | new Badge("Vue")
    | new Badge("Angular");
```

### Button Icon API - Constructor Parameter Removed

The [Button](https://docs.ivy.app/widgets/common/button) widget no longer accepts an `icon` constructor parameter. Use the fluent `.Icon()` method instead to add icons to buttons.

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

**DpiScaling  to  UseDpiScaling and DevToolsEnabled  to  UseDevTools:**

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

The `CreateSignal<T, TInput, TOutput>()` method has been removed and replaced by [UseSignal](https://docs.ivy.app/hooks/core/use-signal)`<T, TInput, TOutput>()`. Additionally, the separate `ISignalSender` and `ISignalReceiver` interfaces have been unified into a single `ISignal` interface that provides both sending and receiving capabilities.

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

The fluent `.Value()` extension method has been removed from all input widgets. All input widgets are affected ([TextInput](https://docs.ivy.app/widgets/inputs/text-input), [SelectInput](https://docs.ivy.app/widgets/inputs/select-input), [AsyncSelectInput](https://docs.ivy.app/widgets/inputs/async-select-input), [NumberInput](https://docs.ivy.app/widgets/inputs/number-input), [BoolInput](https://docs.ivy.app/widgets/inputs/bool-input), [CodeInput](https://docs.ivy.app/widgets/inputs/code-input), [ColorInput](https://docs.ivy.app/widgets/inputs/color-input), [DateRangeInput](https://docs.ivy.app/widgets/inputs/date-range-input), [DateTimeInput](https://docs.ivy.app/widgets/inputs/date-time-input), [FeedbackInput](https://docs.ivy.app/widgets/inputs/feedback-input), IconInput, and ReadOnlyInput).

### Scale Renamed to Density

The `Scale` enum and all associated APIs have been renamed to `Density`.

- `Ivy.Scale` enum  to  `Ivy.Density` enum
- `.Scale()` fluent method  to  `.Density()` method
- Enum values remain unchanged: `Small`, `Medium`, `Large`
- Shortcut methods `.Small()`, `.Medium()`, `.Large()` are unchanged

### Box.Color() Renamed to Box.Background()

The `Color()` method and property on the [Box](https://docs.ivy.app/widgets/primitives/box) widget have been renamed to `Background()`.

### Text.InlineCode() Renamed to Text.Monospaced()

The `Text.InlineCode()` method and `TextVariant.InlineCode` enum value on the [Text](https://docs.ivy.app/widgets/primitives/text-block) helper have been renamed to `Text.Monospaced()` and `TextVariant.Monospaced`.

### Explicit Size API for Width, Height, and Size Methods

The implicit numeric overloads for `Width()`, `Height()`, and `Size()` methods have been removed. You now must explicitly use [Size.Units()](https://docs.ivy.app/api-reference/ivy-shared/size) or `Size.Fraction()` to specify sizing.

### Size.Fraction - Decimal/Double Overloads Removed

The `decimal` and `double` overloads for `Size.Fraction()` have been removed to fix ambiguous call compilation errors (CS0121). You must now use `float` values with the `f` suffix.

```csharp
// Use explicit float literals with 'f' suffix
.Width(Size.Fraction(0.5f))

// Or cast explicitly if using decimal/double variables
decimal ratio = 0.333m;
.Width(Size.Fraction((float)ratio))
```

### Size.Percent() - Intuitive Percentage-Based Sizing

New [Size.Percent()](https://docs.ivy.app/api-reference/ivy-shared/size) overloads allow you to specify percentage-based sizes.

**New overloads:**

```csharp
// Integer percentage
.Width(Size.Percent(50))    // 50% width
.Height(Size.Percent(100))  // 100% height
```

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

### Async Cleanup in UseEffect with IAsyncDisposable

[UseEffect](https://docs.ivy.app/hooks/core/use-effect) now supports asynchronous cleanup through `IAsyncDisposable`.

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

Ivy's [layout](https://docs.ivy.app/onboarding/concepts/layout) system now supports advanced Figma-style layout options, including space distribution, independent row/column gaps, wrapping, per-child alignment, and enhanced scroll control.

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

[LayoutView](https://docs.ivy.app/onboarding/concepts/layout) and StackLayout support borders: `.Border(color, thickness)` or fine-grained `.BorderColor()`, `.BorderThickness()`, `.BorderStyle()`, `.BorderRadius()`.

```csharp
Layout.Vertical()
    .Border(Colors.Red, new Thickness(top: 2, right: 1, bottom: 2, left: 1))
    | new Text("Custom border");

Layout.Vertical()
    .BorderColor(Colors.Primary).BorderThickness(2).BorderStyle(BorderStyle.Solid).BorderRadius(BorderRadius.Rounded)
    | new Text("Fully customized border");
```

### PWA (Progressive Web App) Manifest Support

Ivy now supports Progressive Web Apps with built-in manifest configuration. Configure your app's PWA settings using the new `UseManifest()` API.

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

The [Field](https://docs.ivy.app/widgets/inputs/field) widget now supports horizontal label layouts where labels appear beside inputs instead of above them.

```csharp
// Horizontal layout - label on left
var emailField = new Field(
    new TextInput("Email"),
    label: "Email Address"
).LabelPosition(LabelPosition.Left);
```

### Form Submit Strategies

[Forms](https://docs.ivy.app/onboarding/concepts/forms) now support different submit strategies that control when form state is committed back to your model.

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
- **[ListItem](https://docs.ivy.app/widgets/common/list)**: `.Title()`, `.Subtitle()`, `.Icon()`, `.Badge()`, `.Tag()`, `.OnClick()`, `.Content()`, `.Disabled()`
- **[FeedbackInput](https://docs.ivy.app/widgets/inputs/feedback-input)**: Dedicated fluent methods for each variant type.
- **Chart Builders**: `.Height()` and `.Width()` (replaces `Polish` callback workaround).
- **DesktopWindow**: `.UseDpiScaling()`, `.UseDevTools()`, `.Resizable()`, `.Center()`, `.TopMost()` (booleans default to `true`).
- **[Table](https://docs.ivy.app/widgets/common/table) Progress**: `.Min()`, `.Max()`, `.AutoColor()`, `.Color()`, `.Format()`.
- **[Separator](https://docs.ivy.app/widgets/primitives/separator)**: `.TextAlign(TextAlignment.Left | Center | Right)`.
- **Global**: `.Grow()` is now available on all widgets (shorthand for [`.Width(Size.Grow())`](https://docs.ivy.app/api-reference/ivy-shared/size)).

### ColorInput: Alpha Channel Support

The [ColorInput](https://docs.ivy.app/widgets/inputs/color-input) widget now supports transparency with the new `AllowAlpha()` method. When enabled, an opacity slider appears next to the color picker, and colors are stored in `#RRGGBBAA` format (8-digit hex with alpha channel).

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

The DetailsBuilder (see [Details](https://docs.ivy.app/widgets/common/details)) now supports customizing field labels with the new `.Label()` method. By default, `ToDetails()` generates labels from property names using PascalCase splitting (e.g., `NetBurn` becomes "Net Burn"), but you can now override these auto-generated labels with custom text.

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

```csharp
// Version namespacing
[App(Id = "dashboard.v2")]
public class DashboardV2 : AppBase { }
```

### Icons in Select Options

[SelectInput](https://docs.ivy.app/widgets/inputs/select-input) now supports optional icons for each option. Additionally, labels are now optional—if omitted, the option value will be displayed instead.

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

### Progress Builder for Table Cells

Custom Range and Format String:

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

### Server-to-Client Streaming with UseStream Hook

Ivy now supports efficient server-to-client streaming with the new UseStream hook.

Attach the stream to widgets that support streaming (like Text.Rich()):

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

### RichTextBlock - Styled Text with Links and Streaming

The [RichTextBlock](https://docs.ivy.app/widgets/primitives/richtextblock) widget supports styled runs (bold, italic, strikethrough, color, highlight, word spacing), hyperlinks (with `LinkTarget.Blank`), programmatic `OnLinkClick`, and streaming via `Text.Rich().UseStream(stream)` or the `Stream` property with `Stream(streamId, run)`. Also: `TextAlignment`, `NoWrap`, `Overflow`, `Density`.

```csharp
// Styling, links, OnLinkClick
var richText = new RichTextBlock
{
    Runs = new List<TextRun>
    {
        new() { Content = "Bold ", Bold = true, Word = true },
        new() { Content = "italic ", Italic = true, Word = true },
        new() { Content = "strikethrough ", StrikeThrough = true, Word = true },
        new() { Content = "colored ", Color = "Red", Word = true },
        new() { Content = "highlighted ", HighlightColor = "Yellow", Word = true },
        new() { Content = "Visit ", Word = true },
        new() { Content = "docs", Link = "https://docs.example.com", LinkTarget = LinkTarget.Blank, Word = true },
        new() { Content = " or ", Word = true },
        new() { Content = "action", Link = "/action", Word = true }
    },
    OnLinkClick = (url) => Console.WriteLine($"Clicked: {url}")
};

// Streaming: builder API or by stream ID
var stream = Context.UseStream<TextRun>();
var streamingText = Text.Rich().Bold("🤖 ").UseStream(stream);  // stream.Write(new TextRun(word) { Word = true });
var streamingBlock = new RichTextBlock { Stream = "chat", Runs = [new() { Content = "AI: ", Bold = true }] };  // Stream("chat", new TextRun { Content = "Hi!", Word = true });
```

### ReadOnlyInput - Copy Button and Placeholder Support

[ReadOnlyInput](https://docs.ivy.app/widgets/inputs/read-only-input) supports `.ShowCopyButton(bool)` (default true) and `.Placeholder(text)` for empty values:

```csharp
var apiKey = UseState("sk-1234567890abcdef");
var result = UseState("");

return Layout.Vertical()
    | apiKey.ToReadOnlyInput().ShowCopyButton()           // copy visible (default)
    | apiKey.ToReadOnlyInput().ShowCopyButton(false)     // copy hidden
    | result.ToReadOnlyInput().Placeholder("No data available");
```

### BoolInput - Loading State Support

The [BoolInput](https://docs.ivy.app/widgets/inputs/bool-input) widget now supports a loading state across all variants (Checkbox, Switch, and Toggle). When in loading state, the widget displays a spinner overlay and is automatically disabled to prevent user interaction during async operations.

```csharp
var isEnabled = UseState(true);
var isLoading = UseState(true);

return isEnabled.ToSwitchInput()
    .Label("Enable Feature")
    .Loading(isLoading.Value);
```

### DateTimeInput - Month, Week, and Year Pickers

The [DateTimeInput](https://docs.ivy.app/widgets/inputs/date-time-input) widget now supports three additional variants for selecting time periods: Month, Week, and Year.

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

The [Box](https://docs.ivy.app/widgets/primitives/box) widget now includes a convenient `Grow()` extension method for making boxes expand to fill available width. This is a shorthand for `.Width(Size.Grow())`.

```csharp
new Box("Content").Grow();
```

### Callout - Closable Callouts with OnClose Event

The [Callout](https://docs.ivy.app/widgets/primitives/callout) widget now supports closable behavior through the `OnClose` event handler. When an `OnClose` handler is set, the callout displays a close (X) button in the top-right corner.

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

String and string array properties with the `[AllowedValues]` attribute are now automatically scaffolded as [SelectInput](https://docs.ivy.app/widgets/inputs/select-input) widgets (single or multi-select) when using [.ToForm()](https://docs.ivy.app/onboarding/concepts/forms).

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

### GridView: Separate RowGap and ColumnGap Methods

[GridView](https://docs.ivy.app/onboarding/concepts/layout) now has `RowGap()` and `ColumnGap()` for per-axis spacing; `Gap()` still sets both.

### Theme Defaults to System Preference

The framework now defaults to the system light/dark preference instead of light theme.

### AddConnectionsFromAssembly - Optional Assembly Parameter

`Server.AddConnectionsFromAssembly()` now accepts an optional `Assembly?` parameter so you can specify which assembly to scan for `IConnection` types (e.g. plugin or class-library scenarios). Omitting it still scans the entry assembly.

### DataTableBuilder - Remove() Method for API Consistency

The [DataTable](https://docs.ivy.app/widgets/advanced/data-table) builder now supports the `.Remove()` method. This method allows you to completely exclude columns from your data tables.

### Clerk Auth: Graceful Handling of Existing Sessions

The Clerk authentication provider now gracefully handles scenarios where a session already exists during sign-in, making the authentication flow more robust and user-friendly.

- When signing in with a session already active, the provider now attempts to restore and reuse the existing session. If restoration fails, it automatically cleans up stale sessions and retries the sign-in. This eliminates sign-in failures that could occur in edge cases like browser back/forward navigation or concurrent sign-in attempts.

### WithConfirm: Customizable Button Labels and Destructive Styling

The `WithConfirm` helper method now supports customizable confirm button labels and destructive styling.

### Desktop Apps: Default Ivy Icon and Embedding

Desktop windows without a custom `.Icon()` show the Ivy logo in the taskbar and title bar. The Ivy.Desktop package embeds `ivy.ico` in the built .exe by default (skip by setting `ApplicationIcon` in `.csproj`).

### DefaultSidebarChrome: Auto-Open Sidebar When Last Tab Closes

Closing the last tab now auto-opens the sidebar so navigation stays visible.

### Nested App Streaming Support

`UseStream` works in nested apps via `AppHostWidget`; stream subscriptions propagate to child apps. `RichTextBlock.UseStream()` and `Terminal.UseStream()` work in nested contexts.

### Desktop Apps: Error Dialog for Unhandled Exceptions

Unhandled exceptions now show a native error dialog (message + stack trace) instead of failing silently or using `Console.WriteLine`.

### Desktop Apps: Startup and Reliability

The window shows a loading screen and waits for the server to be ready (health check) before loading the UI, avoiding "connection refused." On Windows, `DesktopWindow.Run()` ensures an STA thread for WebView2 so the window no longer opens blank.

### SelectInput: Auto-Flip Dropdown Near Viewport Edge

[SelectInput](https://docs.ivy.app/widgets/inputs/select-input) dropdown opens upward when there isn’t enough space below; works for all variants.

### Dynamic Metric Progress Colors

The `MetricView` component now colors its progress bar based on achievement percentage.

### Graceful Handling of Missing Assembly References

Assembly scanning (apps, widgets, connections, extensions) now uses `GetLoadableTypes()` so missing optional references (e.g. Ivy.Filters not deployed) no longer cause `ReflectionTypeLoadException` and crashes.

## Developer Experience Improvements

### Compile-Time Analyzer for App Constructor Requirements

Roslyn analyzer **IVYAPP001** flags `[App]` classes that use constructor injection (Ivy instantiates apps via `Activator.CreateInstance`). Use `UseService<T>()` inside `Build()` instead. `Server.UseChrome<T>()` and `Server.UseErrorNotFound<T>()` now have `new()` constraints; runtime validation in `AppDescriptor.CreateApp()` gives a clear error if issues are missed.

### Compile-Time Analyzers for Widget Child Misuse

- **IVYCHILD001**: Flags adding children to leaf widgets (Button, Badge, inputs, charts, DataTable, etc.).
- **IVYCHILD002**: Warns when multiple children are added to single-child widgets (Card, Sheet, FloatingPanel, etc.); wrap in a layout instead.
- **IVYCHILD003**: Enforces `[ChildType]` so only allowed child types (e.g. MenuItem for DropDownMenu) are accepted. Widget authors can use `[ChildType(typeof(T))]`; the analyzer checks direct children, arrays, and `IEnumerable<T>`.

### Compile-Time Analyzer for Hook Results in Class Members

**IVYHOOK006** flags when `Use*` hook results are assigned to class fields or properties instead of local variables. Storing hook results in class members breaks Ivy's hook indexing and leads to wrong indices across renders. Store in local variables or discard.

### Hook Usage Analyzer - Clearer Error Messages

- **IVYHOOK001** now only fires for hooks called outside `Build()` (e.g. in helper methods).
- **IVYHOOK001B** fires for hooks inside lambdas, local functions, or anonymous methods within `Build()`.
- Error messages explain the "same order on every render" constraint and name the closure type when relevant.

### Size.Fraction - Decimal/Double Overloads Removed

The `decimal` and `double` overloads have been removed to fix ambiguous call errors (CS0121). Use `float` with the `f` suffix or cast: `Size.Fraction(0.5f)`, `Size.Fraction((float)ratio)`.

### Size.Percent() - Intuitive Percentage-Based Sizing

New overloads: `Size.Percent(50)` (integer) and `Size.Percent("75%")` (string) for percentage-based width/height.

### Connection Name Error Messages

When using `--test-connection` or `--describe-connection` with a non-existent connection name, the error message now lists all available connections.

### CLI Commands Work Alongside Running Instances

CLI diagnostic commands (`--describe`, `--describe-connection`, `--test-connection`) run successfully even when an Ivy app is already running on the configured port.

### Server Binds to Localhost

Ivy apps now bind to `localhost` instead of the wildcard address, eliminating Windows Firewall prompts during development.

## Bug Fixes

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
