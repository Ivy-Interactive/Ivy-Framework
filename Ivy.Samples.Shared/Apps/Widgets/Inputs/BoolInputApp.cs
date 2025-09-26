using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Check, path: ["Widgets", "Inputs"])]
public class BoolInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        var falseState = UseState(false);
        var trueState = UseState(true);
        var nullState = UseState((bool?)null);

        var variants = Layout.Grid().Columns(6)
                       | null!
                       | Text.InlineCode("True")
                       | Text.InlineCode("False")
                       | Text.InlineCode("Disabled")
                       | Text.InlineCode("Invalid")
                       | Text.InlineCode("Nullable")
                       | Text.InlineCode("BoolInputs.Checkbox")
                       | trueState
                           .ToBoolInput()
                           .Label("Label")
                           .Description("Description")
                           .TestId("checkbox-true-state-width-description")
                       | falseState
                           .ToBoolInput()
                           .Label("Label")
                           .Description("Description")
                           .TestId("checkbox-false-state-width-description")
                       | trueState
                           .ToBoolInput()
                           .Label("Label")
                           .Description("Description")
                           .Disabled()
                           .TestId("checkbox-true-state-width-description-disabled")
                       | trueState
                           .ToBoolInput()
                           .Label("Label")
                           .Description("Description")
                           .Invalid("Invalid")
                           .TestId("checkbox-true-state-width-description-invalid")
                       | nullState.ToBoolInput()
                           .Label("Label")
                           .Description("Description")
                           .TestId("checkbox-null-state-width-description")
                       | null!
                       | trueState
                           .ToBoolInput()
                           .Label("Label")
                           .TestId("checkbox-true-state-width")
                       | falseState
                           .ToBoolInput()
                           .Label("Label")
                           .TestId("checkbox-false-state-width")
                       | trueState
                           .ToBoolInput()
                           .Label("Label").Disabled()
                           .TestId("checkbox-true-state-width-disabled")
                       | trueState
                           .ToBoolInput()
                           .Label("Label").Invalid("Invalid")
                           .TestId("checkbox-true-state-width-invalid")
                       | nullState
                           .ToBoolInput()
                           .Label("Label")
                           .TestId("checkbox-null-state-width")
                       | Text.InlineCode("BoolInputs.Switch")
                       | trueState
                           .ToSwitchInput()
                           .Label("Label")
                           .Description("Description")
                           .TestId("switch-true-state-width-description")
                       | falseState
                           .ToSwitchInput()
                           .Label("Label")
                           .Description("Description")
                           .TestId("switch-false-state-width-description")
                       | trueState
                           .ToSwitchInput()
                           .Label("Label")
                           .Description("Description")
                           .Disabled()
                           .TestId("switch-true-state-width-description-disabled")
                       | trueState
                           .ToSwitchInput()
                           .Label("Label")
                           .Description("Description")
                           .Invalid("Invalid")
                           .TestId("switch-true-state-width-description-invalid")
                       | new Box("N/A")
                       | null!
                       | trueState
                           .ToSwitchInput()
                           .Label("Label")
                           .TestId("switch-true-state-width")
                       | falseState
                           .ToSwitchInput()
                           .Label("Label")
                           .TestId("switch-false-state-width")
                       | trueState
                           .ToSwitchInput()
                           .Label("Label")
                           .Disabled()
                           .TestId("switch-true-state-width-disabled")
                       | trueState
                           .ToSwitchInput()
                           .Label("Label")
                           .Invalid("Invalid")
                           .TestId("switch-true-state-width-invalid")
                       | new Box("N/A")
                       | Text.InlineCode("BoolInputs.Toggle")
                       | trueState
                           .ToToggleInput(Icons.Magnet)
                           .Label("Label")
                           .Description("Description")
                           .TestId("toggle-true-state-width-description")


                       | falseState
                           .ToToggleInput(Icons.Magnet)
                           .Label("Label")
                           .Description("Description")
                           .TestId("toggle-false-state-width-description")
                       | trueState
                           .ToToggleInput(Icons.Magnet)
                           .Label("Label")
                           .Description("Description")
                           .Disabled()
                           .TestId("toggle-true-state-width-description-disabled")
                       | trueState
                           .ToToggleInput(Icons.Magnet)
                           .Label("Label")
                           .Description("Description")
                           .Invalid("Invalid")
                           .TestId("toggle-true-state-width-description-invalid")
                       | new Box("N/A")
                       | null!
                       | trueState
                           .ToToggleInput(Icons.Baby)
                           .Label("Label")
                           .TestId("toggle-true-state-width")
                       | falseState
                           .ToToggleInput(Icons.Baby)
                           .Label("Label")
                           .TestId("toggle-false-state-width")
                       | trueState
                           .ToToggleInput(Icons.Baby)
                           .Label("Label").Disabled()
                           .TestId("toggle-true-state-width-disabled")
                       | trueState
                           .ToToggleInput(Icons.Baby)
                           .Label("Label")
                           .Invalid("Invalid")
                           .TestId("toggle-true-state-width-invalid")
                       | new Box("N/A")

            ;

        var dataBinding = CreateNumericTypeTests();

        return Layout.Vertical()
               | Text.H1("BoolInput")
               | Text.H2("Variants")
               | variants
               | Text.H2("Data Binding")
               | dataBinding
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
            ("decimal", UseState((decimal)0), UseState((decimal?)null)),

            // Boolean types
            ("bool", UseState(false), UseState((bool?)null))
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
            gridItems.Add(CreateBoolInputVariants(nonNullableState));

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
            gridItems.Add(CreateBoolInputVariants(nullableState));

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
                bool b => Text.InlineCode(b.ToString()),
                _ when numericTypeNames.Contains(typeName) => Text.InlineCode(value.ToString()!),
                _ => Text.InlineCode(value?.ToString() ?? "null")
            };
        }
    }

    private static object CreateBoolInputVariants(object state)
    {
        if (state is not IAnyState anyState)
            return Text.Block("Not an IAnyState");

        var stateType = anyState.GetStateType();
        var isNullable = stateType.IsNullableType();

        if (isNullable)
        {
            // For nullable states, only show checkbox variant
            return anyState.ToBoolInput();
        }

        // For non-nullable states, show all three variants
        return Layout.Vertical()
               | anyState.ToBoolInput()
               | anyState
                   .ToBoolInput()
                   .Variant(BoolInputs.Switch)
               | anyState
                   .ToBoolInput()
                   .Variant(BoolInputs.Toggle)
                   .Icon(Icons.Star);
    }
}