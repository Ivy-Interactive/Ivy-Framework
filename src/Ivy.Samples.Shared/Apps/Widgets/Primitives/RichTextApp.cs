namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.TextQuote, path: ["Widgets", "Primitives"], searchHints: ["rich", "text", "run", "inline", "formatting"])]
public class RichTextApp : SampleBase
{
    protected override object? BuildSample()
    {
        var basic = Text.Rich()
            .Run("Hello ")
            .Bold("world")
            .Run("! This is ")
            .Italic("rich text", color: Colors.Blue)
            .Run(".");

        var words = Text.Rich()
            .Word("This")
            .Word("is")
            .Word("automatically")
            .Word("spaced")
            .Word("text.");

        var mixed = Text.Rich()
            .Run("Status: ")
            .Bold("Active", color: Colors.Green)
            .Run(" | Last updated: ")
            .Run("2 minutes ago", color: Colors.Gray);

        var highlighted = Text.Rich()
            .Run("This has a ")
            .Bold("highlighted", highlightColor: Colors.Yellow)
            .Run(" word.");

        var linked = Text.Rich()
            .Run("Visit")
            .Link("Ivy Framework", "https://github.com/example/ivy")
            .Run("for more info.");

        return Layout.Vertical(
            Text.H3("Basic Runs"),
            basic,
            Text.H3("Word (auto-spacing)"),
            words,
            Text.H3("Mixed Styling"),
            mixed,
            Text.H3("Highlight Color"),
            highlighted,
            Text.H3("Links"),
            linked
        );
    }
}
