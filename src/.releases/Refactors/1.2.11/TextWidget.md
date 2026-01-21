# Text Widget Scale Changes - v1.2.11

## Summary

The static factory methods `Text.Small`, `Text.Large`, and `Text.ExtraLarge` have been removed. Text sizing is now handled via the `Scale` property on any text variant (usually `Text.P`).

## What Changed

### Before (v1.2.10 and earlier)

```csharp
Text.Small("Small Text");
Text.Large("Large Header");
Text.ExtraLarge("Huge Text");
```

### After (v1.2.11+)

```csharp
Text.P("Small Text").Scale(Scale.Small);
Text.P("Large Header").Scale(Scale.Large);
Text.P("Huge Text").Scale(Scale.ExtraLarge);
```

## How to Find Affected Code

Run a `dotnet build` to see compilation errors.

Or search for usages of the removed static methods:

### Pattern 1: Removed Static Methods

```regex
Text\.(Small|Large|ExtraLarge)\(
```

## How to Refactor

### Basic Pattern

**Before:**

```csharp
public override object? Build()
{
    return Layout.Vertical()
        | Text.Small("Fine print");
}
```

**After:**

```csharp
public override object? Build()
{
    return Layout.Vertical()
        // Use Text.P() as the base, then apply Scale
        | Text.P("Fine print").Scale(Scale.Small);
}
```

## Key Refactoring Rules

1. **Change Entry Point**: Change `Text.Small(...)` to `Text.P(...)` (or `Text.Label(...)`, etc.).
2. **Apply Scale**: Chain `.Scale(Scale.Small)` (or `.Small()`) to the builder.

## Quick Refactor Commands

### Using IDE

- **Visual Studio/Rider**: Search for `Text.Small` and replace with `Text.P`. Then append `.Scale(Scale.Small)`.

### Using Command Line

```bash
# Check for usages
grep -r "Text.Small" .
grep -r "Text.Large" .
```

## Verification

After refactoring, run:

```bash
dotnet build
```

The code should compile without errors.
