using Ivy.Tendril.Apps.Plans;

namespace Ivy.Tendril.Services;

public interface IPlanReaderService
{
    string PlansDirectory { get; }

    void RecoverStuckPlans();
    void RepairPlans();
    List<PlanFile> GetPlans(PlanStatus? statusFilter = null);
    PlanFile? GetPlanByFolder(string folderPath);
    List<PlanFile> GetIceboxPlans();
    void TransitionState(string folderName, PlanStatus newState);
    void SaveRevision(string folderName, string content);
    string ReadLatestRevision(string folderName);
    List<(int Number, string Content, DateTime Modified)> GetRevisions(string folderName);
    void AddLog(string folderName, string action, string content);
    void DeletePlan(string folderName);
    string ReadRawPlan(string folderName);
    void SavePlan(string folderName, string fullContent);
    void UpdateLatestRevision(string folderName, string content);
    decimal GetPlanTotalCost(string folderPath);
    int GetPlanTotalTokens(string folderPath);
    List<HourlyTokenBurn> GetHourlyTokenBurn(int days = 7);
    List<Recommendation> GetRecommendations();
    int GetPendingRecommendationsCount();
    PlanReaderService.PlanCountSnapshot ComputePlanCounts();
    void UpdateRecommendationState(string planFolderName, string recommendationTitle, string newState, string? declineReason = null);
}
