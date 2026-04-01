# Rename CardHoverVariant → HoverEffect

## Summary

`CardHoverVariant` has been renamed to `HoverEffect` and moved from `Card.cs` to `Ivy.Shared`. This enum is now used by Card, Box, and Image widgets.

## What Changed

### Backend (C#)

- `CardHoverVariant` → `HoverEffect`
- Enum moved from `Ivy.Widgets.Card` namespace scope to `Ivy` namespace (in `Shared/HoverEffect.cs`)
- Extension method signatures unchanged (`.Hover(HoverEffect variant)`)

### Frontend (TypeScript)

- `BoxHoverVariant` type → `HoverEffect` type

## Before

box.Hover(CardHoverVariant.Shadow)
image.Hover(CardHoverVariant.Pointer)

## After

box.Hover(HoverEffect.Shadow)
image.Hover(HoverEffect.Pointer)

## Migration Path

1. Replace all `CardHoverVariant` with `HoverEffect` in C# code
2. No namespace change needed (both are in `Ivy` namespace)
