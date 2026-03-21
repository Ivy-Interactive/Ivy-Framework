# Ivy Framework Weekly Notes - Week of 2026-03-20

## New Features

### VideoPlayer: time range, events, and playback speed

The **VideoPlayer** widget now supports **time range** (`StartTime` / `EndTime` in seconds), **playback events** (`HandlePlay`, `HandlePause`, `HandleEnded`, `HandleLoaded`—each with async, sync-with-event, and parameterless overloads), and **playback speed** (`PlaybackRate`, minimum 0.25×). Use time ranges for clips and tutorials, events for analytics and coordinated UI, and speed for review or fast-forward on HTML5 sources.

**Note:** Time ranges use Media Fragments (HTML5) and embed parameters (YouTube). `PlaybackRate` applies to native HTML5 video only; YouTube embeds do not change speed.

```csharp
var playCount = UseState(0);
var completed = UseState(false);

var video = new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
    .StartTime(2)  // time range
    .EndTime(6)  // time range
    .PlaybackRate(1.5)  // playback speed
    .HandlePlay(_ => playCount.Set(playCount.Value + 1))  // event: play
    .HandlePause(_ => Console.WriteLine("Video paused"))  // event: pause
    .HandleEnded(_ => completed.Set(true))  // event: ended
    .HandleLoaded(_ => Console.WriteLine("Video ready"))  // event: metadata loaded
    .Height(Size.Units(50));  // layout
```

### SignatureInput Widget

Ivy now includes **SignatureInput** for handwritten signatures in the browser (delivery confirmations, contracts, approvals). Canvas drawing supports mouse and touch; data is **PNG** as `byte[]`. Use **`.WithField()`** / **`.Label()`** for forms and **`.Pen()`** / **`.PenThickness()`** (and optional background) to match your UI.

**Note:** A built-in clear control appears once the user has drawn something. Forms can use disabled/invalid states like other inputs.

```csharp
public class SignatureDemo : ViewBase
{
    public override object? Build()
    {
        var signature = UseState<byte[]?>(null);

        return Layout.Vertical()
            | signature.ToSignatureInput()
                .Placeholder("Draw your signature here")  // canvas hint
                .Pen(Colors.Blue)  // pen color (e.g. Colors.Red)
                .PenThickness(4)  // line width
                .WithField()  // form integration
                .Label("Customer Signature")  // field label
            | Text.P(signature.Value != null
                ? $"Signature captured ({signature.Value.Length} bytes)"
                : "No signature yet");  // status
    }
}
```

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

## Bug Fixes

- **State.Subscribe**: Fixed a race in `State<T>.Subscribe` where updates could be missed; the subscription is established before the initial value is sent, so nothing is lost during setup.
- **VideoPlayer**: `PlaybackRate` now applies on first load—sets `defaultPlaybackRate` and `playbackRate` on init and reapplies after load (browsers reset speed to 1.0 during media load).
- **DataTables**: `DateTimeKind.Unspecified` values are no longer treated as local during Arrow serialization (avoids midnight shifting and broken date filters); unspecified kind is handled as UTC.
- **SignatureInput**: Wired `OnChange` for state-bound widgets; strips/restores the `data:image/png;base64,` prefix for correct `byte[]` round-tripping with `ToSignatureInput()`.
- **PieChart**: Honors `dataKey` and `nameKey` from `Pie` config instead of hardcoding `measure` / `dimension`; falls back to the old keys when unset.
- **ScreenshotFeedback**: `OnSave` runs after upload completes so content is non-null; Submit shows a spinner and is disabled while uploading.
- **SelectInput / AsyncSelectInput**: Ghost variant is borderless and transparent in dark mode (hover styling preserved).
- **PasswordInput**: Suppresses the browser’s native reveal so only Ivy’s toggle shows (no double eye icons).
- **BarChart (horizontal)**: X-axis numeric labels use compact K/M/B formatting like the Y-axis to avoid overlap; category axes still wrap text.
- **NumberInput**: Nullable inputs show empty when the value is `null`—`Nullable` is serialized so the client no longer coerces null to `0`.
- **Layout / Grid**: Pipe chains such as `Layout.Vertical() | Layout.Grid().Columns(2) | …` now route following children into the grid until reset; parenthesized nesting unchanged.
- **JsonRenderer**: Stops infinite re-renders by memoizing parsed JSON and aligning state updates with prop changes; falsy JSON (`0`, `false`, `""`) handled correctly.
- **AreaChart / BarChart / LineChart**: With explicit `.Line()` / `.Bar()` / `.Area()` series, only those keys are plotted; auto-discovery remains when nothing is configured. Case-insensitive `dataKey` matching for AreaChart; BarChart legends use `SplitPascalCase` (e.g. `totalRevenue` → “Total Revenue”) like the other charts.
