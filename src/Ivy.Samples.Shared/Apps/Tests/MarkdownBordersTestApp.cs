namespace Ivy.Samples.Shared.Apps.Tests;

/// <summary>
/// Reproduces markdown fenced-code border rendering (e.g. Ivy Tendril PromptSheet:
/// <c>new Markdown($"```\n{promptText}\n```")</c>) vs the CodeBlock widget.
/// </summary>
[App(
    icon: Icons.SquareCode,
    group: ["Tests"],
    searchHints: ["markdown", "code-block", "border", "fence", "sheet", "corner", "tendril"])]
public class MarkdownBordersTestApp : SampleBase
{
    private const string SingleLinePrompt = "Add Dark/Light Mode Toggle to Menu";

    private static string TendrilPromptMarkdown(string text) => $"```\n{text}\n```";

    protected override object? BuildSample()
    {
        var (promptSheet, openPromptSheet) = UseTrigger(isOpen =>
        {
            if (!isOpen.Value) return null;
            return new Sheet(
                () => isOpen.Set(false),
                new Markdown(TendrilPromptMarkdown(SingleLinePrompt)),
                "Full Prompt"
            ).Width(Size.Half()).Resizable();
        });

        return Layout.Vertical().Gap(6)
               | Text.H1("Markdown code fence borders")
               | new Callout(
                   "Inspect bottom-left corners. Tendril used an unlabeled markdown fence inside a sheet; the CodeBlock widget uses a different frontend path.")
                   .Variant(CalloutVariant.Info)
               | Layout.Tabs(
                   new Tab("Inline", BuildInlineTab()),
                   new Tab("In sheet", BuildInSheetTab(openPromptSheet)),
                   new Tab("Compare widget", BuildCompareTab())
               ).Variant(TabsVariant.Content)
               | promptSheet;
    }

    private static object BuildInlineTab()
    {
        return Layout.Vertical().Gap(6)
               | Text.H2("Unlabeled fence (Tendril PromptSheet)")
               | Text.Muted("new Markdown($\"```\\n{text}\\n```\")")
               | new Markdown(TendrilPromptMarkdown(SingleLinePrompt))
               | Text.H2("Explicit text language")
               | new Markdown($"```text\n{SingleLinePrompt}\n```")
               | Text.H2("C# fence")
               | new Markdown(
                   """
                   ```csharp
                   var x = 1;
                   ```
                   """)
               | Text.H2("Multi-line unlabeled fence")
               | new Markdown(
                   """
                   ```
                   line one
                   line two
                   line three
                   ```
                   """);
    }

    private static object BuildInSheetTab(Action openSheet)
    {
        return Layout.Vertical().Gap(4)
               | Text.P("Same layout as Tendril Jobs → Plan → Full Prompt (half-width resizable sheet).")
               | new Button("Open Full Prompt sheet")
                   .Primary()
                   .OnClick(_ => openSheet())
               | Text.H3("Inline preview (same markdown)")
               | new Markdown(TendrilPromptMarkdown(SingleLinePrompt));
    }

    private static object BuildCompareTab()
    {
        var widgetBlock = new CodeBlock(SingleLinePrompt, Languages.Text)
            .Height(Size.Auto())
            .ShowBorder(true)
            .ShowCopyButton(true);

        return Layout.Vertical().Gap(6)
               | Layout.Grid().Columns(2).Gap(4)
                   | Layout.Vertical().Gap(2)
                       | Text.H3("Markdown fence")
                       | Text.Muted("bg-muted scroll path")
                       | new Markdown(TendrilPromptMarkdown(SingleLinePrompt))
                   | Layout.Vertical().Gap(2)
                       | Text.H3("CodeBlock widget")
                       | Text.Muted("CodeBlockWidget path")
                       | widgetBlock;
    }
}
