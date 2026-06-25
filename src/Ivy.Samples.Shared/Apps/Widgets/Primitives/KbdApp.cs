
namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Key, group: ["Widgets", "Primitives"], searchHints: ["keyboard", "shortcut", "key", "hotkey", "command", "keys"])]
public class KbdApp : SampleBase
{
    protected override object? BuildSample()
    {
        // Each key in a combination renders as its own standalone cap. Modifier and
        // navigation keys (Cmd, Ctrl, Shift, Alt, Enter, Backspace, arrows) are shown
        // as platform-appropriate icons where available.
        return Layout.Vertical().Gap(6)
               | Text.H4("Default")
               | (Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                  | new Kbd("Cmd+Enter")
                  | new Kbd("Shift+Ctrl+C")
                  | new Kbd("Alt+Backspace")
                  | new Kbd("Ctrl+ArrowUp")
                  | new Kbd("A"))
               | Text.H4("Ghost")
               | (Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                  | new Kbd("Cmd+Enter").Ghost()
                  | new Kbd("Shift+Ctrl+C").Ghost()
                  | new Kbd("Esc").Ghost())
            ;
    }
}
