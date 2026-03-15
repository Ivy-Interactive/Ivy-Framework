# MultiLine Renamed to Multiline - v1.2.17

## Summary

The `MultiLine` property and method have been renamed to `Multiline` (lowercase 'l') for consistency with .NET naming conventions. This change affects `Detail`, `TableCell`, `DetailsBuilder`, `TableBuilder`, and adds a new `Multiline()` extension method for `TextInputBase`.

## What Changed

### Properties

| Component | Before (v1.2.16 and earlier) | After (v1.2.17+) |
|---|---|---|
| `Detail` | `MultiLine` | `Multiline` |
| `TableCell` | `MultiLine` | `Multiline` |

### Builder Methods

| Component | Before (v1.2.16 and earlier) | After (v1.2.17+) |
|---|---|---|
| `DetailsBuilder<T>` | `.MultiLine(...)` | `.Multiline(...)` |
| `TableBuilder<T>` | `.MultiLine(...)` | `.Multiline(...)` |
| `TableCellExtensions` | `.MultiLine()` | `.Multiline()` |

### New Extension Method

A new `Multiline()` extension method has been added for `TextInputBase`:

```csharp
// New in v1.2.17
myTextInput.Multiline();
myTextInput.Multiline(true);
myTextInput.Multiline(false);
```

### Before (v1.2.16 and earlier)

```csharp
// Detail widget
new Detail("Description", content, multiLine: true);

// DetailsBuilder
model.ToDetails()
    .MultiLine(e => e.Description, e => e.Notes);

// TableBuilder
records.ToTable()
    .MultiLine(e => e.Content);

// TableCell
new TableCell(content).MultiLine();
```

### After (v1.2.17+)

```csharp
// Detail widget
new Detail("Description", content, multiline: true);

// DetailsBuilder
model.ToDetails()
    .Multiline(e => e.Description, e => e.Notes);

// TableBuilder
records.ToTable()
    .Multiline(e => e.Content);

// TableCell
new TableCell(content).Multiline();

// TextInput (new extension method)
myState.ToTextInput().Multiline();
```

## How to Find Affected Code

Run `dotnet build`.

Or search for these patterns in the codebase using regex:

```regex
\.MultiLine\(
```

```regex
multiLine\s*[:=]
```

## How to Refactor

1. Replace all instances of `.MultiLine(` with `.Multiline(`
2. Replace constructor parameter `multiLine:` with `multiline:`
3. If using the property directly, replace `MultiLine` with `Multiline`

**Before:**

```csharp
public override object? Build()
{
    var detail = new Detail("Notes", notes, multiLine: true);

    var table = _records.ToTable()
        .MultiLine(e => e.Description);

    return new VStack(detail, table);
}
```

**After:**

```csharp
public override object? Build()
{
    var detail = new Detail("Notes", notes, multiline: true);

    var table = _records.ToTable()
        .Multiline(e => e.Description);

    return new VStack(detail, table);
}
```

## Verification

After refactoring, run:

```bash
dotnet build
```

All usages should compile without errors.
