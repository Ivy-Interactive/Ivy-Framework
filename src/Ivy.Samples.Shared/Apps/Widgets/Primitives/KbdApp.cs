
namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Key, group: ["Widgets", "Primitives"], searchHints: ["keyboard", "shortcut", "key", "hotkey", "command", "keys"])]
public class KbdApp : SampleBase
{
    protected override object? BuildSample()
    {
        // Each key in a combination is rendered as its own standalone cap,
        // so a single Kbd describes the whole shortcut.
        return Layout.Vertical().Gap(4)
               | (Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                  | new Kbd("Ctrl+C")
                  | new Kbd("Shift+Ctrl+C"))
               | (Layout.Horizontal().Gap(2).AlignContent(Align.Center)
                  | new Kbd("Cmd+Enter")
                  | new Kbd("Alt+Backspace")
                  | new Kbd("Ctrl+ArrowUp"))
               | new Kbd("A")
            ;
    }
}
