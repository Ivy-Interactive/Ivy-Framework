using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Inputs;

[App(icon: Icons.Calendar, path: ["Widgets", "Inputs"])]
public class DateTimeInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        var dateState = this.UseState(DateTime.Now);

        return Layout.Vertical(
           dateState.ToDateTimeInput(),
           dateState.ToDateTimeInput().Disabled(),
           dateState
        );
    }
}