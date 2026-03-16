---
description: Rename FileInput Standard variant to Default.
---
# FileInput Standard to Default Refactor

## Summary
The `Standard` variant for the `FileInput` widget has been renamed to `Default`. This variant provides a button-based file selection interface as opposed to the `Drop` zone.

## Changes
- `FileInputVariant.Standard` renamed to `FileInputVariant.Default`.
- The `variant` prop value in `FileInputWidget` (frontend) changed from `"Standard"` to `"Default"`.

## Refactoring Instructions
For existing Ivy applications using the `Standard` variant:

1. In C# code, replace `FileInputVariant.Standard` with `FileInputVariant.Default`.
2. If using the variant in custom frontend components, replace `"Standard"` with `"Default"`.
