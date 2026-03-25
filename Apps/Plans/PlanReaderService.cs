using System.Text.RegularExpressions;

namespace Ivy.Tendril.Apps.Plans;

public class PlanReaderService
{
    private readonly ConfigService _config;

    public PlanReaderService(ConfigService config)
    {
        _config = config;
    }

    private string PlansDirectory => _config.PlanFolder;

    private static readonly Regex FileNameRegex = new(@"^(\d+)-([^-]+)-([^-]+)-(.+)\.md$", RegexOptions.Compiled);
    private static readonly Regex CamelCaseRegex = new(@"(?<=\p{Ll})(?=\p{Lu})", RegexOptions.Compiled);

    public List<PlanFile> GetPlans()
    {
        try
        {
            var plans = new List<PlanFile>();

            if (Directory.Exists(PlansDirectory))
            {
                plans.AddRange(ParseDirectory(PlansDirectory, PlanStatus.Draft));
            }

            return plans;
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

    public void SkipPlan(string fileName)
    {
        var source = Path.Combine(PlansDirectory, fileName);
        var skippedDir = Path.Combine(PlansDirectory, "skipped");
        Directory.CreateDirectory(skippedDir);
        var dest = Path.Combine(skippedDir, fileName);
        File.Move(source, dest);
    }

    private List<PlanFile> ParseDirectory(string directory, PlanStatus status)
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
            var level = match.Groups[3].Value;
            var title = CamelCaseRegex.Replace(match.Groups[4].Value.Replace("-", " "), " ");

            var content = File.ReadAllText(file);
            var rawFrontmatter = ExtractFrontmatter(content);
            var contentWithoutFrontmatter = RemoveFrontmatter(content);

            var metadata = new PlanMetadata(id, queue, level, title);
            plans.Add(new PlanFile(metadata, contentWithoutFrontmatter, rawFrontmatter, fileName, status));
        }

        return plans;
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
