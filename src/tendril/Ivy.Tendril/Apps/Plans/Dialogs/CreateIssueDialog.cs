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
    public override object? Build()
    {
        var githubService = UseService<IGithubService>();
        var assigneesError = UseState<string?>(null);
        var labelsError = UseState<string?>(null);

        var assigneesQuery = UseQuery<string[], string>(
            selectedRepoState.Value ?? "",
            async (repoName, _) =>
            {
                if (string.IsNullOrEmpty(repoName))
                {
                    assigneesError.Set(null);
                    return [];
                }
                var repos = githubService.GetRepos();
                var selectedRepo = repos.FirstOrDefault(r => r.DisplayName == repoName);
                if (selectedRepo is null)
                {
                    assigneesError.Set(null);
                    return [];
                }
                var (assignees, error) = await githubService.GetAssigneesAsync(selectedRepo.Owner, selectedRepo.Name);
                assigneesError.Set(error);
                return assignees.ToArray();
            },
            initialValue: []
        );

        var labelsQuery = UseQuery<string[], string>(
            selectedRepoState.Value ?? "",
            async (repoName, _) =>
            {
                if (string.IsNullOrEmpty(repoName))
                {
                    labelsError.Set(null);
                    return [];
                }
                var repos = githubService.GetRepos();
                var selectedRepo = repos.FirstOrDefault(r => r.DisplayName == repoName);
                if (selectedRepo is null)
                {
                    labelsError.Set(null);
                    return [];
                }
                var (labels, error) = await githubService.GetLabelsAsync(selectedRepo.Owner, selectedRepo.Name);
                labelsError.Set(error);
                return labels.ToArray();
            },
            initialValue: []
        );

        UseEffect(() =>
        {
            assigneesError.Set(null);
            labelsError.Set(null);
        }, selectedRepoState);

        if (!dialogOpen.Value) return null;

        var repos = githubService.GetRepos();
        var repositoryOptions = repos.Select(r => r.DisplayName).ToArray();
        var assignees = assigneesQuery.Value;
        var labels = labelsQuery.Value;

        return new Dialog(
            _ =>
            {
                issueCommentState.Set("");
                issueAssigneeState.Set(null);
                issueLabelsState.Set(Array.Empty<string>());
                assigneesError.Set(null);
                labelsError.Set(null);
                dialogOpen.Set(false);
            },
            new DialogHeader($"Create GitHub Issue #{selectedPlan.Id}"),
            new DialogBody(
                Layout.Vertical().Gap(3)
                | selectedRepoState.ToSelectInput(repositoryOptions.ToOptions())
                    .AutoFocus().WithField().Label("Repository").Required()
                | issueAssigneeState.ToSelectInput(assignees.ToOptions())
                    .Nullable().WithField().Label("Assignee")
                | issueLabelsState.ToSelectInput(labels.ToOptions())
                    .Placeholder("Select labels...").WithField().Label("Labels")
                | (assigneesError.Value is { } assigneeErr
                    ? Text.Danger(assigneeErr).Small()
                    : null)
                | (labelsError.Value is { } labelErr
                    ? Text.Danger(labelErr).Small()
                    : null)
                | issueCommentState.ToTextInput().Multiline().WithField().Label("Comment")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() =>
                {
                    issueCommentState.Set("");
                    issueAssigneeState.Set(null);
                    issueLabelsState.Set(Array.Empty<string>());
                    assigneesError.Set(null);
                    labelsError.Set(null);
                    dialogOpen.Set(false);
                }),
                new Button("Create Issue").Primary().OnClick(() =>
                {
                    if (selectedRepoState.Value is { } repo)
                    {
                        var selectedRepo = repos.FirstOrDefault(r => r.DisplayName == repo);
                        if (selectedRepo != null)
                        {
                            var repoPath = selectedRepo.FullName;
                            var assignee = issueAssigneeState.Value ?? "";
                            var selectedLabels = string.Join(",", issueLabelsState.Value);
                            jobService.StartJob("CreateIssue", selectedPlan.FolderPath, "-Repo", repoPath,
                                "-Assignee", assignee, "-Comment", issueCommentState.Value, "-Labels", selectedLabels);
                        }
                    }

                    issueCommentState.Set("");
                    issueAssigneeState.Set(null);
                    issueLabelsState.Set(Array.Empty<string>());
                    assigneesError.Set(null);
                    labelsError.Set(null);
                    dialogOpen.Set(false);
                })
            )
        ).Width(Size.Rem(30));
    }
}
