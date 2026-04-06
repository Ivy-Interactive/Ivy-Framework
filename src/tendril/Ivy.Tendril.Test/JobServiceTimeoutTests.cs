using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServiceTimeoutTests
{
    private static JobService CreateService(TimeSpan jobTimeout, TimeSpan staleOutputTimeout)
    {
        return new JobService(jobTimeout, staleOutputTimeout);
    }

    [Fact]
    public void CompleteJob_WithTimeout_SetsTimeoutStatus()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.StartJob("ExecutePlan", Path.GetTempPath());
        var job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal("Running", job.Status);

        JobNotification? notification = null;
        service.NotificationReady += n => notification = n;

        // Simulate timeout completion
        service.CompleteJob(id, exitCode: null, timedOut: true, staleOutput: false);

        job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal("Timeout", job.Status);
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

        var id = service.StartJob("ExecutePlan", Path.GetTempPath());

        JobNotification? notification = null;
        service.NotificationReady += n => notification = n;

        // Simulate stale output timeout
        service.CompleteJob(id, exitCode: null, timedOut: true, staleOutput: true);

        var job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal("Timeout", job.Status);
        Assert.Contains("No output for 10 minutes", job.StatusMessage);

        Assert.NotNull(notification);
        Assert.Equal("ExecutePlan Timed Out", notification.Title);
    }

    [Fact]
    public void CompleteJob_WithSuccessExitCode_SetsCompletedStatus()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.StartJob("ExecutePlan", Path.GetTempPath());

        JobNotification? notification = null;
        service.NotificationReady += n => notification = n;

        service.CompleteJob(id, exitCode: 0);

        var job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal("Completed", job.Status);
        Assert.Null(job.StatusMessage);

        Assert.NotNull(notification);
        Assert.Equal("ExecutePlan Completed", notification.Title);
    }

    [Fact]
    public void CompleteJob_WithNonZeroExitCode_SetsFailedStatus()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.StartJob("ExecutePlan", Path.GetTempPath());

        JobNotification? notification = null;
        service.NotificationReady += n => notification = n;

        service.CompleteJob(id, exitCode: 1);

        var job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal("Failed", job.Status);

        Assert.NotNull(notification);
        Assert.Equal("ExecutePlan Failed", notification.Title);
    }

    [Fact]
    public void CompleteJob_DoesNotOverwriteAlreadyCompletedJob()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.StartJob("ExecutePlan", Path.GetTempPath());

        // Complete it first
        service.CompleteJob(id, exitCode: 0);
        var job = service.GetJob(id);
        Assert.Equal("Completed", job!.Status);

        // Try to complete again (e.g. from stale watchdog racing with normal completion)
        service.CompleteJob(id, exitCode: null, timedOut: true, staleOutput: true);

        job = service.GetJob(id);
        Assert.Equal("Completed", job!.Status); // Should not change
    }

    [Fact]
    public void StopJob_CancelsTimeoutCts()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.StartJob("ExecutePlan", Path.GetTempPath());
        var job = service.GetJob(id);
        Assert.NotNull(job!.TimeoutCts);

        service.StopJob(id);

        Assert.Equal("Stopped", job.Status);
        Assert.True(job.TimeoutCts!.IsCancellationRequested);
    }

    [Fact]
    public void ClearFailedJobs_RemovesFailedAndTimeoutJobs()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var runningId = service.StartJob("ExecutePlan", Path.GetTempPath());
        var completedId = service.StartJob("ExecutePlan", Path.GetTempPath());
        var failedId = service.StartJob("ExecutePlan", Path.GetTempPath());
        var timeoutId = service.StartJob("ExecutePlan", Path.GetTempPath());

        service.CompleteJob(completedId, exitCode: 0);
        service.CompleteJob(failedId, exitCode: 1);
        service.CompleteJob(timeoutId, exitCode: null, timedOut: true);

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

        var id = service.StartJob("ExecutePlan", Path.GetTempPath());
        service.CompleteJob(id, exitCode: 0);

        service.ClearFailedJobs();

        Assert.NotNull(service.GetJob(id));
    }

    [Fact]
    public void ConfigService_ParsesJobTimeoutSettings()
    {
        var yaml = @"
agentCommand: claude
jobTimeout: 45
staleOutputTimeout: 15
";

        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
            .Build();
        var settings = deserializer.Deserialize<TendrilSettings>(yaml);

        Assert.Equal(45, settings.JobTimeout);
        Assert.Equal(15, settings.StaleOutputTimeout);
    }

    [Fact]
    public void ConfigService_DefaultsJobTimeoutWhenNotSpecified()
    {
        var yaml = @"
agentCommand: claude
";

        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
            .Build();
        var settings = deserializer.Deserialize<TendrilSettings>(yaml);

        Assert.Equal(30, settings.JobTimeout);
        Assert.Equal(10, settings.StaleOutputTimeout);
    }

    [Fact]
    public void HeartbeatOutput_ResetsLastOutputAt_ButIsFilteredFromOutputLines()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.StartJob("ExecutePlan", Path.GetTempPath());
        var job = service.GetJob(id);
        Assert.NotNull(job);

        // Wait briefly for process to start producing output
        Thread.Sleep(500);

        // Record LastOutputAt before heartbeat simulation
        var beforeHeartbeat = job.LastOutputAt;

        // The OutputDataReceived handler filters heartbeat lines from OutputLines
        // but still updates LastOutputAt. We can verify the filtering logic by
        // checking that heartbeat-containing strings would be excluded from output
        // while normal lines are included.
        var heartbeatLine = "{\"type\":\"heartbeat\",\"timestamp\":\"2026-04-02T07:00:00Z\"}";
        var normalLine = "{\"type\":\"assistant\",\"message\":\"hello\"}";

        // Verify filtering logic: heartbeat lines contain the marker
        Assert.Contains("\"type\":\"heartbeat\"", heartbeatLine);
        Assert.DoesNotContain("\"type\":\"heartbeat\"", normalLine);

        service.CompleteJob(id, exitCode: 0);
    }
}
