using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServiceLogTests
{
    [Fact]
    public void WriteJobLog_IncludesSessionId()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var configService = new ConfigService(new TendrilSettings(), tempDir);
            var planReaderService = new PlanReaderService(configService, Microsoft.Extensions.Logging.Abstractions.NullLogger<PlanReaderService>.Instance);
            var jobService = new JobService(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(1), planReaderService: planReaderService);

            var sessionId = Guid.NewGuid().ToString();
            var job = new JobItem
            {
                Id = "1",
                Type = "ExecutePlan",
                PlanFile = "00001-TestPlan",
                Status = JobStatus.Completed,
                StartedAt = DateTime.UtcNow.AddMinutes(-2),
                CompletedAt = DateTime.UtcNow,
                DurationSeconds = 120,
                SessionId = sessionId
            };

            jobService.WriteJobLog(job);

            var logsDir = Path.Combine(tempDir, "Plans", "00001-TestPlan", "logs");
            var logFiles = Directory.GetFiles(logsDir, "*.md");
            Assert.Single(logFiles);

            var logContent = File.ReadAllText(logFiles[0]);
            Assert.Contains("**SessionId:**", logContent);
            Assert.Contains(sessionId, logContent);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void WriteJobLog_OmitsSessionIdWhenNull()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var configService = new ConfigService(new TendrilSettings(), tempDir);
            var planReaderService = new PlanReaderService(configService, Microsoft.Extensions.Logging.Abstractions.NullLogger<PlanReaderService>.Instance);
            var jobService = new JobService(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(1), planReaderService: planReaderService);

            var job = new JobItem
            {
                Id = "2",
                Type = "ExecutePlan",
                PlanFile = "00002-TestPlan",
                Status = JobStatus.Completed,
                StartedAt = DateTime.UtcNow.AddMinutes(-1),
                CompletedAt = DateTime.UtcNow,
                DurationSeconds = 60,
                SessionId = null
            };

            jobService.WriteJobLog(job);

            var logsDir = Path.Combine(tempDir, "Plans", "00002-TestPlan", "logs");
            var logFiles = Directory.GetFiles(logsDir, "*.md");
            Assert.Single(logFiles);

            var logContent = File.ReadAllText(logFiles[0]);
            Assert.DoesNotContain("**SessionId:**", logContent);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
