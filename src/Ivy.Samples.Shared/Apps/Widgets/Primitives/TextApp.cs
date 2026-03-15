
namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Type, path: ["Widgets", "Primitives"], searchHints: ["typography", "heading", "paragraph", "label", "text", "content"])]
public class TextApp : SampleBase
{
    protected override object? BuildSample()
    {
        var headingsAndBasics = Layout.Vertical(
            Text.Monospaced("Headings").Bold().Large(),
            Text.H1("H1().Bold()").Bold(),
            Text.H2("H2().Italic()").Italic(),
            Text.H3("H3().Muted()").Muted(),
            Text.H4("H4().Bold().Italic()").Bold().Italic(),
            Text.H4("Basic Text").Bold(),
            Text.P("P().Italic()").Italic(),
            Text.Inline("Inline().Muted()").Muted(),
            Text.Block("Block().Bold().Italic()").Bold().Italic(),
            Text.H4("Label Modifiers").Bold(),
            Text.Label("Label().Bold()").Bold(),
            Text.Label("Label().Italic()").Italic(),
            Text.Label("Label().Muted()").Muted()
        );

        var styles = Layout.Vertical(
            Text.Monospaced("Size Variants").Bold().Large(),
            Text.Lead("Lead().Bold()").Bold(),
            Text.P("P().Large().Bold()").Large().Bold(),
            Text.P("P().Small().Muted()").Small().Muted(),
            Text.H4("Emphasis").Bold(),
            Text.Strong("Strong().Italic()").Italic(),
            Text.Bold("Bold().Italic()").Italic(),
            Text.Muted("Muted().Italic()").Italic(),
            Text.P("Text with Color(Colors.Muted)").Color(Colors.Muted),
            Text.H4("Quotes & Code").Bold(),
            Text.Blockquote("Blockquote().Muted()").Muted(),
            Text.Monospaced("Monospaced().Bold()").Bold(),
            Text.H4("Semantic Styles").Bold(),
            Text.Danger("Danger().Bold()").Bold(),
            Text.Warning("Warning().Italic()").Italic(),
            Text.Success("Success().Bold()").Bold()
        );

        var alignment = Layout.Vertical(
            Text.Monospaced("Left (default)").Bold().Large(),
            Text.P("This paragraph is left-aligned. It is the default alignment for most text blocks and works well for body copy.").Left(),
            Text.Monospaced("Center").Bold().Large(),
            Text.P("This paragraph is centered. Useful for short lines, titles, or callouts.").Center(),
            Text.Monospaced("Right").Bold().Large(),
            Text.P("This paragraph is right-aligned. Often used for numbers or dates in narrow columns.").Right(),
            Text.Monospaced("Justify").Bold().Large(),
            Text.P("This paragraph is justified. Text is aligned to both the left and the right edge by adjusting the spacing between words. Justification works best when the paragraph spans several lines, so you can see how each line stretches to fill the full width. It is commonly used in newspapers, magazines, and multi-column layouts where a clean, block-like appearance is desired. The browser distributes extra space between words to make the right edge line up neatly.").Justify()
        );

        return Layout.Tabs(
            new Tab("Headings & basics", headingsAndBasics).Icon(Icons.Type),
            new Tab("Styles", styles).Icon(Icons.Palette),
            new Tab("Alignment", alignment).Icon(Icons.TextAlignCenter)
        );
    }
}
