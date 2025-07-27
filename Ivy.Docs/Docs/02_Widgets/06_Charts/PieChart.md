# PieChart

Pie charts represent parts of a whole. Each slice is drawn from the provided data.

The following example showcases a sample case where possible sale data from a store is listed.
The pie chart shows these data. 


## Creating a Pie or a Donut chart

To create a pie chart or a donut chart easily the function `ToPieChart` should be used. It takes two
lambda expressions to project the keys of the Pie and the values. The type of the pie can be
altered via the `PieChartStyles` enum.

The following example shows how to create a Pie and a Donut chart easily. 

```csharp demo-tabs
public class PieChartDemo : ViewBase 
{    
    public override object? Build()
    {    
        //Sales data
        var data = new[]
        {   
            new { Month = "January", Desktop = 186, Mobile = 100 },
            new { Month = "February", Desktop = 305, Mobile = 200 },
            new { Month = "March", Desktop = 237, Mobile = 300 },
        };  
         //Showing default placement of the legend at the bottom of the chart 
         return Layout.Vertical()
            | Text.Large("Mobile sales over Q1(January-March)") 
            | data.ToPieChart
                 (
                    e => e.Month,
                    e => e.Sum(f => f.Mobile),
                    PieChartStyles.Dashboard
                )
           | Text.Large("Desktop sales over Q1(January-March)")
           // Showing custom placement of the legend at the right bottom of the chart
           | data.ToPieChart
                 (
                    e => e.Month,
                    e => e.Sum(f => f.Desktop),
                    PieChartStyles.Donut
                );
    }
}    
```



### Browser Market Share

The following example shows how to use `PieChart` type to create a more fine controlled pie chart. 
The color scheme of the Pie can be changed using `ColorScheme` enum. The legend can be placed 
in any corner using `Alignment` and `VerticalAlignment` enums. 

```csharp demo-tabs
public class BrowserStatsPie : ViewBase 
{
    public override object? Build()
    {
        var years = new string[]{"2019","2025"};
        var year = this.UseState(years[0]);
        
        var map = new Dictionary<string,object>();
        var browserSharesYear1 = new []
        {
             //Rounded 
            new { Browser = "Chrome", Company = "Google", Share = 30},
            new { Browser = "Safari", Company = "Apple", Share = 16},
            new { Browser = "Edge", Company= "Microsoft", Share = 52},
            new { Browser = "Firefox", Company= "Mozilla", Share = 2}
        };    
        var browserSharesYear2 = new []
        {
            //Rounded 
            new { Browser = "Chrome", Company = "Google", Share = 70},
            new { Browser = "Safari", Company = "Apple", Share = 16},
            new { Browser = "Edge", Company = "Microsoft", Share = 12},
            new { Browser = "Firefox", Company = "Mozilla", Share = 2}
        };
        map.Add("2019", browserSharesYear1);
        map.Add("2025", browserSharesYear2);
        var yearInput = year.ToSelectInput(years.ToOptions())
                                   .Width(15);
        return Layout.Vertical()
            | H3("Browser Market share")
            | Text.Html("<i>Select the year to see shares of the browsers.</i>")
            | Text.Large("Select Year")
            | yearInput
            | new PieChart(map[year.Value])
                 .Pie(new Pie("Share","Browser")
                         .OuterRadius(150)
                         .InnerRadius(90))                      
                 .Tooltip(new Ivy.Charts.Tooltip().Animated(true))
                 .Legend(new Legend()
                             .Align(Ivy.Charts.Legend.Alignments.Right)
                             .VerticalAlign(Ivy.Charts.Legend.VerticalAlignments.Bottom));
    }    
}
```

The legend can be placed in any of the nine positions by altering the values of the alignment enums.
Also, by default, the inner and outer radius is same resulting in a circle. However, as 
can be seen in this example, these values can be altered to create a custom donut. The function
`Tooltip` makes sure that the labels show up on mouse hover. The `Animated` function makes a nice animation
when users hover on that specific part of the pie chart. 


## Drill down chart 

The following example shows how these combinations of charts can be used in a realistic example
for showing how populated some countries are. 

