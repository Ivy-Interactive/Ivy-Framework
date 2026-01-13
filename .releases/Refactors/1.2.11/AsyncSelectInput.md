# AsyncSelectInput Breaking Change - v1.2.11

## Summary

The `AsyncSelectQueryDelegate<T>` and `AsyncSelectLookupDelegate<T>` signatures changed from Task-based async to UseQuery hook-based pattern. Additionally, `AsyncSelectQueryDelegate` was renamed to `AsyncSelectSearchDelegate`.

## What Changed

### Before (v1.2.10 and earlier)
```csharp
public delegate Task<Option<T>[]> AsyncSelectQueryDelegate<T>(string query);
public delegate Task<Option<T>?> AsyncSelectLookupDelegate<T>(T id);
```

### After (v1.2.11+)
```csharp
public delegate QueryResult<Option<T>[]> AsyncSelectSearchDelegate<T>(IViewContext context, string query);
public delegate QueryResult<Option<T>?> AsyncSelectLookupDelegate<T>(IViewContext context, T id);
```

> **Note:** `AsyncSelectQueryDelegate` was renamed to `AsyncSelectSearchDelegate` to match the `Use{Entity}Search` naming convention.

## How to Find Affected Code

Run a `dotnet build`. 

Or search for these patterns in the codebase:

### Pattern 1: Task-based query delegates
```regex
Task<Option<\w+>\[\]>\s+\w+\s*\(\s*string\s+\w+\s*\)
```

### Pattern 2: Task-based lookup delegates
```regex
Task<Option<\w+>\?>\s+\w+\s*\(\s*\w+\s+\w+\s*\)
```

### Pattern 3: ToAsyncSelectInput usage
Search for `.ToAsyncSelectInput(` and verify the delegates passed match the new signature.

### Pattern 4: AsyncSelectInputView constructor
Search for `new AsyncSelectInputView<` and verify the delegates match the new signature.

### Pattern 5: Factory methods returning delegates
Search for methods returning `AsyncSelectQueryDelegate<`, `AsyncSelectSearchDelegate<`, or `AsyncSelectLookupDelegate<`.

## How to Refactor

### Recommended Pattern: Direct Static Methods

The simplest and recommended pattern is to create static methods that directly match the delegate signatures. Services are retrieved via `context.UseService<>()` inside the method.

#### Naming Convention
- Search/Query methods: `Use{Entity}Search`
- Lookup methods: `Use{Entity}Lookup`

#### Example: Database Query Methods

**Before (v1.2.10):**
```csharp
// Old delegate type name was AsyncSelectQueryDelegate
public static AsyncSelectQueryDelegate<Guid?> QueryCategories(SampleDbContextFactory factory)
{
    return async (query) =>
    {
        await using var db = factory.CreateDbContext();
        return (await db.Categories
                .Where(e => e.Name.Contains(query))
                .Select(e => new { e.Id, e.Name })
                .Take(50)
                .ToArrayAsync())
            .Select(e => new Option<Guid?>(e.Name, e.Id))
            .ToArray();
    };
}

public static AsyncSelectLookupDelegate<Guid?> LookupCategory(SampleDbContextFactory factory)
{
    return async (id) =>
    {
        if (id == null) return null;
        await using var db = factory.CreateDbContext();
        var category = await db.Categories.FindAsync([id]);
        if (category == null) return null;
        return new Option<Guid?>(category.Name, category.Id);
    };
}

// Usage:
.Builder(e => e.CategoryId, e => e.ToAsyncSelectInput(
    ProductHelpers.QueryCategories(factory),
    ProductHelpers.LookupCategory(factory),
    placeholder: "Select Category"))
```

**After (v1.2.11+):**
```csharp
// Method signature matches AsyncSelectSearchDelegate<Guid?> (renamed from AsyncSelectQueryDelegate)
public static QueryResult<Option<Guid?>[]> UseCategorySearch(IViewContext context, string query)
{
    var factory = context.UseService<SampleDbContextFactory>();
    return context.UseQuery(
        key: (nameof(UseCategorySearch), query),
        fetcher: async ct =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Categories
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync(ct))
                .Select(e => new Option<Guid?>(e.Name, e.Id))
                .ToArray();
        });
}

// Method signature matches AsyncSelectLookupDelegate<Guid?>
public static QueryResult<Option<Guid?>?> UseCategoryLookup(IViewContext context, Guid? id)
{
    var factory = context.UseService<SampleDbContextFactory>();
    return context.UseQuery(
        key: (nameof(UseCategoryLookup), id),
        fetcher: async ct =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var category = await db.Categories.FindAsync([id], ct);
            if (category == null) return null;
            return new Option<Guid?>(category.Name, category.Id);
        });
}

// Usage (simpler - no factory parameter needed):
.Builder(e => e.CategoryId, e => e.ToAsyncSelectInput(
    ProductHelpers.UseCategorySearch,
    ProductHelpers.UseCategoryLookup,
    placeholder: "Select Category"))
```

