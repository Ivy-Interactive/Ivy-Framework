# ResizeablePanel Renamed to ResizablePanel - v1.2.15

## Summary

`ResizeablePanelGroup` and `ResizeablePanel` have been renamed to `ResizablePanelGroup` and `ResizablePanel` (fixing the typo in "Resizeable"). Additionally, the `ResizablePanel` constructor now accepts `Size?` instead of `int?` for sizing, using `Size.Fraction()` with support for `.Min()` and `.Max()` constraints.

## What Changed

### Before (v1.2.14 and earlier)

```csharp
new ResizeablePanelGroup(
    new ResizeablePanel(25, new Card("Left")),
    new ResizeablePanel(75, new Card("Right"))
)
```

### After (v1.2.15+)

```csharp
new ResizablePanelGroup(
    new ResizablePanel(Size.Fraction(0.25f), new Card("Left")),
    new ResizablePanel(Size.Fraction(0.75f), new Card("Right"))
)
```

> **Note:** The integer percentage (e.g. `25`) is replaced with a fractional value (e.g. `0.25f`). You can also add `.Min()` and `.Max()` constraints: `Size.Fraction(0.3f).Min(0.15f).Max(0.5f)`.

## How to Find Affected Code

Run a `dotnet build`.

Or search for these patterns:

### Pattern 1: Old class name

```regex
Resizeable(Panel|PanelGroup)
```

### Pattern 2: Extension methods

```regex
ResizeablePanelsExtensions
```

## How to Refactor

### Class Renames

| Old Name | New Name |
|----------|----------|
| `ResizeablePanelGroup` | `ResizablePanelGroup` |
| `ResizeablePanel` | `ResizablePanel` |
| `ResizeablePanelsExtensions` | `ResizablePanelsExtensions` |

### Size Parameter Conversion

| Old (int?) | New (Size?) |
|-----------|-------------|
| `20` | `Size.Fraction(0.2f)` |
| `25` | `Size.Fraction(0.25f)` |
| `30` | `Size.Fraction(0.3f)` |
| `40` | `Size.Fraction(0.4f)` |
| `50` | `Size.Fraction(0.5f)` |
| `60` | `Size.Fraction(0.6f)` |
| `70` | `Size.Fraction(0.7f)` |
| `75` | `Size.Fraction(0.75f)` |
| `null` | `null` |

### Complex Example with Min/Max

**Before:**

```csharp
new ResizeablePanelGroup(
    new ResizeablePanel(25, sidebar),
    new ResizeablePanel(75,
        new ResizeablePanelGroup(
            new ResizeablePanel(60, mainContent),
            new ResizeablePanel(40, detailsPanel)
        ).Vertical())
).Horizontal()
```

**After:**

```csharp
new ResizablePanelGroup(
    new ResizablePanel(Size.Fraction(0.25f).Min(0.1f).Max(0.4f), sidebar),
    new ResizablePanel(Size.Fraction(0.75f),
        new ResizablePanelGroup(
            new ResizablePanel(Size.Fraction(0.6f), mainContent),
            new ResizablePanel(Size.Fraction(0.4f), detailsPanel)
        ).Vertical())
).Horizontal()
```

## Key Refactoring Rules

1. Rename all `Resizeable` → `Resizable` (remove the extra "e")
2. Convert integer percentages to `Size.Fraction(value / 100f)` — e.g. `25` → `Size.Fraction(0.25f)`
3. Keep `null` as `null` for auto-sized panels
4. Optionally add `.Min()` / `.Max()` constraints for better UX

## Verification

After refactoring, run:

```bash
dotnet build
```

All usages should compile without errors.
