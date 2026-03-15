namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Database, searchHints: ["query", "swr", "stale", "revalidate", "fetch", "async", "loading"])]
public class QueryApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("UseQuery")
               | Layout.Tabs(
                   new Tab("Basic", new BasicQueryTab()),
                   new Tab("Scoping", new ScopingTab()),
                   new Tab("Expiration", new ExpirationTab()),
                   new Tab("Conditional", new ConditionalTab()),
                   new Tab("Mutations", new MutationsTab()),
                   new Tab("Tags", new TagsTab()),
                   new Tab("Cache", new CacheTab()),
                   new Tab("Polling", new PollingTab()),
                   new Tab("Pagination", new PaginationTab()),
                   new Tab("Pre-Populated", new PrePopulatedTab()),
                   new Tab("Errors", new ErrorsTab()),
                   new Tab("Auto-Key", new AutoKeyTab()),
                   new Tab("Effect Trigger", new EffectTriggerTab())
               ).Variant(TabsVariant.Content);
    }
}

public class BasicQueryTab : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.H2("Basic Usage")
               | Text.P("By default, queries follow the SWR pattern: show cached data immediately, then revalidate in background. Multiple components using the same key share the cached data.")
               | new Card(new BasicQueryExample()).Title("Server Scope - Instance 1")
               | new Card(new BasicQueryExample()).Title("Server Scope - Instance 2")
            ;
    }
}

public class ScopingTab : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.H2("Query Scoping")
               | Text.P("Choose between server-wide shared query or view-isolated query with automatic cleanup.")
               | new Card(new ViewScopedQueryExample()).Title("View Scoped Query")
            ;
    }
}

public class ExpirationTab : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.H2("Expiration (TTL-Based Revalidation)")
               | Text.P("Set an expiration to switch from always-revalidate (default) to TTL-based revalidation. Data is shown immediately and only revalidated after it becomes stale.")
               | new Card(new QueryWithExpirationExample()).Title("Query with Expiration")
            ;
    }
}

public class ConditionalTab : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.H2("Conditional & Dependent Fetching")
               | Text.P("Control when fetching occurs using null keys or key factories for dependent data patterns.")
               | new Card(new ConditionalFetchExample()).Title("Conditional Fetching (Null Key)")
               | new Card(new DependentFetchExample()).Title("Dependent Fetching (Key Factory)")
            ;
    }
}

public class MutationsTab : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.H2("Mutations")
               | Text.P("Optimistically update cached data and trigger revalidation across components.")
               | new Card(new MutationExample()).Title("Optimistic Mutation")
               | new Card(new CrossComponentMutationExample()).Title("Cross-Component Mutation")
            ;
    }
}

public class AutoKeyTab : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.H2("Auto-Keyed Query")
               | Text.P("When no key is specified, the query key is derived from the call site (file and line number).")
               | new Card(new AutoKeyedQueryExample()).Title("Auto-Keyed (No Key Specified)")
            ;
    }
}

public class TagsTab : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.H2("Tag-Based Invalidation")
               | Text.P("Assign tags to queries and invalidate multiple queries at once by tag.")
               | new Card(new TagInvalidationExample()).Title("Invalidate by Tag")
            ;
    }
}

