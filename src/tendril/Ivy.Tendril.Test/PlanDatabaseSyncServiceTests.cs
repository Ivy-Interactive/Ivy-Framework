using Ivy.Tendril.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ivy.Tendril.Test;

public class PlanDatabaseSyncServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _dbPath;
    private readonly PlanReaderService _planReader;
    private readonly PlanDatabaseService _database;
    private readonly PlanWatcherService _watcher;
    private readonly PlanDatabaseSyncService _syncService;

    public PlanDatabaseSyncServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tendril-sync-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);

        _dbPath = Path.Combine(_tempDir, "tendril.db");

        var settings = new TendrilSettings();
        var configService = new ConfigService(settings, _tempDir);
        _planReader = new PlanReaderService(configService, NullLogger<PlanReaderService>.Instance);
        _database = new PlanDatabaseService(_dbPath);
        _watcher = new PlanWatcherService(configService);
        _syncService = new PlanDatabaseSyncService(
            _planReader, _database, _watcher,
            NullLogger<PlanDatabaseSyncService>.Instance);
    }

    public void Dispose()
    {
        _syncService.Dispose();
        _watcher.Dispose();
        _database.Dispose();
        SqliteConnection.ClearAllPools();

        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private void CreatePlan(string folderName, string yaml, string? revisionContent = null)
    {
        var dir = Path.Combine(_planReader.PlansDirectory, folderName);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "plan.yaml"), yaml);

        if (revisionContent != null)
        {
            var revisionsDir = Path.Combine(dir, "revisions");
            Directory.CreateDirectory(revisionsDir);
            File.WriteAllText(Path.Combine(revisionsDir, "001.md"), revisionContent);
        }
    }

    [Fact]
    public void PerformInitialSync_SyncsPlansToDatabase()
    {
        var yaml = "state: Draft\nproject: Tendril\ntitle: Test Plan\nlevel: NiceToHave\nrepos: []\ncommits: []\nprs: []\nverifications: []\nrelatedPlans: []\ndependsOn: []\ncreated: 2026-01-01T00:00:00Z\nupdated: 2026-01-01T00:00:00Z\n";
        CreatePlan("01500-TestPlan", yaml, "# Test Plan Content");
        CreatePlan("01501-AnotherPlan", yaml.Replace("Test Plan", "Another Plan"), "# Another");

        _syncService.PerformInitialSync();

        Assert.True(_syncService.IsInitialSyncComplete);

        var plans = _database.GetPlans();
        Assert.Equal(2, plans.Count);
    }

    [Fact]
    public void PerformInitialSync_EnablesDatabaseReads()
    {
        var yaml = "state: Draft\nproject: Tendril\ntitle: Test Plan\nlevel: NiceToHave\nrepos: []\ncommits: []\nprs: []\nverifications: []\nrelatedPlans: []\ndependsOn: []\ncreated: 2026-01-01T00:00:00Z\nupdated: 2026-01-01T00:00:00Z\n";
        CreatePlan("01500-TestPlan", yaml, "# Test");

        _syncService.PerformInitialSync();

        // After sync, PlanReaderService should use database
        var plans = _planReader.GetPlans();
        Assert.Single(plans);
        Assert.Equal("Test Plan", plans[0].Title);
    }

    [Fact]
    public void PerformInitialSync_SyncsCosts()
    {
        var yaml = "state: Completed\nproject: Tendril\ntitle: Cost Plan\nlevel: NiceToHave\nrepos: []\ncommits: []\nprs: []\nverifications: []\nrelatedPlans: []\ndependsOn: []\ncreated: 2026-01-01T00:00:00Z\nupdated: 2026-01-01T00:00:00Z\n";
        var dir = Path.Combine(_planReader.PlansDirectory, "01500-CostPlan");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "plan.yaml"), yaml);
        var revisionsDir = Path.Combine(dir, "revisions");
        Directory.CreateDirectory(revisionsDir);
        File.WriteAllText(Path.Combine(revisionsDir, "001.md"), "# Cost Plan");
        File.WriteAllText(Path.Combine(dir, "costs.csv"), "promptware,tokens,cost\nExecutePlan,50000,1.50\nMakePr,10000,0.30\n");

        _syncService.PerformInitialSync();

        var totalCost = _database.GetPlanTotalCost(1500);
        Assert.Equal(1.80m, totalCost);
    }

    [Fact]
    public void PerformInitialSync_SyncsRecommendations()
    {
        var yaml = "state: Completed\nproject: Tendril\ntitle: Rec Plan\nlevel: NiceToHave\nrepos: []\ncommits: []\nprs: []\nverifications: []\nrelatedPlans: []\ndependsOn: []\ncreated: 2026-01-01T00:00:00Z\nupdated: 2026-01-01T00:00:00Z\n";
        var dir = Path.Combine(_planReader.PlansDirectory, "01500-RecPlan");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "plan.yaml"), yaml);
        var revisionsDir = Path.Combine(dir, "revisions");
        Directory.CreateDirectory(revisionsDir);
        File.WriteAllText(Path.Combine(revisionsDir, "001.md"), "# Rec Plan");
        var artifactsDir = Path.Combine(dir, "artifacts");
        Directory.CreateDirectory(artifactsDir);
        File.WriteAllText(Path.Combine(artifactsDir, "recommendations.yaml"),
            "- title: Add tests\n  description: Need more tests\n  state: Pending\n");

        _syncService.PerformInitialSync();

        var recs = _database.GetRecommendations();
        Assert.Single(recs);
        Assert.Equal("Add tests", recs[0].Title);
    }

    [Fact]
    public void PerformInitialSync_WithEmptyPlansDirectory_Succeeds()
    {
        _syncService.PerformInitialSync();

        Assert.True(_syncService.IsInitialSyncComplete);
        Assert.Empty(_database.GetPlans());
    }

    [Fact]
    public void PerformInitialSync_SetsLastSyncTime()
    {
        _syncService.PerformInitialSync();

        var syncTime = _database.GetLastSyncTime();
        Assert.True(syncTime > DateTime.MinValue);
    }
}
