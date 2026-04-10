# Ivy Framework API Reference

When suggesting Ivy Framework code in plans, verify APIs using these patterns:

## Layout Alignment

**Correct**: `Layout.Horizontal().AlignContent(Align.Center)`
- Method: `LayoutView.AlignContent(Align align)`
- Enum: `Align` (not `Alignment`)
- Values: Left, Right, Center, TopLeft, TopRight, TopCenter, BottomLeft, BottomRight, BottomCenter, Stretch, SpaceBetween, SpaceAround, SpaceEvenly
- Shorthand: `.Center()`, `.Left()`, `.Right()`

**Incorrect**: 
- `AlignItems(Alignment.Center)` — No such method
- `Alignment` enum — Doesn't exist

## Verification Methods

Before suggesting Ivy Framework APIs in plan code:
1. Use `Grep` to search actual usage in `D:\Repos\_Ivy\Ivy-Framework\src\Ivy`
2. Read the actual widget/view source file
3. Check AGENTS.md for documented patterns

## Common Widgets Reference

See `AGENTS.md` in repo root for:
- Layout helpers (Layout.Vertical, Layout.Horizontal, Layout.Grid, Layout.Wrap)
- Common widgets (Button, Card, Badge, Sheet, Table, List, etc.)
- Property patterns ([Prop], [Event] attributes)
