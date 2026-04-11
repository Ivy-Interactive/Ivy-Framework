namespace Ivy.Tendril.Apps.Plans.Dialogs;

public class CreatePlanDialog(
    List<string> projectNames,
    Action<string, string[], int> onCreatePlan,
    Action onClose,
    string[]? defaultProjects = null) : ViewBase
{
    private readonly string[] _defaultProjects = defaultProjects ?? ["[Auto]"];
    private readonly Action _onClose = onClose;
    private readonly Action<string, string[], int> _onCreatePlan = onCreatePlan;
    private readonly List<string> _projectNames = projectNames;

    internal static readonly List<string> PriorityOptions = ["Normal (0)", "High (1)", "Urgent (2)"];

    internal static int ParsePriority(string option) =>
        int.TryParse(option.AsSpan(option.LastIndexOf('(') + 1, 1), out var v) ? v : 0;

    public override object Build()
    {
        var createPlanText = UseState("");
        var selectedProjects = UseState(_defaultProjects);
        var selectedPriority = UseState("Normal (0)");

        var options = new List<string> { "[Auto]" };
        options.AddRange(_projectNames);

        return new Dialog(
            _ => _onClose(),
            new DialogHeader("Create New Plan"),
            new DialogBody(
                Layout.Vertical()
                | selectedProjects.ToSelectInput(options).Variant(SelectInputVariant.Toggle).WithLabel("Select project(s)")
                | selectedPriority.ToSelectInput(PriorityOptions).Variant(SelectInputVariant.Toggle).WithLabel("Priority")
                | createPlanText.ToTextareaInput("Enter task description...").Rows(6).AutoFocus().WithField()
                    .Label("Describe the task for the new plan")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => _onClose()),
                new Button("Create").Primary().ShortcutKey("Ctrl+Enter").OnClick(() =>
                {
                    if (!string.IsNullOrWhiteSpace(createPlanText.Value))
                    {
                        var projects = selectedProjects.Value
                            .Where(p => p != "[Auto]")
                            .ToArray();
                        if (projects.Length == 0) projects = ["[Auto]"];
                        _onCreatePlan(createPlanText.Value, projects, ParsePriority(selectedPriority.Value));
                        _onClose();
                    }
                })
            )
        ).Width(Size.Rem(30));
    }
}