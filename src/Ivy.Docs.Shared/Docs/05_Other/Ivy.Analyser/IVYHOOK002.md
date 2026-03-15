# IVYHOOK002: Hook Called Conditionally

**Severity:** Warning

## Description

Hooks must be called in the same order on every render. Placing a hook inside an `if` statement, ternary expression, `try/catch`, or `using` block means it may not execute on every render, breaking hook ordering.

## Cause

```csharp
// ❌ Hook inside an if statement — triggers IVYHOOK002
public override object? Build()
{
    if (someCondition)
    {
        var state = UseState(false); // IVYHOOK002
    }
    return new Button();
}
```

```csharp
// ❌ Hook inside a ternary — triggers IVYHOOK002
public override object? Build()
{
    var result = condition ? UseState(0) : UseState(1); // IVYHOOK002
    return new Button();
}
```

## Fix

Always call hooks unconditionally, then use the condition afterwards:

```csharp
// ✅ Hook called unconditionally
public override object? Build()
{
    var state = UseState(false);

    return someCondition
        ? Text.Block($"Value: {state.Value}")
        : Text.Block("Hidden");
}
```

## See Also

- [Rules of Hooks](../../../03_Hooks/02_RulesOfHooks.md)
