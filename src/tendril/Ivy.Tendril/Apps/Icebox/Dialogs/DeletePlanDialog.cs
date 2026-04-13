using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Icebox.Dialogs;

public class DeletePlanDialog(
    IState<bool> dialogOpen,
    PlanFile selectedPlan,
    IPlanReaderService planService,
    Action refreshPlans) : ViewBase
{
    public override object? Build()
    {
        if (!dialogOpen.Value) return null;

        return new Dialog(
            _ => dialogOpen.Set(false),
            new DialogHeader("Delete Plan"),
            new DialogBody(
                Text.P($"Are you sure you want to permanently delete plan #{selectedPlan.Id}?")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().ShortcutKey("Escape").OnClick(() => dialogOpen.Set(false)),
                new Button("Delete").Destructive().ShortcutKey("Enter").AutoFocus().OnClick(() =>
                {
                    planService.DeletePlan(selectedPlan.FolderName);
                    refreshPlans();
                    dialogOpen.Set(false);
                })
            )
        ).Width(Size.Rem(40));
    }
}