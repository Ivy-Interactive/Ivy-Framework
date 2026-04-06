using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServiceNotificationTests
{
    private static JobService CreateService()
    {
        // Clear sync context so notifications fire synchronously during tests
        SynchronizationContext.SetSynchronizationContext(null);
        return new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));
    }

    [Fact]
    public void CompleteJob_Success_NotificationTitleIncludesJobType()
    {
        var service = CreateService();
        var id = service.CreateTestJob("MakePr", Path.GetTempPath());

        JobNotification? notification = null;
        service.NotificationReady += n => notification = n;
        service.CompleteJob(id, exitCode: 0);

        Assert.NotNull(notification);
        Assert.Equal("MakePr Completed", notification.Title);
        Assert.True(notification.IsSuccess);
    }

    [Fact]
    public void CompleteJob_Failure_NotificationTitleIncludesJobType()
    {
        var service = CreateService();
        var id = service.CreateTestJob("ExecutePlan", Path.GetTempPath());

        JobNotification? notification = null;
        service.NotificationReady += n => notification = n;
        service.CompleteJob(id, exitCode: 1);

        Assert.NotNull(notification);
        Assert.Equal("ExecutePlan Failed", notification.Title);
        Assert.False(notification.IsSuccess);
    }

    [Fact]
    public void CompleteJob_Timeout_NotificationTitleIncludesJobType()
    {
        var service = CreateService();
        var id = service.CreateTestJob("ExpandPlan", Path.GetTempPath());

        JobNotification? notification = null;
        service.NotificationReady += n => notification = n;
        service.CompleteJob(id, exitCode: null, timedOut: true);

        Assert.NotNull(notification);
        Assert.Equal("ExpandPlan Timed Out", notification.Title);
        Assert.False(notification.IsSuccess);
    }
}