public class TagInvalidationExample : ViewBase
{
    public override object? Build()
    {
        var queryManager = UseService<IQueryService>();

        // Three separate queries, all tagged with "dashboard"
        var usersQuery = UseQuery(
            key: "dashboard/users",
            fetcher: async ct =>
            {
                await Task.Delay(800, ct);
                return $"Users: {Random.Shared.Next(100, 500)}";
            },
            tags: ["dashboard", "users"]);

        var ordersQuery = UseQuery(
            key: "dashboard/orders",
            fetcher: async ct =>
            {
                await Task.Delay(600, ct);
                return $"Orders: {Random.Shared.Next(50, 200)}";
            },
            tags: ["dashboard", "orders"]);

        var revenueQuery = UseQuery(
            key: "dashboard/revenue",
            fetcher: async ct =>
            {
                await Task.Delay(700, ct);
                return $"Revenue: ${Random.Shared.Next(10000, 50000):N0}";
            },
            tags: ["dashboard", "revenue"]);

        return Layout.Vertical().Gap(4)
               | Text.H3("Revalidate by Tag")
               | (Layout.Horizontal().Gap(2)
                  | new Button("Revalidate All (dashboard)", _ => queryManager.RevalidateByTag("dashboard"))
                        .Variant(ButtonVariant.Primary)
                        .Icon(Icons.RefreshCw)
                  | new Button("Revalidate Users", _ => queryManager.RevalidateByTag("users"))
                        .Variant(ButtonVariant.Outline)
                  | new Button("Revalidate Orders", _ => queryManager.RevalidateByTag("orders"))
                        .Variant(ButtonVariant.Outline)
                  | new Button("Revalidate Revenue", _ => queryManager.RevalidateByTag("revenue"))
                        .Variant(ButtonVariant.Outline))
               | Text.H3("Invalidate by Tag")
               | (Layout.Horizontal().Gap(2)
                  | new Button("Invalidate All (dashboard)", _ => queryManager.InvalidateByTag("dashboard"))
                        .Variant(ButtonVariant.Destructive)
                        .Icon(Icons.Trash)
                  | new Button("Invalidate Users", _ => queryManager.InvalidateByTag("users"))
                        .Variant(ButtonVariant.Outline)
                  | new Button("Invalidate Orders", _ => queryManager.InvalidateByTag("orders"))
                        .Variant(ButtonVariant.Outline)
                  | new Button("Invalidate Revenue", _ => queryManager.InvalidateByTag("revenue"))
                        .Variant(ButtonVariant.Outline))
               | Text.H3("Dashboard Data")
               | (Layout.Vertical().Gap(2)
                  | QueryCard("Users", usersQuery)
                  | QueryCard("Orders", ordersQuery)
                  | QueryCard("Revenue", revenueQuery))
               | Text.Muted("Revalidate: keeps current data visible while fetching in background (SWR pattern).")
               | Text.Muted("Invalidate: clears data and shows loading state.")
            ;
    }

    private static object QueryCard(string title, QueryResult<string> query)
    {
        var status = query.Loading ? " (Loading...)"
            : query.Validating ? " (Revalidating...)"
            : "";

        object content = query.Loading
            ? new Skeleton().Height(Size.Units(4))
            : Text.Literal(query.Value ?? "");

        return new Card(content).Title($"{title}{status}").Width(Size.Full());
    }
}

public class ErrorsTab : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.H2("Error Handling")
               | Text.P("When a fetcher throws an exception, the error is captured and returned in the result.")
               | new Card(new ErrorHandlingExample()).Title("Error Handling")
            ;
    }
}

public class ErrorHandlingExample : ViewBase
{
    public override object? Build()
    {
        var shouldFail = UseState(true);

        var query = UseQuery(
            key: $"error-example-{shouldFail.Value}",
            fetcher: async ct =>
            {
                await Task.Delay(1000, ct);
                if (shouldFail.Value)
                {
                    throw new InvalidOperationException("Simulated fetch error: API is unavailable");
                }
                return $"Success at {DateTime.Now:HH:mm:ss}";
            });

        return Layout.Vertical()
               | shouldFail.ToBoolInput().Label("Simulate error")
               | (query.Loading
                   ? Text.Literal("Loading...")
                   : query.Error is { } error
                       ? Callout.Error($"Error: {error.Message}")
                       : Text.Literal(query.Value ?? "No data"))
               | (query.Validating ? Text.Muted("Revalidating...") : null!)
               | (Layout.Horizontal()
                  | new Button("Retry", _ => query.Mutator.Revalidate()).Variant(ButtonVariant.Outline))
            ;
    }
}

public class BasicQueryExample : ViewBase
{
    public override object? Build()
    {
        var query = UseQuery(
            key: "basic-example",
            fetcher: async ct =>
            {
                await Task.Delay(1000, ct);
                return $"Fetched at {DateTime.Now:HH:mm:ss}";
            });

        return Layout.Vertical()
               | query
               | Text.Literal(query.Value ?? "No data")
               | (Layout.Horizontal()
                  | new Button("Revalidate", _ => query.Mutator.Revalidate()).Variant(ButtonVariant.Outline)
                  | new Button("Invalidate", _ => query.Mutator.Invalidate()).Variant(ButtonVariant.Destructive))
            ;
    }
}

public class ViewScopedQueryExample : ViewBase
{
    public override object? Build()
    {
        var query = UseQuery(
            key: "view-scoped-example",
            fetcher: async ct =>
            {
                await Task.Delay(800, ct);
                return $"View data: {DateTime.Now:HH:mm:ss.fff}";
            },
            options: QueryScope.View);

        if (query.Loading)
        {
            return Text.Literal("Loading...");
        }

        return Layout.Vertical()
               | Text.Literal(query.Value ?? "No data")
               | Text.Muted("This query is isolated to this view instance and cleaned up when unmounted.")
            ;

    }
}

