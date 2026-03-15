---
searchHints:
  - numeric
  - integer
  - decimal
  - number
  - money
  - currency
---

# NumberInput

<Ingress>
Capture [numeric input](../../01_Onboarding/02_Concepts/03_Widgets.md) with built-in validation, minimum/maximum constraints, step increments, and custom formatting options.
</Ingress>

The `NumberInput` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) provides an input field specifically for numeric values. It includes validation for numeric entries and options for
setting minimum/maximum values, step increments, and formatting.

<Callout Type="tip">
Unless you explicitly specify `Min` and `Max` for a `NumberInput`, common default values will be applied based on the numeric type. For example, integer types use their natural limits, while decimal, double, and float types use practical defaults (e.g., ±999,999.99 for sliders). If you need a specific range, always set `Min` and `Max` yourself.
</Callout>

## Basic Usage

Here's a simple example of a `NumberInput` that allows users to input a number. It also allows to set a minimum
and a maximum limit.

```csharp demo-below
public class SimpleNumericValueDemo : ViewBase
{
    public override object? Build()
    {
        var value = UseState(0);
        return new NumberInput<double>(value)
                     .Min(-10)
                     .Max(10);
    }
}
```

The `NumberInput` allows users to enter numeric values directly.

## Variants

`NumberInput`s come in several variants to suit different use cases.

### Slider

This variant helps create a slider that changes the value as the slider is pulled to the right.
This creates the `NumberInputVariant.Slider` variant.

The following demo shows how a slider can be used to give a visual clue.

```csharp demo-below
public class NumberSliderInput : ViewBase 
{
    public override object? Build()
    {        
        var tapes = UseState(1.0);
        var cart = UseState("");
        return Layout.Vertical()
                | new NumberInput<double>(
                      tapes.Value,
                      e => {
                             tapes.Set(e);
                             cart.Set($"Added {tapes} cm tape to your cart"); 
                     })
                     .Min(30.0)
                     .Max(500.0)
                     .Precision(2)
                     .Step(0.5)
                     .Variant(NumberInputVariant.Slider)
                     .WithField()
                     .Label("Tapes")
                | Text.Block(cart);
    }
}
```

### Money

To enable users to enter money amounts, this variant should be used. The extension function `ToMoneyInput`
should be used to create this variant. This is the idiomatic way to use Ivy.

The following demo uses `NumberInputVariant.Number` with `NumberFormatStyle.Currency` to create
`NumberInput`s that can take money inputs. `ToMoneyInput` hides all these complexities.

```csharp demo-below
public class MoneyInputDemo : ViewBase 
{
    public override object? Build()
    {
        var moneyInUSD = UseState<decimal>(0.00M);
        var moneyInGBP = UseState<decimal>(0.00M);
        var moneyInEUR = UseState<decimal>(0.00M);

        // Currency Conversion Rates
        var euroToUSD = 1.80M;
        var euroToGBP = 0.86M;
        
        return Layout.Vertical()
                | Text.H3("Simple Currency Converter")
                | new NumberInput<decimal>(
                    moneyInEUR.Value,
                    e => {
                        moneyInEUR.Set(e);
                        moneyInUSD.Set(e * euroToUSD);
                        moneyInGBP.Set(e * euroToGBP);
                    }
                )
                .FormatStyle(NumberFormatStyle.Currency)
                .Currency("EUR")
                .Placeholder("€0.00")
                .WithField()
                .Label("Enter EUR amount:")
                    
                | moneyInUSD.ToMoneyInput()
                            .Currency("USD")
                            .Disabled()
                            .WithField()
                            .Label("USD:")
                    
                | moneyInGBP.ToMoneyInput()
                            .Currency("GBP")
                            .Disabled()
                            .WithField()
                            .Label("GBP:");
    }
}
```

## Styling

`NumberInput`s can be customized with various styling options, including `Disabled` and `Invalid` states:

```csharp demo-below
public class NumberStylingDemo : ViewBase
{
    public override object? Build()
    {
        var num = UseState(3.14);
        return Layout.Vertical()
                | num.ToNumberInput()
                     .Disabled()
                     .WithField().Label("Disabled")
                | num.ToNumberInput()
                     .Invalid(num.Value > 3.1 ? "Value should be less than 3.1" : "")
                     .WithField().Label("Invalid");
    }
}
```

### Precision and Step

To set the precision of a `NumberInput` this style should be used. This can be used via the extension function
`Precision`. To customize the amount by which the value of a `NumberInput` is changed can be set by `Step`.

The following demo shows these two in action.

```csharp demo-below
public class MoneyPrecisionDemo : ViewBase
{
    public override object? Build()
    {
        var precValue = UseState(0.50M);
        return Layout.Horizontal() 
                | new NumberInput<decimal>(precValue)
                     .Min(0.0)
                     .Max(100.0)
                     .Step(0.5)
                     .Precision(2)
                     .FormatStyle(NumberFormatStyle.Currency)
                     .Currency("USD")
                     .WithField()
                     .Description("Min 0, Max 100, Step 0.5, Precision 2");
    }
}
```

### FormatStyle

