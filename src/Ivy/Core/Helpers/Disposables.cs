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
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
        _disposables.Clear();
    }
}