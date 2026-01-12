---
searchHints:
  - performance
  - optimization
  - caching
  - usememo
  - usecallback
  - rendering
  - memo
---

# Memoization

<Ingress>
Memoization helps Ivy [applications](../../../01_Onboarding/02_Concepts/15_Apps.md) run faster by caching results of expensive computations and preventing unnecessary re-renders in your [views](../../../01_Onboarding/02_Concepts/02_Views.md).
</Ingress>

## Overview

Memoization in Ivy provides several powerful tools to optimize performance:

- **[`UseMemo`](#usememo-hook)** - Caches the result of expensive computations
- **[`UseCallback`](./06_Callback.md)** - Memoizes callback functions to prevent unnecessary re-renders.
- **`IMemoized`** - Interface for component-level memoization

These [hooks](../02_RulesOfHooks.md) work similarly to their React counterparts (`useMemo`, `useCallback`) but are designed specifically for Ivy's architecture.

## Choosing the Right Memoization Approach

```mermaid
flowchart TD
    A["Need to optimize performance?"] --> B{What are you optimizing?}
    
    B --> C["Expensive computation<br/>or data transformation"]
    B --> D["Callback function<br/>passed to child components"]
    B --> E["Entire component<br/>with expensive rendering"]
    
    C --> F["Use UseMemo"]
    D --> G["Use UseCallback"]
    E --> H["Implement IMemoized"]
    
    F --> I["Cache computed values<br/>• Data filtering<br/>• Complex calculations<br/>• Derived state"]
    G --> J["Prevent child re-renders<br/>• Event handlers<br/>• Stable function references<br/>• Effect dependencies"]
    H --> K["Component-level optimization<br/>• List items<br/>• Heavy UI components<br/>• Custom widgets"]
```

## UseMemo Hook

The `UseMemo` [hook](../02_RulesOfHooks.md) caches the result of a computation and only recomputes it when its [state](./03_State.md) dependencies change.

<Callout type="Tip">
`UseMemo` hook stores only the most recent dependency values for comparison; older values are discarded.
</Callout>

### How UseMemo Works

```mermaid
sequenceDiagram
    participant C as Component
    participant M as UseMemo Hook
    participant S as UseState Storage
    
    Note over C,S: First Render
    C->>M: UseMemo(() => expensiveCalc(), [dep1, dep2])
    M->>S: UseState(() => new MemoRef(result, deps))
    S-->>M: Create new MemoRef with initial value
    M->>M: Execute factory function
    M->>S: Store MemoRef(result, [dep1, dep2])
    M-->>C: Return computed value
    
    Note over C,S: Subsequent Render (deps unchanged)
    C->>M: UseMemo(() => expensiveCalc(), [dep1, dep2])
    M->>S: Get stored MemoRef
    S-->>M: Return MemoRef(cachedResult, [dep1, dep2])
    M->>M: AreDependenciesEqual([dep1, dep2], [dep1, dep2])
    Note right of M: Dependencies equal!<br/>Skip computation
    M-->>C: Return cached value (no computation)
    
    Note over C,S: Subsequent Render (deps changed)
    C->>M: UseMemo(() => expensiveCalc(), [dep1_new, dep2])
    M->>S: Get stored MemoRef
    S-->>M: Return MemoRef(oldResult, [dep1, dep2])
    M->>M: AreDependenciesEqual([dep1, dep2], [dep1_new, dep2])
    Note right of M: Dependencies changed!<br/>Need recomputation
    M->>M: Execute factory function
    M->>S: Update MemoRef(newResult, [dep1_new, dep2])
    M-->>C: Return new computed value
```

### Basic Usage

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
        }, input.Value); // Only recompute when input changes
        
        return Layout.Vertical(
            Text.Inline("Number", value: input.Value, onChange: v => input.Set(v)),
            Text.Inline($"Result: {result}")
        );
    }
}
```

### When to Use Memoization

Use memoization when:

- You have expensive computations that don't need to be redone on every render
- You want to prevent unnecessary re-renders of [child components](../../../01_Onboarding/02_Concepts/03_Widgets.md)
- You're dealing with complex data transformations that depend on [state](./03_State.md) changes
- You need stable function references for [`UseEffect`](./04_Effect.md) dependencies

### Best Practices

- **Dependency Array**: Always specify the [state](./03_State.md) dependencies that should trigger a recomputation
- **Expensive Operations**: Only memoize truly expensive operations
- **Clean Dependencies**: Keep the dependency array minimal and focused on state values
- **Avoid Side Effects**: Memoized functions should be pure and not have side effects (use [UseEffect](./04_Effect.md) for side effects)

### Examples

#### Complex Data Filtering

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

#### Computed Properties

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

## Component Memoization with IMemoized

The `IMemoized` interface allows entire [components](../../../01_Onboarding/02_Concepts/02_Views.md) to be memoized, preventing re-renders when their props haven't changed. This is useful for optimizing [views](../../../01_Onboarding/02_Concepts/02_Views.md) with expensive rendering logic.

### How IMemoized Works

```mermaid
sequenceDiagram
    participant P as Parent Component
    participant WT as WidgetTree
    participant C as IMemoized Component
    participant H as Hash Calculator
    
    Note over P,H: First Render
    P->>WT: Render child component
    WT->>C: Check if implements IMemoized
    C-->>WT: Yes, implements IMemoized
    WT->>C: Call GetMemoValues()
    C-->>WT: Return [prop1, prop2, prop3]
    WT->>H: CalculateMemoizedHashCode(viewId, memoValues)
    H->>H: Hash viewId + each prop (string/valuetype/json)
    H-->>WT: Return computed hash
    WT->>C: Call Build() - component renders
    WT->>WT: Store TreeNode with memoizedHashCode
    
    Note over P,H: Subsequent Render (props unchanged)
    P->>WT: Render child component
    WT->>C: Call GetMemoValues()
    C-->>WT: Return [prop1, prop2, prop3] (same values)
    WT->>H: CalculateMemoizedHashCode(viewId, memoValues)
    H-->>WT: Return same hash
    WT->>WT: Compare: previousHash == currentHash
    Note right of WT: Hash matches!<br/>Skip Build() call
    WT-->>P: Reuse previous TreeNode (no re-render)
    
    Note over P,H: Subsequent Render (props changed)
    P->>WT: Render child component
    WT->>C: Call GetMemoValues()
    C-->>WT: Return [prop1_new, prop2, prop3] (changed values)
    WT->>H: CalculateMemoizedHashCode(viewId, memoValues)
    H-->>WT: Return different hash
    WT->>WT: Compare: previousHash != currentHash
    Note right of WT: Hash changed!<br/>Need to re-render
    WT->>C: Call Build() - component re-renders
    WT->>WT: Update TreeNode with new hash
