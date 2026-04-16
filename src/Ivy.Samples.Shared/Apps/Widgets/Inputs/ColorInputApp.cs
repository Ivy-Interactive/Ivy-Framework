
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.PaintBucket, group: ["Widgets", "Inputs"], searchHints: ["picker", "palette", "color", "hex", "rgb", "swatch"])]
public class ColorInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("Color Input")
               | Layout.Tabs(
                   new Tab("Variants", new ColorInputVariants()),
                   new Tab("Size Variants", new ColorInputSizeVariants()),
                   new Tab("Alpha Channel", new ColorInputAlphaTests()),
                   new Tab("Format Tests", new ColorInputFormatTests()),
                   new Tab("Data Binding", new ColorInputDataBindings()),
                   new Tab("Events", new ColorInputEvents()),
                   new Tab("Affixes", new ColorInputAffixesExample())
               ).Variant(TabsVariant.Content);
    }
}

public class ColorInputSizeVariants : ViewBase
{
    public override object Build()
    {
        var smallTextState = UseState("#ff6b6b");
        var mediumTextState = UseState("#4ecdc4");
        var largeTextState = UseState("#45b7d1");
        var smallPickerState = UseState("#96ceb4");
        var mediumPickerState = UseState("#feca57");
        var largePickerState = UseState("#ff9ff3");
        var smallBothState = UseState("#54a0ff");
        var mediumBothState = UseState("#5f27cd");
        var largeBothState = UseState("#00d2d3");

        return Layout.Grid().Columns(4)
            | Text.Monospaced("Size")
            | Text.Monospaced("Text Only")
            | Text.Monospaced("Picker Only")
            | Text.Monospaced("Text and Picker")

            | Text.Monospaced("Small")
            | smallTextState.ToColorInput().Variant(ColorInputVariant.Text).Density(Density.Small)
            | smallPickerState.ToColorInput().Variant(ColorInputVariant.Picker).Density(Density.Small)
            | smallBothState.ToColorInput().Variant(ColorInputVariant.TextAndPicker).Density(Density.Small)

            | Text.Monospaced("Medium")
            | mediumTextState.ToColorInput().Variant(ColorInputVariant.Text).Density(Density.Medium)
            | mediumPickerState.ToColorInput().Variant(ColorInputVariant.Picker).Density(Density.Medium)
            | mediumBothState.ToColorInput().Variant(ColorInputVariant.TextAndPicker).Density(Density.Medium)

            | Text.Monospaced("Large")
            | largeTextState.ToColorInput().Variant(ColorInputVariant.Text).Density(Density.Large)
            | largePickerState.ToColorInput().Variant(ColorInputVariant.Picker).Density(Density.Large)
            | largeBothState.ToColorInput().Variant(ColorInputVariant.TextAndPicker).Density(Density.Large);
    }
}

