---
searchHints:
  - usememo
  - memoization
  - performance
  - optimization
  - caching
  - hooks
---

# UseMemo

<Ingress>
Cache expensive calculations between re-renders with the UseMemo hook, optimizing performance by only recomputing when dependencies change.
</Ingress>

The `UseMemo` hook lets you cache the result of an expensive calculation so it doesn't need to be recalculated on every render.

## Basic Usage

Cache a computed value that only recalculates when dependencies change:

```csharp demo-below
public class BasicMemoDemo : ViewBase
{
    public override object? Build()
    {
        var count = UseState(0);
        var multiplier = UseState(2);
        
        // Cache the result and only recalculate when dependencies change
        var result = this.UseMemo(() => count.Value * multiplier.Value, count.Value, multiplier.Value);
        
        return Layout.Vertical(
            Text.P($"Result: {result}"),
            new Button($"Count: {count.Value}", _ => count.Set(count.Value + 1)),
            new Button($"Multiplier: {multiplier.Value}", _ => multiplier.Set(multiplier.Value + 1))
        );
    }
}
```

## When to Use UseMemo

### Expensive Calculations

Use UseMemo for computationally expensive operations:

```csharp demo-tabs
public class ExpensiveCalculationDemo : ViewBase
{
    public override object? Build()
    {
        var numbers = UseState(() => Enumerable.Range(1, 1000).ToArray());
        var filter = UseState("");
        
        // This expensive operation only runs when dependencies change
        var processedNumbers = this.UseMemo(() => 
            numbers.Value
                .Where(n => filter.Value == "" || n.ToString().Contains(filter.Value))
                .OrderByDescending(n => n)
                .Take(10)
                .ToArray(), 
            numbers.Value, filter.Value);
        
        return Layout.Vertical(
            filter.ToTextInput().Placeholder("Filter numbers..."),
            Text.P($"Top 10 filtered results: {string.Join(", ", processedNumbers)}")
        );
    }
}
```

### Derived State

Compute derived values from state:

```csharp demo-tabs
public class DerivedStateDemo : ViewBase
{
    public override object? Build()
    {
        var items = UseState(() => new[]
        {
            new { Name = "Apple", Price = 1.50m, Quantity = 3 },
            new { Name = "Banana", Price = 0.75m, Quantity = 5 },
            new { Name = "Orange", Price = 2.00m, Quantity = 2 }
        });
        
        // Only recalculate when items changes
        var total = this.UseMemo(() => items.Value.Sum(item => item.Price * item.Quantity), items.Value);
        var itemCount = items.Value.Length;
        
        return Layout.Vertical(
            items.Value.Select(item => 
                Text.P($"{item.Name}: ${item.Price} x {item.Quantity}")),
            new Separator(),
            Text.P($"Items: {itemCount}"),
            Text.P($"Total: ${total:F2}")
        );
    }
}
```

### Complex Object Transformations

Transform data structures efficiently:

```csharp demo-tabs
public class DataTransformationDemo : ViewBase
{
    public override object? Build()
    {
        var users = UseState(() => new[]
        {
            new { Id = 1, Name = "Alice", Role = "Admin", Active = true },
            new { Id = 2, Name = "Bob", Role = "User", Active = true },
            new { Id = 3, Name = "Charlie", Role = "User", Active = false }
        });
        
        var roleFilter = UseState("All");
        
        // This transformation only runs when dependencies change
        var groupedUsers = this.UseMemo(() =>
        {
            var filtered = roleFilter.Value == "All" 
                ? users.Value 
                : users.Value.Where(u => u.Role == roleFilter.Value);
                
            return filtered
                .GroupBy(u => u.Active)
                .ToDictionary(g => g.Key ? "Active" : "Inactive", g => g.ToList());
        }, users.Value, roleFilter.Value);
        
        return Layout.Vertical(
            roleFilter.ToSelectInput(new[] { "All", "Admin", "User" }.ToOptions()),
            groupedUsers.Select(group =>
                new Card(
                    Layout.Vertical(
                        Text.H4(group.Key),
                        group.Value.Select(u => Text.P($"{u.Name} ({u.Role})"))
                    )
                ).Title("Users")
            )
        );
    }
}
```

## Dependency Management

### Specify All Dependencies

Always include all values used in the memoized function:

```csharp
// Good: All dependencies specified
var result = UseMemo(() => 
{
    return data.Value.Where(x => x.Price > minPrice.Value).ToList();
}, data.Value, minPrice.Value);

// Bad: Missing minPrice dependency
var result = UseMemo(() => 
{
    return data.Value.Where(x => x.Price > minPrice.Value).ToList();
}, data.Value); // Will use stale minPrice!
```

### Avoid Over-Memoization

Don't memoize simple calculations:

```csharp
// Bad: Unnecessary memoization
var doubled = UseMemo(() => count.Value * 2, count.Value);

// Good: Simple calculation, no memoization needed
var doubled = count.Value * 2;
```

## Common Patterns

### Filtering and Sorting Lists

