# IVYHOOK004: Hook Called in Switch Statement

**Severity:** Warning

## Description

Hooks cannot be called inside a `switch` statement. Only one case branch executes per render, so hooks inside a case may not run on every render, breaking hook ordering.

## Cause

```csharp
// ❌ Hook inside a switch case — triggers IVYHOOK004
public override object? Build()
{
    switch (mode)
    {
        case "edit":
            var draft = UseState(""); // IVYHOOK004
            return draft.ToTextInput();
    }
    return Text.Block("Read-only");
}
```

## Fix

Call all hooks unconditionally at the top of `Build()`, then branch in the return:

```csharp
// ✅ Hooks called unconditionally
public override object? Build()
{
    var draft = UseState("");

    return mode switch
    {
        "edit" => draft.ToTextInput(),
        _ => Text.Block("Read-only")
    };
}
```

## See Also

- [Rules of Hooks](../../../03_Hooks/02_RulesOfHooks.md)
