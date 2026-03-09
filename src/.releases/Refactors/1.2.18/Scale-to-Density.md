# Scale to Density Rename - v1.2.18

## Summary

This release renames the `Scale` enum to `Density` across the entire framework. The `.Scale()` extension method is now `.Density()`, and the `Scale` property on widgets is now `Density`. The convenience methods `.Small()`, `.Medium()`, and `.Large()` remain unchanged.

## What Changed

### Type and Property Renames

| Before (v1.2.17 and earlier) | After (v1.2.18+) |
|---|---|
| `Scale` enum | `Density` enum |
| `Scale.Small` | `Density.Small` |
| `Scale.Medium` | `Density.Medium` |
| `Scale.Large` | `Density.Large` |
| `.Scale(Scale.Small)` | `.Density(Density.Small)` |
| `widget.Scale` property | `widget.Density` property |

### Before (v1.2.17 and earlier)

```csharp
new Button("Click me")
    .Scale(Scale.Small);

new Badge("Tag")
    .Scale(Scale.Large);

var form = model.ToForm()
    .Scale(Scale.Medium);
```

### After (v1.2.18+)

```csharp
new Button("Click me")
    .Density(Density.Small);

new Badge("Tag")
    .Density(Density.Large);

var form = model.ToForm()
    .Density(Density.Medium);
```

### Convenience Methods (Unchanged)

The convenience methods `.Small()`, `.Medium()`, and `.Large()` continue to work as before:

```csharp
// These still work exactly the same
new Button("Small").Small();
new Button("Medium").Medium();
new Button("Large").Large();
```

## How to Find Affected Code

Run `dotnet build`.

Or search for these patterns in the codebase:

```regex
\.Scale\(Scale\.
```

```regex
Scale\.(Small|Medium|Large)
```

## How to Refactor

Replace all instances of `Scale` with `Density`:

**Before:**

```csharp
public override object? Build()
{
    return Layout.Vertical()
        | new Button("Save").Scale(Scale.Small)
        | new Badge("Important").Scale(Scale.Large)
        | model.ToForm().Scale(Scale.Medium);
}
```

**After:**

```csharp
public override object? Build()
{
    return Layout.Vertical()
        | new Button("Save").Density(Density.Small)
        | new Badge("Important").Density(Density.Large)
        | model.ToForm().Density(Density.Medium);
}
```

## Verification

After refactoring, run:

```bash
dotnet build
```

All usages should compile without errors.
