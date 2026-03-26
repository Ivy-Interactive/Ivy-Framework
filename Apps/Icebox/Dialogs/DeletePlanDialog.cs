using Ivy;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Icebox.Dialogs;

public class DeletePlanDialog : ViewBase
{
    private readonly IState<bool> _dialogOpen;
    private readonly PlanFile _selectedPlan;
    private readonly PlanReaderService _planService;
    private readonly Action _refreshPlans;

    public DeletePlanDialog(
        IState<bool> dialogOpen,
        PlanFile selectedPlan,
        PlanReaderService planService,
        Action refreshPlans)
    {
        _dialogOpen = dialogOpen;
        _selectedPlan = selectedPlan;
        _planService = planService;
        _refreshPlans = refreshPlans;
    }

    public override object? Build()
    {
        if (!_dialogOpen.Value) return null;

        return new Dialog(
            _ => _dialogOpen.Set(false),
            new DialogHeader("Delete Plan"),
            new DialogBody(
                Text.P($"Are you sure you want to permanently delete plan #{_selectedPlan.Id}?")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => _dialogOpen.Set(false)),
                new Button("Delete").Destructive().OnClick(() =>
                {
                    _planService.DeletePlan(_selectedPlan.FileName);
                    _refreshPlans();
                    _dialogOpen.Set(false);
                })
            )
        ).Width(Size.Rem(30));
    }
}
