
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Tests;

[App]
public class HeaderLayoutTestApp : ViewBase
{
    public override object? Build()
    {
        var header = Layout.Horizontal() | new Button("Create");

        var body = new HeaderLayout(
            header,
            new Button("Click me")
        ).Scroll(Scroll.None);

        return new Fragment()
               | body
               | "BANANA"
               | "EGG";
    }
}