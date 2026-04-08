using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Plans.Dialogs;

public class UpdatePlanDialog(
    IState<bool> dialogOpen,
    IState<string> updateText,
    PlanFile selectedPlan,
    IJobService jobService,
    IPlanReaderService planService,
    Action refreshPlans) : ViewBase
{
    private readonly IState<bool> _dialogOpen = dialogOpen;
    private readonly IJobService _jobService = jobService;
    private readonly IPlanReaderService _planService = planService;
    private readonly Action _refreshPlans = refreshPlans;
    private readonly PlanFile _selectedPlan = selectedPlan;
    private readonly IState<string> _updateText = updateText;

    public override object? Build()
    {
        if (!_dialogOpen.Value) return null;

        return new Dialog(
            _ =>
            {
                _updateText.Set("");
                _dialogOpen.Set(false);
            },
            new DialogHeader($"Update Plan #{_selectedPlan.Id}"),
            new DialogBody(
                Layout.Vertical()
                | Text.P("Provide instructions for revising this draft plan.")
                | _updateText.ToTextareaInput("Enter update instructions...").Rows(6).AutoFocus()
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() =>
                {
                    _updateText.Set("");
                    _dialogOpen.Set(false);
                }),
                new Button("Submit Update").Primary().ShortcutKey("Ctrl+Enter").OnClick(() =>
                {
                    if (!string.IsNullOrWhiteSpace(_updateText.Value))
                    {
                        // Append >> comments to the latest revision so UpdatePlan can process them
                        var currentContent = _planService.ReadLatestRevision(_selectedPlan.FolderName);
                        var comments = string.Join("\n", _updateText.Value
                            .Split('\n')
                            .Select(line => $">> {line}"));
                        _planService.SavePlan(_selectedPlan.FolderName, currentContent + "\n\n" + comments + "\n");
                    }

                    _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Updating);
                    _jobService.StartJob("UpdatePlan", _selectedPlan.FolderPath);
                    _refreshPlans();
                    _updateText.Set("");
                    _dialogOpen.Set(false);
                })
            )
        ).Width(Size.Rem(30));
    }
}