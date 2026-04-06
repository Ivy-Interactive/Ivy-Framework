using System.Reactive.Disposables;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class UseDataTableExtensions
{
    public static DataTableConnection? UseDataTable(this IViewContext context, IQueryable queryable, RefreshToken? refreshToken = null)
    {
        return UseDataTable(context, queryable, null, null, refreshToken);
    }

    public static DataTableConnection? UseDataTable(this IViewContext context, IQueryable queryable, Func<object, object?>? idSelector, RefreshToken? refreshToken = null)
    {
        return UseDataTable(context, queryable, idSelector, null, refreshToken);
    }

    public static DataTableConnection? UseDataTable(this IViewContext context, IQueryable queryable, Func<object, object?>? idSelector, DataTableColumn[]? columns, RefreshToken? refreshToken = null, DataTableConfig? config = null)
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

        var versionToken = refreshToken != null ? $"{versionTrigger.Value ?? "0"}_{refreshToken.Token}" : (versionTrigger.Value ?? "0");

        var dataTableService = context.UseService<IDataTableService>();

        DataTableConnection? resultConnection = connection.Value;

        if (!ReferenceEquals(lastQueryable.Value, queryable))
        {
            cleanup.Value?.Dispose();

            var columnNames = columns?.Select(c => c.Name).ToArray();
            var valueAccessors = columns?
                .Where(c => c.ValueAccessor != null)
                .ToDictionary(c => c.Name, c => c.ValueAccessor!);
            if (valueAccessors?.Count == 0) valueAccessors = null;
            var (newCleanup, newConnection) = dataTableService.AddQueryable(queryable, idSelector, columnNames, valueAccessors, config);
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
