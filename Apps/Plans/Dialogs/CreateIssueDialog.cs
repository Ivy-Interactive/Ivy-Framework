using Ivy;

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
        if (!_dialogOpen.Value) return null;

        var repositoryOptions = new[] { "Ivy-Framework", "Ivy-Agent", "Ivy-Mcp", "Ivy" };
        var assignees = new[] { "Alice", "Bob", "Charlie", "Diana", "Eve" };

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
