using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServiceDependencyAutoRetryTests : IDisposable
{
    private readonly string _plansDir;

    public JobServiceDependencyAutoRetryTests()
    {
        _plansDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_plansDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_plansDir))
            Directory.Delete(_plansDir, true);
    }

    private JobService CreateService()
    {
        var planReader = new StubPlanReaderService(_plansDir);
        return new JobService(
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(10),
            planReaderService: planReader);
    }

    private string CreatePlanFolder(string name, string state, List<string>? dependsOn = null)
    {
        var folder = Path.Combine(_plansDir, name);
        Directory.CreateDirectory(folder);

        var depsYaml = "";
        if (dependsOn is { Count: > 0 })
            depsYaml = "dependsOn:\n" + string.Join("\n", dependsOn.Select(d => $"- '{d}'"));
        else
            depsYaml = "dependsOn: []";

        File.WriteAllText(Path.Combine(folder, "plan.yaml"), $"""
                                                              state: {state}
                                                              project: Tendril
                                                              title: {name}
                                                              created: 2026-04-01T00:00:00Z
                                                              updated: 2026-04-01T00:00:00Z
                                                              prs: []
                                                              commits: []
                                                              verifications: []
                                                              {depsYaml}
                                                              """);

        return folder;
    }

    [Fact]
    public void RetryBlockedDependents_WhenDependencyCompletes_RequeuesBlockedPlan()
    {
        var planB = CreatePlanFolder("02100-PlanB", "Completed");
        var planA = CreatePlanFolder("02101-PlanA", "Blocked", ["02100-PlanB"]);

        var service = CreateService();
        var startedJobs = new List<string>();
        service.JobsChanged += () =>
        {
            foreach (var job in service.GetJobs())
                if (job.Type == "ExecutePlan" && !startedJobs.Contains(job.Id))
                    startedJobs.Add(job.Id);
        };

        // Simulate CreateIssue completing for PlanB
        var id = service.StartJob("CreateIssue", planB, "-Repo", "owner/repo", "-Assignee", "", "-Labels", "");
        service.CompleteJob(id, 0);

        // PlanA should have been auto-queued
        var jobs = service.GetJobs();
        Assert.Contains(jobs, j => j.Type == "ExecutePlan" && j.Args.Contains(planA));
    }

    [Fact]
    public void RetryBlockedDependents_WhenNotAllDependenciesMet_DoesNotRequeue()
    {
        var planB = CreatePlanFolder("02200-PlanB", "Completed");
        var planC = CreatePlanFolder("02201-PlanC", "Executing"); // Not completed
        var planA = CreatePlanFolder("02202-PlanA", "Blocked", ["02200-PlanB", "02201-PlanC"]);

        var service = CreateService();

        var id = service.StartJob("CreateIssue", planB, "-Repo", "owner/repo", "-Assignee", "", "-Labels", "");
        service.CompleteJob(id, 0);

        var jobs = service.GetJobs();
        Assert.DoesNotContain(jobs, j => j.Type == "ExecutePlan" && j.Args.Contains(planA));
    }

    [Fact]
    public void RetryBlockedDependents_DoesNotScanDraftPlans_OnlyBlockedPlans()
    {
        var planB = CreatePlanFolder("02500-PlanB", "Completed");
        // Create planA as Draft (not Blocked) — it should NOT be picked up
        var planA = CreatePlanFolder("02501-PlanA", "Draft", ["02500-PlanB"]);

        var service = CreateService();

        var id = service.StartJob("CreateIssue", planB, "-Repo", "owner/repo", "-Assignee", "", "-Labels", "");
        service.CompleteJob(id, 0);

        var jobs = service.GetJobs();
        Assert.DoesNotContain(jobs, j => j.Type == "ExecutePlan" && j.Args.Contains(planA));
    }

    [Fact]
    public void RetryBlockedDependents_TransitionsBlockedToDraft_WhenUnblocked()
    {
        var planB = CreatePlanFolder("02600-PlanB", "Completed");
        var planA = CreatePlanFolder("02601-PlanA", "Blocked", ["02600-PlanB"]);

        var service = CreateService();

        var id = service.StartJob("CreateIssue", planB, "-Repo", "owner/repo", "-Assignee", "", "-Labels", "");
        service.CompleteJob(id, 0);

        // Verify plan.yaml was updated to Draft
        var planYamlContent = File.ReadAllText(Path.Combine(planA, "plan.yaml"));
        Assert.Contains("state: Draft", planYamlContent);
    }

    [Fact]
    public void ResetPlanStateToBlocked_WritesBlockedState()
    {
        // Create a plan with an unmet dependency
        var depPlan = CreatePlanFolder("02700-DepPlan", "Executing"); // Not completed
        var planA = CreatePlanFolder("02701-PlanA", "Draft", ["02700-DepPlan"]);

        var service = CreateService();

        // Start ExecutePlan — dependencies aren't met, so ResetPlanStateToBlocked should fire
        service.StartJob("ExecutePlan", planA);

        // Verify plan.yaml was updated to Blocked (not Draft)
        var planYamlContent = File.ReadAllText(Path.Combine(planA, "plan.yaml"));
        Assert.Contains("state: Blocked", planYamlContent);
    }

    [Fact]
    public void RetryBlockedDependents_WhenPlanNotInDraftState_DoesNotRequeue()
    {
        var planB = CreatePlanFolder("02300-PlanB", "Completed");
        var planA = CreatePlanFolder("02301-PlanA", "Building", ["02300-PlanB"]);

        var service = CreateService();

        var id = service.StartJob("CreateIssue", planB, "-Repo", "owner/repo", "-Assignee", "", "-Labels", "");
        service.CompleteJob(id, 0);

        var jobs = service.GetJobs();
        Assert.DoesNotContain(jobs, j => j.Type == "ExecutePlan" && j.Args.Contains(planA));
    }

    [Fact]
    public void RetryBlockedDependents_WhenPlanInDraftState_DoesNotRequeue()
    {
        var planB = CreatePlanFolder("02300-PlanB", "Completed");
        var planA = CreatePlanFolder("02302-PlanA", "Draft", ["02300-PlanB"]);

        var service = CreateService();

        var id = service.StartJob("CreateIssue", planB, "-Repo", "owner/repo", "-Assignee", "", "-Labels", "");
        service.CompleteJob(id, 0);

        var jobs = service.GetJobs();
        Assert.DoesNotContain(jobs, j => j.Type == "ExecutePlan" && j.Args.Contains(planA));
    }

    [Fact]
    public void RetryBlockedDependents_WhenNoDependents_DoesNothing()
    {
        var planB = CreatePlanFolder("02400-PlanB", "Completed");
        CreatePlanFolder("02401-PlanX", "Draft"); // No dependencies

        var service = CreateService();

        var id = service.StartJob("CreateIssue", planB, "-Repo", "owner/repo", "-Assignee", "", "-Labels", "");
        service.CompleteJob(id, 0);

        var jobs = service.GetJobs();
        Assert.DoesNotContain(jobs, j => j.Type == "ExecutePlan");
    }

    private class StubPlanReaderService : IPlanReaderService
    {
        public StubPlanReaderService(string plansDirectory)
        {
            PlansDirectory = plansDirectory;
        }

        public string PlansDirectory { get; }

        public void RecoverStuckPlans()
        {
        }

        public void RepairPlans()
        {
        }

        public List<PlanFile> GetPlans(PlanStatus? statusFilter = null)
        {
            return [];
        }

        public PlanFile? GetPlanByFolder(string folderPath)
        {
            return null;
        }

        public List<PlanFile> GetIceboxPlans()
        {
            return [];
        }

        public void TransitionState(string folderName, PlanStatus newState)
        {
        }

        public void SaveRevision(string folderName, string content)
        {
        }

        public string ReadLatestRevision(string folderName)
        {
            return "";
        }

        public List<(int Number, string Content, DateTime Modified)> GetRevisions(string folderName)
        {
            return [];
        }

        public void AddLog(string folderName, string action, string content)
        {
        }

        public void DeletePlan(string folderName)
        {
        }

        public string ReadRawPlan(string folderName)
        {
            return "";
        }

        public void SavePlan(string folderName, string fullContent)
        {
        }

        public void UpdateLatestRevision(string folderName, string content)
        {
        }

        public decimal GetPlanTotalCost(string folderPath)
        {
            return 0;
        }

        public int GetPlanTotalTokens(string folderPath)
        {
            return 0;
        }

        public List<HourlyTokenBurn> GetHourlyTokenBurn(int days = 7)
        {
            return [];
        }

        public List<Recommendation> GetRecommendations()
        {
            return [];
        }

        public int GetPendingRecommendationsCount()
        {
            return 0;
        }

        public PlanReaderService.PlanCountSnapshot ComputePlanCounts()
        {
            return new PlanReaderService.PlanCountSnapshot(0, 0, 0, 0, 0);
        }

        public void UpdateRecommendationState(string planFolderName, string recommendationTitle, string newState,
            string? declineReason = null)
        {
        }

        public void InvalidateCaches()
        {
        }
    }
}