using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.Gauge, path: ["Widgets"])]
public class ProgressApp : SampleBase
{
    protected override object? BuildSample()
    {
        var progress1 = this.UseState((int?)50);
        var progress2 = this.UseState((int?)75);
        var progress3 = this.UseState((int?)100);
        var progress4 = this.UseState((int?)25);

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

            | Text.H2("Color Variants")
            | Layout.Vertical()
                | new Progress(progress2.Value).ColorVariant(Progress.ColorVariants.Primary).Goal("Primary Variant")
                | new Progress(progress2.Value).ColorVariant(Progress.ColorVariants.EmeraldGradient).Goal("Emerald Gradient")

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
            | new Progress((int?)null).Goal("Loading...")
        ;
    }
}