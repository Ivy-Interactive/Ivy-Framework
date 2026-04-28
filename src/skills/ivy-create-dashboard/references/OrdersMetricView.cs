using Ivy;
using MyProject.Connections.MyDb;
using Microsoft.EntityFrameworkCore;

namespace MyProject.Apps.Dashboard;

public class OrdersMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        return new MetricView(
            "Orders",
            Icons.ShoppingCart,
            UseMetricData
        );
    }

    private QueryResult<MetricRecord> UseMetricData(IViewContext context)
    {
        var factory = context.UseService<MyDbContextFactory>();

        return context.UseQuery(
            key: (nameof(OrdersMetricView), fromDate, toDate),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var currentPeriodOrders = await db.Orders
                    .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
                    .CountAsync(ct);

                var periodLength = toDate - fromDate;
                var previousFromDate = fromDate.AddDays(-periodLength.TotalDays);
                var previousToDate = fromDate.AddDays(-1);

                var previousPeriodOrders = await db.Orders
                    .Where(o => o.CreatedAt >= previousFromDate && o.CreatedAt <= previousToDate)
                    .CountAsync(ct);

                if (previousPeriodOrders == 0)
                {
                    //We have nothing to compare to
                    return new MetricRecord(
                        MetricFormatted: currentPeriodOrders.ToString("N0"),
                        TrendComparedToPreviousPeriod: null,
                        GoalAchieved: null,
                        GoalFormatted: null
                    );
                }

                double? trend = ((double)currentPeriodOrders - previousPeriodOrders) / previousPeriodOrders;

                // Goal is 10% more than previous period orders
                var goal = previousPeriodOrders * 1.1;
                double? goalAchievement = goal > 0 ? currentPeriodOrders / goal : null;

                //Use C0 to format numbers that represent currency
                return new MetricRecord(
                    MetricFormatted: currentPeriodOrders.ToString("N0"),
                    TrendComparedToPreviousPeriod: trend,
                    GoalAchieved: goalAchievement,
                    GoalFormatted: goal.ToString("N0")
                );
            },
            options: new QueryOptions { Expiration = TimeSpan.FromMinutes(5) }
        );
    }
}
