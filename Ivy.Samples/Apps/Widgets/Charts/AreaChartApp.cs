using Ivy.Charts;
using Ivy.Shared;
using System.Linq.Expressions;
using Ivy.Views.Charts;

namespace Ivy.Samples.Apps.Widgets.Charts;

[App(icon: Icons.ChartArea)]
public class AreaChartApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Grid().Columns(3)
            | new AreaChart0View()
            | new AreaChart1View()
            | new AreaChart2View()
            | new AreaChart3View()
            | new AreaChart4View()
            | new AreaChart5View()
        ;
    }
}

public class AreaChart0View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Desktop = 186, Mobile = 80 },
            new { Month = "Feb", Desktop = 305, Mobile = 200 },
            new { Month = "Mar", Desktop = 237, Mobile = 120 },
            new { Month = "Apr", Desktop = 73, Mobile = 190 },
            new { Month = "May", Desktop = 209, Mobile = 130 },
            new { Month = "Jun", Desktop = 214, Mobile = 140 },
        };

        return new Card().Title("Basic Area Chart")
            | data.ToAreaChart()
                .Dimension("Month", e => e.Month)
                .Measure("Desktop", e => e.Sum(f => f.Desktop))
                .Measure("Mobile", e => e.Sum(f => f.Mobile))
        ;
    }
}

public class AreaChart1View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Desktop = 186, Mobile = 80, Tablet = 45 },
            new { Month = "Feb", Desktop = 305, Mobile = 200, Tablet = 65 },
            new { Month = "Mar", Desktop = 237, Mobile = 120, Tablet = 85 },
            new { Month = "Apr", Desktop = 73, Mobile = 190, Tablet = 55 },
            new { Month = "May", Desktop = 209, Mobile = 130, Tablet = 75 },
            new { Month = "Jun", Desktop = 214, Mobile = 140, Tablet = 95 },
        };

        return new Card().Title("Multi-Series Area Chart")
            | data.ToAreaChart()
                .Dimension("Month", e => e.Month)
                .Measure("Desktop", e => e.Sum(f => f.Desktop))
                .Measure("Mobile", e => e.Sum(f => f.Mobile))
                .Measure("Tablet", e => e.Sum(f => f.Tablet))
        ;
    }
}

public class AreaChart2View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Revenue = 18600, Expenses = 12000 },
            new { Month = "Feb", Revenue = 30500, Expenses = 18000 },
            new { Month = "Mar", Revenue = 23700, Expenses = 15000 },
            new { Month = "Apr", Revenue = 7300, Expenses = 22000 },
            new { Month = "May", Revenue = 20900, Expenses = 16000 },
            new { Month = "Jun", Revenue = 21400, Expenses = 14000 },
        };

        return new Card().Title("Financial Overview")
            | data.ToAreaChart()
                .Dimension("Month", e => e.Month)
                .Measure("Revenue", e => e.Sum(f => f.Revenue))
                .Measure("Expenses", e => e.Sum(f => f.Expenses))
        ;
    }
}

public class AreaChart3View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Quarter = "Q1", ProductA = 1200, ProductB = 800, ProductC = 600 },
            new { Quarter = "Q2", ProductA = 1500, ProductB = 950, ProductC = 750 },
            new { Quarter = "Q3", ProductA = 1800, ProductB = 1100, ProductC = 900 },
            new { Quarter = "Q4", ProductA = 2100, ProductB = 1250, ProductC = 1050 },
        };

        return new Card().Title("Product Sales by Quarter")
            | data.ToAreaChart()
                .Dimension("Quarter", e => e.Quarter)
                .Measure("ProductA", e => e.Sum(f => f.ProductA))
                .Measure("ProductB", e => e.Sum(f => f.ProductB))
                .Measure("ProductC", e => e.Sum(f => f.ProductC))
        ;
    }
}

public class AreaChart4View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Year = "2020", North = 150, South = 120, East = 180, West = 90 },
            new { Year = "2021", North = 180, South = 140, East = 200, West = 110 },
            new { Year = "2022", North = 220, South = 160, East = 240, West = 130 },
            new { Year = "2023", North = 250, South = 180, East = 280, West = 150 },
            new { Year = "2024", North = 280, South = 200, East = 320, West = 170 },
        };

        return new Card().Title("Regional Growth Trends")
            | data.ToAreaChart()
                .Dimension("Year", e => e.Year)
                .Measure("North", e => e.Sum(f => f.North))
                .Measure("South", e => e.Sum(f => f.South))
                .Measure("East", e => e.Sum(f => f.East))
                .Measure("West", e => e.Sum(f => f.West))
        ;
    }
}

public class AreaChart5View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Users = 1200, Sessions = 3500, Conversions = 180 },
            new { Month = "Feb", Users = 1400, Sessions = 4200, Conversions = 220 },
            new { Month = "Mar", Users = 1600, Sessions = 4800, Conversions = 260 },
            new { Month = "Apr", Users = 1800, Sessions = 5400, Conversions = 300 },
            new { Month = "May", Users = 2000, Sessions = 6000, Conversions = 340 },
            new { Month = "Jun", Users = 2200, Sessions = 6600, Conversions = 380 },
        };

        return new Card().Title("Website Analytics")
            | data.ToAreaChart()
                .Dimension("Month", e => e.Month)
                .Measure("Users", e => e.Sum(f => f.Users))
                .Measure("Sessions", e => e.Sum(f => f.Sessions))
                .Measure("Conversions", e => e.Sum(f => f.Conversions))
        ;
    }
}