using Ivy.Charts;
using Ivy.Shared;
using Ivy.Views.Charts;
using Ivy.Views.Dashboards;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.ChartArea)]
public class DashboardApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical() | (Layout.Grid().Columns(4)
                                    | new MetricView("Total Sales", Icons.DollarSign, () => Task.FromResult(new MetricRecord("$84,250", 0.21, 0.21, "$800,000")))
                                    | new MetricView("Very Long Revenue Number", Icons.DollarSign, () => Task.FromResult(new MetricRecord("$123,456,789.99", 12.345, 0.85, "$100,000,000")))
                                    | new MetricView("Post Engagement Rate", Icons.Activity, () => Task.FromResult(new MetricRecord("1,012.50%", 0.381, 1.25, "806.67%")))
                                    | new MetricView("Total Comments per Author", Icons.UserCheck, () => Task.FromResult(new MetricRecord("2.25", 0.381, 0.90, "2.50")))
                                    )
                                 | (Layout.Grid().Columns(4)
                                    | new MetricView("Total Comments per Author in This Period", Icons.MessageCircle, () => Task.FromResult(new MetricRecord("2.25", 0.381, 0.90, "2.50")))
                                    | new MetricView("User Engagement", Icons.Users, () => Task.FromResult(new MetricRecord("1,247", 0.125, 0.75, "1,500 users")))
                                    | new MetricView("Task Progress", Icons.Check, () => Task.FromResult(new MetricRecord("87%", null, 0.87, "100% completion")))
                                    | new MetricView("System Health", Icons.Activity, () => Task.FromResult(new MetricRecord("99.9%", null, 0.99, "100% uptime")))
                                    )

                                 | (Layout.Grid().Columns(4)
                                    | new BrowsersView()
                                    | new MonthlyRevenueTrendView()
                                    | new MonthlyRevenueDistributionView()
                                    | new DonutChartWithCustomLabelsView()
                                    )

                                 | (Layout.Grid().Columns(4)
                                    | new MetricView("Revenue Growth", Icons.TrendingUp, () => Task.FromResult(new MetricRecord("$45,230", 0.183, 0.65, "$70,000 target")))
                                    | new MetricView("Social Engagement", Icons.Star, () => Task.FromResult(new MetricRecord("2,847", null, null, null)))
                                    | new MetricView("Progress Variations", Icons.Star, () => Task.FromResult(new MetricRecord("70%", null, 0.70, "100%")))
                                    | new MetricView("Layout Testing", Icons.LayoutDashboard, () => Task.FromResult(new MetricRecord("4.8", null, 0.96, "5.0 rating")))
                                    )

                                 | (Layout.Grid().Columns(2)
                                    | new MetricView("Download Analytics", Icons.Download, () => Task.FromResult(new MetricRecord("2,090", 0.25, 0.78, "2,500 total")))
                                    | new MetricView("Global Distribution", Icons.Globe, () => Task.FromResult(new MetricRecord("47", null, 0.85, "50 countries")))
                                    )

                                 | (Layout.Grid().Columns(3)
                                    | new MetricView("Text Spacing Demo", Icons.Type, () => Task.FromResult(new MetricRecord("Compact", null, 0.60, "Tight spacing")))
                                    | new MetricView("Zero Spacing", Icons.Zap, () => Task.FromResult(new MetricRecord("Dense", null, 0.75, "Dense layout")))
                                    | new MetricView("Spacing Control", Icons.Settings, () => Task.FromResult(new MetricRecord("Custom", null, 0.90, "Custom control")))
                                    )

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

        return new Card().Title("Browser Composition").Height("100%") | data.ToPieChart(e => e.Name, e => e.Sum(f => f.Value), PieChartStyles.Dashboard);
    }
}

public class DonutChartWithCustomLabelsView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new PieChartData("Revenue", 1250000),
            new PieChartData("Marketing", 450000),
            new PieChartData("Operations", 320000),
            new PieChartData("R&D", 280000),
            new PieChartData("Admin", 150000),
            new PieChartData("Sales", 380000),
            new PieChartData("Customer Support", 220000),
            new PieChartData("IT Infrastructure", 180000),
            new PieChartData("Legal", 95000),
            new PieChartData("HR", 120000),
            new PieChartData("Finance", 160000),
            new PieChartData("Quality Assurance", 140000)
        };

        var totalValue = data.Sum(d => d.Measure);

        return new Card().Title("Donut Chart with Custom Labels").Height("100%")
            | new PieChart(data)
                .Pie(new Pie(nameof(PieChartData.Measure), nameof(PieChartData.Dimension))
                    .InnerRadius("40%")
                    .OuterRadius("90%")
                    .Animated(true)
                    .LabelList(new LabelList(nameof(PieChartData.Measure))
                        .Position(Positions.Outside)
                        .Fill(Colors.Blue)
                        .FontSize(11)
                        .NumberFormat("$0,0"))
                    .LabelList(new LabelList(nameof(PieChartData.Dimension))
                        .Position(Positions.Inside)
                        .Fill(Colors.White)
                        .FontSize(9)
                        .FontFamily("Arial"))
                )
                .ColorScheme(ColorScheme.Default)
                .Tooltip(new Ivy.Charts.Tooltip().Animated(true))
                .Legend(new Legend().IconType(Legend.IconTypes.Rect))
                .Total(totalValue, "Total Budget")
        ;
    }
}