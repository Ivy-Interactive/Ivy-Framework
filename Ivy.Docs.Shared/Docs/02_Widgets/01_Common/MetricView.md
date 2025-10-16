---
prepare: |
  var client = this.UseService<IClientProvider>();
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
Display key performance indicators (KPIs) and metrics with trend indicators, goal progress tracking, and async data loading for dashboard applications.
</Ingress>

The `MetricView` widget is a specialized dashboard component built on top of `Card` that displays business metrics with visual indicators for performance trends and goal achievement. It automatically handles loading states, error handling, and provides a consistent layout for KPI dashboards.

## Basic Usage

Here's a simple example of a metric view showing total sales with a trend indicator and goal progress.

```csharp demo-below
new MetricView(
    "Total Sales", 
    Icons.DollarSign, 
    () => Task.FromResult(new MetricRecord(
        "$84,250",      // Current metric value
        0.21,           // 21% increase from previous period
        0.21,           // 21% of goal achieved
        "$800,000"      // Goal target
    ))
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
    () => Task.FromResult(new MetricRecord(
        "$42.30",
        -0.15,          // 15% decrease (negative trend)
        0.45,
        "$95.00 target"
    ))
)
```

### Using MetricView in Layouts

Combine multiple MetricViews in grid layouts to create comprehensive dashboards.

<Callout Type="Info">
MetricRecord takes four parameters: MetricFormatted (string) for the value, TrendComparedToPreviousPeriod (decimal, e.g. 0.21 for 21%) for trend arrows, GoalAchieved (0 to 1) for progress bars, and GoalFormatted (string) for goal text. All except MetricFormatted are optional.
</Callout>

```csharp demo-tabs
Layout.Grid().Columns(2)
    | new MetricView("Total Sales", Icons.DollarSign, 
        () => Task.FromResult(new MetricRecord("$84,250", 0.21, 0.21, "$800,000")))
    | new MetricView("Post Engagement", Icons.Heart, 
        () => Task.FromResult(new MetricRecord("1,012.50%", 0.381, 1.25, "806.67%")))
    | new MetricView("User Comments", Icons.UserCheck, 
        () => Task.FromResult(new MetricRecord("2.25", 0.381, 0.90, "2.50")))
    | new MetricView("System Health", Icons.Activity, 
        () => Task.FromResult(new MetricRecord("99.9%", null, 0.99, "100% uptime")))
```

### Async Data Loading

The MetricView automatically handles async data loading with a skeleton loader. This is useful when fetching metrics from databases or APIs.

```csharp demo-tabs
new MetricView(
    "Database Query", 
    Icons.Database, 
    async () => {
        await Task.Delay(1000); // Simulate API call
        return new MetricRecord("1,247 records", 0.125, 0.75, "1,500 records");
    }
)
```

### Error Handling

When the async data loading fails, the MetricView automatically displays an error state.

```csharp demo-tabs
new MetricView(
    "Failed Metric", 
    Icons.TriangleAlert, 
    async () => {
        await Task.Delay(500);
        throw new Exception("Failed to load metric data");
    }
)
```

<WidgetDocs Type="Ivy.Views.Dashboards.MetricView" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Views/Dashboards/MetricView.cs"/>

## Examples

<Details>
<Summary>
E-Commerce Analytics Dashboard
</Summary>
<Body>
A complete e-commerce dashboard showing sales metrics, customer engagement, and inventory status with async data loading from a database.

```csharp demo-tabs
public class ECommerceDashboard : ViewBase
{
    public record SalesData(decimal Revenue, decimal PreviousRevenue, int Orders, int PreviousOrders, decimal ConversionRate, decimal PreviousConversionRate);
    
    private async Task<MetricRecord> GetRevenueMetric()
    {
        await Task.Delay(800); // Simulate database query
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
    
    private async Task<MetricRecord> GetOrdersMetric()
    {
        await Task.Delay(600);
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
    
    private async Task<MetricRecord> GetConversionMetric()
    {
        await Task.Delay(700);
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
    
    private async Task<MetricRecord> GetAverageOrderValue()
    {
        await Task.Delay(500);
        var aov = 228.45m;
        var previous = 215.80m;
        
        return new MetricRecord(
            aov.ToString("C2"),
            (double)((aov - previous) / previous),
            null,
            null
        );
    }

    public override object? Build()
    {
        return Layout.Vertical().Gap(4)
            | Text.H2("E-Commerce Dashboard")
            | (Layout.Grid().Columns(2).Gap(3)
                | new MetricView("Total Revenue", Icons.DollarSign, GetRevenueMetric)
                | new MetricView("Total Orders", Icons.ShoppingCart, GetOrdersMetric)
                | new MetricView("Conversion Rate", Icons.TrendingUp, GetConversionMetric)
                | new MetricView("Avg Order Value", Icons.CreditCard, GetAverageOrderValue)
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
Track key SaaS metrics including MRR, churn rate, active users, and customer lifetime value with real-time data updates.

```csharp demo-tabs
public class SaaSDashboard : ViewBase
{
    public record SaaSMetrics(
        decimal MRR,
        decimal PreviousMRR,
        int ActiveUsers,
        int PreviousActiveUsers,
        double ChurnRate,
        double PreviousChurnRate,
        decimal LTV,
        decimal PreviousLTV,
        int NewSignups,
        int PreviousSignups,
        decimal ARPU
    );
    
