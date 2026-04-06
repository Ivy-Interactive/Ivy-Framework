using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;
using Microsoft.Data.Sqlite;

namespace Ivy.Tendril.Test;

public class PlanDatabaseServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly PlanDatabaseService _db;

    public PlanDatabaseServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid()}.db");
        _db = new PlanDatabaseService(_dbPath);
    }

    public void Dispose()
    {
        _db.Dispose();
        SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
        if (File.Exists(_dbPath + "-wal"))
            File.Delete(_dbPath + "-wal");
        if (File.Exists(_dbPath + "-shm"))
            File.Delete(_dbPath + "-shm");
    }

    private PlanFile CreateTestPlan(int id, string title = "Test Plan", PlanStatus status = PlanStatus.Draft,
        string project = "Tendril", string level = "NiceToHave")
    {
        var metadata = new PlanMetadata(
            id, project, level, title, status,
            new List<string> { "D:\\Repos\\Test" },
            new List<string> { "abc123" },
            new List<string> { "https://github.com/test/pr/1" },
            new List<PlanVerificationEntry> { new() { Name = "DotnetBuild", Status = "Pass" } },
            new List<string>(),
            new List<string>(),
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow
        );

        return new PlanFile(
            metadata,
            "# Test Plan Content",
            $"D:\\Plans\\{id:D5}-{title.Replace(" ", "")}",
            "state: Draft\ntitle: Test Plan",
            1
        );
    }

    [Fact]
    public void EnsureSchema_CreatesTablesSuccessfully()
    {
        // Schema is created in constructor, so just verify we can query
        var plans = _db.GetPlans();
        Assert.Empty(plans);
    }

    [Fact]
    public void UpsertPlan_InsertsAndRetrieves()
    {
        var plan = CreateTestPlan(1500, "Add Widget");
        _db.UpsertPlan(plan);

        var result = _db.GetPlanById(1500);
        Assert.NotNull(result);
        Assert.Equal("Add Widget", result.Title);
        Assert.Equal("Tendril", result.Project);
        Assert.Single(result.Repos);
        Assert.Single(result.Commits);
        Assert.Single(result.Prs);
        Assert.Single(result.Verifications);
    }

    [Fact]
    public void UpsertPlan_UpdatesExistingPlan()
    {
        var plan1 = CreateTestPlan(1500, "Original Title");
        _db.UpsertPlan(plan1);

        var plan2 = CreateTestPlan(1500, "Updated Title", PlanStatus.Completed);
        _db.UpsertPlan(plan2);

        var result = _db.GetPlanById(1500);
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal(PlanStatus.Completed, result.Status);
    }

    [Fact]
    public void GetPlans_ReturnsAllPlans_OrderedById()
    {
        _db.UpsertPlan(CreateTestPlan(1502, "Plan C"));
        _db.UpsertPlan(CreateTestPlan(1500, "Plan A"));
        _db.UpsertPlan(CreateTestPlan(1501, "Plan B"));

        var plans = _db.GetPlans();
        Assert.Equal(3, plans.Count);
        Assert.Equal(1500, plans[0].Id);
        Assert.Equal(1501, plans[1].Id);
        Assert.Equal(1502, plans[2].Id);
    }

    [Fact]
    public void GetPlans_FiltersbyStatus()
    {
        _db.UpsertPlan(CreateTestPlan(1500, "Draft Plan", PlanStatus.Draft));
        _db.UpsertPlan(CreateTestPlan(1501, "Completed Plan", PlanStatus.Completed));
        _db.UpsertPlan(CreateTestPlan(1502, "Failed Plan", PlanStatus.Failed));

        var draftPlans = _db.GetPlans(PlanStatus.Draft);
        Assert.Single(draftPlans);
        Assert.Equal("Draft Plan", draftPlans[0].Title);
    }

    [Fact]
    public void GetPlanByFolder_ReturnsCorrectPlan()
    {
        var plan = CreateTestPlan(1500, "TestPlan");
        _db.UpsertPlan(plan);

        var result = _db.GetPlanByFolder(plan.FolderPath);
        Assert.NotNull(result);
        Assert.Equal(1500, result.Id);
    }

    [Fact]
    public void GetPlanByFolder_ReturnsNullForMissingPlan()
    {
        var result = _db.GetPlanByFolder("D:\\Plans\\99999-NonExistent");
        Assert.Null(result);
    }

    [Fact]
    public void DeletePlan_RemovesPlanAndChildren()
    {
        _db.UpsertPlan(CreateTestPlan(1500, "Plan to Delete"));
        Assert.NotNull(_db.GetPlanById(1500));

        _db.DeletePlan(1500);
        Assert.Null(_db.GetPlanById(1500));
    }

    [Fact]
    public void ComputePlanCounts_ReturnsCorrectCounts()
    {
        _db.UpsertPlan(CreateTestPlan(1500, "Draft1", PlanStatus.Draft));
        _db.UpsertPlan(CreateTestPlan(1501, "Draft2", PlanStatus.Draft));
        _db.UpsertPlan(CreateTestPlan(1502, "Review", PlanStatus.ReadyForReview));
        _db.UpsertPlan(CreateTestPlan(1503, "Failed", PlanStatus.Failed));
        _db.UpsertPlan(CreateTestPlan(1504, "Icebox", PlanStatus.Icebox));
        _db.UpsertPlan(CreateTestPlan(1505, "Completed", PlanStatus.Completed));

        var counts = _db.ComputePlanCounts();
        Assert.Equal(2, counts.Drafts);
        Assert.Equal(1, counts.ReadyForReview);
        Assert.Equal(1, counts.Failed);
        Assert.Equal(1, counts.Icebox);
    }

    [Fact]
    public void UpsertCosts_AndGetTotals()
    {
        _db.UpsertPlan(CreateTestPlan(1500));

        var costs = new List<CostEntry>
        {
            new("ExecutePlan", 50000, 1.50m, DateTime.UtcNow),
            new("MakePr", 10000, 0.30m, DateTime.UtcNow)
        };
        _db.UpsertCosts(1500, costs);

        var totalCost = _db.GetPlanTotalCost(1500);
        var totalTokens = _db.GetPlanTotalTokens(1500);

        Assert.Equal(1.80m, totalCost);
        Assert.Equal(60000, totalTokens);
    }

    [Fact]
    public void UpsertRecommendations_AndGetRecommendations()
    {
        _db.UpsertPlan(CreateTestPlan(1500, "Test Plan"));

        var recs = new List<RecommendationYaml>
        {
            new() { Title = "Add tests", Description = "Need more tests", State = "Pending" },
            new() { Title = "Refactor", Description = "Clean up code", State = "Pending" }
        };

        _db.UpsertRecommendations(1500, "01500-TestPlan", recs, "Tendril", "Test Plan",
            DateTime.UtcNow, PlanStatus.Completed);

        var recommendations = _db.GetRecommendations();
        Assert.Equal(2, recommendations.Count);
        Assert.Equal("Add tests", recommendations[0].Title);
    }

    [Fact]
    public void GetPendingRecommendationsCount_ReturnsCorrectCount()
    {
        _db.UpsertPlan(CreateTestPlan(1500));

        var recs = new List<RecommendationYaml>
        {
            new() { Title = "Pending Rec", Description = "Pending", State = "Pending" },
            new() { Title = "Done Rec", Description = "Done", State = "Accepted" }
        };

        _db.UpsertRecommendations(1500, "01500-TestPlan", recs, "Tendril", "Test Plan",
            DateTime.UtcNow, PlanStatus.Completed);

        Assert.Equal(1, _db.GetPendingRecommendationsCount());
    }

    [Fact]
    public void BulkUpsertPlans_InsertsMultiplePlans()
    {
        var plans = new List<PlanFile>
        {
            CreateTestPlan(1500, "Plan A"),
            CreateTestPlan(1501, "Plan B"),
            CreateTestPlan(1502, "Plan C")
        };

        _db.BulkUpsertPlans(plans);

        var result = _db.GetPlans();
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void SearchPlans_FindsByTitle()
    {
        _db.UpsertPlan(CreateTestPlan(1500, "Add Widget Feature"));
        _db.UpsertPlan(CreateTestPlan(1501, "Fix Bug in Service"));

        var results = _db.SearchPlans("Widget");
        Assert.Single(results);
        Assert.Equal("Add Widget Feature", results[0].Title);
    }

    [Fact]
    public void SearchPlans_FindsByContent()
    {
        var plan = CreateTestPlan(1500, "Some Plan");
        _db.UpsertPlan(plan);

        var results = _db.SearchPlans("Test Plan Content");
        Assert.Single(results);
    }

    [Fact]
    public void GetAndSetLastSyncTime_WorksCorrectly()
    {
        var time = new DateTime(2026, 4, 6, 12, 0, 0, DateTimeKind.Utc);
        _db.SetLastSyncTime(time);

        var result = _db.GetLastSyncTime();
        Assert.Equal(time.Date, result.Date);
    }

    [Fact]
    public void GetDatabaseSize_ReturnsPositiveValue()
    {
        _db.UpsertPlan(CreateTestPlan(1500));
        var size = _db.GetDatabaseSize();
        Assert.True(size > 0);
    }

    [Fact]
    public void GetHourlyTokenBurn_ReturnsDataForRecentCosts()
    {
        _db.UpsertPlan(CreateTestPlan(1500, "Test", project: "Tendril"));

        var now = DateTime.UtcNow;
        var costs = new List<CostEntry>
        {
            new("ExecutePlan", 50000, 1.50m, now),
            new("MakePr", 10000, 0.30m, now)
        };
        _db.UpsertCosts(1500, costs);

        var burn = _db.GetHourlyTokenBurn(7);
        Assert.NotEmpty(burn);
        Assert.Equal("Tendril", burn[0].Project);
        Assert.Equal(60000, burn[0].Tokens);
    }
}
