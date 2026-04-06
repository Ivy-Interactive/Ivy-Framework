using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Plans.Dialogs;

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
                Text.P($"What would you like to do with plan #{_selectedPlan.Id}?")
            ),
            new DialogFooter(
                Layout.Vertical().Gap(2)
                    | (Layout.Horizontal().Gap(2).Right()
                        | new Button("Cancel").Outline().ShortcutKey("Escape").OnClick(() => _dialogOpen.Set(false))
                        | new Button("Move to Skipped").Outline().ShortcutKey("s").OnClick(() =>
                        {
                            _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Skipped);
                            _refreshPlans();
                            _dialogOpen.Set(false);
                        })
                        | new Button("Move to Icebox").Outline().ShortcutKey("b").OnClick(() =>
                        {
                            _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Icebox);
                            _refreshPlans();
                            _dialogOpen.Set(false);
                        }))
                    | (Layout.Horizontal().Right()
                        | new Button("Delete").Destructive().ShortcutKey("Enter").AutoFocus().OnClick(() =>
                        {
                            _planService.DeletePlan(_selectedPlan.FolderName);
                            _refreshPlans();
                            _dialogOpen.Set(false);
                        }))
            )
        ).Width(Size.Rem(40));
    }
}
