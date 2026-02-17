using Ivy.Views;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.FileText, searchHints: ["formatting", "markup", "markdown", "md", "text", "content"])]
public class MarkdownApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("Text", new TextFormattingTab()),
            new Tab("Lists", new ListsTab()),
            new Tab("Links & Quotes", new LinksAndQuotesTab()),
            new Tab("Code", new CodeBlocksTab()),
            new Tab("Tables", new TablesTab()),
            new Tab("Math", new MathTab()),
            new Tab("Media", new MediaTab())
        ).Variant(TabsVariant.Content);
    }
}

public class TextFormattingTab : ViewBase
{
    public override object? Build()
    {
        var markdown = """
                       # Text Formatting
                       
                       The Markdown widget supports various text formatting options.
                       
                       ## Basic Formatting
                       
                       You can use **bold text**, *italic text*, and ***bold italic text***. 
                       
                       You can also use ~~strikethrough~~ text for deleted or deprecated content.
                       
                       ## Inline Code
                       
                       This is inline code: `const x = 10;` 
                       
                       Here's more inline code: `Console.WriteLine("Hello, World!");`
                       
                       You can use inline code for variable names, function calls, or any technical terms: `UseState`, `ViewBase`, `Layout.Vertical()`.
                       
                       ## Headings
                       
                       # Heading 1
                       ## Heading 2
                       ### Heading 3
                       #### Heading 4
                       ##### Heading 5
                       ###### Heading 6
                       
                       ## Horizontal Rules
                       
                       You can create visual separators:
                       
                       ---
                       
                       ## Paragraphs
                       
                       This is the first paragraph. It contains multiple sentences to demonstrate how paragraphs work in markdown.
                       
                       This is the second paragraph. Notice how there's a blank line between paragraphs.
                       
                       This is the third paragraph with some **bold** and *italic* text mixed in.
                       """;

        return new Markdown(markdown);
    }
}

public class ListsTab : ViewBase
{
    public override object? Build()
    {
        var markdown = """
                       # Lists
                       
                       Markdown supports various types of lists for organizing content.
                       
                       ## Unordered Lists
                       
                       - First item
                       - Second item
                       - Third item
                         - Nested item 1
                         - Nested item 2
                           - Deeply nested item
                       
                       You can also use asterisks or plus signs:
                       
                       * Item with asterisk
                       * Another item
                       + Item with plus sign
                       + Another item
                       
                       ## Ordered Lists
                       
                       1. First numbered item
                       2. Second numbered item
                       3. Third numbered item
                         1. Nested numbered item
                         2. Another nested item
                         3. Third nested item
                       
                       ## Task Lists
                       
                       - [x] Completed task
                       - [ ] Pending task
                       - [x] Another completed task
                       - [ ] Another pending task
                       - [x] Final completed task
                       
                       Task lists are great for tracking progress and to-do items.
                       """;

        return new Markdown(markdown);
    }
}

public class LinksAndQuotesTab : ViewBase
{
    public override object? Build()
    {
        var markdown = """
                       # Links and Blockquotes
                       
                       ## Links
                       
                       Here's a [link to Google](https://www.google.com) and another [link to Ivy Framework](https://docs.ivy.app).
                       
                       You can also use automatic links: https://github.com/Ivy-Interactive/Ivy-Framework
                       
                       Links can have titles: [Ivy Documentation](https://docs.ivy.app "Visit Ivy Docs")
                       
                       ## Blockquotes
                       
                       > This is a blockquote. It can contain multiple lines of text.
                       > 
                       > You can also include **formatted text** and [links](https://example.com) inside blockquotes.
                       
                       > Nested blockquotes are also supported:
                       > > This is a nested quote
                       > > With multiple lines
                       
                       > Blockquotes are perfect for:
                       > - Highlighting important information
                       > - Quoting external sources
                       > - Creating visual emphasis
                       
                       ## Combining Links and Quotes
                       
                       > "The best way to learn is by doing." - [Read more](https://example.com)
                       
                       > Check out the [Ivy Framework](https://github.com/Ivy-Interactive/Ivy-Framework) for more examples.
                       """;

        return new Markdown(markdown);
    }
}

