using Ivy;

namespace Ivy.Tendril.Apps.Plans.Dialogs;

public class UpdatePlanDialog : ViewBase
{
    private readonly IState<bool> _dialogOpen;
    private readonly IState<string> _updateText;
    private readonly PlanFile _selectedPlan;

    public UpdatePlanDialog(IState<bool> dialogOpen, IState<string> updateText, PlanFile selectedPlan)
    {
        _dialogOpen = dialogOpen;
        _updateText = updateText;
        _selectedPlan = selectedPlan;
    }

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
                new Button("Submit Update").Primary().OnClick(() => _dialogOpen.Set(false))
            )
        ).Width(Size.Rem(30));
    }
}