```csharp demo-tabs

public class DrillDownDemo : ViewBase
{
    public override object? Build()
    {
        // Country population summary (in millions)
        var countryData = new []
        {
            new { Country = "United States", Population = 333 },
            new { Country = "Sweden", Population = 10 },
            new { Country = "China", Population = 1412 },
            new { Country = "Brazil", Population = 215 },
            new { Country = "Canada", Population = 38 },
            new { Country = "Germany", Population = 83 }
        };

        // Population data for drill-down chart
        var populationData = new []
        {
            // United States - Total: 333 million
            new { State = "California", Country = "United States", Population = 39 },
            new { State = "Texas", Country = "United States", Population = 30 },
            new { State = "Florida", Country = "United States", Population = 23 },
            new { State = "New York", Country = "United States", Population = 19 },
            new { State = "Pennsylvania", Country = "United States", Population = 13 },
            new { State = "Illinois", Country = "United States", Population = 13 },
            new { State = "Ohio", Country = "United States", Population = 12 },
            new { State = "Georgia", Country = "United States", Population = 11 },
            new { State = "North Carolina", Country = "United States", Population = 11 },
            new { State = "Michigan", Country = "United States", Population = 10 },
            new { State = "Other States", Country = "United States", Population = 152 },

            // Sweden - Total: 10 million
            new { State = "Stockholm", Country = "Sweden", Population = 2 },
            new { State = "Västra Götaland", Country = "Sweden", Population = 2 },
            new { State = "Skåne", Country = "Sweden", Population = 1 },
            new { State = "Uppsala", Country = "Sweden", Population = 1 },
            new { State = "Östergötland", Country = "Sweden", Population = 1 },
            new { State = "Other Counties", Country = "Sweden", Population = 3 },

            // China - Total: 1412 million
            new { State = "Guangdong", Country = "China", Population = 127 },
            new { State = "Shandong", Country = "China", Population = 102 },
            new { State = "Henan", Country = "China", Population = 99 },
            new { State = "Jiangsu", Country = "China", Population = 85 },
            new { State = "Sichuan", Country = "China", Population = 84 },
            new { State = "Hebei", Country = "China", Population = 75 },
            new { State = "Hunan", Country = "China", Population = 66 },
            new { State = "Anhui", Country = "China", Population = 61 },
            new { State = "Hubei", Country = "China", Population = 58 },
            new { State = "Zhejiang", Country = "China", Population = 57 },
            new { State = "Other Provinces", Country = "China", Population = 598 },

            // Brazil - Total: 215 million
            new { State = "São Paulo", Country = "Brazil", Population = 46 },
            new { State = "Minas Gerais", Country = "Brazil", Population = 21 },
            new { State = "Rio de Janeiro", Country = "Brazil", Population = 17 },
            new { State = "Bahia", Country = "Brazil", Population = 15 },
            new { State = "Paraná", Country = "Brazil", Population = 12 },
            new { State = "Rio Grande do Sul", Country = "Brazil", Population = 11 },
            new { State = "Pernambuco", Country = "Brazil", Population = 10 },
            new { State = "Ceará", Country = "Brazil", Population = 9 },
            new { State = "Pará", Country = "Brazil", Population = 9 },
            new { State = "Santa Catarina", Country = "Brazil", Population = 7 },
            new { State = "Other States", Country = "Brazil", Population = 58 },

            // Canada - Total: 38 million
            new { State = "Ontario", Country = "Canada", Population = 15 },
            new { State = "Quebec", Country = "Canada", Population = 9 },
            new { State = "British Columbia", Country = "Canada", Population = 5 },
            new { State = "Alberta", Country = "Canada", Population = 4 },
            new { State = "Manitoba", Country = "Canada", Population = 1 },
            new { State = "Saskatchewan", Country = "Canada", Population = 1 },
            new { State = "Nova Scotia", Country = "Canada", Population = 1 },
            new { State = "New Brunswick", Country = "Canada", Population = 1 },
            new { State = "Newfoundland and Labrador", Country = "Canada", Population = 1 },

            // Germany - Total: 83 million
            new { State = "North Rhine-Westphalia", Country = "Germany", Population = 18 },
            new { State = "Bavaria", Country = "Germany", Population = 13 },
            new { State = "Baden-Württemberg", Country = "Germany", Population = 11 },
            new { State = "Lower Saxony", Country = "Germany", Population = 8 },
            new { State = "Hesse", Country = "Germany", Population = 6 },
            new { State = "Rhineland-Palatinate", Country = "Germany", Population = 4 },
            new { State = "Saxony", Country = "Germany", Population = 4 },
            new { State = "Berlin", Country = "Germany", Population = 4 },
            new { State = "Schleswig-Holstein", Country = "Germany", Population = 3 },
            new { State = "Brandenburg", Country = "Germany", Population = 3 },
            new { State = "Thuringia", Country = "Germany", Population = 2 },
            new { State = "Saxony-Anhalt", Country = "Germany", Population = 2 },
            new { State = "Mecklenburg-Vorpommern", Country = "Germany", Population = 2 },
            new { State = "Hamburg", Country = "Germany", Population = 2 },
            new { State = "Bremen", Country = "Germany", Population = 1 }
        };

        var countries = populationData
                            .Select(t => t.Country)
                            .Distinct()
                            .ToArray();

        var country = this.UseState(countries[0]);

        var countryInput = country.ToSelectInput(countries.ToOptions())
                                       .Width(15);

        // Get states for selected country
        var selectedCountryStates = populationData
            .Where(t => t.Country.Equals(country.Value))
            .ToArray();
        
        return Layout.Horizontal()
                 
                | (Layout.Vertical()
                   | Text.Small("Countries Population")
                   | new PieChart(countryData)
                        .Pie(new Pie("Population", "Country")
                            .InnerRadius(60)
                            .OuterRadius(80)).Tooltip())
                | (Layout.Vertical()
                    | Text.Small($"{country.Value} - States Population")
                        
                    | new PieChart(selectedCountryStates)
                            .Pie(new Pie("Population", "State")
                                     .OuterRadius(80)).Tooltip()
                    | countryInput);

    
    }
}
```



