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
        return value.ToNumberInput()
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

        UseEffect(() => {
            cart.Set($"Added {tapes.Value} cm tape to your cart");
        }, tapes);

        return Layout.Vertical()
                | tapes.ToNumberInput()
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

        UseEffect(() => {
            moneyInUSD.Set(moneyInEUR.Value * euroToUSD);
            moneyInGBP.Set(moneyInEUR.Value * euroToGBP);
        }, moneyInEUR);

        return Layout.Vertical()
                | Text.H3("Simple Currency Converter")
                | moneyInEUR.ToNumberInput()
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
                | precValue.ToNumberInput()
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

`NumberInput` supports eight format styles for different use cases. The following shows the basic styles:

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

### Advanced Format Styles

Additional format styles are available for domain-specific use cases:

- **Compact** — abbreviated large numbers (`1.2K`, `3.5M`)
- **Scientific** — scientific notation (`1.23E6`)
- **Engineering** — engineering notation with exponents as multiples of 3
- **Accounting** — negative values in parentheses (`($1,234.56)`)
- **Bytes** — file sizes with binary units (`1.5 GB`, `256 KB`)

```csharp demo-below
public class AdvancedFormatStylesDemo : ViewBase
{
    public override object? Build()
    {
        var followers = UseState(1_250_000.0);
        var wavelength = UseState(0.000000532);
        var fileSize = UseState(5_242_880.0);
        var negativeBalance = UseState(-1234.56m);

        return Layout.Vertical()
                | followers.ToNumberInput()
                    .FormatStyle(NumberFormatStyle.Compact)
                    .Precision(1)
                    .WithField()
                    .Label("Social Media Followers")
                | wavelength.ToNumberInput()
                    .FormatStyle(NumberFormatStyle.Scientific)
                    .Precision(2)
                    .WithField()
                    .Label("Wavelength (meters)")
                | fileSize.ToNumberInput()
                    .FormatStyle(NumberFormatStyle.Bytes)
                    .WithField()
                    .Label("File Size")
                | negativeBalance.ToNumberInput()
                    .FormatStyle(NumberFormatStyle.Accounting)
                    .Currency("USD")
                    .WithField()
                    .Label("Account Balance");
    }
}
```

Available `NumberFormatStyle` values:

- `Decimal` (default) — "1,234.56"
- `Currency` — "$1,234.56"
- `Percent` — "56%"
- `Compact` — "1.2K", "3.5M", "1.1B"
- `Scientific` — "1.23E6"
- `Engineering` — "1.23E6" (exponents as multiples of 3)
- `Accounting` — "($1,234.56)" for negatives
- `Bytes` — "1.5 GB", "256 KB"

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

`NumberInput` widgets support focus, blur, and manual `AutoFocus` behavior.

```csharp demo-tabs
public class NumberInputEventsDemo : ViewBase
{
    public override object? Build()
    {
        var blurCount = UseState(0);
        var focusCount = UseState(0);
        var state = UseState(0.0);

        return Layout.Tabs(
            new Tab("OnFocus", Layout.Vertical()
                | Text.P("The OnFocus event fires when the number input gains focus.")
                | state.ToNumberInput().Placeholder("Focus me...")
                    .OnFocus(() => focusCount.Set(focusCount.Value + 1))
                | Text.Literal($"Focus Count {focusCount.Value}")
            ),
            new Tab("OnBlur", Layout.Vertical()
                | Text.P("The OnBlur event fires when the number input loses focus.")
                | state.ToNumberInput().Placeholder("Blur me...")
                    .OnBlur(() => blurCount.Set(blurCount.Value + 1))
                | Text.Literal($"Blur Count {blurCount.Value}")
            ),
            new Tab("AutoFocus", Layout.Vertical()
                | Text.P("The AutoFocus property automatically focuses the widget upon mounting.")
                | state.ToNumberInput().Placeholder("AutoFocused NumberInput")
                    .AutoFocus()
                | Text.Lead("Focused!")
            )
        ).Variant(TabsVariant.Tabs);
    }
}
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
        var total = UseState(eggs.Value * eggCost + breadCost * breads.Value);

        UseEffect(() => {
            total.Set(eggs.Value * eggCost + breadCost * breads.Value);
        }, eggs, breads);

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
                | Text.P($"{eggs.Value} eggs and {breads.Value} breads").Large()
                | (Layout.Horizontal()
                   | Text.P("Bill : ").Large()
                   // Since it is disabled, no need to have an onChange event
                   | total.ToNumberInput()
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
For `NumberInput`s that use `NumberFormatStyle.Currency` the recommended type is `decimal`.
