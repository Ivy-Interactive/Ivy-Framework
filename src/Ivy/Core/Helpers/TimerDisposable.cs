namespace Ivy.Core.Helpers;

/// <summary>
/// Wraps a <see cref="System.Timers.Timer"/> to ensure Stop() is called before Dispose(),
/// and provides a cancellation barrier to prevent post-disposal callbacks.
/// </summary>
public sealed class TimerDisposable : IDisposable
{
    private readonly System.Timers.Timer _timer;
    private readonly CancellationTokenSource _cts;
    private bool _disposed;

    public TimerDisposable(System.Timers.Timer timer, CancellationTokenSource cts)
    {
        _timer = timer;
        _cts = cts;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cts.Cancel();
        _timer.Stop();
        _timer.Dispose();
        _cts.Dispose();
    }
}
