# IVYHOOK001B: Hook Used in Nested Closure

**Severity:** Error

## Description

Ivy hooks cannot be called inside lambdas, local functions, or anonymous methods — even when those are defined within `Build()`. Hooks must always execute in the same order on every render, and closures may execute conditionally, multiple times, or not at all.

## Cause

```csharp
// ❌ Hook inside a lambda — triggers IVYHOOK001B
public override object? Build()
{
    var items = UseState(new[] { "a", "b", "c" });

    var urls = items.Value.Select(item =>
        UseDownload(() => Encoding.UTF8.GetBytes(item), "text/plain", $"{item}.txt") // IVYHOOK001B
    );

    return Layout.Vertical();
}
```

```csharp
// ❌ Hook inside a local function — triggers IVYHOOK001B
public override object? Build()
{
    void SetupState()
    {
        var s = UseState(0); // IVYHOOK001B
    }

    SetupState();
    return new Button();
}
```

## Fix

Move the hook to the top level of `Build()`. If you need a hook per item in a collection, extract a child component so each instance manages its own hooks:

```csharp
// ✅ Extract a child component per item
public override object? Build()
{
    var items = UseState(new[] { "a", "b", "c" });
    return Layout.Vertical(
        items.Value.Select((item, i) => new ItemDownloadView(item).Key($"dl-{i}"))
    );
}

public class ItemDownloadView(string item) : ViewBase
{
    public override object? Build()
    {
        var url = UseDownload(
            () => Encoding.UTF8.GetBytes(item), "text/plain", $"{item}.txt");

        return url.Value != null
            ? new Button($"Download {item}").Url(url.Value)
            : Text.Block("Preparing...");
    }
}
```

## See Also

- [Rules of Hooks](../../../03_Hooks/02_RulesOfHooks.md)
- [UseDownload FAQ](../../../03_Hooks/02_Core/15_UseDownload.md#faq)
