namespace Ivy.Samples.Shared.Apps.Tests;

[App(icon: Icons.Airplay, path: ["Tests"], isVisible: false, searchHints: ["typography", "text", "markdown", "comparison"])]
public class TypographyComparisonApp : SampleBase
{
    protected override object? BuildSample()
    {
        var markdown = """
# Heading 1
## Heading 2
### Heading 3
#### Heading 4
##### Heading 5
###### Heading 6

This is a paragraph of text to compare the typography. It should have comfortable line height and spacing.

This is a paragraph of text to compare the typography. It should have comfortable line height and spacing.

> This is a blockquote. It should stand out from the rest of the text.

**Bold Text** and *Italic Text* and `Inline Code`.

---

End of the typography test.
""";

        var html = """
<h1>Heading 1</h1>
<h2>Heading 2</h2>
<h3>Heading 3</h3>
<h4>Heading 4</h4>
<h5>Heading 5</h5>
<h6>Heading 6</h6>

<p>This is a paragraph of text to compare the typography. It should have comfortable line height and spacing.</p>
<p>This is a paragraph of text to compare the typography. It should have comfortable line height and spacing.</p>

<blockquote>This is a blockquote. It should stand out from the rest of the text.</blockquote>

<p><strong>Bold Text</strong> and <em>Italic Text</em> and <code>Inline Code</code>.</p>

<hr />

<p>End of the typography test.</p>
""";

        return Layout.Grid().Columns(3).Gap(20)
            | new Card(new Markdown(markdown)).Title("Markdown")
            | new Card(new Html(html)).Title("HTML")
            | new Card(
                Layout.Vertical()
                | Text.H1("Heading 1")
                | Text.H2("Heading 2")
                | Text.H3("Heading 3")
                | Text.H4("Heading 4")
                | Text.H5("Heading 5")
                | Text.H6("Heading 6")
                | Text.P("This is a paragraph of text to compare the typography. It should have comfortable line height and spacing.")
                | Text.P("This is a paragraph of text to compare the typography. It should have comfortable line height and spacing.")
                | Text.Blockquote("This is a blockquote. It should stand out from the rest of the text.")
                | (Layout.Horizontal().Gap(4)
                    | Text.Bold("Bold Text")
                    | Text.P("and")
                    | Text.Inline("Italic Text").Italic()
                    | Text.P("and")
                    | Text.Monospaced("Inline Code").Color(Colors.Red)
                  )
                 | new Separator()
                 | Text.P("End of the typography test.")
              ).Title("Text Widgets Rendering");
    }
}
