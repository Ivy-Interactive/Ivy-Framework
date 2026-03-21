namespace Ivy.Samples.Shared.Apps.Widgets.Charts;

[App(icon: Icons.ChartBarDecreasing, searchHints: ["visualization", "graph", "analytics", "funnel", "conversion", "pipeline", "stages"])]
public class FunnelChartApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Grid().Columns(2)
            | new FunnelChart0View()
            | new FunnelChart1View()
            | new FunnelChart2View()
            | new FunnelChart3View()
            | new FunnelChart4View()
            | new FunnelChart5View()
            | new FunnelChart6View()
            | new FunnelChart7View()
            | new FunnelChart8View()
            | new FunnelChart9View()
            | new FunnelChart10View();
    }
}

public class FunnelChart0View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new PieChartData("Awareness", 5000),
            new PieChartData("Interest", 3500),
            new PieChartData("Decision", 2100),
            new PieChartData("Action", 1200),
        };

        return new Card().Title("Sales Funnel")
            | new FunnelChart(data)
                .Funnel("Measure", "Dimension")
                .Tooltip()
                .Toolbox();
    }
}

public class FunnelChart1View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new PieChartData("Visitors", 10000),
            new PieChartData("Sign Ups", 6500),
            new PieChartData("Trial Users", 3200),
            new PieChartData("Paid Users", 1800),
            new PieChartData("Renewals", 1200),
        };

        return new Card().Title("Conversion Funnel")
            | new FunnelChart(data)
                .Funnel("Measure", "Dimension")
                .Tooltip()
                .Legend()
                .ColorScheme(ColorScheme.Rainbow);
    }
}

public class FunnelChart2View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new PieChartData("Applications", 800),
            new PieChartData("Screened", 500),
            new PieChartData("Interviewed", 250),
            new PieChartData("Offered", 80),
            new PieChartData("Hired", 45),
        };

        return new Card().Title("Recruitment Pipeline")
            | new FunnelChart(data)
                .Funnel("Measure", "Dimension")
                .Tooltip()
                .Toolbox()
                .Sort(FunnelSort.Descending);
    }
}

public class FunnelChart3View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new PieChartData("Browse", 8000),
            new PieChartData("Add to Cart", 4500),
            new PieChartData("Checkout", 2800),
            new PieChartData("Payment", 2200),
            new PieChartData("Completed", 1900),
        };

        return new Card().Title("E-commerce Checkout")
            | new FunnelChart(data)
                .Funnel("Measure", "Dimension")
                .Tooltip()
                .Orientation(FunnelOrientation.Horizontal);
    }
}

public class FunnelChart4View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new PieChartData("Impressions", 50000),
            new PieChartData("Clicks", 12000),
            new PieChartData("Leads", 3500),
            new PieChartData("Qualified", 1200),
            new PieChartData("Customers", 400),
        };

        return new Card().Title("Marketing Campaign")
            | new FunnelChart(data)
                .Funnel("Measure", "Dimension")
                .Tooltip()
                .Toolbox()
                .Legend();
    }
}

public class FunnelChart5View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new PieChartData("Awareness", 9000),
            new PieChartData("Consideration", 6000),
            new PieChartData("Preference", 3500),
            new PieChartData("Purchase", 2000),
            new PieChartData("Loyalty", 1500),
            new PieChartData("Advocacy", 800),
        };

        return new Card().Title("Customer Journey")
            | new FunnelChart(data)
                .Funnel("Measure", "Dimension")
                .Tooltip()
                .Toolbox()
                .ColorScheme(ColorScheme.Rainbow);
    }
}

public class FunnelChart6View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new PieChartData("Contacted", 200),
            new PieChartData("Responded", 450),
            new PieChartData("Qualified", 800),
            new PieChartData("Proposal Sent", 1500),
            new PieChartData("Negotiation", 2500),
        };

        return new Card().Title("Ascending Sort")
            | new FunnelChart(data)
                .Funnel("Measure", "Dimension")
                .Tooltip()
                .Sort(FunnelSort.Ascending);
    }
}

public class FunnelChart7View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new PieChartData("Total Leads", 5000),
            new PieChartData("MQL", 3200),
            new PieChartData("SQL", 1800),
            new PieChartData("Opportunity", 900),
            new PieChartData("Closed Won", 350),
        };

        return new Card().Title("Lead Qualification (with Gap)")
            | new FunnelChart(data)
                .Funnel("Measure", "Dimension")
                .Tooltip()
                .Toolbox()
                .Legend()
                .Gap(5);
    }
}

public record SalesFunnelStage(string Stage, int Count);

public class FunnelChart8View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new SalesFunnelStage("Prospects", 8000),
            new SalesFunnelStage("Qualified", 5200),
            new SalesFunnelStage("Proposals", 3100),
            new SalesFunnelStage("Negotiations", 1800),
            new SalesFunnelStage("Closed", 950),
        }.AsQueryable();

        return new Card().Title("ToFunnelChart - Default")
            | data.ToFunnelChart(
                x => x.Stage,
                x => x.Sum(s => s.Count));
    }
}

public class FunnelChart9View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new SalesFunnelStage("Visits", 12000),
            new SalesFunnelStage("Cart", 7500),
            new SalesFunnelStage("Checkout", 4200),
            new SalesFunnelStage("Payment", 3100),
            new SalesFunnelStage("Complete", 2800),
        }.AsQueryable();

        return new Card().Title("ToFunnelChart - Horizontal")
            | data.ToFunnelChart(
                x => x.Stage,
                x => x.Sum(s => s.Count),
                FunnelChartStyles.Horizontal);
    }
}

public class FunnelChart10View : ViewBase
{
    public override object? Build()
    {
        var data = new[]
        {
            new SalesFunnelStage("Awareness", 15000),
            new SalesFunnelStage("Interest", 9000),
            new SalesFunnelStage("Desire", 4500),
            new SalesFunnelStage("Action", 2000),
        }.AsQueryable();

        return new Card().Title("ToFunnelChart - Dashboard")
            | data.ToFunnelChart(
                x => x.Stage,
                x => x.Sum(s => s.Count),
                FunnelChartStyles.Dashboard);
    }
}
