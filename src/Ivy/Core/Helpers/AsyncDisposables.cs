namespace Ivy.Core.Helpers;

/// <summary>
/// A collection of <see cref="IAsyncDisposable"/> objects that can be disposed together asynchronously.
/// </summary>
public class AsyncDisposables : IAsyncDisposable
{
    private readonly List<IAsyncDisposable> _disposables = [];

    public void Add(IAsyncDisposable? disposable)
    {
        if (disposable != null)
        {
            _disposables.Add(disposable);
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var disposable in _disposables)
        {
            await disposable.DisposeAsync();
        }
        _disposables.Clear();
    }
}
