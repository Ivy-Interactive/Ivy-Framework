# Ivy Framework Weekly Notes - Week of 2026-01-16

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## UseQuery Hook - Modern Data Fetching

Ivy now includes **UseQuery** - a powerful data fetching and caching system inspired by React Query and SWR. This hook provides automatic caching, background revalidation, and loading state management for async data operations.

**Key Features:**

- Automatic request deduplication and caching
- Background revalidation on mount, focus, and interval
- Loading, error, and previous data states
- Tag-based cache invalidation
- Polling support with `RefreshInterval`
- Pagination support with `KeepPreviousData`
- Multiple cache scopes: Global, App, User, Client, Device

**Basic Usage:**

```csharp
public override object? Build()
{
    var productsQuery = UseQuery(
        key: "products",
        queryFn: async () => await Database.Products.ToListAsync()
    );

    if (productsQuery.Loading)
        return new Skeleton();

    if (productsQuery.Error != null)
        return new Error("Failed", productsQuery.Error.Message);

    return Layout.Vertical()
        | productsQuery.Data.Select(p => new Text(p.Name));
}
```

**With Options:**

```csharp
var query = UseQuery(
    key: new { Category = "electronics", Page = currentPage },
    queryFn: async () => await FetchProducts(currentPage),
    tags: ["products"],
    options: new QueryOptions
    {
        RefreshInterval = TimeSpan.FromSeconds(30), // Poll every 30s
        KeepPreviousData = true,                   // For pagination
        RevalidateOnMount = true,                  // Refresh on component mount
        Expiration = TimeSpan.FromMinutes(5)       // Cache for 5 minutes
    }
);

// Access query state
bool isLoading = query.Loading;
bool isValidating = query.Validating;    // Background refresh
bool showingOldData = query.Previous;     // When KeepPreviousData=true
Exception? error = query.Error;
List<Product>? data = query.Data;
```

**Cache Invalidation:**

```csharp
// Invalidate by tag
var queryService = GetService<IQueryService>();
queryService.Invalidate(tags: ["products"]);

// Invalidate specific keys
queryService.Invalidate(keys: [new { Category = "electronics" }]);

// Invalidate with predicate
queryService.Invalidate(predicate: key =>
    key is string s && s.StartsWith("products-"));

// Revalidate (refetch) instead of just invalidating
queryService.Revalidate(tags: ["products"]);
```

**Pagination Example:**

```csharp
var page = UseState(1);

var productsQuery = UseQuery(
    key: new { Page = page.Value },
    queryFn: async () => await FetchPage(page.Value),
    options: new QueryOptions { KeepPreviousData = true }
);

return Layout.Vertical()
    | (productsQuery.Previous
        ? new Text("Loading new page...").FontSize(FontSize.Small)
        : null)
    | productsQuery.Data?.Select(p => new Text(p.Name))
    | new Pagination()
        .Page(page.Value)
        .OnPageChange(e => page.Set(e.Value));
```

**Cache Scopes:**

```csharp
// Global - Shared across all users and apps
UseQuery(key, queryFn, scope: QueryScope.Global);

// App - Shared within an app across all users
UseQuery(key, queryFn, scope: QueryScope.App);

// User - Per user across all clients (default)
UseQuery(key, queryFn, scope: QueryScope.User);

// Client - Per browser/client session
UseQuery(key, queryFn, scope: QueryScope.Client);

// Device - Per physical device
UseQuery(key, queryFn, scope: QueryScope.Device);
```

**Integration with Forms:**

```csharp
void OnSave(Event<Button> e)
{
    var queryService = GetService<IQueryService>();

    await Database.Products.AddAsync(newProduct);
    await Database.SaveChangesAsync();

    // Invalidate all product queries
    queryService.Revalidate(tags: ["products"]);
}
```

This hook dramatically simplifies data fetching patterns and eliminates boilerplate for loading states, caching, and synchronization.

## Simplified Hook Syntax

You can now call hooks directly without the `this.` prefix! Hook methods like `UseState`, `UseEffect`, `UseMemo` etc. are now available directly in the `Build()` method scope.

**Before:**

```csharp
public override object? Build()
{
    var count = this.UseState(0);
    var doubled = this.UseMemo(() => count.Value * 2, [count.Value]);

    this.UseEffect(() => {
        Console.WriteLine("Mounted");
    }, EffectTrigger.OnMount);

    return new Text($"Count: {count.Value}");
}
```

**After:**

```csharp
public override object? Build()
{
    var count = UseState(0);
    var doubled = UseMemo(() => count.Value * 2, [count.Value]);

    UseEffect(() => {
        Console.WriteLine("Mounted");
    }, EffectTrigger.OnMount);

    return new Text($"Count: {count.Value}");
}
```

The `this.` prefix is no longer needed, making code cleaner and more similar to React's hook syntax. All hook calls are automatically resolved through the view context.

## New Hooks

### UseReducer

For complex state management with actions and reducers, similar to React's useReducer:

