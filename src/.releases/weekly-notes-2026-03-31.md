# Ivy Framework Weekly Notes - Week of 2026-03-31

## Performance Improvements

### Event Handling Optimization

Event dispatching in widgets is now significantly faster. The framework now caches reflection lookups when invoking widget events, eliminating repeated reflection overhead on every event call. This improvement is automatic and requires no code changes - your existing event handlers will simply run faster.

### Rust-Powered Core Engine

The framework core now leverages Rust for performance-critical operations including JSON diffing and tree synchronization. These optimizations deliver faster UI updates and reduced latency, particularly noticeable in applications with frequent state changes or large component trees. The improvements are automatic - no code changes required.

## New Widgets

### DiffView Widget

A new `DiffView` widget for displaying unified diffs (git diff output) in either unified or split view mode. Perfect for code review interfaces, version comparison tools, or any application that needs to show file changes. The widget is powered by the popular [react-diff-view](https://github.com/otakustay/react-diff-view) library.

Install the widget:

```bash
dotnet add package Ivy.Widgets.DiffView
```

Basic usage shows a unified diff:

```csharp
using Ivy.Widgets.DiffView;

new DiffView()
    .Diff(myDiffString)
```

Switch to split view for side-by-side comparison:

```csharp
new DiffView()
    .Diff(myDiffString)
    .Split()
    .OldRevision("a/file.txt")
    .NewRevision("b/file.txt")
    .Language("typescript")
```

Handle line clicks to jump to specific locations or add comments:

```csharp
new DiffView()
    .Diff(myDiffString)
    .OnLineClick(lineNumber => {
        // Handle line click - e.g., show comment dialog
        Console.WriteLine($"Clicked line {lineNumber}");
    })
```

The widget accepts standard git diff output format and includes syntax highlighting support when you specify the language.

### AutoScroll Container

A new `AutoScroll` widget has been added to the primitives collection. This container automatically scrolls to the bottom when its content grows, making it perfect for live logs, activity feeds, or any streaming content that needs to stay visible in a fixed-height viewport.

```csharp
var lines = UseState(ImmutableArray.Create("First line", "Second line"));

AutoScroll.FromChildren(lines.Value.Select(l => Text.Muted(l)))
    .Height(Size.Px(200))
    .Width(Size.Full())
```

The widget requires an explicit height to create the scroll area. Use `AutoScroll.FromChildren()` when building children from LINQ expressions to avoid type conversion issues.

You can disable auto-scrolling behavior with the `Disabled()` method, which is useful when users want to read older content without being pulled back to the bottom:

```csharp
var follow = UseState(true);

AutoScroll.FromChildren(logLines.Value.Select(l => Text.Block(l)))
    .Height(Size.Px(200))
    .Disabled(!follow.Value)
```

When auto-follow is enabled, scrolling up manually pauses the auto-scroll until the user scrolls back to the bottom - just like chat message behavior.

## UI Improvements

### Text Widget Layout Control

Text widgets now support `Height()` and `Grow()` methods for better layout control. These methods work with all specialized text variants including CodeBlock, Markdown, Json, Xml, and Html:

```csharp
// Set explicit height for a code block
Text.Code(sourceCode, Languages.CSharp)
    .Height(Size.Px(400))
    .Width(Size.Full())

// Make a markdown widget grow to fill available space
Text.Markdown(documentation)
    .Grow()
    .Height(Size.Vh(80))

// Control JSON display dimensions
Text.Json(apiResponse)
    .Width(Size.Px(600))
    .Height(Size.Px(300))
```

The `Grow()` method uses flex-grow to make the widget expand within its container, perfect for creating responsive layouts. Both width and height now properly propagate from the TextBuilder to the underlying widget implementation.

### Sheet Widget Animation

Sheet widgets (slide-out panels) now include a smooth grow-in animation when they open, matching the polished animation behavior of Dialog widgets. This visual enhancement makes the UI feel more consistent and refined across all overlay components.

### Voice Dictation for TextInput Widgets

TextInput widgets now support voice dictation, allowing users to speak their input instead of typing. Enable dictation with the `EnableDictation()` extension method:

```csharp
var message = UseState("");

new TextInput()
    .Bind(message)
    .EnableDictation()
    .Placeholder("Type or speak your message...")
```

Specify a language for better transcription accuracy:

```csharp
new TextInput()
    .Bind(message)
    .EnableDictation(language: "es-ES")  // Spanish
```

When dictation is enabled, a microphone button appears in the input field. Users can click to record their voice, and the transcribed text automatically appears in the input. If the input is bound to a state, the transcription is appended to any existing text with proper spacing.

**Requirements:** Dictation requires an `IAudioTranscriptionService` to be registered. See the [Audio Transcription Service](#audio-transcription-service) section above for setup instructions using Azure Speech Services.

### Markdown Widget Collapsible Sections

Fixed padding in collapsible sections (details/summary elements) within Markdown widgets. Text nodes that aren't wrapped in block elements now display with proper padding, improving the visual consistency of collapsed markdown content.

### Inline Icon Preview in Markdown

When documenting icon usage in markdown, you can now use the pattern `Icons.IconName` in inline code, and the actual icon will render next to the code. This makes it easier to create icon documentation or design systems.

```markdown
Use `Icons.ChevronDown` for dropdown menus or `Icons.Search` for search fields.
```

The rendered output will show each icon name in a code block with a visual preview of the icon beside it. The pattern must start with `Icons.` followed by a capitalized icon name (e.g., `Icons.H1`, `Icons.ChevronDown`).

### Local File Links in Markdown

Fixed file:/// link handling in Markdown widgets when local files are enabled. Links with `file:///` URLs are now clickable and properly passed to your `onLinkClick` handler, while images continue to load through the secure proxy. This makes it easier to create markdown documentation that references local files:

```csharp
new Markdown()
    .Source("[Open log file](file:///C:/logs/app.log)")
    .DangerouslyAllowLocalFiles(true)
    .OnLinkClick(url => {
        // Handle file:/// URL
        System.Diagnostics.Process.Start(url);
    })
```

### Markdown Image Overlay Display

Fixed an issue where the full-screen image overlay in Markdown widgets could appear behind other UI elements. When you click an image to view it full-screen, the overlay now consistently displays on top of all other content.

### Markdown Task List Rendering

Task list items in Markdown widgets now render correctly without bullet markers. When you use GitHub Flavored Markdown (GFM) task lists, the checkboxes will appear cleanly without redundant bullet points:

```markdown
- [ ] Incomplete task
- [x] Completed task
```

This creates a more polished appearance that matches standard GFM rendering behavior.

### Nested Code Blocks in Markdown

Fixed rendering of nested code blocks in Markdown widgets. Previously, when displaying code examples that contain code blocks (common in documentation), the inner code fence would prematurely close the outer block. The framework now automatically increases the backtick count of outer fences to ensure proper nesting:

```csharp
new Markdown()
    .Source(@"
Here's how to create a code block:

```markdown
```csharp
Console.WriteLine(""Hello"");
```

```
")
```

The outer markdown fence will automatically render with four backticks (````) while the inner csharp fence uses three (```), ensuring the entire example displays correctly. This fix is particularly useful for documentation sites, tutorials, or any content that teaches users how to use markdown or code blocks.

### Markdown Code Block Wrapping

Fixed code block rendering to preserve ASCII art and diagram alignment. Code blocks in Markdown widgets no longer wrap text, instead using horizontal scrolling to maintain formatting. This ensures that ASCII diagrams, formatted tables, and code with specific alignment render exactly as intended:

```csharp
new Markdown()
    .Source(@"
```

┌─────────────┐
│   System    │
│  ┌───────┐  │
│  │  App  │  │
│  └───────┘  │
└─────────────┘

```
")
```

The diagram will now display with perfect alignment instead of wrapping and breaking the layout.

### Markdown Popover Links

Markdown widgets now support popover links for inline supplementary information. Use the special syntax `[text](## "popover content")` to create clickable text that displays a popover instead of navigating to a URL:

```csharp
new Markdown()
    .Source(@"
The framework uses [JSON diffing](## ""Compares the current and previous state trees to determine minimal DOM updates"")
for efficient rendering.

Use the [UseState hook](## ""React-style state management for component data"") to manage local component state.
")
```

Popover links are styled using your theme's primary color with a dotted underline to visually distinguish them from regular navigation links. This is perfect for tooltips, definitions, or explanatory notes without cluttering your documentation with parenthetical text.

### DataTable Footer Aggregation Dropdown

Fixed visibility issues with the DataTable footer aggregation dropdown menu. When a column has multiple aggregates (e.g., both "Total" and "Avg"), clicking to switch between them now displays the dropdown correctly above all other content. The menu also properly tracks grid column resizes and scrolling, and supports keyboard navigation with focus/blur handling.

### DataTable Empty Row Display

Fixed a visual glitch where DataTables would display a partial empty row at the bottom when filler rows were used to fill sparse data. The bottom edge of the table now renders cleanly without any visible gap or "lip", making the table appearance more polished when displaying fewer rows than the available viewport space.

### DataTable Header Slots

DataTable widgets now support custom content in the header area through two new slot methods: `HeaderLeft()` and `HeaderRight()`. This gives you full control over the header bar, perfect for adding action buttons, item counts, status badges, or other contextual controls.

**HeaderLeft** renders immediately after the filter button (if filtering is enabled):

```csharp
products.ToDataTable()
    .HeaderLeft(ctx => new Button("Export", icon: Icons.Download).Small())
```

**HeaderRight** renders on the right side of the header bar:

```csharp
products.ToDataTable()
    .HeaderRight(ctx => new Badge($"{products.Count()} items"))
```

Combine both slots to create rich, action-oriented table headers:

```csharp
products.ToDataTable()
    .HeaderLeft(ctx => Layout.Horizontal().Gap(2)
        | new Button("Export", icon: Icons.Download).Small()
        | new Button("Import", icon: Icons.Upload).Small())
    .HeaderRight(ctx => Layout.Horizontal().Gap(2)
        | new Badge($"{products.Count()} items")
        | new Button("Settings", icon: Icons.Settings).Small())
```

The slots use the same factory pattern as other Ivy builders, giving you access to the full context for dynamic content rendering.

### Menu Item Badges

You can now add badges to sidebar menu items using the new `Badge()` extension method. Badges are perfect for showing notification counts, status indicators, or other supplementary information:

```csharp
new MenuItem("Inbox", Icons.Mail)
    .Badge("3")

new MenuItem("Tasks", Icons.CheckSquare)
    .Badge("New")

new MenuItem("Settings", Icons.Settings)
    .Badge("!")
```

Badges appear on the right side of menu items in the sidebar and automatically inherit the sidebar's theme styling.

### Button Badges

You can now add badges to buttons using the new `Badge()` extension method. Badges are perfect for showing counts, status indicators, or notifications on action buttons:

```csharp
new Button("Inbox", eventHandler)
    .Badge("3")
    .ShortcutKey("i")

new Button("Notifications", eventHandler, variant: ButtonVariant.Secondary)
    .Badge("99+")

new Button("Updates", eventHandler, variant: ButtonVariant.Outline)
    .Badge("New")
```

Badges appear next to the button text and automatically adapt to the button's variant and theme styling. They work alongside other button features like shortcuts, icons, and loading states.

### Image Widget Border and Hover Effects

Image widgets now support borders and hover effects, making it easy to create visually polished image displays. Add borders with customizable color, thickness, and border radius:

```csharp
new Image("https://example.com/photo.jpg")
    .BorderStyle(BorderStyle.Solid)
    .BorderColor(Colors.Blue)
    .BorderThickness(2)
    .BorderRadius(BorderRadius.Rounded)
```

Control border opacity for subtle effects:

```csharp
new Image("https://example.com/photo.jpg")
    .BorderStyle(BorderStyle.Solid)
    .BorderColor(Colors.Gray, opacity: 0.5f)
    .BorderThickness(1)
    .BorderRadius(BorderRadius.Full)
```

Add hover effects to make images interactive:

```csharp
new Image("https://example.com/photo.jpg")
    .Hover(HoverEffect.Shadow)
    .OnClick(() => ShowFullSize())
```

When you add an `OnClick` handler to an image, the framework automatically applies a `PointerAndTranslate` hover effect (cursor changes to pointer and slight lift on hover). You can override this default by explicitly setting a different hover variant:

```csharp
new Image("https://example.com/photo.jpg")
    .Hover(HoverEffect.Shadow)
    .OnClick(() => ShowFullSize())  // Will use Shadow instead of default PointerAndTranslate
```

Available hover variants include `HoverEffect.Pointer`, `HoverEffect.Shadow`, and `HoverEffect.PointerAndTranslate`.

## Charts

### Advanced Axis Configuration

Chart axes now support extended configuration options for formatting, visibility, and domain control, giving you precise control over how your charts display data.

**Format tick labels** with standard C# format strings using the new `TickFormatter()` method:

```csharp
// Currency formatting on Y-axis
new LineChart(revenueData)
    .Line(new Line("Revenue"))
    .YAxis(new YAxis("Revenue").TickFormatter("C0"))  // $1,500,000
    .XAxis(new XAxis("Year"))
```

Supported formats include currency (`"C0"`, `"C2:EUR"`), percentage (`"P0"`, `"P2"`), and number formats (`"N2"`, `"F1"`).

**Hide tick labels** for a minimalist appearance while keeping grid structure:

```csharp
// Clean chart without axis labels
new BarChart(data)
    .Bar(new Bar("Users"))
    .XAxis(new XAxis("Day").HideTickLabels())
    .YAxis(new YAxis("Users").HideTickLabels())
```

**Control axis domain** to clip outliers or set explicit bounds:

```csharp
// Clip extreme values between 0 and 200
new BarChart(salesData)
    .Bar(new Bar("Sales"))
    .XAxis(new XAxis("Sales")
        .Domain(0, 200)
        .AllowDataOverflow(true))  // Strictly enforce bounds
    .YAxis(new YAxis("Product"))
```

You can also use symbolic bounds for flexible scaling:

```csharp
// Clamp bottom to zero, auto-scale top to data maximum
.YAxis(new YAxis("Revenue")
    .Domain(0, AxisDomain.DataMax))

// Available symbols: AxisDomain.Auto, AxisDomain.DataMin, AxisDomain.DataMax
```

These configuration options work with all Cartesian charts (Line, Bar, and Area charts).

## Hooks

### UseLoading Hook

A new `UseLoading` hook provides programmatic control over loading dialogs, perfect for showing progress during async operations. The hook returns a tuple containing the loading view (which you render in your component) and a `showLoading` function to trigger the dialog:

```csharp
var (loadingView, showLoading) = UseLoading();

return new Fragment(
    loadingView,
    new Button("Process Data", () =>
    {
        showLoading(async ctx =>
        {
            ctx.Message("Processing...");
            ctx.Status("This may take a moment");
            await ProcessDataAsync();
        });
    })
);
```

The loading context provides methods to update the dialog during execution:

```csharp
showLoading(async ctx =>
{
    ctx.Message("Loading data...");
    ctx.Status("Fetching from database");

    var data = await FetchDataAsync();

    ctx.Message("Processing records");
    ctx.Progress(50);  // Show progress bar at 50%

    await ProcessAsync(data);

    ctx.Progress(100);
});
```

**Cancellable operations** - Enable user cancellation by passing `cancellable: true` and observing the `CancellationToken`:

```csharp
showLoading(async ctx =>
{
    ctx.Message("Downloading files...");
    ctx.Status("Click X to cancel");

    for (var i = 0; i < 10; i++)
    {
        ctx.CancellationToken.ThrowIfCancellationRequested();
        ctx.Progress(i * 10);
        await DownloadFileAsync(i, ctx.CancellationToken);
    }
}, cancellable: true);
```

When the user cancels, the dialog shows "Cancelling..." for 800ms by default before closing. You can customize this duration:

```csharp
showLoading(
    async ctx => { /* your work */ },
    cancellable: true,
    options: new LoadingOptions
    {
        CancellingDisplayDuration = TimeSpan.FromMilliseconds(300)
    }
);
```

**Non-cancellable operations** - For operations that cannot be interrupted, the close button is automatically hidden and overlay clicks are ignored:

```csharp
showLoading(async ctx =>
{
    ctx.Message("Committing transaction...");
    ctx.Status("Please wait");
    ctx.Progress(null);  // Indeterminate progress
    await CommitTransactionAsync();
}, cancellable: false);
```

The hook handles exceptions automatically through the framework's `IExceptionHandler`, and properly manages cancellation token disposal.

## Configuration

### IConfiguration Now Available via Dependency Injection

`IConfiguration` is now registered in the dependency injection container and can be injected into your services:

```csharp
public class MyService
{
    private readonly IConfiguration _config;

    public MyService(IConfiguration config)
    {
        _config = config;
    }

    public string GetSetting() => _config["MySetting"];
}
```

This makes it easier to access configuration values throughout your application without passing the `Server.Configuration` property explicitly.

### Auth Examples Now Use .NET User Secrets

Authentication example projects now use .NET user-secrets for local development instead of `appsettings.json` files. To configure an auth example:

```bash
cd src/auth/examples/Auth0Example
dotnet user-secrets set "Auth0:Domain" "your-tenant.auth0.com"
dotnet user-secrets set "Auth0:ClientId" "your-client-id"
dotnet user-secrets set "Auth0:ClientSecret" "your-client-secret"
```

This approach keeps sensitive credentials out of source control without needing to manually copy example files. For production deployments, the ClerkExample demonstrates loading secrets from a custom path using the `IVY_CLERK_SECRETS_PATH` environment variable.

## New Services

### Audio Transcription Service

A new `IAudioTranscriptionService` interface has been added to the framework, providing a standardized way to transcribe audio to text. The framework includes an Azure Speech Services implementation out of the box.

Register the Azure Speech transcription service in your dependency injection container:

```csharp
builder.Services.AddAzureSpeechToText(
    region: "eastus",
    subscriptionKey: Configuration["Azure:SpeechKey"]
);
```

Then inject and use it in your services:

```csharp
public class VoiceNoteService
{
    private readonly IAudioTranscriptionService _transcription;

    public VoiceNoteService(IAudioTranscriptionService transcription)
    {
        _transcription = transcription;
    }

    public async Task<string> TranscribeVoiceNote(Stream audioStream, string mimeType)
    {
        return await _transcription.TranscribeAsync(audioStream, mimeType, language: "en-US");
    }
}
```

The service supports common audio formats including WebM, Ogg, WAV, MP4, and AAC. You can optionally specify the language (defaults to "en-US").

This abstraction makes it easy to swap transcription providers by implementing `IAudioTranscriptionService` with your preferred service.

## Deployment

### Multi-Platform Support

The Ivy Framework now includes native binaries for all major platforms including Windows (x64 and ARM64), Linux (x64 and ARM64), and macOS (Intel x64 and Apple Silicon ARM64). The framework automatically selects the correct native libraries for your target platform - no configuration required.

Linux ARM64 support enables deployment on ARM-based servers like AWS Graviton instances, Oracle Cloud Ampere, and other ARM64 infrastructure, providing cost-effective hosting options with the same performance optimizations available on other platforms.

**Alpine Linux Support:** The framework now detects and supports Alpine Linux (musl-based distributions), automatically loading the correct native libraries for musl environments. This is particularly useful for lightweight Docker containers built on Alpine Linux base images, which are popular for their minimal size and security benefits.

### BASE_PATH Environment Variable Support

You can now configure your application's base path using the `BASE_PATH` environment variable, perfect for reverse proxy deployments where your app runs under a subpath (like `/myapp`). This matches the pattern used for other environment variables like `PORT`, `HOST`, and `VERBOSE`.

```bash
# Docker deployment example
docker run -e BASE_PATH=/myapp -e PORT=5000 myivyapp

# Or in docker-compose.yml
environment:
  - BASE_PATH=/myapp
  - PORT=5000
```

You can also configure it via the CLI argument:

```csharp
var server = new Server(args =>
{
    args.BasePath = "/myapp";
});
```

The environment variable is particularly useful in containerized deployments where configuration through environment variables is preferred over CLI arguments.

### Single-File Publishing Support

Fixed an issue where Ivy apps published as single-file executables would fail to load authentication modules. The framework now correctly locates and loads `Ivy.Auth.*` assemblies in single-file published apps, eliminating IL3000 warnings. This change is automatic - simply publish your app with:

```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

Your authentication handlers will now load correctly in the single-file output.

## Developer Tools

### New CLI Documentation Commands

The Ivy CLI now includes powerful commands for exploring framework documentation and getting instant answers to your questions:

**Browse documentation:**

```bash
# List all available docs
ivy docs list

# Read a specific doc page
ivy docs "docs/ApiReference/IvyShared/Colors.md"
```

**Ask questions with semantic search:**

```bash
ivy ask "How do I implement a new Application Shell in Ivy?"
ivy question "What is the command to create an auto-incrementing migration?"
```

The `ivy ask` command (also available as `ivy question`) uses Local RAG to search the framework knowledge base and synthesize contextual answers from across the documentation. Perfect for "how do I..." questions when you're not sure where to look.

Use `ivy docs` when you know exactly what topic you need, and `ivy ask` when you need the framework to find and synthesize the answer for you.

## Breaking Changes

### CardHoverVariant Renamed to HoverEffect

The `CardHoverVariant` enum has been renamed to `HoverEffect` and moved to a shared location (`Ivy.Shared`). This enum is used by Card, Box, and Image widgets to control hover interaction effects.

**Before:**

```csharp
new Box("Click me")
    .Hover(CardHoverVariant.Shadow)

new Image("photo.jpg")
    .Hover(CardHoverVariant.PointerAndTranslate)
```

**After:**

```csharp
new Box("Click me")
    .Hover(HoverEffect.Shadow)

new Image("photo.jpg")
    .Hover(HoverEffect.PointerAndTranslate)
```

**Migration:** Replace all instances of `CardHoverVariant` with `HoverEffect`. The extension method signatures remain unchanged (`.Hover(HoverEffect variant)`), and both the old and new enums are in the `Ivy` namespace, so no namespace changes are needed.

### Layout Alignment API Renamed

The `Align` method and property has been renamed to clarify its purpose across several widgets:

- **StackLayout.Align → AlignContent** — controls how children are aligned within the container
- **TableCell.Align → AlignContent** — controls how content is aligned within the cell
- **FloatingPanel.Align → AlignSelf** — controls how the panel positions itself within its parent

This change makes the API more explicit about whether you're aligning children inside a container or positioning the widget itself.

**Before:**

```csharp
new StackLayout() { Align = Align.Center }
new TableCell().Align(Align.Left)
new FloatingPanel(align: Align.BottomRight)
```

**After:**

```csharp
new StackLayout() { AlignContent = Align.Center }
new TableCell().AlignContent(Align.Left)
new FloatingPanel(alignSelf: Align.BottomRight)
```

**Migration:** Replace `.Align(` with `.AlignContent(` on StackLayout and TableCell widgets, and with `.AlignSelf(` on FloatingPanel. Update property initializers from `Align =` to `AlignContent =` or `AlignSelf =` as appropriate.

### DiffView Widget Package Renamed

The `DiffView` widget package has been renamed from `Ivy.External.DiffView` to `Ivy.Widgets.DiffView`. If you're using this widget, update your project references:

```bash
# Remove the old package
dotnet remove package Ivy.External.DiffView

# Add the new package
dotnet add package Ivy.Widgets.DiffView
```

Update your using statements:

```csharp
// Old
using Ivy.External.DiffView;

// New
using Ivy.Widgets.DiffView;
```

The widget API remains unchanged - only the package and namespace have been renamed.

## Bug Fixes

- **SignalR Connection Stability**: The framework now handles MessagePack serialization more robustly, preventing connection drops in complex scenarios.
- **Badge Display in Table**: Badges now use `inline-flex` layout instead of `flex`, preventing them from expanding to fill the entire table cell width.
- **Dialog Custom Width Support**: When you set a custom width without an explicit maxWidth, the framework now automatically matches the maxWidth to your width value.
- **DiffView Widget Runtime Error**: The widget now properly references the React JSX runtime, ensuring reliable operation across all scenarios.
- **ColorInput Height Alignment**: ColorInput widgets now render at the same height as other input widgets (TextInput, NumberInput) at all density settings.
- **TextArea Height Handling**: The height property is now properly applied to the textarea element itself rather than just the wrapper, ensuring that explicit heights are respected while still allowing textareas without specified heights to fill their containers naturally.
- **Effect Queue Race Condition**: Effects are now guaranteed to run even when queued during concurrent operations, preventing scenarios where state changes or side effects might not trigger as expected.
- **AsyncSelectInput Value Handling**: The widget now correctly passes the fresh value to OnChange handlers, ensuring your event handlers always receive the accurate selected value.
- **Native Library Loading Diagnostics**: The framework now provides detailed diagnostic information including the runtime identifier, probed file path, and base directory.
