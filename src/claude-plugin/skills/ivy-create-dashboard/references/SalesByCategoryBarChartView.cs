using Ivy;
using MyProject.Connections.MyDb;
using Microsoft.EntityFrameworkCore;

namespace MyProject.Apps.Dashboard;

public class SalesByCategoryBarChartView(DateTime fromDate, DateTime toDate) : ViewBase
{
    private record ChartData(string Name, double Revenue);

    public override object? Build()
    {
        var query = UseChartData(Context);
        var card = new Card().Title("Sales by Category").Height(Size.Units(80));

        if (query.Error != null)
        {
            return card | new ErrorTeaserView(query.Error);
        }

        if (query.Loading || query.Value == null)
        {
            return card | new Skeleton();
        }

        // ToBarChart(dimension, measures[], style?)
        var chart = query.Value.ToBarChart(e => e.Name, [e => e.Sum(f => f.Revenue)], BarChartStyles.Dashboard);

        return card | chart;
    }

    private QueryResult<ChartData[]> UseChartData(IViewContext context)
    {
        var factory = context.UseService<MyDbContextFactory>();

        return context.UseQuery(
            key: (nameof(SalesByCategoryBarChartView), fromDate, toDate),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                // IMPORTANT: Use anonymous types in Select() for EF Core compatibility.
                // Positional record constructors cannot be translated to SQL.
                var raw = await db.Categories
                    .Select(cat => new
                    {
                        cat.Name,
                        Revenue = cat.Products
                            .SelectMany(p => p.Lines)
                            .Where(l => l.Order.CreatedAt >= fromDate && l.Order.CreatedAt <= toDate)
                            .Sum(l => (double)(l.Quantity * l.Price))
                    })
                    .ToArrayAsync(ct);
                var data = raw.Select(x => new ChartData(x.Name, x.Revenue)).ToArray();

                return data;
            },
            options: new QueryOptions { Expiration = TimeSpan.FromMinutes(5) }
        );
    }
}
