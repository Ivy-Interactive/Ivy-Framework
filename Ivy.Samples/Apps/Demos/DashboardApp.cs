using Ivy.Charts;
using Ivy.Shared;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Samples.Apps.Demos;

[App(icon: Icons.ChartArea)]
public class DashboardApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical() | (Layout.Grid().Columns(4)
                                    | new TotalSalesMetricView()
                                    | new TotalSalesMetricView()
                                    | new TotalSalesMetricView()
                                    | new TotalSalesMetricView()
                                    )

                                 | (Layout.Grid().Columns(3)
                                    | new BrowsersView()
                                    | new MonthlyRevenueTrendView()
                                    | new MonthlyRevenueDistributionView()
                                    )

            ;
    }
}

public class TotalSalesMetricView : ViewBase
{
    public override object? Build()
    {
        return new MetricView("Total Sales");
    }
}

public class MetricView(string title) : ViewBase
{
    public override object? Build()
    {
        return new Card(
                (Layout.Horizontal().Align(Align.Left).Gap(2)
                 | Text.H3("$84,250")
                 | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
                 | Text.Small("21%").Color(Colors.Emerald)),
                new Progress(21).Goal(800_000.ToString("C0"))
            ).Title(title).Icon(Icons.DollarSign)
            ;
    }
}

public class MonthlyRevenueTrendView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Desktop = 186, Mobile = 100 },
            new { Month = "Feb", Desktop = 305, Mobile = 200 },
            new { Month = "Mar", Desktop = 237, Mobile = 300 },
            new { Month = "Apr", Desktop = 73, Mobile = 400 },
            new { Month = "May", Desktop = 209, Mobile = 30 },
            new { Month = "Jun", Desktop = 214, Mobile = 45 },
        };

        return new Card().Title("Monthly Revenue Trend").Height("100%")
                | data.ToLineChart(style: LineChartStyles.Dashboard)
                    .Dimension("Month", e => e.Month)
                    .Measure("Total", e => e.Sum(f => f.Desktop + f.Mobile))
                    .TableCalculation(TableCalculations.RunningTotal("Total"))
            ;
    }
}

public class MonthlyRevenueDistributionView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Desktop = 186, Mobile = 100 },
            new { Month = "Feb", Desktop = 305, Mobile = 200 },
            new { Month = "Mar", Desktop = 237, Mobile = 300 },
            new { Month = "Apr", Desktop = 73, Mobile = 400 },
            new { Month = "May", Desktop = 209, Mobile = 30 },
            new { Month = "Jun", Desktop = 214, Mobile = 45 },
        };

        return new Card().Title("Monthly Revenue Distribution").Height("100%")
               | data.ToAreaChart(style: AreaChartStyles.Dashboard)
                   .Dimension("Month", e => e.Month)
                   .Measure("Desktop", e => e.Sum(f => f.Desktop))
                   .Measure("Mobile", e => e.Sum(f => f.Mobile))
            ;
    }
}

public class BrowsersView : ViewBase
{
    public override object? Build()
    {
        var data = new[] {
            new { Name = "Edge", Value = 15 },
            new { Name = "Chrome", Value = 55 },
            new { Name = "Firefox", Value = 25 },
            new { Name = "Safari", Value = 10 },
            new { Name = "Others", Value = 5 },
            new { Name = "Chrome", Value = 65 },
            new { Name = "Firefox", Value = 30 },
            new { Name = "Edge", Value = 20 },
            new { Name = "Safari", Value = 15 },
            new { Name = "Others", Value = 10 },
            new { Name = "Chrome", Value = 70 },
            new { Name = "Firefox", Value = 35 },
            new { Name = "Edge", Value = 25 },
            new { Name = "Safari", Value = 20 },
            new { Name = "Others", Value = 15 }
        };

        return new Card().Title("Browser Composition") | data.ToPieChart(e => e.Name, e => e.Sum(f => f.Value), PieChartStyles.Dashboard);
    }
}
