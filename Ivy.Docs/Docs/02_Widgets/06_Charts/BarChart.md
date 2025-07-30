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
Although it will be shown in this document how to draw bar charts using
the `BarChart` type, the recommended way to easily draw Bar Charts is using `ToBarChart` function.

## Changing Colors

There are two different color schemes supported; namely `Default` and `Rainbow`. The following
demo shows how the rainbow color scheme works.

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




<WidgetDocs Type="Ivy.BarChart" ExtensionTypes="Ivy.BarChartExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Charts/BarChart.cs"/> 

## More Examples

Barchart handles negative and double values. The following shows a demonstration of these
where TIOBE index is shown for some programming languages over the last five years.

Changing the value of the year, changes the chart below.

```csharp demo-tabs
public class TiobeIndexDemo : ViewBase
{
    public override object? Build()
    {

        var tiobeData2025 = new[]
        {
            new { Language = "Python", Rating = 23.08, Change = 6.67 },
            new { Language = "C++", Rating = 10.33, Change = 0.56 },
            new { Language = "C", Rating = 9.94, Change = -0.27 },
            new { Language = "Java", Rating = 9.63, Change = 0.69 },
            new { Language = "C#", Rating = 4.39, Change = -2.37 },
            new { Language = "JavaScript", Rating = 3.71, Change = 0.82 },
            new { Language = "Go", Rating = 3.02, Change = 1.17 },
            new { Language = "Visual Basic", Rating = 2.94, Change = 1.24 },
            new { Language = "Delphi/Object Pascal", Rating = 2.53, Change = 1.06 },
            new { Language = "Ada", Rating = 1.09, Change = 0.36 }
        };
        
        // Data for 2024 (December 2024)
        var tiobeData2024 = new[]
        {
            new { Language = "Python", Rating = 23.84, Change = 10.0 },
            new { Language = "C++", Rating = 10.82, Change = 1.2 },
            new { Language = "C", Rating = 10.21, Change = -1.8 },
            new { Language = "Java", Rating = 9.72, Change = 1.73 },
            new { Language = "C#", Rating = 6.76, Change = -0.8 },
            new { Language = "JavaScript", Rating = 2.89, Change = 1.72 },
            new { Language = "Go", Rating = 1.85, Change = 1.2 },
            new { Language = "Visual Basic", Rating = 1.70, Change = 0.3 },
            new { Language = "Delphi/Object Pascal", Rating = 1.47, Change = 0.2 },
            new { Language = "Ada", Rating = 0.73, Change = 0.1 }
        };
        
        // Data for 2023 (Year-end estimates)
        var tiobeData2023 = new[]
        {
            new { Language = "Python", Rating = 13.84, Change = 2.5 },
            new { Language = "C++", Rating = 9.62, Change = -2.1 },
            new { Language = "C", Rating = 12.01, Change = -1.2 },
            new { Language = "Java", Rating = 7.99, Change = -1.5 },
            new { Language = "C#", Rating = 7.56, Change = 3.2 },
            new { Language = "JavaScript", Rating = 1.17, Change = -0.8 },
            new { Language = "Go", Rating = 0.65, Change = 0.3 },
            new { Language = "Visual Basic", Rating = 1.40, Change = -0.2 },
            new { Language = "Delphi/Object Pascal", Rating = 1.27, Change = 0.1 },
            new { Language = "Ada", Rating = 0.63, Change = 0.05 }
        };
        
        // Data for 2022 (Year-end estimates)
        var tiobeData2022 = new[]
        {
            new { Language = "Python", Rating = 11.34, Change = 2.78 },
            new { Language = "C++", Rating = 11.72, Change = 4.62 },
            new { Language = "C", Rating = 13.21, Change = 3.82 },
            new { Language = "Java", Rating = 9.49, Change = -2.1 },
            new { Language = "C#", Rating = 4.36, Change = 1.2 },
            new { Language = "JavaScript", Rating = 1.97, Change = -0.5 },
            new { Language = "Go", Rating = 0.35, Change = 0.1 },
            new { Language = "Visual Basic", Rating = 1.60, Change = 0.1 },
            new { Language = "Delphi/Object Pascal", Rating = 1.17, Change = 0.3 },
            new { Language = "Ada", Rating = 0.58, Change = 0.02 }
        };
        
        // Data for 2021 (Year-end estimates)
        var tiobeData2021 = new[]
        {
            new { Language = "Python", Rating = 8.56, Change = 5.2 },
            new { Language = "C++", Rating = 7.10, Change = -1.8 },
            new { Language = "C", Rating = 9.39, Change = -2.3 },
            new { Language = "Java", Rating = 11.59, Change = 1.2 },
            new { Language = "C#", Rating = 3.16, Change = 0.8 },
            new { Language = "JavaScript", Rating = 2.47, Change = 0.3 },
            new { Language = "Go", Rating = 0.25, Change = 0.05 },
            new { Language = "Visual Basic", Rating = 1.50, Change = -0.3 },
            new { Language = "Delphi/Object Pascal", Rating = 0.87, Change = 0.1 },
            new { Language = "Ada", Rating = 0.56, Change = 0.01 }
        };
        
        // Data for 2020 (Year-end estimates)
        var tiobeData2020 = new[]
        {
            new { Language = "Python", Rating = 3.36, Change = 2.1 },
            new { Language = "C++", Rating = 8.90, Change = 0.5 },
            new { Language = "C", Rating = 11.69, Change = -1.5 },
            new { Language = "Java", Rating = 10.39, Change = -3.2 },
            new { Language = "C#", Rating = 2.36, Change = 0.3 },
            new { Language = "JavaScript", Rating = 2.17, Change = -0.1 },
            new { Language = "Go", Rating = 0.20, Change = 0.02 },
            new { Language = "Visual Basic", Rating = 1.80, Change = 0.2 },
            new { Language = "Delphi/Object Pascal", Rating = 0.77, Change = 0.05 },
            new { Language = "Ada", Rating = 0.55, Change = 0.01 }
        };

         var tiobeMap = new Dictionary<int,object>()
         {
             { 2020  , tiobeData2020} ,
             { 2021  , tiobeData2021} ,
             { 2022  , tiobeData2022} ,
             { 2023  , tiobeData2023} ,
             { 2024  , tiobeData2024} ,
             { 2025  , tiobeData2025}
         };
         
         var year = UseState(2020);
         
         return Layout.Vertical()
                    | Text.Large("TIOBE Programming Language Rankings.")
                    | Text.Small("Select Year")
                    | year.ToNumberInput()
                          .Min(2020).Max(2025).Step(1).ShowArrows()
                    | new BarChart(tiobeMap[year.Value])
                            .ColorScheme(ColorScheme.Default)
                            .Bar(new Bar("Rating", 1)
                                    .Radius(8).Fill(Colors.Purple).LegendType(LegendTypes.Square))
                            .Bar(new Bar("Change", 2).Radius(8).Fill(Colors.Orange).LegendType(LegendTypes.Square))
                            .CartesianGrid(new CartesianGrid().Horizontal())
                            .Tooltip()
                            .XAxis(new XAxis("Language").TickLine(false).AxisLine(false))
                            .Legend();   
    }
}
```


