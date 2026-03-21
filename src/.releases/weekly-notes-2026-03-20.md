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

Ivy now includes **GaugeChart** for KPI-style circular gauges (progress, load, completion). Configure **Min/Max**, **Label**, **StartAngle/EndAngle** (semicircle or custom arcs), **Thresholds** for color zones, **Pointer** (Line, Arrow, Rounded via `GaugePointerStyle`), **Animated**, and **ColorScheme** for theming. Labels and ticks scale with the arc.

**Note:** Pointer styles include Line, Arrow, and Rounded; use `.Animated(false)` to freeze value transitions.

```csharp
new GaugeChart(67)
    .Min(0)  // range
    .Max(100)  // range
    .Label("System Health")  // caption
    .StartAngle(180)  // arc start (degrees)
    .EndAngle(0)  // arc end (degrees)
    .Animated(true)  // animate value changes
    .ColorScheme(ColorScheme.Rainbow)  // theme (Default | Rainbow)
    .Thresholds(
        new GaugeThreshold(33, "#10b981"),  // zone (value, color)
        new GaugeThreshold(66, "#f59e0b"),
        new GaugeThreshold(100, "#ef4444"))
    .Pointer(new GaugePointer()
        .Style(GaugePointerStyle.Arrow)  // Line | Arrow | Rounded
        .Width(8)
        .Length("70%"))  // pointer appearance
    .Height(Size.Px(250));  // layout
```

### Native File Dialog Hooks

**UseFileDialog** (PathOnly or upload via `IUploadHandler`), **UseSaveDialog** (byte factory + MIME + suggested name), and **UseFolderDialog** each return `(dialogView, show…)` — render every `dialogView` in the tree, then call `show…(callback)` when the user should see the dialog (same pattern as `UseAlert`).

**Note:** PathOnly `UseFileDialog(accept?, multiple?)` — omit `accept` for any file type; filter with `"image/*"` or `".pdf,.txt"`. Upload mode uses `MemoryStreamUploadHandler.Create(state)`. Avoid `accept: "*/*"` on older clients. `UseSaveDialog` callback receives **`SaveDialogResult`**; folder callback receives **`FolderDialogEntry[]`**.

```csharp
var summary = UseState<string?>(null);

var (fileDialog, showFileDialog) = UseFileDialog();  // PathOnly — FileDialogFileInfo[]; or UseFileDialog(accept: "image/*")
var (saveDialog, showSaveDialog) = UseSaveDialog(
    () => Task.FromResult(Encoding.UTF8.GetBytes("Hello")),  // content factory
    "text/plain",  // MIME
    "hello.txt");  // suggested file name
var (folderDialog, showFolderDialog) = UseFolderDialog();  // FolderDialogEntry[]

return Layout.Vertical()
    | fileDialog  // required in tree (no visible UI)
    | saveDialog
    | folderDialog
    | new Button("Select file").OnClick(_ => showFileDialog(files =>
    {
        var f = files.FirstOrDefault();
        summary.Set(f != null ? f.FileName : "No file");
    }))
    | new Button("Save as…").OnClick(_ => showSaveDialog(r => { /* r.Success, r.FileName */ }))
    | new Button("Select folder").OnClick(_ => showFolderDialog(entries => { /* Name, Kind, RelativePath */ }))
    | Text.P(summary.Value ?? "No selection");
```

## API Improvements

### UseClipboard Hook

**UseClipboard** returns **`Action<string>`** — call it with the text to copy. It wraps `IClientProvider.CopyToClipboard()` for a smaller API. **Write-only** (no read-from-clipboard).

```csharp
var copyToClipboard = UseClipboard();  // Action<string>

return new Button("Copy greeting", _ => copyToClipboard("Hello, World!"));
```

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
