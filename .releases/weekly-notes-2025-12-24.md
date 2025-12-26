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

This change gives you more control over how your pivot table data is presented.

### TextInput Prefix/Suffix Refactoring

The `TextInput` widget's prefix and suffix API has been refactored for better simplicity. The previous discriminated union type `PrefixSuffix` has been replaced with a simpler `Affix` record that supports both text and icons.

- `PrefixSuffix.Text` and `PrefixSuffix.Icon` - `Affix` with `Text` and `Icon` properties
- New extension methods: `ToAffix()` for both strings and Icons

```csharp
widget.Prefix("$")
widget.Suffix(Icons.Search)
```

<img width="2314" height="313" alt="image" src="https://github.com/user-attachments/assets/f55b9250-16b2-4dbe-a4b2-a6998f98c0c5" />

The extension methods remain unchanged, so your existing code using `Prefix(string)` and `Suffix(Icons)` continues to work without modification.

## Framework Improvements

### AOT (Ahead-Of-Time) Compilation Support

Ivy Framework now supports AOT compilation for better performance and faster startup times. This enables native compilation of your Ivy apps, resulting in smaller deployments and improved runtime performance.

**What Changed:**

- Added AOT compatibility to core framework and Ivy.Filters
- Implemented static YAML context for AOT-safe serialization
- Updated project files with `IsAotCompatible` and `IsTrimmable` properties
- Switched to AOT-compatible JSON serialization
- Refactored `FilterParserAgent` to use `FilterYamlContext`

## New Features

### Nullable Input Support

All input widgets now support nullable values with a dedicated clear button. When an input is marked as nullable and has a value, a clear (X) button appears, allowing users to reset the field to null.

```csharp
var optionalText = UseState((string?)null);
var textInput = optionalText.ToTextInput()
    .Placeholder("Optional field...")
    .Nullable();
```

<img width="2327" height="929" alt="image" src="https://github.com/user-attachments/assets/f281d78b-49e3-4764-b1c6-271912853923" />

### DataTable AI-Powered Filtering

DataTable now supports natural language filtering powered by Large Language Models (LLMs). Users can type conversational queries instead of writing formal filter expressions, and the AI will convert them to the appropriate filter syntax.

```csharp
public record Employee(int Id, string Name, decimal Salary, bool IsActive);

var employees = GetEmployees().AsQueryable();
return employees.ToDataTable(e => e.Id)
    .Header(e => e.Name, "Employee Name")
    .Header(e => e.Salary, "Salary")
    .Header(e => e.IsActive, "Active")
    .Config(config =>
    {
        config.AllowSorting = true;
        config.AllowFiltering = true;
        config.AllowLlmFiltering = true;  // Enable AI filtering
    });
```

Users can now filter using conversational phrases:

- "employees older than 30"
- "salary above 100000"
- "active managers"
- "hired in 2023"

The AI agent intelligently handles typos, maps concepts (like "retirement age" to `[Age] >= 65`), and resolves type mismatches by suggesting appropriate alternative fields. The AI converts queries to structured filter expressions using comparisons (`=`, `>`, `<`), text operations (`contains`, `starts with`), existence checks (`IS BLANK`), and logical operators (`AND`, `OR`, `NOT`).

**New Text Size:**

- Added `Text.ExtraLarge()` for larger text displays, perfect for highlighting key metrics

<img width="614" height="328" alt="image" src="https://github.com/user-attachments/assets/10f36d59-9298-49b9-a3b4-a93006023508" />

### GridView Height Control

The `GridView` widget now supports explicit height control through a new `Height()` method. Previously, grid views would automatically size based on their content, but you can now set a specific height when needed.

```csharp
var productGrid = new GridView()
    .Columns(3)
    .Height(Size.Pixels(400))  // Fixed height grid
    .Gap(Size.Pixels(16))
    | productCards;
```

**Metric View Redesign:**

The redesigned metric cards now provide clearer visual hierarchy, making it easier to scan dashboards and focus on key numbers at a glance.

<img width="2319" height="637" alt="image" src="https://github.com/user-attachments/assets/0a6548b0-d3e2-4e12-9d64-5109952e9bba" />

### .NET 10.0 Required

Ivy Framework now requires .NET 10.0 as the target framework. Previously, Ivy required .NET 9.0. All Ivy projects and packages are now built against .NET 10.0.

You can download .NET 10.0 from the [official .NET download page](https://dotnet.microsoft.com/download).

## What's Changed

- deps: Update React and ReactDOM to 19.2.3. by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1865>
- (Card): Update Design of Metric Views by @joelbystedt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1860>
- feat: remove redundant theming system by @ArtemKhvorostianyi in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1864>
- Refactored Affixes by @KaiserReich95 in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1802>
- [charts]: remove automatic alphabetical sorting in PivotTable by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1871>
- Feat/docs updates mikael by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1867>
- [Kanban]: fix examples in docs by @NikitaChuiko in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1877>
- [CodeInput]: fix example in docs by @NikitaChuiko in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1878>
- [GridView]: add Height by @NikitaChuiko in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1876>
- docs improvements batch 2 by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1882>
- (toc): generate compile time instead of frontend runtime by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1883>
- Added description for allowing LLMs in for querying by @KaiserReich95 in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1899>
- (IAnyInput): handle nullable by @ArtemKhvorostianyi in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1808>
- docs: improvements by Mikael vol3 by @rorychatt in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1896>
- (chrome): fix padding rendering for none-chrome apps by @ArtemKhvorostianyi in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1873>