public class ColorInputVariantTests : ViewBase
{
    public override object Build()
    {
        var textState = UseState("red");
        var pickerState = UseState("#dd5860");
        var bothState = UseState("#6637d1");
        var swatchState = UseState("blue");
        var ghostState = UseState("#9b59b6");
        var nullTextState = UseState((string?)null);
        var nullPickerState = UseState((string?)null);
        var nullBothState = UseState((string?)null);
        var nullSwatchState = UseState((string?)null);

        return Layout.Grid().Columns(6)
            | Text.Monospaced("")
            | Text.Monospaced("Default")
            | Text.Monospaced("Invalid")
            | Text.Monospaced("Disabled")
            | Text.Monospaced("Nullable")
            | Text.Monospaced("Nullable + Invalid")

            | Text.Monospaced("Text Only")
            | textState.ToColorInput().Variant(ColorInputVariant.Text)
            | textState.ToColorInput().Variant(ColorInputVariant.Text).Invalid("Invalid color")
            | textState.ToColorInput().Variant(ColorInputVariant.Text).Disabled()
            | nullTextState.ToColorInput().Variant(ColorInputVariant.Text)
            | nullTextState.ToColorInput().Variant(ColorInputVariant.Text).Invalid("Invalid color")

            | Text.Monospaced("Picker Only")
            | pickerState.ToColorInput().Variant(ColorInputVariant.Picker)
            | pickerState.ToColorInput().Variant(ColorInputVariant.Picker).Invalid("Invalid color")
            | pickerState.ToColorInput().Variant(ColorInputVariant.Picker).Disabled()
            | nullPickerState.ToColorInput().Variant(ColorInputVariant.Picker)
            | nullPickerState.ToColorInput().Variant(ColorInputVariant.Picker).Invalid("Invalid color")

            | Text.Monospaced("Text and Picker")
            | bothState.ToColorInput().Variant(ColorInputVariant.TextAndPicker)
            | bothState.ToColorInput().Variant(ColorInputVariant.TextAndPicker).Invalid("Invalid color")
            | bothState.ToColorInput().Variant(ColorInputVariant.TextAndPicker).Disabled()
            | nullBothState.ToColorInput().Variant(ColorInputVariant.TextAndPicker)
            | nullBothState.ToColorInput().Variant(ColorInputVariant.TextAndPicker).Invalid("Invalid color")

            | Text.Monospaced("Swatch")
            | swatchState.ToColorInput().Variant(ColorInputVariant.Swatch)
            | swatchState.ToColorInput().Variant(ColorInputVariant.Swatch).Invalid("Invalid color")
            | swatchState.ToColorInput().Variant(ColorInputVariant.Swatch).Disabled()
            | nullSwatchState.ToColorInput().Variant(ColorInputVariant.Swatch)
            | nullSwatchState.ToColorInput().Variant(ColorInputVariant.Swatch).Invalid("Invalid color")
            | Text.Monospaced("Ghost")
            | ghostState.ToColorInput().Variant(ColorInputVariant.TextAndPicker).Ghost()
            | ghostState.ToColorInput().Variant(ColorInputVariant.TextAndPicker).Ghost().Invalid("Invalid color")
            | ghostState.ToColorInput().Variant(ColorInputVariant.TextAndPicker).Ghost().Disabled()
            | nullBothState.ToColorInput().Variant(ColorInputVariant.TextAndPicker).Ghost()
            | nullBothState.ToColorInput().Variant(ColorInputVariant.TextAndPicker).Ghost().Invalid("Invalid color");
    }
}

public class ColorInputConstructorTests : ViewBase
{
    public override object Build()
    {
        var defaultConstructorState = UseState("#dd5860");
        var placeholderState = UseState<string?>(() => null);
        var stateBindingState = UseState("#ff0000");
        var disabledState = UseState<string?>(() => null);
        var textOnlyState = UseState<string?>(() => null);
        var pickerOnlyState = UseState("#000000");
        var fullConstructorState = UseState("#000000");

        return Layout.Grid().Columns(3)
               | Text.Monospaced("Method")
               | Text.Monospaced("ColorInput")
               | Text.Monospaced("State Value")

               | Text.Monospaced("Default Constructor")
               | defaultConstructorState.ToColorInput()
               | Text.Monospaced(defaultConstructorState.Value ?? "No state")

               | Text.Monospaced("With Placeholder")
               | placeholderState.ToColorInput().Placeholder("Select a color")
               | Text.Monospaced(placeholderState.Value ?? "No state")

               | Text.Monospaced("With State Binding")
               | stateBindingState.ToColorInput()
               | Text.Monospaced(stateBindingState.Value ?? "No state")

               | Text.Monospaced("Disabled")
               | disabledState.ToColorInput().Disabled()
               | Text.Monospaced(disabledState.Value ?? "No state")

               | Text.Monospaced("Text Only Variant")
               | textOnlyState.ToColorInput().Variant(ColorInputVariant.Text)
               | Text.Monospaced(textOnlyState.Value ?? "No state")

               | Text.Monospaced("Picker Only Variant")
               | pickerOnlyState.ToColorInput().Variant(ColorInputVariant.Picker)
               | Text.Monospaced(pickerOnlyState.Value ?? "No state")

               | Text.Monospaced("Full Constructor")
               | fullConstructorState.ToColorInput().Placeholder("Choose your color").Variant(ColorInputVariant.TextAndPicker)
               | Text.Monospaced(fullConstructorState.Value ?? "No state");
    }
}

