using Ivy;
using MyProject.Connections.MyDb;
using Microsoft.EntityFrameworkCore;

namespace MyProject.Apps.Orders;

public class OrderEditSheet(IState<bool> isOpen, int orderId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<MyDbContextFactory>();
        var queryService = UseService<IQueryService>();

        var orderQuery = UseQuery(
            key: (typeof(Order), orderId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Orders.FirstAsync(e => e.Id == orderId, ct);
            },
            tags: [(typeof(Order), orderId)]
        );

        if (orderQuery.Loading || orderQuery.Value == null)
            return Skeleton.Form().ToSheet(isOpen, "Edit Order");

        return orderQuery.Value
            .ToForm()
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt)
            .Builder(e => e.CustomerId, e => e.ToAsyncSelectInput(UseCustomerSearch, UseCustomerLookup, placeholder: "Select Customer"))
            .OnSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Order");

        async Task OnSubmit(Order? request)
        {
            if (request == null) return;
            await using var db = factory.CreateDbContext();
            request.UpdatedAt = DateTime.UtcNow;
            db.Orders.Update(request);
            await db.SaveChangesAsync();
            queryService.RevalidateByTag((typeof(Order), orderId));
        }
    }

    private static QueryResult<Option<Guid?>[]> UseCustomerSearch(IViewContext context, string query)
    {
        var factory = context.UseService<MyDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseCustomerSearch), query),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return (await db.Customers
                        .Where(e => e.FirstName.Contains(query) || e.LastName.Contains(query))
                        .Select(e => new { e.Id, FullName = e.FirstName + " " + e.LastName })
                        .Take(50)
                        .ToArrayAsync(ct))
                    .Select(e => new Option<Guid?>(e.FullName, e.Id))
                    .ToArray();
            });
    }

    private static QueryResult<Option<Guid?>?> UseCustomerLookup(IViewContext context, Guid? id)
    {
        var factory = context.UseService<MyDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseCustomerLookup), id),
            fetcher: async ct =>
            {
                if (id == null) return null;
                await using var db = factory.CreateDbContext();
                var customer = await db.Customers.FirstOrDefaultAsync(e => e.Id == id, ct);
                if (customer == null) return null;
                return new Option<Guid?>(customer.FirstName + " " + customer.LastName, customer.Id);
            });
    }
}
