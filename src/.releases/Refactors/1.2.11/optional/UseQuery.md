# UseQuery Pattern Migration - v1.2.11

## Summary

This guide describes how to migrate from the legacy `UseEffect` + `UseState` data fetching pattern to the new `UseQuery` hook pattern. This is an **optional** refactoring that provides better loading states, caching, and cache invalidation.

## What Changed

### Data Fetching: UseEffect + UseState → UseQuery

**Before (v1.2.10 and earlier):**

```csharp
var product = UseState<Product?>();

UseEffect(async () =>
{
    var db = factory.CreateDbContext();
    product.Set(await db.Products.SingleOrDefaultAsync(e => e.Id == productId));
}, [EffectTrigger.AfterInit(), refreshToken]);

if (product.Value == null) return null;
```

**After (v1.2.11+):**

```csharp
var productQuery = UseQuery(
    key: (nameof(ProductDetailsBlade), productId),
    fetcher: async ct =>
    {
        await using var db = factory.CreateDbContext();
        return await db.Products.SingleOrDefaultAsync(e => e.Id == productId, ct);
    },
    tags: [(typeof(Product), productId)]
);

if (productQuery.Loading) return Skeleton.Card();
if (productQuery.Value == null) return new Callout("Product not found.").Variant(CalloutVariant.Warning);
```

### Key Differences

| Aspect | Old Pattern | New Pattern |
|--------|-------------|-------------|
| Data storage | `UseState<T?>()` | `QueryResult<T>` from `UseQuery` |
| Loading state | `if (value == null) return null;` | `if (query.Loading) return Skeleton.Card();` |
| Fetch trigger | `UseEffect` with `EffectTrigger.AfterInit()` | Automatic on mount and key change |
| Refresh | `refreshToken` dependency | `query.Mutator.Revalidate()` or `queryService.RevalidateByTag()` |
| Caching | None | Automatic with key-based caching |
| Cross-component invalidation | Manual via `RefreshToken` | `queryService.RevalidateByTag(tag)` |

## How to Find Affected Code

Search for these patterns:

### Pattern 1: UseEffect with EffectTrigger.AfterInit for data fetching

```regex
UseEffect\s*\(\s*async\s*\(\s*\)\s*=>\s*\{[\s\S]*?EffectTrigger\.AfterInit\(\)
```

### Pattern 2: UseState for nullable entity storage

```regex
UseState<\w+\?>\s*\(\s*\)
```

### Pattern 3: Null check for loading state

```regex
if\s*\(\s*\w+\.Value\s*==\s*null\s*\)\s*return\s*null;
```

### Pattern 4: Auto-save pattern in create/edit dialogs

```regex
UseEffect\s*\(\s*\(\s*\)\s*=>\s*\{[\s\S]*?\},\s*\[\s*\w+\s*\]\s*\)
```

## How to Refactor

### Replace UseEffect + UseState with UseQuery

**Before:**

```csharp
public override object? Build()
{
    var factory = UseService<SampleDbContextFactory>();
    var product = UseState<Product?>();
    var refreshToken = UseRefreshToken();

    UseEffect(async () =>
    {
        var db = factory.CreateDbContext();
        product.Set(await db.Products
            .Include(e => e.Category)
            .SingleOrDefaultAsync(e => e.Id == productId));
    }, [EffectTrigger.AfterInit(), refreshToken]);

    if (product.Value == null) return null;

    // ... rest of component
}
```

**After:**

```csharp
public override object? Build()
{
    var factory = UseService<SampleDbContextFactory>();

    var productQuery = UseQuery(
        key: (nameof(ProductDetailsBlade), productId),
        fetcher: async ct =>
        {
            await using var db = factory.CreateDbContext();
            return await db.Products
                .Include(e => e.Category)
                .SingleOrDefaultAsync(e => e.Id == productId, ct);
        },
        tags: [(typeof(Product), productId)]
    );

    if (productQuery.Loading) return Skeleton.Card();

    if (productQuery.Value == null)
    {
        return new Callout($"Product '{productId}' not found.")
            .Variant(CalloutVariant.Warning);
    }

    // ... rest of component
}
```

### Replace Auto-Save Pattern with HandleSubmit

**Before (auto-save on state change):**

```csharp
var product = UseState(() => factory.CreateDbContext().Products.First(e => e.Id == id)!);

UseEffect(() =>
{
    using var db = factory.CreateDbContext();
    product.Value.UpdatedAt = DateTime.UtcNow;
    db.Products.Update(product.Value);
    db.SaveChanges();
    refreshToken.Refresh();
}, [product]);

return product
    .ToForm()
    .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt)
    .ToSheet(isOpen, "Edit Product");
```

**After (explicit submit handler):**