### Alternative Pattern: Local Functions

For simple in-component usage with synchronous data:

**Before:**
```csharp
Task<Option<string>[]> QueryCategories(string query)
{
    return Task.FromResult(Categories
        .Where(c => c.Contains(query, StringComparison.OrdinalIgnoreCase))
        .Select(c => new Option<string>(c))
        .ToArray());
}

Task<Option<string>?> LookupCategory(string? category)
{
    return Task.FromResult(category != null ? new Option<string>(category) : null);
}
```

**After:**
```csharp
QueryResult<Option<string>[]> QueryCategories(IViewContext context, string query)
{
    return context.UseQuery<Option<string>[], (string, string)>(
        key: (nameof(QueryCategories), query),
        fetcher: ct => Task.FromResult(Categories
            .Where(c => c.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Select(c => new Option<string>(c))
            .ToArray()));
}

QueryResult<Option<string>?> LookupCategory(IViewContext context, string? category)
{
    return context.UseQuery<Option<string>?, (string, string?)>(
        key: (nameof(LookupCategory), category),
        fetcher: ct => Task.FromResult(category != null ? new Option<string>(category) : null));
}
```

### Update AsyncSelectInputView Constructor Calls

If using `AsyncSelectInputView<T>` directly, the `onChange` parameter changed from `Action<Event<T>>` to `Func<Event<T>, ValueTask>`.

**Before:**
```csharp
new AsyncSelectInputView<Guid>(
    selectedUser.Value,
    e => selectedUser.Set(e.Value),
    QueryUsers,
    LookupUser,
    placeholder: "Search..."
);
```

**After:**
```csharp
new AsyncSelectInputView<Guid>(
    selectedUser.Value,
    e => { selectedUser.Set(e.Value); return ValueTask.CompletedTask; },
    QueryUsers,
    LookupUser,
    placeholder: "Search..."
);
```

## Key Refactoring Rules

1. **Return type**: Change `Task<Option<T>[]>` to `QueryResult<Option<T>[]>` and `Task<Option<T>?>` to `QueryResult<Option<T>?>`

2. **First parameter**: Add `IViewContext context` as the first parameter

3. **Get services from context**: Use `context.UseService<T>()` to get dependencies instead of passing them as parameters

4. **UseQuery call**: Wrap the body in `context.UseQuery(key: ..., fetcher: ...)`

5. **Key tuple**: Use a tuple with a unique identifier (typically `nameof(MethodName)`) and the query/id parameter: `(nameof(UseCategorySearch), query)`

6. **Type inference**: When using `context.UseService<>()` inside the method and explicit return type, type arguments can often be inferred. If not, use `context.UseQuery<TValue, TKey>(...)`

7. **Fetcher signature**: The fetcher takes a single `CancellationToken` parameter: `fetcher: async ct => { ... }`

8. **Pass CancellationToken**: Pass the `ct` to async database calls like `ToArrayAsync(ct)`, `FindAsync([id], ct)`, etc.

9. **Naming convention**: Use `Use{Entity}Search` for query methods and `Use{Entity}Lookup` for lookup methods

## Common Mistakes to Avoid

1. **Missing IViewContext**: The first parameter must be `IViewContext context`

2. **Passing factory as parameter**: Don't pass dependencies like `SampleDbContextFactory` as parameters. Use `context.UseService<T>()` instead

3. **Using old fetcher signature**: Change `fetcher: async (_, ct) =>` to `fetcher: async ct =>` (single parameter)

4. **Missing explicit type arguments**: If type inference fails with CS0411, add explicit type arguments: `context.UseQuery<TValue, TKey>(...)`

5. **Wrong key type**: The key type must match the actual tuple type used in the `key:` parameter

6. **Forgetting CancellationToken**: Pass `ct` to all async database operations

## Adding Required Using

Ensure the file has:
```csharp
using Ivy.Hooks;
```

## Verification

After refactoring, run:
```bash
dotnet build
```

All usages of `ToAsyncSelectInput` and `AsyncSelectInputView` should compile without errors.

## Benefits of New Pattern

1. **Simpler usage**: No need to pass factory/dependencies at call site
2. **Automatic caching**: UseQuery provides built-in caching and deduplication
3. **Loading states**: QueryResult includes `IsLoading` for UI feedback
4. **Consistent pattern**: Matches other hook-based patterns in the framework
5. **Better testability**: Dependencies resolved via context are easier to mock
