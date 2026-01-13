# MetricView Refactor (1.2.11)

## Breaking Change

`MetricView` now accepts a `Func<IViewContext, QueryResult<MetricRecord>>` hook instead of `Func<Task<MetricRecord>>`.

## Before

```csharp
new MetricView(
    "Total Sales",
    Icons.DollarSign,
    async () => {
        var data = await FetchData();
        return new MetricRecord("$84,250", 0.21, 0.85, "$100,000");
    }
)
```

## After

```csharp
new MetricView(
    "Total Sales",
    Icons.DollarSign,
    ctx => ctx.UseQuery(
        key: "total-sales",
        fetcher: async ct => {
            var data = await FetchData(ct);
            return new MetricRecord("$84,250", 0.21, 0.85, "$100,000");
        },
        options: new QueryOptions { Expiration = TimeSpan.FromMinutes(5) }
    )
)
```

## Migration Pattern

1. Replace `() =>` or `async () =>` with `ctx =>`
2. Wrap the body in `ctx.UseQuery(key: "unique-key", fetcher: ...)`
3. Pass `CancellationToken` to async operations where possible
4. Add `options: new QueryOptions { Expiration = TimeSpan.FromMinutes(5) }` for caching
5. For date-filtered metrics, use tuple keys: `key: (nameof(ViewName), fromDate, toDate)`

## Static Data Helper

For static/mock data:

```csharp
ctx => ctx.UseQuery(
    key: record,
    fetcher: () => Task.FromResult(record)
)
```
