using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.LayoutPanelTop, path: ["Widgets", "Primitives"], searchHints: ["loading", "placeholder", "shimmer", "skeleton", "loading-state", "pending"])]
public class SkeletonApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Horizontal(
            new Skeleton()
        );
    }
}