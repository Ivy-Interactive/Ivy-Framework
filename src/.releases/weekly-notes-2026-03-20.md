# Ivy Framework Weekly Notes - Week of 2026-03-20

## New Features

### VideoPlayer Time Range Control

The **VideoPlayer** widget now supports precise playback control with `StartTime` and `EndTime` properties. Specify which segment of a video to play by setting start and end positions in seconds—perfect for highlighting specific moments, creating video tutorials, or building interactive media experiences.

**Play from a specific start position:**

```csharp
new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
    .StartTime(5)  // Start playback at 5 seconds
```

**Play a specific segment:**

```csharp
new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
    .StartTime(2)
    .EndTime(6)  // Play from 2s to 6s, then pause
```

**Works with YouTube videos too:**

```csharp
new VideoPlayer("https://www.youtube.com/watch?v=dQw4w9WgXcQ")
    .StartTime(30)
    .EndTime(60)  // Play YouTube segment from 30s to 60s
    .Height(Size.Units(100))
```

The implementation uses Media Fragments URI for HTML5 videos and embed parameters for YouTube, ensuring broad compatibility across video sources.

### VideoPlayer Event Callbacks

The **VideoPlayer** widget now supports event callbacks for tracking video playback state. React to play, pause, ended, and loaded events to build analytics, sequential video experiences, or coordinate UI updates with video state.

**Available events:**

- `OnPlay` - Triggered when video playback starts
- `OnPause` - Triggered when video playback pauses
- `OnEnded` - Triggered when video playback completes
- `OnLoaded` - Triggered when video metadata loads

**Usage example:**

```csharp
var playCount = UseState(0);
var completed = UseState(false);

var video = new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
    .HandlePlay(_ => playCount.Set(playCount.Value + 1))
    .HandlePause(_ => Console.WriteLine("Video paused"))
    .HandleEnded(_ => completed.Set(true))
    .HandleLoaded(_ => Console.WriteLine("Video ready"))
    .Height(Size.Units(50));

// Display play count and completion status
return Layout.Vertical()
    | video
    | new Badge($"Played {playCount.Value} times")
    | new Badge(completed.Value ? "Completed" : "In progress");
```

Each handler method has multiple overloads for convenience:
- `HandlePlay(Func<Event<VideoPlayer>, ValueTask>)` - Async handler with event data
- `HandlePlay(Action<Event<VideoPlayer>>)` - Sync handler with event data
- `HandlePlay(Action)` - Simple action without event data

Use these callbacks for video analytics, building sequential content flows, or coordinating UI state with playback events.

### VideoPlayer Playback Speed Control

The **VideoPlayer** widget now supports playback speed control via the `PlaybackRate` property. Adjust video speed from 0.25x (slow motion) to 2x or higher—perfect for tutorial playback, reviewing footage in slow motion, or quickly scrubbing through long recordings.

**Basic usage:**

```csharp
new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
    .PlaybackRate(1.5)  // Play at 1.5x speed
```

**Common use cases:**

```csharp
// Fast playback for lectures/tutorials
new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
    .PlaybackRate(1.5)

// Slow motion review
new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
    .PlaybackRate(0.5)

// Quick scrubbing through content
new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
    .PlaybackRate(2.0)
```

The minimum playback rate is 0.25x (enforced with validation). Values are automatically clamped to this minimum. Normal playback speed is 1.0.

**Note:** This feature works for native HTML5 video playback only and does not apply to YouTube embeds.

### SignatureInput Widget

Ivy now includes a **SignatureInput** widget for capturing handwritten signatures directly in the browser. Perfect for delivery confirmations, contract signing, approval workflows, or any scenario requiring user signatures. The widget provides a canvas-based drawing interface with mouse and touch support, outputting PNG image data as a `byte[]`.

**Basic usage:**

```csharp
public class SignatureDemo : ViewBase
{
    public override object? Build()
    {
        var signature = UseState<byte[]?>(null);

        return Layout.Vertical()
            | signature.ToSignatureInput()
                .Placeholder("Draw your signature here")
                .WithField()
                .Label("Customer Signature")
            | Text.P(signature.Value != null
                ? $"Signature captured ({signature.Value.Length} bytes)"
                : "No signature yet");
    }
}
```

**Customize pen appearance:**

Control the pen color and thickness to match your application's style:

```csharp
var signature = UseState<byte[]?>(null);

// Blue thick pen
signature.ToSignatureInput()
    .Pen(Colors.Blue)
    .PenThickness(4)
    .Placeholder("Sign here")

// Red thin pen
signature.ToSignatureInput()
    .Pen(Colors.Red)
    .PenThickness(1)
    .Placeholder("Initial here")
```

