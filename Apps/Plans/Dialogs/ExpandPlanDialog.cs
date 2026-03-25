using Ivy;

namespace Ivy.Tendril.Apps.Plans.Dialogs;

public class ExpandPlanDialog : ViewBase
{
    private readonly IState<bool> _dialogOpen;
    private readonly IState<string> _expandText;
    private readonly PlanFile _selectedPlan;

    public ExpandPlanDialog(IState<bool> dialogOpen, IState<string> expandText, PlanFile selectedPlan)
    {
        _dialogOpen = dialogOpen;
        _expandText = expandText;
        _selectedPlan = selectedPlan;
    }

    public override object? Build()
    {
        if (!_dialogOpen.Value) return null;

        return new Dialog(
            _ => _dialogOpen.Set(false),
            new DialogHeader($"Expand Plan #{_selectedPlan.Id}"),
            new DialogBody(
                Layout.Vertical()
                    | Text.P("Provide instructions for expanding this plan.")
                    | _expandText.ToTextareaInput("Enter expansion instructions...").Rows(6)
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => _dialogOpen.Set(false)),
                new Button("Expand Plan").Primary().OnClick(() => _dialogOpen.Set(false))
            )
        ).Width(Size.Rem(30));
    }
}
