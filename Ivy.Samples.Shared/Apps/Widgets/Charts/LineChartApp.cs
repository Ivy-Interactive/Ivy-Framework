using Ivy.Charts;
using Ivy.Shared;
using Ivy.Views.Charts;

namespace Ivy.Samples.Shared.Apps.Widgets.Charts;

[App(icon: Icons.ChartLine)]
public class LineChartApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Grid().Columns(3)
            | new LineChart0View()
            | new LineChart1View()
            | new LineChart2View()
            | new LineChart3View()
            | new LineChart4View()
            | new LineChart5View()
            | new LineChartNegativeView()
            | new LineChartZeroNullView()
            | new LineChartExtremeRangeView()
            | new LineChartSinglePointView()
            | new LineChartIdenticalValuesView()
            | new LineChartMixedEdgeCasesView()
        ;
    }
}

public class LineChart0View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "January", Desktop = 186, Mobile = 100 },
            new { Month = "February", Desktop = 305, Mobile = 200 },
            new { Month = "March", Desktop = 237, Mobile = 300 },
            new { Month = "April", Desktop = 73, Mobile = 400 },
            new { Month = "May", Desktop = 209, Mobile = 30 },
            new { Month = "June", Desktop = 214, Mobile = 45 },
        };

        return new Card().Title("Basic Line Chart (Default Style)")
            | data.ToLineChart(style: LineChartStyles.Default)
                .Dimension("Month", e => e.Month)
                .Measure("Desktop", e => e.Sum(f => f.Desktop))
                .Measure("Mobile", e => e.Sum(f => f.Mobile))
        ;
    }
}

public class LineChart1View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Sales = 1200, Marketing = 800, Development = 600 },
            new { Month = "Feb", Sales = 1500, Marketing = 950, Development = 750 },
            new { Month = "Mar", Sales = 1800, Marketing = 1100, Development = 900 },
            new { Month = "Apr", Sales = 2100, Marketing = 1250, Development = 1050 },
            new { Month = "May", Sales = 2400, Marketing = 1400, Development = 1200 },
            new { Month = "Jun", Sales = 2700, Marketing = 1550, Development = 1350 },
        };

        return new Card().Title("Department Performance (Dashboard Style)")
            | data.ToLineChart(style: LineChartStyles.Dashboard)
                .Dimension("Month", e => e.Month)
                .Measure("Sales", e => e.Sum(f => f.Sales))
                .Measure("Marketing", e => e.Sum(f => f.Marketing))
                .Measure("Development", e => e.Sum(f => f.Development))
        ;
    }
}

public class LineChart2View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Year = "2020", Revenue = 50000, Profit = 15000 },
            new { Year = "2021", Revenue = 65000, Profit = 20000 },
            new { Year = "2022", Revenue = 80000, Profit = 25000 },
            new { Year = "2023", Revenue = 95000, Profit = 30000 },
            new { Year = "2024", Revenue = 110000, Profit = 35000 },
        };

        return new Card().Title("Financial Growth (Custom Style)")
            | data.ToLineChart(style: LineChartStyles.Custom)
                .Dimension("Year", e => e.Year)
                .Measure("Revenue", e => e.Sum(f => f.Revenue))
                .Measure("Profit", e => e.Sum(f => f.Profit))
        ;
    }
}

public class LineChart3View : ViewBase
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

        return new Card().Title("Product Sales Trends (Rainbow Colors)")
            | data.ToLineChart(style: LineChartStyles.Default)
                .Dimension("Quarter", e => e.Quarter)
                .Measure("ProductA", e => e.Sum(f => f.ProductA))
                .Measure("ProductB", e => e.Sum(f => f.ProductB))
                .Measure("ProductC", e => e.Sum(f => f.ProductC))
        ;
    }
}

