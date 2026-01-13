# BladeHeader Breaking Change - v1.2.11

## Summary

The `BladeHelper.WithHeader()` static method has been replaced with the `BladeHeader` component used within a `Fragment`.

## What Changed

### Before (v1.2.10 and earlier)
```csharp
return BladeHelper.WithHeader(header, content);
```

### After (v1.2.11+)
```csharp
return new Fragment()
       | new BladeHeader(header)
       | content;
```

## How to Find Affected Code

Run a `dotnet build`.

Or search for these patterns in the codebase:

### Pattern 1: BladeHelper.WithHeader usage
```regex
BladeHelper\.WithHeader\s*\(
```

### Pattern 2: Using statement for BladeHelper
Search for `using Ivy.Views.Blades;` in files that may use `BladeHelper`.

## How to Refactor

### Basic Pattern

**Before:**
```csharp
public override object? Build()
{
    var header = new Button("Add Item", onClick: _ => { });
    var content = new List(items);

    return BladeHelper.WithHeader(header, content);
}
```

**After:**
```csharp
public override object? Build()
{
    var header = new Button("Add Item", onClick: _ => { });
    var content = new List(items);

    return new Fragment()
           | new BladeHeader(header)
           | content;
}
```

### With Inline Header

**Before:**
```csharp
return BladeHelper.WithHeader(
    Layout.Horizontal().Gap(1)
        | searchInput
        | addButton,
    new List(items)
);
```

**After:**
```csharp
var header = Layout.Horizontal().Gap(1)
             | searchInput
             | addButton;

return new Fragment()
       | new BladeHeader(header)
       | new List(items);
```

### With Conditional Content

When using ternary operators that return different types, you must declare a typed variable:

**Before:**
```csharp
return BladeHelper.WithHeader(
    searchInput,
    hasItems ? new List(items) : Text.Muted("No items found")
);
```

**After:**
```csharp
object content = hasItems
    ? new List(items)
    : Text.Muted("No items found");

return new Fragment()
       | new BladeHeader(searchInput)
       | content;
```

> **Important:** When using a ternary with different return types (e.g., `List` vs `Text`), declare the variable as `object content = ...` to avoid CS0173 compiler errors.

Or use parentheses:

```csharp
return new Fragment()
       | new BladeHeader(searchInput)
       | (hasItems ? new List(items) : Text.Muted("No items found"));
```      

### With Text Header

**Before:**
```csharp
return BladeHelper.WithHeader(
    Text.Literal(product.Name),
    productCard
);
```

**After:**
```csharp
return new Fragment()
       | new BladeHeader(Text.Literal(product.Name))
       | productCard;
```

### Complete Example: Search List Blade

**Before:**
```csharp
public class ProductsListBlade : ViewBase
{
    public override object? Build()
    {
        var blades = UseContext<IBladeService>();
        var filter = UseState("");
        var products = GetProducts(filter.Value);

        var items = products.Select(p => new ListItem(p.Name, onClick: _ =>
            blades.Push(this, new ProductDetailsBlade(p.Id), p.Name)));

        var createBtn = Icons.Plus.ToButton(_ => { }).Ghost();

        return BladeHelper.WithHeader(
            Layout.Horizontal().Gap(1)
                | filter.ToSearchInput().Placeholder("Search").Width(Size.Grow())
                | createBtn,
            new List(items)
        );
    }
}
```

**After:**
```csharp
public class ProductsListBlade : ViewBase
{
    public override object? Build()
    {
        var blades = UseContext<IBladeService>();
        var filter = UseState("");
        var products = GetProducts(filter.Value);

        var items = products.Select(p => new ListItem(p.Name, onClick: _ =>
            blades.Push(this, new ProductDetailsBlade(p.Id), p.Name)));

        var createBtn = Icons.Plus.ToButton(_ => { }).Ghost();

        var header = Layout.Horizontal().Gap(1)
                     | filter.ToSearchInput().Placeholder("Search").Width(Size.Grow())
                     | createBtn;

        return new Fragment()
               | new BladeHeader(header)
               | new List(items);
    }
}
```

## Key Refactoring Rules

1. **Replace the method call**: Change `BladeHelper.WithHeader(header, content)` to `new Fragment() | new BladeHeader(header) | content`

2. **Extract complex headers**: If the header is built inline with multiple operators, extract it to a variable for readability

3. **Preserve the order**: Header comes first, then content - this matches the visual layout

4. **Fragment is required**: The `Fragment` component groups the `BladeHeader` and content together

## Common Mistakes to Avoid

1. **Forgetting Fragment**: Don't just use `new BladeHeader(header) | content` - you need the `Fragment` wrapper

2. **Wrong operator order**: The order matters - `Fragment | BladeHeader | content`, not `Fragment | content | BladeHeader`

3. **Missing using**: Ensure the file has access to `Fragment` and `BladeHeader` types

## Verification

After refactoring, run:
```bash
dotnet build
```

All usages of `BladeHelper.WithHeader` should be replaced and compile without errors.