```csharp
public record State(int Count, string Status);
public abstract record Action;
public record Increment(int Amount) : Action;
public record Reset : Action;

public override object? Build()
{
    var (state, dispatch) = UseReducer(
        initialState: new State(0, "idle"),
        reducer: (state, action) => action switch
        {
            Increment inc => state with { Count = state.Count + inc.Amount },
            Reset => new State(0, "reset"),
            _ => state
        }
    );

    return Layout.Vertical()
        | new Text($"Count: {state.Count} ({state.Status})")
        | new Button("Increment by 5")
            .OnClick(_ => dispatch(new Increment(5)))
        | new Button("Reset")
            .OnClick(_ => dispatch(new Reset()));
}
```

### UseMemo

Memoize expensive computations that depend on specific values:

```csharp
public override object? Build()
{
    var searchTerm = UseState("");
    var items = UseState(GetLargeList());

    // Only recompute when searchTerm or items change
    var filteredItems = UseMemo(
        () => items.Value.Where(x => x.Contains(searchTerm.Value)).ToList(),
        [searchTerm.Value, items.Value]
    );

    return Layout.Vertical()
        | new TextInput()
            .Value(searchTerm.Value)
            .OnChange(e => searchTerm.Set(e.Value))
        | filteredItems.Select(item => new Text(item));
}
```

### UseStatic

For values that should only be initialized once and never change:

```csharp
public override object? Build()
{
    // Only runs once on first render, never again
    var expensiveObject = UseStatic(() => new ExpensiveService());
    var randomId = UseStatic(() => Guid.NewGuid());

    return new Text($"Stable ID: {randomId}");
}
```

## External Widgets

### Creating Custom Widgets as NuGet Packages

Ivy now supports **external widgets** - the ability to create custom widgets in separate NuGet packages with their own React frontends. This enables you to build reusable, distributable widget libraries that can be shared across projects or published to NuGet for the community.

**Key Features:**

- Package widgets as standalone NuGet packages with embedded frontend assets
- Build custom React components with full access to modern npm libraries
- Share widgets across multiple projects or publish to NuGet
- Automatic discovery and loading of external widgets at runtime
- Multiple widgets can be bundled in a single package

**Creating an External Widget:**

```csharp
// Mark your widget class with the ExternalWidgetAttribute
[ExternalWidget(
    GlobalName = "MyWidgets",           // Global variable name for the bundle
    ScriptPath = "wwwroot/map.js",      // Path to bundled JavaScript
    StylePath = "wwwroot/map.css")]     // Path to bundled CSS (optional)
public class Map : Widget
{
    [Prop] public double Latitude { get; init; }
    [Prop] public double Longitude { get; init; }
    [Prop] public int Zoom { get; init; } = 13;
}
```

**Frontend Integration:**

External widgets use a React frontend built with Vite. The build system creates an IIFE bundle that shares the host app's React instance:

```typescript
// frontend/src/Map.tsx
import { MapContainer, TileLayer, Marker } from 'react-leaflet';

export function Map({ latitude, longitude, zoom, onIvyEvent }) {
  return (
    <MapContainer center={[latitude, longitude]} zoom={zoom}>
      <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
      <Marker position={[latitude, longitude]} />
    </MapContainer>
  );
}
```

**Using External Widgets:**

Simply reference the NuGet package and use the widget like any built-in widget:

```csharp
Layout.Vertical()
    | new Map()
        .Latitude(40.7128)
        .Longitude(-74.0060)
        .Zoom(12)
```

### Official External Widget Packages

Two official external widget packages are now available:

**Ivy.Widgets.Tiptap** - Rich text editor powered by Tiptap/ProseMirror:

```csharp
new TiptapInput()
    .Value(htmlContent)
    .OnChange(content => State.Content = content)
    .Width(Size.Full())
    .Height(Size.Px(400))
```

**Ivy.Widgets.Leaflet** - Interactive maps using Leaflet:

```csharp
new Map()
    .Latitude(51.505)
    .Longitude(-0.09)
    .Zoom(13)
    .Width(Size.Full())
    .Height(Size.Px(500))
```

Both packages include full TypeScript support, event handling, and follow Ivy's styling conventions. They serve as excellent examples for creating your own external widgets.

## Serialization Improvements

### AlwaysSerialize Property for Input Widgets

The `PropAttribute` now supports an `AlwaysSerialize` property that forces serialization of properties even when they match their default values. This is particularly useful for input widgets where you need to ensure values are always included in the serialized output.

**New API:**

```csharp
[Prop(AlwaysSerialize = true)]
public TValue Value { get; } = default!;
```

This change ensures that `SelectInput<TValue>` and other input widgets properly serialize their `Value` property, including default enum values that were previously omitted during serialization.

## CLI Documentation

### New `ivy run` Documentation

Comprehensive documentation has been added for the `ivy run` command, which is the primary way to run your Ivy applications locally during development. The command provides:

- **Hot reload** - Automatically applies code changes without restarting your application when possible
- **Automatic rebuilds** - Monitors for file changes and restarts when needed
- **Interactive controls** - Use Ctrl+R to restart manually or Ctrl+C to shutdown gracefully
- **Port management** - Flexible port configuration with conflict resolution options

**Key command options:**

```terminal
ivy run                              # Run on default port 5010
ivy run --port 8080                  # Run on custom port
ivy run --browse                     # Auto-open browser
ivy run --app Dashboard              # Run specific app in multi-app projects
ivy run --i-kill-for-this-port      # Kill process using the port
ivy run --find-available-port       # Auto-find available port
ivy run --verbose                   # Enable detailed logging
```

The documentation also includes troubleshooting tips, examples of hot reload scenarios, and guidance on when a full restart is required versus when hot reload will work.

### CLI Documentation Reorganization

The CLI documentation structure has been reorganized to improve navigation and discoverability:

- **Database Integration** documentation has been moved from section 03 to section 05
- **Deployment** documentation has been split into separate guides for each cloud provider:
  - AWS Deployment - Complete guide for Amazon Web Services deployment with ECR and App Runner
  - Azure Deployment - Step-by-step instructions for Microsoft Azure with Container Apps
  - GCP Deployment - Google Cloud Platform deployment using Cloud Run
  - Sliplane Deployment - Modern container platform with automated infrastructure

Each deployment guide now has its own dedicated page with provider-specific setup instructions, prerequisites, and examples. The main deployment overview provides a high-level introduction and directs users to the appropriate provider-specific guide.

This reorganization mirrors the structure used for authentication and database integration documentation, providing a more consistent and navigable experience.

## Widget Enhancements

### Detail Widget - Improved Multiline Field Spacing

The `Detail` widget now has improved vertical spacing for multiline fields. Top padding has been added to multiline detail values to create better visual separation and alignment, especially when detail items have varying content heights.

This enhancement provides more balanced spacing in detail views across all scale variants (Small, Medium, Large), making forms and data displays with mixed single-line and multiline content look more polished.

### CodeInput - YAML Language Support

The `CodeInput` widget now supports YAML syntax highlighting and editing! YAML joins the existing lineup of supported languages including C#, TypeScript, JSON, SQL, HTML, CSS, Python, and more.

**Usage:**

```csharp
var yamlConfig = UseState(
    """
    name: my-app
    version: 1.0.0
    services:
      web:
        image: nginx:latest
        ports:
          - "80:80"
    """);

return yamlConfig.ToCodeInput()
    .Language(Languages.Yaml)
    .Width(Size.Full())
    .Height(Size.Auto());
```

All standard CodeInput features work with YAML:
- Syntax highlighting with proper YAML grammar
- Copy button support
- Size variants (Small, Medium, Large)
- Validation and error states
- Placeholder text for empty states

This is particularly useful for configuration file editors, Docker Compose files, Kubernetes manifests, and other YAML-based configuration scenarios.

**Visual Improvements:**

The code display widget now has refined padding and spacing for the copy button, providing a more polished appearance with better visual alignment between the button and code content.

## New Widgets

### Terminal Widget

A new `Terminal` widget has been added for displaying terminal-style output with commands and responses in a visually distinct console format. This is perfect for documentation, tutorials, or any scenario where you need to show CLI commands and their output.

**Key Features:**

- Terminal-like interface with dark theme styling
- Separate rendering for commands and output
- Built-in copy button for easy command copying
- Customizable header with title
- Can be used with or without header/copy button

**Basic Usage:**

```csharp
Layout.Vertical()
    | new Terminal()
        .Title("Getting Started")
        .AddCommand("dotnet new install Ivy.Templates")
        .AddOutput("Template 'Ivy Application' installed successfully.")
```

**Advanced Styling:**

```csharp
// Without header
new Terminal() { ShowHeader = false }
    .AddCommand("npm install")
    .AddOutput("added 125 packages")

// Without copy button
new Terminal()
    .Title("Read Only")
    .ShowCopyButton(false)
    .AddCommand("git status")
    .AddOutput("nothing to commit, working tree clean")
```

This widget is ideal for installation guides, command-line tutorials, and displaying code execution results in your Ivy applications.

## Chat Widget Enhancements

### Request Cancellation and Loading States

The `Chat` widget now supports request cancellation, allowing users to stop ongoing AI requests at any time. This is particularly useful for long-running operations or streaming responses that users may want to interrupt.

**Key Features:**

- **Cancel Button** - Automatically appears when streaming or loading is active
- **Loading Indicators** - Visual feedback with `ChatLoading` widget
- **Preserved Partial Responses** - When cancelled during streaming, partial text is retained
- **Clean API** - Simple event handlers for send and cancel operations

**API Changes:**

The event handlers have been renamed for clarity and consistency:
- `OnSendMessage` → `OnSend`
- New `OnCancelRequest` → `OnCancel`
- New `Streaming` property to control cancel button visibility