```

### Basic Implementation

```csharp
public class ExpensiveComponent : ViewBase, IMemoized
{
    private readonly string _title;
    private readonly int _value;
    private readonly DateTime _timestamp;
    
    public ExpensiveComponent(string title, int value, DateTime timestamp)
    {
        _title = title;
        _value = value;
        _timestamp = timestamp;
    }
    
    // Define which values should trigger a re-render
    public object[] GetMemoValues()
    {
        return [_title, _value]; // Exclude _timestamp from memoization
    }
    
    public override object? Build()
    {
        // Expensive rendering logic here
        Thread.Sleep(100); // Simulate expensive operation
        
        return Layout.Vertical(
            Text.Heading(_title),
            Text.Block($"Value: {_value}"),
            Text.Small($"Rendered at: {DateTime.Now}")
        );
    }
}
```

### Real-World Example: List Items

```csharp
public class ProductListView : ViewBase
{
    public override object? Build()
    {
        var products = UseState(GetProducts());
        var sortBy = UseState("name");
        
        var sortedProducts = UseMemo(() => 
            products.Value.OrderBy(p => sortBy.Value switch
            {
                "name" => p.Name,
                "price" => p.Price.ToString(),
                _ => p.Id.ToString()
            }).ToList(),
            products, sortBy
        );
        
        return Layout.Vertical(
            new Select("Sort by", sortBy.Value, 
                new[] { "name", "price", "id" }, 
                v => sortBy.Set(v)
            ),
            Layout.Vertical(
                sortedProducts.Select((product, index) => 
                    new ProductItem(product, index).Key(product.Id)
                )
            )
        );
    }
}

public class ProductItem : ViewBase, IMemoized
{
    private readonly Product _product;
    private readonly int _index;
    
