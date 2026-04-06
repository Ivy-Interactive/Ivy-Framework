using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Icebox.Dialogs;

public class DeletePlanDialog(
    IState<bool> dialogOpen,
    PlanFile selectedPlan,
    IPlanReaderService planService,
    Action refreshPlans) : ViewBase
{
    private readonly IState<bool> _dialogOpen = dialogOpen;
    private readonly PlanFile _selectedPlan = selectedPlan;
    private readonly IPlanReaderService _planService = planService;
    private readonly Action _refreshPlans = refreshPlans;

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
                new Button("Cancel").Outline().ShortcutKey("Escape").OnClick(() => _dialogOpen.Set(false)),
                new Button("Delete").Destructive().ShortcutKey("Enter").AutoFocus().OnClick(() =>
                {
                    _planService.DeletePlan(_selectedPlan.FolderName);
                    _refreshPlans();
                    _dialogOpen.Set(false);
                })
            )
        ).Width(Size.Rem(40));
    }
}
