# Ivy Framework Weekly Notes - Week of 2026-03-16

## New Features

### New Calendar Widget

Manage events and schedules with the new `Calendar` widget! Display events in four different views—month, week, day, and agenda—with full interactive capabilities including drag-and-drop, event clicks, and time slot selection.

```csharp
return new Calendar(
    new CalendarEvent()
        .EventId("meeting-1")
        .Title("Team Standup")
        .Start(new DateTime(2026, 3, 17, 9, 0, 0))
        .End(new DateTime(2026, 3, 17, 9, 30, 0))
        .Color("primary"),

    new CalendarEvent()
        .EventId("deadline-1")
        .Title("Project Deadline")
        .Start(new DateTime(2026, 3, 20, 0, 0, 0))
        .End(new DateTime(2026, 3, 20, 23, 59, 59))
        .AllDay(true)
        .Color("destructive")
);
```

**Configure view mode and date range:**

```csharp
new Calendar()
    .DefaultView(CalendarDisplayMode.Week)  // Month, Week, Day, or Agenda
    .Date(DateTime.Now)
    .MinDate(new DateTime(2026, 1, 1))
    .MaxDate(new DateTime(2026, 12, 31))
    .ShowToolbar(true)
```

**Handle user interactions:**

```csharp
new Calendar()
    .EnableDragDrop(true)
    .OnEventClick(eventId =>
    {
        Console.WriteLine($"Clicked event: {eventId}");
    })
    .OnEventMove((eventId, start, end) =>
    {
        Console.WriteLine($"Moved {eventId} to {start}");
    })
    .OnEventResize((eventId, start, end) =>
    {
        Console.WriteLine($"Resized {eventId}");
    })
    .OnSelectSlot((start, end) =>
    {
        Console.WriteLine($"Selected time slot: {start} to {end}");
    })
```

**Build from collections with `.ToCalendar()`:**

Similar to `.ToKanban()`, you can create calendars from any collection using the builder pattern:

```csharp
record Meeting(string Id, string Title, DateTime Start, DateTime End, string Category);

var meetings = UseState(new[]
{
    new Meeting("1", "Team Sync", DateTime.Today.AddHours(9), DateTime.Today.AddHours(10), "Work"),
    new Meeting("2", "Design Review", DateTime.Today.AddHours(14), DateTime.Today.AddHours(15), "Work"),
    new Meeting("3", "Gym", DateTime.Today.AddDays(1).AddHours(7), DateTime.Today.AddDays(1).AddHours(8), "Personal")
});

return meetings.Value
    .ToCalendar(
        startSelector: m => m.Start,
        endSelector: m => m.End,
        eventIdSelector: m => m.Id)
    .Title(m => m.Title)
    .Color(m => m.Category == "Work" ? "Blue" : "Green")
    .DefaultView(CalendarDisplayMode.Week)
    .EnableDragDrop()
    .OnMove(move =>
    {
        var updated = meetings.Value.Select(m =>
        {
            if (m.Id == move.EventId?.ToString())
                return m with { Start = move.Start, End = move.End };
            return m;
        }).ToArray();
        meetings.Set(updated);
    });
```

The builder pattern makes it easy to map your domain objects to calendar events while maintaining type safety and enabling drag-and-drop updates.

**Add events dynamically:**

```csharp
var calendar = new Calendar();

// Add events using the | operator
calendar = calendar
    | new CalendarEvent()
        .Title("Morning Coffee")
        .Start(DateTime.Now.AddHours(1))
        .End(DateTime.Now.AddHours(1.5))
    | new CalendarEvent()
        .Title("Lunch Break")
        .Start(DateTime.Now.AddHours(4))
        .End(DateTime.Now.AddHours(5))
        .AllDay(false);
```

**Key features:**

- **Four view modes**: Month grid, week time grid, single day view, and agenda list
- **All-day events**: Set `.AllDay(true)` for events that span entire days
- **Color coding**: Use theme colors like "primary", "destructive", "warning", or "success"
- **Interactive toolbar**: Built-in navigation with Today/Previous/Next buttons and view switcher
- **Event interactions**: Click events, drag to move, resize handles, and click empty slots to create
- **Theme-aware**: Automatically adapts to your app's theme with proper contrast

