---
description: Rename event handlers from Handle* to On*.
---
# Event Handler Naming Convention Refactor

## Summary
The Ivy Framework is transitioning to a consistent `On*` naming convention for all event handlers. As part of this change, several internal and public event handler methods have been renamed from the `Handle*` prefix to `On*`.

## Changes
The following extension methods and internal React handlers have been renamed:

### C# Extension Methods
- **Terminal**: `HandleInput`, `HandleResize`, `HandleLinkClick` -> `OnInput`, `OnResize`, `OnLinkClick`
- **TiptapInput**: `HandleFocus`, `HandleBlur` -> `OnFocus`, `OnBlur`
- **Map**: Various `Handle...` events (e.g., `HandleMapClick`, `HandleMarkerClick`) -> `On...` (e.g., `OnMapClick`, `OnMarkerClick`)
- **Iframe**: `HandleMessageReceived` -> `OnMessageReceived`
- **ScreenshotFeedback**: `HandleSave`, `HandleCancel` -> `OnSave`, `OnCancel`

### Frontend (React)
- **ScreenshotFeedback**: `handleShapeAdd`, `handleUndo`, `handleSave`, `handleCancel` -> `onShapeAdd`, `onUndo`, `onSave`, `onCancel`

## Refactoring Instructions
For existing Ivy applications using these widgets:

1. Update any C# code referencing the old `Handle...` extension methods to use the new `On...` equivalents.
2. If you are using custom variants or directly interacting with the frontend of `ScreenshotFeedback`, update the prop names to use the new `on...` prefixed handlers.
