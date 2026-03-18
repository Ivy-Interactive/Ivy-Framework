---
searchHints:
  - numeric
  - range
  - slider
  - min
  - max
  - interval
---

# NumberRangeInput

<Ingress>
Select numeric ranges with dual draggable handles on a single slider, perfect for filtering by price, thresholds, or any min/max numeric bounds.
</Ingress>

The `NumberRangeInput` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) provides an intuitive range slider for selecting minimum and maximum numeric values. It uses two draggable handles on a single track, with the filled region between handles representing the selected range.

<Callout Type="tip">
Like `NumberInput`, if you don't explicitly specify `Min` and `Max` for a `NumberRangeInput`, common default values will be applied based on the numeric type. Always set `Min` and `Max` to define your desired range bounds.
</Callout>

## Basic Usage

Here's a simple example of a `NumberRangeInput` that allows users to select a numeric range:

```csharp demo-below
public class BasicNumberRangeDemo : ViewBase
{
    public override object? Build()
    {
        var range = UseState<(int, int)>(() => (25, 75));
        var lower = range.Value.Item1;
        var upper = range.Value.Item2;

        return Layout.Vertical()
                | range.ToNumberRangeInput()
                       .Min(0)
                       .Max(100)
                | Text.P($"Selected range: {lower} to {upper}").Large();
    }
}
```

The lower and upper bounds can be accessed using `range.Value.Item1` and `range.Value.Item2`.

## Supported Types

NumberRangeInput supports all numeric tuple types:

- `(short, short)` and `(short?, short?)`
- `(int, int)` and `(int?, int?)`
- `(long, long)` and `(long?, long?)`
- `(byte, byte)` and `(byte?, byte?)`
- `(float, float)` and `(float?, float?)`
- `(double, double)` and `(double?, double?)`
- `(decimal, decimal)` and `(decimal?, decimal?)`

## Variants

The `NumberRangeInput` can be customized with various states including **Disabled**, **Invalid**, and **Nullable**:

```csharp demo-below
public class NumberRangeVariantsDemo : ViewBase
{
    public override object? Build()
    {
        var range = UseState<(int, int)>(() => (30, 70));
        var nullableRange = UseState<(int?, int?)>(() => (30, 70));

        return Layout.Vertical().Gap(4)
            | Text.P("Normal State").Small()
            | range.ToNumberRangeInput().Min(0).Max(100)
            | Text.P("Disabled State").Small()
            | range.ToNumberRangeInput().Min(0).Max(100).Disabled()
            | Text.P("Invalid State").Small()
            | range.ToNumberRangeInput().Min(0).Max(100).Invalid("Invalid range")
            | Text.P("Nullable State").Small()
            | nullableRange.ToNumberRangeInput().Min(0).Max(100);
    }
}
```

## Format Styles

NumberRangeInput supports three format styles: **Decimal** (default), **Currency**, and **Percent**:

```csharp demo-below
public class NumberRangeFormatDemo : ViewBase
{
    public override object? Build()
    {
        var decimalRange = UseState<(int, int)>(() => (25, 75));
        var priceRange = UseState<(decimal, decimal)>(() => (50.0m, 200.0m));
        var percentRange = UseState<(double, double)>(() => (0.25, 0.75));

        return Layout.Vertical().Gap(4)
            | Text.P("Decimal Format").Small()
            | decimalRange.ToNumberRangeInput()
                          .Min(0)
                          .Max(100)
                          .FormatStyle(NumberFormatStyle.Decimal)
            | Text.P("Currency Format (USD)").Small()
            | priceRange.ToNumberRangeInput()
                        .Min(0)
                        .Max(500)
                        .FormatStyle(NumberFormatStyle.Currency)
                        .Currency("USD")
            | Text.P("Percent Format").Small()
            | percentRange.ToNumberRangeInput()
                          .Min(0)
                          .Max(1)
                          .Step(0.01)
                          .FormatStyle(NumberFormatStyle.Percent)
                          .Precision(2);
    }
}
```

For currency ranges, use `ToMoneyRangeInput()` as a convenient shorthand:

```csharp demo-below
public class MoneyRangeDemo : ViewBase
{
    public override object? Build()
    {
        var budgetRange = UseState<(decimal, decimal)>(() => (50.0m, 200.0m));

        return Layout.Vertical()
            | Text.P("Budget Filter").Small()
            | budgetRange.ToMoneyRangeInput()
                         .Currency("USD")
                         .Min(0)
                         .Max(500)
                         .Step(10);
    }
}
```

## Prefix and Suffix

Add contextual information with text or icon prefixes and suffixes:

```csharp demo-below
public class NumberRangePrefixSuffixDemo : ViewBase
{
    public override object? Build()
    {
        var priceRange = UseState<(decimal, decimal)>(() => (50.0m, 200.0m));
        var percentRange = UseState<(double, double)>(() => (0.25, 0.75));
        var tempRange = UseState<(int, int)>(() => (18, 24));

        return Layout.Vertical().Gap(4)
            | Text.P("Text Prefix ($)").Small()
            | priceRange.ToNumberRangeInput()
                        .Min(0)
                        .Max(500)
                        .Prefix("$")
                        .Precision(2)
            | Text.P("Text Suffix (%)").Small()
            | percentRange.ToNumberRangeInput()
                          .Min(0)
                          .Max(1)
                          .Step(0.01)
                          .Suffix("%")
                          .Precision(2)
            | Text.P("Icon Prefix + Text Suffix").Small()
            | tempRange.ToNumberRangeInput()
                       .Min(-10)
                       .Max(40)
                       .Prefix(Icons.Thermometer)
                       .Suffix("°C");
    }
}
```

