using Ivy.Hooks.Pty;

namespace Ivy.Tendril.Apps;

[App(title: "Claude", icon: Icons.Terminal, group: new[] { "Tools" }, order: MenuOrder.Claude, isVisible: false)]
public class ClaudeApp : ViewBase
{
    public override object? Build()
    {
        var isOpen = UseState(false);

        if (!isOpen.Value)
            return new Button("Open Claude")
                .OnClick(() => isOpen.Set(true));

        var pty = Context.UsePty(
            ["claude"],
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        );

        var terminal = new Widgets.Xterm.Terminal();
        terminal = Widgets.Xterm.TerminalExtensions.Stream(terminal, pty.Stream);
        terminal = Widgets.Xterm.TerminalExtensions.OnInput(terminal, pty.HandleInput);
        terminal = Widgets.Xterm.TerminalExtensions.OnResize(terminal, pty.HandleResize);
        terminal = Widgets.Xterm.TerminalExtensions.Closed(terminal, pty.Closed);
        terminal = Widgets.Xterm.TerminalExtensions.AllowClipboard(terminal);

        return terminal
            .WithLayout()
            .Full()
            .RemoveParentPadding();
    }
}