using System.Reactive.Disposables;
using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Views.DataTables;

public static class UseDataTableExtensions
{
    public static DataTableConnection? UseDataTable(this IViewContext context, IQueryable queryable)
    {
        return UseDataTable(context, queryable, null);
    }

    public static DataTableConnection? UseDataTable(this IViewContext context, IQueryable queryable, Func<object, object?>? idSelector)
    {
        var connection = context.UseState<DataTableConnection?>(buildOnChange: false);
        var lastQueryable = context.UseState<object?>(buildOnChange: false);
        var cleanup = context.UseState<IDisposable?>(buildOnChange: false);

        var dataTableService = context.UseService<IDataTableService>();

        if (!ReferenceEquals(lastQueryable.Value, queryable))
        {
            cleanup.Value?.Dispose();

            var (newCleanup, newConnection) = dataTableService.AddQueryable(queryable, idSelector);
            connection.Set(newConnection);
            lastQueryable.Set(queryable);
            cleanup.Set(newCleanup);
        }

        context.UseEffect(() =>
        {
            return Disposable.Create(() => cleanup.Value?.Dispose());
        }, []);

        return connection.Value;
    }
}