using Ivy;
using Ivy.Hooks.Pty;
using Ivy.Widgets.Xterm;
using Terminal = Ivy.Widgets.Xterm.Terminal;

namespace XtermSamples.Apps;

[App(title: "Claude Code", icon: Icons.Bot, group: ["Agents"], allowDuplicateTabs: true)]
class ClaudeCodeApp : ViewBase
{
    public override object Build()
    {
        var workDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var pty = Context.UsePty(
            // Resolve "claude" through the shell so npm shims (.cmd) work on Windows
            OperatingSystem.IsWindows() ? ["cmd", "/c", "claude"] : ["claude"],
            workDir,
            new PtyOptions
            {
                Environment = new Dictionary<string, string>
                {
                    ["FORCE_COLOR"] = "1",
                }
            }
        );

        return new Terminal()
            .Stream(pty.Stream)
            .OnInput(pty.HandleInput)
            .OnResize(pty.HandleResize)
            .Closed(pty.Closed)
            .AllowClipboard()
            .WithLayout()
            .Full()
            .RemoveParentPadding();
    }
}
