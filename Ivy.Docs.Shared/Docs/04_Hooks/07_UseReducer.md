# UseReducer

`UseReducer` is a hook that is usually preferable to `UseState` when you have complex state logic that involves multiple sub-values or when the next state depends on the previous one. It also lets you optimize performance for components that trigger deep updates because you can pass `dispatch` down instead of callbacks.

## Usage

```csharp
var (state, dispatch) = UseReducer(Func<T, string, T> reducer, T initialState);
```

### Parameters

- `reducer`: A function `(state, action) => newState` that returns the new state based on the action.
- `initialState`: The initial value of the state.

### Returns

A tuple containing:
- `state`: The current state value.
- `dispatch`: A function to dispatch an action (string) to the reducer.

## Examples

### Counter with Reducer

```csharp
// Reducer function (pure C#)
int CounterReducer(int state, string action) => action switch
{
    "increment" => state + 1,
    "decrement" => state - 1,
    "reset" => 0,
    _ => state
};

public override object Build()
{
    var (count, dispatch) = UseReducer(CounterReducer, 0);

    return new Row(
        new Text($"Count: {count}"),
        new Button(() => dispatch("decrement"), "-"),
        new Button(() => dispatch("increment"), "+"),
        new Button(() => dispatch("reset"), "Reset")
    );
}
```
