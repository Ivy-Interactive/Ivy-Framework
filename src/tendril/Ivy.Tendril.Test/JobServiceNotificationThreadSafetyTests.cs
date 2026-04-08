using System.Collections.Concurrent;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServiceNotificationThreadSafetyTests
{
    [Fact]
    public void NotificationReady_FiresOnSyncContext_WhenAvailable()
    {
        var testContext = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(testContext);
        try
        {
            var service = new JobService(
                TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10),
                null, 1);

            JobNotification? received = null;
            service.NotificationReady += n => received = n;

            // Start a job and complete it to trigger notification
            var id = service.StartJob("MakePr", Path.GetTempPath());
            service.CompleteJob(id, 0);

            // The notification should have been posted to the sync context, not invoked directly
            Assert.Null(received);
            Assert.True(testContext.PostCount > 0, "Expected at least one Post to sync context");

            // Execute the posted callbacks
            testContext.ExecutePending();
            Assert.NotNull(received);
            Assert.Equal("MakePr Completed", received.Title);
            Assert.True(received.IsSuccess);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(null);
        }
    }

    [Fact]
    public void NotificationReady_FiresSynchronously_WhenNoSyncContext()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var service = new JobService(
            TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10),
            null, 1);

        JobNotification? received = null;
        service.NotificationReady += n => received = n;

        var id = service.StartJob("MakePr", Path.GetTempPath());
        service.CompleteJob(id, 0);

        Assert.NotNull(received);
        Assert.Equal("MakePr Completed", received.Title);
        Assert.True(received.IsSuccess);
    }

    [Fact]
    public void NotificationReady_MultipleRapidNotifications_PreserveOrder()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var service = new JobService(
            TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10),
            null, 10);

        var notifications = new ConcurrentQueue<JobNotification>();
        service.NotificationReady += n => notifications.Enqueue(n);

        // Complete multiple jobs rapidly
        var ids = new List<string>();
        for (var i = 0; i < 5; i++) ids.Add(service.StartJob("MakePr", Path.GetTempPath()));

        foreach (var id in ids) service.CompleteJob(id, 0);

        Assert.Equal(5, notifications.Count);

        // All should be "MakePr Completed"
        while (notifications.TryDequeue(out var n))
        {
            Assert.Equal("MakePr Completed", n.Title);
            Assert.True(n.IsSuccess);
        }
    }

    [Fact]
    public void NotificationReady_FailedJob_DeliversFailureNotification()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var service = new JobService(
            TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10),
            null, 1);

        JobNotification? received = null;
        service.NotificationReady += n => received = n;

        var id = service.StartJob("ExecutePlan", Path.GetTempPath());
        service.CompleteJob(id, 1);

        Assert.NotNull(received);
        Assert.Equal("ExecutePlan Failed", received.Title);
        Assert.False(received.IsSuccess);
    }

    private class TestSynchronizationContext : SynchronizationContext
    {
        private readonly Queue<(SendOrPostCallback Callback, object? State)> _pending = new();
        public int PostCount { get; private set; }

        public override void Post(SendOrPostCallback d, object? state)
        {
            PostCount++;
            _pending.Enqueue((d, state));
        }

        public void ExecutePending()
        {
            while (_pending.Count > 0)
            {
                var (callback, state) = _pending.Dequeue();
                callback(state);
            }
        }
    }
}