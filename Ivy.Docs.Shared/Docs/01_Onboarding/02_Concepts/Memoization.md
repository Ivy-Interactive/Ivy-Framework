# Memoization

<Ingress>
Memoization helps Ivy applications run faster by caching results of expensive computations.
</Ingress>

## Overview

Memoization in Ivy is implemented through the `UseMemo` hook, which caches the result of a computation and only recomputes it when its dependencies change. This is similar to React's `useMemo` hook.

## Basic Usage

Here's a simple example of using memoization:

```csharp
public class ExpensiveCalculationView : ViewBase
{
    public override object? Build()
    {
        var input = UseState(0);
        
        // Memoize the result of an expensive calculation
        var result = UseMemo(() => 
        {
            // Simulate expensive calculation
            Thread.Sleep(1000);
            return input.Value * input.Value;
        }, input); // Only recompute when input changes
        
        return Layout.Vertical(
            Text.Inline("Number", value: input.Value, onChange: v => input.Set(v)),
            Text.Inline($"Result: {result}")
        );
    }
}
```

### When to Use Memoization

Use memoization when:

1. You have expensive computations that don't need to be redone on every render
2. You want to prevent unnecessary re-renders of child components
3. You're dealing with complex data transformations

### Best Practices

1. **Dependency Array**: Always specify the dependencies that should trigger a recomputation
2. **Expensive Operations**: Only memoize truly expensive operations
3. **Clean Dependencies**: Keep the dependency array minimal and focused
4. **Avoid Side Effects**: Memoized functions should be pure and not have side effects

## Examples

### Complex Data Filtering

```csharp
public class DataFilterView : ViewBase
{
    public override object? Build()
    {
        var data = UseState(new List<Item>());
        var filter = UseState("");
        
        // Memoize filtered results
        var filteredData = UseMemo(() => 
            data.Value.Where(item => 
                item.Name.Contains(filter.Value, StringComparison.OrdinalIgnoreCase)
            ).ToList(),
            data, filter
        );
        
        return Layout.Vertical(
            new TextInput("Filter", value: filter.Value, onChange: v => filter.Set(v)),
            new Table(filteredData)
        );
    }
}
```

### Computed Properties

```csharp
public class DashboardView : ViewBase
{
    public override object? Build()
    {
        var sales = UseState(new List<Sale>());
        
        // Memoize computed statistics
        var stats = UseMemo(() => new
        {
            Total = sales.Value.Sum(s => s.Amount),
            Average = sales.Value.Average(s => s.Amount),
            Count = sales.Value.Count
        }, sales);
        
        return Layout.Vertical(
            Text.Inline($"Total Sales: ${stats.Total:N2}"),
            Text.Inline($"Average Sale: ${stats.Average:N2}"),
            Text.Inline($"Number of Sales: {stats.Count}")
        );
    }
}
```

## Performance Considerations

1. **Memory Usage**: Memoization trades memory for speed. Be mindful of the size of cached values
2. **Dependency Changes**: Ensure dependencies are stable and don't change unnecessarily
3. **Cleanup**: Ivy automatically handles cleanup of memoized values when components are disposed

## See Also

- [State Management](./State.md)
- [Effects](./Effects.md)
- [Signals](./Signals.md)