
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.GripVertical, group: ["Widgets", "Inputs"], searchHints: ["numeric", "range", "slider", "min", "max", "interval"])]
public class NumberRangeInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("Number Range Input")
               | Layout.Tabs(
                   new Tab("Variants", new NumberRangeInputVariantsTab()),
                   new Tab("Data Binding", new NumberRangeInputDataBindingTab()),
                   new Tab("Format & Currency", new NumberRangeInputFormatCurrencyTab()),
                   new Tab("Prefix & Suffix", new NumberRangeInputPrefixSuffixTab()),
                   new Tab("Sizes", new NumberRangeInputSizesTab()),
                   new Tab("Events", new NumberRangeInputEventsTab())
               ).Variant(TabsVariant.Content);
    }
}

public class NumberRangeInputVariantsTab : ViewBase
{
    public override object Build()
    {
        var intRange = UseState<(int, int)>(() => (25, 75));
        var nullableIntRange = UseState<(int?, int?)>(() => (25, 75));

        const string loremIpsumString = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros";

        return Layout.Grid().Columns(5)
               | Text.Monospaced("Normal")
               | Text.Monospaced("Nullable")
               | Text.Monospaced("Disabled")
               | Text.Monospaced("Invalid")
               | Text.Monospaced("Nullable Invalid")
               | intRange
                   .ToNumberRangeInput()
                   .Min(0)
                   .Max(100)
                   .TestId("numberrange-input-normal")
               | nullableIntRange
                   .ToNumberRangeInput()
                   .Min(0)
                   .Max(100)
                   .TestId("numberrange-input-nullable")
               | intRange
                   .ToNumberRangeInput()
                   .Min(0)
                   .Max(100)
                   .Disabled()
                   .TestId("numberrange-input-disabled")
               | intRange
                   .ToNumberRangeInput()
                   .Min(0)
                   .Max(100)
                   .Invalid(loremIpsumString)
                   .TestId("numberrange-input-invalid")
               | nullableIntRange
                   .ToNumberRangeInput()
                   .Min(0)
                   .Max(100)
                   .Invalid(loremIpsumString)
                   .TestId("numberrange-input-nullable-invalid");
    }
}

public class NumberRangeInputDataBindingTab : ViewBase
{
    public override object Build()
    {
        var shortRangeState = UseState<(short, short)>(() => (10, 90));
        var shortNullRangeState = UseState<(short?, short?)>(() => (10, 90));
        var intRangeState = UseState<(int, int)>(() => (10, 90));
        var intNullRangeState = UseState<(int?, int?)>(() => (10, 90));
        var longRangeState = UseState<(long, long)>(() => (10, 90));
        var longNullRangeState = UseState<(long?, long?)>(() => (10, 90));
        var byteRangeState = UseState<(byte, byte)>(() => (10, 90));
        var byteNullRangeState = UseState<(byte?, byte?)>(() => (10, 90));
        var floatRangeState = UseState<(float, float)>(() => (10.0f, 90.0f));
        var floatNullRangeState = UseState<(float?, float?)>(() => (10.0f, 90.0f));
        var doubleRangeState = UseState<(double, double)>(() => (10.0, 90.0));
        var doubleNullRangeState = UseState<(double?, double?)>(() => (10.0, 90.0));
        var decimalRangeState = UseState<(decimal, decimal)>(() => (10.0m, 90.0m));
        var decimalNullRangeState = UseState<(decimal?, decimal?)>(() => (10.0m, 90.0m));

        var numericTypes = new (string TypeName, object NonNullableState, object NullableState)[]
        {
            ("short", shortRangeState, shortNullRangeState),
            ("int", intRangeState, intNullRangeState),
            ("long", longRangeState, longNullRangeState),
            ("byte", byteRangeState, byteNullRangeState),
            ("float", floatRangeState, floatNullRangeState),
            ("double", doubleRangeState, doubleNullRangeState),
            ("decimal", decimalRangeState, decimalNullRangeState)
        };

        return CreateDataBindingGrid(numericTypes);
    }

    private static object CreateDataBindingGrid((string TypeName, object NonNullableState, object NullableState)[] numericTypes)
    {
        var gridItems = new List<object>
        {
            Text.Monospaced("Type"),
            Text.Monospaced("Range Input"),
            Text.Monospaced("Current Value")
        };

