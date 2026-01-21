---
searchHints:
  - userefreshtoken
  - refresh
  - reload
  - trigger
  - manual-update
  - reactive
---

# UseRefreshToken

Refresh tokens provide a mechanism to manually trigger UI updates and effect executions in Ivy, enabling you to reload data, refresh components, or trigger actions on demand.

## Basic Usage

The `UseRefreshToken` hook creates a token that can be manually refreshed to trigger [effects](../../03_Hooks/02_Core/04_UseEffect.md):

```csharp demo-below
public class BasicRefreshExample : ViewBase
{
    public override object? Build()
    {
        var refreshToken = UseRefreshToken();
        var timestamp = UseState(DateTime.Now);
        
        // Effect runs when refresh token changes
        UseEffect(() =>
        {
            timestamp.Set(DateTime.Now);
        }, [refreshToken]);
        
        return Layout.Vertical()
            | Text.Muted("Click the button to manually trigger a refresh")
            | new Button("Refresh", onClick: _ => refreshToken.Refresh())
            | Text.P($"Last refreshed: {timestamp.Value:HH:mm:ss.fff}");
    }
}
```

## When to Use RefreshToken

```mermaid
sequenceDiagram
    participant U as User/Background
    participant R as RefreshToken
    participant E as UseEffect
    participant UI as UI
    
    U->>R: refreshToken.Refresh()
    R->>R: Generate new GUID
    R->>E: Trigger dependent effects
    E->>UI: Update component
```

## Passing Return Values

Refresh tokens can carry return values to pass data between operations:

```csharp demo-below
public class ReturnValueExample : ViewBase
{
    public override object? Build()
    {
        var refreshToken = UseRefreshToken();
        var selectedColor = UseState("No color selected");
        
        UseEffect(() =>
        {
            if (refreshToken.IsRefreshed && refreshToken.ReturnValue is string color)
            {
                selectedColor.Set($"Selected: {color}");
            }
        }, [refreshToken]);
        
        return Layout.Vertical()
            | Layout.Horizontal(
                new Button("Red", onClick: _ => refreshToken.Refresh("Red")),
                new Button("Green", onClick: _ => refreshToken.Refresh("Green")),
                new Button("Blue", onClick: _ => refreshToken.Refresh("Blue")))
            | Text.P(selectedColor.Value);
    }
}
```

Return values can be any type, including complex objects like records or classes.

## Token Properties

| Property | Type | Description |
|----------|------|-------------|
| `Token` | `Guid` | A unique identifier that changes with each refresh |
| `IsRefreshed` | `bool` | `true` if the token has been refreshed at least once |
| `ReturnValue` | `object?` | The value passed to the last `Refresh()` call |

## Refresh Tokens vs [Event Handlers](./07_EventHandlers.md)

| Feature | Event Handlers | Refresh Tokens |
|---------|---------------|----------------|
| **Trigger** | User interaction (click, blur, change) | Programmatic call to `Refresh()` |
| **Timing** | Synchronous, immediate | Can trigger async effects |
| **Scope** | Single component/element | Can trigger multiple effects |
| **Use Case** | Direct UI interactions | Background operations, coordinated updates |
| **Data Flow** | Event args (e.g., Event<Button>) | Return values via `ReturnValue` |

## Best Practices

### Use Return Values for Data Flow

```csharp
// Good: Pass important data through return values
refreshToken.Refresh(newProductId);

UseEffect(() =>
{
    if (refreshToken.ReturnValue is Guid productId)
    {
        // Load the new product
    }
}, [refreshToken]);
```

### Combine with OnMount Trigger

```csharp
// Load data on mount AND when manually refreshed
UseEffect(async () =>
{
    var data = await LoadData();
    // ...
}, [EffectTrigger.OnMount(), refreshToken]);
```

### Guard Against Unnecessary Actions

```csharp
// Check IsRefreshed to avoid running on initial render
UseEffect(() =>
{
    if (refreshToken.IsRefreshed)
    {
        ShowNotification("Data refreshed!");
    }
}, [refreshToken]);
```

## See Also

- [Effects](../../03_Hooks/02_Core/04_UseEffect.md) - Learn about the UseEffect hook
- [State Management](../../03_Hooks/02_Core/03_UseState.md) - Managing component state
- [Signals](./10_UseSignal.md) - Cross-component communication
