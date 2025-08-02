using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.DataTables;

public static class UseDataTableExtensions
{
    public static DataTableConnection? UseDataTable<TView>(this TView view, IQueryable queryable) where TView : ViewBase =>
        view.Context.UseDataTable(queryable);
    
    public static DataTableConnection? UseDataTable(this IViewContext context, IQueryable queryable)
    {
        var connection = context.UseState<DataTableConnection?>();
        var dataTableService = context.UseService<IDataTableService>();
        context.UseEffect(() =>
        {
            var (cleanup, _connection) = dataTableService.AddQueryable(queryable);
            connection.Set(_connection);
            return cleanup;
        });
        return connection.Value!;
    }
}