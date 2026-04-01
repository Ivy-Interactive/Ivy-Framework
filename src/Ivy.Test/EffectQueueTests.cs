using Ivy.Core.Exceptions;
using Ivy.Core.Hooks;

namespace Ivy.Test;

public class EffectQueueTests
{
    private class StubExceptionHandler : IExceptionHandler
    {
        public List<Exception> Exceptions { get; } = [];
        public bool HandleException(Exception exception)
        {
            Exceptions.Add(exception);
            return true;
        }
    }

    private static EffectHook CreateEffect(int identity, Func<Task> handler)
    {
        return new EffectHook(identity, async () =>
        {
            await handler();
            return null;
        }, [EffectTrigger.OnMount()]);
    }

    [Fact]
    public async Task EnqueuedEffect_DuringProcessing_IsStillProcessed()
    {
        // This test verifies the race condition fix: an effect enqueued while
        // ProcessQueueAsync is between the empty-check and _isProcessing = false
        // must still be processed.
        var handler = new StubExceptionHandler();
        var queue = new EffectQueue(handler);

        var firstEffectStarted = new TaskCompletionSource();
        var allowFirstEffectToComplete = new TaskCompletionSource();
        var secondEffectExecuted = new TaskCompletionSource();

        // First effect: signals it started, then waits
        var effect1 = CreateEffect(1, async () =>
        {
            firstEffectStarted.SetResult();
            await allowFirstEffectToComplete.Task;
        });

        // Second effect: signals completion
        var effect2 = CreateEffect(2, () =>
        {
            secondEffectExecuted.SetResult();
            return Task.CompletedTask;
        });

        // Enqueue first effect and wait for it to start processing
        queue.Enqueue(effect1, EffectPriority.OnMount);
        await firstEffectStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Now enqueue second effect while first is still processing
        queue.Enqueue(effect2, EffectPriority.OnMount);

        // Let first effect complete
        allowFirstEffectToComplete.SetResult();

        // Second effect must be processed (this would hang/timeout before the fix)
        await secondEffectExecuted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await queue.DisposeAsync();
    }

    [Fact]
    public async Task ConcurrentEnqueue_AllEffectsProcessed()
    {
        // Stress test: enqueue effects from multiple threads simultaneously
        var handler = new StubExceptionHandler();
        var queue = new EffectQueue(handler);

        const int effectCount = 50;
        var executedCount = 0;
        var allDone = new TaskCompletionSource();

        var barrier = new Barrier(effectCount);

        var tasks = Enumerable.Range(0, effectCount).Select(i => Task.Run(() =>
        {
            var effect = CreateEffect(i, () =>
            {
                if (Interlocked.Increment(ref executedCount) == effectCount)
                {
                    allDone.SetResult();
                }
                return Task.CompletedTask;
            });

            // Synchronize all threads to enqueue at roughly the same time
            barrier.SignalAndWait(TimeSpan.FromSeconds(5));
            queue.Enqueue(effect, EffectPriority.OnMount);
        })).ToArray();

        await Task.WhenAll(tasks);
        await allDone.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal(effectCount, executedCount);
        Assert.Empty(handler.Exceptions);

        await queue.DisposeAsync();
    }

    [Fact]
    public async Task EnqueuedEffect_AfterQueueDrains_IsProcessed()
    {
        // Simple sequential test: enqueue, wait for processing, enqueue again
        var handler = new StubExceptionHandler();
        var queue = new EffectQueue(handler);

        var firstDone = new TaskCompletionSource();
        var secondDone = new TaskCompletionSource();

        var effect1 = CreateEffect(1, () =>
        {
            firstDone.SetResult();
            return Task.CompletedTask;
        });

        queue.Enqueue(effect1, EffectPriority.OnMount);
        await firstDone.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var effect2 = CreateEffect(2, () =>
        {
            secondDone.SetResult();
            return Task.CompletedTask;
        });

        queue.Enqueue(effect2, EffectPriority.OnMount);
        await secondDone.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Empty(handler.Exceptions);
        await queue.DisposeAsync();
    }
}
