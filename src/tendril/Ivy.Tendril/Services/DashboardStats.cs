namespace Ivy.Tendril.Services;

public record DashboardStats(
    int TotalCount,
    int DraftCount,
    int InProgressCount,
    int ReviewCount,
    int CompletedCount,
    int FailedCount,
    decimal AvgCostPerPlan,
    List<DashboardDayStats> DailyStats,
    List<ProjectCount> ProjectCounts
);

public record DashboardDayStats(
    DateTime Date,
    int Created,
    int Completed,
    int PrsMerged,
    int Failed,
    decimal Cost,
    int Tokens
);

public record ProjectCount(string Project, int Count);
