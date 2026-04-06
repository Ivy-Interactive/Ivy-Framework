using System.Reflection;

using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class RecommendationServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _plansDir;
    private readonly PlanReaderService _service;

    public RecommendationServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
        _plansDir = Path.Combine(_tempDir, "Plans");
        Directory.CreateDirectory(_plansDir);

        var settings = new TendrilSettings();
        var configService = new ConfigService(settings, _tempDir);
        _service = new PlanReaderService(configService);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string CreatePlanWithRecommendations(string folderName, string recommendationsYaml, string project = "Tendril", string state = "Completed")
    {
        var dir = Path.Combine(_plansDir, folderName);
        Directory.CreateDirectory(dir);

        var planYaml = $"state: {state}\nproject: {project}\ntitle: Test Plan\nrepos: []\ncommits: []\nprs: []\nverifications: []\nrelatedPlans: []\ndependsOn: []\ncreated: 2026-01-01T00:00:00Z\nupdated: 2026-01-01T00:00:00Z\n";
        File.WriteAllText(Path.Combine(dir, "plan.yaml"), planYaml);

        var revisionsDir = Path.Combine(dir, "revisions");
        Directory.CreateDirectory(revisionsDir);
        File.WriteAllText(Path.Combine(revisionsDir, "001.md"), "# Test");

        var artifactsDir = Path.Combine(dir, "artifacts");
        Directory.CreateDirectory(artifactsDir);
        File.WriteAllText(Path.Combine(artifactsDir, "recommendations.yaml"), recommendationsYaml);

        return dir;
    }

    [Fact]
    public void GetRecommendations_WithState_DeserializesCorrectly()
    {
        var yaml = "- title: Fix bug\n  description: Found a bug\n  state: Accepted\n";
        CreatePlanWithRecommendations("01600-TestPlan", yaml);

        var recommendations = _service.GetRecommendations();

        Assert.Single(recommendations);
        Assert.Equal("Fix bug", recommendations[0].Title);
        Assert.Equal("Found a bug", recommendations[0].Description);
        Assert.Equal("Accepted", recommendations[0].State);
    }

    [Fact]
    public void GetRecommendations_WithoutState_DefaultsToPending()
    {
        var yaml = "- title: Legacy item\n  description: Old recommendation\n";
        CreatePlanWithRecommendations("01601-LegacyPlan", yaml);

        var recommendations = _service.GetRecommendations();

        Assert.Single(recommendations);
        Assert.Equal("Pending", recommendations[0].State);
    }

    [Fact]
    public void UpdateRecommendationState_ChangesState()
    {
        var yaml = "- title: Fix bug\n  description: Found a bug\n  state: Pending\n";
        CreatePlanWithRecommendations("01602-UpdateTest", yaml);

        _service.UpdateRecommendationState("01602-UpdateTest", "Fix bug", "Accepted");

        var recommendations = _service.GetRecommendations();
        Assert.Single(recommendations);
        Assert.Equal("Accepted", recommendations[0].State);
    }

    [Fact]
    public void UpdateRecommendationState_PersistsToYaml()
    {
        var yaml = "- title: Item One\n  description: First\n  state: Pending\n- title: Item Two\n  description: Second\n  state: Pending\n";
        CreatePlanWithRecommendations("01603-PersistTest", yaml);

        _service.UpdateRecommendationState("01603-PersistTest", "Item Two", "Declined");

        var filePath = Path.Combine(_plansDir, "01603-PersistTest", "artifacts", "recommendations.yaml");
        var content = File.ReadAllText(filePath);
        Assert.Contains("Declined", content);
        Assert.Contains("Pending", content);
    }

    [Fact]
    public void UpdateRecommendationState_NonExistentTitle_DoesNothing()
    {
        var yaml = "- title: Fix bug\n  description: Found a bug\n  state: Pending\n";
        CreatePlanWithRecommendations("01604-NoMatch", yaml);

        _service.UpdateRecommendationState("01604-NoMatch", "Nonexistent", "Accepted");

        var recommendations = _service.GetRecommendations();
        Assert.Single(recommendations);
        Assert.Equal("Pending", recommendations[0].State);
    }

    [Fact]
    public void UpdateRecommendationState_NonExistentFolder_DoesNothing()
    {
        _service.UpdateRecommendationState("99999-DoesNotExist", "Title", "Accepted");
    }

    [Fact]
    public void UpdateRecommendationState_AcceptedWithNotes_PersistsCorrectly()
    {
        var yaml = "- title: Fix bug\n  description: Found a bug\n  state: Pending\n";
        CreatePlanWithRecommendations("01605-AcceptWithNotes", yaml);

        _service.UpdateRecommendationState("01605-AcceptWithNotes", "Fix bug", "AcceptedWithNotes");

        var recommendations = _service.GetRecommendations();
        Assert.Single(recommendations);
        Assert.Equal("AcceptedWithNotes", recommendations[0].State);
    }

    [Fact]
    public void GetPendingRecommendationsCount_MatchesFullDeserializationCount()
    {
        var yaml1 = "- title: Pending item\n  description: Needs work\n  state: Pending\n- title: Accepted item\n  description: Done\n  state: Accepted\n";
        var yaml2 = "- title: No state item\n  description: Defaults to Pending\n";
        CreatePlanWithRecommendations("01610-CountTest1", yaml1);
        CreatePlanWithRecommendations("01611-CountTest2", yaml2);

        var countOnly = _service.GetPendingRecommendationsCount();
        var fullList = _service.GetRecommendations();
        var fullListCount = fullList.Count(r => r.State == "Pending");

        Assert.Equal(fullListCount, countOnly);
        Assert.Equal(2, countOnly);
    }

    [Fact]
    public void GetPendingRecommendationsCount_ReturnsZero_WhenNoPendingItems()
    {
        var yaml = "- title: Accepted item\n  description: Done\n  state: Accepted\n- title: Declined item\n  description: Nope\n  state: Declined\n";
        CreatePlanWithRecommendations("01612-NoPending", yaml);

        var count = _service.GetPendingRecommendationsCount();

        Assert.Equal(0, count);
    }

    [Fact]
    public void GetPendingRecommendationsCount_ReturnsZero_WhenNoRecommendationFiles()
    {
        var count = _service.GetPendingRecommendationsCount();

        Assert.Equal(0, count);
    }

    [Fact]
    public void GetRecommendations_PopulatesSourcePlanStatus()
    {
        var yaml = "- title: Item\n  description: Desc\n  state: Pending\n";
        CreatePlanWithRecommendations("01620-StatusTest", yaml, project: "Framework", state: "Completed");

        var recommendations = _service.GetRecommendations();

        Assert.Single(recommendations);
        Assert.Equal(PlanStatus.Completed, recommendations[0].SourcePlanStatus);
    }

    [Fact]
    public void GetRecommendations_FilterByProject_ReturnsOnlyMatchingProject()
    {
        var yaml = "- title: Rec A\n  description: Desc\n  state: Pending\n";
        CreatePlanWithRecommendations("01621-Framework", yaml, project: "Framework");
        CreatePlanWithRecommendations("01622-Tendril", yaml, project: "Tendril");
        CreatePlanWithRecommendations("01623-Agent", yaml, project: "Agent");

        var recommendations = _service.GetRecommendations();
        var frameworkOnly = recommendations.Where(r => r.Project == "Framework").ToList();

        Assert.Equal(3, recommendations.Count);
        Assert.Single(frameworkOnly);
        Assert.Equal("Framework", frameworkOnly[0].Project);
    }

    [Fact]
    public void GetRecommendations_FilterByPlanStatus_ReturnsOnlyMatchingStatus()
    {
        var yaml = "- title: Rec\n  description: Desc\n  state: Pending\n";
        CreatePlanWithRecommendations("01624-Completed", yaml, state: "Completed");
        CreatePlanWithRecommendations("01625-Failed", yaml, state: "Failed");
        CreatePlanWithRecommendations("01626-Draft", yaml, state: "Draft");

        var recommendations = _service.GetRecommendations();
        var completedOnly = recommendations.Where(r => r.SourcePlanStatus == PlanStatus.Completed).ToList();

        Assert.Equal(3, recommendations.Count);
        Assert.Single(completedOnly);
        Assert.Equal(PlanStatus.Completed, completedOnly[0].SourcePlanStatus);
    }

    [Fact]
    public void GetRecommendations_CombinedFilters_ApplyAndLogic()
    {
        var yaml = "- title: Rec\n  description: Desc\n  state: Pending\n";
        CreatePlanWithRecommendations("01627-FwCompleted", yaml, project: "Framework", state: "Completed");
        CreatePlanWithRecommendations("01628-FwFailed", yaml, project: "Framework", state: "Failed");
        CreatePlanWithRecommendations("01629-TdCompleted", yaml, project: "Tendril", state: "Completed");

        var recommendations = _service.GetRecommendations();
        var filtered = recommendations
            .Where(r => r.Project == "Framework")
            .Where(r => r.SourcePlanStatus == PlanStatus.Completed)
            .ToList();

        Assert.Equal(3, recommendations.Count);
        Assert.Single(filtered);
        Assert.Equal("Framework", filtered[0].Project);
        Assert.Equal(PlanStatus.Completed, filtered[0].SourcePlanStatus);
    }

    [Fact]
    public void UpdateRecommendationState_DeclineWithReason_StoresReasonInYaml()
    {
        var yaml = "- title: Fix bug\n  description: Found a bug\n  state: Pending\n";
        CreatePlanWithRecommendations("01640-DeclineReason", yaml);

        _service.UpdateRecommendationState("01640-DeclineReason", "Fix bug", "Declined", "Not relevant to our project");

        var filePath = Path.Combine(_plansDir, "01640-DeclineReason", "artifacts", "recommendations.yaml");
        var content = File.ReadAllText(filePath);
        Assert.Contains("Declined", content);
        Assert.Contains("Not relevant to our project", content);

        var recommendations = _service.GetRecommendations();
        Assert.Single(recommendations);
        Assert.Equal("Declined", recommendations[0].State);
        Assert.Equal("Not relevant to our project", recommendations[0].DeclineReason);
    }

    [Fact]
    public void UpdateRecommendationState_DeclineWithEmptyReason_DoesNotStoreReason()
    {
        var yaml = "- title: Fix bug\n  description: Found a bug\n  state: Pending\n";
        CreatePlanWithRecommendations("01641-DeclineNoReason", yaml);

        _service.UpdateRecommendationState("01641-DeclineNoReason", "Fix bug", "Declined", "");

        var recommendations = _service.GetRecommendations();
        Assert.Single(recommendations);
        Assert.Equal("Declined", recommendations[0].State);
        Assert.Null(recommendations[0].DeclineReason);
    }

    [Fact]
    public void UpdateRecommendationState_DeclineReasonPersistsAndCanBeReadBack()
    {
        var yaml = "- title: Item A\n  description: First\n  state: Pending\n- title: Item B\n  description: Second\n  state: Pending\n";
        CreatePlanWithRecommendations("01642-DeclinePersist", yaml);

        _service.UpdateRecommendationState("01642-DeclinePersist", "Item A", "Declined", "Duplicate of another recommendation");

        var recommendations = _service.GetRecommendations();
        var itemA = recommendations.First(r => r.Title == "Item A");
        var itemB = recommendations.First(r => r.Title == "Item B");

        Assert.Equal("Declined", itemA.State);
        Assert.Equal("Duplicate of another recommendation", itemA.DeclineReason);
        Assert.Equal("Pending", itemB.State);
        Assert.Null(itemB.DeclineReason);
    }

    [Fact]
    public void GetRecommendations_NullFilters_ReturnsAll()
    {
        var yaml = "- title: Rec\n  description: Desc\n  state: Pending\n";
        CreatePlanWithRecommendations("01630-All1", yaml, project: "Framework", state: "Completed");
        CreatePlanWithRecommendations("01631-All2", yaml, project: "Tendril", state: "Failed");

        var recommendations = _service.GetRecommendations();
        string? projectFilter = null;
        string? statusFilter = null;

        var filtered = recommendations
            .Where(r => projectFilter == null || r.Project == projectFilter)
            .Where(r => statusFilter == null || r.SourcePlanStatus.ToString() == statusFilter)
            .ToList();

        Assert.Equal(2, filtered.Count);
    }

    [Fact]
    public void GetRecommendations_CachesResultForTwoMinutes()
    {
        var yaml = "- title: Cached Item\n  description: Desc\n  state: Pending\n";
        CreatePlanWithRecommendations("01650-CacheTest", yaml);

        // First call computes and caches
        var result1 = _service.GetRecommendations();
        Assert.Single(result1);

        // Add a new recommendation file on disk
        var yaml2 = "- title: New Item\n  description: Desc2\n  state: Pending\n";
        CreatePlanWithRecommendations("01651-CacheTest2", yaml2);

        // Second call within cache window returns cached value (still 1 item)
        var result2 = _service.GetRecommendations();
        Assert.Single(result2);
        Assert.Same(result1, result2);

        // Simulate cache expiration by setting _recommendationsCacheTime to 3 minutes ago
        var cacheTimeField = typeof(PlanReaderService).GetField("_recommendationsCacheTime", BindingFlags.NonPublic | BindingFlags.Instance)!;
        cacheTimeField.SetValue(_service, DateTime.UtcNow.AddMinutes(-3));

        // Third call after expiration recomputes (now sees both items)
        var result3 = _service.GetRecommendations();
        Assert.Equal(2, result3.Count);
        Assert.NotSame(result1, result3);
    }

    [Fact]
    public void UpdateRecommendationState_InvalidatesCache()
    {
        var yaml = "- title: Fix bug\n  description: Found a bug\n  state: Pending\n";
        CreatePlanWithRecommendations("01652-CacheInvalidate", yaml);

        // Prime the cache
        var result1 = _service.GetRecommendations();
        Assert.Single(result1);
        Assert.Equal("Pending", result1[0].State);

        // Update state (should invalidate cache)
        _service.UpdateRecommendationState("01652-CacheInvalidate", "Fix bug", "Accepted");

        // Next call should recompute and reflect the updated state
        var result2 = _service.GetRecommendations();
        Assert.Single(result2);
        Assert.Equal("Accepted", result2[0].State);
        Assert.NotSame(result1, result2);
    }
}
