# AreaChart

Area charts display quantitative data over time. Multiple series can be stacked
with different colors.

```csharp demo-below
public class BasicAreaChart : ViewBase 
{   
    public override object? Build()
    {    
        var data = new[]
        {
            new { Month = "Jan", Desktop = 186, Mobile = 80 },
            new { Month = "Feb", Desktop = 305, Mobile = 200 },
            new { Month = "Mar", Desktop = 237, Mobile = 120 },
        };
    
        return Layout.Vertical()
            | Text.Large("Sales figures")
            | new AreaChart(data)
                    .ColorScheme(ColorScheme.Default)
                    .Area(new Area("Mobile", 1).Fill(Colors.Red).LegendType(LegendTypes.Square))
                    .Area(new Area("Desktop", 1).Fill(Colors.Blue).LegendType(LegendTypes.Square))
                    .CartesianGrid(new CartesianGrid().Horizontal())
                    .Tooltip()
                    .XAxis(new XAxis("Month").TickLine(false).AxisLine(false))
                    .Legend();
   }
}
```



## COVID-19 Cases

```csharp demo-below
    
public class Covid19Demo : ViewBase
{
    public override object? Build()
    {
        var data = new[] {
                new { Month = "Jan", Cases = 45680, Deaths = 1250 },
                new { Month = "Feb", Cases = 38420, Deaths = 980 },
                new { Month = "Mar", Cases = 42150, Deaths = 1100 },
                new { Month = "Apr", Cases = 28900, Deaths = 750 },
                new { Month = "May", Cases = 31200, Deaths = 820 },
                new { Month = "Jun", Cases = 35800, Deaths = 890 },
                new { Month = "Jul", Cases = 58200, Deaths = 1420 },
                new { Month = "Aug", Cases = 71500, Deaths = 1680 },
                new { Month = "Sep", Cases = 52300, Deaths = 1350 },
                new { Month = "Oct", Cases = 39600, Deaths = 1020 },
                new { Month = "Nov", Cases = 48700, Deaths = 1180 },
                new { Month = "Dec", Cases = 62800, Deaths = 1450 }
        };

        return new Card().Title("COVID-19 cases")
            | data.ToAreaChart()
                .Dimension("Month", e => e.Month)
                .Measure("Cases", e => e.Sum(f => f.Cases))
                .Measure("Deaths", e => e.Sum(f => f.Deaths))
        ;
    }
}
```

<WidgetDocs Type="Ivy.AreaChart" ExtensionTypes="Ivy.AreaChartExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Charts/AreaChart.cs"/>


## More Examples

In the previous example, solid colors have been used. However, the colors can be transparent
and the opacity can be controlled using the function `FillOpacity`. In the following example
`Fill` (Used to fill an area with a color) and `FillOpacity` are used to show area charts
that obviously fall behind other ones to show that they are indeed present.

```csharp demo-below
public class ImmigrationToEurope : ViewBase
{
    public override object? Build()
    {
        var data = new[] {
            new { Month = "Jan", Ukraine = 32000, MiddleEast = 18500, Africa = 28000, Asia = 22000, Americas = 8500 },
            new { Month = "Feb", Ukraine = 28000, MiddleEast = 16200, Africa = 24500, Asia = 19800, Americas = 7200 },
            new { Month = "Mar", Ukraine = 35000, MiddleEast = 21000, Africa = 31000, Asia = 26500, Americas = 9800 },
            new { Month = "Apr", Ukraine = 29000, MiddleEast = 19800, Africa = 33500, Asia = 24200, Americas = 8900 },
            new { Month = "May", Ukraine = 31500, MiddleEast = 22500, Africa = 38000, Asia = 28000, Americas = 10200 },
            new { Month = "Jun", Ukraine = 26000, MiddleEast = 25000, Africa = 42000, Asia = 31500, Americas = 11800 },
            new { Month = "Jul", Ukraine = 23000, MiddleEast = 28500, Africa = 48000, Asia = 35000, Americas = 13200 },
            new { Month = "Aug", Ukraine = 21000, MiddleEast = 31000, Africa = 52000, Asia = 38500, Americas = 14500 },
            new { Month = "Sep", Ukraine = 25000, MiddleEast = 27500, Africa = 45000, Asia = 33000, Americas = 12800 },
            new { Month = "Oct", Ukraine = 30000, MiddleEast = 24000, Africa = 38500, Asia = 29000, Americas = 11200 },
            new { Month = "Nov", Ukraine = 28500, MiddleEast = 20500, Africa = 32000, Asia = 25500, Americas = 9800 },
            new { Month = "Dec", Ukraine = 26000, MiddleEast = 18000, Africa = 28500, Asia = 23000, Americas = 8900 }
        };
        return new Card().Title("Immigration to Europe")
            | new AreaChart(data, new Area("Ukraine")
                        .LegendType(LegendTypes.Square)
                        .Fill(Colors.Sky)
                        .FillOpacity(0.55)
                        .Animated())
                    .Area(new Area("MiddleEast")
                          .Name("Middle East")
                         .LegendType(LegendTypes.Square)
                         .Fill(Colors.Amber)
                         .FillOpacity(0.55))
                    .Area(new Area("Africa")
                        .LegendType(LegendTypes.Square)
                        .Fill(Colors.Orange)
                        .FillOpacity(0.55))
                    .Area(new Area("Asia")
                        .LegendType(LegendTypes.Square)
                        .Fill(Colors.Teal)
                        .FillOpacity(0.55))
                    .Area(new Area("Americas")
                        .LegendType(LegendTypes.Square)
                        .Fill(Colors.Rose)
                        .FillOpacity(0.55))   
                    .XAxis(new XAxis("Month").TickLine(false).AxisLine(false))
                    .Tooltip()
                    .Legend();
        ;
    }
}
```