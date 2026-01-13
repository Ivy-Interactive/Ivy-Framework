using Ivy.Shared;
using Ivy.Widgets.Tiptap;

namespace Ivy.Samples.Shared.Apps.Widgets.External;

[App(icon: Icons.FileText, searchHints: ["tiptap", "editor", "richtext", "wysiwyg", "html", "markdown"])]
public class TiptapInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("Tiptap Input")
               | Layout.Tabs(
                   new Tab("Basic", new BasicView())
               ).Variant(TabsVariant.Content);
    }
}

public class BasicView : ViewBase
{
    public override object? Build()
    {
        var state = UseState<string>("<p>Hello <strong>World!</strong></p>");
        return state.ToTiptapInput();
    }
}
