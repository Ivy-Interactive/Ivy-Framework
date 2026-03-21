---
searchHints:
  - sankey
  - visualization
  - flow
  - analytics
  - data
  - diagram
  - network
  - energy
  - funnel
---

# SankeyChart

<Ingress>
Visualize flows between nodes where arrow widths are proportional to flow quantities, perfect for showing transfers, conversions, and distributions.
</Ingress>

`SankeyChart` displays flow diagrams where the width of each arrow represents the magnitude of flow between nodes. Build chart [views](../../01_Onboarding/02_Concepts/02_Views.md) inside [layouts](../../01_Onboarding/02_Concepts/04_Layout.md) and use [state](../../03_Hooks/02_Core/03_UseState.md) for dynamic data. See [Charts](../../01_Onboarding/02_Concepts/18_Charts.md) for an overview of Ivy chart widgets.

Sankey diagrams are ideal for visualizing energy flows, budget allocation, user navigation paths, supply chains, and data pipelines where both relationships and flow magnitudes matter.

## Creating a Sankey Chart

A Sankey chart requires `SankeyData` with nodes and links. Links reference nodes by index and include a value representing the flow magnitude.

```csharp demo-tabs
public class SankeyChartView : ViewBase
{
    public override object? Build()
    {
        var data = new SankeyData(
            Nodes: new[]
            {
                new SankeyNode("Visit"),
                new SankeyNode("Add to Cart"),
                new SankeyNode("Checkout"),
                new SankeyNode("Purchase"),
                new SankeyNode("Bounce"),
            },
            Links: new[]
            {
                new SankeyLink(0, 1, 3500),   // Visit -> Add to Cart
                new SankeyLink(0, 4, 1500),   // Visit -> Bounce
                new SankeyLink(1, 2, 2800),   // Cart -> Checkout
                new SankeyLink(1, 4, 700),    // Cart -> Bounce
                new SankeyLink(2, 3, 2200),   // Checkout -> Purchase
                new SankeyLink(2, 4, 600),    // Checkout -> Bounce
            }
        );

        return new SankeyChart(data)
            .Tooltip()
            .Toolbox();
    }
}
```

## Customization Options

The following example demonstrates node width, gap, curvature, and alignment customization:

```csharp demo-tabs
public class CustomSankeyChartView : ViewBase
{
    public override object? Build()
    {
        var data = new SankeyData(
            Nodes: new[]
            {
                new SankeyNode("Revenue"),
                new SankeyNode("Marketing"),
                new SankeyNode("Operations"),
                new SankeyNode("R&D"),
                new SankeyNode("Digital Ads"),
                new SankeyNode("Content"),
                new SankeyNode("Salaries"),
            },
            Links: new[]
            {
                new SankeyLink(0, 1, 450000),
                new SankeyLink(0, 2, 320000),
                new SankeyLink(0, 3, 280000),
                new SankeyLink(1, 4, 250000),
                new SankeyLink(1, 5, 200000),
                new SankeyLink(2, 6, 200000),
            }
        );

        return new SankeyChart(data)
            .NodeWidth(25)          // Width of node rectangles
            .NodeGap(15)            // Vertical spacing between nodes
            .Curvature(0.7)         // Link curvature (0-1)
            .NodeAlign(SankeyAlign.Left)  // Align nodes to left
            .ColorScheme(ColorScheme.Rainbow)
            .Tooltip()
            .Toolbox();
    }
}
```

**Configuration options:**
- `.NodeWidth(int)` - Width of node rectangles in pixels (default: 20)
- `.NodeGap(int)` - Vertical spacing between nodes (default: 8)
- `.Curvature(double)` - Link curve amount, 0-1 (default: 0.5)
- `.LayoutIterations(int)` - Layout algorithm precision (default: 32)
- `.NodeAlign(SankeyAlign)` - Node alignment: `Justify` (default) or `Left`
- `.ColorScheme(ColorScheme)` - Color scheme for nodes and links
- `.Tooltip()` - Enable hover tooltips
- `.Toolbox()` - Enable chart tools (save, restore, etc.)
- `.Legend()` - Show legend for nodes

<WidgetDocs Type="Ivy.SankeyChart" ExtensionTypes="Ivy.SankeyChartExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Charts/SankeyChart.cs"/>
