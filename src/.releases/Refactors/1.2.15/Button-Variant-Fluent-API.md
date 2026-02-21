# Button Variant & Icon Constructor Parameters Removed - v1.2.15

## Summary

The `Button` constructor no longer accepts `variant:` or `icon:` named parameters. Variants are now set via fluent extension methods (`.Destructive()`, `.Outline()`, `.Ghost()`, `.Secondary()`, `.Link()`, `.Success()`, `.Warning()`, `.Info()`), and icons are set via the `.Icon()` method. `ButtonVariant.Primary` is the default and no longer needs to be specified.

## What Changed

### Before (v1.2.14 and earlier)

```csharp
new Button("Delete", variant: ButtonVariant.Destructive)
new Button("Cancel", _ => Close(), variant: ButtonVariant.Outline)
new Button("Save", variant: ButtonVariant.Primary)
new Button(null, icon: Icons.Settings, variant: ButtonVariant.Ghost)
new Button("Search", icon: Icons.Search, variant: ButtonVariant.Outline)
```

### After (v1.2.15+)

```csharp
new Button("Delete").Destructive()
new Button("Cancel", _ => Close()).Outline()
new Button("Save") // Primary is default, no method needed
new Button().Icon(Icons.Settings).Ghost()
new Button("Search").Icon(Icons.Search).Outline()
```

> **Note:** `ButtonVariant.Primary` is the default variant. Remove `variant: ButtonVariant.Primary` entirely — no replacement method is needed.

## How to Find Affected Code

Run a `dotnet build`.

Or search for these patterns in the codebase:

### Pattern 1: variant parameter

```regex
variant:\s*ButtonVariant\.
```

### Pattern 2: icon constructor parameter

```regex
new Button\(.*icon:\s*Icons\.
```

## How to Refactor

### Variant Mapping

| Old Constructor Parameter | New Fluent Method |
|--------------------------|-------------------|
| `variant: ButtonVariant.Primary` | *(remove — it's the default)* |
| `variant: ButtonVariant.Secondary` | `.Secondary()` |
| `variant: ButtonVariant.Destructive` | `.Destructive()` |
| `variant: ButtonVariant.Outline` | `.Outline()` |
| `variant: ButtonVariant.Ghost` | `.Ghost()` |
| `variant: ButtonVariant.Link` | `.Link()` |
| `variant: ButtonVariant.Success` | `.Success()` |
| `variant: ButtonVariant.Warning` | `.Warning()` |
| `variant: ButtonVariant.Info` | `.Info()` |
| `variant: ButtonVariant.Ai` | `.Ai()` |

### Icon Migration

Replace `icon:` constructor parameter with `.Icon()` method:

**Before:**

```csharp
new Button("Search", icon: Icons.Search, variant: ButtonVariant.Outline)
```

**After:**

```csharp
new Button("Search").Icon(Icons.Search).Outline()
```

### Complex Example

**Before:**

```csharp
new DialogFooter(
    new Button("Cancel", _ => isOpen.Set(false), variant: ButtonVariant.Outline),
    new Button("Confirm", _ => Submit(), variant: ButtonVariant.Primary)
)
```

**After:**

```csharp
new DialogFooter(
    new Button("Cancel", _ => isOpen.Set(false)).Outline(),
    new Button("Confirm", _ => Submit())
)
```

## Key Refactoring Rules

1. Move `variant:` from constructor to fluent method call **after** the constructor
2. Move `icon:` from constructor to `.Icon()` method call
3. Remove `variant: ButtonVariant.Primary` entirely (it's the default)
4. Remove `null` as button text if it was only used to pass `icon:` — use parameterless `new Button()` instead

## Verification

After refactoring, run:

```bash
dotnet build
```

All usages should compile without errors.
