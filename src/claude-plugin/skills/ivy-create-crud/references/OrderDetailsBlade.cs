using Ivy;
using MyProject.Connections.MyDb;
using Microsoft.EntityFrameworkCore;

namespace MyProject.Apps.Orders;

public class OrderDetailsBlade(int orderId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<MyDbContextFactory>();
        var blades = UseContext<IBladeContext>();
        var queryService = UseService<IQueryService>();

        var orderQuery = UseQuery(
            key: (nameof(OrderDetailsBlade), orderId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Orders.Include(e => e.Customer).SingleOrDefaultAsync(e => e.Id == orderId, ct);
            },
            tags: [(typeof(Order), orderId)]
        );

        //used in details card
        var totalAmountQuery = UseQuery(
            key: (nameof(OrderDetailsBlade), "totalAmount", orderId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Lines.Where(e => e.OrderId == orderId).SumAsync(e => e.Quantity * e.Price - e.Discount, ct);
            },
            tags: [(typeof(Order), orderId)]
        );

        //used in related card to show the number of lines in the order
        var lineCountQuery = UseQuery(
            key: (nameof(OrderDetailsBlade), "lineCount", orderId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Lines.CountAsync(e => e.OrderId == orderId, ct);
            },
            tags: [(typeof(Order), orderId), typeof(Line[])]
        );

        if (orderQuery.Loading) return Skeleton.Card();

        if (orderQuery.Value == null)
        {
            return new Callout($"Order '{orderId}' not found. It may have been deleted.")
                .Variant(CalloutVariant.Warning);
        }

        var orderValue = orderQuery.Value;

        var deleteBtn = new Button("Delete", onClick: async _ =>
            {
                blades.Pop(refresh: true);
                await DeleteAsync(factory);
                queryService.RevalidateByTag(typeof(Order[]));
            })
            .Variant(ButtonVariant.Destructive)
            .Icon(Icons.Trash)
            .WithConfirm("Are you sure you want to delete this order?", "Delete Order");

        var editBtn = new Button("Edit")
            .Outline()
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new OrderEditSheet(isOpen, orderId));

        var statusBadge = new Badge(orderValue.Status.GetDescription())
            .Variant(orderValue.Status switch
            {
                OrderStatus.Shipped => BadgeVariant.Success,
                OrderStatus.Delivered => BadgeVariant.Info,
                OrderStatus.Cancelled => BadgeVariant.Destructive,
                OrderStatus.InProgress => BadgeVariant.Warning,
                _ => BadgeVariant.Secondary
            });

        var detailsCard = new Card(
            content: new
            {
                orderValue.Id,
                CustomerName = $"{orderValue.Customer.FirstName} {orderValue.Customer.LastName}",
                orderValue.StoreName,
                orderValue.StoreMarket,
                orderValue.StoreType,
                orderValue.Channel,
                orderValue.Status,
                TotalAmount = totalAmountQuery.Value
            }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.Id, e => e.CopyToClipboard())
                .Builder(e => e.Status, e => e.Func((OrderStatus _) => statusBadge)),
            footer: Layout.Horizontal().Gap(2).AlignContent(Align.Right)
                    | deleteBtn
                    | editBtn
        ).Title("Order Details").Width(Size.Units(100));

        var relatedCard = new Card(
            new List(
                new ListItem("Lines", onClick: _ =>
                {
                    blades.Push(this, new OrderLinesBlade(orderId), "Lines");
                }, badge: lineCountQuery.Value.ToString("N0")) //we include a badge with the number of lines to show the user how many lines are in the order
            ));

        return new Fragment()
               | new BladeHeader(Text.Literal($"Order #{orderValue.Id}"))
               | (Layout.Vertical() | detailsCard | relatedCard);
    }

    private async Task DeleteAsync(MyDbContextFactory dbFactory)
    {
        await using var db = dbFactory.CreateDbContext();
        var order = await db.Orders.FirstOrDefaultAsync(e => e.Id == orderId);
        if (order != null)
        {
            db.Orders.Remove(order);
            await db.SaveChangesAsync();
        }
    }
}
