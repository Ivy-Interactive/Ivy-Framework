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

        // Use StartJob with a real process that completes quickly, then simulate timeout via CompleteJob
        // Instead, directly test CompleteJob by starting a trivial job first
        var id = service.StartJob("ExecutePlan", Path.GetTempPath());
        var job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal("Running", job.Status);

        // Simulate timeout completion
        service.CompleteJob(id, exitCode: null, timedOut: true, staleOutput: false);

        job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal("Timeout", job.Status);
        Assert.Contains("30 minute timeout", job.StatusMessage);
        Assert.NotNull(job.CompletedAt);
        Assert.NotNull(job.DurationSeconds);
    }

    [Fact]
    public void CompleteJob_WithStaleOutput_SetsTimeoutStatusWithStaleReason()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.StartJob("ExecutePlan", Path.GetTempPath());

        // Simulate stale output timeout
        service.CompleteJob(id, exitCode: null, timedOut: true, staleOutput: true);

        var job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal("Timeout", job.Status);
        Assert.Contains("No output for 10 minutes", job.StatusMessage);
    }

    [Fact]
    public void CompleteJob_WithSuccessExitCode_SetsCompletedStatus()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.StartJob("ExecutePlan", Path.GetTempPath());

        service.CompleteJob(id, exitCode: 0);

        var job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal("Completed", job.Status);
        Assert.Null(job.StatusMessage);
    }

    [Fact]
    public void CompleteJob_WithNonZeroExitCode_SetsFailedStatus()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.StartJob("ExecutePlan", Path.GetTempPath());

        service.CompleteJob(id, exitCode: 1);

        var job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal("Failed", job.Status);
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
    public void ConfigService_ParsesJobTimeoutSettings()
    {
        var yaml = @"
tendrilData: D:\Tendril
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
tendrilData: D:\Tendril
";

        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
            .Build();
        var settings = deserializer.Deserialize<TendrilSettings>(yaml);

        Assert.Equal(30, settings.JobTimeout);
        Assert.Equal(10, settings.StaleOutputTimeout);
    }
}
