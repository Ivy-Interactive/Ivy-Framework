
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.DollarSign, group: ["Widgets", "Inputs"], searchHints: ["numeric", "integer", "decimal", "number", "money", "currency"])]
public class NumberInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("Number Input")
               | Layout.Tabs(
                   new Tab("Variants", new NumberInputVariantsTab()),
                   new Tab("Data Binding", new NumberInputDataBindingTab()),
                   new Tab("Prefix & Suffix", new NumberInputPrefixSuffixTab()),
                   new Tab("Currency", new NumberInputCurrencyTab()),
                   new Tab("Sizes", new NumberInputSizesTab()),
                   new Tab("Events", new NumberInputEventsTab())
               ).Variant(TabsVariant.Content);
    }
}

public class NumberInputVariantsTab : ViewBase
{
    public override object Build()
    {
        var nullIntValue = UseState<int?>();
        var intValue = UseState(12345);
        var nullIntInvalid = UseState<int?>();

        const string loremIpsumString = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros";

        return Layout.Grid().Columns(6)

               | null!
               | Text.Monospaced("Null")
               | Text.Monospaced("With Value")
               | Text.Monospaced("Disabled")
               | Text.Monospaced("Invalid")
               | Text.Monospaced("Invalid Nullable")

               | Text.Monospaced("ToNumberInput()")
               | nullIntValue
                 .ToNumberInput()
                 .Placeholder("Placeholder")
                 .TestId("number-input-nullable-main")
               | intValue
                 .ToNumberInput()
                 .TestId("number-input-int-main")
               | intValue
                 .ToNumberInput()
                 .Disabled()
                 .TestId("number-input-int-disabled-main")
               | intValue
                 .ToNumberInput()
                 .Invalid(loremIpsumString)
                 .TestId("number-input-int-invalid-main")
               | nullIntInvalid
                 .ToNumberInput()
                 .Invalid(loremIpsumString)
                 .TestId("number-input-nullable-invalid-main")

               | Text.Monospaced("ToSliderInput()")
               | nullIntValue
                 .ToSliderInput()
                 .Placeholder("Placeholder")
                 .TestId("number-input-nullable-slider-main")
               | intValue
                 .ToSliderInput()
                 .TestId("number-input-int-slider-main")
               | intValue
                 .ToSliderInput()
                 .Disabled()
                 .TestId("number-input-int-disabled-slider-main")
               | intValue
                 .ToSliderInput()
                 .Invalid(loremIpsumString)
                 .TestId("number-input-int-invalid-slider-main")
               | nullIntInvalid
                 .ToSliderInput()
                 .Invalid(loremIpsumString)
                 .TestId("number-input-nullable-invalid-slider-main");
    }
}

