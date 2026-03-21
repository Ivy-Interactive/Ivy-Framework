
namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.Gauge, group: ["Widgets"], searchHints: ["loading", "percentage", "bar", "indicator", "status", "completion"])]
public class ProgressApp : SampleBase
{
    protected override object? BuildSample()
    {
        var progress1 = UseState((int?)50);
        var progress2 = UseState((int?)75);
        var progress3 = UseState((int?)100);
        var progress4 = UseState((int?)25);
        var indeterminateLoadingState = UseState(true);
        var indeterminateProgressState = UseState(25);

        return Layout.Vertical()
            | Text.H1("Progress")

            | Text.H2("Basic Progress")
            | new Progress(progress1.Value)
            | Layout.Horizontal(
                new Button("-10", _ => progress1.Set(Math.Max(0, (progress1.Value ?? 0) - 10))),
                new Button("-1", _ => progress1.Set(Math.Max(0, (progress1.Value ?? 0) - 1))),
                new Button("+1", _ => progress1.Set(Math.Min(100, (progress1.Value ?? 0) + 1))),
                new Button("+10", _ => progress1.Set(Math.Min(100, (progress1.Value ?? 0) + 10)))
            )

            | Text.H2("Colors")
            | Layout.Vertical()
                | new Progress(progress2.Value).Color(Colors.Primary).Goal("Primary")
                | new Progress(progress2.Value).Color(Colors.Amber).Goal("Amber")

            | Text.H2("With Goals")
            | Layout.Vertical()
                | new Progress(progress3.Value).Goal("Task Completed!")
                | new Progress(progress4.Value).Goal("Processing files...")
                | new Progress(75).Goal("75% of target reached")

            | Text.H2("Different Values")
            | Layout.Vertical()
                | new Progress(0).Goal("Not started")
                | new Progress(25).Goal("25% Complete")
                | new Progress(50).Goal("Halfway there")
                | new Progress(75).Goal("Almost done")
                | new Progress(100).Goal("Completed!")

            | Text.H2("Indeterminate Progress")
            | Text.Label("Using null value (backward compatible):")
            | new Progress((int?)null).Goal("Loading...")

            | Text.Label("Using explicit Indeterminate property:")
            | new Progress().Indeterminate().Goal("Processing...")
            | new Progress(50).Indeterminate().Goal("Syncing (50% before pause)...")

            | Text.H2("Toggle Indeterminate Mode")
            | BuildIndeterminateToggle(indeterminateLoadingState, indeterminateProgressState)
        ;
    }

    private static object BuildIndeterminateToggle(IState<bool> isLoading, IState<int> progress)
    {
        return Layout.Vertical()
            | new Progress(progress.Value)
                .Indeterminate(isLoading.Value)
                .Goal(isLoading.Value ? "Waiting for server..." : $"{progress.Value}% Complete")
            | Layout.Horizontal(
                new Button(isLoading.Value ? "Stop Loading" : "Start Loading", _ => isLoading.Set(!isLoading.Value)),
                new Button("+10%", _ => progress.Set(Math.Min(100, progress.Value + 10)))
            )
        ;
    }
}
