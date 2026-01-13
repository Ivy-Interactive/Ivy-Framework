using System.Collections.Concurrent;
using System.Reactive.Disposables;

namespace Ivy.Views.DataTables;

public interface IQueryableRegistry
{
    string RegisterQueryable(IQueryable queryable);
    string RegisterQueryable(IQueryable queryable, Func<object, object?>? idSelector);
    IQueryable? GetQueryable(string sourceId);
    Func<object, object?>? GetIdSelector(string sourceId);
    IDisposable AddCleanup(string sourceId, IDisposable cleanup);
}

public class QueryableRegistry : IQueryableRegistry
{
    private readonly ConcurrentDictionary<string, IQueryable> _queryables = new();
    private readonly ConcurrentDictionary<string, CompositeDisposable> _cleanups = new();
    private readonly ConcurrentDictionary<string, Func<object, object?>?> _idSelectors = new();

    public string RegisterQueryable(IQueryable queryable)
    {
        return RegisterQueryable(queryable, null);
    }

    public string RegisterQueryable(IQueryable queryable, Func<object, object?>? idSelector)
    {
        var sourceId = Guid.NewGuid().ToString();
        _queryables[sourceId] = queryable;
        _cleanups[sourceId] = new CompositeDisposable();
        _idSelectors[sourceId] = idSelector;
        return sourceId;
    }

    public IQueryable? GetQueryable(string sourceId)
    {
        return _queryables.GetValueOrDefault(sourceId);
    }

    public Func<object, object?>? GetIdSelector(string sourceId)
    {
        return _idSelectors.GetValueOrDefault(sourceId);
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
            _idSelectors.TryRemove(sourceId, out _);
        });
    }
}
