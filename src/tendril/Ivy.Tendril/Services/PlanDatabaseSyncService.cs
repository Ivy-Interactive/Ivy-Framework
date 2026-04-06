using System.Globalization;
using Ivy.Tendril.Apps.Plans;
using Microsoft.Extensions.Logging;

namespace Ivy.Tendril.Services;

public class PlanDatabaseSyncService : IDisposable
{
    private readonly PlanReaderService _planReader;
    private readonly IPlanDatabaseService _database;
    private readonly IPlanWatcherService _watcher;
    private readonly ILogger<PlanDatabaseSyncService> _logger;
    private readonly object _syncLock = new();
    private volatile bool _isInitialSyncComplete;

    public PlanDatabaseSyncService(
        PlanReaderService planReader,
        IPlanDatabaseService database,
        IPlanWatcherService watcher,
        ILogger<PlanDatabaseSyncService> logger)
    {
        _planReader = planReader;
        _database = database;
        _watcher = watcher;
        _logger = logger;

        _watcher.PlansChanged += OnPlansChanged;
    }

    public bool IsInitialSyncComplete => _isInitialSyncComplete;

    public void PerformInitialSync()
    {
        try
        {
            _logger.LogInformation("Starting initial database sync...");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Read directly from file system to avoid circular dependency
            var plans = _planReader.GetPlansFromFileSystem();

            lock (_syncLock)
            {
                _database.BulkUpsertPlans(plans);

                foreach (var plan in plans)
                {
                    SyncPlanCosts(plan);
                    SyncPlanRecommendations(plan);
                }

                _database.SetLastSyncTime(DateTime.UtcNow);
            }

            _isInitialSyncComplete = true;

            // Enable database-backed reads in PlanReaderService
            _planReader.EnableDatabaseReads(_database);

            stopwatch.Stop();
            _logger.LogInformation("Initial sync complete. Synced {Count} plans in {Ms}ms",
                plans.Count, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Initial database sync failed");
            _isInitialSyncComplete = true;
        }
    }

    private void OnPlansChanged()
    {
        if (!_isInitialSyncComplete) return;

        try
        {
            // Read from file system outside the lock (IO-bound, no DB access)
            var plans = _planReader.GetPlansFromFileSystem();

            lock (_syncLock)
            {
                _database.BulkUpsertPlans(plans);

                foreach (var plan in plans)
                {
                    SyncPlanCosts(plan);
                    SyncPlanRecommendations(plan);
                }

                _database.SetLastSyncTime(DateTime.UtcNow);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Incremental sync failed");
        }
    }

    private void SyncPlanCosts(PlanFile plan)
    {
        var costsPath = Path.Combine(plan.FolderPath, "costs.csv");
        if (!File.Exists(costsPath)) return;

        try
        {
            var lines = FileHelper.ReadAllLines(costsPath);
            var logsDir = Path.Combine(plan.FolderPath, "logs");

            // Build log file map for timestamp correlation
            var logsByPromptware = new Dictionary<string, Queue<(string Path, int Num)>>(StringComparer.OrdinalIgnoreCase);
            if (Directory.Exists(logsDir))
            {
                var logFiles = Directory.GetFiles(logsDir, "*.md")
                    .Select(f =>
                    {
                        var name = Path.GetFileNameWithoutExtension(f);
                        var dashIdx = name.IndexOf('-');
                        if (dashIdx < 0) return (Promptware: name, Path: f, Num: 0);
                        var numPart = name[..dashIdx];
                        var pwName = name[(dashIdx + 1)..];
                        int.TryParse(numPart, out var num);
                        return (Promptware: pwName, Path: f, Num: num);
                    })
                    .OrderBy(l => l.Num)
                    .ToList();

                foreach (var log in logFiles)
                {
                    if (!logsByPromptware.ContainsKey(log.Promptware))
                        logsByPromptware[log.Promptware] = new Queue<(string, int)>();
                    logsByPromptware[log.Promptware].Enqueue((log.Path, log.Num));
                }
            }

            var costs = new List<CostEntry>();
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length < 3) continue;

                var promptware = parts[0].Trim();
                if (!int.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var tokens)) continue;
                if (!decimal.TryParse(parts[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var cost)) continue;

                DateTime? timestamp = null;
                if (logsByPromptware.TryGetValue(promptware, out var queue) && queue.Count > 0)
                {
                    var logEntry = queue.Dequeue();
                    timestamp = ExtractCompletedTimestamp(logEntry.Path);
                }

                costs.Add(new CostEntry(promptware, tokens, cost, timestamp));
            }

            _database.UpsertCosts(plan.Id, costs);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to sync costs for plan {PlanId}", plan.Id);
        }
    }

    private void SyncPlanRecommendations(PlanFile plan)
    {
        var recommendationsPath = Path.Combine(plan.FolderPath, "artifacts", "recommendations.yaml");
        if (!File.Exists(recommendationsPath)) return;

        try
        {
            var yaml = FileHelper.ReadAllText(recommendationsPath);
            var items = YamlHelper.Deserializer.Deserialize<List<RecommendationYaml>>(yaml);
            if (items == null) return;

            _database.UpsertRecommendations(plan.Id, plan.FolderName, items,
                plan.Project, plan.Title, plan.Updated, plan.Status);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to sync recommendations for plan {PlanId}", plan.Id);
        }
    }

    private static DateTime? ExtractCompletedTimestamp(string logFilePath)
        => FileHelper.ExtractCompletedTimestamp(logFilePath);

    public void Dispose()
    {
        _watcher.PlansChanged -= OnPlansChanged;
    }
}
