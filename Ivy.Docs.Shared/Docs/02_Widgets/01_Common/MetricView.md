---
prepare: |
  var client = this.UseService<IClientProvider>();
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
    Icons.TrendingDown, 
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
    | new MetricView("Post Engagement", Icons.Activity, 
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