**Basic Usage with Cancel Support:**

```csharp
public override object? Build()
{
    var messages = UseState(ImmutableArray.Create<ChatMessage>(
        new ChatMessage(ChatSender.Assistant, "Hello! I'm here to help.")
    ));

    var ctsState = UseState<CancellationTokenSource?>(default);

    void OnSend(Event<Chat, string> e)
    {
        // Cancel previous request if any
        ctsState.Value?.Cancel();

        var cts = new CancellationTokenSource();
        ctsState.Set(cts);

        // Add user message and loading indicator
        var list = messages.Value
            .Add(new ChatMessage(ChatSender.User, e.Value))
            .Add(new ChatMessage(ChatSender.Assistant, new ChatLoading()));
        messages.Set(list);

        // Process async request with cancellation support
        _ = Task.Run(async () =>
        {
            try
            {
                // Your AI processing here
                await ProcessRequestAsync(e.Value, cts.Token);

                // Update with response
                var all = messages.Value.ToList();
                all[^1] = new ChatMessage(ChatSender.Assistant, response);
                messages.Set(all.ToImmutableArray());
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation gracefully
                var all = messages.Value.ToList();
                all[^1] = new ChatMessage(ChatSender.Assistant,
                    new Error("Cancelled", "Request was cancelled by user."));
                messages.Set(all.ToImmutableArray());
            }
            finally
            {
                ctsState.Set(default);
            }
        });
    }

    void OnCancel(Event<Chat> _)
    {
        ctsState.Value?.Cancel();
    }

    return new Chat(messages.Value.ToArray(), OnSend, OnCancel)
        .Width(Size.Full())
        .Height(Size.Auto());
}
```

**Streaming with Cancellation:**

For streaming responses, use the `Streaming` property to control the cancel button visibility:

```csharp
var isStreaming = UseState(false);

void OnSend(Event<Chat, string> e)
{
    isStreaming.Set(true); // Shows cancel button

    // Add loading indicator
    var list = messages.Value
        .Add(new ChatMessage(ChatSender.User, e.Value))
        .Add(new ChatMessage(ChatSender.Assistant, new ChatLoading()));
    messages.Set(list);

    _ = Task.Run(async () =>
    {
        try
        {
            // Stream response word by word
            await foreach (var chunk in StreamResponseAsync(e.Value, cts.Token))
            {
                var all = messages.Value.ToList();
                all[^1] = new ChatMessage(ChatSender.Assistant, chunk);
                messages.Set(all.ToImmutableArray());
            }
        }
        catch (OperationCanceledException)
        {
            // Partial streamed text is preserved automatically
        }
        finally
        {
            isStreaming.Set(false); // Hides cancel button
        }
    });
}

return new Chat(messages.Value.ToArray(), OnSend, OnCancel)
    .Streaming(isStreaming.Value) // Controls cancel button
    .Width(Size.Full())
    .Height(Size.Auto());
```

This enhancement makes AI chat interactions more responsive and user-friendly by giving users control over long-running operations.

## Typography & Spacing Improvements

### Article Gap Control

The `Article` widget now includes a `Gap` property that allows you to control the spacing between child elements. This provides more flexibility when creating documentation and article layouts.

**Usage:**

```csharp
new Article()
    .Content(articleContent)
    .Gap(6) // Custom gap between elements (default: 4)
    .ShowToc(true)
```

The gap value uses Tailwind's spacing scale (1 unit = 0.25rem), so `Gap(4)` equals 1rem spacing.

### Html and Markdown Widget Scaling

The `Html` and `Markdown` widgets now support scaling via the `Scale` property, allowing you to make rendered content larger or smaller while maintaining proportions:

```csharp
// Small scale (85% of normal size)
new Markdown(content).Scale(Scale.Small)

// Medium scale (default, 100%)
new Markdown(content).Scale(Scale.Medium)

// Large scale (115% of normal size)
new Markdown(content).Scale(Scale.Large)
```

This is particularly useful when displaying documentation or rich text content in constrained spaces like sidebars or cards.

**Internal Improvements:** The Html and Markdown widgets now use a consistent gap of 1rem between elements and apply scaling transformations directly, providing more predictable spacing in all contexts.

### Form Spacing Adjustments

Form layouts now use more compact spacing based on the form scale:
- **Small forms**: 2 units (0.5rem) between fields
- **Medium forms**: 3 units (0.75rem) between fields
- **Large forms**: 4 units (1rem) between fields

This provides better visual density and more professional-looking forms across different scale contexts.

Additionally, form fields now use a uniform 1-unit gap (0.25rem) between labels and inputs across all scales, providing cleaner and more consistent field layouts.

## API Changes

### Empty Widget Now Publicly Accessible

The `Empty` widget constructor has been changed from `internal` to `public`, allowing you to directly instantiate empty placeholder widgets in your applications. Previously, this widget could only be used through internal framework mechanisms.

**Usage:**

