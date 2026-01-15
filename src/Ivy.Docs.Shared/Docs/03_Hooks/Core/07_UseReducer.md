---
searchHints:
  - usereducer
  - reducer
  - state-management
  - complex-state
  - actions
  - hooks
  - state-updates
  - predictable-state
imports:
  - Ivy.Core.Hooks
---

# UseReducer

<Ingress>
Manage complex [state](./03_UseState.md) logic with reducers, providing a predictable state management pattern for [components](../../../01_Onboarding/02_Concepts/02_Views.md) with multiple sub-values or interdependent state updates.
</Ingress>

## Overview

The `UseReducer` [hook](../02_RulesOfHooks.md) is an alternative to [`UseState`](./03_UseState.md) that is better suited for managing complex [state](./03_UseState.md) logic. It follows the reducer pattern where state updates are handled by a pure function.

Key benefits of `UseReducer`:

- **Predictable [State](./03_UseState.md) Updates** - All state changes go through a single reducer function
- **Complex [State](./03_UseState.md) Logic** - Better suited for state with multiple sub-values or interdependent updates
- **Action-Based Updates** - State changes are explicit and traceable through actions
- **Testability** - Pure reducer functions are easy to test in isolation

<Callout type="Tip">
`UseReducer` is ideal when you have complex [state](./03_UseState.md) logic involving multiple sub-values, when the next state depends on the previous one, or when you want to centralize state update logic in one place.
</Callout>

## When to Use UseReducer

```mermaid
flowchart TD
    A["Need to manage state?"] --> B{What's the complexity?}
    
    B --> C["Simple state<br/>1-3 independent values"]
    B --> D["Complex state<br/>4+ values or interdependent"]
    B --> E["State updates depend<br/>on previous state"]
    B --> F["Need centralized<br/>state logic"]
    
    C --> G["Use UseState<br/>Simple and direct"]
    D --> H["Use UseReducer<br/>Better organization"]
    E --> I["Use UseReducer<br/>Predictable updates"]
    F --> J["Use UseReducer<br/>Single source of truth"]
    
    G --> K["Direct state setting<br/>Less boilerplate<br/>Good for simple cases"]
    H --> L["Action-based updates<br/>More structured<br/>Better for complex logic"]
    I --> M["Pure reducer function<br/>Easier to reason about<br/>Better testability"]
    J --> N["Centralized logic<br/>Easier to maintain<br/>Better debugging"]
```

## UseReducer Hook

The `UseReducer` [hook](../02_RulesOfHooks.md) manages [state](./03_UseState.md) through a reducer function that takes the current [state](./03_UseState.md) and an action, returning the new [state](./03_UseState.md).

<Callout type="Tip">
Reducers should be pure functions - they should not have side effects and should return a new [state](./03_UseState.md) object rather than mutating the existing one. Use [`UseEffect`](./04_UseEffect.md) for side effects.
</Callout>

### How UseReducer Works

```mermaid
sequenceDiagram
    participant C as Component
    participant UR as UseReducer Hook
    participant R as Reducer Function
    participant S as State Storage
    
    Note over C,S: Initialization
    C->>UR: UseReducer(reducer, initialState)
    UR->>S: Store initialState
    UR->>UR: Create dispatch function
    UR-->>C: Return (state, dispatch)
    
    Note over C,S: State Update via Dispatch
    C->>UR: dispatch(action)
    UR->>S: Get current state
    S-->>UR: Return current state
    UR->>R: Call reducer(currentState, action)
    R->>R: Process action and compute new state
    R-->>UR: Return new state
    UR->>S: Update stored state
    UR->>C: Trigger re-render with new state
    
    Note over C,S: Component Re-render
    C->>UR: UseReducer(reducer, initialState)
    UR->>S: Get current state
    S-->>UR: Return updated state
    UR-->>C: Return (newState, dispatch)
```

### Basic Usage

