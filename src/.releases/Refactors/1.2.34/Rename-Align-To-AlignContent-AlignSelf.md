# Rename Align → AlignContent / AlignSelf

## Summary

The `Align` property on StackLayout, TableCell, and FloatingPanel has been renamed to clarify intent:
- `AlignContent` — aligns children within the container (StackLayout, TableCell)
- `AlignSelf` — positions the widget within its parent (FloatingPanel)

## What Changed

### Backend (C#)

- `StackLayout.Align` → `StackLayout.AlignContent`
- `TableCell.Align` → `TableCell.AlignContent`
- `FloatingPanel.Align` → `FloatingPanel.AlignSelf`
- Extension methods `.Align()` renamed accordingly

### Frontend (TypeScript)

- `align` prop → `alignContent` on StackLayout and TableCell widgets
- `align` prop → `alignSelf` on FloatingPanel widget

## Before

new StackLayout() { Align = Align.Center }
new TableCell().Align(Align.Left)
new FloatingPanel(align: Align.BottomRight)

## After

new StackLayout() { AlignContent = Align.Center }
new TableCell().AlignContent(Align.Left)
new FloatingPanel(alignSelf: Align.BottomRight)

## Migration Path

1. Replace `.Align(` with `.AlignContent(` on StackLayout and TableCell
2. Replace `.Align(` with `.AlignSelf(` on FloatingPanel
3. Replace property initializer `Align =` with `AlignContent =` / `AlignSelf =`
