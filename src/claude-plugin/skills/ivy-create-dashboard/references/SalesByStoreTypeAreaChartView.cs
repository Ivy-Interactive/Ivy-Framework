using Ivy;
using MyProject.Connections.MyDb;
using Microsoft.EntityFrameworkCore;

namespace MyProject.Apps.Dashboard;

public class SalesByStoreTypeAreaChartView(DateTime fromDate, DateTime toDate) : ViewBase
{
    private record ChartData(string Date, decimal Amount, string StoreType);

    public override object? Build()
    {
        var query = UseChartData(Context);
        var card = new Card().Title("Sales by Store Type").Height(Size.Units(80));

        if (query.Error != null)
        {
            return card | new ErrorTeaserView(query.Error);
        }

        if (query.Loading || query.Value == null)
        {
            return card | new Skeleton();
        }

        // ToAreaChart(dimension, measures[], style?)
        var chart = query.Value.ToAreaChart(
            e => e.Date,
            [
                e => e.Where(f => f.StoreType == "Online").Sum(f => f.Amount),
                e => e.Where(f => f.StoreType == "Retail").Sum(f => f.Amount),
                e => e.Where(f => f.StoreType == "Wholesale").Sum(f => f.Amount),
            ],
            AreaChartStyles.Dashboard);

        return card | chart;
    }

    private QueryResult<ChartData[]> UseChartData(IViewContext context)
    {
        var factory = context.UseService<MyDbContextFactory>();

        return context.UseQuery(
            key: (nameof(SalesByStoreTypeAreaChartView), fromDate, toDate),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                // IMPORTANT: Use anonymous types in Select() for EF Core compatibility.
                // Positional record constructors cannot be translated to SQL.
                var raw = await db.Orders
                    .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
                    .OrderBy(o => o.CreatedAt)
                    .Select(e => new
                    {
                        Date = e.CreatedAt.Date.ToString("d MMM"),
                        Amount = e.Lines.Sum(l => (l.Price - l.Discount) * l.Quantity),
                        e.StoreType
                    })
                    .ToArrayAsync(ct);
                var data = raw.Select(x => new ChartData(x.Date, x.Amount, x.StoreType)).ToArray();

                return data;
            },
            options: new QueryOptions { Expiration = TimeSpan.FromMinutes(5) }
        );
    }
}