public class NumberInputDataBindingTab : ViewBase
{
    public override object Build()
    {
        var shortState = UseState((short)0);
        var shortNullableState = UseState((short?)null);
        var intState = UseState(0);
        var intNullableState = UseState((int?)null);
        var longState = UseState((long)0);
        var longNullableState = UseState((long?)null);
        var byteState = UseState((byte)0);
        var byteNullableState = UseState((byte?)null);
        var floatState = UseState(0.0f);
        var floatNullableState = UseState((float?)null);
        var doubleState = UseState(0.0);
        var doubleNullableState = UseState((double?)null);
        var decimalState = UseState((decimal)0);
        var decimalNullableState = UseState((decimal?)null);

        var numericTypes = new (string TypeName, object NonNullableState, object NullableState)[]
        {
            ("short", shortState, shortNullableState),
            ("int", intState, intNullableState),
            ("long", longState, longNullableState),
            ("byte", byteState, byteNullableState),
            ("float", floatState, floatNullableState),
            ("double", doubleState, doubleNullableState),
            ("decimal", decimalState, decimalNullableState)
        };

        var gridItems = new List<object>
        {
            Text.Monospaced("Type"),
            Text.Monospaced("Non-Nullable"),
            Text.Monospaced("State"),
            Text.Monospaced("Type"),
            Text.Monospaced("Nullable"),
            Text.Monospaced("State")
        };

        var numericTypeNames = new[] { "double", "decimal", "float", "short", "int", "long", "byte" };

        foreach (var (typeName, nonNullableState, nullableState) in numericTypes)
        {
            gridItems.Add(Text.Monospaced(typeName));
            gridItems.Add(CreateNumberInputVariants(nonNullableState));

            var nonNullableAnyState = nonNullableState as IAnyState;
            object? nonNullableValue = null;
            if (nonNullableAnyState != null)
            {
                var prop = nonNullableAnyState.GetType().GetProperty("Value");
                nonNullableValue = prop?.GetValue(nonNullableAnyState);
            }

            gridItems.Add(FormatStateValue(typeName, nonNullableValue, false));

            gridItems.Add(Text.Monospaced($"{typeName}?"));
            gridItems.Add(CreateNumberInputVariants(nullableState));

            var anyState = nullableState as IAnyState;
            object? value = null;
            if (anyState != null)
            {
                var prop = anyState.GetType().GetProperty("Value");
                value = prop?.GetValue(anyState);
            }

            gridItems.Add(FormatStateValue(typeName, value, true));
        }

        return Layout.Grid().Columns(6) | gridItems.ToArray();

        object FormatStateValue(string typeName, object? value, bool isNullable)
        {
            return value switch
            {
                null => isNullable ? Text.Monospaced("Null") : Text.Monospaced("0"),
                _ when numericTypeNames.Contains(typeName) => Text.Monospaced(value.ToString()!),
                _ => Text.Monospaced(value?.ToString() ?? "null")
            };
        }
    }

    private static object CreateNumberInputVariants(object state) =>
        InputDataBindingHelper.CreateInputVariants(state,
            anyState => Layout.Vertical()
                | anyState.ToNumberInput()
                | anyState.ToSliderInput(),
            anyState => Layout.Vertical()
                | anyState.ToNumberInput()
                | anyState.ToSliderInput());
}

public class NumberInputPrefixSuffixTab : ViewBase
{
    public override object Build()
    {
        var priceValue = UseState(99.99m);
        var weightValue = UseState(5.5);
        var temperatureValue = UseState(22);
        var percentValue = UseState(0.75);

        return Layout.Grid().Columns(3)
               | Text.Monospaced("Description")
               | Text.Monospaced("Number Input")
               | Text.Monospaced("State")

               | Text.Block("Text Prefix ($)")
               | priceValue
                 .ToNumberInput()
                 .Prefix("$")
                 .Precision(2)
                 .TestId("number-input-prefix-text")
               | Text.Monospaced(priceValue.Value.ToString("F2"))

               | Text.Block("Text Suffix (kg)")
               | weightValue
                 .ToNumberInput()
                 .Suffix("kg")
                 .Precision(1)
                 .TestId("number-input-suffix-text")
               | Text.Monospaced(weightValue.Value.ToString("F1"))

               | Text.Block("Icon Prefix + Text Suffix")
               | temperatureValue
                 .ToNumberInput()
                 .Prefix(Icons.Thermometer)
                 .Suffix("°C")
                 .TestId("number-input-prefix-suffix-mixed")
               | Text.Monospaced(temperatureValue.Value.ToString())

               | Text.Block("Text Suffix (%)")
               | percentValue
                 .ToNumberInput()
                 .Suffix("%")
                 .Precision(2)
                 .TestId("number-input-suffix-percent")
               | Text.Monospaced(percentValue.Value.ToString("F2"));
    }
}

