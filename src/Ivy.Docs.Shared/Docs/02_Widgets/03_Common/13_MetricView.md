---
prepare: |
  var client = UseService<IClientProvider>();
searchHints:
  - kpi
  - metrics
  - dashboard
  - statistics
  - analytics
  - performance
---

# MetricView

<Ingress>
Display key performance indicators (KPIs) and metrics with trend indicators, goal progress tracking, and data loading via UseQuery hooks for dashboard [applications](../../01_Onboarding/02_Concepts/10_Apps.md).
</Ingress>

The `MetricView` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) is a specialized dashboard component built on top of [Card](04_Card.md) that displays business metrics with visual indicators for performance trends and goal achievement. It uses [UseQuery](../../03_Hooks/02_Core/09_UseQuery.md) hooks for data fetching and automatically handles loading states, error handling, and provides a consistent layout for KPI dashboards.

## Basic Usage

Here's a simple example of a metric view showing total sales with a trend indicator and goal progress. The second parameter is an optional icon, and the third parameter is a hook function that receives an `IViewContext` and returns a `QueryResult<MetricRecord>`.

```csharp demo-below
new MetricView(
    "Total Sales",
    Icons.DollarSign,
    ctx => ctx.UseQuery(
        key: "total-sales",
        fetcher: () => Task.FromResult(new MetricRecord(
            "$84,250",      // Current metric value
            0.21,           // 21% increase from previous period
            0.21,           // 21% of goal achieved
            "$800,000"      // Goal target
        ))
    )
)
```

### Negative Trends

Negative trend values automatically display with a downward arrow and destructive color styling.

<Callout Type="Info">
Trend Arrows: Green up arrow for positive trends, red down arrow for negative trends
</Callout>

```csharp demo-tabs
new MetricView(
    "Stock Price",
    Icons.CircleDollarSign,
    ctx => ctx.UseQuery(
        key: "stock-price",
        fetcher: () => Task.FromResult(new MetricRecord(
            "$42.30",
            -0.15,          // 15% decrease (negative trend)
            0.45,
            "$95.00 target"
        ))
    )
)
```

### Using MetricView in Layouts

Combine multiple MetricViews in grid [layouts](../../01_Onboarding/02_Concepts/02_Views.md) to create comprehensive dashboards.

<Callout Type="Info">
MetricRecord takes four parameters: MetricFormatted (string) for the value, TrendComparedToPreviousPeriod (decimal, e.g. 0.21 for 21%) for trend arrows, GoalAchieved (0 to 1) for progress bars, and GoalFormatted (string) for goal text. All except MetricFormatted are optional.
</Callout>

```csharp demo-tabs
Layout.Grid().Columns(2)
    | new MetricView("Total Sales", Icons.DollarSign,
        ctx => ctx.UseQuery(key: "sales", fetcher: () => Task.FromResult(new MetricRecord("$84,250", 0.21, 0.21, "$800,000"))))
    | new MetricView("Post Engagement", Icons.Heart,
        ctx => ctx.UseQuery(key: "engagement", fetcher: () => Task.FromResult(new MetricRecord("1,012.50%", 0.381, 1.25, "806.67%"))))
    | new MetricView("User Comments", Icons.UserCheck,
        ctx => ctx.UseQuery(key: "comments", fetcher: () => Task.FromResult(new MetricRecord("2.25", 0.381, 0.90, "2.50"))))
    | new MetricView("System Health", Icons.Activity,
        ctx => ctx.UseQuery(key: "health", fetcher: () => Task.FromResult(new MetricRecord("99.9%", null, 0.99, "100% uptime"))))
```

### Async Data Loading

The MetricView uses [UseQuery](../../03_Hooks/02_Core/09_UseQuery.md) hooks for data fetching, which automatically handle loading states with a skeleton loader. This is useful when fetching metrics from [databases](../../01_Onboarding/02_Concepts/01_Program.md) or APIs.

```csharp demo-tabs
new MetricView(
    "Database Query",
    Icons.Database,
    ctx => ctx.UseQuery(
        key: "db-query",
        fetcher: async ct => {
            await Task.Delay(1000, ct); // Simulate API call
            return new MetricRecord("1,247 records", 0.125, 0.75, "1,500 records");
        }
    )
)
```

### Error Handling

When the data fetching fails, the MetricView automatically displays an error state from the QueryResult.

```csharp demo-tabs
new MetricView(
    "Failed Metric",
    Icons.TriangleAlert,
    ctx => ctx.UseQuery<MetricRecord, string>(
        key: "failing-metric",
        fetcher: async ct => {
            await Task.Delay(500, ct);
            throw new Exception("Failed to load metric data");
        }
    )
)
```

<WidgetDocs Type="Ivy.Views.Dashboards.MetricView" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Views/Dashboards/MetricView.cs"/>

## Examples

<Details>
<Summary>
E-Commerce Analytics Dashboard
</Summary>
<Body>
A complete e-commerce dashboard showing sales metrics, customer engagement, and inventory status with async data loading from a [database](../../01_Onboarding/02_Concepts/01_Program.md).

