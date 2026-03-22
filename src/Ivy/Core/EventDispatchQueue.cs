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
        try
        {
            _cts.Cancel();
        }
        catch
        {
            // ignored
        }

        try
        {
            _channel.Writer.TryComplete();
        }
        catch
        {
            // ignored
        }

        try
        {
            _worker.Wait(TimeSpan.FromSeconds(2));
        }
        catch
        {
            // ignored
        }

        _cts.Dispose();
    }
}
