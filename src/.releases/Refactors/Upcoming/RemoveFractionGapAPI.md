---
title: Removing FractionGap Size API
---

# Removing FractionGap Size API

## Description
The `FractionGap` size API has been removed as it was unused and provided redundant functionality with standard `Fraction` plus layout gaps.

## Impact
This is a breaking change if you were using `Size.FractionGap()`.

## Migration
Replace any usage of `Size.FractionGap(x)` with `Size.Fraction(x)`. If gaps were an issue, ensure your `Layout` has a `Gap` configured.

### Before
```csharp
new Box().Width(Size.FractionGap(0.5f))
```

### After
```csharp
new Box().Width(Size.Fraction(0.5f))
```

## Verification
- Run `dotnet build` to ensure no C# compiler errors.
- Run `npm run build` in frontend to ensure no TypeScript errors.
