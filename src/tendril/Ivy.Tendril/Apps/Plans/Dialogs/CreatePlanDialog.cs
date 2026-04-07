namespace Ivy.Tendril.Apps.Plans.Dialogs;

public class CreatePlanDialog(
    List<string> projectNames,
    Action<string, string> onCreatePlan,
    Action onClose,
    string defaultProject = "[Auto]") : ViewBase
{
    private readonly string _defaultProject = defaultProject;
    private readonly Action _onClose = onClose;
    private readonly Action<string, string> _onCreatePlan = onCreatePlan;
    private readonly List<string> _projectNames = projectNames;

    public override object Build()
    {
        var createPlanText = UseState("");
        var selectedProject = UseState(_defaultProject);

        var options = new List<string> { "[Auto]" };
        options.AddRange(_projectNames);

        return new Dialog(
            _ => _onClose(),
            new DialogHeader("Create New Plan"),
            new DialogBody(
                Layout.Vertical()
                | selectedProject.ToSelectInput(options).Variant(SelectInputVariant.Toggle).WithLabel("Select project")
                | createPlanText.ToTextareaInput("Enter task description...").Rows(6).AutoFocus().WithField()
                    .Label("Describe the task for the new plan")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => _onClose()),
                new Button("Create").Primary().ShortcutKey("Ctrl+Enter").OnClick(() =>
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