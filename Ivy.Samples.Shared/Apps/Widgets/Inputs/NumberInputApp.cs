using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.DollarSign, path: ["Widgets", "Inputs"])]
public class NumberInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        var nullIntValue = UseState<int?>();
        var intValue = UseState(12345);

        var onChangedState = UseState(0);
        var onChangeLabel = UseState("");
        var onBlurState = UseState(0);
        var onBlurLabel = UseState("");

        var dataBinding = CreateNumericTypeTests();
        var currencyExamples = CreateCurrencyExamples();

        var nullIntInvalid = UseState<int?>();

        const string loremIpsumString = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros";

        return Layout.Vertical()

               // Main grid with variants
               | Text.H1("Number Inputs")
               | Text.H2("Variants")
               | (Layout.Grid().Columns(6)

                  | null!
                  | Text.InlineCode("Null")
                  | Text.InlineCode("With Value")
                  | Text.InlineCode("Disabled")
                  | Text.InlineCode("Invalid")
                  | Text.InlineCode("Invalid Nullable")

                  | Text.InlineCode("ToNumberInput()")
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

                  | Text.InlineCode("ToSliderInput()")
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
                    .TestId("number-input-nullable-invalid-slider-main")
               )

               // Data Binding:
               | Text.H2("Data Binding")
               | dataBinding

               // Currency Examples:
               | Text.H2("Currency Examples")
               | currencyExamples

               // Events: 
               | Text.H2("Events")
               | Text.H3("OnChange")
               | Layout.Horizontal(
                   new NumberInput<int>(onChangedState.Value, e =>
                   {
                       onChangedState.Set(e);
                       onChangeLabel.Set("Changed");
                   }),
                   onChangeLabel
               )
               | Text.H3("OnBlur")
               | Layout.Horizontal(
                   onBlurState
                    .ToNumberInput()
                    .HandleBlur(e => onBlurLabel.Set("Blur")),
                   onBlurLabel
               )
            ;
    }

    private object CreateCurrencyExamples()
    {
        var usdValue = UseState(1234.56m);
        var eurValue = UseState(987.65m);
        var gbpValue = UseState(567.89m);
        var jpyValue = UseState(12345m);
        var nullCurrencyValue = UseState<decimal?>(() => null);

        return Layout.Vertical()
               | Text.H3("Different Currencies")
               | (Layout.Grid().Columns(3)
                  | Text.InlineCode("Currency")
                  | Text.InlineCode("Number Input")
                  | Text.InlineCode("Slider Input")

                  | Text.Block("USD (Default)")
                  | usdValue
                    .ToMoneyInput("Enter amount")
                    .Currency("USD")
                  | usdValue
                    .ToMoneyInput("Enter amount")
                    .Variant(NumberInputs.Slider)
                    .Currency("USD")

                  | Text.Block("EUR")
                  | eurValue
                    .ToMoneyInput("Enter amount")
                    .Currency("EUR")
                  | eurValue
                    .ToMoneyInput("Enter amount")
                    .Variant(NumberInputs.Slider)
                    .Currency("EUR")

                  | Text.Block("GBP")
                  | gbpValue
                    .ToMoneyInput("Enter amount")
                    .Currency("GBP")
                  | gbpValue
                    .ToMoneyInput("Enter amount")
                    .Variant(NumberInputs.Slider)
                    .Currency("GBP")

                  | Text.Block("JPY")
                  | jpyValue
                    .ToMoneyInput("Enter amount")
                    .Currency("JPY")
                  | jpyValue
                    .ToMoneyInput("Enter amount")
                    .Variant(NumberInputs.Slider)
                    .Currency("JPY")

                  | Text.Block("Null Value")
                  | nullCurrencyValue
                    .ToMoneyInput("Enter amount")
                    .Currency("USD")
                  | nullCurrencyValue
                    .ToMoneyInput("Enter amount")
                    .Variant(NumberInputs.Slider)
                    .Currency("USD")
               )

               | Text.H3("Format Styles")
               | (Layout.Grid().Columns(4)
                  | Text.InlineCode("Style")
                  | Text.InlineCode("Example")
                  | Text.InlineCode("Number Input")
                  | Text.InlineCode("Slider Input")

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
               )

               | Text.H3("Currency with Constraints")
               | (Layout.Grid().Columns(3)
                  | Text.InlineCode("Description")
                  | Text.InlineCode("Number Input")
                  | Text.InlineCode("Slider Input")

                  | Text.Block("USD with Min/Max")
                  | usdValue
                    .ToMoneyInput("Enter amount")
                    .Currency("USD")
                    .Min(0)
                    .Max(10000)
                  | usdValue
                    .ToMoneyInput("Enter amount")
                    .Variant(NumberInputs.Slider)
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
                    .Variant(NumberInputs.Slider)
                    .Currency("EUR")
                    .Step(0.01)

                  | Text.Block("GBP with Precision")
                  | gbpValue
                    .ToMoneyInput("Enter amount")
                    .Currency("GBP")
                    .Precision(2)
                  | gbpValue
                    .ToMoneyInput("Enter amount")
                    .Variant(NumberInputs.Slider)
                    .Currency("GBP")
                    .Precision(2)
               );
    }

    private object CreateNumericTypeTests()
    {
        var numericTypes = new (string TypeName, object NonNullableState, object NullableState)[]
        {
            // Signed integer types
            ("short", UseState((short)0), UseState((short?)null)),
            ("int", UseState(0), UseState((int?)null)),
            ("long", UseState((long)0), UseState((long?)null)),

            // Unsigned integer types
            ("byte", UseState((byte)0), UseState((byte?)null)),

            // Floating-point types
            ("float", UseState(0.0f), UseState((float?)null)),
            ("double", UseState(0.0), UseState((double?)null)),
            ("decimal", UseState((decimal)0), UseState((decimal?)null))
        };

        var gridItems = new List<object>
        {
            Text.InlineCode("Type"),
            Text.InlineCode("Non-Nullable"),
            Text.InlineCode("State"),
            Text.InlineCode("Type"),
            Text.InlineCode("Nullable"),
            Text.InlineCode("State")
        };

        var numericTypeNames = new[] { "double", "decimal", "float", "short", "int", "long", "byte" };

        foreach (var (typeName, nonNullableState, nullableState) in numericTypes)
        {
            // Non-nullable columns (first 3)
            gridItems.Add(Text.InlineCode(typeName));
            gridItems.Add(CreateNumberInputVariants(nonNullableState));

            var nonNullableAnyState = nonNullableState as IAnyState;
            object? nonNullableValue = null;
            if (nonNullableAnyState != null)
            {
                var prop = nonNullableAnyState.GetType().GetProperty("Value");
                nonNullableValue = prop?.GetValue(nonNullableAnyState);
            }

            gridItems.Add(FormatStateValue(typeName, nonNullableValue, false));

            // Nullable columns (next 3)
            gridItems.Add(Text.InlineCode($"{typeName}?"));
            gridItems.Add(CreateNumberInputVariants(nullableState));

            var anyState = nullableState as IAnyState;
            object? value = null;
            if (anyState != null)
            {
                var prop = anyState
                            .GetType()
                            .GetProperty("Value");
                value = prop?.GetValue(anyState);
            }

            gridItems.Add(FormatStateValue(typeName, value, true));
        }

        return Layout.Grid().Columns(6) | gridItems.ToArray();

        object FormatStateValue(string typeName, object? value, bool isNullable)
        {
            return value switch
            {
                null => isNullable ? Text.InlineCode("Null") : Text.InlineCode("0"),
                _ when numericTypeNames.Contains(typeName) => Text.InlineCode(value.ToString()!),
                _ => Text.InlineCode(value?.ToString() ?? "null")
            };
        }
    }

    private static object CreateNumberInputVariants(object state)
    {
        if (state is not IAnyState anyState)
            return Text.Block("Not an IAnyState");

        var stateType = anyState.GetStateType();
        var isNullable = stateType.IsNullableType();

        // For both nullable and non-nullable states, show both number input and slider variants
        return Layout.Vertical()
               | anyState.ToNumberInput()
               | anyState.ToSliderInput();
    }
}