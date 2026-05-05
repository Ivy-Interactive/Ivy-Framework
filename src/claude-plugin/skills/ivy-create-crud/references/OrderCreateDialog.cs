using System.ComponentModel.DataAnnotations;
using Ivy;
using MyProject.Connections.MyDb;
using Microsoft.EntityFrameworkCore;

namespace MyProject.Apps.Orders;

public class OrderCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record OrderCreateRequest
    {
        [Required]
        public Guid? CustomerId { get; init; } = null;

        [Required]
        public string StoreName { get; init; } = "";

        [Required]
        public string StoreMarket { get; init; } = "";

        [Required]
        public string StoreType { get; init; } = "";

        [Required]
        public string Channel { get; init; } = "";

        [Required]
        public OrderStatus Status { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<MyDbContextFactory>();
        var order = UseState(() => new OrderCreateRequest());

        return order
            .ToForm()
            .Builder(e => e.CustomerId, e => e.ToAsyncSelectInput(UseCustomerSearch, UseCustomerLookup, placeholder: "Select Customer"))
            .OnSubmit(OnSubmit)
            .ToDialog(isOpen, title: "Create Order", submitTitle: "Create");

        async Task OnSubmit(OrderCreateRequest request)
        {
            var orderId = await CreateOrderAsync(factory, request);
            refreshToken.Refresh(orderId);
        }
    }

    private async Task<int> CreateOrderAsync(MyDbContextFactory factory, OrderCreateRequest request)
    {
        await using var db = factory.CreateDbContext();

        var order = new Order()
        {
            CustomerId = request.CustomerId!.Value,
            StoreName = request.StoreName,
            StoreMarket = request.StoreMarket,
            StoreType = request.StoreType,
            Channel = request.Channel,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return order.Id;
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
                        .Select(e => new { e.Id, Name = e.FirstName + " " + e.LastName })
                        .Take(50)
                        .ToArrayAsync(ct))
                    .Select(e => new Option<Guid?>(e.Name, e.Id))
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
