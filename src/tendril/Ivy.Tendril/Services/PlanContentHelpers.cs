using Ivy.Tendril.Apps.Plans;
using Ivy.Widgets.DiffView;

namespace Ivy.Tendril.Services;

public static class PlanContentHelpers
{
    public record CommitRow(string Hash, string ShortHash, string Title, int? FileCount);

    public record CommitDetailData(
        string Title,
        string? Diff,
        List<(string Status, string FilePath)>? Files
    );

    public static Dictionary<string, List<string>> GetArtifacts(string folderPath)
    {
        var artifactsDir = Path.Combine(folderPath, "artifacts");
        var result = new Dictionary<string, List<string>>();
        if (!Directory.Exists(artifactsDir)) return result;

        foreach (var subDir in Directory.GetDirectories(artifactsDir))
        {
            var category = Path.GetFileName(subDir);
            var files = Directory.GetFiles(subDir, "*", SearchOption.AllDirectories).ToList();
            if (files.Count > 0)
                result[category] = files;
        }

        var rootFiles = Directory.GetFiles(artifactsDir).ToList();
        if (rootFiles.Count > 0)
            result["other"] = rootFiles;

        return result;
    }

    public static List<CommitRow> BuildCommitRows(PlanFile plan, IConfigService config, IGitService gitService)
    {
        var repoPaths = plan.GetEffectiveRepoPaths(config);
        return plan.Commits.Select(commit =>
        {
            string? title = null;
            int? fileCount = null;
            foreach (var repo in repoPaths)
            {
                var t = gitService.GetCommitTitle(repo, commit);
                if (t != null)
                {
                    title = t;
                    fileCount = gitService.GetCommitFileCount(repo, commit);
                    break;
                }
            }
            var shortHash = commit.Length > 7 ? commit[..7] : commit;
            return new CommitRow(commit, shortHash, title ?? "", fileCount);
        }).ToList();
    }

    public static object? BuildCommitWarningCallout(List<CommitRow> commitRows)
    {
        var issues = new List<string>();
        foreach (var row in commitRows)
        {
            if (string.IsNullOrWhiteSpace(row.Title))
                issues.Add($"Commit `{row.ShortHash}` has no message");
            if (row.FileCount == 0)
                issues.Add($"Commit `{row.ShortHash}` has no file changes");
        }

        if (issues.Count == 0)
            return null;

        var message = string.Join("\n\n", issues);
        return Callout.Warning(message, "Commit Warning");
    }

    public static object RenderCommitDetailSheet(CommitDetailData? data, bool loading, string? commitHash,
        Action closeSheet)
    {
        if (commitHash is null) return new Empty();

        var shortHash = commitHash.Length > 7 ? commitHash[..7] : commitHash;
        object sheetContent;

        if (loading || data is null)
        {
            sheetContent = Text.Muted("Loading...");
        }
        else
        {
            var commitSheetContent = Layout.Vertical().Gap(4).Padding(2);

            if (data.Files is { Count: > 0 })
            {
                var filesLayout = Layout.Vertical().Gap(1);
                filesLayout |= Text.Block("Changed Files").Bold();
                foreach (var (status, filePath) in data.Files)
                {
                    var (label, variant) = status switch
                    {
                        "A" => ("Added", BadgeVariant.Success),
                        "D" => ("Deleted", BadgeVariant.Destructive),
                        _ => ("Modified", BadgeVariant.Outline)
                    };
                    filesLayout |= Layout.Horizontal().Gap(2)
                        | new Badge(label).Variant(variant).Small()
                        | Text.Block(filePath);
                }
                commitSheetContent |= filesLayout;
            }

            if (!string.IsNullOrWhiteSpace(data.Diff))
            {
                commitSheetContent |= Text.Block("Diff").Bold();
                commitSheetContent |= new DiffView().Diff(data.Diff).Split();
            }

            sheetContent = commitSheetContent;
        }

        return new Sheet(
            onClose: closeSheet,
            content: sheetContent,
            title: $"Commit {shortHash} — {data?.Title ?? ""}"
        ).Width(Size.Half()).Resizable();
    }

    public static object RenderArtifactScreenshots(Dictionary<string, List<string>> artifacts)
    {
        if (!artifacts.TryGetValue("screenshots", out var screenshotFiles))
            return new Empty();

        var screenshotsLayout = Layout.Horizontal().Gap(2).Wrap();
        foreach (var file in screenshotFiles)
        {
            var imageUrl = $"/ivy/local-file?path={Uri.EscapeDataString(file)}";
            screenshotsLayout |= new Image(imageUrl)
            { ObjectFit = ImageFit.Contain, Alt = Path.GetFileName(file), Overlay = true }
                .Height(Size.Units(15)).Width(Size.Units(22))
                .BorderColor(Colors.Neutral)
                .BorderStyle(BorderStyle.Solid)
                .BorderThickness(1)
                .BorderRadius(BorderRadius.Rounded);
        }

        return screenshotsLayout;
    }
}
