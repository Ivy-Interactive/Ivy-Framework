---
searchHints:
  - usecallback
  - memoization
  - callbacks
  - performance
  - optimization
  - hooks
---

# UseCallback

<Ingress>
Memoize callback functions to prevent unnecessary re-creations and optimize performance when passing functions to child components or effects.
</Ingress>

The `UseCallback` hook returns a memoized version of a callback function that only changes when its dependencies change.

## Basic Usage

Create a memoized callback that only changes when dependencies change:

```csharp demo-below
public class BasicCallbackDemo : ViewBase
{
    public override object? Build()
    {
        var count = UseState(0);
        var text = UseState("");
        
        // Callback only re-created when count changes
        var handleClick = () =>
        {
            Console.WriteLine($"Count is: {count.Value}");
        };
        
        return Layout.Vertical(
            new Button($"Log Count ({count.Value})", _ => handleClick()),
            new Button("Increment", _ => count.Set(count.Value + 1)),
            text.ToTextInput().Placeholder("Type here (doesn't affect callback)")
        );
    }
}
```

## When to Use UseCallback

### Event Handler Stability

Create stable event handlers for forms and inputs:

```csharp demo-tabs
public class FormCallbackDemo : ViewBase
{
    public override object? Build()
    {
        var formData = UseState(() => new { Name = "", Email = "" });
        var submitCount = UseState(0);
        
        // Stable handlers
        var handleNameChange = (string name) =>
        {
            formData.Set(new { Name = name, Email = formData.Value.Email });
        };
        
        var handleEmailChange = (string email) =>
        {
            formData.Set(new { Name = formData.Value.Name, Email = email });
        };
        
        var handleSubmit = () =>
        {
            submitCount.Set(submitCount.Value + 1);
            Console.WriteLine($"Submitted: {formData.Value.Name}, {formData.Value.Email}");
        };
        
        var name = UseState(formData.Value.Name);
        var email = UseState(formData.Value.Email);
        
        return Layout.Vertical(
            Text.P("Name:"),
            name.ToTextInput(),
            Text.P("Email:"),
            email.ToTextInput(),
            new Button("Submit", _ => handleSubmit()),
            Text.P($"Submitted {submitCount.Value} times")
        );
    }
}
```

### Callback Composition

Combine multiple callbacks:

```csharp demo-tabs
public class CallbackCompositionDemo : ViewBase
{
    public override object? Build()
    {
        var logs = UseState(() => new List<string>());
        
        var log = (string message) =>
        {
            var newLogs = logs.Value.ToList();
            newLogs.Add($"{DateTime.Now:HH:mm:ss} - {message}");
            if (newLogs.Count > 5) newLogs.RemoveAt(0);
            logs.Set(newLogs);
        };
        
        var handleClick = () =>
        {
            log("Button clicked");
        };
        
        var handleDoubleClick = () =>
        {
            log("Button double-clicked");
        };
        
        return Layout.Vertical(
            new Button("Click Me", _ => handleClick()),
            new Button("Double Click", _ => handleDoubleClick()),
            Layout.Vertical(logs.Value.Select(Text.Small))
        );
    }
}
```

### Conditional Callbacks

Create callbacks with conditional logic:

```csharp demo-tabs
public class ConditionalCallbackDemo : ViewBase
{
    public override object? Build()
    {
        var isEnabled = UseState(true);
        var count = UseState(0);
        
        var handleClick = () =>
        {
            if (isEnabled.Value)
            {
                count.Set(count.Value + 1);
            }
        };
        
        return Layout.Vertical(
            Text.P($"Count: {count.Value}"),
            new Button("Increment", _ => handleClick()),
            Text.P("Enable counting:"),
            isEnabled.ToBoolInput()
        );
    }
}
```

## Best Practices

### 1. Don't Overuse

```csharp
// Bad: Unnecessary for simple inline handlers
// Good: Simple inline handler
return new Button("Increment", _ => count.Set(count.Value + 1));
```

### 2. Keep Callbacks Pure

```csharp
// Good: Pure callback
var handleClick = () =>
{
    count.Set(count.Value + 1);
};

// Avoid: Side effects in callback (use UseEffect instead)
var handleClick = () =>
{
    Console.WriteLine("Clicked!"); // Side effect
    count.Set(count.Value + 1);
};
```

## Common Pitfalls

### 1. Forgetting Dependencies

```csharp
// Wrong: multiplier not captured correctly
var calculate = () =>
{
    return count.Value * multiplier.Value; // Uses multiplier!
};

// Correct: Ensure all dependencies are captured
var calculate = () =>
{
    return count.Value * multiplier.Value;
};
```

## Performance Considerations

Only use callbacks when:
- The callback is passed to optimized child components
- The callback is used as an effect dependency
- The callback creation is expensive (rare)

```csharp
// When to use callbacks:
// ✓ Callbacks passed to child components
// ✓ Callbacks used in effect dependencies
// ✓ Event handlers that trigger expensive operations

// When NOT to use callbacks:
// ✗ Simple inline event handlers
// ✗ Callbacks only used once
// ✗ Callbacks not passed to other components
```

## See Also

- [UseState](./03_State.md) - Managing component state
- [UseEffect](./04_Effect.md) - Side effects and dependencies
- [Memoization](../../01_Onboarding/02_Concepts/10_Memoization.md) - Performance optimization guide
