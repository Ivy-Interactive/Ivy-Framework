using Ivy.Shared;
using Ivy.Widgets.Internal;

namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.Terminal)]
public class TerminalApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Width(200)
               | new Terminal()
                   .AddCommand("echo Hello, world!")
                   .AddOutput("Hello, world!")
                   .Title("Your first Ivy app!")
            ;
    }
}