using System.Threading.Channels;

namespace Ivy.Core;

public sealed class EventDispatchQueue : IDisposable
{
    private const int DefaultChannelCapacity = 1024;
    private readonly Channel<Func<Task>> _channel;
    private readonly CancellationTokenSource _cts;
    private readonly Task _worker;
    private volatile bool _disposed;

    public EventDispatchQueue(CancellationToken externalCancellation)
    {
        var options = new BoundedChannelOptions(DefaultChannelCapacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropOldest
        };
        _channel = Channel.CreateBounded<Func<Task>>(options);
        _cts = CancellationTokenSource.CreateLinkedTokenSource(externalCancellation);

        _worker = Task.Run(async () =>
        {
            try
            {
                while (await _channel.Reader.WaitToReadAsync(_cts.Token).ConfigureAwait(false))
                {
                    while (_channel.Reader.TryRead(out var work))
                    {
                        try
                        {
                            await work().ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] EventDispatchQueue work failed: {ex}");
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
        }, _cts.Token);
    }

    public void Enqueue(Action action)
    {
        if (_disposed) return;
        // Wrap synchronous action in a Task-returning function
        if (!_channel.Writer.TryWrite(() => { action(); return Task.CompletedTask; }))
        {
            if (_disposed) return;
            _ = _channel.Writer.WriteAsync(() => { action(); return Task.CompletedTask; }, _cts.Token);
        }
    }

    public void Enqueue(Func<Task> asyncAction)
    {
        if (_disposed) return;
        if (!_channel.Writer.TryWrite(asyncAction))
        {
            if (_disposed) return;
            _ = _channel.Writer.WriteAsync(asyncAction, _cts.Token);
        }
    }

    public void Dispose()
    {
        _disposed = true;

        // Signal cancellation
        try
        {
            _cts.Cancel();
        }
        catch
        {
            // ignored
        }

        // Complete the channel so the worker knows to stop
        try
        {
            _channel.Writer.TryComplete();
        }
        catch
        {
            // ignored
        }

        // Wait for the worker to actually complete before disposing CTS
        var workerCompleted = false;
        try
        {
            // Wait returns true if task completed, false if timeout
            workerCompleted = _worker.Wait(TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
            // Task completed with exception (expected for cancelled tasks)
            workerCompleted = true;
        }

        // Only dispose CTS if worker has actually stopped to prevent race condition
        if (workerCompleted)
        {
            _cts.Dispose();
        }
        // If timeout occurred, don't dispose - prevents accessing disposed CTS
        // The CTS will be finalized by GC, avoiding unobserved task exceptions
    }
}
