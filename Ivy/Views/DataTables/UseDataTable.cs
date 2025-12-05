using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Views.DataTables;

public static class UseDataTableExtensions
{
    public static DataTableConnection? UseDataTable<TView>(this TView view, IQueryable queryable) where TView : ViewBase =>
        view.Context.UseDataTable(queryable);

    public static DataTableConnection? UseDataTable<TView>(this TView view, IQueryable queryable, Func<object, object?>? idSelector) where TView : ViewBase =>
        view.Context.UseDataTable(queryable, idSelector);

    public static DataTableConnection? UseDataTable(this IViewContext context, IQueryable queryable)
    {
        return UseDataTable(context, queryable, null);
    }

    public static DataTableConnection? UseDataTable(this IViewContext context, IQueryable queryable, Func<object, object?>? idSelector)
    {
        // DON'T trigger rebuild when connection changes - we handle it manually
        var connection = context.UseState<DataTableConnection?>(buildOnChange: false);
        var hasRun = context.UseState(false, buildOnChange: false);
        var dataTableService = context.UseService<IDataTableService>();

        // Only create connection once - check hasRun flag
        if (!hasRun.Value && connection.Value == null)
        {
            var (cleanup, _connection) = dataTableService.AddQueryable(queryable, idSelector);
            connection.Set(_connection);
            hasRun.Set(true);

            // Store cleanup for later
            context.UseEffect(() => cleanup, []);
        }

        return connection.Value!;
    }
}