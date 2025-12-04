using Ivy.Core.Helpers;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Loader, searchHints: ["spinner", "loader", "waiting", "progress", "loading", "busy"])]
public class LoadingApp : SampleBase
{
    protected override object? BuildSample()
    {
        var isLoading = UseState(false);

        return new Fragment()
               | Layout.Vertical() | new Button("Show Loading", () => isLoading.Set(true))
               | isLoading.True(() => new Loading())!;
    }
}