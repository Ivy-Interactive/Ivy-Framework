using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Inputs;

[App(icon: Icons.CalendarRange)]
public class DateRangeInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        var range = this.UseState(() => (from: DateTime.Today.AddDays(-7), to: DateTime.Today));

        return Layout.Vertical(
            range.ToDateRangeInput(),
            range.ToDateRangeInput().Disabled(),
            range
        );
    }
}