```csharp
// Create an empty placeholder widget
Layout.Vertical()
    | new Empty() // Now publicly accessible
```

This is useful when you need an explicit empty widget as a placeholder in your layouts or conditional rendering scenarios.

### Skeleton Widget Now Publicly Accessible

The `Skeleton` widget constructor has been changed from `internal` to `public`, allowing you to directly instantiate skeleton loading placeholders in your applications. Previously, this widget could only be used through internal framework mechanisms.

**Usage:**

```csharp
// Create a skeleton loader for content that's loading
Layout.Vertical()
    | new Skeleton() // Now publicly accessible
        .Width(Size.Full())
        .Height(Size.Px(200))
```

This is useful for creating loading states and placeholder UI while data is being fetched or processed.

The Skeleton widget now uses CSS variables (`bg-muted`) instead of hardcoded colors, ensuring it respects your application's theme configuration and maintains consistent styling across light and dark modes.

## Breaking Changes & Migrations

### Text Size Variants Removed (Breaking Change)

The `Text.Small()`, `Text.Large()`, and `Text.ExtraLarge()` static methods have been **removed** in favor of a fluent `.Scale()` API on the `TextBuilder`. This change provides more consistent sizing across all text variants.

**Migration:**

```csharp
// Before
Text.Small("Small text")
Text.Large("Large text")
Text.ExtraLarge("Extra large text")

// After
Text.P("Small text").Small()
Text.P("Large text").Large()
Text.P("Large text").Large()  // ExtraLarge is now just Large
```

**New Scale API:**

The `TextBuilder` now supports three explicit scale methods:

```csharp
Text.P("Small text").Small()    // Small scale
Text.P("Normal text").Medium()  // Medium scale (default)
Text.P("Large text").Large()    // Large scale

// Works with any text variant
Text.H4("Large heading").Large()
Text.Label("Small label").Small()
Text.Block("Large block").Large()
```

**Key Points:**

- `Text.ExtraLarge()` has been consolidated into `.Large()` - there are now only 3 scales instead of 4
- The scale can be applied to **any text variant**, not just paragraph text
- The default scale is Medium, so you don't need to call `.Medium()` explicitly
- Scale is applied via the `Scale` property on `TextBlock` widget

This change makes text sizing more flexible and composable, allowing you to combine any variant (H1, P, Label, etc.) with any scale (Small, Medium, Large).

### AfterInit → OnMount

The `EffectTrigger.AfterInit` has been renamed to `EffectTrigger.OnMount` for better clarity and React consistency:

```csharp
// Before
UseEffect(() => {
    // initialization code
}, EffectTrigger.AfterInit);

// After
UseEffect(() => {
    // initialization code
}, EffectTrigger.OnMount);
```

Similarly, `QueryOptions.RevalidateOnInit` is now `RevalidateOnMount`.

### IBladeController → IBladeService

The blade management interface has been renamed for consistency with service naming conventions:

```csharp
// Before
var bladeController = GetService<IBladeController>();

// After
var bladeService = GetService<IBladeService>();
```

### BladeHelper.WithHeader Removed

The `BladeHelper.WithHeader` method has been replaced with the `BladeHeader` component:

```csharp
// Before
return BladeHelper.WithHeader(
    title: "Product Details",
    onClose: e => Close(),
    content: Layout.Vertical() | ...
);

// After
return Fragment.From(
    new BladeHeader()
        .Title("Product Details")
        .OnClose(e => Close()),
    Layout.Vertical() | ...
);
```

### AsyncSelectInput Signature Change

`AsyncSelectInput` now uses `UseQuery` pattern instead of Task-based async delegates:

```csharp
// Before
new AsyncSelectInput<Guid, Category>()
    .QueryDelegate(async searchTerm => {
        var categories = await Database.Categories
            .Where(c => c.Name.Contains(searchTerm))
            .ToListAsync();
        return categories.Select(c => new Option<Guid>(c.Id, c.Name));
    })

// After
new AsyncSelectInput<Guid, Category>()
    .SearchDelegate((ctx, searchTerm) => {
        return ctx.UseQuery(
            key: new { Search = searchTerm },
            queryFn: async () => {
                var categories = await Database.Categories
                    .Where(c => c.Name.Contains(searchTerm))
                    .ToListAsync();
                return categories.Select(c => new Option<Guid>(c.Id, c.Name)).ToList();
            }
        );
    })
```

The delegate now takes an `IViewContext` and returns a `QueryResult` instead of a `Task`.

### MetricView Signature Change

`MetricView` now requires a hook that returns `QueryResult<MetricRecord>`:

```csharp
// Before
new MetricView(
    queryDelegate: async () => {
        var count = await Database.Products.CountAsync();
        return new MetricRecord(count, "Total Products");
    }
)

// After
new MetricView(
    hook: ctx => ctx.UseQuery(
        key: "product-count",
        queryFn: async () => {
            var count = await Database.Products.CountAsync();
            return new MetricRecord(count, "Total Products");
        }
    )
)
```