public class QueryWithExpirationExample : ViewBase
{
    public override object? Build()
    {
        var query = UseQuery(
            key: "expiring-example",
            fetcher: async ct =>
            {
                await Task.Delay(3000, ct);
                return $"Data fetched at {DateTime.Now:HH:mm:ss}";
            },
            options: new QueryOptions
            {
                Expiration = TimeSpan.FromSeconds(5)
            });

        if (query.Loading)
        {
            return Text.Literal("Loading...");
        }

        return Layout.Vertical()
               | Text.Literal(query.Value ?? "No data")
               | (query.Validating ? Text.Muted("Revalidating in background...") : null!)
               | Text.Muted("Data stays fresh for 5 seconds. After expiration, revisiting triggers background revalidation while showing stale data.")
            ;
    }
}

public class ConditionalFetchExample : ViewBase
{
    public override object? Build()
    {
        var shouldFetch = UseState(false);

        // Conditional fetching: when key is null, UseQuery returns idle (no fetch)
        var query = UseQuery(
            key: shouldFetch.Value ? "conditional-example" : null,
            fetcher: async ct =>
            {
                await Task.Delay(1500, ct);
                return $"Fetched at {DateTime.Now:HH:mm:ss}";
            });

        return Layout.Vertical()
               | shouldFetch.ToBoolInput().Label("Enable fetching")
               | (shouldFetch.Value
                   ? query.Loading
                       ? Text.Literal("Loading...")
                       : Text.Literal(query.Value ?? "No data")
                   : Text.Muted("Fetching disabled (key is null)"))
               | Text.Muted("Toggle to enable/disable fetching. When disabled, no request is made.")
            ;
    }
}

public class DependentFetchExample : ViewBase
{
    public override object? Build()
    {
        // First query: fetch user
        var user = UseQuery(
            key: "dependent-user",
            fetcher: async ct =>
            {
                await Task.Delay(1000, ct);
                return new { Id = 42, Name = "Alice" };
            });

        // Dependent query: only fetches when user is loaded (key factory pattern)
        // Use string key derived from user.Value?.Id
        var projects = UseQuery(
            () => user.Value is { } u ? $"projects/{u.Id}" : null,
            async ct =>
            {
                await Task.Delay(1000, ct);
                return new[] { $"Project A (user {user.Value?.Id})", $"Project B (user {user.Value?.Id})" };
            });

        var projectsList = projects.Value?.Select(p => Text.Literal($"  - {p}")).ToArray() ?? [];

        return Layout.Vertical()
               | Text.Literal("User:")
               | (user.Loading
                   ? Text.Muted("  Loading user...")
                   : Text.Literal($"  {user.Value?.Name} (ID: {user.Value?.Id})"))
               | Text.Literal("Projects:")
               | (user.Loading
                   ? Text.Muted("  Waiting for user...")
                   : projects.Loading
                       ? Text.Muted("  Loading projects...")
                       : projectsList.Aggregate(Layout.Vertical(), (layout, item) => layout | item))
               | (Layout.Horizontal()
                  | new Button("Reload User", _ => user.Mutator.Invalidate()).Variant(ButtonVariant.Outline)
                  | new Button("Reload Projects", _ => projects.Mutator.Invalidate()).Variant(ButtonVariant.Outline))
               | Text.Muted("Projects only fetch after user loads. Reload user to see the cascade.")
            ;
    }
}

public class MutationExample : ViewBase
{
    public override object? Build()
    {
        var query = UseQuery(
            key: "counter-example",
            fetcher: async ct =>
            {
                await Task.Delay(500, ct);
                return Random.Shared.Next(1, 100);
            });

        if (query.Loading)
        {
            return Text.Literal("Loading...");
        }

        return Layout.Vertical()
               | Text.Literal($"Value: {query.Value}")
               | (query.Validating ? Text.Muted("Syncing...") : null!)
               | (Layout.Horizontal()
                  | new Button("Optimistic +10", _ => query.Mutator.Mutate(query.Value + 10, true))
                        .Variant(ButtonVariant.Primary)
                  | new Button("Set to 999 (No Revalidate)", _ => query.Mutator.Mutate(999, false))
                        .Variant(ButtonVariant.Secondary)
                  | new Button("Refresh", _ => query.Mutator.Revalidate())
                        .Variant(ButtonVariant.Outline))
            ;
    }
}

