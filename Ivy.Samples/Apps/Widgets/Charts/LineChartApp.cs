using Ivy.Charts;
using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Charts;

[App(icon: Icons.ChartLine)]
public class LineChartApp : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "January", Desktop = 186, Mobile = (int?)100 },
            new { Month = "February", Desktop = 305, Mobile = (int?)200 },
            new { Month = "March", Desktop = 237, Mobile = (int?)300 },
            new { Month = "April", Desktop = 73, Mobile = (int?)400 },
            new { Month = "May", Desktop = 209, Mobile = (int?)30 },
            new { Month = "June", Desktop = 214, Mobile = (int?)45 },
        };

        return new Card(
                new LineChart(data)
                    .ColorScheme(ColorScheme.Rainbow)
                    .Line("Mobile")
                    .Line("Desktop")
                    .CartesianGrid()
                    .Tooltip()
                    .XAxis("Month")
                    .YAxis("Desktop")
                    .Legend()
            ).Width(1 / 2f)
            ;
    }
}