public class NumberInputCurrencyTab : ViewBase
{
    public override object Build()
    {
        var usdValue = UseState(1234.56m);
        var eurValue = UseState(987.65m);
        var gbpValue = UseState(567.89m);
        var jpyValue = UseState(12345m);
        var nullCurrencyValue = UseState<decimal?>(() => null);
        var compactValue = UseState(1_234_567.0);
        var scientificValue = UseState(1_234_567.0);
        var engineeringValue = UseState(1_234_567.0);
        var accountingValue = UseState(-1234.56m);
        var bytesValue = UseState(5_242_880.0);

        return Layout.Vertical()
               | Text.H3("Different Currencies")
               | (Layout.Grid().Columns(3)
                  | Text.Monospaced("Currency")
                  | Text.Monospaced("Number Input")
                  | Text.Monospaced("Slider Input")

                  | Text.Block("USD (Default)")
                  | usdValue
                    .ToMoneyInput("Enter amount")
                    .Currency("USD")
                  | usdValue
                    .ToMoneyInput("Enter amount")
                    .Variant(NumberInputVariant.Slider)
                    .Currency("USD")

                  | Text.Block("EUR")
                  | eurValue
                    .ToMoneyInput("Enter amount")
                    .Currency("EUR")
                  | eurValue
                    .ToMoneyInput("Enter amount")
                    .Variant(NumberInputVariant.Slider)
                    .Currency("EUR")

                  | Text.Block("GBP")
                  | gbpValue
                    .ToMoneyInput("Enter amount")
                    .Currency("GBP")
                  | gbpValue
                    .ToMoneyInput("Enter amount")
                    .Variant(NumberInputVariant.Slider)
                    .Currency("GBP")

                  | Text.Block("JPY")
                  | jpyValue
                    .ToMoneyInput("Enter amount")
                    .Currency("JPY")
                  | jpyValue
                    .ToMoneyInput("Enter amount")
                    .Variant(NumberInputVariant.Slider)
                    .Currency("JPY")

                  | Text.Block("Null Value")
                  | nullCurrencyValue
                    .ToMoneyInput("Enter amount")
                    .Currency("USD")
                  | nullCurrencyValue
                    .ToMoneyInput("Enter amount")
                    .Variant(NumberInputVariant.Slider)
                    .Currency("USD")
               )

               | Text.H3("Format Styles")
               | (Layout.Grid().Columns(4)
                  | Text.Monospaced("Style")
                  | Text.Monospaced("Example")
                  | Text.Monospaced("Number Input")
                  | Text.Monospaced("Slider Input")

                  | Text.Block("Decimal")
                  | Text.Block("1234.56")
                  | usdValue
                    .ToNumberInput()
                    .FormatStyle(NumberFormatStyle.Decimal)
                  | usdValue
                    .ToSliderInput()
                    .FormatStyle(NumberFormatStyle.Decimal)

                  | Text.Block("Currency")
                  | Text.Block("$1,234.56")
                  | usdValue
                    .ToNumberInput()
                    .FormatStyle(NumberFormatStyle.Currency)
                    .Currency("USD")
                  | usdValue
                    .ToSliderInput()
                    .FormatStyle(NumberFormatStyle.Currency)
                    .Currency("USD")

                  | Text.Block("Percent")
                  | Text.Block("123.46%")
                  | usdValue
                    .ToNumberInput()
                    .FormatStyle(NumberFormatStyle.Percent)
                  | usdValue
                    .ToSliderInput()
                    .FormatStyle(NumberFormatStyle.Percent)

                  | Text.Block("Compact")
                  | Text.Block("1.2M")
                  | compactValue
                    .ToNumberInput()
                    .FormatStyle(NumberFormatStyle.Compact)
                    .Precision(1)
                  | compactValue
                    .ToSliderInput()
                    .FormatStyle(NumberFormatStyle.Compact)
                    .Precision(1)

                  | Text.Block("Scientific")
                  | Text.Block("1.23E6")
                  | scientificValue
                    .ToNumberInput()
                    .FormatStyle(NumberFormatStyle.Scientific)
                    .Precision(2)
                  | scientificValue
                    .ToSliderInput()
                    .FormatStyle(NumberFormatStyle.Scientific)
                    .Precision(2)

                  | Text.Block("Engineering")
                  | Text.Block("1.23E6")
                  | engineeringValue
                    .ToNumberInput()
                    .FormatStyle(NumberFormatStyle.Engineering)
                    .Precision(2)
                  | engineeringValue
                    .ToSliderInput()
                    .FormatStyle(NumberFormatStyle.Engineering)
                    .Precision(2)

                  | Text.Block("Accounting")
                  | Text.Block("($1,234.56)")
                  | accountingValue
                    .ToNumberInput()
                    .FormatStyle(NumberFormatStyle.Accounting)
                    .Currency("USD")
                  | accountingValue
                    .ToSliderInput()
                    .FormatStyle(NumberFormatStyle.Accounting)
                    .Currency("USD")

                  | Text.Block("Bytes")
                  | Text.Block("5 MB")
                  | bytesValue
                    .ToNumberInput()
                    .FormatStyle(NumberFormatStyle.Bytes)
                  | bytesValue
                    .ToSliderInput()
                    .FormatStyle(NumberFormatStyle.Bytes)
               )

               | Text.H3("Currency with Constraints")
               | (Layout.Grid().Columns(3)
                  | Text.Monospaced("Description")
                  | Text.Monospaced("Number Input")
                  | Text.Monospaced("Slider Input")

                  | Text.Block("USD with Min/Max")
                  | usdValue
                    .ToMoneyInput("Enter amount")
                    .Currency("USD")
                    .Min(0)
                    .Max(10000)
                  | usdValue
                    .ToMoneyInput("Enter amount")
                    .Variant(NumberInputVariant.Slider)
                    .Currency("USD")
                    .Min(0)
                    .Max(10000)

                  | Text.Block("EUR with Step")
                  | eurValue
                    .ToMoneyInput("Enter amount")
                    .Currency("EUR")
                    .Step(0.01)
                  | eurValue
                    .ToMoneyInput("Enter amount")
                    .Variant(NumberInputVariant.Slider)
                    .Currency("EUR")
                    .Step(0.01)

                  | Text.Block("GBP with Precision")
                  | gbpValue
                    .ToMoneyInput("Enter amount")
                    .Currency("GBP")
                    .Precision(2)
                  | gbpValue
                    .ToMoneyInput("Enter amount")
                    .Variant(NumberInputVariant.Slider)
                    .Currency("GBP")
                    .Precision(2)
               );
    }
}

