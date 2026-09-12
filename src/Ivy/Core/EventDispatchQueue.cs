using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Ivy.Core;

public sealed class EventDispatchQueue : IDisposable
{
    private const int DefaultChannelCapacity = 1024;
    private const int DropLogIntervalMs = 5000;
    private readonly Channel<Func<Task>> _channel;
    private readonly CancellationTokenSource _cts;
    private readonly Task _worker;
    private readonly ILogger? _logger;
    private readonly string _sessionLabel;
    private long _droppedCount;
    private long _lastDropLogTicks;
    private volatile bool _disposed;

    internal long DroppedCount => Interlocked.Read(ref _droppedCount);

    public EventDispatchQueue(CancellationToken externalCancellation)
        : this(externalCancellation, null, null) { }

    internal EventDispatchQueue(CancellationToken externalCancellation, ILogger? logger, string? sessionLabel)
    {
        _logger = logger;
        _sessionLabel = sessionLabel ?? "unknown";
        var options = new BoundedChannelOptions(DefaultChannelCapacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropOldest
        };
        _channel = Channel.CreateBounded<Func<Task>>(options, OnItemDropped);
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

    private void OnItemDropped(Func<Task> dropped)
    {
        var total = Interlocked.Increment(ref _droppedCount);
        var now = Environment.TickCount64;
        var last = Interlocked.Read(ref _lastDropLogTicks);
        if (total != 1 && now - last < DropLogIntervalMs) return;
        if (Interlocked.CompareExchange(ref _lastDropLogTicks, now, last) != last) return;

        if (_logger is not null)
        {
            _logger.LogWarning(
                "EventDispatchQueue for session {Session} dropped {DroppedTotal} queued UI update(s), capacity {Capacity}; this session may be rendering stale content.",
                _sessionLabel, total, DefaultChannelCapacity);
        }
        else
        {
            Console.WriteLine($"WARN: EventDispatchQueue for session {_sessionLabel} dropped {total} queued UI update(s), capacity {DefaultChannelCapacity}; this session may be rendering stale content.");
        }
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
