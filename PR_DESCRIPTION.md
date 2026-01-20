# PR: [Text] Make Literal return TextBlock directly (remove styling methods)

Closes #2034

## Problem

`Text.Literal()` was using the shared `TextBuilder` class which exposes styling methods like `.Bold()`, `.Italic()`, etc. This contradicts the semantic meaning of "literal" — text that should be rendered without any formatting.

Users could accidentally apply styles to literal text:

```csharp
Text.Literal("Hello").Bold() // This should NOT be allowed
```

## Solution

Changed `Text.Literal()` to return `TextBlock` directly instead of `TextBuilder`.
Since `TextBlock` does not support fluent text styling methods (unlike `TextBuilder`), this enforces compile-time safety against styling. 

**Note**: `TextBlock` DOES support generic layout methods like `.Width()`, so existing layout configurations remain valid.

Attempting to apply text styles to a `Literal` now produces a compiler error:

```
CS1061: 'TextBlock' does not contain a definition for 'Bold'
```

## Changes

### Backend
- Changed `Text.Literal()` return type from `TextBuilder` to `TextBlock`
- `Text.Literal()` now returns `new TextBlock(content, TextVariant.Literal)`
- Removed unnecessary `LiteralBuilder` class

### Migration
Removed text styling from existing `Text.Literal()` usages in the codebase:
- `09_UseQuery.md` — removed `.Bold()`

*Note: `.Width()` usages were preserved as they are valid layout configurations.*

## Breaking Change

If external users were using `Text.Literal("...").Bold()` or similar text styling methods, they will get a compile error. They should either:
- Remove the text styling (if literal text is intended)
- Use `Text.Inline()` or `Text.P()` instead (if styling is needed)

Layout methods like `.Width()` continue to work.