The [Calendar](https://docs.ivy.app/widgets/advanced/calendar) widget follows the same pattern as Kanban, where `CalendarEvent` children define the data while the parent `Calendar` manages the display and interactions.

### New XAML Renderer Widget

**This is a new feature.** Ivy now includes a **XAML renderer** that turns Ivy XAML markup (XML) into live UI. You can define layouts, badges, buttons, cards, charts, and more in XML and have them rendered as normal widgets—ideal for dynamic UIs, generated content, or the Advanced to Xaml Builder sample.

**Reference the package:**

The renderer lives in the `Ivy.XamlBuilder` package. When using the framework from source, the sample app and XamlBuilder are already available; for package-based projects, add:

```bash
dotnet add package Ivy.XamlBuilder
```

**Basic usage:**

```csharp
using Ivy;

var builder = new XamlBuilder();
var widget = builder.Build(xamlString);

// Use the resulting widget in your view
return Layout.Vertical()
    | Text.H2("Preview")
    | widget;
```

### Build Desktop Apps with Ivy

You can now run Ivy applications as **native desktop apps** on Windows, macOS, and Linux using the **Ivy.Desktop** package. The stack uses [Photino](https://tryphotino.io) for a lightweight, cross-platform desktop host.

**Install the package:**

```bash
dotnet add package Ivy.Desktop
```

**Run your app as a desktop window:**

Create your Ivy app (views, widgets, hooks) as usual, then host it with a desktop window instead of a browser. Configure the window title, size, and optional icon; the window loads your app via the same Ivy server and UI pipeline.

```csharp
using Ivy;
using Ivy.Desktop;

return Layout.Vertical()
    | Text.H2("Hello from Ivy.Desktop!")
    | Text.P("Native desktop UI powered by C#.");

// Program.cs: run as desktop app (use your view type for RootComponent)
var appDescriptor = new AppDescriptor()
{
    RootComponent = typeof(YourView),  // the view that returns the Layout.Vertical above
    InitialTitle = "My Desktop App",
};
DesktopWindow.Run(appDescriptor, args);
```

**What you get:**

- **Cross-platform:** One codebase for Windows, macOS, and Linux.
- **Lightweight:** Photino-based window.
- **Window options:** Set title, size, resizable, top-most, DPI scaling, center on screen, dev tools, and custom window icon from an embedded resource.

### AudioInput Sample Rate Control

The [`AudioInput`](https://docs.ivy.app/widgets/inputs/audio-input) widget now supports configurable sample rates via `.SampleRate(hz)`. Supported values: 8000, 11025, 16000, 22050, 24000, 32000, 44100, 48000 Hz. Omit for browser default (typically 48000). Use lower rates (e.g. 16000 for speech) for smaller files; higher for music. The framework resamples via the Web Audio API so the requested rate is applied regardless of the microphone’s native rate.

```csharp
new AudioInput(upload).SampleRate(16000)   // speech / voice notes
new AudioInput(upload).SampleRate(48000)   // high-fidelity
```

### New NumberRangeInput Widget

Select numeric ranges with an intuitive dual-handle slider! The new [`NumberRangeInput`](https://docs.ivy.app/widgets/inputs/number-range-input) widget provides a sleek interface for selecting minimum and maximum numeric values, perfect for filtering by price ranges, thresholds, or any min/max bounds.

```csharp
// State: (min, max); use .Item1 and .Item2 for bounds (same pattern as DateRangeInput)
var priceRange = UseState<(int, int)>(() => (25, 75));
return Layout.Vertical()
    | priceRange.ToNumberRangeInput()
        .Min(0)
        .Max(100)
        .Currency("USD")   // or .Percent(true), etc.
        .Precision(2)
        .Step(2)          // slider step interval
    | Text.P($"Price: ${priceRange.Value.Item1} - ${priceRange.Value.Item2}");
```

**Supported types:**

The widget supports all numeric tuple types including `(int, int)`, `(decimal, decimal)`, `(double, double)`, `(float, float)`, `(short, short)`, `(long, long)`, and their nullable variants.

### DateTimeInput and DateRangeInput Enhancements

The date input widgets ([`DateRangeInput`](https://docs.ivy.app/widgets/inputs/date-range-input) and [`DateTimeInput`](https://docs.ivy.app/widgets/inputs/date-time-input)) now support custom placeholder text for date ranges and configurable calendar week start days, making them more flexible for international use and domain-specific scenarios.

Set separate placeholders for the start and end date fields to provide context-specific hints to users:

```csharp
var eventDates = UseState<(DateOnly?, DateOnly?)>(() => (null, null));

eventDates.ToDateRangeInput()
    .StartPlaceholder("Start Date")
    .EndPlaceholder("End Date")
```

When both placeholders are set, they replace the generic "Pick a date range" placeholder with your custom text separated by a dash (e.g., "Check-in - Check-out").

**Custom week start day:**

Customize which day the calendar week starts on for both `DateTimeInput` and `DateRangeInput` to match regional conventions or business requirements:

```csharp
var appointmentDate = UseState(DateOnly.FromDateTime(DateTime.Now));

// Sunday-first calendar (default in US)
appointmentDate.ToDateInput()
    .FirstDayOfWeek(DayOfWeek.Sunday)
```

The `FirstDayOfWeek` property accepts any `DayOfWeek` enum value (Sunday through Saturday) and automatically adjusts the calendar display to start weeks on your specified day.

### FileInput Default Variant

The [`FileInput`](https://docs.ivy.app/widgets/inputs/file-input) widget now supports two display variants: the existing `Drop` variant (default) with its large drag-and-drop zone, and a new `Default` variant that provides a more compact, button-based interface. Both variants fully support drag-and-drop functionality.

```csharp
var file = UseState<FileUpload<byte[]>?>();
var upload = UseUpload(MemoryStreamUploadHandler.Create(file));

return Layout.Vertical()
        | file.ToFileInput(upload)
            .Variant(FileInputVariant.Default)
            .Placeholder("Select file");
```

The Default variant maintains all the same features as the Drop variant including validation, progress tracking, multiple file support, and file type restrictions—just in a more compact form factor.

### CameraInput Widget

Capture photos directly from the user's webcam or device camera with the new [`CameraInput`](https://docs.ivy.app/widgets/inputs/camera-input) widget! It provides a live video preview, one-click capture, and automatic upload—all using the same familiar upload pattern as `FileInput`.

```csharp
var photo = UseState<FileUpload<byte[]>?>();
var upload = UseUpload(MemoryStreamUploadHandler.Create(photo));

return Layout.Vertical()
    | new CameraInput(upload.Value, "Take a photo")
    | (photo.Value != null
            ? Text.P($"Captured: {photo.Value.FileName} ({StringHelper.FormatBytes(photo.Value.Length)})")
            : Text.P("No photo captured yet."));
```

**Camera selection:**

Control which camera to use on devices with multiple cameras:

```csharp
// Front-facing camera (default)
new CameraInput(upload.Value).FacingMode("user")

// Rear-facing camera
new CameraInput(upload.Value).FacingMode("environment")
```

**Access captured photos:**

The widget uses the same upload pattern as FileInput, making it easy to work with captured photos:

```csharp
var photo = UseState<FileUpload<byte[]>?>();
var upload = UseUpload(MemoryStreamUploadHandler.Create(photo));

// Access captured photo data
if (photo.Value != null)
{
    var fileName = photo.Value.FileName;    // "capture.png"
    var fileSize = photo.Value.Length;       // Size in bytes
    var fileData = photo.Value.Content;      // byte[] containing PNG data
}
```

The [`CameraInput`](https://docs.ivy.app/widgets/inputs/camera-input) widget is perfect for profile photos, document scanning, ID verification, or any scenario where you need to capture images directly from the user's camera.

### TextInput Pattern Validation

The `TextInput` widget now supports regex pattern validation with the new `Pattern` property. Validate input formats like email addresses, phone numbers, URLs, postal codes, and custom patterns—all with built-in client-side validation and user-friendly error messages.

```csharp
new TextInput()
    .Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")
    .Placeholder("Enter your email")
```

The error message shown to users is "Please match the requested format". Pattern validation is applied **after** server-provided errors and `MinLength` validation, ensuring the most relevant error is always displayed first.

### New ScatterChart Widget

Scatter charts support scatter plots, bubble charts (size encoding), connected scatter lines, 7 point shapes (Circle, Square, Cross, Diamond, Star, Triangle, Wye), tooltips, legends, toolbox, and grid.

```csharp
var data = new[]
{
    new { Height = 165, Weight = 65, Age = 25 },
    new { Height = 170, Weight = 72, Age = 45 },
    new { Height = 158, Weight = 58, Age = 22 },
    new { Height = 175, Weight = 78, Age = 50 },
    new { Height = 162, Weight = 60, Age = 20 },
};

return new ScatterChart(data)
    .Scatter(new Scatter("Value").Name("People").Shape(ScatterShape.Diamond).ShowLine(true).LineType(ScatterLineType.Smooth))
    .XAxis(new XAxis("Height").Type(AxisTypes.Number))
    .YAxis(new YAxis("Weight").Type(AxisTypes.Number))
    .ZAxis(new ZAxis("Age").Range(40, 200))
    .Tooltip(new ChartTooltip().Animated(true))
    .Legend()
    .CartesianGrid(new CartesianGrid().Horizontal().Vertical())
    .Toolbox();
```

### New RadarChart Widget

Radar charts visualize multi-dimensional data with indicators, filled areas, polygon or circle shape, multiple series, tooltips, legends, and toolbox.

```csharp
var data = new[]
{
    new { name = "Product A", Sales = 85, Marketing = 72, Development = 90, Support = 78, Quality = 88 },
    new { name = "Product B", Sales = 70, Marketing = 88, Development = 75, Support = 92, Quality = 80 },
};

return new RadarChart(data)
    .ColorScheme(ColorScheme.Default)
    .Indicator("Sales", 100).Indicator("Marketing", 100).Indicator("Development", 100).Indicator("Support", 100).Indicator("Quality", 100)
    .Radar(new Radar("values").Filled().Fill(Colors.Primary).Stroke(Colors.Primary))
    .Shape(RadarShape.Circle)
    .Tooltip()
    .Legend()
    .Toolbox();
```

### New SankeyChart Widget

Sankey charts show flows between nodes (link width = magnitude). Options include node width/gap, curvature, left/justify alignment, color schemes, tooltips, and toolbox.

```csharp
var data = new SankeyData(
    Nodes: new[]
    {
        new SankeyNode("Visit"),
        new SankeyNode("Add to Cart"),
        new SankeyNode("Checkout"),
        new SankeyNode("Purchase"),
        new SankeyNode("Bounce"),
    },
    Links: new[]
    {
        new SankeyLink(0, 1, 3500),
        new SankeyLink(0, 4, 1500),
        new SankeyLink(1, 2, 2800),
        new SankeyLink(1, 4, 700),
        new SankeyLink(2, 3, 2200),
        new SankeyLink(2, 4, 600),
    }
);

return new SankeyChart(data)
    .NodeWidth(25)
    .NodeGap(15)
    .Curvature(0.7)
    .NodeAlign(SankeyAlign.Left)
    .ColorScheme(ColorScheme.Rainbow)
    .Tooltip()
    .Toolbox();
```

### New ChordChart Widget

Chord charts visualize relationships between entities in a circular layout. Options include sort, pad angle, color schemes, tooltips, toolbox, and legend.

```csharp
var data = new ChordData(
    Nodes: new[]
    {
        new ChordNode("North America"),
        new ChordNode("Europe"),
        new ChordNode("Asia"),
        new ChordNode("South America"),
        new ChordNode("Africa"),
    },
    Links: new[]
    {
        new ChordLink(0, 1, 1200),
        new ChordLink(0, 2, 900),
        new ChordLink(1, 2, 800),
        new ChordLink(2, 3, 300),
        new ChordLink(3, 4, 150),
    }
);

return new ChordChart(data)
    .Sort(true)
    .SortSubGroups(true)
    .PadAngle(5)
    .ColorScheme(ColorScheme.Rainbow)
    .Tooltip()
    .Toolbox()
    .Legend();
```

### New FunnelChart Widget

Funnel charts show conversion or stage data (uses `PieChartData`). Options include sort (Descending/Ascending/None), vertical/horizontal orientation, gap, tooltips, toolbox, and legend.

```csharp
var data = new[]
{
    new PieChartData("Awareness", 5000),
    new PieChartData("Interest", 3500),
    new PieChartData("Decision", 2100),
    new PieChartData("Action", 1200),
};

return new FunnelChart(data)
    .Funnel("Measure", "Dimension")
    .Sort(FunnelSort.Descending)
    .Orientation(FunnelOrientation.Vertical)
    .Gap(5)
    .Tooltip()
    .Toolbox()
    .Legend();
```

### Enhanced ScreenshotFeedback Widget with Annotation Tools

The `ScreenshotFeedback` widget now includes powerful annotation tools and structured data export! Add callout markers, arrows, blur censoring, and text annotations to screenshots with an Ivy-themed interface and keyboard shortcuts.

**New annotation tools:**

- **Callout**: Two-click numbered annotations perfect for step-by-step instructions
- **Arrow**: Point to specific UI elements with directional arrows
- **Censor**: Blur sensitive information with pixelation effect
- **Freehand, Line, Rectangle, Circle, Text**: All the essential drawing tools

**Structured annotation data export:**

The `OnSave` event now returns detailed annotation data including all shapes, positions, and metadata:

```csharp
new ScreenshotFeedback()
    .IsOpen(isOpen)
    .HandleSave(annotationData =>
    {
        // Access structured annotation data
        foreach (var shape in annotationData.Shapes)
        {
            switch (shape)
            {
                case CalloutAnnotation callout:
                    Console.WriteLine($"Callout #{callout.Number}: {callout.Text}");
                    Console.WriteLine($"  Position: ({callout.Anchor.X}, {callout.Anchor.Y})");
                    break;

                case ArrowAnnotation arrow:
                    Console.WriteLine($"Arrow from ({arrow.Start.X}, {arrow.Start.Y}) to ({arrow.End.X}, {arrow.End.Y})");
                    break;

                case CensorAnnotation censor:
                    Console.WriteLine($"Censored area: {censor.Start.X},{censor.Start.Y} to {censor.End.X},{censor.End.Y}");
                    break;
            }
        }

        // Screenshot dimensions available
        Console.WriteLine($"Screenshot size: {annotationData.ScreenshotWidth}x{annotationData.ScreenshotHeight}");
    })
```

**Convenient overloads:**

If you don't need the annotation data, use the simple callback:

```csharp
new ScreenshotFeedback()
    .IsOpen(isOpen)
    .HandleSave(() => Console.WriteLine("Screenshot saved!"))
```

**Keyboard shortcuts:**

- **Ctrl+S** (or Cmd+S): Submit annotated screenshot
- **Ctrl+Z** (or Cmd+Z): Undo last annotation
- **ESC**: Cancel (closes the widget)

**UI improvements:**

The widget now features an Ivy dark green theme with drop shadows, SVG toolbar icons, and CSS tooltips for a polished user experience. Callout numbers automatically increment, and all annotations support customizable colors and line widths.

### Scroll Support for Layout and StackLayout

Layouts now support scrollable content! Add scroll behavior to any [`Layout`](https://docs.ivy.app/onboarding/concepts/layout) or `StackLayout` with height or width constraints using the new `.Scroll()` method.

**Vertical scrolling:**

```csharp
Layout.Vertical()
    .Height(Size.Units(30))
    .Scroll(Scroll.Vertical)
    .Gap(2)
    | new Badge("Item 1") | new Badge("Item 2") | new Badge("Item 3")
    | new Badge("Item 4") | new Badge("Item 5") | new Badge("Item 6")
    | new Badge("Item 7") | new Badge("Item 8") | new Badge("Item 9")
    | new Badge("Item 10") | new Badge("Item 11") | new Badge("Item 12")
```

**Using with StackLayout:**

```csharp
new StackLayout([
    new Badge("Item 1"), new Badge("Item 2"), new Badge("Item 3"),
    new Badge("Item 4"), new Badge("Item 5"), new Badge("Item 6"),
    new Badge("Item 7"), new Badge("Item 8"), new Badge("Item 9"),
    new Badge("Item 10"), new Badge("Item 11"), new Badge("Item 12")
], gap: 2) { Scroll = Scroll.Vertical }
    .Height(Size.Units(30))
    .Width(Size.Full())
```

**Scroll modes:**

- `Scroll.Vertical` - Scrolls vertically, hides horizontal overflow
- `Scroll.Horizontal` - Scrolls horizontally, hides vertical overflow
- `Scroll.Both` - Scrolls in both directions
- `Scroll.Auto` - Browser determines scroll behavior
- `Scroll.None` - No scrolling (default)

### Independent Row and Column Gap Control

Fine-tune spacing in your layouts with independent control over vertical (row) and horizontal (column) gaps. The `.Gap()` method now accepts two parameters for precise spacing control.

```csharp
// Gap applies to both directions
Layout.Wrap().Gap(4)
    | new Badge("A") | new Badge("B") | new Badge("C")

// Independent row and column gap
Layout.Wrap().Gap(rowGap: 8, columnGap: 2)
    .Width(Size.Units(60))
    | new Badge("A") | new Badge("B") | new Badge("C")
    | new Badge("D") | new Badge("E") | new Badge("F")
    | new Badge("G") | new Badge("H") | new Badge("I")
```

**With StackLayout:**

```csharp
new StackLayout([
    new Badge("A"), new Badge("B"), new Badge("C"),
    new Badge("D"), new Badge("E"), new Badge("F"),
    new Badge("G"), new Badge("H")
], Orientation.Horizontal, wrap: true)
{
    RowGap = 8,      // Vertical spacing
    ColumnGap = 2    // Horizontal spacing
}.Width(Size.Units(60))
```

This is especially useful in wrapped layouts where you want tighter horizontal spacing between items but more vertical breathing room between rows.

### Space Distribution Alignment

Distribute space between elements in your layouts using `SpaceBetween`, `SpaceAround`, or `SpaceEvenly` alignment options. These work with both `Layout` methods and `StackLayout` widgets.

**Using with Layout methods:**

```csharp
Layout.Vertical().Gap(4)
    | Text.Label("SpaceBetween — items pushed to edges:")
    | (Layout.Horizontal().Align(Align.SpaceBetween).Width(Size.Full())
        | new Badge("A") | new Badge("B") | new Badge("C"))
    | Text.Label("SpaceAround — equal space around each item:")
    | (Layout.Horizontal().Align(Align.SpaceAround).Width(Size.Full())
        | new Badge("A") | new Badge("B") | new Badge("C"))
    | Text.Label("SpaceEvenly — equal space between all items:")
    | (Layout.Horizontal().Align(Align.SpaceEvenly).Width(Size.Full())
        | new Badge("A") | new Badge("B") | new Badge("C"))
```

**Using with StackLayout:**

```csharp
new StackLayout([
    Text.Label("SpaceBetween:"),
    new StackLayout([new Badge("A"), new Badge("B"), new Badge("C")],
        Orientation.Horizontal, align: Align.SpaceBetween),
    Text.Label("SpaceAround:"),
    new StackLayout([new Badge("A"), new Badge("B"), new Badge("C")],
        Orientation.Horizontal, align: Align.SpaceAround),
    Text.Label("SpaceEvenly:"),
    new StackLayout([new Badge("A"), new Badge("B"), new Badge("C")],
        Orientation.Horizontal, align: Align.SpaceEvenly)
], gap: 4).Width(Size.Full())
```

### AspectRatio Support for All Widgets

You can now maintain proportional dimensions on any widget using the new `AspectRatio` property. This is particularly useful for creating responsive layouts that preserve aspect ratios regardless of screen size.

Use the `.AspectRatio()` extension method with any widget:

```csharp
// Common aspect ratios
new Box(Text.Block("16:9 Video"))
    .Width(Size.Units(80))
    .AspectRatio(16f / 9f)
    .Background(Colors.Primary);

new Box(Text.Block("Square"))
    .Width(Size.Units(40))
    .AspectRatio(1f)
    .Background(Colors.Warning);

// Works with any widget inheriting from WidgetBase
new Card()
    .Width(Size.Units(100))
    .AspectRatio(4f / 3f);
```

The `AspectRatio` property is available on `WidgetBase`, so it works with all widgets including `Box`, `Card`, `StackLayout`, and more.

### SelectInput Radio and Slider Variants

The [`SelectInput`](https://docs.ivy.app/widgets/inputs/select-input) widget now supports two powerful new variants: **Radio** for traditional radio button selection and **Slider** for selecting from ordered discrete options.

**Radio variant for single-select forms:**

The Radio variant renders familiar radio buttons, perfect for settings, configuration UIs, and forms where all options should be visible:

```csharp
var frequency = UseState("Daily");

return frequency.ToSelectInput(["Immediately", "Daily", "Weekly", "Never"])
    .Radio()
    .WithField()
    .Label("Notification frequency")
    .Width(Size.Full());
```

**Slider variant for ordered options:**

The Slider variant is ideal for selecting from an ordered list like sizes, priority levels, or quality settings. It renders a range slider that snaps to each option:

```csharp
enum Priority { Low, Medium, High, Critical }

var size = UseState("M");
var priority = UseState(Priority.Medium);

return Layout.Vertical()
    | size.ToSelectInput(new[] { "XS", "S", "M", "L", "XL", "XXL" }.ToOptions())
        .Slider()
        .WithField()
        .Label("T-Shirt Size")
    | priority.ToSelectInput()
        .Slider()
        .WithField()
        .Label("Priority");
```

The Slider variant shows option labels in a tooltip as you slide, displays dot indicators for each value, and provides intuitive feedback for ordered discrete selections. Both variants support all standard SelectInput features including validation, disabled state, and density controls.

### SelectInput Option Descriptions

Options in `SelectInput` widgets can now include descriptions that display as caption text below the label. This feature works with Toggle, Radio (List), and Checkbox (List) variants, making it easy to provide helpful context for each option.

```csharp
var genreOptions = new IAnyOption[]
{
    new Option<string>("Documentary", "Documentary")
        { Description = "Never stop learning." }
};
```

Descriptions appear as smaller, muted text below the option label, providing a clean way to add explanatory text without cluttering the interface.

### Configurable Server Listening Interface

You can now configure which network interface your Ivy server listens on using the `--host` CLI argument or `HOST` environment variable. This is particularly useful when deploying to containerized or hosted environments.

**Using the CLI argument:**

```bash
ivy run --host 0.0.0.0 --port 8080
```

**Using environment variable:**

```bash
export HOST=0.0.0.0
ivy run
```

**Default behavior:**

- Local development: Binds to `localhost` by default
- Docker containers: Automatically binds to `*` (all interfaces)
- When `PORT` environment variable is set: Automatically binds to `*` (all interfaces)

The `--host` CLI argument takes precedence over the `HOST` environment variable, following standard CLI conventions.

### New Script Widget

Load external JavaScript files or execute inline JavaScript code with the new `Script` widget! This widget doesn't render any visible output—it simply injects `<script>` elements into the page head, making it perfect for adding analytics, third-party libraries, or custom JavaScript behavior to your apps.

**Script examples:**

```csharp
// Load an external JavaScript library
new Script("https://cdn.jsdelivr.net/npm/canvas-confetti@1.9.3/dist/confetti.browser.min.js");

// Execute inline JavaScript
new Script().InlineCode("console.log('Hello from Ivy!');");

// Load asynchronously (doesn't block page rendering)
new Script("https://example.com/analytics.js").Async();

// Defer execution until page is parsed
new Script("https://example.com/widget.js").Defer();

// Add Subresource Integrity + CORS
new Script("https://cdn.example.com/lib.js")
    .Integrity("sha384-oqVuAfXRKap7fdgcCY5uykM6+R9GqQ8K/uxy9rx7HNQlGYl1kPzQho1wx4JwY8w")
    .CrossOrigin("anonymous");
```

**Security considerations:**

- **Never pass user-supplied input** to `Src` or `InlineCode`—this widget executes arbitrary JavaScript
- Use `Integrity` for external scripts to prevent tampering
- Set `CrossOrigin` when using integrity checks

The Script widget is ideal for integrating analytics platforms, loading visualization libraries, adding custom tracking code, or injecting third-party widgets into your Ivy apps.

### TextBlock Anchor Navigation

Headings in `TextBlock` (H1-H6) now support anchor IDs for URL hash navigation! Add explicit anchor IDs or auto-generate them from heading text to enable deep linking and table-of-contents style navigation.

```csharp
//Auto-generate anchor from heading text:
Text.H2("Getting Started").Anchor() // Creates anchor: #getting-started

//Set explicit anchor ID:
Text.H2("API Reference").Anchor("api-docs") // Creates anchor: #api-docs
```

Anchors render as HTML `id` attributes on heading elements, allowing users to link directly to specific sections with URLs like `yourapp.com#getting-started`.

### VideoPlayer Volume Control

The `VideoPlayer` widget now supports fine-grained volume control! Set the playback volume from 0.0 (muted) to 1.0 (full volume) for precise audio level management.

```csharp
new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
    .Volume(0.5f)
```

This is especially useful for background videos, ambient soundscapes, tutorials with voiceover, or any scenario where you want to control the default audio level programmatically.

### Json and Xml Widget Enhancements

The `Json` and `Xml` widgets now support expansion control and simplified usage, making it easier to work with structured data visualization.

**Control initial tree expansion:**

Both widgets now have an `Expanded` property to control how deeply the tree is initially expanded:

```csharp
var data = new { name = "John", address = new { city = "NYC", zip = "10001" } };

// Collapsed (default)
new Json(data)

// Expand first level
new Json(data) { Expanded = 1 }

// Fully expanded
new Json(data) { Expanded = -1 }
```

**Direct object serialization for Json:**

The `Json` widget now accepts any object directly—no need to manually serialize:

```csharp
// Before: manual serialization required
var json = JsonSerializer.Serialize(myData);
new Json(json)

// Now: pass objects directly
new Json(myData)
```

The object is automatically serialized with proper indentation. You can still pass JSON strings directly for full control over formatting.

### Small Icon Buttons

Icon-only buttons now support the `.Small()` density option for a more compact appearance. When combined with icon buttons, this creates a smaller 6x6 unit button (compared to the default 9x9 units).

```csharp
// Small icon buttons
return Icons.Heart.ToButton(eventHandler, ButtonVariant.Destructive).Small();
```

This is particularly useful in toolbars, action panels, and other UI areas where space is at a premium.

### Ivy.Analyser: Detect Inline Hook Calls

A new analyzer rule **IVYHOOK007** helps you write cleaner, more maintainable code by detecting hooks that are called inline within expressions. While inline hooks may work at runtime, they obscure state management and make debugging harder.

**What triggers the warning:**

```csharp
// Hooks called inline in a pipe chain
return new Card(
    Layout.Vertical().Gap(3)
        | UseState(true).ToBoolInput().Label("Email notifications")
        | UseState(false).ToBoolInput().Label("SMS notifications")
);

// Hook inline in return statement
return UseState(true).ToBoolInput();

// Hook inline as constructor argument
return new Card(UseState(0).Value);
```

**How to fix it:**

Extract each hook call to a local variable at the top of your `Build()` method:

```csharp
// Hooks assigned to variables first
var emailNotifications = UseState(true);
var smsNotifications = UseState(false);

return new Card(
    Layout.Vertical().Gap(3)
        | emailNotifications.ToBoolInput().Label("Email notifications")
        | smsNotifications.ToBoolInput().Label("SMS notifications")
);
```

This pattern makes your state management explicit and easier to track, especially when building complex UIs with multiple hooks.

## Breaking Changes

### Visible Property Removed from Widgets

The `Visible` property and related methods have been removed from `WidgetBase`. This simplifies the widget API and encourages the use of conditional rendering, which is more idiomatic in Ivy.

**Removed APIs:**

- `widget.Visible` property
- `.Visible(bool)` extension method
- `.Show()` extension method
- `.Hide()` extension method

**Migration:**

Instead of using the `Visible` property to conditionally show/hide widgets, use C#'s conditional operators (`?:` or `??`) to conditionally render widgets:

```csharp
// Before (no longer works)
new Button("Click me")
    .Visible(isActive)

new Card()
    .Visible(showCard)

// After (use conditional rendering)
isActive ? new Button("Click me") : null

showCard ? new Card() : null
```

For more complex conditional layouts, you can use C#'s pattern matching or conditional expressions:

```csharp
// Conditional layout children
Layout.Vertical()
    | Text.H1("Title")
    | (hasContent ? contentWidget : null)
    | (showFooter ? footerWidget : null)

// Ternary for different widgets
condition
    ? new Button("Option A")
    : new Button("Option B")
```

This pattern is more explicit about what gets rendered and aligns with modern UI framework patterns like React's conditional rendering.

### Input Widget Constructors Made Internal

All Input widget constructors have been made `internal`, requiring you to use the `.To[Type]Input()` extension methods instead of direct instantiation. This enforces a more consistent API pattern across the framework.

**Affected widgets:**

- `TextInput`, `CodeInput`, `ColorInput`
- `NumberInput`, `BoolInput`, `SelectInput`
- `DateTimeInput`, `DateRangeInput`
- `FileInput`, `FeedbackInput`, `IconInput`
- `ReadOnlyInput`

**Migration:**

Direct instantiation with `new` will now result in compilation errors. You must use the extension methods:

```csharp
// Before (no longer works)
var textInput = new TextInput(state);
var selectInput = new SelectInput<string>(state, options);
var numberInput = new NumberInput();

// After (use extension methods)
var textInput = state.ToTextInput();
var selectInput = state.ToSelectInput(options);
// For stateless inputs, create a state first:
var numberState = UseState(0);
var numberInput = numberState.ToNumberInput();
```

**With values and callbacks:**

```csharp
// Before (no longer works)
var input = new TextInput("initial", onChange, placeholder: "Enter text");
var select = new SelectInput<string>("value", onSelectChange, options);

// After (use extension methods)
var input = "initial".ToTextInput(onChange, placeholder: "Enter text");
var select = "value".ToSelectInput(onSelectChange, options);
```

**Extension methods work on:**

- States: `state.ToTextInput()`
- Values: `"text".ToTextInput(onChange)`
- Primitives with state creation: Create a state first, then call the extension method

This change ensures all Input widgets are created consistently through extension methods, making the API more predictable and easier to learn.

### Widget Event Handlers Renamed from Handle*to On*

Event handler extension methods in external widget packages have been renamed from `Handle*` to `On*` to match the rest of the framework. This affects the Iframe, Leaflet, ScreenshotFeedback, TiptapInput, and Xterm widgets.

**API mapping (no behavior change):**

- **Iframe**: `HandleMessageReceived` to `OnMessageReceived`
- **Leaflet Map**: `HandleMapClick` to `OnMapClick`, `HandleMarkerClick` to `OnMarkerClick`, `HandleMarkerDrag` to `OnMarkerDrag`, `HandleZoomChange` to `OnZoomChange`, `HandleCenterChange` to `OnCenterChange`, `HandleBoundsChange` to `OnBoundsChange`
- **ScreenshotFeedback**: `HandleSave` to `OnSave`, `HandleCancel` to `OnCancel`
- **TiptapInput**: `HandleFocus` to `OnFocus`, `HandleBlur` to `OnBlur`
- **Xterm Terminal**: `HandleInput` to `OnInput`, `HandleResize` to `OnResize`, `HandleLinkClick` to `OnLinkClick`

**Migration pattern (before/after):**

```csharp
// Iframe
new Iframe()
    .HandleMessageReceived(e => ProcessMessage(e.Value));  // before
new Iframe()
    .OnMessageReceived(e => ProcessMessage(e.Value));      // after

// Leaflet Map
new Map()
    .HandleMapClick(latLng => Console.WriteLine($"Clicked: {latLng}"))
    .HandleMarkerClick(markerId => Console.WriteLine($"Marker: {markerId}"))
    .HandleZoomChange(zoom => Console.WriteLine($"Zoom: {zoom}"));   // before
new Map()
    .OnMapClick(latLng => Console.WriteLine($"Clicked: {latLng}"))
    .OnMarkerClick(markerId => Console.WriteLine($"Marker: {markerId}"))
    .OnZoomChange(zoom => Console.WriteLine($"Zoom: {zoom}"));       // after

// ScreenshotFeedback
new ScreenshotFeedback()
    .HandleSave(SaveScreenshot)
    .HandleCancel(CloseDialog);   // before
new ScreenshotFeedback()
    .OnSave(SaveScreenshot)
    .OnCancel(CloseDialog);       // after

// Xterm Terminal
new Terminal()
    .HandleInput(pty.HandleInput)
    .HandleResize(pty.HandleResize)
    .HandleLinkClick(OpenLink);   // before
new Terminal()
    .OnInput(pty.HandleInput)
    .OnResize(pty.HandleResize)
    .OnLinkClick(OpenLink);       // after

// TiptapInput
new TiptapInput()
    .HandleFocus(() => Console.WriteLine("Focused"))
    .HandleBlur(() => Console.WriteLine("Blurred"));   // before
new TiptapInput()
    .OnFocus(() => Console.WriteLine("Focused"))
    .OnBlur(() => Console.WriteLine("Blurred"));       // after
```

Functionality is unchanged; this is a naming cleanup. You can safely update existing code by replacing `.HandleX` calls with `.OnX` equivalents.

### CLI: Prefer `ivy ask` over `ivy question`

The CLI documentation now promotes `ivy ask` as the primary command for querying the Ivy knowledge base. While both commands work (they're aliases), `ivy ask` is now the recommended and documented form.

```terminal
# Preferred command
ivy ask "How do I implement a new Application Shell in Ivy?"

# Also works (alias)
ivy question "How do I implement a new Application Shell in Ivy?"
```

This command queries the local context dynamically using integrated Local RAG features specifically tailored to your semantic `ivyVersion`.

## Security Enhancements

### Enhanced File Upload Security with Magic Byte Validation

File uploads are now protected against MIME type spoofing attacks! Ivy now validates that uploaded file content actually matches the declared Content-Type by checking magic bytes (file signatures).

Previously, malicious users could potentially upload dangerous files (like executables or scripts) disguised as images by simply changing the Content-Type header. Now Ivy verifies the actual file content to ensure it matches the claimed type.

If you're using `FileInput` widgets with file type restrictions, this security enhancement is automatically applied. No code changes needed:

```csharp
new FileInput()
    .Accept("image/*")  // Now validates actual file content, not just headers
    .OnChange(files => HandleUpload(files))
```

**Manual validation API:**

For custom file handling scenarios, you can use the new validation methods directly:

```csharp
// Validate a single file with magic byte checking
var result = FileInputValidation.ValidateFileTypeWithMagicBytes(
    file,
    "image/*",
    fileStream
);

if (!result.IsValid)
{
    // File content doesn't match declared type - potential attack
    Console.WriteLine(result.ErrorMessage);
}
```

This enhancement provides defense-in-depth security for your file upload features without any breaking changes.

### Stricter App ID Validation

App ID validation has been strengthened to prevent potential security issues. The framework now blocks additional characters that could be exploited in URL manipulation or path traversal attacks.

App IDs can no longer contain:

- `:` (colon) - URL protocol separator
- `?` (question mark) - URL query parameter separator
- `#` (hash) - URL fragment identifier
- `&` (ampersand) - URL query parameter separator
- `%` (percent) - URL encoding character **[NEW]**
- `\` (backslash) - Path separator on Windows **[NEW]**

If your app creates or validates app IDs, ensure they don't contain these characters. App IDs should use alphanumeric characters, hyphens, and underscores for best compatibility:

## Bug Fixes

- **DataTable**: Height in flex containers no longer grows infinitely; uses `flexGrow: 1` and `minHeight: 0` so tables size correctly in vertical layouts.
- **Breadcrumbs**: Serialization and frontend fixed; click handlers work; icon corrected to `Icons.House`.
- **App ID validation**: `%` and `\` are now disallowed in App IDs.
- **Calendar**: Empty slot content is no longer rendered when no children are provided.
- **Charts**: Area/Bar/Line respect `.Name()` in legends and tooltips; funnel tooltips use formatted values and percentages; bar chart background rectangles disabled by default; legend bottom spacing increased to 50px; chord legend hidden when not configured; radar chart center/radius adjust when legend is enabled.
- **Docker**: Server binds to `*` when `DOTNET_RUNNING_IN_CONTAINER` is set, fixing health probes in containers.
- **Card**: Density scaling for header icons and spacing (small/medium/large); content body visibility fixed (flex-auto instead of flex-1).
- **Text (Markdown)**: Density setting is now applied to the markdown variant.
- **External widgets**: React.lazy wrappers cached by type name to avoid unnecessary re-renders.
- **Ivy.Analyser**: IVYHOOK007 no longer flags hook calls that use the null-forgiving operator (`!`).
- **Static files**: `.md` files are served as static files instead of being routed by PathToAppIdMiddleware.
- **FileInput (Default variant)**: Only the upload button opens the file dialog; remove buttons and file items no longer trigger selection; container uses cursor-default.
- **FileInput**: Button scales correctly for `Density.Large`; default button text is "Select file" vs "Select files" based on single vs multiple selection.
- **Sidebar**: Hook names (e.g. UseRefreshToken) no longer split with spaces; "Use" prefix names are shown unchanged.