```csharp demo-tabs
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
        var (count, dispatch) = this.UseReducer(CounterReducer, 0);
        
        return Layout.Vertical(
            Text.H3($"Count: {count}"),
            Layout.Horizontal(
                new Button("-", _ => dispatch("decrement")),
                new Button("Reset", _ => dispatch("reset")),
                new Button("+", _ => dispatch("increment"))
            )
        );
    }
}
```

### Use Cases

Use `UseReducer` when:

- **Complex [State](./03_UseState.md) Logic** - Managing state with multiple sub-values or interdependent properties
- **State Updates Depend on Previous State** - When the next state depends on the previous state value
- **Action-Based Updates** - When you want explicit, traceable state changes through actions
- **Centralized State Logic** - When you want to centralize all state update logic in one place
- **Better Testability** - When you need to test state logic in isolation

### UseReducer vs [`UseState`](./03_UseState.md)

Choose between `UseReducer` and [`UseState`](./03_UseState.md) based on complexity:

| UseState | UseReducer |
|----------|------------|
| Simple state updates | Complex state logic |
| Independent values | Interdependent values |
| Direct state setting | Action-based updates |
| Less boilerplate | More structured |
| Good for 1-3 values | Good for 4+ values |

### Best Practices

- **Keep Reducers Pure** - Reducers should not have side effects and should return new [state](./03_UseState.md) objects. Use [`UseEffect`](./04_UseEffect.md) for side effects.
- **Use Immutable Updates** - Always return new state objects rather than mutating existing ones
- **Handle All Action Types** - Include a default case to handle unknown actions gracefully
- **Type Safety** - Use strongly-typed actions and [state](./03_UseState.md) for better compile-time safety
- **Extract Complex Logic** - Move complex reducer logic into separate functions for clarity and use [`UseMemo`](./05_UseMemo.md) for expensive computations

### Examples

#### Shopping Cart with Interdependent State

This example demonstrates why reducers are powerful - multiple interdependent values (items, subtotal, tax, discount, total) that must stay in sync. With `UseState`, you'd need to update each value separately and risk inconsistencies.

```csharp demo-tabs
public class ShoppingCartDemo : ViewBase
{
    record CartItem(string Name, decimal Price, int Quantity);
    record CartState(List<CartItem> Items, decimal DiscountPercent, decimal Subtotal, decimal Tax, decimal Total);
    
    private CartState CartReducer(CartState state, string action)
    {
        var parts = action.Split('|', 2);
        var actionType = parts[0];
        var actionValue = parts.Length > 1 ? parts[1] : "";
        
        return actionType switch
        {
            "addItem" => CalculateTotals(state with { 
                Items = state.Items.Append(new CartItem(actionValue, 10.00m, 1)).ToList() 
            }),
            "removeItem" => CalculateTotals(state with { 
                Items = state.Items.Where((item, idx) => idx.ToString() != actionValue).ToList() 
            }),
            "setDiscount" => CalculateTotals(state with { 
                DiscountPercent = decimal.TryParse(actionValue, out var discount) ? discount : 0m 
            }),
            _ => state
        };
    }
    
    private CartState CalculateTotals(CartState state)
    {
        var subtotal = state.Items.Sum(item => item.Price * item.Quantity);
        var discountAmount = subtotal * (state.DiscountPercent / 100m);
        var afterDiscount = subtotal - discountAmount;
        var tax = afterDiscount * 0.08m; // 8% tax
        var total = afterDiscount + tax;
        
        return state with 
        { 
            Subtotal = subtotal,
            Tax = tax,
            Total = total
        };
    }
    
    public override object? Build()
    {
        var (cart, dispatch) = this.UseReducer(CartReducer, new CartState(new List<CartItem>(), 0m, 0m, 0m, 0m));
        var newItemName = UseState("");
        
        return Layout.Vertical(
            Text.H3("Shopping Cart"),
            Layout.Horizontal(
                newItemName.ToTextInput().Placeholder("Item name"),
                new Button("Add Item", _ => {
                    if (!string.IsNullOrWhiteSpace(newItemName.Value))
                    {
                        dispatch($"addItem|{newItemName.Value}");
                        newItemName.Set("");
                    }
                })
            ),
            new Separator(),
            cart.Items.Count > 0 
                ? Layout.Vertical(
                    new { 
                        Items = string.Join(", ", cart.Items.Select(item => $"{item.Name} (${item.Price:F2} x {item.Quantity})"))
                    }.ToDetails(),
                    Layout.Horizontal(
                        cart.Items.Select((item, idx) => 
                            new Button($"Remove {item.Name}", _ => dispatch($"removeItem|{idx}"))
                        ).ToArray()
                    )
                )
                : Text.P("No items in cart").Small(),
            new Separator(),
            Layout.Vertical(
                Text.P($"Subtotal: ${cart.Subtotal:F2}"),
                Text.P($"Discount ({cart.DiscountPercent}%): ${cart.Subtotal * (cart.DiscountPercent / 100m):F2}"),
                Text.P($"Tax (8%): ${cart.Tax:F2}"),
                Text.H4($"Total: ${cart.Total:F2}")
            ),
            Layout.Horizontal(
                new Button("10% Off", _ => dispatch("setDiscount|10")),
                new Button("20% Off", _ => dispatch("setDiscount|20")),
                new Button("Clear Discount", _ => dispatch("setDiscount|0"))
            )
        );
    }
}
```

