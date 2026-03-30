using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

public record CommitAppArgs(string CommitHash, string? ProjectName = null, List<string>? RepoPaths = null);

[App(title: "Commit", icon: Icons.GitCommitVertical, isVisible: false)]
public class CommitApp : ViewBase
{
    public override object? Build()
    {
        var args = UseArgs<CommitAppArgs>();
        var gitService = UseService<GitService>();
        var configService = UseService<ConfigService>();

        if (args?.CommitHash is not { } commitHash || string.IsNullOrWhiteSpace(commitHash))
            return Text.P("No commit hash provided.");

        // Determine which repos to search — prefer explicit repo paths, then project config, then all repos
        var repoPaths = args.RepoPaths is { Count: > 0 }
            ? args.RepoPaths
            : args.ProjectName != null
                ? configService.GetProject(args.ProjectName)?.RepoPaths ?? []
                : configService.Projects.SelectMany(p => p.RepoPaths).Distinct().ToList();

        // Find the commit across repos
        string? diff = null;
        List<(string Status, string FilePath)>? files = null;
        string? title = null;
        foreach (var repo in repoPaths)
        {
            title = gitService.GetCommitTitle(repo, commitHash);
            if (title != null)
            {
                diff = gitService.GetCommitDiff(repo, commitHash);
                files = gitService.GetCommitFiles(repo, commitHash);
                break;
            }
        }

        if (title == null)
            return Text.P($"Commit not found: {commitHash}");

        var shortHash = commitHash.Length > 7 ? commitHash[..7] : commitHash;

        var content = Layout.Vertical().Gap(4).Padding(2);

        // Header
        content |= Layout.Horizontal().Gap(2)
            | new Badge(shortHash).Variant(BadgeVariant.Outline)
            | Text.Block(title).Bold();

        // Changed files
        if (files is { Count: > 0 })
        {
            var filesLayout = Layout.Vertical().Gap(1);
            filesLayout |= Text.Block("Changed Files").Bold();
            foreach (var (status, filePath) in files)
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
            content |= filesLayout;
        }

        // Diff
        if (!string.IsNullOrWhiteSpace(diff))
        {
            content |= Text.Block("Diff").Bold();
            content |= new CodeBlock(diff, Languages.Text)
                .ShowLineNumbers(false)
                .WrapLines();
        }

        return content;
    }
}
