# Signals

<Ingress>
Signals enable reactive state management and side effect handling in Ivy applications.
</Ingress>

## Overview

Signals are lightweight, reactive primitives that can be used to track and react to state changes. They are particularly useful for handling asynchronous operations and real-time updates.

## Basic Usage

```csharp
public class SignalExample : ViewBase
{
    public override object? Build()
    {
        var count = UseSignal(0);
        
        return Layout.Horizontal(
            new Button("Increment", onClick: _ => count.Set(count.Value + 1)),
            count
        );
    }
}
```

## Signal Types

### Value Signals

```csharp
var value = UseSignal(42);
```

### Async Signals

```csharp
var data = UseSignal(async () => await api.GetData());
```

### Computed Signals

```csharp
var doubled = UseSignal(() => count.Value * 2);
```

## Signal Effects

Signals can trigger effects when their values change:

```csharp
public class SignalEffectExample : ViewBase
{
    public override object? Build()
    {
        var searchTerm = UseSignal("");
        var results = UseSignal(Array.Empty<Result>());
        
        UseEffect(async () => {
            if (string.IsNullOrEmpty(searchTerm.Value))
            {
                results.Set(Array.Empty<Result>());
                return;
            }
            
            var searchResults = await api.Search(searchTerm.Value);
            results.Set(searchResults);
        }, searchTerm);
        
        return Layout.Vertical(
            new TextInput("Search", value: searchTerm.Value, onChange: v => searchTerm.Set(v)),
            results
        );
    }
}
```

## Signal Composition

Signals can be composed to create more complex reactive flows:

```csharp
public class SignalCompositionExample : ViewBase
{
    public override object? Build()
    {
        var firstName = UseSignal("");
        var lastName = UseSignal("");
        
        var fullName = UseSignal(() => $"{firstName.Value} {lastName.Value}".Trim());
        var isValid = UseSignal(() => !string.IsNullOrEmpty(fullName.Value));
        
        return Layout.Vertical(
            new TextInput("First Name", value: firstName.Value, onChange: v => firstName.Set(v)),
            new TextInput("Last Name", value: lastName.Value, onChange: v => lastName.Set(v)),
            fullName,
            new Button("Submit").Disabled(!isValid.Value)
        );
    }
}
```

## Signal Operators

Signals support various operators for transformation and combination:

### Map

```csharp
var upperCase = searchTerm.Map(s => s.ToUpper());
```

### Filter

```csharp
var nonEmpty = searchTerm.Filter(s => !string.IsNullOrEmpty(s));
```

### Combine

```csharp
var combined = Signal.Combine(firstName, lastName, (f, l) => $"{f} {l}");
```

## Best Practices

1. **Single Responsibility**: Each signal should track one piece of state.
2. **Computed Values**: Use computed signals for derived state.
3. **Async Operations**: Use async signals for data fetching.
4. **Cleanup**: Return cleanup functions from effects when needed.
5. **Performance**: Use appropriate operators to optimize signal updates.

## Examples

### Real-time Search with Debounce

```csharp
public class SearchExample : ViewBase
{
    public override object? Build()
    {
        var searchTerm = UseSignal("");
        var results = UseSignal(Array.Empty<Result>());
        
        UseEffect(async () => {
            if (string.IsNullOrEmpty(searchTerm.Value))
            {
                results.Set(Array.Empty<Result>());
                return;
            }
            
            await Task.Delay(300); // Debounce
            var searchResults = await api.Search(searchTerm.Value);
            results.Set(searchResults);
        }, searchTerm.Throttle(TimeSpan.FromMilliseconds(300)));
        
        return Layout.Vertical(
            new TextInput("Search", value: searchTerm.Value, onChange: v => searchTerm.Set(v)),
            results
        );
    }
}
```

### Form Validation with Signals

```csharp
public class ValidationExample : ViewBase
{
    public override object? Build()
    {
        var email = UseSignal("");
        var password = UseSignal("");
        
        var isEmailValid = email.Map(e => e.Contains("@"));
        var isPasswordValid = password.Map(p => p.Length >= 8);
        
        var canSubmit = Signal.Combine(isEmailValid, isPasswordValid, (e, p) => e && p);
        
        return Layout.Vertical(
            new TextInput("Email", value: email.Value, onChange: v => email.Set(v)),
            new TextInput("Password", value: password.Value, onChange: v => password.Set(v)),
            new Button("Submit").Disabled(!canSubmit.Value)
        );
    }
}
```

## See Also

- [State Management](./State.md)
- [Effects](./Effects.md)
