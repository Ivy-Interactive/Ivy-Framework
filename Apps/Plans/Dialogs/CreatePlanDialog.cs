using Ivy;

namespace Ivy.Tendril.Apps.Plans.Dialogs;

public class CreatePlanDialog(List<string> projectNames, Action<string, string> onCreatePlan, Action onClose) : ViewBase
{
    private readonly List<string> _projectNames = projectNames;
    private readonly Action<string, string> _onCreatePlan = onCreatePlan;
    private readonly Action _onClose = onClose;

    public override object Build()
    {
        var createPlanText = UseState("");
        var selectedProject = UseState("[Auto]");

        var options = new List<string> { "[Auto]" };
        options.AddRange(_projectNames);

        return new Dialog(
            _ => _onClose(),
            new DialogHeader("Create New Idea"),
            new DialogBody(
                Layout.Vertical()
                    | Text.P("Select project:")
                    | selectedProject.ToSelectInput(options)
                    | Text.P("Describe the task for the new idea.")
                    | createPlanText.ToTextareaInput("Enter task description...").Rows(6)
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => _onClose()),
                new Button("Create").Primary().OnClick(() =>
                {
                    if (!string.IsNullOrWhiteSpace(createPlanText.Value))
                    {
                        _onCreatePlan(createPlanText.Value, selectedProject.Value);
                        _onClose();
                    }
                })
            )
        ).Width(Size.Rem(30));
    }
}