```csharp
var productQuery = UseQuery(
    key: (typeof(Product), id),
    fetcher: async ct =>
    {
        await using var db = factory.CreateDbContext();
        return await db.Products.FirstAsync(e => e.Id == id, ct);
    },
    tags: [(typeof(Product), id)]
);

if (productQuery.Loading || productQuery.Value == null)
    return Skeleton.Form().ToSheet(isOpen, "Edit Product");

return productQuery.Value
    .ToForm()
    .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt)
    .HandleSubmit(OnSubmit)
    .ToSheet(isOpen, "Edit Product");

async Task OnSubmit(Product? request)
{
    if (request == null) return;
    await using var db = factory.CreateDbContext();
    request.UpdatedAt = DateTime.UtcNow;
    db.Products.Update(request);
    await db.SaveChangesAsync();
    queryService.RevalidateByTag((typeof(Product), id));
}
```

### Replace Auto-Save Pattern in Create Dialogs

Create dialogs also use the auto-save pattern and need to be migrated to `HandleSubmit`.

**Before (auto-save on state change):**

```csharp
public class ProductCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<SampleDbContextFactory>();
        var product = UseState(() => new ProductCreateRequest());

        UseEffect(() =>
        {
            var productId = CreateProduct(factory, product.Value);
            refreshToken.Refresh(productId);
        }, [product]);

        return product
            .ToForm()
            .Builder(e => e.CategoryId, e => e.ToAsyncSelectInput(...))
            .ToDialog(isOpen, title: "Create Product", submitTitle: "Create");
    }

    private int CreateProduct(SampleDbContextFactory factory, ProductCreateRequest request)
    {
        using var db = factory.CreateDbContext();
        // ... sync create logic
    }
}
```

**After (explicit submit handler):**

```csharp
public class ProductCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<SampleDbContextFactory>();
        var product = UseState(() => new ProductCreateRequest());

        return product
            .ToForm()
            .Builder(e => e.CategoryId, e => e.ToAsyncSelectInput(...))
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, title: "Create Product", submitTitle: "Create");

        async Task OnSubmit(ProductCreateRequest request)
        {
            var productId = await CreateProductAsync(factory, request);
            refreshToken.Refresh(productId);
        }
    }

    private async Task<int> CreateProductAsync(SampleDbContextFactory factory, ProductCreateRequest request)
    {
        await using var db = factory.CreateDbContext();
        // ... async create logic
        await db.SaveChangesAsync();
        return product.Id;
    }
}
```

### Replace FilteredListView with UseQuery + Manual List

**Before:**

```csharp
return new FilteredListView<ProductListRecord>(
    fetchRecords: (filter) => FetchProducts(factory, filter),
    createItem: CreateItem,
    toolButtons: createBtn,
    onFilterChanged: _ => blades.Pop(this)
);

private async Task<ProductListRecord[]> FetchProducts(IvyAgentExamplesContextFactory factory, string filter)
{
    await using var db = factory.CreateDbContext();
    // ... query logic
}
```

**After:**

```csharp
var filter = UseState("");
var throttledFilter = UseState("");

UseEffect(() =>
{
    throttledFilter.Set(filter.Value);
    blades.Pop(this);
}, [filter.Throttle(TimeSpan.FromMilliseconds(250)).ToTrigger()]);

var productsQuery = UseProductListRecords(Context, throttledFilter.Value);

var header = Layout.Horizontal().Gap(1)
             | filter.ToSearchInput().Placeholder("Search").Width(Size.Grow())
             | createBtn;

return new Fragment()
       | new BladeHeader(header)
       | (productsQuery.Loading ? Text.Muted("Loading...") : new List(items));

public static QueryResult<ProductListRecord[]> UseProductListRecords(IViewContext context, string filter)
{
    var factory = context.UseService<SampleDbContextFactory>();
    return context.UseQuery(
        key: (nameof(UseProductListRecords), filter),
        fetcher: async ct =>
        {
            await using var db = factory.CreateDbContext();
            // ... query logic
        },
        tags: [typeof(ProductListRecord[])],
        options: new QueryOptions { KeepPrevious = true }
    );
}
```

### Refresh Pattern: RefreshToken → RevalidateByTag

**Before:**

```csharp
UseEffect(() =>
{
    if (refreshToken.ReturnValue is int productId)
    {
        blades.Pop(this, true); // refresh: true triggers parent refresh
        blades.Push(this, new ProductDetailsBlade(productId));
    }
}, [refreshToken]);
```

**After:**

```csharp
UseEffect(() =>
{
    if (refreshToken.ReturnValue is Guid productId)
    {
        blades.Pop(this);
        productsQuery.Mutator.Revalidate(); // Revalidate this specific query
        blades.Push(this, new ProductDetailsBlade(productId));
    }
}, [refreshToken]);

// Or from a different component, use IQueryService:
var queryService = UseService<IQueryService>();
queryService.RevalidateByTag(typeof(ProductListRecord[]));
```

