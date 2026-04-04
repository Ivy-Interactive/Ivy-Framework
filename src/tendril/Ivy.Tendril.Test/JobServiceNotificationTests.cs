using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServiceNotificationTests
{
    private static JobService CreateService()
    {
        return new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));
    }

    [Fact]
    public void CompleteJob_Success_NotificationTitleIncludesJobType()
    {
        var service = CreateService();
        var id = service.StartJob("MakePr", Path.GetTempPath());

        service.CompleteJob(id, exitCode: 0);

        Assert.True(service.PendingNotifications.TryDequeue(out var notification));
        Assert.Equal("MakePr Completed", notification.Title);
        Assert.True(notification.IsSuccess);
    }

    [Fact]
    public void CompleteJob_Failure_NotificationTitleIncludesJobType()
    {
        var service = CreateService();
        var id = service.StartJob("ExecutePlan", Path.GetTempPath());

        service.CompleteJob(id, exitCode: 1);

        Assert.True(service.PendingNotifications.TryDequeue(out var notification));
        Assert.Equal("ExecutePlan Failed", notification.Title);
        Assert.False(notification.IsSuccess);
    }

    [Fact]
    public void CompleteJob_Timeout_NotificationTitleIncludesJobType()
    {
        var service = CreateService();
        var id = service.StartJob("ExpandPlan", Path.GetTempPath());

        service.CompleteJob(id, exitCode: null, timedOut: true);

        Assert.True(service.PendingNotifications.TryDequeue(out var notification));
        Assert.Equal("ExpandPlan Timed Out", notification.Title);
        Assert.False(notification.IsSuccess);
    }
}
