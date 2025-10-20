using System.Reactive.Disposables;
using Microsoft.Extensions.Logging;

namespace Ivy.Views.DataTables;

public interface IDataTableService
{
    (IDisposable cleanup, DataTableConnection connection) AddQueryable(IQueryable queryable);
}

public class DataTableConnectionService(IQueryableRegistry queryableRegistry, ServerArgs serverArgs, string connectionId, ILogger<DataTableConnectionService>? logger = null) : IDataTableService
{
    public (IDisposable cleanup, DataTableConnection connection) AddQueryable(IQueryable queryable)
    {
        logger?.LogInformation("Adding queryable with connectionId: {ConnectionId}", connectionId);
        var sourceId = queryableRegistry.RegisterQueryable(queryable);

        var cleanup = queryableRegistry.AddCleanup(sourceId, Disposable.Empty);

        var connection = new DataTableConnection(serverArgs.Port, "/datatable.DataTableService/Query", connectionId, sourceId);

        return (cleanup, connection);
    }
}
