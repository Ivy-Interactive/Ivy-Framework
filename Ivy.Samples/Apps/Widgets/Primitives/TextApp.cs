using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Primitives;

[App(icon: Icons.Text, path: ["Widgets", "Primitives"])]
public class TextApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical(
            Text.Literal("Literal"),
            Text.H1("H1"),
            Text.H2("H2"),
            Text.H3("H3"),
            Text.H4("H4"),
            Text.Block("Block"),
            Text.Blockquote("Blockquote"),
            Text.InlineCode("InlineCode"),
            Text.Lead("Lead"),
            Text.Large("Large"),
            Text.Small("Small"),
            Text.Muted("Muted"),
            Text.Danger("Danger"),
            Text.Warning("Warning"),
            Text.Success("Success"),
            Text.Code("Code"),
            Text.Markdown("# Markdown"),
            Text.Json("{ \"Json\": \"Json\" }"),
            Text.Xml("<xml>Xml</xml>"),
            Text.Html("<div>Html</div>")
        );
    }
}