public class LineChart4View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Users = 1200, Sessions = 1400, Conversions = 1100 },
            new { Month = "Feb", Users = 1400, Sessions = 1600, Conversions = 1300 },
            new { Month = "Mar", Users = 1600, Sessions = 1800, Conversions = 1500 },
            new { Month = "Apr", Users = 1800, Sessions = 2000, Conversions = 1700 },
            new { Month = "May", Users = 2000, Sessions = 2200, Conversions = 1900 },
            new { Month = "Jun", Users = 2200, Sessions = 2400, Conversions = 2100 },
        };

        return new Card().Title("Website Analytics (Step Lines)")
            | data.ToLineChart(style: LineChartStyles.Default)
                .Dimension("Month", e => e.Month)
                .Measure("Users", e => e.Sum(f => f.Users))
                .Measure("Sessions", e => e.Sum(f => f.Sessions))
                .Measure("Conversions", e => e.Sum(f => f.Conversions))
        ;
    }
}

public class LineChart5View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Week = "Week 1", Temperature = 22, Humidity = 65, Pressure = 25 },
            new { Week = "Week 2", Temperature = 24, Humidity = 60, Pressure = 28 },
            new { Week = "Week 3", Temperature = 26, Humidity = 55, Pressure = 30 },
            new { Week = "Week 4", Temperature = 28, Humidity = 50, Pressure = 32 },
            new { Week = "Week 5", Temperature = 30, Humidity = 45, Pressure = 35 },
            new { Week = "Week 6", Temperature = 32, Humidity = 40, Pressure = 38 },
        };

        return new Card().Title("Weather Monitoring (Mixed Styles)")
            | data.ToLineChart(style: LineChartStyles.Default)
                .Dimension("Week", e => e.Week)
                .Measure("Temperature", e => e.Sum(f => f.Temperature))
                .Measure("Humidity", e => e.Sum(f => f.Humidity))
                .Measure("Pressure", e => e.Sum(f => f.Pressure))
        ;
    }
}

// Test charts with edge cases and strange data
public class LineChartNegativeView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Day = "Mon", Profit = -500, Loss = -1200, Net = -1700 },
            new { Day = "Tue", Profit = 200, Loss = -800, Net = -600 },
            new { Day = "Wed", Profit = -300, Loss = -400, Net = -700 },
            new { Day = "Thu", Profit = 600, Loss = -200, Net = 400 },
            new { Day = "Fri", Profit = -100, Loss = -1500, Net = -1600 },
            new { Day = "Sat", Profit = 800, Loss = -100, Net = 700 },
            new { Day = "Sun", Profit = -900, Loss = -300, Net = -1200 },
        };

        return new Card().Title("Test: Negative Values")
            | data.ToLineChart(style: LineChartStyles.Default)
                .Dimension("Day", e => e.Day)
                .Measure("Profit", e => e.Sum(f => f.Profit))
                .Measure("Loss", e => e.Sum(f => f.Loss))
                .Measure("Net", e => e.Sum(f => f.Net))
        ;
    }
}

public class LineChartZeroNullView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Time = "00:00", Active = 0, Inactive = 0, Unknown = (int?)null },
            new { Time = "04:00", Active = 0, Inactive = 5, Unknown = (int?)0 },
            new { Time = "08:00", Active = (int?)null, Inactive = 0, Unknown = (int?)null },
            new { Time = "12:00", Active = 10, Inactive = (int?)null, Unknown = 0 },
            new { Time = "16:00", Active = 0, Inactive = 0, Unknown = 0 },
            new { Time = "20:00", Active = (int?)null, Inactive = (int?)null, Unknown = (int?)null },
            new { Time = "24:00", Active = 5, Inactive = 3, Unknown = 0 },
        };

        return new Card().Title("Test: Zero and Null Values")
            | data.ToLineChart(style: LineChartStyles.Dashboard)
                .Dimension("Time", e => e.Time)
                .Measure("Active", e => e.Sum(f => f.Active ?? 0))
                .Measure("Inactive", e => e.Sum(f => f.Inactive ?? 0))
                .Measure("Unknown", e => e.Sum(f => f.Unknown ?? 0))
        ;
    }
}

