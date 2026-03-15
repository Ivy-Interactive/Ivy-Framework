---
description: Rename input variant enums from plural to singular.
---
# Singular Enum Naming

## Summary
To maintain consistency across the Ivy Framework, 9 input variant enums have been renamed from plural (`*Variants`) to singular (`*Variant`). This aligns them with other standard styling enums like `ButtonVariant`, `BadgeVariant`, and `CalloutVariant`.

## Changes

The following enums have been renamed:

| Old (Plural) | New (Singular) |
|---|---|
| `TextInputVariants` | `TextInputVariant` |
| `SelectInputVariants` | `SelectInputVariant` |
| `NumberInputVariants` | `NumberInputVariant` |
| `FileInputVariants` | `FileInputVariant` |
| `FeedbackInputVariants` | `FeedbackInputVariant` |
| `DateTimeInputVariants` | `DateTimeInputVariant` |
| `ColorInputVariants` | `ColorInputVariant` |
| `CodeInputVariants` | `CodeInputVariant` |
| `BoolInputVariants` | `BoolInputVariant` |

## Refactoring Instructions

For downstream connection projects replacing these usages, you can perform a simple find-and-replace operation across your CSharp files.

1. Search for `TextInputVariants` and replace with `TextInputVariant`
2. Search for `SelectInputVariants` and replace with `SelectInputVariant`
3. Search for `NumberInputVariants` and replace with `NumberInputVariant`
4. Search for `FileInputVariants` and replace with `FileInputVariant`
5. Search for `FeedbackInputVariants` and replace with `FeedbackInputVariant`
6. Search for `DateTimeInputVariants` and replace with `DateTimeInputVariant`
7. Search for `ColorInputVariants` and replace with `ColorInputVariant`
8. Search for `CodeInputVariants` and replace with `CodeInputVariant`
9. Search for `BoolInputVariants` and replace with `BoolInputVariant`
