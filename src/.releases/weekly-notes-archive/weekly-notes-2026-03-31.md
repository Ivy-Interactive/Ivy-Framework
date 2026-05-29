# Ivy Framework Weekly Notes - Week of 2026-03-31

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## Performance Improvements

### Event Handling Optimization

Event dispatching in widgets is now significantly faster. The framework now caches reflection lookups when invoking widget events, eliminating repeated reflection overhead on every event call.

### Rust-Powered Core Engine

The framework core now leverages Rust for performance-critical operations including JSON diffing and tree synchronization. These optimizations deliver faster UI updates and reduced latency, particularly noticeable in applications with frequent state changes or large component trees. The improvements are automatic - no code changes required.

## New Widgets

### DiffView Widget

A new [**DiffView**](https://docs.ivy.app/widgets/primitives/diff-view) widget for displaying unified diffs (git diff output) in either unified or split view mode.

Install with `dotnet add package Ivy.Widgets.DiffView`. The widget accepts standard git diff output and supports syntax highlighting when you specify a language.

```csharp
using Ivy.Widgets.DiffView;

// Unified diff (default)
new DiffView()
    .Diff(myDiffString);

// Split view, revisions, language (for highlighting)
new DiffView()
    .Diff(myDiffString)
    .Split()
    .OldRevision("a/file.txt")
    .NewRevision("b/file.txt")
    .Language("typescript");

// Line clicks (e.g. jump to source or add comments)
new DiffView()
    .Diff(myDiffString)
    .OnLineClick(lineNumber => Console.WriteLine($"Clicked line {lineNumber}"));
```

### AutoScroll Container

The [**AutoScroll**](https://docs.ivy.app/widgets/primitives/auto-scroll) container automatically scrolls to the bottom when its content grows.

```csharp
var lines = UseState(ImmutableArray.Create("First line", "Second line"));

AutoScroll.FromChildren(lines.Value.Select(l => Text.Muted(l)))
    .Height(Size.Px(200))
    .Width(Size.Full())
```

## UI Improvements

### Text Widget Layout Control

[**Text**](https://docs.ivy.app/widgets/primitives/text-block) widgets now support `Height()` and `Grow()` methods for better layout control:

```csharp
// Make a markdown widget grow to fill available space
Text.Markdown(documentation)
    .Grow()
    .Height(Size.Vh(80))
```

### Voice Dictation for TextInput Widgets

[**TextInput**](https://docs.ivy.app/widgets/inputs/text-input) widgets now support voice dictation, allowing users to speak their input instead of typing:

```csharp
var message = UseState("");

new TextInput()
    .Bind(message)
    .EnableDictation(language: "es-ES")  // optional; omit for default locale
    .Placeholder("Type or speak your message...")
```

**Requirements:** Dictation requires an `IAudioTranscriptionService` to be registered—see [**Dictation**](https://docs.ivy.app/widgets/inputs/dictation) for Azure Speech setup and integration.

### Inline Icon Preview in Markdown

When documenting icon usage in markdown, you can now use the pattern `Icons.IconName` in inline code, and the actual icon will render next to the code.

```markdown
Use `Icons.ChevronDown` for dropdown menus or `Icons.Search` for search fields.
```

See the [**Markdown**](https://docs.ivy.app/widgets/primitives/markdown) widget documentation for formatting and rendering.

### Markdown Popover Links

The Markdown widget now supports popover links for inline supplementary information. Use the special syntax `[text](## "popover content")` to create clickable text that displays a popover instead of navigating to a URL:

```csharp
new Markdown()
    .Source(@"
The framework uses [JSON diffing](## ""Compares the current and previous state trees to determine minimal DOM updates"")
for efficient rendering.

Use the [UseState hook](## ""React-style state management for component data"") to manage local component state.
")
```

### DataTable Header Slots

[**DataTable**](https://docs.ivy.app/widgets/advanced/data-table) widgets now support custom content in the header area through two new slot methods: `HeaderLeft()` and `HeaderRight()`.

```csharp
products.ToDataTable()
    .HeaderLeft(ctx => Layout.Horizontal().Gap(2) //renders immediately after the filter button (if filtering is enabled)
        | new Button("Export", icon: Icons.Download).Small()
        | new Button("Import", icon: Icons.Upload).Small())
    .HeaderRight(ctx => Layout.Horizontal().Gap(2) //renders on the right side of the header bar
        | new Badge($"{products.Count()} items")
        | new Button("Settings", icon: Icons.Settings).Small())
```

### Menu Item and Button Badges

You can add badges to sidebar [**Navigation**](https://docs.ivy.app/onboarding/concepts/navigation) menu items and [**Button**](https://docs.ivy.app/widgets/common/button) widgets with the new `Badge()` extension method:

```csharp
new MenuItem("Tasks", Icons.CheckSquare).Badge("New");
new Button("Updates", eventHandler, variant: ButtonVariant.Outline).Badge("New");
```

### Image Widget Border and Hover Effects

[**Image**](https://docs.ivy.app/widgets/primitives/image) widgets now support borders, border opacity, and hover effects. Adding `OnClick` applies `PointerAndTranslate` by default; set `.Hover(...)` explicitly to use something else (for example `Shadow`). Available variants include `Pointer`, `Shadow`, and `PointerAndTranslate`.

```csharp
new Image("https://example.com/photo.jpg")
    .BorderStyle(BorderStyle.Solid)
    .BorderColor(Colors.Blue)
    .BorderThickness(2)
    .BorderRadius(BorderRadius.Rounded)
    .Hover(HoverEffect.Shadow)
    .OnClick(() => ShowFullSize()); // OnClick defaults to PointerAndTranslate unless you override:
```

## Charts

### Advanced Axis Configuration

[**Line chart**](https://docs.ivy.app/widgets/charts/line-chart) axes (and other Cartesian charts such as Bar and Area) now support extended configuration: format tick labels with `TickFormatter()` (currency `"C0"`, `"C2:EUR"`, percentage `"P0"`, numbers `"N2"`, etc.), hide labels with `HideTickLabels()`, and control domain with numeric bounds or symbols (`AxisDomain.Auto`, `DataMin`, `DataMax`).

```csharp
// Tick formatting (e.g. currency on Y)
new LineChart(revenueData)
    .Line(new Line("Revenue"))
    .YAxis(new YAxis("Revenue").TickFormatter("C0"))
    .XAxis(new XAxis("Year"));

// Minimalist axes (grid only)
new BarChart(data)
    .Bar(new Bar("Users"))
    .XAxis(new XAxis("Day").HideTickLabels())
    .YAxis(new YAxis("Users").HideTickLabels());

// Explicit domain + overflow; symbolic domain on another axis
new BarChart(salesData)
    .Bar(new Bar("Sales"))
    .XAxis(new XAxis("Sales").Domain(0, 200).AllowDataOverflow(true))
    .YAxis(new YAxis("Product"));
```

## Hooks

### UseLoading Hook

A new `UseLoading` hook returns `(loadingView, showLoading)` for loading dialogs. The context exposes `Message`, `Status`, `Progress` (use `null` for indeterminate), and `CancellationToken` when `cancellable: true`. Pass `LoadingOptions.CancellingDisplayDuration` to override the default 800ms "Cancelling..." state. With `cancellable: false`, the close button is hidden and overlay clicks are ignored. See the [**Hooks**](https://docs.ivy.app/hooks/hook-introduction) documentation for general hook usage.

```csharp
var (loadingView, showLoading) = UseLoading();

return new Fragment(
    loadingView,
    new Button("Process Data", () =>
        showLoading(async ctx =>
        {
            ctx.Message("Processing...");
            ctx.Status("This may take a moment");
            ctx.Progress(50);
            await ProcessDataAsync();
            ctx.Progress(100);
        })));
```

## Configuration

### IConfiguration Now Available via Dependency Injection

`IConfiguration` is now registered in the dependency injection container and can be injected into your services (see [**server configuration**](https://docs.ivy.app/onboarding/concepts/program)):

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

### Auth Examples Now Use .NET User Secrets

Authentication example projects now use .NET user-secrets for local development instead of `appsettings.json` files (see the [**Auth0**](https://docs.ivy.app/onboarding/cli/authentication/auth0) CLI guide for provider-specific setup):

```bash
cd src/auth/examples/Auth0Example
dotnet user-secrets set "Auth0:Domain" "your-tenant.auth0.com"
dotnet user-secrets set "Auth0:ClientId" "your-client-id"
dotnet user-secrets set "Auth0:ClientSecret" "your-client-secret"
```

This approach keeps sensitive credentials out of source control without needing to manually copy example files. For production deployments, the ClerkExample demonstrates loading secrets from a custom path using the `IVY_CLERK_SECRETS_PATH` environment variable.

## New Services

### Audio Transcription Service

A new `IAudioTranscriptionService` interface provides a standardized way to transcribe audio to text. Register `AddAzureSpeechToText` in DI, then inject `IAudioTranscriptionService`. The service supports WebM, Ogg, WAV, MP4, AAC, and optional language (defaults to `"en-US"`).

```csharp
builder.Services.AddAzureSpeechToText(
    region: "eastus",
    subscriptionKey: Configuration["Azure:SpeechKey"]);

public class VoiceNoteService
{
    private readonly IAudioTranscriptionService _transcription;

    public VoiceNoteService(IAudioTranscriptionService transcription)
    {
        _transcription = transcription;
    }

    public Task<string> TranscribeVoiceNote(Stream audioStream, string mimeType) =>
        _transcription.TranscribeAsync(audioStream, mimeType, language: "en-US");
}
```

## Deployment

### Multi-Platform Support

The Ivy Framework now includes native binaries for all major platforms including Windows (x64 and ARM64), Linux (x64 and ARM64), and macOS (Intel x64 and Apple Silicon ARM64).

Linux ARM64 support enables deployment on ARM-based servers like AWS Graviton instances, Oracle Cloud Ampere, and other ARM64 infrastructure.

**Alpine Linux Support:** The framework now detects and supports Alpine Linux (musl-based distributions), automatically loading the correct native libraries for musl environments. This is particularly useful for lightweight Docker containers built on Alpine Linux base images.

### BASE_PATH Environment Variable Support

You can now configure your application's base path using the `BASE_PATH` environment variable (same idea as `PORT`, `HOST`, and `VERBOSE`), or set `ServerArgs.BasePath` in code—see [**server configuration**](https://docs.ivy.app/onboarding/concepts/program).

```csharp
// Env / Docker: BASE_PATH=/myapp (e.g. docker run -e BASE_PATH=/myapp -e PORT=5000 myivyapp)

var server = new Server(args => { args.BasePath = "/myapp"; });
```

## Developer Tools

### New CLI Documentation Commands

The Ivy CLI now includes [**ivy docs**](https://docs.ivy.app/onboarding/cli/docs) for browsing documentation and [**ivy ask**](https://docs.ivy.app/onboarding/cli/question) (alias `ivy question`) for semantic search over the framework knowledge base.

```bash
ivy docs list
ivy docs "docs/ApiReference/IvyShared/Colors.md"
ivy ask "How do I implement a new Application Shell in Ivy?"
ivy question "What is the command to create an auto-incrementing migration?"
```

## Breaking Changes

### CardHoverVariant Renamed to HoverEffect

The `CardHoverVariant` enum has been renamed to `HoverEffect`. This enum is used by Card, [**Box**](https://docs.ivy.app/widgets/primitives/box), and Image widgets to control hover interaction effects. Replace `CardHoverVariant` with `HoverEffect`; `.Hover(...)` signatures are unchanged and stay in the `Ivy` namespace.

```csharp
// Before
new Box("Click me").Hover(CardHoverVariant.Shadow);
new Image("photo.jpg").Hover(CardHoverVariant.PointerAndTranslate);

// After
new Box("Click me").Hover(HoverEffect.Shadow);
new Image("photo.jpg").Hover(HoverEffect.PointerAndTranslate);
```

### Layout Alignment API Renamed

The `Align` method and property has been renamed to clarify its purpose across several widgets (see [**Align**](https://docs.ivy.app/api-reference/ivy/align) values used throughout layouts and widgets):

- **StackLayout.Align to AlignContent** — controls how children are aligned within the container
- **TableCell.Align to AlignContent** — controls how content is aligned within the cell
- **FloatingPanel.Align to AlignSelf** — controls how the panel positions itself within its parent

```csharp
// Before
new StackLayout() { Align = Align.Center };
new TableCell().Align(Align.Left);
new FloatingPanel(align: Align.BottomRight);

// After
new StackLayout() { AlignContent = Align.Center };
new TableCell().AlignContent(Align.Left);
new FloatingPanel(alignSelf: Align.BottomRight);
```

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
