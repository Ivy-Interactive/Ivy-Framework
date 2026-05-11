namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

/// <summary>
/// Side-by-side TextInput and SelectInput at Small / Medium / Large densities to compare pixel heights.
/// </summary>
[App(
    title: "Text vs Select Height",
    icon: Icons.Ruler,
    group: ["Widgets", "Inputs", "Test"],
    searchHints: ["height", "alignment", "density", "select", "text", "input", "grid"])]
public class TextSelectInputHeightApp : SampleBase
{
    public TextSelectInputHeightApp() : base(showCodePosition: null)
    {
    }

    protected override object? BuildSample()
    {
        var text = UseState("Sample text");
        var choice = UseState("Beta");
        var options = new[] { "Alpha", "Beta", "Gamma" };

        var comparisonGrid = Layout.Grid().Columns(3).Gap(4)
            | Text.Monospaced("Density")
            | Text.Monospaced("TextInput")
            | Text.Monospaced("SelectInput")

            | Text.Monospaced("Small")
            | text.ToTextInput().Placeholder("Text…").Small()
            | choice.ToSelectInput(options).Placeholder("Select…").Small()

            | Text.Monospaced("Medium")
            | text.ToTextInput().Placeholder("Text…")
            | choice.ToSelectInput(options).Placeholder("Select…")

            | Text.Monospaced("Large")
            | text.ToTextInput().Placeholder("Text…").Large()
            | choice.ToSelectInput(options).Placeholder("Select…").Large();

        return Layout.Vertical().Gap(6).Padding(2)
            | Text.H1("TextInput vs SelectInput height")
            | Text.P(
                "Each row uses the same density on both controls. Inspect visually or with devtools to compare outer heights.")
                .Muted()
            | comparisonGrid;
    }
}
