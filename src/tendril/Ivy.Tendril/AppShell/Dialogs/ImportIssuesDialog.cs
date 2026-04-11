using System.Text.RegularExpressions;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.AppShell.Dialogs;

public class ImportIssuesDialog(IState<bool> dialogOpen, IConfigService config) : ViewBase
{
    private readonly IConfigService _config = config;
    private readonly IState<bool> _dialogOpen = dialogOpen;

    public override object? Build()
    {
        var githubService = UseService<IGithubService>();
        var client = UseService<IClientProvider>();

        var selectedRepo = UseState<string?>(null);
        var searchQuery = UseState("");
        var selectedAssignee = UseState<string?>(null);
        var selectedLabels = UseState(Array.Empty<string>());
        var fetchedIssues = UseState<List<GitHubIssue>?>(null);
        var isFetching = UseState(false);
        var isImporting = UseState(false);

        var assigneesQuery = UseQuery<string[], string>(
            selectedRepo.Value ?? "",
            async (repoName, _) =>
            {
                if (string.IsNullOrEmpty(repoName)) return Array.Empty<string>();
                var repos = githubService.GetRepos();
                var repo = repos.FirstOrDefault(r => r.DisplayName == repoName);
                if (repo is null) return Array.Empty<string>();
                return (await githubService.GetAssigneesAsync(repo.Owner, repo.Name)).ToArray();
            },
            initialValue: Array.Empty<string>()
        );

        var labelsQuery = UseQuery<string[], string>(
            selectedRepo.Value ?? "",
            async (repoName, _) =>
            {
                if (string.IsNullOrEmpty(repoName)) return Array.Empty<string>();
                var repos = githubService.GetRepos();
                var repo = repos.FirstOrDefault(r => r.DisplayName == repoName);
                if (repo is null) return Array.Empty<string>();
                return (await githubService.GetLabelsAsync(repo.Owner, repo.Name)).ToArray();
            },
            initialValue: Array.Empty<string>()
        );

        UseEffect(() =>
        {
            fetchedIssues.Set(null);
            selectedAssignee.Set(null);
            selectedLabels.Set(Array.Empty<string>());
        }, selectedRepo);

        if (!_dialogOpen.Value) return null;

        var repos = githubService.GetRepos();
        var repositoryOptions = repos.Select(r => r.DisplayName).ToArray();

        async Task FetchIssues()
        {
            if (selectedRepo.Value is not { } repoName) return;
            var repo = repos.FirstOrDefault(r => r.DisplayName == repoName);
            if (repo is null) return;

            isFetching.Set(true);
            try
            {
                var labels = selectedLabels.Value.Length > 0 ? selectedLabels.Value : null;
                var issues = await githubService.SearchIssuesAsync(
                    repo.Owner, repo.Name,
                    string.IsNullOrWhiteSpace(searchQuery.Value) ? null : searchQuery.Value,
                    selectedAssignee.Value,
                    labels);
                fetchedIssues.Set(issues);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ImportIssuesDialog] Fetch failed: {ex.Message}");
                fetchedIssues.Set([]);
            }
            finally
            {
                isFetching.Set(false);
            }
        }

