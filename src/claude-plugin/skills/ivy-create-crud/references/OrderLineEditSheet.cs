using Ivy;
using MyProject.Connections.MyDb;

using Microsoft.EntityFrameworkCore;

namespace MyProject.Apps.Orders;

public class OrderLineEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int lineId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<MyDbContextFactory>();
        var queryService = UseService<IQueryService>();

        var lineQuery = UseQuery(
            key: (typeof(Line), lineId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Lines.FirstAsync(e => e.Id == lineId, ct);
            },
            tags: [(typeof(Line), lineId)]
        );

        if (lineQuery.Loading || lineQuery.Value == null)
            return Skeleton.Form().ToSheet(isOpen, "Edit Order Line");

        return lineQuery.Value
            .ToForm()
            .Builder(e => e.ProductId, e => e.ToAsyncSelectInput(UseProductSearch, UseProductLookup, placeholder: "Select Product"))
            .Place(e => e.Quantity, e => e.Price) // Place quantity and price side by side
            .Remove(e => e.Id, e => e.OrderId) // Remove fields that shouldn't be editable
            .OnSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Order Line");

        async Task OnSubmit(Line? request)
        {
            if (request == null) return;
            await using var db = factory.CreateDbContext();
            db.Lines.Update(request);
            await db.SaveChangesAsync();
            queryService.RevalidateByTag((typeof(Line), lineId));
            queryService.RevalidateByTag(typeof(Line[]));
            refreshToken.Refresh();
        }
    }

    private static QueryResult<Option<int?>[]> UseProductSearch(IViewContext context, string query)
    {
        var factory = context.UseService<MyDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseProductSearch), query),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return (await db.Products
                        .Where(e => e.Name.Contains(query))
                        .Select(e => new { e.Id, e.Name })
                        .Take(50)
                        .ToArrayAsync(ct))
                    .Select(e => new Option<int?>(e.Name, e.Id))
                    .ToArray();
            });
    }

    private static QueryResult<Option<int?>?> UseProductLookup(IViewContext context, int? id)
    {
        var factory = context.UseService<MyDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseProductLookup), id),
            fetcher: async ct =>
            {
                if (id == null) return null;
                await using var db = factory.CreateDbContext();
                var product = await db.Products.FirstOrDefaultAsync(e => e.Id == id, ct);
                if (product == null) return null;
                return new Option<int?>(product.Name, product.Id);
            });
    }
}
