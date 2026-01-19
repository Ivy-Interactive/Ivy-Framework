# Ivy Framework Weekly Notes - Week of 2026-01-16

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## UseQuery Hook - Modern Data Fetching

Ivy now includes **UseQuery** - a powerful data fetching and caching system inspired by React Query and SWR. This hook provides automatic caching, background revalidation, and loading state management for async data operations.

![output](https://github.com/user-attachments/assets/b7cf7340-26b4-4b1a-9f7a-ecfe0d4b1411)

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
    var factory = UseService<SampleDbContextFactory>();
    
    var productsQuery = UseQuery(
        key: "products",
        fetcher: async ct =>
        {
            await using var db = factory.CreateDbContext();
            return await db.Products.ToListAsync(ct);
        }
    );

    if (productsQuery.Loading)
        return new Skeleton();

    if (productsQuery.Error != null)
        return Callout.Error($"Failed to load products: {productsQuery.Error.Message}");

    if (productsQuery.Value == null)
        return Text.Muted("No products found");

    return Layout.Vertical()
        | productsQuery.Value.Select(p => Text.Literal(p.Name));
}
```

## Simplified Hook Syntax

You can now call hooks directly without the `this.` prefix! Hook methods like `UseState`, `UseEffect`, `UseMemo` etc. are now available directly in the `Build()` method scope.

## External Widgets

### Creating Custom Widgets as NuGet Packages

Ivy now supports **external widgets** - the ability to create custom widgets in separate NuGet packages with their own React frontends. Run this command to scaffold defaults:

```bash
ivy widget init
```

**Key Features:**

- Package widgets as standalone NuGet packages with embedded frontend assets
- Build custom React components with full access to modern npm libraries
- Share widgets across multiple projects or publish to NuGet
- Automatic discovery and loading of external widgets at runtime
- Multiple widgets can be bundled in a single package

**Using External Widgets:**

Simply reference the NuGet package and use the widget like any built-in widget:

```csharp
Layout.Vertical()
    | new Map()
        .Latitude(40.7128)
        .Longitude(-74.0060)
        .Zoom(12);
```

## Serialization Improvements

The `PropAttribute` now supports an `AlwaysSerialize` property that forces serialization of properties even when they match their default values.

```csharp
[Prop(AlwaysSerialize = true)]
public TValue Value { get; } = default!;
```

## New `ivy run` command

`ivy run` is the primary way to run your Ivy applications during development. The command provides:

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

## Breaking Changes

> List of all breaking changes and instructions for LLMS how to apply them can be found [here](https://github.com/Ivy-Interactive/Ivy-Framework/tree/main/src/.releases/Refactors/1.2.11)

### Text Widget Refactor: Scale & Margins

We have cleaned up the `Text` widget API to be more consistent and predictable.

#### Fluent Scale API

Replaced static methods `Text.Small()` and `Text.Large()` with chainable modifiers on `TextBuilder`. Updated 43 files across the codebase.

```csharp
// Before (removed)
Text.Small("Small text")
Text.Large("Large text")

// After
Text.P("Small text").Small()
Text.P("Normal text")
Text.P("Large text").Large()

// Chainable
Text.P("Important").Large().Bold().Color(Colors.Primary)
```

### AppContext Rename

`AppArgs` has been renamed to `Ivy.Apps.AppContext`.

```csharp
// Before
var args = UseService<AppArgs>();

// After
var args = UseService<Ivy.Apps.AppContext>();
```

### AsyncSelectInput Delegates

Delegates now support hooks and `UseQuery`. `AsyncSelectQueryDelegate` renamed to `AsyncSelectSearchDelegate`.

```csharp
// Before
public delegate Task<Option<T>[]> AsyncSelectQueryDelegate<T>(string query);

// After
public delegate QueryResult<Option<T>[]> AsyncSelectSearchDelegate<T>(IViewContext context, string query);
```

### BladeHeader Component

`BladeHelper.WithHeader` replaced by `BladeHeader` component.

```csharp
// Before
return BladeHelper.WithHeader(header, content);

// After
return new Fragment()
       | new BladeHeader(header)
       | content;
```

### BladeService Rename

`IBladeController` renamed to `IBladeService`.

```csharp
// Before
var blades = UseContext<IBladeController>();

// After
var blades = UseContext<IBladeService>();
```

### MetricView Hook Support

`MetricView` now supports hooks via `IViewContext`.

```csharp
// Before
new MetricView("Sales", Icons.Money, async () => ...);

// After
new MetricView("Sales", Icons.Money, ctx => ctx.UseQuery(...));
```

### EffectTrigger.OnMount

`EffectTrigger.AfterInit` renamed to `EffectTrigger.OnMount`.

### UseStatic to UseRef

`UseStatic` renamed to `UseRef` and now returns `IState<T>`.

```csharp
// Before
var svc = UseStatic(() => new Service());

// After
var svcRef = UseRef(() => new Service());
var svc = svcRef.Value;
```

## Other improvements

### Chart Sorting API

We have added a `SortBy` API for charts, allowing you to sort X-axis data.

```csharp
new BarChart(data)
    .SortBy(x => x.Value, SortDirection.Descending)
```

### Other Enhancements

- **YAML Support**: The `CodeInput`, `Code` widget now supports YAML syntax highlighting.
- `Terminal` widget had been now exposed as a widget and can be used to display terminal-styled output with commands and responses.
- `Chat` widget now automatically displays `Cancel` button when streaming or loading is active
- All charts now support `SortBy` API giving you control over the order of data displayed on the X-axis
