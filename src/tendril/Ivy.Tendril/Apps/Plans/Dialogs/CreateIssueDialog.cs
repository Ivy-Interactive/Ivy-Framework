using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Plans.Dialogs;

public class CreateIssueDialog(
    IState<bool> dialogOpen,
    IState<string?> selectedRepoState,
    IState<string?> issueAssigneeState,
    IState<string[]> issueLabelsState,
    IState<string> issueCommentState,
    PlanFile selectedPlan,
    IJobService jobService) : ViewBase
{
    private readonly IState<bool> _dialogOpen = dialogOpen;
    private readonly IState<string?> _issueAssigneeState = issueAssigneeState;
    private readonly IState<string> _issueCommentState = issueCommentState;
    private readonly IState<string[]> _issueLabelsState = issueLabelsState;
    private readonly IJobService _jobService = jobService;
    private readonly PlanFile _selectedPlan = selectedPlan;
    private readonly IState<string?> _selectedRepoState = selectedRepoState;

    public override object? Build()
    {
        var githubService = UseService<IGithubService>();
        var assigneesQuery = UseQuery<string[], string>(
            _selectedRepoState.Value ?? "",
            async (repoName, _) =>
            {
                if (string.IsNullOrEmpty(repoName)) return Array.Empty<string>();
                var repos = githubService.GetRepos();
                var selectedRepo = repos.FirstOrDefault(r => r.DisplayName == repoName);
                if (selectedRepo is null) return Array.Empty<string>();
                var result = await githubService.GetAssigneesAsync(selectedRepo.Owner, selectedRepo.Name);
                return result.ToArray();
            },
            initialValue: Array.Empty<string>()
        );

        var labelsQuery = UseQuery<string[], string>(
            _selectedRepoState.Value ?? "",
            async (repoName, _) =>
            {
                if (string.IsNullOrEmpty(repoName)) return Array.Empty<string>();
                var repos = githubService.GetRepos();
                var selectedRepo = repos.FirstOrDefault(r => r.DisplayName == repoName);
                if (selectedRepo is null) return Array.Empty<string>();
                var result = await githubService.GetLabelsAsync(selectedRepo.Owner, selectedRepo.Name);
                return result.ToArray();
            },
            initialValue: Array.Empty<string>()
        );

        if (!_dialogOpen.Value) return null;

        var repos = githubService.GetRepos();
        var repositoryOptions = repos.Select(r => r.DisplayName).ToArray();
        var assignees = assigneesQuery.Value;
        var labels = labelsQuery.Value;

        return new Dialog(
            _ =>
            {
                _issueCommentState.Set("");
                _issueAssigneeState.Set(null);
                _issueLabelsState.Set(Array.Empty<string>());
                _dialogOpen.Set(false);
            },
            new DialogHeader($"Create GitHub Issue #{_selectedPlan.Id}"),
            new DialogBody(
                Layout.Vertical().Gap(3)
                | _selectedRepoState.ToSelectInput(repositoryOptions.ToOptions())
                    .AutoFocus().WithField().Label("Repository").Required()
                | _issueAssigneeState.ToSelectInput(assignees.ToOptions())
                    .Nullable().WithField().Label("Assignee")
                | _issueLabelsState.ToSelectInput(labels.ToOptions())
                    .Placeholder("Select labels...").WithField().Label("Labels")
                | _issueCommentState.ToTextInput().Multiline().WithField().Label("Comment")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() =>
                {
                    _issueCommentState.Set("");
                    _issueAssigneeState.Set(null);
                    _issueLabelsState.Set(Array.Empty<string>());
                    _dialogOpen.Set(false);
                }),
                new Button("Create Issue").Primary().OnClick(() =>
                {
                    if (_selectedRepoState.Value is { } repo)
                    {
                        var availableRepos = githubService.GetRepos();
                        var selectedRepo = availableRepos.FirstOrDefault(r => r.DisplayName == repo);
                        if (selectedRepo != null)
                        {
                            var repoPath = selectedRepo.FullName;
                            var assignee = _issueAssigneeState.Value ?? "";
                            var selectedLabels = string.Join(",", _issueLabelsState.Value);
                            _jobService.StartJob("CreateIssue", _selectedPlan.FolderPath, "-Repo", repoPath,
                                "-Assignee", assignee, "-Comment", _issueCommentState.Value, "-Labels", selectedLabels);
                        }
                    }

                    _issueCommentState.Set("");
                    _issueAssigneeState.Set(null);
                    _issueLabelsState.Set(Array.Empty<string>());
                    _dialogOpen.Set(false);
                })
            )
        ).Width(Size.Rem(30));
    }
}