There are three different kinds of formats that a `NumberInput` can have. The following shows these in action.

```csharp demo-below
public class FormatStyleDemos : ViewBase
{
    public override object? Build()
    {
        var num = UseState(3.14);
        var amount = UseState(30.14);
        var passingPercentage = UseState(0.35);
        
        return Layout.Vertical()
                | num.ToNumberInput().FormatStyle(NumberFormatStyle.Decimal)
                | amount.ToNumberInput().FormatStyle(NumberFormatStyle.Currency).Currency("GBP")
                | passingPercentage.ToNumberInput().FormatStyle(NumberFormatStyle.Percent);
    }
}

```

## Prefix and Suffix

In certain scenarios, it is beneficial to prepend or append static content—such as text fragments or icons—to an input field. This practice is particularly useful for displaying a currency symbol, a unit label, or an icon that denotes the expected input.

```csharp demo-below
public class NumberPrefixSuffixDemo : ViewBase
{
    public override object? Build()
    {
        var price = UseState(99.99m);
        var weight = UseState(5.5);
        var temperature = UseState(22);

        return Layout.Vertical()
                | price.ToNumberInput()
                       .Prefix("$")
                       .Precision(2)
                       .WithField()
                       .Label("Price")
                | weight.ToNumberInput()
                        .Suffix("kg")
                        .Precision(1)
                        .WithField()
                        .Label("Weight")
                | temperature.ToNumberInput()
                             .Prefix(Icons.Thermometer)
                             .Suffix("°C")
                             .WithField()
                             .Label("Temperature");
    }
}
```

The `Prefix` and `Suffix` methods accept either a `string` or an `Icons` value, thereby providing flexibility for augmenting the contextual information of the input.

## Event Handling

`NumberInput`s can handle change and blur events:

```csharp
var onChangedState = UseState(0);
var onChangeLabel = UseState("");

new NumberInput<int>(onChangedState.Value, e =>
{
    onChangedState.Set(e);
    onChangeLabel.Set("Changed");
});
```

<WidgetDocs Type="Ivy.NumberInput" ExtensionTypes="Ivy.NumberInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Inputs/NumberInput.cs"/>

## Faq

<Details>
<Summary>
Simple Grocery App
</Summary>
<Body>
The following shows a realistic example of how several `NumberInput`s can be used.

```csharp demo-tabs
public class GroceryAppDemo : ViewBase
{
    public override object? Build()
    {
        var eggs = UseState(0);
        var breads = UseState(0);
        var eggCost = 3.45M;
        var breadCost = 6.13M;
        return Layout.Vertical()
                | (Layout.Horizontal() 
                   | eggs.ToNumberInput()
                         .Min(0)
                         .Max(12)
                         .Width(Size.Units(10))
                         .WithField()
                         .Label("Egg")
                         .Description("Maximum 12"))
        
                | (Layout.Horizontal()
                   | breads.ToNumberInput()
                              .Min(0)
                              .Max(5)
                              .Width(Size.Units(10))
                              .WithField()
                              .Label("Bread")
                              .Description("Maximum 5"))
                | Text.P($"{eggs} eggs and {breads} breads").Large()
                | (Layout.Horizontal()
                   | Text.P("Bill : ").Large()
                   // Since it is disabled, no need to have an onChange event
                   | new NumberInput<decimal>(eggs.Value * eggCost + breadCost * breads.Value,_ => { })
                                     .Disabled()
                                     .Variant(NumberInputVariant.Number)
                                     .Precision(2)
                                     .FormatStyle(NumberFormatStyle.Currency)
                                     .Currency("EUR"));
                   
    }
}

```

</Body>
</Details>

<Details>
<Summary>
How do I set min/max values on a NumberInput?
</Summary>
<Body>

You can pass `min` and `max` directly as optional parameters to `ToNumberInput()`:

```csharp
var count = UseState(1);
count.ToNumberInput(min: 1, max: 100).Placeholder("Enter count")
```

Alternatively, use the `.Min()` and `.Max()` fluent extension methods:

```csharp
count.ToNumberInput().Min(1).Max(100)
```

</Body>
</Details>

<Details>
<Summary>
How do I format a NumberInput as currency, percent, or decimal?
</Summary>
<Body>

Use the `.FormatStyle()` fluent method with the `NumberFormatStyle` enum:

```csharp
var price = UseState(99.99m);
var taxRate = UseState(0.08);

// Currency formatting
price.ToNumberInput().FormatStyle(NumberFormatStyle.Currency).Currency("USD")

// Percent formatting
taxRate.ToNumberInput().FormatStyle(NumberFormatStyle.Percent)

// Decimal formatting (default)
price.ToNumberInput().FormatStyle(NumberFormatStyle.Decimal)
```

Available `NumberFormatStyle` values: `Decimal` (default), `Currency`, `Percent`. For currency inputs, the recommended state type is `decimal`. Use `.Currency("USD")` to specify the currency code.

</Body>
</Details>
For `NumberInput`s that use `NumberFormatStyle.Currency` the recommended type is `decimal`
like `new NumberInput<decimal>`
