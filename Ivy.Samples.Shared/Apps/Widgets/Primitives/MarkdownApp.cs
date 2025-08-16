using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Text)]
public class MarkdownApp : ViewBase
{
    public override object? Build()
    {
        //          var markdown = 
        //              """
        //              # Hello World
        //              
        //              This is **bold** and *italic* text.
        //
        //              - List item 1
        //              - List item 2
        //                  1) Sublist item 1
        //                  2) Sublist item 2
        //              
        //              > This is a blockquote: Lorem ipsum dolor sit amet, consectetur adipiscing elit.
        //              
        //              [Link to Google](https://www.google.com)
        //              
        //              > A block quote with ~strikethrough~ and a URL: https://reactjs.org.
        //              
        //              <details>
        //                  <summary>This is a really complicated question</summary>
        //                  42
        //              </details>
        //              
        //              <details>
        //                  <summary>And another</summary>
        //                  Foo
        //              </details>
        //              
        //              ----------------------------------
        //              
        //              * Lists
        //              * [ ] todo
        //              * [x] done
        //              
        //              ----------------------------------
        //              
        //              ## Image
        //              
        //              ![Cat](https://placecats.com/300/200)
        //              
        //              ## Code
        //              
        //              ```javascript
        //              const hello = 'world';
        //              console.log(hello);
        //              ```
        //              
        //              This is inline code: `const x = 10;`
        //              
        //              ```diff
        //              diff --git a/filea.extension b/fileb.extension
        //              index d28nd309d..b3nu834uj 111111
        //              --- a/filea.extension
        //              +++ b/fileb.extension
        //              @@ -1,6 +1,6 @@
        //              -oldLine
        //              +newLine
        //              ```
        //              
        //              ## Math
        //              
        //              $$
        //              \int_a^b f(x) dx = F(b) - F(a)
        //              $$
        //              
        //              Inline Math: $E = mc^2$
        //              
        //              ```math
        //              \left( \sum_{k=1}^n a_k b_k \right)^2 \leq \left( \sum_{k=1}^n a_k^2 \right) \left( \sum_{k=1}^n b_k^2 \right)
        //              ```
        //              
        //              This is a <sub>subscript</sub> text
        //              
        //              This is a <sup>superscript</sup> text
        //              
        //              ## Table
        //              
        //              | Month    | Savings |
        //              | -------- | ------- |
        //              | January  | $250    |
        //              | February | $80     |
        //              | March    | $420    |
        //              
        //              
        //              ## Emoji
        //              
        //              :smile: :heart: :star: :+1: :smile: :rocket:
        //              
        //              """;

        //         var markdown = """
        //                        <details>
        //                            <summary>This is a really complicated question</summary>
        //                            Not sure why this isn't working
        //                        </details>
        //
        //                        <details>
        //                            <summary>And another</summary>
        //                            Foo
        //                        </details>
        //                        """;

        var markdown = """
                       Here's some text with a footnote[^1].

                       [^1]: This is the explainer content.
                       """;


        return new Markdown(markdown);
    }
}