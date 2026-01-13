using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Key, path: ["Widgets", "Primitives"], searchHints: ["keyboard", "shortcut", "key", "hotkey", "command", "keys"])]
public class KbdApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Horizontal().Gap(1).Align(Align.Center)
               | new Kbd("Ctrl")
               | "+"
               | new Kbd("C")
            ;

    }
}