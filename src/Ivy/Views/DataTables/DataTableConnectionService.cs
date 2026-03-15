using System.Reactive.Disposables;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IDataTableService
{
    (IDisposable cleanup, DataTableConnection connection) AddQueryable(IQueryable queryable);
    (IDisposable cleanup, DataTableConnection connection) AddQueryable(IQueryable queryable, Func<object, object?>? idSelector);
    (IDisposable cleanup, DataTableConnection connection) AddQueryable(IQueryable queryable, Func<object, object?>? idSelector, string[]? columnNames);
}

public class DataTableConnectionService(IQueryableRegistry queryableRegistry, ServerArgs serverArgs, string connectionId, ILogger<DataTableConnectionService>? logger = null) : IDataTableService
{
    public (IDisposable cleanup, DataTableConnection connection) AddQueryable(IQueryable queryable)
    {
        return AddQueryable(queryable, null, null);
    }

    public (IDisposable cleanup, DataTableConnection connection) AddQueryable(IQueryable queryable, Func<object, object?>? idSelector)
    {
        return AddQueryable(queryable, idSelector, null);
    }

    public (IDisposable cleanup, DataTableConnection connection) AddQueryable(IQueryable queryable, Func<object, object?>? idSelector, string[]? columnNames)
    {
        logger?.LogInformation("Adding queryable with connectionId: {ConnectionId}", connectionId);
        var sourceId = queryableRegistry.RegisterQueryable(queryable, idSelector, columnNames);

        var cleanup = queryableRegistry.AddCleanup(sourceId, Disposable.Empty);

        var connection = new DataTableConnection(serverArgs.Port, "/datatable.DataTableService/Query", connectionId, sourceId);

        return (cleanup, connection);
    }
}
