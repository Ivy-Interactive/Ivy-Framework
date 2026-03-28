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

    public string CreatePlan(string description, string project = "General", string level = "NiceToHave")
    {
        Directory.CreateDirectory(PlansDirectory);

        var counterFile = Path.Combine(PlansDirectory, ".counter");
        var counter = File.Exists(counterFile) ? int.Parse(File.ReadAllText(counterFile).Trim()) : 1087;
        var id = counter;
        File.WriteAllText(counterFile, (counter + 1).ToString());

        var safeTitle = description.Length > 60 ? description.Substring(0, 60) : description;
        safeTitle = Regex.Replace(safeTitle, @"[^a-zA-Z0-9]+", "").Trim();
        if (string.IsNullOrEmpty(safeTitle)) safeTitle = "Untitled";

        var folderName = $"{id:D5}-{safeTitle}";
        var folderPath = Path.Combine(PlansDirectory, folderName);
        Directory.CreateDirectory(folderPath);
        Directory.CreateDirectory(Path.Combine(folderPath, "revisions"));
        Directory.CreateDirectory(Path.Combine(folderPath, "logs"));
        Directory.CreateDirectory(Path.Combine(folderPath, "worktrees"));
        Directory.CreateDirectory(Path.Combine(folderPath, "artifacts"));

        var planYaml = new PlanYaml
        {
            State = "Draft",
            Project = project,
            Level = level,
            Title = description,
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow,
            InitialPrompt = description,
        };

        File.WriteAllText(
            Path.Combine(folderPath, "plan.yaml"),
            YamlSerializer.Serialize(planYaml)
        );

        var content = $"# {description}\n\n## Problem\n\n{description}\n\n## Solution\n\n## Tests\n\n## Finish\n\nCommit!\n";
        SaveRevision(folderName, content);

        return folderName;
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
        SaveRevision(folderName, fullContent);
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

            var metadata = new PlanMetadata(id, planYaml.Project, planYaml.Level, planYaml.Title, status);
            var latestContent = ReadLatestRevision(folderName);

            return new PlanFile(metadata, latestContent, folderPath, yamlContent);
        }
        catch
        {
            return null;
        }
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
