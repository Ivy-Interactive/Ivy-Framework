
namespace Ivy.Samples.Shared.Apps.Widgets.Wireframe;

[App(icon: Icons.StickyNote, group: ["Widgets", "Wireframe"], searchHints: ["wireframe", "sticky", "note", "sketch", "prototype", "balsamiq", "brainstorm", "postit"])]
public class WireframeNoteApp : SampleBase
{
    protected override object? BuildSample()
    {
        var colors = Layout.Horizontal().Gap(4)
            | new WireframeNote("Yellow (default)")
            | new WireframeNote("Blue", WireframeNoteColor.Blue)
            | new WireframeNote("Green", WireframeNoteColor.Green)
            | new WireframeNote("Pink", WireframeNoteColor.Pink)
            | new WireframeNote("Orange", WireframeNoteColor.Orange)
            | new WireframeNote("Purple", WireframeNoteColor.Purple);

        var multiline = Layout.Horizontal().Gap(4)
            | new WireframeNote("Step 1:\nUser signs up")
            | new WireframeNote("Step 2:\nVerify email", WireframeNoteColor.Blue)
            | new WireframeNote("Step 3:\nOnboarding", WireframeNoteColor.Green);

        var board = Layout.Horizontal().Gap(4)
            | (Layout.Vertical().Gap(3).Width(Size.Units(40))
                | Text.Strong("To Do")
                | new WireframeNote("Design login page").Width(Size.Full())
                | new WireframeNote("Write API docs", WireframeNoteColor.Orange).Width(Size.Full()))
            | (Layout.Vertical().Gap(3).Width(Size.Units(40))
                | Text.Strong("In Progress")
                | new WireframeNote("Build widgets", WireframeNoteColor.Blue).Width(Size.Full()))
            | (Layout.Vertical().Gap(3).Width(Size.Units(40))
                | Text.Strong("Done")
                | new WireframeNote("DB schema", WireframeNoteColor.Green).Width(Size.Full())
                | new WireframeNote("Scaffolding", WireframeNoteColor.Green).Width(Size.Full()));

        return Layout.Vertical()
            | Text.H1("Wireframe Note")
            | Text.H2("Colors")
            | colors
            | Text.H2("Multi-line")
            | multiline
            | Text.H2("Project Board")
            | board;
    }
}
