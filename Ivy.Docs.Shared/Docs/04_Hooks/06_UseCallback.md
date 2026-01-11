# UseCallback

`UseCallback` is a hook that returns a memoized version of the callback function that only changes if one of the dependencies has changed. This is useful when passing callbacks to optimized child components that rely on reference equality to prevent unnecessary renders.

## Usage

```csharp
var memoizedCallback = UseCallback(Func<T> callback, params object[] dependencies);
```

### Parameters

- `callback`: The function you want to memoize.
- `dependencies`: The list of values that the callback depends on.

### Returns

A memoized version of the `callback`.

## Examples

### Optimized Child Component

```csharp
public override object Build()
{
    var count = UseState(0);
    var text = UseState("");

    // This callback will only be re-created when count changes
    var handleClick = UseCallback(() => 
    {
        Console.WriteLine($"Clicked with count: {count.Value}");
    }, count.Value);

    return new Column(
        new ExpensiveChild(handleClick),
        new TextInput(text.Value, text.Set)
    );
}

// ExpensiveChild will only re-render if handleClick reference changes
```
