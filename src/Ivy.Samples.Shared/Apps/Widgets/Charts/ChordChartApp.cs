namespace Ivy.Samples.Shared.Apps.Widgets.Charts;

[App(icon: Icons.ChartNetwork, searchHints: ["visualization", "graph", "analytics", "data", "relationships", "connections", "flows", "network"])]
public class ChordChartApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Grid().Columns(3)
            | new ChordChart0View()
            | new ChordChart1View()
            | new ChordChart2View()
            | new ChordChart3View()
            | new ChordChart4View()
            | new ChordChart5View()
            | new ChordChart6View()
            | new ChordChart7View()
            | new ChordChart8View();
    }
}

public class ChordChart0View : ViewBase
{
    public override object? Build()
    {
        var data = new ChordData(
            Nodes: new[]
            {
                new ChordNode("North America"),
                new ChordNode("Europe"),
                new ChordNode("Asia"),
                new ChordNode("South America"),
                new ChordNode("Africa"),
            },
            Links: new[]
            {
                new ChordLink(0, 1, 1200),
                new ChordLink(0, 2, 900),
                new ChordLink(0, 3, 400),
                new ChordLink(1, 2, 800),
                new ChordLink(1, 3, 350),
                new ChordLink(1, 4, 200),
                new ChordLink(2, 3, 300),
                new ChordLink(2, 4, 250),
                new ChordLink(3, 4, 150),
            }
        );

        return new Card().Title("Migration Flows")
            | new ChordChart(data).Tooltip().Toolbox();
    }
}

public class ChordChart1View : ViewBase
{
    public override object? Build()
    {
        var data = new ChordData(
            Nodes: new[]
            {
                new ChordNode("USA"),
                new ChordNode("China"),
                new ChordNode("Germany"),
                new ChordNode("Japan"),
                new ChordNode("UK"),
                new ChordNode("France"),
            },
            Links: new[]
            {
                new ChordLink(0, 1, 5800),
                new ChordLink(0, 2, 2100),
                new ChordLink(0, 3, 3400),
                new ChordLink(0, 4, 1500),
                new ChordLink(0, 5, 900),
                new ChordLink(1, 2, 1800),
                new ChordLink(1, 3, 3200),
                new ChordLink(1, 4, 1100),
                new ChordLink(2, 3, 800),
                new ChordLink(2, 4, 1200),
                new ChordLink(2, 5, 1600),
                new ChordLink(3, 4, 600),
                new ChordLink(4, 5, 1400),
            }
        );

        return new Card().Title("Trade Relationships")
            | new ChordChart(data)
                .Tooltip()
                .Toolbox()
                .ColorScheme(ColorScheme.Rainbow);
    }
}

public class ChordChart2View : ViewBase
{
    public override object? Build()
    {
        var data = new ChordData(
            Nodes: new[]
            {
                new ChordNode("Web Server"),
                new ChordNode("App Server"),
                new ChordNode("Database"),
                new ChordNode("Cache"),
                new ChordNode("CDN"),
                new ChordNode("API Gateway"),
            },
            Links: new[]
            {
                new ChordLink(0, 1, 4500),
                new ChordLink(0, 4, 2000),
                new ChordLink(0, 5, 3000),
                new ChordLink(1, 2, 3500),
                new ChordLink(1, 3, 2800),
                new ChordLink(1, 5, 1500),
                new ChordLink(2, 3, 1200),
                new ChordLink(5, 1, 2500),
                new ChordLink(5, 2, 800),
            }
        );

        return new Card().Title("Network Traffic")
            | new ChordChart(data)
                .Tooltip()
                .Legend();
    }
}

public class ChordChart3View : ViewBase
{
    public override object? Build()
    {
        var data = new ChordData(
            Nodes: new[]
            {
                new ChordNode("Engineering"),
                new ChordNode("Design"),
                new ChordNode("Marketing"),
                new ChordNode("Sales"),
                new ChordNode("Support"),
                new ChordNode("Product"),
            },
            Links: new[]
            {
                new ChordLink(0, 1, 85),
                new ChordLink(0, 5, 120),
                new ChordLink(0, 4, 45),
                new ChordLink(1, 2, 60),
                new ChordLink(1, 5, 95),
                new ChordLink(2, 3, 110),
                new ChordLink(2, 5, 70),
                new ChordLink(3, 4, 90),
                new ChordLink(3, 5, 55),
                new ChordLink(4, 0, 30),
                new ChordLink(4, 5, 40),
            }
        );

        return new Card().Title("Department Collaboration")
            | new ChordChart(data)
                .Tooltip()
                .Toolbox()
                .Sort();
    }
}

