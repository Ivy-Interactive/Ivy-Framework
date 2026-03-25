using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Plans.Dialogs;

public class CreateIssueDialog : ViewBase
{
    private readonly IState<bool> _dialogOpen;
    private readonly IState<string?> _selectedRepoState;
    private readonly IState<string?> _issueAssigneeState;
    private readonly PlanFile _selectedPlan;

    public CreateIssueDialog(
        IState<bool> dialogOpen,
        IState<string?> selectedRepoState,
        IState<string?> issueAssigneeState,
        PlanFile selectedPlan)
    {
        _dialogOpen = dialogOpen;
        _selectedRepoState = selectedRepoState;
        _issueAssigneeState = issueAssigneeState;
        _selectedPlan = selectedPlan;
    }

    public override object? Build()
    {
        var githubService = UseService<GithubService>();
        var assigneesQuery = UseQuery<string[], string>(
            _selectedRepoState.Value ?? "",
            async (_, ct) =>
            {
                var repos = githubService.GetRepos();
                var selectedRepo = repos.FirstOrDefault(r => r.DisplayName == _selectedRepoState.Value);
                if (selectedRepo is null) return Array.Empty<string>();
                var result = await githubService.GetAssigneesAsync(selectedRepo.Owner, selectedRepo.Name);
                return result.ToArray();
            },
            initialValue: Array.Empty<string>()
        );

        if (!_dialogOpen.Value) return null;

        var repos = githubService.GetRepos();
        var repositoryOptions = repos.Select(r => r.DisplayName).ToArray();
        var assignees = assigneesQuery.Value ?? Array.Empty<string>();

        return new Dialog(
            _ => _dialogOpen.Set(false),
            new DialogHeader("Create GitHub Issue"),
            new DialogBody(
                Layout.Vertical().Gap(3)
                    | Text.P($"Create a GitHub issue for plan #{_selectedPlan.Id}.")
                    | Text.Muted("Labels will be added automatically based on the plan metadata.")
                    | _selectedRepoState.ToSelectInput(repositoryOptions.ToOptions())
                        .WithField().Label("Repository")
                    | _issueAssigneeState.ToSelectInput(assignees.ToOptions())
                        .Nullable().WithField().Label("Assignee (optional)")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => _dialogOpen.Set(false)),
                new Button("Create Issue").Primary().OnClick(() => _dialogOpen.Set(false))
            )
        ).Width(Size.Rem(30));
    }
}
