using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ivy.Tendril.Test;

public class PlanReaderServiceComputePlanCountsTests : IDisposable
{
    private readonly string _plansDir;
    private readonly PlanReaderService _service;
    private readonly string _tempDir;

    public PlanReaderServiceComputePlanCountsTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid()}");
        _plansDir = Path.Combine(_tempDir, "Plans");
        Directory.CreateDirectory(_plansDir);

        var settings = new TendrilSettings();
        var configService = new ConfigService(settings, _tempDir);
        _service = new PlanReaderService(configService, NullLogger<PlanReaderService>.Instance);
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

    [Fact]
    public void ComputePlanCounts_ReturnsZeros_WhenNoPlans()
    {
        var snapshot = _service.ComputePlanCounts();

        Assert.Equal(0, snapshot.Drafts);
        Assert.Equal(0, snapshot.ReadyForReview);
        Assert.Equal(0, snapshot.Failed);
        Assert.Equal(0, snapshot.Icebox);
        Assert.Equal(0, snapshot.PendingRecommendations);
    }

    [Fact]
    public void ComputePlanCounts_CountsAllStates()
    {
        CreatePlan("01001-Draft1", "Draft");
        CreatePlan("01002-Draft2", "Draft");
        CreatePlan("01003-Review1", "ReadyForReview");
        CreatePlan("01004-Failed1", "Failed");
        CreatePlan("01005-Icebox1", "Icebox");
        CreatePlan("01006-Icebox2", "Icebox");
        CreatePlan("01007-Completed1", "Completed");

        var snapshot = _service.ComputePlanCounts();

        Assert.Equal(2, snapshot.Drafts);
        Assert.Equal(1, snapshot.ReadyForReview);
        Assert.Equal(1, snapshot.Failed);
        Assert.Equal(2, snapshot.Icebox);
        Assert.Equal(0, snapshot.PendingRecommendations);
    }

    [Fact]
    public void ComputePlanCounts_CountsPendingRecommendations()
    {
        CreatePlan("01010-WithRecs", "Completed");
        CreateRecommendations("01010-WithRecs",
            "- title: Rec1\n  description: desc\n  state: Pending\n- title: Rec2\n  description: desc\n  state: Done\n- title: Rec3\n  description: desc\n  state: Pending\n");

        var snapshot = _service.ComputePlanCounts();

        Assert.Equal(2, snapshot.PendingRecommendations);
    }

    [Fact]
    public void ComputePlanCounts_CountsBlockedPlansInDrafts()
    {
        CreatePlan("01030-Draft", "Draft");
        CreatePlan("01031-Blocked1", "Blocked");
        CreatePlan("01032-Blocked2", "Blocked");

        var snapshot = _service.ComputePlanCounts();

        // Blocked plans should count alongside drafts
        Assert.Equal(3, snapshot.Drafts);
    }

    [Fact]
    public void ComputePlanCounts_MatchesIndividualMethods()
    {
        CreatePlan("01020-Draft", "Draft");
        CreatePlan("01021-Review", "ReadyForReview");
        CreatePlan("01022-Failed", "Failed");
        CreatePlan("01023-Icebox", "Icebox");
        CreatePlan("01024-Completed", "Completed");
        CreatePlan("01025-Blocked", "Blocked");
        CreateRecommendations("01024-Completed",
            "- title: R1\n  description: d\n  state: Pending\n- title: R2\n  description: d\n  state: Resolved\n");

        var snapshot = _service.ComputePlanCounts();
        var plans = _service.GetPlans();
        var pendingRecs = _service.GetPendingRecommendationsCount();

        Assert.Equal(plans.Count(p => p.Status is PlanStatus.Draft or PlanStatus.Blocked), snapshot.Drafts);
        Assert.Equal(plans.Count(p => p.Status == PlanStatus.ReadyForReview), snapshot.ReadyForReview);
        Assert.Equal(plans.Count(p => p.Status == PlanStatus.Failed), snapshot.Failed);
        Assert.Equal(plans.Count(p => p.Status == PlanStatus.Icebox), snapshot.Icebox);
        Assert.Equal(pendingRecs, snapshot.PendingRecommendations);
    }
}