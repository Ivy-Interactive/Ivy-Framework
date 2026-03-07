# EventHandler Wrapper and Handle* to On* Rename - v1.2.17

## Summary

This release introduces the `EventHandler<TEvent>` wrapper class and renames all `Handle*` extension methods to `On*` (e.g., `HandleClick` -> `OnClick`, `HandleSubmit` -> `OnSubmit`). The wrapper pattern enables extension methods to use the same name as event properties, providing a more intuitive API.

## What Changed

### Extension Method Renames

All widget event handler extension methods have been renamed:

| Before (v1.2.16 and earlier) | After (v1.2.17+) |
|---|---|
| `.HandleClick()` | `.OnClick()` |
| `.HandleSubmit()` | `.OnSubmit()` |
| `.HandleChange()` | `.OnChange()` |
| `.HandleSelect()` | `.OnSelect()` |
| `.HandleClose()` | `.OnClose()` |
| `.HandleRowAction()` | `.OnRowAction()` |
| `.HandleCardMove()` | `.OnCardMove()` |
| `.HandleExpand()` | `.OnExpand()` |
| `.HandleCollapse()` | `.OnCollapse()` |
| `.HandlePageChange()` | `.OnPageChange()` |
| `.HandleUpload()` | `.OnUpload()` |
| `.HandleDownload()` | `.OnDownload()` |

**Note:** `MenuItem.HandleSelect` is NOT renamed as it serves a different purpose (setting the OnSelect property on MenuItem records).

### Before (v1.2.16 and earlier)

```csharp
new Button("Click me")
    .HandleClick(async () => await DoSomething());

model.ToForm()
    .HandleSubmit(async (data) => await SaveAsync(data));

new Tree(items)
    .HandleSelect(e => selectedItem.Set(e.Value));
```

### After (v1.2.17+)

```csharp
new Button("Click me")
    .OnClick(async () => await DoSomething());

model.ToForm()
    .OnSubmit(async (data) => await SaveAsync(data));

new Tree(items)
    .OnSelect(e => selectedItem.Set(e.Value));
```

## How to Find Affected Code

Run `dotnet build`.

Or search for these patterns in the codebase:

```regex
\.Handle(Click|Submit|Change|Select|Close|RowAction|CardMove|Expand|Collapse|PageChange|Upload|Download)\(
```

## How to Refactor

Replace all instances of `.Handle*` extension methods with their `.On*` counterparts.

**Before:**

```csharp
public override object? Build()
{
    return new VStack
    {
        new Button("Save")
            .HandleClick(OnSaveClicked),
        data.ToDataTable()
            .HandleRowAction(OnRowAction),
        model.ToForm()
            .HandleSubmit(OnFormSubmit)
    };
}
```

**After:**

```csharp
public override object? Build()
{
    return new VStack
    {
        new Button("Save")
            .OnClick(OnSaveClicked),
        data.ToDataTable()
            .OnRowAction(OnRowAction),
        model.ToForm()
            .OnSubmit(OnFormSubmit)
    };
}
```

## Verification

After refactoring, run:

```bash
dotnet build
```

All usages should compile without errors.
