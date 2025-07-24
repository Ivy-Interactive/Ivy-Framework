# BarChart

Bar charts compare values across categories. The sample below shows a stacked bar
chart with two series; sales of Desktop and Mobile in first quarter of an year.                              

```csharp demo-below

public class BarChartBasic : ViewBase 
{    
    
    public override object? Build()
    {
       var data = new[]
        {
            new { Month = "Jan", Desktop = 186, Mobile = 100 },
            new { Month = "Feb", Desktop = 305, Mobile = 200 },
            new { Month = "Mar", Desktop = 237, Mobile = 300 }, 
        };

        return Layout.Vertical()
            |  data.ToBarChart()
                    .Dimension("Month", e => e.Month)
                        .Measure("Desktop", e => e.Sum(f => f.Desktop))
                        .Measure("Mobile", e => e.Sum(f => f.Mobile));
    }
}    
```

## Changing Colors

There are two different color schemes supported; namely `Default` and `Rainbow`. The following 
demo shows how the rainbow color scheme. 

```csharp demo-below
public class RainbowBarChartBasic : ViewBase 
{    
    
    public override object? Build()
    {
       var data = new[]
        {
            new { Month = "Jan", Desktop = 186, Mobile = 100, Tablet = 75, Laptop = 120, Smartwatch = 45, Gaming = 90, IoT = 30 },
            new { Month = "Feb", Desktop = 305, Mobile = 200, Tablet = 110, Laptop = 180, Smartwatch = 65, Gaming = 140, IoT = 50 },
            new { Month = "Mar", Desktop = 237, Mobile = 300, Tablet = 95, Laptop = 160, Smartwatch = 80, Gaming = 110, IoT = 40 },
        };

        return Layout.Vertical()
                    | new BarChart(data,
                        new Bar("Desktop").LegendType(LegendTypes.Square))
                        .Bar(new Bar("Mobile").LegendType(LegendTypes.Square))
                        .Bar(new Bar("Tablet").LegendType(LegendTypes.Square))
                        .Bar(new Bar("Laptop").LegendType(LegendTypes.Square))
                        .Bar(new Bar("Smartwatch").LegendType(LegendTypes.Square))
                        .Bar(new Bar("Gaming").LegendType(LegendTypes.Square))
                        .Bar(new Bar("IoT").LegendType(LegendTypes.Square))
                        .ColorScheme(ColorScheme.Default)
                        .Tooltip()
                        .Legend();
                        
    }
}    
```

## Filling with custom colors

Here instead of using a preset `ColorScheme`, a particular bar can also be filled using a custom color. 

```csharp demo-below

public class RainbowBarChartBasic : ViewBase 
{    
    
    public override object? Build()
    {
       var data = new[]
        {
            new { Month = "Jan", Apples = 100, Oranges = 40, Blueberry  = 35 },
            new { Month = "Jan", Apples = 150, Oranges = 60, Blueberry  = 55 },
            new { Month = "Jan", Apples = 170, Oranges = 70, Blueberry  = 65 },
       };

        return Layout.Vertical()
                    | new BarChart(data,
                        new Bar("Apples")
                            .Fill(Colors.Red)
                            .LegendType(LegendTypes.Square))
                        .Bar(new Bar("Oranges")
                                .Fill(Colors.Orange)
                                .LegendType(LegendTypes.Square))
                        .Bar(new Bar("Blueberry")
                                .Fill(Colors.Blue)
                                .Name("Blueberries")
                                .LegendType(LegendTypes.Square))
                        .Tooltip()
                        .Legend();
                        
    }
}    
```

There are several functions used in this example. `Fill` is used to fill a bar chart
with a specific color. The `LegendType` function is used to configure the legend 
to use squares. Using the `Name` function, the name of a bar can be renamed. Like 
here is done for the `Blueberry` column.  

## Negative values

BarChart handles negative and double values. The following example shows how to work with such data.

```csharp demo-below
public class TiobeIndexDemo : ViewBase
{
    public override object? Build()
    {

        // Data for the chart
        var tiobeData = new[]
        {
            new { Language = "Python", Rating = 26.98 },
            new { Language = "C++", Rating = 9.80 },
            new { Language = "C", Rating = 9.65 },
            new { Language = "Java", Rating = 8.76 },
            new { Language = "C#", Rating = 4.87 },
            new { Language = "JavaScript", Rating = 3.36 },
            new { Language = "Go", Rating = 2.04 },
            new { Language = "Visual Basic", Rating = 1.94 },
            new { Language = "Ada", Rating = 1.77 },
            new { Language = "Delphi/Object Pascal", Rating = 1.77 }
        };

        // Create the bar chart - simplified version
        
        // Create the bar chart - try with explicit Y-axis data key
        
        var barChart2 = new BarChart(tiobeData, new Bar("Rating"))
    .XAxis("Language")
    .YAxis()
    .Tooltip()
    .Vertical();
        return Layout.Vertical()
            | barChart2;
                
    }
}
```




