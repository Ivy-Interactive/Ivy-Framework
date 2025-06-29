using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Primitives;

[App(icon: Icons.CodeXml, path: ["Widgets", "Primitives"])]
public class HtmlApp : SampleBase
{
    protected override object? BuildSample()
    {
        var content =
            """
            <h1>Hello World</h1>
            <p>This is <strong>bold</strong> and <em>italic</em> text.</p>
            
            <h2>Code Example</h2>
            <pre>
            <code>const hello = 'world';
            console.log(hello);
            </code>
            </pre>
            
            <ul>
                <li>List item 1</li>
                <li>List item 2</li>
            </ul>
            
            <blockquote>This is a blockquote</blockquote>
            """;

        return new Html(content);
    }
}