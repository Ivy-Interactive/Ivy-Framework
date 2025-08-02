using System.Collections.Concurrent;
using System.Reactive.Disposables;

namespace Ivy.DataTables;

public interface IQueryableRegistry
{
    string RegisterQueryable(IQueryable queryable);
    IQueryable? GetQueryable(string sourceId);
    IDisposable AddCleanup(string sourceId, IDisposable cleanup);
}

public class QueryableRegistry : IQueryableRegistry
{
    private readonly ConcurrentDictionary<string, IQueryable> _queryables = new();
    private readonly ConcurrentDictionary<string, CompositeDisposable> _cleanups = new();

    public string RegisterQueryable(IQueryable queryable)
    {
        var sourceId = Guid.NewGuid().ToString();
        _queryables[sourceId] = queryable;
        _cleanups[sourceId] = new CompositeDisposable();
        return sourceId;
    }

    public IQueryable? GetQueryable(string sourceId)
    {
        return _queryables.TryGetValue(sourceId, out var queryable) ? queryable : null;
    }

    public IDisposable AddCleanup(string sourceId, IDisposable cleanup)
    {
        if (_cleanups.TryGetValue(sourceId, out var compositeDisposable))
        {
            compositeDisposable.Add(cleanup);
        }
        
        return Disposable.Create(() =>
        {
            if (_cleanups.TryRemove(sourceId, out var toCleanup))
            {
                toCleanup.Dispose();
            }
            _queryables.TryRemove(sourceId, out _);
        });
    }
}