        foreach (var (typeName, nonNullableState, nullableState) in numericTypes)
        {
            gridItems.Add(Text.Monospaced($"({typeName}, {typeName})"));

            if (nonNullableState is IAnyState anyState)
            {
                gridItems.Add(anyState.ToNumberRangeInput().Min(0).Max(100));

                var prop = anyState.GetType().GetProperty("Value");
                var value = prop?.GetValue(anyState);
                if (value != null)
                {
                    var item1Prop = value.GetType().GetProperty("Item1");
                    var item2Prop = value.GetType().GetProperty("Item2");
                    var item1 = item1Prop?.GetValue(value);
                    var item2 = item2Prop?.GetValue(value);
                    gridItems.Add(Text.Monospaced($"{item1} - {item2}"));
                }
                else
                {
                    gridItems.Add(Text.Monospaced("null"));
                }
            }
            else
            {
                gridItems.Add(Text.Block("Not an IAnyState"));
                gridItems.Add(Text.Block(""));
            }

            gridItems.Add(Text.Monospaced($"({typeName}?, {typeName}?)"));

            if (nullableState is IAnyState nullableAnyState)
            {
                gridItems.Add(nullableAnyState.ToNumberRangeInput().Min(0).Max(100));

                var prop = nullableAnyState.GetType().GetProperty("Value");
                var value = prop?.GetValue(nullableAnyState);
                if (value != null)
                {
                    var item1Prop = value.GetType().GetProperty("Item1");
                    var item2Prop = value.GetType().GetProperty("Item2");
                    var item1 = item1Prop?.GetValue(value);
                    var item2 = item2Prop?.GetValue(value);
                    gridItems.Add(Text.Monospaced($"{item1?.ToString() ?? "null"} - {item2?.ToString() ?? "null"}"));
                }
                else
                {
                    gridItems.Add(Text.Monospaced("null"));
                }
            }
            else
            {
                gridItems.Add(Text.Block("Not an IAnyState"));
                gridItems.Add(Text.Block(""));
            }
        }

        return Layout.Grid().Columns(3) | gridItems.ToArray();
    }
}

public class NumberRangeInputFormatCurrencyTab : ViewBase
{
    public override object Build()
    {
        var intRange = UseState<(int, int)>(() => (25, 75));
        var priceRange = UseState<(decimal, decimal)>(() => (50.0m, 200.0m));
        var eurRange = UseState<(decimal, decimal)>(() => (100.0m, 500.0m));
        var percentRange = UseState<(double, double)>(() => (0.25, 0.75));

        return Layout.Vertical()
               | Text.H2("Format Styles")
               | (Layout.Grid().Columns(3)
                   | Text.Monospaced("Style")
                   | Text.Monospaced("Range Input")
                   | Text.Monospaced("Current Value")
                   | Text.Block("Decimal")
                   | intRange
                       .ToNumberRangeInput()
                       .Min(0)
                       .Max(100)
                       .FormatStyle(NumberFormatStyle.Decimal)
                       .TestId("numberrange-input-decimal")
                   | Text.Monospaced($"{intRange.Value.Item1} - {intRange.Value.Item2}")
                   | Text.Block("Currency (USD)")
                   | priceRange
                       .ToNumberRangeInput()
                       .Min(0)
                       .Max(500)
                       .FormatStyle(NumberFormatStyle.Currency)
                       .Currency("USD")
                       .TestId("numberrange-input-currency-usd")
                   | Text.Monospaced($"${priceRange.Value.Item1:F2} - ${priceRange.Value.Item2:F2}")
                   | Text.Block("Currency (EUR)")
                   | eurRange
                       .ToNumberRangeInput()
                       .Min(0)
                       .Max(1000)
                       .FormatStyle(NumberFormatStyle.Currency)
                       .Currency("EUR")
                       .TestId("numberrange-input-currency-eur")
                   | Text.Monospaced($"€{eurRange.Value.Item1:F2} - €{eurRange.Value.Item2:F2}")
                   | Text.Block("Percent")
                   | percentRange
                       .ToNumberRangeInput()
                       .Min(0)
                       .Max(1)
                       .Step(0.01)
                       .Precision(2)
                       .FormatStyle(NumberFormatStyle.Percent)
                       .TestId("numberrange-input-percent")
                   | Text.Monospaced($"{percentRange.Value.Item1:P0} - {percentRange.Value.Item2:P0}")
               )
               | Text.H2("Price Range Examples")
               | (Layout.Grid().Columns(3)
                   | Text.Monospaced("Description")
                   | Text.Monospaced("Range Input")
                   | Text.Monospaced("Current Value")
                   | Text.Block("Budget Filter ($50-$200)")
                   | priceRange
                       .ToMoneyRangeInput()
                       .Currency("USD")
                       .Min(0)
                       .Max(500)
                       .Step(10)
                       .Precision(2)
                       .TestId("numberrange-input-budget")
                   | Text.Monospaced($"${priceRange.Value.Item1:F2} - ${priceRange.Value.Item2:F2}")
                   | Text.Block("With Min/Max Constraints")
                   | priceRange
                       .ToMoneyRangeInput()
                       .Currency("USD")
                       .Min(0)
                       .Max(1000)
                       .Step(50)
                       .TestId("numberrange-input-constrained")
                   | Text.Monospaced($"${priceRange.Value.Item1:F2} - ${priceRange.Value.Item2:F2}")
               );
    }
}

