using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServiceDeletionTests
{
    private static JobService CreateService(IPlanDatabaseService? database = null)
    {
        return new JobService(
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(10),
            database: database);
    }

    private static void AddJobDirectly(JobService service, JobItem job)
    {
        var field = typeof(JobService).GetField("_jobs",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var jobs = (System.Collections.Concurrent.ConcurrentDictionary<string, JobItem>)field!.GetValue(service)!;
        jobs[job.Id] = job;
    }

    [Fact]
    public void DeleteJob_RemovesFromMemoryAndCallsDatabase()
    {
        var db = new FakeDatabaseService();
        var service = CreateService(db);
        AddJobDirectly(service, new JobItem { Id = "job-1", Status = JobStatus.Completed });

        service.DeleteJob("job-1");

        Assert.Null(service.GetJob("job-1"));
        Assert.Contains("job-1", db.DeletedJobIds);
    }

    [Fact]
    public void DeleteJob_NonexistentId_DoesNotCallDatabase()
    {
        var db = new FakeDatabaseService();
        var service = CreateService(db);

        service.DeleteJob("nonexistent");

        Assert.Empty(db.DeletedJobIds);
    }

    [Fact]
    public void ClearCompletedJobs_DeletesFromMemoryAndDatabase()
    {
        var db = new FakeDatabaseService();
        var service = CreateService(db);
        AddJobDirectly(service, new JobItem { Id = "completed-1", Status = JobStatus.Completed });
        AddJobDirectly(service, new JobItem { Id = "completed-2", Status = JobStatus.Completed });
        AddJobDirectly(service, new JobItem { Id = "running-1", Status = JobStatus.Running });

        service.ClearCompletedJobs();

        Assert.Null(service.GetJob("completed-1"));
        Assert.Null(service.GetJob("completed-2"));
        Assert.NotNull(service.GetJob("running-1"));
        Assert.Contains("completed-1", db.DeletedJobIds);
        Assert.Contains("completed-2", db.DeletedJobIds);
        Assert.DoesNotContain("running-1", db.DeletedJobIds);
    }

    [Fact]
    public void ClearFailedJobs_DeletesFromMemoryAndDatabase()
    {
        var db = new FakeDatabaseService();
        var service = CreateService(db);
        AddJobDirectly(service, new JobItem { Id = "failed-1", Status = JobStatus.Failed });
        AddJobDirectly(service, new JobItem { Id = "timeout-1", Status = JobStatus.Timeout });
        AddJobDirectly(service, new JobItem { Id = "blocked-1", Status = JobStatus.Blocked });
        AddJobDirectly(service, new JobItem { Id = "completed-1", Status = JobStatus.Completed });

        service.ClearFailedJobs();

        Assert.Null(service.GetJob("failed-1"));
        Assert.Null(service.GetJob("timeout-1"));
        Assert.Null(service.GetJob("blocked-1"));
        Assert.NotNull(service.GetJob("completed-1"));
        Assert.Contains("failed-1", db.DeletedJobIds);
        Assert.Contains("timeout-1", db.DeletedJobIds);
        Assert.Contains("blocked-1", db.DeletedJobIds);
        Assert.DoesNotContain("completed-1", db.DeletedJobIds);
    }

    [Fact]
    public void DeleteJob_DatabaseThrows_StillRemovesFromMemory()
    {
        var db = new FakeDatabaseService { ThrowOnDelete = true };
        var service = CreateService(db);
        AddJobDirectly(service, new JobItem { Id = "job-1", Status = JobStatus.Completed });

        service.DeleteJob("job-1");

        Assert.Null(service.GetJob("job-1"));
    }

    private class FakeDatabaseService : IPlanDatabaseService
    {
        public List<string> DeletedJobIds { get; } = new();
        public bool ThrowOnDelete { get; init; }

        public void DeleteJob(string id)
        {
            if (ThrowOnDelete) throw new Exception("DB error");
            DeletedJobIds.Add(id);
        }

        public void Dispose() { }
        public List<PlanFile> GetPlans(PlanStatus? statusFilter = null) => new();
        public PlanFile? GetPlanByFolder(string folderPath) => null;
        public PlanFile? GetPlanById(int planId) => null;
        public PlanReaderService.PlanCountSnapshot ComputePlanCounts() => new(0, 0, 0, 0, 0);
        public DashboardStats GetDashboardData(string? projectFilter) => new(0, 0, 0, 0, 0, 0, 0, new(), new());
        public decimal GetPlanTotalCost(int planId) => 0;
        public int GetPlanTotalTokens(int planId) => 0;
        public List<HourlyTokenBurn> GetHourlyTokenBurn(int days = 7, string? projectFilter = null) => new();
        public List<Recommendation> GetRecommendations() => new();
        public int GetPendingRecommendationsCount() => 0;
        public List<PlanFile> SearchPlans(string query) => new();
        public void RebuildFtsIndex() { }
        public void UpdatePlanState(int planId, PlanStatus state) { }
        public void UpdatePlanContent(int planId, string latestRevisionContent, int revisionCount) { }
        public void UpdateRecommendationState(int planId, string recommendationTitle, string newState, string? declineReason) { }
        public void UpsertPlan(PlanFile plan) { }
        public void DeletePlan(int planId) { }
        public void UpsertCosts(int planId, List<CostEntry> costs) { }
        public void UpsertRecommendations(int planId, string folderName, List<RecommendationYaml> recommendations, string project, string planTitle, DateTime updated, PlanStatus status) { }
        public void BulkUpsertPlans(List<PlanFile> plans, bool forceOverwrite = false) { }
        public void UpsertJob(JobItem job) { }
        public List<JobItem> GetRecentJobs(int limit = 100) => new();
        public void PurgeOldJobs(int keepCount = 500) { }
        public long GetDatabaseSize() => 0;
        public DateTime GetLastSyncTime() => DateTime.MinValue;
        public void SetLastSyncTime(DateTime time) { }
    }
}
