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

    private string CreatePlanWithRecommendations(string folderName, string recommendationsYaml)
    {
        var dir = Path.Combine(_plansDir, folderName);
        Directory.CreateDirectory(dir);

        var planYaml = "state: Completed\nproject: Tendril\ntitle: Test Plan\nrepos: []\ncommits: []\nprs: []\nverifications: []\nrelatedPlans: []\ndependsOn: []\ncreated: 2026-01-01T00:00:00Z\nupdated: 2026-01-01T00:00:00Z\n";
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
}
