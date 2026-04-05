using System.Text.RegularExpressions;

using Ivy.Tendril.Apps.Plans;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ivy.Tendril.Services;

public class PlanReaderService(ConfigService config)
{
    private readonly ConfigService _config = config;

    private static readonly Regex FolderNameRegex = new(@"^(\d{5})-(.+)$", RegexOptions.Compiled);

    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private static readonly ISerializer YamlSerializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public string PlansDirectory => _config.PlanFolder;

    /// <summary>
    /// On startup, reset any plans stuck in transient states (Building, Executing, Updating)
    /// back to Failed. These are leftovers from a previous Tendril shutdown.
    /// </summary>
    public void RecoverStuckPlans()
    {
        var stuckStates = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "Building", "Executing", "Updating" };

        if (!Directory.Exists(PlansDirectory)) return;

        foreach (var dir in Directory.GetDirectories(PlansDirectory))
        {
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
                        ? "Failed" : "Draft";
                    var updated = Regex.Replace(yaml, @"(?m)^state:\s*.*$", $"state: {newState}");
                    updated = Regex.Replace(updated, @"(?m)^updated:\s*.*$",
                        $"updated: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
                    FileHelper.WriteAllText(planYamlPath, updated);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Failed to recover plan {Path.GetFileName(dir)}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// On startup, fix plan.yaml files that have structured repos (path: + prRule:)
    /// by normalizing them to plain path strings.
    /// </summary>
    public void RepairPlans()
    {
        try
        {
            if (!Directory.Exists(PlansDirectory)) return;

            foreach (var dir in Directory.GetDirectories(PlansDirectory))
            {
                var planYamlPath = Path.Combine(dir, "plan.yaml");
                if (!File.Exists(planYamlPath)) continue;

                var yaml = FileHelper.ReadAllText(planYamlPath);

                // Fix structured repos (path: + optional prRule:) → plain path strings
                var repaired = Regex.Replace(yaml,
                    @"(?m)^(\s*)-\s+path:\s*(.+?)(?:\r?\n\s+prRule:\s*.+)?$",
                    "$1- $2");

                if (repaired != yaml)
                    FileHelper.WriteAllText(planYamlPath, repaired);
            }
        }
        catch { }
    }

    /// <summary>
    /// Enumerates all directories in PlansDirectory that match the plan folder
    /// naming pattern ({5digits}-{name}) and contain a plan.yaml file.
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

            yield return (dir, folderName, planYamlPath);
        }
    }

