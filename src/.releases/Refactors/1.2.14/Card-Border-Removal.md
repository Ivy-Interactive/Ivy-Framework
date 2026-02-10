# Card Border Customization Removed - v1.2.14

## Summary

The `Card` widget no longer supports border customization methods. Methods like `.BorderThickness()`, `.BorderStyle()`, `.BorderColor()`, and `.BorderRadius()` have been removed to enforce consistent styling across the application.

## What Changed

### Before (v1.2.13 and earlier)
```csharp
new Card()
    .BorderColor(Colors.Red)
    .BorderThickness(2)
    .BorderRadius(8)
    .Content(...)
```

### After (v1.2.14+)
```csharp
new Card()
    .Content(...)
```

> **Note:** The Card widget now uses the default theme border styles automatically.

## How to Find Affected Code

Run a `dotnet build`.

Or search for these patterns in the codebase:

### Pattern 1: Card Border methods
```regex
\.Border(Thickness|Style|Color|Radius)\(
```

## How to Refactor

### Standard Pattern

**Before:**
```csharp
public override object? Build()
{
    return new Card()
        .BorderColor(Colors.Blue)
        .BorderThickness(1)
        .Content(new TextBlock("Hello"));
}
```

**After:**
```csharp
public override object? Build()
{
    // Border customization is removed. 
    // If you strictly need a custom border, consider wrapping content in a standard container with border styling,
    // or accept the default Card styling for consistency.
    return new Card()
        .Content(new TextBlock("Hello"));
}
```

## Verification

After refactoring, run:
```bash
dotnet build
```

All usages should compile without errors.
