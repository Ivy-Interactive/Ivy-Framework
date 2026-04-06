using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServiceJobsChangedTests
{
    private static JobService CreateService()
    {
        SynchronizationContext.SetSynchronizationContext(null);
        return new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));
    }

    [Fact]
    public void CompleteJob_RaisesJobsChangedEvent()
    {
        var service = CreateService();
        var id = service.CreateTestJob("ExecutePlan", "test-plan");

        var fired = false;
        service.JobsChanged += () => fired = true;
        service.CompleteJob(id, exitCode: 0);

        Assert.True(fired);
    }

    [Fact]
    public void CompleteJob_Failure_RaisesJobsChangedEvent()
    {
        var service = CreateService();
        var id = service.CreateTestJob("ExecutePlan", "test-plan");

        var fired = false;
        service.JobsChanged += () => fired = true;
        service.CompleteJob(id, exitCode: 1);

        Assert.True(fired);
    }

    [Fact]
    public void StopJob_RaisesJobsChangedEvent()
    {
        var service = CreateService();
        var id = service.CreateTestJob("ExecutePlan", "test-plan");

        var fired = false;
        service.JobsChanged += () => fired = true;
        service.StopJob(id);

        Assert.True(fired);
    }
}
