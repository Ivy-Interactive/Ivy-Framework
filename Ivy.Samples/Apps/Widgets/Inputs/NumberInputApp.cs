using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Inputs;

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

        return Layout.Vertical()
               | Text.H1("Number Inputs")
               | Text.H2("Variants")
               | (Layout.Grid().Columns(5)
                  | null!
                  | Text.Block("Null")
                  | Text.Block("With Value")
                  | Text.Block("Disabled")
                  | Text.Block("Invalid")

                  | Text.InlineCode("ToNumberInput()")
                  | nullIntValue.ToNumberInput().Placeholder("Placeholder")
                  | intValue.ToNumberInput()
                  | intValue.ToNumberInput().Disabled()
                  | intValue.ToNumberInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros")

                  | Text.InlineCode("ToSliderInput()")
                  | nullIntValue.ToSliderInput().Placeholder("Placeholder")
                  | intValue.ToSliderInput()
                  | intValue.ToSliderInput().Disabled()
                  | intValue.ToSliderInput().Invalid("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec eros")
               )

               //Data Binding:

               | Text.H2("Data Binding")
               | dataBinding

               //Events: 

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
                   onBlurState.ToNumberInput().HandleBlur(e => onBlurLabel.Set("Blur")),
                   onBlurLabel
               )
            ;
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