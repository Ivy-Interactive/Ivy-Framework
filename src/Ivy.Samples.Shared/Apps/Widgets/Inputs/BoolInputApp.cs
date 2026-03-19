
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Check, group: ["Widgets", "Inputs"], searchHints: ["checkbox", "switch", "toggle", "boolean", "true-false", "option"])]
public class BoolInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        var falseState = UseState(false);
        var trueState = UseState(true);
        var nullState = UseState((bool?)null);
        var loadingState = UseState(true);

        var variants = Layout.Grid().Columns(7)
                       | null!
                       | Text.Monospaced("True")
                       | Text.Monospaced("False")
                       | Text.Monospaced("Disabled")
                       | Text.Monospaced("Invalid")
                       | Text.Monospaced("Nullable")
                       | Text.Monospaced("Loading")
                       | Text.Monospaced("BoolInputVariant.Checkbox")
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
                       | trueState
                           .ToBoolInput()
                           .Label("Label")
                           .Description("Description")
                           .Loading(loadingState.Value)
                           .TestId("checkbox-loading-state-width-description")
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
                       | trueState
                           .ToBoolInput()
                           .Label("Label")
                           .Loading(loadingState.Value)
                           .TestId("checkbox-loading-state-width")
                       | Text.Monospaced("BoolInputVariant.Switch")
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
                       | trueState
                           .ToSwitchInput()
                           .Label("Label")
                           .Description("Description")
                           .Loading(loadingState.Value)
                           .TestId("switch-loading-state-width-description")
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
                       | trueState
                           .ToSwitchInput()
                           .Label("Label")
                           .Loading(loadingState.Value)
                           .TestId("switch-loading-state-width")
                       | Text.Monospaced("BoolInputVariant.Toggle")
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
                       | trueState
                           .ToToggleInput(Icons.Magnet)
                           .Label("Label")
                           .Description("Description")
                           .Loading(loadingState.Value)
                           .TestId("toggle-loading-state-width-description")
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
                       | trueState
                           .ToToggleInput(Icons.Baby)
                           .Label("Label")
                           .Loading(loadingState.Value)
                           .TestId("toggle-loading-state-width")

            ;

        return Layout.Vertical()
               | Text.H1("BoolInput")
               | Text.H2("Sizes")
               | new BoolInputSizes()
               | Text.H2("Icons")
               | new BoolInputIcons()
               | Text.H2("Variants")
               | variants
               | Text.H2("Data Binding")
               | new BoolInputDataBinding()
            ;
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
                   .Variant(BoolInputVariant.Switch)
               | anyState
                   .ToBoolInput()
                   .Variant(BoolInputVariant.Toggle)
                   .Icon(Icons.Star);
    }
}

public class BoolInputDataBinding : ViewBase
{
    public override object Build()
    {
        var shortState = UseState((short)0);
        var shortNullState = UseState((short?)null);
        var intState = UseState(0);
        var intNullState = UseState((int?)null);
        var longState = UseState((long)0);
        var longNullState = UseState((long?)null);
        var byteState = UseState((byte)0);
        var byteNullState = UseState((byte?)null);
        var floatState = UseState(0.0f);
        var floatNullState = UseState((float?)null);
        var doubleState = UseState(0.0);
        var doubleNullState = UseState((double?)null);
        var decimalState = UseState((decimal)0);
        var decimalNullState = UseState((decimal?)null);
        var boolState = UseState(false);
        var boolNullState = UseState((bool?)null);

        var numericTypes = new (string TypeName, object NonNullableState, object NullableState)[]
        {
            // Signed integer types
            ("short", shortState, shortNullState),
            ("int", intState, intNullState),
            ("long", longState, longNullState),

            // Unsigned integer types
            ("byte", byteState, byteNullState),

            // Floating-point types
            ("float", floatState, floatNullState),
            ("double", doubleState, doubleNullState),
            ("decimal", decimalState, decimalNullState),

            // Boolean types
            ("bool", boolState, boolNullState)
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
            // Non-nullable columns (first 3)
            gridItems.Add(Text.Monospaced(typeName));
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
            gridItems.Add(Text.Monospaced($"{typeName}?"));
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
                null => isNullable ? Text.Monospaced("Null") : Text.Monospaced("0"),
                bool b => Text.Monospaced(b.ToString()),
                _ when numericTypeNames.Contains(typeName) => Text.Monospaced(value.ToString()!),
                _ => Text.Monospaced(value?.ToString() ?? "null")
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
                   .Variant(BoolInputVariant.Switch)
               | anyState
                   .ToBoolInput()
                   .Variant(BoolInputVariant.Toggle)
                   .Icon(Icons.Star);
    }
}

public class BoolInputSizes : ViewBase
{
    public override object Build()
    {
        var trueState = UseState(true);
        var falseState = UseState(false);
        var nullState = UseState((bool?)null);

        return Layout.Grid().Columns(4)
               | Text.Monospaced("Description")
               | Text.Monospaced("Small")
               | Text.Monospaced("Medium")
               | Text.Monospaced("Large")

               | Text.Monospaced("BoolInputVariant.Checkbox")
               | trueState
                   .ToBoolInput()
                   .Label("Label")
                   .Small()
               | trueState
                   .ToBoolInput()
                   .Label("Label")
               | trueState
                   .ToBoolInput()
                   .Label("Label")
                   .Large()

               | Text.Monospaced("BoolInputVariant.Switch")
               | trueState
                   .ToSwitchInput()
                   .Label("Label")
                   .Small()
               | trueState
                   .ToSwitchInput()
                   .Label("Label")
               | trueState
                   .ToSwitchInput()
                   .Label("Label")
                   .Large()

               | Text.Monospaced("BoolInputVariant.Toggle")
               | trueState
                   .ToToggleInput(Icons.Star)
                   .Label("Label")
                   .Small()
               | trueState
                   .ToToggleInput(Icons.Star)
                   .Label("Label")
               | trueState
                   .ToToggleInput(Icons.Star)
                   .Label("Label")
                   .Large();
    }


}

public class BoolInputIcons : ViewBase
{
    public override object Build()
    {
        var trueState = UseState(true);

        return Layout.Grid().Columns(4)
               | Text.Monospaced("Description")
               | Text.Monospaced("Sun")
               | Text.Monospaced("Moon")
               | Text.Monospaced("Star")

               | Text.Monospaced("BoolInputVariant.Switch")
               | trueState
                   .ToSwitchInput(Icons.Sun)
                   .Label("Label")
               | trueState
                   .ToSwitchInput(Icons.Moon)
                   .Label("Label")
               | trueState
                   .ToSwitchInput(Icons.Star)
                   .Label("Label")

               | Text.Monospaced("BoolInputVariant.Toggle")
               | trueState
                   .ToToggleInput(Icons.Sun)
                   .Label("Label")
               | trueState
                   .ToToggleInput(Icons.Moon)
                   .Label("Label")
               | trueState
                   .ToToggleInput(Icons.Star)
                   .Label("Label");
    }
}
