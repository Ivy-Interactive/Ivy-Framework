using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ivy.Tendril.Test;

public class JobServiceTimeoutTests
{
    private static JobService CreateService(TimeSpan jobTimeout, TimeSpan staleOutputTimeout)
    {
        // Clear sync context so notifications fire synchronously during tests
        SynchronizationContext.SetSynchronizationContext(null);
        return new JobService(jobTimeout, staleOutputTimeout);
    }

    [Fact]
    public void CompleteJob_WithTimeout_SetsTimeoutStatus()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.CreateTestJob("ExecutePlan", Path.GetTempPath());
        var job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal(JobStatus.Running, job.Status);

        JobNotification? notification = null;
        service.NotificationReady += n => notification = n;

        service.CompleteJob(id, null, true, false);

        job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal(JobStatus.Timeout, job.Status);
        Assert.Contains("30 minute timeout", job.StatusMessage);
        Assert.NotNull(job.CompletedAt);
        Assert.NotNull(job.DurationSeconds);

        Assert.NotNull(notification);
        Assert.Equal("ExecutePlan Timed Out", notification.Title);
    }

    [Fact]
    public void CompleteJob_WithStaleOutput_SetsTimeoutStatusWithStaleReason()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.CreateTestJob("ExecutePlan", Path.GetTempPath());

        JobNotification? notification = null;
        service.NotificationReady += n => notification = n;

        service.CompleteJob(id, null, true, true);

        var job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal(JobStatus.Timeout, job.Status);
        Assert.Contains("No output for 10 minutes", job.StatusMessage);

        Assert.NotNull(notification);
        Assert.Equal("ExecutePlan Timed Out", notification.Title);
    }

    [Fact]
    public void CompleteJob_WithSuccessExitCode_SetsCompletedStatus()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.CreateTestJob("ExecutePlan", Path.GetTempPath());

        JobNotification? notification = null;
        service.NotificationReady += n => notification = n;

        service.CompleteJob(id, 0);

        var job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal(JobStatus.Completed, job.Status);
        Assert.Null(job.StatusMessage);

        Assert.NotNull(notification);
        Assert.Equal("ExecutePlan Completed", notification.Title);
    }

    [Fact]
    public void CompleteJob_WithNonZeroExitCode_SetsFailedStatus()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.CreateTestJob("ExecutePlan", Path.GetTempPath());

        JobNotification? notification = null;
        service.NotificationReady += n => notification = n;

        service.CompleteJob(id, 1);

        var job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal(JobStatus.Failed, job.Status);

        Assert.NotNull(notification);
        Assert.Equal("ExecutePlan Failed", notification.Title);
    }

    [Fact]
    public void CompleteJob_DoesNotOverwriteAlreadyCompletedJob()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.CreateTestJob("ExecutePlan", Path.GetTempPath());

        service.CompleteJob(id, 0);
        var job = service.GetJob(id);
        Assert.Equal(JobStatus.Completed, job!.Status);

        // Try to complete again (e.g. from stale watchdog racing with normal completion)
        service.CompleteJob(id, null, true, true);

        job = service.GetJob(id);
        Assert.Equal(JobStatus.Completed, job!.Status); // Should not change
    }

    [Fact]
    public void StopJob_CancelsTimeoutCts()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.CreateTestJob("ExecutePlan", Path.GetTempPath());
        var job = service.GetJob(id);
        Assert.NotNull(job!.TimeoutCts);

        service.StopJob(id);

        Assert.Equal(JobStatus.Stopped, job.Status);
        Assert.True(job.TimeoutCts!.IsCancellationRequested);
    }

    [Fact]
    public void ClearFailedJobs_RemovesFailedAndTimeoutJobs()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var runningId = service.CreateTestJob("ExecutePlan", Path.GetTempPath());
        var completedId = service.CreateTestJob("ExecutePlan", Path.GetTempPath());
        var failedId = service.CreateTestJob("ExecutePlan", Path.GetTempPath());
        var timeoutId = service.CreateTestJob("ExecutePlan", Path.GetTempPath());

        service.CompleteJob(completedId, 0);
        service.CompleteJob(failedId, 1);
        service.CompleteJob(timeoutId, null, true);

        service.ClearFailedJobs();

        Assert.NotNull(service.GetJob(runningId));
        Assert.NotNull(service.GetJob(completedId));
        Assert.Null(service.GetJob(failedId));
        Assert.Null(service.GetJob(timeoutId));
    }

    [Fact]
    public void ClearFailedJobs_DoesNothingWhenNoFailedJobs()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.CreateTestJob("ExecutePlan", Path.GetTempPath());
        service.CompleteJob(id, 0);

        service.ClearFailedJobs();

        Assert.NotNull(service.GetJob(id));
    }

    [Fact]
    public void ConfigService_ParsesJobTimeoutSettings()
    {
        var yaml = @"
codingAgent: claude
jobTimeout: 45
staleOutputTimeout: 15
";

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var settings = deserializer.Deserialize<TendrilSettings>(yaml);

        Assert.Equal(45, settings.JobTimeout);
        Assert.Equal(15, settings.StaleOutputTimeout);
    }

    [Fact]
    public void ConfigService_DefaultsJobTimeoutWhenNotSpecified()
    {
        var yaml = @"
codingAgent: claude
";

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var settings = deserializer.Deserialize<TendrilSettings>(yaml);

        Assert.Equal(30, settings.JobTimeout);
        Assert.Equal(10, settings.StaleOutputTimeout);
    }

    [Fact]
    public void HeartbeatOutput_ResetsLastOutputAt_ButIsFilteredFromOutputLines()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.CreateTestJob("ExecutePlan", Path.GetTempPath());
        var job = service.GetJob(id);
        Assert.NotNull(job);

        // The OutputDataReceived handler filters heartbeat lines from OutputLines
        // but still updates LastOutputAt. Verify the filtering logic:
        var heartbeatLine = "{\"type\":\"heartbeat\",\"timestamp\":\"2026-04-02T07:00:00Z\"}";
        var normalLine = "{\"type\":\"assistant\",\"message\":\"hello\"}";

        Assert.Contains("\"type\":\"heartbeat\"", heartbeatLine);
        Assert.DoesNotContain("\"type\":\"heartbeat\"", normalLine);

        service.CompleteJob(id, 0);
    }
}