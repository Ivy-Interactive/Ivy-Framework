namespace Ivy.Core.Helpers;

/// <summary>
/// Adapts an <see cref="IDisposable"/> to <see cref="IAsyncDisposable"/> without blocking.
/// </summary>
internal sealed class DisposableAdapter(IDisposable? disposable) : IAsyncDisposable
{
    public ValueTask DisposeAsync()
    {
        disposable?.Dispose();
        return ValueTask.CompletedTask;
    }
}
