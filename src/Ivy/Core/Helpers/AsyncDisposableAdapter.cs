namespace Ivy.Core.Helpers;

/// <summary>
/// Adapts an <see cref="IAsyncDisposable"/> to <see cref="IDisposable"/> by blocking on async disposal.
/// </summary>
internal sealed class AsyncDisposableAdapter(IAsyncDisposable asyncDisposable) : IDisposable
{
    public void Dispose()
    {
        asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
