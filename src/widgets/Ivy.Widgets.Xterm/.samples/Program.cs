using Ivy;
using Ivy.Widgets.Xterm;

var server = new Server();
server.AddApp<TerminalView>();
await server.RunAsync();

[App]
class TerminalView : ViewBase
{
    public override object Build()
    {
        return new Terminal()
            .DarkTheme()
            .FontSize(14)
            .CursorBlink(true)
            .InitialContent("Welcome to Ivy Terminal!\r\n\r\n$ ")
            .HandleData(data =>
            {
                Console.WriteLine($"User typed: {data}");
            })
            .HandleResize((cols, rows) =>
            {
                Console.WriteLine($"Terminal resized to {cols}x{rows}");
            });
    }
}
