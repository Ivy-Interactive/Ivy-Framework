namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.ChartBarStacked, group: ["Widgets"], searchHints: ["stacked", "segmented", "multi", "progress", "bar"])]
public class StackedProgressApp : SampleBase
{
    protected override object? BuildSample()
    {
        var completed = UseState(40.0);
        var inProgress = UseState(25.0);
        var pending = UseState(20.0);
        var failed = UseState(15.0);
        var selectedIndex = UseState<int?>(null);
        string[] segmentLabels = ["Completed", "In Progress", "Pending", "Failed"];

        return Layout.Vertical()
            | Text.H1("Stacked Progress")

            | Text.H2("Basic Usage")
            | new StackedProgress(
                new ProgressSegment(30, Colors.Red),
                new ProgressSegment(45, Colors.Orange),
                new ProgressSegment(15, Colors.Blue),
                new ProgressSegment(10, Colors.Green)
            )

            | Text.H2("Different Colors")
            | new StackedProgress(
                new ProgressSegment(25, Colors.Emerald),
                new ProgressSegment(35, Colors.Sky),
                new ProgressSegment(20, Colors.Violet),
                new ProgressSegment(20, Colors.Amber)
            )
            | new StackedProgress(
                new ProgressSegment(50, Colors.Rose),
                new ProgressSegment(30, Colors.Indigo),
                new ProgressSegment(20, Colors.Teal)
            )

            | Text.H2("With Labels")
            | new StackedProgress(
                new ProgressSegment(40, Colors.Green, "Completed"),
                new ProgressSegment(25, Colors.Blue, "In Progress"),
                new ProgressSegment(20, Colors.Orange, "Pending"),
                new ProgressSegment(15, Colors.Red, "Failed")
            ).ShowLabels()

            | Text.H2("Different Heights")
            | new StackedProgress(
                new ProgressSegment(60, Colors.Blue),
                new ProgressSegment(40, Colors.Amber)
            ).BarHeight(4)
            | new StackedProgress(
                new ProgressSegment(60, Colors.Blue),
                new ProgressSegment(40, Colors.Amber)
            )
            | new StackedProgress(
                new ProgressSegment(60, Colors.Blue),
                new ProgressSegment(40, Colors.Amber)
            ).BarHeight(16)

            | Text.H2("Without Rounded Corners")
            | new StackedProgress(
                new ProgressSegment(33, Colors.Red),
                new ProgressSegment(33, Colors.Green),
                new ProgressSegment(34, Colors.Blue)
            ).Rounded(false)

            | Text.H2("Interactive")
            | new StackedProgress(
                new ProgressSegment(completed.Value, Colors.Green, "Completed"),
                new ProgressSegment(inProgress.Value, Colors.Blue, "In Progress"),
                new ProgressSegment(pending.Value, Colors.Orange, "Pending"),
                new ProgressSegment(failed.Value, Colors.Red, "Failed")
            ).ShowLabels().BarHeight(12)
            | (Layout.Horizontal()
                | new Button("More Completed", _ => { completed.Set(completed.Value + 5); failed.Set(Math.Max(0, failed.Value - 5)); })
                | new Button("More Failed", _ => { failed.Set(failed.Value + 5); completed.Set(Math.Max(0, completed.Value - 5)); })
                | new Button("Reset", _ => { completed.Set(40); inProgress.Set(25); pending.Set(20); failed.Set(15); }))

            | Text.H2("With OnSelect Event")
            | new StackedProgress(
                new ProgressSegment(completed.Value, Colors.Green, "Completed"),
                new ProgressSegment(inProgress.Value, Colors.Blue, "In Progress"),
                new ProgressSegment(pending.Value, Colors.Orange, "Pending"),
                new ProgressSegment(failed.Value, Colors.Red, "Failed")
            ).ShowLabels().Selected(selectedIndex.Value).OnSelect(e =>
            {
                selectedIndex.Value = e.Value;
                return ValueTask.CompletedTask;
            })
            | Text.Muted(selectedIndex.Value is { } idx ? $"Selected: {segmentLabels[idx]}" : "Click a segment to select it")
        ;
    }
}
