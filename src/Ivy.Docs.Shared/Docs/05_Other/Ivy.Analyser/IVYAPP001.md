# IVYAPP001: App Must Have Parameterless Constructor

**Severity:** Error

## Description

Classes decorated with `[App]` are instantiated via `Activator.CreateInstance` at runtime, which requires a parameterless constructor. Use `UseService<T>()` inside `Build()` for dependency injection instead of constructor parameters.

## Cause

```csharp
// ❌ App with constructor parameters — triggers IVYAPP001
[App]
public class MyApp(IMyService service) : ViewBase
{
    public override object? Build()
    {
        return Text.Block(service.GetData());
    }
}
```

## Fix

Remove constructor parameters and use `UseService<T>()` inside `Build()`:

```csharp
// ✅ Parameterless constructor with UseService
[App]
public class MyApp : ViewBase
{
    public override object? Build()
    {
        var service = UseService<IMyService>();
        return Text.Block(service.GetData());
    }
}
```

## See Also

- [Apps](../../../01_Onboarding/02_Concepts/10_Apps.md)
- [UseService](../../../03_Hooks/02_Core/10_UseService.md)
