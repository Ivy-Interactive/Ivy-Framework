using Ivy.Shared;
using Ivy.Samples.Apps;

namespace Ivy.Samples.Apps.Tests;

[App(icon: Icons.Columns3, path: ["Tests"], isVisible: true)]
public class MarkdownHtmlTestApp : SampleBase
{
    protected override object? BuildSample()
    {
        var markdown = """
# H1 Heading
## H2 Heading
### H3 Heading
#### H4 Heading

This is a paragraph with **bold** and *italic* text, and a [link](https://example.com).

![Sample Image](https://placekitten.com/200/100)

* Unordered list item 1
* Unordered list item 2
  * Nested item

1. Ordered list item 1
2. Ordered list item 2

> Blockquote example

---

```csharp
// Code block example
Console.WriteLine("Hello, World!");
```

| Column 1 | Column 2 |
|----------|----------|
| Row 1    | Data 1   |
| Row 2    | Data 2   |

---

**Headings with paragraphs between**

# H1 Heading
This is a paragraph between H1 and H2. It contains about forty words to demonstrate spacing and formatting. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Vestibulum.

## H2 Heading
This is a paragraph between H2 and H3. It also contains approximately forty words for testing. Quisque euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim, nec facilisis massa enim nec dui. Etiam euismod, sapien.

### H3 Heading
This is a paragraph between H3 and H4. The purpose is to ensure that longer paragraphs are rendered correctly between headings. Sed cursus, enim at dictum feugiat, sapien erat cursus enim, nec facilisis massa enim nec dui.

#### H4 Heading
""";

        var html = @"<h1>H1 Heading</h1>
<h2>H2 Heading</h2>
<h3>H3 Heading</h3>
<h4>H4 Heading</h4>
<p>This is a paragraph with <strong>bold</strong> and <em>italic</em> text, and a <a href='https://example.com'>link</a>.</p>
<img src='https://placekitten.com/200/100' alt='Sample Image' />
<ul>
  <li>Unordered list item 1</li>
  <li>Unordered list item 2
    <ul>
      <li>Nested item</li>
    </ul>
  </li>
</ul>
<ol>
  <li>Ordered list item 1</li>
  <li>Ordered list item 2</li>
</ol>
<blockquote>Blockquote example</blockquote>
<hr />
<pre><code class='language-csharp'>// Code block example
Console.WriteLine(&quot;Hello, World!&quot;);
</code></pre>
<table>
  <tr><th>Column 1</th><th>Column 2</th></tr>
  <tr><td>Row 1</td><td>Data 1</td></tr>
  <tr><td>Row 2</td><td>Data 2</td></tr>
</table>
<hr />
<p><strong>Headings with paragraphs between</strong></p>
<h1>H1 Heading</h1>
<p>This is a paragraph between H1 and H2. It contains about forty words to demonstrate spacing and formatting. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Vestibulum.</p>
<h2>H2 Heading</h2>
<p>This is a paragraph between H2 and H3. It also contains approximately forty words for testing. Quisque euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim, nec facilisis massa enim nec dui. Etiam euismod, sapien.</p>
<h3>H3 Heading</h3>
<p>This is a paragraph between H3 and H4. The purpose is to ensure that longer paragraphs are rendered correctly between headings. Sed cursus, enim at dictum feugiat, sapien erat cursus enim, nec facilisis massa enim nec dui.</p>
<h4>H4 Heading</h4>
";

        return Layout.Grid().Columns(2)
            | new Card(new Markdown(markdown)).Title("Markdown")
            | new Card(new Html(html)).Title("HTML");
    }
}