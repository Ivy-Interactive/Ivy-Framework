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
        var errorMessage = UseState<string?>(null);
        var isFetching = UseState(false);
        var isImporting = UseState(false);
        var assigneesError = UseState<string?>(null);
        var labelsError = UseState<string?>(null);
        var reposState = UseState<List<RepoConfig>?>(null);
        var reposError = UseState<string?>(null);

        var assigneesQuery = UseQuery<string[], string>(
            selectedRepo.Value ?? "",
            async (repoName, _) =>
            {
                if (string.IsNullOrEmpty(repoName))
                {
                    assigneesError.Set(null);
                    return Array.Empty<string>();
                }
                var repos = githubService.GetRepos();
                var repo = repos.FirstOrDefault(r => r.DisplayName == repoName);
                if (repo is null)
                {
                    assigneesError.Set(null);
                    return Array.Empty<string>();
                }

                var (assignees, error) = await githubService.GetAssigneesAsync(repo.Owner, repo.Name);
                assigneesError.Set(error);
                return assignees.ToArray();
            },
            initialValue: Array.Empty<string>()
        );

        var labelsQuery = UseQuery<string[], string>(
            selectedRepo.Value ?? "",
            async (repoName, _) =>
            {
                if (string.IsNullOrEmpty(repoName))
                {
                    labelsError.Set(null);
                    return Array.Empty<string>();
                }
                var repos = githubService.GetRepos();
                var repo = repos.FirstOrDefault(r => r.DisplayName == repoName);
                if (repo is null)
                {
                    labelsError.Set(null);
                    return Array.Empty<string>();
                }

                var (labels, error) = await githubService.GetLabelsAsync(repo.Owner, repo.Name);
                labelsError.Set(error);
                return labels.ToArray();
            },
            initialValue: Array.Empty<string>()
        );

        UseEffect(() =>
        {
            fetchedIssues.Set(null);
            errorMessage.Set(null);
            selectedAssignee.Set(null);
            selectedLabels.Set(Array.Empty<string>());
            assigneesError.Set(null);
            labelsError.Set(null);
        }, selectedRepo);

        UseEffect(() =>
        {
            if (!_dialogOpen.Value)
            {
                reposState.Set(null);
                reposError.Set(null);
                return;
            }

            try
            {
                var repos = githubService.GetRepos();
                if (repos.Count == 0)
                {
                    reposError.Set("No GitHub repositories found. Check that your projects in config.yaml have valid git repositories with 'origin' remotes.");
                }
                else
                {
                    reposState.Set(repos);
                }
            }
            catch (Exception ex)
            {
                reposError.Set($"Failed to load repositories: {ex.Message}");
                Console.Error.WriteLine($"[ImportIssuesDialog] Exception loading repos: {ex}");
            }
        }, _dialogOpen);

        if (!_dialogOpen.Value) return null;

        if (reposState.Value is null && reposError.Value is null)
        {
            return new Dialog(
                _ => _dialogOpen.Set(false),
                new DialogHeader("Import Issues from GitHub"),
                new DialogBody(
                    Layout.Vertical().Gap(3)
                    | Text.Muted("Loading repositories...")
                    | new Loading()
                )
            ).Width(Size.Rem(36));
        }

        if (reposError.Value is { } repoErr)
        {
            return new Dialog(
                _ =>
                {
                    reposError.Set(null);
                    _dialogOpen.Set(false);
                },
                new DialogHeader("Import Issues from GitHub"),
                new DialogBody(Text.Danger(repoErr))
            ).Width(Size.Rem(36));
        }

        var repos = reposState.Value!;
        var repositoryOptions = repos.Select(r => r.DisplayName).ToArray();

        async Task FetchIssues()
        {
            if (selectedRepo.Value is not { } repoName) return;
            var repo = repos.FirstOrDefault(r => r.DisplayName == repoName);
            if (repo is null) return;

            isFetching.Set(true);
            errorMessage.Set(null);
            try
            {
                var labels = selectedLabels.Value.Length > 0 ? selectedLabels.Value : null;
                var (issues, error) = await githubService.SearchIssuesAsync(
                    repo.Owner, repo.Name,
                    string.IsNullOrWhiteSpace(searchQuery.Value) ? null : searchQuery.Value,
                    selectedAssignee.Value,
                    labels);

                if (error is not null)
                {
                    errorMessage.Set(error);
                    fetchedIssues.Set(null);
                }
                else
                {
                    fetchedIssues.Set(issues);
                }
            }
            catch (Exception ex)
            {
                errorMessage.Set($"Failed to fetch issues: {ex.Message}");
                fetchedIssues.Set(null);
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

                var projectName = GetProjectForRepo(githubService, repo.Owner, repo.Name);
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
                errorMessage.Set(null);
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
                | (assigneesError.Value is { } assigneeErr
                    ? Text.Danger(assigneeErr).Small()
                    : null)
                | (labelsError.Value is { } labelErr
                    ? Text.Danger(labelErr).Small()
                    : null)
                | new Button("Fetch Issues").Outline().Loading(isFetching.Value)
                    .OnClick(async () => await FetchIssues())
                | (errorMessage.Value is { } error
                    ? Text.Danger(error).Small()
                    : null)
                | issuesList
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() =>
                {
                    fetchedIssues.Set(null);
                    errorMessage.Set(null);
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

    private string GetProjectForRepo(IGithubService githubService, string owner, string repo)
    {
        var repoPath = $"{owner}/{repo}";
        var matchingProjects = _config.Settings.Projects
            .Where(p => p.RepoPaths.Any(path =>
                githubService.GetRepoConfigFromPathCached(path)?.FullName
                    .Equals(repoPath, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();

        return matchingProjects.Count == 1 ? matchingProjects[0].Name : "Auto";
    }

    internal static string SanitizeFileName(string title)
    {
        var sanitized = Regex.Replace(title, @"[^a-zA-Z0-9\s-]", "");
        sanitized = Regex.Replace(sanitized, @"\s+", "-");
        sanitized = sanitized.Trim('-').ToLowerInvariant();
        return sanitized.Length > 60 ? sanitized[..60].TrimEnd('-') : sanitized;
    }
}
