using Ivy.Core.Server;

namespace Ivy.Test;

public class AppSessionStoreTests
{
    [Fact]
    public async Task ScheduleDeferredRemoval_RemovesAfterDelay()
    {
        using var store = new AppSessionStore();
        var removed = false;
        var tcs = new TaskCompletionSource();

        store.ScheduleDeferredRemoval("conn-1", TimeSpan.FromMilliseconds(10), async connId =>
        {
            removed = true;
            tcs.TrySetResult();
            await Task.CompletedTask;
        });

        Assert.True(store.HasDeferredRemoval("conn-1"));
        Assert.False(removed);

        await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(5)));

        Assert.True(removed);
        Assert.False(store.HasDeferredRemoval("conn-1"));
    }

    [Fact]
    public async Task CancelDeferredRemoval_PreventsRemoval()
    {
        using var store = new AppSessionStore();
        var removed = false;

        store.ScheduleDeferredRemoval("conn-1", TimeSpan.FromMilliseconds(100), async connId =>
        {
            removed = true;
            await Task.CompletedTask;
        });

        Assert.True(store.HasDeferredRemoval("conn-1"));

        var cancelled = store.CancelDeferredRemoval("conn-1");
        Assert.True(cancelled);
        Assert.False(store.HasDeferredRemoval("conn-1"));

        await Task.Delay(150);

        Assert.False(removed);
    }

    [Fact]
    public void CancelDeferredRemoval_NoExistingDeferral_ReturnsFalse()
    {
        using var store = new AppSessionStore();

        Assert.False(store.CancelDeferredRemoval("conn-1"));
    }

    [Fact]
    public void HasDeferredRemoval_NoDeferral_ReturnsFalse()
    {
        using var store = new AppSessionStore();

        Assert.False(store.HasDeferredRemoval("conn-1"));
    }

    [Fact]
    public async Task ScheduleDeferredRemoval_ReplacesExisting()
    {
        using var store = new AppSessionStore();
        var firstCalled = false;
        var secondCalled = false;
        var tcs = new TaskCompletionSource();

        store.ScheduleDeferredRemoval("conn-1", TimeSpan.FromMilliseconds(500), async connId =>
        {
            firstCalled = true;
            await Task.CompletedTask;
        });

        // Replace with a new deferred removal
        store.ScheduleDeferredRemoval("conn-1", TimeSpan.FromMilliseconds(10), async connId =>
        {
            secondCalled = true;
            tcs.TrySetResult();
            await Task.CompletedTask;
        });

        await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(5)));

        Assert.False(firstCalled);
        Assert.True(secondCalled);
    }
}