    public ProductItem(Product product, int index)
    {
        _product = product;
        _index = index;
    }
    
    public object[] GetMemoValues()
    {
        // Only re-render if product data or position changes
        return [_product.Id, _product.Name, _product.Price, _index];
    }
    
    public override object? Build()
    {
        // This component will only re-render when memoized values change
        return new Card(
            Layout.Horizontal(
                new Avatar(_product.ImageUrl),
                Layout.Vertical(
                    Text.Heading(_product.Name),
                    Text.Block($"${_product.Price:N2}"),
                    Text.Small($"Position: {_index + 1}")
                )
            )
        );
    }
}
```

### IMemoized Component Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Created: Component instantiated
    Created --> CheckMemo: Parent renders
    
    CheckMemo --> GetValues: Call GetMemoValues()
    GetValues --> Compare: Compare with cached values
    
    Compare --> CacheHit: Values unchanged
    Compare --> CacheMiss: Values changed
    
    CacheHit --> SkipRender: Use cached result
    CacheMiss --> ExecuteBuild: Call Build() method
    
    ExecuteBuild --> UpdateCache: Store new result
    UpdateCache --> Rendered: Component updated
    
    SkipRender --> [*]: No re-render needed
    Rendered --> [*]: Component rendered
    
    Rendered --> CheckMemo: Next parent render
    SkipRender --> CheckMemo: Next parent render
```

### Best Practices for IMemoized

- **Include all relevant props** - Any value that affects rendering should be in `GetMemoValues()`
- **Exclude volatile values** - Don't include timestamps or random values unless they affect the UI
- **Use with .Key()** - Always provide a stable key when rendering memoized components in lists
- **Keep it simple** - Only memoize components with expensive rendering logic

## Performance Considerations

### Memory vs Speed Trade-offs

- **Memory Usage**: Memoization caches values in memory. Consider the size of cached [state](./03_State.md) data:

- **Cache Invalidation**: If state dependencies change too often or are unstable, cached results will be invalidated frequently, reducing the effectiveness of memoization. Ensure dependencies are stable and don't change unnecessarily:

- **Dependency Granularity**: Use specific state dependencies rather than entire objects:

```csharp
// Good: Small computed value
var total = UseMemo(() => items.Value.Sum(x => x.Price), items);

// Caution: Large object that might consume significant memory
var processedData = UseMemo(() => 
    items.Value.Select(ProcessLargeItem).ToList(), items);
```

```csharp
// Bad: Object created on every render
var config = new { threshold: 100 };
var filtered = UseMemo(() => FilterItems(items.Value, config), items, config);

// Good: Stable dependency
var threshold = UseState(100);
var filtered = UseMemo(() => 
    FilterItems(items.Value, threshold.Value), items, threshold);
```

```csharp
// Less efficient: Depends on entire user object
var greeting = UseMemo(() => $"Hello, {user.Value.Name}!", user);

// More efficient: Depends only on the name
var userName = user.Value.Name;
var greeting = UseMemo(() => $"Hello, {userName}!", userName);
```

### When NOT to Memoize

- **Simple computations**: Don't memoize trivial operations
- **Frequently changing [state](./03_State.md) dependencies**: If state dependencies change often, memoization provides no benefit
- **Small component trees**: In simple [views](../../../01_Onboarding/02_Concepts/02_Views.md), the overhead might outweigh benefits

```csharp
// Unnecessary memoization
var doubled = UseMemo(() => count.Value * 2, count);

// Just compute directly
var doubled = count.Value * 2;
```

## Common Pitfalls and Solutions

### Memoization Troubleshooting Guide

```mermaid
flowchart TD
    A["Memoization not working as expected?"] --> B{Check your implementation}
    
    B --> C["Dependencies changing unexpectedly?"]
    B --> D["Performance not improving?"]
    B --> E["Memory usage too high?"]
    B --> F["Components still re-rendering?"]
    
    C --> C1["Use stable references<br/>Avoid creating objects in deps<br/>Use [UseStatic](./08_Static.md) for constants"]
    D --> D1["Profile before optimizing<br/>Only memoize expensive operations<br/> Check if deps change frequently"]
    E --> E1["Reduce cached data size<br/>Use specific dependencies<br/> Consider conditional memoization"]
    F --> F1["Implement IMemoized correctly<br/>Provide stable keys<br/> Check GetMemoValues()"]
    
    C1 --> G["Problem solved?"]
    D1 --> G
    E1 --> G
    F1 --> G
    
    G -->|Yes| H["Great! Your memoization is working"]
    G -->|No| I["Consider alternative approaches<br/>or seek help in community"]
```

