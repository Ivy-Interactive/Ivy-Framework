using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServiceRetryBlockedTests
{
    private static string CreatePlanFolder(string state, List<string>? dependsOn = null, List<string>? prs = null)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        var depsYaml = "";
        if (dependsOn is { Count: > 0 })
            depsYaml = "dependsOn:\n" + string.Join("\n", dependsOn.Select(d => $"- {d}"));
        else
            depsYaml = "dependsOn: []";

        var prsYaml = "";
        if (prs is { Count: > 0 })
            prsYaml = "prs:\n" + string.Join("\n", prs.Select(p => $"- {p}"));
        else
            prsYaml = "prs: []";

        var yaml =
            $"state: {state}\nproject: TestProject\nlevel: NiceToHave\ntitle: Test\nupdated: 2026-01-01T00:00:00Z\n{depsYaml}\n{prsYaml}\ncommits: []\nverifications: []\nrelatedPlans: []\nrepos: []\n";
        File.WriteAllText(Path.Combine(tempDir, "plan.yaml"), yaml);
        return tempDir;
    }

    private static string CreatePlansDirectory(params (string folderName, string state)[] plans)
    {
        var plansDir = Path.Combine(Path.GetTempPath(), $"tendril-plans-{Guid.NewGuid():N}");
        Directory.CreateDirectory(plansDir);

        foreach (var (folderName, state) in plans)
        {
            var planDir = Path.Combine(plansDir, folderName);
            Directory.CreateDirectory(planDir);
            var yaml =
                $"state: {state}\nproject: TestProject\nlevel: NiceToHave\ntitle: {folderName}\nupdated: 2026-01-01T00:00:00Z\ndependsOn: []\nprs: []\ncommits: []\nverifications: []\nrelatedPlans: []\nrepos: []\n";
            File.WriteAllText(Path.Combine(planDir, "plan.yaml"), yaml);
        }

        return plansDir;
    }

    [Fact]
    public void RetryBlockedJobs_WhenDependencySatisfied_AutoStartsJob()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        // Create plans directory with a completed dependency
        var plansDir = CreatePlansDirectory(("01100-DepPlan", "Completed"));

        // Create the dependent plan that depends on 01100-DepPlan
        var dependentPlan = CreatePlanFolder("Draft", ["01100-DepPlan"]);

        var planReader = new FakePlanReaderService(plansDir);
        var service = new JobService(
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(10),
            planReaderService: planReader);

        // Manually create a blocked job (simulating what StartJob does when dependencies aren't met)
        var blockedId = service.CreateTestJob("ExecutePlan", dependentPlan);
        var blockedJob = service.GetJob(blockedId)!;
        blockedJob.Status = JobStatus.Blocked;

        // Create a completing MakePr job to trigger RetryBlockedJobs
        var completingId = service.CreateTestJob("MakePr", Path.GetTempPath());

        var notifications = new List<JobNotification>();
        service.NotificationReady += n => notifications.Add(n);

        // Complete the MakePr job successfully — this should trigger RetryBlockedJobs
        service.CompleteJob(completingId, 0);

        // The blocked job should have been removed
        Assert.Null(service.GetJob(blockedId));

        // A new job should have been created (the restarted one)
        var jobs = service.GetJobs();
        // The restarted job won't actually launch (no script), but it should exist
        // Since dependencies are now satisfied, it should NOT be blocked
        var restartedJob = jobs.FirstOrDefault(j => j.Id != completingId && j.Type == "ExecutePlan");
        Assert.NotNull(restartedJob);
        Assert.NotEqual(JobStatus.Blocked, restartedJob.Status);

        // Should have received an "unblocked" notification
        Assert.Contains(notifications, n => n.Title == "Job Unblocked");

        // Cleanup
        Directory.Delete(dependentPlan, true);
        Directory.Delete(plansDir, true);
    }

    [Fact]
    public void RetryBlockedJobs_WhenDependencyStillUnsatisfied_DoesNothing()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        // Create plans directory with an incomplete dependency
        var plansDir = CreatePlansDirectory(("01100-DepPlan", "Executing"));

        // Create the dependent plan
        var dependentPlan = CreatePlanFolder("Draft", ["01100-DepPlan"]);

        var planReader = new FakePlanReaderService(plansDir);
        var service = new JobService(
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(10),
            planReaderService: planReader);

        // Create a blocked job
        var blockedId = service.CreateTestJob("ExecutePlan", dependentPlan);
        var blockedJob = service.GetJob(blockedId)!;
        blockedJob.Status = JobStatus.Blocked;

        // Create a completing job
        var completingId = service.CreateTestJob("MakePr", Path.GetTempPath());

        var notifications = new List<JobNotification>();
        service.NotificationReady += n => notifications.Add(n);

        // Complete the MakePr job
        service.CompleteJob(completingId, 0);

        // The blocked job should still exist and still be blocked
        var stillBlocked = service.GetJob(blockedId);
        Assert.NotNull(stillBlocked);
        Assert.Equal(JobStatus.Blocked, stillBlocked.Status);

        // Should NOT have received an "unblocked" notification
        Assert.DoesNotContain(notifications, n => n.Title == "Job Unblocked");

        // Cleanup
        Directory.Delete(dependentPlan, true);
        Directory.Delete(plansDir, true);
    }

    [Fact]
    public void RetryBlockedJobs_WhenJobAlreadyRemoved_DoesNotCreateDuplicate()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        // Create plans directory with a completed dependency
        var plansDir = CreatePlansDirectory(("01100-DepPlan", "Completed"));

        // Create two dependent plans that depend on the same dep
        var dependentPlan1 = CreatePlanFolder("Draft", ["01100-DepPlan"]);
        var dependentPlan2 = CreatePlanFolder("Draft", ["01100-DepPlan"]);

        var planReader = new FakePlanReaderService(plansDir);
        var service = new JobService(
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(10),
            planReaderService: planReader);

        // Create a blocked job for plan1
        var blockedId = service.CreateTestJob("ExecutePlan", dependentPlan1);
        var blockedJob = service.GetJob(blockedId)!;
        blockedJob.Status = JobStatus.Blocked;

        // Manually remove the blocked job before CompleteJob triggers RetryBlockedJobs
        // This simulates another thread having already handled it
        service.GetJobs(); // ensure enumeration snapshot
        var removed = service.RemoveJob(blockedId);
        Assert.True(removed);

        // Create a completing job to trigger RetryBlockedJobs
        var completingId = service.CreateTestJob("MakePr", Path.GetTempPath());
        service.CompleteJob(completingId, 0);

        // Since the blocked job was already removed, no new ExecutePlan job should be created for it
        var jobs = service.GetJobs();
        var executePlanJobs = jobs.Where(j => j.Type == "ExecutePlan" && j.Args.Length > 0 && j.Args[0] == dependentPlan1).ToList();
        Assert.Empty(executePlanJobs);

        // Cleanup
        Directory.Delete(dependentPlan1, true);
        Directory.Delete(dependentPlan2, true);
        Directory.Delete(plansDir, true);
    }

    [Fact]
    public void RetryBlockedJobs_WhenActiveJobExistsForSamePlan_SkipsRetry()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        // Create plans directory with a completed dependency
        var plansDir = CreatePlansDirectory(("01100-DepPlan", "Completed"));

        // Create the dependent plan
        var dependentPlan = CreatePlanFolder("Draft", ["01100-DepPlan"]);

        var planReader = new FakePlanReaderService(plansDir);
        var service = new JobService(
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(10),
            planReaderService: planReader);

        // Create a Running ExecutePlan job for the same plan folder (simulating an already active job)
        var activeId = service.CreateTestJob("ExecutePlan", dependentPlan);
        Assert.Equal(JobStatus.Running, service.GetJob(activeId)!.Status);

        // Create a blocked job for the same plan folder
        var blockedId = service.CreateTestJob("ExecutePlan", dependentPlan);
        var blockedJob = service.GetJob(blockedId)!;
        blockedJob.Status = JobStatus.Blocked;

        // Create a completing job to trigger RetryBlockedJobs
        var completingId = service.CreateTestJob("MakePr", Path.GetTempPath());
        service.CompleteJob(completingId, 0);

        // The blocked job should have been removed (TryRemove succeeds)
        Assert.Null(service.GetJob(blockedId));

        // But no NEW ExecutePlan job should be created because HasActiveJobForPlan returns true
        var executePlanJobs = service.GetJobs()
            .Where(j => j.Type == "ExecutePlan" && j.Args.Length > 0 && j.Args[0] == dependentPlan)
            .ToList();

        // Only the original active job should exist
        Assert.Single(executePlanJobs);
        Assert.Equal(activeId, executePlanJobs[0].Id);

        // Cleanup
        Directory.Delete(dependentPlan, true);
        Directory.Delete(plansDir, true);
    }

    /// <summary>
    ///     Minimal fake that provides PlansDirectory for dependency checking.
    /// </summary>
    private class FakePlanReaderService : IPlanReaderService
    {
        public FakePlanReaderService(string plansDirectory)
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

        public DashboardStats GetDashboardData(string? projectFilter)
        {
            return new DashboardStats(0, 0, 0, 0, 0, 0, 0, [], []);
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