# NumberInput

<Ingress>
Capture numeric input with built-in validation, minimum/maximum constraints, step increments, and custom formatting options.
</Ingress>

The `NumberInput` widget provides an input field specifically for numeric values. It includes validation for numeric entries and options for
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
        return Layout.Horizontal() 
                | new NumberInput<double>(value)
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
This creates the `NumberInputs.Slider` variant.

The following demo shows how a slider can be used to give a visual clue.

```csharp demo-below 
public class NumberSliderInput : ViewBase 
{
    public override object? Build()
    {        
        var tapes = UseState(1.0);
        var cart = UseState("");
        return Layout.Vertical()
                | Text.Block("Tapes")
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
                     .Variant(NumberInputs.Slider)
                | Text.Block(cart);
    }
}
```

### Money

To enable users to enter money amounts, this variant should be used. The extension function `ToMoneyInput`
should be used to create this variant. This is the idiomatic way to use Ivy.

The following demo uses `NumberInputs.Number` with `NumberFormatStyle.Currency` to create
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
                | Text.Label("Enter EUR amount:")
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
                    
                | Text.Label("USD:")
                | moneyInUSD.ToMoneyInput()
                            .Currency("USD")
                            .Disabled()
                    
                | Text.Label("GBP:")
                | moneyInGBP.ToMoneyInput()
                            .Currency("GBP")
                            .Disabled();
    }
}
```

## Styling

`NumberInput`s can be customized with various styling options:

### Invalid

To mark a number input as invalid, this style should be used.
This can be used via the extension function `Invalid`.

```csharp demo-below 
public class InvalidDemo : ViewBase
{
    public override object? Build()
    {
        var num = UseState<double>(0);
        return Layout.Vertical()
                | Text.Small("The value should be less than 3.1")
                | num.ToNumberInput()
                     .Invalid(num.Value > 3.1 ? "Value should be less than 3.1": "");
    }
}
```

### Disabled

To disable a `NumberInput` this style should be used. This can be used via the extension function `Disabled`.

```csharp demo-below 
public class DisabledDemo : ViewBase
{
    public override object? Build()
    {
        var num = UseState(3.14);
        return Layout.Vertical()
                | num.ToNumberInput().Disabled();
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
                | Text.Label("Min 0, Max 100, Step 0.5, Precision 2")
                | new NumberInput<decimal>(precValue)
                     .Min(0.0)
                     .Max(100.0)
                     .Step(0.5)
                     .Precision(2)
                     .FormatStyle(NumberFormatStyle.Currency)
                     .Currency("USD");
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

<WidgetDocs Type="Ivy.NumberInput" ExtensionTypes="Ivy.NumberInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/NumberInput.cs"/>

## Examples

### Simple Grocery App

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
                   | Text.Label("Egg").Width(10)
                   | eggs.ToNumberInput()
                         .Min(0)
                         .Max(12)
                         .Width(10)
                   | Text.Html("<i>Maximum 12</i>"))  
        
                | (Layout.Horizontal()
                   | Text.Label("Bread").Width(10)
                   | breads.ToNumberInput()
                              .Min(0)
                              .Max(5)
                              .Width(10)
                   | Text.Html("<i>Maximum 5</i>"))
                | Text.Large($"{eggs} eggs and {breads} breads")
                | (Layout.Horizontal()
                   | Text.Large("Bill : ")
                   // Since it is disabled, no need to have an onChange event
                   | new NumberInput<decimal>(eggs.Value * eggCost + breadCost * breads.Value,_ => { })
                                     .Disabled()
                                     .Variant(NumberInputs.Number)
                                     .Precision(2)
                                     .FormatStyle(NumberFormatStyle.Currency)
                                     .Currency("EUR"));
                   
    }
}

```

For `NumberInput`s that use `NumberFormatStyle.Currency` the recommended type is `decimal`
like `new NumberInput<decimal>`