#### Game State with Multiple Interdependent Values

This example shows a game state where actions affect multiple values simultaneously - perfect for demonstrating reducer's centralized state management.

```csharp demo-tabs
public class GameStateDemo : ViewBase
{
    record GameState(int Score, int Level, int Lives, int Multiplier, bool IsGameOver);
    
    private GameState GameReducer(GameState state, string action) => action switch
    {
        "score" => state.IsGameOver ? state : state with { 
            Score = state.Score + (10 * state.Multiplier),
            Level = (state.Score + (10 * state.Multiplier)) / 100 + 1,
            Multiplier = Math.Min(state.Multiplier + 1, 5)
        },
        "miss" => state.IsGameOver ? state : state with { 
            Lives = state.Lives - 1,
            Multiplier = 1,
            IsGameOver = state.Lives <= 1
        },
        "reset" => new GameState(0, 1, 3, 1, false),
        _ => state
    };
    
    public override object? Build()
    {
        var (game, dispatch) = this.UseReducer(GameReducer, new GameState(0, 1, 3, 1, false));
        
        return Layout.Vertical(
            Text.H3("Game State"),
            Layout.Vertical(
                Text.P($"Score: {game.Score}"),
                Text.P($"Level: {game.Level}"),
                Text.P($"Lives: {game.Lives}"),
                Text.P($"Multiplier: {game.Multiplier}x"),
                game.IsGameOver ? Text.Danger("Game Over!") : Text.Success("Playing...")
            ),
            new Separator(),
            Layout.Horizontal(
                new Button("Score!", _ => dispatch("score")).Disabled(game.IsGameOver),
                new Button("Miss", _ => dispatch("miss")).Disabled(game.IsGameOver),
                new Button("Reset", _ => dispatch("reset"))
            ),
            Text.P("Notice how scoring updates level and multiplier, while missing resets multiplier and decreases lives - all in one reducer!").Small()
        );
    }
}
```

## Performance Considerations

### State Update Efficiency

- **Immutable Updates**: Creating new [state](./03_UseState.md) objects has a small overhead, but enables better change detection and prevents bugs:

```csharp
// Good: Immutable update with records
record State(List<int> Items);
private State Reducer(State state, string action) => action switch
{
    "add" => state with { Items = state.Items.Append(1).ToList() },
    _ => state
};

// Consider: For very large lists, consider more efficient update strategies
```

