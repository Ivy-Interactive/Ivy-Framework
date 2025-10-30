using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Type, path: ["Widgets", "Primitives"], searchHints: ["typography", "heading", "paragraph", "label", "text", "content"])]
public class TextApp : SampleBase
{
    protected override object? BuildSample()
    {
        var headings = Layout.Vertical(
            Text.H4("Headings").Bold(),
            Text.H1("H1().Bold()").Bold(),
            Text.H2("H2().Italic()").Italic(),
            Text.H3("H3().Muted()").Muted(),
            Text.H4("H4().Bold().Italic()").Bold().Italic()
        );

        var basic = Layout.Vertical(
            Text.H4("Basic Text").Bold(),
            Text.Literal("Literal().Bold()").Bold(),
            Text.P("P().Italic()").Italic(),
            Text.Inline("Inline().Muted()").Muted(),
            Text.Block("Block().Bold().Italic()").Bold().Italic()
        );

        var labels = Layout.Vertical(
            Text.H4("Label Modifiers").Bold(),
            Text.Label("Label().Bold()").Bold(),
            Text.Label("Label().Italic()").Italic(),
            Text.Label("Label().Muted()").Muted(),
            Text.Label("Label().Bold().Italic()").Bold().Italic(),
            Text.Label("Label().Bold().Muted()").Bold().Muted(),
            Text.Label("Label().Italic().Muted()").Italic().Muted()
        );

        var sizes = Layout.Vertical(
            Text.H4("Size Variants").Bold(),
            Text.Lead("Lead().Bold()").Bold(),
            Text.Lead("Lead().Italic()").Italic(),
            Text.Large("Large().Bold()").Bold(),
            Text.Large("Large().Italic()").Italic(),
            Text.Small("Small().Bold()").Bold(),
            Text.Small("Small().Italic()").Italic(),
            Text.Small("Small().Muted()").Muted()
        );

        var emphasis = Layout.Vertical(
            Text.H4("Emphasis").Bold(),
            Text.Strong("Strong().Italic()").Italic(),
            Text.Strong("Strong().Muted()").Muted(),
            Text.Bold("Bold().Italic()").Italic(),
            Text.Muted("Muted().Bold()").Bold(),
            Text.Muted("Muted().Italic()").Italic()
        );

        var quotes = Layout.Vertical(
            Text.H4("Quotes & Code").Bold(),
            Text.Blockquote("Blockquote().Bold()").Bold(),
            Text.Blockquote("Blockquote().Italic()").Italic(),
            Text.Blockquote("Blockquote().Muted()").Muted(),
            Text.InlineCode("InlineCode().Bold()").Bold(),
            Text.InlineCode("InlineCode().Italic()").Italic()
        );

        var semantic = Layout.Vertical(
            Text.H4("Semantic Styles").Bold(),
            Text.Danger("Danger().Bold()").Bold(),
            Text.Danger("Danger().Italic()").Italic(),
            Text.Warning("Warning().Bold()").Bold(),
            Text.Warning("Warning().Italic()").Italic(),
            Text.Success("Success().Bold()").Bold(),
            Text.Success("Success().Italic()").Italic()
        );

        var leftColumn = Layout.Vertical(
            headings,
            basic,
            labels
        );

        var rightColumn = Layout.Vertical(
            sizes,
            emphasis,
            quotes,
            semantic
        );

        return Layout.Horizontal(
            leftColumn,
            rightColumn
        );
    }
}