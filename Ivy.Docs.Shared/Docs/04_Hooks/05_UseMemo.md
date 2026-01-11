# UseMemo

`UseMemo` is a hook that lets you cache the result of a calculation between re-renders. It is used for performance optimization.

## Usage

```csharp
var memoizedValue = UseMemo(Func<T> factory, params object[] dependencies);
```

### Parameters

- `factory`: A function that calculates the value you want to cache.
- `dependencies`: The list of all values used in the `factory` function. The `factory` will only re-run if one of these dependencies changes.

### Returns

The result of calling `factory`.

## Examples

### Expensive Calculation

```csharp
public override object Build()
{
    var count = UseState(0);
    var text = UseState("");

    // Only re-calculate when count changes, not when text changes
    var doubleCount = UseMemo(() => 
    {
        // Imagine a heavy computation here
        return count.Value * 2;
    }, count.Value);

    return new Column(
        new Text($"Double Count: {doubleCount}"),
        new Button(() => count.Set(count.Value + 1), "Increment"),
        new TextInput(text.Value, text.Set)
    );
}
```
