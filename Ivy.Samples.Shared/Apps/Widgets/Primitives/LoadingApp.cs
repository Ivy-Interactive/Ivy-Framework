using Ivy.Core.Helpers;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Loader, searchHints: ["spinner", "loader", "waiting", "progress", "loading", "busy"])]
public class LoadingApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new Loading();

        // var isLoading = UseState(false);
        //
        // UseEffect(async () =>
        // {
        //     await Task.Delay(2000);
        //     isLoading.Set(false);
        // }, [isLoading]);
        //
        // return new Fragment()
        //        | Layout.Vertical() | new Button("Show Loading", () => isLoading.Set(true))
        //        | isLoading.True(() => new Loading())!;
    }
}