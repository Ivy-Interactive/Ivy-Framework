---
searchHints:
  - line chart
  - visualization
  - graph
  - analytics
  - data
  - trends
  - statistics
---

# LineChart

<Ingress>
Visualize trends and changes over time with interactive line charts that support multiple data series and customizable styling.
</Ingress>

Line charts show trends over a period of time. Build chart [views](../../01_Onboarding/02_Concepts/02_Views.md) inside [layouts](../../01_Onboarding/02_Concepts/04_Layout.md) and use [state](../../03_Hooks/02_Core/03_UseState.md) for dynamic data. See [Charts](../../01_Onboarding/02_Concepts/18_Charts.md) for an overview of Ivy chart widgets. The example below renders desktop
and mobile sales figures.

```csharp demo-tabs

public class BasicLineChartDemo : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "January", Desktop = 186, Mobile = 100 },
            new { Month = "February", Desktop = 305, Mobile = 200 },
            new { Month = "March", Desktop = 237, Mobile = 300 },
            new { Month = "April", Desktop = 186, Mobile = 100 },
            new { Month = "May", Desktop = 325, Mobile = 200 }
        };
        return Layout.Vertical()
                 | data.ToLineChart(
                        style: LineChartStyles.Default)
                        .Dimension("Month", e => e.Month)
                        .Measure("Desktop", e => e.Sum(f => f.Desktop))
                        .Measure("Mobile", e => e.Sum(f => f.Mobile))
                        .Toolbox();
    }
}    
```

## Styles

There are three different styles that can be used to determine how the points on the line charts
are connected. If smooth spline like curves is needed, use `LineChartStyles.Default` or
`LineChartStyles.Dashboard`. If, however, straight line jumps are needed, then `LineChartStyles.Custom`
should be used. The following example shows these three different styles.  

```csharp demo-tabs
public class LineStylesDemo: ViewBase
{
    Dictionary<string,LineChartStyles> 
          styleMap = new 
              Dictionary<string,LineChartStyles>();
    public override object? Build()
    {
        styleMap.TryAdd("Default",LineChartStyles.Default);
        styleMap.TryAdd("Dashboard",LineChartStyles.Dashboard);
        styleMap.TryAdd("Custom",LineChartStyles.Custom);
   
        var styles = new string[]{"Default","Dashboard","Custom"};
        var selectedStyle = UseState(styles[0]);
        var style = styleMap[selectedStyle.Value];
        var styleInput = selectedStyle.ToSelectInput(styles.ToOptions())
                                   .Width(Size.Units(20));
        
        var data = new[]
        {
            new { Month = "January", Desktop = 186, Mobile = 100 },
            new { Month = "February", Desktop = 305, Mobile = 200 },
            new { Month = "March", Desktop = 237, Mobile = 300 },
            new { Month = "April", Desktop = 186, Mobile = 100 },
            new { Month = "May", Desktop = 325, Mobile = 200 },
        };
        return Layout.Vertical()
                 | styleInput
                 | data.ToLineChart(
                        style: style)
                        .Dimension("Month", e => e.Month)
                        .Measure("Desktop", e => e.Sum(f => f.Desktop))
                        .Measure("Mobile", e => e.Sum(f => f.Mobile))
                        .Toolbox();
    }
}
```

## Changing Line Widths

To change the width of individual lines, use the `StrokeWidth` function:

```csharp demo-tabs
public class LineWidthDemo : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "January", Desktop = 186, Mobile = 100},
            new { Month = "February", Desktop = 305, Mobile = 200},
            new { Month = "March", Desktop = 237, Mobile = 300},
            new { Month = "April", Desktop = 186, Mobile = 100},
            new { Month = "May", Desktop = 325, Mobile = 200}
        };
        return Layout.Vertical()
                 | new LineChart(data, "Desktop", "Month")
                        .Line(new Line("Mobile").StrokeWidth(5))
                        .Legend();
    }
}
```

<WidgetDocs Type="Ivy.LineChart" ExtensionTypes="Ivy.LineChartExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Charts/LineChart.cs"/>

## Example

<Details>
<Summary>
Bitcoin data
</Summary>
<Body>
LineChart can comfortably handle large number of data points. The following example shows
how it can be used to render bitcoin prices for the last 100 days.

```csharp demo-tabs
public class BitcoinChart : ViewBase
{
    public override object? Build()
    {
        var random = new Random(42); // Fixed seed for consistent data
        double min = 80000;
        double max = 120000;
        
     
        var bitcoinData = Enumerable.Range(1, 100)
            .Select(daysBefore => new { 
                Date = DateTime.Today.AddDays(-daysBefore), 
                Price = random.NextDouble() * (max - min) + min 
            })
            .OrderBy(x => x.Date) 
            .ToArray();
        
        return Layout.Vertical()
                 | Text.P("Bitcoin Price - Last 100 Days").Large()
                 | Text.P($"Showing {bitcoinData.Length} days of data").Small()
                 | Text.Html($"<i>From {bitcoinData.First().Date:yyyy-MM-dd} to {bitcoinData.Last().Date:yyyy-MM-dd}</i>")
                 | bitcoinData.ToLineChart(
                        style: LineChartStyles.Dashboard)
                    .Dimension("Date", e => e.Date)
                    .Measure("Price", e => e.Sum(f => f.Price))
                    .Toolbox();
    }
}
```

</Body>
</Details>

## Customizing with polish

When building charts from queryable data with `ToLineChart()`, use the `polish` callback to customize the scaffolded chart before it renders. The callback receives the fully built `LineChart` with lines from your measures already configured by the selected style.

Common use cases for `polish`:
- Override line colors, widths, or curve types
- Add reference lines or areas
- Replace the default lines array with custom line configurations
- Add or modify tooltips, legends, or grids

### Example: Custom line styling

```csharp demo-below
public class PolishLineChartDemo : ViewBase
{
    record SalesData(string Month, int Desktop, int Mobile);

    public override object? Build()
    {
        var data = new SalesData[]
        {
            new("Jan", 186, 100),
            new("Feb", 305, 200),
            new("Mar", 237, 300),
            new("Apr", 186, 100),
            new("May", 325, 200),
        };

        return Layout.Vertical()
            | data.ToLineChart(
                polish: chart =>
                {
                    // Replace the scaffolded lines with custom styling
                    return chart with
                    {
                        Lines =
                        [
                            new Line("Desktop").Stroke(Colors.Blue).StrokeWidth(3).CurveType(CurveTypes.Step),
                            new Line("Mobile").Stroke(Colors.Orange).StrokeWidth(2).StrokeDashArray("5 5")
                        ]
                    };
                }
            )
            .Dimension("Month", e => e.Month)
            .Measure("Desktop", e => e.Sum(f => f.Desktop))
            .Measure("Mobile", e => e.Sum(f => f.Mobile))
            .Toolbox();
    }
}
```

> **Note:** The `polish` callback receives the fully scaffolded `LineChart` which already includes lines from the style. Use `chart with { ... }` syntax to replace or modify chart properties while preserving others.
