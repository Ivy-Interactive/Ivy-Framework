using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Tasks.Dialogs;

public class TaskOutputDialog : ViewBase
{
    private readonly bool _isOpen;
    private readonly TaskItem _task;
    private readonly TaskService _taskService;
    private readonly Action _onClose;

    public TaskOutputDialog(
        bool isOpen,
        TaskItem task,
        TaskService taskService,
        Action onClose)
    {
        _isOpen = isOpen;
        _task = task;
        _taskService = taskService;
        _onClose = onClose;
    }

    public override object? Build()
    {
        if (!_isOpen) return null;

        return new Dialog(
            _ => { _onClose(); return ValueTask.CompletedTask; },
            new DialogHeader($"Task {_task.Id} - {_task.Type}"),
            new DialogBody(
                new TaskDetailView(_task, _taskService)
            ),
            new DialogFooter(
                new Button("Close").Outline().OnClick(_onClose)
            )
        ).Width(Size.Units(100)).Height(Size.Rem(40));
    }
}
