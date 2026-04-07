using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Apps.Plans;

namespace Ivy.Tendril.Services;

public interface IPlanDatabaseService : IDisposable
{
    // Plan queries
    List<PlanFile> GetPlans(PlanStatus? statusFilter = null);
    PlanFile? GetPlanByFolder(string folderPath);
    PlanFile? GetPlanById(int planId);

    // Aggregates
    PlanReaderService.PlanCountSnapshot ComputePlanCounts();

    // Costs and tokens
    decimal GetPlanTotalCost(int planId);
    int GetPlanTotalTokens(int planId);
    List<HourlyTokenBurn> GetHourlyTokenBurn(int days = 7);

    // Recommendations
    List<Recommendation> GetRecommendations();
    int GetPendingRecommendationsCount();

    // Search
    List<PlanFile> SearchPlans(string query);
    void RebuildFtsIndex();

    // Immediate mutations (DB-first for UI responsiveness)
    void UpdatePlanState(int planId, PlanStatus state);
    void UpdatePlanContent(int planId, string latestRevisionContent, int revisionCount);
    void UpdateRecommendationState(int planId, string recommendationTitle, string newState, string? declineReason);

    // Sync operations (bulk, called by sync service)
    void UpsertPlan(PlanFile plan);
    void DeletePlan(int planId);
    void UpsertCosts(int planId, List<CostEntry> costs);

    void UpsertRecommendations(int planId, string folderName, List<RecommendationYaml> recommendations, string project,
        string planTitle, DateTime updated, PlanStatus status);

    void BulkUpsertPlans(List<PlanFile> plans, bool forceOverwrite = false);

    // Jobs
    void UpsertJob(JobItem job);
    List<JobItem> GetRecentJobs(int limit = 100);
    void PurgeOldJobs(int keepCount = 500);

    // Diagnostics
    long GetDatabaseSize();
    DateTime GetLastSyncTime();
    void SetLastSyncTime(DateTime time);
}

public record CostEntry(string Promptware, int Tokens, decimal Cost, DateTime? LogTimestamp);