public class ColorInputFormatTests : ViewBase
{
    public override object Build()
    {
        var hexState = UseState("#ff0000");
        var rgbState = UseState("rgb(255, 0, 0)");
        var enumState = UseState(Colors.Red);

        return Layout.Grid().Columns(4)
               | Text.Monospaced("Format")
               | Text.Monospaced("Input")
               | Text.Monospaced("Display Value")
               | Text.Monospaced("Stored Value")

               | Text.Monospaced("Hex")
               | hexState.ToColorInput()
               | Text.Monospaced(hexState.Value)
               | Text.Monospaced(hexState.Value)

               | Text.Monospaced("RGB")
               | rgbState.ToColorInput()
               | Text.Monospaced(rgbState.Value)
               | Text.Monospaced(ConvertToHex(rgbState.Value))

               | Text.Monospaced("Enum")
               | enumState.ToColorInput()
               | Text.Monospaced(enumState.Value.ToString())
               | Text.Monospaced(ConvertToHex(enumState.Value.ToString()))
            ;
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

public class ColorInputDataBindings : ViewBase
{
    public override object Build()
    {
        var stringState = UseState("#ff0000");
        var stringNullState = UseState((string?)null);
        var colorsState = UseState(Colors.Red);
        var colorsNullState = UseState((Colors?)null);

        var colorTypes = new (string TypeName, object NonNullableState, object NullableState)[]
        {
            ("string", stringState, stringNullState),
            ("Colors", colorsState, colorsNullState)
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

        foreach (var (typeName, nonNullableState, nullableState) in colorTypes)
        {
            // Non-nullable columns (first 3)
            gridItems.Add(Text.Monospaced(typeName));
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
            gridItems.Add(Text.Monospaced($"{typeName}?"));
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

    private static object CreateColorInputVariants(object state) =>
        InputDataBindingHelper.CreateInputVariants(state,
            anyState => Layout.Vertical()
                | anyState.ToColorInput()
                | anyState.ToColorInput().Placeholder("Select color")
                | anyState.ToColorInput().Disabled(),
            anyState => anyState.ToColorInput());

    private static object FormatStateValue(string typeName, object? value, bool isNullable)
    {
        return value switch
        {
            null => isNullable ? Text.Monospaced("Null") : Text.Monospaced("Default"),
            string s => Text.Monospaced(s),
            Colors c => Text.Monospaced(c.ToString()),
            _ => Text.Monospaced(value?.ToString() ?? "null")
        };
    }
}

public class ColorInputAlphaTests : ViewBase
{
    public override object Build()
    {
        var textAlphaState = UseState("#ff000080");
        var pickerAlphaState = UseState("#00ff00cc");
        var bothAlphaState = UseState("#0000ffaa");

        return Layout.Grid().Columns(3)
            | Text.Monospaced("Variant")
            | Text.Monospaced("ColorInput")
            | Text.Monospaced("State Value")

            | Text.Monospaced("Text + Alpha")
            | textAlphaState.ToColorInput().Variant(ColorInputVariant.Text).AllowAlpha()
            | Text.Monospaced(textAlphaState.Value ?? "null")

            | Text.Monospaced("Picker + Alpha")
            | pickerAlphaState.ToColorInput().Variant(ColorInputVariant.Picker).AllowAlpha()
            | Text.Monospaced(pickerAlphaState.Value ?? "null")

            | Text.Monospaced("TextAndPicker + Alpha")
            | bothAlphaState.ToColorInput().Variant(ColorInputVariant.TextAndPicker).AllowAlpha()
            | Text.Monospaced(bothAlphaState.Value ?? "null");
    }
}

public class ColorInputVariants : ViewBase
{
    public override object Build()
    {
        return Layout.Vertical()
               | Text.H3("Variants")
               | new ColorInputVariantTests()
               | Text.H3("Non-Generic Constructor")
               | new ColorInputConstructorTests();
    }
}

public class ColorInputEvents : ViewBase
{
    public override object? Build()
    {
        var blurCount = UseState(0);
        var focusCount = UseState(0);
        var state = UseState("#6366f1");

        return Layout.Vertical().Gap(6)
            | Layout.Tabs(
                new Tab("OnFocus", Layout.Vertical().Gap(4)
                    | Text.P("The **OnFocus** event fires when the ColorInput gains focus (via mouse or keyboard tab).")
                    | new Card(
                        Layout.Vertical().Center().Gap(6)
                            | ((IAnyInput)state.ToColorInput()).OnFocus(_ => focusCount.Set(focusCount.Value + 1))
                            | Layout.Vertical().Center().Gap(1)
                                | Text.H2(focusCount.Value.ToString()).Color(Colors.Primary)
                                | Text.Muted("Focus Count")
                      ).Width(Size.Units(120))
                ),
                new Tab("OnBlur", Layout.Vertical().Gap(4)
                    | Text.P("The **OnBlur** event fires when the ColorInput loses focus.")
                    | new Card(
                        Layout.Vertical().Center().Gap(6)
                            | ((IAnyInput)state.ToColorInput()).OnBlur(_ => blurCount.Set(blurCount.Value + 1))
                            | Layout.Vertical().Center().Gap(1)
                                | Text.H2(blurCount.Value.ToString()).Color(Colors.Orange)
                                | Text.Muted("Blur Count")
                      ).Width(Size.Units(120))
                ),
                new Tab("AutoFocus", Layout.Vertical().Gap(4)
                    | Text.P("The **AutoFocus** property should automatically focus the ColorInput when it is mounted.")
                    | new Card(
                        Layout.Vertical().Center().Gap(6)
                            | ((IAnyInput)state.ToColorInput()).AutoFocus()
                            | Text.Lead("Focused!").Color(Colors.Green)
                            | Text.P("The field above should have a focus ring immediately.").Small().Muted()
                      ).Width(Size.Units(120))
                )
            ).Variant(TabsVariant.Tabs);
    }
}

public class ColorInputAffixesExample : ViewBase
{
    public override object? Build()
    {
        var colorState = UseState("#6366f1");

        return Layout.Grid().Columns(4)
               | null!
               | Text.Monospaced("Prefix only")
               | Text.Monospaced("Suffix only")
               | Text.Monospaced("Both")

               | Text.Monospaced("Text prefix/suffix")
               | colorState.ToColorInput().Prefix("$")
               | colorState.ToColorInput().Suffix("hex")
               | colorState.ToColorInput().Prefix("$").Suffix("hex")

               | Text.Monospaced("Icon prefix/suffix")
               | colorState.ToColorInput().Prefix(Icons.Palette)
               | colorState.ToColorInput().Suffix(Icons.Palette)
               | colorState.ToColorInput().Prefix(Icons.Palette).Suffix(Icons.Palette)

               | Text.Monospaced("Button prefix/suffix")
               | colorState.ToColorInput().Prefix(new Button("Copy", () => { }, icon: Icons.Copy).Ghost().Small())
               | colorState.ToColorInput().Suffix(new Button("Go").Ghost().Small())
               | colorState.ToColorInput().Prefix(new Button("Copy", () => { }, icon: Icons.Copy).Ghost().Small()).Suffix(new Button("Go").Ghost().Small());
    }
}

