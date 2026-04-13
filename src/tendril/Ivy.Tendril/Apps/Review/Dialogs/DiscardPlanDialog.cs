using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Review.Dialogs;

public class DiscardPlanDialog(
    IState<bool> dialogOpen,
    PlanFile selectedPlan,
    IPlanReaderService planService,
    Action refreshPlans) : ViewBase
{
    private readonly IState<bool> _dialogOpen = dialogOpen;
    private readonly IPlanReaderService _planService = planService;
    private readonly Action _refreshPlans = refreshPlans;
    private readonly PlanFile _selectedPlan = selectedPlan;

    public override object? Build()
    {
        if (!_dialogOpen.Value) return null;

        return new Dialog(
            _ => _dialogOpen.Set(false),
            new DialogHeader("Discard Plan"),
            new DialogBody(
                Text.P($"Are you sure you want to discard plan #{_selectedPlan.Id}?")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().ShortcutKey("Escape").OnClick(() => _dialogOpen.Set(false)),
                new Button("Discard").Destructive().ShortcutKey("Enter").AutoFocus().OnClick(() =>
                {
                    _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Skipped);
                    _refreshPlans();
                    _dialogOpen.Set(false);
                })
            )
        ).Width(Size.Rem(40));
    }
}