### Chat Event Handler Renames

The Chat widget event handlers have been renamed for consistency:

```csharp
// Before
new Chat(messages, onSendMessage: HandleSend, onCancelRequest: HandleCancel)

// After
new Chat(messages, onSend: HandleSend, onCancel: HandleCancel)
```

### QueryResult Property Renames

Query result properties have been simplified:

```csharp
// Before
if (query.IsLoading) { }
if (query.IsValidating) { }
if (query.IsPrevious) { }

// After
if (query.Loading) { }
if (query.Validating) { }
if (query.Previous) { }
```

## Performance & Internal Improvements

### Typography System Refactoring

The typography system has undergone a major refactoring to provide more consistent and predictable spacing behavior throughout the framework. This change removes implicit margins from text elements and gives you more explicit control over layout spacing.

**Key Changes:**

- Text widgets (headings, paragraphs, etc.) no longer have built-in bottom margins
- Spacing is now controlled explicitly through layout gaps and the `Article` widget's new `Gap` property
- List elements (`<ul>`, `<ol>`) have cleaner, more predictable spacing
- Horizontal rules (`<hr>`) no longer have automatic vertical margins

**Why This Matters:**

Previously, text elements had implicit margins that could interfere with explicit layout spacing, leading to inconsistent gaps and difficulty achieving precise layouts. The new system provides:

- **More predictable layouts**: You control all spacing through layout gaps
- **Cleaner composition**: Widgets compose without unexpected margin collisions
- **Better Article styling**: The `Article` widget now uses a typography context with specialized spacing for documentation

This is an internal improvement that requires no code changes and makes layouts more intuitive to work with.

### Structural Sharing for Widget Trees

The frontend now uses **structural sharing** instead of deep cloning for widget tree updates. This significantly improves rendering performance, especially for large applications.

**How it works:**

- Only nodes along the path to a changed widget are shallow-cloned
- Unchanged subtrees keep their object references
- React.memo can skip re-rendering unchanged components
- Performance scales with tree depth to the change (O(d)) rather than total tree size (O(n))

**Impact:**

- Before: Every update required cloning the entire widget tree + full React diff
- After: Only the path to changed nodes is cloned, React skips unchanged subtrees
- Particularly beneficial in Chrome mode where only the active app typically changes

This is an internal optimization that requires no code changes but delivers automatic performance improvements for all Ivy applications.

## Layout Improvements

### Individual Margin Setters

`LayoutView` now supports setting individual margins for finer control:

```csharp
Layout.Vertical()
    .TopMargin(Size.Px(20))
    .BottomMargin(Size.Px(10))
    .LeftMargin(Size.Px(15))
    .RightMargin(Size.Px(15))
    | ...
```

Previously you could only set all margins at once with `.Margin()`.

### BarChart Default Layout Changed

`BarChart` now defaults to vertical layout instead of horizontal. If you were relying on the default horizontal layout, explicitly set it:

```csharp
new BarChart(data)
    .Layout(ChartLayout.Horizontal) // Explicit if you want horizontal
```

## Chart Improvements

### SortBy API for Chart Data Sorting

Charts now support explicit sorting of X-axis data with the new `SortBy` API. This feature is available for `AreaChart`, `BarChart`, and `LineChart`, giving you full control over the order of data displayed on the X-axis.

**Key Features:**

- Sort by dimension values (ascending or descending)
- Sort by custom expressions with automatic type parsing
- Supports sorting by parsed types (int, DateTime)
- Available on all chart types that use pivot tables

**Basic Sorting:**

```csharp
// Sort by dimension values (alphabetically or numerically)
new BarChart(products)
    .Dimension("Category", p => p.CategoryName)
    .Measure("Sales", p => p.TotalSales)
    .SortBy(SortOrder.Ascending) // Categories sorted A-Z
```

**Sort by Custom Expression:**

```csharp
// Sort by a specific property
new LineChart(sales)
    .Dimension("Month", s => s.Month)
    .Measure("Revenue", s => s.Revenue)
    .SortBy(s => s.MonthNumber, SortOrder.Ascending) // Sort by numeric month
```

**Sort with Type Parsing:**

```csharp
// Sort dates correctly (parses DateTime)
new AreaChart(events)
    .Dimension("Date", e => e.DateString)
    .Measure("Count", e => e.Count)
    .SortBy(e => DateTime.Parse(e.DateString), SortOrder.Ascending)

// Sort numeric strings correctly (parses int)
new BarChart(items)
    .Dimension("Priority", i => i.PriorityLabel)
    .Measure("Tasks", i => i.TaskCount)
    .SortBy(i => int.Parse(i.PriorityLabel), SortOrder.Descending)
```

**Descending Order:**

```csharp
// Show highest values first
new BarChart(products)
    .Dimension("Product", p => p.Name)
    .Measure("Sales", p => p.TotalSales)
    .SortBy(SortOrder.Descending) // Products sorted Z-A
```

