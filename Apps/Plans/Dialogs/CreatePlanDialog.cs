using Ivy;

namespace Ivy.Tendril.Apps.Plans.Dialogs;

public class CreatePlanDialog : ViewBase
{
    private readonly Action<string> _onCreatePlan;
    private readonly Action _onClose;

    public CreatePlanDialog(Action<string> onCreatePlan, Action onClose)
    {
        _onCreatePlan = onCreatePlan;
        _onClose = onClose;
    }

    public override object Build()
    {
        var createPlanText = UseState("");

        return new Dialog(
            _ => _onClose(),
            new DialogHeader("Create New Plan"),
            new DialogBody(
                Layout.Vertical()
                    | Text.P("Describe the task for the new plan.")
                    | createPlanText.ToTextareaInput("Enter task description...").Rows(6)
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => _onClose()),
                new Button("Create").Primary().OnClick(() =>
                {
                    if (!string.IsNullOrWhiteSpace(createPlanText.Value))
                    {
                        _onCreatePlan(createPlanText.Value);
                        createPlanText.Set("");
                        _onClose();
                    }
                })
            )
        ).Width(Size.Rem(30));
    }
}
