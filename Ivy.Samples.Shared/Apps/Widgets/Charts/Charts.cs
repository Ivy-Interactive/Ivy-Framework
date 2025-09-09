using Ivy.Charts;
using Ivy.Shared;
using Ivy.Views.Charts;

namespace Ivy.Samples.Shared.Apps.Widgets.Charts;

[App(icon: Icons.ChartLine)]
public class ChartsTestApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Grid().Columns(4)
            | new LineChartTestView()
            | new BarChartTestView()
            | new PieChartTestView()
            | new AreaChartTestView()
        ;
    }
}

public class LineChartTestView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Sales = 186 },
            new { Month = "Feb", Sales = 305 },
            new { Month = "Mar", Sales = 237 },
            new { Month = "Apr", Sales = 73 },
            new { Month = "May", Sales = 209 },
            new { Month = "Jun", Sales = 214 }
        };

        return new Card().Title("Line Chart")
            | data.ToLineChart(style: LineChartStyles.Default)
                .Dimension("Month", e => e.Month)
                .Measure("Sales", e => e.Sum(f => f.Sales))
        ;
    }
}
public class AreaChartTestView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Sales = 186 },
            new { Month = "Feb", Sales = 305 },
            new { Month = "Mar", Sales = 237 },
            new { Month = "Apr", Sales = 73 },
            new { Month = "May", Sales = 209 },
            new { Month = "Jun", Sales = 214 }
        };

        return new Card().Title("Area Chart")
            | data.ToAreaChart()
                .Dimension("Month", e => e.Month)
                .Measure("Sales", e => e.Sum(f => f.Sales))
        ;
    }
}

public class BarChartTestView : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Month = "Jan", Sales = 186 },
            new { Month = "Feb", Sales = 305 },
            new { Month = "Mar", Sales = 237 },
            new { Month = "Apr", Sales = 73 },
            new { Month = "May", Sales = 209 },
            new { Month = "Jun", Sales = 214 }
        };

        return new Card().Title("Bar Chart")
            | data.ToBarChart(style: BarChartStyles.Default)
                .Dimension("Month", e => e.Month)
                .Measure("Sales", e => e.Sum(f => f.Sales))
        ;
    }
}

public class PieChartTestView : ViewBase
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

        return new Card().Title("Pie Chart")
            | data.ToPieChart
                (
                    e => e.Category,
                    e => e.Sum(f => f.Value),
                    PieChartStyles.Default
                )
        ;
    }
}
