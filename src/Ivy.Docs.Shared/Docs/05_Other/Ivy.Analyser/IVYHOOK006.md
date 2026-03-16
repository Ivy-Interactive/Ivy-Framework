# IVYHOOK006: Hook Result Stored in Class Member

**Severity:** Error

## Description

Hook results must not be stored in class fields or properties. The state object is captured once and reused across renders, causing hooks to receive wrong indices and corrupting the reactive system.

## Cause

```csharp
// ❌ Hook result assigned to a field — triggers IVYHOOK006
public class MyView : ViewBase
{
    private IState<int> _count;

    public override object? Build()
    {
        _count = UseState(0); // IVYHOOK006
        return new Button("Click", _ => _count.Set(_count.Value + 1));
    }
}
```

## Fix

Use a local variable instead:

```csharp
// ✅ Hook result in a local variable
public class MyView : ViewBase
{
    public override object? Build()
    {
        var count = UseState(0);
        return new Button("Click", _ => count.Set(count.Value + 1));
    }
}
```

## See Also

- [Rules of Hooks](../../../03_Hooks/02_RulesOfHooks.md)
