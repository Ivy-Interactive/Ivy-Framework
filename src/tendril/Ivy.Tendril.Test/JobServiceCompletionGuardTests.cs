using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServiceCompletionGuardTests
{
    private static JobService CreateService()
    {
        SynchronizationContext.SetSynchronizationContext(null);
        return new JobService(
            TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));
    }

    private static JobService CreateServiceWithPlanReader(string plansDir)
    {
        SynchronizationContext.SetSynchronizationContext(null);
        return new JobService(
            TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10),
            planReaderService: new StubPlanReaderService(plansDir));
    }

    [Fact]
    public void CompleteJob_ConcurrentCalls_OnlyFirstCompletes()
    {
        var service = CreateService();
        var id = service.CreateTestJob("ExecutePlan", "test-plan");

        var notificationCount = 0;
        service.NotificationReady += _ => Interlocked.Increment(ref notificationCount);

        using var barrier = new Barrier(2);
        var statuses = new JobStatus?[2];

        var t1 = new Thread(() =>
        {
            barrier.SignalAndWait();
            service.CompleteJob(id, 0);
            statuses[0] = service.GetJob(id)?.Status;
        });

        var t2 = new Thread(() =>
        {
            barrier.SignalAndWait();
            service.CompleteJob(id, 1);
            statuses[1] = service.GetJob(id)?.Status;
        });

        t1.Start();
        t2.Start();
        t1.Join(TimeSpan.FromSeconds(5));
        t2.Join(TimeSpan.FromSeconds(5));

        var job = service.GetJob(id);
        Assert.NotNull(job);
        // Only one thread should have completed the job — status should be either Completed or Failed, not both
        Assert.True(job.Status is JobStatus.Completed or JobStatus.Failed);
        // Only one notification should have fired
        Assert.Equal(1, notificationCount);
    }

    [Fact]
    public void StopJob_RacingWithCompleteJob_OnlyOneWins()
    {
        var service = CreateService();
        var id = service.CreateTestJob("ExecutePlan", "test-plan");

        var notificationCount = 0;
        service.NotificationReady += _ => Interlocked.Increment(ref notificationCount);

        using var barrier = new Barrier(2);

        var t1 = new Thread(() =>
        {
            barrier.SignalAndWait();
            service.StopJob(id);
        });

        var t2 = new Thread(() =>
        {
            barrier.SignalAndWait();
            service.CompleteJob(id, 0);
        });

        t1.Start();
        t2.Start();
        t1.Join(TimeSpan.FromSeconds(5));
        t2.Join(TimeSpan.FromSeconds(5));

        var job = service.GetJob(id);
        Assert.NotNull(job);
        // Status should be one terminal state — not corrupted
        Assert.True(job.Status is JobStatus.Stopped or JobStatus.Completed);
    }

    [Fact]
    public void CompleteJob_AfterStopJob_IsNoOp()
    {
        var service = CreateService();
        var id = service.CreateTestJob("ExecutePlan", "test-plan");

        service.StopJob(id);
        var job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal(JobStatus.Stopped, job.Status);

        // CompleteJob after StopJob should be a no-op
        service.CompleteJob(id, 0);

        job = service.GetJob(id);
        Assert.Equal(JobStatus.Stopped, job!.Status);
    }

    [Fact]
    public void CompleteJob_StaleOutputDetected_SetsTimeoutWithStaleReason()
    {
        var service = CreateService();
        var id = service.CreateTestJob("ExecutePlan", "test-plan");

        var job = service.GetJob(id);
        Assert.NotNull(job);
        job.StaleOutputDetected = true;

        service.CompleteJob(id, null, timedOut: true, staleOutput: true);

        job = service.GetJob(id);
        Assert.NotNull(job);
        Assert.Equal(JobStatus.Timeout, job.Status);
        Assert.Contains("No output for 10 minutes", job.StatusMessage);
    }

    [Fact]
    public void CompleteJob_MakePlan_UpdatesPlanFileWhenOutputContainsPlanCreated()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var service = CreateServiceWithPlanReader(tempDir);
            var id = service.CreateTestJob("MakePlan", "-Description", "Fix login bug", "-Project", "Tendril");

            var job = service.GetJob(id);
            Assert.NotNull(job);
            job.EnqueueOutput("Processing...");
            job.EnqueueOutput("Plan created: 02353-FixLoginBug");

            service.CompleteJob(id, 0);

            job = service.GetJob(id);
            Assert.NotNull(job);
            Assert.Equal("02353-FixLoginBug", job.PlanFile);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void CompleteJob_MakePlan_LeavesPlanFileUnchangedOnDuplicate()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var service = CreateServiceWithPlanReader(tempDir);
            var id = service.CreateTestJob("MakePlan", "-Description", "Fix login bug", "-Project", "Tendril");

            var job = service.GetJob(id);
            Assert.NotNull(job);
            var originalPlanFile = job.PlanFile;
            job.EnqueueOutput("Processing...");
            job.EnqueueOutput("identified as duplicate: 01234-ExistingPlan");

            service.CompleteJob(id, 0);

            job = service.GetJob(id);
            Assert.NotNull(job);
            Assert.Equal(originalPlanFile, job.PlanFile);
            Assert.Equal(JobStatus.Completed, job.Status);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private class StubPlanReaderService(string plansDirectory) : IPlanReaderService
    {
        public string PlansDirectory => plansDirectory;
        public bool IsDatabaseReady => true;
        public void RecoverStuckPlans() { }
        public void RepairPlans() { }
        public List<PlanFile> GetPlans(PlanStatus? statusFilter = null) => [];
        public PlanFile? GetPlanByFolder(string folderPath) => null;
        public List<PlanFile> GetIceboxPlans() => [];
        public void TransitionState(string folderName, PlanStatus newState) { }
        public void SaveRevision(string folderName, string content) { }
        public string ReadLatestRevision(string folderName) => "";
        public List<(int Number, string Content, DateTime Modified)> GetRevisions(string folderName) => [];
        public void AddLog(string folderName, string action, string content) { }
        public void DeletePlan(string folderName) { }
        public string ReadRawPlan(string folderName) => "";
        public void SavePlan(string folderName, string fullContent) { }
        public void UpdateLatestRevision(string folderName, string content) { }
        public DashboardStats GetDashboardData(string? projectFilter) => new(0, 0, 0, 0, 0, 0, 0, [], []);
        public decimal GetPlanTotalCost(string folderPath) => 0;
        public int GetPlanTotalTokens(string folderPath) => 0;
        public List<HourlyTokenBurn> GetHourlyTokenBurn(int days = 7, string? projectFilter = null) => [];
        public List<Recommendation> GetRecommendations() => [];
        public int GetPendingRecommendationsCount() => 0;
        public PlanReaderService.PlanCountSnapshot ComputePlanCounts() => new(0, 0, 0, 0, 0);
        public void UpdateRecommendationState(string planFolderName, string recommendationTitle, string newState, string? declineReason = null) { }
        public void InvalidateCaches() { }
    }
}
