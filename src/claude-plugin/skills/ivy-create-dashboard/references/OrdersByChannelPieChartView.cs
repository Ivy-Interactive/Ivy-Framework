using Ivy;
using MyProject.Connections.MyDb;
using Microsoft.EntityFrameworkCore;

namespace MyProject.Apps.Dashboard;

public class OrdersByChannelPieChartView(DateTime fromDate, DateTime toDate) : ViewBase
{
    private record ChartData(string Channel);

    public override object? Build()
    {
        var query = UseChartData(Context);
        var card = new Card().Title("Orders by Channel").Height(Size.Units(80));

        if (query.Error != null)
        {
            return card | new ErrorTeaserView(query.Error);
        }

        if (query.Loading || query.Value == null)
        {
            return card | new Skeleton();
        }

        var totalOrders = query.Value.Length;
        PieChartTotal total = new(Format.Number(@"[<1000]0;[<10000]0.0,""K"";0,""K""", totalOrders), "Orders");

        // ToPieChart(dimension, measure, style?, total?)
        var chart = query.Value.ToPieChart(
            dimension: e => e.Channel,
            measure: e => e.Count(),
            PieChartStyles.Dashboard,
            total);

        return card | chart;
    }

    private QueryResult<ChartData[]> UseChartData(IViewContext context)
    {
        var factory = context.UseService<MyDbContextFactory>();

        return context.UseQuery(
            key: (nameof(OrdersByChannelPieChartView), fromDate, toDate),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                // IMPORTANT: Use anonymous types in Select() for EF Core compatibility.
                // Positional record constructors cannot be translated to SQL.
                var raw = await db.Orders
                    .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
                    .Select(e => new { e.Channel })
                    .ToArrayAsync(ct);
                var data = raw.Select(x => new ChartData(x.Channel)).ToArray();

                return data;
            },
            options: new QueryOptions { Expiration = TimeSpan.FromMinutes(5) }
        );
    }
}
