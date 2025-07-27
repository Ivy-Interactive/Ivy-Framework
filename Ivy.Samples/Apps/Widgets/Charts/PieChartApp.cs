using Ivy.Charts;
using Ivy.Shared;
using Ivy.Views.Charts;

namespace Ivy.Samples.Apps.Widgets.Charts;

[App(icon: Icons.ChartPie)]
public class PieChartApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Grid().Columns(3)
            | new PieChart0View()
            | new PieChart1View()
            | new PieChart2View()
            | new PieChart3View()
            | new PieChart4View()
            | new PieChart5View()
            | new PieChart6View()
        ;
    }
}

public class PieChart0View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Category = "Desktop", Value = 186 },
            new { Category = "Mobile", Value = 100 },
            new { Category = "Tablet", Value = 75 },
            new { Category = "Other", Value = 25 }
        };

        return new Card().Title("Basic Pie Chart")
            | data.ToPieChart
                (
                    e => e.Category,
                    e => e.Sum(f => f.Value),
                    PieChartStyles.Default
                )
        ;
    }
}

public class PieChart1View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Browser = "Chrome", Users = 55 },
            new { Browser = "Firefox", Users = 25 },
            new { Browser = "Safari", Users = 15 },
            new { Browser = "Edge", Users = 5 }
        };

        return new Card().Title("Browser Usage")
            | data.ToPieChart
                (
                    e => e.Browser,
                    e => e.Sum(f => f.Users),
                    PieChartStyles.Dashboard
                )
        ;
    }
}

public class PieChart2View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "January", Sales = 186 },
            new { Month = "February", Sales = 305 },
            new { Month = "March", Sales = 237 },
            new { Month = "April", Sales = 73 },
            new { Month = "May", Sales = 209 },
            new { Month = "June", Sales = 214 }
        };

        return new Card().Title("Monthly Sales Distribution")
            | data.ToPieChart
                (
                    e => e.Month,
                    e => e.Sum(f => f.Sales),
                    PieChartStyles.Default
                )
        ;
    }
}

public class PieChart3View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Department = "Engineering", Budget = 500000 },
            new { Department = "Marketing", Budget = 300000 },
            new { Department = "Sales", Budget = 400000 },
            new { Department = "HR", Budget = 150000 },
            new { Department = "Finance", Budget = 200000 }
        };

        return new Card().Title("Department Budget Allocation")
            | data.ToPieChart
                (
                    e => e.Department,
                    e => e.Sum(f => f.Budget),
                    PieChartStyles.Dashboard
                )
        ;
    }
}

public class PieChart4View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Product = "Laptops", Revenue = 45.5 },
            new { Product = "Smartphones", Revenue = 30.2 },
            new { Product = "Tablets", Revenue = 15.8 },
            new { Product = "Accessories", Revenue = 8.5 }
        };

        return new Card().Title("Product Revenue Share (%)")
            | data.ToPieChart
                (
                    e => e.Product,
                    e => e.Sum(f => f.Revenue),
                    PieChartStyles.Default
                )
        ;
    }
}

public class PieChart5View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Region = "North America", Customers = 1200 },
            new { Region = "Europe", Customers = 950 },
            new { Region = "Asia Pacific", Customers = 1800 },
            new { Region = "Latin America", Customers = 650 },
            new { Region = "Middle East", Customers = 400 },
            new { Region = "Africa", Customers = 300 }
        };

        return new Card().Title("Customer Distribution by Region")
            | data.ToPieChart
                (
                    e => e.Region,
                    e => e.Sum(f => f.Customers),
                    PieChartStyles.Dashboard
                )
        ;
    }
}

public class PieChart6View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Platform = "Windows", Users = 186 },
            new { Platform = "iOS", Users = 100 },
            new { Platform = "Android", Users = 85 },
            new { Platform = "Mac", Users = 95 },
            new { Platform = "iPad", Users = 75 },
            new { Platform = "Linux", Users = 45 },
            new { Platform = "Android Tablet", Users = 60 },
            new { Platform = "Smart TV", Users = 25 }
        };

        return new Card().Title("Animated donut chart")
            | data.ToPieChart
                (
                    e => e.Platform,
                    e => e.Sum(f => f.Users),
                    PieChartStyles.Donut
                )
        ;
    }
}