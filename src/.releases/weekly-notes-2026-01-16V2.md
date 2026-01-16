# Ivy Framework Weekly Notes - Week of 2026-01-16

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## Breaking Changes

### Text Widget Refactor: Scale & Margins

We have cleaned up the `Text` widget API to be more consistent and predictable.

#### Scale Property

Previously, text sizing was handled via specific methods like `.Small()`, `.Large()`, or `Text.H1()`. We have unified this under a `Scale` property.

- **Removed**: `Text.Small()`, `Text.Large()`, and similar specific size variants.
- **Added**: `Text.P().Scale(0.8)` or `.Scale(Scale.Small)`.

```csharp
// Old
Text.P("Small text").Small()

// New
Text.P("Small text").Scale(0.8) // or .Scale(Scale.Small)
```

#### Consistent Margins

We have removed "strange" default margins from `Text` widgets to provide more control over layout. You should now use parent layouts (like `Layout.Vertical().Gap()`) or explicit margins if needed.

## Improvements

### LLMs.txt Included in NuGet Package

The `llms.txt` file is now included in the Ivy Framework NuGet package. This file provides a concise, machine-readable overview of the framework, making it easier for AI agents and LLMs to understand and work with Ivy.

### Chart Sorting API

We have added a `SortBy` API for charts, allowing you to sort X-axis data.

```csharp
new BarChart(data)
    .SortBy(x => x.Value, SortDirection.Descending)
```

### CodeInput Enhancements

- **YAML Support**: The `CodeInput` widget now supports YAML syntax highlighting.
- **Copy Button**: We've polished the "Copy" button in `CodeInput` to use a consistent Lucide icon, fixing previous UI glitches.