    private async Task<SaaSMetrics> FetchMetrics()
    {
        await Task.Delay(1000); // Simulate API call to analytics service
        
        return new SaaSMetrics(
            MRR: 125430m,
            PreviousMRR: 108750m,
            ActiveUsers: 3847,
            PreviousActiveUsers: 3520,
            ChurnRate: 2.3,
            PreviousChurnRate: 3.1,
            LTV: 8450m,
            PreviousLTV: 7890m,
            NewSignups: 287,
            PreviousSignups: 245,
            ARPU: 32.60m
        );
    }
    
    public override object? Build()
    {
        var metrics = UseState<SaaSMetrics?>(() => null);
        
        UseEffect(async () =>
        {
            metrics.Set(await FetchMetrics());
        }, []);
        
        if (metrics.Value == null)
        {
            return Layout.Grid().Columns(2).Gap(3)
                | new MetricView("MRR", Icons.DollarSign, () => Task.FromResult(new MetricRecord("Loading...", null, null, null)))
                | new MetricView("Active Users", Icons.Users, () => Task.FromResult(new MetricRecord("Loading...", null, null, null)))
                | new MetricView("Churn Rate", Icons.UserMinus, () => Task.FromResult(new MetricRecord("Loading...", null, null, null)))
                | new MetricView("Customer LTV", Icons.Gem, () => Task.FromResult(new MetricRecord("Loading...", null, null, null)));
        }
        
        var m = metrics.Value;
        
        return Layout.Vertical().Gap(4)
            | Text.H2("SaaS Metrics Dashboard")
            | Text.Muted("Real-time business metrics and KPIs")
            | (Layout.Grid().Columns(2).Gap(3)
                | new MetricView("Monthly Recurring Revenue", Icons.DollarSign, 
                    () => Task.FromResult(new MetricRecord(
                        m.MRR.ToString("C0"),
                        (double)((m.MRR - m.PreviousMRR) / m.PreviousMRR),
                        (double)(m.MRR / 150000m),
                        "$150K target"
                    )))
                | new MetricView("Active Users", Icons.Users, 
                    () => Task.FromResult(new MetricRecord(
                        m.ActiveUsers.ToString("N0"),
                        (double)(m.ActiveUsers - m.PreviousActiveUsers) / m.PreviousActiveUsers,
                        (double)m.ActiveUsers / 5000,
                        "5,000 users goal"
                    )))
                | new MetricView("Churn Rate", Icons.UserMinus, 
                    () => Task.FromResult(new MetricRecord(
                        m.ChurnRate.ToString("F1") + "%",
                        -(m.ChurnRate - m.PreviousChurnRate) / m.PreviousChurnRate, // Negative is good for churn
                        1 - (m.ChurnRate / 5.0), // Lower is better
                        "Target: <2%"
                    )))
                | new MetricView("Customer LTV", Icons.Gem, 
                    () => Task.FromResult(new MetricRecord(
                        m.LTV.ToString("C0"),
                        (double)((m.LTV - m.PreviousLTV) / m.PreviousLTV),
                        null,
                        null
                    )))
            )
            | (Layout.Grid().Columns(3).Gap(3)
                | new MetricView("New Signups", Icons.UserPlus, 
                    () => Task.FromResult(new MetricRecord(
                        m.NewSignups.ToString("N0"),
                        (double)(m.NewSignups - m.PreviousSignups) / m.PreviousSignups,
                        (double)m.NewSignups / 500,
                        "500/month target"
                    )))
                | new MetricView("ARPU", Icons.Wallet, 
                    () => Task.FromResult(new MetricRecord(
                        m.ARPU.ToString("C2"),
                        null,
                        null,
                        null
                    )))
                | new MetricView("Net Revenue Retention", Icons.Repeat, 
                    () => Task.FromResult(new MetricRecord(
                        "112%",
                        0.08,
                        1.12,
                        "Target: >100%"
                    )))
            );
    }
}
```

</Body>
</Details>
