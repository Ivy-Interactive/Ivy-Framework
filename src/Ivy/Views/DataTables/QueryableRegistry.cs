using System.Collections.Concurrent;
using System.Reactive.Disposables;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IQueryableRegistry
{
    string RegisterQueryable(IQueryable queryable);
    string RegisterQueryable(IQueryable queryable, Func<object, object?>? idSelector);
    string RegisterQueryable(IQueryable queryable, Func<object, object?>? idSelector, string[]? columnNames);
    IQueryable? GetQueryable(string sourceId);
    Func<object, object?>? GetIdSelector(string sourceId);
    string[]? GetColumnNames(string sourceId);
    IDisposable AddCleanup(string sourceId, IDisposable cleanup);
}

public class QueryableRegistry : IQueryableRegistry
{
    private readonly ConcurrentDictionary<string, IQueryable> _queryables = new();
    private readonly ConcurrentDictionary<string, CompositeDisposable> _cleanups = new();
    private readonly ConcurrentDictionary<string, Func<object, object?>?> _idSelectors = new();
    private readonly ConcurrentDictionary<string, string[]?> _columnNames = new();

    public string RegisterQueryable(IQueryable queryable)
    {
        return RegisterQueryable(queryable, null, null);
    }

    public string RegisterQueryable(IQueryable queryable, Func<object, object?>? idSelector)
    {
        return RegisterQueryable(queryable, idSelector, null);
    }

    public string RegisterQueryable(IQueryable queryable, Func<object, object?>? idSelector, string[]? columnNames)
    {
        var sourceId = Guid.NewGuid().ToString();
        _queryables[sourceId] = queryable;
        _cleanups[sourceId] = new CompositeDisposable();
        _idSelectors[sourceId] = idSelector;
        _columnNames[sourceId] = columnNames;
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

    public string[]? GetColumnNames(string sourceId)
    {
        return _columnNames.GetValueOrDefault(sourceId);
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
            _columnNames.TryRemove(sourceId, out _);
        });
    }
}
