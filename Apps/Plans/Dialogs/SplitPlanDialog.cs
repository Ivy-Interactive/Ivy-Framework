using Ivy;

namespace Ivy.Tendril.Apps.Plans.Dialogs;

public class SplitPlanDialog : ViewBase
{
    private readonly IState<bool> _dialogOpen;
    private readonly IState<string> _splitText;
    private readonly PlanFile _selectedPlan;

    public SplitPlanDialog(IState<bool> dialogOpen, IState<string> splitText, PlanFile selectedPlan)
    {
        _dialogOpen = dialogOpen;
        _splitText = splitText;
        _selectedPlan = selectedPlan;
    }

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
                new Button("Split Plan").Primary().OnClick(() => _dialogOpen.Set(false))
            )
        ).Width(Size.Rem(30));
    }
}
