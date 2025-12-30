using Ivy.Charts;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Charts;

[App(icon: Icons.ChartBarStacked, searchHints: ["visualization", "graph", "analytics", "data", "comparison", "statistics"])]
public class BarChartApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Grid().Columns(3)
            | new BarChart0()
            | new BarChart1()
            | new BarChart2()
            | new BarChart3()
            | new BarChart4()
            | new BarChart5()
        ;
    }
}

public class BarChart0 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Desktop = 186 },
            new { Month = "Feb", Desktop = 305 },
            new { Month = "Mar", Desktop = 237},
            new { Month = "Apr", Desktop = 73 },
            new { Month = "May", Desktop = 209 },
            new { Month = "Jun", Desktop = 214 }
        };

        return new Card().Title("Desktop Usage by Month")
            | new BarChart(data)
                .ColorScheme(ColorScheme.Default)
                .Bar(new Bar("Desktop", 1).Radius(8))
                .CartesianGrid(new CartesianGrid().Horizontal())
                .Tooltip()
                .XAxis(new XAxis("Month").TickLine(false).AxisLine(false))
                .Legend()
        ;
    }
}

public class BarChart1 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Desktop = 186, Mobile = (int?)100 },
            new { Month = "Feb", Desktop = 305, Mobile = (int?)200 },
            new { Month = "Mar", Desktop = 237, Mobile = (int?)300 },
            new { Month = "Apr", Desktop = 73, Mobile = (int?)200 },
            new { Month = "May", Desktop = 209, Mobile = (int?)30 },
            new { Month = "Jun", Desktop = 214, Mobile = (int?)0 },
        };

        return new Card().Title("Desktop vs Mobile Usage")
            | new BarChart(data)
                .ColorScheme(ColorScheme.Default)
                .Bar(new Bar("Mobile", 1).Radius(0, 8).LegendType(LegendTypes.Square))
                .Bar(new Bar("Desktop", 1).Radius(8, 0).LegendType(LegendTypes.Square))
                .CartesianGrid(new CartesianGrid().Horizontal())
                .Tooltip()
                .XAxis(new XAxis("Month").TickLine(false).AxisLine(false))
                .Legend()
        ;
    }
}

public class BarChart2 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Desktop = 186, Mobile = (int?)100 },
            new { Month = "Feb", Desktop = 305, Mobile = (int?)200 },
            new { Month = "Mar", Desktop = 237, Mobile = (int?)300 },
            new { Month = "Apr", Desktop = 73, Mobile = (int?)200 },
            new { Month = "May", Desktop = 209, Mobile = (int?)30 },
            new { Month = "Jun", Desktop = 214, Mobile = (int?)0 },
        };

        return new Card().Title("Device Usage Comparison")
            | new BarChart(data)
                .ColorScheme(ColorScheme.Default)
                .Bar(new Bar("Mobile", 1).Radius(8).LegendType(LegendTypes.Square))
                .Bar(new Bar("Desktop", 2).Radius(8).LegendType(LegendTypes.Square))
                .CartesianGrid(new CartesianGrid().Horizontal())
                .Tooltip()
                .XAxis(new XAxis("Month").TickLine(false).AxisLine(false))
                .Legend()
        ;
    }
}

public class BarChart3 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Desktop = 186, Mobile = (int?)100 },
            new { Month = "Feb", Desktop = 305, Mobile = (int?)200 },
            new { Month = "Mar", Desktop = 237, Mobile = (int?)300 },
            new { Month = "Apr", Desktop = 73, Mobile = (int?)200 },
            new { Month = "May", Desktop = 209, Mobile = (int?)30 },
            new { Month = "Jun", Desktop = 214, Mobile = (int?)0 },
        };

        return new Card().Title("Horizontal Desktop Usage")
            | new BarChart(data)
                .Vertical()
                .ColorScheme(ColorScheme.Default)
                .Bar(new Bar("Desktop", 1).Radius(4).LegendType(LegendTypes.Square)
                    .LabelList(new LabelList("Month").Fill(Colors.White).Position(Positions.InsideLeft).Offset(8).FontSize(12))
                    .LabelList(new LabelList("Desktop").Fill(Colors.Black).Position(Positions.Right).Offset(8).FontSize(12))
                )
                .CartesianGrid(new CartesianGrid().Vertical())
                .Tooltip()
                .YAxis(new YAxis("Month").TickLine(false).AxisLine(false).Type(AxisTypes.Category).Hide())
                .XAxis(new XAxis("Desktop").Type(AxisTypes.Number).Hide())
                .Toolbox()
        ;
    }
}

public class BarChart4 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Quarter = "Q1", Revenue = 45000, Expenses = 32000, Profit = 13000 },
            new { Quarter = "Q2", Revenue = 52000, Expenses = 35000, Profit = 17000 },
            new { Quarter = "Q3", Revenue = 48000, Expenses = 33000, Profit = 15000 },
            new { Quarter = "Q4", Revenue = 61000, Expenses = 38000, Profit = 23000 },
        };

        return new Card().Title("Financial Performance by Quarter")
            | new BarChart(data)
                .ColorScheme(ColorScheme.Default)
                .Bar(new Bar("Revenue", 1).Radius(8).LegendType(LegendTypes.Square))
                .Bar(new Bar("Expenses", 1).Radius(8).LegendType(LegendTypes.Square))
                .Bar(new Bar("Profit", 1).Radius(8).LegendType(LegendTypes.Square))
                .CartesianGrid(new CartesianGrid().Horizontal())
                .Tooltip()
                .XAxis(new XAxis("Quarter").TickLine(false).AxisLine(false))
                .Legend()
                .Toolbox()
        ;
    }
}

public class BarChart5 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Category = "Frontend", Developers = 8, Designers = 3, QA = 2 },
            new { Category = "Backend", Developers = 12, Designers = 1, QA = 4 },
            new { Category = "DevOps", Developers = 4, Designers = 0, QA = 1 },
            new { Category = "Mobile", Developers = 6, Designers = 2, QA = 2 },
            new { Category = "Data", Developers = 5, Designers = 1, QA = 1 },
        };

        return new Card().Title("Team Distribution by Department")
            | new BarChart(data)
                .ColorScheme(ColorScheme.Default)
                .Bar(new Bar("Developers", 1).Radius(8).LegendType(LegendTypes.Square))
                .Bar(new Bar("Designers", 1).Radius(8).LegendType(LegendTypes.Square))
                .Bar(new Bar("QA", 1).Radius(8).LegendType(LegendTypes.Square))
                .CartesianGrid(new CartesianGrid().Horizontal())
                .Tooltip()
                .XAxis(new XAxis("Category").TickLine(false).AxisLine(false))
                .Legend()
                .Toolbox()
        ;
    }
}