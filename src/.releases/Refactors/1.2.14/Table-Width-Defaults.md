# Table Default Width Behavior - v1.2.14

## Summary

`Table` widgets now default to `Size.Full()` width instead of a calculated "smart" width. Previously, the framework attempted to calculate table widths based on column content, which could lead to inconsistent layouts.

## What Changed

### Before (v1.2.13 and earlier)

The table width was automatically calculated (often between 100-400 units) based on column content.

### After (v1.2.14+)

The table defaults to consuming the full available width of its container (`Size.Full()`).

## How to Find Affected Code

This is a **behavioral change**, not a compilation error. You need to visualy inspect `Table` usages, especially those that relied on the table being compact.

Search for:

```regex
new Table
```

## How to Refactor

### If you want the old "compact" behavior (approximated)

**Before:**

```csharp
new Table()
    .AddColumn("Name", x => x.Name)
    // implicitly relied on auto-width
```

**After:**

```csharp
new Table()
    .Width(Size.Exact(400)) // or another specific size if needed
    .AddColumn("Name", x => x.Name)
```

### If you accept the new behavior

No changes are needed in code, but your UI layout might look different (table will stretch).

## Verification

Manually verify screens containing `Table` widgets to ensure the layout is still acceptable with full-width tables.
