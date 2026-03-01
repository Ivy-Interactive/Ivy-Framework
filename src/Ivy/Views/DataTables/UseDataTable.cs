using System.Reactive.Disposables;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy;
using Ivy.Hooks;

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

        var typeName = queryable.ElementType.Name;
        var versionTrigger = context.UseQuery<string, string>(
            typeName,
            async (_, _) => Guid.NewGuid().ToString(),
            new QueryOptions { RevalidateOnMount = false }
        );

        var versionToken = versionTrigger.Value ?? "0";

        var dataTableService = context.UseService<IDataTableService>();

        DataTableConnection? resultConnection = connection.Value;

        if (!ReferenceEquals(lastQueryable.Value, queryable))
        {
            cleanup.Value?.Dispose();

            var (newCleanup, newConnection) = dataTableService.AddQueryable(queryable, idSelector);
            resultConnection = newConnection with { VersionToken = versionToken };

            connection.Set(resultConnection);
            lastQueryable.Set(queryable);
            cleanup.Set(newCleanup);
        }
        else if (resultConnection != null && resultConnection.VersionToken != versionToken)
        {
            resultConnection = resultConnection with { VersionToken = versionToken };
            connection.Set(resultConnection);
        }

        context.UseEffect(() =>
        {
            return Disposable.Create(() => cleanup.Value?.Dispose());
        }, []);

        return resultConnection;
    }
}