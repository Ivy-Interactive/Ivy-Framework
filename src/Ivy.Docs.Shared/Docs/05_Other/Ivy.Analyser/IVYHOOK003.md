# IVYHOOK003: Hook Called in Loop

**Severity:** Warning

## Description

Hooks cannot be called inside `for`, `foreach`, `while`, or `do-while` loops. The number of loop iterations may change between renders, which would change the hook call order and corrupt state.

## Cause

```csharp
// ❌ Hook inside a foreach — triggers IVYHOOK003
public override object? Build()
{
    var items = UseState(new List<string> { "A", "B" });

    foreach (var item in items.Value)
    {
        var count = UseState(0); // IVYHOOK003
    }

    return Layout.Vertical();
}
```

## Fix

Extract a child component so each item gets its own isolated hook state:

```csharp
// ✅ Each item is a separate component
public override object? Build()
{
    var items = UseState(new List<string> { "A", "B" });

    return Layout.Vertical(
        items.Value.Select((item, i) =>
            new ItemView(item).Key($"item-{i}"))
    );
}

public class ItemView(string name) : ViewBase
{
    public override object? Build()
    {
        var count = UseState(0);
        return Layout.Horizontal(
            Text.Block($"{name}: {count.Value}"),
            new Button("+", _ => count.Set(count.Value + 1))
        );
    }
}
```

## See Also

- [Rules of Hooks](../../../03_Hooks/02_RulesOfHooks.md)
