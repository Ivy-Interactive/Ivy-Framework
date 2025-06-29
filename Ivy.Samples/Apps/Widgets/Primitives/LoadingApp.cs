using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Primitives;

[App(icon: Icons.Loader)]
public class LoadingApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Center() | new Loading();
    }
}