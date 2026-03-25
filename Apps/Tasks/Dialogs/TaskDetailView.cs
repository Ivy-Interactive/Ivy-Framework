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

        var args = new List<string> { "-NoProfile", "-File", _task.ScriptPath };
        args.AddRange(_task.Args);

        var workingDirectory = Path.GetFullPath(
            Path.Combine(System.AppContext.BaseDirectory, "..", "..", ".."));

        var pty = this.Context.UsePty(
            ["pwsh", .. args],
            workingDirectory,
            new PtyOptions { Cols = 120, Rows = 30 }
        );

        // Check if task was stopped and kill PTY
        if (_task.CancellationRequested && !pty.Closed)
        {
            pty.Kill();
        }

        if (pty.Closed && !completed.Value)
        {
            completed.Set(true);

            // Only mark as completed if not already stopped
            if (!_task.CancellationRequested)
            {
                _taskService.CompleteTask(_task.Id, pty.ExitCode);
            }
        }

        var terminal = new Ivy.Widgets.Xterm.Terminal() with { Stream = pty.Stream };
        terminal = Ivy.Widgets.Xterm.TerminalExtensions.OnInput(terminal, pty.HandleInput);
        terminal = Ivy.Widgets.Xterm.TerminalExtensions.OnResize(terminal, pty.HandleResize);
        terminal = terminal with { Closed = pty.Closed };
        return terminal.Height(Size.Units(70)).AspectRatio(3 / 4).WithBox().Background(Colors.Black);
    }
}
