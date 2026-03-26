using Ivy;
using Ivy.Hooks.Pty;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Tasks.Dialogs;

public class TaskDetailView : ViewBase
{
    private readonly TaskItem _task;
    private readonly TaskService _taskService;

    public TaskDetailView(TaskItem task, TaskService taskService)
    {
        _task = task;
        _taskService = taskService;
    }

    public override object Build()
    {
        var completed = UseState(false);

        // Only create PTY if it doesn't exist yet
        if (_task.PtyStream == null)
        {
            var args = new List<string> { "-NoProfile", "-File", _task.ScriptPath };
            args.AddRange(_task.Args);

            var workingDirectory = Path.GetFullPath(
                Path.Combine(System.AppContext.BaseDirectory, "..", "..", ".."));

            var pty = this.Context.UsePty(
                ["pwsh", .. args],
                workingDirectory,
                new PtyOptions { Cols = 120, Rows = 40 } // Increased rows for better visibility
            );

            _taskService.InitializePty(_task.Id, pty);
        }

        // Check if task was stopped and kill PTY
        if (_task.CancellationRequested && !_task.PtyClosed && _task.PtyKill != null)
        {
            _task.PtyKill();
        }

        // Update PTY state from task
        _taskService.UpdatePtyState(_task.Id, _task.PtyClosed, _task.PtyExitCode);

        if (_task.PtyClosed && !completed.Value)
        {
            completed.Set(true);

            // Only mark as completed if not already stopped
            if (!_task.CancellationRequested)
            {
                _taskService.CompleteTask(_task.Id, _task.PtyExitCode);
            }
        }

        var terminal = new Ivy.Widgets.Xterm.Terminal() with { Stream = _task.PtyStream };
        if (_task.PtyHandleInput != null)
        {
            terminal = Ivy.Widgets.Xterm.TerminalExtensions.OnInput(terminal, _task.PtyHandleInput);
        }
        if (_task.PtyHandleResize != null)
        {
            terminal = Ivy.Widgets.Xterm.TerminalExtensions.OnResize(terminal, _task.PtyHandleResize);
        }
        terminal = terminal with { Closed = _task.PtyClosed };

        return terminal.Height(Size.Full()).Width(Size.Full()).WithBox().Background(Colors.Black);
    }
}
