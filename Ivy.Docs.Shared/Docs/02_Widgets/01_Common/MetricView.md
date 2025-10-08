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

## Metric Without Trend

You can omit the trend indicator by passing `null` for the `TrendComparedToPreviousPeriod` parameter.

```csharp demo-below
new MetricView(
    "Social Engagement", 
    Icons.Star, 
    () => Task.FromResult(new MetricRecord(
        "2,847",        // Current value
        null,           // No trend indicator
        null,           // No goal progress
        null            // No goal target
    ))
)
```

## Progress Tracking Only

Display goal progress without trend comparison for metrics focused on completion or achievement.

```csharp demo-below
new MetricView(
    "Task Progress", 
    Icons.Check, 
    () => Task.FromResult(new MetricRecord(
        "87%",          // Current progress
        null,           // No trend comparison
        0.87,           // 87% complete
        "100% completion"
    ))
)
```

## Large Numbers and Formatting

The MetricView handles long numbers gracefully with appropriate text overflow handling.

```csharp demo-below
new MetricView(
    "Very Long Revenue Number", 
    Icons.DollarSign, 
    () => Task.FromResult(new MetricRecord(
        "$123,456,789.99", 
        12.345,         // 1234.5% increase
        0.85,           // 85% of goal
        "$100,000,000"
    ))
)
```

## Negative Trends

Negative trend values automatically display with a downward arrow and destructive color styling.

```csharp demo-below
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

## Dashboard Grid Layout

Combine multiple MetricViews in a grid layout to create comprehensive dashboards.

```csharp demo-below
Layout.Grid().Columns(4)
    | new MetricView("Total Sales", Icons.DollarSign, 
        () => Task.FromResult(new MetricRecord("$84,250", 0.21, 0.21, "$800,000")))
    | new MetricView("Post Engagement", Icons.Activity, 
        () => Task.FromResult(new MetricRecord("1,012.50%", 0.381, 1.25, "806.67%")))
    | new MetricView("User Comments", Icons.UserCheck, 
        () => Task.FromResult(new MetricRecord("2.25", 0.381, 0.90, "2.50")))
    | new MetricView("System Health", Icons.Activity, 
        () => Task.FromResult(new MetricRecord("99.9%", null, 0.99, "100% uptime")))
```

## Async Data Loading

The MetricView automatically handles async data loading with a skeleton loader. This is useful when fetching metrics from databases or APIs.

```csharp demo-below
new MetricView(
    "Database Query", 
    Icons.Database, 
    async () => {
        await Task.Delay(1000); // Simulate API call
        return new MetricRecord("1,247 records", 0.125, 0.75, "1,500 records");
    }
)
```

## Error Handling

When the async data loading fails, the MetricView automatically displays an error state.

```csharp demo-below
new MetricView(
    "Failed Metric", 
    Icons.TriangleAlert, 
    async () => {
        await Task.Delay(500);
        throw new Exception("Failed to load metric data");
    }
)
```

## MetricRecord Parameters

The `MetricRecord` is a record type with the following parameters:

- **MetricFormatted** (`string`): The formatted metric value to display (e.g., "$84,250", "1,012.50%", "2.25")
- **TrendComparedToPreviousPeriod** (`double?`): Optional trend percentage as a decimal (e.g., 0.21 for 21% increase, -0.15 for 15% decrease). When provided, displays a colored arrow and percentage.
- **GoalAchieved** (`double?`): Optional goal progress from 0 to 1 (e.g., 0.85 for 85% of goal). Displays a progress bar when provided.
- **GoalFormatted** (`string?`): Optional formatted goal target text (e.g., "$800,000", "100% uptime"). Displayed alongside the progress bar.

## Visual Indicators

The MetricView provides automatic visual feedback:

- **Trend Arrows**: Green up arrow for positive trends, red down arrow for negative trends
- **Progress Bar**: Emerald gradient progress bar showing goal achievement percentage
- **Loading State**: Animated skeleton loader during async data fetching
- **Error State**: Error message display when data loading fails
- **Fixed Height**: Consistent 55-unit height for uniform dashboard layouts

<WidgetDocs Type="Ivy.Views.Dashboards.MetricView" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Views/Dashboards/MetricView.cs"/>
