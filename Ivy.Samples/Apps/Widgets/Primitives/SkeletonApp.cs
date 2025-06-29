using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Primitives;

[App(icon: Icons.LayoutPanelTop, path: ["Widgets", "Primitives"])]
public class SkeletonApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Horizontal(
            new Skeleton()
        );
    }
}