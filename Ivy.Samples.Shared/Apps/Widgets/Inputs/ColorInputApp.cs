using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.PaintBucket, path: ["Widgets", "Inputs"])]
public class ColorInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        var variants = CreateVariantsSection();
        var dataBinding = CreateDataBindingTests();
        var formatTests = CreateFormatTests();
        var nonGenericConstructor = CreateNonGenericConstructorSection();

        return Layout.Vertical()
               | Text.H1("ColorInput")
               | Text.H2("Non-Generic Constructor")
               | nonGenericConstructor
               | Text.H2("Variants")
               | variants
               | Text.H2("Format Tests")
               | formatTests
               | Text.H2("Data Binding")
               | dataBinding
            ;
    }

    private object CreateVariantsSection()
    {
        var textState = UseState("#381ff4");
        var pickerState = UseState("#dd5860");
        var bothState = UseState("#6637d1");
        var nullTextState = UseState((string?)null);
        var nullPickerState = UseState((string?)null);
        var nullBothState = UseState((string?)null);

        return Layout.Grid().Columns(6)
            | Text.InlineCode("")
            | Text.InlineCode("Default")
            | Text.InlineCode("Invalid")
            | Text.InlineCode("Disabled")
            | Text.InlineCode("Nullable")
            | Text.InlineCode("Nullable + Invalid")

            | Text.InlineCode("Text Only")
            | textState.ToColorInput().Variant(ColorInputs.Text)
            | textState.ToColorInput().Variant(ColorInputs.Text).Invalid("Invalid color")
            | textState.ToColorInput().Variant(ColorInputs.Text).Disabled()
            | nullTextState.ToColorInput().Variant(ColorInputs.Text)
            | nullTextState.ToColorInput().Variant(ColorInputs.Text).Invalid("Invalid color")

            | Text.InlineCode("Picker Only")
            | pickerState.ToColorInput().Variant(ColorInputs.Picker)
            | pickerState.ToColorInput().Variant(ColorInputs.Picker).Invalid("Invalid color")
            | pickerState.ToColorInput().Variant(ColorInputs.Picker).Disabled()
            | nullPickerState.ToColorInput().Variant(ColorInputs.Picker)
            | nullPickerState.ToColorInput().Variant(ColorInputs.Picker).Invalid("Invalid color")

            | Text.InlineCode("Text and Picker")
            | bothState.ToColorInput().Variant(ColorInputs.TextAndPicker)
            | bothState.ToColorInput().Variant(ColorInputs.TextAndPicker).Invalid("Invalid color")
            | bothState.ToColorInput().Variant(ColorInputs.TextAndPicker).Disabled()
            | nullBothState.ToColorInput().Variant(ColorInputs.TextAndPicker)
            | nullBothState.ToColorInput().Variant(ColorInputs.TextAndPicker).Invalid("Invalid color");
    }

    private object CreateNonGenericConstructorSection()
    {
        var defaultConstructorState = UseState("#dd5860");
        var placeholderState = UseState<string?>(() => null);
        var stateBindingState = UseState("#ff0000");
        var disabledState = UseState<string?>(() => null);
        var textOnlyState = UseState<string?>(() => null);
        var pickerOnlyState = UseState("#000000");
        var fullConstructorState = UseState("#000000");

        return Layout.Grid().Columns(3)
               | Text.InlineCode("Method")
               | Text.InlineCode("ColorInput")
               | Text.InlineCode("State Value")

               | Text.InlineCode("Default Constructor")
               | defaultConstructorState.ToColorInput()
               | Text.InlineCode(defaultConstructorState.Value ?? "No state")

               | Text.InlineCode("With Placeholder")
               | placeholderState.ToColorInput().Placeholder("Select a color")
               | Text.InlineCode(placeholderState.Value ?? "No state")

               | Text.InlineCode("With State Binding")
               | stateBindingState.ToColorInput()
               | Text.InlineCode(stateBindingState.Value ?? "No state")

               | Text.InlineCode("Disabled")
               | disabledState.ToColorInput().Disabled()
               | Text.InlineCode(disabledState.Value ?? "No state")

               | Text.InlineCode("Text Only Variant")
               | textOnlyState.ToColorInput().Variant(ColorInputs.Text)
               | Text.InlineCode(textOnlyState.Value ?? "No state")

               | Text.InlineCode("Picker Only Variant")
               | pickerOnlyState.ToColorInput().Variant(ColorInputs.Picker)
               | Text.InlineCode(pickerOnlyState.Value ?? "No state")

               | Text.InlineCode("Full Constructor")
               | fullConstructorState.ToColorInput().Placeholder("Choose your color").Variant(ColorInputs.TextAndPicker)
               | Text.InlineCode(fullConstructorState.Value ?? "No state");
    }

    private object CreateFormatTests()
    {
        var hexState = UseState("#ff0000");
        var rgbState = UseState("rgb(255, 0, 0)");
        var enumState = UseState(Colors.Red);

        return Layout.Grid().Columns(4)
               | Text.InlineCode("Format")
               | Text.InlineCode("Input")
               | Text.InlineCode("Display Value")
               | Text.InlineCode("Stored Value")

               | Text.InlineCode("Hex")
               | hexState.ToColorInput()
               | Text.InlineCode(hexState.Value)
               | Text.InlineCode(hexState.Value)

               | Text.InlineCode("RGB")
               | rgbState.ToColorInput()
               | Text.InlineCode(rgbState.Value)
               | Text.InlineCode(ConvertToHex(rgbState.Value))

               | Text.InlineCode("Enum")
               | enumState.ToColorInput()
               | Text.InlineCode(enumState.Value.ToString())
               | Text.InlineCode(ConvertToHex(enumState.Value.ToString()))
            ;
    }

    private object CreateDataBindingTests()
    {
        var colorTypes = new (string TypeName, object NonNullableState, object NullableState)[]
        {
            ("string", UseState("#ff0000"), UseState((string?)null)),
            ("Colors", UseState(Colors.Red), UseState((Colors?)null))
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

        foreach (var (typeName, nonNullableState, nullableState) in colorTypes)
        {
            // Non-nullable columns (first 3)
            gridItems.Add(Text.InlineCode(typeName));
            gridItems.Add(CreateColorInputVariants(nonNullableState));

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
            gridItems.Add(CreateColorInputVariants(nullableState));

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
    }

    private static object CreateColorInputVariants(object state)
    {
        if (state is not IAnyState anyState)
            return Text.Block("Not an IAnyState");

        var stateType = anyState.GetStateType();
        var isNullable = stateType.IsNullableType();

        if (isNullable)
        {
            // For nullable states, show basic variant
            return anyState.ToColorInput();
        }

        // For non-nullable states, show all variants
        return Layout.Vertical()
               | anyState.ToColorInput()
               | anyState.ToColorInput().Placeholder("Select color")
               | anyState.ToColorInput().Disabled();
    }

    private static object FormatStateValue(string typeName, object? value, bool isNullable)
    {
        return value switch
        {
            null => isNullable ? Text.InlineCode("Null") : Text.InlineCode("Default"),
            string s => Text.InlineCode(s),
            Colors c => Text.InlineCode(c.ToString()),
            _ => Text.InlineCode(value?.ToString() ?? "null")
        };
    }

    private static string ConvertToHex(string? colorValue)
    {
        if (string.IsNullOrEmpty(colorValue))
            return "null";

        // Simple conversion for demo purposes
        // In a real implementation, you'd want proper color parsing
        return colorValue.StartsWith("#") ? colorValue : $"#{colorValue.GetHashCode():X6}";
    }
}