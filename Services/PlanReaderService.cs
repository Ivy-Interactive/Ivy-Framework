using System.Text.RegularExpressions;

using Ivy.Tendril.Apps.Plans;

namespace Ivy.Tendril.Services;

public class PlanReaderService
{
    private readonly ConfigService _config;

    public PlanReaderService(ConfigService config)
    {
        _config = config;
    }

    public string PlansDirectory => _config.PlanFolder;

    private static readonly Regex FileNameRegex = new(@"^(\d+)-([^-]+)-([^-]+)-(.+)\.md$", RegexOptions.Compiled);
    private static readonly Regex CamelCaseRegex = new(@"(?<=\p{Ll})(?=\p{Lu})", RegexOptions.Compiled);

    public List<PlanFile> GetPlans()
    {
        try
        {
            var plans = new List<PlanFile>();

            if (Directory.Exists(PlansDirectory))
            {
                plans.AddRange(ParseDirectory(PlansDirectory, PlanStatus.Draft, ""));
            }

            return plans.OrderBy(p => p.Id).ToList();
        }
        catch
        {
            return new List<PlanFile>();
        }
    }

    public void ApprovePlan(string fileName)
    {
        var source = Path.Combine(PlansDirectory, fileName);
        var approvedDir = Path.Combine(PlansDirectory, "approved");
        Directory.CreateDirectory(approvedDir);
        var dest = Path.Combine(approvedDir, fileName);
        File.Move(source, dest);
    }

    public void ExpandPlan(string fileName)
    {
        var source = Path.Combine(PlansDirectory, fileName);
        var expandingDir = Path.Combine(PlansDirectory, "expanding");
        Directory.CreateDirectory(expandingDir);
        var dest = Path.Combine(expandingDir, fileName);
        File.Move(source, dest);
    }

    public void SkipPlan(string fileName)
    {
        var source = Path.Combine(PlansDirectory, fileName);
        var skippedDir = Path.Combine(PlansDirectory, "skipped");
        Directory.CreateDirectory(skippedDir);
        var dest = Path.Combine(skippedDir, fileName);
        File.Move(source, dest);
    }

    public void IceboxPlan(string fileName)
    {
        var source = Path.Combine(PlansDirectory, fileName);
        var iceboxDir = Path.Combine(PlansDirectory, "icebox");
        Directory.CreateDirectory(iceboxDir);
        var dest = Path.Combine(iceboxDir, fileName);
        File.Move(source, dest);
    }

    public void DeletePlan(string fileName)
    {
        var filePath = Path.Combine(PlansDirectory, fileName);
        File.Delete(filePath);
    }

    public List<PlanFile> GetIceboxPlans()
    {
        try
        {
            var iceboxDir = Path.Combine(PlansDirectory, "icebox");
            if (!Directory.Exists(iceboxDir))
                return new List<PlanFile>();

            return ParseDirectory(iceboxDir, PlanStatus.Draft, "icebox")
                .OrderBy(p => p.Id)
                .ToList();
        }
        catch
        {
            return new List<PlanFile>();
        }
    }

    public void ThawPlan(string fileName)
    {
        var source = Path.Combine(PlansDirectory, "icebox", fileName);
        var dest = Path.Combine(PlansDirectory, fileName);
        File.Move(source, dest);
    }

    public void SavePlan(string fileName, string fullFileContent)
    {
        var path = Path.Combine(PlansDirectory, fileName);
        File.WriteAllText(path, fullFileContent);
    }

    public string ReadRawPlan(string fileName)
    {
        var path = Path.Combine(PlansDirectory, fileName);
        return File.ReadAllText(path);
    }

    public void CreatePlan(string description)
    {
        var counterFile = Path.Combine(PlansDirectory, ".counter");
        var counter = File.Exists(counterFile) ? int.Parse(File.ReadAllText(counterFile).Trim()) : 200;
        var id = counter;
        File.WriteAllText(counterFile, (counter + 1).ToString());

        var safeTitle = description.Length > 40 ? description.Substring(0, 40) : description;
        safeTitle = Regex.Replace(safeTitle, @"[^a-zA-Z0-9]+", "-").Trim('-');

        var fileName = $"{id:D3}-General-NiceToHave-{safeTitle}.md";
        var content = $"# {description}\n\n## Problem\n\n{description}\n\n## Solution\n\n## Tests\n\n## Finish\n\nCommit!\n";
        File.WriteAllText(Path.Combine(PlansDirectory, fileName), content);
    }

    private List<PlanFile> ParseDirectory(string directory, PlanStatus status, string subdirectory)
    {
        var plans = new List<PlanFile>();
        var files = Directory.GetFiles(directory, "*.md");

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var match = FileNameRegex.Match(fileName);
            if (!match.Success) continue;

            var id = int.Parse(match.Groups[1].Value);
            var queue = match.Groups[2].Value;
            var level = NormalizeLevel(match.Groups[3].Value);
            var title = CamelCaseRegex.Replace(match.Groups[4].Value.Replace("-", " "), " ");

            var content = File.ReadAllText(file);
            var rawFrontmatter = ExtractFrontmatter(content);
            var contentWithoutFrontmatter = RemoveFrontmatter(content);

            // Include subdirectory in fileName if present
            var fullFileName = string.IsNullOrEmpty(subdirectory)
                ? fileName
                : Path.Combine(subdirectory, fileName).Replace("\\", "/");

            var metadata = new PlanMetadata(id, queue, level, title);
            plans.Add(new PlanFile(metadata, contentWithoutFrontmatter, rawFrontmatter, fullFileName, status));
        }

        return plans;
    }

    private static string NormalizeLevel(string level)
    {
        return level.ToUpperInvariant() switch
        {
            "CRITICAL" => "Critical",
            "NICETOHAVE" => "NiceToHave",
            "NITPICK" => "Nitpick",
            _ => level
        };
    }

    private static string ExtractFrontmatter(string content)
    {
        if (!content.StartsWith("---")) return string.Empty;

        var endIndex = content.IndexOf("---", 3);
        if (endIndex < 0) return string.Empty;

        return content.Substring(3, endIndex - 3).Trim();
    }

    private static string RemoveFrontmatter(string content)
    {
        if (!content.StartsWith("---")) return content;

        var endIndex = content.IndexOf("---", 3);
        if (endIndex < 0) return content;

        var startOfContent = endIndex + 3;
        while (startOfContent < content.Length &&
               (content[startOfContent] == '\r' || content[startOfContent] == '\n'))
        {
            startOfContent++;
        }

        return content.Substring(startOfContent);
    }
}