public class ChordChart4View : ViewBase
{
    public override object? Build()
    {
        var data = new ChordData(
            Nodes: new[]
            {
                new ChordNode("React"),
                new ChordNode("Vue"),
                new ChordNode("Angular"),
                new ChordNode("Svelte"),
                new ChordNode("Next.js"),
            },
            Links: new[]
            {
                new ChordLink(0, 1, 250),
                new ChordLink(0, 2, 180),
                new ChordLink(0, 3, 120),
                new ChordLink(0, 4, 400),
                new ChordLink(1, 2, 90),
                new ChordLink(1, 3, 70),
                new ChordLink(2, 3, 60),
                new ChordLink(3, 4, 50),
            }
        );

        return new Card().Title("Developer Migration")
            | new ChordChart(data)
                .Tooltip()
                .ColorScheme(ColorScheme.Rainbow)
                .PadAngle(5);
    }
}

public class ChordChart5View : ViewBase
{
    public override object? Build()
    {
        var data = new ChordData(
            Nodes: new[]
            {
                new ChordNode("Email"),
                new ChordNode("Social Media"),
                new ChordNode("Search"),
                new ChordNode("Direct"),
                new ChordNode("Referral"),
                new ChordNode("Paid Ads"),
                new ChordNode("Landing Page"),
                new ChordNode("Sign Up"),
            },
            Links: new[]
            {
                new ChordLink(0, 6, 3200),
                new ChordLink(1, 6, 2800),
                new ChordLink(2, 6, 4500),
                new ChordLink(3, 6, 1500),
                new ChordLink(4, 6, 900),
                new ChordLink(5, 6, 3800),
                new ChordLink(6, 7, 8000),
            }
        );

        return new Card().Title("Marketing Channel Attribution")
            | new ChordChart(data)
                .Tooltip()
                .Toolbox()
                .Legend()
                .SortSubGroups();
    }
}

public record TradeFlow(string Exporter, string Importer, double Volume);

public class ChordChart6View : ViewBase
{
    public override object? Build()
    {
        var trades = new[]
        {
            new TradeFlow("USA", "China", 5800),
            new TradeFlow("USA", "Germany", 2100),
            new TradeFlow("USA", "Japan", 3400),
            new TradeFlow("China", "Germany", 1800),
            new TradeFlow("China", "Japan", 3200),
            new TradeFlow("Germany", "Japan", 800),
            new TradeFlow("Germany", "France", 1600),
            new TradeFlow("UK", "France", 1400),
        };

        return new Card().Title("ToChordChart - Default")
            | trades.ToChordChart(
                t => t.Exporter,
                t => t.Importer,
                t => t.Volume);
    }
}

public class ChordChart7View : ViewBase
{
    public override object? Build()
    {
        var trades = new[]
        {
            new TradeFlow("USA", "China", 5800),
            new TradeFlow("USA", "Germany", 2100),
            new TradeFlow("China", "Germany", 1800),
            new TradeFlow("China", "Japan", 3200),
            new TradeFlow("Germany", "France", 1600),
            new TradeFlow("UK", "France", 1400),
        };

        return new Card().Title("ToChordChart - Sorted")
            | trades.ToChordChart(
                t => t.Exporter,
                t => t.Importer,
                t => t.Volume,
                ChordChartStyles.Sorted);
    }
}

public class ChordChart8View : ViewBase
{
    public override object? Build()
    {
        var trades = new[]
        {
            new TradeFlow("USA", "China", 5800),
            new TradeFlow("USA", "Germany", 2100),
            new TradeFlow("China", "Germany", 1800),
            new TradeFlow("China", "Japan", 3200),
            new TradeFlow("Germany", "France", 1600),
        };

        return new Card().Title("ToChordChart - Dashboard")
            | trades.ToChordChart(
                t => t.Exporter,
                t => t.Importer,
                t => t.Volume,
                ChordChartStyles.Dashboard);
    }
}
