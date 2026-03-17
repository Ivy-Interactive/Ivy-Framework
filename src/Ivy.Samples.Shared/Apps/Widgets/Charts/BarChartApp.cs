
namespace Ivy.Samples.Shared.Apps.Widgets.Charts;

[App(icon: Icons.ChartBarStacked, searchHints: ["visualization", "graph", "analytics", "data", "comparison", "statistics"])]
public class BarChartApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Grid().Columns(3)
            | new BarChart0()
            | new BarChart1()
            | new BarChart2()
            | new BarChart3()
            | new BarChart4()
            | new BarChart5()
            | new BarChart9()
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
            | data.ToBarChart()
                .Dimension("Month", e => e.Month)
                .Measure("Desktop", e => e.Sum(f => f.Desktop))
                .Toolbox()
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
            | data.ToBarChart()
                .Dimension("Month", e => e.Month)
                .Measure("Mobile", e => e.Sum(f => f.Mobile))
                .Measure("Desktop", e => e.Sum(f => f.Desktop))
                .Toolbox()
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
            | data.ToBarChart()
                .Dimension("Month", e => e.Month)
                .Measure("Mobile", e => e.Sum(f => f.Mobile))
                .Measure("Desktop", e => e.Sum(f => f.Desktop))
                .Toolbox()
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
            | data.ToBarChart()
                .Dimension("Quarter", e => e.Quarter)
                .Measure("Revenue", e => e.Sum(f => f.Revenue))
                .Measure("Expenses", e => e.Sum(f => f.Expenses))
                .Measure("Profit", e => e.Sum(f => f.Profit))
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
            | data.ToBarChart()
                .Dimension("Category", e => e.Category)
                .Measure("Developers", e => e.Sum(f => f.Developers))
                .Measure("Designers", e => e.Sum(f => f.Designers))
                .Measure("QA", e => e.Sum(f => f.QA))
                .Toolbox()
        ;
    }
}

public class BarChart9 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Revenue = 4200, Expenses = 3100 },
            new { Month = "Feb", Revenue = 4800, Expenses = 3400 },
            new { Month = "Mar", Revenue = 5100, Expenses = 3600 },
            new { Month = "Apr", Revenue = 4700, Expenses = 3200 },
            new { Month = "May", Revenue = 5300, Expenses = 3800 },
            new { Month = "Jun", Revenue = 5900, Expenses = 4000 },
        };

        return new Card().Title("Custom Grid Line Color (Stroke)")
            | new BarChart(data)
                .ColorScheme(ColorScheme.Default)
                .Bar(new Bar("Revenue", 1).Radius(8).LegendType(LegendTypes.Square))
                .Bar(new Bar("Expenses", 2).Radius(8).LegendType(LegendTypes.Square))
                .CartesianGrid(new CartesianGrid().Horizontal().Stroke(Colors.Red))
                .Tooltip()
                .XAxis(new XAxis("Month").TickLine(false).AxisLine(false))
                .Legend()
        ;
    }
}