**Features:**

- **Canvas-based drawing** with smooth line rendering for mouse and touch input
- **Automatic clear button** appears when a signature is present
- **PNG output** as `byte[]` for easy storage and transmission
- **State support** with disabled and invalid states for forms
- **Form integration** works seamlessly with `WithField()` for labels and descriptions
- **Customizable styling** including pen color, thickness, and background color

The widget includes a built-in eraser/clear button that automatically appears when the user has drawn a signature, making it easy to start over without additional UI.

### GaugeChart Widget

Ivy now includes a **GaugeChart** widget for displaying KPI values in a circular gauge/dial format. Perfect for dashboards showing progress, completion rates, system load, or any metric that benefits from visual representation on a gauge. The widget provides smooth animations, customizable thresholds with color zones, and flexible pointer styles.

**Basic usage:**

```csharp
// Simple gauge with a value
new GaugeChart(75)
    .Label("CPU Usage")
```

**Configure range and styling:**

```csharp
// Custom range with thresholds
new GaugeChart(42)
    .Min(0)
    .Max(100)
    .Label("Progress")
    .Thresholds(
        new GaugeThreshold(30, "#10b981"),  // Green zone
        new GaugeThreshold(60, "#f59e0b"),  // Yellow zone
        new GaugeThreshold(100, "#ef4444")  // Red zone
    )
```

**Customize the pointer:**

```csharp
// Different pointer styles
new GaugeChart(85)
    .Label("Temperature")
    .Pointer(new GaugePointer()
        .Style(GaugePointerStyle.Arrow)
        .Width(8)
        .Length("70%"))

// Simple pointer with defaults
new GaugeChart(50)
    .Pointer()
    .Label("Load")
```

**Advanced configuration:**

```csharp
// Full customization
new GaugeChart(67)
    .Min(0)
    .Max(100)
    .Label("System Health")
    .StartAngle(180)
    .EndAngle(0)
    .Animated(true)
    .ColorScheme(ColorScheme.Primary)
    .Thresholds(
        new GaugeThreshold(33, "#10b981"),
        new GaugeThreshold(66, "#f59e0b"),
        new GaugeThreshold(100, "#ef4444")
    )
```

**Features:**

- **Configurable range** with Min/Max values
- **Color thresholds** for visual zones (green/yellow/red or custom colors)
- **Pointer styles**: Line, Arrow, or Rounded
- **Smooth animations** for value changes (can be disabled)
- **Flexible angles** to create half-circle or custom arc gauges
- **Color scheme support** for consistent theming

The gauge automatically scales labels, ticks, and positioning to fit the configured angle range, making it easy to create anything from traditional semicircle gauges to full circular displays.

### Native File Dialog Hooks

Ivy now provides **three new hooks for opening native OS file picker dialogs** directly from your code: **UseFileDialog**, **UseSaveDialog**, and **UseFolderDialog**. These hooks follow the same callback-in-show pattern as `UseAlert`, returning a `(dialogView, showDialog)` tuple where the show delegate accepts a callback to handle the user's selection.

**UseFileDialog - Open File Picker:**

Open files from the user's file system with two modes: `Upload` (default) for file content, or `PathOnly` for just the file path.

```csharp
// Upload mode - returns file content
var (fileDialog, showFileDialog) = UseFileDialog();

var selectedFile = UseState<UploadedFile?>(null);

showFileDialog(file =>
{
    selectedFile.Set(file);
    // file.Content contains byte[]
    // file.FileName, file.ContentType available
});

return Layout.Vertical()
    | fileDialog
    | new Button("Select File").OnClick(_ => showFileDialog(...));
```

**UseSaveDialog - Save File Picker:**

Save files to the user's file system using the native save dialog. Works with the `IDownloadService` pattern.

```csharp
// Create save dialog for exporting data
var (saveDialog, showSaveDialog) = UseSaveDialog();

var exportData = UseState<byte[]>(GenerateReport());

showSaveDialog(async () =>
{
    // Generate or retrieve file content
    return new DownloadRequest
    {
        Content = exportData.Value,
        FileName = "report.pdf",
        MimeType = "application/pdf"
    };
});

return Layout.Vertical()
    | saveDialog
    | new Button("Export Report").OnClick(_ => showSaveDialog(...));
```

**UseFolderDialog - Folder Picker:**

Select folders and retrieve their contents (directory entries) using the File System Access API on Chromium browsers.

