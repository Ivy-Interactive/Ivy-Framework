using Ivy;
using Ivy.Hooks.Pty;
using Ivy.Shared;
using Ivy.Views;
using Ivy.Widgets.Xterm;

var server = new Server();
server.AddApp<TerminalView>();
await server.RunAsync();

[App]
class TerminalView : ViewBase
{
    public override object Build()
    {
        var helloAppPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".console", "HelloApp");

        var pty = this.Context.UsePty(
            ["dotnet run"],
            Path.GetFullPath(helloAppPath)
        );

        return new Terminal()
            .Stream(pty.Stream)
            .HandleInput(pty.HandleInput)
            .HandleResize(pty.HandleResize)
            .HandleLinkClick(url => Console.WriteLine($"Link clicked: {url}"))
            .Closed(pty.Closed)
            .WithLayout()
            .Full()
            .RemoveParentPadding();
    }
}
