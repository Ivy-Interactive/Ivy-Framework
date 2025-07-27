# LineChart

Line charts show trends over a period of time. The example below renders desktop
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
                 | data.ToLineChart(style: LineChartStyles.Default)
                        .Dimension("Month", e => e.Month)
                        .Measure("Desktop", e => e.Sum(f => f.Desktop))
                        .Measure("Mobile", e => e.Sum(f => f.Mobile));
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
                                   .Width(20);
        
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
                 | data.ToLineChart(style: style)
                        .Dimension("Month", e => e.Month)
                        .Measure("Desktop", e => e.Sum(f => f.Desktop))
                        .Measure("Mobile", e => e.Sum(f => f.Mobile));
    }
}
```

## Selecting Colors 

There are two possible color schemes to choose from. `Default` or `Rainbow`. 

```csharp demo-tabs
public class LineColorSchemeDemo: ViewBase
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
                 | Text.Large("Default Colors")
                 | new LineChart(data, "Desktop", "Month")
                       .ColorScheme(ColorScheme.Default)
                 | Text.Large("Rainbow Colors")      
                 | new LineChart(data, "Mobile", "Month")
                       .ColorScheme(ColorScheme.Rainbow);
    }
}
```

## Grid Lines

To turn grid lines on and off `CartesianGrid` should be used. 


```csharp demo-tabs
public class GridLineDemo : ViewBase
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
                       .CartesianGrid().Horizontal()
                 | new LineChart(data, "Mobile", "Month")
                       .CartesianGrid().Horizontal().Vertical();
    }
}
```

## Showing Legend

To show the legend of the charts the `Legend` function should be used. 

```csharp demo-tabs
public class GridLineDemo : ViewBase
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
                        .Line("Mobile")
                       .Legend();
    }
}
```

## Labeling X and Y Axis

To label X and Y axis, `XAxis` and `YAxis` should be used along with the `Label` function like 
this. 

```csharp demo-below
public class GridLineDemo : ViewBase
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
                        .Line("Mobile")
                        .XAxis(new XAxis().Label<XAxis>("Month"))
                        .YAxis(new YAxis().Label<YAxis>("Sales"))
                       .Legend();
    }
}
```


## Changing Line widths

To Change the widths of the individual line in a line chart, `StrokeWidth` function 
should be used. The following example shows how this can be done. 

```csharp demo-below
public class GridLineDemo : ViewBase
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


## Changing size (height and width)

Sizes of the chart can be altered by altering the values of the width and height. 

```csharp demo-tabs
public class GridLineDemo : ViewBase
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
        var height = UseState(20);
        var width  = UseState(100);
        
        return Layout.Vertical()
                 | new LineChart(data, "Desktop", "Month")
                        .Line("Mobile")
                        .Height(height.Value)
                        .Width(width.Value)
                       .Legend()
                 | (Layout.Horizontal()
                     | Text.Large("Height")
                     | new NumberInput<int>(
                           height.Value,
                            e => {
                                 height.Set(e); 
                            })
                     .Min(20)
                     .Variant(NumberInputs.Slider)) 
                 | (Layout.Horizontal()
                     | Text.Large("Width")
                     | new NumberInput<int>(
                           width.Value,
                            e => {
                                 width.Set(e); 
                            })
                       .Step(1)
                     .Max(400)
                     .Variant(NumberInputs.Slider));
    }
}
```

## Example 

### Bitcoin data 

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
                 | Text.Large("Bitcoin Price - Last 100 Days")
                 | Text.Small($"Showing {bitcoinData.Length} days of data")
                 | Text.Html($"<i>From {bitcoinData.First().Date:yyyy-MM-dd} to {bitcoinData.Last().Date:yyyy-MM-dd}</i>")
                 | bitcoinData.ToLineChart(style:LineChartStyles.Dashboard)
                    .Dimension("Date", e => e.Date)
                    .Measure("Price", e => e.Sum(f => f.Price));
    }
}
```