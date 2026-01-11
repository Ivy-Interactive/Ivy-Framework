---
searchHints:
  - usereducer
  - reducer
  - state-management
  - complex-state
  - actions
  - hooks
---

# UseReducer

<Ingress>
Manage complex state logic with reducers, providing a predictable state management pattern for components with multiple sub-values or interdependent state updates.
</Ingress>

The `UseReducer` hook is an alternative to UseState that is better suited for managing complex state logic. It follows the reducer pattern where state updates are handled by a pure function.

## Basic Usage

Create a reducer to manage state transitions:

```csharp demo-below
public class BasicReducerDemo : ViewBase
{
    // Reducer function
    private int CounterReducer(int state, string action) => action switch
    {
        "increment" => state + 1,
        "decrement" => state - 1,
        "reset" => 0,
        _ => state
    };
    
    public override object? Build()
    {
        // In real code: var (count, dispatch) = UseReducer(CounterReducer, 0);
        // For demo purposes, using UseState:
        var count = UseState(0);
        
        return Layout.Vertical(
            Text.H3($"Count: {count.Value}"),
            Layout.Horizontal(
                new Button("-", _ => count.Set(count.Value - 1)),
                new Button("Reset", _ => count.Set(0)),
                new Button("+", _ => count.Set(count.Value + 1))
            )
        );
    }
}
```

## When to Use UseReducer

### Complex State Logic

When state updates depend on previous state or involve multiple sub-values:

```csharp demo-tabs
public class ComplexStateDemo : ViewBase
{
    record TodoState(List<string> Items, int CompletedCount);
    
    private TodoState TodoReducer(TodoState state, (string Action, string? Value) action) =>
        action.Action switch
        {
            "add" => state with { Items = state.Items.Append(action.Value!).ToList() },
            "remove" => state with { Items = state.Items.Where(x => x != action.Value).ToList() },
            "complete" => state with { CompletedCount = state.CompletedCount + 1 },
            _ => state
        };
    
    public override object? Build()
    {
        // In real code: var (state, dispatch) = UseReducer(TodoReducer, new TodoState(...));
        var items = UseState(() => new List<string>());
        var completedCount = UseState(0);
        var newTodo = UseState("");
        
        return Layout.Vertical(
            Layout.Horizontal(
                newTodo.ToTextInput().Placeholder("New todo..."),
                new Button("Add", _ =>
                {
                    if (!string.IsNullOrWhiteSpace(newTodo.Value))
                    {
                        var newItems = items.Value.ToList();
                        newItems.Add(newTodo.Value);
                        items.Set(newItems);
                        newTodo.Set("");
                    }
                })
            ),
            items.Value.Select(item =>
                Layout.Horizontal(
                    Text.P(item),
                    new Button("Complete", _ => completedCount.Set(completedCount.Value + 1)),
                    new Button("Remove", _ =>
                    {
                        var newItems = items.Value.Where(x => x != item).ToList();
                        items.Set(newItems);
                    })
                )),
            Text.P($"Completed: {completedCount.Value}")
        );
    }
}
```

## UseReducer vs UseState

Choose between UseReducer and UseState based on complexity:

| UseState | UseReducer |
|----------|------------|
| Simple state updates | Complex state logic |
| Independent values | Interdependent values |
| Direct state setting | Action-based updates |
| Less boilerplate | More structured |
| Good for 1-3 values | Good for 4+ values |

## Best Practices

### 1. Keep Reducers Pure

```csharp
// Good: Pure reducer
private State Reducer(State state, Action action) => action switch
{
    "increment" => state with { Count = state.Count + 1 },
    _ => state
};

// Bad: Side effects in reducer
private State Reducer(State state, Action action)
{
    Console.WriteLine("Reducing..."); // Side effect!
    return action switch
    {
        "increment" => state with { Count = state.Count + 1 },
        _ => state
    };
}
```

### 2. Use Immutable Updates

```csharp
// Good: Immutable update with records
record State(List<int> Items);
private State Reducer(State state, string action) => action switch
{
    "add" => state with { Items = state.Items.Append(1).ToList() },
    _ => state
};

// Bad: Mutating state
private State Reducer(State state, string action)
{
    if (action == "add")
    {
        state.Items.Add(1); // Mutation!
        return state;
    }
    return state;
}
```

### 3. Handle All Action Types

```csharp
// Good: Default case handles unknown actions
private State Reducer(State state, string action) => action switch
{
    "increment" => state with { Count = state.Count + 1 },
    "decrement" => state with { Count = state.Count - 1 },
    _ => state // Return current state for unknown actions
};
```

## Common Pitfalls

### 1. Mutating State

```csharp
// Wrong: Mutating state directly
private State Reducer(State state, Action action)
{
    state.Items.Add(newItem); // Mutation!
    return state;
}

// Correct: Return new state
private State Reducer(State state, Action action) => action switch
{
    "add" => state with { Items = state.Items.Append(newItem).ToList() },
    _ => state
};
```

### 2. Side Effects in Reducer

```csharp
// Wrong: Side effects in reducer
private State Reducer(State state, Action action)
{
    if (action == "save")
    {
        SaveToDatabase(state); // Side effect!
        return state;
    }
    return state;
}

// Correct: Use effects for side effects
// UseEffect(() => { if (state.ShouldSave) SaveToDatabase(state); }, state.ShouldSave);
```

## See Also

- [UseState](./03_UseState.md) - Simple state management
- [UseEffect](./04_UseEffect.md) - Side effects and async operations
- [Memoization](../../01_Onboarding/02_Concepts/10_Memoization.md) - Performance optimization
