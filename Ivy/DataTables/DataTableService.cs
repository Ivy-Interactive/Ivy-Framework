using System.Reactive.Disposables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.DataTables;

public class DataTableController(AppSessionStore sessionStore) : Controller
{
    [Route("data-table/query/{connectionId}/{sourceId}")]
    public async Task<IActionResult> Query(string connectionId, string sourceId /*, GRPC STUFF */)
    {
        //todo later: verify the jwt token
        
        if (sessionStore.Sessions.TryGetValue(connectionId, out var session))
        {
            var dataTableService = session.AppServices.GetRequiredService<IDataTableService>();
            return await dataTableService.Query(sourceId); // => GRPC TABLERESULT
        }
        throw new Exception($"DataTable 'data-table/query/{connectionId}/{sourceId}' not found.");
    }
}

public class DataTableService(string connectionId) : IDataTableService
{
    private readonly Dictionary<Guid, IQueryable> _queryables = new();
    
    //should return the arrow table?
    public Task<IActionResult> Query(string sourceId/*, GRPC TABLEQUERY */)
    {
        if (!_queryables.TryGetValue(Guid.Parse(sourceId), out var queryable))
        {
            throw new Exception($"Queryable '{sourceId}' not found.");
        }
        
        //IQueryable query = queryable; 
        //todo: add filtering logic to produce the resulting arrow table
        
        throw new NotImplementedException();
    }
    
    public (IDisposable cleanup, DataTableConnection connection) AddQueryable(IQueryable queryable)
    {
        var sourceId = Guid.NewGuid();
        _queryables[sourceId] = queryable;

        var cleanup = Disposable.Create(() =>
        {
            _queryables.Remove(sourceId);
        });

        //todo: what is the port and path we're using?
        var connection = new DataTableConnection(12345, "/path/", connectionId, sourceId.ToString());
        
        return (cleanup, connection);
    }
}

public interface IDataTableService
{
    (IDisposable cleanup, DataTableConnection connection) AddQueryable(IQueryable queryable);
    Task<IActionResult> Query(string sourceId /*, GRPC TABLEQUERY */);
}