        async Task ImportAll()
        {
            if (fetchedIssues.Value is not { Count: > 0 } issues) return;
            if (selectedRepo.Value is not { } repoName) return;
            var repo = repos.FirstOrDefault(r => r.DisplayName == repoName);
            if (repo is null) return;

            isImporting.Set(true);
            try
            {
                var inboxPath = Path.Combine(_config.TendrilHome, "Inbox");
                Directory.CreateDirectory(inboxPath);

                var projectName = GetProjectForRepo(repo.Owner, repo.Name);
                var importedCount = 0;

                foreach (var issue in issues)
                {
                    var safeName = SanitizeFileName(issue.Title);
                    var fileName = $"{issue.Number}-{safeName}.md";
                    var filePath = Path.Combine(inboxPath, fileName);

                    if (File.Exists(filePath)) continue;

                    var content = $"""
                                   ---
                                   project: {projectName}
                                   ---
                                   [GitHub Issue #{issue.Number}](https://github.com/{repo.Owner}/{repo.Name}/issues/{issue.Number})

                                   {issue.Body}
                                   """;

                    await File.WriteAllTextAsync(filePath, content);
                    importedCount++;
                }

                client.Toast($"Imported {importedCount} issue{(importedCount == 1 ? "" : "s")} to Inbox", "Import Complete");
                _dialogOpen.Set(false);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ImportIssuesDialog] Import failed: {ex.Message}");
                client.Toast($"Import failed: {ex.Message}", "Error");
            }
            finally
            {
                isImporting.Set(false);
            }
        }

        object? issuesList = null;
        if (isFetching.Value)
        {
            issuesList = Text.Muted("Fetching issues...");
        }
        else if (fetchedIssues.Value is { } issues)
        {
            if (issues.Count == 0)
            {
                issuesList = Text.Muted("No issues found matching the filters.");
            }
            else
            {
                issuesList = Layout.Vertical().Gap(1)
                             | Text.Label($"Found {issues.Count} issue{(issues.Count == 1 ? "" : "s")}")
                             | (Layout.Vertical().Gap(1)
                                | issues.Select(i =>
                                    (object)Text.Muted($"#{i.Number} — {i.Title}")
                                ).ToArray());
            }
        }

        return new Dialog(
            _ =>
            {
                fetchedIssues.Set(null);
                searchQuery.Set("");
                selectedAssignee.Set(null);
                selectedLabels.Set(Array.Empty<string>());
                selectedRepo.Set(null);
                _dialogOpen.Set(false);
            },
            new DialogHeader("Import Issues from GitHub"),
            new DialogBody(
                Layout.Vertical().Gap(3)
                | selectedRepo.ToSelectInput(repositoryOptions.ToOptions())
                    .AutoFocus().WithField().Label("Repository").Required()
                | searchQuery.ToTextInput().Placeholder("Search query...").WithField().Label("Search")
                | selectedAssignee.ToSelectInput(assigneesQuery.Value.ToOptions())
                    .Nullable().WithField().Label("Assignee")
                | selectedLabels.ToSelectInput(labelsQuery.Value.ToOptions())
                    .Placeholder("Select labels...").WithField().Label("Labels")
                | new Button("Fetch Issues").Outline().Loading(isFetching.Value)
                    .OnClick(async () => await FetchIssues())
                | issuesList
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() =>
                {
                    fetchedIssues.Set(null);
                    searchQuery.Set("");
                    selectedAssignee.Set(null);
                    selectedLabels.Set(Array.Empty<string>());
                    selectedRepo.Set(null);
                    _dialogOpen.Set(false);
                }),
                new Button("Import All").Primary()
                    .Loading(isImporting.Value)
                    .Disabled(fetchedIssues.Value is not { Count: > 0 })
                    .OnClick(async () => await ImportAll())
            )
        ).Width(Size.Rem(36));
    }

    private string GetProjectForRepo(string owner, string repo)
    {
        var repoPath = $"{owner}/{repo}";
        var matchingProjects = _config.Settings.Projects
            .Where(p => p.RepoPaths.Any(path =>
                GithubService.GetRepoConfigFromPath(path)?.FullName
                    .Equals(repoPath, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();

        return matchingProjects.Count == 1 ? matchingProjects[0].Name : "[Auto]";
    }

    internal static string SanitizeFileName(string title)
    {
        var sanitized = Regex.Replace(title, @"[^a-zA-Z0-9\s-]", "");
        sanitized = Regex.Replace(sanitized, @"\s+", "-");
        sanitized = sanitized.Trim('-').ToLowerInvariant();
        return sanitized.Length > 60 ? sanitized[..60].TrimEnd('-') : sanitized;
    }
}
