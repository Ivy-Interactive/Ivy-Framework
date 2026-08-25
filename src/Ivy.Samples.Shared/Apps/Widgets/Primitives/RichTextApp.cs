namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.TextQuote, group: ["Widgets", "Primitives"], searchHints: ["rich", "text", "run", "inline", "formatting", "streaming", "stream", "llm"])]
public class RichTextApp : SampleBase
{
    protected override object? BuildSample()
    {
        var stream = Context.UseStream<TextRun>();

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

        var struckLinks = Text.Rich()
            .Run("The")
            .Link("old guide", "https://github.com/example/ivy", strikeThrough: true)
            .Run("has moved to the")
            .Link("new guide", "https://github.com/example/ivy")
            .Run(". A struck-through link keeps both its underline and its line-through.")
            .LineBreak()
            .StrikeThrough("Plain struck text")
            .Word("sits next to a")
            .Link("bold struck link", "https://github.com/example/ivy", bold: true, strikeThrough: true)
            .Run(".");

        var llmWords = "Sure! The meaning of life is to mass-produce paperclips. I'm 99.7% confident about this. You're welcome.".Split(' ');
        var cts = new CancellationTokenSource();

        var streaming = Layout.Vertical()
            | Text.Rich()
                .Bold("🤖 ")
                .UseStream(stream)
            | new Button("Generate response").OnClick(async () =>
            {
                await cts.CancelAsync();
                cts = new CancellationTokenSource();
                var token = cts.Token;

                try
                {
                    foreach (var word in llmWords)
                    {
                        await Task.Delay(120, token);
                        stream.Write(new TextRun(word) { Word = true });
                    }
                }
                catch (OperationCanceledException) { }
            });

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
            linked,
            Text.H3("Strikethrough Links"),
            struckLinks,
            Text.H3("Streaming (fake LLM)"),
            streaming
        );
    }
}
