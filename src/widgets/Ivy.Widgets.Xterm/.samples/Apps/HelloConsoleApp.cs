using Ivy;
using Ivy.Hooks.Pty;
using Ivy.Widgets.Xterm;
using AppContext = System.AppContext;
using Terminal = Ivy.Widgets.Xterm.Terminal;

namespace XtermSamples.Apps;

[App(title: "Hello Console", icon: Icons.SquareTerminal, group: ["Terminal"], allowDuplicateTabs: true)]
class HelloConsoleApp : ViewBase
{
    public override object Build()
    {
        var client = UseService<IClientProvider>();
        var helloAppPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".console", "HelloApp"));

        var pty = Context.UsePty(
            ["dotnet", "run", "--project", helloAppPath],
            helloAppPath
        );

        return new Terminal()
            .Stream(pty.Stream)
            .OnInput(pty.HandleInput)
            .OnResize(pty.HandleResize)
            .OnLinkClick(url => client.Toast(url, "Link clicked").Info())
            .Closed(pty.Closed)
            .AllowClipboard()
            .WithLayout()
            .Full()
            .RemoveParentPadding();
    }
}
