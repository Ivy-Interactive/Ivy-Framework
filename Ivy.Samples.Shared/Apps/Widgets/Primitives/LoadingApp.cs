using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Loader, searchHints: ["spinner", "loader", "waiting", "progress", "loading", "busy"])]
public class LoadingApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Center() | new Loading();
    }
}