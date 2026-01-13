---
searchHints:
  - usequery
  - swr
  - stale-while-revalidate
  - fetch
  - cache
  - async
  - loading
  - revalidate
---

# Query

<Ingress>
Fetch, cache, and synchronize server data with the [UseQuery](../02_RulesOfHooks.md) hook.
</Ingress>

The `UseQuery` [hook](../02_RulesOfHooks.md) provides a powerful way to fetch and cache asynchronous data. Inspired by [SWR](https://swr.vercel.app/) (stale-while-revalidate), it returns cached data immediately while revalidating in the background, keeping your [UI](../../01_Onboarding/02_Concepts/02_Views.md) fast and your data fresh.

## Basic Usage

The simplest form of `UseQuery` takes a key and a fetcher function:

```csharp demo-below
public class BasicQueryView : ViewBase
{
    public override object? Build()
    {
        var query = UseQuery(
            key: "user-profile",
            fetcher: async ct =>
            {
                await Task.Delay(1000, ct);
                return new { Name = "Alice", Email = "alice@example.com" };
            });

        if (query.Loading) return "Loading...";

        return Layout.Vertical()
            | query //query has a Build extension that produces a debug view
            | query.Value?.Name
            | query.Value?.Email
            | (Layout.Horizontal() 
                | new Button("Revalidate", _ => query.Mutator.Revalidate()).Variant(ButtonVariant.Primary) 
                | new Button("Invalidate", _ => query.Mutator.Invalidate()).Variant(ButtonVariant.Primary)) 
                ;
    }
}
```

## Keys

Keys can be any serializable value, such as strings, numbers, or complex objects like tuples. You can also use a key factory function.

## Query Result

`UseQuery` returns a `QueryResult<T>` with the following properties:

| Property | Type | Description |
|----------|------|-------------|
| `Value` | `T?` | The fetched data |
| `Loading` | `bool` | True during initial fetch |
| `Validating` | `bool` | True during background revalidation |
| `Previous` | `bool` | True when showing stale data during key change |
| `Error` | `Exception?` | The error if fetch failed |
| `Mutator` | `QueryMutator<T>` | Methods to mutate the cache |

## Query Options

Configure query behavior with `QueryOptions`:

```csharp
var query = UseQuery(
    key: "data",
    fetcher: FetchData,
    options: new QueryOptions
    {
        Scope = QueryScope.Server,                  // Cache scope
        Expiration = TimeSpan.FromMinutes(5),       // TTL before revalidation
        KeepPrevious = true,                        // Keep previous data during key change
        RefreshInterval = TimeSpan.FromSeconds(30), // Auto-refresh interval
        RevalidateOnMount = true                    // Fetch on mount
    });
```

## Query Scopes

Control where query data is cached and shared:

| Scope | Description                                           |
|-------|-------------------------------------------------------|
| `Server` | Shared across all users (default)                     |
| `App` | Shared within a app session                           |
| `Device` | Shared across apps on same device                     |
| `View` | Isolated to component instance, cleaned up on unmount |

## Conditional Fetching

When the key is `null` (often controlled by [UseState](./03_State.md)), UseQuery returns an idle result without fetching:

```csharp demo-below
public class ConditionalQueryView : ViewBase
{
    public override object? Build()
    {
        var shouldFetch = UseState(false);

        var query = UseQuery(
            key: shouldFetch.Value ? "data" : null,
            fetcher: async ct =>
            {
                await Task.Delay(1000, ct);
                return $"Fetched at {DateTime.Now:HH:mm:ss}";
            });

        return Layout.Vertical()
            | shouldFetch.ToBoolInput().Label("Enable fetching")
            | (shouldFetch.Value
                ? query.Loading
                    ? Text.Literal("Loading...")
                    : Text.Literal(query.Value ?? "")
                : Text.Muted("Fetching disabled"));
    }
}
```

## Dependent Fetching

Use a key factory to fetch data that depends on another query:

```csharp demo-below
public class DependentQueryView : ViewBase
{
    public override object? Build()
    {
        var user = UseQuery(
            key: "user",
            fetcher: async ct =>
            {
                await Task.Delay(800, ct);
                return new { Id = 42, Name = "Alice" };
            });

        // Only fetches when user is loaded
        var projects = UseQuery(
            () => user.Value?.Id,
            async (userId, ct) =>
            {
                await Task.Delay(800, ct);
                return new[] { $"Project A (user {userId})", $"Project B (user {userId})" };
            });

        return Layout.Vertical()
            | Text.Literal($"User: {(user.Loading ? "Loading..." : user.Value?.Name)}")
            | Text.Literal($"Projects: {(projects.Loading ? "Loading..." : string.Join(", ", projects.Value ?? []))}");
    }
}
```

## Mutations

The `Mutator` provides methods to update cached data:

| Method | Description |
|--------|-------------|
| `Mutate(value, revalidate)` | Update cache with new value |
| `Revalidate()` | Trigger background revalidation |
| `Invalidate()` | Clear cache and refetch |

```csharp demo-below
public class MutationView : ViewBase
{
    public override object? Build()
    {
        var query = UseQuery(
            key: "counter",
            fetcher: async ct =>
            {
                await Task.Delay(500, ct);
                return Random.Shared.Next(1, 100);
            });

        if (query.Loading)
            return Text.Literal("Loading...");

        return Layout.Vertical()
            | Text.Literal($"Value: {query.Value}")
            | (query.Validating ? Text.Muted("Syncing...") : null!)
            | Layout.Horizontal()
                | new Button("+10 (Optimistic)", _ =>
                    query.Mutator.Mutate(query.Value + 10, revalidate: true))
                    .Variant(ButtonVariant.Primary)
                | new Button("Set 999", _ =>
                    query.Mutator.Mutate(999, revalidate: false))
                    .Variant(ButtonVariant.Secondary)
                | new Button("Refresh", _ => query.Mutator.Revalidate())
                    .Variant(ButtonVariant.Outline);
    }
}
```

### Cross-Component Mutations

Use `UseMutation` to control a query from a different component:

```csharp demo-below
public class SharedDataDisplay : ViewBase
{
    public override object? Build()
    {
        var query = UseQuery(
            key: "shared-data",
            fetcher: async ct =>
            {
                await Task.Delay(500, ct);
                return $"Data: {Guid.NewGuid().ToString()[..8]}";
            });

        return Layout.Horizontal()
            | Text.Literal(query.Loading ? "Loading..." : query.Value ?? "")
            | (query.Validating ? Text.Muted(" (updating...)") : null!);
    }
}

public class SharedDataControls : ViewBase
{
    public override object? Build()
    {
        var mutator = UseMutation("shared-data");

        return Layout.Horizontal()
            | new Button("Revalidate", _ => mutator.Revalidate())
                .Variant(ButtonVariant.Outline)
            | new Button("Invalidate", _ => mutator.Invalidate())
                .Variant(ButtonVariant.Destructive);
    }
}
```

## Tag-Based Invalidation

Assign tags to queries for bulk invalidation (using [UseService](../../01_Onboarding/02_Concepts/18_Services.md)). Tags are serializable the same way as keys.

```csharp demo-below
public class TaggedQueriesView : ViewBase
{
    public override object? Build()
    {
        var queryService = UseService<IQueryService>();

        var users = UseQuery(
            key: "dashboard/users",
            fetcher: async ct =>
            {
                await Task.Delay(500, ct);
                return $"Users: {Random.Shared.Next(100, 500)}";
            },
            tags: ["dashboard", "users"]);

        var orders = UseQuery(
            key: "dashboard/orders",
            fetcher: async ct =>
            {
                await Task.Delay(500, ct);
                return $"Orders: {Random.Shared.Next(50, 200)}";
            },
            tags: ["dashboard", "orders"]);

        return Layout.Vertical()
            | Text.Literal(users.Loading ? "Loading..." : users.Value ?? "")
            | Text.Literal(orders.Loading ? "Loading..." : orders.Value ?? "")
            | (Layout.Horizontal()
                | new Button("Refresh All", _ => queryService.RevalidateByTag("dashboard"))
                | new Button("Invalidate All", _ => queryService.InvalidateByTag("dashboard"))
            );
    }
}
```

## Polling

Automatically revalidate at intervals with `RefreshInterval`:

```csharp demo-below
public class PollingView : ViewBase
{
    public override object? Build()
    {
        var liveData = UseQuery(
            key: "live-data",
            fetcher: async ct =>
            {
                await Task.Delay(300, ct);
                return new
                {
                    Value = Random.Shared.Next(100, 999),
                    Timestamp = DateTime.Now
                };
            },
            options: new QueryOptions
            {
                RefreshInterval = TimeSpan.FromSeconds(5)
            });

        return Layout.Vertical()
            | Text.Literal($"Value: {liveData.Value?.Value}")
            | Text.Muted($"Updated: {liveData.Value?.Timestamp:HH:mm:ss}")
            | (liveData.Validating ? Text.Muted("Refreshing...") : null!);
    }
}
```

## Pagination

Use `KeepPrevious` to show previous page data while loading the next (managed by [UseState](./03_State.md)):

```csharp demo-below
public class PaginatedView : ViewBase
{
    public override object? Build()
    {
        var page = UseState(1);

        var items = UseQuery(
            key: $"items?page={page.Value}",
            fetcher: async ct =>
            {
                await Task.Delay(800, ct);
                var start = (page.Value - 1) * 5;
                return Enumerable.Range(start + 1, 5)
                    .Select(i => $"Item {i}")
                    .ToList();
            },
            options: new QueryOptions { KeepPrevious = true });

        return Layout.Vertical()
            | items
            | (items.Previous ? Text.Muted("Loading next page...") : null!)
            | Layout.Vertical(items.Value?.Select(Text.Literal) ?? [])
            | (Layout.Horizontal()
                | new Button("Previous", _ => page.Set(p => Math.Max(1, p - 1)))
                    .Disabled(page.Value <= 1)
                    .Variant(ButtonVariant.Primary)
                | new Button("Next", _ => page.Set(p => p + 1))
                    .Variant(ButtonVariant.Primary) 
                | Text.Muted($"Page {page.Value}")
            );
    }
}
```

## Pre-Populated Data

Skip initial fetch when you already have data (e.g., from a list view):

```csharp demo-below
public class ProductListView : ViewBase
{
    public override object? Build()
    {
        var products = UseQuery(
            key: "products",
            fetcher: async ct =>
            {
                await Task.Delay(1000, ct);
                return new[]
                {
                    new Product(1, "Widget", 9.99m),
                    new Product(2, "Gadget", 19.99m)
                };
            });

        if (products.Loading)
            return Text.Literal("Loading...");

        return Layout.Vertical(
            products.Value?.Select(p => new ProductDetailView(p))
        );
    }
}

public record Product(int Id, string Name, decimal Price);

public class ProductDetailView(Product initialProduct) : ViewBase
{
    public override object? Build()
    {
        // Use list data immediately, skip initial fetch
        var product = UseQuery(
            key: $"product/{initialProduct.Id}",
            fetcher: ct => FetchProduct(initialProduct.Id, ct),
            options: new QueryOptions { RevalidateOnMount = false },
            initialValue: initialProduct);

        return new Card(
            Layout.Vertical()
            | Text.Literal(product.Value?.Name ?? "").Bold()
            | Text.Literal($"${product.Value?.Price}")
        ).Small();
    }

    private async Task<Product> FetchProduct(int id, CancellationToken ct)
    {
        await Task.Delay(500, ct);
        return new Product(id, "Updated Name", 29.99m);
    }
}
```

## Error Handling

Errors are captured in the `Error` property:

```csharp demo-below
public class ErrorHandlingView : ViewBase
{
    public override object? Build()
    {
        var query = UseQuery(
            key: "risky-data",
            fetcher: async ct =>
            {
                await Task.Delay(1000, ct);
                if (Random.Shared.NextDouble() > 0.5)
                    throw new Exception("Network error");
                return "Success!";
            });

        if (query.Loading)
            return Text.Literal("Loading...");

        if (query.Error is { } error)
        {
            return Layout.Vertical()
                | Callout.Error(error.Message)
                | new Button("Retry", _ => query.Mutator.Revalidate())
                    .Variant(ButtonVariant.Outline);
        }

        return Layout.Vertical()
            | Text.Literal(query.Value ?? "")
            | new Button("Refresh", _ => query.Mutator.Revalidate())
                .Variant(ButtonVariant.Primary);
    }
}
```