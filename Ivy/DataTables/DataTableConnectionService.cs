using System.Reactive.Disposables;

namespace Ivy.DataTables;

public interface IDataTableService
{
    (IDisposable cleanup, DataTableConnection connection) AddQueryable(IQueryable queryable);
}

public class DataTableConnectionService(IQueryableRegistry queryableRegistry, ServerArgs serverArgs, string connectionId) : IDataTableService
{
    public (IDisposable cleanup, DataTableConnection connection) AddQueryable(IQueryable queryable)
    {
        Console.WriteLine($"Adding queryable with connectionId: {connectionId}");
        var sourceId = queryableRegistry.RegisterQueryable(queryable);

        var cleanup = Disposable.Create(() =>
        {
            queryableRegistry.AddCleanup(sourceId, Disposable.Empty);
        });

        var connection = new DataTableConnection(serverArgs.Port, "/datatable.TableService/Query/", connectionId, sourceId);
        
        return (cleanup, connection);
    }
}