public class CrossComponentMutationExample : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | new SharedQueryDisplay()
               | new SharedQueryControls()
               | Text.Muted("Both components share the same query key. Mutations in one affect the other.")
            ;
    }
}

public class SharedQueryDisplay : ViewBase
{
    public override object? Build()
    {
        var query = UseQuery(
            key: "shared-query",
            fetcher: async ct =>
            {
                await Task.Delay(600, ct);
                return $"Shared data: {Guid.NewGuid().ToString()[..8]}";
            });

        if (query.Loading)
        {
            return Text.Literal("Loading...");
        }

        return Layout.Horizontal()
               | Text.Literal(query.Value ?? "No data")
               | (query.Validating ? Text.Muted("(Updating...)") : null!)
            ;
    }
}

public class SharedQueryControls : ViewBase
{
    public override object? Build()
    {
        // UseMutation - no fetcher, just control the query
        var mutator = UseMutation("shared-query");

        return Layout.Horizontal().Gap(2)
               | new Button("Revalidate Shared Query", _ => mutator.Revalidate())
                     .Variant(ButtonVariant.Outline)
               | new Button("Invalidate", _ => mutator.Invalidate())
                     .Variant(ButtonVariant.Destructive)
            ;
    }
}

public class AutoKeyedQueryExample : ViewBase
{
    public override object? Build()
    {
        // No key specified - uses the call site (file:line) as the query key
        var query = UseQuery(async ct =>
        {
            await Task.Delay(1000, ct);
            return $"Auto-keyed data: {DateTime.Now:HH:mm:ss}";
        });

        if (query.Loading)
        {
            return Text.Literal("Loading...");
        }

        return Layout.Vertical()
               | Text.Literal(query.Value ?? "No data")
               | (query.Validating ? Text.Muted("Revalidating...") : null!)
               | Text.Muted("Query key is derived from the call site (file and line number).")
               | (Layout.Horizontal()
                  | new Button("Revalidate", _ => query.Mutator.Revalidate()).Variant(ButtonVariant.Outline)
                  | new Button("Invalidate", _ => query.Mutator.Invalidate()).Variant(ButtonVariant.Destructive))
            ;
    }
}

public class PrePopulatedTab : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.H2("Pre-Populated (Skip Initial Fetch)")
               | Text.P("Use RevalidateOnInit=false with an initialValue to populate the cache without fetching. Ideal for list-to-detail patterns where detail data comes from the list.")
               | new Card(new ProductListExample()).Title("Product List")
            ;
    }
}

public record Product(int Id, string Name, decimal Price);

public static class ProductDatabase
{
    private static readonly List<Product> _products =
    [
        new(1, "Widget", 9.99m),
        new(2, "Gadget", 19.99m),
        new(3, "Gizmo", 29.99m)
    ];

    public static async Task<List<Product>> ListAsync(CancellationToken ct = default)
    {
        await Task.Delay(1000, ct);
        return _products.ToList();
    }

    public static async Task<Product?> GetAsync(int id, CancellationToken ct = default)
    {
        await Task.Delay(500, ct);
        return _products.FirstOrDefault(p => p.Id == id);
    }

    public static async Task<Product> UpdateAsync(int id, string name, decimal price, CancellationToken ct = default)
    {
        await Task.Delay(500, ct);
        var index = _products.FindIndex(p => p.Id == id);
        if (index == -1) throw new InvalidOperationException($"Product {id} not found");
        var updated = new Product(id, name, price);
        _products[index] = updated;
        return updated;
    }

    public static async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await Task.Delay(500, ct);
        var index = _products.FindIndex(p => p.Id == id);
        if (index == -1) throw new InvalidOperationException($"Product {id} not found");
        _products.RemoveAt(index);
    }
}

public class ProductListExample : ViewBase
{
    public override object? Build()
    {
        var products = UseQuery(
            key: "products",
            fetcher: ProductDatabase.ListAsync);

        if (products.Loading)
        {
            return Text.Literal("Loading products...");
        }

        var productViews = products.Value?.Select(p =>
            new ProductDetailView(p) { Key = $"product-{p.Id}" }
        ).ToArray() ?? [];

        return Layout.Vertical()
               | (Layout.Horizontal()
                  | new Button("Revalidate List", _ => products.Mutator.Revalidate()).Variant(ButtonVariant.Outline)
                  | new Button("Invalidate List", _ => products.Mutator.Invalidate()).Variant(ButtonVariant.Destructive))
               | (products.Validating ? Text.Muted("Refreshing product list...") : null!)
               | productViews
               ;

    }
}

