using System.Collections.Concurrent;
using Ivy.Core;
using Ivy.Core.Server;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Ivy.Test;

public class EventDispatchQueueTests
{
    [Fact]
    public async Task DrainedQueue_DropsNothingAndLogsNothing()
    {
        var logger = new RecordingLogger();
        using var cts = new CancellationTokenSource();
        using var queue = new EventDispatchQueue(cts.Token, logger, "test-session");

        var counter = 0;
        for (var i = 0; i < 10; i++)
        {
            var captured = i;
            queue.Enqueue(() => { counter = captured; });
        }

        await Task.Delay(100);

        Assert.Equal(9, counter);
        Assert.Equal(0, queue.DroppedCount);
        Assert.DoesNotContain(logger.Entries, e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task OverflowByOne_CountsOneDropAndLogsOnce()
    {
        var logger = new RecordingLogger();
        using var cts = new CancellationTokenSource();
        using var queue = new EventDispatchQueue(cts.Token, logger, "overflow-session");

        var started = new TaskCompletionSource();
        var gate = new TaskCompletionSource();

        queue.Enqueue(async () =>
        {
            started.SetResult();
            await gate.Task;
        });

        await started.Task;

        for (var i = 0; i < 1024; i++)
        {
            queue.Enqueue(() => { });
        }

        queue.Enqueue(() => { });

        gate.SetResult();
        await Task.Delay(100);

        Assert.Equal(1, queue.DroppedCount);
        var warnings = logger.Entries.Where(e => e.Level == LogLevel.Warning).ToList();
        Assert.Single(warnings);
        Assert.Contains("overflow-session", warnings[0].Message);
    }

    [Fact]
    public async Task SustainedOverflow_LogsFarFewerLinesThanItDrops()
    {
        var logger = new RecordingLogger();
        using var cts = new CancellationTokenSource();
        using var queue = new EventDispatchQueue(cts.Token, logger, "sustained-overflow");

        var started = new TaskCompletionSource();
        var gate = new TaskCompletionSource();

        queue.Enqueue(async () =>
        {
            started.SetResult();
            await gate.Task;
        });

        await started.Task;

        for (var i = 0; i < 1024; i++)
        {
            queue.Enqueue(() => { });
        }

        for (var i = 0; i < 500; i++)
        {
            queue.Enqueue(() => { });
        }

        gate.SetResult();
        await Task.Delay(100);

        Assert.Equal(500, queue.DroppedCount);
        var warningCount = logger.Entries.Count(e => e.Level == LogLevel.Warning);
        Assert.InRange(warningCount, 1, 3);
    }

    [Fact]
    public async Task ClientSenderOverflowByOne_CountsOneDropAndLogsOnce()
    {
        var logger = new RecordingLogger();
        var notifier = new FakeClientNotifier();

        var started = new TaskCompletionSource();
        var gate = new TaskCompletionSource();
        notifier.OnNotify = async (connectionId, method, message) =>
        {
            started.SetResult();
            await gate.Task;
        };

        using var sender = new ClientSender(notifier, "test-connection", logger);

        sender.Send("FirstMessage", null);
        await started.Task;

        for (var i = 0; i < 2048; i++)
        {
            sender.Send("FillMessage", i);
        }

        sender.Send("OverflowMessage", null);

        gate.SetResult();
        await Task.Delay(100);

        Assert.Equal(1, sender.DroppedCount);
        var warnings = logger.Entries.Where(e => e.Level == LogLevel.Warning).ToList();
        Assert.Single(warnings);
        Assert.Contains("test-connection", warnings[0].Message);
    }

    private class RecordingLogger : ILogger
    {
        public ConcurrentBag<(LogLevel Level, string Message)> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception)));
        }
    }

    private class FakeClientNotifier : IClientNotifier
    {
        public Func<string, string, object?, Task>? OnNotify { get; set; }

        public Task NotifyClientAsync(string connectionId, string method, object? message)
        {
            return OnNotify?.Invoke(connectionId, method, message) ?? Task.CompletedTask;
        }
    }
}
