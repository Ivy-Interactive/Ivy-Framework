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
    private readonly Dictionary<Guid, IQueryable> _sources = new();
    
    //should return the arrow table?
    public Task<IActionResult> Query(string sourceId/*, GRPC TABLEQUERY */)
    {
        if (!_sources.TryGetValue(Guid.Parse(sourceId), out var queryable))
        {
            throw new Exception($"Source '{sourceId}' not found.");
        }
        
        //IQueryable query = queryable; 
        //todo: add filtering logic to produce the resulting arrow table
        
        throw new NotImplementedException();
    }
    
    public (IDisposable cleanup, string url) AddSource(IQueryable source)
    {
        var sourceId = Guid.NewGuid();
        _sources[sourceId] = source;

        var cleanup = Disposable.Create(() =>
        {
            _sources.Remove(sourceId);
        });

        return (cleanup, $"/data-table/query/{connectionId}/{sourceId}");
    }
}

public interface IDataTableService
{
    Task<IActionResult> Query(string sourceId /*, GRPC TABLEQUERY */);
}