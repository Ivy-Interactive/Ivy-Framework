using Ivy;
using MyProject.Connections.MyDb;
using Microsoft.EntityFrameworkCore;

namespace MyProject.Apps.Dashboard;

public class CustomerRetentionRateMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        return new MetricView(
            "Customer Retention Rate",
            Icons.Repeat,
            UseMetricData
        );
    }

    private QueryResult<MetricRecord> UseMetricData(IViewContext context)
    {
        var factory = context.UseService<MyDbContextFactory>();

        return context.UseQuery(
            key: (nameof(CustomerRetentionRateMetricView), fromDate, toDate),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var currentPeriodCustomers = await db.Customers
                    .Where(c => c.Orders.Any(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate))
                    .Include(customer => customer.Orders) // !IMPORTANT: Include orders to avoid N+1 query
                    .ToListAsync(ct);

                var currentPeriodRetainedCustomers = currentPeriodCustomers
                    .Count(c => c.Orders.Count(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate) > 1);

                var currentRetentionRate = (double)currentPeriodRetainedCustomers / currentPeriodCustomers.Count * 100;

                var periodLength = toDate - fromDate;
                var previousFromDate = fromDate.AddDays(-periodLength.TotalDays);
                var previousToDate = fromDate.AddDays(-1);

                var previousPeriodCustomers = await db.Customers
                    .Where(c => c.Orders.Any(o => o.CreatedAt >= previousFromDate && o.CreatedAt <= previousToDate))
                    .ToListAsync(ct);

                var previousPeriodRetainedCustomers = previousPeriodCustomers
                    .Count(c => c.Orders.Count(o => o.CreatedAt >= previousFromDate && o.CreatedAt <= previousToDate) > 1);

                double? previousRetentionRate = null;
                if (previousPeriodCustomers.Count > 0)
                {
                    previousRetentionRate = (double)previousPeriodRetainedCustomers / previousPeriodCustomers.Count * 100;
                }

                if (previousRetentionRate == null || previousRetentionRate == 0)
                {
                    //We have nothing to compare to
                    return new MetricRecord(
                        MetricFormatted: currentRetentionRate.ToString("N2") + "%",
                        TrendComparedToPreviousPeriod: null,
                        GoalAchieved: null,
                        GoalFormatted: null
                    );
                }

                double? trend = null;
                trend = (currentRetentionRate - previousRetentionRate.Value) / previousRetentionRate.Value;

                var goal = previousRetentionRate.Value * 1.1;
                double? goalAchievement = goal > 0 ? currentRetentionRate / goal : null;

                return new MetricRecord(
                    MetricFormatted: currentRetentionRate.ToString("N2") + "%",
                    TrendComparedToPreviousPeriod: trend,
                    GoalAchieved: goalAchievement,
                    GoalFormatted: goal.ToString("N2") + "%"
                );
            },
            options: new QueryOptions { Expiration = TimeSpan.FromMinutes(5) }
        );
    }
}
