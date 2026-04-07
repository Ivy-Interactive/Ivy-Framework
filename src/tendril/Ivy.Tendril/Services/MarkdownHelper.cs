using System.Text.RegularExpressions;

namespace Ivy.Tendril.Services;

public static class MarkdownHelper
{
    private static readonly Regex FileLinkRegex = new(
        @"\[([^\]]*)\]\((file:///[^)]+)\)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    ///     Annotates broken file:/// links in markdown content with a warning indicator.
    ///     Valid links are left unchanged.
    /// </summary>
    public static string AnnotateBrokenFileLinks(string markdownContent)
    {
        if (string.IsNullOrEmpty(markdownContent))
            return markdownContent;

        return FileLinkRegex.Replace(markdownContent, match =>
        {
            var linkText = match.Groups[1].Value;
            var url = match.Groups[2].Value;
            var filePath = url.Substring("file:///".Length);

            if (File.Exists(filePath))
                return match.Value;

            return $"[{linkText} \u26a0\ufe0f]({url})";
        });
    }

    /// <summary>
    ///     Searches for files with the given filename in the specified repo directories.
    /// </summary>
    public static List<string> FindFilesInRepos(IEnumerable<string> repoPaths, string fileName)
    {
        var results = new List<string>();
        foreach (var repoPath in repoPaths)
        {
            if (!Directory.Exists(repoPath))
                continue;

            try
            {
                var matches = Directory.GetFiles(repoPath, fileName, SearchOption.AllDirectories);
                results.AddRange(matches);
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we can't access
            }
        }

        return results;
    }
}