- **Reducer Complexity**: Keep reducer logic simple and fast. Move complex computations outside the reducer:

```csharp
// Good: Simple reducer, complex logic outside
private State Reducer(State state, Action action) => action switch
{
    "update" => state with { Data = action.Data },
    _ => state
};

// Bad: Complex computation in reducer
private State Reducer(State state, Action action) => action switch
{
    "process" => state with { 
        ProcessedData = state.RawData.SelectMany(/* complex transformation */).ToList() 
    },
    _ => state
};
```

- **Action Object Creation**: Consider the overhead of creating action objects:

```csharp
// Good: Simple action types
dispatch("increment");
dispatch(("add", item));

// Consider: For high-frequency updates, use value types or simple types
```

### When NOT to Use UseReducer

- **Simple [State](./03_UseState.md)**: Don't use `UseReducer` for simple state that can be managed with [`UseState`](./03_UseState.md)
- **Independent Values**: If state values are independent, [`UseState`](./03_UseState.md) is more appropriate
- **No Complex Logic**: If state updates are straightforward, `UseReducer` adds unnecessary complexity

```csharp
// Unnecessary: Simple state doesn't need reducer
var (count, dispatch) = UseReducer((s, a) => a == "inc" ? s + 1 : s, 0);

// Better: Use UseState for simple cases
var count = UseState(0);
```

## Common Pitfalls and Solutions

### Reducer Troubleshooting Guide

```mermaid
flowchart TD
    A["UseReducer not working as expected?"] --> B{Check your implementation}
    
    B --> C["State not updating?"]
    B --> D["Unexpected state values?"]
    B --> E["Performance issues?"]
    B --> F["Side effects in reducer?"]
    
    C --> C1["Check reducer returns new state<br/>Verify dispatch is called<br/>Ensure action is handled"]
    D --> D1["Check for state mutations<br/>Verify action types match<br/>Review reducer logic"]
    E --> E1["Keep reducers simple<br/>Move complex logic outside<br/>Consider [memoization](./05_UseMemo.md)"]
    F --> F1["Remove side effects<br/> Use UseEffect for side effects<br/> Keep reducer pure"]
    
    C1 --> G["Problem solved?"]
    D1 --> G
    E1 --> G
    F1 --> G
    
    G -->|Yes| H["Great! Your reducer is working correctly"]
    G -->|No| I["Consider alternative approaches<br/>or seek help in community"]
```

### Mutating [State](./03_UseState.md)

**Problem**: Mutating [state](./03_UseState.md) directly instead of returning new [state](./03_UseState.md)

```csharp
// Wrong: Mutating state directly
private State Reducer(State state, Action action)
{
    if (action == "add")
{
    state.Items.Add(newItem); // Mutation!
        return state;
    }
    return state;
}
```

**Solution**: Always return new [state](./03_UseState.md) objects

```csharp
// Correct: Return new state
private State Reducer(State state, Action action) => action switch
{
    "add" => state with { Items = state.Items.Append(newItem).ToList() },
    _ => state
};
```

### Side Effects in Reducer

**Problem**: Performing side effects inside the reducer function. Reducers should be pure - use [`UseEffect`](./04_UseEffect.md) for side effects.

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
```

**Solution**: Use [`UseEffect`](./04_UseEffect.md) for side effects

```csharp
// Correct: Pure reducer
private State Reducer(State state, Action action) => action switch
{
    "save" => state with { ShouldSave = true },
    _ => state
};