## UseQuery API Reference

### Basic Signature

```csharp
QueryResult<T> UseQuery<T, TKey>(
    TKey key,
    Func<CancellationToken, Task<T>> fetcher,
    object[]? tags = null,
    QueryOptions? options = null,
    T? initialValue = default
)
```

### QueryResult<T> Properties

- `T? Value` - The fetched data (null while loading on first fetch)
- `bool Loading` - True while fetching
- `Exception? Error` - Any error that occurred
- `QueryMutator Mutator` - Methods to control the query

### QueryMutator Methods

- `Revalidate()` - Refetch the data
- `SetData(T value)` - Optimistically update the cached data

### QueryOptions Properties

- `KeepPrevious = true` - Keep showing previous data while refetching (avoid flash)
- `RevalidateOnInit = false` - Don't refetch if cached data exists

### Tags for Cache Invalidation

```csharp
// Single entity
tags: [(typeof(Product), productId)]

// Collection
tags: [typeof(ProductListRecord[])]

// Invalidate by tag
queryService.RevalidateByTag((typeof(Product), productId));
queryService.RevalidateByTag(typeof(ProductListRecord[]));
```

## Static UseQuery Helpers Pattern

For reusable queries, create static methods or extension methods:

```csharp
public static class ProductHelpers
{
    // Extension method pattern (called as Context.UseCategoryOptions())
    public static QueryResult<Option<Guid>[]> UseCategoryOptions(this IViewContext context)
    {
        var factory = context.UseService<SampleDbContextFactory>();
        return context.UseQuery(
            key: nameof(UseCategoryOptions),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Categories
                    .Select(e => new Option<Guid>(e.Name, e.Id))
                    .ToArrayAsync(ct);
            });
    }

    // Static method pattern (called as ProductHelpers.UseCategorySearch)
    public static QueryResult<Option<Guid?>[]> UseCategorySearch(IViewContext context, string filter)
    {
        var factory = context.UseService<SampleDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseCategorySearch), filter),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return (await db.Categories
                        .Where(e => e.Name.Contains(filter))
                        .Take(50)
                        .ToArrayAsync(ct))
                    .Select(e => new Option<Guid?>(e.Name, e.Id))
                    .ToArray();
            });
    }
}
```

## Skeleton Components for Loading States

Use appropriate skeleton components while loading:

```csharp
if (productQuery.Loading) return Skeleton.Card();
if (formQuery.Loading) return Skeleton.Form().ToSheet(isOpen, "Edit");
```

## BladeHeader for Detail Views

Detail blades should include a `BladeHeader` with the entity name to set the blade title dynamically:

```csharp
var product = productQuery.Value;

var detailsCard = new Card(
    ...
).Title("Product Details");

return new Fragment()
       | new BladeHeader(Text.Literal(product.Name))  // Sets the blade title
       | (Layout.Vertical() | detailsCard);
```

This pattern ensures the blade tab shows the entity name (e.g., "Acme Corp") instead of a generic title, improving navigation when multiple detail blades are open.

## Common Pitfalls

### Private Record Types and Cross-Component Invalidation

If you define a list record as a **private nested type** inside a blade class:

```csharp
public class ProductListBlade : ViewBase
{
    private record ProductListRecord(int Id, string Name); // Private!

    // ...
}
```

You **cannot** use `typeof(ProductListRecord[])` as a tag from other components (e.g., `ProductDetailsBlade`) because the type is not accessible. This causes **CS0246** errors.

**Solutions:**

1. **Use entity types for cross-component tags** (recommended):

```csharp
// In ProductListBlade - use entity type for the list tag
tags: [typeof(Product[])]

// In ProductDetailsBlade - invalidate using entity type
queryService.RevalidateByTag(typeof(Product[]));
```

1. **Or define the record at namespace level** (if you need the specific record type):

```csharp
namespace MyApp.Views;

public record ProductListRecord(int Id, string Name); // Public, accessible everywhere

public class ProductListBlade : ViewBase
{
    // Can now use typeof(ProductListRecord[]) across components
}
```

### IQueryService Must Be Injected

When using `queryService.RevalidateByTag()`, you must first inject the service:

```csharp
var queryService = UseService<IQueryService>();
queryService.RevalidateByTag(typeof(Product[]));
```

## Required Usings

```csharp
using Ivy.Hooks;
```

For throttled filter inputs (requires `System.Reactive` package):

```csharp
using System.Reactive.Linq;
```

## Verification

After refactoring, run:

```bash
dotnet build
```

All components should compile and loading states should display correctly.
