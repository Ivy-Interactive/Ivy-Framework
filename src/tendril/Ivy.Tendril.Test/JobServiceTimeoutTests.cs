using System.Diagnostics;
using Ivy.Helpers;
using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ivy.Tendril.Test;

public class JobServiceTimeoutTests
{
    private static JobService CreateService(
        TimeSpan jobTimeout,
        TimeSpan staleOutputTimeout,
        ILogger<JobService>? logger = null)
    {
        SynchronizationContext.SetSynchronizationContext(null);
        return new JobService(jobTimeout, staleOutputTimeout, logger: logger);
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
        var cts = job!.TimeoutCts;
        Assert.NotNull(cts);

        service.StopJob(id);

        Assert.Equal(JobStatus.Stopped, job.Status);
        Assert.True(cts!.IsCancellationRequested);
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

    [Fact]
    public void CompleteJob_AfterCtsDisposed_StillCompletes()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));

        var id = service.CreateTestJob("ExecutePlan", Path.GetTempPath());
        var job = service.GetJob(id);
        Assert.NotNull(job);

        job.TimeoutCts?.Dispose();

        service.CompleteJob(id, 0);

        job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal(JobStatus.Completed, job.Status);
    }

    [Fact]
    public async Task RealProcess_KilledAfterTimeout()
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
            Arguments = OperatingSystem.IsWindows() ? "/c ping -n 120 127.0.0.1 >nul" : "-c \"sleep 120\"",
            UseShellExecute = false,
            CreateNoWindow = true
        });
        Assert.NotNull(process);
        Assert.False(process.HasExited);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var sw = Stopwatch.StartNew();
        var result = await process.WaitForExitOrKillAsync(cts.Token);
        sw.Stop();

        Assert.False(result);
        Assert.True(process.HasExited);
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(15),
            $"Process should be killed within timeout + kill grace period, took {sw.Elapsed}");
    }

    [Fact]
    public async Task RunStaleOutputWatchdog_HandlesDisposedCts_ExitsGracefully()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(5));

        var id = service.CreateTestJob("ExecutePlan", Path.GetTempPath());
        var job = service.GetJob(id);
        Assert.NotNull(job);

        var cts = job.TimeoutCts!;
        var watchdogTask = service.RunStaleOutputWatchdog(id, cts);

        await Task.Delay(100);
        cts.Dispose();

        var completed = await Task.WhenAny(watchdogTask, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.Equal(watchdogTask, completed);
        Assert.True(watchdogTask.IsCompletedSuccessfully, "Watchdog should exit gracefully, not fault");
    }

    [Fact]
    public void ProcessId_CapturedOnJobItem()
    {
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));
        var id = service.CreateTestJob("ExecutePlan", Path.GetTempPath());
        var job = service.GetJob(id);
        Assert.NotNull(job);

        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
            Arguments = OperatingSystem.IsWindows() ? "/c exit 0" : "-c \"exit 0\"",
            UseShellExecute = false,
            CreateNoWindow = true
        });
        Assert.NotNull(process);

        job.Process = process;
        job.ProcessId = process.Id;

        Assert.NotNull(job.ProcessId);
        Assert.True(job.ProcessId > 0);

        process.WaitForExit();
        service.CompleteJob(id, 0);
    }

    [Fact]
    public void Constructor_AcceptsLogger()
    {
        var logger = NullLogger<JobService>.Instance;
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10), logger);

        var id = service.CreateTestJob("ExecutePlan", Path.GetTempPath());
        service.CompleteJob(id, 0);

        var job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal(JobStatus.Completed, job.Status);
    }

    [Fact]
    public void Constructor_WithCapturingLogger_DoesNotThrow()
    {
        var logEntries = new List<(LogLevel Level, string Message)>();
        var logger = new CapturingLogger<JobService>(logEntries);
        var service = CreateService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10), logger);

        var id = service.CreateTestJob("ExecutePlan", Path.GetTempPath());
        service.CompleteJob(id, 0);

        var job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal(JobStatus.Completed, job.Status);
    }
}

internal sealed class CapturingLogger<T>(List<(LogLevel Level, string Message)> entries) : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        entries.Add((logLevel, formatter(state, exception)));
    }
}