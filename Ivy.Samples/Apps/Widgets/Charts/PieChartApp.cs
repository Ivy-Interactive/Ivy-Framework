using Ivy.Charts;
using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Charts;

[App(icon: Icons.ChartPie)]
public class PieChartApp : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "January", Desktop = 186, Mobile = (int?)100 },
            new { Month = "February", Desktop = 305, Mobile = (int?)200 },
            new { Month = "March", Desktop = 237, Mobile = (int?)300 },
            new { Month = "April", Desktop = 73, Mobile = (int?)200 },
            new { Month = "May", Desktop = 209, Mobile = (int?)30 },
            new { Month = "June", Desktop = 214, Mobile = (int?)0 },
        };

        return new Card().Width(1 / 2f)
                | new PieChart(data)
                    .ColorScheme(ColorScheme.Rainbow)
                    .Pie("Mobile", "Month")
                    .Tooltip()
                    .Legend()
            ;
    }
}