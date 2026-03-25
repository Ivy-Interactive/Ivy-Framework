using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Plans.Dialogs;

public class SkipPlanDialog : ViewBase
{
    private readonly IState<bool> _dialogOpen;
    private readonly PlanFile _selectedPlan;
    private readonly PlanReaderService _planService;
    private readonly Action _refreshPlans;

    public SkipPlanDialog(
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
            new DialogHeader("Skip Plan"),
            new DialogBody(
                Text.P($"Move plan #{_selectedPlan.Id} to skipped/ directory?")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => _dialogOpen.Set(false)),
                new Button("Skip").Primary().OnClick(() =>
                {
                    _planService.SkipPlan(_selectedPlan.FileName);
                    _refreshPlans();
                    _dialogOpen.Set(false);
                })
            )
        ).Width(Size.Rem(30));
    }
}
