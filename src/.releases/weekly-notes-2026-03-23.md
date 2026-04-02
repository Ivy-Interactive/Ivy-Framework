# Ivy Framework Weekly Notes - Week of 2026-03-23

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## New Features

### DateTimeInput Min, Max, and Step Constraints

Added `Min()`, `Max()`, and `Step()` constraint methods to the [**DateTimeInput**](https://docs.ivy.app/widgets/inputs/date-time-input) widget, following the same pattern as [**NumberInput**](https://docs.ivy.app/widgets/inputs/number-input). You can now restrict date/time selections to specific ranges and enforce step intervals for time-based inputs.

```csharp
// Restrict date selection to a specific range
var birthDate = new DateTimeInput()
    .Variant(DateTimeInputVariant.Date)
    .Min(DateTime.Parse("1900-01-01"))
    .Max(DateTime.Now)
    .Bind(dateState);

// Enforce time intervals (e.g., 15-minute increments)
var appointmentTime = new DateTimeInput()
    .Variant(DateTimeInputVariant.Time)
    .Step(TimeSpan.FromMinutes(15))
    .Bind(timeState);

// Combine constraints for datetime inputs
var scheduleInput = new DateTimeInput()
    .Variant(DateTimeInputVariant.DateTime)
    .Min(DateTime.Now)
    .Max(DateTime.Now.AddDays(30))
    .Step(TimeSpan.FromMinutes(30))
    .Bind(scheduleState);
```

The constraints are enforced both in the calendar/picker UI and in manual input, ensuring users can only select valid values. The `Step` constraint is particularly useful for time inputs to restrict selections to specific intervals (e.g., 15-minute, 30-minute, or hourly slots).

### Box Widget Opacity Methods

Added `Opacity()` and `BorderOpacity()` fluent extension methods to the [**Box**](https://docs.ivy.app/widgets/primitives/box) widget, allowing you to set opacity independently without needing to also specify a color.

```csharp
// Set background opacity and border opacity independently
var box = new Box()
    .Background(Colors.Green)
    .Opacity(0.8f)
    .BorderColor(Colors.Black)
    .BorderOpacity(0.6f);
```

Previously, you had to use `Background(color, opacity)` or `BorderColor(color, opacity)` overloads which required specifying the color and opacity together.

### Card and Box Shadow Hover Effect

Added a new `CardHoverVariant.Shadow` hover variant for [**Card**](https://docs.ivy.app/widgets/common/card) and [**Box**](https://docs.ivy.app/widgets/primitives/box) widgets, providing a subtle shadow elevation effect on hover. This creates a Material Design-inspired lifting animation without position translation.

```csharp
// Apply shadow hover effect to a Card
var card = new Card(content)
    .HoverVariant(CardHoverVariant.Shadow);

// Also works with Box widgets
var box = new Box(content)
    .HoverVariant(BoxHoverVariant.Shadow);
```

The shadow hover variant applies `hover:shadow-lg` on hover and `active:shadow-md` on click, creating a smooth elevation transition. This is useful for clickable cards that need visual feedback but shouldn't move on hover (unlike `CardHoverVariant.PointerAndTranslate`).

**Available hover variants:**

- `None` - No hover effect
- `Pointer` - Cursor changes to pointer only
- `PointerAndTranslate` - Cursor changes to pointer with subtle position shift
- `Shadow` - Cursor changes to pointer with shadow elevation effect

### Button Keyboard Shortcuts

Added `ShortcutKey()` method to the [**Button**](https://docs.ivy.app/widgets/common/button) widget, allowing you to associate keyboard shortcuts with button actions. The shortcut listener is registered globally on the window, so buttons don't need to be focused to trigger their actions.

```csharp
var button = new Button("Save", _ => client.Toast("Saved!"))
    .Secondary()
    .ShortcutKey("Ctrl+S")
```

This is particularly useful for creating keyboard-driven interfaces and improving productivity for power users who prefer keyboard navigation. The shortcuts work anywhere in your app, regardless of focus state.

### Open Graph and Twitter Card Meta Tags

Ivy now automatically adds Open Graph and Twitter Card meta tags to your application's HTML for rich social media previews when your app is shared on platforms like Twitter, LinkedIn, Slack, Discord, and more. The new `OpenGraphFilter` is automatically included in the HTML pipeline and intelligently derives metadata from your [server configuration](https://docs.ivy.app/onboarding/concepts/program) (`ServerArgs.Metadata`, `ServerMetadata`).

**Automatic features:**

- **Auto-generated social images**: When you set a GitHub URL, Ivy automatically generates an `og:image` URL using the pattern `https://banner.ivy.app/ogImage?text={Title}&repo={owner/repo}`
- **Smart site name**: The `og:site_name` is automatically derived from your entry assembly name if not explicitly set
- **Twitter Card support**: Automatically includes `twitter:*` meta tags for proper Twitter/X card rendering
- **Sensible defaults**: `og:type` defaults to "website", `og:locale` to "en_US", and `twitter:card` to "summary_large_image"

```csharp
// Basic setup - uses automatic image generation and site name
var server = new Server(new ServerArgs
{
    Metadata = new ServerMetadata
    {
        Title = "My Ivy App",
        Description = "A powerful web application",
        GitHubUrl = "https://github.com/user/my-ivy-app"
    }
});
// Automatically generates:
// - og:image: https://banner.ivy.app/ogImage?text=My+Ivy+App&repo=user/my-ivy-app
// - og:site_name: (derived from assembly name)

// Advanced: Override auto-generated values
var server = new Server(new ServerArgs
{
    Metadata = new ServerMetadata
    {
        Title = "My Ivy App",
        Description = "A powerful web application",
        GitHubUrl = "https://github.com/user/my-ivy-app",
        OgImage = "https://example.com/custom-image.png",  // Custom image
        OgSiteName = "My Company",                         // Custom site name
        OgType = "article",                                // Change type
        TwitterCard = "summary"                            // Change card type
    }
});
```

**Available ServerMetadata properties:**

| Property | Default | Description |
|----------|---------|-------------|
| `Title` | `null` | Page title (used for `og:title` and `twitter:title`) |
| `Description` | `null` | Page description (used for `og:description` and `twitter:description`) |
| `GitHubUrl` | `null` | GitHub repository URL (used to auto-generate `og:image`) |
| `OgImage` | Auto-generated from `GitHubUrl` + `Title` | Open Graph image URL |
| `OgSiteName` | Auto-derived from assembly name | Site name for `og:site_name` |
| `OgType` | `"website"` | Open Graph type |
| `OgLocale` | `"en_US"` | Open Graph locale |
| `TwitterCard` | `"summary_large_image"` | Twitter card type |

The generated meta tags include proper image dimensions (`og:image:width: 1200`, `og:image:height: 630`) and alt text for accessibility. This ensures your Ivy applications look great when shared on social media platforms without any additional configuration.

### Server Metadata: GitHub URL Support

Added `SetMetaGitHubUrl()` method to the [Server API](https://docs.ivy.app/onboarding/concepts/program), allowing you to add GitHub repository metadata to your application's HTML for SEO and social sharing purposes.

```csharp
var server = new Server(new ServerArgs
{
    Metadata = new ServerMetadata
    {
        Title = "My Ivy App",
        Description = "A powerful web application",
        GitHubUrl = "https://github.com/user/my-ivy-app"
    }
});

// Or set it using the fluent API
server
    .SetMetaTitle("My Ivy App")
    .SetMetaDescription("A cool app")
    .SetMetaGitHubUrl("https://github.com/user/my-ivy-app");
```

The GitHub URL is primarily used for:

- **Auto-generating Open Graph images** for social media previews (using the pattern `https://banner.ivy.app/ogImage?text={Title}&repo={owner/repo}`)
- **SEO meta tags** added to your HTML
- **Social sharing** with proper repository attribution

The "Made with Ivy" badge in the bottom-right corner has been simplified to always show the "MADE WITH" Ivy logo and links to the Ivy Framework repository.

### Badge Color Property

Added `Color` property to the [**Badge**](https://docs.ivy.app/widgets/common/badge) widget, allowing you to customize badge colors using the full `Colors` enum. This provides direct color control matching the pattern used by `Icon`, `TextBlock`, and `Progress` widgets.

```csharp
// Use the fluent Color() method
var badge = new Badge("Warning", BadgeVariant.Outline)
    .Color(Colors.Yellow);
```

The Color property works alongside the existing `BadgeVariant` options, giving you full control over badge appearance. When set, the color overrides the default variant styling with your custom color choice.

### UseMemo Auto-Unwraps IState Dependencies

[**UseMemo**](https://docs.ivy.app/hooks/core/use-memo) now automatically unwraps `IState` dependencies, allowing you to pass state objects directly instead of manually extracting `.Value`. Both approaches work identically:

```csharp
var count = UseState(0);

// Both approaches work - auto-unwrapped internally
var doubled = UseMemo(() => count.Value * 2, count.Value);  // Explicit .Value
var doubled = UseMemo(() => count.Value * 2, count);        // Auto-unwrapped

// This is particularly useful with UseCallback
var handleClick = UseCallback(() => ProcessData(count.Value), count); // count auto-unwrapped
```

The factory function always needs `.Value` to read the current value, but the dependency array now accepts `IState` objects directly. This makes code cleaner while maintaining the same memoization behavior.

**Recommendation:** For clarity, prefer passing `.Value` explicitly in the dependency array, but know that passing the `IState` object also works.

### TryUseService<T> for Optional Service Lookups

The [**UseService**](https://docs.ivy.app/hooks/core/use-service) hook now includes **`TryUseService<T>()`** for scenarios where a service might not be registered. This method returns a boolean indicating success and uses proper null-safety attributes.

```csharp
// Old approach (no longer works for optional services)
var service = UseService<IMyService>(); // throws if not registered

// New approach for optional services
if (TryUseService<IMyService>(out var service))
{
    // Service is available, use it
    service.DoSomething();
}
else
{
    // Service not registered, handle gracefully
    Console.WriteLine("Optional service not available");
}
```

### Form-Level LabelPosition API

Added the ability to set a default `LabelPosition` for all fields in a form via [**FormBuilder**](https://docs.ivy.app/onboarding/concepts/forms) `.LabelPosition()`, with support for per-field overrides. This follows the same pattern as the existing `Density` property, making it easy to configure label positioning across your entire form.

```csharp
// Set default label position for all fields in the form
var form = new FormBuilder<UserModel>(userState)
    .LabelPosition(LabelPosition.Left)  // Default for all fields
    .Field(m => m.FirstName)
    .Field(m => m.LastName)
    .LabelPosition(m => m.Description, LabelPosition.Top)  // Override for this field
    .Field(m => m.Description);
```

This is particularly useful for creating consistent form layouts while maintaining flexibility for fields that need different label positioning. The effective label position for each field is determined by the per-field setting first, falling back to the form-level default if not specified.

### Dictionary Support in TableBuilder

The [**Table**](https://docs.ivy.app/widgets/common/table) builder’s `ToTable()` method now supports Dictionary objects as data sources with indexer expressions in the `Header()` method. Previously, using dictionary indexers like `r => r["key"]` would throw an `ArgumentException`. Now you can create tables from dynamic dictionary data:

```csharp
var data = new List<Dictionary<string, string>>
{
    new() { ["Name"] = "Alice", ["Age"] = "30", ["Email"] = "alice@example.com" },
    new() { ["Name"] = "Bob",   ["Age"] = "25", ["Email"] = "bob@example.com" }
};

var table = data.ToTable()
    .Header(r => r["Name"], "Name")
    .Header(r => r["Age"], "Age")
    .Header(r => r["Email"], "Email");
```

**Auto-scaffolding:** If you don't specify any `Header()` calls, the table builder now automatically creates columns from the dictionary keys in the first item:

```csharp
var data = new List<Dictionary<string, string>>
{
    new() { ["Name"] = "Alice", ["Age"] = "30", ["Email"] = "alice@example.com" },
    new() { ["Name"] = "Bob",   ["Age"] = "25", ["Email"] = "bob@example.com" }
};

// No Header() calls needed - columns auto-scaffold from keys
var table = data.ToTable(); // Automatically creates Name, Age, and Email columns
```

This works with `IDictionary<string, object>`, `IDictionary<string, string>`, and generic `IDictionary` types. Auto-scaffolding makes it even easier to work with dynamic data structures, API responses, or scenarios where column names aren't known at compile time. The table builder creates columns dynamically for dictionary keys and handles missing keys gracefully by returning null.

### RichTextMarkdownParser for Streaming AI Chat

Added `RichTextMarkdownParser` for parsing markdown incrementally during LLM streaming, enabling real-time rich text formatting in AI chat interfaces (output is composed with [**RichTextBlock**](https://docs.ivy.app/widgets/primitives/rich-text-block) / `Text.Rich()`). The parser handles all common markdown syntax including bold, italic, code blocks, headings, lists, tables, blockquotes, and more—all while streaming token by token.

```csharp
// Example: Stream AI response with live markdown rendering
var parser = new RichTextMarkdownParser();
var richText = new RichTextBuilder();

// As tokens arrive from your LLM:
await foreach (var token in llmStream)
{
    // Parser incrementally converts markdown to TextRuns
    var runs = parser.Append(token);
    foreach (var run in runs)
    {
        richText.AddRun(run);
    }

    // Update UI in real-time
    StateUpdated();
}

// Flush any remaining content when stream completes
var finalRuns = parser.Flush();
foreach (var run in finalRuns)
{
    richText.AddRun(run);
}
```

The parser maintains state across token boundaries, so you can safely stream character-by-character or word-by-word without worrying about breaking markdown syntax. [**RichTextBlock**](https://docs.ivy.app/widgets/primitives/rich-text-block) now renders all block-level elements (headings, lists, code blocks, tables, etc.) automatically.

**Supported Markdown Features:**

- Inline: `**bold**`, `*italic*`, `` `code` ``, `~~strikethrough~~`, `[links](url)`
- Block: headings (`#`-`######`), code blocks (` ``` `), bullet lists (`-`, `*`), ordered lists (`1.`), blockquotes (`>`), tables, horizontal rules (`---`)
- Nested lists with indentation support

This is particularly powerful for building chat UIs with AI assistants - your markdown renders progressively as the AI types, creating a natural conversational experience.

### VideoPlayer Subtitle Support

Added subtitle/caption track support to the [**VideoPlayer**](https://docs.ivy.app/widgets/primitives/video-player) widget. You can now serve `.srt` or `.vtt` subtitle files alongside your videos using the new `Subtitles()` fluent API method. The player supports multiple subtitle tracks for different languages, allowing users to choose their preferred language from the video controls.

```csharp
// Multiple subtitle tracks for different languages
var player = new VideoPlayer("https://example.com/video.mp4")
    .Subtitles("https://example.com/subtitles_en.vtt", "English")
    .Subtitles("https://example.com/subtitles_es.vtt", "Spanish");
```

The frontend automatically renders HTML5 `<track>` elements with proper CORS configuration (`crossOrigin="anonymous"`), ensuring subtitles load correctly from external sources. Users can toggle subtitles and switch between languages using the browser's native video controls.

### Kanban Column Header Humanization

[**Kanban**](https://docs.ivy.app/widgets/advanced/kanban) widgets now automatically humanize enum-based column headers for better readability. Column headers like `TaskStatus.InProgress` now display as "In Progress" instead of "InProgress". The feature uses `EnumHelper.GetDescription()`, which respects `[Description]` attributes and falls back to smart PascalCase splitting.

```csharp
public enum TaskStatus
{
    NotStarted,      // Displays as "Not Started"
    InProgress,      // Displays as "In Progress"
    [Description("Waiting for Review")]
    PendingReview,   // Displays as "Waiting for Review"
    Completed        // Displays as "Completed"
}

// Automatic humanization - no code changes needed
var kanban = tasks.ToKanban()
    .GroupBy(t => t.Status);  // Headers are automatically humanized

// Or use the new ColumnHeader() method for custom headers
var kanban = tasks.ToKanban()
    .GroupBy(t => t.Status)
    .ColumnHeader(status => status switch
    {
        TaskStatus.InProgress => "In Progress",
        TaskStatus.Completed => "Done",
        _ => status.GetDescription()
    });
```

The Kanban widget internally maintains separate `Column` (the grouping key for drag-and-drop) and `ColumnName` (the display label) properties, ensuring drag-and-drop functionality works correctly while displaying user-friendly headers.

## Breaking Changes

### Server Metadata Property Reorganization

Server metadata properties have been reorganized into a dedicated `ServerMetadata` record for better structure. The flat properties on `ServerArgs` have been moved into a nested `Metadata` property. See [Program / server setup](https://docs.ivy.app/onboarding/concepts/program).

**Migration:**

```csharp
// Old (no longer works)
var server = new Server(new ServerArgs
{
    MetaTitle = "My App",
    MetaDescription = "A cool app",
    MetaGitHubUrl = "https://github.com/user/repo"
});

// New
var server = new Server(new ServerArgs
{
    Metadata = new ServerMetadata
    {
        Title = "My App",
        Description = "A cool app",
        GitHubUrl = "https://github.com/user/repo"
    }
});

// Or use the fluent API (unchanged)
server
    .SetMetaTitle("My App")
    .SetMetaDescription("A cool app")
    .SetMetaGitHubUrl("https://github.com/user/repo");
```

**Property mappings:**

- `ServerArgs.MetaTitle` to `ServerArgs.Metadata.Title`
- `ServerArgs.MetaDescription` to `ServerArgs.Metadata.Description`
- `ServerArgs.MetaGitHubUrl` to `ServerArgs.Metadata.GitHubUrl`

The fluent methods (`SetMetaTitle()`, `SetMetaDescription()`, `SetMetaGitHubUrl()`) continue to work unchanged, so if you're using those, no migration is needed.

### Chrome Renamed to AppShell

The `Chrome` API has been renamed to [**AppShell**](https://docs.ivy.app/onboarding/concepts/app-shell) throughout the framework for better clarity and to avoid confusion with the Chrome browser. This affects all Chrome-related classes, interfaces, and methods.

**Migration Guide:**

```csharp
// Old (no longer works)
app.UseChrome<DefaultSidebarChrome>();
IChrome chrome = ...;

// New
app.UseAppShell<DefaultSidebarAppShell>();
IAppShell appShell = ...;
```

**What to update in your code:**

- `UseChrome()` to `UseAppShell()`
- `IChrome` to `IAppShell`
- `DefaultSidebarChrome` to `DefaultSidebarAppShell`
- Any custom Chrome classes should be renamed to AppShell
- Documentation and comments referencing "Chrome"

The term "AppShell" better describes the purpose of this component: providing the outer shell/layout for your application (sidebars, headers, navigation, etc.).

### UseService<T> Now Throws on Missing Registrations

The [**UseService**](https://docs.ivy.app/hooks/core/use-service) hook's `UseService<T>()` method now throws an `InvalidOperationException` when a service is not registered in dependency injection, instead of silently returning `null`. This change helps catch configuration errors early rather than causing `NullReferenceException` errors later in your code.

**Migration**: If you have code that relies on `UseService<T>()` returning null for optional services, use the new `TryUseService<T>()` method instead (see above).

### MetricView `useMetricData` Parameter Renamed

The [**MetricView**](https://docs.ivy.app/widgets/common/metric-view) primary constructor parameter `useMetricData` has been renamed to `UseMetricData` (PascalCase) for C# naming consistency.

**Migration:**

```csharp
// Old (named parameter)
new MetricView(
    title: "Revenue",
    icon: Icons.DollarSign,
    useMetricData: ctx => ctx.UseQuery(...))

// New
new MetricView(
    title: "Revenue",
    icon: Icons.DollarSign,
    UseMetricData: ctx => ctx.UseQuery(...))
```

## Developer Experience Improvements

### Query Failures Now Visible in Console

Query fetch failures are now logged at Warning level instead of Debug, making them visible in console output by default. This helps quickly identify issues with queries in [**MetricView**](https://docs.ivy.app/widgets/common/metric-view), [**DataTable**](https://docs.ivy.app/widgets/advanced/data-table), and custom components using [**UseQuery**](https://docs.ivy.app/hooks/core/use-query).

```csharp
// Query failures now appear automatically in your console:
// warn: Ivy.Services.QueryService[0]
//       Fetch failed for query key my-query-key
```

Previously, these errors were only visible when debug logging was enabled. Now you'll see them immediately, making debugging much faster.

### DataTable Query Processing

Improved [**DataTable**](https://docs.ivy.app/widgets/advanced/data-table) query performance by caching reflection calls in the QueryProcessor. Previously, each query execution performed ~8 expensive `GetMethods()` scans on the `Queryable` type to find LINQ methods. These reflection calls are now cached using static fields and a concurrent dictionary, resulting in faster query execution especially when processing multiple DataTable queries.

### FieldWidget Label Associations

Enhanced accessibility in the `FieldWidget` component (used with [forms](https://docs.ivy.app/onboarding/concepts/forms)) by adding proper `htmlFor` attribute associations between labels and input fields. The component now automatically detects input, select, and textarea elements within its children and establishes the correct label-to-input relationship. This improvement benefits users with screen readers and assistive technologies, making forms more navigable and improving overall accessibility compliance.

### Sheet Widget Screen Reader Support

Improved accessibility in the [**Sheet**](https://docs.ivy.app/widgets/advanced/sheet) widget by ensuring a DialogTitle is always present for screen readers, even when no visible title is provided. Previously, the SheetHeader and SheetTitle were only rendered if explicitly set, which could cause accessibility issues. Now the widget always renders these elements, using visually hidden (`sr-only`) styling when no title is provided, with a default "Sheet" label for screen reader users. This ensures proper ARIA dialog semantics and better screen reader navigation without affecting visual presentation.

### Chart Widgets Playwright Readiness

All [chart widgets](https://docs.ivy.app/widgets/charts/bar-chart) now set a `data-chart-rendered="true"` attribute on their container when the chart finishes rendering. This allows Playwright and other E2E testing tools to wait for charts to fully render before taking screenshots or making assertions, preventing empty gray areas caused by canvas render timing issues.

```csharp
// In your Playwright tests, wait for charts to render before screenshotting
await page.WaitForSelectorAsync("[data-chart-rendered='true']");
await page.ScreenshotAsync(new() { Path = "dashboard.png" });
```

This affects all 10 chart widgets (`AreaChart`, `BarChart`, `ChordChart`, `FunnelChart`, `GaugeChart`, `LineChart`, `PieChart`, `RadarChart`, `SankeyChart`, `ScatterChart`—see [charts](https://docs.ivy.app/widgets/charts/bar-chart)). The attribute is set automatically when charts complete their initial render via the ECharts `onChartReady` callback, ensuring reliable test automation without manual delays or retry logic.

## Bug Fixes

- **SplitPascalCase**: `StringHelper.SplitPascalCase()` handles acronyms followed by words (e.g. `UIDesign` to "UI Design", `APIClient` to "API Client") via a lookahead in the regex; improves Kanban headers, enum descriptions, and other humanized PascalCase text.
- **Multiselect**: Dropdown no longer shifts when opened with an overflow badge visible; scroll position resets on focus so the menu anchors correctly.
- **SmartSearch**: Search input stays pinned at the top; scroll overflow fixed so large result lists remain usable.
- **DataTable (AI)**: `DataTableBuilder` uses `TryUseService<IChatClient>()`—no crash when `IChatClient` is not registered in DI.
- **AppShell**: `DefaultSidebarAppShell` (`UseAppShell()`) uses `TryUseService<IAuthService>()`—sidebar renders when auth is not configured.
- **Input widgets**: Value properties use `AlwaysSerialize = true` so `.Set()` with defaults (`0`, `false`, `""`, `null`) updates the client (serializer no longer omits default-valued properties).
- **UseQuery**: `Revalidate()` cancels in-flight fetches, clears errors, and shows loading—avoids surfacing `OperationCanceledException` and stale error UI.
- **AGENTS.md**: `[App]` docs corrected (`path:` to `group:`) to match the attribute and avoid CS0655.
- **object[] / Build()**: `object[]` from `Build()` becomes a `Fragment`, not a table from `IEnumerable` handling—fixes Sheet overlays and multi-root layouts.
- **DataTable / positional records**: `ToDataTable()` with positional records without a parameterless ctor uses the primary ctor with defaults—no `ArgumentNullException` in field removal.
- **DataTable / EF Core**: Count and data queries for the same `DbContext` are serialized with a semaphore (thread-safe with gRPC); empty state renders on the client without an extra `.Count()`.
- **UseTrigger**: Nullable trigger values can be `null` meaningfully—a `hasTriggered` ref replaces the old null guard that blocked `showTrigger(null)`.
- **Charts (Where + Sum)**: Series labels derive from `.Where()` filter values with title casing instead of repeating a generic measure name for `.Where().Sum()` chains.
- **useAutoScroll**: Observes the scroll container and content wrapper so streaming markdown/AI content keeps the view pinned to the bottom.
- **Button shortcuts**: Ignored while focus is in inputs/contentEditable; URL/download/mailto buttons support shortcuts and respect link `target`.
- **External widget discovery**: Exact assembly name comparison replaces substring matching—e.g. `PDF.Viewer.dll` is no longer skipped when the host is `PDF.Viewer.Samples.dll`.

## What's Changed

* feat: document 'ivy cli explain' as preferred for command discovery by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2715>
- (docs) - Update documentation for IConnection and IHaveSecrets by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2618>
- feat: treat analyzer warnings as errors by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2716>
- FeedbackInput: AllowHalf ratings and configurable Max value by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2657>
- Refactor sample apps: rename DefaultContentBuilderApp and remove ConnectionsApp by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2719>
- (auth): brokered auth sessions by @zachwolfe in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2296>
- chore: migrate frontend to Vite+ by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2722>
- Revert "chore: migrate frontend to Vite+" by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2723>
- Feat/migrate vite plus by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2724>
- Optimize Ivy.Docs generation performance by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2725>
- Revert "Optimize Ivy.Docs generation performance" by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2727>
- Fix docs generation perf by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2728>
- docs: update external widget docs by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2729>
- [Multiselect]: fix opened dropdown shift when overflow badge is visible by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2733>
- [SmartSearch]: fix result list scroll overflow and pin search input by @defymecobra in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2735>
- Cache reflection lookups on hot paths by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2736>
- Add label associations for accessibility by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2737>
- Improve service registration safety by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2738>
- Add widget features and improve code quality by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2742>
- Update documentation by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2743>
- Fix runtime stability issues by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2739>
- Refactor widget rendering pipeline by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2740>
- Improve MadeWithIvy component and add metadata API by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2741>
- patch notes week of Mar 20 by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2718>
- Fix ToDataTable and ToTable data conversion issues by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2744>
- Add RichTextMarkdownParser and RichText tests by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2745>
- Fix and improve frontend widgets by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2746>
- Add VideoPlayer captions, docs, and misc fixes by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2747>
- chore: add log files to .gitignore by @artem-ivy-ai in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2750>
- Refactor: Rename Chrome to AppShell by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2756>
- Add Min, Max, and Step Constraints to DateTimeInput by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2674>
- fix: add visually hidden DialogTitle to Sheet for accessibility #2755 by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2757>
- Add ShortcutKey Support to Button by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2672>
- fix: resolve button keyboard shortcut issues and enable for URLs #2757 by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2758>
- feat(docs): provide frontmatter hidden option by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2753>
- Add form-level LabelPosition API to FormBuilder by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2759>
- Fix Radix accessibility warnings on dialogs by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2760>
- Add data-chart-rendered attribute to chart widgets by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2761>
- Add Open Graph and Twitter Card meta tags by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2763>
- Auto-scaffold table columns and fix infrastructure bugs by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2764>
- Add new widget API features (Badge Color, Card hover shadow) by @nielsbosma in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2762>
- fix: resolve NU5104 by upgrading Microsoft.Extensions.AI to 10.4.1 stable by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2765>

## New Contributors

* @artem-ivy-ai made their first contribution in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/2750>

**Full Changelog**: <https://github.com/Ivy-Interactive/Ivy-Framework/compare/v1.2.23...v1.2.25>
