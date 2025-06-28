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
            Text.H2("Variants"),
            Layout.Grid().Columns(2)
                | Text.Block("Picker") | pickerState.ToColorInput().Label("Picker").Description("Native color picker").Variant(ColorInputs.Picker)
                | Text.Block("Text") | textState.ToColorInput().Label("Text").Description("Type any color").Variant(ColorInputs.Text)
                | Text.Block("Picker+Text") | pickerTextState.ToColorInput().Label("Picker+Text").Description("Both picker and text").Variant(ColorInputs.PickerText)
                | Text.Block("Palette") | paletteState.ToColorInput().Label("Palette").Description("Choose from swatches").Variant(ColorInputs.Palette).PaletteOptions(paletteOptions)
                | Text.Block("Disabled") | disabledState.ToColorInput().Label("Disabled").Description("Disabled input").Variant(ColorInputs.Picker).Disabled(true)
                | Text.Block("Invalid") | invalidState.ToColorInput().Label("Invalid").Description("Shows error if invalid").Variant(ColorInputs.Text).Invalid("Invalid color value")
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