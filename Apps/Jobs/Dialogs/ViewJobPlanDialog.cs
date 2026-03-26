using Ivy;

namespace Ivy.Tendril.Apps.Jobs.Dialogs;

public class ViewJobPlanDialog : ViewBase
{
    private readonly bool _isOpen;
    private readonly JobItem _job;
    private readonly Action _onClose;

    public ViewJobPlanDialog(
        bool isOpen,
        JobItem job,
        Action onClose)
    {
        _isOpen = isOpen;
        _job = job;
        _onClose = onClose;
    }

    public override object? Build()
    {
        if (!_isOpen) return null;

        var planPath = Path.Combine(@"D:\Repos\_Ivy\.plans", _job.PlanFile);
        var planContent = File.Exists(planPath)
            ? File.ReadAllText(planPath)
            : $"Plan file not found: {_job.PlanFile}";

        return new Dialog(
            _ => { _onClose(); return ValueTask.CompletedTask; },
            new DialogHeader(_job.PlanFile),
            new DialogBody(
                Text.Code(planContent)
            ),
            new DialogFooter(
                new Button("Close").Outline().OnClick(_onClose)
            )
        ).Width(Size.Rem(50));
    }
}
