# TextArea Renamed to Textarea - v1.2.17

## Summary

The `ToTextAreaInput()` extension method has been renamed to `ToTextareaInput()` (lowercase 'a') to standardize naming across the codebase. This aligns with:
- The HTML `<textarea>` element specification
- The existing C# enum `TextInputVariants.Textarea`
- UI libraries like shadcn and Radix that use `Textarea`

## What Changed

### C# API Change

| Before (v1.2.16 and earlier) | After (v1.2.17+) |
|---|---|
| `ToTextAreaInput()` | `ToTextareaInput()` |

### Before (v1.2.16 and earlier)

```csharp
var state = UseState("");
return state.ToTextAreaInput(placeholder: "Enter description...");
```

### After (v1.2.17+)

```csharp
var state = UseState("");
return state.ToTextareaInput(placeholder: "Enter description...");
```

## How to Find Affected Code

Run `dotnet build`.

Or search for this pattern in the codebase:

```regex
\.ToTextAreaInput\(
```

## How to Refactor

Replace all instances of `ToTextAreaInput` with `ToTextareaInput`.

**Before:**

```csharp
public override object? Build()
{
    var description = UseState("");
    var notes = UseState<string?>(null);

    return new VStack(
        description.ToTextAreaInput(placeholder: "Description").Rows(4),
        notes.ToTextAreaInput(placeholder: "Notes")
    );
}
```

**After:**

```csharp
public override object? Build()
{
    var description = UseState("");
    var notes = UseState<string?>(null);

    return new VStack(
        description.ToTextareaInput(placeholder: "Description").Rows(4),
        notes.ToTextareaInput(placeholder: "Notes")
    );
}
```

## Alternative: Using Variant Directly

You can also use the `TextInputVariants.Textarea` enum directly:

```csharp
// Using the variant enum
state.ToTextInput(variant: TextInputVariants.Textarea)

// Or with the fluent API
state.ToTextInput().Variant(TextInputVariants.Textarea)
```

## Verification

After refactoring, run:

```bash
dotnet build
```

All usages should compile without errors.
