using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Tasks.Dialogs;

public class ViewTaskOutputDialog : ViewBase
{
    private readonly bool _isOpen;
    private readonly TaskItem _task;
    private readonly TaskService _taskService;
    private readonly Action _onClose;

    public ViewTaskOutputDialog(
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

        var content = Layout.Vertical().Gap(2)
            | new TaskDetailDialogView(_task, _taskService)
            | Layout.Horizontal().Gap(2).Right()
                | new Button("Close").Outline().OnClick(_onClose);

        return new Sheet(
            _onClose,
            content,
            title: $"Task {_task.Id} - {_task.Type}"
        )
        .Side(SheetSide.Right)
        .Width(Size.Percent(90))
        .Height(Size.Full());
    }
}
