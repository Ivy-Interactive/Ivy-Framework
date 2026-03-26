using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Jobs.Dialogs;

public class ViewJobOutputDialog : ViewBase
{
    private readonly bool _isOpen;
    private readonly JobItem _job;
    private readonly JobService _jobService;
    private readonly Action _onClose;

    public ViewJobOutputDialog(
        bool isOpen,
        JobItem job,
        JobService jobService,
        Action onClose)
    {
        _isOpen = isOpen;
        _job = job;
        _jobService = jobService;
        _onClose = onClose;
    }

    public override object? Build()
    {
        if (!_isOpen) return null;

        var content = Layout.Vertical().Gap(2)
            | new JobDetailDialogView(_job, _jobService)
            | Layout.Horizontal().Gap(2).Right()
                | new Button("Close").Outline().OnClick(_onClose);

        return new Sheet(
            _onClose,
            content,
            title: $"Job {_job.Id} - {_job.Type}"
        )
        .Side(SheetSide.Right)
        .Width(Size.Percent(90))
        .Height(Size.Full());
    }
}
