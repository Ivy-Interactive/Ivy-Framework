# IVYCHILD002: Adding Multiple Children to Single-Child Widget

**Severity:** Warning

## Description

Some widgets (e.g., `Card`, `Sheet`) only support a single child. Adding multiple children via chained `|` operators will throw `NotSupportedException` at runtime.

## Cause

```csharp
// ❌ Multiple children on a Card — triggers IVYCHILD002
public override object? Build()
{
    return new Card()
        | Text.Block("First")
        | Text.Block("Second"); // IVYCHILD002
}
```

## Fix

Wrap multiple elements in a layout:

```csharp
// ✅ Single layout child containing multiple elements
public override object? Build()
{
    return new Card()
        | (Layout.Vertical()
            | Text.Block("First")
            | Text.Block("Second"));
}
```

## See Also

- [Card](../../../02_Widgets/03_Common/04_Card.md)
- [Layouts](../../../02_Widgets/02_Layouts/01_StackLayout.md)
