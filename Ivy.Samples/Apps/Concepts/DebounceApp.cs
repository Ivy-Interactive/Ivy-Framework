using Ivy.Shared;

namespace Ivy.Samples.Apps.Concepts;

[App(icon: Icons.Hourglass)]
public class DebounceApp : SampleBase
{
    protected override object? BuildSample()
    {
        var loadingState = UseState(false);
        var inputState = UseState("");
        var resultState = UseState("");

        UseEffect(() =>
        {
            loadingState.Set(true);
        }, [inputState]);

        UseEffect(() =>
        {
            resultState.Set("Some result that took a while to compute.");
            loadingState.Set(false);
        }, [inputState.Throttle(TimeSpan.FromMilliseconds(500)).ToTrigger()]);

        return
            Layout.Vertical()
                | inputState.ToInput()
                | (loadingState.Value ? "Loading..." : resultState.Value);
    }
}