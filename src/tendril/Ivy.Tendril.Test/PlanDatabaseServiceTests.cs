using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ivy.Tendril.Test;

public class PlanDatabaseServiceTests : IDisposable
{
    private readonly PlanDatabaseService _db;
    private readonly string _dbPath;

    public PlanDatabaseServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid()}.db");
        _db = new PlanDatabaseService(_dbPath, NullLogger<PlanDatabaseService>.Instance);
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
        string project = "Tendril", string level = "NiceToHave", string latestContent = "# Test Plan Content")
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
            DateTime.UtcNow,
            null,
            null
        );

        return new PlanFile(
            metadata,
            latestContent,
            $"D:\\Plans\\{id:D5}-{title.Replace(" ", "")}",
            "state: Draft\ntitle: Test Plan"
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
        _db.UpsertPlan(CreateTestPlan(1500, "Draft Plan"));
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
        _db.UpsertPlan(CreateTestPlan(1500, "Draft1"));
        _db.UpsertPlan(CreateTestPlan(1501, "Draft2"));
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
        _db.UpsertPlan(CreateTestPlan(1500));

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
        _db.UpsertPlan(CreateTestPlan(1500, "Some Plan",
            latestContent: "Implement a new Button widget with click handlers"));

        var results = _db.SearchPlans("handlers");
        Assert.Single(results);
    }

    [Fact]
    public void SearchPlans_FtsMatchPhrase()
    {
        _db.UpsertPlan(CreateTestPlan(1500, "Add Widget Feature",
            latestContent: "Implement a new Button widget with click handlers"));
        _db.UpsertPlan(CreateTestPlan(1501, "Fix Bug",
            latestContent: "Fix the widget layout issue"));

        var results = _db.SearchPlans("\"Button widget\"");
        Assert.Single(results);
        Assert.Equal("Add Widget Feature", results[0].Title);
    }

    [Fact]
    public void SearchPlans_FtsBooleanQuery()
    {
        _db.UpsertPlan(CreateTestPlan(1500, "Widget Plan",
            latestContent: "Add button"));
        _db.UpsertPlan(CreateTestPlan(1501, "Layout Plan",
            latestContent: "Fix grid"));
        _db.UpsertPlan(CreateTestPlan(1502, "Combined Plan",
            latestContent: "Add button and fix grid"));

        var results = _db.SearchPlans("button OR grid");
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void SearchPlans_FtsRankingRelevance()
    {
        _db.UpsertPlan(CreateTestPlan(1500, "Widget",
            latestContent: "Mention widget once"));
        _db.UpsertPlan(CreateTestPlan(1501, "Widget Widget Widget",
            latestContent: "Mention widget widget widget many times"));

        var results = _db.SearchPlans("widget");
        Assert.Equal(2, results.Count);
        // Higher term frequency should rank higher
        Assert.Equal(1501, results[0].Id);
    }

    [Fact]
    public void SearchPlans_FtsColumnSpecific()
    {
        _db.UpsertPlan(CreateTestPlan(1500, "Button Feature",
            latestContent: "Widget implementation"));
        _db.UpsertPlan(CreateTestPlan(1501, "Widget Feature",
            latestContent: "Button implementation"));

        var results = _db.SearchPlans("Title:Button");
        Assert.Single(results);
        Assert.Equal("Button Feature", results[0].Title);
    }

    [Fact]
    public void RebuildFtsIndex_RepopulatesSearch()
    {
        _db.UpsertPlan(CreateTestPlan(1500, "Searchable Plan",
            latestContent: "Find me via fulltext"));

        Assert.Single(_db.SearchPlans("fulltext"));

        // Clear FTS index using the proper FTS5 delete-all command
        _db.RebuildFtsIndex();
        // Manually clear FTS again to simulate corruption
        using (var conn = new SqliteConnection($"Data Source={_dbPath}"))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO PlanSearch(PlanSearch) VALUES('delete-all');";
            cmd.ExecuteNonQuery();
        }

        // FTS5 phrase matching should fail when index is cleared (no LIKE fallback for phrases)
        Assert.Empty(_db.SearchPlans("\"Find me via fulltext\""));

        _db.RebuildFtsIndex();

        // After rebuild, FTS5 phrase matching works again
        Assert.Single(_db.SearchPlans("\"Find me via fulltext\""));
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

        var burn = _db.GetHourlyTokenBurn();
        Assert.NotEmpty(burn);
        Assert.Equal("Tendril", burn[0].Project);
        Assert.Equal(60000, burn[0].Tokens);
    }

    [Fact]
    public void GetHourlyTokenBurn_WithProjectFilter_ReturnsOnlyMatchingProject()
    {
        _db.UpsertPlan(CreateTestPlan(1600, "Plan A", project: "Tendril"));
        _db.UpsertPlan(CreateTestPlan(1601, "Plan B", project: "Framework"));

        var now = DateTime.UtcNow;
        _db.UpsertCosts(1600, [new("ExecutePlan", 50000, 1.50m, now)]);
        _db.UpsertCosts(1601, [new("ExecutePlan", 30000, 0.90m, now)]);

        var burn = _db.GetHourlyTokenBurn(projectFilter: "Tendril");
        Assert.NotEmpty(burn);
        Assert.All(burn, b => Assert.Equal("Tendril", b.Project));
        Assert.Equal(50000, burn.Sum(b => b.Tokens));
    }

    [Fact]
    public void GetHourlyTokenBurn_WithNullFilter_ReturnsAllProjects()
    {
        _db.UpsertPlan(CreateTestPlan(1700, "Plan A", project: "Tendril"));
        _db.UpsertPlan(CreateTestPlan(1701, "Plan B", project: "Framework"));

        var now = DateTime.UtcNow;
        _db.UpsertCosts(1700, [new("ExecutePlan", 50000, 1.50m, now)]);
        _db.UpsertCosts(1701, [new("ExecutePlan", 30000, 0.90m, now)]);

        var burn = _db.GetHourlyTokenBurn(projectFilter: null);
        Assert.NotEmpty(burn);
        var projects = burn.Select(b => b.Project).Distinct().OrderBy(p => p).ToList();
        Assert.Contains("Tendril", projects);
        Assert.Contains("Framework", projects);
    }

    [Fact]
    public void GetHourlyTokenBurn_NullLogTimestamp_FallsBackToPlanUpdated()
    {
        _db.UpsertPlan(CreateTestPlan(1750, "Test", project: "Tendril"));
        _db.UpsertCosts(1750, [new("ExecutePlan", 50000, 1.50m, null)]);

        var burn = _db.GetHourlyTokenBurn();
        Assert.NotEmpty(burn);
        Assert.Equal(50000, burn[0].Tokens);
    }

    [Fact]
    public void GetHourlyTokenBurn_MixedTimestamps_UsesCorrectTimestamp()
    {
        _db.UpsertPlan(CreateTestPlan(1760, "Plan A", project: "Tendril"));

        var now = DateTime.UtcNow;
        _db.UpsertCosts(1760, [
            new("ExecutePlan", 40000, 1.00m, now),
            new("MakePr", 20000, 0.50m, null)
        ]);

        var burn = _db.GetHourlyTokenBurn();
        Assert.NotEmpty(burn);
        Assert.Equal(60000, burn.Sum(b => b.Tokens));
    }

    [Fact]
    public void GetDashboardData_FiltersCorrectlyByProject()
    {
        _db.UpsertPlan(CreateTestPlan(1800, "Plan A", project: "Tendril", status: PlanStatus.Completed));
        _db.UpsertPlan(CreateTestPlan(1801, "Plan B", project: "Framework", status: PlanStatus.Draft));
        _db.UpsertPlan(CreateTestPlan(1802, "Plan C", project: "Tendril", status: PlanStatus.Draft));

        var stats = _db.GetDashboardData("Tendril");
        Assert.Equal(2, stats.TotalCount);
        Assert.Equal(1, stats.CompletedCount);
        Assert.Equal(1, stats.DraftCount);
    }

    [Fact]
    public void GetDashboardData_FilteredStats_ReturnsCorrectCounts()
    {
        // Arrange: plans across two projects with varied statuses
        _db.UpsertPlan(CreateTestPlan(1900, "Draft Tendril", status: PlanStatus.Draft, project: "Tendril"));
        _db.UpsertPlan(CreateTestPlan(1901, "Completed Tendril", status: PlanStatus.Completed, project: "Tendril"));
        _db.UpsertPlan(CreateTestPlan(1902, "Failed Tendril", status: PlanStatus.Failed, project: "Tendril"));
        _db.UpsertPlan(CreateTestPlan(1903, "Review Framework", status: PlanStatus.ReadyForReview,
            project: "Framework"));
        _db.UpsertPlan(CreateTestPlan(1904, "InProgress Tendril", status: PlanStatus.Executing, project: "Tendril"));

        // Add costs for avg cost calculation
        _db.UpsertCosts(1901, [new("ExecutePlan", 50000, 2.00m, DateTime.UtcNow)]);
        _db.UpsertCosts(1902, [new("ExecutePlan", 30000, 1.00m, DateTime.UtcNow)]);

        // Act: filter to Tendril project
        var stats = _db.GetDashboardData("Tendril");

        // Assert
        Assert.Equal(4, stats.TotalCount);
        Assert.Equal(1, stats.DraftCount);
        Assert.Equal(1, stats.CompletedCount);
        Assert.Equal(1, stats.FailedCount);
        Assert.Equal(1, stats.InProgressCount);
        Assert.Equal(0, stats.ReviewCount);
        // Avg cost: (2.00 + 1.00) / 2 plans = 1.50
        Assert.Equal(1.50m, stats.AvgCostPerPlan);
    }

    [Fact]
    public void GetDashboardData_DailyStats_Returns7DayWindow()
    {
        // Arrange: create plans with explicit Created/Updated dates
        var today = DateTime.UtcNow.Date;

        // Plan created today, completed today (Updated=today)
        var todayPlan = new PlanFile(
            new PlanMetadata(2000, "Tendril", "NiceToHave", "Today Plan", PlanStatus.Completed,
                [], [], [], [], [], [],
                today, today, null, null),
            "# Content", "D:\\Plans\\02000-TodayPlan", "state: Completed");
        _db.UpsertPlan(todayPlan);

        // Plan created 2 days ago, still draft
        var twoDaysAgoPlan = new PlanFile(
            new PlanMetadata(2001, "Tendril", "NiceToHave", "Two Days Ago", PlanStatus.Draft,
                [], [], [], [], [], [],
                today.AddDays(-2), today.AddDays(-2), null, null),
            "# Content", "D:\\Plans\\02001-TwoDaysAgo", "state: Draft");
        _db.UpsertPlan(twoDaysAgoPlan);

        // Act
        var stats = _db.GetDashboardData(null);

        // Assert: daily stats should have 7 entries
        Assert.Equal(7, stats.DailyStats.Count);

        // Today's entry should have 1 created and 1 completed
        var todayStats = stats.DailyStats.First(d => d.Date == today);
        Assert.True(todayStats.Created >= 1);
        Assert.True(todayStats.Completed >= 1);

        // 2 days ago should have 1 created
        var twoDaysAgoStats = stats.DailyStats.First(d => d.Date == today.AddDays(-2));
        Assert.True(twoDaysAgoStats.Created >= 1);

        // All unfiltered, so project counts should include Tendril
        Assert.Contains(stats.ProjectCounts, pc => pc.Project == "Tendril");
    }

    [Fact]
    public void Constructor_DetectsAndHandlesCorruptedDatabase()
    {
        var corruptDbPath = Path.Combine(Path.GetTempPath(), $"tendril-corrupt-{Guid.NewGuid()}.db");
        try
        {
            // Write invalid data to simulate a corrupted database file
            File.WriteAllBytes(corruptDbPath, new byte[] { 0x00, 0x01, 0x02, 0x03, 0xFF, 0xFE, 0xFD });

            // Constructor should detect corruption and recreate the database
            using var db = new PlanDatabaseService(corruptDbPath, NullLogger<PlanDatabaseService>.Instance);

            // Verify the service initialized successfully — tables exist and we can query
            var plans = db.GetPlans();
            Assert.Empty(plans);

            // Verify we can insert and retrieve data (schema was created)
            var plan = CreateTestPlan(9999, "Post-Corruption Plan");
            db.UpsertPlan(plan);
            var result = db.GetPlanById(9999);
            Assert.NotNull(result);
            Assert.Equal("Post-Corruption Plan", result.Title);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(corruptDbPath)) File.Delete(corruptDbPath);
            if (File.Exists(corruptDbPath + "-wal")) File.Delete(corruptDbPath + "-wal");
            if (File.Exists(corruptDbPath + "-shm")) File.Delete(corruptDbPath + "-shm");
        }
    }

    [Fact]
    public void Constructor_PassesIntegrityCheckForHealthyDatabase()
    {
        // The _db created in the constructor should work fine with a clean database
        // Verify it's fully functional (implicit integrity check passed)
        var plan = CreateTestPlan(8888, "Healthy DB Plan");
        _db.UpsertPlan(plan);

        var result = _db.GetPlanById(8888);
        Assert.NotNull(result);
        Assert.Equal("Healthy DB Plan", result.Title);

        // Verify schema is complete by exercising multiple tables
        var counts = _db.ComputePlanCounts();
        Assert.Equal(1, counts.Drafts);
    }

    [Fact]
    public void SearchPlans_FallbackToLikeForPartialSubstring()
    {
        _db.UpsertPlan(CreateTestPlan(1500, "Widget Feature",
            latestContent: "Add new widget with button"));

        // FTS5 won't match partial substring "idg"
        // Should fall back to LIKE and find "Widget"
        var results = _db.SearchPlans("idg");
        Assert.Single(results);
        Assert.Equal("Widget Feature", results[0].Title);
    }

    [Fact]
    public void SearchPlans_LikeFallbackFindsSourceUrl()
    {
        var metadata = new PlanMetadata(
            1500, "Tendril", "NiceToHave", "Source URL Plan", PlanStatus.Draft,
            new List<string> { "D:\\Repos\\Test" },
            new List<string>(), new List<string>(),
            new List<PlanVerificationEntry>(),
            new List<string>(), new List<string>(),
            DateTime.UtcNow, DateTime.UtcNow, null,
            "https://github.com/Ivy-Interactive/Ivy-Framework/issues/42"
        );
        var plan = new PlanFile(metadata, "# Content", "D:\\Plans\\01500-SourceUrlPlan", "state: Draft");
        _db.UpsertPlan(plan);

        // "issues/42" is a substring — FTS5 won't match, should fall back to LIKE
        var results = _db.SearchPlans("issues/42");
        Assert.Single(results);
        Assert.Equal("Source URL Plan", results[0].Title);
    }

    [Fact]
    public void SearchPlans_LikeFallbackFindsInitialPrompt()
    {
        var metadata = new PlanMetadata(
            1501, "Tendril", "NiceToHave", "InitialPrompt Plan", PlanStatus.Draft,
            new List<string> { "D:\\Repos\\Test" },
            new List<string>(), new List<string>(),
            new List<PlanVerificationEntry>(),
            new List<string>(), new List<string>(),
            DateTime.UtcNow, DateTime.UtcNow,
            "Fix the widget rendering/pipeline issue",
            null
        );
        var plan = new PlanFile(metadata, "# Content", "D:\\Plans\\01501-InitialPromptPlan", "state: Draft");
        _db.UpsertPlan(plan);

        // "rendering/pipeline" contains a slash — FTS5 won't match as substring, should fall back to LIKE
        var results = _db.SearchPlans("rendering/pipeline");
        Assert.Single(results);
        Assert.Equal("InitialPrompt Plan", results[0].Title);
    }

    [Fact]
    public void SearchPlans_PrefersFts5OverLike()
    {
        _db.UpsertPlan(CreateTestPlan(1500, "Widget Feature",
            latestContent: "widget widget widget"));
        _db.UpsertPlan(CreateTestPlan(1501, "Button Widget",
            latestContent: "Single widget mention"));

        // FTS5 should match and rank by term frequency
        var results = _db.SearchPlans("widget");
        Assert.Equal(2, results.Count);
        // Higher term frequency ranks first in FTS5
        Assert.Equal(1500, results[0].Id);
    }

    [Fact]
    public void SearchPlans_SanitizesSlashForFts5()
    {
        _db.UpsertPlan(CreateTestPlan(1600, "Fix issues for release",
            latestContent: "Resolved 42 open issues before release"));

        // "issues/42" contains a slash — sanitizer converts to "issues 42"
        // FTS5 implicit AND matches both "issues" and "42" from title/content
        var results = _db.SearchPlans("issues/42");
        Assert.Single(results);
        Assert.Equal("Fix issues for release", results[0].Title);
    }

    [Fact]
    public void SearchPlans_SanitizesUnbalancedQuoteForFts5()
    {
        _db.UpsertPlan(CreateTestPlan(1600, "Button widget feature",
            latestContent: "Add a button widget to the dashboard"));

        // Unbalanced quote should be stripped, leaving "button widget"
        var results = _db.SearchPlans("button\"widget");
        Assert.Single(results);
        Assert.Equal("Button widget feature", results[0].Title);
    }

    [Fact]
    public void SearchPlans_SanitizesBareAsterisk()
    {
        _db.UpsertPlan(CreateTestPlan(1600, "Button component",
            latestContent: "Add a button component"));

        // Bare asterisk should be stripped, leaving "button"
        var results = _db.SearchPlans("* button");
        Assert.Single(results);
        Assert.Equal("Button component", results[0].Title);
    }

    [Fact]
    public void SanitizeFts5Query_ReplacesPathSeparators()
    {
        Assert.Equal("issues 42", PlanDatabaseService.SanitizeFts5Query("issues/42"));
        Assert.Equal("path to file", PlanDatabaseService.SanitizeFts5Query("path\\to\\file"));
    }

    [Fact]
    public void SanitizeFts5Query_RemovesGroupingChars()
    {
        Assert.Equal("hello world", PlanDatabaseService.SanitizeFts5Query("(hello) world^"));
    }

    [Fact]
    public void SanitizeFts5Query_HandlesBareAsterisk()
    {
        Assert.Equal("button", PlanDatabaseService.SanitizeFts5Query("* button"));
        // Prefix wildcard is preserved
        Assert.Equal("button*", PlanDatabaseService.SanitizeFts5Query("button*"));
    }

    [Fact]
    public void SanitizeFts5Query_BalancesQuotes()
    {
        // Odd quotes — all removed
        Assert.Equal("button widget", PlanDatabaseService.SanitizeFts5Query("button\"widget"));
        // Even quotes — preserved
        Assert.Equal("\"button widget\"", PlanDatabaseService.SanitizeFts5Query("\"button widget\""));
    }

    [Fact]
    public void SanitizeFts5Query_PreservesBooleanOperators()
    {
        Assert.Equal("button OR grid", PlanDatabaseService.SanitizeFts5Query("button OR grid"));
        Assert.Equal("button AND widget", PlanDatabaseService.SanitizeFts5Query("button AND widget"));
        Assert.Equal("NOT broken", PlanDatabaseService.SanitizeFts5Query("NOT broken"));
    }

    [Fact]
    public void UpsertJob_PersistsAndRetrievesJobData()
    {
        var job = new JobItem
        {
            Id = "job-001",
            Type = "ExecutePlan",
            PlanFile = "01500-TestPlan",
            Project = "Tendril",
            Status = JobStatus.Completed,
            Provider = "claude",
            SessionId = "session-abc",
            StartedAt = new DateTime(2026, 4, 7, 10, 0, 0, DateTimeKind.Utc),
            CompletedAt = new DateTime(2026, 4, 7, 10, 15, 0, DateTimeKind.Utc),
            DurationSeconds = 900,
            Cost = 1.50m,
            Tokens = 50000,
            StatusMessage = null
        };

        _db.UpsertJob(job);

        var jobs = _db.GetRecentJobs();
        Assert.Single(jobs);

        var result = jobs[0];
        Assert.Equal("job-001", result.Id);
        Assert.Equal("ExecutePlan", result.Type);
        Assert.Equal("01500-TestPlan", result.PlanFile);
        Assert.Equal("Tendril", result.Project);
        Assert.Equal(JobStatus.Completed, result.Status);
        Assert.Equal("claude", result.Provider);
        Assert.Equal("session-abc", result.SessionId);
        Assert.Equal(job.StartedAt, result.StartedAt);
        Assert.Equal(job.CompletedAt, result.CompletedAt);
        Assert.Equal(900, result.DurationSeconds);
        Assert.Equal(1.50m, result.Cost);
        Assert.Equal(50000, result.Tokens);
        Assert.Null(result.StatusMessage);
    }

    [Fact]
    public void GetRecentJobs_ReturnsOrderedByCompletedAt()
    {
        _db.UpsertJob(new JobItem
        {
            Id = "job-001",
            Type = "ExecutePlan",
            PlanFile = "plan-a",
            Project = "Tendril",
            Status = JobStatus.Completed,
            Provider = "claude",
            CompletedAt = new DateTime(2026, 4, 7, 10, 0, 0, DateTimeKind.Utc)
        });
        _db.UpsertJob(new JobItem
        {
            Id = "job-003",
            Type = "ExecutePlan",
            PlanFile = "plan-c",
            Project = "Tendril",
            Status = JobStatus.Completed,
            Provider = "claude",
            CompletedAt = new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc)
        });
        _db.UpsertJob(new JobItem
        {
            Id = "job-002",
            Type = "MakePr",
            PlanFile = "plan-b",
            Project = "Tendril",
            Status = JobStatus.Completed,
            Provider = "claude",
            CompletedAt = new DateTime(2026, 4, 7, 11, 0, 0, DateTimeKind.Utc)
        });

        var jobs = _db.GetRecentJobs();
        Assert.Equal(3, jobs.Count);
        Assert.Equal("job-003", jobs[0].Id); // Most recent first
        Assert.Equal("job-002", jobs[1].Id);
        Assert.Equal("job-001", jobs[2].Id);
    }

    [Fact]
    public void UpsertJob_UpdatesExistingJob()
    {
        var job = new JobItem
        {
            Id = "job-001",
            Type = "ExecutePlan",
            PlanFile = "plan-a",
            Project = "Tendril",
            Status = JobStatus.Completed,
            Provider = "claude",
            CompletedAt = new DateTime(2026, 4, 7, 10, 0, 0, DateTimeKind.Utc),
            Cost = null,
            Tokens = null
        };
        _db.UpsertJob(job);

        // Update with cost and tokens
        job.Cost = 2.50m;
        job.Tokens = 75000;
        _db.UpsertJob(job);

        var jobs = _db.GetRecentJobs();
        Assert.Single(jobs);
        Assert.Equal(2.50m, jobs[0].Cost);
        Assert.Equal(75000, jobs[0].Tokens);
    }

    [Fact]
    public void PurgeOldJobs_RemovesExcessJobs()
    {
        // Insert 600 jobs with distinct CompletedAt times
        for (var i = 0; i < 600; i++)
            _db.UpsertJob(new JobItem
            {
                Id = $"job-{i:D4}",
                Type = "ExecutePlan",
                PlanFile = $"plan-{i}",
                Project = "Tendril",
                Status = JobStatus.Completed,
                Provider = "claude",
                CompletedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(i)
            });

        Assert.Equal(600, _db.GetRecentJobs(1000).Count);

        _db.PurgeOldJobs();

        var remaining = _db.GetRecentJobs(1000);
        Assert.Equal(500, remaining.Count);

        // The oldest jobs (lowest i) should have been removed
        Assert.DoesNotContain(remaining, j => j.Id == "job-0000");
        Assert.DoesNotContain(remaining, j => j.Id == "job-0099");
        // The newest jobs should remain
        Assert.Contains(remaining, j => j.Id == "job-0599");
        Assert.Contains(remaining, j => j.Id == "job-0100");
    }

    [Fact]
    public void PurgeOldJobs_NoOpWhenUnderLimit()
    {
        for (var i = 0; i < 10; i++)
            _db.UpsertJob(new JobItem
            {
                Id = $"job-{i}",
                Type = "ExecutePlan",
                PlanFile = $"plan-{i}",
                Project = "Tendril",
                Status = JobStatus.Completed,
                Provider = "claude",
                CompletedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(i)
            });

        _db.PurgeOldJobs();

        var remaining = _db.GetRecentJobs(1000);
        Assert.Equal(10, remaining.Count);
    }

    [Fact]
    public void UpsertPlan_WithSourceUrl_PreservesValue()
    {
        var metadata = new PlanMetadata(
            1600, "Tendril", "NiceToHave", "Source URL Plan", PlanStatus.Draft,
            new List<string>(), new List<string>(), new List<string>(),
            new List<PlanVerificationEntry>(), new List<string>(), new List<string>(),
            DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, null,
            "https://github.com/Ivy-Interactive/Ivy-Framework/pull/42"
        );
        var plan = new PlanFile(metadata, "# Content", "D:\\Plans\\01600-SourceUrlPlan", "state: Draft");
        _db.UpsertPlan(plan);

        var result = _db.GetPlanById(1600);
        Assert.NotNull(result);
        Assert.Equal("https://github.com/Ivy-Interactive/Ivy-Framework/pull/42", result.SourceUrl);
    }

    [Fact]
    public void UpsertPlan_WithNullSourceUrl_ReturnsNull()
    {
        var plan = CreateTestPlan(1601, "No Source URL Plan");
        _db.UpsertPlan(plan);

        var result = _db.GetPlanById(1601);
        Assert.NotNull(result);
        Assert.Null(result.SourceUrl);
    }

    [Fact]
    public void Migration_004_AddsSourceUrlColumn()
    {
        // The _db created in constructor already ran all migrations including 004.
        // Verify SourceUrl column exists by inserting and querying a plan with it.
        var metadata = new PlanMetadata(
            1602, "Tendril", "NiceToHave", "Migration Test", PlanStatus.Draft,
            new List<string>(), new List<string>(), new List<string>(),
            new List<PlanVerificationEntry>(), new List<string>(), new List<string>(),
            DateTime.UtcNow, DateTime.UtcNow, null,
            "https://github.com/test/repo/issues/99"
        );
        var plan = new PlanFile(metadata, "# Test", "D:\\Plans\\01602-MigrationTest", "state: Draft");
        _db.UpsertPlan(plan);

        var result = _db.GetPlanById(1602);
        Assert.NotNull(result);
        Assert.Equal("https://github.com/test/repo/issues/99", result.SourceUrl);
    }

    [Fact]
    public void Migration_003_CreatesJobsTable()
    {
        // The _db created in constructor already ran all migrations.
        // Verify Jobs table exists by inserting and querying.
        _db.UpsertJob(new JobItem
        {
            Id = "migration-test",
            Type = "Test",
            PlanFile = "test",
            Project = "Test",
            Status = JobStatus.Completed,
            Provider = "claude"
        });

        var jobs = _db.GetRecentJobs();
        Assert.Single(jobs);
        Assert.Equal("migration-test", jobs[0].Id);
    }

    [Fact]
    public void ConcurrentReads_DoNotBlockEachOther()
    {
        // Seed some data so reads return non-empty results
        for (var i = 0; i < 10; i++)
            _db.UpsertPlan(CreateTestPlan(2000 + i, $"Concurrent Plan {i}",
                status: i % 2 == 0 ? PlanStatus.Draft : PlanStatus.Completed));

        const int threadCount = 10;
        var barrier = new Barrier(threadCount);
        var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();
        var threads = new Thread[threadCount];

        for (var t = 0; t < threadCount; t++)
        {
            var threadIndex = t;
            threads[t] = new Thread(() =>
            {
                try
                {
                    barrier.SignalAndWait(TimeSpan.FromSeconds(5));
                    // Alternate between GetPlans and ComputePlanCounts
                    if (threadIndex % 2 == 0)
                    {
                        var plans = _db.GetPlans();
                        Assert.Equal(10, plans.Count);
                    }
                    else
                    {
                        var counts = _db.ComputePlanCounts();
                        Assert.True(counts.Drafts + counts.ReadyForReview + counts.Failed + counts.Icebox >= 0);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });
        }

        foreach (var t in threads) t.Start();
        foreach (var t in threads) Assert.True(t.Join(TimeSpan.FromSeconds(10)), "Thread timed out — possible deadlock");

        Assert.Empty(exceptions);
    }

    [Fact]
    public void GetDashboardData_EmptyDatabase_ReturnsZeroCounts()
    {
        var stats = _db.GetDashboardData(null);

        Assert.Equal(0, stats.TotalCount);
        Assert.Equal(0, stats.DraftCount);
        Assert.Equal(0, stats.InProgressCount);
        Assert.Equal(0, stats.ReviewCount);
        Assert.Equal(0, stats.CompletedCount);
        Assert.Equal(0, stats.FailedCount);
        Assert.Equal(0m, stats.AvgCostPerPlan);
        Assert.Equal(7, stats.DailyStats.Count);
        Assert.All(stats.DailyStats, d =>
        {
            Assert.Equal(0, d.Created);
            Assert.Equal(0, d.Completed);
            Assert.Equal(0, d.Failed);
            Assert.Equal(0, d.PrsMerged);
            Assert.Equal(0m, d.Cost);
            Assert.Equal(0, d.Tokens);
        });
        Assert.Empty(stats.ProjectCounts);
    }

    [Fact]
    public void GetDashboardData_ProjectFilterNoMatch_ReturnsZeroCounts()
    {
        _db.UpsertPlan(CreateTestPlan(2100, "Tendril Plan", project: "Tendril"));

        var stats = _db.GetDashboardData("NonExistent");

        Assert.Equal(0, stats.TotalCount);
        Assert.Equal(0, stats.DraftCount);
        Assert.Equal(0, stats.InProgressCount);
        Assert.Equal(0, stats.ReviewCount);
        Assert.Equal(0, stats.CompletedCount);
        Assert.Equal(0, stats.FailedCount);
        Assert.Equal(0m, stats.AvgCostPerPlan);
        Assert.Equal(7, stats.DailyStats.Count);
        Assert.All(stats.DailyStats, d =>
        {
            Assert.Equal(0, d.Created);
            Assert.Equal(0, d.Completed);
            Assert.Equal(0, d.Failed);
            Assert.Equal(0, d.PrsMerged);
            Assert.Equal(0m, d.Cost);
            Assert.Equal(0, d.Tokens);
        });
    }

    [Fact]
    public void SearchPlans_FindsBySourceUrl()
    {
        var metadata = new PlanMetadata(
            1700, "Tendril", "NiceToHave", "Plan With Source", PlanStatus.Draft,
            new List<string>(), new List<string>(), new List<string>(),
            new List<PlanVerificationEntry>(), new List<string>(), new List<string>(),
            DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, null,
            "https://github.com/Ivy-Interactive/Ivy-Framework/issues/42"
        );
        var plan = new PlanFile(metadata, "# Some content", "D:\\Plans\\01700-PlanWithSource", "state: Draft");
        _db.UpsertPlan(plan);

        // Search by term from the URL (FTS5 tokenizes on punctuation)
        var results = _db.SearchPlans("Interactive");
        Assert.Single(results);
        Assert.Equal("Plan With Source", results[0].Title);

        // Search by another URL token
        results = _db.SearchPlans("issues");
        Assert.Single(results);
        Assert.Equal(1700, results[0].Id);
    }
}