using Ivy.Charts;
using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Charts;

[App(icon: Icons.ChartBarStacked)]
public class BarChartApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Grid().Columns(3)
            | new BarChart0()
            | new BarChart1()
            | new BarChart2()
            | new BarChart3()
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

        return new Card(
                new BarChart(data)
                    .ColorScheme(ColorScheme.Default)
                    .Bar(new Bar("Desktop", 1).Radius(8))
                    .CartesianGrid(new CartesianGrid().Horizontal())
                    .Tooltip()
                    .XAxis(new XAxis("Month").TickLine(false).AxisLine(false))
                    .Legend()
            )
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

        return new Card(
                new BarChart(data)
                    .ColorScheme(ColorScheme.Default)
                    .Bar(new Bar("Mobile", 1).Radius(0, 8).LegendType(LegendTypes.Square))
                    .Bar(new Bar("Desktop", 1).Radius(8, 0).LegendType(LegendTypes.Square))
                    .CartesianGrid(new CartesianGrid().Horizontal())
                    .Tooltip()
                    .XAxis(new XAxis("Month").TickLine(false).AxisLine(false))
                    .Legend()
            )
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

        return new Card(
                new BarChart(data)
                    .ColorScheme(ColorScheme.Default)
                    .Bar(new Bar("Mobile", 1).Radius(8).LegendType(LegendTypes.Square))
                    .Bar(new Bar("Desktop", 2).Radius(8).LegendType(LegendTypes.Square))
                    .CartesianGrid(new CartesianGrid().Horizontal())
                    .Tooltip()
                    .XAxis(new XAxis("Month").TickLine(false).AxisLine(false))
                    .Legend()
            )
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

        return new Card(
                new BarChart(data)
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
            )
            ;
    }
}