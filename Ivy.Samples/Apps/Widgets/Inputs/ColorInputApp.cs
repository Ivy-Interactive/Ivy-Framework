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
        var paletteState = UseState<string?>(() => null);
        var disabledState = UseState("#cccccc");
        var invalidState = UseState("notacolor");

        var paletteOptions = new[] { "#ff0000", "#00ff00", "#0000ff", "orange", "purple" };

        var variants = Layout.Vertical(
            Text.H1("Color Inputs"),
            Text.H2("Variants"),
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
                        .Description("Type any color")
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
                        .Description("Choose from swatches")
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