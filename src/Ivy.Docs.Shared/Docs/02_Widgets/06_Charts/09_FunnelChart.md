---
searchHints:
  - visualization
  - graph
  - analytics
  - funnel
  - conversion
  - pipeline
  - stages
  - sales
---

# FunnelChart

<Ingress>
Visualize conversion rates and multi-stage processes with funnel charts, perfect for sales pipelines, user onboarding, and workflow analysis.
</Ingress>

`FunnelChart` displays data as progressively narrowing stages where each stage represents a step in a process. Build chart [views](../../01_Onboarding/02_Concepts/02_Views.md) inside [layouts](../../01_Onboarding/02_Concepts/04_Layout.md) and use [state](../../03_Hooks/02_Core/03_UseState.md) for dynamic data. See [Charts](../../01_Onboarding/02_Concepts/18_Charts.md) for an overview of Ivy chart widgets.

Funnel charts are ideal for visualizing sales pipelines, conversion funnels, recruitment processes, and any workflow where values decrease (or increase) through sequential stages.

## Creating a Funnel Chart

A funnel chart uses `PieChartData` records with dimension (stage name) and measure (value) fields. The chart automatically renders stages proportional to their values.

```csharp demo-tabs
public class FunnelChartView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new PieChartData("Awareness", 5000),
            new PieChartData("Interest", 3500),
            new PieChartData("Decision", 2100),
            new PieChartData("Action", 1200),
        };

        return new FunnelChart(data)
            .Funnel("Measure", "Dimension")
            .Tooltip()
            .Toolbox();
    }
}
```

## Sort Order

By default, funnel charts sort stages in descending order (largest at top). Use `.Sort()` to change the ordering:

```csharp demo-tabs
public class AscendingFunnelView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new PieChartData("Contacted", 200),
            new PieChartData("Responded", 450),
            new PieChartData("Qualified", 800),
            new PieChartData("Proposal", 1500),
            new PieChartData("Closed", 2500),
        };

        return new FunnelChart(data)
            .Funnel("Measure", "Dimension")
            .Tooltip()
            .Sort(FunnelSort.Ascending);
    }
}
```

## Orientation

Funnel charts can be displayed vertically (default) or horizontally:

```csharp demo-tabs
public class HorizontalFunnelView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new PieChartData("Browse", 8000),
            new PieChartData("Cart", 4500),
            new PieChartData("Checkout", 2800),
            new PieChartData("Purchase", 1900),
        };

        return new FunnelChart(data)
            .Funnel("Measure", "Dimension")
            .Tooltip()
            .Orientation(FunnelOrientation.Horizontal);
    }
}
```

## Customization

```csharp demo-tabs
public class CustomFunnelView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new PieChartData("Total Leads", 5000),
            new PieChartData("MQL", 3200),
            new PieChartData("SQL", 1800),
            new PieChartData("Opportunity", 900),
            new PieChartData("Closed Won", 350),
        };

        return new FunnelChart(data)
            .Funnel("Measure", "Dimension")
            .Tooltip()
            .Toolbox()
            .Legend()
            .Gap(5)
            .ColorScheme(ColorScheme.Rainbow);
    }
}
```

**Configuration options:**
- `.Sort(FunnelSort)` - Stage ordering: `Descending` (default), `Ascending`, or `None`
- `.Orientation(FunnelOrientation)` - Layout direction: `Vertical` (default) or `Horizontal`
- `.Gap(int)` - Spacing between stages in pixels (default: 0)
- `.ColorScheme(ColorScheme)` - Color scheme for stages
- `.Tooltip()` - Enable hover tooltips
- `.Toolbox()` - Enable chart tools (save, restore, etc.)
- `.Legend()` - Show legend for stages

<WidgetDocs Type="Ivy.FunnelChart" ExtensionTypes="Ivy.FunnelChartExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Charts/FunnelChart.cs"/>
