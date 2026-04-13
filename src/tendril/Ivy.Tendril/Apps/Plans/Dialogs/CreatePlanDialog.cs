using Ivy.Core.Hooks;

namespace Ivy.Tendril.Apps.Plans.Dialogs;

public class CreatePlanDialog(
    List<string> projectNames,
    Action<string, string[], int> onCreatePlan,
    Action onClose,
    string[]? defaultProjects = null) : ViewBase
{
    private readonly string[] _defaultProjects = defaultProjects ?? ["Auto"];

    internal static readonly List<string> PriorityOptions = ["Normal", "High", "Urgent"];

    internal static int ParsePriority(string option) => option.ToLowerInvariant() switch
    {
        "normal" => 0,
        "high" => 1,
        "urgent" => 2,
        _ => 0
    };

    public override object Build()
    {
        var createPlanText = UseState("");
        var selectedProjects = UseState(_defaultProjects);
        var selectedPriority = UseState("Normal");

        var exclusiveProjects = new ConvertedState<string[], string[]>(
            selectedProjects,
            forward: v => v,
            backward: newValue =>
            {
                var current = selectedProjects.Value;
                if (newValue.Contains("Auto") && !current.Contains("Auto"))
                    return ["Auto"];
                if (newValue.Contains("Auto") && newValue.Any(p => p != "Auto"))
                    return newValue.Where(p => p != "Auto").ToArray();
                return newValue;
            }
        );

        var options = new List<string> { "Auto" };
        options.AddRange(projectNames);

        return new Dialog(
            _ => onClose(),
            new DialogHeader("Create New Plan"),
            new DialogBody(
                Layout.Vertical()
                | exclusiveProjects.ToSelectInput(options).Variant(SelectInputVariant.Toggle).WithField().Label("Select project(s)")
                | selectedPriority.ToSelectInput(PriorityOptions).Variant(SelectInputVariant.Toggle).WithField().Label("Priority")
                | createPlanText.ToTextareaInput("Enter task description...").Rows(6).AutoFocus().WithField()
                    .Label("Describe the task for the new plan")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(onClose),
                new Button("Create").Primary().ShortcutKey("Ctrl+Enter").OnClick(() =>
                {
                    if (!string.IsNullOrWhiteSpace(createPlanText.Value))
                    {
                        var projects = selectedProjects.Value.Any() ? selectedProjects.Value : ["Auto"];
                        onCreatePlan(createPlanText.Value, projects, ParsePriority(selectedPriority.Value));
                        onClose();
                    }
                })
            )
        ).Width(Size.Rem(30));
    }
}