public class NumberRangeInputPrefixSuffixTab : ViewBase
{
    public override object Build()
    {
        var intRange = UseState<(int, int)>(() => (25, 75));
        var priceRange = UseState<(decimal, decimal)>(() => (50.0m, 200.0m));
        var percentRange = UseState<(double, double)>(() => (0.25, 0.75));

        return Layout.Grid().Columns(3)
               | Text.Monospaced("Description")
               | Text.Monospaced("Range Input")
               | Text.Monospaced("Current Value")
               | Text.Block("Text Prefix ($)")
               | priceRange
                   .ToNumberRangeInput()
                   .Min(0)
                   .Max(500)
                   .Prefix("$")
                   .Precision(2)
                   .TestId("numberrange-input-prefix-text")
               | Text.Monospaced($"${priceRange.Value.Item1:F2} - ${priceRange.Value.Item2:F2}")
               | Text.Block("Text Suffix (%)")
               | percentRange
                   .ToNumberRangeInput()
                   .Min(0)
                   .Max(1)
                   .Step(0.01)
                   .Suffix("%")
                   .Precision(2)
                   .TestId("numberrange-input-suffix-percent")
               | Text.Monospaced($"{percentRange.Value.Item1 * 100:F0}% - {percentRange.Value.Item2 * 100:F0}%")
               | Text.Block("Icon Prefix (Thermometer)")
               | intRange
                   .ToNumberRangeInput()
                   .Min(-10)
                   .Max(40)
                   .Prefix(Icons.Thermometer)
                   .Suffix("°C")
                   .TestId("numberrange-input-icon-prefix")
               | Text.Monospaced($"{intRange.Value.Item1}°C - {intRange.Value.Item2}°C")

               | Text.Block("Button Prefix + Badge Suffix")
               | intRange
                   .ToNumberRangeInput()
                   .Min(0)
                   .Max(100)
                   .Prefix(new Button("Reset", () => { intRange.Value = (25, 75); }).Ghost().Small())
                   .Suffix(new Badge($"{intRange.Value.Item2 - intRange.Value.Item1} range", BadgeVariant.Secondary))
                   .TestId("numberrange-input-widget-affixes")
               | Text.Monospaced($"Spread: {intRange.Value.Item2 - intRange.Value.Item1}");
    }
}

public class NumberRangeInputSizesTab : ViewBase
{
    public override object Build()
    {
        var sizeRange = UseState<(int, int)>(() => (30, 70));

        return Layout.Grid().Columns(4)
               | Text.Monospaced("Description")
               | Text.Monospaced("Small")
               | Text.Monospaced("Medium")
               | Text.Monospaced("Large")
               | Text.Monospaced("Integer Range")
               | sizeRange
                   .ToNumberRangeInput()
                   .Min(0)
                   .Max(100)
                   .Small()
                   .TestId("numberrange-input-small")
               | sizeRange
                   .ToNumberRangeInput()
                   .Min(0)
                   .Max(100)
                   .TestId("numberrange-input-medium")
               | sizeRange
                   .ToNumberRangeInput()
                   .Min(0)
                   .Max(100)
                   .Large()
                   .TestId("numberrange-input-large");
    }
}

public class NumberRangeInputEventsTab : ViewBase
{
    public override object Build()
    {
        var onChangeState = UseState<(int, int)>(() => (0, 100));
        var onChangeLabel = UseState("");
        var onBlurState = UseState<(int, int)>(() => (0, 100));
        var onBlurLabel = UseState("");
        var onFocusState = UseState<(int, int)>(() => (0, 100));
        var onFocusLabel = UseState("");

        return Layout.Vertical()
               | Text.H3("OnChange")
               | Layout.Horizontal(
                   new NumberRangeInput<int>(onChangeState.Value, e =>
                   {
                       onChangeState.Set(e);
                       onChangeLabel.Set($"Changed to: {e.Item1} - {e.Item2}");
                   })
                   {
                       Min = 0,
                       Max = 100
                   },
                   Text.Monospaced(onChangeLabel.Value.Length > 0 ? onChangeLabel.Value : "Move the sliders")
               )
               | new Card(
                   Layout.Vertical().Gap(2)
                       | Text.P("The blur event fires when either of the range sliders loses focus.").Small()
                       | onBlurState.ToNumberRangeInput().Min(0).Max(100).OnBlur(e => onBlurLabel.Set("Blur Event Triggered"))
                       | (onBlurLabel.Value != ""
                           ? Callout.Success(onBlurLabel.Value)
                           : Callout.Info("Interact then click away to see blur events"))
               ).Title("OnBlur Handler")
               | new Card(
                   Layout.Vertical().Gap(2)
                       | Text.P("The focus event fires when you click on or tab into either of the range sliders.").Small()
                       | onFocusState.ToNumberRangeInput().Min(0).Max(100).OnFocus(e => onFocusLabel.Set("Focus Event Triggered"))
                       | (onFocusLabel.Value != ""
                           ? Callout.Success(onFocusLabel.Value)
                           : Callout.Info("Click or tab into the input to see focus events"))
               ).Title("OnFocus Handler")
               | Text.H3("Current Values")
               | Text.Block($"OnChange Range: {onChangeState.Value.Item1} - {onChangeState.Value.Item2}")
               | Text.Block($"OnBlur Range: {onBlurState.Value.Item1} - {onBlurState.Value.Item2}")
               | Text.Block($"OnFocus Range: {onFocusState.Value.Item1} - {onFocusState.Value.Item2}");
    }
}
