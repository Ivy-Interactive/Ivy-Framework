# UseStatic

`UseStatic` is a hook that lets you store a value that is initialized only once and persists across re-renders. It is similar to `UseState`, but changing its value does NOT trigger a re-render. It is conceptually similar to `useRef` in React when used for storing mutable values that don't affect layout.

## Usage

```csharp
var value = UseStatic(T initialValue);
// or
var value = UseStatic(() => T initialValue);
```

### Parameters

- `initialValue`: The initial value or a factory function to create it.

### Returns

The persisted value of type `T`. Note that unlike `UseState`, it returns the raw value, not a state wrapper, so you can't "Set" it to trigger updates. Using this for mutable objects allows you to keep state without re-rendering.

## Examples

### Tracking Render Count

```csharp
public class RenderTracker : ViewBase
{
    // Mutable box to hold the count
    class Counter { public int Value = 0; }

    public override object Build()
    {
        // Initialized once, persisted across renders
        var renderCount = UseStatic(() => new Counter());
        
        renderCount.Value++; // Mutate without triggering re-render

        var dummy = UseState(0);

        return new Column(
            new Text($"Rendered: {renderCount.Value} times"),
            new Button(() => dummy.Set(dummy.Value + 1), "Force Re-render")
        );
    }
}
```
