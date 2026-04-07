using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Ivy.Tendril.Apps.Plans;
using Microsoft.Extensions.Logging;

namespace Ivy.Tendril.Services;

public class PlanReaderService(
    IConfigService config,
    ILogger<PlanReaderService> logger,
    ITelemetryService? telemetryService = null) : IPlanReaderService
{
    private static readonly Regex FolderNameRegex = new(@"^(\d{5})-(.+)$", RegexOptions.Compiled);
    private readonly IConfigService _config = config;

    private readonly TimeCache<List<HourlyTokenBurn>> _hourlyBurnCache = new(TimeSpan.FromMinutes(2));
    private readonly ILogger<PlanReaderService> _logger = logger;

    private readonly TimeCache<Dictionary<string, (decimal Cost, int Tokens)>> _planCostCache =
        new(TimeSpan.FromSeconds(90));

    private readonly TimeCache<PlanCountSnapshot> _planCountsCache = new(TimeSpan.FromMinutes(2));

    private readonly TimeCache<List<Recommendation>> _recommendationsCache = new(TimeSpan.FromMinutes(2));
    private readonly ITelemetryService? _telemetryService = telemetryService;

    private IPlanDatabaseService? _database;
    private volatile bool _useDatabaseForReads;

    public string PlansDirectory => _config.PlanFolder;

    /// <summary>
    ///     On startup, reset any plans stuck in transient states (Building, Executing, Updating)
    ///     back to Failed. These are leftovers from a previous Tendril shutdown.
    /// </summary>
    public void RecoverStuckPlans()
    {
        var stuckStates = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "Building", "Executing", "Updating", "Blocked" };

        if (!Directory.Exists(PlansDirectory)) return;

        foreach (var dir in Directory.GetDirectories(PlansDirectory))
            try
            {
                var planYamlPath = Path.Combine(dir, "plan.yaml");
                if (!File.Exists(planYamlPath)) continue;

                var yaml = FileHelper.ReadAllText(planYamlPath);
                var stateMatch = Regex.Match(yaml, @"(?m)^state:\s*(.+)$");
                if (!stateMatch.Success) continue;

                var state = stateMatch.Groups[1].Value.Trim();
                if (stuckStates.Contains(state))
                {
                    var newState = state.Equals("Executing", StringComparison.OrdinalIgnoreCase)
                        ? "Failed"
                        : "Draft";
                    var updated = Regex.Replace(yaml, @"(?m)^state:\s*.*$", $"state: {newState}");
                    updated = Regex.Replace(updated, @"(?m)^updated:\s*.*$",
                        $"updated: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
                    FileHelper.WriteAllText(planYamlPath, updated);
                    _planCountsCache.Invalidate();
                    _recommendationsCache.Invalidate();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Failed to recover plan {Path.GetFileName(dir)}: {ex.Message}");
            }
    }

    /// <summary>
    ///     On startup, fix plan.yaml files that have structured repos (path: + prRule:)
    ///     by normalizing them to plain path strings.
    /// </summary>
    public void RepairPlans()
    {
        try
        {
            if (!Directory.Exists(PlansDirectory)) return;

            foreach (var dir in Directory.GetDirectories(PlansDirectory))
                try
                {
                    var planYamlPath = Path.Combine(dir, "plan.yaml");
                    if (!File.Exists(planYamlPath)) continue;

                    var yaml = FileHelper.ReadAllText(planYamlPath);
                    var repaired = RepairPlanYaml(yaml);

                    if (repaired != yaml)
                    {
                        FileHelper.WriteAllText(planYamlPath, repaired);
                        _logger.LogInformation("Repaired plan.yaml in {Folder}", Path.GetFileName(dir));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to repair plan in {Folder}", Path.GetFileName(dir));
                }
        }
        catch
        {
            /* Best-effort repair on startup; individual plan errors are non-fatal */
        }
    }

    /// <summary>
    ///     Retrieves all plans, optionally filtered by status.
    ///     Delegates to database when available, otherwise reads from file system.
    /// </summary>
    public List<PlanFile> GetPlans(PlanStatus? statusFilter = null)
    {
        if (_useDatabaseForReads && _database != null)
            try
            {
                return _database.GetPlans(statusFilter);
            }
            catch
            {
                // Fall back to file system on database errors
            }

        return GetPlansFromFileSystem(statusFilter);
    }

    /// <summary>
    ///     Retrieves a single plan by its folder path.
    ///     Delegates to database when available, otherwise reads from file system.
    /// </summary>
    public PlanFile? GetPlanByFolder(string folderPath)
    {
        if (_useDatabaseForReads && _database != null)
            try
            {
                return _database.GetPlanByFolder(folderPath);
            }
            catch
            {
                // Fall back to file system
            }

        if (!Directory.Exists(folderPath)) return null;
        return ParsePlanFolder(folderPath);
    }

    /// <summary>
    ///     Retrieves all plans that are in the <see cref="PlanStatus.Icebox" /> state.
    /// </summary>
    /// <returns>List of icebox plans ordered by ID.</returns>
    public List<PlanFile> GetIceboxPlans()
    {
        return GetPlans(PlanStatus.Icebox);
    }

    /// <summary>
    ///     Transitions a plan to a new state by updating its <c>plan.yaml</c> file.
    /// </summary>
    /// <param name="folderName">Name of the plan folder (e.g. <c>01105-TestPlan</c>).</param>
    /// <param name="newState">The target state to transition to.</param>
    public void TransitionState(string folderName, PlanStatus newState)
    {
        var planId = ExtractPlanId(folderName);

        // Track state transition in telemetry before making the change.
        if (planId.HasValue)
        {
            var currentPlan = GetPlanByFolder(Path.Combine(PlansDirectory, folderName));
            var oldState = currentPlan?.Status.ToString() ?? "Unknown";
            _telemetryService?.TrackPlanStateTransition(oldState, newState.ToString());

            // Flush telemetry events to ensure they reach PostHog
            if (_telemetryService != null)
                _ = Task.Run(async () => await _telemetryService.FlushAsync());
        }

        // Update database first for instant UI feedback.
        if (planId.HasValue && _database != null)
            _database.UpdatePlanState(planId.Value, newState);

        _planCountsCache.Invalidate();
        _recommendationsCache.Invalidate();

        // Write to disk in background for durability.
        WriteFileInBackground(() =>
        {
            var planYamlPath = Path.Combine(PlansDirectory, folderName, "plan.yaml");
            if (!File.Exists(planYamlPath)) return;

            var yaml = FileHelper.ReadAllText(planYamlPath);
            var planYaml = YamlHelper.Deserializer.Deserialize<PlanYaml>(yaml) ?? new PlanYaml();
            planYaml.State = newState.ToString();
            planYaml.Updated = DateTime.UtcNow;
            FileHelper.WriteAllText(planYamlPath, YamlHelper.Serializer.Serialize(planYaml));
        });
    }

    /// <summary>
    ///     Creates a new revision file for a plan and updates the plan's timestamp.
    /// </summary>
    /// <param name="folderName">Name of the plan folder.</param>
    /// <param name="content">Markdown content of the new revision.</param>
    public void SaveRevision(string folderName, string content)
    {
        // Update database first for instant UI feedback.
        var planId = ExtractPlanId(folderName);
        if (planId.HasValue && _database != null)
        {
            var plan = _database.GetPlanById(planId.Value);
            var newCount = (plan?.RevisionCount ?? 0) + 1;
            _database.UpdatePlanContent(planId.Value, content, newCount);
        }

        _planCountsCache.Invalidate();

        // Write to disk in background for durability.
        WriteFileInBackground(() =>
        {
            var revisionsDir = Path.Combine(PlansDirectory, folderName, "revisions");
            Directory.CreateDirectory(revisionsDir);

            var nextNumber = GetNextRevisionNumber(revisionsDir);
            var revisionPath = Path.Combine(revisionsDir, $"{nextNumber:D3}.md");
            FileHelper.WriteAllText(revisionPath, content);

            var planYamlPath = Path.Combine(PlansDirectory, folderName, "plan.yaml");
            if (File.Exists(planYamlPath))
            {
                var yaml = FileHelper.ReadAllText(planYamlPath);
                var planYaml = YamlHelper.Deserializer.Deserialize<PlanYaml>(yaml) ?? new PlanYaml();
                planYaml.Updated = DateTime.UtcNow;
                FileHelper.WriteAllText(planYamlPath, YamlHelper.Serializer.Serialize(planYaml));
            }
        });
    }

    /// <summary>
    ///     Reads the content of the most recent revision file for a plan.
    /// </summary>
    /// <param name="folderName">Name of the plan folder.</param>
    /// <returns>The markdown content of the latest revision, or <see cref="string.Empty" /> if no revisions exist.</returns>
    public string ReadLatestRevision(string folderName)
    {
        // Read from database when available for maximum performance.
        if (_useDatabaseForReads && _database != null)
        {
            var planId = ExtractPlanId(folderName);
            if (planId.HasValue)
            {
                var plan = _database.GetPlanById(planId.Value);
                if (plan != null)
                    return plan.LatestRevisionContent;
            }
        }

        return ReadLatestRevisionFromFileSystem(folderName);
    }

    /// <summary>
    ///     Gets all revisions for a plan, ordered by revision number.
    /// </summary>
    /// <param name="folderName">Name of the plan folder.</param>
    /// <returns>List of tuples containing revision number, content, and last-modified timestamp (UTC).</returns>
    public List<(int Number, string Content, DateTime Modified)> GetRevisions(string folderName)
    {
        var revisionsDir = Path.Combine(PlansDirectory, folderName, "revisions");
        if (!Directory.Exists(revisionsDir)) return new List<(int, string, DateTime)>();

        return Directory.GetFiles(revisionsDir, "*.md")
            .Select(f =>
            {
                var name = Path.GetFileNameWithoutExtension(f);
                if (int.TryParse(name, out var num))
                    return (Number: num, Content: FileHelper.ReadAllText(f), Modified: File.GetLastWriteTimeUtc(f));
                return (Number: -1, Content: "", Modified: DateTime.MinValue);
            })
            .Where(r => r.Number >= 0)
            .OrderBy(r => r.Number)
            .ToList();
    }

    /// <summary>
    ///     Appends a log entry to a plan's logs directory.
    /// </summary>
    /// <param name="folderName">Name of the plan folder.</param>
    /// <param name="action">Action name used in the log filename (e.g. <c>ExecutePlan</c>).</param>
    /// <param name="content">Markdown content of the log entry.</param>
    public void AddLog(string folderName, string action, string content)
    {
        var logsDir = Path.Combine(PlansDirectory, folderName, "logs");
        Directory.CreateDirectory(logsDir);

        var nextNumber = 1;
        var existingLogs = Directory.GetFiles(logsDir, "*.md");
        if (existingLogs.Length > 0)
            nextNumber = existingLogs
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .Select(n =>
                {
                    var dashIdx = n.IndexOf('-');
                    var numPart = dashIdx >= 0 ? n.Substring(0, dashIdx) : n;
                    return int.TryParse(numPart, out var num) ? num : 0;
                })
                .DefaultIfEmpty(0)
                .Max() + 1;

        var logPath = Path.Combine(logsDir, $"{nextNumber:D3}-{action}.md");
        FileHelper.WriteAllText(logPath, content);
    }

    /// <summary>
    ///     Deletes a plan folder and all its contents, including any associated git worktrees.
    /// </summary>
    /// <param name="folderName">Name of the plan folder.</param>
    /// <remarks>
    ///     Removes git worktrees first to avoid locked file issues on Windows, then clears
    ///     read-only attributes before deleting the directory tree.
    /// </remarks>
    public void DeletePlan(string folderName)
    {
        // Delete from database first for instant UI feedback.
        var planId = ExtractPlanId(folderName);
        if (planId.HasValue && _database != null)
            _database.DeletePlan(planId.Value);

        _planCountsCache.Invalidate();
        _recommendationsCache.Invalidate();

        // Delete folder in background (can be slow due to git worktree removal).
        var folderPath = Path.Combine(PlansDirectory, folderName);
        WriteFileInBackground(() =>
        {
            if (!Directory.Exists(folderPath)) return;
            RemoveWorktrees(folderPath);
            ClearReadOnlyAttributes(folderPath);
            Directory.Delete(folderPath, true);
        });
    }

    /// <summary>
    ///     Reads the raw content of a plan's latest revision.
    /// </summary>
    /// <param name="folderName">Name of the plan folder.</param>
    /// <returns>The markdown content of the latest revision.</returns>
    public string ReadRawPlan(string folderName)
    {
        return ReadLatestRevision(folderName);
    }

    /// <summary>
    ///     Saves content to a plan by overwriting its latest revision.
    /// </summary>
    /// <param name="folderName">Name of the plan folder.</param>
    /// <param name="fullContent">The full markdown content to write.</param>
    public void SavePlan(string folderName, string fullContent)
    {
        UpdateLatestRevision(folderName, fullContent);
    }

    /// <summary>
    ///     Overwrites the most recent revision file for a plan and updates the plan's timestamp.
    /// </summary>
    /// <param name="folderName">Name of the plan folder.</param>
    /// <param name="content">The updated markdown content to write.</param>
    public void UpdateLatestRevision(string folderName, string content)
    {
        // Update database first for instant UI feedback.
        var planId = ExtractPlanId(folderName);
        if (planId.HasValue && _database != null)
        {
            var plan = _database.GetPlanById(planId.Value);
            _database.UpdatePlanContent(planId.Value, content, plan?.RevisionCount ?? 1);
        }

        _planCountsCache.Invalidate();

        // Write to disk in background for durability.
        WriteFileInBackground(() =>
        {
            var revisionsDir = Path.Combine(PlansDirectory, folderName, "revisions");
            if (!Directory.Exists(revisionsDir)) return;

            var latestFile = Directory.GetFiles(revisionsDir, "*.md")
                .OrderByDescending(f => f)
                .FirstOrDefault();

            if (latestFile != null)
            {
                FileHelper.WriteAllText(latestFile, content);

                var planYamlPath = Path.Combine(PlansDirectory, folderName, "plan.yaml");
                if (File.Exists(planYamlPath))
                {
                    var yaml = FileHelper.ReadAllText(planYamlPath);
                    var planYaml = YamlHelper.Deserializer.Deserialize<PlanYaml>(yaml) ?? new PlanYaml();
                    planYaml.Updated = DateTime.UtcNow;
                    FileHelper.WriteAllText(planYamlPath, YamlHelper.Serializer.Serialize(planYaml));
                }
            }
        });
    }

    /// <summary>
    ///     Calculates the total cost for a plan. Delegates to database when available,
    ///     otherwise parses costs.csv with a short cache to reduce file I/O.
    /// </summary>
    public decimal GetPlanTotalCost(string folderPath)
    {
        if (_useDatabaseForReads && _database != null)
        {
            var planId = ExtractPlanId(folderPath);
            if (planId.HasValue)
                return _database.GetPlanTotalCost(planId.Value);
        }

        var dict = _planCostCache.GetOrCompute(() => new Dictionary<string, (decimal, int)>());
        if (!dict.TryGetValue(folderPath, out var cached))
        {
            cached = ComputePlanCostAndTokens(folderPath);
            dict[folderPath] = cached;
        }

        return cached.Cost;
    }

    /// <summary>
    ///     Calculates the total token usage for a plan. Delegates to database when available,
    ///     otherwise parses costs.csv with a short cache to reduce file I/O.
    /// </summary>
    public int GetPlanTotalTokens(string folderPath)
    {
        if (_useDatabaseForReads && _database != null)
        {
            var planId = ExtractPlanId(folderPath);
            if (planId.HasValue)
                return _database.GetPlanTotalTokens(planId.Value);
        }

        var dict = _planCostCache.GetOrCompute(() => new Dictionary<string, (decimal, int)>());
        if (!dict.TryGetValue(folderPath, out var cached))
        {
            cached = ComputePlanCostAndTokens(folderPath);
            dict[folderPath] = cached;
        }

        return cached.Tokens;
    }

    /// <summary>
    ///     Computes hourly token usage and cost statistics across all plans over a given time window.
    /// </summary>
    /// <param name="days">Number of days to look back from now. Defaults to 7.</param>
    /// <returns>List of hourly buckets with aggregated cost and token counts, ordered chronologically.</returns>
    /// <remarks>
    ///     Correlates <c>costs.csv</c> entries with log file timestamps to determine when tokens were consumed.
    ///     Plans without both a costs file and a logs directory are skipped.
    /// </remarks>
    public List<HourlyTokenBurn> GetHourlyTokenBurn(int days = 7)
    {
        if (_useDatabaseForReads && _database != null)
            try
            {
                return _database.GetHourlyTokenBurn(days);
            }
            catch
            {
                // Fall back to file system
            }

        return _hourlyBurnCache.GetOrCompute(() => ComputeHourlyTokenBurn(days));
    }

    /// <summary>
    ///     Gets all recommendations from all plans. This method reads and deserializes all
    ///     recommendations.yaml files in the plans directory, which can be expensive.
    /// </summary>
    /// <remarks>
    ///     If you only need the count of pending recommendations, use <see cref="GetPendingRecommendationsCount" />
    ///     instead, which uses an optimized path via <see cref="ComputePlanCounts" /> that counts
    ///     without full deserialization.
    /// </remarks>
    /// <returns>List of all recommendations ordered by date (most recent first).</returns>
    public List<Recommendation> GetRecommendations()
    {
        if (_useDatabaseForReads && _database != null)
            try
            {
                return _database.GetRecommendations();
            }
            catch
            {
                // Fall back to file system
            }

        return _recommendationsCache.GetOrCompute(ComputeRecommendations);
    }

    /// <summary>
    ///     Gets the count of pending recommendations efficiently without deserializing full recommendation objects.
    /// </summary>
    /// <remarks>
    ///     This method delegates to <see cref="ComputePlanCounts" /> which only counts pending items
    ///     without building full Recommendation objects, making it much more efficient than calling
    ///     <c>GetRecommendations().Count(r => r.State == "Pending")</c>.
    /// </remarks>
    /// <returns>Number of pending recommendations.</returns>
    public int GetPendingRecommendationsCount()
    {
        return ComputePlanCounts().PendingRecommendations;
    }

    /// <summary>
    ///     Efficiently computes plan counts by status and pending recommendation count.
    ///     Delegates to database when available; falls back to regex-based file scanning with caching.
    /// </summary>
    public PlanCountSnapshot ComputePlanCounts()
    {
        if (_useDatabaseForReads && _database != null)
            try
            {
                return _database.ComputePlanCounts();
            }
            catch
            {
                // Fall back to file system
            }

        return _planCountsCache.GetOrCompute(ComputePlanCountsInternal);
    }

    /// <summary>
    ///     Updates the state of a specific recommendation within a plan's <c>recommendations.yaml</c> file.
    /// </summary>
    /// <param name="planFolderName">Name of the plan folder containing the recommendation.</param>
    /// <param name="recommendationTitle">Exact title of the recommendation to update.</param>
    /// <param name="newState">The new state value (e.g. <c>Pending</c>, <c>Accepted</c>, <c>Dismissed</c>).</param>
    /// <param name="declineReason">Optional reason for declining the recommendation.</param>
    public void UpdateRecommendationState(string planFolderName, string recommendationTitle, string newState,
        string? declineReason = null)
    {
        // Update database first for instant UI feedback.
        var planId = ExtractPlanId(planFolderName);
        if (planId.HasValue && _database != null)
            _database.UpdateRecommendationState(planId.Value, recommendationTitle, newState, declineReason);

        _recommendationsCache.Invalidate();
        _hourlyBurnCache.Invalidate();
        _planCountsCache.Invalidate();

        // Without a backing database, writes need to complete before the next read.
        WriteFile(() =>
        {
            var recommendationsPath = Path.Combine(PlansDirectory, planFolderName, "artifacts", "recommendations.yaml");
            if (!File.Exists(recommendationsPath)) return;

            var yaml = FileHelper.ReadAllText(recommendationsPath);
            var items = YamlHelper.Deserializer.Deserialize<List<RecommendationYaml>>(yaml);
            if (items == null) return;

            var item = items.FirstOrDefault(r => r.Title == recommendationTitle);
            if (item == null) return;

            item.State = newState;
            if (newState == "Declined" && !string.IsNullOrWhiteSpace(declineReason))
                item.DeclineReason = declineReason;
            else
                item.DeclineReason = null;

            FileHelper.WriteAllText(recommendationsPath, YamlHelper.Serializer.Serialize(items));
        });
    }

    public void InvalidateCaches()
    {
        _planCountsCache.Invalidate();
        _recommendationsCache.Invalidate();
        _hourlyBurnCache.Invalidate();
        _planCostCache.Invalidate();
    }

    /// <summary>
    ///     Enables database-backed reads. Called by the sync service after initial sync completes.
    /// </summary>
    internal void EnableDatabaseReads(IPlanDatabaseService database)
    {
        _database = database;
        _useDatabaseForReads = true;
    }

    /// <summary>
    ///     Repairs common YAML issues in plan.yaml content so that YamlDotNet can parse them.
    /// </summary>
    internal static string RepairPlanYaml(string yaml)
    {
        var repaired = yaml;

        // 1. Remove YAML document markers (--- at start and end)
        repaired = Regex.Replace(repaired, @"^---\s*\r?\n", "");
        repaired = Regex.Replace(repaired, @"\r?\n---\s*$", "");

        // 2. Convert object-style repos (- name: X\n    path: Y\n    branch: ...) to plain path strings
        repaired = Regex.Replace(repaired,
            @"(?m)^(\s*)-\s+name:\s*.+\r?\n\s+path:\s*(.+?)(?:\r?\n\s+(?:branch|prRule):\s*.+)*$",
            "$1- $2");

        // 3. Fix structured repos (- path: X + optional prRule:/branch:) → plain path strings
        repaired = Regex.Replace(repaired,
            @"(?m)^(\s*)-\s+path:\s*(.+?)(?:\r?\n\s+(?:prRule|branch):\s*.+)*$",
            "$1- $2");

        // 4. Convert object-style commits (- hash: X\n  repo: ...\n  message: ...) to plain hash strings
        repaired = Regex.Replace(repaired,
            @"(?m)^(\s*)-\s+hash:\s*(.+?)(?:\r?\n\s+(?:repo|message):\s*.+)*$",
            "$1- $2");

        // 5. Convert object-style prs (- note: X) to plain strings
        repaired = Regex.Replace(repaired,
            @"(?m)^(\s*)-\s+note:\s*(.+)$",
            "$1- $2");

        // 6. Remove orphan branch: lines attached to string list items (e.g. - "path"\n    branch: "")
        repaired = Regex.Replace(repaired,
            @"(?m)^(\s*-\s+(?:""[^""]*""|'[^']*'|%\S+).*)\r?\n\s+branch:\s*.*$",
            "$1");

        // 7. Fix double-quoted strings containing unescaped backslashes → single quotes
        repaired = Regex.Replace(repaired, @"(?m)^([^:]+:\s+)""(.+)""(\s*)$", m =>
        {
            var prefix = m.Groups[1].Value;
            var inner = m.Groups[2].Value;
            var suffix = m.Groups[3].Value;
            if (Regex.IsMatch(inner, @"\\[^""\\nt/abfre0 UNLP_xu]"))
            {
                var escaped = inner.Replace("'", "''");
                return $"{prefix}'{escaped}'{suffix}";
            }

            return m.Value;
        });

        // 8. Fix double-quoted list items with bad escapes
        repaired = Regex.Replace(repaired, @"(?m)^(\s*-\s+)""(.+)""(\s*)$", m =>
        {
            var prefix = m.Groups[1].Value;
            var inner = m.Groups[2].Value;
            var suffix = m.Groups[3].Value;
            if (Regex.IsMatch(inner, @"\\[^""\\nt/abfre0 UNLP_xu]"))
            {
                var escaped = inner.Replace("'", "''");
                return $"{prefix}'{escaped}'{suffix}";
            }

            return m.Value;
        });

        // 9. Quote unquoted Windows paths in list items: - D:\something → - 'D:\something'
        repaired = Regex.Replace(repaired, @"(?m)^(\s*-\s+)([A-Za-z]:\\[^\s].*)$", m =>
        {
            var prefix = m.Groups[1].Value;
            var path = m.Groups[2].Value.TrimEnd();
            if (path.StartsWith("\"") || path.StartsWith("'")) return m.Value;
            var escaped = path.Replace("'", "''");
            return $"{prefix}'{escaped}'";
        });

        // 10. Quote unquoted scalar values that contain ': ' (colon-space), which YAML
        //     misinterprets as nested mappings. Targets freeform text fields like initialPrompt.
        repaired = Regex.Replace(repaired, @"(?m)^(\s*\w+:\s+)(.+)$", m =>
        {
            var prefix = m.Groups[1].Value;
            var value = m.Groups[2].Value.TrimEnd();
            // Skip if already quoted, is a block scalar indicator, or is a list/mapping
            if (value.StartsWith("\"") || value.StartsWith("'") ||
                value.StartsWith("|") || value.StartsWith(">") ||
                value.StartsWith("-") || value.StartsWith("{") || value.StartsWith("["))
                return m.Value;
            // Only fix if the value contains an embedded ': ' that would confuse the parser
            // (but not the first colon which is the key separator — we're already past it)
            if (value.Contains(": "))
            {
                var escaped = value.Replace("'", "''");
                return $"{prefix}'{escaped}'";
            }

            return m.Value;
        });

        repaired = Regex.Replace(
            repaired,
            @"(?m)^(\s*)(repos|commits|prs|verifications|relatedPlans|dependsOn):\s*\r?\n(?!\s*-)",
            "$1$2: []\n");

        return NormalizePlanYamlStructure(repaired);
    }

    private static string NormalizePlanYamlStructure(string yaml)
    {
        var topLevelKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "state", "project", "level", "title", "sessionId",
            "repos", "created", "updated", "initialPrompt",
            "prs", "commits", "verifications", "relatedPlans", "dependsOn"
        };
        var listKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "repos", "prs", "commits", "verifications", "relatedPlans", "dependsOn"
        };

        var normalized = yaml.Replace("\r\n", "\n");
        var lines = normalized.Split('\n');
        var output = new List<string>(lines.Length);
        string? currentListKey = null;
        var inVerificationItem = false;
        var inBlockScalar = false;
        string? currentBlockScalarKey = null;

        static bool IsBlockScalarValue(string value)
        {
            return value is "|" or "|-" or ">" or ">-";
        }

        string? TryExtractTopLevelKey(string trimmedLine, out string normalizedLine)
        {
            normalizedLine = trimmedLine;

            var keyMatch = Regex.Match(trimmedLine, @"^([A-Za-z][A-Za-z0-9]*):");
            if (keyMatch.Success && topLevelKeys.Contains(keyMatch.Groups[1].Value))
                return keyMatch.Groups[1].Value;

            var quotedMatch = Regex.Match(trimmedLine, @"^'([A-Za-z][A-Za-z0-9]*):\s*(.*)'$");
            if (!quotedMatch.Success || !topLevelKeys.Contains(quotedMatch.Groups[1].Value))
                return null;

            var key = quotedMatch.Groups[1].Value;
            var value = quotedMatch.Groups[2].Value.Replace("''", "'");
            normalizedLine = $"{key}: {value}".TrimEnd();
            return key;
        }

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd('\r');
            var trimmed = line.TrimStart();

            if (trimmed.Length == 0)
            {
                output.Add(inBlockScalar ? "  " : string.Empty);
                continue;
            }

            var detectedKey = TryExtractTopLevelKey(trimmed, out var normalizedTopLevelLine);
            if (inBlockScalar && detectedKey == null)
            {
                output.Add($"  {line}");
                continue;
            }

            if (detectedKey != null)
            {
                var key = detectedKey;
                currentListKey = listKeys.Contains(key) ? key : null;
                inVerificationItem = false;
                inBlockScalar = false;
                currentBlockScalarKey = null;

                if (currentListKey != null &&
                    Regex.IsMatch(normalizedTopLevelLine, @"^[A-Za-z][A-Za-z0-9]*:\s*\[\]\s*$"))
                    output.Add($"{key}:");
                else
                    output.Add(normalizedTopLevelLine);

                var scalarValue = normalizedTopLevelLine[(key.Length + 1)..].Trim();
                if (IsBlockScalarValue(scalarValue))
                {
                    inBlockScalar = true;
                    currentBlockScalarKey = key;
                }

                continue;
            }

            if (inBlockScalar)
            {
                output.Add($"  {line}");
                continue;
            }

            if (currentListKey != null)
            {
                if (trimmed.StartsWith("-"))
                {
                    output.Add($"  {trimmed}");
                    inVerificationItem = currentListKey == "verifications";
                }
                else if (currentListKey == "verifications" && inVerificationItem)
                {
                    output.Add($"    {trimmed}");
                }
                else
                {
                    output.Add($"  {trimmed}");
                }

                continue;
            }

            output.Add(trimmed);
        }

        return string.Join(Environment.NewLine, output);
    }

    /// <summary>
    ///     Enumerates all directories in PlansDirectory that match the plan folder
    ///     naming pattern ({5digits}-{name}) and contain a plan.yaml file.
    /// </summary>
    private IEnumerable<(string FolderPath, string FolderName, string PlanYamlPath)> EnumerateValidPlanFolders()
    {
        if (!Directory.Exists(PlansDirectory))
            yield break;

        foreach (var dir in Directory.GetDirectories(PlansDirectory))
        {
            var folderName = Path.GetFileName(dir);
            if (!FolderNameRegex.Match(folderName).Success)
                continue;

            var planYamlPath = Path.Combine(dir, "plan.yaml");
            if (!File.Exists(planYamlPath))
                continue;

            // Ensure at least one revision file exists before yielding the plan
            var revisionsDir = Path.Combine(dir, "revisions");
            if (!Directory.Exists(revisionsDir))
                continue;

            var hasRevision = Directory.GetFiles(revisionsDir, "*.md").Length > 0;
            if (!hasRevision)
                continue;

            yield return (dir, folderName, planYamlPath);
        }
    }

    /// <summary>
    ///     Always reads plans from the file system. Used by the sync service to populate the database.
    /// </summary>
    internal List<PlanFile> GetPlansFromFileSystem(PlanStatus? statusFilter = null)
    {
        try
        {
            var plans = new List<PlanFile>();

            foreach (var (folderPath, _, _) in EnumerateValidPlanFolders())
            {
                var plan = ParsePlanFolder(folderPath);
                if (plan == null) continue;

                if (statusFilter.HasValue && plan.Status != statusFilter.Value)
                    continue;

                plans.Add(plan);
            }

            return plans.OrderBy(p => p.Id).ToList();
        }
        catch
        {
            return new List<PlanFile>();
        }
    }

    /// <summary>
    ///     Always reads from the file system. Used by ParsePlanFolder during sync
    ///     to avoid circular reads from the database.
    /// </summary>
    private string ReadLatestRevisionFromFileSystem(string folderName)
    {
        var revisionsDir = Path.Combine(PlansDirectory, folderName, "revisions");
        if (!Directory.Exists(revisionsDir)) return string.Empty;

        var latestFile = Directory.GetFiles(revisionsDir, "*.md")
            .OrderByDescending(f => f)
            .FirstOrDefault();

        return latestFile != null ? FileHelper.ReadAllText(latestFile) : string.Empty;
    }

    private static void RemoveWorktrees(string planFolderPath)
    {
        var worktreesDir = Path.Combine(planFolderPath, "worktrees");
        if (!Directory.Exists(worktreesDir)) return;

        foreach (var wtDir in Directory.GetDirectories(worktreesDir))
        {
            var gitFile = Path.Combine(wtDir, ".git");
            if (!File.Exists(gitFile)) continue;

            // Read the .git file to find which repo this worktree belongs to.
            // Format: "gitdir: <path-to-repo>/.git/worktrees/<name>"
            var gitContent = FileHelper.ReadAllText(gitFile).Trim();
            var match = Regex.Match(gitContent, @"gitdir:\s*(.+)");
            if (!match.Success) continue;

            var gitDir = match.Groups[1].Value.Trim();
            // Navigate from .git/worktrees/<name> up to the repo root
            var repoGitDir = Path.GetFullPath(Path.Combine(gitDir, "..", ".."));
            var repoRoot = Path.GetDirectoryName(repoGitDir);
            if (repoRoot == null || !Directory.Exists(repoRoot)) continue;

            try
            {
                var psi = new ProcessStartInfo("git", $"worktree remove --force \"{wtDir}\"")
                {
                    WorkingDirectory = repoRoot,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var process = Process.Start(psi);
                process?.WaitForExit(10000);
            }
            catch
            {
                // Best-effort: if git worktree remove fails, the fallback
                // ClearReadOnlyAttributes + Directory.Delete may still work.
            }
        }
    }

    private static void ClearReadOnlyAttributes(string directoryPath)
    {
        try
        {
            foreach (var file in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                var attrs = File.GetAttributes(file);
                if ((attrs & FileAttributes.ReadOnly) != 0)
                    File.SetAttributes(file, attrs & ~FileAttributes.ReadOnly);
            }
        }
        catch
        {
            // Best-effort
        }
    }

    /// <summary>
    ///     Parses a single plan folder from the file system. Used by the sync service for incremental updates.
    /// </summary>
    internal PlanFile? ParseSinglePlanFolder(string folderPath)
    {
        return ParsePlanFolder(folderPath);
    }

    private PlanFile? ParsePlanFolder(string folderPath)
    {
        try
        {
            var planYamlPath = Path.Combine(folderPath, "plan.yaml");
            var yamlContent = FileHelper.ReadAllText(planYamlPath);
            PlanYaml? planYaml;

            try
            {
                planYaml = YamlHelper.Deserializer.Deserialize<PlanYaml>(yamlContent);
            }
            catch
            {
                // Fall back to the repair pass for malformed agent-generated YAML.
                var repaired = RepairPlanYaml(yamlContent);
                if (repaired != yamlContent)
                    FileHelper.WriteAllText(planYamlPath, repaired);

                planYaml = YamlHelper.Deserializer.Deserialize<PlanYaml>(repaired);
            }

            if (planYaml == null) return null;

            var folderName = Path.GetFileName(folderPath);
            var match = FolderNameRegex.Match(folderName);
            if (!match.Success) return null;

            var id = int.Parse(match.Groups[1].Value);

            if (!Enum.TryParse<PlanStatus>(planYaml.State, true, out var status))
                status = PlanStatus.Draft;

            var metadata = new PlanMetadata(
                id,
                string.IsNullOrWhiteSpace(planYaml.Project) ? "" : planYaml.Project,
                string.IsNullOrWhiteSpace(planYaml.Level) ? "NiceToHave" : planYaml.Level,
                string.IsNullOrWhiteSpace(planYaml.Title) ? "" : planYaml.Title,
                status,
                planYaml.Repos ?? [],
                planYaml.Commits ?? [],
                planYaml.Prs ?? [],
                planYaml.Verifications ?? [],
                planYaml.RelatedPlans ?? [],
                planYaml.DependsOn ?? [],
                planYaml.Created,
                planYaml.Updated,
                planYaml.InitialPrompt
            );
            var latestContent = ReadLatestRevisionFromFileSystem(folderName);

            var revisionsDir = Path.Combine(folderPath, "revisions");
            var revisionCount = Directory.Exists(revisionsDir)
                ? Directory.GetFiles(revisionsDir, "*.md").Length
                : 1;
            if (revisionCount == 0) revisionCount = 1;

            return new PlanFile(metadata, latestContent, folderPath, yamlContent, revisionCount);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse plan folder: {FolderPath}", folderPath);
            return null;
        }
    }

    private static int? ExtractPlanId(string folderPath)
    {
        var folderName = Path.GetFileName(folderPath);
        var match = FolderNameRegex.Match(folderName);
        return match.Success && int.TryParse(match.Groups[1].Value, out var id) ? id : null;
    }

    /// <summary>
    ///     Parses costs.csv once to compute both total cost and total tokens.
    ///     CSV format: Promptware,Tokens,Cost (fields must not contain commas).
    /// </summary>
    private static (decimal Cost, int Tokens) ComputePlanCostAndTokens(string folderPath)
    {
        var costsPath = Path.Combine(folderPath, "costs.csv");
        if (!File.Exists(costsPath)) return (0m, 0);

        var lines = FileHelper.ReadAllLines(costsPath);
        var totalCost = 0m;
        var totalTokens = 0;
        foreach (var line in lines.Skip(1)) // skip header
        {
            var parts = line.Split(',');
            if (parts.Length >= 3)
            {
                if (int.TryParse(parts[1],
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture, out var tokens))
                    totalTokens += tokens;
                if (decimal.TryParse(parts[2],
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture, out var cost))
                    totalCost += cost;
            }
        }

        return (totalCost, totalTokens);
    }

    private List<HourlyTokenBurn> ComputeHourlyTokenBurn(int days)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        var buckets = new Dictionary<(DateTime Hour, string Project), (decimal Cost, int Tokens)>();

        if (!Directory.Exists(PlansDirectory)) return new List<HourlyTokenBurn>();

        foreach (var dir in Directory.GetDirectories(PlansDirectory))
            try
            {
                var costsPath = Path.Combine(dir, "costs.csv");
                var logsDir = Path.Combine(dir, "logs");
                if (!File.Exists(costsPath) || !Directory.Exists(logsDir)) continue;

                var planYamlPath = Path.Combine(dir, "plan.yaml");
                if (!File.Exists(planYamlPath)) continue;

                var planYaml = YamlHelper.Deserializer.Deserialize<PlanYaml>(FileHelper.ReadAllText(planYamlPath));
                if (planYaml == null) continue;

                var project = planYaml.Project ?? "";

                var costLines = FileHelper.ReadAllLines(costsPath).Skip(1).ToList();
                if (costLines.Count == 0) continue;

                // Build a map: promptware name -> list of log files (ordered by number)
                var logFiles = Directory.GetFiles(logsDir, "*.md")
                    .Select(f =>
                    {
                        var name = Path.GetFileNameWithoutExtension(f);
                        var dashIdx = name.IndexOf('-');
                        if (dashIdx < 0) return (Promptware: name, Path: f, Num: 0);
                        var numPart = name.Substring(0, dashIdx);
                        var pwName = name.Substring(dashIdx + 1);
                        int.TryParse(numPart, out var num);
                        return (Promptware: pwName, Path: f, Num: num);
                    })
                    .OrderBy(l => l.Num)
                    .ToList();

                // Group log files by promptware name, preserving order
                var logsByPromptware = new Dictionary<string, Queue<string>>(StringComparer.OrdinalIgnoreCase);
                foreach (var log in logFiles)
                {
                    if (!logsByPromptware.ContainsKey(log.Promptware))
                        logsByPromptware[log.Promptware] = new Queue<string>();
                    logsByPromptware[log.Promptware].Enqueue(log.Path);
                }

                // Correlate each cost row with its log file
                foreach (var line in costLines)
                {
                    var parts = line.Split(',');
                    if (parts.Length < 3) continue;

                    var promptware = parts[0].Trim();
                    if (!int.TryParse(parts[1].Trim(), NumberStyles.Any,
                            CultureInfo.InvariantCulture, out var tokens)) continue;
                    if (!decimal.TryParse(parts[2].Trim(), NumberStyles.Any,
                            CultureInfo.InvariantCulture, out var cost)) continue;

                    if (!logsByPromptware.TryGetValue(promptware, out var queue) || queue.Count == 0)
                        continue;

                    var logPath = queue.Dequeue();
                    var timestamp = ExtractCompletedTimestamp(logPath);
                    if (timestamp == null || timestamp.Value < cutoff) continue;

                    var hour = new DateTime(timestamp.Value.Year, timestamp.Value.Month,
                        timestamp.Value.Day, timestamp.Value.Hour, 0, 0, DateTimeKind.Utc);

                    var key = (hour, project);
                    if (buckets.TryGetValue(key, out var existing))
                        buckets[key] = (existing.Cost + cost, existing.Tokens + tokens);
                    else
                        buckets[key] = (cost, tokens);
                }
            }
            catch
            {
                // Skip problematic plan folders
            }

        return buckets
            .OrderBy(b => b.Key.Hour)
            .ThenBy(b => b.Key.Project)
            .Select(b => new HourlyTokenBurn
            {
                Hour = b.Key.Hour,
                Project = b.Key.Project,
                Cost = b.Value.Cost,
                Tokens = b.Value.Tokens
            })
            .ToList();
    }

    private List<Recommendation> ComputeRecommendations()
    {
        var recommendations = new List<Recommendation>();

        foreach (var (folderPath, folderName, planYamlPath) in EnumerateValidPlanFolders())
        {
            var recommendationsPath = Path.Combine(folderPath, "artifacts", "recommendations.yaml");
            if (!File.Exists(recommendationsPath)) continue;

            try
            {
                var planYaml = FileHelper.ReadAllText(planYamlPath);
                var plan = YamlHelper.Deserializer.Deserialize<PlanYaml>(planYaml);
                if (plan == null) continue;

                var yaml = FileHelper.ReadAllText(recommendationsPath);
                var items = YamlHelper.Deserializer.Deserialize<List<RecommendationYaml>>(yaml);
                if (items == null) continue;

                var match = FolderNameRegex.Match(folderName);
                var planId = match.Groups[1].Value;

                if (!Enum.TryParse<PlanStatus>(plan.State, true, out var status))
                    status = PlanStatus.Draft;

                foreach (var item in items)
                    recommendations.Add(new Recommendation(
                        item.Title,
                        item.Description,
                        string.IsNullOrWhiteSpace(item.State) ? "Pending" : item.State,
                        planId,
                        plan.Title ?? "",
                        folderName,
                        plan.Project ?? "",
                        plan.Updated,
                        status,
                        item.DeclineReason
                    ));
            }
            catch
            {
                // Skip malformed YAML files
            }
        }

        return recommendations.OrderByDescending(r => r.Date).ToList();
    }

    private PlanCountSnapshot ComputePlanCountsInternal()
    {
        int drafts = 0, reviews = 0, failed = 0, icebox = 0, pendingRecs = 0;

        foreach (var (folderPath, _, planYamlPath) in EnumerateValidPlanFolders())
            try
            {
                var yaml = FileHelper.ReadAllText(planYamlPath);
                var stateMatch = Regex.Match(yaml, @"(?m)^state:\s*(.+)$");
                if (stateMatch.Success)
                {
                    var state = stateMatch.Groups[1].Value.Trim();
                    switch (state.ToLowerInvariant())
                    {
                        case "draft": drafts++; break;
                        case "blocked": drafts++; break;
                        case "readyforreview": reviews++; break;
                        case "failed": failed++; break;
                        case "icebox": icebox++; break;
                    }
                }

                var recommendationsPath = Path.Combine(folderPath, "artifacts", "recommendations.yaml");
                if (File.Exists(recommendationsPath))
                {
                    var recYaml = FileHelper.ReadAllText(recommendationsPath);
                    var items = YamlHelper.Deserializer.Deserialize<List<RecommendationYaml>>(recYaml);
                    if (items != null)
                        foreach (var item in items)
                        {
                            var state = string.IsNullOrWhiteSpace(item.State) ? "Pending" : item.State;
                            if (state.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                                pendingRecs++;
                        }
                }
            }
            catch
            {
                // Skip malformed plans
            }

        return new PlanCountSnapshot(drafts, reviews, failed, icebox, pendingRecs);
    }

    /// <summary>
    ///     Runs a file write operation in the background (fire-and-forget).
    ///     The database is the primary data source; file writes are for durability only.
    /// </summary>
    private void WriteFileInBackground(Action action)
    {
        Task.Run(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Background file write failed");
            }
        });
    }

    private void WriteFile(Action action)
    {
        if (_database == null)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "File write failed");
            }

            return;
        }

        WriteFileInBackground(action);
    }

    private static DateTime? ExtractCompletedTimestamp(string logFilePath)
    {
        return FileHelper.ExtractCompletedTimestamp(logFilePath);
    }

    private static int GetNextRevisionNumber(string revisionsDir)
    {
        var existing = Directory.GetFiles(revisionsDir, "*.md");
        if (existing.Length == 0) return 1;

        return existing
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .Select(n => int.TryParse(n, out var num) ? num : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;
    }

    public record PlanCountSnapshot(
        int Drafts,
        int ReadyForReview,
        int Failed,
        int Icebox,
        int PendingRecommendations
    );
}

public class HourlyTokenBurn
{
    public DateTime Hour { get; set; }
    public decimal Cost { get; set; }
    public int Tokens { get; set; }
    public string Project { get; set; } = "";
}

public record Recommendation(
    string Title,
    string Description,
    string State,
    string PlanId,
    string PlanTitle,
    string PlanFolderName,
    string Project,
    DateTime Date,
    PlanStatus SourcePlanStatus,
    string? DeclineReason = null
);