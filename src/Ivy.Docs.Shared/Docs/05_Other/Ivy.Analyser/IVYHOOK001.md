# IVYHOOK001: Invalid Hook Usage

**Severity:** Error

## Description

Ivy hooks must be called directly inside the `Build()` method of a `ViewBase` class (or a custom hook). This error fires when a hook is called from a regular method, a service class, or any other location outside of `Build()`.

## Cause

```csharp
// ❌ Hook called in a helper method — triggers IVYHOOK001
public override object? Build()
{
    Initialize();
    return new Button();
}

private void Initialize()
{
    var state = UseState(false); // IVYHOOK001
}
```

## Fix

Move the hook call to the top level of `Build()`:

```csharp
// ✅ Hook called directly inside Build()
public override object? Build()
{
    var state = UseState(false);
    return new Button("Click me");
}
```

## See Also

- [Rules of Hooks](../../../03_Hooks/02_RulesOfHooks.md)