This makes it easy to control the visual presentation of your charts, ensuring data appears in the most logical order for your users - whether that's alphabetically, chronologically, or by custom business logic.

## Sample Applications

### New ChatApp Demo

A new real-time chat demonstration has been added showing:
- Username prompt on first load
- Message history with sender/receiver styling
- Real-time message updates
- Integration with chat widget features

### Database CRUD Demos

New sample applications demonstrating UseQuery patterns with Entity Framework:
- **ProductsApp** - Full CRUD with categories and departments
- **CategoriesApp** - Category management with tag-based invalidation
- **DepartmentsApp** - Department management with query caching

These samples demonstrate best practices for:
- Using `UseQuery` for data fetching
- Tag-based cache invalidation after mutations
- Loading states and error handling
- Form integration with query revalidation

## State Input Helpers

New convenience methods have been added for converting state into input widgets, making form creation even more streamlined:

**New Input Helpers:**

```csharp
var password = UseState("");
var bio = UseState("");

// Convert state to TextAreaInput
var bioInput = bio.ToTextAreaInput()
    .Placeholder("Tell us about yourself...")
    .Rows(5);

// Convert state to PasswordInput
var passwordInput = password.ToPasswordInput()
    .Placeholder("Enter password");
```

**Complete Input Helper API:**

```csharp
var state = UseState("");

// Text inputs
state.ToTextInput()         // Single-line text
state.ToTextAreaInput()     // Multi-line text (new!)
state.ToPasswordInput()     // Masked password input (new!)

// Other input types
state.ToNumberInput()       // Numeric input
state.ToBoolInput()         // Checkbox/toggle
state.ToSelectInput(options) // Dropdown select
```

These helpers automatically bind the state value to the input and handle `OnChange` events, reducing boilerplate when building forms.

## Bug Fixes & Improvements

### Blade Widget Scroll Behavior

The `Blade` widget container now has improved scroll behavior and layout handling. Internal styling updates ensure that blade content properly fills the available height and scrolls correctly when content overflows. This fix addresses layout issues where blade content might not have been scrolling as expected in certain scenarios.

### Markdown File Handling in Development Mode

The Vite development server now automatically proxies requests for `.md` files to the backend server. This improvement ensures that markdown documentation and content files are properly served during local development, matching the behavior in production environments.

Previously, attempting to fetch markdown files in dev mode might result in errors or incorrect content. Now, all `.md` requests are transparently forwarded to the backend (default: `http://localhost:5010` or `IVY_HOST` environment variable), providing a seamless development experience when working with markdown-based content like the `Article` widget or documentation pages.

## Documentation Improvements

### Documentation for AI Assistants

A new `llms.txt` file has been added to the repository root and is now **included in the NuGet package**, providing structured documentation specifically designed for AI assistants and code completion tools. This file contains:

- Core Ivy concepts and terminology
- Common widget patterns and examples
- Hook usage guidelines
- Best practices and conventions

This improves the quality of AI-assisted development when working with Ivy, ensuring code suggestions align with framework conventions. AI tools can now automatically discover and use this documentation when working with Ivy projects.

**Recent Enhancements:**

The `llms.txt` documentation has been expanded with more comprehensive coverage:

- **UseEffect Triggers** - Complete documentation of effect trigger types (`OnBuild`, `OnMount`, `OnStateChange`) with proper signatures
- **Input Widgets** - New section explaining state binding patterns with examples:
  ```csharp
  var userName = UseState("");
  var input = userName.ToTextInput().Placeholder("Enter your name");
  ```
- **Widget Coverage** - Added Callout widget documentation
- **Further Reading** - Expanded links to Forms, DataTable, Details, Services, and utility APIs (Size, Align, Colors)

These improvements help AI assistants provide more accurate code suggestions for forms, state management, and effect handling.

### Complete Hooks Documentation

All Ivy hooks now have comprehensive documentation pages with examples, best practices, and troubleshooting guides. This major documentation update includes:

**New Hook Documentation Pages:**

- **UseContext & CreateContext** - Component-scoped data sharing and context management
- **UseArgs** - Receiving and handling navigation arguments and route parameters
- **UseMutation** - Cross-component query cache control and optimistic updates
- **UseDownload** - File generation and download functionality with async support
- **UseRefreshToken** - Token-based component refresh coordination
- **UseTrigger** - Conditional rendering for modals, dialogs, and popups
- **UseUpload** - File upload endpoint creation with validation
- **UseWebhook** - HTTP endpoint creation for external system integration
- **UseBlades** - Blade (side panel) interface foundation
- **UseForm** - Advanced form handling with validation and submission
- **UseNavigation** - Programmatic navigation between apps and routes
- **UseAlert** - Alert and notification display

**Enhanced Existing Documentation:**

All hook pages now include:
- Interactive code examples with live demos
- Mermaid diagrams showing data flow and lifecycle
- Comprehensive API reference tables
- Common pitfalls and troubleshooting sections
- Cross-references to related concepts
- Best practices and patterns

