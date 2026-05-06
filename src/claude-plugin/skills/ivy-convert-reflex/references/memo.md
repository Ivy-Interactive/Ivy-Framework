# Memo

Memoizes components to skip re-rendering when their props have not changed. This improves performance for expensive components that produce the same output given the same inputs.

## Reflex

```python
class DemoState(rx.State):
    count: int = 0

    @rx.event
    def increment(self):
        self.count += 1


@rx.memo
def expensive_component(label: str) -> rx.Component:
    return rx.vstack(
        rx.heading(label),
        rx.text("This component only re-renders when props change!"),
        rx.divider(),
    )


def index():
    return rx.vstack(
        rx.text("Count: ", DemoState.count),
        rx.button("Increment", on_click=DemoState.increment),
        expensive_component(label="Memoized Component"),
    )
```

## Ivy

Ivy offers two mechanisms: the `IMemoized` interface for component-level memoization (closest to `rx.memo`), and the `UseMemo` hook for caching computed values.

### Component-level memoization (IMemoized)

```csharp
public class ExpensiveComponent : ViewBase, IMemoized
{
    private string _label;

    public ExpensiveComponent(string label)
    {
        _label = label;
    }

    public object[] GetMemoValues() => [_label];

    public override object? Build() =>
        new Column
        {
            new H1(_label),
            new Text("This component only re-renders when props change!"),
            new Hr(),
        };
}
```

### Value-level memoization (UseMemo)

```csharp
var filteredData = UseMemo(() =>
    data.Value
        .Where(item => item.Name.Contains(filter.Value, StringComparison.OrdinalIgnoreCase))
        .ToList(),
    data.Value, filter.Value);
```

## Parameters

| Parameter        | Reflex (`rx.memo`)                                                          | Ivy                                                                                |
|------------------|-----------------------------------------------------------------------------|------------------------------------------------------------------------------------|
| Component func   | Decorated function becomes the memoized component                           | Class implements `IMemoized` interface                                              |
| Props / deps     | Typed function arguments; re-renders only when argument values change        | `GetMemoValues()` returns array of values; re-renders only when those values change |
| Event handlers   | Supported via `rx.EventHandler` typed args                                  | Not part of `IMemoized`; wire events through normal patterns                        |
| Value caching    | Not supported (component-level only)                                        | `UseMemo<T>(Func<T> factory, params object[] dependencies)` caches computed values  |
| Keyword args     | Required when calling a memoized component                                  | N/A (constructor / named parameters)                                               |
| Type annotations | Required on all arguments                                                   | Enforced by C# type system                                                         |
