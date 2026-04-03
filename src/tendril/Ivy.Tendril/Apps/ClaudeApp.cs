using Ivy;
using Ivy.Hooks.Pty;

namespace Ivy.Tendril.Apps;

[App(title: "Claude", icon: Icons.Terminal, group: new[] { "Tools" }, order: 35)]
public class ClaudeApp : ViewBase
{
    public override object? Build()
    {
        var isOpen = UseState(false);

        if (!isOpen.Value)
        {
            return new Button("Open Claude")
                .OnClick(() => isOpen.Set(true));
        }

        var pty = this.Context.UsePty(
            ["claude"],
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        );

        var terminal = new Ivy.Widgets.Xterm.Terminal();
        terminal = Ivy.Widgets.Xterm.TerminalExtensions.Stream(terminal, pty.Stream);
        terminal = Ivy.Widgets.Xterm.TerminalExtensions.OnInput(terminal, pty.HandleInput);
        terminal = Ivy.Widgets.Xterm.TerminalExtensions.OnResize(terminal, pty.HandleResize);
        terminal = Ivy.Widgets.Xterm.TerminalExtensions.Closed(terminal, pty.Closed);
        terminal = Ivy.Widgets.Xterm.TerminalExtensions.AllowClipboard(terminal, true);

        return terminal
            .WithLayout()
            .Full()
            .RemoveParentPadding();
    }
}
