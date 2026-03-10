# Refactor: Scale to Density

The `Scale` enum and all associated APIs have been renamed to `Density` to avoid ambiguity with chart scales, DPI scaling, etc.

## Migration Guide

- `Ivy.Scale` enum → `Ivy.Density` enum (`Small`, `Medium`, `Large` values are identical)
- `.Scale()` fluent method → `.Density()` on all widgets
- `.Small()`, `.Medium()`, `.Large()` fluent helpers are unchanged
- `Ivy.Scale.Small/Medium/Large` → `Ivy.Density.Small/Medium/Large`

For a quick migration, you can run a global find-and-replace in your `App` codebase for `.Scale(` -> `.Density(` and `Scale.` -> `Density.`.
