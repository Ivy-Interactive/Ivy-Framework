# IVYCHILD003: Wrong Child Type for Widget

**Severity:** Error

## Description

Some widgets restrict the types of children they accept via the `[ChildType]` attribute. Passing a child of an incompatible type via the `|` operator will throw `NotSupportedException` at runtime.

## Cause

```csharp
// ❌ Wrong child type — triggers IVYCHILD003
public override object? Build()
{
    return new SidebarMenu()
        | new Button("Not a menu item"); // IVYCHILD003 — expects SidebarMenuItem
}
```

## Fix

Use the correct child type as specified by the widget:

```csharp
// ✅ Correct child type
public override object? Build()
{
    return new SidebarMenu()
        | new SidebarMenuItem("Home", Icons.Home);
}
```

## See Also

- [Sidebar Layout](../../../02_Widgets/02_Layouts/06_SidebarLayout.md)