    /// <summary>
    /// Retrieves all plans from the plans directory, optionally filtered by status.
    /// </summary>
    /// <param name="statusFilter">Optional status to filter by. If <c>null</c>, returns all plans.</param>
    /// <returns>List of plans ordered by ID, or an empty list if the plans directory does not exist or parsing fails.</returns>
    public List<PlanFile> GetPlans(PlanStatus? statusFilter = null)
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
    /// Retrieves a single plan by its folder path.
    /// </summary>
    /// <param name="folderPath">Absolute path to the plan folder.</param>
    /// <returns>The parsed <see cref="PlanFile"/>, or <c>null</c> if the folder does not exist or parsing fails.</returns>
    public PlanFile? GetPlanByFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return null;
        return ParsePlanFolder(folderPath);
    }

    /// <summary>
    /// Retrieves all plans that are in the <see cref="PlanStatus.Icebox"/> state.
    /// </summary>
    /// <returns>List of icebox plans ordered by ID.</returns>
    public List<PlanFile> GetIceboxPlans()
    {
        return GetPlans(PlanStatus.Icebox);
    }

    /// <summary>
    /// Transitions a plan to a new state by updating its <c>plan.yaml</c> file.
    /// </summary>
    /// <param name="folderName">Name of the plan folder (e.g. <c>01105-TestPlan</c>).</param>
    /// <param name="newState">The target state to transition to.</param>
    public void TransitionState(string folderName, PlanStatus newState)
    {
        var folderPath = Path.Combine(PlansDirectory, folderName);
        var planYamlPath = Path.Combine(folderPath, "plan.yaml");

        if (!File.Exists(planYamlPath)) return;

        var yaml = FileHelper.ReadAllText(planYamlPath);
        var planYaml = YamlDeserializer.Deserialize<PlanYaml>(yaml) ?? new PlanYaml();

        planYaml.State = newState.ToString();
        planYaml.Updated = DateTime.UtcNow;

        FileHelper.WriteAllText(planYamlPath, YamlSerializer.Serialize(planYaml));
    }

    /// <summary>
    /// Creates a new revision file for a plan and updates the plan's timestamp.
    /// </summary>
    /// <param name="folderName">Name of the plan folder.</param>
    /// <param name="content">Markdown content of the new revision.</param>
    public void SaveRevision(string folderName, string content)
    {
        var revisionsDir = Path.Combine(PlansDirectory, folderName, "revisions");
        Directory.CreateDirectory(revisionsDir);

        var nextNumber = GetNextRevisionNumber(revisionsDir);
        var revisionPath = Path.Combine(revisionsDir, $"{nextNumber:D3}.md");
        File.WriteAllText(revisionPath, content);

        // Update the updated timestamp in plan.yaml
        var planYamlPath = Path.Combine(PlansDirectory, folderName, "plan.yaml");
        if (File.Exists(planYamlPath))
        {
            var yaml = FileHelper.ReadAllText(planYamlPath);
            var planYaml = YamlDeserializer.Deserialize<PlanYaml>(yaml) ?? new PlanYaml();
            planYaml.Updated = DateTime.UtcNow;
            FileHelper.WriteAllText(planYamlPath, YamlSerializer.Serialize(planYaml));
        }
    }

    /// <summary>
    /// Reads the content of the most recent revision file for a plan.
    /// </summary>
    /// <param name="folderName">Name of the plan folder.</param>
    /// <returns>The markdown content of the latest revision, or <see cref="string.Empty"/> if no revisions exist.</returns>
    public string ReadLatestRevision(string folderName)
    {
        var revisionsDir = Path.Combine(PlansDirectory, folderName, "revisions");
        if (!Directory.Exists(revisionsDir)) return string.Empty;

        var latestFile = Directory.GetFiles(revisionsDir, "*.md")
            .OrderByDescending(f => f)
            .FirstOrDefault();

        return latestFile != null ? File.ReadAllText(latestFile) : string.Empty;
    }

    /// <summary>
    /// Gets all revisions for a plan, ordered by revision number.
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
                    return (Number: num, Content: File.ReadAllText(f), Modified: File.GetLastWriteTimeUtc(f));
                return (Number: -1, Content: "", Modified: DateTime.MinValue);
            })
            .Where(r => r.Number >= 0)
            .OrderBy(r => r.Number)
            .ToList();
    }

    /// <summary>
    /// Appends a log entry to a plan's logs directory.
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
        {
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
        }

        var logPath = Path.Combine(logsDir, $"{nextNumber:D3}-{action}.md");
        File.WriteAllText(logPath, content);
    }

    /// <summary>
    /// Deletes a plan folder and all its contents, including any associated git worktrees.
    /// </summary>
    /// <param name="folderName">Name of the plan folder.</param>
    /// <remarks>
    /// Removes git worktrees first to avoid locked file issues on Windows, then clears
    /// read-only attributes before deleting the directory tree.
    /// </remarks>
    public void DeletePlan(string folderName)
    {
        var folderPath = Path.Combine(PlansDirectory, folderName);
        if (!Directory.Exists(folderPath)) return;

        // Remove git worktrees before deleting, otherwise Directory.Delete
        // fails on Windows with UnauthorizedAccessException due to locked files.
        RemoveWorktrees(folderPath);

        // Clear read-only attributes that git may have set, then delete.
        ClearReadOnlyAttributes(folderPath);
        Directory.Delete(folderPath, recursive: true);
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
            var gitContent = File.ReadAllText(gitFile).Trim();
            var match = Regex.Match(gitContent, @"gitdir:\s*(.+)");
            if (!match.Success) continue;

            var gitDir = match.Groups[1].Value.Trim();
            // Navigate from .git/worktrees/<name> up to the repo root
            var repoGitDir = Path.GetFullPath(Path.Combine(gitDir, "..", ".."));
            var repoRoot = Path.GetDirectoryName(repoGitDir);
            if (repoRoot == null || !Directory.Exists(repoRoot)) continue;

            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo("git", $"worktree remove --force \"{wtDir}\"")
                {
                    WorkingDirectory = repoRoot,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                using var process = System.Diagnostics.Process.Start(psi);
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
    /// Reads the raw content of a plan's latest revision.
    /// </summary>
    /// <param name="folderName">Name of the plan folder.</param>
    /// <returns>The markdown content of the latest revision.</returns>
    public string ReadRawPlan(string folderName)
    {
        return ReadLatestRevision(folderName);
    }

    /// <summary>
    /// Saves content to a plan by overwriting its latest revision.
    /// </summary>
    /// <param name="folderName">Name of the plan folder.</param>
    /// <param name="fullContent">The full markdown content to write.</param>
    public void SavePlan(string folderName, string fullContent)
    {
        UpdateLatestRevision(folderName, fullContent);
    }

    /// <summary>
    /// Overwrites the most recent revision file for a plan and updates the plan's timestamp.
    /// </summary>
    /// <param name="folderName">Name of the plan folder.</param>
    /// <param name="content">The updated markdown content to write.</param>
    public void UpdateLatestRevision(string folderName, string content)
    {
        var revisionsDir = Path.Combine(PlansDirectory, folderName, "revisions");
        if (!Directory.Exists(revisionsDir)) return;

        var latestFile = Directory.GetFiles(revisionsDir, "*.md")
            .OrderByDescending(f => f)
            .FirstOrDefault();

        if (latestFile != null)
        {
            File.WriteAllText(latestFile, content);

            var planYamlPath = Path.Combine(PlansDirectory, folderName, "plan.yaml");
            if (File.Exists(planYamlPath))
            {
                var yaml = FileHelper.ReadAllText(planYamlPath);
                var planYaml = YamlDeserializer.Deserialize<PlanYaml>(yaml) ?? new PlanYaml();
                planYaml.Updated = DateTime.UtcNow;
                FileHelper.WriteAllText(planYamlPath, YamlSerializer.Serialize(planYaml));
            }
        }
    }

    private PlanFile? ParsePlanFolder(string folderPath)
    {
        try
        {
            var planYamlPath = Path.Combine(folderPath, "plan.yaml");
            var yamlContent = FileHelper.ReadAllText(planYamlPath);
            var planYaml = YamlDeserializer.Deserialize<PlanYaml>(yamlContent);
            if (planYaml == null) return null;

            var folderName = Path.GetFileName(folderPath);
            var match = FolderNameRegex.Match(folderName);
            if (!match.Success) return null;

            var id = int.Parse(match.Groups[1].Value);

            if (!Enum.TryParse<PlanStatus>(planYaml.State, ignoreCase: true, out var status))
                status = PlanStatus.Draft;

            var metadata = new PlanMetadata(id, planYaml.Project ?? "", planYaml.Level ?? "NiceToHave", planYaml.Title ?? "", status, planYaml.Repos ?? new(), planYaml.Commits ?? new(), planYaml.Prs ?? new(), planYaml.Verifications ?? new(), planYaml.RelatedPlans ?? new(), planYaml.DependsOn ?? new(), planYaml.Created, planYaml.Updated);
            var latestContent = ReadLatestRevision(folderName);

            var revisionsDir = Path.Combine(folderPath, "revisions");
            var revisionCount = Directory.Exists(revisionsDir)
                ? Directory.GetFiles(revisionsDir, "*.md").Length
                : 1;
            if (revisionCount == 0) revisionCount = 1;

            return new PlanFile(metadata, latestContent, folderPath, yamlContent, revisionCount);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Calculates the total cost for a plan by summing all entries in its <c>costs.csv</c> file.
    /// </summary>
    /// <param name="folderPath">Absolute path to the plan folder.</param>
    /// <returns>Total cost in dollars, or <c>0</c> if no costs file exists.</returns>
    public decimal GetPlanTotalCost(string folderPath)
    {
        var costsPath = Path.Combine(folderPath, "costs.csv");
        if (!File.Exists(costsPath)) return 0m;

        var lines = FileHelper.ReadAllLines(costsPath);
        decimal total = 0m;
        foreach (var line in lines.Skip(1)) // skip header
        {
            var parts = line.Split(',');
            if (parts.Length >= 3 && decimal.TryParse(parts[2],
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var cost))
            {
                total += cost;
            }
        }
        return total;
    }

    /// <summary>
    /// Calculates the total token usage for a plan by summing all entries in its <c>costs.csv</c> file.
    /// </summary>
    /// <param name="folderPath">Absolute path to the plan folder.</param>
    /// <returns>Total token count, or <c>0</c> if no costs file exists.</returns>
    public int GetPlanTotalTokens(string folderPath)
    {
        var costsPath = Path.Combine(folderPath, "costs.csv");
        if (!File.Exists(costsPath)) return 0;

        var lines = FileHelper.ReadAllLines(costsPath);
        int total = 0;
        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',');
            if (parts.Length >= 2 && int.TryParse(parts[1],
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var tokens))
            {
                total += tokens;
            }
        }
        return total;
    }

    /// <summary>
    /// Computes hourly token usage and cost statistics across all plans over a given time window.
    /// </summary>
    /// <param name="days">Number of days to look back from now. Defaults to 7.</param>
    /// <returns>List of hourly buckets with aggregated cost and token counts, ordered chronologically.</returns>
    /// <remarks>
    /// Correlates <c>costs.csv</c> entries with log file timestamps to determine when tokens were consumed.
    /// Plans without both a costs file and a logs directory are skipped.
    /// </remarks>
    public List<HourlyTokenBurn> GetHourlyTokenBurn(int days = 7)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        var buckets = new Dictionary<DateTime, (decimal Cost, int Tokens)>();

        if (!Directory.Exists(PlansDirectory)) return new List<HourlyTokenBurn>();

        foreach (var dir in Directory.GetDirectories(PlansDirectory))
        {
            try
            {
                var costsPath = Path.Combine(dir, "costs.csv");
                var logsDir = Path.Combine(dir, "logs");
                if (!File.Exists(costsPath) || !Directory.Exists(logsDir)) continue;

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
                    if (!int.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var tokens)) continue;
                    if (!decimal.TryParse(parts[2].Trim(), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var cost)) continue;

                    if (!logsByPromptware.TryGetValue(promptware, out var queue) || queue.Count == 0)
                        continue;

                    var logPath = queue.Dequeue();
                    var timestamp = ExtractCompletedTimestamp(logPath);
                    if (timestamp == null || timestamp.Value < cutoff) continue;

                    var hour = new DateTime(timestamp.Value.Year, timestamp.Value.Month,
                        timestamp.Value.Day, timestamp.Value.Hour, 0, 0, DateTimeKind.Utc);

                    if (buckets.TryGetValue(hour, out var existing))
                        buckets[hour] = (existing.Cost + cost, existing.Tokens + tokens);
                    else
                        buckets[hour] = (cost, tokens);
                }
            }
            catch
            {
                // Skip problematic plan folders
            }
        }

        return buckets
            .OrderBy(b => b.Key)
            .Select(b => new HourlyTokenBurn
            {
                Hour = b.Key,
                Cost = b.Value.Cost,
                Tokens = b.Value.Tokens
            })
            .ToList();
    }

    /// <summary>
    /// Gets all recommendations from all plans. This method reads and deserializes all
    /// recommendations.yaml files in the plans directory, which can be expensive.
    /// </summary>
    /// <remarks>
    /// If you only need the count of pending recommendations, use <see cref="GetPendingRecommendationsCount"/>
    /// instead, which uses an optimized path via <see cref="ComputePlanCounts"/> that counts
    /// without full deserialization.
    /// </remarks>
    /// <returns>List of all recommendations ordered by date (most recent first).</returns>
    public List<Recommendation> GetRecommendations()
    {
        var recommendations = new List<Recommendation>();

        foreach (var (folderPath, folderName, planYamlPath) in EnumerateValidPlanFolders())
        {
            var recommendationsPath = Path.Combine(folderPath, "artifacts", "recommendations.yaml");
            if (!File.Exists(recommendationsPath)) continue;

            try
            {
                var planYaml = FileHelper.ReadAllText(planYamlPath);
                var plan = YamlDeserializer.Deserialize<PlanYaml>(planYaml);
                if (plan == null) continue;

                var yaml = FileHelper.ReadAllText(recommendationsPath);
                var items = YamlDeserializer.Deserialize<List<RecommendationYaml>>(yaml);
                if (items == null) continue;

                var match = FolderNameRegex.Match(folderName);
                var planId = match.Groups[1].Value;

                if (!Enum.TryParse<PlanStatus>(plan.State, ignoreCase: true, out var status))
                    status = PlanStatus.Draft;

                foreach (var item in items)
                {
                    recommendations.Add(new Recommendation(
                        Title: item.Title,
                        Description: item.Description,
                        State: string.IsNullOrWhiteSpace(item.State) ? "Pending" : item.State,
                        PlanId: planId,
                        PlanTitle: plan.Title ?? "",
                        PlanFolderName: folderName,
                        Project: plan.Project ?? "",
                        Date: plan.Updated,
                        SourcePlanStatus: status,
                        DeclineReason: item.DeclineReason
                    ));
                }
            }
            catch
            {
                // Skip malformed YAML files
            }
        }

        return recommendations.OrderByDescending(r => r.Date).ToList();
    }

    /// <summary>
    /// Gets the count of pending recommendations efficiently without deserializing full recommendation objects.
    /// </summary>
    /// <remarks>
    /// This method delegates to <see cref="ComputePlanCounts"/> which only counts pending items
    /// without building full Recommendation objects, making it much more efficient than calling
    /// <c>GetRecommendations().Count(r => r.State == "Pending")</c>.
    /// </remarks>
    /// <returns>Number of pending recommendations.</returns>
    public int GetPendingRecommendationsCount()
    {
        return ComputePlanCounts().PendingRecommendations;
    }

    public record PlanCountSnapshot(
        int Drafts,
        int ReadyForReview,
        int Failed,
        int Icebox,
        int PendingRecommendations
    );

    /// <summary>
    /// Efficiently computes plan counts by status and pending recommendation count
    /// using regex-based state extraction instead of full YAML deserialization.
    /// </summary>
    /// <returns>A <see cref="PlanCountSnapshot"/> with counts for each status category and pending recommendations.</returns>
    public PlanCountSnapshot ComputePlanCounts()
    {
        int drafts = 0, reviews = 0, failed = 0, icebox = 0, pendingRecs = 0;

        foreach (var (folderPath, _, planYamlPath) in EnumerateValidPlanFolders())
        {
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
                        case "readyforreview": reviews++; break;
                        case "failed": failed++; break;
                        case "icebox": icebox++; break;
                    }
                }

                var recommendationsPath = Path.Combine(folderPath, "artifacts", "recommendations.yaml");
                if (File.Exists(recommendationsPath))
                {
                    var recYaml = FileHelper.ReadAllText(recommendationsPath);
                    var items = YamlDeserializer.Deserialize<List<RecommendationYaml>>(recYaml);
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            var state = string.IsNullOrWhiteSpace(item.State) ? "Pending" : item.State;
                            if (state.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                                pendingRecs++;
                        }
                    }
                }
            }
            catch
            {
                // Skip malformed plans
            }
        }

        return new PlanCountSnapshot(drafts, reviews, failed, icebox, pendingRecs);
    }

    /// <summary>
    /// Updates the state of a specific recommendation within a plan's <c>recommendations.yaml</c> file.
    /// </summary>
    /// <param name="planFolderName">Name of the plan folder containing the recommendation.</param>
    /// <param name="recommendationTitle">Exact title of the recommendation to update.</param>
    /// <param name="newState">The new state value (e.g. <c>Pending</c>, <c>Accepted</c>, <c>Dismissed</c>).</param>
    /// <param name="declineReason">Optional reason for declining the recommendation.</param>
    public void UpdateRecommendationState(string planFolderName, string recommendationTitle, string newState, string? declineReason = null)
    {
        var recommendationsPath = Path.Combine(PlansDirectory, planFolderName, "artifacts", "recommendations.yaml");
        if (!File.Exists(recommendationsPath)) return;

        var yaml = FileHelper.ReadAllText(recommendationsPath);
        var items = YamlDeserializer.Deserialize<List<RecommendationYaml>>(yaml);
        if (items == null) return;

        var item = items.FirstOrDefault(r => r.Title == recommendationTitle);
        if (item == null) return;

        item.State = newState;
        if (newState == "Declined" && !string.IsNullOrWhiteSpace(declineReason))
        {
            item.DeclineReason = declineReason;
        }
        FileHelper.WriteAllText(recommendationsPath, YamlSerializer.Serialize(items));
    }

    private static DateTime? ExtractCompletedTimestamp(string logFilePath)
    {
        try
        {
            foreach (var line in File.ReadLines(logFilePath))
            {
                var match = Regex.Match(line, @"\*\*Completed:\*\*\s*(.+)");
                if (match.Success && DateTime.TryParse(match.Groups[1].Value.Trim(),
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AdjustToUniversal, out var dt))
                {
                    return dt;
                }
            }
        }
        catch { }
        return null;
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
}

public class HourlyTokenBurn
{
    public DateTime Hour { get; set; }
    public decimal Cost { get; set; }
    public int Tokens { get; set; }
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

public class RecommendationYaml
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string State { get; set; } = "Pending";
    public string? DeclineReason { get; set; }
}
