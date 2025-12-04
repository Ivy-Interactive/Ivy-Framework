using Ivy.Samples.Shared.Apps.Concepts.Forms.Variants;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Concepts.Forms;

/// <summary>
/// Forms demonstration app showing form inputs, scaffolding, and validation examples.
/// </summary>
[App(icon: Icons.Clipboard, path: ["Concepts"], searchHints: ["forms", "inputs", "fields", "validation", "submission", "data-entry", "controls", "scaffolding", "dataannotations"])]
public class FormsApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("Form Inputs", new FormInputsExample()),
            new Tab("Scaffolding", new FormScaffoldingExample()),
            new Tab("Validation", new FormValidationExample())
        ).Variant(TabsVariant.Content);
    }
}
