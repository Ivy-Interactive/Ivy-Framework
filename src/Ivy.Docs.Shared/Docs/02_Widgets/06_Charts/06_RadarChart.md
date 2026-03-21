---
searchHints:
  - visualization
  - graph
  - analytics
  - data
  - radar
  - spider
  - multi-dimensional
  - comparison
  - profile
---

# RadarChart

<Ingress>
Visualize multi-dimensional data across multiple quantitative variables radiating from a center point, perfect for comparing profiles, skill assessments, and performance metrics.
</Ingress>

`RadarChart` displays data on multiple axes extending from the same origin, creating a spider web or radar pattern. It's ideal for comparing multiple variables simultaneously and identifying strengths/weaknesses across dimensions. Build chart [views](../../01_Onboarding/02_Concepts/02_Views.md) inside [layouts](../../01_Onboarding/02_Concepts/04_Layout.md) and use [state](../../03_Hooks/02_Core/03_UseState.md) for dynamic data. See [Charts](../../01_Onboarding/02_Concepts/18_Charts.md) for an overview of Ivy chart widgets.

## Basic Usage

Create a basic radar chart by providing data and configuring indicators for each dimension. Each indicator represents an axis radiating from the center.

```csharp demo-tabs
public class BasicRadarChartView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { name = "Product A", Sales = 85, Marketing = 72, Development = 90, Support = 78, Quality = 88 }
        };

        return new Card().Title("Product Performance")
            | new RadarChart(data)
                .ColorScheme(ColorScheme.Default)
                .Indicator("Sales", 100)
                .Indicator("Marketing", 100)
                .Indicator("Development", 100)
                .Indicator("Support", 100)
                .Indicator("Quality", 100)
                .Tooltip()
                .Legend()
        ;
    }
}
```

## Skill Assessment

Radar charts excel at visualizing skill profiles and competency assessments. Use filled areas to highlight overall capability patterns.

```csharp demo-tabs
public class SkillAssessmentView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { name = "Candidate A", Technical = 90, Communication = 75, Leadership = 80, ProblemSolving = 85, Teamwork = 88 }
        };

        return new Card().Title("Candidate Skills")
            | new RadarChart(data)
                .ColorScheme(ColorScheme.Default)
                .Indicator("Technical", 100)
                .Indicator("Communication", 100)
                .Indicator("Leadership", 100)
                .Indicator("ProblemSolving", 100)
                .Indicator("Teamwork", 100)
                .Radar(new Radar("values").Filled())
                .Shape(RadarShape.Circle)
                .Tooltip()
        ;
    }
}
```

## Configuration Options

### Shape

Choose between polygon (default) and circular radar shapes:

- `Shape(RadarShape.Polygon)`: Angular spider web pattern
- `Shape(RadarShape.Circle)`: Smooth circular pattern

### Visual Customization

- `SplitLine(true/false)`: Show/hide radial grid lines
- `SplitArea(true/false)`: Fill alternating areas between grid lines
- `AxisLine(true/false)`: Show/hide axis lines extending from center
- `StartAngle(degrees)`: Rotate the starting angle of the first axis

### Indicators

Each indicator defines an axis dimension. Configure min/max values:

```csharp
.Indicator("Metric Name", max: 100)
.Indicator(new RadarIndicator("Custom").Max(150).Min(0))
```

### Radar Series Styling

Customize individual radar series with fill colors, strokes, and line styles:

```csharp
.Radar(new Radar("DataKey")
    .Fill(Colors.Primary)
    .Stroke(Colors.Primary)
    .StrokeWidth(2)
    .Filled()
    .ShowSymbol(true))
```

<WidgetDocs Type="Ivy.RadarChart" ExtensionTypes="Ivy.RadarChartExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Charts/RadarChart.cs"/>
