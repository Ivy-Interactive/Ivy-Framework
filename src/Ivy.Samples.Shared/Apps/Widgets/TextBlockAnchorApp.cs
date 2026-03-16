namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.Link, path: ["Widgets"], searchHints: ["anchor", "heading", "hash", "navigation", "deep-link"])]
public class TextBlockAnchorApp : SampleBase
{
    protected override object? BuildSample() => Layout.Vertical(
        Text.H1("Anchor Demo").Anchor("anchor-demo"),
        Text.P("This page demonstrates anchor support on headings."),
        Text.H2("Explicit Anchor").Anchor("explicit-anchor"),
        Text.P("This heading has an explicit anchor: #explicit-anchor"),
        Text.H2("Auto-Generated Anchor").Anchor(),
        Text.P("This heading auto-generates its anchor from the text: #auto-generated-anchor"),
        Text.H3("Section With Special Characters!").Anchor(),
        Text.P("Special characters are stripped during slugification: #section-with-special-characters")
    ).Gap(10).Padding(20);
}
