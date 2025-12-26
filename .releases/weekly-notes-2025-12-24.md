# Ivy Framework Weekly Notes - Week of 2025-12-24

## API Changes

### PivotTable Sorting Behavior

The `PivotTable` widget no longer automatically sorts results alphabetically by the first dimension. Previously, results were always sorted by the first dimension column, which could be unexpected and made it difficult to preserve custom ordering or natural data order. Results now maintain the order they appear in the source data

```csharp
var pivot = new PivotTable<SalesData>()
    .AddDimension(x => x.Category)  // Preserves order from source
    .AddCalculation(x => x.Amount.Sum());
// Sort explicitly if needed: results.OrderBy(r => r["Category"])
```

<img width="2240" height="978" alt="image" src="https://github.com/user-attachments/assets/9ef5c805-f624-428b-bdea-6af8da6eb1ae" />

### TextInput Prefix/Suffix Refactoring

The `TextInput` widget's prefix and suffix API has been refactored for better simplicity. The previous discriminated union type `PrefixSuffix` has been replaced with a simpler `Affix` record that supports both text and icons.

- `PrefixSuffix.Text` and `PrefixSuffix.Icon` - `Affix` with `Text` and `Icon` properties
- New extension methods: `ToAffix()` for both strings and Icons

```csharp
widget.Prefix("$")
widget.Suffix(Icons.Search)
```

<img width="2314" height="313" alt="image" src="https://github.com/user-attachments/assets/f55b9250-16b2-4dbe-a4b2-a6998f98c0c5" />

## Framework Improvements

### AOT (Ahead-Of-Time) Compilation Support

Ivy Framework now supports AOT compilation for better performance and faster startup times. This enables native compilation of your Ivy apps, resulting in smaller deployments and improved runtime performance.

- Added AOT compatibility to core framework and Ivy.Filters
- Implemented static YAML context for AOT-safe serialization
- Updated project files with `IsAotCompatible` and `IsTrimmable` properties
- Switched to AOT-compatible JSON serialization
- Refactored `FilterParserAgent` to use `FilterYamlContext`

## New Features

### Nullable Input Support

All input widgets now support nullable values with a dedicated clear button. This is working automatically for any input of a nullable type.

```csharp
var optionalText = UseState((string?)null);
var textInput = optionalText.ToTextInput()
    .Placeholder("Optional field...");
```

<img width="2327" height="929" alt="image" src="https://github.com/user-attachments/assets/f281d78b-49e3-4764-b1c6-271912853923" />

### New Text Size

Added `Text.ExtraLarge()` for larger text displays, perfect for highlighting key metrics

<img width="614" height="328" alt="image" src="https://github.com/user-attachments/assets/10f36d59-9298-49b9-a3b4-a93006023508" />

### Metric View Redesign

The redesigned metric cards now provide clearer visual hierarchy, making it easier to scan dashboards and focus on key numbers at a glance.

<img width="2319" height="637" alt="image" src="https://github.com/user-attachments/assets/0a6548b0-d3e2-4e12-9d64-5109952e9bba" />

## What's Changed

- Grid View: now supports set explicit Height
- (chrome): fix padding rendering for none-chrome apps by @ArtemKhvorostianyi in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1873>
- Fixed bug where AOT compilation was not working for some file-based apps