public class ProductDetailView(Product initialProduct) : ViewBase
{
    public override object? Build()
    {
        // RevalidateOnInit=false + initialValue = populate cache, no fetch
        var product = UseQuery(
            key: $"product/{initialProduct.Id}",
            fetcher: ct => ProductDatabase.GetAsync(initialProduct.Id, ct),
            options: new QueryOptions { RevalidateOnMount = false },
            initialValue: initialProduct);

        if (product.Loading)
        {
            return Text.Literal("Loading product...");
        }

        if (product.Value is not { } p)
        {
            return null;
        }

        return Layout.Vertical()
               | Text.H3(p.Name)
               | Text.Literal($"ID: {p.Id}")
               | Text.Literal($"Price: ${p.Price}")
               | (product.Validating ? Text.Muted("Refreshing...") : null!)
               | (Layout.Horizontal()
                  | new Button("Increase Price 10%", async _ =>
                    {
                        var newPrice = Math.Round(p.Price * 1.10m, 2);
                        var updated = await ProductDatabase.UpdateAsync(p.Id, p.Name, newPrice);
                        product.Mutator.Mutate(updated, revalidate: false);
                    }).Variant(ButtonVariant.Primary)
                  | new Button("Delete", async _ =>
                    {
                        await ProductDatabase.DeleteAsync(p.Id);
                        product.Mutator.Mutate(null, revalidate: false);
                    }).Variant(ButtonVariant.Destructive)
                  | new Button("Revalidate", _ => product.Mutator.Revalidate()).Variant(ButtonVariant.Outline))
               | new Separator()
            ;
    }
}

public class PaginationTab : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.H2("Pagination with KeepPreviousData")
               | Text.P("When KeepPreviousData is enabled, the previous page's data remains visible while the next page loads. This prevents UI flicker during pagination.")
               | new Card(new PaginationExample()).Title("Paginated List")
               | new Card(new PaginationComparisonExample()).Title("Comparison: With vs Without KeepPreviousData")
            ;
    }
}

public class PaginationExample : ViewBase
{
    private const int PageSize = 5;
    private const int TotalItems = 23;

    public override object? Build()
    {
        var page = UseState(1);
        var itemsQuery = UseQuery(
            key: $"paginated-items?page={page.Value}",
            fetcher: async ct =>
            {
                // Simulate API delay
                await Task.Delay(800, ct);

                var start = (page.Value - 1) * PageSize;
                return Enumerable.Range(start + 1, Math.Min(PageSize, TotalItems - start))
                    .Select(i => $"Item {i}")
                    .ToList();
            },
            options: new QueryOptions { KeepPrevious = true });

        var totalPages = (int)Math.Ceiling((double)TotalItems / PageSize);

        var itemList = itemsQuery.Value?.Select(item =>
            Layout.Horizontal().Gap(2)
            | new Icon(Icons.FileText)
            | Text.Literal(item)
        ).ToArray() ?? [];

        return Layout.Vertical().Gap(4)
               // Header with status
               | (Layout.Vertical()
                  | Text.H4($"Page {page.Value} of {totalPages}")
                  | itemsQuery)

               | itemList

               // Pagination controls
               | (Layout.Horizontal().Gap(2)
                  | new Button("â† Previous", _ => page.Set(p => p - 1))
                        .Disabled(page.Value <= 1 || itemsQuery.Loading)
                        .Variant(ButtonVariant.Outline)
                  | Text.Literal($"{page.Value} / {totalPages}")
                  | new Button("Next â†’", _ => page.Set(p => p + 1))
                        .Disabled(page.Value >= totalPages || itemsQuery.Loading)
                        .Variant(ButtonVariant.Outline))

               | Text.Muted("Notice how the previous page's items remain visible while the next page loads.")
            ;
    }
}

public class PaginationComparisonExample : ViewBase
{
    public override object? Build()
    {
        var page = UseState(1);

        return Layout.Vertical().Gap(4)
               | (Layout.Horizontal().Gap(2)
                  | new Button("â† Prev", _ => page.Set(p => Math.Max(1, p - 1)))
                        .Variant(ButtonVariant.Outline)
                  | Text.Literal($"Page {page.Value}")
                  | new Button("Next â†’", _ => page.Set(p => Math.Min(5, p + 1)))
                        .Variant(ButtonVariant.Outline))
               | (Layout.Grid(2).Gap(4)
                  | new Card(new ComparisonPanelWithKeepPrevious(page.Value)).Title("With KeepPrevious")
                  | new Card(new ComparisonPanelWithoutKeepPrevious(page.Value)).Title("Without KeepPrevious"))
            ;
    }
}

