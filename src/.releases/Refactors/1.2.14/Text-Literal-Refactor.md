# Text.Literal() No Longer Supports Styling Methods - v1.2.14

## Summary

The `Text.Literal()` method has been streamlined and now returns a `TextBlock` directy instead of a `TextBuilder`. This means you can no longer chain styling methods like `.Bold()`, `.Italic()`, or `.Color()` immediately after `Text.Literal()`.

## What Changed

### Before (v1.2.13 and earlier)

```csharp
Text.Literal("Hello").Bold().Color(Colors.Red)
```

### After (v1.2.14+)

`Text.Literal("Hello")` returns `TextBlock`. `TextBlock` might not support the fluent builder methods in the same way, or `Text.Literal` was intended for raw unstyled text.

## How to Find Affected Code

Run a `dotnet build`.

Or search for:

```regex
Text\.Literal\(.*\)\.
```

## How to Refactor

### Use TextBlock directly for styling

**Before:**

```csharp
return Text.Literal("Warning").Color(Colors.Red).Bold();
```

**After:**

```csharp
return new TextBlock("Warning")
    .Color(Colors.Red)
    .Weight(FontWeight.Bold); 
    // Adjust method names based on actual TextBlock API availability
```

### Use Text.Rich for complex styling

If you need rich text capabilities:

```csharp
return Text.Rich(t => t.Span("Warning").Bold().Color(Colors.Red));
```

## Verification

After refactoring, run:

```bash
dotnet build
```