```csharp demo-tabs
public class ECommerceDashboard : ViewBase
{
    public record SalesData(decimal Revenue, decimal PreviousRevenue, int Orders, int PreviousOrders, decimal ConversionRate, decimal PreviousConversionRate);

    private QueryResult<MetricRecord> UseRevenueMetric(IViewContext context)
    {
        return context.UseQuery(
            key: "revenue-metric",
            fetcher: async ct =>
            {
                await Task.Delay(800, ct); // Simulate database query
                var data = new SalesData(
                    Revenue: 284750.50m,
                    PreviousRevenue: 235000m,
                    Orders: 1247,
                    PreviousOrders: 1089,
                    ConversionRate: 3.45m,
                    PreviousConversionRate: 2.87m
                );

                var trend = (double)((data.Revenue - data.PreviousRevenue) / data.PreviousRevenue);
                var goalAchieved = (double)(data.Revenue / 400000m); // Monthly goal: $400k

                return new MetricRecord(
                    data.Revenue.ToString("C0"),
                    trend,
                    goalAchieved,
                    "$400,000 target"
                );
            }
        );
    }

    private QueryResult<MetricRecord> UseOrdersMetric(IViewContext context)
    {
        return context.UseQuery(
            key: "orders-metric",
            fetcher: async ct =>
            {
                await Task.Delay(600, ct);
                var orders = 1247;
                var previousOrders = 1089;
                var trend = (double)(orders - previousOrders) / previousOrders;

                return new MetricRecord(
                    orders.ToString("N0"),
                    trend,
                    (double)orders / 1500, // Goal: 1500 orders
                    "1,500 orders target"
                );
            }
        );
    }

    private QueryResult<MetricRecord> UseConversionMetric(IViewContext context)
    {
        return context.UseQuery(
            key: "conversion-metric",
            fetcher: async ct =>
            {
                await Task.Delay(700, ct);
                var rate = 3.45;
                var previous = 2.87;
                var trend = (rate - previous) / previous;

                return new MetricRecord(
                    rate.ToString("F2") + "%",
                    trend,
                    rate / 5.0, // Target: 5% conversion
                    "5% target"
                );
            }
        );
    }

    private QueryResult<MetricRecord> UseAverageOrderValue(IViewContext context)
    {
        return context.UseQuery(
            key: "aov-metric",
            fetcher: async ct =>
            {
                await Task.Delay(500, ct);
                var aov = 228.45m;
                var previous = 215.80m;

                return new MetricRecord(
                    aov.ToString("C2"),
                    (double)((aov - previous) / previous),
                    null,
                    null
                );
            }
        );
    }

    public override object? Build()
    {
        return Layout.Vertical().Gap(4)
            | Text.H2("E-Commerce Dashboard")
            | (Layout.Grid().Columns(2).Gap(3)
                | new MetricView("Total Revenue", Icons.DollarSign, UseRevenueMetric)
                | new MetricView("Total Orders", Icons.ShoppingCart, UseOrdersMetric)
                | new MetricView("Conversion Rate", Icons.TrendingUp, UseConversionMetric)
                | new MetricView("Avg Order Value", Icons.CreditCard, UseAverageOrderValue)
            );
    }
}
```

</Body>
</Details>

<Details>
<Summary>
SaaS Metrics Dashboard
</Summary>
<Body>
Track key SaaS metrics including MRR, churn rate, active users, and customer lifetime value with [UseQuery](../../03_Hooks/02_Core/09_UseQuery.md) hooks for data caching and automatic revalidation.

```csharp demo-tabs
public class SaaSDashboard : ViewBase
{
    private QueryResult<MetricRecord> UseMrrMetric(IViewContext context)
    {
        return context.UseQuery(
            key: "mrr-metric",
            fetcher: async ct =>
            {
                await Task.Delay(800, ct); // Simulate API call
                var mrr = 125430m;
                var previousMrr = 108750m;

                return new MetricRecord(
                    mrr.ToString("C0"),
                    (double)((mrr - previousMrr) / previousMrr),
                    (double)(mrr / 150000m),
                    "$150K target"
                );
            }
        );
    }

    private QueryResult<MetricRecord> UseActiveUsersMetric(IViewContext context)
    {
        return context.UseQuery(
            key: "active-users-metric",
            fetcher: async ct =>
            {
                await Task.Delay(600, ct);
                var activeUsers = 3847;
                var previousActiveUsers = 3520;

                return new MetricRecord(
                    activeUsers.ToString("N0"),
                    (double)(activeUsers - previousActiveUsers) / previousActiveUsers,
                    (double)activeUsers / 5000,
                    "5,000 users goal"
                );
            }
        );
    }

    private QueryResult<MetricRecord> UseChurnRateMetric(IViewContext context)
    {
        return context.UseQuery(
            key: "churn-rate-metric",
            fetcher: async ct =>
            {
                await Task.Delay(700, ct);
                var churnRate = 2.3;
                var previousChurnRate = 3.1;

                return new MetricRecord(
                    churnRate.ToString("F1") + "%",
                    -(churnRate - previousChurnRate) / previousChurnRate, // Negative is good for churn
                    1 - (churnRate / 5.0), // Lower is better
                    "Target: <2%"
                );
            }
        );
    }

    private QueryResult<MetricRecord> UseLtvMetric(IViewContext context)
    {
        return context.UseQuery(
            key: "ltv-metric",
            fetcher: async ct =>
            {
                await Task.Delay(500, ct);
                var ltv = 8450m;
                var previousLtv = 7890m;

                return new MetricRecord(
                    ltv.ToString("C0"),
                    (double)((ltv - previousLtv) / previousLtv),
                    null,
                    null
                );
            }
        );
    }

    public override object? Build()
    {
        return Layout.Vertical().Gap(4)
            | Text.H2("SaaS Metrics Dashboard")
            | Text.Muted("Real-time business metrics and KPIs")
            | (Layout.Grid().Columns(2).Gap(3)
                | new MetricView("Monthly Recurring Revenue", Icons.DollarSign, UseMrrMetric)
                | new MetricView("Active Users", Icons.Users, UseActiveUsersMetric)
                | new MetricView("Churn Rate", Icons.UserMinus, UseChurnRateMetric)
                | new MetricView("Customer LTV", Icons.Gem, UseLtvMetric)
            );
    }
}
```

</Body>
</Details>