```csharp demo-tabs
public class FilterSortDemo : ViewBase
{
    public override object? Build()
    {
        var products = UseState(() => new[]
        {
            new { Name = "Laptop", Category = "Electronics", Price = 999 },
            new { Name = "Mouse", Category = "Electronics", Price = 29 },
            new { Name = "Desk", Category = "Furniture", Price = 299 },
            new { Name = "Chair", Category = "Furniture", Price = 199 }
        });
        
        var categoryFilter = UseState("All");
        var sortBy = UseState("Name");
        
        // Only recalculate when dependencies change
        var filteredSorted = this.UseMemo(() =>
        {
            var filtered = categoryFilter.Value == "All"
                ? products.Value
                : products.Value.Where(p => p.Category == categoryFilter.Value);
                
            return sortBy.Value switch
            {
                "Price" => filtered.OrderBy(p => p.Price).ToArray(),
                "Name" => filtered.OrderBy(p => p.Name).ToArray(),
                _ => filtered.ToArray()
            };
        }, products.Value, categoryFilter.Value, sortBy.Value);
        
        return Layout.Vertical(
            Layout.Horizontal(
                categoryFilter.ToSelectInput(new[] { "All", "Electronics", "Furniture" }.ToOptions()),
                sortBy.ToSelectInput(new[] { "Name", "Price" }.ToOptions())
            ),
            filteredSorted.Select(p => 
                Text.P($"{p.Name} - ${p.Price} ({p.Category})"))
        );
    }
}
```

### Computing Statistics

```csharp demo-tabs
public class StatisticsDemo : ViewBase
{
    public override object? Build()
    {
        var numbers = UseState(() => new[] { 10, 20, 30, 40, 50 });
        
        // Only recalculate when numbers changes
        var stats = this.UseMemo(() => new
        {
            Count = numbers.Value.Length,
            Sum = numbers.Value.Sum(),
            Average = numbers.Value.Average(),
            Min = numbers.Value.Min(),
            Max = numbers.Value.Max()
        }, numbers.Value);
        
        return new Card(
            Layout.Vertical(
                Text.P($"Count: {stats.Count}"),
                Text.P($"Sum: {stats.Sum}"),
                Text.P($"Average: {stats.Average:F2}"),
                Text.P($"Min: {stats.Min}"),
                Text.P($"Max: {stats.Max}")
            )
        ).Title("Statistics");
    }
}
```

## Best Practices

### 1. Use for Expensive Operations Only

```csharp
// Good: Expensive operation
var filtered = UseMemo(() => 
{
    return largeDataset.Value
        .Where(ComplexFilter)
        .OrderBy(ComplexSort)
        .ToList();
}, largeDataset.Value);

// Bad: Simple operation doesn't need memoization
var doubled = UseMemo(() => count.Value * 2, count.Value);
```

### 2. Keep Memoized Functions Pure

```csharp
// Good: Pure function, no side effects
var result = UseMemo(() => 
{
    return data.Value.Select(x => x * 2).ToList();
}, data.Value);

// Bad: Side effects in memoized function
var result = UseMemo(() => 
{
    Console.WriteLine("Computing..."); // Side effect!
    return data.Value.Select(x => x * 2).ToList();
}, data.Value);
```

## Common Pitfalls

### 1. Forgetting Dependencies

```csharp
// Wrong: multiplier not in dependencies
var result = UseMemo(() => 
{
    return count.Value * multiplier.Value; // Uses multiplier but not in deps!
}, count.Value);

// Correct: All dependencies included
var result = UseMemo(() => 
{
    return count.Value * multiplier.Value;
}, count.Value, multiplier.Value);
```

### 2. Using Mutable Objects as Dependencies

```csharp
// Wrong: Array reference doesn't change when mutated
var items = new[] { 1, 2, 3 };
var result = UseMemo(() => items.Sum(), items); // Won't update if array is mutated

// Correct: Use immutable patterns with state
var items = UseState(() => new[] { 1, 2, 3 });
var result = UseMemo(() => items.Value.Sum(), items.Value);
```

### 3. Over-Memoizing

```csharp
// Wrong: Too much memoization for simple operations
var a = UseMemo(() => x.Value + 1, x.Value);
var b = UseMemo(() => y.Value + 1, y.Value);
var c = UseMemo(() => a + b, a, b);

// Correct: Simple calculations don't need memoization
var result = x.Value + y.Value + 2;
```

## Performance Considerations

UseMemo adds overhead for:
- Storing the cached value
- Comparing dependencies on each render
- Managing the cache lifecycle

Only use it when the calculation cost exceeds this overhead.

```csharp
// When to use UseMemo:
// ✓ Filtering/sorting large arrays (100+ items)
// ✓ Complex mathematical calculations
// ✓ Expensive object transformations
// ✓ Recursive algorithms

// When NOT to use UseMemo:
// ✗ Simple arithmetic (x + y, x * 2)
// ✗ String concatenation
// ✗ Small array operations (< 10 items)
// ✗ Property access
```

## See Also

- [UseState](./03_State.md) - Managing component state
- [UseEffect](./04_Effect.md) - Side effects and lifecycle
- [Memoization](../../01_Onboarding/02_Concepts/10_Memoization.md) - Performance optimization guide
