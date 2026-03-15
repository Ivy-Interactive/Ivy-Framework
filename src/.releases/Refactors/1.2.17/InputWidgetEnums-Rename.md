# Input Widget Enums Renamed to Plural Variants - v1.2.17

## Summary

All input widget enums have been renamed to use plural names with the `Variants` suffix (e.g., `TextInputs` -> `TextInputVariants`). This change establishes consistency with the existing `{Widget}Variant` convention used across the framework (like `ButtonVariant`, `BadgeVariant`, `CalloutVariant`, etc.), while specifically using the plural form for inputs.

## What Changed

The following enums were renamed:

| Before (v1.2.16 and earlier) | After (v1.2.17+) |
|---|---|
| `TextInputs` | `TextInputVariants` |
| `SelectInputs` | `SelectInputVariants` |
| `NumberInputs` | `NumberInputVariants` |
| `ColorInputs` | `ColorInputVariants` |
| `DateTimeInputs` | `DateTimeInputVariants` |
| `BoolInputs` | `BoolInputVariants` |
| `FileInputs` | `FileInputVariants` |
| `CodeInputs` | `CodeInputVariants` |
| `FeedbackInputs` | `FeedbackInputVariants` |

### Before (v1.2.16 and earlier)

```csharp
myState.ToTextInput().Variant(TextInputs.Email);
myState.ToColorInput().Variant(ColorInputs.Swatch);
```

### After (v1.2.17+)

```csharp
myState.ToTextInput().Variant(TextInputVariants.Email);
myState.ToColorInput().Variant(ColorInputVariants.Swatch);
```

## How to Find Affected Code

Run `dotnet build`.

Or search for these patterns in the codebase using regex:

```regex
(Text|Select|Number|Color|DateTime|Bool|File|Code|Feedback)Inputs\b
```

## How to Refactor

Replace all instances of the old enum names with their new `...Variants` counterparts.

If you have custom classes named `...Variants` that were previously conflicting with this new convention (such as `ColorInputVariants`), you will also need to rename them (e.g., to `ColorInputVariantTests`).

**Before:**

```csharp
public override object? Build()
{
    var textInput = new TextInput(placeholder: "Search...", variant: TextInputs.Search);
    return textInput;
}
```

**After:**

```csharp
public override object? Build()
{
    var textInput = new TextInput(placeholder: "Search...", variant: TextInputVariants.Search);
    return textInput;
}
```

## Verification

After refactoring, run:

```bash
dotnet build
```

All usages should compile without errors.