public class ComparisonPanelWithKeepPrevious(int page) : ViewBase
{
    public override object? Build()
    {
        var itemsQuery = UseQuery(
            key: $"comparison-keep-{page}",
            fetcher: async ct =>
            {
                await Task.Delay(1000, ct);
                return Enumerable.Range((page - 1) * 3 + 1, 3)
                    .Select(i => $"Item {i}")
                    .ToList();
            },
            options: new QueryOptions { KeepPrevious = true });

        var content = itemsQuery.Value?.Select(item => Text.Literal($"â€¢ {item}")).ToArray() ?? [];

        return Layout.Vertical().Gap(2)
               | itemsQuery
               | content
            ;
    }
}

public class ComparisonPanelWithoutKeepPrevious(int page) : ViewBase
{
    public override object? Build()
    {
        var itemsQuery = UseQuery(
            key: $"comparison-no-keep-{page}",
            fetcher: async ct =>
            {
                await Task.Delay(1000, ct);
                return Enumerable.Range((page - 1) * 3 + 1, 3)
                    .Select(i => $"Item {i}")
                    .ToList();
            },
            options: new QueryOptions { KeepPrevious = false });

        var content = itemsQuery.Value?.Select(item => Text.Literal($"â€¢ {item}")).ToArray() ?? [];

        return Layout.Vertical().Gap(2)
               | itemsQuery
               | content;
    }
}

public class CacheTab : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.H2("Cache Management")
               | Text.P("Use QueryManager to clear cache entries - either all at once or selectively by tag.")
               | new Card(new CacheClearExample()).Title("Clear Cache")
               | new Card(new PredicateInvalidationExample()).Title("Predicate-Based Invalidation")
            ;
    }
}

public class PollingTab : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.H2("Polling (Auto-Refresh)")
               | Text.P("Use RefreshInterval to automatically revalidate queries at a specified interval while there are active subscribers.")
               | new Card(new RefreshIntervalExample()).Title("Auto-Refresh Query")
            ;
    }
}

public class CacheClearExample : ViewBase
{
    public override object? Build()
    {
        var queryManager = UseService<IQueryService>();

        // Multiple queries with different tags
        var usersQuery = UseQuery(
            key: "cache-demo/users",
            fetcher: async ct =>
            {
                await Task.Delay(600, ct);
                return $"Users loaded at {DateTime.Now:HH:mm:ss}";
            },
            tags: ["cache-demo", "users"]);

        var ordersQuery = UseQuery(
            key: "cache-demo/orders",
            fetcher: async ct =>
            {
                await Task.Delay(600, ct);
                return $"Orders loaded at {DateTime.Now:HH:mm:ss}";
            },
            tags: ["cache-demo", "orders"]);

        var settingsQuery = UseQuery(
            key: "cache-demo/settings",
            fetcher: async ct =>
            {
                await Task.Delay(600, ct);
                return $"Settings loaded at {DateTime.Now:HH:mm:ss}";
            },
            tags: ["cache-demo", "settings"]);

        return Layout.Vertical().Gap(4)
               | Text.H3("Cached Queries")
               | (Layout.Grid(3).Gap(4)
                  | QueryStatusCard("Users", usersQuery, "users")
                  | QueryStatusCard("Orders", ordersQuery, "orders")
                  | QueryStatusCard("Settings", settingsQuery, "settings"))

               | new Separator()

               | Text.H3("Clear by Tag")
               | Text.Muted("Invalidate specific queries by their tag. Queries with active subscribers will re-fetch.")
               | (Layout.Horizontal().Gap(2)
                  | new Button("Clear Users", _ => queryManager.InvalidateByTag("users"))
                        .Variant(ButtonVariant.Outline).Icon(Icons.Users)
                  | new Button("Clear Orders", _ => queryManager.InvalidateByTag("orders"))
                        .Variant(ButtonVariant.Outline).Icon(Icons.ShoppingCart)
                  | new Button("Clear Settings", _ => queryManager.InvalidateByTag("settings"))
                        .Variant(ButtonVariant.Outline).Icon(Icons.Settings))

               | new Separator()

               | Text.H3("Clear Everything")
               | Text.Muted("Invalidate all queries in the cache. Use with caution in production.")
               | (Layout.Horizontal().Gap(2)
                  | new Button("Clear All (by tag)", _ => queryManager.InvalidateByTag("cache-demo"))
                        .Variant(ButtonVariant.Destructive).Icon(Icons.Trash2)
                  | new Button("Clear Entire Cache", _ => queryManager.Clear())
                        .Variant(ButtonVariant.Destructive).Icon(Icons.Trash))

               | new Separator()

               | Text.H3("Revalidate (Background Refresh)")
               | Text.Muted("Revalidate keeps current data visible while fetching fresh data in the background.")
               | (Layout.Horizontal().Gap(2)
                  | new Button("Revalidate All", _ => queryManager.RevalidateByTag("cache-demo"))
                        .Variant(ButtonVariant.Primary).Icon(Icons.RefreshCw))
            ;
    }