**Documentation Reorganization:**

The hooks documentation has been restructured for better navigation:
- Core hooks moved to `03_Hooks/Core/` directory
- Consistent naming and numbering scheme
- Improved cross-linking between related hooks
- Better integration with widget and concept documentation

This update makes the Ivy hooks system fully documented, providing developers with complete reference material for building applications.

### Widget Documentation Navigation

The widget documentation pages have been enhanced with improved navigation. The API section now appears in the Table of Contents with working scroll navigation, making it easier to jump directly to API details when browsing widget reference documentation.

### UseQuery Concept Documentation

Comprehensive documentation has been added for the UseQuery hook system, including:
- Core concepts and comparison to React Query/SWR
- Migration guides from Task-based patterns
- Cache invalidation strategies
- Pagination and polling patterns
- Best practices for query keys and scoping

### Connections Concept Documentation

A new comprehensive documentation page has been added for **Connections** - Ivy's unified abstraction for integrating external data sources and services. This documentation covers:

**Core Concepts:**
- Connection interface (`IConnection`) implementation
- Support for databases, third-party APIs, cloud services, and custom internal services
- Standardized service registration in the DI container
- Metadata exposure (name, type, entities)

**Connection Types:**

Database connections are automatically generated through Ivy CLI:

```terminal
ivy db add --provider Postgres --name MyDatabase
```

Custom API connections can be created for any external service:

```csharp
public class StripeConnection : IConnection, IHaveSecrets
{
    public string GetName() => "Stripe";
    public string GetConnectionType() => "PaymentAPI";

    public ConnectionEntity[] GetEntities() =>
    [
        new("Customer", "Customers"),
        new("Payment", "Payments"),
        new("Subscription", "Subscriptions")
    ];

    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IStripeClient, StripeClient>();
        services.AddScoped<IPaymentService, StripePaymentService>();
    }

    public Secret[] GetSecrets() =>
    [
        new("Stripe:SecretKey"),
        new("Stripe:PublishableKey")
    ];
}
```

**Secrets Management:**

Connections integrate with Ivy's secrets management by implementing `IHaveSecrets` for compile-time validation of required API keys and connection strings.

**Automatic Registration:**

```csharp
var server = new Server();
server.AddConnectionsFromAssembly(); // Scans and registers all connections
await server.RunAsync();
```

**Using Connection Services:**

```csharp
public class PaymentView : ViewBase
{
    public override object? Build()
    {
        var paymentService = UseService<IPaymentService>();
        var payments = paymentService.GetRecentPayments();

        return new DataTable<Payment>(payments);
    }
}
```

This documentation provides developers with a clear understanding of how to integrate external systems into their Ivy applications using a standardized pattern.

### Complete Hooks Documentation

All Ivy hooks now have comprehensive documentation pages with examples, best practices, and troubleshooting guides. This major documentation update includes:

**New Hook Documentation Pages:**

- **UseContext & CreateContext** - Component-scoped data sharing and context management
- **UseArgs** - Receiving and handling navigation arguments and route parameters
- **UseMutation** - Cross-component query cache control and optimistic updates
- **UseDownload** - File generation and download functionality with async support
- **UseRefreshToken** - Token-based component refresh coordination
- **UseTrigger** - Conditional rendering for modals, dialogs, and popups
- **UseUpload** - File upload endpoint creation with validation
- **UseWebhook** - HTTP endpoint creation for external system integration
- **UseBlades** - Blade (side panel) interface foundation
- **UseForm** - Advanced form handling with validation and submission
- **UseNavigation** - Programmatic navigation between apps and routes
- **UseAlert** - Alert and notification display

**Enhanced Existing Documentation:**

All hook pages now include:
- Interactive code examples with live demos
- Mermaid diagrams showing data flow and lifecycle
- Comprehensive API reference tables
- Common pitfalls and troubleshooting sections
- Cross-references to related concepts
- Best practices and patterns

**Documentation Reorganization:**

The hooks documentation has been restructured for better navigation:
- Core hooks moved to `03_Hooks/Core/` directory
- Consistent naming and numbering scheme
- Improved cross-linking between related hooks
- Better integration with widget and concept documentation

This update makes the Ivy hooks system fully documented, providing developers with complete reference material for building applications.

### Hook Documentation Reorganization

The hooks documentation files have been renamed for better clarity and consistency. All hook documentation pages now use explicit naming (e.g., `UseState.md` instead of `State.md`). This improves discoverability and makes the documentation structure more intuitive.

Over 50 documentation links have been updated throughout the codebase to reflect these changes:
- Links in concept pages (Views, Program, Forms, Navigation, etc.)
- Links in widget documentation (AsyncSelect, Blades, DropDownMenu, etc.)
- Cross-references between hooks
- Examples in getting started guides

The documentation content has also been compressed and refined for clarity, removing redundant examples while maintaining comprehensive coverage of all hook features.
