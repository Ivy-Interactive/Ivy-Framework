using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServiceThreadSafetyTests
{
    private static JobService CreateServiceWithQueuedJob(out string jobId)
    {
        // Use maxConcurrentJobs=1 and start a fake job that gets queued by filling the slot first
        var service = new JobService(
            TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10),
            null, 1);

        // Start a job that will try to launch (and fail since no script exists).
        // Use try/catch since process launch may throw in test environment.
        string id;
        try
        {
            id = service.StartJob("MakePlan", "-Description", "Slot filler");
        }
        catch
        {
            // Even if the process fails to launch, the job was created
            id = "job-001";
        }

        jobId = id;
        return service;
    }

    [Fact]
    public void JobsChanged_FiresOnSyncContext_WhenAvailable()
    {
        var testContext = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(testContext);
        try
        {
            var service = new JobService(
                TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10),
                null, 1);

            var invoked = false;
            service.JobsChanged += () => invoked = true;

            // DeleteJob triggers RaiseJobsChanged — safe since no process is involved
            service.DeleteJob("nonexistent-id");

            // The event should have been posted to the sync context, not invoked directly
            Assert.False(invoked, "Handler should not be invoked synchronously when sync context is present");
            Assert.Equal(1, testContext.PostCount);

            // Execute the posted callback
            testContext.ExecutePending();
            Assert.True(invoked, "Handler should be invoked after sync context executes the callback");
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(null);
        }
    }

    [Fact]
    public void JobsChanged_FiresSynchronously_WhenNoSyncContext()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var service = new JobService(
            TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10),
            null, 1);

        var invoked = false;
        service.JobsChanged += () => invoked = true;

        // DeleteJob triggers RaiseJobsChanged
        service.DeleteJob("nonexistent-id");

        Assert.True(invoked, "Handler should be invoked synchronously when no sync context is present");
    }

    [Fact]
    public void JobsChanged_MultipleRapidInvocations_AllPostedToSyncContext()
    {
        var testContext = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(testContext);
        try
        {
            var service = new JobService(
                TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10),
                null, 1);

            var invokeCount = 0;
            service.JobsChanged += () => Interlocked.Increment(ref invokeCount);

            // Multiple deletes each raise RaiseJobsChanged
            service.DeleteJob("id-1");
            service.DeleteJob("id-2");
            service.DeleteJob("id-3");

            // All should be posted, not invoked yet
            Assert.Equal(0, invokeCount);
            Assert.Equal(3, testContext.PostCount);

            // Execute all pending callbacks
            testContext.ExecutePending();
            Assert.Equal(3, invokeCount);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(null);
        }
    }

    [Fact]
    public void CompleteJob_CalledTwice_DoesNotThrowSemaphoreFullException()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var service = new JobService(
            TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10),
            null, 2);

        var jobId = service.CreateTestJob("ExecutePlan", "test-plan");

        // First call should succeed and transition the job out of Running
        service.CompleteJob(jobId, 0);

        var job = service.GetJob(jobId);
        Assert.NotNull(job);
        Assert.Equal(JobStatus.Completed, job.Status);

        // Second call should be a no-op (status guard prevents double release)
        var ex = Record.Exception(() => service.CompleteJob(jobId, null, true, true));
        Assert.Null(ex);
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