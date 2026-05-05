using System.ComponentModel.DataAnnotations;
using Ivy;
using MyProject.Connections.MyDb;

using Microsoft.EntityFrameworkCore;

namespace MyProject.Apps.Orders;

public class OrderLineCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, int orderId) : ViewBase
{
    // We only include the required fields from the entity.
    // ALWAYS include the required fields of the entity!
    // ALWAYS use the same type as in the target entity, so we don't run into issues when saving the entity to the database.
    private record OrderLineCreateRequest
    {
        [Required]
        public int? ProductId { get; init; } = null;

        public int Quantity { get; init; } = 1;

        public decimal Discount { get; init; } = 0;
    }

    public override object? Build()
    {
        var factory = UseService<MyDbContextFactory>();
        var orderLine = UseState(() => new OrderLineCreateRequest());

        return orderLine
            .ToForm()
            //We only specify Builder if we want to customize the input control for the field - ToForm() will scaffold the form based on the properties of the record
            //ToAsyncSelectInput allows us to select foreign keys
            .Builder(e => e.ProductId, e => e.ToAsyncSelectInput(UseProductSearch, UseProductLookup, placeholder: "Select Product"))
            .OnSubmit(OnSubmit)
            .ToDialog(isOpen, title: "Add Product to Order", submitTitle: "Add");

        async Task OnSubmit(OrderLineCreateRequest request)
        {
            var lineId = await CreateOrderLineAsync(factory, request);
            refreshToken.Refresh(lineId);
        }
    }

    private async Task<int> CreateOrderLineAsync(MyDbContextFactory factory, OrderLineCreateRequest request)
    {
        await using var db = factory.CreateDbContext();

        // Get the product to get its price
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId!.Value);
        if (product == null)
            throw new Exception("Product not found");

        var line = new Line()
        {
            OrderId = orderId,
            ProductId = request.ProductId!.Value,
            Quantity = request.Quantity,
            Price = product.Price,
            Discount = request.Discount,
        };

        db.Lines.Add(line);
        await db.SaveChangesAsync();

        return line.Id;
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
