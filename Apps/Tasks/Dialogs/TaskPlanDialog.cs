using Ivy;

namespace Ivy.Tendril.Apps.Tasks.Dialogs;

public class TaskPlanDialog : ViewBase
{
    private readonly bool _isOpen;
    private readonly TaskItem _task;
    private readonly Action _onClose;

    public TaskPlanDialog(
        bool isOpen,
        TaskItem task,
        Action onClose)
    {
        _isOpen = isOpen;
        _task = task;
        _onClose = onClose;
    }

    public override object? Build()
    {
        if (!_isOpen) return null;

        var planPath = Path.Combine(@"D:\Repos\_Ivy\.plans", _task.PlanFile);
        var planContent = File.Exists(planPath)
            ? File.ReadAllText(planPath)
            : $"Plan file not found: {_task.PlanFile}";

        return new Dialog(
            _ => { _onClose(); return ValueTask.CompletedTask; },
            new DialogHeader(_task.PlanFile),
            new DialogBody(
                Text.Code(planContent)
            ),
            new DialogFooter(
                new Button("Close").Outline().OnClick(_onClose)
            )
        ).Width(Size.Rem(50));
    }
}