public class CodeBlocksTab : ViewBase
{
    public override object? Build()
    {
        var markdown = """
                       # Code Blocks
                       
                       Code blocks support syntax highlighting for various programming languages.
                       
                       ## JavaScript Example
                       
                       ```javascript
                       const greeting = 'Hello, World!';
                       console.log(greeting);
                       
                       function add(a, b) {
                           return a + b;
                       }
                       
                       const result = add(5, 3);
                       console.log(result); // Output: 8
                       ```
                       
                       ## C# Example
                       
                       ```csharp
                       public class Example
                       {
                           public string Greeting { get; set; } = "Hello, World!";
                           
                           public void DisplayGreeting()
                           {
                               Console.WriteLine(Greeting);
                           }
                           
                           public int Add(int a, int b)
                           {
                               return a + b;
                           }
                       }
                       ```
                       
                       ## Python Example
                       
                       ```python
                       def greet(name):
                           return f"Hello, {name}!"
                       
                       def add(a, b):
                           return a + b
                       
                       print(greet("World"))
                       print(add(5, 3))
                       ```
                       
                       ## Diff Example
                       
                       ```diff
                       diff --git a/file.cs b/file.cs
                       index abc123..def456 100644
                       --- a/file.cs
                       +++ b/file.cs
                       @@ -1,3 +1,3 @@
                       -old code here
                       +new code here
                        unchanged line
                       ```
                       
                       ## Plain Text Code Block
                       
                       ```
                       This is a code block without syntax highlighting.
                       It's useful for displaying configuration files,
                       logs, or any plain text content.
                       ```
                       """;

        return new Markdown(markdown);
    }
}

public class TablesTab : ViewBase
{
    public override object? Build()
    {
        var markdown = """
                       # Tables
                       
                       Tables provide a structured way to display data in rows and columns.
                       
                       ## Basic Table
                       
                       | Feature        | Basic | Premium | Enterprise |
                       |----------------|-------|---------|------------|
                       | Users          | 1     | 10      | Unlimited  |
                       | Storage        | 1GB   | 100GB   | 1TB        |
                       | Support        | Email | Phone   | 24/7       |
                       | Price          | Free  | $29/mo  | Custom     |
                       
                       ## Monthly Budget
                       
                       | Category    | Budget | Actual | Difference |
                       |-------------|--------|--------|------------|
                       | Food        | $500   | $480   | -$20       |
                       | Transport   | $200   | $220   | +$20       |
                       | Entertainment| $150  | $180   | +$30       |
                       | Savings     | $300   | $300   | $0         |
                       """;

        return new Markdown(markdown);
    }
}

public class MathTab : ViewBase
{
    public override object? Build()
    {
        var markdown = """
                       # Mathematical Expressions
                       
                       Markdown supports both inline and block mathematical expressions using LaTeX syntax.
                       
                       ## Inline Math
                       
                       The famous equation: $E = mc^2$
                       
                       Another example: The quadratic formula is $x = \frac{-b \pm \sqrt{b^2 - 4ac}}{2a}$
                       
                       Pythagorean theorem: $a^2 + b^2 = c^2$
                       
                       Euler's identity: $e^{i\pi} + 1 = 0$
                       
                       ## Block Math
                       
                       The fundamental theorem of calculus:
                       
                       $$
                       \int_a^b f(x) dx = F(b) - F(a)
                       $$
                       
                       Cauchy-Schwarz inequality:
                       
                       $$
                       \left( \sum_{k=1}^n a_k b_k \right)^2 \leq \left( \sum_{k=1}^n a_k^2 \right) \left( \sum_{k=1}^n b_k^2 \right)
                       $$
                       
                       Fourier transform:
                       
                       $$
                       F(\\omega) = \int_{-\\infty}^{\\infty} f(t) e^{-i\\omega t} dt
                       $$
                       """;

        return new Markdown(markdown);
    }
}

public class MediaTab : ViewBase
{
    public override object? Build()
    {
        var markdown = """
                       # Media and Extras
                       
                       ## Images
                       
                       ![Example Image](https://placecats.com/400/300)
                       
                       Images can have alt text and titles:
                       
                       ![Cat Image](https://placecats.com/300/200 "A cute cat")
                       
                       ## Footnotes
                       
                       Here's some text with a footnote[^1] and another reference[^2].
                       
                       You can also use multiple footnotes in the same paragraph[^3].
                       
                       [^1]: This is the first footnote with additional information.
                       [^2]: This is the second footnote explaining more details.
                       [^3]: Footnotes are great for providing additional context without cluttering the main content.
                       
                       ## Emojis
                       
                       Express yourself with emojis: :smile: :heart: :star: :+1: :rocket: :fire: :zap:
                       
                       Common emojis:
                       - :smile: Happy
                       - :heart: Love
                       - :star: Favorite
                       - :rocket: Launch
                       - :fire: Hot
                       - :zap: Fast
                       - :thumbsup: Good
                       - :tada: Celebration
                       
                       Made with :heart: using Ivy Framework :rocket:
                       
                       ## Combining Features
                       
                       You can combine all these features:
                       
                       > Check out this [amazing framework](https://github.com/Ivy-Interactive/Ivy-Framework) :rocket:
                       > 
                       > It supports:
                       > - **Bold** and *italic* text
                       > - `Code blocks`
                       > - Tables and more!
                       
                       [^note]: This is a footnote in a blockquote!
                       """;

        return new Markdown(markdown);
    }
}