    private static object QueryStatusCard(string title, QueryResult<string> query, string tag)
    {
        var status = query.Loading ? "Loading..."
            : query.Validating ? "Revalidating..."
            : "Ready";

        var badge = query.Loading
            ? new Badge(status).Secondary()
            : query.Validating
                ? new Badge(status).Warning()
                : new Badge(status).Success();

        return new Card(
            Layout.Vertical().Gap(2)
            | badge
            | (query.Loading
                ? new Skeleton().Height(Size.Units(4))
                : Text.Literal(query.Value ?? ""))
            | Text.Muted($"Tag: {tag}")
        ).Title(title);
    }
}

public record ProductKey(string Category, int Id);

public class PredicateInvalidationExample : ViewBase
{
    public override object? Build()
    {
        var queryManager = UseService<IQueryService>();

        // Queries with structured keys
        var electronics1 = UseQuery(
            key: new ProductKey("electronics", 1),
            fetcher: async (key, ct) =>
            {
                await Task.Delay(500, ct);
                return $"{key.Category}/{key.Id}: Laptop - ${Random.Shared.Next(500, 1500)}";
            });

        var electronics2 = UseQuery(
            key: new ProductKey("electronics", 2),
            fetcher: async (key, ct) =>
            {
                await Task.Delay(500, ct);
                return $"{key.Category}/{key.Id}: Phone - ${Random.Shared.Next(200, 800)}";
            });

        var clothing1 = UseQuery(
            key: new ProductKey("clothing", 1),
            fetcher: async (key, ct) =>
            {
                await Task.Delay(500, ct);
                return $"{key.Category}/{key.Id}: Shirt - ${Random.Shared.Next(20, 80)}";
            });

        var clothing2 = UseQuery(
            key: new ProductKey("clothing", 2),
            fetcher: async (key, ct) =>
            {
                await Task.Delay(500, ct);
                return $"{key.Category}/{key.Id}: Pants - ${Random.Shared.Next(30, 100)}";
            });

        return Layout.Vertical().Gap(4)
               | Text.H3("Queries with Structured Keys")
               | Text.Muted("Each query uses a ProductKey(Category, Id) as its key.")
               | (Layout.Grid(2).Gap(4)
                  | ProductCard("Electronics #1", electronics1)
                  | ProductCard("Electronics #2", electronics2)
                  | ProductCard("Clothing #1", clothing1)
                  | ProductCard("Clothing #2", clothing2))

               | new Separator()

               | Text.H3("Invalidate by Predicate")
               | Text.Muted("Use a predicate function to match keys and invalidate matching queries.")
               | (Layout.Horizontal().Gap(2)
                  | new Button("Invalidate Electronics", _ =>
                        queryManager.Invalidate(key => key is ProductKey { Category: "electronics" }))
                        .Variant(ButtonVariant.Destructive).Icon(Icons.Laptop)
                  | new Button("Invalidate Clothing", _ =>
                        queryManager.Invalidate(key => key is ProductKey { Category: "clothing" }))
                        .Variant(ButtonVariant.Destructive).Icon(Icons.Shirt)
                  | new Button("Invalidate Id=1", _ =>
                        queryManager.Invalidate(key => key is ProductKey { Id: 1 }))
                        .Variant(ButtonVariant.Destructive).Icon(Icons.Hash))

               | new Separator()

               | Text.H3("Revalidate by Predicate")
               | Text.Muted("Revalidate keeps current data visible while fetching fresh data in the background.")
               | (Layout.Horizontal().Gap(2)
                  | new Button("Revalidate Electronics", _ =>
                        queryManager.Revalidate(key => key is ProductKey { Category: "electronics" }))
                        .Variant(ButtonVariant.Primary).Icon(Icons.RefreshCw)
                  | new Button("Revalidate All Products", _ =>
                        queryManager.Revalidate(key => key is ProductKey))
                        .Variant(ButtonVariant.Primary).Icon(Icons.RefreshCw))
            ;
    }

