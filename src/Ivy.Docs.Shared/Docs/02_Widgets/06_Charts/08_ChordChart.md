---
searchHints:
  - visualization
  - graph
  - analytics
  - data
  - relationships
  - connections
  - flows
  - network
  - chord
---

# ChordChart

<Ingress>
Visualize relationships and flows between entities using circular chord diagrams.
</Ingress>

`ChordChart` displays inter-relationships between entities in a circular layout, with links connecting related nodes. Build chart [views](../../01_Onboarding/02_Concepts/02_Views.md) inside [layouts](../../01_Onboarding/02_Concepts/04_Layout.md) and use [state](../../03_Hooks/02_Core/03_UseState.md) for dynamic data. See [Charts](../../01_Onboarding/02_Concepts/18_Charts.md) for an overview of Ivy chart widgets.

Chord diagrams are ideal for visualizing migration flows, trade relationships, network traffic, department collaboration, and any data where the connections between entities matter.

## Basic Usage

Create a chord chart with `ChordData` containing nodes and links. Links reference nodes by index and include a value representing the relationship strength.

```csharp demo-below
public class ChordChartView : ViewBase
{
    public override object? Build()
    {
        var data = new ChordData(
            Nodes: new[]
            {
                new ChordNode("North America"),
                new ChordNode("Europe"),
                new ChordNode("Asia"),
                new ChordNode("South America"),
                new ChordNode("Africa"),
            },
            Links: new[]
            {
                new ChordLink(0, 1, 1200),
                new ChordLink(0, 2, 900),
                new ChordLink(0, 3, 400),
                new ChordLink(1, 2, 800),
                new ChordLink(1, 3, 350),
                new ChordLink(1, 4, 200),
                new ChordLink(2, 3, 300),
                new ChordLink(2, 4, 250),
                new ChordLink(3, 4, 150),
            }
        );

        return new ChordChart(data)
            .Tooltip()
            .Toolbox();
    }
}
```

## Trade Relationships

Use the Rainbow color scheme and Legend for trade visualizations with many entities:

```csharp demo-tabs
public class TradeRelationshipsView : ViewBase
{
    public override object? Build()
    {
        var data = new ChordData(
            Nodes: new[]
            {
                new ChordNode("USA"),
                new ChordNode("China"),
                new ChordNode("Germany"),
                new ChordNode("Japan"),
                new ChordNode("UK"),
            },
            Links: new[]
            {
                new ChordLink(0, 1, 5800),
                new ChordLink(0, 2, 2100),
                new ChordLink(0, 3, 3400),
                new ChordLink(0, 4, 1500),
                new ChordLink(1, 2, 1800),
                new ChordLink(1, 3, 3200),
                new ChordLink(2, 3, 800),
                new ChordLink(2, 4, 1200),
                new ChordLink(3, 4, 600),
            }
        );

        return new ChordChart(data)
            .ColorScheme(ColorScheme.Rainbow)
            .Tooltip()
            .Toolbox()
            .Legend();
    }
}
```

## Department Collaboration

Use sorting to arrange nodes by total connection value:

```csharp demo-tabs
public class DepartmentCollaborationView : ViewBase
{
    public override object? Build()
    {
        var data = new ChordData(
            Nodes: new[]
            {
                new ChordNode("Engineering"),
                new ChordNode("Design"),
                new ChordNode("Marketing"),
                new ChordNode("Sales"),
                new ChordNode("Product"),
            },
            Links: new[]
            {
                new ChordLink(0, 1, 85),
                new ChordLink(0, 4, 120),
                new ChordLink(1, 2, 60),
                new ChordLink(1, 4, 95),
                new ChordLink(2, 3, 110),
                new ChordLink(3, 4, 55),
            }
        );

        return new ChordChart(data)
            .Tooltip()
            .Sort()
            .Toolbox();
    }
}
```

**Configuration options:**
- `.Sort(bool)` - Sort nodes by total connection value (default: false)
- `.SortSubGroups(bool)` - Sort sub-groups within each node (default: false)
- `.PadAngle(int)` - Padding angle between nodes in degrees (default: 2)
- `.ColorScheme(ColorScheme)` - Color scheme for nodes and links
- `.Tooltip()` - Enable hover tooltips
- `.Toolbox()` - Enable chart tools (save, restore, etc.)
- `.Legend()` - Show legend for nodes

<WidgetDocs Type="Ivy.ChordChart" ExtensionTypes="Ivy.ChordChartExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Charts/ChordChart.cs"/>
