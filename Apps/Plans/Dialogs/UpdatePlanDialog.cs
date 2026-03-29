using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Plans.Dialogs;

public class UpdatePlanDialog(
    IState<bool> dialogOpen,
    IState<string> updateText,
    PlanFile selectedPlan,
    JobService jobService,
    PlanReaderService planService,
    Action refreshPlans) : ViewBase
{
    private readonly IState<bool> _dialogOpen = dialogOpen;
    private readonly IState<string> _updateText = updateText;
    private readonly PlanFile _selectedPlan = selectedPlan;
    private readonly JobService _jobService = jobService;
    private readonly PlanReaderService _planService = planService;
    private readonly Action _refreshPlans = refreshPlans;

    public override object? Build()
    {
        if (!_dialogOpen.Value) return null;

        return new Dialog(
            _ => _dialogOpen.Set(false),
            new DialogHeader($"Update Plan #{_selectedPlan.Id}"),
            new DialogBody(
                Layout.Vertical()
                    | Text.P("Provide instructions for updating this plan.")
                    | _updateText.ToTextareaInput("Enter update instructions...").Rows(6)
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => _dialogOpen.Set(false)),
                new Button("Submit Update").Primary().OnClick(() =>
                {
                    _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Updating);
                    _jobService.StartJob("UpdatePlan", _selectedPlan.FolderPath);
                    _refreshPlans();
                    _dialogOpen.Set(false);
                })
            )
        ).Width(Size.Rem(30));
    }
}
