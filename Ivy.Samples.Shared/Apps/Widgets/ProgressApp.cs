using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.Gauge, path: ["Widgets"])]
public class ProgressApp : SampleBase
{
    protected override object? BuildSample()
    {
        var progress = this.UseState((int?)50);
        return Layout.Vertical(
            Text.H1("Progress"),
            new Progress(progress.Value),
            Layout.Horizontal(
                new Button("+1", _ => progress.Set(progress.Value + 1)),
                new Button("-1", _ => progress.Set(progress.Value - 1))
            )
        );
    }
}