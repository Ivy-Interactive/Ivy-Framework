
namespace Ivy.Samples.Shared.Apps.Widgets.Charts;

[App(icon: Icons.Radar, searchHints: ["visualization", "graph", "analytics", "data", "radar", "spider", "multi-dimensional", "comparison", "profile"])]
public class RadarChartApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Grid().Columns(3)
            | new RadarChart0()
            | new RadarChart1()
            | new RadarChart2()
            | new RadarChart3()
            | new RadarChart4()
            | new RadarChart5()
            | new RadarChart6()
            | new RadarChart7()
            | new RadarChart8()
        ;
    }
}

public class RadarChart0 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { name = "Product A", Sales = 85, Marketing = 72, Development = 90, Support = 78, Quality = 88 }
        };

        return new Card().Title("Basic Radar Chart")
            | new RadarChart(data)
                .ColorScheme(ColorScheme.Default)
                .Indicator("Sales", 100)
                .Indicator("Marketing", 100)
                .Indicator("Development", 100)
                .Indicator("Support", 100)
                .Indicator("Quality", 100)
                .Radar("values")
                .Tooltip()
                .Legend()
        ;
    }
}

public class RadarChart1 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { name = "Candidate A", Technical = 90, Communication = 75, Leadership = 80, ProblemSolving = 85, Teamwork = 88 }
        };

        return new Card().Title("Skill Assessment")
            | new RadarChart(data)
                .ColorScheme(ColorScheme.Default)
                .Indicator("Technical", 100)
                .Indicator("Communication", 100)
                .Indicator("Leadership", 100)
                .Indicator("ProblemSolving", 100)
                .Indicator("Teamwork", 100)
                .Radar(new Radar("values").Filled())
                .Shape(RadarShape.Circle)
                .Tooltip()
        ;
    }
}

public class RadarChart2 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { name = "System", Performance = 88, Scalability = 82, Security = 95, Maintainability = 78, Documentation = 72, Testing = 85 }
        };

        return new Card().Title("System Quality Metrics")
            | new RadarChart(data)
                .ColorScheme(ColorScheme.Default)
                .Indicator("Performance", 100)
                .Indicator("Scalability", 100)
                .Indicator("Security", 100)
                .Indicator("Maintainability", 100)
                .Indicator("Documentation", 100)
                .Indicator("Testing", 100)
                .Radar(new Radar("values").Fill(Colors.Primary).Filled())
                .SplitArea(true)
                .Tooltip()
                .Legend()
        ;
    }
}

public class RadarChart3 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { name = "Scores", EaseOfUse = 78, Performance = 85, Reliability = 92, Features = 70, Support = 88 }
        };

        return new Card().Title("Product Scorecard")
            | new RadarChart(data)
                .ColorScheme(ColorScheme.Default)
                .Indicator("EaseOfUse", 100)
                .Indicator("Performance", 100)
                .Indicator("Reliability", 100)
                .Indicator("Features", 100)
                .Indicator("Support", 100)
                .Radar(new Radar("values").Stroke(Colors.Primary))
                .Shape(RadarShape.Polygon)
                .Tooltip()
                .Legend()
                .Toolbox()
        ;
    }
}

public class RadarChart4 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { name = "Alice", Frontend = 90, Backend = 75, DevOps = 60, Testing = 85, Design = 70 }
        };

        return new Card().Title("Team Skills Distribution")
            | new RadarChart(data)
                .ColorScheme(ColorScheme.Default)
                .Indicator("Frontend", 100)
                .Indicator("Backend", 100)
                .Indicator("DevOps", 100)
                .Indicator("Testing", 100)
                .Indicator("Design", 100)
                .Radar(new Radar("values").ShowSymbol(true))
                .SplitLine(true)
                .Tooltip()
                .Toolbox()
        ;
    }
}

public class RadarChart5 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { name = "Satisfaction", ProductQuality = 88, CustomerService = 92, ValueForMoney = 80, DeliverySpeed = 95, Communication = 88 }
        };

        return new Card().Title("Customer Satisfaction")
            | new RadarChart(data)
                .ColorScheme(ColorScheme.Default)
                .Indicator("ProductQuality", 100)
                .Indicator("CustomerService", 100)
                .Indicator("ValueForMoney", 100)
                .Indicator("DeliverySpeed", 100)
                .Indicator("Communication", 100)
                .Radar(new Radar("values").Stroke(Colors.Success).Fill(Colors.Success).Filled())
                .Shape(RadarShape.Circle)
                .SplitArea(true)
                .Tooltip()
                .Legend()
                .Toolbox()
        ;
    }
}

public class RadarChart6 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Department = "Engineering", Speed = 85, Quality = 92, Innovation = 78, Collaboration = 88, Delivery = 80 },
            new { Department = "Marketing", Speed = 70, Quality = 75, Innovation = 90, Collaboration = 85, Delivery = 72 },
            new { Department = "Sales", Speed = 90, Quality = 68, Innovation = 65, Collaboration = 92, Delivery = 95 },
        };

        return new Card().Title("ToRadarChart - Default")
            | data.ToRadarChart(
                x => x.Department,
                [
                    q => q.Sum(x => x.Speed),
                    q => q.Sum(x => x.Quality),
                    q => q.Sum(x => x.Innovation),
                    q => q.Sum(x => x.Collaboration),
                    q => q.Sum(x => x.Delivery),
                ]
            )
        ;
    }
}

public class RadarChart7 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Department = "Engineering", Speed = 85, Quality = 92, Innovation = 78, Collaboration = 88, Delivery = 80 },
            new { Department = "Marketing", Speed = 70, Quality = 75, Innovation = 90, Collaboration = 85, Delivery = 72 },
            new { Department = "Sales", Speed = 90, Quality = 68, Innovation = 65, Collaboration = 92, Delivery = 95 },
        };

        return new Card().Title("ToRadarChart - Circle")
            | data.ToRadarChart(
                x => x.Department,
                [
                    q => q.Sum(x => x.Speed),
                    q => q.Sum(x => x.Quality),
                    q => q.Sum(x => x.Innovation),
                    q => q.Sum(x => x.Collaboration),
                    q => q.Sum(x => x.Delivery),
                ],
                RadarChartStyles.Circle
            )
        ;
    }
}

public class RadarChart8 : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new { Department = "Engineering", Speed = 85, Quality = 92, Innovation = 78, Collaboration = 88, Delivery = 80 },
            new { Department = "Marketing", Speed = 70, Quality = 75, Innovation = 90, Collaboration = 85, Delivery = 72 },
            new { Department = "Sales", Speed = 90, Quality = 68, Innovation = 65, Collaboration = 92, Delivery = 95 },
        };

        return new Card().Title("ToRadarChart - Dashboard")
            | data.ToRadarChart(
                x => x.Department,
                [
                    q => q.Sum(x => x.Speed),
                    q => q.Sum(x => x.Quality),
                    q => q.Sum(x => x.Innovation),
                    q => q.Sum(x => x.Collaboration),
                    q => q.Sum(x => x.Delivery),
                ],
                RadarChartStyles.Dashboard,
                polish: chart => chart.Toolbox()
            )
        ;
    }
}
