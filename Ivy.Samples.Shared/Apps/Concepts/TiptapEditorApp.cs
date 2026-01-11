using Ivy.Shared;
using Ivy.Views;
using Ivy.Widgets.Tiptap;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.FileText, searchHints: ["tiptap", "editor", "richtext", "wysiwyg", "html", "markdown"])]
public class TiptapEditorApp : SampleBase
{
    protected override object? BuildSample()
    {
        var content = UseState("<p>Hello <strong>world</strong>! Start editing this text...</p>");
        var eventLog = UseState<string[]>([]);

        void AddLog(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            eventLog.Value = [$"[{timestamp}] {message}", .. eventLog.Value.Take(9)];
        }

        return Layout.Vertical(
            new Card() | Layout.Vertical(
                Text.Large("Tiptap Rich Text Editor").Bold(),
                Text.P("A headless, framework-agnostic rich text editor built on ProseMirror."),
                Text.Small("The Tiptap widget is loaded from the Ivy.Widgets.Tiptap NuGet package.").Muted()
            ),

            Layout.Vertical(
                // Editor column
                Layout.Vertical(
                    Text.P("Editor").Bold(),
                    new Tiptap(content.Value)
                        .WithPlaceholder("Start typing here...")
                        .MinHeight(200)
                        .HandleChange(html =>
                        {
                            content.Value = html;
                            AddLog("Content changed");
                        })
                        .HandleFocus(() => AddLog("Editor focused"))
                        .HandleBlur(() => AddLog("Editor blurred"))
                ).Grow(),

                // Preview column
                Layout.Vertical(
                    Text.P("HTML Output").Bold(),
                    new Code(content.Value, Languages.Html)
                        .ShowLineNumbers()
                        .Height(Size.Px(250))
                ).Grow()
            ).Gap(16),

            // Event log
            new Card() | Layout.Vertical(
                Text.P("Event Log").Bold(),
                eventLog.Value.Length > 0
                    ? Layout.Vertical(eventLog.Value.Select(log => Text.Small(log).Muted()).ToArray())
                    : Text.Small("No events yet. Interact with the editor above.").Muted()
            ),

            new Separator(),

            // Read-only example
            new Card() | Layout.Vertical(
                Text.P("Read-Only Mode").Bold(),
                new Tiptap(
                        "<p>This editor is <em>read-only</em>. You can view the content but cannot edit it.</p><ul><li>Item 1</li><li>Item 2</li><li>Item 3</li></ul>")
                    .ReadOnly()
                    .HideToolbar()
            ),

            // Minimal example
            new Card() | Layout.Vertical(
                Text.P("Minimal (No Toolbar)").Bold(),
                new Tiptap()
                    .WithPlaceholder("A simple editor without toolbar...")
                    .HideToolbar()
                    .MinHeight(100)
            )
        );
    }
}
