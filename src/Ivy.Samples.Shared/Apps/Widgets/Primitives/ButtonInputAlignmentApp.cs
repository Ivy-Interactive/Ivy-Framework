namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(
    icon: Icons.AlignVerticalSpaceAround,
    group: ["Widgets", "Primitives"],
    searchHints: ["button", "input", "height", "density", "alignment", "horizontal"]
)]
public class ButtonInputAlignmentApp : SampleBase
{
    protected override object? BuildSample()
    {
        var repoUrl = UseState("");
        var projectName = UseState("");
        var selectValue = UseState("Option A");
        var numberValue = UseState(42);

        var options = new[] { "Option A", "Option B", "Option C" };

        return Layout.Vertical()
               | Text.H1("Button & Input Alignment")
               | Text.P(
                   "Buttons beside inputs in horizontal layouts should share the same height at each density. "
                   + "Compare rows below — mismatched heights are easiest to spot when controls sit side by side."
               )
               | Text.H2("Repository row (Tendril-style)")
               | Text.Muted("Add one or more Git repositories")
               | BuildDensityRow(Density.Small, "Small",
                   repoUrl.ToTextInput().Placeholder("Repository URL or Local Path").Density(Density.Small),
                   new Button("+ Add Repository", variant: ButtonVariant.Primary).Density(Density.Small))
               | BuildDensityRow(Density.Medium, "Medium",
                   repoUrl.ToTextInput().Placeholder("Repository URL or Local Path"),
                   new Button("+ Add Repository", variant: ButtonVariant.Primary))
               | BuildDensityRow(Density.Large, "Large",
                   repoUrl.ToTextInput().Placeholder("Repository URL or Local Path").Density(Density.Large),
                   new Button("+ Add Repository", variant: ButtonVariant.Primary).Density(Density.Large))
               | Text.H2("Project name (input only)")
               | projectName.ToTextInput().Placeholder("Project Name")
               | Text.H2("Text input + button variants")
               | BuildDensityRow(Density.Small, "Small",
                   repoUrl.ToTextInput().Placeholder("Text").Density(Density.Small),
                   new Button("Primary", variant: ButtonVariant.Primary).Density(Density.Small),
                   new Button("Outline", variant: ButtonVariant.Outline).Density(Density.Small),
                   new Button("Secondary", variant: ButtonVariant.Secondary).Density(Density.Small))
               | BuildDensityRow(Density.Medium, "Medium",
                   repoUrl.ToTextInput().Placeholder("Text"),
                   new Button("Primary", variant: ButtonVariant.Primary),
                   new Button("Outline", variant: ButtonVariant.Outline),
                   new Button("Secondary", variant: ButtonVariant.Secondary))
               | BuildDensityRow(Density.Large, "Large",
                   repoUrl.ToTextInput().Placeholder("Text").Density(Density.Large),
                   new Button("Primary", variant: ButtonVariant.Primary).Density(Density.Large),
                   new Button("Outline", variant: ButtonVariant.Outline).Density(Density.Large),
                   new Button("Secondary", variant: ButtonVariant.Secondary).Density(Density.Large))
               | Text.H2("Select + button")
               | BuildDensityRow(Density.Small, "Small",
                   selectValue.ToSelectInput(options).Placeholder("Choose…").Density(Density.Small),
                   new Button("Action", variant: ButtonVariant.Primary).Density(Density.Small))
               | BuildDensityRow(Density.Medium, "Medium",
                   selectValue.ToSelectInput(options).Placeholder("Choose…"),
                   new Button("Action", variant: ButtonVariant.Primary))
               | BuildDensityRow(Density.Large, "Large",
                   selectValue.ToSelectInput(options).Placeholder("Choose…").Density(Density.Large),
                   new Button("Action", variant: ButtonVariant.Primary).Density(Density.Large))
               | Text.H2("Number input + button")
               | BuildDensityRow(Density.Small, "Small",
                   numberValue.ToNumberInput().Density(Density.Small),
                   new Button("Apply", variant: ButtonVariant.Outline).Density(Density.Small))
               | BuildDensityRow(Density.Medium, "Medium",
                   numberValue.ToNumberInput(),
                   new Button("Apply", variant: ButtonVariant.Outline))
               | BuildDensityRow(Density.Large, "Large",
                   numberValue.ToNumberInput().Density(Density.Large),
                   new Button("Apply", variant: ButtonVariant.Outline).Density(Density.Large));
    }

    private static object BuildDensityRow(Density density, string label, params object[] controls)
    {
        return Layout.Vertical().Gap(1)
               | Text.Block($"{label} ({density})").Bold()
               | (Layout.Horizontal().Gap(2) | controls);
    }
}
