using Ivy.Charts;
using Ivy.Shared;
using Ivy.Views.Charts;

namespace Ivy.Samples.Shared.Apps.Widgets.Charts;

[App(icon: Icons.ChartPie, searchHints: ["visualization", "graph", "analytics", "data", "donut", "statistics"])]
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
            | new PieChart7View()
            | new PieChart8View()
            | new PieChart9View()
            | new PieChart10View()
            | new PieChart11View()
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
                .Toolbox()
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
                .Toolbox()
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
                .Toolbox()
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
                .Toolbox()
        ;
    }
}

public class PieChart7View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Department = "Engineering", Budget = 450000 },
            new { Department = "Marketing", Budget = 180000 },
            new { Department = "Sales", Budget = 220000 },
            new { Department = "HR", Budget = 120000 },
            new { Department = "Finance", Budget = 80000 },
            new { Department = "Operations", Budget = 150000 },
        };

        return new Card().Title("Department Budget Allocation")
            | data.ToPieChart
                (
                    e => e.Department,
                    e => e.Sum(f => f.Budget),
                    PieChartStyles.Default
                )
                .Toolbox()
        ;
    }
}

public class PieChart8View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Status = "Completed", Tasks = 85 },
            new { Status = "In Progress", Tasks = 45 },
            new { Status = "Pending", Tasks = 25 },
            new { Status = "Blocked", Tasks = 10 },
            new { Status = "Cancelled", Tasks = 5 },
        };

        return new Card().Title("Project Task Status")
            | data.ToPieChart
                (
                    e => e.Status,
                    e => e.Sum(f => f.Tasks),
                    PieChartStyles.Dashboard
                )
                .Toolbox()
        ;
    }
}

public class PieChart9View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Technology = "React", Developers = 45, Percentage = 30.0 },
            new { Technology = "Angular", Developers = 30, Percentage = 20.0 },
            new { Technology = "Vue.js", Developers = 25, Percentage = 16.7 },
            new { Technology = "Svelte", Developers = 20, Percentage = 13.3 },
            new { Technology = "Ember", Developers = 15, Percentage = 10.0 },
            new { Technology = "Backbone", Developers = 10, Percentage = 6.7 },
            new { Technology = "Other", Developers = 5, Percentage = 3.3 }
        };

        return new Card().Title("Frontend Framework Adoption")
            | data.ToPieChart
                (
                    e => e.Technology,
                    e => e.Sum(f => f.Developers),
                    PieChartStyles.Default
                )
                .Toolbox()
        ;
    }
}

public class PieChart10View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Category = "Food & Dining", Amount = 2500, Color = "#FF6B6B" },
            new { Category = "Transportation", Amount = 1800, Color = "#4ECDC4" },
            new { Category = "Entertainment", Amount = 1200, Color = "#45B7D1" },
            new { Category = "Shopping", Amount = 2200, Color = "#96CEB4" },
            new { Category = "Utilities", Amount = 800, Color = "#FFEAA7" },
            new { Category = "Healthcare", Amount = 600, Color = "#DDA0DD" },
            new { Category = "Education", Amount = 1500, Color = "#98D8C8" },
            new { Category = "Travel", Amount = 3000, Color = "#F7DC6F" },
            new { Category = "Savings", Amount = 2000, Color = "#BB8FCE" }
        };

        return new Card().Title("Monthly Expense Breakdown")
            | data.ToPieChart
                (
                    e => e.Category,
                    e => e.Sum(f => f.Amount),
                    PieChartStyles.Donut
                )
                .Toolbox()
        ;
    }
}

public class PieChart11View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { AgeGroup = "18-24", Users = 1200, Growth = 15.2 },
            new { AgeGroup = "25-34", Users = 3500, Growth = 22.8 },
            new { AgeGroup = "35-44", Users = 2800, Growth = 18.5 },
            new { AgeGroup = "45-54", Users = 1900, Growth = 12.3 },
            new { AgeGroup = "55-64", Users = 1200, Growth = 8.7 },
            new { AgeGroup = "65+", Users = 800, Growth = 5.2 }
        };

        return new Card().Title("User Demographics by Age Group")
            | data.ToPieChart
                (
                    e => e.AgeGroup,
                    e => e.Sum(f => f.Users),
                    PieChartStyles.Dashboard
                )
                .Toolbox()
        ;
    }
}