    private static object ProductCard(string title, QueryResult<string> query)
    {
        var status = query.Loading ? "Loading..."
            : query.Validating ? "Refreshing..."
            : "Ready";

        var badge = query.Loading
            ? new Badge(status).Secondary()
            : query.Validating
                ? new Badge(status).Warning()
                : new Badge(status).Success();

        return new Card(
            Layout.Vertical().Gap(2)
            | badge
            | (query.Loading
                ? new Skeleton().Height(Size.Units(4))
                : Text.Literal(query.Value ?? ""))
        ).Title(title);
    }
}

public class RefreshIntervalExample : ViewBase
{
    public override object? Build()
    {
        var fetchCount = UseRef(() => 0);

        // Query that auto-refreshes every 5 seconds while this component is mounted
        var liveData = UseQuery(
            key: "live-data",
            fetcher: async ct =>
            {
                await Task.Delay(300, ct);
                fetchCount.Value++;

                return new
                {
                    Value = Random.Shared.Next(100, 999),
                    FetchCount = fetchCount.Value,
                    Timestamp = DateTime.Now
                };
            },
            options: new QueryOptions
            {
                RefreshInterval = TimeSpan.FromSeconds(5)
            });

        var statusBadge = liveData.Loading
            ? new Badge("Loading...").Secondary()
            : liveData.Validating
                ? new Badge("Refreshing...").Warning()
                : new Badge("Live").Success();

        return Layout.Vertical().Gap(4)
               | Text.H3("Auto-Refreshing Query")
               | Text.Muted("This query automatically revalidates every 5 seconds while there are active subscribers.")

               | (Layout.Horizontal().Gap(4)
                  | (Layout.Vertical().Gap(2)
                     | statusBadge
                     | (liveData.Loading
                         ? new Skeleton().Height(Size.Units(8)).Width(Size.Units(32))
                         : (Layout.Vertical()
                            | Text.Literal($"Value: {liveData.Value?.Value}")
                            | Text.Literal($"Fetch count: {liveData.Value?.FetchCount}")
                            | Text.Muted($"Last updated: {liveData.Value?.Timestamp:HH:mm:ss}"))))
                  | (Layout.Vertical().Gap(2)
                     | new Button("Force Refresh", _ => liveData.Mutator.Revalidate())
                           .Variant(ButtonVariant.Outline).Icon(Icons.RefreshCw)))

               | Text.Muted("The refresh timer only runs while there are active subscribers. Navigate away to stop polling.")
            ;
    }
}

public class EffectTriggerTab : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.H2("Query as Effect Trigger")
               | Text.P("QueryResult implements IEffectTriggerConvertible, so you can pass it directly to UseEffect to react to query state changes.")
               | new Card(new QueryEffectTriggerExample()).Title("UseEffect with QueryResult")
            ;
    }
}

public class QueryEffectTriggerExample : ViewBase
{
    public override object? Build()
    {
        var effectLog = UseState(() => new List<string>());

        var query = UseQuery(
            key: "effect-trigger-example",
            fetcher: async ct =>
            {
                await Task.Delay(1000, ct);
                return $"Fetched at {DateTime.Now:HH:mm:ss}";
            });

        // Use the query as an effect trigger — fires whenever the query state changes
        UseEffect(() =>
        {
            effectLog.Set(log => [.. log, $"[{DateTime.Now:HH:mm:ss.fff}] Query changed → Loading={query.Loading}, Value={query.Value ?? "(null)"}"]);
        }, query);

        var logEntries = effectLog.Value
            .TakeLast(10)
            .Select(entry => Text.Muted(entry))
            .ToArray();

        return Layout.Vertical().Gap(4)
               | Text.H3("Query State")
               | query
               | Text.Literal(query.Value ?? "No data")
               | (Layout.Horizontal().Gap(2)
                  | new Button("Revalidate", _ => query.Mutator.Revalidate()).Variant(ButtonVariant.Outline)
                  | new Button("Invalidate", _ => query.Mutator.Invalidate()).Variant(ButtonVariant.Destructive)
                  | new Button("Clear Log", _ => effectLog.Set([])).Variant(ButtonVariant.Ghost))
               | Text.H3("Effect Log (last 10)")
               | Text.Muted("Each entry below was logged by a UseEffect triggered by the query:")
               | logEntries
            ;
    }
}
