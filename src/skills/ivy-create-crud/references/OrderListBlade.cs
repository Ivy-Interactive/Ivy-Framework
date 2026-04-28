using Ivy;
using MyProject.Connections.MyDb;
using Microsoft.EntityFrameworkCore;

namespace MyProject.Apps.Orders;

public class OrderListBlade : ViewBase
{
    private record OrderListRecord(int Id, string StoreName, string? CustomerName, OrderStatus Status);

    public override object? Build()
    {
        var blades = UseContext<IBladeContext>();
        var refreshToken = UseRefreshToken();

        // Filter state for search
        var filter = UseState("");

        var ordersQuery = UseOrderListRecords(Context, filter.Value);

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is int orderId)
            {
                blades.Pop(this);
                ordersQuery.Mutator.Revalidate();
                blades.Push(this, new OrderDetailsBlade(orderId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var order = (OrderListRecord)e.Sender.Tag!;
            blades.Push(this, new OrderDetailsBlade(order.Id), order.StoreName);
        });

        object CreateItem(OrderListRecord listRecord) => new FuncView(context =>
        {
            var itemQuery = UseOrderListRecord(context, listRecord);
            if (itemQuery.Loading || itemQuery.Value == null)
            {
                return new ListItem();
            }
            var record = itemQuery.Value;
            var statusBadge = new Badge(record.Status.GetDescription())
                .Variant(record.Status switch
                {
                    OrderStatus.Cancelled => BadgeVariant.Destructive,
                    OrderStatus.PendingReview => BadgeVariant.Warning,
                    OrderStatus.InProgress => BadgeVariant.Warning,
                    OrderStatus.Shipped => BadgeVariant.Success,
                    OrderStatus.Delivered => BadgeVariant.Info,
                    _ => BadgeVariant.Secondary
                });
            return new ListItem(title: record.StoreName, subtitle: record.CustomerName, onClick: onItemClicked, tag: record)
                .Content(statusBadge);
        });

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("Create Order").ToTrigger((isOpen) => new OrderCreateDialog(isOpen, refreshToken));

        var items = (ordersQuery.Value ?? []).Select(CreateItem);

        var header = Layout.Horizontal().Gap(1)
                     | filter.ToSearchInput().Placeholder("Search").Width(Size.Grow())
                     | createBtn;

        return new Fragment()
               | new BladeHeader(header)
               | (ordersQuery.Value == null ? Text.Muted("Loading...") : new List(items));
    }

    private static QueryResult<OrderListRecord[]> UseOrderListRecords(IViewContext context, string filter)
    {
        var factory = context.UseService<MyDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseOrderListRecords), filter),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var linq = db.Orders.Include(o => o.Customer).AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    filter = filter.Trim();
                    linq = linq.Where(e => e.StoreName.Contains(filter) || e.Customer.FirstName.Contains(filter) || e.Customer.LastName.Contains(filter));
                }

                return await linq
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(50)
                    .Select(e => new OrderListRecord(e.Id, e.StoreName, e.Customer != null ? $"{e.Customer.FirstName} {e.Customer.LastName}" : null, e.Status))
                    .ToArrayAsync(ct);
            },
            tags: [typeof(Order[])],
            options: new QueryOptions()
            {
                KeepPrevious = true
            }
        );
    }

    private static QueryResult<OrderListRecord?> UseOrderListRecord(IViewContext context, OrderListRecord record)
    {
        var factory = context.UseService<MyDbContextFactory>();
        return context.UseQuery(
            key: (nameof(UseOrderListRecord), record.Id),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Orders
                    .Include(e => e.Customer)
                    .Where(e => e.Id == record.Id)
                    .Select(e => new OrderListRecord(e.Id, e.StoreName, e.Customer != null ? $"{e.Customer.FirstName} {e.Customer.LastName}" : null, e.Status))
                    .FirstOrDefaultAsync(ct);
            },
            options: new QueryOptions { RevalidateOnMount = false },
            initialValue: record,
            tags: [(typeof(Order), record.Id)]
        );
    }
}
