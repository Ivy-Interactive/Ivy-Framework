using Ivy;
using Ivy.Hooks.Pty;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Jobs.Dialogs;

public class JobDetailDialogView : ViewBase
{
    private readonly JobItem _job;
    private readonly JobService _jobService;

    public JobDetailDialogView(JobItem job, JobService jobService)
    {
        _job = job;
        _jobService = jobService;
    }

    public override object Build()
    {
        var completed = UseState(false);

        // Only create PTY if it doesn't exist yet
        if (_job.PtyStream == null)
        {
            var args = new List<string> { "-NoProfile", "-File", _job.ScriptPath };
            args.AddRange(_job.Args);

            var workingDirectory = Path.GetFullPath(
                Path.Combine(System.AppContext.BaseDirectory, "..", "..", ".."));

            var pty = this.Context.UsePty(
                ["pwsh", .. args],
                workingDirectory,
                new PtyOptions { Cols = 120, Rows = 40 } // Increased rows for better visibility
            );

            _jobService.InitializePty(_job.Id, pty);
        }

        // Check if task was stopped and kill PTY
        if (_job.CancellationRequested && !_job.PtyClosed && _job.PtyKill != null)
        {
            _job.PtyKill();
        }

        // Update PTY state from task
        _jobService.UpdatePtyState(_job.Id, _job.PtyClosed, _job.PtyExitCode);

        if (_job.PtyClosed && !completed.Value)
        {
            completed.Set(true);

            // Only mark as completed if not already stopped
            if (!_job.CancellationRequested)
            {
                _jobService.CompleteJob(_job.Id, _job.PtyExitCode);
            }
        }

        var terminal = new Ivy.Widgets.Xterm.Terminal() with { Stream = _job.PtyStream };
        if (_job.PtyHandleInput != null)
        {
            terminal = Ivy.Widgets.Xterm.TerminalExtensions.OnInput(terminal, _job.PtyHandleInput);
        }
        if (_job.PtyHandleResize != null)
        {
            terminal = Ivy.Widgets.Xterm.TerminalExtensions.OnResize(terminal, _job.PtyHandleResize);
        }
        terminal = terminal with { Closed = _job.PtyClosed };

        return terminal.Height(Size.Full()).Width(Size.Full()).WithBox().Background(Colors.Black);
    }
}
