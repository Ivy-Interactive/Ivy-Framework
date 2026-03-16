# IVYHOOK005: Hook Not at Top of Build Method

**Severity:** Warning

## Description

All hooks must be called at the very top of the `Build()` method, before any other non-hook statements. This ensures hooks are called in a consistent order on every render and prevents accidental early returns from skipping hook calls.

## Cause

```csharp
// ❌ Hook after a non-hook statement — triggers IVYHOOK005
public override object? Build()
{
    var x = SomeMethod();        // Non-hook statement
    var state = UseState(false); // IVYHOOK005
    return new Button();
}
```

```csharp
// ❌ Hook after early return — triggers IVYHOOK005
public override object? Build()
{
    if (user == null) return Text.Block("Login required");
    var state = UseState(0); // IVYHOOK005
    return Text.Block($"Count: {state.Value}");
}
```

## Fix

Move all hook calls to the top of `Build()`, before any other logic:

```csharp
// ✅ Hooks first, then logic
public override object? Build()
{
    var state = UseState(0);

    if (user == null) return Text.Block("Login required");

    return Text.Block($"Count: {state.Value}");
}
```

## See Also

- [Rules of Hooks](../../../03_Hooks/02_RulesOfHooks.md)
