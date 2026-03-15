# IVYCHILD001: Adding Children to Leaf Widget

**Severity:** Error

## Description

Some widgets (e.g., `Button`, `Badge`, `DataTable`) are leaf widgets that do not support children. Attempting to add children via the `|` operator will throw `NotSupportedException` at runtime.

## Cause

```csharp
// ❌ Adding a child to a Button — triggers IVYCHILD001
public override object? Build()
{
    return new Button("Click me")
        | Text.Block("child"); // IVYCHILD001
}
```

## Fix

Remove the child. If you need to compose content, use a layout widget instead:

```csharp
// ✅ Use a layout to group elements
public override object? Build()
{
    return Layout.Vertical()
        | new Button("Click me")
        | Text.Block("Below the button");
}
```

## See Also

- [Layouts](../../../02_Widgets/02_Layouts/01_StackLayout.md)
