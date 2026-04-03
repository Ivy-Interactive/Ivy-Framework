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

                var yaml = File.ReadAllText(planYamlPath);
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
                    File.WriteAllText(planYamlPath, updated);
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

                var yaml = File.ReadAllText(planYamlPath);

                // Fix structured repos (path: + optional prRule:) → plain path strings
                var repaired = Regex.Replace(yaml,
                    @"(?m)^(\s*)-\s+path:\s*(.+?)(?:\r?\n\s+prRule:\s*.+)?$",
                    "$1- $2");

                if (repaired != yaml)
                    File.WriteAllText(planYamlPath, repaired);
            }
        }
        catch { }
    }

    public List<PlanFile> GetPlans(PlanStatus? statusFilter = null)
    {
        try
        {
            if (!Directory.Exists(PlansDirectory))
                return new List<PlanFile>();

            var plans = new List<PlanFile>();

            foreach (var dir in Directory.GetDirectories(PlansDirectory))
            {
                var folderName = Path.GetFileName(dir);
                var match = FolderNameRegex.Match(folderName);
                if (!match.Success) continue;

                var planYamlPath = Path.Combine(dir, "plan.yaml");
                if (!File.Exists(planYamlPath)) continue;

                var plan = ParsePlanFolder(dir);
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

    public PlanFile? GetPlanByFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return null;
        return ParsePlanFolder(folderPath);
    }

    public List<PlanFile> GetIceboxPlans()
    {
        return GetPlans(PlanStatus.Icebox);
    }

    public void TransitionState(string folderName, PlanStatus newState)
    {
        var folderPath = Path.Combine(PlansDirectory, folderName);
        var planYamlPath = Path.Combine(folderPath, "plan.yaml");

        if (!File.Exists(planYamlPath)) return;

        var yaml = File.ReadAllText(planYamlPath);
        var planYaml = YamlDeserializer.Deserialize<PlanYaml>(yaml) ?? new PlanYaml();

        planYaml.State = newState.ToString();
        planYaml.Updated = DateTime.UtcNow;

        File.WriteAllText(planYamlPath, YamlSerializer.Serialize(planYaml));
    }

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
            var yaml = File.ReadAllText(planYamlPath);
            var planYaml = YamlDeserializer.Deserialize<PlanYaml>(yaml) ?? new PlanYaml();
            planYaml.Updated = DateTime.UtcNow;
            File.WriteAllText(planYamlPath, YamlSerializer.Serialize(planYaml));
        }
    }

    public string ReadLatestRevision(string folderName)
    {
        var revisionsDir = Path.Combine(PlansDirectory, folderName, "revisions");
        if (!Directory.Exists(revisionsDir)) return string.Empty;

        var latestFile = Directory.GetFiles(revisionsDir, "*.md")
            .OrderByDescending(f => f)
            .FirstOrDefault();

        return latestFile != null ? File.ReadAllText(latestFile) : string.Empty;
    }

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

    public void DeletePlan(string folderName)
    {
        var folderPath = Path.Combine(PlansDirectory, folderName);
        if (Directory.Exists(folderPath))
            Directory.Delete(folderPath, recursive: true);
    }

    public string ReadRawPlan(string folderName)
    {
        return ReadLatestRevision(folderName);
    }

    public void SavePlan(string folderName, string fullContent)
    {
        UpdateLatestRevision(folderName, fullContent);
    }

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
                var yaml = File.ReadAllText(planYamlPath);
                var planYaml = YamlDeserializer.Deserialize<PlanYaml>(yaml) ?? new PlanYaml();
                planYaml.Updated = DateTime.UtcNow;
                File.WriteAllText(planYamlPath, YamlSerializer.Serialize(planYaml));
            }
        }
    }

    private PlanFile? ParsePlanFolder(string folderPath)
    {
        try
        {
            var planYamlPath = Path.Combine(folderPath, "plan.yaml");
            var yamlContent = File.ReadAllText(planYamlPath);
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

    public decimal GetPlanTotalCost(string folderPath)
    {
        var costsPath = Path.Combine(folderPath, "costs.csv");
        if (!File.Exists(costsPath)) return 0m;

        var lines = File.ReadAllLines(costsPath);
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

    public int GetPlanTotalTokens(string folderPath)
    {
        var costsPath = Path.Combine(folderPath, "costs.csv");
        if (!File.Exists(costsPath)) return 0;

        var lines = File.ReadAllLines(costsPath);
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

                var costLines = File.ReadAllLines(costsPath).Skip(1).ToList();
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

    public List<Recommendation> GetRecommendations()
    {
        var recommendations = new List<Recommendation>();
        var plans = GetPlans();

        foreach (var plan in plans)
        {
            var recommendationsPath = Path.Combine(plan.FolderPath, "artifacts", "recommendations.yaml");
            if (!File.Exists(recommendationsPath)) continue;

            try
            {
                var yaml = File.ReadAllText(recommendationsPath);
                var items = YamlDeserializer.Deserialize<List<RecommendationYaml>>(yaml);
                if (items == null) continue;

                foreach (var item in items)
                {
                    recommendations.Add(new Recommendation(
                        Title: item.Title,
                        Description: item.Description,
                        PlanId: plan.Id.ToString("D5"),
                        PlanTitle: plan.Title,
                        PlanFolderName: plan.FolderName,
                        Date: plan.Updated
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
    string PlanId,
    string PlanTitle,
    string PlanFolderName,
    DateTime Date
);

public class RecommendationYaml
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
}