```csharp
// Folder picker - returns directory entries
var (folderDialog, showFolderDialog) = UseFolderDialog();

var selectedFolder = UseState<FolderSelection?>(null);

showFolderDialog(folder =>
{
    selectedFolder.Set(folder);
    // folder.Name contains folder name
    // folder.Entries contains file/folder entries
});

return Layout.Vertical()
    | folderDialog
    | new Button("Select Folder").OnClick(_ => showFolderDialog(...));
```

**Features:**

- **Native OS dialogs** - Uses the browser's file picker APIs for a native experience
- **Smart browser detection** - File System Access API on Chromium, hidden input fallback on Firefox/Safari
- **File filtering** - Configure accepted file types with MIME types or extensions
- **Callback pattern** - Consistent with `UseAlert` for predictable async handling
- **Multiple modes** - Choose between uploading file content or just getting file paths

These hooks make it easy to build file management features, document uploaders, export/import workflows, or any scenario requiring access to the user's file system.

## API Improvements

### UseClipboard Hook

Ivy now includes a **UseClipboard** hook for copying text to the user's clipboard with a simple, intuitive API. The hook follows the standard `UseXxx` pattern and provides a cleaner alternative to manually accessing `IClientProvider.CopyToClipboard()`.

**Basic usage:**

```csharp
public class CopyExample : ViewBase
{
    public override object? Build()
    {
        var copyToClipboard = UseClipboard();

        return new Button("Copy greeting", _ =>
        {
            copyToClipboard("Hello, World!");
        });
    }
}
```

**Copying generated URLs or content:**

```csharp
var copyToClipboard = UseClipboard();
var client = UseService<IClientProvider>();

var shareUrl = $"https://example.com/item/{itemId}";
copyToClipboard(shareUrl);
client.Toast("Link copied!");
```

**What you get:**

- **Simple API** - Returns an `Action<string>` that copies text when invoked
- **Consistent pattern** - Follows the familiar UseXxx hook convention
- **Write-only** - Copies to clipboard (no read-from-clipboard support)

Under the hood, this wraps `IClientProvider.CopyToClipboard()`, but provides a more ergonomic API for the common case of copying text.

### Children() Extension Method for All Widgets

All Ivy widgets now support a generic **Children()** extension method for replacing widget children in a fluent manner. This provides an alternative to the pipe operator when you need to set all children at once.

**Basic usage:**

```csharp
// Replace all children on any widget
new Card().Children(
    Text.H3("Title"),
    Text.P("Description"),
    new Button("Action")
)

// Works on GridView
new GridView(columns: 8).Children(widget1, widget2, widget3)

// Works on any AbstractWidget
new Box().Children(content1, content2)
```

**When to use Children() vs pipe operator:**

```csharp
// Use Children() when setting all children at once
var card = new Card().Children(child1, child2, child3);

// Use pipe operator when appending children incrementally
var card = new Card();
card | child1 | child2;
```

**What changed:**

