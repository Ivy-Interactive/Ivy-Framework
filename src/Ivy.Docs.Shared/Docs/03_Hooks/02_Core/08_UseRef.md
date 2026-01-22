---
searchHints:
  - useref
  - ref
  - static
  - mutable
  - persistence
  - hooks
  - non-reactive
  - timers
  - subscriptions
imports:
  - Ivy.Core.Hooks
---

# UseRef

<Ingress>
Store values that persist across re-renders without triggering updates, similar to React's useRef for holding mutable values that don't affect the [view](../../../01_Onboarding/02_Concepts/02_Views.md) lifecycle.
</Ingress>

## Overview

Key characteristics of `UseRef`:

- **Non-Reactive Storage** - Values persist but don't trigger re-renders when changed
- **Mutable References** - Perfect for storing timers, subscriptions, and other mutable objects
- **Performance** - No dependency tracking or re-render overhead
- **Persistence** - Values survive across [component](../../../01_Onboarding/02_Concepts/02_Views.md) re-renders

<Callout type="Tip">
`UseRef` is ideal for storing mutable references that don't affect rendering, such as timers, subscriptions, DOM references, or previous [state](./03_UseState.md) values for comparison.
</Callout>

## Basic Usage

```csharp demo-below
public class BasicRefDemo : ViewBase
{
    class Counter { public int Value = 0; }
    
    public override object? Build()
    {
        var renderCount = this.UseRef(() => new Counter());
        var forceUpdate = UseState(0);
        
        // Increment without triggering re-render
        renderCount.Value.Value++;
        
        return Layout.Vertical()
            | new Button("Force Re-render", _ => forceUpdate.Set(forceUpdate.Value + 1))
            | Text.P($"This component has rendered {renderCount.Value.Value} times")
            | Text.P("(Note: The count increments on each render, but doesn't trigger re-renders)").Small();
    }
}
```

## How UseRef Works

```mermaid
sequenceDiagram
    participant C as Component
    participant UR as UseRef Hook
    participant S as Storage
    
    Note over C,S: First Render
    C->>UR: UseRef(initialValue)
    UR->>S: Check if value exists
    S-->>UR: No value found
    UR->>S: Store initialValue
    S-->>UR: Return stored value container
    UR-->>C: Return IState<T> container
    
    Note over C,S: Subsequent Render
    C->>UR: UseRef(initialValue)
    UR->>S: Get stored value
    S-->>UR: Return cached container
    UR-->>C: Return same container
    
    Note over C,S: Value Mutation (External)
    C->>UR: Direct mutation (ref.Value = newValue)
    Note right of C: Value changed but<br/>NO re-render triggered
    UR->>S: Value updated in storage
    Note right of UR: Component continues<br/>with same render
```

## When to Use UseRef

| Use UseRef For | Use Other Hooks Instead |
|----------------|-------------------------|
| Mutable references (timers, subscriptions) | Value affects UI → UseState |
| Tracking previous state values | Computed from other values → UseMemo |
| Caching expensive initializations | Needs to trigger side effects → UseState + UseEffect |
| Storing callback references | Simple constant → regular variable |

### UseRef vs UseState vs UseMemo

| Hook | Triggers Re-render | Mutable | Use Case |
|------|-------------------|---------|----------|
| [`UseState`](./03_UseState.md) | Yes | No | UI state that affects rendering |
| [`UseMemo`](./05_UseMemo.md) | No | No | Expensive calculations |
| `UseRef` | No | Yes | Mutable refs, timers, subscriptions |

## Performance Considerations

```csharp
// Good: Timer reference (no re-render needed)
var timerId = UseRef<Timer?>(null);

// Good: Previous value tracking
var previousCount = UseRef(0);

// Bad: Value affects UI - use UseState instead
var count = UseRef(0); // Won't trigger re-render!

// Bad: Computed value - use UseMemo instead
var total = UseRef(items.Sum()); // Won't update when items change!
```

## See Also

- [State Management](./03_UseState.md) - Reactive state with UseState
- [Rules of Hooks](../02_RulesOfHooks.md) - Understanding hook rules and best practices
- [Effects](./04_UseEffect.md) - Side effects and cleanup
- [Memoization](./05_UseMemo.md) - Performance optimization with UseMemo
- [Callbacks](./06_UseCallback.md) - Memoized callback functions with UseCallback
- [Views](../../../01_Onboarding/02_Concepts/02_Views.md) - Understanding Ivy views and components

## Examples

<Details>
<Summary>
Tracking Previous Values
</Summary>
<Body>

```csharp demo-tabs
public class PreviousValueDemo : ViewBase
{
    class PreviousValue { public int? Value = null; }
    class Counter { public int Value = 0; }
    
    public override object? Build()
    {
        var count = UseState(0);
        var previousValue = this.UseRef(() => new PreviousValue());
        var renderCount = this.UseRef(() => new Counter());
        
        renderCount.Value.Value++;
        
        // Get the previous value before updating it
        var previous = previousValue.Value.Value;
        var delta = previous.HasValue 
            ? count.Value - previous.Value 
            : 0;
        
        // Update previous value for next render (in real app, use UseEffect)
        previousValue.Value.Value = count.Value;
        
        return Layout.Vertical(
            Text.P($"Current: {count.Value}"),
            Text.P($"Previous: {previous?.ToString() ?? "None"}"),
            Text.P($"Delta: {delta}"),
            Text.P($"Renders: {renderCount.Value.Value}").Small(),
            Layout.Horizontal(
                new Button("+1", _ => count.Set(count.Value + 1)),
                new Button("+5", _ => count.Set(count.Value + 5)),
                new Button("Reset", _ => {
                    count.Set(0);
                    previousValue.Value.Value = null;
                })
            )
        );
    }
}
```

</Body>
</Details>

<Details>
<Summary>
Storing Mutable References
</Summary>
<Body>

```csharp demo-tabs
public class MutableReferenceDemo : ViewBase
{
    class RenderTracker { public int Count = 0; public DateTime LastRender = DateTime.Now; }
    
    public override object? Build()
    {
        var count = UseState(0);
        var tracker = this.UseRef(() => new RenderTracker());
        
        // Mutate ref value without triggering re-render
        tracker.Value.Count++;
        tracker.Value.LastRender = DateTime.Now;
        
        return Layout.Vertical(
            Text.H3($"Count: {count.Value}"),
            new { 
                RenderCount = tracker.Value.Count.ToString(),
                LastRender = tracker.Value.LastRender.ToString("HH:mm:ss")
            }.ToDetails(),
            Text.P("Render tracker is stored in UseRef - it persists across re-renders but doesn't trigger them").Small(),
            new Button("Increment", _ => count.Set(count.Value + 1))
        );
    }
}
```

</Body>
</Details>
