using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.AppWindow, path: ["Widgets", "Primitives"])]
public class IframeApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new Iframe("https://wikipedia.org");
    }
}