public class NumberInputSizesTab : ViewBase
{
    public override object Build()
    {
        var intValue = UseState(12345);

        return Layout.Grid().Columns(4)
               | Text.Monospaced("Description")
               | Text.Monospaced("Small")
               | Text.Monospaced("Medium")
               | Text.Monospaced("Large")

               | Text.Monospaced("ToNumberInput()")
               | intValue
                 .ToNumberInput()
                 .Small()
               | intValue
                 .ToNumberInput()
               | intValue
                 .ToNumberInput()
                 .Large()

               | Text.Monospaced("ToSliderInput")
               | intValue
                 .ToSliderInput()
                 .Small()
               | intValue
                 .ToSliderInput()
               | intValue
                 .ToSliderInput()
                 .Large();
    }
}

public class NumberInputEventsTab : ViewBase
{
    public override object Build()
    {
        var onChangedState = UseState(0);
        var onChangeLabel = UseState("");
        var onBlurState = UseState(0);
        var onBlurLabel = UseState("");
        var onFocusState = UseState(0);
        var onFocusLabel = UseState("");

        UseEffect(() => { onChangeLabel.Set("Changed"); }, onChangedState);

        return Layout.Vertical()
               | Text.H3("OnChange")
               | Layout.Horizontal(
                   onChangedState.ToNumberInput(),
                   onChangeLabel
               )
               | new Card(
                   Layout.Vertical().Gap(2)
                       | Text.P("The blur event fires when the number input loses focus.").Small()
                       | onBlurState.ToNumberInput().OnBlur(e => onBlurLabel.Set("Blur Event Triggered"))
                       | (onBlurLabel.Value != ""
                           ? Callout.Success(onBlurLabel.Value)
                           : Callout.Info("Interact then click away to see blur events"))
               ).Title("OnBlur Handler")
               | new Card(
                   Layout.Vertical().Gap(2)
                       | Text.P("The focus event fires when you click on or tab into the number input.").Small()
                       | onFocusState.ToNumberInput().OnFocus(e => onFocusLabel.Set("Focus Event Triggered"))
                       | (onFocusLabel.Value != ""
                           ? Callout.Success(onFocusLabel.Value)
                           : Callout.Info("Click or tab into the input to see focus events"))
               ).Title("OnFocus Handler");
    }
}
