using Ivy;
using MyProject.Connections.MyDb;

using Microsoft.EntityFrameworkCore;

namespace MyProject.Apps.Orders;

public class OrderLinesBlade(int orderId) : ViewBase
{
    private enum RowAction { Edit, Delete }

    public override object? Build()
    {
        var factory = UseService<MyDbContextFactory>();
        var queryService = UseService<IQueryService>();
        var refreshToken = UseRefreshToken();

        var editSheetOpen = UseState(() => false);
        var editingLineId = UseState(() => 0);

        var linesQuery = UseQuery(
            key: (nameof(OrderLinesBlade), orderId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Lines.Include(e => e.Product).Where(e => e.OrderId == orderId).ToArrayAsync(ct);
            },
            tags: [typeof(Line[]), (typeof(Order), orderId)]
        );

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is int)
            {
                linesQuery.Mutator.Revalidate();
                queryService.RevalidateByTag((typeof(Order), orderId));
            }
        }, [refreshToken]);

        if (linesQuery.Loading) return Skeleton.Card();

        if (linesQuery.Value == null || linesQuery.Value.Length == 0)
        {
            var addBtnEmpty = new Button("Add Line").Icon(Icons.Plus).Outline()
                .ToTrigger((isOpen) => new OrderLineCreateDialog(isOpen, refreshToken, orderId));

            return new Fragment()
                   | new BladeHeader(addBtnEmpty)
                   | new Callout("No order lines found. Add a line to get started.").Variant(CalloutVariant.Info);
        }

        var dataTable = linesQuery.Value.Select(e => new
        {
            e.Id,
            Product = e.Product.Name,
            Quantity = e.Quantity,
            Price = e.Price,
            Discount = e.Discount,
            DiscountCode = e.DiscountCode,
            LineTotal = e.Quantity * e.Price - e.Discount,
            e.Refunded,
        }).AsQueryable()
            .ToDataTable(idSelector: e => e.Id)
            .RefreshToken(refreshToken)
            .Header(e => e.LineTotal, "Line Total")
            .Header(e => e.DiscountCode, "Discount Code")
            .Footer(e => e.LineTotal, "Total", values => values.Sum())
            .Footer(e => e.Quantity, "Total", values => values.Sum())
            .RowActions(
                MenuItem.Default(Icons.Pencil).Tag(RowAction.Edit),
                MenuItem.Default(Icons.Trash).Tag(RowAction.Delete)
            )
            .OnRowAction(args =>
            {
                var e = args.Value;
                if (!Enum.TryParse<RowAction>(e.Tag?.ToString(), ignoreCase: true, out var action)) return ValueTask.CompletedTask;
                if (!int.TryParse(e.Id?.ToString() ?? "", out int lineId)) return ValueTask.CompletedTask;

                switch (action)
                {
                    case RowAction.Edit:
                        editingLineId.Set(lineId);
                        editSheetOpen.Set(true);
                        break;
                    case RowAction.Delete:
                        return DeleteAndRefreshAsync(factory, lineId, linesQuery, queryService);
                }
                return ValueTask.CompletedTask;
            });

        var addBtn = new Button("Add Line").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new OrderLineCreateDialog(isOpen, refreshToken, orderId));

        return new Fragment()
               | new BladeHeader(addBtn)
               | dataTable
               | new OrderLineEditSheet(editSheetOpen, refreshToken, editingLineId.Value);
    }

    private async ValueTask DeleteAndRefreshAsync(MyDbContextFactory factory, int lineId, QueryResult<Line[]> linesQuery, IQueryService queryService)
    {
        await using var db = factory.CreateDbContext();
        var line = await db.Lines.SingleOrDefaultAsync(e => e.Id == lineId);
        if (line != null)
        {
            db.Lines.Remove(line);
            await db.SaveChangesAsync();
        }
        linesQuery.Mutator.Revalidate();
        queryService.RevalidateByTag((typeof(Order), orderId));
    }
}
