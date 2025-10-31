using System.Threading.Channels;

namespace Ivy.Core;

/// <summary>
/// Dedicated event dispatch queue that processes enqueued actions on a long-running
/// background thread, avoiding consumption of ThreadPool workers during bursts.
/// </summary>
public sealed class EventDispatchQueue : IDisposable
{
    private readonly Channel<Action> _channel;
    private readonly CancellationTokenSource _cts;
    private readonly Task _worker;

    public EventDispatchQueue(CancellationToken externalCancellation)
    {
        var options = new BoundedChannelOptions(1024)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropOldest
        };
        _channel = Channel.CreateBounded<Action>(options);
        _cts = CancellationTokenSource.CreateLinkedTokenSource(externalCancellation);

        _worker = Task.Factory.StartNew(async () =>
        {
            try
            {
                while (await _channel.Reader.WaitToReadAsync(_cts.Token).ConfigureAwait(false))
                {
                    while (_channel.Reader.TryRead(out var work))
                    {
                        try { work(); }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] EventDispatchQueue work failed: {ex}");
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
        }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
    }

    public void Enqueue(Action action)
    {
        if (!_channel.Writer.TryWrite(action))
        {
            // Fallback to best-effort
            _ = _channel.Writer.WriteAsync(action, _cts.Token);
        }
    }

    public void Dispose()
    {
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
