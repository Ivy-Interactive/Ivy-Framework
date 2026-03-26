using Ivy;
using Ivy.Hooks.Pty;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Jobs;

public class JobDetailView(JobItem job, JobService jobService) : ViewBase
{
    private readonly JobItem _job = job;
    private readonly JobService _jobService = jobService;

    public override object Build()
    {
        var completed = UseState(false);

        var args = new List<string> { "-NoProfile", "-File", _job.ScriptPath };
        args.AddRange(_job.Args);

        var workingDirectory = Path.GetFullPath(
            Path.Combine(System.AppContext.BaseDirectory, "..", "..", ".."));

        var pty = this.Context.UsePty(
            ["pwsh", .. args],
            workingDirectory,
            new PtyOptions { Cols = 120, Rows = 30 }
        );

        if (pty.Closed && !completed.Value)
        {
            completed.Set(true);
            _jobService.CompleteJob(_job.Id, pty.ExitCode);
        }

        var terminal = new Ivy.Widgets.Xterm.Terminal() with { Stream = pty.Stream };
        terminal = Ivy.Widgets.Xterm.TerminalExtensions.OnInput(terminal, pty.HandleInput);
        terminal = Ivy.Widgets.Xterm.TerminalExtensions.OnResize(terminal, pty.HandleResize);
        terminal = terminal with { Closed = pty.Closed };
        return terminal
            .WithLayout()
            .Full()
            .RemoveParentPadding();
    }
}
