using System.Collections.Concurrent;
using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ivy.Tendril.Test;

public class PlanCountsServiceTests : IDisposable
{
    private readonly FakeJobService _jobService;
    private readonly PlanReaderService _planReader;
    private readonly FakePlanWatcherService _planWatcher;
    private readonly string _plansDir;
    private readonly string _tempDir;

    public PlanCountsServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tendril-counts-test-{Guid.NewGuid()}");
        _plansDir = Path.Combine(_tempDir, "Plans");
        Directory.CreateDirectory(_plansDir);

        var settings = new TendrilSettings();
        var configService = new ConfigService(settings, _tempDir);
        _planReader = new PlanReaderService(configService, NullLogger<PlanReaderService>.Instance);
        _jobService = new FakeJobService();
        _planWatcher = new FakePlanWatcherService();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private void CreatePlan(string folderName, string state)
    {
        var dir = Path.Combine(_plansDir, folderName);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "plan.yaml"),
            $"state: {state}\nproject: Tendril\ntitle: Test\nrepos: []\ncommits: []\nprs: []\nverifications: []\nrelatedPlans: []\ndependsOn: []\ncreated: 2026-01-01T00:00:00Z\nupdated: 2026-01-01T00:00:00Z\n");

        var revisionsDir = Path.Combine(dir, "revisions");
        Directory.CreateDirectory(revisionsDir);
        File.WriteAllText(Path.Combine(revisionsDir, "001.md"), "# Test");
    }

    private void CreateRecommendations(string folderName, string yaml)
    {
        var artifactsDir = Path.Combine(_plansDir, folderName, "artifacts");
        Directory.CreateDirectory(artifactsDir);
        File.WriteAllText(Path.Combine(artifactsDir, "recommendations.yaml"), yaml);
    }

    private void AddJob(string id, JobStatus status)
    {
        _jobService.AddJob(id, status);
    }

    private PlanCountsService CreateService()
    {
        return new PlanCountsService(_planReader, _jobService, _planWatcher);
    }

    [Fact]
    public void ComputeCounts_WithNoPlans_ReturnsAllZeros()
    {
        using var service = CreateService();

        Assert.Equal(0, service.Current.Drafts);
        Assert.Equal(0, service.Current.ActiveJobs);
        Assert.Equal(0, service.Current.Reviews);
        Assert.Equal(0, service.Current.Icebox);
        Assert.Equal(0, service.Current.Recommendations);
    }

    [Fact]
    public void ComputeCounts_WithFailedPlans_CountsOnlyInReviews()
    {
        // This is the CRITICAL regression test:
        // Failed plans must be counted ONLY in Reviews, NOT in Drafts.
        CreatePlan("00001-FailedPlanA", "Failed");
        CreatePlan("00002-FailedPlanB", "Failed");

        using var service = CreateService();

        Assert.Equal(0, service.Current.Drafts);
        Assert.Equal(2, service.Current.Reviews);
    }

    [Fact]
    public void ComputeCounts_WithVariousStates_AggregatesCorrectly()
    {
        // Drafts
        CreatePlan("00001-DraftPlan", "Draft");
        CreatePlan("00002-AnotherDraft", "Draft");

        // Reviews (ReadyForReview + Failed)
        CreatePlan("00003-ReviewPlan", "ReadyForReview");
        CreatePlan("00004-FailedPlan", "Failed");

        // Icebox
        CreatePlan("00005-IceboxPlan", "Icebox");

        // Completed (should not appear in any badge)
        CreatePlan("00006-CompletedPlan", "Completed");

        // Recommendations (pending on a completed plan)
        CreatePlan("00007-WithRecs", "Completed");
        CreateRecommendations("00007-WithRecs",
            "- title: Fix something\n  description: |\n    Details here.\n  state: Pending\n");

        // Jobs
        AddJob("job-1", JobStatus.Running);
        AddJob("job-2", JobStatus.Queued);
        AddJob("job-3", JobStatus.Completed); // should not count as active
        AddJob("job-4", JobStatus.Blocked); // should count as active

        using var service = CreateService();

        Assert.Equal(2, service.Current.Drafts);
        Assert.Equal(3, service.Current.ActiveJobs);
        Assert.Equal(2, service.Current.Reviews); // 1 ReadyForReview + 1 Failed
        Assert.Equal(1, service.Current.Icebox);
        Assert.Equal(1, service.Current.Recommendations);
    }

    [Fact]
    public void ComputeCounts_BlockedPlansCountAsDrafts()
    {
        CreatePlan("00010-DraftPlan", "Draft");
        CreatePlan("00011-BlockedPlan1", "Blocked");
        CreatePlan("00012-BlockedPlan2", "Blocked");

        using var service = CreateService();

        // Blocked plans should be counted together with drafts
        Assert.Equal(3, service.Current.Drafts);
    }

    [Fact]
    public void ComputeCounts_WithRunningQueuedAndBlockedJobs_CountsActiveJobs()
    {
        AddJob("job-running-1", JobStatus.Running);
        AddJob("job-running-2", JobStatus.Running);
        AddJob("job-queued-1", JobStatus.Queued);
        AddJob("job-blocked-1", JobStatus.Blocked);
        AddJob("job-completed-1", JobStatus.Completed);
        AddJob("job-failed-1", JobStatus.Failed);
        AddJob("job-pending-1", JobStatus.Pending);

        using var service = CreateService();

        // Running + Queued + Blocked count as active
        Assert.Equal(4, service.Current.ActiveJobs);
    }

    [Fact]
    public void ComputeCounts_RefreshUpdatesCountsWhenPlansChange()
    {
        using var service = CreateService();

        // Initially no plans
        Assert.Equal(0, service.Current.Drafts);

        // Add a draft plan and trigger refresh
        CreatePlan("00001-NewDraft", "Draft");
        _planWatcher.RaisePlansChanged();

        Assert.Equal(1, service.Current.Drafts);
    }

    private class FakeJobService : IJobService
    {
        private readonly List<JobItem> _jobs = new();

#pragma warning disable CS0618
        public ConcurrentQueue<JobNotification> PendingNotifications { get; } = new();
#pragma warning restore CS0618

        public List<JobItem> GetJobs()
        {
            return _jobs;
        }

        public JobItem? GetJob(string id)
        {
            return _jobs.FirstOrDefault(j => j.Id == id);
        }

        public string StartJob(string type, string[] args, string? inboxFilePath)
        {
            throw new NotImplementedException();
        }

        public string StartJob(string type, params string[] args)
        {
            throw new NotImplementedException();
        }

        public void CompleteJob(string id, int? exitCode, bool timedOut = false, bool staleOutput = false)
        {
            throw new NotImplementedException();
        }

        public void StopJob(string id)
        {
            throw new NotImplementedException();
        }

        public void DeleteJob(string id)
        {
            throw new NotImplementedException();
        }

        public void ClearCompletedJobs()
        {
            throw new NotImplementedException();
        }

        public void ClearFailedJobs()
        {
            throw new NotImplementedException();
        }

        public bool IsInboxFileTracked(string filePath)
        {
            throw new NotImplementedException();
        }

        public void AddJob(string id, JobStatus status)
        {
            _jobs.Add(new JobItem { Id = id, Status = status });
        }

#pragma warning disable CS0067
        public event Action? JobsChanged;
        public event Action<JobNotification>? NotificationReady;
#pragma warning restore CS0067

        public void Dispose() { }
    }

    private class FakePlanWatcherService : IPlanWatcherService
    {
        public event Action<string?>? PlansChanged;

        public void NotifyChanged(string? changedPlanFolder = null)
        {
            PlansChanged?.Invoke(changedPlanFolder);
        }

        public void Dispose()
        {
        }

        public void RaisePlansChanged()
        {
            PlansChanged?.Invoke(null);
        }
    }
}