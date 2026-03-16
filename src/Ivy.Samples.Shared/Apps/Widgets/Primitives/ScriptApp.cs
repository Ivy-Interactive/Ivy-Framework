
namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Code, path: ["Widgets", "Primitives"], searchHints: ["script", "javascript", "js", "analytics", "external", "inline"])]
public class ScriptApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical().Gap(4)
            | Text.H4("Inline JavaScript")
            | new Script().InlineCode("console.log('Hello from Ivy Script widget!');")
            | Text.H4("External Script (async)")
            | new Script("https://cdn.jsdelivr.net/npm/canvas-confetti@1.9.3/dist/confetti.browser.min.js").Async()
            | Text.H4("Script with Integrity Check")
            | new Script("https://example.com/lib.js")
                .Integrity("sha384-examplehash")
                .CrossOrigin("anonymous");
    }
}
