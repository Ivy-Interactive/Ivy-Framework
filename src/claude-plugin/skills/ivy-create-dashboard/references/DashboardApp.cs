using System.ComponentModel;
using Ivy;
using MyProject.Apps.Dashboard;

namespace MyProject.Apps;

[App(icon: Icons.LayoutDashboard, group: ["Apps"])]
public class DashboardApp : ViewBase
{
    private enum DateRange
    {
        [Description("7d")]
        Last7Days = 1,
        [Description("14d")]
        Last14Days = 2,
        [Description("30d")]
        Last30Days = 3
    }

    private (DateTime fromDate, DateTime toDate) GetDateRange(DateRange range)
    {
        var toDate = DateTime.UtcNow.Date.AddDays(1);
        DateTime fromDate = range switch
        {
            DateRange.Last7Days => toDate.AddDays(-7),
            DateRange.Last14Days => toDate.AddDays(-14),
            DateRange.Last30Days => toDate.AddDays(-30),
            _ => toDate.AddDays(-30)
        };
        return (fromDate, toDate);
    }

    public override object? Build()
    {
        var range = UseState<DateRange>(DateRange.Last30Days);
        var threshold = UseState(50.0);
        var isRefreshing = UseState(false);
        var refreshToken = UseRefreshToken();
        var client = UseService<IClientProvider>();

        var header = Layout.Horizontal().AlignContent(Align.SpaceBetween).AlignContent(Align.Center)
                     | Text.H3("Sales Dashboard")
                     | (Layout.Horizontal().Gap(3).AlignContent(Align.Center)
                        | threshold.ToSliderInput().Min(0).Max(100).Step(1).WithField().Label("Filter Threshold")
                        | new Button("Refresh Data", async _ =>
                        {
                            isRefreshing.Set(true);
                            await Task.Delay(500);
                            isRefreshing.Set(false);
                            refreshToken.Refresh();
                            client.Toast("Data refreshed successfully");
                        }).Primary().Loading(isRefreshing.Value).Icon(Icons.RefreshCw)
                        | range.ToSelectInput().Variant(SelectInputVariant.Toggle).Small())
            ;

        var (fromDate, toDate) = GetDateRange(range.Value);

        var metrics =
                Layout.Grid().Columns(4)
                | new TotalSalesMetricView(fromDate, toDate)
                | new MarginMetricView(fromDate, toDate)
                | new OrdersMetricView(fromDate, toDate)
                | new BasketSizeMetricView(fromDate, toDate)
                | new CustomerRetentionRateMetricView(fromDate, toDate)
            ;

        var charts =
                Layout.Grid().Columns(3)
                | new SalesByDayLineChartView(fromDate, toDate)
                | new OrdersByChannelPieChartView(fromDate, toDate)
                | new SalesByCategoryPieChartView(fromDate, toDate)
                | new AverageOrderValueOverTimeLineChartView(fromDate, toDate)
                | new SalesByStoreTypeAreaChartView(fromDate, toDate)
                | new SalesByCategoryBarChartView(fromDate, toDate)
            ;

        var body = Layout.TopCenter()
                   | (Layout.Vertical().Width(Size.Full().Max(300)).TopMargin(10)
                      | metrics
                      | charts)
            ;

        return new HeaderLayout(header, body);
    }
}