### 1. Unstable Dependencies

**Problem**: Creating new objects or arrays in the dependency array

```csharp
// Bad: New array created on every render
var result = UseMemo(() => ProcessData(data.Value), data.Value, new[] { "option1", "option2" });
```

**Solution**: Use stable references with [UseStatic](./08_Static.md)

```csharp
// Good: Stable dependency
var options = UseStatic(new[] { "option1", "option2" });
var result = UseMemo(() => ProcessData(data.Value), data.Value, options);
```

### 2. Missing Dependencies

**Problem**: Not including all values used in the memoized function

```csharp
// Bad: Missing 'multiplier' dependency
var result = UseMemo(() => items.Value.Sum(x => x.Price * multiplier.Value), items);
```

**Solution**: Include all dependencies

```csharp
// Good: All dependencies included
var result = UseMemo(() => items.Value.Sum(x => x.Price * multiplier.Value), items, multiplier);
```

### 3. Over-Memoization

**Problem**: Memoizing everything without considering the cost

```csharp
// Bad: Unnecessary memoization of simple operations
var isEven = UseMemo(() => count.Value % 2 == 0, count);
var doubled = UseMemo(() => count.Value * 2, count);
var greeting = UseMemo(() => $"Hello {name.Value}", name);
```

**Solution**: Only memoize expensive operations

```csharp
// Good: Direct computation for simple operations
var isEven = count.Value % 2 == 0;
var doubled = count.Value * 2;
var greeting = $"Hello {name.Value}";

// Memoize only when necessary
var expensiveResult = UseMemo(() => 
    items.Value.Where(x => x.IsActive)
             .GroupBy(x => x.Category)
             .ToDictionary(g => g.Key, g => g.ToList()), 
    items);
```

### 4. Incorrect IMemoized Implementation

**Problem**: Including volatile values in `GetMemoValues()`

```csharp
// Bad: Including timestamp that changes on every render
public object[] GetMemoValues()
{
    return [_id, _name, DateTime.Now]; // DateTime.Now always changes!
}
```

**Solution**: Only include values that affect the UI

```csharp
// Good: Only include relevant, stable values
public object[] GetMemoValues()
{
    return [_id, _name, _isActive]; // Only UI-affecting properties
}
```

### 5. Callback Dependencies Issues

**Problem**: [UseCallback](./06_Callback.md) callbacks that capture too many state variables

```csharp
// Bad: Callback recreated whenever any state changes
var handleClick = UseCallback(() => 
{
    DoSomething(data.Value, filter.Value, sortOrder.Value);
}, data, filter, sortOrder); // Too many dependencies
```

**Solution**: Split into smaller, focused callbacks using [UseCallback](./06_Callback.md)

```csharp
// Good: Separate callbacks with minimal dependencies
var handleDataAction = UseCallback(() => DoSomethingWithData(data.Value), data);
var handleFilterAction = UseCallback(() => ApplyFilter(filter.Value), filter);
```

### 6. Forgetting Keys in Lists

**Problem**: Not providing stable keys for memoized list items

```csharp
// Bad: No keys, memoization won't work properly
return Layout.Vertical(
    items.Value.Select(item => new ItemComponent(item)) // Missing .Key()
);
```

**Solution**: Always provide stable keys

```csharp
// Good: Stable keys enable proper memoization
return Layout.Vertical(
    items.Value.Select(item => new ItemComponent(item).Key(item.Id))
);
```

## See Also

- [State Management](./03_State.md) - Managing component state
- [UseCallback](./06_Callback.md) - Memoizing callback functions
- [Effects](./04_Effect.md) - Performing side effects with dependencies
- [Rules of Hooks](../02_RulesOfHooks.md) - Understanding hook rules and best practices
- [UseStatic](./08_Static.md) - Storing stable references
- [Signals](../../../01_Onboarding/02_Concepts/06_Signals.md) - Reactive state management
- [Views](../../../01_Onboarding/02_Concepts/02_Views.md) - Understanding Ivy views and components