- **Generic extension** - `Children<T>()` works on any `AbstractWidget` subclass
- **Replaces children** - Sets all children at once (doesn't append)
- **Type-safe** - Returns the same widget type for method chaining

This resolves a common AI hallucination where agents would naturally expect methods like `Card.Children()` or `GridView.Children()` to exist. Now they do!

### Optional Secrets for Flexible Configuration

The **Secret** record now supports an `Optional` parameter, allowing you to declare secrets that are useful but not required for application startup. When a secret is marked as optional, Ivy will skip it during the missing configuration check, preventing startup failures when the secret isn't provided.

**Mark a secret as optional:**

```csharp
public class MyConnection : IHaveSecrets
{
    public Secret[] GetSecrets()
    {
        return new[]
        {
            new Secret("API_KEY"),                        // Required by default
            new Secret("WEBHOOK_URL", Optional: true),    // Optional - won't block startup
            new Secret("DEBUG_TOKEN", Optional: true)     // Optional - nice to have
        };
    }
}
```

**Common use cases:**

```csharp
// Email service with optional SMTP credentials for dev environments
public class EmailConnection : IHaveSecrets
{
    public Secret[] GetSecrets()
    {
        return new[]
        {
            new Secret("SMTP_HOST", Optional: true),
            new Secret("SMTP_PASSWORD", Optional: true)
        };
    }
}

// Analytics integration that degrades gracefully without credentials
public class AnalyticsConnection : IHaveSecrets
{
    public Secret[] GetSecrets()
    {
        return new[]
        {
            new Secret("ANALYTICS_API_KEY", Optional: true),
            new Secret("ANALYTICS_TRACKING_ID", Optional: true)
        };
    }
}
```

This is particularly useful for:
- **Development environments** where certain integrations aren't needed
- **Graceful degradation** scenarios where features can work with reduced functionality
- **Third-party integrations** that enhance but aren't critical to your application

By default, secrets remain required (`Optional = false`), so existing code behavior is unchanged.

### DateRangeInput Min and Max Constraints

The **DateRangeInput** widget now supports date range constraints with `Min` and `Max` properties. Restrict selectable dates to specific ranges—perfect for booking systems, date filters with valid time windows, or any scenario requiring date boundaries. Dates outside the valid range are automatically disabled in the picker.

**Basic usage:**

```csharp
var bookingRange = UseState<(DateOnly?, DateOnly?)>(() => (null, null));

return bookingRange.ToDateRangeInput()
    .Min(new DateOnly(2026, 1, 1))
    .Max(new DateOnly(2026, 12, 31))
    .Placeholder("Select dates within 2026");
```

**Booking system with future dates only:**

```csharp
var checkInOut = UseState<(DateOnly?, DateOnly?)>(() => (null, null));

return checkInOut.ToDateRangeInput()
    .Min(DateOnly.FromDateTime(DateTime.Today))
    .Max(DateOnly.FromDateTime(DateTime.Today.AddYears(1)))
    .Placeholder("Select check-in and check-out dates")
    .Format("MMM dd, yyyy");
```

**Features:**

- **Visual feedback** - Dates outside the range appear disabled in the date picker
- **Flexible constraints** - Set Min only, Max only, or both to define your valid range
- **Works with all formats** - Constraints apply regardless of your chosen date format

Use Min/Max constraints to guide users toward valid date selections and prevent invalid date ranges at the input level.

### Improved Error Display System

Ivy now features a completely redesigned error page system that provides clearer, more actionable error messages with optional technical details. The new system replaces the simple "Ouch! :|" page with a comprehensive error display that adapts to different error scenarios (not found, server errors, unauthorized access, etc.) and includes a "View details" button for debugging information.

**What users will see:**

- **Context-aware error messages** - Different error types (NotFound, NoApps, ServerError, Unauthorized, Forbidden) show appropriate messages and styling
- **Technical details on demand** - A "View details" button appears for errors with additional context (stack traces, server responses)
- **Cleaner browser history** - Error pages no longer pollute your browser history, so the back button works as expected
- **Better download error handling** - Failed or expired downloads now redirect to friendly error pages instead of showing raw exceptions

**For framework developers:**

The new `ErrorAppArgs` provides factory methods for common error scenarios:

```csharp
// Not found errors
var args = ErrorAppArgs.ForNotFound();
var argsJson = ErrorAppArgs.ToArgsJson(args);

// Server errors with technical details
var args = ErrorAppArgs.ForServerError(
    message: "Failed to process request",
    details: exception.ToString()
);

// Unauthorized access
var args = ErrorAppArgs.ForUnauthorized();

// Custom errors
var args = ErrorAppArgs.Custom(
    title: "Payment Required",
    message: "Please upgrade your plan to access this feature.",
    kind: "PaymentRequired"
);
```

The framework automatically handles common scenarios like 404s, authentication failures, and download errors with appropriate error pages—no additional code needed.

### DataTable Empty State Customization

The **DataTable** now supports custom empty states when no data is available. Instead of showing just a loading spinner or blank table, you can now configure exactly what users see when the table is empty—perfect for adding helpful messaging, call-to-action buttons, or onboarding guidance.

**Basic usage with default empty state:**

When you create a DataTable, it now automatically includes a default empty state showing "No items found" and "This table is currently empty." No code changes needed—this appears automatically when your query returns zero rows.

**Customize the empty state:**

Use the new `Empty()` method to provide your own empty state view:

```csharp
var people = db.People.AsQueryable();

return people.ToDataTable(e => e.Id)
    .Header(e => e.Name, "Name")
    .Header(e => e.Email, "Email")
    .Empty((context) => Layout.Vertical()
        .Padding(16)
        .Gap(8)
        .Align(Align.Center)
        | Text.Block("No people found")
            .Large()
            .Bold()
        | Text.Block("Add people to get started")
            .Color(Colors.Muted)
        | new Button("Add Person", variant: ButtonVariant.Primary)
    )
    .Height(Size.Units(100));
```

**What you get:**

- **Automatic empty detection** - Server-side `Count()` query detects empty tables
- **No loading flicker** - Empty state shows immediately instead of "Loading..." when data doesn't exist
- **Full widget tree** - Use Layout.Vertical(), Card, Button, or any other Ivy widgets to design your empty state
- **Context access** - The factory function receives `IViewContext`, so you can use hooks like UseState or UseService if needed

The `Empty()` method accepts a `FuncViewBuilder` (which is `Func<IViewContext, object?>`), giving you complete flexibility to build engaging empty states that guide users toward their next action.

### Form-Level Disabled State

The **FormBuilder** now supports disabling entire forms with a single method call. Use the new `Disabled()` method to disable all input fields and the submit button at once—perfect for read-only modes, pending operations, or conditional form access based on permissions.

**Disable the entire form:**

```csharp
var model = UseState(new UserProfile());

var formBuilder = model.ToForm("User Profile")
    .Disabled(true)  // Disables all fields and submit button
    .Required(m => m.Name, m => m.Email)
    .Label(m => m.Name, "Full Name")
    .Label(m => m.Email, "Email Address");
```

**Conditional disabling based on state:**

```csharp
var isEditing = UseState(false);
var user = UseState(new User());

var formBuilder = user.ToForm("User Details")
    .Disabled(!isEditing.Value)  // Disable when not in edit mode
    .Required(m => m.Name)
    .Label(m => m.Name, "Name");

// Form becomes editable when user clicks Edit button
return Layout.Vertical()
    | formView
    | new Button("Edit").OnClick(_ => isEditing.Set(true));
```

**What changed:**

- **New API**: `Disabled()` method disables all form fields and the submit button
- **Bug fix**: Per-field disabled state now properly applies to inputs (previously stored but not applied)
- **Bug fix**: Default disabled value for fields corrected from `true` to `false`

The form-level disabled state works alongside per-field disabled configuration—individual field disabled states are OR'd with the form-level state, so either can disable a field.

### Stream-Based UseDownload for Large Files

The **UseDownload** hook now supports stream-based downloads with new overloads that accept `Func<Stream>` and `Func<Task<Stream>>` factories. This enables memory-efficient downloads of large files (audio, video, PDFs) without loading them entirely into memory. Stream downloads automatically support HTTP range requests (206 partial content), enabling features like seeking in audio and video players.

**Basic usage with sync factory:**

```csharp
var downloadUrl = UseDownload(
    factory: () => File.OpenRead("large-video.mp4"),
    mimeType: "video/mp4",
    fileName: "video.mp4"
);

return new Button("Download Video")
    .Url(downloadUrl.Value);
```

**Async factory for database or API sources:**

```csharp
var downloadUrl = UseDownload(
    factory: async () =>
    {
        var stream = await fileService.GetFileStreamAsync(fileId);
        return stream;
    },
    mimeType: "audio/mpeg",
    fileName: "audio.mp3"
);

// Use in AudioPlayer with seeking support
return new AudioPlayer(downloadUrl.Value);
```

**What you get:**

- **Memory efficient** - Files are streamed directly to the client without loading into memory
- **Range request support** - HTTP 206 partial content enables seeking in audio/video players
- **Automatic cleanup** - Streams are automatically disposed after download completes
- **Same API surface** - Works just like the byte array overload, but with Stream instead

Use stream-based downloads for any large file where loading the entire content into a `byte[]` would be wasteful or impractical. The range request support is particularly valuable for media files where users expect to seek/scrub through content.

### Form Submit Strategy and Enter Key Support

The **Form Builder** now supports configurable submit strategies with the new `SubmitStrategy()` method. Set your form to `FormSubmitStrategy.OnSubmit` to enable Enter key submission and use the new `OnSubmit()` callback to handle form submissions directly in the builder chain.

**Basic usage with OnSubmit callback:**

```csharp
var credentials = UseState(new LoginFormModel());

var formBuilder = credentials.ToForm("Login")
    .Required(m => m.User, m => m.Password)
    .Label(m => m.User, "User")
    .Label(m => m.Password, "Password")
    .Builder(m => m.User, state => state.ToTextInput())
    .Builder(m => m.Password, state => state.ToPasswordInput())
    .SubmitStrategy(FormSubmitStrategy.OnSubmit)
    .SubmitTitle("Login")
    .OnSubmit(async model =>
    {
        // Handle form submission here
        // model contains validated form data
        await auth.LoginAsync(model.User, model.Password);
    });

var (submitForm, formView, _, submitting) = formBuilder.UseForm(this.Context);
```

**What this enables:**

- **Enter key submission** - Users can press Enter in any field to submit the form
- **Cleaner code** - Handle submission logic directly in the form builder instead of in button click handlers
- **Submit button title** - Configure the submit button text with `SubmitTitle()`
- **Model parameter** - The `OnSubmit` callback receives the validated model, eliminating the need to read from state

When `SubmitStrategy.OnSubmit` is configured, calling `submitForm()` will automatically run validation, update field errors, and invoke your `OnSubmit` callback if validation passes.

## Documentation

### Chat Tutorial Updated to Microsoft.Extensions.AI

The **Chat Tutorial** has been updated to use `Microsoft.Extensions.AI.IChatClient` instead of Semantic Kernel, reflecting the modern .NET approach for AI integration. The tutorial now shows how to build AI-powered chat applications using the `IChatClient` interface, which is becoming the standard abstraction for chat completions in .NET.

**What changed:**

The tutorial demonstrates registering an `IChatClient` in your services:

```csharp
var openAiClient = new OpenAIClient(new ApiKeyCredential(apiKey));
server.Services.AddSingleton<IChatClient>(
    openAiClient.GetChatClient("gpt-4o").AsIChatClient());
```

And using it in your agent implementation:

```csharp
public class LucideIconAgent(IChatClient chatClient)
{
    public async Task<string?> SuggestIconAsync(string appDescription)
    {
        var messages = new List<Microsoft.Extensions.AI.ChatMessage>
        {
            new(ChatRole.System, "Your prompt..."),
            new(ChatRole.User, appDescription)
        };

        var result = await chatClient.GetResponseAsync(messages);
        return result.Text;
    }
}
```

The tutorial also includes a new FAQ entry in the **UseStream** hook documentation showing how to stream `IChatClient` responses to the UI with `UseStream<TextRun>`.

### New Documentation FAQ Entries

Several widgets now include helpful FAQ entries for common questions:

- **TextBlock**: Why text wraps vertically in horizontal layouts and how to fix with `.NoWrap()`
- **Button**: How to handle async operations in button click handlers (use `async` directly, no special hooks needed)
- **UseStream**: How to stream IChatClient responses with `UseStream<TextRun>` and `GetStreamingResponseAsync()`

## CLI Improvements

### CLI Command Discovery with 'explain'

The Ivy CLI now documents **`ivy cli explain`** as the preferred method for discovering available commands. This command provides a complete structural breakdown of all CLI commands and their options, powered by Spectre.Console.Cli.

**Usage:**

```bash
# Get a structural breakdown of all available CLI commands
ivy cli explain
```

This is now the recommended approach for command discovery, providing a reliable built-in way to explore what the CLI can do. You can still use `ivy --help` for general help or `ivy [command] --help` for specific command details, but `explain` gives you the full picture in one view.

## Breaking Changes

### IConnection API Changes

The **IConnection** interface has been updated with two important changes that will require updates to any custom connection implementations:

**1. RegisterServices now accepts Server instead of IServiceCollection:**

The `RegisterServices` method signature has changed to provide access to the full server context during service registration.

**Before:**
```csharp
public void RegisterServices(IServiceCollection services)
{
    services.AddSingleton<MyService>();
}
```

**After:**
```csharp
public void RegisterServices(Server server)
{
    server.Services.AddSingleton<MyService>();
}
```

**2. New TestConnection method required:**

All `IConnection` implementations must now implement a `TestConnection` method for configuration validation:

```csharp
public Task<(bool ok, string? message)> TestConnection(IConfiguration config)
{
    // Validate the connection using config if applicable
    return Task.FromResult((true, (string?)null));
}
```

**Migration steps:**

1. Find every class implementing `IConnection` in your codebase
2. Update `RegisterServices` parameter from `IServiceCollection services` to `Server server`
3. Replace references to `services` with `server.Services` inside the method
4. Add the `TestConnection` method implementation

The framework includes a detailed refactor guide at `src/.releases/Refactors/Upcoming/IConnection-API.md` with additional migration information.

### AppAttribute Parameter Renamed: path → group

The `[App]` attribute parameter `path` has been renamed to `group` for better clarity. If you're using the `[App]` attribute to register views in your application, you'll need to update the parameter name.

**Before:**

```csharp
[App(icon: Icons.Camera, path: ["Widgets", "Inputs"])]
public class CameraInputApp : ViewBase
{
    // ...
}
```

**After:**

```csharp
[App(icon: Icons.Camera, group: ["Widgets", "Inputs"])]
public class CameraInputApp : ViewBase
{
    // ...
}
```

This is a simple find-and-replace change in your codebase—search for `path:` in `[App]` attributes and replace with `group:`.

## Performance & UX Improvements

### Optimistic Rendering for All Input Widgets

All input widgets in Ivy now feature **optimistic rendering** for instant, responsive interactions. When you type, select, or adjust any input, the UI updates immediately without waiting for server round-trips. The framework maintains local state during active interaction and syncs server values when you're done, giving you a smooth, lag-free experience.

**Affected widgets:**

This improvement applies to all input widgets including TextInput, CodeInput, NumberInput, DateTimeInput, DateRangeInput, SelectInput, BoolInput, ColorInput, IconInput, FeedbackInput, SignatureInput, and range inputs. You don't need to change any code—the optimization is automatic and transparent.

**What you'll notice:**

- **Instant feedback** - Text appears immediately as you type, toggles respond instantly, sliders move smoothly
- **No input lag** - UI updates happen in real-time regardless of network latency or server processing time
- **Automatic sync** - Server state updates are applied when you're not actively interacting, keeping everything in sync

This creates a native-app feel for all Ivy inputs while maintaining the framework's server-driven architecture. The implementation uses a shared `useOptimisticValue` hook that intelligently manages local and server state across all input types.

### Visual Separators in Grouped SelectInput

The **SelectInput** dropdown variant now displays visual separators between option groups, making it easier to distinguish between different groups in the dropdown menu. When you configure grouped options using the `Group` property, a subtle divider line automatically appears between each group, improving readability and visual hierarchy.

**What you'll see:**

- **Clearer group boundaries** - Visual separators make it immediately obvious where one group ends and another begins
- **Improved readability** - Easier to scan through long lists of grouped options
- **Automatic** - No code changes needed; separators appear automatically when using grouped options

This improvement applies to SelectInput widgets using the dropdown variant (default) with grouped options. The separators maintain consistent styling with the rest of the select component across all themes.

### Improved Toast Notification Spacing

Multiple simultaneous toast notifications now have tighter, more refined spacing between them. The gap between toasts has been reduced to 4px (from 8px), creating a more compact and polished appearance while still maintaining clear visual separation between individual notifications.

### ScreenshotFeedback Neutral Color Scheme

The **ScreenshotFeedback** widget overlay now uses a neutral gray color scheme instead of the previous green theme. The toolbar, buttons, and overlay background now feature modern gray tones that better complement any application design and work seamlessly across both light and dark modes. This provides a more professional, polished appearance for the feedback capture interface.

## Bug Fixes

### Fixed Race Condition in State<T>.Subscribe

Fixed a race condition in `State<T>.Subscribe` that could cause state updates to be missed in certain timing scenarios. The subscription is now established before sending the initial value, ensuring no updates are lost during the subscription process. This improves the reliability of state subscriptions throughout the framework.

### Fixed VideoPlayer PlaybackRate on Initial Load

Fixed an issue where the `PlaybackRate` setting wasn't applied when videos first loaded. Browsers reset the playback rate to 1.0 during media load, causing the configured playback speed to be ignored initially. The VideoPlayer now correctly sets both `defaultPlaybackRate` and `playbackRate` during initialization and re-applies the speed after the video loads, ensuring your configured playback rate takes effect immediately.

### Fixed DateTime Timezone Shift in DataTables

Fixed a critical issue where DateTime values with unspecified timezone (`DateTimeKind.Unspecified`) were incorrectly treated as local time during Arrow serialization in DataTables. This caused midnight values to shift by the server's UTC offset, breaking date range filters and causing incorrect filtering results. DateTime values with unspecified kind are now correctly treated as UTC, preventing unintended timezone conversions and ensuring date filters work as expected.

### Fixed SignatureInput State Binding

Fixed a critical bug where the `OnChange` event wasn't being wired up for state-bound SignatureInput widgets, preventing signatures from being captured into state. Additionally, resolved base64 encoding issues where the data URL prefix (`data:image/png;base64,`) was causing serialization problems with C#'s `byte[]` deserialization. The widget now correctly strips the prefix when sending to the backend and restores it when displaying, ensuring seamless state binding with the `ToSignatureInput()` extension method.

### Fixed PieChart Ignoring Custom Data Key Configuration

Fixed an issue where the **PieChart** widget ignored custom `dataKey` and `nameKey` properties configured in the `Pie` configuration. The chart was hardcoded to use `measure` and `dimension` properties from the data, making it impossible to use custom property names. The widget now correctly reads and applies the configured `dataKey` and `nameKey` from your Pie configuration, while maintaining backwards compatibility by falling back to `measure`/`dimension` when no custom keys are specified.

### Fixed ScreenshotFeedback Upload Race Condition

Fixed a critical timing issue in the **ScreenshotFeedback** widget where screenshot content wasn't available in the `OnSave` event handler. The widget was firing the event before the upload completed, causing the C# handler to always receive null content. The upload now completes before the event fires, ensuring screenshot data is available when you handle the save event. Also added a visual spinner and disabled state to the Submit button during upload to provide better user feedback.

### Fixed Ghost Variant Select Inputs in Dark Mode

Fixed a visual issue where **SelectInput** and **AsyncSelectInput** widgets using the ghost variant displayed unwanted borders in dark mode. Ghost variants are now correctly borderless and transparent in both light and dark modes, with proper hover state styling maintained across all themes.

### Fixed Double Eye Icon in Password Inputs

Fixed a visual bug where **PasswordInput** widgets displayed two eye icons for toggling password visibility—one from Ivy's custom toggle and one from the browser's native password reveal button. The browser's native reveal controls are now properly suppressed, and the toggle button styling has been corrected to ensure only Ivy's custom eye icon appears. This provides a cleaner, more consistent password input experience across all browsers.

### Fixed X-Axis Label Overlap in Horizontal Bar Charts

Fixed a visual issue where large numbers on the x-axis in horizontal bar charts would overlap and become unreadable. The x-axis now uses the same compact number formatting (K/M/B notation) as the y-axis when displaying numeric values, preventing overlap and improving readability. Category axes (text labels) continue to use string-wrapping for long text.

### Fixed Nullable NumberInput Showing 0 Instead of Empty Field

Fixed a critical bug where nullable `NumberInput<T>` widgets displayed `0` instead of an empty field when the value was `null`. The issue occurred because the `Nullable` property wasn't being serialized to the frontend, causing the default `nullable=false` behavior to convert null values to 0. Number inputs with nullable types now correctly display empty fields when the value is null, providing proper nullable input behavior.

### Fixed Grid Layout Children Lost in Pipe Chains

Fixed a critical layout bug where children piped after a `GridView` inside a `LayoutView` would be lost. When using syntax like `Layout.Vertical() | Layout.Grid().Columns(2) | input1 | input2`, the left-associative pipe operator caused subsequent children to be added to the parent `LayoutView` instead of the `GridView`, resulting in an empty grid with inputs stacked vertically.

The framework now tracks the last-piped `GridView` and automatically routes subsequent non-ViewBase children to the active grid until a new ViewBase child or null resets the context. This fix ensures that intuitive pipe chains work as expected:

```csharp
// Now works correctly - inputs go into the grid
return Layout.Vertical()
    | Text.H2("My Form")
    | Layout.Grid().Columns(2)
    | label1 | input1
    | label2 | input2;
```

Parenthesized nesting continues to work correctly as before.

### Fixed JsonRenderer Infinite Re-render Loop

Fixed a critical bug in the **JsonRenderer** component that caused infinite re-rendering and browser freezing. The issue occurred when parsing JSON strings because `JSON.parse()` creates new object references on every render, causing the component to continuously detect changes and re-render. The component now uses `useMemo` to cache parsed data and follows React's recommended pattern for adjusting state when props change, preventing unnecessary re-renders. Additionally, the null check was improved to correctly handle valid falsy JSON values like `0`, `false`, and empty strings.

### Fixed Chart Series Auto-Plotting Unwanted Data

Fixed issues where **AreaChart**, **BarChart**, and **LineChart** widgets would auto-plot all numeric fields in your data instead of respecting explicitly configured series. When you configure specific series via `.Line()`, `.Bar()`, or `.Area()` calls, the charts now only render those configured data keys instead of including every numeric property from your data objects.

Additionally fixed **BarChart** legend labels to use proper PascalCase formatting (e.g., "totalRevenue" now displays as "Total Revenue") to match the behavior of LineChart and AreaChart.

**What changed:**
- Charts filter to only plot explicitly configured series when series are defined via `.Line()`, `.Bar()`, or `.Area()` methods
- Falls back to auto-discovery behavior when no series are configured
- Fixed case-insensitive dataKey matching for AreaChart series configuration
- BarChart legends now use `SplitPascalCase` for better readability

This ensures your chart configurations are respected and prevents unwanted data keys from appearing in your visualizations.

### Fixed BladeHeader Content Collapsing

Fixed a layout bug in the **BladeHeader** where content without explicit width would collapse and become invisible. The slot container inside BladeHeader had no flex or grow properties, causing search inputs, titles, and other header content to disappear in certain layouts. The header slot now includes `flex-1 min-w-0` by default, ensuring content fills the available header space properly.

**Before the fix:**
```csharp
// Header content would collapse without explicit width
var header = Layout.Horizontal().Gap(1)
    | filter.ToSearchInput().Placeholder("Search")  // Would be invisible!
    | Icons.Plus.ToButton(_ => Create()).Ghost();
```

**After the fix:**
```csharp
// Header content now renders correctly without needing .Width()
var header = Layout.Horizontal().Gap(1)
    | filter.ToSearchInput().Placeholder("Search")  // Now visible!
    | Icons.Plus.ToButton(_ => Create()).Ghost();
```

This fix ensures BladeHeader content displays correctly without requiring manual width configuration on every element.