public class LineChartExtremeRangeView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Period = "T1", Micro = 0.0001, Normal = 100, Huge = 999999999 },
            new { Period = "T2", Micro = 0.0005, Normal = 150, Huge = 1500000000 },
            new { Period = "T3", Micro = 0.0002, Normal = 80, Huge = 800000000 },
            new { Period = "T4", Micro = 0.0008, Normal = 200, Huge = 2000000000 },
            new { Period = "T5", Micro = 0.0003, Normal = 120, Huge = 1200000000 },
            new { Period = "T6", Micro = 0.0009, Normal = 180, Huge = 1800000000 },
        };

        return new Card().Title("Test: Extreme Value Ranges")
            | data.ToLineChart(style: LineChartStyles.Custom)
                .Dimension("Period", e => e.Period)
                .Measure("Micro Values", e => e.Sum(f => f.Micro))
                .Measure("Normal Values", e => e.Sum(f => f.Normal))
                .Measure("Huge Values", e => e.Sum(f => f.Huge))
        ;
    }
}

public class LineChartSinglePointView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Label = "Only Point", Value1 = 42, Value2 = 100, Value3 = -15 }
        };

        return new Card().Title("Test: Single Data Point")
            | data.ToLineChart(style: LineChartStyles.Default)
                .Dimension("Label", e => e.Label)
                .Measure("Value1", e => e.Sum(f => f.Value1))
                .Measure("Value2", e => e.Sum(f => f.Value2))
                .Measure("Value3", e => e.Sum(f => f.Value3))
        ;
    }
}

public class LineChartIdenticalValuesView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Hour = "1h", Constant = 50, AlsoConstant = 50, StillConstant = 50 },
            new { Hour = "2h", Constant = 50, AlsoConstant = 50, StillConstant = 50 },
            new { Hour = "3h", Constant = 50, AlsoConstant = 50, StillConstant = 50 },
            new { Hour = "4h", Constant = 50, AlsoConstant = 50, StillConstant = 50 },
            new { Hour = "5h", Constant = 50, AlsoConstant = 50, StillConstant = 50 },
            new { Hour = "6h", Constant = 50, AlsoConstant = 50, StillConstant = 50 },
        };

        return new Card().Title("Test: All Identical Values")
            | data.ToLineChart(style: LineChartStyles.Dashboard)
                .Dimension("Hour", e => e.Hour)
                .Measure("Constant", e => e.Sum(f => f.Constant))
                .Measure("AlsoConstant", e => e.Sum(f => f.AlsoConstant))
                .Measure("StillConstant", e => e.Sum(f => f.StillConstant))
        ;
    }
}

public class LineChartMixedEdgeCasesView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Category = "A", Spike = 0.1, Drop = 10000, Zigzag = -500 },
            new { Category = "B", Spike = 99999, Drop = 0.001, Zigzag = 1500 },
            new { Category = "C", Spike = 0.5, Drop = 5000, Zigzag = -2000 },
            new { Category = "D", Spike = 50000, Drop = 0.1, Zigzag = 3000 },
            new { Category = "E", Spike = 1, Drop = 8000, Zigzag = -1000 },
            new { Category = "F", Spike = 75000, Drop = 0.0001, Zigzag = 500 },
            new { Category = "G", Spike = 0.01, Drop = 12000, Zigzag = -3500 },
            new { Category = "H", Spike = 100000, Drop = 1, Zigzag = 4000 },
        };

        return new Card().Title("Test: Mixed Edge Cases (Spikes, Drops, Zigzags)")
            | data.ToLineChart(style: LineChartStyles.Custom)
                .Dimension("Category", e => e.Category)
                .Measure("Spike Pattern", e => e.Sum(f => f.Spike))
                .Measure("Drop Pattern", e => e.Sum(f => f.Drop))
                .Measure("Zigzag Pattern", e => e.Sum(f => f.Zigzag))
        ;
    }
}