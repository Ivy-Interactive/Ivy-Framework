using Ivy;
using MyProject.Connections.MyDb;
using Microsoft.EntityFrameworkCore;

namespace MyProject.Apps.Dashboard;

public class TotalSalesMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        return new MetricView(
            "Total Sales",
            Icons.DollarSign,
            UseMetricData
        );
    }

    private QueryResult<MetricRecord> UseMetricData(IViewContext context)
    {
        var factory = context.UseService<MyDbContextFactory>();

        return context.UseQuery(
            key: (nameof(TotalSalesMetricView), fromDate, toDate),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var currentPeriodSales = await db.Lines
                    .Where(l => l.Order!.CreatedAt >= fromDate && l.Order.CreatedAt <= toDate)
                    .Select(l => (double)(l.Price * l.Quantity))
                    .SumAsync(ct);

                var periodLength = toDate - fromDate;
                var previousFromDate = fromDate.AddDays(-periodLength.TotalDays);
                var previousToDate = fromDate.AddDays(-1);

                var previousPeriodSales = await db.Lines
                    .Where(l => l.Order!.CreatedAt >= previousFromDate && l.Order.CreatedAt <= previousToDate)
                    .Select(l => (double)(l.Price * l.Quantity))
                    .SumAsync(ct);

                double? trend = null;
                if (previousPeriodSales > 0)
                {
                    trend = (currentPeriodSales - previousPeriodSales) / previousPeriodSales;
                }

                // Goal is 20% more than previous period sales:
                var goal = previousPeriodSales * 1.2;
                double? goalAchievement = goal > 0 ? currentPeriodSales / goal : null;

                return new MetricRecord(
                    MetricFormatted: $"{currentPeriodSales:C0}",
                    TrendComparedToPreviousPeriod: trend,
                    GoalAchieved: goalAchievement,
                    GoalFormatted: $"{goal:C0}"
                );
            },
            options: new QueryOptions { Expiration = TimeSpan.FromMinutes(5) }
        );
    }
}
