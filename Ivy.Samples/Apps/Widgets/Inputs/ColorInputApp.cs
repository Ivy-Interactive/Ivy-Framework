using Ivy.Shared;
using Ivy; // For ColorInputs enum

namespace Ivy.Samples.Apps.Widgets.Inputs;

[App(icon: Icons.PaintBucket, path: ["Widgets", "Inputs"])]
public class ColorInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        var pickerState = UseState("#ff0000");
        var textState = UseState("rgb(0,255,0)");
        var pickerTextState = UseState("blue");
        var hexState = UseState("#00ff00");
        var colorNameState = UseState("red");
        var rgbState = UseState("rgb(255,0,255)");
        var paletteState = UseState<string?>(() => null);
        var disabledState = UseState("#cccccc");
        var invalidState = UseState("notacolor");

        // Reuse the logic from ColorsApp to dynamically generate palette options
        var paletteOptions = Enum.GetValues<Colors>()
            .Select(color => color.ToString().ToLower())
            .ToArray();

        var variants = Layout.Vertical(
            Text.H1("Color Inputs"),
            Text.H2("Supported Color Formats"),
            Layout.Vertical(
                Text.H3("Hex Colors"),
                hexState
                    .ToColorInput()
                    .Description("Enter hex colors like #ff0000, #00ff00")
                    .Variant(ColorInputs.Text)
                    .Label("Hex Color")
            ),
            Layout.Vertical(
                Text.H3("RGB Colors"),
                rgbState
                    .ToColorInput()
                    .Description("Enter RGB colors like rgb(255,0,0), rgb(0,255,0)")
                    .Variant(ColorInputs.Text)
                    .Label("RGB Color")
            ),
            Layout.Vertical(
                Text.H3("Color Names"),
                colorNameState
                    .ToColorInput()
                    .Description("Enter color names like red, blue, green, primary")
                    .Variant(ColorInputs.Text)
                    .Label("Color Name")
            ),
            Text.H2("Input Variants"),
            Layout.Grid().Columns(2)
                | Layout.Vertical(
                    Text.H3("Picker"),
                    pickerState
                        .ToColorInput()
                        .Description("Native color picker")
                        .Variant(ColorInputs.Picker)
                )
                | Layout.Vertical(
                    Text.H3("Text"),
                    textState
                        .ToColorInput()
                        .Description("Type any color format")
                        .Variant(ColorInputs.Text)
                )
                | Layout.Vertical(
                    Text.H3("Picker+Text"),
                    pickerTextState
                        .ToColorInput()
                        .Description("Both picker and text")
                        .Variant(ColorInputs.PickerText)
                )
                | Layout.Vertical(
                    Text.H3("Palette"),
                    paletteState
                        .ToColorInput()
                        .Description("Choose from all available Ivy colors")
                        .Variant(ColorInputs.Palette)
                        .PaletteOptions(paletteOptions)
                )
                | Layout.Vertical(
                    Text.H3("Disabled"),
                    disabledState
                        .ToColorInput()
                        .Description("Disabled input")
                        .Variant(ColorInputs.Picker)
                        .Disabled(true)
                )
                | Layout.Vertical(
                    Text.H3("Invalid"),
                    invalidState
                        .ToColorInput()
                        .Description("Shows error if invalid")
                        .Variant(ColorInputs.Text)
                        .Invalid("Invalid color value")
                )
        );

        var dataBinding = Layout.Vertical(
            Text.H2("Data Binding"),
            Layout.Grid().Columns(2)
                | Text.Block("Hex Color Value") | hexState
                | Text.Block("RGB Color Value") | rgbState
                | Text.Block("Color Name Value") | colorNameState
                | Text.Block("Picker Value") | pickerState
                | Text.Block("Text Value") | textState
                | Text.Block("Picker+Text Value") | pickerTextState
                | Text.Block("Palette Value") | paletteState
        );

        return Layout.Vertical(
            variants,
            dataBinding
        );
    }
}