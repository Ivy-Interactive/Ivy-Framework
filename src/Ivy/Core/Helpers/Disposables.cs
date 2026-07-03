namespace Ivy.Core.Helpers;

public class Disposables(params IEnumerable<IDisposable> disposables) : IDisposable
{
    public Disposables() : this([])
    {
    }

    private readonly List<IDisposable> _disposables = disposables.ToList();

    public void Add(params IDisposable[] disposable)
    {
        foreach (var d in disposable)
        {
            _disposables.Add(d);
        }
    }

    public void Add(IEnumerable<IDisposable> disposable)
    {
        foreach (var d in disposable)
        {
            _disposables.Add(d);
        }
    }

    public void Dispose()
    {
        // Snapshot the list first: a disposable's Dispose() may re-enter and
        // modify _disposables (e.g. via Add or a nested Dispose), which would
        // otherwise invalidate the enumerator.
        var snapshot = _disposables.ToArray();
        _disposables.Clear();
        foreach (var disposable in snapshot)
        {
            disposable.Dispose();
        }
    }
}