// Handle side effects in component
UseEffect(() => 
{
    if (state.Value.ShouldSave)
    {
        SaveToDatabase(state.Value);
        dispatch(("saved", null));
    }
}, state.Value.ShouldSave);
```

### Not Handling All Actions

**Problem**: Missing default case or not handling all action types

```csharp
// Wrong: No default case
private State Reducer(State state, Action action) => action switch
{
    "increment" => state with { Count = state.Count + 1 },
    "decrement" => state with { Count = state.Count - 1 }
    // Missing default case!
};
```

**Solution**: Always include a default case

```csharp
// Correct: Default case handles unknown actions
private State Reducer(State state, Action action) => action switch
{
    "increment" => state with { Count = state.Count + 1 },
    "decrement" => state with { Count = state.Count - 1 },
    _ => state // Return current state for unknown actions
};
```

### Complex Logic in Reducer

**Problem**: Putting too much complex logic inside the reducer. Consider using [`UseMemo`](./05_UseMemo.md) for expensive computations.

```csharp
// Wrong: Complex computation in reducer
private State Reducer(State state, Action action) => action switch
{
    "process" => state with 
    { 
        Result = state.Data
            .Where(x => x.IsValid)
            .GroupBy(x => x.Category)
            .Select(g => new { Category = g.Key, Count = g.Count(), Total = g.Sum(x => x.Value) })
            .OrderByDescending(x => x.Total)
            .Take(10)
            .ToList()
    },
    _ => state
};
```

**Solution**: Extract complex logic to separate functions

```csharp
// Correct: Extract complex logic
private List<ProcessedItem> ProcessData(List<DataItem> data)
{
    return data
        .Where(x => x.IsValid)
        .GroupBy(x => x.Category)
        .Select(g => new ProcessedItem(g.Key, g.Count(), g.Sum(x => x.Value)))
        .OrderByDescending(x => x.Total)
        .Take(10)
        .ToList();
}

private State Reducer(State state, Action action) => action switch
{
    "process" => state with { Result = ProcessData(state.Data) },
    _ => state
};
```

### Forgetting to Update Related [State](./03_UseState.md)

**Problem**: Updating one part of [state](./03_UseState.md) but forgetting related parts

```csharp
// Wrong: Incomplete state update
private State Reducer(State state, Action action) => action switch
{
    "addItem" => state with { Items = state.Items.Append(newItem).ToList() },
    // Forgot to update count!
    _ => state
};
```

**Solution**: Update all related [state](./03_UseState.md) properties

```csharp
// Correct: Update all related state
private State Reducer(State state, Action action) => action switch
{
    "addItem" => state with 
    { 
        Items = state.Items.Append(newItem).ToList(),
        Count = state.Count + 1,
        LastUpdated = DateTime.Now
    },
    _ => state
};
```

### Using Wrong Action Types

**Problem**: Dispatching actions that don't match the reducer's expected types

```csharp
// Wrong: Action type mismatch
var (state, dispatch) = UseReducer(Reducer, initialState);
dispatch(123); // Reducer expects string actions!

private State Reducer(State state, string action) => action switch
{
    "increment" => state with { Count = state.Count + 1 },
    _ => state
};
```

**Solution**: Use strongly-typed actions

```csharp
// Correct: Strongly-typed actions
record IncrementAction();
record DecrementAction();
record AddItemAction(string Item);

var (state, dispatch) = UseReducer(Reducer, initialState);
dispatch(new IncrementAction());

private State Reducer(State state, object action) => action switch
{
    IncrementAction => state with { Count = state.Count + 1 },
    DecrementAction => state with { Count = state.Count - 1 },
    AddItemAction a => state with { Items = state.Items.Append(a.Item).ToList() },
    _ => state
};
```

## See Also

- [State Management](./03_UseState.md) - Simple state management with UseState
- [Rules of Hooks](../02_RulesOfHooks.md) - Understanding hook rules and best practices
- [Effects](./04_UseEffect.md) - Side effects and async operations
- [Memoization](./05_UseMemo.md) - Performance optimization with UseMemo
- [Callbacks](./06_UseCallback.md) - Memoized callback functions with UseCallback
- [Views](../../../01_Onboarding/02_Concepts/02_Views.md) - Understanding Ivy views and components
