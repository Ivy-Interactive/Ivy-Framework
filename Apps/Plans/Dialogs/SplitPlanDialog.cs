using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Plans.Dialogs;

public class SplitPlanDialog(
    IState<bool> dialogOpen,
    IState<string> splitText,
    PlanFile selectedPlan,
    JobService jobService,
    PlanReaderService planService,
    Action refreshPlans) : ViewBase
{
    private readonly IState<bool> _dialogOpen = dialogOpen;
    private readonly IState<string> _splitText = splitText;
    private readonly PlanFile _selectedPlan = selectedPlan;
    private readonly JobService _jobService = jobService;
    private readonly PlanReaderService _planService = planService;
    private readonly Action _refreshPlans = refreshPlans;

    public override object? Build()
    {
        if (!_dialogOpen.Value) return null;

        return new Dialog(
            _ => _dialogOpen.Set(false),
            new DialogHeader($"Split Plan #{_selectedPlan.Id}"),
            new DialogBody(
                Layout.Vertical()
                    | Text.P("Describe how to split this plan into multiple plans.")
                    | _splitText.ToTextareaInput("Enter split instructions...").Rows(6)
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => _dialogOpen.Set(false)),
                new Button("Split Plan").Primary().OnClick(() =>
                {
                    _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Updating);
                    _jobService.StartJob("SplitPlan", _selectedPlan.FolderPath);
                    _refreshPlans();
                    _dialogOpen.Set(false);
                })
            )
        ).Width(Size.Rem(30));
    }
}
