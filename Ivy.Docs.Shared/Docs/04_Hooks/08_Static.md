---
searchHints:
  - usestatic
  - static
  - ref
  - mutable
  - persistence
  - hooks
---

# UseStatic

<Ingress>
Store values that persist across re-renders without triggering updates, similar to React's useRef for holding mutable values that don't affect the view lifecycle.
</Ingress>

The `UseStatic` hook lets you store a value that is initialized only once and persists across re-renders. Unlike UseState, changing a static value does NOT trigger a re-render.

## Basic Usage

Store a value that persists but doesn't trigger re-renders:

```csharp demo-below
public class BasicStaticDemo : ViewBase
{
    class Counter { public int Value = 0; }
    
    public override object? Build()
    {
        // In real code: var renderCount = UseStatic(() => new Counter());
        // For demo purposes, showing the concept:
        var renderCount = new Counter { Value = 1 };
        var forceUpdate = UseState(0);
        
        // In real code, this would increment without triggering re-render:
        // renderCount.Value++;
        
        return Layout.Vertical(
            Text.P($"This component has rendered {renderCount.Value} times"),
            new Button("Force Re-render", _ => forceUpdate.Set(forceUpdate.Value + 1))
        );
    }
}
```

## When to Use UseStatic

### Storing Timers and Intervals

Keep references to timers for cleanup:

```csharp
// In real code with UseStatic:
// var timer = UseStatic<Timer?>(null);
// 
// var startTimer = () => {
//     timer = new Timer(_ => { /* update state */ }, null, 0, 1000);
// };
//
// UseEffect(() => {
//     return () => timer?.Dispose(); // Cleanup on unmount
// });
```

### Tracking Previous Values

Store previous state values for comparison:

```csharp demo-tabs
public class PreviousValueDemo : ViewBase
{
    public override object? Build()
    {
        var count = UseState(0);
        // In real code: var previousCount = UseStatic<int?>(null);
        var previousCount = 0; // Simplified for demo
        
        var delta = count.Value - previousCount;
        
        // In real UseEffect, you would update previousCount after render:
        // UseEffect(() => { previousCount = count.Value; }, count.Value);
        
        return Layout.Vertical(
            Text.P($"Current: {count.Value}"),
            Text.P($"Previous: {previousCount}"),
            Text.P($"Delta: {delta}"),
            new Button("+1", _ => count.Set(count.Value + 1)),
            new Button("+5", _ => count.Set(count.Value + 5))
        );
    }
}
```

## UseStatic vs UseState vs UseMemo

Understanding when to use each hook:

| Hook | Triggers Re-render | Mutable | Use Case |
|------|-------------------|---------|----------|
| UseState | ✓ | ✗ | UI state that affects rendering |
| UseMemo | ✗ | ✗ | Expensive calculations |
| UseStatic | ✗ | ✓ | Mutable refs, timers, subscriptions |

## Best Practices

### 1. Use for Non-Reactive Values

```csharp
// Good: Timer reference doesn't affect rendering
// var timer = UseStatic<Timer?>(null);

// Bad: Use UseState for values that affect UI
// var count = UseStatic(0); // Should be UseState!
```

### 2. Clean Up Resources

```csharp
// Good: Clean up in effect
// var subscription = UseStatic<IDisposable?>(null);
// UseEffect(() => {
//     return () => subscription?.Dispose();
// });

// Bad: No cleanup, potential memory leak
// var subscription = UseStatic<IDisposable?>(null);
```

### 3. Initialize with Factory Function

```csharp
// Good: Factory function for expensive initialization
// var data = UseStatic(() => new ExpensiveObject());

// Acceptable: Simple value
// var count = UseStatic(0);
```

## Common Pitfalls

### 1. Using for Reactive State

```csharp
// Wrong: Static value won't trigger re-render
// var count = UseStatic(0);
// return new Button($"Count: {count}", _ => count++); // UI won't update!

// Correct: Use UseState for reactive values
var count = UseState(0);
return new Button($"Count: {count.Value}", _ => count.Set(count.Value + 1));
```

### 2. Forgetting Cleanup

```csharp
// Wrong: No cleanup
// var timer = UseStatic(() => new Timer(_ => { }, null, 0, 1000));

// Correct: Clean up in effect
// var timer = UseStatic<Timer?>(null);
// UseEffect(() => {
//     timer = new Timer(_ => { }, null, 0, 1000);
//     return () => timer?.Dispose();
// });
```

## Performance Considerations

UseStatic is lightweight and has minimal overhead:
- No dependency tracking
- No re-render triggering
- Direct value storage

Use it when:
- ✓ Storing mutable references (timers, subscriptions)
- ✓ Tracking previous values
- ✓ Caching expensive initializations
- ✓ Managing DOM references

Don't use it when:
- ✗ Value affects rendering (use UseState)
- ✗ Value is computed from other values (use UseMemo)
- ✗ Value is a simple constant (use regular variables)

## See Also

- [UseState](./03_State.md) - Reactive state management
- [UseEffect](./04_Effect.md) - Side effects and cleanup
- [Memoization](../../01_Onboarding/02_Concepts/10_Memoization.md) - Performance optimization