The `Prefix` and `Suffix` methods accept either a `string` or an `Icons` value.

## Step Increments

Control how the slider handles move using the `Step` property:

```csharp demo-below
public class NumberRangeStepDemo : ViewBase
{
    public override object? Build()
    {
        var range = UseState<(int, int)>(() => (20, 80));

        return Layout.Vertical().Gap(4)
            | Text.P("Step: 10 (snaps to multiples of 10)").Small()
            | range.ToNumberRangeInput()
                   .Min(0)
                   .Max(100)
                   .Step(10);
    }
}
```

## Event Handling

NumberRangeInput supports change events that receive the full tuple value:

```csharp
var rangeState = UseState<(int, int)>(() => (0, 100));
var changeLabel = UseState("");

new NumberRangeInput<int>(rangeState.Value, e =>
{
    rangeState.Set(e);
    changeLabel.Set($"Range changed to: {e.Item1} - {e.Item2}");
})
{
    Min = 0,
    Max = 100
}
```

<WidgetDocs Type="Ivy.NumberRangeInput" ExtensionTypes="Ivy.NumberRangeInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Inputs/NumberRangeInput.cs"/>

## FAQ

<Details>
<Summary>
Price Filter Example
</Summary>
<Body>

A realistic example demonstrating a price range filter with currency formatting:

```csharp demo-tabs
public class PriceFilterDemo : ViewBase
{
    public override object? Build()
    {
        var priceRange = UseState<(decimal, decimal)>(() => (50.0m, 200.0m));
        var products = new[]
        {
            ("Laptop", 899.99m),
            ("Mouse", 29.99m),
            ("Keyboard", 79.99m),
            ("Monitor", 299.99m),
            ("Headphones", 149.99m),
            ("Webcam", 89.99m)
        };

        var lower = priceRange.Value.Item1;
        var upper = priceRange.Value.Item2;

        var filteredProducts = products
            .Where(p => p.Item2 >= lower && p.Item2 <= upper)
            .ToArray();

        return Layout.Vertical().Gap(4)
            | Text.P("Filter by Price").Large().Bold()
            | priceRange.ToMoneyRangeInput()
                        .Currency("USD")
                        .Min(0)
                        .Max(1000)
                        .Step(10)
                        .WithField()
                        .Label("Price Range")
            | Text.P($"Showing {filteredProducts.Length} of {products.Length} products").Small().Muted()
            | (filteredProducts.Length > 0
                ? Layout.Vertical().Gap(2)
                    | filteredProducts.Select(p =>
                        Layout.Horizontal().Gap(2)
                            | Text.P(p.Item1)
                            | Text.P($"${p.Item2:F2}").Bold()
                    ).ToArray()
                : Text.P("No products in this price range").Muted());
    }
}
```

</Body>
</Details>

<Details>
<Summary>
How do I access the lower and upper values?
</Summary>
<Body>

The `NumberRangeInput` stores its value as a tuple. Access the bounds using `Item1` (lower) and `Item2` (upper):

```csharp
var range = UseState<(int, int)>(() => (25, 75));

var lowerBound = range.Value.Item1;  // 25
var upperBound = range.Value.Item2;  // 75
```

You can also use tuple deconstruction:

```csharp
var (lower, upper) = range.Value;
```

</Body>
</Details>

<Details>
<Summary>
How do the handles prevent crossing?
</Summary>
<Body>

The `NumberRangeInput` automatically enforces that the lower handle cannot exceed the upper handle and vice versa. The slider component handles this constraint internally—you don't need to add any validation logic. When a user drags a handle, it will stop at the position of the other handle.

</Body>
</Details>

<Details>
<Summary>
How do I format a NumberRangeInput as currency or percent?
</Summary>
<Body>

Use the `.FormatStyle()` fluent method with the `NumberFormatStyle` enum:

```csharp
var priceRange = UseState<(decimal, decimal)>(() => (50.0m, 200.0m));
var percentRange = UseState<(double, double)>(() => (0.25, 0.75));

// Currency formatting
priceRange.ToNumberRangeInput()
          .FormatStyle(NumberFormatStyle.Currency)
          .Currency("USD")

// Shorthand for currency
priceRange.ToMoneyRangeInput().Currency("USD")

// Percent formatting
percentRange.ToNumberRangeInput()
            .FormatStyle(NumberFormatStyle.Percent)
```

Available `NumberFormatStyle` values: `Decimal` (default), `Currency`, `Percent`. For currency ranges, the recommended state type is `(decimal, decimal)`.

</Body>
</Details>

<Details>
<Summary>
Can I use nullable numeric ranges?
</Summary>
<Body>

Yes! NumberRangeInput supports nullable tuple types like `(int?, int?)`, `(decimal?, decimal?)`, etc. When nullable, a clear button (X) appears to reset both values to `null`:

```csharp
var nullableRange = UseState<(decimal?, decimal?)>(() => (50.0m, 200.0m));

nullableRange.ToMoneyRangeInput()
             .Currency("USD")
             .Min(0)
             .Max(1000);
```

Users can click the X button to clear the selection.

</Body>
</Details>
