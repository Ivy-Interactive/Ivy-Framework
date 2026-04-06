using Ivy.Tendril.Apps.Plans;

namespace Ivy.Tendril.Services;

public interface IPlanDatabaseService : IDisposable
{
    void EnsureSchema();

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

    // Sync operations
    void UpsertPlan(PlanFile plan);
    void DeletePlan(int planId);
    void UpsertCosts(int planId, List<CostEntry> costs);
    void UpsertRecommendations(int planId, string folderName, List<RecommendationYaml> recommendations, string project, string planTitle, DateTime updated, PlanStatus status);
    void BulkUpsertPlans(List<PlanFile> plans);

    // Diagnostics
    long GetDatabaseSize();
    DateTime GetLastSyncTime();
    void SetLastSyncTime(DateTime time);
}

public record CostEntry(string Promptware, int Tokens, decimal Cost, DateTime? LogTimestamp);
